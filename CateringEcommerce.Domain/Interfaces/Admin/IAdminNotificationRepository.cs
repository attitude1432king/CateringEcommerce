using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminNotificationRepository
    {
        // Get Notifications
        AdminNotificationListResponse GetNotifications(AdminNotificationListRequest request, long? adminId = null);

        // Get Unread Count
        int GetUnreadCount(long? adminId = null);

        // Mark as Read
        bool MarkAsRead(long notificationId, long adminId);
        bool MarkAllAsRead(long adminId);

        // Create Notification
        bool CreateNotification(string notificationType, string title, string? message, long? entityId, string? entityType, string? link, long? adminId = null);

        // Delete Notification
        bool DeleteNotification(long notificationId);
    }
}
