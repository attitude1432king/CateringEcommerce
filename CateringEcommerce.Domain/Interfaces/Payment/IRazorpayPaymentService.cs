using CateringEcommerce.Domain.Models.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Payment
{
    public interface IRazorpayPaymentService
    {
        /// <summary>
        /// Creates a Razorpay order for payment processing
        /// </summary>
        Task<RazorpayOrderResponseDto> CreateOrderAsync(RazorpayOrderRequestDto orderRequest);

        /// <summary>
        /// Verifies the payment signature to ensure payment authenticity
        /// </summary>
        bool VerifyPaymentSignature(RazorpayPaymentVerificationDto verificationData);

        /// <summary>
        /// Verifies webhook signature for callback verification
        /// </summary>
        bool VerifyWebhookSignature(string webhookBody, string receivedSignature);

        /// <summary>
        /// Retrieves payment details from Razorpay
        /// </summary>
        Task<Dictionary<string, object>> GetPaymentDetailsAsync(string paymentId);

        /// <summary>
        /// Processes a refund for a completed payment
        /// </summary>
        Task<Dictionary<string, object>> ProcessRefundAsync(string paymentId, decimal refundAmount, string reason = "Customer request");

        /// <summary>
        /// Retrieves order details from Razorpay
        /// </summary>
        Task<Dictionary<string, object>> GetOrderDetailsAsync(string razorpayOrderId);
    }
}
