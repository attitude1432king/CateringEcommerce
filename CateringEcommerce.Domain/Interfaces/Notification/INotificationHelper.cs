using CateringEcommerce.Domain.Models.Notification;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface INotificationHelper
    {
        /// <summary>
        /// Sends multi-channel notification (Email + SMS + In-App)
        /// </summary>
        Task SendMultiChannelNotificationAsync(
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
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends notification to admin panel (in-app notification)
        /// </summary>
        bool SendAdminNotification(
            string notificationType,
            string title,
            string message,
            long? entityId = null,
            string? entityType = null,
            string? link = null,
            long? adminId = null);

        /// <summary>
        /// Sends order-related notification (convenience method)
        /// </summary>
        Task SendOrderNotificationAsync(
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
            bool notifyAdmin = false);

        /// <summary>
        /// Sends payment-related notification (convenience method)
        /// </summary>
        Task SendPaymentNotificationAsync(
            string templatePrefix,
            string customerName,
            string customerEmail,
            string customerPhone,
            Dictionary<string, object> paymentData,
            bool notifyAdmin = false);

        /// <summary>
        /// Sends partner-related notification (convenience method)
        /// </summary>
        Task SendPartnerNotificationAsync(
            string templatePrefix,
            string ownerName,
            string ownerEmail,
            string ownerPhone,
            Dictionary<string, object> partnerData);
    }
}
