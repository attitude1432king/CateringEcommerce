using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Supervisor
{
    /// <summary>
    /// PRE-EVENT VERIFICATION MODELS
    /// Before Event: Verify menu, materials, guest count, evidence
    /// </summary>
    public class PreEventVerificationModel
    {
        public long ChecklistId { get; set; }
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public long OrderId { get; set; }

        // Menu Verification
        public bool MenuVerified { get; set; }
        public bool? MenuVsContractMatch { get; set; }
        public string MenuVerificationNotes { get; set; }
        public List<string> MenuVerificationPhotos { get; set; }
        public List<string> MenuItemsReceived { get; set; }
        public List<string> MenuItemsVerified { get; set; }
        public List<string> MissingItems { get; set; }
        public List<MenuSubstitution> SubstitutedItems { get; set; }

        // Raw Material & Quantity Verification
        public bool RawMaterialVerified { get; set; }
        public bool? RawMaterialQualityOK { get; set; }
        public bool? RawMaterialQuantityOK { get; set; }
        public string RawMaterialNotes { get; set; }
        public List<string> RawMaterialPhotos { get; set; }
        public int? ExpectedPortions { get; set; }
        public int? VerifiedPortions { get; set; }
        public bool? PortionVarianceAcceptable { get; set; }

        // Quality Checks
        public bool FreshnessCheckDone { get; set; }
        public bool HygieneCheckDone { get; set; }
        public bool PackagingCheckDone { get; set; }
        public bool TemperatureCheckDone { get; set; }
        public bool QualityIssuesFound { get; set; }
        public string QualityIssuesDetails { get; set; }

        // Guest Count Confirmation
        public bool GuestCountConfirmed { get; set; }
        public int? ConfirmedGuestCount { get; set; }
        public int? LockedGuestCount { get; set; }
        public bool? GuestCountMismatch { get; set; }
        public DateTime? GuestCountConfirmationDate { get; set; }

        // Evidence
        public List<string> ChecklistPhotos { get; set; }
        public DateTime Timestamp { get; set; }

        // Sign-off
        public bool SupervisorSignedOff { get; set; }
        public bool VendorSignedOff { get; set; }
        public string VendorSignatureUrl { get; set; }

        // Status
        public string VerificationStatus { get; set; }
        public bool IssuesFound { get; set; }
        public string IssuesDescription { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class MenuSubstitution
    {
        public string OriginalItem { get; set; }
        public string SubstituteItem { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// DTO for submitting pre-event verification
    /// </summary>
    public class SubmitPreEventVerificationDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }

        // Menu Verification
        public bool MenuVerified { get; set; }
        public bool MenuVsContractMatch { get; set; }
        public string MenuVerificationNotes { get; set; }
        public List<string> MenuVerificationPhotos { get; set; }

        // Raw Material Verification
        public bool RawMaterialVerified { get; set; }
        public bool RawMaterialQualityOK { get; set; }
        public bool RawMaterialQuantityOK { get; set; }
        public string RawMaterialNotes { get; set; }
        public List<string> RawMaterialPhotos { get; set; }

        // Guest Count Confirmation
        public bool GuestCountConfirmed { get; set; }
        public int ConfirmedGuestCount { get; set; }

        // Evidence (Timestamped)
        public List<TimestampedEvidence> PreEventEvidence { get; set; }

        // Issues
        public bool IssuesFound { get; set; }
        public string IssuesDescription { get; set; }
    }

    /// <summary>
    /// DURING-EVENT TRACKING MODELS
    /// During Event: Monitor serving, guest count, extra requests, client approval
    /// </summary>
    public class DuringEventTrackingModel
    {
        public long TrackingId { get; set; }
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public long OrderId { get; set; }

        // Tracking Type
        public DuringEventTrackingType TrackingType { get; set; }

        // Details
        public string TrackingDescription { get; set; }
        public string TrackingData { get; set; } // JSON

        // Guest Count Tracking
        public int? GuestCount { get; set; }
        public int? GuestCountVariance { get; set; }

        // Extra Quantity Request
        public string ExtraItemName { get; set; }
        public int? ExtraQuantity { get; set; }
        public decimal? ExtraCost { get; set; }
        public string ExtraReason { get; set; }

        // Client Approval
        public ClientApprovalMethod? ApprovalMethod { get; set; }
        public string OTPCode { get; set; }
        public DateTime? OTPSentTime { get; set; }
        public bool? OTPVerified { get; set; }
        public ClientApprovalStatus? ApprovalStatus { get; set; }
        public DateTime? ApprovalTimestamp { get; set; }

        // Evidence
        public List<string> EvidenceUrls { get; set; }
        public string GPSLocation { get; set; }

        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// DTO for food serving monitoring
    /// </summary>
    public class FoodServingMonitorDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public int QualityRating { get; set; } // 1-5
        public bool TemperatureOK { get; set; }
        public bool PresentationOK { get; set; }
        public string Notes { get; set; }
        public List<string> Photos { get; set; }
    }

    /// <summary>
    /// DTO for updating guest count (real-time)
    /// </summary>
    public class UpdateGuestCountDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public int ActualGuestCount { get; set; }
        public string Notes { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// DTO for requesting extra quantity with client approval
    /// </summary>
    public class RequestExtraQuantityDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public string ItemName { get; set; }
        public int ExtraQuantity { get; set; }
        public decimal ExtraCost { get; set; }
        public string Reason { get; set; }
        public string ClientPhone { get; set; }
        public ClientApprovalMethod ApprovalMethod { get; set; } // IN_APP, OTP, SIGNATURE
    }

    /// <summary>
    /// DTO for verifying client OTP
    /// </summary>
    public class VerifyClientOTPDto
    {
        public long AssignmentId { get; set; }
        public string OTPCode { get; set; }
        public string ClientIPAddress { get; set; }
    }

    /// <summary>
    /// Client OTP Verification Model
    /// </summary>
    public class ClientOTPVerificationModel
    {
        public long OTPId { get; set; }
        public long AssignmentId { get; set; }
        public long OrderId { get; set; }
        public long SupervisorId { get; set; }

        public string OTPCode { get; set; }
        public string OTPPurpose { get; set; } // EXTRA_QUANTITY_APPROVAL, PAYMENT_RELEASE
        public string OTPSentTo { get; set; }
        public DateTime OTPSentTime { get; set; }
        public DateTime OTPExpiresAt { get; set; }

        public bool OTPVerified { get; set; }
        public DateTime? OTPVerifiedTime { get; set; }
        public int VerificationAttempts { get; set; }
        public int MaxAttempts { get; set; }

        public string ContextData { get; set; } // JSON
        public string ClientIPAddress { get; set; }
        public string Status { get; set; } // SENT, VERIFIED, EXPIRED, FAILED
    }

    /// <summary>
    /// POST-EVENT REPORT MODELS
    /// After Event: Collect feedback, record issues, final payment request, completion report
    /// </summary>
    public class PostEventReportModel
    {
        public long ReportId { get; set; }
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public long OrderId { get; set; }

        // Event Summary
        public DateTime? EventStartedTime { get; set; }
        public DateTime? EventEndedTime { get; set; }
        public int? EventDurationMinutes { get; set; }
        public int? FinalGuestCount { get; set; }
        public int? EventRating { get; set; } // Supervisor's overall rating 1-5

        // Structured Client Feedback
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientEmail { get; set; }
        public int? ClientSatisfactionRating { get; set; } // 1-5
        public int? FoodQualityRating { get; set; }
        public int? FoodQuantityRating { get; set; }
        public int? ServiceQualityRating { get; set; }
        public int? PresentationRating { get; set; }
        public int? ValueForMoneyRating { get; set; }
        public bool? WouldRecommend { get; set; }
        public string ClientComments { get; set; }
        public string ClientSignatureUrl { get; set; }

        // Partner Performance
        public int? VendorPunctualityRating { get; set; }
        public int? VendorPreparationRating { get; set; }
        public int? VendorCooperationRating { get; set; }
        public int? VendorHygieneRating { get; set; }
        public string VendorComments { get; set; }

        // Issues Summary
        public int TotalIssuesCount { get; set; }
        public int CriticalIssuesCount { get; set; }
        public int MajorIssuesCount { get; set; }
        public int MinorIssuesCount { get; set; }
        public List<EventIssue> Issues { get; set; }
        public bool? AllIssuesResolved { get; set; }

        // Financial Summary
        public decimal? BaseOrderAmount { get; set; }
        public decimal ExtraChargesAmount { get; set; }
        public decimal DeductionsAmount { get; set; }
        public decimal? FinalPayableAmount { get; set; }
        public PaymentBreakdown PaymentBreakdown { get; set; }

        // Completion Evidence
        public List<string> CompletionPhotos { get; set; }
        public List<string> CompletionVideos { get; set; }
        public List<string> WastePhotos { get; set; }
        public string LeftoverNotes { get; set; }

        // Report Details
        public string ReportSummary { get; set; }
        public string Recommendations { get; set; }
        public string SupervisorNotes { get; set; }
        public string ReportPdfUrl { get; set; }

        // Verification
        public bool ReportVerified { get; set; }
        public long? VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerificationNotes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }

    public class EventIssue
    {
        public string IssueType { get; set; }
        public string Severity { get; set; } // CRITICAL, MAJOR, MINOR
        public string Description { get; set; }
        public string Resolution { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> EvidenceUrls { get; set; }
    }

    public class PaymentBreakdown
    {
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceCharges { get; set; }
        public decimal ExtraCharges { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// DTO for submitting post-event report
    /// </summary>
    public class SubmitPostEventReportDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }

        // Event Summary
        public int FinalGuestCount { get; set; }
        public int EventRating { get; set; }

        // Structured Client Feedback
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public int ClientSatisfactionRating { get; set; }
        public int FoodQualityRating { get; set; }
        public int FoodQuantityRating { get; set; }
        public int ServiceQualityRating { get; set; }
        public int PresentationRating { get; set; }
        public bool WouldRecommend { get; set; }
        public string ClientComments { get; set; }
        public string ClientSignatureUrl { get; set; }

        // Partner Performance
        public int VendorPunctualityRating { get; set; }
        public int VendorPreparationRating { get; set; }
        public int VendorCooperationRating { get; set; }
        public string VendorComments { get; set; }

        // Issues
        public int IssuesCount { get; set; }
        public List<EventIssue> Issues { get; set; }

        // Financial
        public decimal FinalPayableAmount { get; set; }
        public PaymentBreakdown PaymentBreakdown { get; set; }

        // Report
        public string ReportSummary { get; set; }
        public string Recommendations { get; set; }
        public List<string> CompletionPhotos { get; set; }
        public List<string> CompletionVideos { get; set; }
    }

    /// <summary>
    /// Event Supervision Complete Summary
    /// </summary>
    public class EventSupervisionSummaryDto
    {
        public long AssignmentId { get; set; }
        public string AssignmentNumber { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }

        // Pre-Event
        public PreEventSummary PreEvent { get; set; }

        // During-Event
        public DuringEventSummary DuringEvent { get; set; }

        // Post-Event
        public PostEventSummary PostEvent { get; set; }

        // Supervisor Details
        public string SupervisorName { get; set; }
        public SupervisorType SupervisorType { get; set; }
    }

    public class PreEventSummary
    {
        public string VerificationStatus { get; set; }
        public bool MenuVerified { get; set; }
        public bool MaterialVerified { get; set; }
        public bool GuestCountConfirmed { get; set; }
        public int? ConfirmedGuestCount { get; set; }
        public bool IssuesFound { get; set; }
        public string IssuesDescription { get; set; }
    }

    public class DuringEventSummary
    {
        public int? ActualGuestCount { get; set; }
        public int? GuestCountVariance { get; set; }
        public bool ExtraQuantityRequested { get; set; }
        public string ClientApprovalStatus { get; set; }
        public List<DuringEventTrackingModel> TrackingLog { get; set; }
    }

    public class PostEventSummary
    {
        public bool ReportSubmitted { get; set; }
        public int? ClientSatisfaction { get; set; }
        public int? IssuesCount { get; set; }
        public decimal? FinalPaymentAmount { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }

    /// <summary>
    /// Common: Timestamped Evidence Model
    /// </summary>
    public class TimestampedEvidence
    {
        public string Type { get; set; } // PHOTO, VIDEO
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public string GPSLocation { get; set; }
        public string Description { get; set; }
    }

    #region Enums

    public enum DuringEventTrackingType
    {
        GUEST_COUNT_UPDATE,
        FOOD_SERVING_CHECK,
        EXTRA_QUANTITY_REQUEST,
        CLIENT_APPROVAL,
        ISSUE_REPORTED,
        QUALITY_CHECK
    }

    public enum ClientApprovalMethod
    {
        IN_APP,
        OTP,
        SIGNATURE
    }

    public enum ClientApprovalStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    #endregion
}
