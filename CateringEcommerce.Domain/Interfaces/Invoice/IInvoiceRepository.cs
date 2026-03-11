using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Models.Invoice;

namespace CateringEcommerce.Domain.Interfaces.Invoice
{
    /// <summary>
    /// Invoice repository interface
    /// Handles all database operations for invoices, payment schedules, and audit logs
    /// </summary>
    public interface IInvoiceRepository
    {
        // ===================================
        // INVOICE GENERATION
        // ===================================

        /// <summary>
        /// Generates a new invoice based on order and invoice type
        /// Auto-calculates amounts, GST, and creates line items
        /// Returns the generated invoice ID
        /// </summary>
        /// <param name="request">Invoice generation request</param>
        /// <returns>Invoice ID</returns>
        Task<long> GenerateInvoiceAsync(InvoiceGenerationRequestDto request);

        /// <summary>
        /// Creates a new invoice with manual data
        /// Used for custom invoice generation or admin overrides
        /// </summary>
        /// <param name="invoice">Invoice creation data</param>
        /// <returns>Invoice ID</returns>
        Task<long> CreateInvoiceAsync(CreateInvoiceDto invoice);

        // ===================================
        // INVOICE RETRIEVAL
        // ===================================

        /// <summary>
        /// Gets complete invoice details by ID
        /// Includes line items, order summary, payment history
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>Invoice DTO or null if not found</returns>
        Task<InvoiceDto?> GetInvoiceByIdAsync(long invoiceId);

        /// <summary>
        /// Gets invoice by invoice number
        /// </summary>
        /// <param name="invoiceNumber">Invoice number (e.g., INV-20260220-00001)</param>
        /// <returns>Invoice DTO or null if not found</returns>
        Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);

        /// <summary>
        /// Gets all invoices for a specific order
        /// Returns all three invoices (BOOKING, PRE_EVENT, FINAL) if generated
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of invoices</returns>
        Task<List<InvoiceDto>> GetInvoicesByOrderIdAsync(long orderId);

        /// <summary>
        /// Gets invoice by order ID and invoice type
        /// Used to check if specific invoice exists for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="invoiceType">Invoice type (BOOKING/PRE_EVENT/FINAL)</param>
        /// <returns>Invoice DTO or null if not found</returns>
        Task<InvoiceDto?> GetInvoiceByOrderAndTypeAsync(long orderId, InvoiceType invoiceType);

        /// <summary>
        /// Gets paginated list of invoices with filters
        /// Used for admin/user invoice listing pages
        /// </summary>
        /// <param name="request">List request with filters and pagination</param>
        /// <returns>Paginated invoice list</returns>
        Task<InvoiceListResponseDto> GetInvoicesAsync(InvoiceListRequestDto request);

        /// <summary>
        /// Gets all invoices for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's invoices</returns>
        Task<List<InvoiceSummaryDto>> GetInvoicesByUserIdAsync(long userId);

        /// <summary>
        /// Gets paginated invoices for a user with filters
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="type">Optional type filter</param>
        /// <returns>Paginated invoice list</returns>
        Task<InvoiceListResponseDto> GetInvoicesByUserAsync(long userId, int pageNumber, int pageSize, InvoiceStatus? status, InvoiceType? type);

        /// <summary>
        /// Gets all invoices for a catering owner/partner
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <returns>List of partner's invoices</returns>
        Task<List<InvoiceSummaryDto>> GetInvoicesByOwnerIdAsync(long ownerId);

        // ===================================
        // INVOICE UPDATE
        // ===================================

        /// <summary>
        /// Updates invoice status
        /// Validates status transition and logs to audit trail
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="newStatus">New status</param>
        /// <param name="remarks">Optional remarks</param>
        /// <param name="updatedBy">User ID who updated</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateInvoiceStatusAsync(long invoiceId, InvoiceStatus newStatus, string? remarks = null, long? updatedBy = null);

        /// <summary>
        /// Links a Razorpay payment to an invoice
        /// Updates amount paid, balance due, and status
        /// Creates audit log entry
        /// </summary>
        /// <param name="paymentData">Payment linkage data</param>
        /// <returns>True if successful</returns>
        Task<bool> LinkPaymentToInvoiceAsync(LinkPaymentToInvoiceDto paymentData);

        /// <summary>
        /// Updates PDF path after PDF generation
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="pdfPath">Path to generated PDF</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateInvoicePdfPathAsync(long invoiceId, string pdfPath);

        /// <summary>
        /// Recalculates final invoice after event completion
        /// Adds extra guests, add-ons, overtime charges
        /// Regenerates invoice if already exists
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Updated invoice ID</returns>
        Task<long> RecalculateFinalInvoiceAsync(long orderId);

        /// <summary>
        /// Regenerates an existing invoice (creates new version)
        /// Old invoice is marked as cancelled, new one created
        /// </summary>
        /// <param name="invoiceId">Invoice ID to regenerate</param>
        /// <param name="reason">Reason for regeneration</param>
        /// <param name="regeneratedBy">User ID who regenerated</param>
        /// <returns>New invoice ID</returns>
        Task<long> RegenerateInvoiceAsync(long invoiceId, string reason, long? regeneratedBy = null);

        // ===================================
        // PAYMENT SCHEDULE
        // ===================================

        /// <summary>
        /// Creates payment schedule for an order
        /// Generates 3 payment stages (40%, 35%, 25%)
        /// Called when order is first approved
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="totalAmount">Total order amount</param>
        /// <param name="eventDate">Event date</param>
        /// <returns>True if successful</returns>
        Task<bool> CreatePaymentScheduleAsync(long orderId, decimal totalAmount, DateTime eventDate);

        /// <summary>
        /// Gets complete payment schedule for an order
        /// Shows all three stages with status and invoices
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Payment schedule DTO</returns>
        Task<PaymentScheduleDto?> GetPaymentScheduleAsync(long orderId);

        /// <summary>
        /// Gets specific payment stage
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="stageType">Stage type (BOOKING/PRE_EVENT/FINAL)</param>
        /// <returns>Payment stage DTO or null</returns>
        Task<PaymentStageDto?> GetPaymentStageAsync(long orderId, PaymentStageType stageType);

        /// <summary>
        /// Updates payment stage status after invoice payment
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="stageType">Stage type</param>
        /// <param name="invoiceId">Associated invoice ID</param>
        /// <param name="status">New status</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdatePaymentStageStatusAsync(long orderId, PaymentStageType stageType, long invoiceId, PaymentScheduleStatus status);

        /// <summary>
        /// Gets all orders with invoices due for auto-generation
        /// Used by background job to trigger invoice creation
        /// </summary>
        /// <returns>List of order IDs</returns>
        Task<List<long>> GetOrdersForAutoInvoiceGenerationAsync();

        /// <summary>
        /// Updates reminder sent count for a payment stage
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdatePaymentReminderSentAsync(long scheduleId);

        // ===================================
        // INVOICE AUDIT
        // ===================================

        /// <summary>
        /// Logs an audit entry for invoice action
        /// Automatically called by other methods, but can be called manually
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="orderId">Order ID</param>
        /// <param name="action">Action performed</param>
        /// <param name="performedBy">User ID</param>
        /// <param name="performedByType">User type (USER/ADMIN/OWNER/SYSTEM)</param>
        /// <param name="remarks">Optional remarks</param>
        /// <param name="oldStatus">Old status (if status change)</param>
        /// <param name="newStatus">New status (if status change)</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="userAgent">Client user agent</param>
        /// <returns>True if successful</returns>
        Task<bool> LogInvoiceAuditAsync(
            long invoiceId,
            long orderId,
            InvoiceAuditAction action,
            long? performedBy = null,
            InvoiceUserType? performedByType = null,
            string? remarks = null,
            InvoiceStatus? oldStatus = null,
            InvoiceStatus? newStatus = null,
            string? ipAddress = null,
            string? userAgent = null
        );

        /// <summary>
        /// Gets audit log for an invoice
        /// Returns chronological list of all actions
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>List of audit log entries</returns>
        Task<List<InvoiceAuditLogDto>> GetInvoiceAuditLogAsync(long invoiceId);

        // ===================================
        // STATISTICS & REPORTS
        // ===================================

        /// <summary>
        /// Gets invoice statistics for dashboard
        /// Aggregates counts and amounts by status
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="ownerId">Optional owner filter (for partner dashboard)</param>
        /// <returns>Invoice statistics</returns>
        Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, long? ownerId = null);

        /// <summary>
        /// Gets all overdue invoices
        /// Used for reminders and collections
        /// </summary>
        /// <returns>List of overdue invoices</returns>
        Task<List<InvoiceSummaryDto>> GetOverdueInvoicesAsync();

        /// <summary>
        /// Gets invoices due within X days
        /// Used for proactive reminders
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead</param>
        /// <returns>List of invoices due soon</returns>
        Task<List<InvoiceSummaryDto>> GetInvoicesDueSoonAsync(int daysAhead = 3);

        // ===================================
        // VALIDATION & CHECKS
        // ===================================

        /// <summary>
        /// Checks if invoice exists for order and type
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="invoiceType">Invoice type</param>
        /// <returns>True if exists</returns>
        Task<bool> InvoiceExistsAsync(long orderId, InvoiceType invoiceType);

        /// <summary>
        /// Validates if invoice can be paid (status check)
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>True if can be paid</returns>
        Task<bool> CanPayInvoiceAsync(long invoiceId);

        /// <summary>
        /// Validates if next stage invoice can be generated
        /// Checks if previous stage is paid
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="nextStage">Next stage to generate</param>
        /// <returns>True if can generate</returns>
        Task<bool> CanGenerateNextStageInvoiceAsync(long orderId, InvoiceType nextStage);

        /// <summary>
        /// Gets total amount paid for an order across all invoices
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Total paid amount</returns>
        Task<decimal> GetTotalPaidAmountAsync(long orderId);

        /// <summary>
        /// Gets payment progress percentage for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Percentage (0-100)</returns>
        Task<decimal> GetPaymentProgressPercentageAsync(long orderId);

        // Background Job Helper Methods

        /// <summary>
        /// Gets all overdue invoices for background job processing (unpaid and past due date)
        /// Used by background job to mark invoices as overdue
        /// </summary>
        /// <returns>List of overdue invoices with full details</returns>
        Task<List<InvoiceDto>> GetOverdueInvoicesForJobAsync();

        /// <summary>
        /// Gets orders approaching guest lock date (5 days before event)
        /// Used by background job to auto-lock guest counts
        /// </summary>
        /// <returns>List of orders approaching lock</returns>
        Task<List<dynamic>> GetOrdersApproachingGuestLockAsync();

        /// <summary>
        /// Gets orders approaching menu lock date (3 days before event)
        /// Used by background job to auto-lock menus
        /// </summary>
        /// <returns>List of orders approaching lock</returns>
        Task<List<dynamic>> GetOrdersApproachingMenuLockAsync();

        /// <summary>
        /// Gets pending invoices due within specified days
        /// Used by background job to send payment reminders
        /// </summary>
        /// <param name="daysThreshold">Days threshold</param>
        /// <returns>List of pending invoices</returns>
        Task<List<InvoiceDto>> GetPendingInvoicesDueWithinDaysAsync(int daysThreshold);

        /// <summary>
        /// Gets orders ready for PRE_EVENT invoice generation
        /// Orders with BOOKING_PAID status approaching guest lock date
        /// </summary>
        /// <returns>List of orders ready for PRE_EVENT invoice</returns>
        Task<List<dynamic>> GetOrdersReadyForPreEventInvoiceAsync();

        /// <summary>
        /// Gets invoice statistics for a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order invoice statistics</returns>
        Task<object> GetOrderInvoiceStatsAsync(long orderId);
    }
}
