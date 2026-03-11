using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Invoice;
using CateringEcommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Invoice notification service
    /// Handles email and SMS notifications for invoice events
    /// </summary>
    public class InvoiceNotificationService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoicePdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<InvoiceNotificationService> _logger;

        public InvoiceNotificationService(
            IInvoiceRepository invoiceRepository,
            IInvoicePdfService pdfService,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<InvoiceNotificationService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _pdfService = pdfService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        #region Invoice Generation Notifications

        /// <summary>
        /// Send notification when booking invoice is generated
        /// </summary>
        public async Task SendBookingInvoiceNotificationAsync(long orderId, long invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null) return;

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoice);

                // Email notification
                var emailSubject = $"Booking Invoice {invoice.InvoiceNumber} - Payment Required";
                var emailBody = GenerateBookingInvoiceEmail(invoice);

                await _emailService.SendEmailWithAttachmentAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody,
                    pdfBytes,
                    $"Invoice_{invoice.InvoiceNumber}.pdf"
                );

                // SMS notification
                var smsMessage = $"Your booking invoice {invoice.InvoiceNumber} for ₹{invoice.TotalAmount:N2} is ready. " +
                                $"Pay within {invoice.DueDate?.ToString("dd-MMM")} to confirm booking. Check email for details.";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation(
                    "Booking invoice notification sent for Invoice {InvoiceId}, Order {OrderId}",
                    invoiceId,
                    orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking invoice notification for Invoice {InvoiceId}", invoiceId);
            }
        }

        /// <summary>
        /// Send notification when pre-event invoice is generated
        /// </summary>
        public async Task SendPreEventInvoiceNotificationAsync(long orderId, long invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null) return;

                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoice);

                var emailSubject = $"Pre-Event Invoice {invoice.InvoiceNumber} - Event Payment Required";
                var emailBody = GeneratePreEventInvoiceEmail(invoice);

                await _emailService.SendEmailWithAttachmentAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody,
                    pdfBytes,
                    $"Invoice_{invoice.InvoiceNumber}.pdf"
                );

                // SMS with urgency
                var smsMessage = $"URGENT: Pre-event payment of ₹{invoice.TotalAmount:N2} required before {invoice.DueDate?.ToString("dd-MMM")}. " +
                                $"Your event CANNOT start without this payment (75% total). Invoice: {invoice.InvoiceNumber}";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation(
                    "Pre-event invoice notification sent for Invoice {InvoiceId}, Order {OrderId}",
                    invoiceId,
                    orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pre-event invoice notification for Invoice {InvoiceId}", invoiceId);
            }
        }

        /// <summary>
        /// Send notification when final invoice is generated
        /// </summary>
        public async Task SendFinalInvoiceNotificationAsync(long orderId, long invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null) return;

                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoice);

                var emailSubject = $"Final Settlement Invoice {invoice.InvoiceNumber}";
                var emailBody = GenerateFinalInvoiceEmail(invoice);

                await _emailService.SendEmailWithAttachmentAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody,
                    pdfBytes,
                    $"Invoice_{invoice.InvoiceNumber}.pdf"
                );

                // SMS notification
                var smsMessage = invoice.BalanceDue > 0
                    ? $"Final invoice {invoice.InvoiceNumber} for ₹{invoice.BalanceDue:N2} is ready. Payment due by {invoice.DueDate?.ToString("dd-MMM")}."
                    : $"Thank you! All payments complete. Your final invoice {invoice.InvoiceNumber} is attached to email.";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation(
                    "Final invoice notification sent for Invoice {InvoiceId}, Order {OrderId}",
                    invoiceId,
                    orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending final invoice notification for Invoice {InvoiceId}", invoiceId);
            }
        }

        #endregion

        #region Payment Notifications

        /// <summary>
        /// Send payment receipt after successful payment
        /// </summary>
        public async Task SendPaymentReceiptAsync(long orderId, long invoiceId, string paymentId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null) return;

                // Generate receipt PDF
                var receiptPdf = await _pdfService.GeneratePaymentReceiptPdfAsync(invoice, paymentId);

                var emailSubject = $"Payment Receipt - {invoice.InvoiceNumber}";
                var emailBody = GeneratePaymentReceiptEmail(invoice, paymentId);

                await _emailService.SendEmailWithAttachmentAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody,
                    receiptPdf,
                    $"Receipt_{invoice.InvoiceNumber}_{paymentId}.pdf"
                );

                // SMS confirmation
                var smsMessage = $"Payment received! ₹{invoice.AmountPaid:N2} for invoice {invoice.InvoiceNumber}. " +
                                $"Payment ID: {paymentId}. Receipt sent to your email.";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation(
                    "Payment receipt sent for Invoice {InvoiceId}, Payment {PaymentId}",
                    invoiceId,
                    paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment receipt for Invoice {InvoiceId}", invoiceId);
            }
        }

        /// <summary>
        /// Send payment reminder for pending invoices
        /// </summary>
        public async Task SendPaymentReminderAsync(long invoiceId, int daysUntilDue)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null) return;

                var emailSubject = daysUntilDue > 0
                    ? $"Payment Reminder - Invoice {invoice.InvoiceNumber} (Due in {daysUntilDue} days)"
                    : $"URGENT: Invoice {invoice.InvoiceNumber} Overdue";

                var emailBody = GeneratePaymentReminderEmail(invoice, daysUntilDue);

                await _emailService.SendEmailAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody
                );

                // SMS reminder
                var smsMessage = daysUntilDue > 0
                    ? $"Reminder: Payment of ₹{invoice.BalanceDue:N2} for invoice {invoice.InvoiceNumber} due in {daysUntilDue} days ({invoice.DueDate?.ToString("dd-MMM")})."
                    : $"OVERDUE: Invoice {invoice.InvoiceNumber} payment of ₹{invoice.BalanceDue:N2} is overdue. Please pay immediately.";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation(
                    "Payment reminder sent for Invoice {InvoiceId}, Days until due: {Days}",
                    invoiceId,
                    daysUntilDue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder for Invoice {InvoiceId}", invoiceId);
            }
        }

        #endregion

        #region Lock Notifications

        /// <summary>
        /// Send notification when guest count is locked
        /// </summary>
        public async Task SendGuestCountLockNotificationAsync(long orderId)
        {
            try
            {
                // Get pre-event invoice
                var invoice = await _invoiceRepository.GetInvoiceByOrderAndTypeAsync(
                    orderId,
                    InvoiceType.PRE_EVENT);

                if (invoice == null) return;

                var emailSubject = "Guest Count Locked - Pre-Event Payment Required";
                var emailBody = GenerateGuestCountLockEmail(invoice);

                await _emailService.SendEmailAsync(
                    invoice.OrderSummary?.CustomerEmail,
                    emailSubject,
                    emailBody
                );

                var smsMessage = $"Guest count locked for your event (Order #{orderId}). " +
                                $"Any changes now will result in extra charges. Pre-event payment of ₹{invoice.BalanceDue:N2} required.";

                await _smsService.SendSmsAsync(invoice.OrderSummary?.CustomerPhone, smsMessage);

                _logger.LogInformation("Guest count lock notification sent for Order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending guest count lock notification for Order {OrderId}", orderId);
            }
        }

        /// <summary>
        /// Send notification when menu is locked
        /// </summary>
        public async Task SendMenuLockNotificationAsync(long orderId)
        {
            try
            {
                var emailSubject = "Menu Locked - Final Details Confirmed";
                var emailBody = $@"
                    <h2>Menu Locked</h2>
                    <p>Your menu for Order #{orderId} has been locked as you are now 3 days away from your event.</p>
                    <p><strong>Important:</strong> Any menu changes from this point will be treated as add-ons and charged separately.</p>
                    <p>Your event preparation is in progress. We look forward to serving you!</p>
                ";

                // Get customer email from order
                // TODO: Add method to get customer email from order
                // await _emailService.SendEmailAsync(customerEmail, emailSubject, emailBody);

                _logger.LogInformation("Menu lock notification sent for Order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending menu lock notification for Order {OrderId}", orderId);
            }
        }

        #endregion

        #region Email Template Generators

        private string GenerateBookingInvoiceEmail(InvoiceDto invoice)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 8px; margin-top: 20px; }}
                        .invoice-details {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .amount {{ font-size: 24px; font-weight: bold; color: #667eea; }}
                        .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px; margin: 15px 0; }}
                        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Booking Invoice Generated</h1>
                            <p>Order #{invoice.OrderId}</p>
                        </div>

                        <div class='content'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <p>Thank you for placing your order! Your booking invoice has been generated and is attached to this email.</p>

                            <div class='invoice-details'>
                                <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                                <p><strong>Invoice Date:</strong> {invoice.InvoiceDate:dd-MMM-yyyy}</p>
                                <p><strong>Due Date:</strong> {invoice.DueDate?.ToString("dd-MMM-yyyy")}</p>
                                <p><strong>Total Amount:</strong> <span class='amount'>₹{invoice.TotalAmount:N2}</span></p>
                            </div>

                            <div class='warning'>
                                <strong>⚠️ Payment Required to Confirm Booking</strong>
                                <p>This is a 40% advance payment to confirm your booking. Please complete payment by {invoice.DueDate?.ToString("dd-MMM-yyyy")} to secure your event date.</p>
                            </div>

                            <p>Payment Breakdown:</p>
                            <ul>
                                <li>Subtotal: ₹{invoice.Subtotal:N2}</li>
                                <li>CGST (9%): ₹{invoice.CgstAmount:N2}</li>
                                <li>SGST (9%): ₹{invoice.SgstAmount:N2}</li>
                                <li><strong>Total: ₹{invoice.TotalAmount:N2}</strong></li>
                            </ul>

                            <a href='#' class='button'>Pay Now</a>

                            <p>If you have any questions, please don't hesitate to contact us.</p>

                            <p>Best regards,<br>Catering Services Team</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GeneratePreEventInvoiceEmail(InvoiceDto invoice)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #f59e0b 0%, #ea580c 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 8px; margin-top: 20px; }}
                        .urgent {{ background: #fee2e2; border-left: 4px solid #ef4444; padding: 12px; margin: 15px 0; }}
                        .amount {{ font-size: 24px; font-weight: bold; color: #ea580c; }}
                        .button {{ display: inline-block; background: #ea580c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>⚠️ Pre-Event Payment Required</h1>
                            <p>Your event is approaching!</p>
                        </div>

                        <div class='content'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <div class='urgent'>
                                <strong>🔒 CRITICAL: Event Cannot Start Without This Payment</strong>
                                <p>Your guest count has been locked. This 35% pre-event payment is REQUIRED for your event to proceed. Without 75% total payment (40% booking + 35% pre-event), our team cannot start event preparation.</p>
                            </div>

                            <div style='background: white; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                                <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                                <p><strong>Amount Due:</strong> <span class='amount'>₹{invoice.TotalAmount:N2}</span></p>
                                <p><strong>Payment Deadline:</strong> {invoice.DueDate?.ToString("dd-MMM-yyyy")}</p>
                            </div>

                            <p><strong>Payment Progress:</strong></p>
                            <ul>
                                <li>✅ Booking Payment (40%) - Completed</li>
                                <li>⚠️ Pre-Event Payment (35%) - <strong>REQUIRED NOW</strong></li>
                                <li>⏳ Final Settlement (25%) - After event</li>
                            </ul>

                            <a href='#' class='button'>Pay Now - ₹{invoice.TotalAmount:N2}</a>

                            <p style='color: #ef4444; font-weight: bold;'>⏰ Time Sensitive: Please complete payment immediately to avoid event cancellation.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerateFinalInvoiceEmail(InvoiceDto invoice)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                        <div style='background: #10b981; color: white; padding: 20px; text-align: center;'>
                            <h1>Final Settlement Invoice</h1>
                        </div>

                        <div style='padding: 20px;'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <p>Thank you for choosing our catering services! Your event has been completed successfully.</p>

                            <div style='background: #f3f4f6; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                                <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                                <p><strong>Balance Due:</strong> ₹{invoice.BalanceDue:N2}</p>
                                {(invoice.BalanceDue > 0 ? $"<p><strong>Due Date:</strong> {invoice.DueDate?.ToString("dd-MMM-yyyy")}</p>" : "")}
                            </div>

                            {(invoice.BalanceDue > 0
                                ? "<p>This invoice includes the final 25% settlement plus any additional charges for extra guests or add-ons.</p>"
                                : "<p>✅ Congratulations! All payments have been completed. This is your final invoice for records.</p>"
                            )}

                            <p>We hope you enjoyed our services. Thank you for your business!</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GeneratePaymentReceiptEmail(InvoiceDto invoice, string paymentId)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                        <div style='background: #10b981; color: white; padding: 20px; text-align: center;'>
                            <h1>✓ Payment Received</h1>
                        </div>

                        <div style='padding: 20px;'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <div style='background: #d1fae5; border-left: 4px solid #10b981; padding: 15px; margin: 15px 0;'>
                                <p><strong>✓ Payment Successful!</strong></p>
                                <p>Amount Paid: <strong>₹{invoice.AmountPaid:N2}</strong></p>
                                <p>Payment ID: {paymentId}</p>
                                <p>Invoice: {invoice.InvoiceNumber}</p>
                            </div>

                            <p>Your payment receipt is attached to this email.</p>

                            <p>Thank you for your payment!</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GeneratePaymentReminderEmail(InvoiceDto invoice, int daysUntilDue)
        {
            var urgency = daysUntilDue <= 0 ? "OVERDUE" : daysUntilDue <= 1 ? "URGENT" : "REMINDER";
            var color = daysUntilDue <= 0 ? "#ef4444" : daysUntilDue <= 1 ? "#f59e0b" : "#3b82f6";

            return $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                        <div style='background: {color}; color: white; padding: 20px; text-align: center;'>
                            <h1>{urgency}: Payment Reminder</h1>
                        </div>

                        <div style='padding: 20px;'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <p>{(daysUntilDue > 0
                                ? $"This is a friendly reminder that your payment for invoice {invoice.InvoiceNumber} is due in {daysUntilDue} day(s)."
                                : $"<strong>URGENT:</strong> Your payment for invoice {invoice.InvoiceNumber} is now OVERDUE by {Math.Abs(daysUntilDue)} day(s)."
                            )}</p>

                            <div style='background: #f3f4f6; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                                <p><strong>Invoice:</strong> {invoice.InvoiceNumber}</p>
                                <p><strong>Amount Due:</strong> ₹{invoice.BalanceDue:N2}</p>
                                <p><strong>Due Date:</strong> {invoice.DueDate?.ToString("dd-MMM-yyyy")}</p>
                            </div>

                            <p>Please complete payment at your earliest convenience.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerateGuestCountLockEmail(InvoiceDto invoice)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                        <div style='background: #6366f1; color: white; padding: 20px; text-align: center;'>
                            <h1>🔒 Guest Count Locked</h1>
                        </div>

                        <div style='padding: 20px;'>
                            <p>Dear {invoice.OrderSummary?.CustomerName},</p>

                            <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 15px 0;'>
                                <p><strong>Important Notice:</strong></p>
                                <p>Your guest count has been locked as you are now 5 days away from your event.</p>
                                <p>Any increase in guest count from this point will be charged as extra guests.</p>
                            </div>

                            <p>Your pre-event invoice ({invoice.InvoiceNumber}) for ₹{invoice.TotalAmount:N2} is now ready.</p>

                            <p><strong>Critical:</strong> This 35% payment is required for your event to proceed. Without 75% total payment, our team cannot start event preparation.</p>

                            <p>Please complete payment by {invoice.DueDate?.ToString("dd-MMM-yyyy")}.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        #endregion
    }
}
