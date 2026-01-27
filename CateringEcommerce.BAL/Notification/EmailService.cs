using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using System.Text.RegularExpressions;

namespace CateringEcommerce.BAL.Notification
{
    public class EmailService : Domain.Interfaces.Notification.IEmailService
    {
        private readonly IEnumerable<IEmailProvider> _providers;
        private readonly ILogger<EmailService> _logger;
        private readonly INotificationRepository _repository;

        public EmailService(
            IEnumerable<IEmailProvider> providers,
            ILogger<EmailService> logger,
            INotificationRepository repository)
        {
            _providers = providers.OrderBy(p => p.Priority);
            _logger = logger;
            _repository = repository;
        }

        public async Task<EmailResult> SendEmailAsync(
            NotificationMessage notification,
            string renderedHtml,
            CancellationToken cancellationToken)
        {
            var emailMessage = new EmailMessage
            {
                To = notification.Recipient.Email,
                Subject = notification.Data["subject"]?.ToString() ?? "Notification",
                HtmlBody = renderedHtml,
                TextBody = StripHtml(renderedHtml),
                FromEmail = notification.Options?.From?.Email ?? "noreply@cateringecommerce.com",
                FromName = notification.Options?.From?.Name ?? "Catering Ecommerce",
                ReplyTo = notification.Options?.ReplyTo,
                TrackOpens = notification.Options?.TrackOpens ?? true,
                TrackClicks = notification.Options?.TrackClicks ?? true
            };

            EmailResult result = null;

            // Try providers in priority order
            foreach (var provider in _providers.Where(p => p.IsAvailable))
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to send email via {Provider} for message {MessageId}",
                        provider.ProviderName, notification.MessageId);

                    result = await provider.SendAsync(emailMessage, cancellationToken);

                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "Email sent successfully via {Provider}. MessageId: {MessageId}",
                            provider.ProviderName, notification.MessageId);

                        // Store delivery record
                        await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
                        {
                            NotificationId = notification.MessageId,
                            Channel = "EMAIL",
                            Status = "DELIVERED",
                            Provider = provider.ProviderName,
                            ProviderMessageId = result.ProviderMessageId,
                            SentAt = DateTime.UtcNow,
                            Recipient = notification.Recipient.Email
                        });

                        return result;
                    }

                    _logger.LogWarning(
                        "Email send failed via {Provider}. Error: {Error}",
                        provider.ProviderName, result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Exception occurred while sending email via {Provider}",
                        provider.ProviderName);
                }
            }

            // All providers failed
            _logger.LogError(
                "All email providers failed for message {MessageId}",
                notification.MessageId);

            await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
            {
                NotificationId = notification.MessageId,
                Channel = "EMAIL",
                Status = "FAILED",
                ErrorMessage = result?.ErrorMessage ?? "All providers failed",
                SentAt = DateTime.UtcNow,
                Recipient = notification.Recipient.Email
            });

            return result ?? new EmailResult
            {
                Success = false,
                ErrorMessage = "All email providers failed"
            };
        }

        private string StripHtml(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
