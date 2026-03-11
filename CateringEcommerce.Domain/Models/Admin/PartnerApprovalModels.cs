namespace CateringEcommerce.Domain.Models.Admin
{
    #region Request/Filter Models

    /// <summary>
    /// Filter and pagination parameters for partner request listing
    /// </summary>
    public class PartnerRequestFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }

        // Use INT enum values for filtering (NOT strings)
        public int? ApprovalStatusId { get; set; } // 1=Pending, 2=Approved, 3=Rejected, etc.
        public int? PriorityId { get; set; }        // 0=Low, 1=Normal, 2=High, 3=Urgent

        public int? CityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "c_createddate";
        public string? SortOrder { get; set; } = "DESC";
    }

    #endregion

    #region List/Grid Response Models

    /// <summary>
    /// Response model for partner request listing with pagination
    /// </summary>
    public class PartnerRequestListResponse
    {
        public List<PartnerRequestListItem> Requests { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public PartnerRequestStatistics Stats { get; set; } = new();
    }

    /// <summary>
    /// Individual partner request item for grid display
    /// Contains ONLY essential info for list view
    /// </summary>
    public class PartnerRequestListItem
    {
        public long OwnerId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? RequestNumber { get; set; }

        // Approval Status (both ID and display name)
        public int ApprovalStatusId { get; set; }           // INT enum value (1, 2, 3...)
        public string ApprovalStatusName { get; set; } = string.Empty;  // Display text ("Pending", "Approved"...)

        // Priority (both ID and display name)
        public int PriorityId { get; set; }                 // INT enum value (0, 1, 2, 3)
        public string PriorityName { get; set; } = string.Empty;        // Display text ("Low", "Normal", "High"...)

        public DateTime RegistrationDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int DocumentCount { get; set; }
    }

    /// <summary>
    /// Summary statistics for partner requests dashboard
    /// </summary>
    public class PartnerRequestStatistics
    {
        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int InfoRequestedCount { get; set; }
    }

    #endregion

    #region Detail View Response Models

    /// <summary>
    /// Complete partner request detail for admin review
    /// Contains ALL registration data submitted by the partner
    /// This is READ-ONLY - Admin reviews but does not edit partner data
    /// </summary>
    public class PartnerRequestDetailResponse
    {
        // Basic Business Information
        public long OwnerId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? SupportContact { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? AlternateEmail { get; set; }
        public string? CateringNumber { get; set; }
        public string? StdNumber { get; set; }
        public string? LogoPath { get; set; }

        // Workflow Status & Priority
        public int ApprovalStatusId { get; set; }
        public string ApprovalStatusName { get; set; } = string.Empty;
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;

        // Dates
        public DateTime RegistrationDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectionReason { get; set; }

        // Verification Flags
        public bool IsActive { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public bool VerifiedByAdmin { get; set; }

        // Related Registration Data (Structured Sections)
        public PartnerAddressDetails? Address { get; set; }
        public PartnerLegalComplianceDetails? LegalCompliance { get; set; }
        public PartnerBankAccountDetails? BankDetails { get; set; }
        public PartnerServiceOperationsDetails? ServiceOperations { get; set; }

        // Documents & Photos for Review
        public List<PartnerDocumentInfo> Documents { get; set; } = new();
        public List<PartnerPhotoInfo> Photos { get; set; } = new();
    }

    /// <summary>
    /// Partner address details from registration
    /// </summary>
    public class PartnerAddressDetails
    {
        public long AddressId { get; set; }
        public string Building { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? Area { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? MapUrl { get; set; }
    }

    /// <summary>
    /// Partner legal and compliance details (FSSAI, GST, PAN)
    /// </summary>
    public class PartnerLegalComplianceDetails
    {
        public long ComplianceId { get; set; }
        public string FssaiNumber { get; set; } = string.Empty;
        public DateTime FssaiExpiryDate { get; set; }
        public string FssaiCertificatePath { get; set; } = string.Empty;
        public bool GstApplicable { get; set; }
        public string? GstNumber { get; set; }
        public string? GstCertificatePath { get; set; }
        public string PanName { get; set; } = string.Empty;
        public string PanNumber { get; set; } = string.Empty;
        public string? PanFilePath { get; set; }
    }

    /// <summary>
    /// Partner bank account details for payment settlement
    /// </summary>
    public class PartnerBankAccountDetails
    {
        public long BankId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string? ChequePath { get; set; }
        public string? UpiId { get; set; }
    }

    /// <summary>
    /// Partner service and operations details
    /// </summary>
    public class PartnerServiceOperationsDetails
    {
        public long OperationId { get; set; }
        public string? CuisineTypes { get; set; }      // Comma-separated IDs
        public string? ServiceTypes { get; set; }       // Comma-separated IDs
        public string? EventTypes { get; set; }         // Comma-separated IDs
        public string? FoodTypes { get; set; }          // Comma-separated IDs
        public decimal? MinDishOrder { get; set; }
        public bool DeliveryAvailable { get; set; }
        public int? DeliveryRadiusKm { get; set; }
        public string? ServingTimeSlots { get; set; }   // Comma-separated time slot IDs
    }

    /// <summary>
    /// Document information for admin review
    /// </summary>
    public class PartnerDocumentInfo
    {
        public long MediaId { get; set; }
        public int DocumentTypeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// Photo information (kitchen, menu, ambience, etc.)
    /// </summary>
    public class PartnerPhotoInfo
    {
        public long MediaId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    #endregion

    #region Action Request/Response Models

    /// <summary>
    /// Request model for approval action
    /// </summary>
    public class ApprovePartnerRequest
    {
        public long OwnerId { get; set; }
        public string? Remarks { get; set; }
        public bool SendNotification { get; set; } = true;
    }

    /// <summary>
    /// Request model for rejection action
    /// </summary>
    public class RejectPartnerRequest
    {
        public long OwnerId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;  // Mandatory
        public bool SendNotification { get; set; } = true;
    }

    /// <summary>
    /// Request model for updating priority
    /// </summary>
    public class UpdatePriorityRequest
    {
        public long OwnerId { get; set; }
        public int PriorityId { get; set; }  // 0=Low, 1=Normal, 2=High, 3=Urgent
    }

    /// <summary>
    /// Result of approval/rejection action
    /// </summary>
    public class ApprovalActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NewStatusId { get; set; }
        public string NewStatusName { get; set; } = string.Empty;
    }

    #endregion

    #region Enum Reference Models (for UI dropdowns)

    /// <summary>
    /// Approval status options for UI dropdowns
    /// </summary>
    public class ApprovalStatusOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Priority status options for UI dropdowns
    /// </summary>
    public class PriorityStatusOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
