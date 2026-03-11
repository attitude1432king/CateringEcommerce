using System;

namespace CateringEcommerce.Domain.Models.Order
{
    /// <summary>
    /// Model for order modifications (guest count, menu changes, etc.)
    /// </summary>
    public class OrderModificationModel
    {
        public long ModificationId { get; set; }
        public long OrderId { get; set; }
        public string ModificationType { get; set; } // GUEST_COUNT_INCREASE, GUEST_COUNT_DECREASE, MENU_CHANGE, etc.

        // Guest Count Changes
        public int? OriginalGuestCount { get; set; }
        public int? ModifiedGuestCount { get; set; }
        public int? GuestCountChange { get; set; }

        // Menu Changes
        public string MenuChangeDetails { get; set; } // JSON

        // Financial Impact
        public decimal? OriginalAmount { get; set; }
        public decimal AdditionalAmount { get; set; }
        public decimal PricingMultiplier { get; set; }

        // Request Details
        public string ModificationReason { get; set; }
        public long RequestedBy { get; set; }
        public string RequestedByType { get; set; } // CUSTOMER, PARTNER, ADMIN
        public DateTime RequestDate { get; set; }

        // Approval Workflow
        public bool RequiresApproval { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByType { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected, Paid, Cancelled
        public string RejectionReason { get; set; }

        // Payment
        public bool PaymentCollected { get; set; }
        public long? PaymentTransactionId { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for guest count change request
    /// </summary>
    public class GuestCountChangeRequestDto
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public int NewGuestCount { get; set; }
        public string ChangeReason { get; set; }
    }

    /// <summary>
    /// DTO for menu change request
    /// </summary>
    public class MenuChangeRequestDto
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public string MenuChanges { get; set; } // JSON: { removeItems: [], addItems: [], replaceItems: [] }
        public string ChangeReason { get; set; }
    }

    /// <summary>
    /// Response for modification request
    /// </summary>
    public class ModificationRequestResponse
    {
        public long ModificationId { get; set; }
        public string ModificationType { get; set; }
        public int? GuestCountChange { get; set; }
        public decimal AdditionalAmount { get; set; }
        public decimal PricingMultiplier { get; set; }
        public bool RequiresPartnerApproval { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string PaymentInstructions { get; set; }
    }
}
