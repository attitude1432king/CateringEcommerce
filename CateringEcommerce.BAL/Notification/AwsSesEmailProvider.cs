using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace CateringEcommerce.BAL.Notification
{
    public class AwsSesEmailProvider : IEmailProvider
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly ILogger<AwsSesEmailProvider> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public string ProviderName => "AWS_SES";
        public int Priority => 2; // Fallback provider
        public bool IsAvailable => _circuitBreaker.CircuitState != CircuitState.Open;

        public AwsSesEmailProvider(
            IOptions<AwsSesSettings> settings,
            ILogger<AwsSesEmailProvider> logger)
        {
            _sesClient = new AmazonSimpleEmailServiceClient(
                settings.Value.AccessKeyId,
                settings.Value.SecretAccessKey,
                RegionEndpoint.GetBySystemName(settings.Value.Region));

            _logger = logger;

            _circuitBreaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(60));
        }

        public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var request = new SendEmailRequest
                {
                    Source = $"{message.FromName} <{message.FromEmail}>",
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { message.To }
                    },
                    Message = new Message
                    {
                        Subject = new Content(message.Subject),
                        Body = new Body
                        {
                            Html = new Content { Data = message.HtmlBody },
                            Text = new Content { Data = message.TextBody }
                        }
                    },
                    ReplyToAddresses = !string.IsNullOrEmpty(message.ReplyTo)
                        ? new List<string> { message.ReplyTo }
                        : null
                };

                var response = await _circuitBreaker.ExecuteAsync(async () =>
                    await _sesClient.SendEmailAsync(request, cancellationToken));

                _logger.LogInformation(
                    "Email sent via AWS SES. MessageId: {MessageId}",
                    response.MessageId);

                return new EmailResult
                {
                    Success = true,
                    MessageId = Guid.NewGuid().ToString(),
                    ProviderMessageId = response.MessageId,
                    SentAt = DateTime.UtcNow,
                    ProviderName = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via AWS SES");
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
                await _sesClient.GetSendQuotaAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
