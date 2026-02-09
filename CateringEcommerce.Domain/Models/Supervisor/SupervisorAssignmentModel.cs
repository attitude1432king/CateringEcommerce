using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Supervisor
{
    /// <summary>
    /// Supervisor Assignment Model - Event Assignment
    /// Links supervisor to specific event/order with tracking
    /// </summary>
    public class SupervisorAssignmentModel
    {
        public long AssignmentId { get; set; }
        public string AssignmentNumber { get; set; }
        public long SupervisorId { get; set; }
        public long OrderId { get; set; }

        // Assignment Details
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; }
        public string EventType { get; set; }
        public decimal SupervisorFee { get; set; }
        public string AssignmentNotes { get; set; }

        // Status Tracking
        public string AssignmentStatus { get; set; } // ASSIGNED, ACCEPTED, REJECTED, IN_PROGRESS, COMPLETED, CANCELLED
        public DateTime AssignedDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string RejectionReason { get; set; }

        // Check-in Details
        public bool CheckedIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string CheckInLocation { get; set; }
        public string CheckInPhoto { get; set; }

        // Payment Details
        public bool PaymentReleaseRequested { get; set; }
        public DateTime? PaymentReleaseRequestDate { get; set; }
        public bool PaymentReleaseApproved { get; set; }
        public DateTime? PaymentReleaseApprovalDate { get; set; }
        public long? PaymentApprovedBy { get; set; }

        // Quality & Performance
        public int? SupervisorRating { get; set; }
        public string SupervisorFeedback { get; set; }
        public bool IssuesReported { get; set; }
        public string IssuesSummary { get; set; }

        // Audit Fields
        public long AssignedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }

        // Navigation Properties (for display)
        public string SupervisorName { get; set; }
        public SupervisorType SupervisorType { get; set; }
        public string OrderNumber { get; set; }
        public string VendorName { get; set; }
    }

    /// <summary>
    /// Assignment Status Enum
    /// </summary>
    public enum AssignmentStatus
    {
        ASSIGNED,       // Supervisor assigned by admin
        ACCEPTED,       // Supervisor accepted assignment
        REJECTED,       // Supervisor rejected assignment
        IN_PROGRESS,    // Event in progress
        COMPLETED,      // Event completed, report submitted
        CANCELLED       // Assignment cancelled
    }
}
