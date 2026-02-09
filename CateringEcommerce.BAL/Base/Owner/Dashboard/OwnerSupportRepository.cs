using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerSupportRepository
    {
        private readonly string _connStr;

        public OwnerSupportRepository(string connStr)
        {
            _connStr = connStr ?? throw new ArgumentNullException(nameof(connStr));
        }

        /// <summary>
        /// Create a new support ticket
        /// </summary>
        public async Task<SupportTicketItemDto> CreateTicket(long ownerId, CreateSupportTicketDto dto)
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // Generate ticket number: TKT-YYYYMMDD-NNN
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var countSql = $@"
                SELECT COUNT(*) + 1 FROM {Table.SysSupportTickets}
                WHERE CAST(c_created_date AS DATE) = CAST(GETDATE() AS DATE)";

            int seqNum;
            using (var countCmd = new SqlCommand(countSql, conn))
            {
                seqNum = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            }

            var ticketNumber = $"TKT-{dateStr}-{seqNum:D3}";

            var insertSql = $@"
                INSERT INTO {Table.SysSupportTickets}
                    (c_ticket_number, c_ownerid, c_subject, c_description, c_category, c_priority, c_status, c_related_order_id, c_created_date)
                VALUES
                    (@TicketNumber, @OwnerId, @Subject, @Description, @Category, @Priority, 'Open', @RelatedOrderId, GETDATE());
                SELECT SCOPE_IDENTITY();";

            long ticketId;
            using (var insertCmd = new SqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("@TicketNumber", ticketNumber);
                insertCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                insertCmd.Parameters.AddWithValue("@Subject", dto.Subject);
                insertCmd.Parameters.AddWithValue("@Description", dto.Description);
                insertCmd.Parameters.AddWithValue("@Category", dto.Category);
                insertCmd.Parameters.AddWithValue("@Priority", dto.Priority ?? "Medium");
                insertCmd.Parameters.AddWithValue("@RelatedOrderId", (object?)dto.RelatedOrderId ?? DBNull.Value);

                ticketId = Convert.ToInt64(await insertCmd.ExecuteScalarAsync());
            }

            return new SupportTicketItemDto
            {
                TicketId = ticketId,
                TicketNumber = ticketNumber,
                Subject = dto.Subject,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority ?? "Medium",
                Status = "Open",
                RelatedOrderId = dto.RelatedOrderId,
                CreatedDate = DateTime.Now,
                MessageCount = 0
            };
        }

        /// <summary>
        /// Get paginated and filtered support tickets
        /// </summary>
        public async Task<PaginatedSupportTicketsDto> GetTickets(long ownerId, SupportTicketFilterDto filter)
        {
            var result = new PaginatedSupportTicketsDto
            {
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // Build WHERE clause
            var where = new StringBuilder($"WHERE t.c_ownerid = @OwnerId");
            if (!string.IsNullOrEmpty(filter.Status))
                where.Append(" AND t.c_status = @Status");
            if (!string.IsNullOrEmpty(filter.Category))
                where.Append(" AND t.c_category = @Category");

            // Count
            var countSql = $"SELECT COUNT(*) FROM {Table.SysSupportTickets} t {where}";
            using (var countCmd = new SqlCommand(countSql, conn))
            {
                countCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                if (!string.IsNullOrEmpty(filter.Status))
                    countCmd.Parameters.AddWithValue("@Status", filter.Status);
                if (!string.IsNullOrEmpty(filter.Category))
                    countCmd.Parameters.AddWithValue("@Category", filter.Category);

                result.TotalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            }

            result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / filter.PageSize);

            // Sort
            var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CreatedDate", "Priority" };
            var sortCol = allowedSorts.Contains(filter.SortBy ?? "") ? filter.SortBy : "CreatedDate";
            var sortDir = string.Equals(filter.SortOrder, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            var sortExpression = sortCol == "Priority"
                ? "CASE t.c_priority WHEN 'Urgent' THEN 1 WHEN 'High' THEN 2 WHEN 'Medium' THEN 3 WHEN 'Low' THEN 4 ELSE 5 END"
                : "t.c_created_date";

            var dataSql = $@"
                SELECT
                    t.c_ticket_id AS TicketId,
                    t.c_ticket_number AS TicketNumber,
                    t.c_subject AS Subject,
                    t.c_description AS Description,
                    t.c_category AS Category,
                    t.c_priority AS Priority,
                    t.c_status AS Status,
                    t.c_related_order_id AS RelatedOrderId,
                    t.c_created_date AS CreatedDate,
                    t.c_resolved_date AS ResolvedDate,
                    (SELECT COUNT(*) FROM {Table.SysSupportTicketMessages} m WHERE m.c_ticket_id = t.c_ticket_id) AS MessageCount
                FROM {Table.SysSupportTickets} t
                {where}
                ORDER BY {sortExpression} {sortDir}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using (var dataCmd = new SqlCommand(dataSql, conn))
            {
                dataCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                dataCmd.Parameters.AddWithValue("@Offset", (filter.Page - 1) * filter.PageSize);
                dataCmd.Parameters.AddWithValue("@PageSize", filter.PageSize);
                if (!string.IsNullOrEmpty(filter.Status))
                    dataCmd.Parameters.AddWithValue("@Status", filter.Status);
                if (!string.IsNullOrEmpty(filter.Category))
                    dataCmd.Parameters.AddWithValue("@Category", filter.Category);

                using var reader = await dataCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Tickets.Add(new SupportTicketItemDto
                    {
                        TicketId = reader.GetInt64(reader.GetOrdinal("TicketId")),
                        TicketNumber = reader.GetString(reader.GetOrdinal("TicketNumber")),
                        Subject = reader.GetString(reader.GetOrdinal("Subject")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Priority = reader.GetString(reader.GetOrdinal("Priority")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        RelatedOrderId = reader.IsDBNull(reader.GetOrdinal("RelatedOrderId")) ? null : reader.GetInt64(reader.GetOrdinal("RelatedOrderId")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        ResolvedDate = reader.IsDBNull(reader.GetOrdinal("ResolvedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ResolvedDate")),
                        MessageCount = reader.GetInt32(reader.GetOrdinal("MessageCount"))
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Get ticket details with messages
        /// </summary>
        public async Task<SupportTicketDetailDto?> GetTicketDetail(long ownerId, long ticketId)
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // Get ticket
            var ticketSql = $@"
                SELECT
                    c_ticket_id, c_ticket_number, c_subject, c_description,
                    c_category, c_priority, c_status, c_related_order_id,
                    c_resolution_notes, c_resolved_date, c_created_date
                FROM {Table.SysSupportTickets}
                WHERE c_ticket_id = @TicketId AND c_ownerid = @OwnerId";

            SupportTicketDetailDto? detail = null;

            using (var ticketCmd = new SqlCommand(ticketSql, conn))
            {
                ticketCmd.Parameters.AddWithValue("@TicketId", ticketId);
                ticketCmd.Parameters.AddWithValue("@OwnerId", ownerId);

                using var reader = await ticketCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    detail = new SupportTicketDetailDto
                    {
                        TicketId = reader.GetInt64(0),
                        TicketNumber = reader.GetString(1),
                        Subject = reader.GetString(2),
                        Description = reader.GetString(3),
                        Category = reader.GetString(4),
                        Priority = reader.GetString(5),
                        Status = reader.GetString(6),
                        RelatedOrderId = reader.IsDBNull(7) ? null : reader.GetInt64(7),
                        ResolutionNotes = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ResolvedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedDate = reader.GetDateTime(10)
                    };
                }
            }

            if (detail == null) return null;

            // Get messages
            var msgSql = $@"
                SELECT c_message_id, c_sender_type, c_message_text, c_created_date
                FROM {Table.SysSupportTicketMessages}
                WHERE c_ticket_id = @TicketId
                ORDER BY c_created_date ASC";

            using (var msgCmd = new SqlCommand(msgSql, conn))
            {
                msgCmd.Parameters.AddWithValue("@TicketId", ticketId);

                using var reader = await msgCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    detail.Messages.Add(new TicketMessageDto
                    {
                        MessageId = reader.GetInt64(0),
                        SenderType = reader.GetString(1),
                        MessageText = reader.GetString(2),
                        CreatedDate = reader.GetDateTime(3)
                    });
                }
            }

            return detail;
        }

        /// <summary>
        /// Send a message on a ticket
        /// </summary>
        public async Task<TicketMessageDto?> SendMessage(long ownerId, long ticketId, string messageText)
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // Verify ownership
            var verifySql = $"SELECT c_status FROM {Table.SysSupportTickets} WHERE c_ticket_id = @TicketId AND c_ownerid = @OwnerId";
            string? status;
            using (var verifyCmd = new SqlCommand(verifySql, conn))
            {
                verifyCmd.Parameters.AddWithValue("@TicketId", ticketId);
                verifyCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                status = (await verifyCmd.ExecuteScalarAsync())?.ToString();
            }

            if (status == null) return null;
            if (status == "Closed" || status == "Resolved")
                throw new InvalidOperationException("Cannot send messages on a resolved or closed ticket.");

            var insertSql = $@"
                INSERT INTO {Table.SysSupportTicketMessages}
                    (c_ticket_id, c_sender_type, c_sender_id, c_message_text, c_created_date)
                VALUES
                    (@TicketId, 'Owner', @OwnerId, @MessageText, GETDATE());
                SELECT SCOPE_IDENTITY();";

            long messageId;
            using (var insertCmd = new SqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("@TicketId", ticketId);
                insertCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                insertCmd.Parameters.AddWithValue("@MessageText", messageText);
                messageId = Convert.ToInt64(await insertCmd.ExecuteScalarAsync());
            }

            return new TicketMessageDto
            {
                MessageId = messageId,
                SenderType = "Owner",
                MessageText = messageText,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Get ticket statistics
        /// </summary>
        public async Task<SupportTicketStatsDto> GetTicketStats(long ownerId)
        {
            var sql = $@"
                SELECT
                    COUNT(*) AS TotalTickets,
                    SUM(CASE WHEN c_status = 'Open' THEN 1 ELSE 0 END) AS OpenTickets,
                    SUM(CASE WHEN c_status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressTickets,
                    SUM(CASE WHEN c_status = 'Resolved' THEN 1 ELSE 0 END) AS ResolvedTickets,
                    SUM(CASE WHEN c_status = 'Closed' THEN 1 ELSE 0 END) AS ClosedTickets
                FROM {Table.SysSupportTickets}
                WHERE c_ownerid = @OwnerId";

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SupportTicketStatsDto
                {
                    TotalTickets = reader.GetInt32(0),
                    OpenTickets = reader.GetInt32(1),
                    InProgressTickets = reader.GetInt32(2),
                    ResolvedTickets = reader.GetInt32(3),
                    ClosedTickets = reader.GetInt32(4)
                };
            }

            return new SupportTicketStatsDto();
        }
    }
}
