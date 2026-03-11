using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.Domain.Interfaces.Supervisor
{
    /// <summary>
    /// Event Supervision Repository - Handles Pre/During/Post Event Workflows
    /// Covers all supervisor responsibilities: verification, monitoring, reporting
    /// </summary>
    public interface IEventSupervisionRepository
    {
        // =============================================
        // PRE-EVENT VERIFICATION
        // =============================================

        /// <summary>
        /// Submit complete pre-event verification checklist
        /// Covers: Menu vs contract, raw materials, guest count, evidence
        /// </summary>
        Task<bool> SubmitPreEventVerificationAsync(SubmitPreEventVerificationDto request);

        /// <summary>
        /// Get pre-event verification details for an assignment
        /// </summary>
        Task<PreEventVerificationModel> GetPreEventVerificationAsync(long assignmentId);

        /// <summary>
        /// Update specific pre-event checklist items
        /// </summary>
        Task<bool> UpdatePreEventChecklistAsync(long checklistId, PreEventVerificationModel updates);

        // =============================================
        // DURING-EVENT MONITORING
        // =============================================

        /// <summary>
        /// Monitor food serving quality during event
        /// </summary>
        Task<bool> RecordFoodServingMonitorAsync(FoodServingMonitorDto request);

        /// <summary>
        /// Update guest count in real-time during event
        /// Tracks variance from confirmed count
        /// </summary>
        Task<bool> UpdateGuestCountAsync(UpdateGuestCountDto request);

        /// <summary>
        /// Request extra quantity with client approval required
        /// Generates OTP if approval method is OTP
        /// </summary>
        Task<RequestExtraQuantityResponse> RequestExtraQuantityAsync(RequestExtraQuantityDto request);

        /// <summary>
        /// Verify client OTP for extra quantity approval
        /// Validates OTP expiry and attempt limits
        /// </summary>
        Task<OTPVerificationResponse> VerifyClientOTPAsync(VerifyClientOTPDto request);

        /// <summary>
        /// Get all during-event tracking logs for an assignment
        /// </summary>
        Task<List<DuringEventTrackingModel>> GetDuringEventTrackingAsync(long assignmentId);

        // =============================================
        // POST-EVENT COMPLETION
        // =============================================

        /// <summary>
        /// Submit comprehensive post-event completion report
        /// Covers: Feedback, issues, final payment, completion evidence
        /// </summary>
        Task<bool> SubmitPostEventReportAsync(SubmitPostEventReportDto request);

        /// <summary>
        /// Get post-event report details
        /// </summary>
        Task<PostEventReportModel> GetPostEventReportAsync(long assignmentId);

        /// <summary>
        /// Update post-event report (before admin verification)
        /// </summary>
        Task<bool> UpdatePostEventReportAsync(long reportId, PostEventReportModel updates);

        /// <summary>
        /// Admin verification of post-event report
        /// </summary>
        Task<bool> VerifyPostEventReportAsync(long reportId, long verifiedBy, string verificationNotes);

        // =============================================
        // COMPLETE EVENT SUPERVISION SUMMARY
        // =============================================

        /// <summary>
        /// Get complete event supervision summary (Pre + During + Post)
        /// </summary>
        Task<EventSupervisionSummaryDto> GetEventSupervisionSummaryAsync(long assignmentId);

        // =============================================
        // OTP MANAGEMENT
        // =============================================

        /// <summary>
        /// Resend OTP to client (if expired or lost)
        /// </summary>
        Task<string> ResendClientOTPAsync(long assignmentId, string purpose);

        /// <summary>
        /// Get OTP verification status
        /// </summary>
        Task<ClientOTPVerificationModel> GetOTPVerificationStatusAsync(string otpCode);

        // =============================================
        // EVIDENCE & DOCUMENTATION
        // =============================================

        /// <summary>
        /// Upload timestamped evidence (photos/videos) for any phase
        /// </summary>
        Task<bool> UploadTimestampedEvidenceAsync(long assignmentId, List<TimestampedEvidence> evidence, string phase);

        /// <summary>
        /// Get all evidence for an assignment
        /// </summary>
        Task<Dictionary<string, List<TimestampedEvidence>>> GetAssignmentEvidenceAsync(long assignmentId);
    }

    #region Response Models

    public class RequestExtraQuantityResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long TrackingId { get; set; }
        public string OTPCode { get; set; } // Only if approval method is OTP
        public DateTime? OTPExpiresAt { get; set; }
        public bool RequiresApproval { get; set; }
        public ClientApprovalMethod ApprovalMethod { get; set; }
    }

    public class OTPVerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool OTPVerified { get; set; }
        public ClientApprovalStatus ApprovalStatus { get; set; }
        public int RemainingAttempts { get; set; }
        public bool IsExpired { get; set; }
    }

    #endregion
}
