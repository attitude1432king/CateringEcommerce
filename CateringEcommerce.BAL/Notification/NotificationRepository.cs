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

        // Stub methods for in-app notifications - To be implemented
        public async Task<int> GetUnreadCountAsync(string userId, string? userType = null)
        {
            await Task.CompletedTask;
            return 0;
        }

        public async Task MarkAsReadAsync(string notificationId, string userId)
        {
            await Task.CompletedTask;
        }

        public async Task<List<InAppNotificationDto>> GetNotificationsAsync(string userId, string? userType = null, int pageSize = 20, int pageNumber = 1)
        {
            await Task.CompletedTask;
            return new List<InAppNotificationDto>();
        }

        public async Task SaveInAppNotificationAsync(InAppNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
