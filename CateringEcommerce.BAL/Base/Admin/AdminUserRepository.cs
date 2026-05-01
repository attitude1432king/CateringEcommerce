using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminUserRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminUserListResponse GetAllUsers(AdminUserListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    u.c_userid AS UserId,
                    u.c_name AS FullName,
                    u.c_mobile AS Phone,
                    u.c_email AS Email,
                    u.c_isemailverified AS IsEmailVerified,
                    u.c_isphoneverified AS IsPhoneVerified,
                    COALESCE(u.c_isactive, 1) AS IsActive,
                    COALESCE(u.c_isblocked, 0) AS IsBlocked,
                    COALESCE(u.c_is_deleted, 0) AS IsDeleted,
                    c.c_cityname AS CityName,
                    s.c_statename AS StateName,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    COALESCE(SUM(o.c_total_amount), 0) AS TotalSpent,
                    COUNT(DISTINCT r.c_reviewid) AS TotalReviews,
                    u.c_createddate AS CreatedDate,
                    u.c_last_login AS LastLogin
                FROM {Table.SysUser} u
                LEFT JOIN {Table.City} c ON u.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON u.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                LEFT JOIN {Table.SysCateringReview} r ON u.c_userid = r.c_userid
                WHERE 1=1");

            var parameters = new List<NpgsqlParameter>();

            AppendFilters(queryBuilder, parameters, request);

            queryBuilder.Append(@"
                GROUP BY u.c_userid, u.c_name, u.c_mobile, u.c_email,
                         u.c_isemailverified, u.c_isphoneverified, u.c_isactive,
                         u.c_isblocked, u.c_is_deleted,
                         c.c_cityname, s.c_statename,
                         u.c_createddate, u.c_last_login");

            string sortColumn = request.SortBy switch
            {
                "FullName" => "u.c_name",
                "TotalOrders" => "TotalOrders",
                "TotalSpent" => "TotalSpent",
                "Phone" => "u.c_mobile",
                "Email" => "u.c_email",
                "CityName" => "c.c_cityname",
                "StateName" => "s.c_statename",
                _ => "u.c_createddate"
            };

            queryBuilder.Append($" ORDER BY {sortColumn} {(request.SortOrder == "ASC" ? "ASC" : "DESC")}");

            // Count query
            int totalRecords = GetTotalCount(request);

            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" LIMIT {request.PageSize} OFFSET {offset}");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var users = new List<AdminUserListItem>();
            foreach (DataRow row in dt.Rows)
            {
                users.Add(MapToListItem(row));
            }

            return new AdminUserListResponse
            {
                Users = users,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public AdminUserDetail? GetUserById(long userId)
        {
            string query = $@"
                SELECT
                    u.c_userid, u.c_name, u.c_mobile, u.c_email, u.c_picture,
                    u.c_isemailverified, u.c_isphoneverified,
                    COALESCE(u.c_isactive, 1) AS IsActive,
                    COALESCE(u.c_isblocked, 0) AS IsBlocked,
                    COALESCE(u.c_is_deleted, 0) AS IsDeleted,
                    u.c_block_reason, u.c_description,
                    c.c_cityname AS CityName,
                    s.c_statename AS StateName,
                    u.c_createddate, u.c_last_login
                FROM {Table.SysUser} u
                LEFT JOIN {Table.City} c ON u.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON u.c_stateid = s.c_stateid
                WHERE u.c_userid = @UserId";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@UserId", userId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];

            var (totalOrders, totalSpent, totalReviews, avgRating) = GetUserStats(userId);
            var recentOrders = GetUserRecentOrders(userId);
            var recentReviews = GetUserRecentReviews(userId);

            return new AdminUserDetail
            {
                UserId = Convert.ToInt64(row["c_userid"]),
                FullName = row["c_name"]?.ToString() ?? string.Empty,
                Phone = row["c_mobile"]?.ToString() ?? string.Empty,
                Email = row["c_email"]?.ToString(),
                ProfilePhoto = row["c_picture"]?.ToString(),
                Description = row["c_description"]?.ToString(),
                IsEmailVerified = row["c_isemailverified"] != DBNull.Value && Convert.ToBoolean(row["c_isemailverified"]),
                IsPhoneVerified = row["c_isphoneverified"] != DBNull.Value && Convert.ToBoolean(row["c_isphoneverified"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                BlockReason = row["c_block_reason"]?.ToString(),
                CityName = row["CityName"]?.ToString(),
                StateName = row["StateName"]?.ToString(),
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                TotalReviews = totalReviews,
                AverageRating = avgRating,
                RecentOrders = recentOrders,
                RecentReviews = recentReviews,
                CreatedDate = row["c_createddate"] != DBNull.Value ? Convert.ToDateTime(row["c_createddate"]) : DateTime.MinValue,
                LastLogin = row["c_last_login"] != DBNull.Value ? Convert.ToDateTime(row["c_last_login"]) : null
            };
        }

        public bool UpdateUserStatus(AdminUserStatusUpdate request)
        {
            string query = $@"
                UPDATE {Table.SysUser}
                SET c_isblocked = @IsBlocked,
                    c_block_reason = @Reason,
                    c_modifieddate = NOW()
                WHERE c_userid = @UserId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@IsBlocked", request.IsBlocked),
                new NpgsqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool SoftDeleteUser(long userId, long adminId)
        {
            string query = $@"
                UPDATE {Table.SysUser}
                SET c_is_deleted = TRUE,
                    c_isactive = FALSE,
                    c_deleted_by = @AdminId,
                    c_deleted_date = NOW(),
                    c_modifieddate = NOW()
                WHERE c_userid = @UserId AND COALESCE(c_is_deleted, FALSE) = 0";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@UserId", userId),
                new NpgsqlParameter("@AdminId", adminId)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool RestoreUser(long userId, long adminId)
        {
            string query = $@"
                UPDATE {Table.SysUser}
                SET c_is_deleted = FALSE,
                    c_isactive = TRUE,
                    c_deleted_by = NULL,
                    c_deleted_date = NULL,
                    c_modifieddate = NOW()
                WHERE c_userid = @UserId AND c_is_deleted = TRUE";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@UserId", userId),
                new NpgsqlParameter("@AdminId", adminId)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public List<AdminUserExportItem> GetUsersForExport(AdminUserListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    u.c_userid AS UserId,
                    u.c_name AS FullName,
                    u.c_mobile AS Phone,
                    u.c_email AS Email,
                    c.c_cityname AS CityName,
                    s.c_statename AS StateName,
                    COALESCE(u.c_isactive, 1) AS IsActive,
                    COALESCE(u.c_isblocked, 0) AS IsBlocked,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    COALESCE(SUM(o.c_total_amount), 0) AS TotalSpent,
                    u.c_createddate AS CreatedDate,
                    u.c_last_login AS LastLogin
                FROM {Table.SysUser} u
                LEFT JOIN {Table.City} c ON u.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON u.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                WHERE COALESCE(u.c_is_deleted, 0) = 0");

            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (u.c_name LIKE @SearchTerm OR u.c_mobile LIKE @SearchTerm OR u.c_email LIKE @SearchTerm)");
                parameters.Add(new NpgsqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.IsActive.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(u.c_isactive, 1) = @IsActive");
                parameters.Add(new NpgsqlParameter("@IsActive", request.IsActive.Value));
            }

            if (request.IsBlocked.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(u.c_isblocked, 0) = @IsBlocked");
                parameters.Add(new NpgsqlParameter("@IsBlocked", request.IsBlocked.Value));
            }

            if (request.StateId.HasValue)
            {
                queryBuilder.Append(" AND u.c_stateid = @StateId");
                parameters.Add(new NpgsqlParameter("@StateId", request.StateId.Value));
            }

            if (request.CityId.HasValue)
            {
                queryBuilder.Append(" AND u.c_cityid = @CityId");
                parameters.Add(new NpgsqlParameter("@CityId", request.CityId.Value));
            }

            queryBuilder.Append(@"
                GROUP BY u.c_userid, u.c_name, u.c_mobile, u.c_email,
                         c.c_cityname, s.c_statename, u.c_isactive, u.c_isblocked,
                         u.c_createddate, u.c_last_login
                ORDER BY u.c_createddate DESC");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var items = new List<AdminUserExportItem>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new AdminUserExportItem
                {
                    UserId = Convert.ToInt64(row["UserId"]),
                    FullName = row["FullName"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString(),
                    CityName = row["CityName"]?.ToString(),
                    StateName = row["StateName"]?.ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalSpent = Convert.ToDecimal(row["TotalSpent"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.MinValue,
                    LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
                });
            }

            return items;
        }

        #region Private Helpers

        private void AppendFilters(StringBuilder queryBuilder, List<NpgsqlParameter> parameters, AdminUserListRequest request)
        {
            // By default, hide deleted users unless explicitly requested
            if (request.IsDeleted.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(u.c_is_deleted, 0) = @IsDeleted");
                parameters.Add(new NpgsqlParameter("@IsDeleted", request.IsDeleted.Value));
            }
            else
            {
                queryBuilder.Append(" AND COALESCE(u.c_is_deleted, 0) = 0");
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (u.c_name LIKE @SearchTerm OR u.c_mobile LIKE @SearchTerm OR u.c_email LIKE @SearchTerm)");
                parameters.Add(new NpgsqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.IsBlocked.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(u.c_isblocked, 0) = @IsBlocked");
                parameters.Add(new NpgsqlParameter("@IsBlocked", request.IsBlocked.Value));
            }

            if (request.IsActive.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(u.c_isactive, 1) = @IsActive");
                parameters.Add(new NpgsqlParameter("@IsActive", request.IsActive.Value));
            }

            if (request.StateId.HasValue)
            {
                queryBuilder.Append(" AND u.c_stateid = @StateId");
                parameters.Add(new NpgsqlParameter("@StateId", request.StateId.Value));
            }

            if (request.CityId.HasValue)
            {
                queryBuilder.Append(" AND u.c_cityid = @CityId");
                parameters.Add(new NpgsqlParameter("@CityId", request.CityId.Value));
            }
        }

        private int GetTotalCount(AdminUserListRequest request)
        {
            var countBuilder = new StringBuilder($@"
                SELECT COUNT(DISTINCT u.c_userid)
                FROM {Table.SysUser} u
                WHERE 1=1");

            var countParams = new List<NpgsqlParameter>();
            AppendFilters(countBuilder, countParams, request);

            return Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));
        }

        private AdminUserListItem MapToListItem(DataRow row)
        {
            return new AdminUserListItem
            {
                UserId = Convert.ToInt64(row["UserId"]),
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString(),
                IsEmailVerified = row["IsEmailVerified"] != DBNull.Value && Convert.ToBoolean(row["IsEmailVerified"]),
                IsPhoneVerified = row["IsPhoneVerified"] != DBNull.Value && Convert.ToBoolean(row["IsPhoneVerified"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                CityName = row["CityName"]?.ToString(),
                StateName = row["StateName"]?.ToString(),
                TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                TotalSpent = Convert.ToDecimal(row["TotalSpent"]),
                TotalReviews = Convert.ToInt32(row["TotalReviews"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.MinValue,
                LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
            };
        }

        private (int TotalOrders, decimal TotalSpent, int TotalReviews, decimal AvgRating) GetUserStats(long userId)
        {
            string query = $@"
                SELECT
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    COALESCE(SUM(o.c_total_amount), 0) AS TotalSpent,
                    COUNT(DISTINCT r.c_reviewid) AS TotalReviews,
                    COALESCE(AVG(CAST(r.c_overall_rating AS DECIMAL(3,2))), 0) AS AvgRating
                FROM {Table.SysUser} u
                LEFT JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                LEFT JOIN {Table.SysCateringReview} r ON u.c_userid = r.c_userid
                WHERE u.c_userid = @UserId";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@UserId", userId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToInt32(row["TotalOrders"]),
                    Convert.ToDecimal(row["TotalSpent"]),
                    Convert.ToInt32(row["TotalReviews"]),
                    Convert.ToDecimal(row["AvgRating"])
                );
            }

            return (0, 0, 0, 0);
        }

        private List<AdminUserOrderSummary> GetUserRecentOrders(long userId)
        {
            string query = $@"
                SELECT
                    o.c_orderid, co.c_catering_name, o.c_total_amount,
                    o.c_order_status, o.c_createddate, o.c_event_date
                FROM {Table.SysOrders} o
                JOIN {Table.SysCateringOwner} co ON o.c_ownerid = co.c_ownerid
                WHERE o.c_userid = @UserId
                ORDER BY o.c_createddate DESC
                LIMIT 5";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@UserId", userId) };
            var dt = _dbHelper.Execute(query, parameters);

            var orders = new List<AdminUserOrderSummary>();
            foreach (DataRow row in dt.Rows)
            {
                orders.Add(new AdminUserOrderSummary
                {
                    OrderId = Convert.ToInt64(row["c_orderid"]),
                    CateringName = row["c_business_name"]?.ToString() ?? string.Empty,
                    TotalAmount = Convert.ToDecimal(row["c_total_amount"]),
                    Status = row["c_status"]?.ToString() ?? string.Empty,
                    OrderDate = Convert.ToDateTime(row["c_order_date"]),
                    EventDate = Convert.ToDateTime(row["c_event_date"])
                });
            }

            return orders;
        }

        private List<AdminUserReviewSummary> GetUserRecentReviews(long userId)
        {
            string query = $@"
                SELECT
                    r.c_reviewid, co.c_catering_name, r.c_overall_rating,
                    r.c_review_comment, r.c_createddate
                FROM {Table.SysCateringReview} r
                JOIN {Table.SysCateringOwner} co ON r.c_ownerid = co.c_ownerid
                WHERE r.c_userid = @UserId
                ORDER BY r.c_createddate DESC
                LIMIT 5";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@UserId", userId) };
            var dt = _dbHelper.Execute(query, parameters);

            var reviews = new List<AdminUserReviewSummary>();
            foreach (DataRow row in dt.Rows)
            {
                reviews.Add(new AdminUserReviewSummary
                {
                    ReviewId = Convert.ToInt64(row["c_reviewid"]),
                    CateringName = row["c_business_name"]?.ToString() ?? string.Empty,
                    Rating = Convert.ToInt32(row["c_rating"]),
                    Comment = row["c_comment"]?.ToString(),
                    ReviewDate = Convert.ToDateTime(row["c_createddate"])
                });
            }

            return reviews;
        }

        #endregion
    }
}

