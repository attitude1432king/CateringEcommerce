import { useState, useEffect } from 'react';
import { Bell } from 'lucide-react';

/**
 * NotificationBell Component
 * Displays a bell icon with unread notification count badge
 * Supports both User and Owner notification APIs
 *
 * @param {Object} props
 * @param {Function} props.notificationApi - API service (userNotificationApi or ownerNotificationApi)
 * @param {Function} props.onClick - Handler when bell is clicked
 * @param {number} props.refreshInterval - Auto-refresh interval in ms (default: 30000)
 */
const NotificationBell = ({
  notificationApi,
  onClick,
  refreshInterval = 30000
}) => {
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(false);

  const fetchUnreadCount = async () => {
    try {
      setLoading(true);
      const response = await notificationApi.getUnreadCount();
      if (response.result && response.data) {
        setUnreadCount(response.data.unreadCount || 0);
      }
    } catch (error) {
      console.error('Error fetching unread notification count:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Fetch immediately on mount
    fetchUnreadCount();

    // Set up polling interval
    const intervalId = setInterval(fetchUnreadCount, refreshInterval);

    // Cleanup interval on unmount
    return () => clearInterval(intervalId);
  }, [refreshInterval]);

  // Expose refresh method for parent components
  useEffect(() => {
    if (window.refreshNotificationBell) {
      window.refreshNotificationBell = fetchUnreadCount;
    }
  }, []);

  return (
    <button
      onClick={onClick}
      className="relative p-2 text-neutral-600 hover:text-neutral-900 hover:bg-gray-100 rounded-full transition-colors duration-200"
      aria-label="Notifications"
      disabled={loading}
    >
      <Bell size={24} className={loading ? 'animate-pulse' : ''} />

      {unreadCount > 0 && (
        <span className="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </button>
  );
};

export default NotificationBell;
