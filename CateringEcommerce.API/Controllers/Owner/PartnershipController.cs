using CateringEcommerce.API.Filters;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Route("api/owner/partnership")]
    [ApiController]
    [OwnerAuthorize]

    public class PartnershipController : ControllerBase
    {
        private readonly IPartnershipRepository _partnershipRepo;

        public PartnershipController(IPartnershipRepository partnershipRepo)
        {
            _partnershipRepo = partnershipRepo;
        }

        /// <summary>
        /// Get current partnership tier details
        /// </summary>
        /// <returns>Partner's current tier information</returns>
        [HttpGet("tier")]
        public async Task<IActionResult> GetPartnerTier()
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var result = await _partnershipRepo.GetPartnerTierAsync(ownerId);

                if (result == null)
                {
                    return NotFound(new { message = "Partnership tier not found. Please contact support." });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.IsLockPeriodActive
                        ? $"You are locked in at {result.CurrentCommissionRate}% commission until {result.TierLockEndDate:yyyy-MM-dd}"
                        : $"Current commission rate: {result.CurrentCommissionRate}%"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive partner dashboard
        /// </summary>
        /// <returns>Partnership dashboard with tier, deposit, and performance metrics</returns>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetPartnerDashboard()
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var result = await _partnershipRepo.GetPartnerDashboardAsync(ownerId);

                if (result == null)
                {
                    return NotFound(new { message = "Partnership dashboard not available" });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get commission tier change history
        /// </summary>
        /// <returns>List of tier changes</returns>
        [HttpGet("commission-history")]
        public async Task<IActionResult> GetCommissionHistory()
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var results = await _partnershipRepo.GetCommissionHistoryAsync(ownerId);

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
        /// Acknowledge a tier change notification
        /// </summary>
        /// <param name="historyId">Tier history ID</param>
        /// <returns>Acknowledgment confirmation</returns>
        [HttpPost("acknowledge-tier-change/{historyId}")]
        public async Task<IActionResult> AcknowledgeTierChange(long historyId)
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var success = await _partnershipRepo.AcknowledgeTierChangeAsync(historyId, ownerId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to acknowledge tier change" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Tier change acknowledged successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get security deposit details
        /// </summary>
        /// <returns>Security deposit information</returns>
        [HttpGet("security-deposit")]
        public async Task<IActionResult> GetSecurityDeposit()
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var result = await _partnershipRepo.GetPartnerSecurityDepositAsync(ownerId);

                if (result == null)
                {
                    return NotFound(new { message = "Security deposit not found" });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = $"Current balance: ₹{result.CurrentBalance:N2}, Available: ₹{result.AvailableBalance:N2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get deposit transaction history
        /// </summary>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of deposit transactions</returns>
        [HttpGet("deposit-transactions")]
        public async Task<IActionResult> GetDepositTransactions([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var results = await _partnershipRepo.GetDepositTransactionHistoryAsync(ownerId, startDate, endDate);

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
        /// Request refund of security deposit
        /// </summary>
        /// <param name="request">Refund request details</param>
        /// <returns>Refund request confirmation</returns>
        [HttpPost("request-deposit-refund")]
        public async Task<IActionResult> RequestDepositRefund([FromBody] RequestDepositRefundDto request)
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                request.OwnerId = long.Parse(ownerIdClaim);
                var success = await _partnershipRepo.RequestDepositRefundAsync(request);

                if (!success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to request deposit refund. Please ensure you have sufficient available balance and no pending holds."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Deposit refund request submitted successfully. Your full available balance will be refunded. Your request will be reviewed by admin."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate commission for an amount
        /// </summary>
        /// <param name="orderAmount">Order amount</param>
        /// <returns>Commission calculation</returns>
        [HttpGet("calculate-commission")]
        public async Task<IActionResult> CalculateCommission([FromQuery] decimal orderAmount)
        {
            try
            {
                // Get owner ID from claims
                var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim))
                {
                    return Unauthorized(new { message = "Owner not authenticated" });
                }

                var ownerId = long.Parse(ownerIdClaim);
                var commission = await _partnershipRepo.CalculateCommissionAsync(ownerId, orderAmount);
                var netAmount = orderAmount - commission;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderAmount,
                        commissionAmount = commission,
                        netAmount,
                        commissionPercentage = (commission / orderAmount) * 100
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
