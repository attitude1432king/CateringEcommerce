import { fetchApi } from './apiUtils';

/**
 * Notification API Service
 *
 * Handles all API calls related to admin notifications
 */

const BASE_URL = '/admin/notifications';

export const notificationApi = {
  /**
   * Get unread notification count
   */
  async getUnreadCount() {
        const result = await fetchApi(`${BASE_URL}/unread-count`);
    // Extract the actual count from the response
    return result?.data?.unreadCount || 0;
  },

  /**
   * Get notifications with filters
   */
  async getNotifications(params = {}) {
    const queryParams = new URLSearchParams();

    if (params.type) queryParams.append('NotificationType', params.type);
    if (params.isRead !== undefined) queryParams.append('IsRead', params.isRead);
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);

      const result = await fetchApi(`${BASE_URL}`, 'GET', null, queryParams);
    return result?.data || { notifications: [], totalCount: 0, unreadCount: 0 };
  },

  /**
   * Mark notification as read
   */
  async markAsRead(notificationId) {
      return fetchApi(`${BASE_URL}/${notificationId}/read`, 'PUT');
  },

  /**
   * Mark all notifications as read
   */
  async markAllAsRead() {
      return fetchApi(`${BASE_URL}/read-all`, 'PUT');
  }
};

export default notificationApi;
