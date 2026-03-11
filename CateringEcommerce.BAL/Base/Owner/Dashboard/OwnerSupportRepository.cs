using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
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
        private readonly IDatabaseHelper _dbHelper;

        public OwnerSupportRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        /// <summary>
        /// Create a new support ticket
        /// </summary>
        public async Task<SupportTicketItemDto> CreateTicket(long ownerId, CreateSupportTicketDto dto)
        {
            // Generate ticket number: TKT-YYYYMMDD-NNN
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var countSql = $@"
                SELECT COUNT(*) + 1 FROM {Table.SysSupportTickets}
                WHERE CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE)";

            var seqObj = await _dbHelper.ExecuteScalarAsync(countSql);
            var seqNum = seqObj == null || seqObj == DBNull.Value ? 1 : Convert.ToInt32(seqObj);

            var ticketNumber = $"TKT-{dateStr}-{seqNum:D3}";

            var insertSql = $@"
                INSERT INTO {Table.SysSupportTickets}
                    (c_ticket_number, c_ownerid, c_subject, c_description, c_category, c_priority, c_status, c_related_order_id, c_createddate)
                VALUES
                    (@TicketNumber, @OwnerId, @Subject, @Description, @Category, @Priority, 'Open', @RelatedOrderId, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var insertParams = new[]
            {
                new SqlParameter("@TicketNumber", ticketNumber),
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@Subject", dto.Subject),
                new SqlParameter("@Description", dto.Description),
                new SqlParameter("@Category", dto.Category),
                new SqlParameter("@Priority", dto.Priority ?? "Medium"),
                new SqlParameter("@RelatedOrderId", (object?)dto.RelatedOrderId ?? DBNull.Value)
            };

            var ticketIdObj = await _dbHelper.ExecuteScalarAsync(insertSql, insertParams);
            var ticketId = ticketIdObj == null || ticketIdObj == DBNull.Value ? 0L : Convert.ToInt64(ticketIdObj);

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

            // Build WHERE clause
            var where = new StringBuilder($"WHERE t.c_ownerid = @OwnerId");
            if (!string.IsNullOrEmpty(filter.Status))
                where.Append(" AND t.c_status = @Status");
            if (!string.IsNullOrEmpty(filter.Category))
                where.Append(" AND t.c_category = @Category");

            // Count
            var countSql = $"SELECT COUNT(*) FROM {Table.SysSupportTickets} t {where}";
            var countParams = new List<SqlParameter>
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            if (!string.IsNullOrEmpty(filter.Status))
                countParams.Add(new SqlParameter("@Status", filter.Status));
            if (!string.IsNullOrEmpty(filter.Category))
                countParams.Add(new SqlParameter("@Category", filter.Category));

            var totalObj = await _dbHelper.ExecuteScalarAsync(countSql, countParams.ToArray());
            result.TotalCount = totalObj == null || totalObj == DBNull.Value ? 0 : Convert.ToInt32(totalObj);

            result.TotalPages = filter.PageSize > 0
                ? (int)Math.Ceiling((double)result.TotalCount / filter.PageSize)
                : 0;

            // Sort
            var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CreatedDate", "Priority" };
            var sortCol = allowedSorts.Contains(filter.SortBy ?? "") ? filter.SortBy : "CreatedDate";
            var sortDir = string.Equals(filter.SortOrder, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            var sortExpression = sortCol == "Priority"
                ? "CASE t.c_priority WHEN 'Urgent' THEN 1 WHEN 'High' THEN 2 WHEN 'Medium' THEN 3 WHEN 'Low' THEN 4 ELSE 5 END"
                : "t.c_createddate";

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
                    t.c_createddate AS CreatedDate,
                    t.c_resolved_date AS ResolvedDate,
                    (SELECT COUNT(*) FROM {Table.SysSupportTicketMessages} m WHERE m.c_ticket_id = t.c_ticket_id) AS MessageCount
                FROM {Table.SysSupportTickets} t
                {where}
                ORDER BY {sortExpression} {sortDir}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dataParams = new List<SqlParameter>
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@Offset", (filter.Page - 1) * filter.PageSize),
                new SqlParameter("@PageSize", filter.PageSize)
            };
            if (!string.IsNullOrEmpty(filter.Status))
                dataParams.Add(new SqlParameter("@Status", filter.Status));
            if (!string.IsNullOrEmpty(filter.Category))
                dataParams.Add(new SqlParameter("@Category", filter.Category));

            var dataTable = await _dbHelper.ExecuteAsync(dataSql, dataParams.ToArray());
            foreach (DataRow row in dataTable.Rows)
            {
                result.Tickets.Add(new SupportTicketItemDto
                {
                    TicketId = row.GetValue<long>("TicketId"),
                    TicketNumber = row.GetValue<string>("TicketNumber", string.Empty),
                    Subject = row.GetValue<string>("Subject", string.Empty),
                    Description = row.GetValue<string>("Description", string.Empty),
                    Category = row.GetValue<string>("Category", string.Empty),
                    Priority = row.GetValue<string>("Priority", string.Empty),
                    Status = row.GetValue<string>("Status", string.Empty),
                    RelatedOrderId = row.GetValue<long?>("RelatedOrderId"),
                    CreatedDate = row.GetValue<DateTime>("CreatedDate"),
                    ResolvedDate = row.GetValue<DateTime?>("ResolvedDate"),
                    MessageCount = row.GetValue<int>("MessageCount")
                });
            }

            return result;
        }

        /// <summary>
        /// Get ticket details with messages
        /// </summary>
        public async Task<SupportTicketDetailDto?> GetTicketDetail(long ownerId, long ticketId)
        {
            // Get ticket
            var ticketSql = $@"
                SELECT
                    c_ticket_id, c_ticket_number, c_subject, c_description,
                    c_category, c_priority, c_status, c_related_order_id,
                    c_resolution_notes, c_resolved_date, c_createddate
                FROM {Table.SysSupportTickets}
                WHERE c_ticket_id = @TicketId AND c_ownerid = @OwnerId";

            SupportTicketDetailDto? detail = null;
            var ticketParams = new[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@OwnerId", ownerId)
            };
            var ticketTable = await _dbHelper.ExecuteAsync(ticketSql, ticketParams);

            if (ticketTable.Rows.Count > 0)
            {
                var row = ticketTable.Rows[0];
                detail = new SupportTicketDetailDto
                {
                    TicketId = row.GetValue<long>("c_ticket_id"),
                    TicketNumber = row.GetValue<string>("c_ticket_number", string.Empty),
                    Subject = row.GetValue<string>("c_subject", string.Empty),
                    Description = row.GetValue<string>("c_description", string.Empty),
                    Category = row.GetValue<string>("c_category", string.Empty),
                    Priority = row.GetValue<string>("c_priority", string.Empty),
                    Status = row.GetValue<string>("c_status", string.Empty),
                    RelatedOrderId = row.GetValue<long?>("c_related_order_id"),
                    ResolutionNotes = row.GetValue<string?>("c_resolution_notes"),
                    ResolvedDate = row.GetValue<DateTime?>("c_resolved_date"),
                    CreatedDate = row.GetValue<DateTime>("c_createddate")
                };
            }

            if (detail == null) return null;

            // Get messages
            var msgSql = $@"
                SELECT c_message_id, c_sender_type, c_message_text, c_createddate
                FROM {Table.SysSupportTicketMessages}
                WHERE c_ticket_id = @TicketId
                ORDER BY c_createddate ASC";

            var msgParams = new[] { new SqlParameter("@TicketId", ticketId) };
            var msgTable = await _dbHelper.ExecuteAsync(msgSql, msgParams);
            foreach (DataRow row in msgTable.Rows)
            {
                detail.Messages.Add(new TicketMessageDto
                {
                    MessageId = row.GetValue<long>("c_message_id"),
                    SenderType = row.GetValue<string>("c_sender_type", string.Empty),
                    MessageText = row.GetValue<string>("c_message_text", string.Empty),
                    CreatedDate = row.GetValue<DateTime>("c_createddate")
                });
            }

            return detail;
        }

        /// <summary>
        /// Send a message on a ticket
        /// </summary>
        public async Task<TicketMessageDto?> SendMessage(long ownerId, long ticketId, string messageText)
        {
            // Verify ownership
            var verifySql = $"SELECT c_status FROM {Table.SysSupportTickets} WHERE c_ticket_id = @TicketId AND c_ownerid = @OwnerId";
            var verifyParams = new[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@OwnerId", ownerId)
            };
            var statusObj = await _dbHelper.ExecuteScalarAsync(verifySql, verifyParams);
            var status = statusObj?.ToString();

            if (status == null) return null;
            if (status == "Closed" || status == "Resolved")
                throw new InvalidOperationException("Cannot send messages on a resolved or closed ticket.");

            var insertSql = $@"
                INSERT INTO {Table.SysSupportTicketMessages}
                    (c_ticket_id, c_sender_type, c_sender_id, c_message_text, c_createddate)
                VALUES
                    (@TicketId, 'Owner', @OwnerId, @MessageText, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var insertParams = new[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@MessageText", messageText)
            };
            var messageIdObj = await _dbHelper.ExecuteScalarAsync(insertSql, insertParams);
            var messageId = messageIdObj == null || messageIdObj == DBNull.Value ? 0L : Convert.ToInt64(messageIdObj);

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

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = await _dbHelper.ExecuteAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                return new SupportTicketStatsDto
                {
                    TotalTickets = row.GetValue<int>("TotalTickets"),
                    OpenTickets = row.GetValue<int>("OpenTickets"),
                    InProgressTickets = row.GetValue<int>("InProgressTickets"),
                    ResolvedTickets = row.GetValue<int>("ResolvedTickets"),
                    ClosedTickets = row.GetValue<int>("ClosedTickets")
                };
            }

            return new SupportTicketStatsDto();
        }
    }
}
