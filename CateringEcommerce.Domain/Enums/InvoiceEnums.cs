using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Invoice type enumeration - ONLY Model A (Installment) supported
    /// Model B (Full Payment) has been removed as per business requirements
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// Booking Invoice - 40% advance payment (Proforma Invoice)
        /// Generated when: Partner approves order
        /// Nature: Proforma (not valid for GST input credit)
        /// Due: Within 7 days of generation
        /// </summary>
        [Display(Name = "Booking Invoice", Description = "40% Advance Payment (Proforma)")]
        BOOKING = 1,

        /// <summary>
        /// Pre-Event Tax Invoice - 35% payment before event
        /// Generated when: Guest lock date reached (X days before event)
        /// Nature: Tax Invoice (valid for GST input credit)
        /// Due: Before event date (typically 3 days before)
        /// </summary>
        [Display(Name = "Pre-Event Tax Invoice", Description = "35% Pre-Event Payment")]
        PRE_EVENT = 2,

        /// <summary>
        /// Final Settlement Invoice - 25% + extras after event
        /// Generated when: Supervisor submits event completion report
        /// Nature: Tax Invoice (valid for GST input credit)
        /// Due: Within 7 days after event
        /// Includes: Extra guests, add-ons, overtime charges
        /// </summary>
        [Display(Name = "Final Settlement Invoice", Description = "25% + Extra Charges")]
        FINAL = 3

        // NOTE: FULL_PAYMENT (Model B) has been removed - ONLY installment model supported
    }

    /// <summary>
    /// Invoice status enumeration
    /// Tracks payment status and lifecycle of each invoice
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// Draft - Invoice created but not finalized
        /// Not visible to customer, internal use only
        /// </summary>
        [Display(Name = "Draft", Description = "Invoice in draft state")]
        DRAFT = 0,

        /// <summary>
        /// Unpaid - Invoice generated and sent to customer, awaiting payment
        /// Customer can view and download invoice
        /// Payment gateway link active
        /// </summary>
        [Display(Name = "Unpaid", Description = "Awaiting payment")]
        UNPAID = 1,

        /// <summary>
        /// Partially Paid - Some amount paid but balance remains
        /// Rare case, typically happens with payment failures or partial refunds
        /// </summary>
        [Display(Name = "Partially Paid", Description = "Partial payment received")]
        PARTIALLY_PAID = 2,

        /// <summary>
        /// Paid - Full invoice amount received
        /// Payment verified and confirmed
        /// Triggers next stage invoice generation (if applicable)
        /// </summary>
        [Display(Name = "Paid", Description = "Fully paid")]
        PAID = 3,

        /// <summary>
        /// Overdue - Payment not received by due date
        /// Automated reminders triggered
        /// May result in order cancellation if booking invoice
        /// </summary>
        [Display(Name = "Overdue", Description = "Payment overdue")]
        OVERDUE = 4,

        /// <summary>
        /// Expired - Invoice validity period expired
        /// Typically booking invoices that weren't paid within deadline
        /// Order may be auto-cancelled
        /// </summary>
        [Display(Name = "Expired", Description = "Invoice expired")]
        EXPIRED = 5,

        /// <summary>
        /// Cancelled - Invoice cancelled (order cancelled or invoice regenerated)
        /// Not deletable, kept for audit trail
        /// </summary>
        [Display(Name = "Cancelled", Description = "Invoice cancelled")]
        CANCELLED = 6,

        /// <summary>
        /// Refunded - Payment refunded to customer
        /// Typically occurs during cancellations with refund policy
        /// </summary>
        [Display(Name = "Refunded", Description = "Payment refunded")]
        REFUNDED = 7
    }

    /// <summary>
    /// Payment stage type enumeration
    /// Defines the three payment stages in Model A (Installment)
    /// Percentages are LOCKED and cannot be changed
    /// </summary>
    public enum PaymentStageType
    {
        /// <summary>
        /// Booking Stage - 40% of total order value
        /// First payment, advance booking confirmation
        /// Due: Within 7 days of order approval
        /// </summary>
        [Display(Name = "Booking (40%)", Description = "Advance booking payment")]
        BOOKING = 1,

        /// <summary>
        /// Pre-Event Stage - 35% of total order value
        /// Second payment, before event starts
        /// Due: Before event date (triggered by guest lock)
        /// Total paid at this stage: 75% (40% + 35%)
        /// </summary>
        [Display(Name = "Pre-Event (35%)", Description = "Pre-event payment")]
        PRE_EVENT = 2,

        /// <summary>
        /// Final Stage - 25% of total order value + extra charges
        /// Third and final payment, after event completion
        /// Due: Within 7 days after event
        /// Includes: Base 25% + extra guests + add-ons + overtime
        /// </summary>
        [Display(Name = "Final (25%)", Description = "Final settlement payment")]
        FINAL = 3
    }

    /// <summary>
    /// Invoice line item type enumeration
    /// Categorizes different types of charges on an invoice
    /// </summary>
    public enum InvoiceLineItemType
    {
        /// <summary>
        /// Package - Catering package (e.g., Wedding Package - Deluxe)
        /// Includes multiple items bundled together
        /// </summary>
        [Display(Name = "Package", Description = "Catering package")]
        PACKAGE = 1,

        /// <summary>
        /// Food Item - Individual food item (e.g., Paneer Tikka - 50 plates)
        /// </summary>
        [Display(Name = "Food Item", Description = "Individual food item")]
        FOOD_ITEM = 2,

        /// <summary>
        /// Decoration - Decoration service (e.g., Stage Decoration - Premium)
        /// </summary>
        [Display(Name = "Decoration", Description = "Decoration service")]
        DECORATION = 3,

        /// <summary>
        /// Extra Guest - Additional guests beyond locked count
        /// Only appears in FINAL invoice
        /// Calculated: (Final guest count - Locked guest count) × Per plate rate
        /// </summary>
        [Display(Name = "Extra Guests", Description = "Additional guests beyond locked count")]
        EXTRA_GUEST = 4,

        /// <summary>
        /// Add-on - Additional items ordered after menu lock
        /// Only appears in FINAL invoice
        /// </summary>
        [Display(Name = "Add-on Item", Description = "Items added after menu lock")]
        ADDON = 5,

        /// <summary>
        /// Overtime - Staff overtime charges
        /// Only appears in FINAL invoice
        /// Calculated based on actual event duration vs planned
        /// </summary>
        [Display(Name = "Overtime Charges", Description = "Staff overtime charges")]
        OVERTIME = 6,

        /// <summary>
        /// Delivery - Delivery charges (if applicable)
        /// </summary>
        [Display(Name = "Delivery Charges", Description = "Delivery fees")]
        DELIVERY = 7,

        /// <summary>
        /// Staff - Additional staff charges
        /// </summary>
        [Display(Name = "Staff Charges", Description = "Additional staff")]
        STAFF = 8,

        /// <summary>
        /// Other - Miscellaneous charges
        /// </summary>
        [Display(Name = "Other", Description = "Other charges")]
        OTHER = 9
    }

    /// <summary>
    /// Invoice audit action enumeration
    /// Tracks all actions performed on invoices for audit trail
    /// </summary>
    public enum InvoiceAuditAction
    {
        /// <summary>
        /// Invoice generated - Initial creation
        /// </summary>
        [Display(Name = "Generated", Description = "Invoice generated")]
        GENERATED = 1,

        /// <summary>
        /// Invoice viewed - Customer/Admin viewed invoice
        /// </summary>
        [Display(Name = "Viewed", Description = "Invoice viewed")]
        VIEWED = 2,

        /// <summary>
        /// Invoice downloaded - PDF downloaded
        /// </summary>
        [Display(Name = "Downloaded", Description = "Invoice PDF downloaded")]
        DOWNLOADED = 3,

        /// <summary>
        /// Payment received - Invoice marked as paid
        /// </summary>
        [Display(Name = "Paid", Description = "Payment received")]
        PAID = 4,

        /// <summary>
        /// Payment failed - Payment attempt failed
        /// </summary>
        [Display(Name = "Payment Failed", Description = "Payment attempt failed")]
        PAYMENT_FAILED = 5,

        /// <summary>
        /// Invoice cancelled - Invoice cancelled
        /// </summary>
        [Display(Name = "Cancelled", Description = "Invoice cancelled")]
        CANCELLED = 6,

        /// <summary>
        /// Invoice regenerated - New version created
        /// </summary>
        [Display(Name = "Regenerated", Description = "Invoice regenerated")]
        REGENERATED = 7,

        /// <summary>
        /// Status changed - Manual status update by admin
        /// </summary>
        [Display(Name = "Status Changed", Description = "Status manually updated")]
        STATUS_CHANGED = 8,

        /// <summary>
        /// Email sent - Invoice email sent to customer
        /// </summary>
        [Display(Name = "Email Sent", Description = "Invoice email sent")]
        EMAIL_SENT = 9,

        /// <summary>
        /// SMS sent - Invoice SMS sent to customer
        /// </summary>
        [Display(Name = "SMS Sent", Description = "Invoice SMS sent")]
        SMS_SENT = 10,

        /// <summary>
        /// Reminder sent - Payment reminder sent
        /// </summary>
        [Display(Name = "Reminder Sent", Description = "Payment reminder sent")]
        REMINDER_SENT = 11,

        /// <summary>
        /// Marked overdue - Automatically marked as overdue
        /// </summary>
        [Display(Name = "Marked Overdue", Description = "Marked as overdue")]
        MARKED_OVERDUE = 12
    }

    /// <summary>
    /// Payment schedule status enumeration
    /// Tracks status of each payment stage in the schedule
    /// </summary>
    public enum PaymentScheduleStatus
    {
        /// <summary>
        /// Pending - Awaiting payment
        /// Invoice may or may not be generated yet
        /// </summary>
        [Display(Name = "Pending", Description = "Awaiting payment")]
        PENDING = 0,

        /// <summary>
        /// Paid - Payment received for this stage
        /// Invoice marked as paid
        /// </summary>
        [Display(Name = "Paid", Description = "Payment received")]
        PAID = 1,

        /// <summary>
        /// Overdue - Payment deadline passed
        /// Automatic reminders and escalation triggered
        /// </summary>
        [Display(Name = "Overdue", Description = "Payment overdue")]
        OVERDUE = 2,

        /// <summary>
        /// Cancelled - Stage cancelled (order cancelled)
        /// </summary>
        [Display(Name = "Cancelled", Description = "Payment cancelled")]
        CANCELLED = 3
    }

    /// <summary>
    /// Payment trigger event enumeration
    /// Defines what event triggers invoice generation for each stage
    /// </summary>
    public enum PaymentTriggerEvent
    {
        /// <summary>
        /// Order approved by partner - Triggers BOOKING invoice
        /// </summary>
        [Display(Name = "Order Approved", Description = "Partner approved the order")]
        ORDER_APPROVED = 1,

        /// <summary>
        /// Guest lock date reached - Triggers PRE_EVENT invoice
        /// Typically 5 days before event
        /// </summary>
        [Display(Name = "Guest Lock Date", Description = "Guest count lock date reached")]
        GUEST_LOCK_DATE = 2,

        /// <summary>
        /// Event completed - Triggers FINAL invoice
        /// Supervisor submits event completion report
        /// </summary>
        [Display(Name = "Event Completed", Description = "Event marked as completed")]
        EVENT_COMPLETED = 3,

        /// <summary>
        /// Manual trigger - Admin manually triggers invoice generation
        /// </summary>
        [Display(Name = "Manual Trigger", Description = "Manually triggered by admin")]
        MANUAL_TRIGGER = 4
    }

    /// <summary>
    /// Invoice user type enumeration
    /// Identifies who performed an action on the invoice
    /// </summary>
    public enum InvoiceUserType
    {
        /// <summary>
        /// Customer/User
        /// </summary>
        [Display(Name = "User", Description = "Customer")]
        USER = 1,

        /// <summary>
        /// Admin
        /// </summary>
        [Display(Name = "Admin", Description = "Admin user")]
        ADMIN = 2,

        /// <summary>
        /// Catering partner/owner
        /// </summary>
        [Display(Name = "Owner", Description = "Catering partner")]
        OWNER = 3,

        /// <summary>
        /// Supervisor
        /// </summary>
        [Display(Name = "Supervisor", Description = "Event supervisor")]
        SUPERVISOR = 4,

        /// <summary>
        /// System - Automated actions
        /// </summary>
        [Display(Name = "System", Description = "System automated action")]
        SYSTEM = 5
    }
}
