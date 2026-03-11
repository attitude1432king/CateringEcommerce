using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Order status enumeration with payment-aware workflow
    /// Implements strict state machine for payment enforcement
    /// Status progression is SEQUENTIAL and NON-REVERSIBLE
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order created by customer, awaiting partner review
        /// Next: PARTNER_REVIEW or CANCELLED
        /// </summary>
        [Display(Name = "Order Created", Description = "Order placed, awaiting review")]
        ORDER_CREATED = 1,

        /// <summary>
        /// Partner is reviewing the order
        /// Next: ORDER_APPROVED or REJECTED
        /// </summary>
        [Display(Name = "Under Review", Description = "Partner reviewing order")]
        PARTNER_REVIEW = 2,

        /// <summary>
        /// Partner approved order
        /// Triggers: BOOKING invoice generation (40%)
        /// Next: BOOKING_PENDING
        /// </summary>
        [Display(Name = "Order Approved", Description = "Partner accepted the order")]
        ORDER_APPROVED = 3,

        /// <summary>
        /// BOOKING invoice generated, awaiting 40% payment
        /// Payment Gate: Cannot proceed until paid
        /// Next: BOOKING_PAID or BOOKING_OVERDUE
        /// </summary>
        [Display(Name = "Booking Pending", Description = "Awaiting 40% booking payment")]
        BOOKING_PENDING = 4,

        /// <summary>
        /// BOOKING invoice overdue
        /// Risk: Order may be auto-cancelled after X days
        /// Next: BOOKING_PAID or CANCELLED
        /// </summary>
        [Display(Name = "Booking Overdue", Description = "Booking payment overdue")]
        BOOKING_OVERDUE = 5,

        /// <summary>
        /// 40% booking payment received
        /// Order confirmed, partner can start preparation
        /// Next: PRE_EVENT_PENDING (auto-triggered by guest lock date)
        /// </summary>
        [Display(Name = "Booking Paid", Description = "40% paid, order confirmed")]
        BOOKING_PAID = 6,

        /// <summary>
        /// Guest lock date reached (5 days before event)
        /// Triggers: PRE_EVENT invoice generation (35%)
        /// Guest count is now LOCKED
        /// Next: PRE_EVENT_PAID
        /// </summary>
        [Display(Name = "Pre-Event Payment Pending", Description = "Awaiting 35% pre-event payment")]
        PRE_EVENT_PENDING = 7,

        /// <summary>
        /// PRE_EVENT invoice overdue
        /// Critical: Event cannot start if not paid
        /// Next: PRE_EVENT_PAID or CANCELLED
        /// </summary>
        [Display(Name = "Pre-Event Overdue", Description = "Pre-event payment overdue")]
        PRE_EVENT_OVERDUE = 8,

        /// <summary>
        /// 75% total paid (40% + 35%)
        /// Payment Gate PASSED: Event can now start
        /// Partner can dispatch, supervisor can be assigned
        /// Next: PREPARING or EVENT_IN_PROGRESS
        /// </summary>
        [Display(Name = "Pre-Event Paid", Description = "75% paid, ready for event")]
        PRE_EVENT_PAID = 9,

        /// <summary>
        /// Partner preparing for event
        /// Menu locked (3 days before event)
        /// Supervisor assigned
        /// Next: DISPATCHED
        /// </summary>
        [Display(Name = "Preparing", Description = "Partner preparing for event")]
        PREPARING = 10,

        /// <summary>
        /// Items dispatched to event location
        /// Supervisor tracking delivery
        /// Next: ARRIVED
        /// </summary>
        [Display(Name = "Dispatched", Description = "Items dispatched to venue")]
        DISPATCHED = 11,

        /// <summary>
        /// Items arrived at event location
        /// Supervisor confirmed arrival
        /// Next: EVENT_IN_PROGRESS
        /// </summary>
        [Display(Name = "Arrived", Description = "Items arrived at venue")]
        ARRIVED = 12,

        /// <summary>
        /// Event currently in progress
        /// Supervisor on-site, tracking actual execution
        /// Recording: actual guest count, service quality, extra items
        /// Next: EVENT_COMPLETED
        /// </summary>
        [Display(Name = "Event In Progress", Description = "Event currently happening")]
        EVENT_IN_PROGRESS = 13,

        /// <summary>
        /// Event finished, supervisor submitted report
        /// Triggers: FINAL invoice generation (25% + extras)
        /// Extra charges calculated (extra guests, add-ons, overtime)
        /// Next: FINAL_PENDING (if balance > 0) or ORDER_COMPLETED (if no balance)
        /// </summary>
        [Display(Name = "Event Completed", Description = "Event finished successfully")]
        EVENT_COMPLETED = 14,

        /// <summary>
        /// FINAL invoice generated, awaiting payment
        /// Includes: 25% base + extra charges
        /// Next: FINAL_PAID or FINAL_OVERDUE
        /// </summary>
        [Display(Name = "Final Payment Pending", Description = "Awaiting final settlement")]
        FINAL_PENDING = 15,

        /// <summary>
        /// Final invoice overdue
        /// Collections process initiated
        /// Next: FINAL_PAID
        /// </summary>
        [Display(Name = "Final Payment Overdue", Description = "Final payment overdue")]
        FINAL_OVERDUE = 16,

        /// <summary>
        /// All payments received (100%)
        /// Order financially complete
        /// Next: ORDER_COMPLETED
        /// </summary>
        [Display(Name = "Fully Paid", Description = "100% payment received")]
        FULLY_PAID = 17,

        /// <summary>
        /// Order completely finished
        /// All payments received, event executed, no pending actions
        /// Terminal state (no next status)
        /// </summary>
        [Display(Name = "Completed", Description = "Order fully completed")]
        ORDER_COMPLETED = 18,

        /// <summary>
        /// Order cancelled (by customer, partner, or system)
        /// Refund processing if applicable
        /// Terminal state (no next status)
        /// </summary>
        [Display(Name = "Cancelled", Description = "Order cancelled")]
        CANCELLED = 19,

        /// <summary>
        /// Order rejected by partner
        /// Refund processed if any payment made
        /// Terminal state (no next status)
        /// </summary>
        [Display(Name = "Rejected", Description = "Order rejected by partner")]
        REJECTED = 20
    }

    /// <summary>
    /// Payment gate validation result
    /// Used to determine if order can proceed to next status
    /// </summary>
    public enum PaymentGateStatus
    {
        /// <summary>
        /// Payment gate passed, can proceed
        /// </summary>
        [Display(Name = "Passed", Description = "Payment requirement met")]
        PASSED = 1,

        /// <summary>
        /// Payment gate failed, cannot proceed
        /// </summary>
        [Display(Name = "Failed", Description = "Payment requirement not met")]
        FAILED = 2,

        /// <summary>
        /// Payment gate not applicable for this status
        /// </summary>
        [Display(Name = "Not Applicable", Description = "No payment gate for this status")]
        NOT_APPLICABLE = 3
    }

    /// <summary>
    /// Lock type enumeration
    /// Defines what gets locked and when
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// Guest count lock (5 days before event)
        /// After lock: changes result in extra charges
        /// </summary>
        [Display(Name = "Guest Count Lock", Description = "Guest count finalized")]
        GUEST_COUNT = 1,

        /// <summary>
        /// Menu lock (3 days before event)
        /// After lock: changes result in add-on charges
        /// </summary>
        [Display(Name = "Menu Lock", Description = "Menu finalized")]
        MENU = 2,

        /// <summary>
        /// Both guest count and menu locked
        /// </summary>
        [Display(Name = "Full Lock", Description = "All items locked")]
        FULL = 3
    }

    /// <summary>
    /// Business rule violation type
    /// Used for validation and error reporting
    /// </summary>
    public enum BusinessRuleViolation
    {
        /// <summary>
        /// Insufficient payment to proceed
        /// </summary>
        [Display(Name = "Insufficient Payment", Description = "Payment requirement not met")]
        INSUFFICIENT_PAYMENT = 1,

        /// <summary>
        /// Invalid status transition
        /// </summary>
        [Display(Name = "Invalid Status Transition", Description = "Cannot move to requested status")]
        INVALID_STATUS_TRANSITION = 2,

        /// <summary>
        /// Trying to modify locked data
        /// </summary>
        [Display(Name = "Data Locked", Description = "Cannot modify locked data")]
        DATA_LOCKED = 3,

        /// <summary>
        /// Event date in the past
        /// </summary>
        [Display(Name = "Event Date Past", Description = "Event date has passed")]
        EVENT_DATE_PAST = 4,

        /// <summary>
        /// Supervisor not assigned
        /// </summary>
        [Display(Name = "No Supervisor", Description = "Supervisor must be assigned")]
        NO_SUPERVISOR = 5,

        /// <summary>
        /// Invoice not generated
        /// </summary>
        [Display(Name = "Invoice Missing", Description = "Required invoice not generated")]
        INVOICE_MISSING = 6,

        /// <summary>
        /// Prerequisite status not reached
        /// </summary>
        [Display(Name = "Prerequisite Not Met", Description = "Previous step not completed")]
        PREREQUISITE_NOT_MET = 7
    }

    /// <summary>
    /// State transition trigger type
    /// Defines what causes a status change
    /// </summary>
    public enum StateTransitionTrigger
    {
        /// <summary>
        /// Manual action by user/admin/partner
        /// </summary>
        [Display(Name = "Manual", Description = "User initiated")]
        MANUAL = 1,

        /// <summary>
        /// Automatic trigger by system/job
        /// </summary>
        [Display(Name = "Automatic", Description = "System triggered")]
        AUTOMATIC = 2,

        /// <summary>
        /// Triggered by payment success
        /// </summary>
        [Display(Name = "Payment", Description = "Payment received")]
        PAYMENT = 3,

        /// <summary>
        /// Triggered by date/time
        /// </summary>
        [Display(Name = "Scheduled", Description = "Date/time reached")]
        SCHEDULED = 4,

        /// <summary>
        /// Triggered by supervisor action
        /// </summary>
        [Display(Name = "Supervisor", Description = "Supervisor action")]
        SUPERVISOR = 5
    }
}
