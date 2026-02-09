using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Extensions.Configuration;

namespace CateringEcommerce.BAL.Base.User
{
    public class PaymentStageService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly PaymentStageRepository _paymentStageRepository;
        private readonly INotificationService? _notificationService;

        public PaymentStageService(IDatabaseHelper dbHelper, INotificationService? notificationService = null)
        {
            _dbHelper = dbHelper;
            _paymentStageRepository = new PaymentStageRepository(_dbHelper);
            _notificationService = notificationService;
        }

        // ===================================
        // CREATE PAYMENT STAGES (40%/60%)
        // ===================================
        public async Task<List<PaymentStageDto>> CreatePaymentStagesAsync(CreatePaymentStagesDto stagesData, DateTime eventDate)
        {
            try
            {
                if (stagesData == null)
                {
                    throw new ArgumentNullException(nameof(stagesData));
                }

                if (stagesData.TotalAmount <= 0)
                {
                    throw new ArgumentException("Total amount must be greater than zero.", nameof(stagesData.TotalAmount));
                }

                List<PaymentStageDto> createdStages = new List<PaymentStageDto>();

                if (stagesData.EnableSplitPayment)
                {
                    // Create two stages: 40% Pre-booking and 60% Post-event
                    decimal preBookingPercentage = 40.00m;
                    decimal postEventPercentage = 60.00m;

                    decimal preBookingAmount = Math.Round(stagesData.TotalAmount * (preBookingPercentage / 100), 2);
                    decimal postEventAmount = stagesData.TotalAmount - preBookingAmount; // Ensure total adds up

                    // Stage 1: Pre-booking (40%)
                    long preBookingStageId = await _paymentStageRepository.InsertPaymentStageAsync(
                        stagesData.OrderId,
                        "PreBooking",
                        preBookingPercentage,
                        preBookingAmount,
                        null // No due date for pre-booking
                    );

                    if (preBookingStageId <= 0)
                    {
                        throw new InvalidOperationException("Failed to create pre-booking payment stage.");
                    }

                    // Stage 2: Post-event (60%)
                    DateTime postEventDueDate = eventDate.AddDays(1); // Due 1 day after event
                    long postEventStageId = await _paymentStageRepository.InsertPaymentStageAsync(
                        stagesData.OrderId,
                        "PostEvent",
                        postEventPercentage,
                        postEventAmount,
                        postEventDueDate
                    );

                    if (postEventStageId <= 0)
                    {
                        throw new InvalidOperationException("Failed to create post-event payment stage.");
                    }

                    // Fetch created stages
                    createdStages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(stagesData.OrderId);
                }
                else
                {
                    // Create single stage: 100% Full payment
                    long fullPaymentStageId = await _paymentStageRepository.InsertPaymentStageAsync(
                        stagesData.OrderId,
                        "Full",
                        100.00m,
                        stagesData.TotalAmount,
                        null
                    );

                    if (fullPaymentStageId <= 0)
                    {
                        throw new InvalidOperationException("Failed to create full payment stage.");
                    }

                    // Fetch created stage
                    createdStages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(stagesData.OrderId);
                }

                return createdStages;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating payment stages: " + ex.Message, ex);
            }
        }

        // ===================================
        // PROCESS PAYMENT STAGE
        // ===================================
        public async Task<bool> ProcessPaymentStageAsync(ProcessPaymentStageDto paymentData)
        {
            try
            {
                if (paymentData == null)
                {
                    throw new ArgumentNullException(nameof(paymentData));
                }

                if (paymentData.OrderId <= 0)
                {
                    throw new ArgumentException("Invalid order ID.", nameof(paymentData.OrderId));
                }

                // Get pending payment stages for the order
                var pendingStages = await _paymentStageRepository.GetPendingPaymentStagesAsync(paymentData.OrderId);

                // Find the stage that matches the stage type
                var matchingStage = pendingStages.FirstOrDefault(s => s.StageType == paymentData.StageType);

                if (matchingStage == null)
                {
                    throw new InvalidOperationException($"No pending payment stage found for stage type: {paymentData.StageType}");
                }

                // Update payment stage status to "Success"
                bool updated = await _paymentStageRepository.UpdatePaymentStageStatusAsync(
                    matchingStage.PaymentStageId,
                    "Success",
                    paymentData
                );

                if (!updated)
                {
                    throw new InvalidOperationException("Failed to update payment stage status.");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing payment stage: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET PENDING PAYMENT STAGES
        // ===================================
        public async Task<PendingPaymentStagesDto> GetPendingPaymentStagesAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    throw new ArgumentException("Invalid order ID.", nameof(orderId));
                }

                // Get all payment stages for the order
                var allStages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(orderId);

                if (allStages == null || allStages.Count == 0)
                {
                    throw new InvalidOperationException("No payment stages found for this order.");
                }

                // Separate pending and completed stages
                var pendingStages = allStages.Where(s => s.Status == "Pending").ToList();
                var completedStages = allStages.Where(s => s.Status == "Success").ToList();

                // Calculate amounts
                decimal totalAmount = allStages.Sum(s => s.StageAmount);
                decimal paidAmount = completedStages.Sum(s => s.StageAmount);
                decimal pendingAmount = pendingStages.Sum(s => s.StageAmount);

                return new PendingPaymentStagesDto
                {
                    OrderId = orderId,
                    OrderNumber = string.Empty, // Will be populated by calling service
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    PendingAmount = pendingAmount,
                    PendingStages = pendingStages,
                    CompletedStages = completedStages
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving pending payment stages: " + ex.Message, ex);
            }
        }

        // ===================================
        // TRIGGER POST-EVENT PAYMENT REMINDERS (Background Job)
        // ===================================
        public async Task TriggerPostEventPaymentRemindersAsync()
        {
            try
            {
                // Get all orders with pending post-event payments
                DataTable ordersTable = await _paymentStageRepository.GetOrdersWithPendingPostEventPaymentsAsync();

                if (ordersTable.Rows.Count == 0)
                {
                    return; // No reminders to send
                }

                foreach (DataRow row in ordersTable.Rows)
                {
                    long orderId = Convert.ToInt64(row["c_orderid"]);
                    string orderNumber = row["c_order_number"].ToString() ?? string.Empty;
                    DateTime eventDate = Convert.ToDateTime(row["c_event_date"]);
                    string contactEmail = row["c_contact_email"].ToString() ?? string.Empty;
                    string contactPhone = row["c_contact_phone"].ToString() ?? string.Empty;
                    string contactPerson = row["c_contact_person"].ToString() ?? string.Empty;
                    long paymentStageId = Convert.ToInt64(row["c_payment_stage_id"]);
                    decimal stageAmount = Convert.ToDecimal(row["c_stage_amount"]);
                    int remindersSent = Convert.ToInt32(row["c_reminder_sent_count"]);
                    DateTime? lastReminderDate = row["c_last_reminder_date"] != DBNull.Value
                        ? Convert.ToDateTime(row["c_last_reminder_date"])
                        : null;

                    // Calculate days since event
                    int daysSinceEvent = (DateTime.Now - eventDate).Days;

                    // Send reminders on Day 1, 3, 7, 14
                    bool shouldSendReminder = false;

                    if (daysSinceEvent == 1 && remindersSent == 0)
                    {
                        shouldSendReminder = true;
                    }
                    else if (daysSinceEvent == 3 && remindersSent == 1)
                    {
                        shouldSendReminder = true;
                    }
                    else if (daysSinceEvent == 7 && remindersSent == 2)
                    {
                        shouldSendReminder = true;
                    }
                    else if (daysSinceEvent == 14 && remindersSent == 3)
                    {
                        shouldSendReminder = true;
                    }

                    if (shouldSendReminder && _notificationService != null)
                    {
                        // Generate payment link (this should be your actual payment page URL)
                        string paymentLink = $"https://yourapp.com/orders/{orderId}/complete-payment";

                        // Send email reminder
                        string emailSubject = "Payment Reminder - Complete Your Event Payment";
                        string emailBody = $@"
                            <h2>Dear {contactPerson},</h2>
                            <p>Your event was successfully completed on {eventDate:dd-MMM-yyyy}.</p>
                            <p>Please complete the remaining payment of <strong>₹{stageAmount:N2}</strong> for order <strong>{orderNumber}</strong>.</p>
                            <p><a href='{paymentLink}' style='background-color: #FF6B6B; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Complete Payment</a></p>
                            <p>If you have any questions, please contact our support team.</p>
                            <p>Thank you for choosing Enyvora Catering!</p>
                        ";

                        await _notificationService.SendEmailAsync(contactEmail, emailSubject, emailBody);

                        // Send SMS reminder
                        string smsBody = $"Dear {contactPerson}, Please complete payment of Rs.{stageAmount} for order {orderNumber}. Payment link: {paymentLink}";
                        await _notificationService.SendSMSAsync(contactPhone, smsBody);

                        // Update reminder sent count
                        await _paymentStageRepository.UpdateReminderSentCountAsync(paymentStageId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error triggering post-event payment reminders: " + ex.Message, ex);
            }
        }

        // ===================================
        // CHECK IF ORDER HAS SPLIT PAYMENT
        // ===================================
        public async Task<bool> HasSplitPaymentAsync(long orderId)
        {
            try
            {
                var stages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(orderId);
                return stages.Count > 1; // More than one stage means split payment
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking split payment: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET PAYMENT STAGE BY TYPE
        // ===================================
        public async Task<PaymentStageDto?> GetPaymentStageByTypeAsync(long orderId, string stageType)
        {
            try
            {
                var stages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(orderId);
                return stages.FirstOrDefault(s => s.StageType == stageType);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving payment stage: " + ex.Message, ex);
            }
        }
    }
}
