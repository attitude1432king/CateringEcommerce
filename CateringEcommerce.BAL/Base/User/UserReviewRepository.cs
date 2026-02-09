using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// User Review Repository Implementation
    /// </summary>
    public class UserReviewRepository : IUserReviewRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public UserReviewRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<SubmitReviewResponse> SubmitReviewAsync(long userId, SubmitReviewRequest request)
        {
            try
            {
                // Validate: Check if order belongs to user and is completed
                var orderCheckSql = @"
                    SELECT o.c_orderid, o.c_order_status, o.c_ownerid, co.c_catering_name
                    FROM t_sys_orders o
                    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
                    WHERE o.c_orderid = @OrderId AND o.c_userid = @UserId";

                var orderCheckParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", request.OrderId),
                    new SqlParameter("@UserId", userId)
                };

                var orderCheckResults = await _dbHelper.ExecuteQueryAsync<dynamic>(orderCheckSql, orderCheckParams);
                var orderCheck = orderCheckResults?.FirstOrDefault();

                if (orderCheck == null)
                    return new SubmitReviewResponse { Success = false, Message = "Order not found" };

                if (orderCheck.c_order_status != "Completed")
                    return new SubmitReviewResponse { Success = false, Message = "Can only review completed orders" };

                // Check if already reviewed
                var existingSql = "SELECT c_reviewid FROM t_sys_catering_review WHERE c_orderid = @OrderId AND c_userid = @UserId";
                var existingParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", request.OrderId),
                    new SqlParameter("@UserId", userId)
                };

                var existingResults = await _dbHelper.ExecuteQueryAsync<long?>(existingSql, existingParams);
                var existingReview = existingResults?.FirstOrDefault();

                if (existingReview.HasValue)
                    return new SubmitReviewResponse { Success = false, Message = "You have already reviewed this order" };

                // Insert review
                var insertSql = @"
                    INSERT INTO t_sys_catering_review (
                        c_ownerid, c_userid, c_orderid,
                        c_overall_rating, c_food_quality_rating, c_hygiene_rating,
                        c_staff_behavior_rating, c_decoration_rating, c_punctuality_rating,
                        c_review_title, c_review_comment,
                        c_is_verified, c_is_visible, c_admin_status, c_ishidden,
                        c_createddate
                    )
                    VALUES (
                        @OwnerId, @UserId, @OrderId,
                        @OverallRating, @FoodQualityRating, @HygieneRating,
                        @StaffBehaviorRating, @DecorationRating, @PunctualityRating,
                        @ReviewTitle, @ReviewComment,
                        1, 1, 'Approved', 0,
                        GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var insertParams = new SqlParameter[]
                {
                    new SqlParameter("@OwnerId", orderCheck.c_ownerid),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@OrderId", request.OrderId),
                    new SqlParameter("@OverallRating", request.OverallRating),
                    new SqlParameter("@FoodQualityRating", (object)request.FoodQualityRating ?? DBNull.Value),
                    new SqlParameter("@HygieneRating", (object)request.HygieneRating ?? DBNull.Value),
                    new SqlParameter("@StaffBehaviorRating", (object)request.StaffBehaviorRating ?? DBNull.Value),
                    new SqlParameter("@DecorationRating", (object)request.DecorationRating ?? DBNull.Value),
                    new SqlParameter("@PunctualityRating", (object)request.PunctualityRating ?? DBNull.Value),
                    new SqlParameter("@ReviewTitle", (object)request.ReviewTitle ?? DBNull.Value),
                    new SqlParameter("@ReviewComment", (object)request.ReviewComment ?? DBNull.Value)
                };

                var reviewIdResult = await _dbHelper.ExecuteScalarAsync(insertSql, insertParams);
                var reviewId = Convert.ToInt64(reviewIdResult);

                return new SubmitReviewResponse
                {
                    Success = true,
                    ReviewId = reviewId,
                    Message = $"Thank you for reviewing {orderCheck.c_catering_name}!"
                };
            }
            catch (Exception ex)
            {
                return new SubmitReviewResponse
                {
                    Success = false,
                    Message = $"Error submitting review: {ex.Message}"
                };
            }
        }

        public async Task<CanReviewResponse> CanReviewOrderAsync(long userId, long orderId)
        {
            try
            {
                // Check order status
                var orderSql = "SELECT c_orderid, c_order_status FROM t_sys_orders WHERE c_orderid = @OrderId AND c_userid = @UserId";
                var orderParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@UserId", userId)
                };

                var orderResults = await _dbHelper.ExecuteQueryAsync<dynamic>(orderSql, orderParams);
                var order = orderResults?.FirstOrDefault();

                if (order == null)
                    return new CanReviewResponse { CanReview = false, Message = "Order not found" };

                if (order.c_order_status != "Completed")
                    return new CanReviewResponse { CanReview = false, Message = "Order must be completed to leave a review" };

                // Check if already reviewed
                var reviewSql = "SELECT c_reviewid FROM t_sys_catering_review WHERE c_orderid = @OrderId AND c_userid = @UserId";
                var reviewParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@UserId", userId)
                };

                var reviewResults = await _dbHelper.ExecuteQueryAsync<long?>(reviewSql, reviewParams);
                var existingReview = reviewResults?.FirstOrDefault();

                if (existingReview.HasValue)
                {
                    return new CanReviewResponse
                    {
                        CanReview = false,
                        Message = "You have already reviewed this order",
                        AlreadyReviewed = true,
                        ExistingReviewId = existingReview.Value
                    };
                }

                return new CanReviewResponse
                {
                    CanReview = true,
                    Message = "You can review this order"
                };
            }
            catch (Exception ex)
            {
                return new CanReviewResponse
                {
                    CanReview = false,
                    Message = $"Error checking review eligibility: {ex.Message}"
                };
            }
        }

        public async Task<UserReviewDetail> GetUserReviewByOrderAsync(long userId, long orderId)
        {
            var sql = @"
                SELECT
                    r.c_reviewid AS ReviewId,
                    r.c_orderid AS OrderId,
                    o.c_order_number AS OrderNumber,
                    r.c_ownerid AS CateringId,
                    co.c_catering_name AS CateringName,
                    co.c_logo AS CateringLogo,
                    r.c_overall_rating AS OverallRating,
                    r.c_food_quality_rating AS FoodQualityRating,
                    r.c_hygiene_rating AS HygieneRating,
                    r.c_staff_behavior_rating AS StaffBehaviorRating,
                    r.c_decoration_rating AS DecorationRating,
                    r.c_punctuality_rating AS PunctualityRating,
                    r.c_review_title AS ReviewTitle,
                    r.c_review_comment AS ReviewComment,
                    orr.c_reply_text AS OwnerReply,
                    orr.c_reply_date AS OwnerReplyDate,
                    r.c_createddate AS ReviewDate,
                    r.c_is_verified AS IsVerified,
                    r.c_is_visible AS IsVisible,
                    o.c_event_type AS EventType
                FROM t_sys_catering_review r
                INNER JOIN t_sys_orders o ON r.c_orderid = o.c_orderid
                INNER JOIN t_sys_catering_owner co ON r.c_ownerid = co.c_ownerid
                LEFT JOIN t_sys_owner_review_reply orr ON r.c_reviewid = orr.c_reviewid
                WHERE r.c_orderid = @OrderId AND r.c_userid = @UserId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@UserId", userId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<UserReviewDetail>(sql, parameters);
            return results?.FirstOrDefault();
        }

        public async Task<List<UserReviewListItem>> GetUserReviewsAsync(long userId, int pageNumber = 1, int pageSize = 20)
        {
            var sql = @"
                SELECT
                    r.c_reviewid AS ReviewId,
                    r.c_orderid AS OrderId,
                    o.c_order_number AS OrderNumber,
                    r.c_ownerid AS CateringId,
                    co.c_catering_name AS CateringName,
                    co.c_logo AS CateringLogo,
                    r.c_overall_rating AS OverallRating,
                    r.c_review_title AS ReviewTitle,
                    r.c_review_comment AS ReviewComment,
                    r.c_createddate AS ReviewDate,
                    CASE WHEN orr.c_reply_text IS NOT NULL THEN 1 ELSE 0 END AS HasOwnerReply,
                    r.c_is_visible AS IsVisible
                FROM t_sys_catering_review r
                INNER JOIN t_sys_orders o ON r.c_orderid = o.c_orderid
                INNER JOIN t_sys_catering_owner co ON r.c_ownerid = co.c_ownerid
                LEFT JOIN t_sys_owner_review_reply orr ON r.c_reviewid = orr.c_reviewid
                WHERE r.c_userid = @UserId
                ORDER BY r.c_createddate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Offset", (pageNumber - 1) * pageSize),
                new SqlParameter("@PageSize", pageSize)
            };

            var result = await _dbHelper.ExecuteQueryAsync<UserReviewListItem>(sql, parameters);
            return result?.ToList() ?? new List<UserReviewListItem>();
        }

        public async Task<UserReviewDetail> GetReviewDetailAsync(long reviewId, long userId)
        {
            var sql = @"
                SELECT
                    r.c_reviewid AS ReviewId,
                    r.c_orderid AS OrderId,
                    o.c_order_number AS OrderNumber,
                    r.c_ownerid AS CateringId,
                    co.c_catering_name AS CateringName,
                    co.c_logo AS CateringLogo,
                    r.c_overall_rating AS OverallRating,
                    r.c_food_quality_rating AS FoodQualityRating,
                    r.c_hygiene_rating AS HygieneRating,
                    r.c_staff_behavior_rating AS StaffBehaviorRating,
                    r.c_decoration_rating AS DecorationRating,
                    r.c_punctuality_rating AS PunctualityRating,
                    r.c_review_title AS ReviewTitle,
                    r.c_review_comment AS ReviewComment,
                    orr.c_reply_text AS OwnerReply,
                    orr.c_reply_date AS OwnerReplyDate,
                    r.c_createddate AS ReviewDate,
                    r.c_is_verified AS IsVerified,
                    r.c_is_visible AS IsVisible,
                    o.c_event_type AS EventType
                FROM t_sys_catering_review r
                INNER JOIN t_sys_orders o ON r.c_orderid = o.c_orderid
                INNER JOIN t_sys_catering_owner co ON r.c_ownerid = co.c_ownerid
                LEFT JOIN t_sys_owner_review_reply orr ON r.c_reviewid = orr.c_reviewid
                WHERE r.c_reviewid = @ReviewId AND r.c_userid = @UserId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ReviewId", reviewId),
                new SqlParameter("@UserId", userId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<UserReviewDetail>(sql, parameters);
            return results?.FirstOrDefault();
        }

        public async Task<bool> UpdateReviewAsync(long userId, UpdateReviewRequest request)
        {
            try
            {
                // Verify ownership
                var checkSql = "SELECT COUNT(*) FROM t_sys_catering_review WHERE c_reviewid = @ReviewId AND c_userid = @UserId";
                var checkParams = new SqlParameter[]
                {
                    new SqlParameter("@ReviewId", request.ReviewId),
                    new SqlParameter("@UserId", userId)
                };

                var ownsReviewResult = await _dbHelper.ExecuteScalarAsync(checkSql, checkParams);
                var ownsReview = Convert.ToInt32(ownsReviewResult) > 0;

                if (!ownsReview)
                    return false;

                var updateSql = @"
                    UPDATE t_sys_catering_review
                    SET c_overall_rating = @OverallRating,
                        c_food_quality_rating = @FoodQualityRating,
                        c_hygiene_rating = @HygieneRating,
                        c_staff_behavior_rating = @StaffBehaviorRating,
                        c_decoration_rating = @DecorationRating,
                        c_punctuality_rating = @PunctualityRating,
                        c_review_title = @ReviewTitle,
                        c_review_comment = @ReviewComment,
                        c_modifieddate = GETDATE()
                    WHERE c_reviewid = @ReviewId";

                var updateParams = new SqlParameter[]
                {
                    new SqlParameter("@ReviewId", request.ReviewId),
                    new SqlParameter("@OverallRating", request.OverallRating),
                    new SqlParameter("@FoodQualityRating", (object)request.FoodQualityRating ?? DBNull.Value),
                    new SqlParameter("@HygieneRating", (object)request.HygieneRating ?? DBNull.Value),
                    new SqlParameter("@StaffBehaviorRating", (object)request.StaffBehaviorRating ?? DBNull.Value),
                    new SqlParameter("@DecorationRating", (object)request.DecorationRating ?? DBNull.Value),
                    new SqlParameter("@PunctualityRating", (object)request.PunctualityRating ?? DBNull.Value),
                    new SqlParameter("@ReviewTitle", (object)request.ReviewTitle ?? DBNull.Value),
                    new SqlParameter("@ReviewComment", (object)request.ReviewComment ?? DBNull.Value)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateSql, updateParams);
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(long userId, long reviewId)
        {
            try
            {
                var deleteSql = "DELETE FROM t_sys_catering_review WHERE c_reviewid = @ReviewId AND c_userid = @UserId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ReviewId", reviewId),
                    new SqlParameter("@UserId", userId)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(deleteSql, parameters);
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CateringReviewsResponse> GetCateringReviewsAsync(long cateringId, int pageNumber = 1, int pageSize = 10)
        {
            // Get reviews
            var reviewsSql = @"
                SELECT
                    r.c_reviewid AS ReviewId,
                    COALESCE(u.c_firstname + ' ' + LEFT(u.c_lastname, 1) + '.', 'Anonymous') AS UserName,
                    LEFT(u.c_firstname, 1) + LEFT(u.c_lastname, 1) AS UserInitials,
                    r.c_overall_rating AS OverallRating,
                    r.c_food_quality_rating AS FoodQualityRating,
                    r.c_hygiene_rating AS HygieneRating,
                    r.c_staff_behavior_rating AS StaffBehaviorRating,
                    r.c_decoration_rating AS DecorationRating,
                    r.c_punctuality_rating AS PunctualityRating,
                    r.c_review_title AS ReviewTitle,
                    r.c_review_comment AS ReviewComment,
                    orr.c_reply_text AS OwnerReply,
                    orr.c_reply_date AS OwnerReplyDate,
                    r.c_createddate AS ReviewDate,
                    r.c_is_verified AS IsVerified,
                    o.c_event_type AS EventType
                FROM t_sys_catering_review r
                INNER JOIN t_sys_user u ON r.c_userid = u.c_userid
                INNER JOIN t_sys_orders o ON r.c_orderid = o.c_orderid
                LEFT JOIN t_sys_owner_review_reply orr ON r.c_reviewid = orr.c_reviewid
                WHERE r.c_ownerid = @CateringId
                  AND r.c_is_visible = 1
                  AND r.c_ishidden = 0
                ORDER BY r.c_createddate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var reviewsParams = new SqlParameter[]
            {
                new SqlParameter("@CateringId", cateringId),
                new SqlParameter("@Offset", (pageNumber - 1) * pageSize),
                new SqlParameter("@PageSize", pageSize)
            };

            var reviews = await _dbHelper.ExecuteQueryAsync<CateringReviewDisplayDto>(reviewsSql, reviewsParams);

            // Get total count
            var countSql = "SELECT COUNT(*) FROM t_sys_catering_review WHERE c_ownerid = @CateringId AND c_is_visible = 1 AND c_ishidden = 0";
            var countParams = new SqlParameter[] { new SqlParameter("@CateringId", cateringId) };
            var totalCountResult = await _dbHelper.ExecuteScalarAsync(countSql, countParams);
            var totalCount = Convert.ToInt32(totalCountResult);

            // Get stats
            var stats = await GetCateringReviewStatsAsync(cateringId);

            return new CateringReviewsResponse
            {
                Reviews = reviews?.ToList() ?? new List<CateringReviewDisplayDto>(),
                Stats = stats,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ReviewStatsDto> GetCateringReviewStatsAsync(long cateringId)
        {
            var sql = @"
                SELECT
                    COALESCE(AVG(c_overall_rating), 0) AS AverageRating,
                    COUNT(*) AS TotalReviews,
                    SUM(CASE WHEN c_overall_rating >= 4.5 THEN 1 ELSE 0 END) AS FiveStarCount,
                    SUM(CASE WHEN c_overall_rating >= 3.5 AND c_overall_rating < 4.5 THEN 1 ELSE 0 END) AS FourStarCount,
                    SUM(CASE WHEN c_overall_rating >= 2.5 AND c_overall_rating < 3.5 THEN 1 ELSE 0 END) AS ThreeStarCount,
                    SUM(CASE WHEN c_overall_rating >= 1.5 AND c_overall_rating < 2.5 THEN 1 ELSE 0 END) AS TwoStarCount,
                    SUM(CASE WHEN c_overall_rating < 1.5 THEN 1 ELSE 0 END) AS OneStarCount,
                    AVG(c_food_quality_rating) AS AvgFoodQuality,
                    AVG(c_hygiene_rating) AS AvgHygiene,
                    AVG(c_staff_behavior_rating) AS AvgStaffBehavior,
                    AVG(c_decoration_rating) AS AvgDecoration,
                    AVG(c_punctuality_rating) AS AvgPunctuality
                FROM t_sys_catering_review
                WHERE c_ownerid = @CateringId AND c_is_visible = 1 AND c_ishidden = 0";

            var parameters = new SqlParameter[] { new SqlParameter("@CateringId", cateringId) };

            var statsResults = await _dbHelper.ExecuteQueryAsync<ReviewStatsDto>(sql, parameters);
            var stats = statsResults?.FirstOrDefault();
            return stats ?? new ReviewStatsDto();
        }
    }
}
