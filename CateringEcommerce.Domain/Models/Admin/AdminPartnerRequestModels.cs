namespace CateringEcommerce.Domain.Models.Admin
{
    #region Partner Request List & Filter Models

    public class AdminPartnerRequestListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string? Status { get; set; } // PENDING, UNDER_REVIEW, APPROVED, REJECTED, INFO_REQUESTED
        public int? CityId { get; set; }
        public string? State { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Priority { get; set; } // NORMAL, HIGH, URGENT
        public string? SortBy { get; set; } = "c_createddate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminPartnerRequestListResponse
    {
        public List<AdminPartnerRequestListItem> Requests { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public PartnerRequestStats Stats { get; set; } = new();
    }

    public class AdminPartnerRequestListItem
    {
        public long OwnerId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = "NORMAL";
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public bool HasUnreadDocuments { get; set; }
        public int DocumentCount { get; set; }
        public int PhotoCount { get; set; }
    }

    public class PartnerRequestStats
    {
        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int InfoRequestedCount { get; set; }
    }

    #endregion

    #region Partner Request Detail Models

    public class AdminPartnerRequestDetail
    {
        // Basic Info
        public long OwnerId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? AlternatePhone { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? AlternateEmail { get; set; }
        public string? CateringNumber { get; set; }
        public string? StdNumber { get; set; }
        public string? LogoUrl { get; set; }

        // Address
        public PartnerAddress? Address { get; set; }

        // Compliance
        public PartnerCompliance? Compliance { get; set; }

        // Bank Details
        public PartnerBankDetails? BankDetails { get; set; }

        // Operations
        public PartnerOperations? Operations { get; set; }

        // Documents & Photos
        public List<PartnerDocument> Documents { get; set; } = new();
        public List<PartnerPhoto> Photos { get; set; } = new();

        // Status & Workflow
        public string Status { get; set; } = "PENDING";
        public string Priority { get; set; } = "NORMAL";
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public long? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectionReason { get; set; }
        public string? InternalNotes { get; set; }

        // Flags
        public bool IsActive { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public bool VerifiedByAdmin { get; set; }

        // Timeline
        public List<PartnerActionLog> Timeline { get; set; } = new();
    }

    public class PartnerAddress
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

    public class PartnerCompliance
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

    public class PartnerBankDetails
    {
        public long BankId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string? ChequePath { get; set; }
        public string? UpiId { get; set; }
    }

    public class PartnerOperations
    {
        public long OperationId { get; set; }
        public string? CuisineTypes { get; set; }
        public string? ServiceTypes { get; set; }
        public string? EventTypes { get; set; }
        public string? FoodTypes { get; set; }
        public int? MinGuestCount { get; set; }
        public bool DeliveryAvailable { get; set; }
        public int? DeliveryRadiusKm { get; set; }
        public string? ServingTimeSlots { get; set; }
    }

    public class PartnerDocument
    {
        public long MediaId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class PartnerPhoto
    {
        public long MediaId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Kitchen, Menu, Logo, etc.
        public DateTime UploadedAt { get; set; }
    }

    public class PartnerActionLog
    {
        public long ActionId { get; set; }
        public long AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Remarks { get; set; }
        public DateTime ActionDate { get; set; }
        public string? IpAddress { get; set; }
    }

    #endregion

    #region Partner Request Action Models

    public class PartnerRequestActionRequest
    {
        public long OwnerId { get; set; }
        public string ActionType { get; set; } = string.Empty; // APPROVE, REJECT, REQUEST_INFO
        public string? Remarks { get; set; }
        public string? RejectionReason { get; set; }
        public List<string>? InfoRequirements { get; set; }
        public CommunicationSettings? Communication { get; set; }
    }

    public class CommunicationSettings
    {
        public bool SendNotification { get; set; } = true;
        public bool SendEmail { get; set; } = true;
        public bool SendSms { get; set; } = false;
        public string? EmailSubject { get; set; }
        public string? EmailBody { get; set; }
        public string? SmsBody { get; set; }
    }

    public class PartnerRequestActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public long? CateringId { get; set; } // If approved, the created catering ID
    }

    #endregion

    #region Communication Models

    public class PartnerCommunicationRequest
    {
        public long OwnerId { get; set; }
        public string CommunicationType { get; set; } = "EMAIL"; // EMAIL, SMS, BOTH
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? SentToEmail { get; set; }
        public string? SentToPhone { get; set; }
    }

    public class PartnerCommunicationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public string? EmailStatus { get; set; }
        public string? SmsStatus { get; set; }
    }

    public class PartnerCommunicationHistory
    {
        public long CommunicationId { get; set; }
        public long AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string CommunicationType { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SentToEmail { get; set; }
        public string? SentToPhone { get; set; }
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public string? EmailStatus { get; set; }
        public string? SmsStatus { get; set; }
        public DateTime SentDate { get; set; }
    }

    #endregion

    #region Notification Models

    public class AdminNotificationListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsRead { get; set; }
        public string? NotificationType { get; set; }
    }

    public class AdminNotificationListResponse
    {
        public List<AdminNotificationItem> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }

    public class AdminNotificationItem
    {
        public long NotificationId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public long? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MarkNotificationReadRequest
    {
        public long NotificationId { get; set; }
    }

    public class MarkAllNotificationsReadRequest
    {
        // Optional: Can add filters here if needed
    }

    #endregion
}
