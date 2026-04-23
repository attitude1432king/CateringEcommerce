using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Invoice;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using CateringEcommerce.BAL.Configuration;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Invoice automation service
    /// Handles automatic invoice generation based on order lifecycle events
    /// </summary>
    public class InvoiceAutomationService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentStateMachineService _stateMachine;
        private readonly IDatabaseHelper _dbHelper;
        private readonly ILogger<InvoiceAutomationService> _logger;
        private readonly InvoiceNotificationService _notificationService;

        public InvoiceAutomationService(
            IInvoiceRepository invoiceRepository,
            IPaymentStateMachineService stateMachine,
            IDatabaseHelper dbHelper,
            ILogger<InvoiceAutomationService> logger,
            InvoiceNotificationService notificationService)
        {
            _invoiceRepository = invoiceRepository;
            _stateMachine = stateMachine;
            _dbHelper = dbHelper;
            _logger = logger;
            _notificationService = notificationService;
        }

        #region Auto-Generate Invoice Triggers

        /// <summary>
        /// Auto-generate BOOKING invoice when order is approved by partner
        /// Triggered: Partner approves order (ORDER_APPROVED → BOOKING_PENDING)
        /// </summary>
        public async Task<long> GenerateBookingInvoiceAsync(long orderId)
        {
            try
            {
                _logger.LogInformation("Auto-generating BOOKING invoice for Order {OrderId}", orderId);

                // Check if booking invoice already exists
                var existingInvoice = await _invoiceRepository.GetInvoiceByOrderAndTypeAsync(
                    orderId,
                    InvoiceType.BOOKING);

                if (existingInvoice != null)
                {
                    _logger.LogWarning("BOOKING invoice already exists for Order {OrderId}", orderId);
                    return existingInvoice.InvoiceId;
                }

                // Generate booking invoice (40%)
                var invoiceId = await _invoiceRepository.GenerateInvoiceAsync(new InvoiceGenerationRequestDto
                {
                    OrderId = orderId,
                    InvoiceType = InvoiceType.BOOKING,
                    ExtraGuestCharges = 0,
                    AddonCharges = 0
                });

                _logger.LogInformation(
                    "BOOKING invoice {InvoiceId} generated for Order {OrderId}",
                    invoiceId,
                    orderId);

                return invoiceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating BOOKING invoice for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Auto-generate PRE_EVENT invoice 5 days before event
        /// Triggered: Guest lock date reached OR background job
        /// </summary>
        public async Task<long> GeneratePreEventInvoiceAsync(long orderId)
        {
            try
            {
                _logger.LogInformation("Auto-generating PRE_EVENT invoice for Order {OrderId}", orderId);

                // Check if pre-event invoice already exists
                var existingInvoice = await _invoiceRepository.GetInvoiceByOrderAndTypeAsync(
                    orderId,
                    InvoiceType.PRE_EVENT);

                if (existingInvoice != null)
                {
                    _logger.LogWarning("PRE_EVENT invoice already exists for Order {OrderId}", orderId);
                    return existingInvoice.InvoiceId;
                }

                // Verify booking payment received (40%)
                var paymentProgress = await _invoiceRepository.GetPaymentProgressPercentageAsync(orderId);
                if (paymentProgress < 40m)
                {
                    throw new InvalidOperationException(
                        $"Cannot generate PRE_EVENT invoice. Booking payment not received (Current: {paymentProgress}%)");
                }

                // Generate pre-event invoice (35%)
                var invoiceId = await _invoiceRepository.GenerateInvoiceAsync(new InvoiceGenerationRequestDto
                {
                    OrderId = orderId,
                    InvoiceType = InvoiceType.PRE_EVENT,
                    ExtraGuestCharges = 0,
                    AddonCharges = 0
                });

                // Auto-lock guest count
                await _stateMachine.AutoLockGuestCountAsync(orderId);

                _logger.LogInformation(
                    "PRE_EVENT invoice {InvoiceId} generated for Order {OrderId}",
                    invoiceId,
                    orderId);

                return invoiceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PRE_EVENT invoice for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Auto-generate FINAL invoice after event completion
        /// Triggered: Supervisor marks event as completed
        /// </summary>
        public async Task<long> GenerateFinalInvoiceAsync(long orderId, decimal extraGuestCharges = 0, decimal addonCharges = 0)
        {
            try
            {
                _logger.LogInformation(
                    "Auto-generating FINAL invoice for Order {OrderId}, ExtraCharges: ₹{Extra}, Addons: ₹{Addons}",
                    orderId,
                    extraGuestCharges,
                    addonCharges);

                // Check if final invoice already exists
                var existingInvoice = await _invoiceRepository.GetInvoiceByOrderAndTypeAsync(
                    orderId,
                    InvoiceType.FINAL);

                if (existingInvoice != null)
                {
                    _logger.LogWarning("FINAL invoice already exists for Order {OrderId}", orderId);
                    return existingInvoice.InvoiceId;
                }

                // Verify pre-event payment received (75% total)
                var paymentProgress = await _invoiceRepository.GetPaymentProgressPercentageAsync(orderId);
                if (paymentProgress < 75m)
                {
                    throw new InvalidOperationException(
                        $"Cannot generate FINAL invoice. Pre-event payment not received (Current: {paymentProgress}%)");
                }

                // Generate final invoice (25% + extras)
                var invoiceId = await _invoiceRepository.GenerateInvoiceAsync(new InvoiceGenerationRequestDto
                {
                    OrderId = orderId,
                    InvoiceType = InvoiceType.FINAL,
                    ExtraGuestCharges = extraGuestCharges,
                    AddonCharges = addonCharges
                });

                _logger.LogInformation(
                    "FINAL invoice {InvoiceId} generated for Order {OrderId}",
                    invoiceId,
                    orderId);

                return invoiceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating FINAL invoice for Order {OrderId}", orderId);
                throw;
            }
        }

        #endregion

        #region Order Lifecycle Integration

        /// <summary>
        /// Handle order approval event
        /// Creates booking invoice and transitions order status
        /// </summary>
        public async Task<OrderApprovalResult> HandleOrderApprovalAsync(long orderId)
        {
            try
            {
                _logger.LogInformation("Handling order approval for Order {OrderId}", orderId);

                // Generate booking invoice
                var invoiceId = await GenerateBookingInvoiceAsync(orderId);

                // Update order status to BOOKING_PENDING
                await UpdateOrderStatusAsync(orderId, OrderStatus.BOOKING_PENDING);

                // Send notification to customer
                await _notificationService.SendBookingInvoiceNotificationAsync(orderId, invoiceId);

                return new OrderApprovalResult
                {
                    Success = true,
                    InvoiceId = invoiceId,
                    Message = "Order approved and booking invoice generated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling order approval for Order {OrderId}", orderId);
                return new OrderApprovalResult
                {
                    Success = false,
                    Message = $"Failed to approve order: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Handle guest lock date reached event
        /// Creates pre-event invoice and locks guest count
        /// </summary>
        public async Task<GuestLockResult> HandleGuestLockDateReachedAsync(long orderId)
        {
            try
            {
                _logger.LogInformation("Handling guest lock date for Order {OrderId}", orderId);

                // Check if already locked
                var isLocked = await _stateMachine.IsGuestCountLockedAsync(orderId);
                if (isLocked)
                {
                    return new GuestLockResult
                    {
                        Success = true,
                        Message = "Guest count already locked",
                        WasAlreadyLocked = true
                    };
                }

                // Generate pre-event invoice
                var invoiceId = await GeneratePreEventInvoiceAsync(orderId);

                // Update order status to PRE_EVENT_PENDING
                await UpdateOrderStatusAsync(orderId, OrderStatus.PRE_EVENT_PENDING);

                // Send notification to customer
                await _notificationService.SendPreEventInvoiceNotificationAsync(orderId, invoiceId);

                return new GuestLockResult
                {
                    Success = true,
                    InvoiceId = invoiceId,
                    Message = "Guest count locked and pre-event invoice generated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling guest lock for Order {OrderId}", orderId);
                return new GuestLockResult
                {
                    Success = false,
                    Message = $"Failed to lock guest count: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Handle event completion
        /// Creates final invoice with extra charges
        /// </summary>
        public async Task<EventCompletionResult> HandleEventCompletionAsync(
            long orderId,
            decimal extraGuestCharges,
            decimal addonCharges)
        {
            try
            {
                _logger.LogInformation("Handling event completion for Order {OrderId}", orderId);

                // Generate final invoice
                var invoiceId = await GenerateFinalInvoiceAsync(orderId, extraGuestCharges, addonCharges);

                // Get invoice details to check if payment needed
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                OrderStatus newStatus;
                if (invoice.TotalAmount > 0)
                {
                    // Has balance due - transition to FINAL_PENDING
                    newStatus = OrderStatus.FINAL_PENDING;
                }
                else
                {
                    // No balance (100% already paid) - complete order
                    newStatus = OrderStatus.ORDER_COMPLETED;
                }

                await UpdateOrderStatusAsync(orderId, newStatus);

                // Send notification to customer
                if (invoice.TotalAmount > 0)
                {
                    await _notificationService.SendFinalInvoiceNotificationAsync(orderId, invoiceId);
                }

                return new EventCompletionResult
                {
                    Success = true,
                    InvoiceId = invoiceId,
                    BalanceDue = invoice.BalanceDue,
                    Message = invoice.BalanceDue > 0
                        ? "Event completed. Final invoice generated with balance due."
                        : "Event completed. All payments received. Order complete!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event completion for Order {OrderId}", orderId);
                return new EventCompletionResult
                {
                    Success = false,
                    Message = $"Failed to complete event: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Handle payment success
        /// Links payment to invoice and transitions order status
        /// </summary>
        public async Task<PaymentSuccessResult> HandlePaymentSuccessAsync(
            long invoiceId,
            string razorpayPaymentId,
            decimal amountPaid)
        {
            try
            {
                _logger.LogInformation(
                    "Handling payment success for Invoice {InvoiceId}, Payment: {PaymentId}, Amount: ₹{Amount}",
                    invoiceId,
                    razorpayPaymentId,
                    amountPaid);

                // Link payment to invoice
                var linkSuccess = await _invoiceRepository.LinkPaymentToInvoiceAsync(new LinkPaymentToInvoiceDto
                {
                    InvoiceId = invoiceId,
                    RazorpayPaymentId = razorpayPaymentId,
                    RazorpayOrderId = null, // Can be added if needed
                    AmountPaid = amountPaid,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Razorpay"
                });

                if (!linkSuccess)
                {
                    throw new Exception("Failed to link payment to invoice");
                }

                // Get invoice to determine type
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                // Trigger payment-based state transition
                var paymentStage = invoice.InvoiceType switch
                {
                    InvoiceType.BOOKING => "BOOKING",
                    InvoiceType.PRE_EVENT => "PRE_EVENT",
                    InvoiceType.FINAL => "FINAL",
                    _ => throw new Exception("Unknown invoice type")
                };

                var newStatus = await _stateMachine.TriggerPaymentBasedTransitionAsync(
                    invoice.OrderId,
                    paymentStage);

                // Send payment receipt email
                await _notificationService.SendPaymentReceiptAsync(invoice.OrderId, invoiceId, razorpayPaymentId);

                return new PaymentSuccessResult
                {
                    Success = true,
                    InvoiceId = invoiceId,
                    NewOrderStatus = newStatus,
                    Message = $"Payment received successfully. Order status: {newStatus}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment success for Invoice {InvoiceId}", invoiceId);
                return new PaymentSuccessResult
                {
                    Success = false,
                    Message = $"Failed to process payment: {ex.Message}"
                };
            }
        }

        #endregion

        #region Helper Methods

        private async Task UpdateOrderStatusAsync(long orderId, OrderStatus status)
        {
            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@OrderId", orderId),
                new NpgsqlParameter("@Status", (int)status)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                $@"UPDATE {Table.SysOrders}
                  SET c_order_status = @Status,
                      c_last_modified = NOW()
                  WHERE c_orderid = @OrderId",
                parameters,
                CommandType.Text);
        }

        #endregion
    }

    #region Result Classes

    public class OrderApprovalResult
    {
        public bool Success { get; set; }
        public long InvoiceId { get; set; }
        public string Message { get; set; }
    }

    public class GuestLockResult
    {
        public bool Success { get; set; }
        public long InvoiceId { get; set; }
        public string Message { get; set; }
        public bool WasAlreadyLocked { get; set; }
    }

    public class EventCompletionResult
    {
        public bool Success { get; set; }
        public long InvoiceId { get; set; }
        public decimal BalanceDue { get; set; }
        public string Message { get; set; }
    }

    public class PaymentSuccessResult
    {
        public bool Success { get; set; }
        public long InvoiceId { get; set; }
        public OrderStatus NewOrderStatus { get; set; }
        public string Message { get; set; }
    }

    #endregion
}

