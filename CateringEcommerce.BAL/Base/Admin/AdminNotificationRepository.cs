using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminNotificationRepository : IAdminNotificationRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminNotificationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<AdminNotificationListResponse> GetNotifications(AdminNotificationListRequest request, long? adminId = null)
        {
            var response = new AdminNotificationListResponse();

            var whereClause = "WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // Filter by admin ID
            if (adminId.HasValue)
            {
                whereClause += " AND (c_adminid = @AdminId OR c_adminid IS NULL)";
                parameters.Add(new SqlParameter("@AdminId", adminId.Value));
            }

            // Filter by read status
            if (request.IsRead.HasValue)
            {
                whereClause += " AND c_is_read = @IsRead";
                parameters.Add(new SqlParameter("@IsRead", request.IsRead.Value));
            }

            // Filter by notification type
            if (!string.IsNullOrEmpty(request.NotificationType))
            {
                whereClause += " AND c_notification_type = @NotificationType";
                parameters.Add(new SqlParameter("@NotificationType", request.NotificationType));
            }

            // Get notifications
            var query = $@"
                SELECT
                    c_notification_id AS NotificationId,
                    c_notification_type AS NotificationType,
                    c_title AS Title,
                    c_message AS Message,
                    c_entity_id AS EntityId,
                    c_entity_type AS EntityType,
                    c_link AS Link,
                    c_is_read AS IsRead,
                    c_read_date AS ReadDate,
                    c_createddate AS CreatedAt
                FROM {Table.SysAdminNotifications}
                {whereClause}
                ORDER BY c_createddate DESC
                OFFSET {(request.PageNumber - 1) * request.PageSize} ROWS
                FETCH NEXT {request.PageSize} ROWS ONLY";

            var dataTable = _dbHelper.Execute(query, parameters.ToArray());

            foreach (DataRow row in dataTable.Rows)
            {
                response.Notifications.Add(new AdminNotificationItem
                {
                    NotificationId = Convert.ToInt64(row["NotificationId"]),
                    NotificationType = row["NotificationType"].ToString() ?? string.Empty,
                    Title = row["Title"].ToString() ?? string.Empty,
                    Message = row["Message"] != DBNull.Value ? row["Message"].ToString() : null,
                    EntityId = row["EntityId"] != DBNull.Value ? Convert.ToInt64(row["EntityId"]) : null,
                    EntityType = row["EntityType"] != DBNull.Value ? row["EntityType"].ToString() : null,
                    Link = row["Link"] != DBNull.Value ? row["Link"].ToString() : null,
                    IsRead = row["IsRead"] != DBNull.Value && Convert.ToBoolean(row["IsRead"]),
                    ReadDate = row["ReadDate"] != DBNull.Value ? Convert.ToDateTime(row["ReadDate"]) : null,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                });
            }

            // Get total count
            var countQuery = $@"
                SELECT COUNT(*) FROM {Table.SysAdminNotifications}
                {whereClause}";

            var responseResult = _dbHelper.ExecuteScalar(countQuery, parameters.ToArray());
            response.TotalCount = responseResult != null ? Convert.ToInt32(responseResult) : 0;

            // Get unread count
            response.UnreadCount = await GetUnreadCount(adminId);

            return response;
        }

        public async Task<int> GetUnreadCount(long? adminId = null)
        {
            var whereClause = "WHERE c_is_read = 0";
            var parameters = new List<SqlParameter>();

            if (adminId.HasValue)
            {
                whereClause += " AND (c_adminid = @AdminId OR c_adminid IS NULL)";
                parameters.Add(new SqlParameter("@AdminId", adminId.Value));
            }

            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdminNotifications}
                {whereClause}";

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());
            return result != null ? Convert.ToInt16(result) : 0;
        }

        public async Task<bool> MarkAsRead(long notificationId, long adminId)
        {
            try
            {
                var query = $@"
                    UPDATE {Table.SysAdminNotifications}
                    SET c_is_read = 1,
                        c_read_date = GETDATE()
                    WHERE c_notification_id = @NotificationId
                    AND (c_adminid = @AdminId OR c_adminid IS NULL)";

                var parameters = new[]
                {
                    new SqlParameter("@NotificationId", notificationId),
                    new SqlParameter("@AdminId", adminId)
                };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkAllAsRead(long adminId)
        {
            try
            {
                var query = $@"
                    UPDATE {Table.SysAdminNotifications}
                    SET c_is_read = 1,
                        c_read_date = GETDATE()
                    WHERE c_is_read = 0
                    AND (c_adminid = @AdminId OR c_adminid IS NULL)";

                var parameters = new[]
                {
                    new SqlParameter("@AdminId", adminId)
                };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateNotification(string notificationType, string title, string? message, long? entityId, string? entityType, string? link, long? adminId = null)
        {
            try
            {
                var query = $@"
                    INSERT INTO {Table.SysAdminNotifications}
                    (c_adminid, c_notification_type, c_title, c_message, c_entity_id, c_entity_type, c_link, c_is_read, c_createddate)
                    VALUES
                    (@AdminId, @NotificationType, @Title, @Message, @EntityId, @EntityType, @Link, 0, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@AdminId", (object?)adminId ?? DBNull.Value),
                    new SqlParameter("@NotificationType", notificationType),
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Message", (object?)message ?? DBNull.Value),
                    new SqlParameter("@EntityId", (object?)entityId ?? DBNull.Value),
                    new SqlParameter("@EntityType", (object?)entityType ?? DBNull.Value),
                    new SqlParameter("@Link", (object?)link ?? DBNull.Value)
                };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteNotification(long notificationId)
        {
            try
            {
                var query = $"DELETE FROM {Table.SysAdminNotifications} WHERE c_notification_id = @NotificationId";
                var parameters = new[] { new SqlParameter("@NotificationId", notificationId) };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
