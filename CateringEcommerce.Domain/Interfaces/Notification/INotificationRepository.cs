using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface INotificationRepository
    {
        Task SaveDeliveryStatusAsync(NotificationDelivery delivery);
        Task<NotificationDelivery?> GetDeliveryStatusAsync(string notificationId);
        Task<List<NotificationDelivery>> GetDeliveryHistoryAsync(string recipient, int limit = 50);
        Task UpdateDeliveryStatusAsync(string notificationId, string status, string? errorMessage = null);

        // In-app notification methods
        Task<int> GetUnreadCountAsync(string userId, string? userType = null);
        Task MarkAsReadAsync(string notificationId, string userId);
        Task<List<InAppNotificationDto>> GetNotificationsAsync(string userId, string? userType = null, int pageSize = 20, int pageNumber = 1);
        Task SaveInAppNotificationAsync(InAppNotification notification, CancellationToken cancellationToken = default);
    }
}
