using CateringEcommerce.Domain.Interfaces.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/modification")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminModificationController : ControllerBase
    {
        private readonly IOrderModificationRepository _modificationRepo;

        public AdminModificationController(IOrderModificationRepository modificationRepo)
        {
            _modificationRepo = modificationRepo;
        }

        /// <summary>
        /// Get all pending modification requests
        /// </summary>
        /// <returns>List of pending modifications</returns>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingModifications()
        {
            try
            {
                // Get all pending modifications
                var results = await _modificationRepo.GetPendingModificationsAsync();

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

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Approve a modification request
        /// </summary>
        /// <param name="modificationId">Modification ID</param>
        /// <param name="adminNotes">Admin notes (optional)</param>
        /// <returns>Approval confirmation</returns>
        [HttpPost("approve/{modificationId}")]
        public async Task<IActionResult> ApproveModification(long modificationId, [FromBody] string adminNotes = null)
        {
            try
            {
                // Get admin ID from claims
                var adminIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                {
                    return Unauthorized(new { message = "Admin not authenticated" });
                }

                var adminId = long.Parse(adminIdClaim);
                var success = await _modificationRepo.ApproveModificationAsync(modificationId, adminId, "Admin");

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to approve modification" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Modification approved successfully. Order updated."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reject a modification request
        /// </summary>
        /// <param name="modificationId">Modification ID</param>
        /// <param name="rejectionReason">Rejection reason</param>
        /// <returns>Rejection confirmation</returns>
        [HttpPost("reject/{modificationId}")]
        public async Task<IActionResult> RejectModification(long modificationId, [FromBody] string rejectionReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    return BadRequest(new { success = false, message = "Rejection reason is required" });
                }

                // Get admin ID from claims
                var adminIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                {
                    return Unauthorized(new { message = "Admin not authenticated" });
                }

                var adminId = long.Parse(adminIdClaim);
                var success = await _modificationRepo.RejectModificationAsync(modificationId, adminId, rejectionReason);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to reject modification" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Modification rejected. Reason: " + rejectionReason
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all modifications for an order (admin view)
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
    }
}
