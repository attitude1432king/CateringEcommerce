using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Common.Admin
{
    public class AdminNotificationRepository : IAdminNotificationRepository
    {
        private readonly SqlDatabaseManager _db;

        public AdminNotificationRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public AdminNotificationListResponse GetNotifications(AdminNotificationListRequest request, long? adminId = null)
        {
            var response = new AdminNotificationListResponse();

            var whereClause = "WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // Filter by admin ID
            if (adminId.HasValue)
            {
                whereClause += " AND (c_admin_id = @AdminId OR c_admin_id IS NULL)";
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
                    c_created_date AS CreatedAt
                FROM t_sys_admin_notifications
                {whereClause}
                ORDER BY c_created_date DESC
                OFFSET {(request.PageNumber - 1) * request.PageSize} ROWS
                FETCH NEXT {request.PageSize} ROWS ONLY";

            var dataTable = _db.Execute(query, parameters.ToArray());

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
                SELECT COUNT(*) FROM t_sys_admin_notifications
                {whereClause}";

            var responseResult = _db.ExecuteScalar(countQuery, parameters.ToArray());
            response.TotalCount = responseResult != null ? Convert.ToInt32(responseResult) : 0;

            // Get unread count
            response.UnreadCount = GetUnreadCount(adminId);

            return response;
        }

        public int GetUnreadCount(long? adminId = null)
        {
            var whereClause = "WHERE c_is_read = 0";
            var parameters = new List<SqlParameter>();

            if (adminId.HasValue)
            {
                whereClause += " AND (c_admin_id = @AdminId OR c_admin_id IS NULL)";
                parameters.Add(new SqlParameter("@AdminId", adminId.Value));
            }

            var query = $@"
                SELECT COUNT(*)
                FROM t_sys_admin_notifications
                {whereClause}";

            return Convert.ToInt16(_db.ExecuteScalar(query, parameters.ToArray()));
        }

        public bool MarkAsRead(long notificationId, long adminId)
        {
            try
            {
                var query = @"
                    UPDATE t_sys_admin_notifications
                    SET c_is_read = 1,
                        c_read_date = GETDATE()
                    WHERE c_notification_id = @NotificationId
                    AND (c_admin_id = @AdminId OR c_admin_id IS NULL)";

                var parameters = new[]
                {
                    new SqlParameter("@NotificationId", notificationId),
                    new SqlParameter("@AdminId", adminId)
                };

                _db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MarkAllAsRead(long adminId)
        {
            try
            {
                var query = @"
                    UPDATE t_sys_admin_notifications
                    SET c_is_read = 1,
                        c_read_date = GETDATE()
                    WHERE c_is_read = 0
                    AND (c_admin_id = @AdminId OR c_admin_id IS NULL)";

                var parameters = new[]
                {
                    new SqlParameter("@AdminId", adminId)
                };

                _db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateNotification(string notificationType, string title, string? message, long? entityId, string? entityType, string? link, long? adminId = null)
        {
            try
            {
                var query = @"
                    INSERT INTO t_sys_admin_notifications
                    (c_admin_id, c_notification_type, c_title, c_message, c_entity_id, c_entity_type, c_link, c_is_read, c_created_date)
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

                _db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteNotification(long notificationId)
        {
            try
            {
                var query = "DELETE FROM t_sys_admin_notifications WHERE c_notification_id = @NotificationId";
                var parameters = new[] { new SqlParameter("@NotificationId", notificationId) };

                _db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
