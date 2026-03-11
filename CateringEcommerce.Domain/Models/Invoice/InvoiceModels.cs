using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Models.Invoice
{
    // ===================================
    // INVOICE RESPONSE DTO
    // ===================================
    /// <summary>
    /// Complete invoice details - returned to clients
    /// Contains all invoice information including line items and payment details
    /// </summary>
    public class InvoiceDto
    {
        public long InvoiceId { get; set; }
        public long OrderId { get; set; }
        public long? EventId { get; set; }
        public long UserId { get; set; }
        public long CateringOwnerId { get; set; }

        // Classification
        public InvoiceType InvoiceType { get; set; }
        public string InvoiceTypeDisplay { get; set; } = string.Empty;
        public bool IsProforma { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        // Financial Details
        public decimal Subtotal { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }

        // Payment Stage
        public PaymentStageType PaymentStageType { get; set; }
        public string PaymentStageTypeDisplay { get; set; } = string.Empty;
        public decimal PaymentPercentage { get; set; }

        // Status
        public InvoiceStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;

        // Payment Gateway
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }

        // GST Compliance
        public string? CompanyGstin { get; set; }
        public string? CustomerGstin { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string SacCode { get; set; } = "996331";

        // Additional Info
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? InternalRemarks { get; set; }

        // PDF
        public string? PdfPath { get; set; }
        public DateTime? PdfGeneratedDate { get; set; }
        public bool IsPdfAvailable => !string.IsNullOrEmpty(PdfPath);

        // Audit
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Version Control
        public int Version { get; set; }
        public long? ParentInvoiceId { get; set; }
        public bool IsLatestVersion => ParentInvoiceId == null;

        // Related Data
        public List<InvoiceLineItemDto> LineItems { get; set; } = new List<InvoiceLineItemDto>();
        public InvoiceOrderSummaryDto? OrderSummary { get; set; }
        public List<InvoicePaymentHistoryDto>? PaymentHistory { get; set; }

        // Computed Properties
        public bool IsOverdue => Status == InvoiceStatus.OVERDUE || (Status == InvoiceStatus.UNPAID && DueDate.HasValue && DueDate.Value < DateTime.Now);
        public int? DaysUntilDue => DueDate.HasValue ? (DueDate.Value.Date - DateTime.Now.Date).Days : null;
        public bool IsPaymentPending => Status == InvoiceStatus.UNPAID || Status == InvoiceStatus.PARTIALLY_PAID;
    }

    // ===================================
    // INVOICE LINE ITEM DTO
    // ===================================
    /// <summary>
    /// Individual line item on an invoice
    /// Represents packages, food items, decorations, extras, etc.
    /// </summary>
    public class InvoiceLineItemDto
    {
        public long LineItemId { get; set; }
        public long InvoiceId { get; set; }

        public InvoiceLineItemType ItemType { get; set; }
        public string ItemTypeDisplay { get; set; } = string.Empty;
        public long? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? HsnSacCode { get; set; }

        public decimal Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? Unit { get; set; } // Alias for UnitOfMeasure for compatibility
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public decimal TaxPercent { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }

        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal Total { get; set; }
        public decimal TotalAmount { get; set; } // Alias for Total for compatibility
        public int Sequence { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ===================================
    // CREATE INVOICE REQUEST DTO
    // ===================================
    /// <summary>
    /// Request to generate a new invoice
    /// Used by invoice generation service
    /// </summary>
    public class CreateInvoiceDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public InvoiceType InvoiceType { get; set; }

        [Required]
        public PaymentStageType PaymentStageType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be greater than zero")]
        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        public string? Notes { get; set; }

        [Required]
        public List<CreateInvoiceLineItemDto> LineItems { get; set; } = new List<CreateInvoiceLineItemDto>();

        // Optional overrides
        public decimal? CgstPercent { get; set; }
        public decimal? SgstPercent { get; set; }
        public DateTime? DueDate { get; set; }
        public long? CreatedBy { get; set; }
    }

    // ===================================
    // CREATE INVOICE LINE ITEM DTO
    // ===================================
    /// <summary>
    /// Line item to be added to a new invoice
    /// </summary>
    public class CreateInvoiceLineItemDto
    {
        [Required]
        public InvoiceLineItemType ItemType { get; set; }

        public long? ItemId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? HsnSacCode { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [MaxLength(20)]
        public string? UnitOfMeasure { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        public int Sequence { get; set; } = 1;
    }

    // ===================================
    // PAYMENT SCHEDULE DTO
    // ===================================
    /// <summary>
    /// Payment schedule for an order
    /// Shows all three payment stages with timeline
    /// </summary>
    public class PaymentScheduleDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal PaymentProgressPercentage { get; set; }

        public List<PaymentStageDto> Stages { get; set; } = new List<PaymentStageDto>();

        // Computed properties
        public PaymentStageDto? CurrentStage => Stages.FirstOrDefault(s => s.Status == PaymentScheduleStatus.PENDING);
        public PaymentStageDto? NextStage => Stages.Where(s => s.Status == PaymentScheduleStatus.PENDING).OrderBy(s => s.StageSequence).FirstOrDefault();
        public bool IsFullyPaid => TotalPendingAmount == 0 && Stages.All(s => s.Status == PaymentScheduleStatus.PAID);
        public bool CanStartEvent => PaymentProgressPercentage >= 75; // 40% + 35% = 75%
    }

    // ===================================
    // PAYMENT STAGE DTO
    // ===================================
    /// <summary>
    /// Individual payment stage in the schedule
    /// </summary>
    public class PaymentStageDto
    {
        public long ScheduleId { get; set; }
        public long OrderId { get; set; }

        public PaymentStageType StageType { get; set; }
        public string StageTypeDisplay { get; set; } = string.Empty;
        public int StageSequence { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }
        public PaymentTriggerEvent TriggerEvent { get; set; }
        public string TriggerEventDisplay { get; set; } = string.Empty;
        public DateTime? AutoGenerateDate { get; set; }

        public long? InvoiceId { get; set; }
        public InvoiceDto? Invoice { get; set; }

        public PaymentScheduleStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;

        public int ReminderSentCount { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public DateTime? NextReminderDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Computed properties
        public bool IsInvoiceGenerated => InvoiceId.HasValue;
        public bool IsPaid => Status == PaymentScheduleStatus.PAID;
        public bool IsPending => Status == PaymentScheduleStatus.PENDING;
        public bool IsOverdue => Status == PaymentScheduleStatus.OVERDUE;
        public int? DaysUntilDue => DueDate.HasValue ? (DueDate.Value.Date - DateTime.Now.Date).Days : null;
    }

    // ===================================
    // INVOICE GENERATION REQUEST DTO
    // ===================================
    /// <summary>
    /// Request to auto-generate invoice based on order
    /// Used by automated invoice generation jobs
    /// </summary>
    public class InvoiceGenerationRequestDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public InvoiceType InvoiceType { get; set; }

        public long? TriggeredBy { get; set; }
        public InvoiceUserType? TriggeredByType { get; set; }
        public string? TriggerReason { get; set; }

        // Optional: For FINAL invoice with extra charges
        public int? ExtraGuestCount { get; set; }
        public decimal? ExtraGuestCharges { get; set; }
        public decimal? AddonCharges { get; set; }
        public decimal? OvertimeCharges { get; set; }
        public decimal? OtherCharges { get; set; }
    }

    // ===================================
    // INVOICE PDF REQUEST DTO
    // ===================================
    /// <summary>
    /// Request to generate PDF for an invoice
    /// </summary>
    public class InvoicePdfRequestDto
    {
        [Required]
        public long InvoiceId { get; set; }

        public bool RegeneratePdf { get; set; } = false;
        public string? Watermark { get; set; }
        public bool IncludeCompanyLogo { get; set; } = true;
        public long? RequestedBy { get; set; }
    }

    // ===================================
    // GST BREAKDOWN DTO
    // ===================================
    /// <summary>
    /// GST tax breakdown for display purposes
    /// </summary>
    public class GstBreakdownDto
    {
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string PlaceOfSupply { get; set; } = string.Empty;
        public string SacCode { get; set; } = "996331";
        public bool IsIntraState { get; set; } = true; // CGST+SGST
        public bool IsInterState { get; set; } = false; // IGST (not implemented yet)
    }

    // ===================================
    // INVOICE ORDER SUMMARY DTO
    // ===================================
    /// <summary>
    /// Order summary included in invoice
    /// Minimal order details for invoice context
    /// </summary>
    public class InvoiceOrderSummaryDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public int? OriginalGuestCount { get; set; }
        public int? FinalGuestCount { get; set; }
        public bool GuestCountLocked { get; set; }
        public bool MenuLocked { get; set; }

        // Customer details
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerGstin { get; set; }

        // Partner details
        public string PartnerName { get; set; } = string.Empty;
        public string PartnerPhone { get; set; } = string.Empty;
        public string PartnerEmail { get; set; } = string.Empty;
        public string? PartnerGstin { get; set; }
        public string? PartnerAddress { get; set; }
    }

    // ===================================
    // INVOICE PAYMENT HISTORY DTO
    // ===================================
    /// <summary>
    /// Payment history for an invoice
    /// Shows all payment attempts and transactions
    /// </summary>
    public class InvoicePaymentHistoryDto
    {
        public long PaymentId { get; set; }
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? RazorpayPaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? Remarks { get; set; }
    }

    // ===================================
    // INVOICE SUMMARY DTO
    // ===================================
    /// <summary>
    /// Lightweight invoice summary for list views
    /// Does not include line items or full details
    /// </summary>
    public class InvoiceSummaryDto
    {
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceType InvoiceType { get; set; }
        public string InvoiceTypeDisplay { get; set; } = string.Empty;
        public InvoiceStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public int? DaysUntilDue { get; set; }
    }

    // ===================================
    // INVOICE LIST REQUEST DTO
    // ===================================
    /// <summary>
    /// Request parameters for listing invoices with filters
    /// Used by admin/user invoice list pages
    /// </summary>
    public class InvoiceListRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public long? OrderId { get; set; }
        public long? UserId { get; set; }
        public long? CateringOwnerId { get; set; }

        public InvoiceType? InvoiceType { get; set; }
        public InvoiceStatus? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SearchTerm { get; set; } // Invoice number, order number, customer name

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public bool? IsOverdue { get; set; }
        public bool? IsPaid { get; set; }

        public string SortBy { get; set; } = "InvoiceDate";
        public string SortOrder { get; set; } = "DESC";
    }

    // ===================================
    // INVOICE LIST RESPONSE DTO
    // ===================================
    /// <summary>
    /// Paginated response for invoice list
    /// </summary>
    public class InvoiceListResponseDto
    {
        public List<InvoiceSummaryDto> Invoices { get; set; } = new List<InvoiceSummaryDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // ===================================
    // UPDATE INVOICE STATUS DTO
    // ===================================
    /// <summary>
    /// Request to update invoice status
    /// Used by admin to manually change status or by payment service
    /// </summary>
    public class UpdateInvoiceStatusDto
    {
        [Required]
        public long InvoiceId { get; set; }

        [Required]
        public InvoiceStatus NewStatus { get; set; }

        public string? Remarks { get; set; }
        public long? UpdatedBy { get; set; }
    }

    // ===================================
    // LINK PAYMENT TO INVOICE DTO
    // ===================================
    /// <summary>
    /// Request to link a payment to an invoice
    /// Called after successful Razorpay payment
    /// </summary>
    public class LinkPaymentToInvoiceDto
    {
        [Required]
        public long InvoiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RazorpayPaymentId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RazorpaySignature { get; set; } = string.Empty;

        [Required]
        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? TransactionId { get; set; }
        public string? PaymentRemarks { get; set; }
    }

    // ===================================
    // INVOICE AUDIT LOG DTO
    // ===================================
    /// <summary>
    /// Audit log entry for invoice action tracking
    /// </summary>
    public class InvoiceAuditLogDto
    {
        public long AuditId { get; set; }
        public long InvoiceId { get; set; }
        public long OrderId { get; set; }

        public InvoiceAuditAction Action { get; set; }
        public string ActionDisplay { get; set; } = string.Empty;

        public long? PerformedBy { get; set; }
        public InvoiceUserType? PerformedByType { get; set; }
        public string? PerformedByName { get; set; }

        public InvoiceStatus? OldStatus { get; set; }
        public InvoiceStatus? NewStatus { get; set; }
        public decimal? OldAmountPaid { get; set; }
        public decimal? NewAmountPaid { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Remarks { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // ===================================
    // INVOICE STATISTICS DTO
    // ===================================
    /// <summary>
    /// Invoice statistics for dashboard/analytics
    /// </summary>
    public class InvoiceStatisticsDto
    {
        public int TotalInvoices { get; set; }
        public int UnpaidInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int OverdueInvoices { get; set; }

        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalOverdueAmount { get; set; }

        public int BookingInvoiceCount { get; set; }
        public int PreEventInvoiceCount { get; set; }
        public int FinalInvoiceCount { get; set; }

        public decimal AverageInvoiceAmount { get; set; }
        public decimal AveragePaymentTime { get; set; } // Days from invoice date to payment date
        public decimal PaymentSuccessRate { get; set; } // Percentage

        public Dictionary<string, int> InvoicesByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByStage { get; set; } = new Dictionary<string, decimal>();
    }
}
