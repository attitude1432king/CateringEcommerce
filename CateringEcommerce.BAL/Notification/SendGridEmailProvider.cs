using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CateringEcommerce.BAL.Notification
{
    public class SendGridEmailProvider : IEmailProvider
    {
        private readonly SendGridClient _client;
        private readonly ILogger<SendGridEmailProvider> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public string ProviderName => "SendGrid";
        public int Priority => 1; // Primary provider
        public bool IsAvailable => _circuitBreaker.CircuitState != CircuitState.Open;

        public SendGridEmailProvider(
            IOptions<SendGridSettings> settings,
            ILogger<SendGridEmailProvider> logger)
        {
            _client = new SendGridClient(settings.Value.ApiKey);
            _logger = logger;

            // Circuit breaker: Open after 3 failures, reset after 60 seconds
            _circuitBreaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(60));
        }

        public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(message.FromEmail, message.FromName),
                    Subject = message.Subject,
                    HtmlContent = message.HtmlBody,
                    PlainTextContent = message.TextBody
                };

                msg.AddTo(new EmailAddress(message.To));

                if (!string.IsNullOrEmpty(message.ReplyTo))
                {
                    msg.ReplyTo = new EmailAddress(message.ReplyTo);
                }

                // Add attachments
                foreach (var attachment in message.Attachments)
                {
                    msg.AddAttachment(attachment.FileName, attachment.Content, attachment.ContentType);
                }

                // Tracking
                msg.SetClickTracking(message.TrackClicks, message.TrackClicks);
                msg.SetOpenTracking(message.TrackOpens);

                var response = await _circuitBreaker.ExecuteAsync(async () =>
                    await _client.SendEmailAsync(msg, cancellationToken));

                _logger.LogInformation(
                    "Email sent via SendGrid. StatusCode: {StatusCode}, MessageId: {MessageId}",
                    response.StatusCode, response.Headers.GetValues("X-Message-Id").FirstOrDefault());

                return new EmailResult
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString(),
                    ProviderMessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                    SentAt = DateTime.UtcNow,
                    ProviderName = ProviderName,
                    ErrorMessage = response.IsSuccessStatusCode ? null : await response.Body.ReadAsStringAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SendGrid");
                return new EmailResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName
                };
            }
        }

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
        {
            try
            {
                // SendGrid doesn't have a dedicated health endpoint
                // Check circuit breaker state
                return _circuitBreaker.CircuitState != CircuitState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
