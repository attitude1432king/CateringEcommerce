using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Order
{
    /// <summary>
    /// Model for customer complaints
    /// </summary>
    public class CustomerComplaintModel
    {
        public long ComplaintId { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long OwnerId { get; set; }

        // Complaint Type & Category
        public string ComplaintType { get; set; } // FOOD_COLD, FOOD_QUALITY, QUANTITY_SHORT, LATE_ARRIVAL, PARTIAL_ISSUE, etc.
        public string Severity { get; set; } // CRITICAL, MAJOR, MINOR
        public string ComplaintSummary { get; set; }
        public string ComplaintDetails { get; set; }

        // Specific Issue Details
        public string AffectedItems { get; set; } // JSON
        public int AffectedItemCount { get; set; }
        public int TotalItemCount { get; set; }
        public int GuestComplaintCount { get; set; }
        public int TotalGuestCount { get; set; }

        // Evidence
        public string PhotoEvidencePaths { get; set; } // JSON
        public string VideoEvidencePaths { get; set; } // JSON
        public string WitnessStatements { get; set; } // JSON
        public string TimestampEvidence { get; set; }

        // Timing
        public DateTime? IssueOccurredAt { get; set; }
        public DateTime ReportedAt { get; set; }
        public bool IsReportedDuringEvent { get; set; }

        // Partner Response
        public DateTime? PartnerNotifiedDate { get; set; }
        public string PartnerResponse { get; set; }
        public DateTime? PartnerResponseDate { get; set; }
        public bool? PartnerAdmittedFault { get; set; }
        public bool PartnerOfferedReplacement { get; set; }
        public bool PartnerProvidedReplacement { get; set; }

        // Resolution
        public string Status { get; set; } // Open, Under_Investigation, Resolved, Rejected, Escalated
        public string ResolutionType { get; set; } // FULL_REFUND, PARTIAL_REFUND, REPLACEMENT, GOODWILL_CREDIT, NO_RESOLUTION
        public decimal RefundPercentage { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal GoodwillCredit { get; set; }

        // Validity Assessment
        public bool? IsValidComplaint { get; set; }
        public string ValidityReason { get; set; }
        public decimal SeverityFactor { get; set; }

        // Admin Review
        public long? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string AdminNotes { get; set; }
        public string ResolutionNotes { get; set; }
        public DateTime? ResolvedDate { get; set; }

        // Fraud Detection
        public bool IsFlaggedSuspicious { get; set; }
        public int CustomerComplaintHistoryCount { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for filing a complaint
    /// </summary>
    public class FileComplaintDto
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public string ComplaintType { get; set; }
        public string ComplaintSummary { get; set; }
        public string ComplaintDetails { get; set; }
        public List<string> PhotoEvidencePaths { get; set; }
        public List<string> VideoEvidencePaths { get; set; }
        public List<string> AffectedItems { get; set; }
        public int? GuestComplaintCount { get; set; }
        public DateTime? IssueOccurredAt { get; set; }
    }

    /// <summary>
    /// Response for complaint filing
    /// </summary>
    public class FileComplaintResponse
    {
        public long ComplaintId { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string AdditionalInfo { get; set; }
        public DateTime ExpectedResolutionBy { get; set; }
    }

    /// <summary>
    /// DTO for resolving a complaint (Admin)
    /// </summary>
    public class ResolveComplaintDto
    {
        public long ComplaintId { get; set; }
        public long AdminId { get; set; }
        public string ResolutionType { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal GoodwillCredit { get; set; }
        public bool IsValidComplaint { get; set; }
        public string ValidityReason { get; set; }
        public string ResolutionNotes { get; set; }
    }

    /// <summary>
    /// Complaint refund calculation response
    /// </summary>
    public class ComplaintRefundCalculation
    {
        public long ComplaintId { get; set; }
        public string ComplaintType { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal ItemValue { get; set; }
        public decimal SeverityFactor { get; set; }
        public decimal CalculatedRefund { get; set; }
        public decimal MaxRefundAllowed { get; set; }
        public decimal RecommendedRefund { get; set; }
        public string Explanation { get; set; }
    }
}
