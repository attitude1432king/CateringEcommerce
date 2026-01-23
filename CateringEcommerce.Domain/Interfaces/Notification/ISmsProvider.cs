using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface ISmsProvider
    {
        string ProviderName { get; }
        int Priority { get; }
        bool IsAvailable { get; }

        Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
