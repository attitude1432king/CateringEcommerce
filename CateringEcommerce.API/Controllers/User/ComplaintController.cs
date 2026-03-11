using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/user/complaint")]
    [ApiController]
    [UserAuthorize]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintRepository _complaintRepo;

        public ComplaintController(IComplaintRepository complaintRepo)
        {
            _complaintRepo = complaintRepo;
        }

        /// <summary>
        /// File a complaint for an order
        /// </summary>
        /// <param name="request">Complaint details</param>
        /// <returns>Complaint filing response</returns>
        [HttpPost("file")]
        public async Task<IActionResult> FileComplaint([FromBody] FileComplaintDto request)
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

                var result = await _complaintRepo.FileComplaintAsync(request);

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Failed to file complaint" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = $"Complaint filed successfully. Status: {result.Status}. {result.Message}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all complaints for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of complaints</returns>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderComplaints(long orderId)
        {
            try
            {
                var results = await _complaintRepo.GetComplaintsByOrderAsync(orderId);

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
        /// Get complaint details
        /// </summary>
        /// <param name="complaintId">Complaint ID</param>
        /// <returns>Complaint details</returns>
        [HttpGet("{complaintId}")]
        public async Task<IActionResult> GetComplaint(long complaintId)
        {
            try
            {
                var result = await _complaintRepo.GetComplaintAsync(complaintId);

                if (result == null)
                {
                    return NotFound(new { message = "Complaint not found" });
                }

                // Verify user owns this complaint
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
        /// Get all complaints filed by current user
        /// </summary>
        /// <returns>List of user's complaints</returns>
        [HttpGet("my-complaints")]
        public async Task<IActionResult> GetMyComplaints()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);
                var results = await _complaintRepo.GetComplaintsByUserAsync(userId);

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
