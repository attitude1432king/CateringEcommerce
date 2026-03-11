using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Payment
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ISplitPaymentRepository _paymentRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ISplitPaymentRepository paymentRepo,
            ICurrentUserService currentUser,
            ILogger<PaymentController> logger)
        {
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // =============================================
        // Payment Initialization & Processing
        // =============================================

        /// <summary>
        /// Initialize payment breakdown for an order
        /// </summary>
        [HttpPost("initialize")]
        [Authorize]
        public async Task<IActionResult> InitializeOrderPayment([FromBody] InitializePaymentRequest request)
        {
            try
            {
                var result = await _paymentRepo.InitializeOrderPaymentAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Process advance payment (30-40% of order amount)
        /// </summary>
        [HttpPost("advance")]
        [Authorize]
        public async Task<IActionResult> ProcessAdvancePayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var result = await _paymentRepo.ProcessAdvancePaymentAsync(request);
                return Ok(new { success = true, data = result, message = "Advance payment processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Process final payment (remaining amount)
        /// </summary>
        [HttpPost("final")]
        [Authorize]
        public async Task<IActionResult> ProcessFinalPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var result = await _paymentRepo.ProcessFinalPaymentAsync(request);
                return Ok(new { success = true, data = result, message = "Final payment processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        // =============================================
        // Payment Information & Status
        // =============================================

        /// <summary>
        /// Get complete payment summary for an order
        /// </summary>
        [HttpGet("summary/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentSummary(long orderId)
        {
            try
            {
                var summary = await _paymentRepo.GetPaymentSummaryAsync(orderId);
                var transactions = await _paymentRepo.GetOrderTransactionsAsync(orderId);
                var escrowLedger = await _paymentRepo.GetEscrowLedgerAsync(orderId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        summary,
                        transactions,
                        escrowLedger
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get all transactions for an order
        /// </summary>
        [HttpGet("transactions/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderTransactions(long orderId)
        {
            try
            {
                var result = await _paymentRepo.GetOrderTransactionsAsync(orderId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get escrow ledger for an order
        /// </summary>
        [HttpGet("escrow-ledger/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetEscrowLedger(long orderId)
        {
            try
            {
                var result = await _paymentRepo.GetEscrowLedgerAsync(orderId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        // =============================================
        // EMI Management
        // =============================================

        /// <summary>
        /// Get available EMI plans for order amount
        /// </summary>
        [HttpGet("emi-plans")]
        [Authorize]
        public async Task<IActionResult> GetEMIPlans([FromQuery] decimal orderAmount)
        {
            try
            {
                var result = await _paymentRepo.GetAvailableEMIPlansAsync(orderAmount);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Calculate EMI breakdown
        /// </summary>
        [HttpPost("emi-calculate")]
        [Authorize]
        public async Task<IActionResult> CalculateEMI([FromBody] EMICalculationRequest request)
        {
            try
            {
                var result = await _paymentRepo.CalculateEMIAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        // =============================================
        // Partner Settlement Management
        // =============================================

        /// <summary>
        /// Release advance amount to partner (Admin only)
        /// </summary>
        [HttpPost("partner/release-advance")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReleaseAdvanceToPartner([FromBody] ReleaseAdvanceRequest request)
        {
            try
            {
                var result = await _paymentRepo.ReleaseAdvanceToPartnerAsync(request);
                return Ok(new { success = true, message = "Advance released to partner successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Process final partner settlement after commission deduction (Admin only)
        /// </summary>
        [HttpPost("partner/final-settlement")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessFinalPartnerPayout([FromBody] ProcessFinalPayoutRequest request)
        {
            try
            {
                var result = await _paymentRepo.ProcessFinalPartnerPayoutAsync(request);
                return Ok(new { success = true, message = "Final settlement processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get partner payout requests (Owner role only, can only access own data)
        /// </summary>
        [HttpGet("partner/payout-requests/{cateringOwnerId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetPartnerPayoutRequests(long cateringOwnerId)
        {
            try
            {
                // SECURITY: Verify that the authenticated owner is requesting their own data
                var currentOwnerId = _currentUser.UserId;

                if (cateringOwnerId != currentOwnerId)
                {
                    _logger.LogWarning("SECURITY ALERT: User {CurrentUserId} attempted to access payout requests for Owner {RequestedOwnerId}",
                        currentOwnerId, cateringOwnerId);
                    return Forbid(); // Return 403 Forbidden
                }

                var result = await _paymentRepo.GetPartnerPayoutRequestsAsync(cateringOwnerId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partner payout requests for Owner {OwnerId}", cateringOwnerId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving payout requests." });
            }
        }

        // =============================================
        // Dashboards
        // =============================================

        /// <summary>
        /// Get payment dashboard for admin
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentDashboard()
        {
            try
            {
                var result = await _paymentRepo.GetPaymentDashboardAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get partner settlement dashboard (Owner role only, can only access own data)
        /// </summary>
        [HttpGet("partner/dashboard/{cateringOwnerId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetPartnerPayoutDashboard(long cateringOwnerId)
        {
            try
            {
                // SECURITY: Verify that the authenticated owner is requesting their own dashboard
                var currentOwnerId = _currentUser.UserId;

                if (cateringOwnerId != currentOwnerId)
                {
                    _logger.LogWarning("SECURITY ALERT: User {CurrentUserId} attempted to access dashboard for Owner {RequestedOwnerId}",
                        currentOwnerId, cateringOwnerId);
                    return Forbid(); // Return 403 Forbidden
                }

                var result = await _paymentRepo.GetPartnerPayoutDashboardAsync(cateringOwnerId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partner payout dashboard for Owner {OwnerId}", cateringOwnerId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving dashboard data." });
            }
        }

        /// <summary>
        /// Get escrow dashboard (Admin only)
        /// </summary>
        [HttpGet("escrow/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEscrowDashboard()
        {
            try
            {
                var result = await _paymentRepo.GetEscrowDashboardAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        // =============================================
        // Payment Gateway Configuration
        // =============================================

        /// <summary>
        /// Get payment gateway configuration
        /// </summary>
        [HttpGet("gateway/{gatewayName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentGatewayConfig(string gatewayName)
        {
            try
            {
                var result = await _paymentRepo.GetPaymentGatewayConfigAsync(gatewayName);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get all active payment gateways
        /// </summary>
        [HttpGet("gateways")]
        [Authorize]
        public async Task<IActionResult> GetActivePaymentGateways()
        {
            try
            {
                var result = await _paymentRepo.GetActivePaymentGatewaysAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment operation failed");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }
    }
}
