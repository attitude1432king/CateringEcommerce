import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CheckCheck, Loader2 } from 'lucide-react';
import NotificationItem from './NotificationItem';
import { notificationApi } from '../../../services/notificationApi';

/**
 * NotificationDropdown - Dropdown list of notifications
 *
 * @param {Object} props
 * @param {boolean} props.isOpen - Whether dropdown is open
 * @param {Function} props.onClose - Close handler
 * @param {Function} props.onNotificationRead - Callback when notification is marked as read
 */
const NotificationDropdown = ({ isOpen, onClose, onNotificationRead }) => {
  const navigate = useNavigate();
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(false);
  const [markingAllRead, setMarkingAllRead] = useState(false);

  useEffect(() => {
    if (isOpen) {
      loadNotifications();
    }
  }, [isOpen]);

  const loadNotifications = async () => {
    setLoading(true);
    try {
      const data = await notificationApi.getNotifications({ pageSize: 10 });
      setNotifications(data.notifications || []);
    } catch (error) {
      console.error('Failed to load notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleNotificationClick = async (notification) => {
    try {
      // Mark as read
      if (!notification.isRead) {
        await notificationApi.markAsRead(notification.notificationId);
        onNotificationRead?.();
      }

      // Navigate to the referenced entity
      if (notification.entityType === 'PARTNER_REQUEST' && notification.entityId) {
        navigate(`/admin/partner-requests?id=${notification.entityId}`);
      }

      onClose();
    } catch (error) {
      console.error('Failed to handle notification:', error);
    }
  };

  const handleMarkAllRead = async () => {
    setMarkingAllRead(true);
    try {
      await notificationApi.markAllAsRead();
      await loadNotifications();
      onNotificationRead?.();
    } catch (error) {
      console.error('Failed to mark all as read:', error);
    } finally {
      setMarkingAllRead(false);
    }
  };

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 z-30" onClick={onClose} />

      {/* Dropdown */}
      <div className="absolute right-0 mt-2 w-96 bg-white rounded-lg shadow-xl border border-gray-200 z-40 max-h-[600px] flex flex-col">
        {/* Header */}
        <div className="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
          <h3 className="font-semibold text-gray-900">Notifications</h3>
          {notifications.some(n => !n.isRead) && (
            <button
              onClick={handleMarkAllRead}
              disabled={markingAllRead}
              className="text-xs text-indigo-600 hover:text-indigo-700 font-medium disabled:opacity-50 flex items-center"
            >
              {markingAllRead ? (
                <>
                  <Loader2 className="w-3 h-3 mr-1 animate-spin" />
                  Marking...
                </>
              ) : (
                <>
                  <CheckCheck className="w-3 h-3 mr-1" />
                  Mark all read
                </>
              )}
            </button>
          )}
        </div>

        {/* Notifications List */}
        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <div className="py-12 text-center">
              <Loader2 className="w-8 h-8 text-gray-400 animate-spin mx-auto" />
              <p className="text-sm text-gray-500 mt-2">Loading notifications...</p>
            </div>
          ) : notifications.length === 0 ? (
            <div className="py-12 text-center">
              <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto">
                <CheckCheck className="w-8 h-8 text-gray-400" />
              </div>
              <p className="text-sm text-gray-500 mt-4">No notifications</p>
              <p className="text-xs text-gray-400 mt-1">You're all caught up!</p>
            </div>
          ) : (
            notifications.map((notification) => (
              <NotificationItem
                key={notification.notificationId}
                notification={notification}
                onClick={handleNotificationClick}
              />
            ))
          )}
        </div>

        {/* Footer */}
        {notifications.length > 0 && (
          <div className="px-4 py-3 border-t border-gray-200">
            <button
              onClick={() => {
                navigate('/admin/notifications');
                onClose();
              }}
              className="text-sm text-indigo-600 hover:text-indigo-700 font-medium w-full text-center"
            >
              View all notifications
            </button>
          </div>
        )}
      </div>
    </>
  );
};

export default NotificationDropdown;
