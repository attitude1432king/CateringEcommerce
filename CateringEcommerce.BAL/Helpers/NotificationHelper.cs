using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CateringEcommerce.BAL.Helpers
{
    /// <summary>
    /// Helper class for sending multi-channel notifications (Email, SMS, In-App)
    /// Provides easy-to-use methods for triggering notifications across the application
    /// </summary>
    public class NotificationHelper : INotificationHelper
    {
        private readonly ILogger<NotificationHelper> _logger;
        private readonly IDatabaseHelper _dbHelper;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly RabbitMQPublisher? _rabbitMQPublisher;

        public NotificationHelper(
            ILogger<NotificationHelper> logger,
            IDatabaseHelper dbHelper,
            string apiBaseUrl = "https://localhost:44368",
            RabbitMQPublisher? rabbitMQPublisher = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _httpClient = new HttpClient();
            _apiBaseUrl = apiBaseUrl;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        /// <summary>
        /// Sends multi-channel notification (Email + SMS + In-App)
        /// </summary>
        /// <param name="templateCodePrefix">Template code prefix (e.g., "ORDER_CONFIRMATION")</param>
        /// <param name="audience">Audience type: "USER", "PARTNER", "ADMIN"</param>
        /// <param name="recipientId">Recipient ID</param>
        /// <param name="recipientEmail">Recipient email address</param>
        /// <param name="recipientPhone">Recipient phone number</param>
        /// <param name="data">Template data dictionary</param>
        /// <param name="sendEmail">Send email notification</param>
        /// <param name="sendSms">Send SMS notification</param>
        /// <param name="sendInApp">Send in-app notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>Task</returns>
        public async Task SendMultiChannelNotificationAsync(
            string templateCodePrefix,
            string audience,
            string recipientId,
            string? recipientEmail,
            string? recipientPhone,
            Dictionary<string, object> data,
            bool sendEmail = true,
            bool sendSms = true,
            bool sendInApp = true,
            NotificationPriority priority = NotificationPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();

            try
            {
                // Send Email
                if (sendEmail && !string.IsNullOrEmpty(recipientEmail))
                {
                    tasks.Add(SendNotificationAsync(
                        $"{templateCodePrefix}_EMAIL",
                        NotificationChannel.Email,
                        audience,
                        recipientId,
                        recipientEmail,
                        recipientPhone,
                        data,
                        priority,
                        cancellationToken
                    ));
                }

                // Send SMS
                if (sendSms && !string.IsNullOrEmpty(recipientPhone))
                {
                    tasks.Add(SendNotificationAsync(
                        $"{templateCodePrefix}_SMS",
                        NotificationChannel.Sms,
                        audience,
                        recipientId,
                        recipientEmail,
                        recipientPhone,
                        data,
                        priority,
                        cancellationToken
                    ));
                }

                // Send In-App
                if (sendInApp)
                {
                    tasks.Add(SendNotificationAsync(
                        $"{templateCodePrefix}_INAPP",
                        NotificationChannel.InApp,
                        audience,
                        recipientId,
                        recipientEmail,
                        recipientPhone,
                        data,
                        priority,
                        cancellationToken
                    ));
                }

                // Wait for all notifications to be queued
                await Task.WhenAll(tasks);

                _logger.LogInformation(
                    "Multi-channel notification sent: {TemplatePrefix} | Channels: Email={Email}, SMS={SMS}, InApp={InApp} | Recipient: {RecipientId}",
                    templateCodePrefix, sendEmail, sendSms, sendInApp, recipientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send multi-channel notification: {TemplatePrefix} | Recipient: {RecipientId}",
                    templateCodePrefix, recipientId);
                // Don't throw - notifications should never block business logic
            }
        }

        /// <summary>
        /// Sends notification to admin panel (in-app notification)
        /// </summary>
        /// <param name="notificationType">Type of notification (e.g., "NEW_PARTNER_REGISTRATION")</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="entityId">Related entity ID</param>
        /// <param name="entityType">Entity type (e.g., "OWNER", "ORDER")</param>
        /// <param name="link">Optional link to related page</param>
        /// <param name="adminId">Specific admin ID (null = broadcast to all admins)</param>
        /// <returns>Success status</returns>
        public bool SendAdminNotification(
            string notificationType,
            string title,
            string message,
            long? entityId = null,
            string? entityType = null,
            string? link = null,
            long? adminId = null)
        {
            try
            {
                var adminNotificationRepo = new AdminNotificationRepository(_dbHelper);
                return adminNotificationRepo.CreateNotification(
                    notificationType,
                    title,
                    message,
                    entityId,
                    entityType,
                    link,
                    adminId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin notification: {Type}", notificationType);
                return false;
            }
        }

        /// <summary>
        /// Sends notification for a single channel
        /// </summary>
        private async Task SendNotificationAsync(
            string templateCode,
            Domain.Models.Notification.NotificationChannel channel,
            string audience,
            string recipientId,
            string? recipientEmail,
            string? recipientPhone,
            Dictionary<string, object> data,
            Domain.Models.Notification.NotificationPriority priority,
            CancellationToken cancellationToken)
        {
            try
            {
                // Parse audience string to enum
                var audienceEnum = audience.ToUpper() switch
                {
                    "ADMIN" => Domain.Models.Notification.NotificationAudience.Admin,
                    "PARTNER" => Domain.Models.Notification.NotificationAudience.Partner,
                    "USER" => Domain.Models.Notification.NotificationAudience.User,
                    _ => Domain.Models.Notification.NotificationAudience.User
                };

                // Create notification message
                var notification = new NotificationMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Channel = channel,
                    Audience = audienceEnum,
                    Priority = priority,
                    Category = ExtractCategory(templateCode),
                    Recipient = new NotificationRecipient
                    {
                        Id = recipientId,
                        Email = recipientEmail,
                        Phone = recipientPhone,
                        PreferredLanguage = "en"
                    },
                    TemplateCode = templateCode,
                    Data = data,
                    Options = new NotificationOptions
                    {
                        TrackOpens = channel == Domain.Models.Notification.NotificationChannel.Email,
                        TrackClicks = channel == Domain.Models.Notification.NotificationChannel.Email
                    },
                    RetryConfig = new RetryConfiguration
                    {
                        MaxAttempts = 3,
                        BackoffMultiplier = 2.0
                    },
                    Source = new NotificationSource
                    {
                        Service = "CateringEcommerce",
                        Action = templateCode
                    }
                };

                // TODO: Publish to RabbitMQ queue when available
                // For now, log the notification
                await PublishToQueueAsync(notification, cancellationToken);

                _logger.LogInformation(
                    "Notification queued: {TemplateCode} | Channel: {Channel} | Recipient: {RecipientId}",
                    templateCode, channel, recipientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to queue notification: {TemplateCode} | Recipient: {RecipientId}",
                    templateCode, recipientId);
                throw; // Re-throw to be caught by caller
            }
        }

        /// <summary>
        /// Extracts category from template code
        /// </summary>
        private string ExtractCategory(string templateCode)
        {
            // Extract category from template code
            // E.g., "ORDER_CONFIRMATION_EMAIL" -> "ORDER"
            var parts = templateCode.Split('_');
            return parts.Length > 0 ? parts[0] : "GENERAL";
        }

        /// <summary>
        /// Publishes notification to RabbitMQ queue (or logs if RabbitMQ not available)
        /// </summary>
        private async Task PublishToQueueAsync(NotificationMessage notification, CancellationToken cancellationToken)
        {
            try
            {
                var queueName = notification.Channel switch
                {
                    Domain.Models.Notification.NotificationChannel.Email => "email.queue",
                    Domain.Models.Notification.NotificationChannel.Sms => "sms.queue",
                    Domain.Models.Notification.NotificationChannel.InApp => "inapp.queue",
                    _ => "notifications.queue"
                };

                // Use RabbitMQ if available, otherwise just log
                if (_rabbitMQPublisher != null && _rabbitMQPublisher.IsConnected())
                {
                    await _rabbitMQPublisher.PublishAsync(queueName, notification, cancellationToken);
                    _logger.LogDebug(
                        "Notification published to RabbitMQ queue '{Queue}': {MessageId}",
                        queueName, notification.MessageId);
                }
                else
                {
                    // Fallback: Serialize and log for debugging
                    var messageJson = JsonSerializer.Serialize(notification, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });

                    _logger.LogInformation(
                        "RabbitMQ not available. Notification logged for queue '{Queue}': {TemplateCode} | Recipient: {RecipientId} | Channel: {Channel}",
                        queueName, notification.TemplateCode, notification.Recipient?.Id, notification.Channel);

                    // In production without RabbitMQ, you could directly call notification services here:
                    // - For Email: Call EmailService directly
                    // - For SMS: Call SmsService directly
                    // - For InApp: Call InAppNotificationService directly
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish notification to queue");
                throw;
            }
        }

        /// <summary>
        /// Sends order-related notification (convenience method)
        /// </summary>
        public async Task SendOrderNotificationAsync(
            string templatePrefix,
            string customerName,
            string customerEmail,
            string customerPhone,
            string? partnerName,
            string? partnerEmail,
            string? partnerPhone,
            Dictionary<string, object> orderData,
            bool notifyCustomer = true,
            bool notifyPartner = true,
            bool notifyAdmin = false)
        {
            var tasks = new List<Task>();

            // Notify customer
            if (notifyCustomer)
            {
                tasks.Add(SendMultiChannelNotificationAsync(
                    templatePrefix,
                    "USER",
                    orderData.ContainsKey("order_id") ? orderData["order_id"].ToString()! : Guid.NewGuid().ToString(),
                    customerEmail,
                    customerPhone,
                    orderData,
                    sendEmail: true,
                    sendSms: true,
                    sendInApp: true
                ));
            }

            // Notify partner
            if (notifyPartner && !string.IsNullOrEmpty(partnerEmail))
            {
                var partnerTemplatePrefix = templatePrefix + "_PARTNER";
                tasks.Add(SendMultiChannelNotificationAsync(
                    partnerTemplatePrefix,
                    "PARTNER",
                    orderData.ContainsKey("partner_id") ? orderData["partner_id"].ToString()! : Guid.NewGuid().ToString(),
                    partnerEmail,
                    partnerPhone,
                    orderData,
                    sendEmail: true,
                    sendSms: true,
                    sendInApp: true
                ));
            }

            // Notify admin
            if (notifyAdmin && orderData.ContainsKey("order_number"))
            {
                SendAdminNotification(
                    $"ADMIN_{templatePrefix}",
                    $"New {templatePrefix}",
                    $"Order #{orderData["order_number"]} - {customerName}",
                    orderData.ContainsKey("order_id") ? Convert.ToInt64(orderData["order_id"]) : null,
                    "ORDER"
                );
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Sends payment-related notification (convenience method)
        /// </summary>
        public async Task SendPaymentNotificationAsync(
            string templatePrefix,
            string customerName,
            string customerEmail,
            string customerPhone,
            Dictionary<string, object> paymentData,
            bool notifyAdmin = false)
        {
            await SendMultiChannelNotificationAsync(
                templatePrefix,
                "USER",
                paymentData.ContainsKey("user_id") ? paymentData["user_id"].ToString()! : Guid.NewGuid().ToString(),
                customerEmail,
                customerPhone,
                paymentData,
                sendEmail: true,
                sendSms: true,
                sendInApp: true
            );

            // Notify admin for failed payments
            if (notifyAdmin && paymentData.ContainsKey("order_number"))
            {
                SendAdminNotification(
                    $"ADMIN_{templatePrefix}",
                    $"Payment Event: {templatePrefix}",
                    $"Order #{paymentData["order_number"]} - {customerName} - Rs.{paymentData.GetValueOrDefault("amount", 0)}",
                    paymentData.ContainsKey("order_id") ? Convert.ToInt64(paymentData["order_id"]) : null,
                    "PAYMENT"
                );
            }
        }

        /// <summary>
        /// Sends partner-related notification (convenience method)
        /// </summary>
        public async Task SendPartnerNotificationAsync(
            string templatePrefix,
            string ownerName,
            string ownerEmail,
            string ownerPhone,
            Dictionary<string, object> partnerData)
        {
            await SendMultiChannelNotificationAsync(
                templatePrefix,
                "PARTNER",
                partnerData.ContainsKey("owner_id") ? partnerData["owner_id"].ToString()! : Guid.NewGuid().ToString(),
                ownerEmail,
                ownerPhone,
                partnerData,
                sendEmail: true,
                sendSms: true,
                sendInApp: false // Partners may not have in-app yet
            );
        }
    }
}
