using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public PaymentGatewayController(
            ILogger<PaymentGatewayController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
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

                // Create Razorpay service
                ILogger<RazorpayPaymentService> razorpayLogger = _logger as ILogger<RazorpayPaymentService> ?? 
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorpayPaymentService>.Instance;
                RazorpayPaymentService razorpayService = new RazorpayPaymentService(_configuration, razorpayLogger);

                // Create Razorpay order
                RazorpayOrderResponseDto razorpayOrder = await razorpayService.CreateOrderAsync(orderRequest);

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

                // Create Razorpay service
                ILogger<RazorpayPaymentService> razorpayLogger = _logger as ILogger<RazorpayPaymentService> ?? 
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorpayPaymentService>.Instance;
                RazorpayPaymentService razorpayService = new RazorpayPaymentService(_configuration, razorpayLogger);

                // Verify payment signature
                bool isValid = razorpayService.VerifyPaymentSignature(verificationData);

                if (!isValid)
                {
                    _logger.LogWarning($"Payment verification failed for OrderId: {verificationData.OrderId}");
                    return ApiResponseHelper.Failure("Payment verification failed. Invalid signature.", "error");
                }

                _logger.LogInformation($"Payment verified successfully for OrderId: {verificationData.OrderId}");

                // TODO: Update payment stage status to "Success" in database
                // This will be handled by PaymentStageService after we create it

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

                // Create Razorpay service
                ILogger<RazorpayPaymentService> razorpayLogger = _logger as ILogger<RazorpayPaymentService> ?? 
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorpayPaymentService>.Instance;
                RazorpayPaymentService razorpayService = new RazorpayPaymentService(_configuration, razorpayLogger);

                // Get payment details
                var paymentDetails = await razorpayService.GetPaymentDetailsAsync(paymentId);

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

                // Create Razorpay service
                ILogger<RazorpayPaymentService> razorpayLogger = _logger as ILogger<RazorpayPaymentService> ?? 
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorpayPaymentService>.Instance;
                RazorpayPaymentService razorpayService = new RazorpayPaymentService(_configuration, razorpayLogger);

                // Get order details
                var orderDetails = await razorpayService.GetOrderDetailsAsync(razorpayOrderId);

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

                // Create Razorpay service
                ILogger<RazorpayPaymentService> razorpayLogger = _logger as ILogger<RazorpayPaymentService> ?? 
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorpayPaymentService>.Instance;
                RazorpayPaymentService razorpayService = new RazorpayPaymentService(_configuration, razorpayLogger);

                // Process refund
                var refundDetails = await razorpayService.ProcessRefundAsync(
                    refundRequest.PaymentId,
                    refundRequest.Amount,
                    refundRequest.Reason ?? "Customer request"
                );

                _logger.LogInformation($"Refund processed successfully for PaymentId: {refundRequest.PaymentId}");

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
