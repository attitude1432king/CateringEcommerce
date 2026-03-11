using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// Represents refund information for sample orders
    /// </summary>
    public class SampleRefundModel
    {
        public long RefundID { get; set; }

        [Required]
        public long SampleOrderID { get; set; }

        // Refund Details
        [Required]
        [Range(0, 999999.99)]
        public decimal RefundAmount { get; set; }

        [Required]
        [MaxLength(100)]
        public string RefundReason { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RefundStatus { get; set; } = "PENDING";

        // Payment Gateway Integration
        [MaxLength(100)]
        public string? PaymentGatewayRefundID { get; set; }

        [MaxLength(50)]
        public string? RefundMethod { get; set; }

        // Timing
        public DateTime RefundInitiatedDate { get; set; } = DateTime.Now;

        public DateTime? RefundCompletedDate { get; set; }

        public DateTime? ExpectedRefundDate { get; set; }

        // Additional Info
        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? ProcessedBy { get; set; }

        public bool IsAutoRefund { get; set; } = false;
    }
}
