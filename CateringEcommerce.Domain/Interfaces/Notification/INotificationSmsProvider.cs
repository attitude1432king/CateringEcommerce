using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    /// <summary>
    /// Abstraction for SMS notification delivery providers.
    /// Used exclusively for order/system SMS notifications (NOT OTP).
    /// Examples: order confirmation, booking approval, payment confirmation, event alerts.
    /// </summary>
    public interface INotificationSmsProvider
    {
        string ProviderName { get; }
        int Priority { get; }
        bool IsAvailable { get; }

        Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
