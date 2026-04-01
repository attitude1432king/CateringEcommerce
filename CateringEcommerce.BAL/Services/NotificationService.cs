using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.User;
using System.Net.Http;
using System.Collections.Generic;

namespace CateringEcommerce.BAL.Services
{
    public interface INotificationService
    {
        Task SendOrderConfirmationAsync(OrderDto order, string userEmail, string userPhone);
        Task SendOrderCancellationAsync(long orderId, string orderNumber, string userEmail, string userPhone, string reason);
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendSMSAsync(string phoneNumber, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly ISystemSettingsProvider _settings;
        private readonly HttpClient _httpClient;

        public NotificationService(ISystemSettingsProvider settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = new HttpClient();
        }

        // ===================================
        // SEND ORDER CONFIRMATION
        // ===================================
        public async Task SendOrderConfirmationAsync(OrderDto order, string userEmail, string userPhone)
        {
            try
            {
                // Prepare email content
                string emailSubject = $"Order Confirmation - {order.OrderNumber}";
                string emailBody = GenerateOrderConfirmationEmail(order);

                // Send email
                await SendEmailAsync(userEmail, emailSubject, emailBody);

                // Prepare SMS content
                string smsMessage = $"Order {order.OrderNumber} confirmed! Event on {order.EventDate:dd-MMM-yyyy}. Total: Rs.{order.TotalAmount:N2}. Thank you for choosing Enyvora!";

                // Send SMS
                await SendSMSAsync(userPhone, smsMessage);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notification failure shouldn't block order creation
                Console.WriteLine($"Error sending order confirmation: {ex.Message}");
            }
        }

        // ===================================
        // SEND ORDER CANCELLATION
        // ===================================
        public async Task SendOrderCancellationAsync(long orderId, string orderNumber, string userEmail, string userPhone, string reason)
        {
            try
            {
                // Prepare email content
                string emailSubject = $"Order Cancelled - {orderNumber}";
                string emailBody = GenerateOrderCancellationEmail(orderNumber, reason);

                // Send email
                await SendEmailAsync(userEmail, emailSubject, emailBody);

                // Prepare SMS content
                string smsMessage = $"Order {orderNumber} has been cancelled successfully. Refund will be processed within 5-7 business days. - Enyvora";

                // Send SMS
                await SendSMSAsync(userPhone, smsMessage);
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error sending cancellation notification: {ex.Message}");
            }
        }

        // ===================================
        // SEND EMAIL (SMTP)
        // ===================================
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                string smtpServer = _settings.GetString("EMAIL.SMTP_HOST", "smtp.gmail.com");
                int smtpPort = _settings.GetInt("EMAIL.SMTP_PORT", 587);
                string senderEmail = _settings.GetString("EMAIL.FROM_ADDRESS", "noreply@enyvora.com");
                string senderName = _settings.GetString("EMAIL.FROM_NAME", "Enyvora Catering");
                string username = _settings.GetString("EMAIL.SMTP_USERNAME");
                string password = _settings.GetString("EMAIL.SMTP_PASSWORD");
                bool enableSsl = _settings.GetBool("EMAIL.ENABLE_SSL", true);

                // Skip if no credentials configured
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Email credentials not configured. Skipping email send.");
                    return;
                }

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail, senderName);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = htmlBody;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(username, password);
                        smtp.EnableSsl = enableSsl;

                        await smtp.SendMailAsync(mail);
                    }
                }

                Console.WriteLine($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

        // ===================================
        // SEND SMS (MSG91 — OTP only via SmsService; AWS SNS for notifications)
        // ===================================
        public async Task SendSMSAsync(string phoneNumber, string message)
        {
            try
            {
                string apiKey = _settings.GetString("SMS.API_KEY");
                string senderId = _settings.GetString("SMS.SENDER_ID", "ENYVORA");

                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("SMS API key not configured. Skipping SMS send.");
                    return;
                }

                await SendViaMSG91(phoneNumber, message, apiKey, senderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                throw;
            }
        }

        // ===================================
        // SEND VIA MSG91
        // ===================================
        private async Task SendViaMSG91(string phoneNumber, string message, string apiKey, string senderId)
        {
            try
            {
                // Format phone number (remove +91 if present)
                phoneNumber = phoneNumber.Replace("+91", "").Replace("+", "").Trim();

                string url = $"https://api.msg91.com/api/v5/flow/";

                var payload = new
                {
                    sender = senderId,
                    route = "4",
                    country = "91",
                    sms = new[]
                    {
                        new
                        {
                            message = new[] { message },
                            to = new[] { phoneNumber }
                        }
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("authkey", apiKey);

                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"SMS sent successfully via MSG91 to {phoneNumber}");
                }
                else
                {
                    Console.WriteLine($"MSG91 SMS failed: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS via MSG91: {ex.Message}");
                throw;
            }
        }

// ===================================
        // GENERATE ORDER CONFIRMATION EMAIL HTML
        // ===================================
        private string GenerateOrderConfirmationEmail(OrderDto order)
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #ff6b6b; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .order-details { background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }
        .order-details h3 { margin-top: 0; color: #ff6b6b; }
        .order-summary { margin: 15px 0; }
        .order-summary table { width: 100%; border-collapse: collapse; }
        .order-summary td { padding: 8px; border-bottom: 1px solid #ddd; }
        .total { font-weight: bold; font-size: 1.2em; color: #ff6b6b; }
        .footer { text-align: center; padding: 20px; font-size: 0.9em; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <p>Dear Customer,</p>
            <p>Thank you for your order! Your order has been successfully placed and is being processed.</p>

            <div class='order-details'>
                <h3>Order Details</h3>
                <p><strong>Order Number:</strong> " + order.OrderNumber + @"</p>
                <p><strong>Order Date:</strong> " + order.CreatedDate.ToString("dd MMM yyyy, hh:mm tt") + @"</p>
                <p><strong>Order Status:</strong> " + order.OrderStatus + @"</p>
            </div>

            <div class='order-details'>
                <h3>Event Information</h3>
                <p><strong>Event Type:</strong> " + order.EventType + @"</p>
                <p><strong>Event Date:</strong> " + order.EventDate.ToString("dd MMM yyyy") + @"</p>
                <p><strong>Event Time:</strong> " + order.EventTime + @"</p>
                <p><strong>Guest Count:</strong> " + order.GuestCount + @"</p>
                <p><strong>Location:</strong> " + order.EventLocation + @"</p>
            </div>

            <div class='order-details'>
                <h3>Catering Service</h3>
                <p><strong>Service Provider:</strong> " + order.CateringName + @"</p>
            </div>

            <div class='order-details'>
                <h3>Order Summary</h3>
                <div class='order-summary'>
                    <table>
                        <tr><td>Base Amount</td><td>₹" + order.BaseAmount.ToString("N2") + @"</td></tr>
                        <tr><td>Tax (18%)</td><td>₹" + order.TaxAmount.ToString("N2") + @"</td></tr>");

            if (order.DeliveryCharges > 0)
            {
                html.Append(@"<tr><td>Delivery Charges</td><td>₹" + order.DeliveryCharges.ToString("N2") + @"</td></tr>");
            }

            if (order.DiscountAmount > 0)
            {
                html.Append(@"<tr><td>Discount</td><td>-₹" + order.DiscountAmount.ToString("N2") + @"</td></tr>");
            }

            html.Append(@"
                        <tr class='total'><td>Total Amount</td><td>₹" + order.TotalAmount.ToString("N2") + @"</td></tr>
                    </table>
                </div>
            </div>

            <div class='order-details'>
                <h3>Payment Information</h3>
                <p><strong>Payment Method:</strong> " + order.PaymentMethod + @"</p>
                <p><strong>Payment Status:</strong> " + order.PaymentStatus + @"</p>
            </div>

            <p style='margin-top: 20px;'>We will contact you shortly to confirm your order and discuss any specific requirements.</p>
            <p>If you have any questions, please feel free to contact us.</p>
        </div>
        <div class='footer'>
            <p>Thank you for choosing Enyvora Catering!</p>
            <p>&copy; 2026 Enyvora. All rights reserved.</p>
        </div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        // ===================================
        // GENERATE ORDER CANCELLATION EMAIL HTML
        // ===================================
        private string GenerateOrderCancellationEmail(string orderNumber, string reason)
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #e74c3c; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .cancellation-details { background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }
        .footer { text-align: center; padding: 20px; font-size: 0.9em; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Cancelled</h1>
        </div>
        <div class='content'>
            <p>Dear Customer,</p>
            <p>Your order <strong>" + orderNumber + @"</strong> has been cancelled successfully.</p>

            <div class='cancellation-details'>
                <h3>Cancellation Details</h3>
                <p><strong>Order Number:</strong> " + orderNumber + @"</p>
                <p><strong>Cancellation Date:</strong> " + DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") + @"</p>
                <p><strong>Reason:</strong> " + reason + @"</p>
            </div>

            <div class='cancellation-details'>
                <h3>Refund Information</h3>
                <p>If you have made any payment, the refund will be processed within 5-7 business days.</p>
                <p>The amount will be credited to your original payment method.</p>
            </div>

            <p style='margin-top: 20px;'>We're sorry to see you cancel your order. If you have any concerns or need assistance, please don't hesitate to contact us.</p>
            <p>We hope to serve you again in the future!</p>
        </div>
        <div class='footer'>
            <p>Thank you for considering Enyvora Catering</p>
            <p>&copy; 2026 Enyvora. All rights reserved.</p>
        </div>
    </div>
</body>
</html>");

            return html.ToString();
        }
    }
}
