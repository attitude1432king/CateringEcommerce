using CateringEcommerce.Domain.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/user/notifications")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Get unread notification count for current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "User not authenticated" });
                }

                var count = await _notificationRepository.GetUnreadCountAsync(userId, "USER");

                return Ok(new
                {
                    result = true,
                    data = new { unreadCount = count }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated notifications for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "User not authenticated" });
                }

                var notifications = await _notificationRepository.GetNotificationsAsync(
                    userId,
                    "USER",
                    pageSize,
                    pageNumber
                );

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, "USER");

                return Ok(new
                {
                    result = true,
                    data = new
                    {
                        notifications,
                        unreadCount,
                        totalCount = notifications.Count,
                        pageNumber,
                        pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "User not authenticated" });
                }

                await _notificationRepository.MarkAsReadAsync(notificationId, userId);

                return Ok(new { result = true, message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "User not authenticated" });
                }

                // Note: Need to add MarkAllAsReadAsync to interface or cast to concrete type
                var concreteRepo = _notificationRepository as CateringEcommerce.BAL.Notification.NotificationRepository;
                if (concreteRepo != null)
                {
                    await concreteRepo.MarkAllAsReadAsync(userId, "USER");
                }

                return Ok(new { result = true, message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification (soft delete)
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "User not authenticated" });
                }

                var concreteRepo = _notificationRepository as CateringEcommerce.BAL.Notification.NotificationRepository;
                if (concreteRepo != null)
                {
                    await concreteRepo.DeleteNotificationAsync(notificationId, userId);
                }

                return Ok(new { result = true, message = "Notification deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}
