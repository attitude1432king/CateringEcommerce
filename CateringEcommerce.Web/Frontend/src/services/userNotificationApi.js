import { fetchApi } from './apiUtils';

const userNotificationApi = {
    /**
     * Get unread notification count for current user
     */
    getUnreadCount: async () => fetchApi('/user/notifications/unread-count'),
    /**
     * Get paginated notifications for current user
     * @param {number} pageNumber - Page number (default: 1)
     * @param {number} pageSize - Items per page (default: 20)
     */

    getNotifications: async (pageNumber = 1, pageSize = 20) => fetchApi(`/user/notifications?pageNumber=${pageNumber}&pageSize${pageSize}`),

    /**
     * Mark a specific notification as read
     * @param {string} notificationId - Notification UUID
     */
    markAsRead: async (notificationId) => fetchApi(`/user/notifications/${notificationId}/read`, 'PUT'),

    /**
     * Mark all notifications as read
     */
    markAllAsRead: async () =>  fetchApi('/user/notifications/read-all','PUT'),

    /**
     * Delete a notification (soft delete)
     * @param {string} notificationId - Notification UUID
     */
    deleteNotification: async (notificationId) => fetchApi(`/user/notifications/${notificationId}`, 'DELETE'),
};

export default userNotificationApi;
