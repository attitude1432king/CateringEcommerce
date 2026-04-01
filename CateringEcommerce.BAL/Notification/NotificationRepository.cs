using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private const string DefaultUserType = "USER";
        private readonly IDatabaseHelper _dbHelper;

        public NotificationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Delivery Status Methods

        public async Task SaveDeliveryStatusAsync(NotificationDelivery delivery)
        {
            string query = $@"INSERT INTO {Table.SysNotificationDelivery}
                              (c_notification_id, c_channel, c_status, c_provider, c_provider_message_id,
                               c_recipient, c_sent_at, c_delivered_at, c_error_message, c_retry_count, c_cost)
                              VALUES (@NotificationId, @Channel, @Status, @Provider, @ProviderMessageId,
                                      @Recipient, @SentAt, @DeliveredAt, @ErrorMessage, @RetryCount, @Cost)";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                BuildDeliveryParameters(delivery)
            );
        }

        public async Task<NotificationDelivery?> GetDeliveryStatusAsync(string notificationId)
        {
            string query = $"SELECT * FROM {Table.SysNotificationDelivery} WHERE c_notification_id = @NotificationId";

            var dt = await _dbHelper.ExecuteAsync(
                query,
                new[] { new SqlParameter("@NotificationId", notificationId) }
            );

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            return MapNotificationDelivery(dt.Rows[0]);
        }

        public async Task<List<NotificationDelivery>> GetDeliveryHistoryAsync(string recipient, int limit = 50)
        {
            string query = $@"SELECT TOP (@Limit) *
                              FROM {Table.SysNotificationDelivery}
                              WHERE c_recipient = @Recipient
                              ORDER BY c_sent_at DESC";

            var dt = await _dbHelper.ExecuteAsync(
                query,
                new[]
                {
                    new SqlParameter("@Recipient", recipient),
                    new SqlParameter("@Limit", limit)
                }
            );

            return dt.Rows.Cast<DataRow>()
                .Select(MapNotificationDelivery)
                .ToList();
        }

        public async Task UpdateDeliveryStatusAsync(string notificationId, string status, string? errorMessage = null)
        {
            string query = $@"UPDATE {Table.SysNotificationDelivery}
                              SET c_status = @Status,
                                  c_error_message = @ErrorMessage
                              WHERE c_notification_id = @NotificationId";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                new[]
                {
                    new SqlParameter("@NotificationId", notificationId),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@ErrorMessage", errorMessage ?? (object)DBNull.Value)
                }
            );
        }

        #endregion

        #region In-App Notification Methods

        public async Task<int> GetUnreadCountAsync(string userId, string? userType = null)
        {
            string query = $@"SELECT COUNT(*)
                              FROM {Table.SysNotifications}
                              WHERE c_userid = @UserId
                                AND c_user_type = @UserType
                                AND c_is_read = 0
                                AND c_is_deleted = 0
                                AND (c_expires_at IS NULL OR c_expires_at > GETDATE())";

            var result = await _dbHelper.ExecuteScalarAsync(
                query,
                BuildUserParameters(userId, userType)
            );

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task MarkAsReadAsync(string notificationId, string userId)
        {
            string query = $@"UPDATE {Table.SysNotifications}
                              SET c_is_read = 1,
                                  c_read_at = GETDATE()
                              WHERE c_notification_uuid = @NotificationId
                                AND c_userid = @UserId
                                AND c_is_read = 0";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                new[]
                {
                    new SqlParameter("@NotificationId", notificationId),
                    new SqlParameter("@UserId", userId)
                }
            );
        }

        public async Task<List<InAppNotificationDto>> GetNotificationsAsync(string userId, string? userType = null, int pageSize = 20, int pageNumber = 1)
        {
            var offset = (pageNumber - 1) * pageSize;
            var parameters = BuildUserParameters(
                userId,
                userType,
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offset));

            string query = $@"SELECT
                                    c_notification_uuid AS NotificationId,
                                    c_title AS Title,
                                    c_message AS Message,
                                    c_category AS Category,
                                    c_priority AS Priority,
                                    c_action_url AS ActionUrl,
                                    c_icon_url AS IconUrl,
                                    c_is_read AS IsRead,
                                    c_createddate AS CreatedAt
                               FROM {Table.SysNotifications}
                               WHERE c_userid = @UserId
                                 AND c_user_type = @UserType
                                 AND c_is_deleted = 0
                                 AND (c_expires_at IS NULL OR c_expires_at > GETDATE())
                               ORDER BY c_priority DESC, c_createddate DESC
                               OFFSET @Offset ROWS
                               FETCH NEXT @PageSize ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(
                query,
                parameters
            );

            return dt.Rows.Cast<DataRow>()
                .Select(MapInAppNotification)
                .ToList();
        }

        public async Task SaveInAppNotificationAsync(InAppNotification notification, CancellationToken cancellationToken = default)
        {
            string query = $@"INSERT INTO {Table.SysNotifications}
                              (c_userid, c_user_type, c_title, c_message, c_category,
                               c_priority, c_action_url, c_icon_url, c_data, c_expires_at, c_createddate)
                              VALUES
                              (@UserId, @UserType, @Title, @Message, @Category,
                               @Priority, @ActionUrl, @IconUrl, @Data, @ExpiresAt, GETDATE())";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                new[]
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
                }
            );
        }

        public async Task MarkAllAsReadAsync(string userId, string userType = DefaultUserType)
        {
            string query = $@"UPDATE {Table.SysNotifications}
                              SET c_is_read = 1,
                                  c_read_at = GETDATE()
                              WHERE c_userid = @UserId
                                AND c_user_type = @UserType
                                AND c_is_read = 0
                                AND c_is_deleted = 0";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                BuildUserParameters(userId, userType)
            );
        }

        public async Task DeleteNotificationAsync(string notificationId, string userId)
        {
            string query = $@"UPDATE {Table.SysNotifications}
                              SET c_is_deleted = 1,
                                  c_deleted_at = GETDATE()
                              WHERE c_notification_uuid = @NotificationId
                                AND c_userid = @UserId";

            await _dbHelper.ExecuteNonQueryAsync(
                query,
                new[]
                {
                    new SqlParameter("@NotificationId", notificationId),
                    new SqlParameter("@UserId", userId)
                }
            );
        }

        #endregion

        #region Private Helpers

        private static SqlParameter[] BuildDeliveryParameters(NotificationDelivery delivery)
        {
            return new[]
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
        }

        private static SqlParameter[] BuildUserParameters(string userId, string? userType, params SqlParameter[] additionalParameters)
        {
            var parameters = new SqlParameter[2 + additionalParameters.Length];
            parameters[0] = new SqlParameter("@UserId", userId);
            parameters[1] = new SqlParameter("@UserType", userType ?? DefaultUserType);

            if (additionalParameters.Length > 0)
            {
                Array.Copy(additionalParameters, 0, parameters, 2, additionalParameters.Length);
            }

            return parameters;
        }

        private static NotificationDelivery MapNotificationDelivery(DataRow row)
        {
            return new NotificationDelivery
            {
                Id = row.GetValue<long>("c_id"),
                NotificationId = row.GetValue<string>("c_notification_id", string.Empty),
                Channel = row.GetValue<string>("c_channel", string.Empty),
                Status = row.GetValue<string>("c_status", string.Empty),
                Provider = row.GetValue<string?>("c_provider", null),
                ProviderMessageId = row.GetValue<string?>("c_provider_message_id", null),
                Recipient = row.GetValue<string>("c_recipient", string.Empty),
                SentAt = row.GetValue<DateTime>("c_sent_at"),
                DeliveredAt = row.GetValue<DateTime?>("c_delivered_at", null),
                ErrorMessage = row.GetValue<string?>("c_error_message", null),
                RetryCount = row.GetValue<int>("c_retry_count"),
                Cost = row.GetValue<decimal?>("c_cost", null)
            };
        }

        private static InAppNotificationDto MapInAppNotification(DataRow row)
        {
            return new InAppNotificationDto
            {
                NotificationId = row.GetValue<string>("NotificationId", string.Empty),
                Title = row.GetValue<string>("Title", string.Empty),
                Message = row.GetValue<string>("Message", string.Empty),
                Category = row.GetValue<string>("Category", string.Empty),
                Priority = row.GetValue<int>("Priority"),
                ActionUrl = row.GetValue<string>("ActionUrl", string.Empty),
                IconUrl = row.GetValue<string>("IconUrl", string.Empty),
                IsRead = row.GetBoolean("IsRead"),
                CreatedAt = row.GetValue<DateTime>("CreatedAt")
            };
        }

        #endregion
    }
}
