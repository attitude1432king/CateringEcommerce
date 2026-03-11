using System;

namespace CateringEcommerce.Domain.Models.Order
{
    /// <summary>
    /// Model for order cancellation requests
    /// </summary>
    public class CancellationRequestModel
    {
        public long CancellationId { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long OwnerId { get; set; }

        // Timing Details
        public DateTime EventDate { get; set; }
        public DateTime CancellationRequestDate { get; set; }
        public int HoursBeforeEvent { get; set; }
        public int DaysBeforeEvent { get; set; }

        // Policy Applied
        public string PolicyTier { get; set; } // FULL_REFUND, PARTIAL_REFUND, NO_REFUND, FORCE_MAJEURE
        public decimal RefundPercentage { get; set; }

        // Financial Breakdown
        public decimal OrderTotalAmount { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RetentionAmount { get; set; }
        public decimal PartnerCompensation { get; set; }
        public decimal PlatformCommissionForfeited { get; set; }

        // Reason & Evidence
        public string CancellationReason { get; set; }
        public bool IsForceMajeure { get; set; }
        public string ForceMajeureEvidence { get; set; } // JSON

        // Status & Approval
        public string Status { get; set; } // Pending, Approved, Rejected, Refunded
        public long? AdminApprovedBy { get; set; }
        public DateTime? AdminApprovalDate { get; set; }
        public string AdminNotes { get; set; }
        public string PartnerResponse { get; set; }
        public DateTime? PartnerResponseDate { get; set; }

        // Refund Processing
        public DateTime? RefundInitiatedDate { get; set; }
        public DateTime? RefundCompletedDate { get; set; }
        public string RefundTransactionId { get; set; }
        public string RefundMethod { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a cancellation request
    /// </summary>
    public class CreateCancellationRequestDto
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public string CancellationReason { get; set; }
        public bool IsForceMajeure { get; set; }
        public string ForceMajeureEvidence { get; set; } // JSON: { documents: [], description: "" }
    }

    /// <summary>
    /// Response for cancellation policy calculation
    /// </summary>
    public class CancellationPolicyResponse
    {
        public long OrderId { get; set; }
        public DateTime EventDate { get; set; }
        public int DaysBeforeEvent { get; set; }
        public int HoursBeforeEvent { get; set; }
        public string PolicyTier { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal PartnerCompensation { get; set; }
        public string PolicyDescription { get; set; }
        public string Warning { get; set; }
    }
}
