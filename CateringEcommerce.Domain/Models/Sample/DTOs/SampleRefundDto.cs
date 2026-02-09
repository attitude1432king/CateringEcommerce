using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for initiating sample order refund
    /// </summary>
    public class InitiateRefundRequest
    {
        [Required(ErrorMessage = "Refund reason is required")]
        [MaxLength(100)]
        public string RefundReason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response DTO for refund details
    /// </summary>
    public class SampleRefundResponse
    {
        public long RefundID { get; set; }
        public long SampleOrderID { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string RefundReasonDisplay { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
        public string RefundStatusDisplay { get; set; } = string.Empty;
        public string? PaymentGatewayRefundID { get; set; }
        public string? RefundMethod { get; set; }
        public DateTime RefundInitiatedDate { get; set; }
        public DateTime? RefundCompletedDate { get; set; }
        public DateTime? ExpectedRefundDate { get; set; }
        public string? Notes { get; set; }
        public bool IsAutoRefund { get; set; }
        public int ExpectedRefundDays { get; set; }
    }

    /// <summary>
    /// DTO for refund list item
    /// </summary>
    public class RefundListItemDto
    {
        public long RefundID { get; set; }
        public long SampleOrderID { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
        public DateTime RefundInitiatedDate { get; set; }
        public DateTime? RefundCompletedDate { get; set; }
    }
}
