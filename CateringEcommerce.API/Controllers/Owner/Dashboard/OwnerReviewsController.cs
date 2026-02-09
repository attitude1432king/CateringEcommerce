using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Reviews Controller
    /// Provides review management functionality for partner owners
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerReviewsController : ControllerBase
    {
        private readonly ILogger<OwnerReviewsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly string _connStr;

        public OwnerReviewsController(
            ILogger<OwnerReviewsController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get filtered and paginated reviews list
        /// </summary>
        [HttpPost("list")]
        public async Task<IActionResult> GetReviews([FromBody] OwnerReviewFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting reviews list for owner {ownerId}, page: {filter.Page}");

                var repository = new OwnerReviewRepository(_connStr);
                var reviews = await repository.GetReviews(ownerId, filter);

                return ApiResponseHelper.Success(reviews, "Reviews retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews list");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving reviews."));
            }
        }

        /// <summary>
        /// Get review statistics for dashboard
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetReviewStats()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting review stats for owner {ownerId}");

                var repository = new OwnerReviewRepository(_connStr);
                var stats = await repository.GetReviewStats(ownerId);

                return ApiResponseHelper.Success(stats, "Review statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review stats");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving review statistics."));
            }
        }

        /// <summary>
        /// Submit owner reply to a review
        /// </summary>
        [HttpPost("{reviewId}/reply")]
        public async Task<IActionResult> SubmitReply(long reviewId, [FromBody] OwnerReviewReplyDto reply)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(reply.ReplyText))
                {
                    return ApiResponseHelper.Failure("Reply text cannot be empty.");
                }

                _logger.LogInformation($"Owner {ownerId} submitting reply to review {reviewId}");

                var repository = new OwnerReviewRepository(_connStr);
                var success = await repository.SubmitReply(ownerId, reviewId, reply.ReplyText);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Reply submitted successfully.");
                }
                else
                {
                    return ApiResponseHelper.Failure("Review not found or does not belong to this owner.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting reply to review {reviewId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting the reply."));
            }
        }
    }
}
