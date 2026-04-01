import { fetchApi } from './apiUtils';

const ownerNotificationApi = {
    /**
     * Get unread notification count for current owner
     */
    getUnreadCount: async () => fetchApi('/owner/notifications/unread-count'),

    /**
     * Get paginated notifications for current owner
     * @param {number} pageNumber - Page number (default: 1)
     * @param {number} pageSize - Items per page (default: 20)
     */
    getNotifications: async (pageNumber = 1, pageSize = 20) => fetchApi(`/owner/notifications?pageNumber=${pageNumber}&pageSize${pageSize}`),

    /**
     * Mark a specific notification as read
     * Mark a specific notification as read
     * @param {string} notificationId - Notification UUID
     */
    markAsRead: async (notificationId) => fetchApi(`/owner/notifications/${notificationId}/read`, 'PUT'),
    
    /**
     * Mark all notifications as read
     */
    markAllAsRead: async () => fetchApi('/owner/notifications/read-all','PUT'),
        
    /**
     * Delete a notification (soft delete)
     * @param {string} notificationId - Notification UUID
     */
    deleteNotification: async (notificationId) => fetchApi(`/owner/notifications/${notificationId}`, 'DELETE'),
};

export default ownerNotificationApi;
