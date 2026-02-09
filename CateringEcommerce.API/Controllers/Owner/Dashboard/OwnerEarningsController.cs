using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    [Authorize]
    [Route("api/Owner/Earnings")]
    [ApiController]
    public class OwnerEarningsController : ControllerBase
    {
        private readonly IOwnerEarningsRepository _earningsRepository;
        private readonly ILogger<OwnerEarningsController> _logger;

        public OwnerEarningsController(
            IOwnerEarningsRepository earningsRepository,
            ILogger<OwnerEarningsController> logger)
        {
            _earningsRepository = earningsRepository ?? throw new ArgumentNullException(nameof(earningsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get owner earnings summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetEarningsSummary()
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var summary = await _earningsRepository.GetEarningsSummaryAsync(ownerId);

                return Ok(new
                {
                    result = true,
                    data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting earnings summary");
                return StatusCode(500, new { result = false, message = "Failed to get earnings summary" });
            }
        }

        /// <summary>
        /// Get available balance for withdrawal
        /// </summary>
        [HttpGet("available-balance")]
        public async Task<IActionResult> GetAvailableBalance()
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var balance = await _earningsRepository.GetAvailableBalanceAsync(ownerId);

                return Ok(new
                {
                    result = true,
                    data = balance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available balance");
                return StatusCode(500, new { result = false, message = "Failed to get available balance" });
            }
        }

        /// <summary>
        /// Get settlement history with pagination
        /// </summary>
        [HttpGet("settlement-history")]
        public async Task<IActionResult> GetSettlementHistory([FromQuery] SettlementFilterDto filter)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var (settlements, totalCount) = await _earningsRepository.GetSettlementHistoryAsync(ownerId, filter);

                return Ok(new
                {
                    result = true,
                    data = settlements,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settlement history");
                return StatusCode(500, new { result = false, message = "Failed to get settlement history" });
            }
        }

        /// <summary>
        /// Request withdrawal
        /// </summary>
        [HttpPost("request-withdrawal")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto request)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid withdrawal amount" });
                }

                var response = await _earningsRepository.RequestWithdrawalAsync(ownerId, request);

                if (response.Status == "FAILED")
                {
                    return BadRequest(new { result = false, message = response.Message });
                }

                _logger.LogInformation("Withdrawal request created. OwnerId: {OwnerId}, Amount: {Amount}, WithdrawalId: {WithdrawalId}",
                    ownerId, request.Amount, response.WithdrawalId);

                return Ok(new
                {
                    result = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting withdrawal");
                return StatusCode(500, new { result = false, message = "Failed to request withdrawal" });
            }
        }

        /// <summary>
        /// Get payout history with pagination
        /// </summary>
        [HttpGet("payout-history")]
        public async Task<IActionResult> GetPayoutHistory([FromQuery] PayoutFilterDto filter)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var (payouts, totalCount) = await _earningsRepository.GetPayoutHistoryAsync(ownerId, filter);

                return Ok(new
                {
                    result = true,
                    data = payouts,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout history");
                return StatusCode(500, new { result = false, message = "Failed to get payout history" });
            }
        }

        /// <summary>
        /// Get transaction details
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetTransactionDetails(long transactionId)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var transaction = await _earningsRepository.GetTransactionDetailsAsync(ownerId, transactionId);

                if (transaction == null)
                {
                    return NotFound(new { result = false, message = "Transaction not found" });
                }

                return Ok(new
                {
                    result = true,
                    data = transaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction details");
                return StatusCode(500, new { result = false, message = "Failed to get transaction details" });
            }
        }

        /// <summary>
        /// Get earnings chart data
        /// </summary>
        [HttpGet("chart")]
        public async Task<IActionResult> GetEarningsChart([FromQuery] string period = "week")
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var chartData = await _earningsRepository.GetEarningsChartDataAsync(ownerId, period);

                return Ok(new
                {
                    result = true,
                    data = chartData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting earnings chart");
                return StatusCode(500, new { result = false, message = "Failed to get earnings chart" });
            }
        }
    }
}
