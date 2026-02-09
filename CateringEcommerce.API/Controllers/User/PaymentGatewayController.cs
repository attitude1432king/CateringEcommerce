using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class PaymentGatewayController : ControllerBase
    {
        private readonly ILogger<PaymentGatewayController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly INotificationHelper _notificationHelper;

        public PaymentGatewayController(
            ILogger<PaymentGatewayController> logger,
            ICurrentUserService currentUser,
            IRazorpayPaymentService razorpayService,
            INotificationHelper notificationHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _razorpayService = razorpayService ?? throw new ArgumentNullException(nameof(razorpayService));
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
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

                _logger.LogInformation($"Creating Razorpay order for user {userId}, OrderId: {orderRequest.OrderId}, Amount: ₹{orderRequest.Amount}, Stage: {orderRequest.StageType}");

                // Create Razorpay order
                RazorpayOrderResponseDto razorpayOrder = await _razorpayService.CreateOrderAsync(orderRequest);

                _logger.LogInformation($"Razorpay order created successfully: {razorpayOrder.Id}");

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

                // TODO: Update payment stage status to "Success" in database
                // This will be handled by PaymentStageService after we create it

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

                _logger.LogInformation($"Processing refund for user {userId}, PaymentId: {refundRequest.PaymentId}, Amount: ₹{refundRequest.Amount}");

                // Process refund
                var refundDetails = await _razorpayService.ProcessRefundAsync(
                    refundRequest.PaymentId,
                    refundRequest.Amount,
                    refundRequest.Reason ?? "Customer request"
                );

                _logger.LogInformation($"Refund processed successfully for PaymentId: {refundRequest.PaymentId}");

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
                            { "order_number", "N/A" }, // Order number not available in this context
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
