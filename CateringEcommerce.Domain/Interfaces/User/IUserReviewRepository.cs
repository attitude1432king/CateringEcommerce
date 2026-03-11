using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    /// <summary>
    /// User Review Repository Interface
    /// Handles review submission, editing, and viewing
    /// </summary>
    public interface IUserReviewRepository
    {
        /// <summary>
        /// Submit a new review for a completed order
        /// </summary>
        Task<SubmitReviewResponse> SubmitReviewAsync(long userId, SubmitReviewRequest request);

        /// <summary>
        /// Check if user can review this order
        /// </summary>
        Task<CanReviewResponse> CanReviewOrderAsync(long userId, long orderId);

        /// <summary>
        /// Get user's review for a specific order
        /// </summary>
        Task<UserReviewDetail> GetUserReviewByOrderAsync(long userId, long orderId);

        /// <summary>
        /// Get all reviews submitted by user
        /// </summary>
        Task<List<UserReviewListItem>> GetUserReviewsAsync(long userId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Get specific review details
        /// </summary>
        Task<UserReviewDetail> GetReviewDetailAsync(long reviewId, long userId);

        /// <summary>
        /// Update an existing review
        /// </summary>
        Task<bool> UpdateReviewAsync(long userId, UpdateReviewRequest request);

        /// <summary>
        /// Delete user's review
        /// </summary>
        Task<bool> DeleteReviewAsync(long userId, long reviewId);

        /// <summary>
        /// Get reviews for a specific catering (public display)
        /// </summary>
        Task<CateringReviewsResponse> GetCateringReviewsAsync(long cateringId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Get review statistics for a catering
        /// </summary>
        Task<ReviewStatsDto> GetCateringReviewStatsAsync(long cateringId);
    }
}
