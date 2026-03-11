using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [UserAuthorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class PaymentGatewayController : ControllerBase
    {
        private readonly ILogger<PaymentGatewayController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly INotificationHelper _notificationHelper;
        private readonly PaymentStageService _paymentStageService;
        private readonly IOrderRepository _orderRepository;

        public PaymentGatewayController(
            ILogger<PaymentGatewayController> logger,
            ICurrentUserService currentUser,
            IRazorpayPaymentService razorpayService,
            INotificationHelper notificationHelper,
            PaymentStageService paymentStageService,
            IOrderRepository orderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _razorpayService = razorpayService ?? throw new ArgumentNullException(nameof(razorpayService));
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            _paymentStageService = paymentStageService ?? throw new ArgumentNullException(nameof(paymentStageService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        // ===================================
        // POST: api/User/PaymentGateway/CreateRazorpayOrder
        // Create a Razorpay order for payment
        // ===================================
        [HttpPost("CreateRazorpayOrder")]
        public async Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayOrderRequestDto orderRequest)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                // Validate order request
                if (orderRequest == null)
                {
                    return ApiResponseHelper.Failure("Invalid order request.");
                }

                // Ensure the user ID matches
                if (orderRequest.UserId != userId)
                {
                    return ApiResponseHelper.Failure("User ID mismatch. Unauthorized access.");
                }

                // SECURITY FIX: Verify order ownership before processing payment
                var orderDetails = await _orderRepository.GetOrderByIdAsync(orderRequest.OrderId, userId);
                if (orderDetails == null)
                {
                    _logger.LogWarning("Payment attempt for non-existent or unauthorized order. OrderId: {OrderId}, UserId: {UserId}",
                        orderRequest.OrderId, userId);
                    return ApiResponseHelper.Failure("Order not found or you do not have permission to access this order.");
                }

                // SECURITY FIX: Fetch the expected payment amount from database (NOT from client)
                var paymentStage = await _paymentStageService.GetPaymentStageByTypeAsync(orderRequest.OrderId, orderRequest.StageType);
                if (paymentStage == null)
                {
                    _logger.LogWarning("Payment stage not found. OrderId: {OrderId}, StageType: {StageType}",
                        orderRequest.OrderId, orderRequest.StageType);
                    return ApiResponseHelper.Failure($"Payment stage '{orderRequest.StageType}' not found for this order.");
                }

                // SECURITY FIX: Verify payment stage is still pending
                if (paymentStage.Status != "Pending")
                {
                    _logger.LogWarning("Payment attempt for non-pending stage. OrderId: {OrderId}, StageType: {StageType}, Status: {Status}",
                        orderRequest.OrderId, orderRequest.StageType, paymentStage.Status);

                    if (paymentStage.Status == "Success")
                    {
                        return ApiResponseHelper.Failure("This payment has already been completed.");
                    }

                    return ApiResponseHelper.Failure($"Payment cannot be processed. Current status: {paymentStage.Status}");
                }

                // SECURITY FIX: Use the database amount (ignore client-provided amount)
                decimal authorizedAmount = paymentStage.StageAmount;

                // Optional: Validate client amount matches database (for error detection, not security)
                if (orderRequest.Amount > 0 && Math.Abs(orderRequest.Amount - authorizedAmount) > 0.01m)
                {
                    _logger.LogWarning("Client-provided amount mismatch. OrderId: {OrderId}, ClientAmount: {ClientAmount}, DatabaseAmount: {DatabaseAmount}",
                        orderRequest.OrderId, orderRequest.Amount, authorizedAmount);
                    // Don't reject - just log and use database amount
                }

                _logger.LogInformation("Creating Razorpay order for user {UserId}, OrderId: {OrderId}, Amount: ₹{Amount} (verified from database), Stage: {StageType}",
                    userId, orderRequest.OrderId, authorizedAmount, orderRequest.StageType);

                // Create Razorpay order with VALIDATED amount from database
                var validatedOrderRequest = new RazorpayOrderRequestDto
                {
                    Amount = authorizedAmount, // Use database amount, NOT client amount
                    Receipt = orderRequest.Receipt,
                    OrderId = orderRequest.OrderId,
                    UserId = userId, // Use authenticated user ID
                    StageType = orderRequest.StageType,
                    Notes = orderRequest.Notes
                };

                RazorpayOrderResponseDto razorpayOrder = await _razorpayService.CreateOrderAsync(validatedOrderRequest);

                _logger.LogInformation("Razorpay order created successfully: {RazorpayOrderId}, Amount: ₹{Amount}",
                    razorpayOrder.Id, authorizedAmount);

                return ApiResponseHelper.Success(razorpayOrder, "Payment order created successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Razorpay order creation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Razorpay order validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Razorpay order");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while creating the payment order. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/PaymentGateway/VerifyPayment
        // Verify Razorpay payment signature
        // ===================================
        [HttpPost("VerifyPayment")]
        public async Task<IActionResult> VerifyPayment([FromBody] RazorpayPaymentVerificationDto verificationData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                // Validate verification data
                if (verificationData == null)
                {
                    return ApiResponseHelper.Failure("Invalid verification data.");
                }

                _logger.LogInformation($"Verifying payment for user {userId}, OrderId: {verificationData.OrderId}, PaymentId: {verificationData.RazorpayPaymentId}");

                // Verify payment signature
                bool isValid = _razorpayService.VerifyPaymentSignature(verificationData);

                if (!isValid)
                {
                    _logger.LogWarning($"Payment verification failed for OrderId: {verificationData.OrderId}");

                    // Send payment failed notification
                    try
                    {
                        var userEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                        var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value ?? "Customer";
                        var userPhone = User.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;

                        await _notificationHelper.SendPaymentNotificationAsync(
                            "PAYMENT_FAILED",
                            userName,
                            userEmail ?? "",
                            userPhone ?? "",
                            new Dictionary<string, object>
                            {
                                { "customer_name", userName },
                                { "order_number", verificationData.OrderId },
                                { "reason", "Invalid payment signature" },
                                { "user_id", userId.ToString() },
                                { "order_id", verificationData.OrderId },
                                { "retry_url", $"https://enyvora.com/orders/{verificationData.OrderId}/payment" },
                                { "support_email", "support@enyvora.com" },
                                { "support_phone", "+91-1234567890" }
                            },
                            notifyAdmin: true // Notify admin for failed payments
                        );
                        _logger.LogInformation("Payment failed notification sent. OrderId: {OrderId}, UserId: {UserId}",
                            verificationData.OrderId, userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payment failed notification. OrderId: {OrderId}", verificationData.OrderId);
                    }

                    return ApiResponseHelper.Failure("Payment verification failed. Invalid signature.", "error");
                }

                _logger.LogInformation($"Payment verified successfully for OrderId: {verificationData.OrderId}");

                // Update payment stage status to "Success" in database
                try
                {
                    var processPaymentDto = new ProcessPaymentStageDto
                    {
                        OrderId = verificationData.OrderId,
                        StageType = verificationData.StageType,
                        PaymentMethod = "Online",
                        PaymentGateway = "Razorpay",
                        RazorpayOrderId = verificationData.RazorpayOrderId,
                        RazorpayPaymentId = verificationData.RazorpayPaymentId,
                        RazorpaySignature = verificationData.RazorpaySignature,
                        TransactionId = verificationData.RazorpayPaymentId
                    };

                    bool paymentUpdated = await _paymentStageService.ProcessPaymentStageAsync(processPaymentDto);

                    if (!paymentUpdated)
                    {
                        _logger.LogCritical("CRITICAL: Payment verified but database update failed - OrderId: {OrderId}, PaymentId: {PaymentId}. Manual reconciliation required!",
                            verificationData.OrderId, verificationData.RazorpayPaymentId);

                        // Still return success to user since payment was actually verified
                        // But log critical error for manual reconciliation
                    }
                    else
                    {
                        _logger.LogInformation("Payment stage updated successfully in database - OrderId: {OrderId}, StageType: {StageType}",
                            verificationData.OrderId, verificationData.StageType);

                        // Update order status based on payment stage type
                        if (verificationData.StageType == "PreBooking" || verificationData.StageType == "Full")
                        {
                            // For PreBooking or Full payment, update order status to Confirmed
                            await _orderRepository.UpdateOrderStatusAsync(
                                verificationData.OrderId,
                                "Confirmed",
                                $"Order confirmed after successful {verificationData.StageType} payment via Razorpay (Payment ID: {verificationData.RazorpayPaymentId})"
                            );
                            _logger.LogInformation("Order status updated to Confirmed - OrderId: {OrderId}", verificationData.OrderId);
                        }
                        else if (verificationData.StageType == "PostEvent")
                        {
                            // For PostEvent payment, order should already be in Completed status
                            // Just log for reference
                            _logger.LogInformation("PostEvent payment completed - OrderId: {OrderId}", verificationData.OrderId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "CRITICAL: Exception while updating payment in database - OrderId: {OrderId}, PaymentId: {PaymentId}. Manual reconciliation required!",
                        verificationData.OrderId, verificationData.RazorpayPaymentId);

                    // Continue execution to send notification
                    // Payment was verified, so we don't want to fail the user's request
                }

                // Send payment success notification
                try
                {
                    // Get user details
                    var userEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                    var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value ?? "Customer";
                    var userPhone = User.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;

                    // Get payment amount from Razorpay order
                    var paymentDetails = await _razorpayService.GetPaymentDetailsAsync(verificationData.RazorpayPaymentId);
                    var amount = paymentDetails != null && paymentDetails.ContainsKey("Amount")
                        ? Convert.ToInt32(paymentDetails["Amount"])
                        : 0;

                    await _notificationHelper.SendPaymentNotificationAsync(
                        "PAYMENT_SUCCESS",
                        userName,
                        userEmail ?? "",
                        userPhone ?? "",
                        new Dictionary<string, object>
                        {
                            { "customer_name", userName },
                            { "order_number", verificationData.OrderId },
                            { "amount", (amount / 100m).ToString("N2") }, // Razorpay amount is in paise
                            { "transaction_id", verificationData.RazorpayPaymentId },
                            { "payment_method", "Razorpay" },
                            { "payment_date", DateTime.Now.ToString("dd MMM yyyy hh:mm tt") },
                            { "user_id", userId.ToString() },
                            { "order_id", verificationData.OrderId },
                            { "order_url", $"https://enyvora.com/orders/{verificationData.OrderId}" }
                        },
                        notifyAdmin: false
                    );
                    _logger.LogInformation("Payment success notification sent. OrderId: {OrderId}, UserId: {UserId}",
                        verificationData.OrderId, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send payment success notification. OrderId: {OrderId}", verificationData.OrderId);
                }

                return ApiResponseHelper.Success(new
                {
                    verified = true,
                    orderId = verificationData.OrderId,
                    razorpayOrderId = verificationData.RazorpayOrderId,
                    razorpayPaymentId = verificationData.RazorpayPaymentId
                }, "Payment verified successfully!");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Payment verification validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while verifying the payment. Please try again."));
            }
        }

        // ===================================
        // GET: api/User/PaymentGateway/GetPaymentDetails/{paymentId}
        // Get payment details from Razorpay
        // ===================================
        [HttpGet("GetPaymentDetails/{paymentId}")]
        public async Task<IActionResult> GetPaymentDetails(string paymentId)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (string.IsNullOrEmpty(paymentId))
                {
                    return ApiResponseHelper.Failure("Payment ID is required.");
                }

                _logger.LogInformation($"Fetching payment details for user {userId}, PaymentId: {paymentId}");

                // Get payment details
                var paymentDetails = await _razorpayService.GetPaymentDetailsAsync(paymentId);

                _logger.LogInformation($"Payment details fetched successfully for PaymentId: {paymentId}");

                return ApiResponseHelper.Success(paymentDetails);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Get payment details validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching payment details for PaymentId: {paymentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching payment details. Please try again."));
            }
        }

        // ===================================
        // GET: api/User/PaymentGateway/GetOrderDetails/{razorpayOrderId}
        // Get order details from Razorpay
        // ===================================
        [HttpGet("GetOrderDetails/{razorpayOrderId}")]
        public async Task<IActionResult> GetOrderDetails(string razorpayOrderId)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (string.IsNullOrEmpty(razorpayOrderId))
                {
                    return ApiResponseHelper.Failure("Razorpay Order ID is required.");
                }

                _logger.LogInformation($"Fetching order details for user {userId}, RazorpayOrderId: {razorpayOrderId}");

                // Get order details
                var orderDetails = await _razorpayService.GetOrderDetailsAsync(razorpayOrderId);

                _logger.LogInformation($"Order details fetched successfully for RazorpayOrderId: {razorpayOrderId}");

                return ApiResponseHelper.Success(orderDetails);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Get order details validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order details for RazorpayOrderId: {razorpayOrderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching order details. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/PaymentGateway/ProcessRefund
        // Process refund for a payment
        // ===================================
        [HttpPost("ProcessRefund")]
        public async Task<IActionResult> ProcessRefund([FromBody] RefundRequestDto refundRequest)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (refundRequest == null)
                {
                    return ApiResponseHelper.Failure("Invalid refund request.");
                }

                // SECURITY FIX: Verify payment ownership before processing refund
                // Step 1: Get the payment stage associated with this Razorpay payment ID
                var paymentStage = await _paymentStageService.GetPaymentStageByRazorpayPaymentIdAsync(refundRequest.PaymentId);
                if (paymentStage == null)
                {
                    _logger.LogWarning("Refund attempt for non-existent payment. PaymentId: {PaymentId}, UserId: {UserId}",
                        refundRequest.PaymentId, userId);
                    return ApiResponseHelper.Failure("Payment not found.");
                }

                // Step 2: Verify the associated order belongs to the authenticated user
                var orderDetails = await _orderRepository.GetOrderByIdAsync(paymentStage.OrderId, userId);
                if (orderDetails == null)
                {
                    _logger.LogWarning("SECURITY ALERT: User {UserId} attempted to refund payment {PaymentId} for order {OrderId} that does not belong to them",
                        userId, refundRequest.PaymentId, paymentStage.OrderId);
                    return ApiResponseHelper.Failure("Unauthorized. This payment does not belong to you.");
                }

                // Step 3: Verify payment status is eligible for refund
                if (paymentStage.Status != "Success")
                {
                    _logger.LogWarning("Refund attempt for non-successful payment. PaymentId: {PaymentId}, Status: {Status}, UserId: {UserId}",
                        refundRequest.PaymentId, paymentStage.Status, userId);

                    if (paymentStage.Status == "Refunded")
                    {
                        return ApiResponseHelper.Failure("This payment has already been refunded.");
                    }

                    return ApiResponseHelper.Failure($"Cannot refund payment with status: {paymentStage.Status}");
                }

                // Step 4: Validate refund amount does not exceed payment amount
                if (refundRequest.Amount > paymentStage.StageAmount)
                {
                    _logger.LogWarning("Refund amount exceeds payment amount. PaymentId: {PaymentId}, RequestedAmount: {RequestedAmount}, PaymentAmount: {PaymentAmount}",
                        refundRequest.PaymentId, refundRequest.Amount, paymentStage.StageAmount);
                    return ApiResponseHelper.Failure($"Refund amount (₹{refundRequest.Amount:N2}) cannot exceed payment amount (₹{paymentStage.StageAmount:N2})");
                }

                _logger.LogInformation("Processing refund for user {UserId}, PaymentId: {PaymentId}, Amount: ₹{Amount}, OrderId: {OrderId} (ownership verified)",
                    userId, refundRequest.PaymentId, refundRequest.Amount, paymentStage.OrderId);

                // Process refund (ownership verified)
                var refundDetails = await _razorpayService.ProcessRefundAsync(
                    refundRequest.PaymentId,
                    refundRequest.Amount,
                    refundRequest.Reason ?? "Customer request"
                );

                _logger.LogInformation("Refund processed successfully for PaymentId: {PaymentId}, OrderId: {OrderId}",
                    refundRequest.PaymentId, paymentStage.OrderId);

                // Send refund initiated notification
                try
                {
                    var userEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                    var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value ?? "Customer";
                    var userPhone = User.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;

                    await _notificationHelper.SendPaymentNotificationAsync(
                        "REFUND_INITIATED",
                        userName,
                        userEmail ?? "",
                        userPhone ?? "",
                        new Dictionary<string, object>
                        {
                            { "customer_name", userName },
                            { "order_number", orderDetails.OrderNumber ?? paymentStage.OrderId.ToString() },
                            { "order_id", paymentStage.OrderId.ToString() },
                            { "amount", refundRequest.Amount.ToString("N2") },
                            { "refund_id", refundDetails != null && refundDetails.ContainsKey("Id") ? refundDetails["Id"]?.ToString() ?? "N/A" : "N/A" },
                            { "payment_id", refundRequest.PaymentId },
                            { "refund_reason", refundRequest.Reason ?? "Customer request" },
                            { "estimated_days", "5-7 business days" },
                            { "user_id", userId.ToString() },
                            { "support_email", "support@enyvora.com" }
                        },
                        notifyAdmin: false
                    );
                    _logger.LogInformation("Refund initiated notification sent. PaymentId: {PaymentId}, UserId: {UserId}",
                        refundRequest.PaymentId, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send refund notification. PaymentId: {PaymentId}", refundRequest.PaymentId);
                }

                return ApiResponseHelper.Success(refundDetails, "Refund processed successfully!");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Refund processing validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while processing the refund. Please try again."));
            }
        }
    }

    // ===================================
    // REFUND REQUEST DTO
    // ===================================
    public class RefundRequestDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }
}
