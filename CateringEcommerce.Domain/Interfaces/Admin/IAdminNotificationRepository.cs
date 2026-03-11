using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminNotificationRepository
    {
        // Get Notifications
        Task<AdminNotificationListResponse> GetNotifications(AdminNotificationListRequest request, long? adminId = null);

        // Get Unread Count
        Task<int> GetUnreadCount(long? adminId = null);

        // Mark as Read
        Task<bool> MarkAsRead(long notificationId, long adminId);
        Task<bool> MarkAllAsRead(long adminId);
                 
        // Create Notification
        Task<bool> CreateNotification(string notificationType, string title, string? message, long? entityId, string? entityType, string? link, long? adminId = null);
                 
        // Delete Notification
        Task<bool> DeleteNotification(long notificationId);
    }
}
