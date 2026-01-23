using System;
using System.Data;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Background job service for sending post-event payment reminders
    /// Runs daily to check for pending post-event payments and send reminders
    /// </summary>
    public class PaymentReminderJob
    {
        private readonly string _connectionString;
        private readonly PaymentStageRepository _paymentStageRepository;
        private readonly NotificationService _notificationService;
        private readonly ILogger<PaymentReminderJob>? _logger;

        public PaymentReminderJob(string connectionString, IConfiguration configuration, ILogger<PaymentReminderJob>? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _paymentStageRepository = new PaymentStageRepository(connectionString);
            _notificationService = new NotificationService(configuration ?? throw new ArgumentNullException(nameof(configuration)));
            _logger = logger;
        }

        /// <summary>
        /// Main job method to send payment reminders for pending post-event payments
        /// This method is called by Hangfire recurring job scheduler
        /// </summary>
        public async Task SendPostEventPaymentRemindersAsync()
        {
            try
            {
                _logger?.LogInformation("Starting post-event payment reminder job...");

                // Get all orders with pending post-event payments
                DataTable pendingPayments = await _paymentStageRepository.GetOrdersWithPendingPostEventPaymentsAsync();

                if (pendingPayments.Rows.Count == 0)
                {
                    _logger?.LogInformation("No pending post-event payments found.");
                    return;
                }

                _logger?.LogInformation($"Found {pendingPayments.Rows.Count} orders with pending post-event payments.");

                int successCount = 0;
                int skipCount = 0;
                int failCount = 0;

                foreach (DataRow row in pendingPayments.Rows)
                {
                    try
                    {
                        long orderId = Convert.ToInt64(row["c_orderid"]);
                        long paymentStageId = Convert.ToInt64(row["c_payment_stage_id"]);
                        string orderNumber = row["c_order_number"].ToString() ?? "";
                        decimal stageAmount = Convert.ToDecimal(row["c_stage_amount"]);
                        DateTime? dueDate = row["c_due_date"] != DBNull.Value ? Convert.ToDateTime(row["c_due_date"]) : null;
                        int reminderSentCount = Convert.ToInt32(row["c_reminder_sent_count"]);
                        DateTime? lastReminderDate = row["c_last_reminder_date"] != DBNull.Value ? Convert.ToDateTime(row["c_last_reminder_date"]) : null;

                        string contactEmail = row["c_contact_email"].ToString() ?? "";
                        string contactPhone = row["c_contact_phone"].ToString() ?? "";
                        string contactPerson = row["c_contact_person"].ToString() ?? "";

                        // Business rules for sending reminders
                        // 1. Max 3 reminders per order
                        if (reminderSentCount >= 3)
                        {
                            _logger?.LogInformation($"Order {orderNumber} already has {reminderSentCount} reminders. Skipping.");
                            skipCount++;
                            continue;
                        }

                        // 2. Send reminder only once per day
                        if (lastReminderDate.HasValue && lastReminderDate.Value.Date == DateTime.Now.Date)
                        {
                            _logger?.LogInformation($"Order {orderNumber} already received reminder today. Skipping.");
                            skipCount++;
                            continue;
                        }

                        // 3. Send first reminder 1 day after due date, then every 2 days
                        if (dueDate.HasValue)
                        {
                            int daysSinceDue = (DateTime.Now.Date - dueDate.Value.Date).Days;

                            if (reminderSentCount == 0 && daysSinceDue < 1)
                            {
                                // First reminder: wait at least 1 day after due date
                                skipCount++;
                                continue;
                            }
                            else if (reminderSentCount > 0 && lastReminderDate.HasValue)
                            {
                                int daysSinceLastReminder = (DateTime.Now.Date - lastReminderDate.Value.Date).Days;
                                if (daysSinceLastReminder < 2)
                                {
                                    // Subsequent reminders: wait at least 2 days
                                    skipCount++;
                                    continue;
                                }
                            }
                        }

                        // Send reminder notification
                        await SendReminderNotificationAsync(
                            orderId,
                            orderNumber,
                            stageAmount,
                            contactPerson,
                            contactEmail,
                            contactPhone,
                            reminderSentCount
                        );

                        // Update reminder sent count
                        await _paymentStageRepository.UpdateReminderSentCountAsync(paymentStageId);

                        successCount++;
                        _logger?.LogInformation($"Payment reminder sent for order {orderNumber}. Total reminders: {reminderSentCount + 1}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger?.LogError($"Error sending reminder for order: {ex.Message}");
                    }
                }

                _logger?.LogInformation($"Post-event payment reminder job completed. Success: {successCount}, Skipped: {skipCount}, Failed: {failCount}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Fatal error in post-event payment reminder job: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Send payment reminder notification via email and SMS
        /// </summary>
        private async Task SendReminderNotificationAsync(
            long orderId,
            string orderNumber,
            decimal amount,
            string contactPerson,
            string contactEmail,
            string contactPhone,
            int reminderCount)
        {
            try
            {
                // Determine reminder urgency based on count
                string urgency = reminderCount switch
                {
                    0 => "Friendly Reminder",
                    1 => "Second Reminder",
                    2 => "Final Reminder",
                    _ => "Payment Due"
                };

                // Email notification
                string emailSubject = $"{urgency}: Post-Event Payment Due for Order #{orderNumber}";
                string emailBody = $@"
                    <html>
                    <body>
                        <h2>Post-Event Payment Reminder</h2>
                        <p>Dear {contactPerson},</p>

                        <p>This is a {urgency.ToLower()} for the remaining payment for your catering order.</p>

                        <h3>Order Details:</h3>
                        <ul>
                            <li><strong>Order Number:</strong> {orderNumber}</li>
                            <li><strong>Pending Amount:</strong> ₹{amount:N2}</li>
                            <li><strong>Payment Type:</strong> Post-Event Balance (60%)</li>
                        </ul>

                        <p>Please complete your payment at your earliest convenience.</p>

                        <p>If you have already made the payment, please ignore this reminder.</p>

                        <p>For any questions, please contact us.</p>

                        <p>Thank you for choosing our catering services!</p>
                    </body>
                    </html>
                ";

                await _notificationService.SendEmailAsync(contactEmail, emailSubject, emailBody);

                // SMS notification
                string smsMessage = reminderCount >= 2
                    ? $"FINAL REMINDER: Post-event payment of ₹{amount:N2} is due for order #{orderNumber}. Please pay ASAP to avoid service interruption."
                    : $"Reminder: Post-event payment of ₹{amount:N2} is due for order #{orderNumber}. Please complete payment. Thank you!";

                await _notificationService.SendSMSAsync(contactPhone, smsMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error sending notification: {ex.Message}");
                throw;
            }
        }
    }
}
