using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    // ===================================
    // CREATE ORDER MODIFICATION DTO (Request from Owner)
    // ===================================
    public class CreateOrderModificationDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ModificationType { get; set; } = string.Empty; // GuestCountIncrease, ItemAddition, ServiceExtension, DecorationUpgrade

        public int? OriginalGuestCount { get; set; }

        public int? ModifiedGuestCount { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal AdditionalAmount { get; set; }

        [Required]
        [MaxLength(500)]
        public string ModificationReason { get; set; } = string.Empty;

        [Required]
        public long RequestedBy { get; set; } // Owner ID
    }

    // ===================================
    // ORDER MODIFICATION DTO (Response)
    // ===================================
    public class OrderModificationDto
    {
        public long ModificationId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ModificationType { get; set; } = string.Empty;
        public int? OriginalGuestCount { get; set; }
        public int? ModifiedGuestCount { get; set; }
        public decimal AdditionalAmount { get; set; }
        public string ModificationReason { get; set; } = string.Empty;
        public long RequestedBy { get; set; }
        public string RequestedByName { get; set; } = string.Empty; // Owner/Partner name
        public long? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; } // User name
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected, Paid
        public long? PaymentStageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    // ===================================
    // APPROVE ORDER MODIFICATION DTO
    // ===================================
    public class ApproveOrderModificationDto
    {
        [Required]
        public long ModificationId { get; set; }

        [Required]
        public long UserId { get; set; } // User approving the modification

        [MaxLength(500)]
        public string? ApprovalNotes { get; set; }
    }

    // ===================================
    // REJECT ORDER MODIFICATION DTO
    // ===================================
    public class RejectOrderModificationDto
    {
        [Required]
        public long ModificationId { get; set; }

        [Required]
        public long UserId { get; set; } // User rejecting the modification

        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    // ===================================
    // ORDER MODIFICATIONS SUMMARY DTO
    // ===================================
    public class OrderModificationsSummaryDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAdditionalAmount { get; set; }
        public int TotalModifications { get; set; }
        public int PendingModifications { get; set; }
        public int ApprovedModifications { get; set; }
        public int RejectedModifications { get; set; }
        public int PaidModifications { get; set; }
        public List<OrderModificationDto> Modifications { get; set; } = new List<OrderModificationDto>();
    }
}
