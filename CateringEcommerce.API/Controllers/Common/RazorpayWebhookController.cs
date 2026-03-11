using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Common
{
    /// <summary>
    /// Webhook endpoint for receiving Razorpay payment notifications
    /// This endpoint is called by Razorpay servers and MUST NOT require authentication
    /// </summary>
    [ApiController]
    [Route("api/webhooks/razorpay")]
    public class RazorpayWebhookController : ControllerBase
    {
        private readonly ILogger<RazorpayWebhookController> _logger;
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly PaymentStageService _paymentStageService;
        private readonly IOrderRepository _orderRepository;

        public RazorpayWebhookController(
            ILogger<RazorpayWebhookController> logger,
            IRazorpayPaymentService razorpayService,
            PaymentStageService paymentStageService,
            IOrderRepository orderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _razorpayService = razorpayService ?? throw new ArgumentNullException(nameof(razorpayService));
            _paymentStageService = paymentStageService ?? throw new ArgumentNullException(nameof(paymentStageService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        /// <summary>
        /// Razorpay webhook endpoint for payment notifications
        /// Handles: payment.authorized, payment.captured, payment.failed events
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                // Read the raw request body
                string requestBody;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    _logger.LogWarning("Received empty webhook payload from Razorpay");
                    return BadRequest(new { error = "Empty payload" });
                }

                _logger.LogInformation("Received Razorpay webhook payload: {Payload}", requestBody);

                // Get signature from headers
                var signature = Request.Headers["X-Razorpay-Signature"].ToString();
                if (string.IsNullOrWhiteSpace(signature))
                {
                    _logger.LogWarning("Webhook received without signature header");
                    return BadRequest(new { error = "Missing signature" });
                }

                // Verify webhook signature
                bool isValid = _razorpayService.VerifyWebhookSignature(requestBody, signature);
                if (!isValid)
                {
                    _logger.LogError("SECURITY ALERT: Invalid webhook signature received. Possible attack attempt.");
                    return Unauthorized(new { error = "Invalid signature" });
                }

                _logger.LogInformation("Webhook signature verified successfully");

                // Parse webhook payload
                RazorpayWebhookDto? webhookData;
                try
                {
                    webhookData = JsonSerializer.Deserialize<RazorpayWebhookDto>(requestBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize webhook payload");
                    return BadRequest(new { error = "Invalid JSON payload" });
                }

                if (webhookData == null || webhookData.Payload == null || webhookData.Payload.Payment == null)
                {
                    _logger.LogWarning("Webhook payload missing required fields");
                    return BadRequest(new { error = "Invalid payload structure" });
                }

                var payment = webhookData.Payload.Payment;
                var eventType = webhookData.Event;

                _logger.LogInformation("Processing webhook event: {Event}, PaymentId: {PaymentId}, OrderId: {OrderId}",
                    eventType, payment.Id, payment.OrderId);

                // Check for idempotency - has this payment already been processed?
                bool alreadyProcessed = await _paymentStageService.IsPaymentAlreadyProcessedAsync(payment.Id);
                if (alreadyProcessed)
                {
                    _logger.LogInformation("Payment {PaymentId} already processed. Returning success (idempotent).", payment.Id);
                    return Ok(new { status = "already_processed", message = "Payment already processed" });
                }

                // Extract order details from notes
                if (payment.Notes == null || !payment.Notes.ContainsKey("order_id") || !payment.Notes.ContainsKey("stage_type"))
                {
                    _logger.LogError("Webhook payment missing required notes fields (order_id, stage_type). PaymentId: {PaymentId}", payment.Id);
                    return BadRequest(new { error = "Missing order metadata in payment notes" });
                }

                if (!long.TryParse(payment.Notes["order_id"], out long orderId))
                {
                    _logger.LogError("Invalid order_id in webhook notes. PaymentId: {PaymentId}", payment.Id);
                    return BadRequest(new { error = "Invalid order_id" });
                }

                string stageType = payment.Notes["stage_type"];

                // Process payment based on event type
                switch (eventType)
                {
                    case "payment.captured":
                    case "payment.authorized":
                        await ProcessSuccessfulPaymentAsync(payment, orderId, stageType);
                        break;

                    case "payment.failed":
                        await ProcessFailedPaymentAsync(payment, orderId, stageType);
                        break;

                    default:
                        _logger.LogInformation("Received unhandled webhook event: {Event}", eventType);
                        break;
                }

                return Ok(new { status = "processed" });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "CRITICAL: Webhook processing failed with exception");
                // Return 200 to prevent Razorpay from retrying, but log the error for manual investigation
                return Ok(new { status = "error_logged", message = "Error logged for manual review" });
            }
        }

        /// <summary>
        /// Process successful payment (captured or authorized)
        /// </summary>
        private async Task ProcessSuccessfulPaymentAsync(RazorpayWebhookPaymentDto payment, long orderId, string stageType)
        {
            try
            {
                _logger.LogInformation("Processing successful payment: PaymentId={PaymentId}, OrderId={OrderId}, StageType={StageType}",
                    payment.Id, orderId, stageType);

                var processPaymentDto = new ProcessPaymentStageDto
                {
                    OrderId = orderId,
                    StageType = stageType,
                    PaymentMethod = payment.Method ?? "Online",
                    PaymentGateway = "Razorpay",
                    RazorpayOrderId = payment.OrderId,
                    RazorpayPaymentId = payment.Id,
                    TransactionId = payment.Id
                };

                bool paymentUpdated = await _paymentStageService.ProcessPaymentStageAsync(processPaymentDto);

                if (!paymentUpdated)
                {
                    _logger.LogCritical("CRITICAL: Webhook payment verified but database update failed - OrderId: {OrderId}, PaymentId: {PaymentId}. MANUAL RECONCILIATION REQUIRED!",
                        orderId, payment.Id);
                }
                else
                {
                    _logger.LogInformation("Payment successfully processed via webhook - OrderId: {OrderId}, StageType: {StageType}",
                        orderId, stageType);

                    // Update order status based on payment stage
                    await UpdateOrderStatusAsync(orderId, stageType, payment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "CRITICAL: Exception processing successful webhook payment - OrderId: {OrderId}, PaymentId: {PaymentId}. MANUAL RECONCILIATION REQUIRED!",
                    orderId, payment.Id);
            }
        }

        /// <summary>
        /// Process failed payment
        /// </summary>
        private async Task ProcessFailedPaymentAsync(RazorpayWebhookPaymentDto payment, long orderId, string stageType)
        {
            try
            {
                _logger.LogWarning("Processing failed payment: PaymentId={PaymentId}, OrderId={OrderId}, StageType={StageType}",
                    payment.Id, orderId, stageType);

                // Log the failed payment - the existing payment stage should remain in Pending status
                // so the user can retry the payment
                // We just record that this particular payment attempt failed

                _logger.LogInformation("Failed payment recorded - OrderId: {OrderId}, PaymentId: {PaymentId}, StageType: {StageType}",
                    orderId, payment.Id, stageType);

                // The payment stage remains in 'Pending' status so user can retry
                // Future enhancement: Could track failed attempts in a separate table
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed payment webhook - OrderId: {OrderId}, PaymentId: {PaymentId}",
                    orderId, payment.Id);
            }
        }

        /// <summary>
        /// Update order status based on payment stage completion
        /// </summary>
        private async Task UpdateOrderStatusAsync(long orderId, string stageType, string paymentId)
        {
            try
            {
                string statusNote = $"Order confirmed after successful {stageType} payment via Razorpay webhook (Payment ID: {paymentId})";

                if (stageType == "PreBooking" || stageType == "Full")
                {
                    await _orderRepository.UpdateOrderStatusAsync(orderId, "Confirmed", statusNote);
                    _logger.LogInformation("Order status updated to Confirmed via webhook - OrderId: {OrderId}", orderId);
                }
                else if (stageType == "PostEvent")
                {
                    _logger.LogInformation("PostEvent payment completed via webhook - OrderId: {OrderId}", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status via webhook - OrderId: {OrderId}", orderId);
            }
        }
    }
}
