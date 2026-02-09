import { useState } from 'react';
import NotificationBell from '../common/NotificationBell';
import NotificationCenter from '../common/NotificationCenter';
import userNotificationApi from '../../services/userNotificationApi';

/**
 * UserNotifications Component
 * Wrapper component that integrates NotificationBell and NotificationCenter
 * for the User portal
 */
const UserNotifications = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);

  const handleBellClick = () => {
    setIsOpen(!isOpen);
  };

  const handleClose = () => {
    setIsOpen(false);
  };

  const handleNotificationUpdate = (count) => {
    setUnreadCount(count);
    // Refresh the bell icon if needed
    if (window.refreshNotificationBell) {
      window.refreshNotificationBell();
    }
  };

  return (
    <div className="relative">
      <NotificationBell
        notificationApi={userNotificationApi}
        onClick={handleBellClick}
        refreshInterval={30000}
      />

      <NotificationCenter
        isOpen={isOpen}
        onClose={handleClose}
        notificationApi={userNotificationApi}
        onNotificationUpdate={handleNotificationUpdate}
      />
    </div>
  );
};

export default UserNotifications;
