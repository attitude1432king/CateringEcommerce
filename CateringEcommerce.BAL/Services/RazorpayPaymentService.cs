using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.Configuration;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Razorpay.Api;

namespace CateringEcommerce.BAL.Services
{
    public class RazorpayPaymentService : IRazorpayPaymentService
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly string _webhookSecret;
        private readonly RazorpayClient _client;
        private readonly ILogger<RazorpayPaymentService>? _logger;

        public RazorpayPaymentService(IOptions<RazorpaySettings> options, ILogger<RazorpayPaymentService>? logger = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var settings = options.Value;

            _keyId = settings.KeyId;
            if (string.IsNullOrEmpty(_keyId))
                throw new InvalidOperationException("Razorpay KeyId is not configured in secure configuration");

            _keySecret = settings.KeySecret;
            if (string.IsNullOrEmpty(_keySecret))
                throw new InvalidOperationException("Razorpay KeySecret is not configured in secure configuration");

            _webhookSecret = settings.WebhookSecret;

            _logger = logger;

            // Initialize Razorpay client
            _client = new RazorpayClient(_keyId, _keySecret);
        }

        // ===================================
        // CREATE RAZORPAY ORDER
        // ===================================
        public async Task<RazorpayOrderResponseDto> CreateOrderAsync(RazorpayOrderRequestDto orderRequest)
        {
            try
            {
                if (orderRequest == null)
                {
                    throw new ArgumentNullException(nameof(orderRequest));
                }

                if (orderRequest.Amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than zero.", nameof(orderRequest.Amount));
                }

                _logger?.LogInformation($"Creating Razorpay order for OrderId: {orderRequest.OrderId}, Amount: ₹{orderRequest.Amount}, Stage: {orderRequest.StageType}");

                // SECURITY FIX: Use precise rounding before conversion to prevent paise discrepancies
                // Convert amount to paise (Razorpay expects amount in paise)
                long amountInPaise = (long)Math.Round(orderRequest.Amount * 100, MidpointRounding.AwayFromZero);

                // Prepare order options
                Dictionary<string, object> options = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "currency", "INR" },
                    { "receipt", orderRequest.Receipt },
                    { "notes", new Dictionary<string, string>
                        {
                            { "order_id", orderRequest.OrderId.ToString() },
                            { "user_id", orderRequest.UserId.ToString() },
                            { "stage_type", orderRequest.StageType }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(orderRequest.Notes))
                {
                    options["notes"] = orderRequest.Notes;
                }

                // Create order using Razorpay API
                Order razorpayOrder = await Task.Run(() => _client.Order.Create(options));

                _logger?.LogInformation($"Razorpay order created successfully: {razorpayOrder["id"]}");

                // Map to DTO
                return new RazorpayOrderResponseDto
                {
                    Id = razorpayOrder["id"].ToString() ?? string.Empty,
                    Entity = razorpayOrder["entity"].ToString() ?? "order",
                    Amount = Convert.ToInt64(razorpayOrder["amount"]),
                    AmountPaid = Convert.ToInt64(razorpayOrder["amount_paid"]),
                    AmountDue = Convert.ToInt64(razorpayOrder["amount_due"]),
                    Currency = razorpayOrder["currency"].ToString() ?? "INR",
                    Receipt = razorpayOrder["receipt"].ToString() ?? string.Empty,
                    Status = razorpayOrder["status"].ToString() ?? "created",
                    Attempts = Convert.ToInt32(razorpayOrder["attempts"]),
                    CreatedAt = Convert.ToInt64(razorpayOrder["created_at"])
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating Razorpay order");
                throw new Exception("Failed to create payment order. Please try again.", ex);
            }
        }

        // ===================================
        // VERIFY PAYMENT SIGNATURE
        // ===================================
        public bool VerifyPaymentSignature(RazorpayPaymentVerificationDto verificationData)
        {
            try
            {
                if (verificationData == null)
                {
                    throw new ArgumentNullException(nameof(verificationData));
                }

                if (string.IsNullOrEmpty(verificationData.RazorpayOrderId))
                {
                    throw new ArgumentException("RazorpayOrderId is required.", nameof(verificationData.RazorpayOrderId));
                }

                if (string.IsNullOrEmpty(verificationData.RazorpayPaymentId))
                {
                    throw new ArgumentException("RazorpayPaymentId is required.", nameof(verificationData.RazorpayPaymentId));
                }

                if (string.IsNullOrEmpty(verificationData.RazorpaySignature))
                {
                    throw new ArgumentException("RazorpaySignature is required.", nameof(verificationData.RazorpaySignature));
                }

                _logger?.LogInformation($"Verifying Razorpay payment signature for OrderId: {verificationData.OrderId}");

                // Generate expected signature using HMAC SHA256
                string payload = $"{verificationData.RazorpayOrderId}|{verificationData.RazorpayPaymentId}";
                string generatedSignature = GenerateSignature(payload, _keySecret);

                // Compare signatures
                bool isValid = generatedSignature.Equals(verificationData.RazorpaySignature, StringComparison.OrdinalIgnoreCase);

                if (isValid)
                {
                    _logger?.LogInformation($"Payment signature verified successfully for OrderId: {verificationData.OrderId}");
                }
                else
                {
                    _logger?.LogWarning($"Payment signature verification failed for OrderId: {verificationData.OrderId}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying payment signature");
                return false;
            }
        }

        // ===================================
        // VERIFY WEBHOOK SIGNATURE
        // ===================================
        public bool VerifyWebhookSignature(string webhookBody, string receivedSignature)
        {
            try
            {
                if (string.IsNullOrEmpty(webhookBody))
                {
                    throw new ArgumentException("Webhook body is required.", nameof(webhookBody));
                }

                if (string.IsNullOrEmpty(receivedSignature))
                {
                    throw new ArgumentException("Signature is required.", nameof(receivedSignature));
                }

                // SECURITY FIX: Make webhook secret mandatory - prevent payment bypass attacks
                if (string.IsNullOrEmpty(_webhookSecret))
                {
                    _logger?.LogError("CRITICAL: Webhook secret is not configured. All webhook requests will be rejected.");
                    throw new InvalidOperationException(
                        "Razorpay webhook secret (PAYMENT:RAZORPAY_WEBHOOK_SECRET) is not configured in secure configuration. " +
                        "Configure the PAYMENT__RAZORPAY_WEBHOOK_SECRET environment variable to enable webhook verification.");
                }

                // Generate expected signature
                string generatedSignature = GenerateSignature(webhookBody, _webhookSecret);

                // Compare signatures
                return generatedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying webhook signature");
                return false;
            }
        }

        // ===================================
        // GET PAYMENT DETAILS
        // ===================================
        public async Task<Dictionary<string, object>> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                {
                    throw new ArgumentException("Payment ID is required.", nameof(paymentId));
                }

                _logger?.LogInformation($"Fetching payment details for PaymentId: {paymentId}");

                // Fetch payment details from Razorpay
                Payment payment = await Task.Run(() => _client.Payment.Fetch(paymentId));

                _logger?.LogInformation($"Payment details fetched successfully for PaymentId: {paymentId}");

                // Return payment details as dictionary
                return payment.Attributes;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching payment details for PaymentId: {paymentId}");
                throw new Exception("Failed to fetch payment details. Please try again.", ex);
            }
        }

        // ===================================
        // PROCESS REFUND
        // ===================================
        public async Task<Dictionary<string, object>> ProcessRefundAsync(string paymentId, decimal refundAmount, string reason = "Customer request")
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                {
                    throw new ArgumentException("Payment ID is required.", nameof(paymentId));
                }

                if (refundAmount <= 0)
                {
                    throw new ArgumentException("Refund amount must be greater than zero.", nameof(refundAmount));
                }

                _logger?.LogInformation($"Processing refund for PaymentId: {paymentId}, Amount: ₹{refundAmount}");

                // SECURITY FIX: Use precise rounding to prevent amount discrepancies
                // Convert amount to paise
                long amountInPaise = (long)Math.Round(refundAmount * 100, MidpointRounding.AwayFromZero);

                // Prepare refund options
                Dictionary<string, object> refundOptions = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "notes", new Dictionary<string, string>
                        {
                            { "reason", reason }
                        }
                    }
                };

                // Create refund
                Refund refund = await Task.Run(() => _client.Payment.Fetch(paymentId).Refund(refundOptions));

                _logger?.LogInformation($"Refund processed successfully: {refund["id"]}");

                return refund.Attributes;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing refund for PaymentId: {paymentId}");
                throw new Exception("Failed to process refund. Please try again.", ex);
            }
        }

        // ===================================
        // GET ORDER DETAILS
        // ===================================
        public async Task<Dictionary<string, object>> GetOrderDetailsAsync(string razorpayOrderId)
        {
            try
            {
                if (string.IsNullOrEmpty(razorpayOrderId))
                {
                    throw new ArgumentException("Razorpay Order ID is required.", nameof(razorpayOrderId));
                }

                _logger?.LogInformation($"Fetching order details for RazorpayOrderId: {razorpayOrderId}");

                // Fetch order details from Razorpay
                Order order = await Task.Run(() => _client.Order.Fetch(razorpayOrderId));

                _logger?.LogInformation($"Order details fetched successfully for RazorpayOrderId: {razorpayOrderId}");

                return order.Attributes;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching order details for RazorpayOrderId: {razorpayOrderId}");
                throw new Exception("Failed to fetch order details. Please try again.", ex);
            }
        }

        // ===================================
        // HELPER: GENERATE HMAC SHA256 SIGNATURE
        // ===================================
        private string GenerateSignature(string payload, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // ===================================
        // CONVERT PAISE TO RUPEES
        // ===================================
        public static decimal ConvertPaiseToRupees(long amountInPaise)
        {
            return amountInPaise / 100m;
        }

        // ===================================
        // CONVERT RUPEES TO PAISE
        // ===================================
        public static long ConvertRupeesToPaise(decimal amountInRupees)
        {
            return (long)(amountInRupees * 100);
        }
    }
}
