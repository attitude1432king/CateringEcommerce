using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/reviews")]
    [ApiController]
    [AdminAuthorize]
    public class AdminReviewsController : ControllerBase
    {
        private readonly IAdminReviewRepository _reviewRepository;
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly ILogger<AdminReviewsController> _logger;

        public AdminReviewsController(
            IAdminReviewRepository reviewRepository,
            IAdminAuthRepository adminAuthRepository,
            ILogger<AdminReviewsController> logger)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _adminAuthRepository = adminAuthRepository ?? throw new ArgumentNullException(nameof(adminAuthRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all reviews with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllReviews([FromQuery] AdminReviewListRequest request)
        {
            try
            {
                var result = _reviewRepository.GetAllReviews(request);
                return ApiResponseHelper.Success(result, "Reviews retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process review request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get review details by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetReviewById(long id)
        {
            try
            {
                var review = _reviewRepository.GetReviewById(id);

                if (review == null)
                    return ApiResponseHelper.Failure("Review not found.");

                return ApiResponseHelper.Success(review, "Review details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process review request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Hide or unhide a review
        /// </summary>
        [HttpPut("{id}/hide")]
        public IActionResult UpdateReviewVisibility(long id, [FromBody] AdminReviewHideRequest request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                request.ReviewId = id;
                request.UpdatedBy = adminId;

                bool success = _reviewRepository.UpdateReviewVisibility(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update review visibility.");

                // Log activity
                _adminAuthRepository.LogAdminActivity(adminId, "UPDATE_REVIEW_VISIBILITY", $"Updated review {id} visibility - Hidden: {request.IsHidden}");

                return ApiResponseHelper.Success(null, "Review visibility updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process review request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a review permanently
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteReview(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _reviewRepository.DeleteReview(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to delete review.");

                // Log activity
                _adminAuthRepository.LogAdminActivity(adminId, "DELETE_REVIEW", $"Deleted review {id}");

                return ApiResponseHelper.Success(null, "Review deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process review request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
