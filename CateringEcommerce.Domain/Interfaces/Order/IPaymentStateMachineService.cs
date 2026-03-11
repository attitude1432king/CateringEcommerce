using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces.Order
{
    /// <summary>
    /// Payment state machine service interface
    /// Enforces payment-aware order status workflow and business rules
    /// </summary>
    public interface IPaymentStateMachineService
    {
        /// <summary>
        /// Validates if a state transition is allowed
        /// Enforces sequential, non-reversible status progression
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="currentStatus">Current order status</param>
        /// <param name="targetStatus">Desired target status</param>
        /// <returns>Validation result with allowed flag and error details</returns>
        Task<StateTransitionValidationResult> ValidateStateTransitionAsync(
            long orderId,
            OrderStatus currentStatus,
            OrderStatus targetStatus);

        /// <summary>
        /// Checks if payment gate is passed for current status
        /// Validates that required payment percentage has been received
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="currentStatus">Current order status</param>
        /// <returns>Payment gate validation result</returns>
        Task<PaymentGateValidationResult> ValidatePaymentGateAsync(
            long orderId,
            OrderStatus currentStatus);

        /// <summary>
        /// Checks if event can start (75% rule)
        /// Event CANNOT start unless PRE_EVENT_PAID (40% + 35% = 75% total)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>True if 75% paid and event can start</returns>
        Task<bool> CanStartEventAsync(long orderId);

        /// <summary>
        /// Enforces business rules for order operations
        /// Validates guest count locks, menu locks, date constraints
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="operation">Operation type (modify guest, modify menu, etc.)</param>
        /// <returns>Business rule validation result</returns>
        Task<BusinessRuleValidationResult> EnforceBusinessRulesAsync(
            long orderId,
            string operation);

        /// <summary>
        /// Auto-locks guest count 5 days before event
        /// Triggered by background job or manual check
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Lock result with success flag</returns>
        Task<LockResult> AutoLockGuestCountAsync(long orderId);

        /// <summary>
        /// Auto-locks menu 3 days before event
        /// Triggered by background job or manual check
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Lock result with success flag</returns>
        Task<LockResult> AutoLockMenuAsync(long orderId);

        /// <summary>
        /// Checks if guest count is locked
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>True if guest count is locked</returns>
        Task<bool> IsGuestCountLockedAsync(long orderId);

        /// <summary>
        /// Checks if menu is locked
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>True if menu is locked</returns>
        Task<bool> IsMenuLockedAsync(long orderId);

        /// <summary>
        /// Gets next allowed statuses from current status
        /// Based on payment gates and business rules
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="currentStatus">Current order status</param>
        /// <returns>List of allowed next statuses</returns>
        Task<List<OrderStatus>> GetAllowedNextStatusesAsync(
            long orderId,
            OrderStatus currentStatus);

        /// <summary>
        /// Gets payment stage requirement for status transition
        /// Returns required payment percentage to move to target status
        /// </summary>
        /// <param name="targetStatus">Target status</param>
        /// <returns>Required payment percentage (0-100)</returns>
        decimal GetRequiredPaymentPercentage(OrderStatus targetStatus);

        /// <summary>
        /// Triggers automatic state transition based on payment
        /// Called after successful payment to advance order status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="paymentStage">Payment stage that was completed</param>
        /// <returns>New order status after transition</returns>
        Task<OrderStatus> TriggerPaymentBasedTransitionAsync(
            long orderId,
            string paymentStage);

        /// <summary>
        /// Marks invoice as overdue and updates order status
        /// Called by background job for overdue invoices
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>True if status updated to overdue</returns>
        Task<bool> MarkInvoiceOverdueAsync(long invoiceId);

        /// <summary>
        /// Calculates days until guest lock for an order
        /// Returns negative if already locked
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Days until guest count lock (negative if locked)</returns>
        Task<int> GetDaysUntilGuestLockAsync(long orderId);

        /// <summary>
        /// Calculates days until menu lock for an order
        /// Returns negative if already locked
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Days until menu lock (negative if locked)</returns>
        Task<int> GetDaysUntilMenuLockAsync(long orderId);
    }

    /// <summary>
    /// State transition validation result
    /// </summary>
    public class StateTransitionValidationResult
    {
        public bool IsAllowed { get; set; }
        public string ErrorMessage { get; set; }
        public BusinessRuleViolation? ViolationType { get; set; }
        public Dictionary<string, object> ValidationDetails { get; set; }
    }

    /// <summary>
    /// Payment gate validation result
    /// </summary>
    public class PaymentGateValidationResult
    {
        public PaymentGateStatus Status { get; set; }
        public decimal RequiredPercentage { get; set; }
        public decimal CurrentPercentage { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRequired { get; set; }
        public decimal BalanceDue { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Business rule validation result
    /// </summary>
    public class BusinessRuleValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public BusinessRuleViolation? ViolationType { get; set; }
        public Dictionary<string, object> ValidationDetails { get; set; }
    }

    /// <summary>
    /// Lock operation result
    /// </summary>
    public class LockResult
    {
        public bool Success { get; set; }
        public LockType LockType { get; set; }
        public DateTime LockedAt { get; set; }
        public string Message { get; set; }
        public bool WasAlreadyLocked { get; set; }
    }
}
