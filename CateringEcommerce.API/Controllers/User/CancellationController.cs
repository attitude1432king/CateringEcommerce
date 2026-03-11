using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/user/cancellation")]
    [ApiController]
    [UserAuthorize]
    public class CancellationController : ControllerBase
    {
        private readonly ICancellationRepository _cancellationRepo;

        public CancellationController(ICancellationRepository cancellationRepo)
        {
            _cancellationRepo = cancellationRepo;
        }

        /// <summary>
        /// Calculate cancellation refund based on policy
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Cancellation policy calculation</returns>
        [HttpGet("policy/calculate")]
        public async Task<IActionResult> CalculateCancellationPolicy([FromQuery] long orderId)
        {
            try
            {
                var result = await _cancellationRepo.CalculateCancellationRefundAsync(orderId);

                if (result == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "Cancellation policy calculated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Submit a cancellation request
        /// </summary>
        /// <param name="request">Cancellation request details</param>
        /// <returns>Cancellation request confirmation</returns>
        [HttpPost("request")]
        public async Task<IActionResult> RequestCancellation([FromBody] CreateCancellationRequestDto request)
        {
            try
            {
                // Get user ID from claims (authenticated user)
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                request.UserId = long.Parse(userIdClaim);

                var result = await _cancellationRepo.ProcessCancellationRequestAsync(request);

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Failed to process cancellation request" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = $"Cancellation request submitted. Expected refund: ₹{result.RefundAmount:N2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get cancellation request details
        /// </summary>
        /// <param name="cancellationId">Cancellation ID</param>
        /// <returns>Cancellation request details</returns>
        [HttpGet("{cancellationId}")]
        public async Task<IActionResult> GetCancellationRequest(long cancellationId)
        {
            try
            {
                var result = await _cancellationRepo.GetCancellationRequestAsync(cancellationId);

                if (result == null)
                {
                    return NotFound(new { message = "Cancellation request not found" });
                }

                // Verify user owns this cancellation request
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (result.UserId.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get cancellation request by order ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Cancellation request for the order</returns>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetCancellationByOrder(long orderId)
        {
            try
            {
                var result = await _cancellationRepo.GetCancellationRequestByOrderAsync(orderId);

                if (result == null)
                {
                    return NotFound(new { message = "No cancellation request found for this order" });
                }

                // Verify user owns this order
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (result.UserId.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all cancellation requests for current user
        /// </summary>
        /// <returns>List of user's cancellation requests</returns>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyCancellationRequests()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);
                var results = await _cancellationRepo.GetUserCancellationRequestsAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = results,
                    count = results.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
