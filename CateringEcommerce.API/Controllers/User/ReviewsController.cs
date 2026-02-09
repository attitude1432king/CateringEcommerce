using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/user/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IUserReviewRepository _reviewRepository;

        public ReviewsController(IUserReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// Submit a new review for a completed order
        /// </summary>
        [HttpPost("submit")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewRequest request)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                // Validate ratings
                if (request.OverallRating < 1 || request.OverallRating > 5)
                    return BadRequest(new { result = false, message = "Overall rating must be between 1 and 5" });

                var response = await _reviewRepository.SubmitReviewAsync(userId, request);

                if (response.Success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = response.Message,
                        data = new { reviewId = response.ReviewId }
                    });
                }

                return BadRequest(new { result = false, message = response.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while submitting the review", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if user can review an order
        /// </summary>
        [HttpGet("can-review/{orderId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CanReviewOrder(long orderId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                var response = await _reviewRepository.CanReviewOrderAsync(userId, orderId);

                return Ok(new
                {
                    result = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's review for a specific order
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetReviewByOrder(long orderId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                var review = await _reviewRepository.GetUserReviewByOrderAsync(userId, orderId);

                if (review == null)
                    return NotFound(new { result = false, message = "Review not found" });

                return Ok(new
                {
                    result = true,
                    data = review
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all reviews submitted by the user
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyReviews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                var reviews = await _reviewRepository.GetUserReviewsAsync(userId, pageNumber, pageSize);

                return Ok(new
                {
                    result = true,
                    data = reviews,
                    pageNumber,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific review details
        /// </summary>
        [HttpGet("{reviewId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetReviewDetail(long reviewId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                var review = await _reviewRepository.GetReviewDetailAsync(reviewId, userId);

                if (review == null)
                    return NotFound(new { result = false, message = "Review not found" });

                return Ok(new
                {
                    result = true,
                    data = review
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing review
        /// </summary>
        [HttpPut("update")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewRequest request)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                // Validate ratings
                if (request.OverallRating < 1 || request.OverallRating > 5)
                    return BadRequest(new { result = false, message = "Overall rating must be between 1 and 5" });

                var success = await _reviewRepository.UpdateReviewAsync(userId, request);

                if (success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Review updated successfully"
                    });
                }

                return BadRequest(new { result = false, message = "Failed to update review" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while updating the review", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteReview(long reviewId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized(new { result = false, message = "User not authenticated" });

                var success = await _reviewRepository.DeleteReviewAsync(userId, reviewId);

                if (success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Review deleted successfully"
                    });
                }

                return BadRequest(new { result = false, message = "Failed to delete review" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while deleting the review", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reviews for a specific catering (Public - no auth required)
        /// </summary>
        [HttpGet("catering/{cateringId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCateringReviews(long cateringId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _reviewRepository.GetCateringReviewsAsync(cateringId, pageNumber, pageSize);

                return Ok(new
                {
                    result = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get review statistics for a catering (Public - no auth required)
        /// </summary>
        [HttpGet("catering/{cateringId}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCateringReviewStats(long cateringId)
        {
            try
            {
                var stats = await _reviewRepository.GetCateringReviewStatsAsync(cateringId);

                return Ok(new
                {
                    result = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}
