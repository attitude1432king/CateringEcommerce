using CateringEcommerce.BAL.Configuration;
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
        private readonly string _connStr;

        public OwnerReviewRepository(string connStr)
        {
            _connStr = connStr ?? throw new ArgumentNullException(nameof(connStr));
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

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // Count query
            var countSql = $@"
                SELECT COUNT(*)
                FROM {Table.SysCateringReview} r
                WHERE r.c_ownerid = @OwnerId
                  AND r.c_is_visible = 1";

            if (filter.Rating.HasValue)
                countSql += " AND FLOOR(r.c_overall_rating) = @Rating";

            if (filter.HasReply.HasValue)
            {
                countSql += filter.HasReply.Value
                    ? " AND r.c_owner_reply IS NOT NULL"
                    : " AND r.c_owner_reply IS NULL";
            }

            using (var countCmd = new SqlCommand(countSql, conn))
            {
                countCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                if (filter.Rating.HasValue)
                    countCmd.Parameters.AddWithValue("@Rating", filter.Rating.Value);

                result.TotalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            }

            result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / filter.PageSize);

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
                    ISNULL(u.c_fullname, 'Customer') AS CustomerName,
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

            using (var dataCmd = new SqlCommand(dataSql, conn))
            {
                dataCmd.Parameters.AddWithValue("@OwnerId", ownerId);
                dataCmd.Parameters.AddWithValue("@Offset", (filter.Page - 1) * filter.PageSize);
                dataCmd.Parameters.AddWithValue("@PageSize", filter.PageSize);

                if (filter.Rating.HasValue)
                    dataCmd.Parameters.AddWithValue("@Rating", filter.Rating.Value);

                using var reader = await dataCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Reviews.Add(new OwnerReviewItemDto
                    {
                        ReviewId = reader.GetInt64(reader.GetOrdinal("ReviewId")),
                        OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
                        OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                        UserId = reader.GetInt64(reader.GetOrdinal("UserId")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        OverallRating = reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                        FoodQualityRating = reader.IsDBNull(reader.GetOrdinal("FoodQualityRating")) ? null : reader.GetDecimal(reader.GetOrdinal("FoodQualityRating")),
                        HygieneRating = reader.IsDBNull(reader.GetOrdinal("HygieneRating")) ? null : reader.GetDecimal(reader.GetOrdinal("HygieneRating")),
                        StaffBehaviorRating = reader.IsDBNull(reader.GetOrdinal("StaffBehaviorRating")) ? null : reader.GetDecimal(reader.GetOrdinal("StaffBehaviorRating")),
                        DecorationRating = reader.IsDBNull(reader.GetOrdinal("DecorationRating")) ? null : reader.GetDecimal(reader.GetOrdinal("DecorationRating")),
                        PunctualityRating = reader.IsDBNull(reader.GetOrdinal("PunctualityRating")) ? null : reader.GetDecimal(reader.GetOrdinal("PunctualityRating")),
                        ReviewTitle = reader.IsDBNull(reader.GetOrdinal("ReviewTitle")) ? null : reader.GetString(reader.GetOrdinal("ReviewTitle")),
                        ReviewComment = reader.IsDBNull(reader.GetOrdinal("ReviewComment")) ? null : reader.GetString(reader.GetOrdinal("ReviewComment")),
                        EventType = reader.GetString(reader.GetOrdinal("EventType")),
                        OwnerReply = reader.IsDBNull(reader.GetOrdinal("OwnerReply")) ? null : reader.GetString(reader.GetOrdinal("OwnerReply")),
                        OwnerReplyDate = reader.IsDBNull(reader.GetOrdinal("OwnerReplyDate")) ? null : reader.GetDateTime(reader.GetOrdinal("OwnerReplyDate")),
                        ReviewDate = reader.GetDateTime(reader.GetOrdinal("ReviewDate")),
                        IsVerified = reader.GetBoolean(reader.GetOrdinal("IsVerified")),
                        IsVisible = reader.GetBoolean(reader.GetOrdinal("IsVisible"))
                    });
                }
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

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                stats.AverageRating = reader.IsDBNull(reader.GetOrdinal("AverageRating")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AverageRating"));
                stats.TotalReviews = reader.GetInt32(reader.GetOrdinal("TotalReviews"));
                stats.FiveStarCount = reader.GetInt32(reader.GetOrdinal("FiveStarCount"));
                stats.FourStarCount = reader.GetInt32(reader.GetOrdinal("FourStarCount"));
                stats.ThreeStarCount = reader.GetInt32(reader.GetOrdinal("ThreeStarCount"));
                stats.TwoStarCount = reader.GetInt32(reader.GetOrdinal("TwoStarCount"));
                stats.OneStarCount = reader.GetInt32(reader.GetOrdinal("OneStarCount"));
                stats.UnrepliedCount = reader.GetInt32(reader.GetOrdinal("UnrepliedCount"));
                stats.AvgFoodQuality = reader.IsDBNull(reader.GetOrdinal("AvgFoodQuality")) ? null : reader.GetDecimal(reader.GetOrdinal("AvgFoodQuality"));
                stats.AvgHygiene = reader.IsDBNull(reader.GetOrdinal("AvgHygiene")) ? null : reader.GetDecimal(reader.GetOrdinal("AvgHygiene"));
                stats.AvgStaffBehavior = reader.IsDBNull(reader.GetOrdinal("AvgStaffBehavior")) ? null : reader.GetDecimal(reader.GetOrdinal("AvgStaffBehavior"));
                stats.AvgPunctuality = reader.IsDBNull(reader.GetOrdinal("AvgPunctuality")) ? null : reader.GetDecimal(reader.GetOrdinal("AvgPunctuality"));
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

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReplyText", replyText);
            cmd.Parameters.AddWithValue("@ReviewId", reviewId);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
