using CateringEcommerce.Domain.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Route("api/owner/notifications")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Get unread notification count for current owner
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { result = false, message = "Owner not authenticated" });
                }

                var count = await _notificationRepository.GetUnreadCountAsync(userId, "OWNER");

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
        /// Get paginated notifications for current owner
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
                    return Unauthorized(new { result = false, message = "Owner not authenticated" });
                }

                var notifications = await _notificationRepository.GetNotificationsAsync(
                    userId,
                    "OWNER",
                    pageSize,
                    pageNumber
                );

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, "OWNER");

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
                    return Unauthorized(new { result = false, message = "Owner not authenticated" });
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
                    return Unauthorized(new { result = false, message = "Owner not authenticated" });
                }

                var concreteRepo = _notificationRepository as CateringEcommerce.BAL.Notification.NotificationRepository;
                if (concreteRepo != null)
                {
                    await concreteRepo.MarkAllAsReadAsync(userId, "OWNER");
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
                    return Unauthorized(new { result = false, message = "Owner not authenticated" });
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
