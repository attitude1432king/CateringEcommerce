using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerReviewRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public OwnerReviewRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        /// <summary>
        /// Get paginated and filtered reviews for an owner
        /// </summary>
        public async Task<PaginatedReviewsDto> GetReviews(long ownerId, OwnerReviewFilterDto filter)
        {
            var result = new PaginatedReviewsDto
            {
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            // Count query
            var countSql = $@"
                SELECT COUNT(*)
                FROM {Table.SysCateringReview} r
                WHERE r.c_ownerid = @OwnerId
                  AND r.c_is_visible = 1";

            var countParams = new List<SqlParameter>
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            if (filter.Rating.HasValue)
            {
                countSql += " AND FLOOR(r.c_overall_rating) = @Rating";
                countParams.Add(new SqlParameter("@Rating", filter.Rating.Value));
            }

            if (filter.HasReply.HasValue)
            {
                countSql += filter.HasReply.Value
                    ? " AND r.c_owner_reply IS NOT NULL"
                    : " AND r.c_owner_reply IS NULL";
            }

            var countResult = await _dbHelper.ExecuteScalarAsync(countSql, countParams.ToArray());
            result.TotalCount = countResult == null || countResult == DBNull.Value ? 0 : Convert.ToInt32(countResult);

            result.TotalPages = filter.PageSize > 0
                ? (int)Math.Ceiling((double)result.TotalCount / filter.PageSize)
                : 0;

            // Data query
            var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ReviewDate", "Rating" };
            var sortCol = allowedSorts.Contains(filter.SortBy ?? "") ? filter.SortBy : "ReviewDate";
            var sortDir = string.Equals(filter.SortOrder, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            var sortExpression = sortCol == "Rating" ? "r.c_overall_rating" : "r.c_createddate";

            var dataSql = $@"
                SELECT
                    r.c_reviewid AS ReviewId,
                    r.c_orderid AS OrderId,
                    ISNULL(o.c_order_number, '') AS OrderNumber,
                    r.c_userid AS UserId,
                    ISNULL(u.c_name, 'Customer') AS CustomerName,
                    r.c_overall_rating AS OverallRating,
                    r.c_food_quality_rating AS FoodQualityRating,
                    r.c_hygiene_rating AS HygieneRating,
                    r.c_staff_behavior_rating AS StaffBehaviorRating,
                    r.c_decoration_rating AS DecorationRating,
                    r.c_punctuality_rating AS PunctualityRating,
                    r.c_review_title AS ReviewTitle,
                    r.c_review_comment AS ReviewComment,
                    ISNULL(o.c_event_type, '') AS EventType,
                    r.c_owner_reply AS OwnerReply,
                    r.c_owner_reply_date AS OwnerReplyDate,
                    r.c_createddate AS ReviewDate,
                    r.c_is_verified AS IsVerified,
                    r.c_is_visible AS IsVisible
                FROM {Table.SysCateringReview} r
                LEFT JOIN {Table.SysOrders} o ON r.c_orderid = o.c_orderid
                LEFT JOIN {Table.SysUser} u ON r.c_userid = u.c_userid
                WHERE r.c_ownerid = @OwnerId
                  AND r.c_is_visible = 1";

            if (filter.Rating.HasValue)
                dataSql += " AND FLOOR(r.c_overall_rating) = @Rating";

            if (filter.HasReply.HasValue)
            {
                dataSql += filter.HasReply.Value
                    ? " AND r.c_owner_reply IS NOT NULL"
                    : " AND r.c_owner_reply IS NULL";
            }

            dataSql += $" ORDER BY {sortExpression} {sortDir}";
            dataSql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dataParams = new List<SqlParameter>
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@Offset", (filter.Page - 1) * filter.PageSize),
                new SqlParameter("@PageSize", filter.PageSize)
            };

            if (filter.Rating.HasValue)
            {
                dataParams.Add(new SqlParameter("@Rating", filter.Rating.Value));
            }

            var dataTable = await _dbHelper.ExecuteAsync(dataSql, dataParams.ToArray());
            foreach (DataRow row in dataTable.Rows)
            {
                result.Reviews.Add(new OwnerReviewItemDto
                {
                    ReviewId = row.GetValue<long>("ReviewId"),
                    OrderId = row.GetValue<long>("OrderId"),
                    OrderNumber = row.GetValue<string>("OrderNumber", string.Empty),
                    UserId = row.GetValue<long>("UserId"),
                    CustomerName = row.GetValue<string>("CustomerName", string.Empty),
                    OverallRating = row.GetValue<decimal>("OverallRating"),
                    FoodQualityRating = row.GetValue<decimal?>("FoodQualityRating"),
                    HygieneRating = row.GetValue<decimal?>("HygieneRating"),
                    StaffBehaviorRating = row.GetValue<decimal?>("StaffBehaviorRating"),
                    DecorationRating = row.GetValue<decimal?>("DecorationRating"),
                    PunctualityRating = row.GetValue<decimal?>("PunctualityRating"),
                    ReviewTitle = row.GetValue<string?>("ReviewTitle"),
                    ReviewComment = row.GetValue<string?>("ReviewComment"),
                    EventType = row.GetValue<string?>("EventType"),
                    OwnerReply = row.GetValue<string?>("OwnerReply"),
                    OwnerReplyDate = row.GetValue<DateTime?>("OwnerReplyDate"),
                    ReviewDate = row.GetValue<DateTime>("ReviewDate"),
                    IsVerified = row.GetValue<bool>("IsVerified"),
                    IsVisible = row.GetValue<bool>("IsVisible")
                });
            }

            return result;
        }

        /// <summary>
        /// Get review statistics for the owner dashboard
        /// </summary>
        public async Task<OwnerReviewStatsDto> GetReviewStats(long ownerId)
        {
            var stats = new OwnerReviewStatsDto();

            var sql = $@"
                SELECT
                    ISNULL(AVG(c_overall_rating), 0) AS AverageRating,
                    COUNT(*) AS TotalReviews,
                    SUM(CASE WHEN FLOOR(c_overall_rating) = 5 THEN 1 ELSE 0 END) AS FiveStarCount,
                    SUM(CASE WHEN FLOOR(c_overall_rating) = 4 THEN 1 ELSE 0 END) AS FourStarCount,
                    SUM(CASE WHEN FLOOR(c_overall_rating) = 3 THEN 1 ELSE 0 END) AS ThreeStarCount,
                    SUM(CASE WHEN FLOOR(c_overall_rating) = 2 THEN 1 ELSE 0 END) AS TwoStarCount,
                    SUM(CASE WHEN FLOOR(c_overall_rating) = 1 THEN 1 ELSE 0 END) AS OneStarCount,
                    SUM(CASE WHEN c_owner_reply IS NULL THEN 1 ELSE 0 END) AS UnrepliedCount,
                    AVG(c_food_quality_rating) AS AvgFoodQuality,
                    AVG(c_hygiene_rating) AS AvgHygiene,
                    AVG(c_staff_behavior_rating) AS AvgStaffBehavior,
                    AVG(c_punctuality_rating) AS AvgPunctuality
                FROM {Table.SysCateringReview}
                WHERE c_ownerid = @OwnerId
                  AND c_is_visible = 1";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = await _dbHelper.ExecuteAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                stats.AverageRating = row.GetValue<decimal>("AverageRating");
                stats.TotalReviews = row.GetValue<int>("TotalReviews");
                stats.FiveStarCount = row.GetValue<int>("FiveStarCount");
                stats.FourStarCount = row.GetValue<int>("FourStarCount");
                stats.ThreeStarCount = row.GetValue<int>("ThreeStarCount");
                stats.TwoStarCount = row.GetValue<int>("TwoStarCount");
                stats.OneStarCount = row.GetValue<int>("OneStarCount");
                stats.UnrepliedCount = row.GetValue<int>("UnrepliedCount");
                stats.AvgFoodQuality = row.GetValue<decimal?>("AvgFoodQuality");
                stats.AvgHygiene = row.GetValue<decimal?>("AvgHygiene");
                stats.AvgStaffBehavior = row.GetValue<decimal?>("AvgStaffBehavior");
                stats.AvgPunctuality = row.GetValue<decimal?>("AvgPunctuality");
            }

            return stats;
        }

        /// <summary>
        /// Submit owner reply to a review
        /// </summary>
        public async Task<bool> SubmitReply(long ownerId, long reviewId, string replyText)
        {
            var sql = $@"
                UPDATE {Table.SysCateringReview}
                SET c_owner_reply = @ReplyText,
                    c_owner_reply_date = GETDATE(),
                    c_modifieddate = GETDATE()
                WHERE c_reviewid = @ReviewId
                  AND c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new SqlParameter("@ReplyText", replyText),
                new SqlParameter("@ReviewId", reviewId),
                new SqlParameter("@OwnerId", ownerId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
    }
}
