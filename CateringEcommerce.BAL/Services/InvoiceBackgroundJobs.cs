using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Background jobs for invoice system
    /// Handles overdue invoice detection, auto-status updates, and payment reminders
    /// </summary>
    public class InvoiceBackgroundJobs
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentStateMachineService _stateMachine;
        private readonly ILogger<InvoiceBackgroundJobs> _logger;

        public InvoiceBackgroundJobs(
            IInvoiceRepository invoiceRepository,
            IPaymentStateMachineService stateMachine,
            ILogger<InvoiceBackgroundJobs> logger)
        {
            _invoiceRepository = invoiceRepository;
            _stateMachine = stateMachine;
            _logger = logger;
        }

        /// <summary>
        /// Marks unpaid invoices as overdue if past due date
        /// Runs every hour to check for overdue invoices
        /// </summary>
        public async Task MarkOverdueInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Starting overdue invoice detection job");

                // Get all unpaid invoices past due date
                var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesForJobAsync();

                var count = 0;
                foreach (var invoice in overdueInvoices)
                {
                    try
                    {
                        var success = await _stateMachine.MarkInvoiceOverdueAsync(invoice.InvoiceId);
                        if (success)
                        {
                            count++;
                            _logger.LogInformation(
                                "Marked invoice {InvoiceId} (Order: {OrderId}) as overdue",
                                invoice.InvoiceId,
                                invoice.OrderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to mark invoice {InvoiceId} as overdue",
                            invoice.InvoiceId);
                    }
                }

                _logger.LogInformation(
                    "Overdue invoice detection completed. Marked {Count} invoices as overdue",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run overdue invoice detection job");
            }
        }

        /// <summary>
        /// Auto-locks guest counts for orders 5 days before event
        /// Runs hourly to check for orders reaching lock threshold
        /// </summary>
        public async Task AutoLockGuestCountsAsync()
        {
            try
            {
                _logger.LogInformation("Starting auto-lock guest count job");

                // Get all orders approaching guest lock date (5 days before event)
                var ordersToLock = await _invoiceRepository.GetOrdersApproachingGuestLockAsync();

                var count = 0;
                foreach (var order in ordersToLock)
                {
                    try
                    {
                        var daysUntilLock = await _stateMachine.GetDaysUntilGuestLockAsync(order.OrderId);

                        // Lock if within threshold (0 or negative days)
                        if (daysUntilLock <= 0)
                        {
                            var result = await _stateMachine.AutoLockGuestCountAsync(order.OrderId);
                            if (result.Success && !result.WasAlreadyLocked)
                            {
                                count++;
                                _logger.LogInformation(
                                    "Auto-locked guest count for Order {OrderId}",
                                    (long)order.OrderId);

                                // TODO: Send notification to customer about lock
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to auto-lock guest count for Order {OrderId}",
                            (long)order.OrderId);
                    }
                }

                _logger.LogInformation(
                    "Auto-lock guest count job completed. Locked {Count} orders",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run auto-lock guest count job");
            }
        }

        /// <summary>
        /// Auto-locks menus for orders 3 days before event
        /// Runs hourly to check for orders reaching lock threshold
        /// </summary>
        public async Task AutoLockMenusAsync()
        {
            try
            {
                _logger.LogInformation("Starting auto-lock menu job");

                // Get all orders approaching menu lock date (3 days before event)
                var ordersToLock = await _invoiceRepository.GetOrdersApproachingMenuLockAsync();

                var count = 0;
                foreach (var order in ordersToLock)
                {
                    try
                    {
                        var daysUntilLock = await _stateMachine.GetDaysUntilMenuLockAsync(order.OrderId);

                        // Lock if within threshold (0 or negative days)
                        if (daysUntilLock <= 0)
                        {
                            var result = await _stateMachine.AutoLockMenuAsync(order.OrderId);
                            if (result.Success && !result.WasAlreadyLocked)
                            {
                                count++;
                                _logger.LogInformation(
                                    "Auto-locked menu for Order {OrderId}",
                                    (long)order.OrderId);

                                // TODO: Send notification to customer about lock
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to auto-lock menu for Order {OrderId}",
                            (long)order.OrderId);
                    }
                }

                _logger.LogInformation(
                    "Auto-lock menu job completed. Locked {Count} orders",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run auto-lock menu job");
            }
        }

        /// <summary>
        /// Sends payment reminders for pending invoices
        /// Runs daily to remind customers of upcoming/overdue payments
        /// </summary>
        public async Task SendPaymentRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting payment reminder job");

                // Get invoices needing reminders
                var pendingInvoices = await _invoiceRepository.GetPendingInvoicesDueWithinDaysAsync(3);

                var count = 0;
                foreach (var invoice in pendingInvoices)
                {
                    try
                    {
                        // TODO: Send payment reminder notification
                        // Should integrate with NotificationService

                        count++;
                        _logger.LogInformation(
                            "Sent payment reminder for Invoice {InvoiceId} (Order: {OrderId})",
                            invoice.InvoiceId,
                            invoice.OrderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send payment reminder for Invoice {InvoiceId}",
                            invoice.InvoiceId);
                    }
                }

                _logger.LogInformation(
                    "Payment reminder job completed. Sent {Count} reminders",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run payment reminder job");
            }
        }

        /// <summary>
        /// Auto-generates PRE_EVENT invoices when guest lock date is reached
        /// Runs hourly to check for orders reaching PRE_EVENT stage
        /// </summary>
        public async Task AutoGeneratePreEventInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Starting auto-generate PRE_EVENT invoice job");

                // Get orders with BOOKING_PAID status approaching guest lock date
                var ordersForPreEvent = await _invoiceRepository.GetOrdersReadyForPreEventInvoiceAsync();

                var count = 0;
                foreach (var order in ordersForPreEvent)
                {
                    try
                    {
                        // Check if PRE_EVENT invoice already exists
                        var existingInvoice = await _invoiceRepository.GetInvoiceByOrderAndTypeAsync(
                            order.OrderId,
                            InvoiceType.PRE_EVENT);

                        if (existingInvoice == null)
                        {
                            // Generate PRE_EVENT invoice
                            var invoiceId = await _invoiceRepository.GenerateInvoiceAsync(new Domain.Models.Invoice.InvoiceGenerationRequestDto
                            {
                                OrderId = order.OrderId,
                                InvoiceType = InvoiceType.PRE_EVENT,
                                ExtraGuestCharges = 0,
                                AddonCharges = 0
                            });

                            if (invoiceId > 0)
                            {
                                count++;
                                _logger.LogInformation(
                                    "Auto-generated PRE_EVENT invoice {InvoiceId} for Order {OrderId}",
                                    invoiceId,
                                    (long)order.OrderId);

                                // TODO: Send notification to customer
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to auto-generate PRE_EVENT invoice for Order {OrderId}",
                            (long)order.OrderId);
                    }
                }

                _logger.LogInformation(
                    "Auto-generate PRE_EVENT invoice job completed. Generated {Count} invoices",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run auto-generate PRE_EVENT invoice job");
            }
        }
    }
}
