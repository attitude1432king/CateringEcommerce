import { FileCheck, Clock } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

/**
 * NotificationItem - Individual notification card
 *
 * @param {Object} props
 * @param {Object} props.notification - Notification data
 * @param {Function} props.onClick - Click handler
 */
const NotificationItem = ({ notification, onClick }) => {
  const formatDate = (dateString) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch {
      return 'Recently';
    }
  };

  return (
    <button
      onClick={() => onClick(notification)}
      className={`
        w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors
        border-b border-gray-100 last:border-b-0
        ${!notification.isRead ? 'bg-indigo-50' : ''}
      `}
    >
      <div className="flex items-start space-x-3">
        {/* Icon */}
        <div className={`
          w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0
          ${!notification.isRead ? 'bg-indigo-100' : 'bg-gray-100'}
        `}>
          <FileCheck className={`w-5 h-5 ${!notification.isRead ? 'text-indigo-600' : 'text-gray-600'}`} />
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <p className={`text-sm ${!notification.isRead ? 'font-semibold text-gray-900' : 'font-medium text-gray-700'}`}>
            {notification.title}
          </p>
          <p className="text-xs text-gray-500 mt-1 line-clamp-2">
            {notification.message}
          </p>
          <div className="flex items-center mt-1 text-xs text-gray-400">
            <Clock className="w-3 h-3 mr-1" />
            {formatDate(notification.createdAt)}
          </div>
        </div>

        {/* Unread indicator */}
        {!notification.isRead && (
          <div className="w-2 h-2 bg-indigo-600 rounded-full flex-shrink-0 mt-2" />
        )}
      </div>
    </button>
  );
};

export default NotificationItem;
