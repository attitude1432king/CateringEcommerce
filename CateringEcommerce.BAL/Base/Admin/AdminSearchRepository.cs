using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminSearchRepository : IAdminSearchRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly IRBACRepository _rbacRepo;

        public AdminSearchRepository(IDatabaseHelper dbHelper, IRBACRepository rbacRepo)
        {
            _dbHelper = dbHelper;
            _rbacRepo = rbacRepo;
        }

        public async Task<GlobalSearchResponse> GlobalSearchAsync(GlobalSearchRequest request, long adminId)
        {
            var query = request.Query.Trim();
            if (query.Length < 2)
            {
                return new GlobalSearchResponse { Query = query };
            }

            var permCodes = new HashSet<string>(await _rbacRepo.GetAdminPermissionsAsync(adminId));
            bool isSuperAdmin = await _rbacRepo.IsSuperAdminAsync(adminId);

            var permissionsUsed = new List<string>();

            var userTask = (isSuperAdmin || permCodes.Contains("USER_VIEW"))
                ? SearchUsersAsync(query, request.MaxResultsPerModule)
                : Task.FromResult(new List<GlobalSearchResultItem>());
            if (isSuperAdmin || permCodes.Contains("USER_VIEW")) permissionsUsed.Add("USER_VIEW");

            var partnerTask = (isSuperAdmin || permCodes.Contains("CATERING_VIEW"))
                ? SearchPartnersAsync(query, request.MaxResultsPerModule)
                : Task.FromResult(new List<GlobalSearchResultItem>());
            if (isSuperAdmin || permCodes.Contains("CATERING_VIEW")) permissionsUsed.Add("CATERING_VIEW");

            var orderTask = (isSuperAdmin || permCodes.Contains("ORDER_VIEW"))
                ? SearchOrdersAsync(query, request.MaxResultsPerModule)
                : Task.FromResult(new List<GlobalSearchResultItem>());
            if (isSuperAdmin || permCodes.Contains("ORDER_VIEW")) permissionsUsed.Add("ORDER_VIEW");

            var supTask = (isSuperAdmin || permCodes.Contains("SUPERVISOR_VIEW"))
                ? SearchSupervisorsAsync(query, request.MaxResultsPerModule)
                : Task.FromResult(new List<GlobalSearchResultItem>());
            if (isSuperAdmin || permCodes.Contains("SUPERVISOR_VIEW")) permissionsUsed.Add("SUPERVISOR_VIEW");

            var earnTask = (isSuperAdmin || permCodes.Contains("EARNINGS_VIEW"))
                ? SearchEarningsAsync(query, request.MaxResultsPerModule)
                : Task.FromResult(new List<GlobalSearchResultItem>());
            if (isSuperAdmin || permCodes.Contains("EARNINGS_VIEW")) permissionsUsed.Add("EARNINGS_VIEW");

            await Task.WhenAll(userTask, partnerTask, orderTask, supTask, earnTask);

            var allResults = userTask.Result
                .Concat(partnerTask.Result)
                .Concat(orderTask.Result)
                .Concat(supTask.Result)
                .Concat(earnTask.Result)
                .ToList();

            return new GlobalSearchResponse
            {
                Results = allResults,
                TotalCount = allResults.Count,
                Query = query,
                PermissionsUsed = permissionsUsed.Distinct().ToList()
            };
        }

        // ==========================================
        // MODULE: USERS
        // ==========================================
        private async Task<List<GlobalSearchResultItem>> SearchUsersAsync(string query, int max)
        {
            var sql = $@"
                SELECT
                    c_userid,
                    COALESCE(c_name, '') AS c_name,
                    COALESCE(c_email, '') AS c_email,
                    COALESCE(c_mobile, '') AS c_mobile,
                    CASE WHEN COALESCE(c_isblocked, FALSE) = TRUE THEN 'Blocked'
                         WHEN COALESCE(c_isactive, TRUE) = FALSE  THEN 'Inactive'
                         ELSE 'Active' END AS Status,
                    CASE WHEN COALESCE(c_isblocked, FALSE) = TRUE THEN 'red'
                         WHEN COALESCE(c_isactive, TRUE) = FALSE  THEN 'gray'
                         ELSE 'green' END AS StatusColor
                FROM {Table.SysUser}
                WHERE (c_name LIKE @Q OR c_email LIKE @Q OR c_mobile LIKE @Q)
                  AND COALESCE(c_is_deleted, FALSE) = FALSE
                ORDER BY c_createddate DESC
                LIMIT @Max";

            var parameters = new[]
            {
                new NpgsqlParameter("@Max", max),
                new NpgsqlParameter("@Q", $"%{query}%")
            };

            var dt = await _dbHelper.ExecuteAsync(sql, parameters);
            var results = new List<GlobalSearchResultItem>();

            foreach (DataRow row in dt.Rows)
            {
                var id = Convert.ToInt64(row["c_userid"]);
                var name = row["c_name"].ToString() ?? string.Empty;
                var email = row["c_email"].ToString() ?? string.Empty;
                var mobile = row["c_mobile"].ToString() ?? string.Empty;

                results.Add(new GlobalSearchResultItem
                {
                    Type = "USER",
                    Id = id,
                    Title = name,
                    Subtitle = string.Join(" | ", new[] { email, mobile }.Where(s => !string.IsNullOrEmpty(s))),
                    Status = row["Status"].ToString() ?? "Active",
                    StatusColor = row["StatusColor"].ToString() ?? "green",
                    ViewUrl = $"/admin/users?userId={id}",
                    ModuleLabel = "Customer",
                    MatchedOn = DetermineMatch(query, name, email, mobile)
                });
            }

            return results;
        }

        // ==========================================
        // MODULE: PARTNERS / OWNERS
        // ==========================================
        private async Task<List<GlobalSearchResultItem>> SearchPartnersAsync(string query, int max)
        {
            var sql = $@"
                SELECT
                    c_ownerid,
                    COALESCE(c_catering_name, '') AS c_catering_name,
                    COALESCE(c_owner_name, '') AS c_owner_name,
                    COALESCE(c_email, '') AS c_email,
                    COALESCE(c_mobile, '') AS c_mobile,
                    c_approval_status,
                    CASE c_approval_status
                         WHEN 2 THEN 'Approved'
                         WHEN 3 THEN 'Rejected'
                         ELSE 'Pending' END AS Status,
                    CASE c_approval_status
                         WHEN 2 THEN 'green'
                         WHEN 3 THEN 'red'
                         ELSE 'yellow' END AS StatusColor
                FROM {Table.SysCateringOwner}
                WHERE (c_catering_name LIKE @Q OR c_owner_name LIKE @Q OR c_email LIKE @Q)
                  AND COALESCE(c_is_deleted, FALSE) = FALSE
                ORDER BY c_createddate DESC
                LIMIT @Max";

            var parameters = new[]
            {
                new NpgsqlParameter("@Max", max),
                new NpgsqlParameter("@Q", $"%{query}%")
            };

            var dt = await _dbHelper.ExecuteAsync(sql, parameters);
            var results = new List<GlobalSearchResultItem>();

            foreach (DataRow row in dt.Rows)
            {
                var id = Convert.ToInt64(row["c_ownerid"]);
                var cateringName = row["c_catering_name"].ToString() ?? string.Empty;
                var ownerName = row["c_owner_name"].ToString() ?? string.Empty;
                var email = row["c_email"].ToString() ?? string.Empty;
                var approvalStatus = row["c_approval_status"] != DBNull.Value ? Convert.ToInt32(row["c_approval_status"]) : 0;

                var viewUrl = approvalStatus == 2
                    ? $"/admin/caterings?cateringId={id}"
                    : $"/admin/partner-requests?ownerId={id}";

                results.Add(new GlobalSearchResultItem
                {
                    Type = "PARTNER",
                    Id = id,
                    Title = cateringName,
                    Subtitle = string.Join(" | ", new[] { ownerName, email }.Where(s => !string.IsNullOrEmpty(s))),
                    Status = row["Status"].ToString() ?? "Pending",
                    StatusColor = row["StatusColor"].ToString() ?? "yellow",
                    ViewUrl = viewUrl,
                    ModuleLabel = "Partner",
                    MatchedOn = DetermineMatch(query, cateringName, ownerName, email)
                });
            }

            return results;
        }

        // ==========================================
        // MODULE: ORDERS
        // ==========================================
        private async Task<List<GlobalSearchResultItem>> SearchOrdersAsync(string query, int max)
        {
            var sql = $@"
                SELECT
                    o.c_orderid,
                    COALESCE(o.c_order_number, CAST(o.c_orderid AS VARCHAR)) AS c_order_number,
                    COALESCE(u.c_name, '') AS c_name,
                    COALESCE(CAST(o.c_total_amount AS VARCHAR), '0') AS c_total_amount,
                    COALESCE(o.c_order_status, 'Pending') AS Status,
                    CASE COALESCE(o.c_order_status, 'Pending')
                         WHEN 'Completed'  THEN 'green'
                         WHEN 'Cancelled'  THEN 'red'
                         WHEN 'Pending'    THEN 'yellow'
                         WHEN 'Confirmed'  THEN 'blue'
                         WHEN 'InProgress' THEN 'blue'
                         ELSE 'gray' END AS StatusColor
                FROM {Table.SysOrders} o
                INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                WHERE (CAST(o.c_orderid AS VARCHAR) = @RawQ
                    OR COALESCE(o.c_order_number, '') LIKE @Q
                    OR u.c_name LIKE @Q)
                ORDER BY o.c_createddate DESC
                LIMIT @Max";

            var parameters = new[]
            {
                new NpgsqlParameter("@Max", max),
                new NpgsqlParameter("@Q", $"%{query}%"),
                new NpgsqlParameter("@RawQ", query)
            };

            var dt = await _dbHelper.ExecuteAsync(sql, parameters);
            var results = new List<GlobalSearchResultItem>();

            foreach (DataRow row in dt.Rows)
            {
                var id = Convert.ToInt64(row["c_orderid"]);
                var orderNum = row["c_order_number"].ToString() ?? string.Empty;
                var customerName = row["c_name"].ToString() ?? string.Empty;
                var amount = row["c_total_amount"].ToString() ?? "0";

                results.Add(new GlobalSearchResultItem
                {
                    Type = "ORDER",
                    Id = id,
                    Title = orderNum,
                    Subtitle = string.Join(" | ", new[] { customerName, $"₹{amount}" }.Where(s => !string.IsNullOrEmpty(s))),
                    Status = row["Status"].ToString() ?? "Pending",
                    StatusColor = row["StatusColor"].ToString() ?? "gray",
                    ViewUrl = $"/admin/orders?orderId={id}",
                    ModuleLabel = "Order",
                    MatchedOn = DetermineMatch(query, orderNum, customerName, id.ToString())
                });
            }

            return results;
        }

        // ==========================================
        // MODULE: SUPERVISORS
        // ==========================================
        private async Task<List<GlobalSearchResultItem>> SearchSupervisorsAsync(string query, int max)
        {
            var sql = $@"
                SELECT
                    c_supervisor_id,
                    COALESCE(c_full_name, '') AS c_full_name,
                    COALESCE(c_email, '') AS c_email,
                    COALESCE(c_phone, '') AS c_phone,
                    COALESCE(c_current_status, 'APPLIED') AS c_current_status,
                    CASE COALESCE(c_current_status, 'APPLIED')
                         WHEN 'ACTIVE'     THEN 'Active'
                         WHEN 'SUSPENDED'  THEN 'Suspended'
                         ELSE 'Pending' END AS Status,
                    CASE COALESCE(c_current_status, 'APPLIED')
                         WHEN 'ACTIVE'     THEN 'green'
                         WHEN 'SUSPENDED'  THEN 'red'
                         ELSE 'yellow' END AS StatusColor
                FROM {Table.SysSupervisor}
                WHERE (c_full_name LIKE @Q OR c_email LIKE @Q)
                  AND COALESCE(c_is_deleted, FALSE) = FALSE
                ORDER BY c_createddate DESC
                LIMIT @Max";

            var parameters = new[]
            {
                new NpgsqlParameter("@Max", max),
                new NpgsqlParameter("@Q", $"%{query}%")
            };

            var dt = await _dbHelper.ExecuteAsync(sql, parameters);
            var results = new List<GlobalSearchResultItem>();

            foreach (DataRow row in dt.Rows)
            {
                var id = Convert.ToInt64(row["c_supervisor_id"]);
                var fullName = row["c_full_name"].ToString() ?? string.Empty;
                var email = row["c_email"].ToString() ?? string.Empty;
                var phone = row["c_phone"].ToString() ?? string.Empty;
                var rawStatus = row["c_current_status"].ToString() ?? "APPLIED";

                var viewUrl = rawStatus == "ACTIVE"
                    ? $"/admin/supervisor-management/approved?supervisorId={id}"
                    : $"/admin/supervisor-management/pending?supervisorId={id}";

                results.Add(new GlobalSearchResultItem
                {
                    Type = "SUPERVISOR",
                    Id = id,
                    Title = fullName,
                    Subtitle = string.Join(" | ", new[] { email, phone }.Where(s => !string.IsNullOrEmpty(s))),
                    Status = row["Status"].ToString() ?? "Pending",
                    StatusColor = row["StatusColor"].ToString() ?? "yellow",
                    ViewUrl = viewUrl,
                    ModuleLabel = "Supervisor",
                    MatchedOn = DetermineMatch(query, fullName, email),
                    ExtraData = new Dictionary<string, object?> { { "rawStatus", rawStatus } }
                });
            }

            return results;
        }

        // ==========================================
        // MODULE: EARNINGS
        // ==========================================
        private async Task<List<GlobalSearchResultItem>> SearchEarningsAsync(string query, int max)
        {
            var sql = $@"
                SELECT
                    o.c_orderid,
                    COALESCE(o.c_order_number, CAST(o.c_orderid AS VARCHAR)) AS c_order_number,
                    COALESCE(co.c_catering_name, '') AS c_catering_name,
                    COALESCE(CAST(o.c_platform_commission AS VARCHAR), '0') AS c_commission,
                    COALESCE(o.c_order_status, 'Pending') AS Status,
                    CASE COALESCE(o.c_order_status, 'Pending')
                         WHEN 'Completed' THEN 'green'
                         WHEN 'Cancelled' THEN 'red'
                         ELSE 'yellow' END AS StatusColor
                FROM {Table.SysOrders} o
                INNER JOIN {Table.SysCateringOwner} co ON o.c_ownerid = co.c_ownerid
                WHERE (COALESCE(o.c_order_number, '') LIKE @Q
                    OR co.c_catering_name LIKE @Q
                    OR CAST(o.c_orderid AS VARCHAR) = @RawQ)
                ORDER BY o.c_createddate DESC
                LIMIT @Max";

            var parameters = new[]
            {
                new NpgsqlParameter("@Max", max),
                new NpgsqlParameter("@Q", $"%{query}%"),
                new NpgsqlParameter("@RawQ", query)
            };

            var dt = await _dbHelper.ExecuteAsync(sql, parameters);
            var results = new List<GlobalSearchResultItem>();

            foreach (DataRow row in dt.Rows)
            {
                var id = Convert.ToInt64(row["c_orderid"]);
                var orderNum = row["c_order_number"].ToString() ?? string.Empty;
                var cateringName = row["c_catering_name"].ToString() ?? string.Empty;
                var commission = row["c_commission"].ToString() ?? "0";

                results.Add(new GlobalSearchResultItem
                {
                    Type = "EARNINGS",
                    Id = id,
                    Title = orderNum,
                    Subtitle = $"{cateringName} | ₹{commission} commission",
                    Status = row["Status"].ToString() ?? "Pending",
                    StatusColor = row["StatusColor"].ToString() ?? "yellow",
                    ViewUrl = "/admin/earnings",
                    ModuleLabel = "Earnings",
                    MatchedOn = DetermineMatch(query, orderNum, cateringName, id.ToString())
                });
            }

            return results;
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private static string DetermineMatch(string query, params string[] fields)
        {
            var lowerQuery = query.ToLowerInvariant();
            var labels = new[] { "name", "email", "phone", "id", "reference" };
            for (int i = 0; i < fields.Length; i++)
            {
                if (!string.IsNullOrEmpty(fields[i]) &&
                    fields[i].ToLowerInvariant().Contains(lowerQuery))
                {
                    return i < labels.Length ? labels[i] : "field";
                }
            }
            return "other";
        }
    }
}

