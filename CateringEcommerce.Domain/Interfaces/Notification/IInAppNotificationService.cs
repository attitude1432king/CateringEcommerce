using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface IInAppNotificationService
    {
        Task BroadcastAsync(string userType, string title, string message, CancellationToken cancellationToken);
        Task SendInAppNotificationAsync(NotificationMessage notification, string renderedMessage, CancellationToken cancellationToken);
    }
}
