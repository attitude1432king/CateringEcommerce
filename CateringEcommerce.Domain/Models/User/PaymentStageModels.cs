using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.User
{
    // ===================================
    // PAYMENT STAGE DTO (Response)
    // ===================================
    public class PaymentStageDto
    {
        public long PaymentStageId { get; set; }
        public long OrderId { get; set; }
        public string StageType { get; set; } = string.Empty; // PreBooking, PostEvent
        public decimal StagePercentage { get; set; } // 40.00, 60.00
        public decimal StageAmount { get; set; }
        public string? PaymentMethod { get; set; } // Online, COD, UPI, Card, Wallet
        public string? PaymentGateway { get; set; } // Razorpay, PhonePe
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string? UpiId { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Success, Failed, Refunded
        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int ReminderSentCount { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ===================================
    // CREATE PAYMENT STAGES REQUEST DTO
    // ===================================
    public class CreatePaymentStagesDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public bool EnableSplitPayment { get; set; }

        // If split payment enabled, calculate 40%/60%
        // If not, create single "Full" payment stage
    }

    // ===================================
    // PROCESS PAYMENT STAGE DTO
    // ===================================
    public class ProcessPaymentStageDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StageType { get; set; } = string.Empty; // PreBooking, PostEvent, Full

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Online, COD, UPI, Card, Wallet

        [MaxLength(50)]
        public string? PaymentGateway { get; set; } // Razorpay, PhonePe

        [MaxLength(100)]
        public string? RazorpayOrderId { get; set; }

        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [MaxLength(100)]
        public string? RazorpaySignature { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(100)]
        public string? UpiId { get; set; }
    }

    // ===================================
    // RAZORPAY ORDER REQUEST DTO
    // ===================================
    public class RazorpayOrderRequestDto
    {
        [Required]
        public decimal Amount { get; set; } // Amount in rupees (will be converted to paise)

        [Required]
        [MaxLength(40)]
        public string Receipt { get; set; } = string.Empty; // Order number or unique identifier

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StageType { get; set; } = string.Empty; // PreBooking, PostEvent, Full

        public string? Notes { get; set; } // Additional notes
    }

    // ===================================
    // RAZORPAY ORDER RESPONSE DTO
    // ===================================
    public class RazorpayOrderResponseDto
    {
        public string Id { get; set; } = string.Empty; // Razorpay order ID
        public string Entity { get; set; } = string.Empty; // "order"
        public long Amount { get; set; } // Amount in paise
        public long AmountPaid { get; set; }
        public long AmountDue { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // created, attempted, paid
        public int Attempts { get; set; }
        public long CreatedAt { get; set; } // Unix timestamp
    }

    // ===================================
    // RAZORPAY PAYMENT VERIFICATION DTO
    // ===================================
    public class RazorpayPaymentVerificationDto
    {
        [Required]
        [MaxLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RazorpayPaymentId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RazorpaySignature { get; set; } = string.Empty;

        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StageType { get; set; } = string.Empty; // PreBooking, PostEvent, Full
    }

    // ===================================
    // PENDING PAYMENT STAGES RESPONSE DTO
    // ===================================
    public class PendingPaymentStagesDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public List<PaymentStageDto> PendingStages { get; set; } = new List<PaymentStageDto>();
        public List<PaymentStageDto> CompletedStages { get; set; } = new List<PaymentStageDto>();
    }

    // ===================================
    // RAZORPAY WEBHOOK PAYLOAD DTO
    // ===================================
    public class RazorpayWebhookDto
    {
        public string Event { get; set; } = string.Empty; // payment.authorized, payment.captured, payment.failed
        public RazorpayWebhookPayloadDto Payload { get; set; } = new RazorpayWebhookPayloadDto();
    }

    public class RazorpayWebhookPayloadDto
    {
        public RazorpayWebhookPaymentDto Payment { get; set; } = new RazorpayWebhookPaymentDto();
    }

    public class RazorpayWebhookPaymentDto
    {
        public string Id { get; set; } = string.Empty; // Payment ID
        public string Entity { get; set; } = string.Empty; // "payment"
        public long Amount { get; set; } // Amount in paise
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = string.Empty; // captured, authorized, failed
        public string? OrderId { get; set; } // Razorpay Order ID
        public string? Method { get; set; } // card, netbanking, wallet, upi
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public Dictionary<string, string>? Notes { get; set; } // Custom notes (order_id, user_id, stage_type)
        public long CreatedAt { get; set; } // Unix timestamp
    }
}
