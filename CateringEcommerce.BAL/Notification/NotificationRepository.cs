using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public NotificationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task SaveDeliveryStatusAsync(NotificationDelivery delivery)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", delivery.NotificationId),
                new SqlParameter("@Channel", delivery.Channel),
                new SqlParameter("@Status", delivery.Status),
                new SqlParameter("@Provider", delivery.Provider ?? (object)DBNull.Value),
                new SqlParameter("@ProviderMessageId", delivery.ProviderMessageId ?? (object)DBNull.Value),
                new SqlParameter("@Recipient", delivery.Recipient),
                new SqlParameter("@SentAt", delivery.SentAt),
                new SqlParameter("@DeliveredAt", delivery.DeliveredAt ?? (object)DBNull.Value),
                new SqlParameter("@ErrorMessage", delivery.ErrorMessage ?? (object)DBNull.Value),
                new SqlParameter("@RetryCount", delivery.RetryCount),
                new SqlParameter("@Cost", delivery.Cost ?? (object)DBNull.Value)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                "INSERT INTO t_sys_notification_delivery " +
                "(c_notification_id, c_channel, c_status, c_provider, c_provider_message_id, " +
                "c_recipient, c_sent_at, c_delivered_at, c_error_message, c_retry_count, c_cost) " +
                "VALUES (@NotificationId, @Channel, @Status, @Provider, @ProviderMessageId, " +
                "@Recipient, @SentAt, @DeliveredAt, @ErrorMessage, @RetryCount, @Cost)",
                parameters
            );
        }

        public async Task<NotificationDelivery?> GetDeliveryStatusAsync(string notificationId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", notificationId)
            };

            var dt = await _dbHelper.ExecuteAsync(
                "SELECT * FROM t_sys_notification_delivery WHERE c_notification_id = @NotificationId",
                parameters
            );

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];
            return new NotificationDelivery
            {
                Id = Convert.ToInt64(row["c_id"]),
                NotificationId = row["c_notification_id"].ToString() ?? string.Empty,
                Channel = row["c_channel"].ToString() ?? string.Empty,
                Status = row["c_status"].ToString() ?? string.Empty,
                Provider = row["c_provider"] != DBNull.Value ? row["c_provider"].ToString() : null,
                ProviderMessageId = row["c_provider_message_id"] != DBNull.Value ? row["c_provider_message_id"].ToString() : null,
                Recipient = row["c_recipient"].ToString() ?? string.Empty,
                SentAt = Convert.ToDateTime(row["c_sent_at"]),
                DeliveredAt = row["c_delivered_at"] != DBNull.Value ? Convert.ToDateTime(row["c_delivered_at"]) : null,
                ErrorMessage = row["c_error_message"] != DBNull.Value ? row["c_error_message"].ToString() : null,
                RetryCount = Convert.ToInt32(row["c_retry_count"]),
                Cost = row["c_cost"] != DBNull.Value ? Convert.ToDecimal(row["c_cost"]) : null
            };
        }

        public async Task<List<NotificationDelivery>> GetDeliveryHistoryAsync(string recipient, int limit = 50)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Recipient", recipient),
                new SqlParameter("@Limit", limit)
            };

            var dt = await _dbHelper.ExecuteAsync(
                "SELECT TOP (@Limit) * FROM t_sys_notification_delivery " +
                "WHERE c_recipient = @Recipient " +
                "ORDER BY c_sent_at DESC",
                parameters
            );

            var deliveries = new List<NotificationDelivery>();

            foreach (DataRow row in dt.Rows)
            {
                deliveries.Add(new NotificationDelivery
                {
                    Id = Convert.ToInt64(row["c_id"]),
                    NotificationId = row["c_notification_id"].ToString() ?? string.Empty,
                    Channel = row["c_channel"].ToString() ?? string.Empty,
                    Status = row["c_status"].ToString() ?? string.Empty,
                    Provider = row["c_provider"] != DBNull.Value ? row["c_provider"].ToString() : null,
                    ProviderMessageId = row["c_provider_message_id"] != DBNull.Value ? row["c_provider_message_id"].ToString() : null,
                    Recipient = row["c_recipient"].ToString() ?? string.Empty,
                    SentAt = Convert.ToDateTime(row["c_sent_at"]),
                    DeliveredAt = row["c_delivered_at"] != DBNull.Value ? Convert.ToDateTime(row["c_delivered_at"]) : null,
                    ErrorMessage = row["c_error_message"] != DBNull.Value ? row["c_error_message"].ToString() : null,
                    RetryCount = Convert.ToInt32(row["c_retry_count"]),
                    Cost = row["c_cost"] != DBNull.Value ? Convert.ToDecimal(row["c_cost"]) : null
                });
            }

            return deliveries;
        }

        public async Task UpdateDeliveryStatusAsync(string notificationId, string status, string? errorMessage = null)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", notificationId),
                new SqlParameter("@Status", status),
                new SqlParameter("@ErrorMessage", errorMessage ?? (object)DBNull.Value)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                "UPDATE t_sys_notification_delivery " +
                "SET c_status = @Status, c_error_message = @ErrorMessage " +
                "WHERE c_notification_id = @NotificationId",
                parameters
            );
        }

        // ============================================
        // IN-APP NOTIFICATION METHODS
        // ============================================

        public async Task<int> GetUnreadCountAsync(string userId, string? userType = null)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@UserType", userType ?? "USER")
            };

            var result = await _dbHelper.ExecuteScalarAsync(
                @"SELECT COUNT(*)
                  FROM t_sys_notifications
                  WHERE c_user_id = @UserId
                    AND c_user_type = @UserType
                    AND c_is_read = 0
                    AND c_is_deleted = 0
                    AND (c_expires_at IS NULL OR c_expires_at > GETDATE())",
                parameters
            );

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task MarkAsReadAsync(string notificationId, string userId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", notificationId),
                new SqlParameter("@UserId", userId)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                @"UPDATE t_sys_notifications
                  SET c_is_read = 1, c_read_at = GETDATE()
                  WHERE c_notification_uuid = @NotificationId
                    AND c_user_id = @UserId
                    AND c_is_read = 0",
                parameters
            );
        }

        public async Task<List<InAppNotificationDto>> GetNotificationsAsync(string userId, string? userType = null, int pageSize = 20, int pageNumber = 1)
        {
            var offset = (pageNumber - 1) * pageSize;

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@UserType", userType ?? "USER"),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offset)
            };

            var dt = await _dbHelper.ExecuteAsync(
                @"SELECT
                    c_notification_uuid AS NotificationId,
                    c_title AS Title,
                    c_message AS Message,
                    c_category AS Category,
                    c_priority AS Priority,
                    c_action_url AS ActionUrl,
                    c_icon_url AS IconUrl,
                    c_is_read AS IsRead,
                    c_created_at AS CreatedAt
                  FROM t_sys_notifications
                  WHERE c_user_id = @UserId
                    AND c_user_type = @UserType
                    AND c_is_deleted = 0
                    AND (c_expires_at IS NULL OR c_expires_at > GETDATE())
                  ORDER BY c_priority DESC, c_created_at DESC
                  OFFSET @Offset ROWS
                  FETCH NEXT @PageSize ROWS ONLY",
                parameters
            );

            var notifications = new List<InAppNotificationDto>();

            foreach (DataRow row in dt.Rows)
            {
                notifications.Add(new InAppNotificationDto
                {
                    NotificationId = row["NotificationId"].ToString() ?? string.Empty,
                    Title = row["Title"].ToString() ?? string.Empty,
                    Message = row["Message"].ToString() ?? string.Empty,
                    Category = row["Category"].ToString() ?? string.Empty,
                    Priority = Convert.ToInt32(row["Priority"]),
                    ActionUrl = row["ActionUrl"] != DBNull.Value ? row["ActionUrl"].ToString() ?? string.Empty : string.Empty,
                    IconUrl = row["IconUrl"] != DBNull.Value ? row["IconUrl"].ToString() ?? string.Empty : string.Empty,
                    IsRead = Convert.ToBoolean(row["IsRead"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                });
            }

            return notifications;
        }

        public async Task SaveInAppNotificationAsync(InAppNotification notification, CancellationToken cancellationToken = default)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", notification.UserId),
                new SqlParameter("@UserType", notification.UserType.ToString()),
                new SqlParameter("@Title", notification.Title),
                new SqlParameter("@Message", notification.Message),
                new SqlParameter("@Category", notification.Category),
                new SqlParameter("@Priority", notification.Priority),
                new SqlParameter("@ActionUrl", notification.ActionUrl ?? (object)DBNull.Value),
                new SqlParameter("@IconUrl", notification.IconUrl ?? (object)DBNull.Value),
                new SqlParameter("@Data", notification.Data ?? (object)DBNull.Value),
                new SqlParameter("@ExpiresAt", notification.ExpiresAt ?? (object)DBNull.Value)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                @"INSERT INTO t_sys_notifications
                  (c_user_id, c_user_type, c_title, c_message, c_category,
                   c_priority, c_action_url, c_icon_url, c_data, c_expires_at, c_created_at)
                  VALUES
                  (@UserId, @UserType, @Title, @Message, @Category,
                   @Priority, @ActionUrl, @IconUrl, @Data, @ExpiresAt, GETDATE())",
                parameters
            );
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        public async Task MarkAllAsReadAsync(string userId, string userType = "USER")
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@UserType", userType)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                @"UPDATE t_sys_notifications
                  SET c_is_read = 1, c_read_at = GETDATE()
                  WHERE c_user_id = @UserId
                    AND c_user_type = @UserType
                    AND c_is_read = 0
                    AND c_is_deleted = 0",
                parameters
            );
        }

        /// <summary>
        /// Delete/soft-delete a notification
        /// </summary>
        public async Task DeleteNotificationAsync(string notificationId, string userId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", notificationId),
                new SqlParameter("@UserId", userId)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                @"UPDATE t_sys_notifications
                  SET c_is_deleted = 1, c_deleted_at = GETDATE()
                  WHERE c_notification_uuid = @NotificationId
                    AND c_user_id = @UserId",
                parameters
            );
        }
    }
}
