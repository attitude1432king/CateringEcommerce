using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/user/order-modification")]
    [ApiController]
    [UserAuthorize]
    public class OrderModificationController : ControllerBase
    {
        private readonly IOrderModificationRepository _modificationRepo;

        public OrderModificationController(IOrderModificationRepository modificationRepo)
        {
            _modificationRepo = modificationRepo;
        }

        /// <summary>
        /// Request guest count change for an order
        /// </summary>
        /// <param name="request">Guest count change request</param>
        /// <returns>Modification request response with pricing details</returns>
        [HttpPost("guest-count/request")]
        public async Task<IActionResult> RequestGuestCountChange([FromBody] GuestCountChangeRequestDto request)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                request.UserId = long.Parse(userIdClaim);

                var result = await _modificationRepo.RequestGuestCountChangeAsync(request);

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Failed to process guest count change request" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.RequiresPartnerApproval
                        ? $"Guest count change request submitted. Additional cost: ₹{result.AdditionalAmount:N2}. Awaiting approval."
                        : $"Guest count updated successfully. Additional cost: ₹{result.AdditionalAmount:N2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Request menu change for an order
        /// </summary>
        /// <param name="request">Menu change request</param>
        /// <returns>Modification request response</returns>
        [HttpPost("menu/request")]
        public async Task<IActionResult> RequestMenuChange([FromBody] MenuChangeRequestDto request)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                request.UserId = long.Parse(userIdClaim);

                var result = await _modificationRepo.RequestMenuChangeAsync(request);

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Failed to process menu change request" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.RequiresPartnerApproval
                        ? "Menu change request submitted. Awaiting approval."
                        : "Menu updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all modifications for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of order modifications</returns>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderModifications(long orderId)
        {
            try
            {
                var results = await _modificationRepo.GetOrderModificationsAsync(orderId);

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

        /// <summary>
        /// Get modification details
        /// </summary>
        /// <param name="modificationId">Modification ID</param>
        /// <returns>Modification details</returns>
        [HttpGet("{modificationId}")]
        public async Task<IActionResult> GetModification(long modificationId)
        {
            try
            {
                var result = await _modificationRepo.GetModificationAsync(modificationId);

                if (result == null)
                {
                    return NotFound(new { message = "Modification not found" });
                }

                // Verify user owns this modification
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (result.RequestedBy.ToString() != userIdClaim)
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
    }
}
