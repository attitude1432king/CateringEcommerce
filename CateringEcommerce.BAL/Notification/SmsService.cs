using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Notification
{
    public class SmsService : ISmsService
    {
        private readonly IEnumerable<ISmsProvider> _providers;
        private readonly ILogger<SmsService> _logger;
        private readonly INotificationRepository _repository;
        private readonly IRateLimiter _rateLimiter;

        public SmsService(
            IEnumerable<ISmsProvider> providers,
            ILogger<SmsService> logger,
            INotificationRepository repository,
            IRateLimiter rateLimiter)
        {
            _providers = providers.OrderBy(p => p.Priority);
            _logger = logger;
            _repository = repository;
            _rateLimiter = rateLimiter;
        }

        public async Task<SmsResult> SendSmsAsync(
            NotificationMessage notification,
            string renderedMessage,
            CancellationToken cancellationToken)
        {
            // Rate limiting: Max 10 SMS per phone number per minute
            var rateLimitKey = $"sms:{notification.Recipient.Phone}";
            if (!await _rateLimiter.AllowAsync(rateLimitKey, 10, TimeSpan.FromMinutes(1)))
            {
                _logger.LogWarning(
                    "Rate limit exceeded for phone number {Phone}",
                    notification.Recipient.Phone);

                return new SmsResult
                {
                    Success = false,
                    ErrorMessage = "Rate limit exceeded"
                };
            }

            var smsMessage = new SmsMessage
            {
                To = notification.Recipient.Phone,
                Message = renderedMessage,
                From = "CATRGAPP", // Sender ID
                IsOtp = notification.Category == "OTP",
                ValidityPeriod = notification.Options?.ExpiresAt != null
                    ? (int)(notification.Options.ExpiresAt.Value - DateTime.UtcNow).TotalMinutes
                    : 0
            };

            SmsResult result = null;

            // Try providers in priority order
            foreach (var provider in _providers.Where(p => p.IsAvailable))
            {
                try
                {
                    result = await provider.SendAsync(smsMessage, cancellationToken);

                    if (result.Success)
                    {
                        await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
                        {
                            NotificationId = notification.MessageId,
                            Channel = "SMS",
                            Status = "DELIVERED",
                            Provider = provider.ProviderName,
                            ProviderMessageId = result.ProviderMessageId,
                            SentAt = DateTime.UtcNow,
                            Recipient = notification.Recipient.Phone,
                            Cost = result.Cost
                        });

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Exception while sending SMS via {Provider}",
                        provider.ProviderName);
                }
            }

            // All providers failed
            await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
            {
                NotificationId = notification.MessageId,
                Channel = "SMS",
                Status = "FAILED",
                ErrorMessage = result?.ErrorMessage ?? "All providers failed",
                SentAt = DateTime.UtcNow,
                Recipient = notification.Recipient.Phone
            });

            return result ?? new SmsResult
            {
                Success = false,
                ErrorMessage = "All SMS providers failed"
            };
        }
    }
}
