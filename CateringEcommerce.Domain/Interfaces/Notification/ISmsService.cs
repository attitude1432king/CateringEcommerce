using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface ISmsService
    {
        Task<SmsResult> SendSmsAsync(
            NotificationMessage notification,
            string renderedMessage,
            CancellationToken cancellationToken = default);
    }
}
