using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/complaint")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminComplaintController : ControllerBase
    {
        private readonly IComplaintRepository _complaintRepo;

        public AdminComplaintController(IComplaintRepository complaintRepo)
        {
            _complaintRepo = complaintRepo;
        }

        /// <summary>
        /// Get all pending complaints for admin review
        /// </summary>
        /// <returns>List of pending complaints</returns>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingComplaints()
        {
            try
            {
                var results = await _complaintRepo.GetPendingComplaintsAsync();

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
        /// Get complaint details (admin view)
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

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate refund amount for a complaint
        /// </summary>
        /// <param name="complaintId">Complaint ID</param>
        /// <returns>Refund calculation details</returns>
        [HttpPost("calculate-refund/{complaintId}")]
        public async Task<IActionResult> CalculateComplaintRefund(long complaintId)
        {
            try
            {
                var result = await _complaintRepo.CalculateComplaintRefundAsync(complaintId);

                if (result == null)
                {
                    return NotFound(new { message = "Complaint not found or cannot calculate refund" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = $"Calculated refund: ₹{result.RecommendedRefund:N2} (Severity factor: {result.SeverityFactor}x)"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Resolve a complaint
        /// </summary>
        /// <param name="request">Complaint resolution details</param>
        /// <returns>Resolution confirmation</returns>
        [HttpPost("resolve")]
        public async Task<IActionResult> ResolveComplaint([FromBody] ResolveComplaintDto request)
        {
            try
            {
                // Get admin ID from claims
                var adminIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                {
                    return Unauthorized(new { message = "Admin not authenticated" });
                }

                request.AdminId = long.Parse(adminIdClaim);

                var success = await _complaintRepo.ResolveComplaintAsync(request);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to resolve complaint" });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Complaint resolved successfully. Resolution: {request.ResolutionType}, Refund: ₹{request.RefundAmount:N2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Escalate a complaint
        /// </summary>
        /// <param name="complaintId">Complaint ID</param>
        /// <returns>Escalation confirmation</returns>
        [HttpPost("escalate/{complaintId}")]
        public async Task<IActionResult> EscalateComplaint(long complaintId)
        {
            try
            {
                var success = await _complaintRepo.EscalateComplaintAsync(complaintId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to escalate complaint" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Complaint escalated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}
