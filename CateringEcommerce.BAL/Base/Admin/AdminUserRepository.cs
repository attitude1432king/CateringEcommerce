using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
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
                    ISNULL(u.c_isblocked, 0) AS IsBlocked,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalSpent,
                    COUNT(DISTINCT r.c_reviewid) AS TotalReviews,
                    u.c_created_date AS CreatedDate,
                    u.c_last_login AS LastLogin
                FROM {Table.SysUser} u
                LEFT JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                LEFT JOIN {Table.SysCateringReview} r ON u.c_userid = r.c_userid
                WHERE 1=1");

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (u.c_name LIKE @SearchTerm OR u.c_mobile LIKE @SearchTerm OR u.c_email LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.IsBlocked.HasValue)
            {
                queryBuilder.Append(" AND ISNULL(u.c_isblocked, 0) = @IsBlocked");
                parameters.Add(new SqlParameter("@IsBlocked", request.IsBlocked.Value));
            }

            queryBuilder.Append(@"
                GROUP BY u.c_userid, u.c_name, u.c_mobile, u.c_email,
                         u.c_isemailverified, u.c_isphoneverified, u.c_isblocked,
                         u.c_created_date, u.c_last_login");

            string sortColumn = request.SortBy switch
            {
                "FullName" => "u.c_name",
                "TotalOrders" => "TotalOrders",
                "TotalSpent" => "TotalSpent",
                _ => "u.c_created_date"
            };

            queryBuilder.Append($" ORDER BY {sortColumn} {request.SortOrder}");

            string countQuery = $@"
                SELECT COUNT(DISTINCT u.c_userid)
                FROM {Table.SysUser} u
                WHERE 1=1" + GetWhereClauseForCount(request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countQuery, parameters.ToArray()));

            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var users = new List<AdminUserListItem>();
            foreach (DataRow row in dt.Rows)
            {
                users.Add(new AdminUserListItem
                {
                    UserId = Convert.ToInt64(row["UserId"]),
                    FullName = row["FullName"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString(),
                    IsEmailVerified = row["IsEmailVerified"] != DBNull.Value && Convert.ToBoolean(row["IsEmailVerified"]),
                    IsPhoneVerified = row["IsPhoneVerified"] != DBNull.Value && Convert.ToBoolean(row["IsPhoneVerified"]),
                    IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalSpent = Convert.ToDecimal(row["TotalSpent"]),
                    TotalReviews = Convert.ToInt32(row["TotalReviews"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
                });
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
                    ISNULL(u.c_isblocked, 0) AS IsBlocked,
                    u.c_block_reason,
                    u.c_created_date, u.c_last_login
                FROM {Table.SysUser} u
                WHERE u.c_userid = @UserId";

            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
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
                IsEmailVerified = row["c_isemailverified"] != DBNull.Value && Convert.ToBoolean(row["c_isemailverified"]),
                IsPhoneVerified = row["c_isphoneverified"] != DBNull.Value && Convert.ToBoolean(row["c_isphoneverified"]),
                IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                BlockReason = row["c_block_reason"]?.ToString(),
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                TotalReviews = totalReviews,
                AverageRating = avgRating,
                RecentOrders = recentOrders,
                RecentReviews = recentReviews,
                CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                LastLogin = row["c_last_login"] != DBNull.Value ? Convert.ToDateTime(row["c_last_login"]) : null
            };
        }

        public bool UpdateUserStatus(AdminUserStatusUpdate request)
        {
            string query = $@"
                UPDATE {Table.SysUser}
                SET c_isblocked = @IsBlocked,
                    c_block_reason = @Reason,
                    c_last_modified = GETDATE()
                WHERE c_userid = @UserId";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", request.UserId),
                new SqlParameter("@IsBlocked", request.IsBlocked),
                new SqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        private (int TotalOrders, decimal TotalSpent, int TotalReviews, decimal AvgRating) GetUserStats(long userId)
        {
            string query = $@"
                SELECT
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalSpent,
                    COUNT(DISTINCT r.c_reviewid) AS TotalReviews,
                    ISNULL(AVG(CAST(r.c_rating AS DECIMAL(3,2))), 0) AS AvgRating
                FROM {Table.SysUser} u
                LEFT JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                LEFT JOIN {Table.SysCateringReview} r ON u.c_userid = r.c_userid
                WHERE u.c_userid = @UserId";

            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
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
                SELECT TOP 5
                    o.c_orderid, co.c_business_name, o.c_total_amount,
                    o.c_status, o.c_order_date, o.c_event_date
                FROM {Table.SysOrders} o
                JOIN {Table.SysCateringOwner} co ON o.c_catering_ownerid = co.c_catering_ownerid
                WHERE o.c_userid = @UserId
                ORDER BY o.c_order_date DESC";

            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
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
                SELECT TOP 5
                    r.c_reviewid, co.c_business_name, r.c_rating,
                    r.c_comment, r.c_created_date
                FROM {Table.SysCateringReview} r
                JOIN {Table.SysCateringOwner} co ON r.c_catering_ownerid = co.c_catering_ownerid
                WHERE r.c_userid = @UserId
                ORDER BY r.c_created_date DESC";

            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
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
                    ReviewDate = Convert.ToDateTime(row["c_created_date"])
                });
            }

            return reviews;
        }

        private string GetWhereClauseForCount(AdminUserListRequest request)
        {
            var whereBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                whereBuilder.Append(" AND (u.c_name LIKE @SearchTerm OR u.c_mobile LIKE @SearchTerm OR u.c_email LIKE @SearchTerm)");

            if (request.IsBlocked.HasValue)
                whereBuilder.Append($" AND ISNULL(u.c_isblocked, 0) = {(request.IsBlocked.Value ? "1" : "0")}");

            return whereBuilder.ToString();
        }
    }
}
