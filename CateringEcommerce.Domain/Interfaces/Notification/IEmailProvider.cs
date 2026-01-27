using System.Net.Mail;
using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface IEmailProvider
    {
        string ProviderName { get; }
        int Priority { get; } // Lower number = higher priority
        bool IsAvailable { get; }

        Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
