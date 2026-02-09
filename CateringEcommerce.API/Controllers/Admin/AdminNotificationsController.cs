using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/notifications")]
    [ApiController]
    [AdminAuthorize]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly IAdminNotificationRepository _notificationRepository;
        private readonly ILogger<AdminNotificationsController> _logger;

        public AdminNotificationsController(
            IAdminNotificationRepository notificationRepository,
            ILogger<AdminNotificationsController> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all notifications for the current admin
        /// </summary>
        [HttpGet]
        public IActionResult GetNotifications([FromQuery] AdminNotificationListRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();

                var result = _notificationRepository.GetNotifications(request, adminId);

                return ApiResponseHelper.Success(result, "Notifications retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public IActionResult GetUnreadCount()
        {
            try
            {
                var adminId = GetAdminIdFromToken();

                var count = _notificationRepository.GetUnreadCount(adminId);

                return ApiResponseHelper.Success(new { unreadCount = count }, "Unread count retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public IActionResult MarkAsRead(long notificationId)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                var result = _notificationRepository.MarkAsRead(notificationId, adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "Notification marked as read.");

                return ApiResponseHelper.Failure("Failed to mark notification as read.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("read-all")]
        public IActionResult MarkAllAsRead()
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                var result = _notificationRepository.MarkAllAsRead(adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "All notifications marked as read.");

                return ApiResponseHelper.Failure("Failed to mark all notifications as read.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{notificationId}")]
        public IActionResult DeleteNotification(long notificationId)
        {
            try
            {
                var result = _notificationRepository.DeleteNotification(notificationId);

                if (result)
                    return ApiResponseHelper.Success(null, "Notification deleted successfully.");

                return ApiResponseHelper.Failure("Failed to delete notification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #region Helper Methods

        private long? GetAdminIdFromToken()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId" || c.Type == "userId")?.Value;
            if (long.TryParse(adminIdClaim, out var adminId))
                return adminId;

            return null;
        }

        #endregion
    }
}
