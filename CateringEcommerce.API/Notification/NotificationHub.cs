using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CateringEcommerce.API.Notification
{
    [Authorize] // Require authentication
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly INotificationRepository _repository;

        public NotificationHub(
            ILogger<NotificationHub> logger,
            INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = Context.User?.FindFirst("UserType")?.Value; // ADMIN, PARTNER, USER

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            {
                _logger.LogWarning("Connection rejected: Missing user identity");
                Context.Abort();
                return;
            }

            // Add to user-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{userType}_{userId}");

            // Add to role-specific group (for broadcasts)
            await Groups.AddToGroupAsync(Context.ConnectionId, userType);

            _logger.LogInformation(
                "User {UserId} ({UserType}) connected. ConnectionId: {ConnectionId}",
                userId, userType, Context.ConnectionId);

            // Send unread count on connection
            var unreadCount = await _repository.GetUnreadCountAsync(userId, userType);
            await Clients.Caller.SendAsync("UnreadCount", unreadCount);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = Context.User?.FindFirst("UserType")?.Value;

            _logger.LogInformation(
                "User {UserId} ({UserType}) disconnected. ConnectionId: {ConnectionId}",
                userId, userType, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        // Client can call this to mark notification as read
        public async Task MarkAsRead(string notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return;

            await _repository.MarkAsReadAsync(notificationId, userId);

            // Send updated unread count
            var userType = Context.User?.FindFirst("UserType")?.Value;
            var unreadCount = await _repository.GetUnreadCountAsync(userId, userType);
            await Clients.Caller.SendAsync("UnreadCount", unreadCount);

            _logger.LogInformation(
                "Notification {NotificationId} marked as read by user {UserId}",
                notificationId, userId);
        }

        // Client can call this to get notification history
        public async Task<List<InAppNotificationDto>> GetNotifications(int pageSize = 20, int pageNumber = 1)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = Context.User?.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId))
                return new List<InAppNotificationDto>();

            return await _repository.GetNotificationsAsync(userId, userType, pageSize, pageNumber);
        }
    }
}
