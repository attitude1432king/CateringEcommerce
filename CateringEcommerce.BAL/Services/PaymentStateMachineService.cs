using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using CateringEcommerce.BAL.Configuration;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Payment state machine service implementation
    /// Enforces payment-aware order status workflow and business rules
    /// Implements strict sequential status progression with payment gates
    /// </summary>
    public class PaymentStateMachineService : IPaymentStateMachineService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly IInvoiceRepository _invoiceRepository;

        public PaymentStateMachineService(
            IDatabaseHelper dbHelper,
            IInvoiceRepository invoiceRepository)
        {
            _dbHelper = dbHelper;
            _invoiceRepository = invoiceRepository;
        }

        #region State Transition Validation

        /// <summary>
        /// Validates if a state transition is allowed
        /// Enforces sequential, non-reversible status progression
        /// </summary>
        public async Task<StateTransitionValidationResult> ValidateStateTransitionAsync(
            long orderId,
            OrderStatus currentStatus,
            OrderStatus targetStatus)
        {
            var result = new StateTransitionValidationResult
            {
                ValidationDetails = new Dictionary<string, object>()
            };

            // Check if trying to go backwards (non-reversible)
            if ((int)targetStatus < (int)currentStatus &&
                targetStatus != OrderStatus.CANCELLED &&
                targetStatus != OrderStatus.REJECTED)
            {
                result.IsAllowed = false;
                result.ErrorMessage = "Cannot move backwards in order status. Status progression is sequential and non-reversible.";
                result.ViolationType = BusinessRuleViolation.INVALID_STATUS_TRANSITION;
                return result;
            }

            // Check if transition is valid based on state machine rules
            var allowedNextStatuses = await GetAllowedNextStatusesAsync(orderId, currentStatus);
            if (!allowedNextStatuses.Contains(targetStatus))
            {
                result.IsAllowed = false;
                result.ErrorMessage = $"Invalid status transition from {currentStatus} to {targetStatus}. Allowed: {string.Join(", ", allowedNextStatuses)}";
                result.ViolationType = BusinessRuleViolation.INVALID_STATUS_TRANSITION;
                result.ValidationDetails["AllowedStatuses"] = allowedNextStatuses;
                return result;
            }

            // Check payment gate if moving to payment-dependent status
            var paymentGate = await ValidatePaymentGateAsync(orderId, targetStatus);
            if (paymentGate.Status == PaymentGateStatus.FAILED)
            {
                result.IsAllowed = false;
                result.ErrorMessage = paymentGate.ErrorMessage;
                result.ViolationType = BusinessRuleViolation.INSUFFICIENT_PAYMENT;
                result.ValidationDetails["PaymentRequired"] = paymentGate.AmountRequired;
                result.ValidationDetails["PaymentReceived"] = paymentGate.AmountPaid;
                result.ValidationDetails["BalanceDue"] = paymentGate.BalanceDue;
                return result;
            }

            // Check business rules (supervisor assignment, invoice generation, etc.)
            var businessRuleCheck = await EnforceBusinessRulesAsync(orderId, $"TRANSITION_TO_{targetStatus}");
            if (!businessRuleCheck.IsValid)
            {
                result.IsAllowed = false;
                result.ErrorMessage = businessRuleCheck.ErrorMessage;
                result.ViolationType = businessRuleCheck.ViolationType;
                result.ValidationDetails = businessRuleCheck.ValidationDetails;
                return result;
            }

            result.IsAllowed = true;
            result.ErrorMessage = "Transition allowed";
            return result;
        }

        /// <summary>
        /// Gets next allowed statuses from current status
        /// Based on payment gates and business rules
        /// </summary>
        public async Task<List<OrderStatus>> GetAllowedNextStatusesAsync(
            long orderId,
            OrderStatus currentStatus)
        {
            var allowed = new List<OrderStatus>();

            // Can always cancel (with refund processing)
            allowed.Add(OrderStatus.CANCELLED);

            switch (currentStatus)
            {
                case OrderStatus.ORDER_CREATED:
                    allowed.Add(OrderStatus.PARTNER_REVIEW);
                    break;

                case OrderStatus.PARTNER_REVIEW:
                    allowed.Add(OrderStatus.ORDER_APPROVED);
                    allowed.Add(OrderStatus.REJECTED);
                    break;

                case OrderStatus.ORDER_APPROVED:
                    allowed.Add(OrderStatus.BOOKING_PENDING);
                    break;

                case OrderStatus.BOOKING_PENDING:
                    allowed.Add(OrderStatus.BOOKING_PAID);
                    allowed.Add(OrderStatus.BOOKING_OVERDUE);
                    break;

                case OrderStatus.BOOKING_OVERDUE:
                    allowed.Add(OrderStatus.BOOKING_PAID);
                    break;

                case OrderStatus.BOOKING_PAID:
                    // Auto-triggered when guest lock date reached
                    allowed.Add(OrderStatus.PRE_EVENT_PENDING);
                    break;

                case OrderStatus.PRE_EVENT_PENDING:
                    allowed.Add(OrderStatus.PRE_EVENT_PAID);
                    allowed.Add(OrderStatus.PRE_EVENT_OVERDUE);
                    break;

                case OrderStatus.PRE_EVENT_OVERDUE:
                    allowed.Add(OrderStatus.PRE_EVENT_PAID);
                    break;

                case OrderStatus.PRE_EVENT_PAID:
                    // Event can now start (75% paid)
                    allowed.Add(OrderStatus.PREPARING);
                    allowed.Add(OrderStatus.EVENT_IN_PROGRESS);
                    break;

                case OrderStatus.PREPARING:
                    allowed.Add(OrderStatus.DISPATCHED);
                    break;

                case OrderStatus.DISPATCHED:
                    allowed.Add(OrderStatus.ARRIVED);
                    break;

                case OrderStatus.ARRIVED:
                    allowed.Add(OrderStatus.EVENT_IN_PROGRESS);
                    break;

                case OrderStatus.EVENT_IN_PROGRESS:
                    allowed.Add(OrderStatus.EVENT_COMPLETED);
                    break;

                case OrderStatus.EVENT_COMPLETED:
                    // Auto-generate final invoice
                    allowed.Add(OrderStatus.FINAL_PENDING);
                    allowed.Add(OrderStatus.ORDER_COMPLETED); // If no balance due
                    break;

                case OrderStatus.FINAL_PENDING:
                    allowed.Add(OrderStatus.FULLY_PAID);
                    allowed.Add(OrderStatus.FINAL_OVERDUE);
                    break;

                case OrderStatus.FINAL_OVERDUE:
                    allowed.Add(OrderStatus.FULLY_PAID);
                    break;

                case OrderStatus.FULLY_PAID:
                    allowed.Add(OrderStatus.ORDER_COMPLETED);
                    break;

                case OrderStatus.ORDER_COMPLETED:
                case OrderStatus.CANCELLED:
                case OrderStatus.REJECTED:
                    // Terminal states - no transitions
                    allowed.Clear();
                    break;
            }

            return allowed;
        }

        #endregion

        #region Payment Gate Validation

        /// <summary>
        /// Checks if payment gate is passed for current status
        /// Validates that required payment percentage has been received
        /// </summary>
        public async Task<PaymentGateValidationResult> ValidatePaymentGateAsync(
            long orderId,
            OrderStatus targetStatus)
        {
            var result = new PaymentGateValidationResult();
            var requiredPercentage = GetRequiredPaymentPercentage(targetStatus);

            if (requiredPercentage == 0)
            {
                result.Status = PaymentGateStatus.NOT_APPLICABLE;
                return result;
            }

            // Get order total and payment summary
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var paymentData = await _dbHelper.ExecuteQueryFirstAsync<PaymentSummary>(
                $"SELECT c_order_total, c_total_paid_percentage FROM {Table.SysOrders} WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            if (paymentData == null)
            {
                result.Status = PaymentGateStatus.FAILED;
                result.ErrorMessage = "Order not found";
                return result;
            }

            result.RequiredPercentage = requiredPercentage;
            result.CurrentPercentage = paymentData.TotalPaidPercentage;
            result.AmountRequired = paymentData.OrderTotal * requiredPercentage / 100;
            result.AmountPaid = paymentData.OrderTotal * paymentData.TotalPaidPercentage / 100;
            result.BalanceDue = result.AmountRequired - result.AmountPaid;

            if (result.CurrentPercentage >= requiredPercentage)
            {
                result.Status = PaymentGateStatus.PASSED;
            }
            else
            {
                result.Status = PaymentGateStatus.FAILED;
                result.ErrorMessage = $"Insufficient payment. Required: {requiredPercentage}%, Current: {result.CurrentPercentage}%. Balance due: ₹{result.BalanceDue:N2}";
            }

            return result;
        }

        /// <summary>
        /// Gets payment stage requirement for status transition
        /// Returns required payment percentage to move to target status
        /// </summary>
        public decimal GetRequiredPaymentPercentage(OrderStatus targetStatus)
        {
            return targetStatus switch
            {
                // Booking payment required (40%)
                OrderStatus.BOOKING_PAID => 40m,

                // Pre-event payment required (75% total = 40% + 35%)
                OrderStatus.PRE_EVENT_PAID => 75m,
                OrderStatus.PREPARING => 75m,
                OrderStatus.DISPATCHED => 75m,
                OrderStatus.ARRIVED => 75m,
                OrderStatus.EVENT_IN_PROGRESS => 75m,

                // Full payment required (100%)
                OrderStatus.FULLY_PAID => 100m,
                OrderStatus.ORDER_COMPLETED => 100m,

                // No payment requirement for these statuses
                _ => 0m
            };
        }

        /// <summary>
        /// Checks if event can start (75% rule)
        /// Event CANNOT start unless PRE_EVENT_PAID (40% + 35% = 75% total)
        /// </summary>
        public async Task<bool> CanStartEventAsync(long orderId)
        {
            var paymentGate = await ValidatePaymentGateAsync(orderId, OrderStatus.PRE_EVENT_PAID);
            return paymentGate.Status == PaymentGateStatus.PASSED;
        }

        #endregion

        #region Business Rule Enforcement

        /// <summary>
        /// Enforces business rules for order operations
        /// Validates guest count locks, menu locks, date constraints
        /// </summary>
        public async Task<BusinessRuleValidationResult> EnforceBusinessRulesAsync(
            long orderId,
            string operation)
        {
            var result = new BusinessRuleValidationResult
            {
                ValidationDetails = new Dictionary<string, object>()
            };

            // Get order details
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var order = await _dbHelper.ExecuteQueryFirstAsync<OrderLockInfo>(
                $@"SELECT c_event_date, c_guest_count_locked, c_menu_locked,
                         c_guest_lock_date, c_menu_lock_date, c_order_status
                  FROM {Table.SysOrders}
                  WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            if (order == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Order not found";
                return result;
            }

            // Check if event date is in the past
            if (order.EventDate < DateTime.Now.Date)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot modify order - event date has passed";
                result.ViolationType = BusinessRuleViolation.EVENT_DATE_PAST;
                return result;
            }

            // Operation-specific validations
            switch (operation.ToUpper())
            {
                case "MODIFY_GUEST_COUNT":
                    if (order.GuestCountLocked)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Guest count is locked (locked on {order.GuestLockDate:yyyy-MM-dd}). Changes will result in extra charges.";
                        result.ViolationType = BusinessRuleViolation.DATA_LOCKED;
                        result.ValidationDetails["LockedDate"] = order.GuestLockDate;
                        return result;
                    }
                    break;

                case "MODIFY_MENU":
                    if (order.MenuLocked)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Menu is locked (locked on {order.MenuLockDate:yyyy-MM-dd}). Changes will result in add-on charges.";
                        result.ViolationType = BusinessRuleViolation.DATA_LOCKED;
                        result.ValidationDetails["LockedDate"] = order.MenuLockDate;
                        return result;
                    }
                    break;

                case string s when s.StartsWith("TRANSITION_TO_"):
                    var targetStatusString = s.Replace("TRANSITION_TO_", "");
                    if (Enum.TryParse<OrderStatus>(targetStatusString, out var targetStatus))
                    {
                        // Check supervisor assignment for event execution statuses
                        if (targetStatus >= OrderStatus.DISPATCHED && targetStatus <= OrderStatus.EVENT_COMPLETED)
                        {
                            var hasSupervisor = await CheckSupervisorAssignedAsync(orderId);
                            if (!hasSupervisor)
                            {
                                result.IsValid = false;
                                result.ErrorMessage = "Supervisor must be assigned before event can be dispatched";
                                result.ViolationType = BusinessRuleViolation.NO_SUPERVISOR;
                                return result;
                            }
                        }

                        // Check invoice generation for payment-dependent statuses
                        if (targetStatus == OrderStatus.BOOKING_PENDING ||
                            targetStatus == OrderStatus.PRE_EVENT_PENDING ||
                            targetStatus == OrderStatus.FINAL_PENDING)
                        {
                            var invoiceType = targetStatus switch
                            {
                                OrderStatus.BOOKING_PENDING => InvoiceType.BOOKING,
                                OrderStatus.PRE_EVENT_PENDING => InvoiceType.PRE_EVENT,
                                OrderStatus.FINAL_PENDING => InvoiceType.FINAL,
                                _ => InvoiceType.BOOKING
                            };

                            var hasInvoice = await CheckInvoiceGeneratedAsync(orderId, invoiceType);
                            if (!hasInvoice)
                            {
                                result.IsValid = false;
                                result.ErrorMessage = $"{invoiceType} invoice must be generated before moving to {targetStatus}";
                                result.ViolationType = BusinessRuleViolation.INVOICE_MISSING;
                                return result;
                            }
                        }
                    }
                    break;
            }

            result.IsValid = true;
            return result;
        }

        private async Task<bool> CheckSupervisorAssignedAsync(long orderId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var count = await _dbHelper.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM {Table.SysSupervisorAssignment} WHERE c_orderid = @OrderId AND c_status = 'ASSIGNED'",
                parameters,
                CommandType.Text);

            return count > 0;
        }

        private async Task<bool> CheckInvoiceGeneratedAsync(long orderId, InvoiceType invoiceType)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@InvoiceType", (int)invoiceType)
            };

            var count = await _dbHelper.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM {Table.SysInvoice} WHERE c_orderid = @OrderId AND c_invoice_type = @InvoiceType",
                parameters,
                CommandType.Text);

            return count > 0;
        }

        #endregion

        #region Auto-Lock Mechanisms

        /// <summary>
        /// Auto-locks guest count 5 days before event
        /// Triggered by background job or manual check
        /// </summary>
        public async Task<LockResult> AutoLockGuestCountAsync(long orderId)
        {
            var result = new LockResult { LockType = LockType.GUEST_COUNT };

            // Check if already locked
            var isLocked = await IsGuestCountLockedAsync(orderId);
            if (isLocked)
            {
                result.Success = true;
                result.WasAlreadyLocked = true;
                result.Message = "Guest count was already locked";
                return result;
            }

            // Lock guest count
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@LockDate", DateTime.Now),
                new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteNonQueryAsync(
                $@"UPDATE {Table.SysOrders}
                  SET c_guest_count_locked = 1,
                      c_guest_lock_date = @LockDate,
                      c_last_modified = GETDATE()
                  WHERE c_orderid = @OrderId AND c_guest_count_locked = 0;

                  SET @Success = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;",
                parameters,
                CommandType.Text);

            result.Success = parameters[2].Value != DBNull.Value && (bool)parameters[2].Value;
            result.LockedAt = DateTime.Now;
            result.Message = result.Success ? "Guest count locked successfully" : "Failed to lock guest count";

            return result;
        }

        /// <summary>
        /// Auto-locks menu 3 days before event
        /// Triggered by background job or manual check
        /// </summary>
        public async Task<LockResult> AutoLockMenuAsync(long orderId)
        {
            var result = new LockResult { LockType = LockType.MENU };

            // Check if already locked
            var isLocked = await IsMenuLockedAsync(orderId);
            if (isLocked)
            {
                result.Success = true;
                result.WasAlreadyLocked = true;
                result.Message = "Menu was already locked";
                return result;
            }

            // Lock menu
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@LockDate", DateTime.Now),
                new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteNonQueryAsync(
                $@"UPDATE {Table.SysOrders}
                  SET c_menu_locked = 1,
                      c_menu_lock_date = @LockDate,
                      c_last_modified = GETDATE()
                  WHERE c_orderid = @OrderId AND c_menu_locked = 0;

                  SET @Success = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;",
                parameters,
                CommandType.Text);

            result.Success = parameters[2].Value != DBNull.Value && (bool)parameters[2].Value;
            result.LockedAt = DateTime.Now;
            result.Message = result.Success ? "Menu locked successfully" : "Failed to lock menu";

            return result;
        }

        /// <summary>
        /// Checks if guest count is locked
        /// </summary>
        public async Task<bool> IsGuestCountLockedAsync(long orderId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var locked = await _dbHelper.ExecuteScalarAsync<bool>(
                $"SELECT ISNULL(c_guest_count_locked, 0) FROM {Table.SysOrders} WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            return locked;
        }

        /// <summary>
        /// Checks if menu is locked
        /// </summary>
        public async Task<bool> IsMenuLockedAsync(long orderId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var locked = await _dbHelper.ExecuteScalarAsync<bool>(
                $"SELECT ISNULL(c_menu_locked, 0) FROM {Table.SysOrders} WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            return locked;
        }

        /// <summary>
        /// Calculates days until guest lock for an order
        /// Returns negative if already locked
        /// </summary>
        public async Task<int> GetDaysUntilGuestLockAsync(long orderId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var order = await _dbHelper.ExecuteQueryFirstAsync<dynamic>(
                $@"SELECT c_event_date, c_guest_count_locked
                  FROM {Table.SysOrders}
                  WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            if (order == null) return 0;

            if (order.c_guest_count_locked)
                return -999; // Already locked

            var guestLockDate = ((DateTime)order.c_event_date).AddDays(-5);
            return (guestLockDate.Date - DateTime.Now.Date).Days;
        }

        /// <summary>
        /// Calculates days until menu lock for an order
        /// Returns negative if already locked
        /// </summary>
        public async Task<int> GetDaysUntilMenuLockAsync(long orderId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var order = await _dbHelper.ExecuteQueryFirstAsync<dynamic>(
                $@"SELECT c_event_date, c_menu_locked
                  FROM {Table.SysOrders}
                  WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);

            if (order == null) return 0;

            if (order.c_menu_locked)
                return -999; // Already locked

            var menuLockDate = ((DateTime)order.c_event_date).AddDays(-3);
            return (menuLockDate.Date - DateTime.Now.Date).Days;
        }

        #endregion

        #region Payment-Based Transitions

        /// <summary>
        /// Triggers automatic state transition based on payment
        /// Called after successful payment to advance order status
        /// USES TRANSACTION for atomic update
        /// </summary>
        public async Task<OrderStatus> TriggerPaymentBasedTransitionAsync(
            long orderId,
            string paymentStage)
        {
            // TRANSACTION FIX: Wrap in transaction to ensure atomicity
            await using var transaction = await _dbHelper.BeginTransactionAsync();
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                var currentStatus = await transaction.ExecuteScalarAsync<int>(
                    $"SELECT c_order_status FROM {Table.SysOrders} WHERE c_orderid = @OrderId",
                    parameters,
                    CommandType.Text);

                var newStatus = paymentStage.ToUpper() switch
                {
                    "BOOKING" => OrderStatus.BOOKING_PAID,
                    "PRE_EVENT" => OrderStatus.PRE_EVENT_PAID,
                    "FINAL" => OrderStatus.FULLY_PAID,
                    _ => (OrderStatus)currentStatus
                };

                // Update order status within transaction
                var updateParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@NewStatus", (int)newStatus)
                };

                await transaction.ExecuteNonQueryAsync(
                    $@"UPDATE {Table.SysOrders}
                      SET c_order_status = @NewStatus,
                          c_last_modified = GETDATE()
                      WHERE c_orderid = @OrderId",
                    updateParams,
                    CommandType.Text);

                // Commit transaction - both operations succeed or both fail
                await transaction.CommitAsync();

                return newStatus;
            }
            catch
            {
                // Transaction automatically rolls back on exception (via Dispose)
                throw;
            }
        }

        /// <summary>
        /// Marks invoice as overdue and updates order status
        /// Called by background job for overdue invoices
        /// USES TRANSACTION for atomic update
        /// </summary>
        public async Task<bool> MarkInvoiceOverdueAsync(long invoiceId)
        {
            // TRANSACTION FIX: Wrap in transaction to ensure invoice and order status stay in sync
            await using var transaction = await _dbHelper.BeginTransactionAsync();
            try
            {
                // Get invoice details
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null || invoice.Status != InvoiceStatus.UNPAID)
                    return false;

                // Update invoice status to OVERDUE (within transaction)
                // NOTE: This still uses the repository method which uses its own connection
                // For full transactional integrity, UpdateInvoiceStatusAsync should accept a transaction parameter
                // TODO: Refactor invoice repository methods to support transactions
                await _invoiceRepository.UpdateInvoiceStatusAsync(invoiceId, InvoiceStatus.OVERDUE);

                // Update order status based on invoice type (within transaction)
                var overdueStatus = invoice.InvoiceType switch
                {
                    InvoiceType.BOOKING => OrderStatus.BOOKING_OVERDUE,
                    InvoiceType.PRE_EVENT => OrderStatus.PRE_EVENT_OVERDUE,
                    InvoiceType.FINAL => OrderStatus.FINAL_OVERDUE,
                    _ => (OrderStatus?)null
                };

                if (overdueStatus.HasValue)
                {
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@OrderId", invoice.OrderId),
                        new SqlParameter("@Status", (int)overdueStatus.Value)
                    };

                    await transaction.ExecuteNonQueryAsync(
                        $@"UPDATE {Table.SysOrders}
                          SET c_order_status = @Status,
                              c_last_modified = GETDATE()
                          WHERE c_orderid = @OrderId",
                        parameters,
                        CommandType.Text);
                }

                // Commit transaction - both invoice and order status updates succeed together
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                // Transaction automatically rolls back on exception (via Dispose)
                return false;
            }
        }

        #endregion

        #region Helper Classes

        private class PaymentSummary
        {
            public decimal OrderTotal { get; set; }
            public decimal TotalPaidPercentage { get; set; }
        }

        private class OrderLockInfo
        {
            public DateTime EventDate { get; set; }
            public bool GuestCountLocked { get; set; }
            public bool MenuLocked { get; set; }
            public DateTime? GuestLockDate { get; set; }
            public DateTime? MenuLockDate { get; set; }
            public int OrderStatus { get; set; }
        }

        #endregion
    }
}
