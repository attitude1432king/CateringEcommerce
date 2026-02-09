using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for initiating sample order payment
    /// </summary>
    public class InitiateSamplePaymentRequest
    {
        [Required(ErrorMessage = "Sample order ID is required")]
        public long SampleOrderID { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // "RAZORPAY", "PAYTM", "WALLET", etc.

        [MaxLength(100)]
        public string? CallbackUrl { get; set; }

        [MaxLength(100)]
        public string? CancelUrl { get; set; }
    }

    /// <summary>
    /// Response DTO for payment initiation
    /// </summary>
    public class SamplePaymentInitiationResponse
    {
        public long SampleOrderID { get; set; }
        public long PaymentID { get; set; }
        public string PaymentGatewayOrderID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentGatewayUrl { get; set; } = string.Empty;
        public string? PaymentKey { get; set; }
        public string? Signature { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    /// <summary>
    /// Request DTO for payment verification
    /// </summary>
    public class VerifySamplePaymentRequest
    {
        [Required(ErrorMessage = "Sample order ID is required")]
        public long SampleOrderID { get; set; }

        [Required(ErrorMessage = "Payment gateway order ID is required")]
        [MaxLength(100)]
        public string PaymentGatewayOrderID { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PaymentGatewayTransactionID { get; set; }

        [MaxLength(500)]
        public string? PaymentSignature { get; set; }
    }

    /// <summary>
    /// Response DTO for payment verification
    /// </summary>
    public class SamplePaymentVerificationResponse
    {
        public bool IsSuccess { get; set; }
        public long SampleOrderID { get; set; }
        public long PaymentID { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? TransactionID { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Message { get; set; }
    }
}
