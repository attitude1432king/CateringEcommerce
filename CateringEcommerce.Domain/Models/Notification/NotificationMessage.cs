namespace CateringEcommerce.Domain.Models.Notification
{
    /// <summary>
    /// Represents a unified notification message that can be sent via multiple channels (Email, SMS, In-App).
    /// This class handles routing, templating, and delivery configuration.
    /// </summary>
    public class NotificationMessage
    {
        #region Message Metadata

        /// <summary>
        /// Unique message ID (UUID) for tracking and identification.
        /// </summary>
        public required string MessageId { get; init; }

        /// <summary>
        /// Correlation ID for tracing across microservices and systems.
        /// </summary>
        public required string CorrelationId { get; init; }

        /// <summary>
        /// Message creation timestamp (UTC).
        /// </summary>
        public required DateTime Timestamp { get; init; }

        #endregion

        #region Routing Information

        /// <summary>
        /// Delivery channel for this notification (Email, SMS, or In-App).
        /// </summary>
        public required NotificationChannel Channel { get; init; }

        /// <summary>
        /// Target audience for this notification (Admin, Partner, or User).
        /// </summary>
        public required NotificationAudience Audience { get; init; }

        /// <summary>
        /// Priority level of the notification (Immediate, High, Normal, Low).
        /// </summary>
        public required NotificationPriority Priority { get; init; }

        /// <summary>
        /// Category/type of notification (e.g., 'ORDER_CONFIRMATION', 'OTP', 'PARTNER_APPROVAL').
        /// </summary>
        public required string Category { get; init; }

        #endregion

        #region Recipient Information

        /// <summary>
        /// Recipient details including ID, contact information, and preferences.
        /// </summary>
        public required NotificationRecipient Recipient { get; init; }

        #endregion

        #region Template Information

        /// <summary>
        /// Template code to identify the notification template (e.g., 'PARTNER_APPROVAL_EMAIL').
        /// </summary>
        public required string TemplateCode { get; init; }

        /// <summary>
        /// Template version for versioning and A/B testing (defaults to latest if null).
        /// </summary>
        public int? TemplateVersion { get; init; }

        #endregion

        #region Dynamic Data

        /// <summary>
        /// Dynamic data containing placeholder values for template rendering.
        /// Example: { "partnerName": "John Doe", "approvalDate": "2024-01-15" }
        /// </summary>
        public required Dictionary<string, object?> Data { get; init; }

        #endregion

        #region Optional Configurations

        /// <summary>
        /// Optional delivery and rendering configurations.
        /// </summary>
        public NotificationOptions? Options { get; init; }

        #endregion

        #region Retry Configuration

        /// <summary>
        /// Retry strategy configuration for failed delivery attempts.
        /// </summary>
        public RetryConfiguration? RetryConfig { get; init; }

        #endregion

        #region Source Information

        /// <summary>
        /// Source information for auditing and tracking where the notification was triggered from.
        /// </summary>
        public required NotificationSource Source { get; init; }

        #endregion
    }

    /// <summary>
    /// Represents notification recipient details.
    /// </summary>
    public class NotificationRecipient
    {
        /// <summary>
        /// Unique identifier for the recipient (User ID, Admin ID, or Partner ID).
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Email address of the recipient (required for EMAIL channel).
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Phone number of the recipient (required for SMS channel).
        /// </summary>
        public string? Phone { get; init; }

        /// <summary>
        /// Device tokens for push notifications (future enhancement).
        /// </summary>
        public List<string>? DeviceTokens { get; init; }

        /// <summary>
        /// Preferred language for the notification (e.g., 'en', 'hi', 'es').
        /// </summary>
        public string? PreferredLanguage { get; init; }
    }

    /// <summary>
    /// Represents optional notification delivery and rendering configurations.
    /// </summary>
    public class NotificationOptions
    {
        /// <summary>
        /// Optional delay before sending the notification.
        /// </summary>
        public DateTime? SendAfter { get; init; }

        /// <summary>
        /// Timestamp after which the notification expires and should not be sent.
        /// </summary>
        public DateTime? ExpiresAt { get; init; }

        /// <summary>
        /// Whether to track email open events (default: false).
        /// </summary>
        public bool TrackOpens { get; init; }

        /// <summary>
        /// Whether to track link click events (default: false).
        /// </summary>
        public bool TrackClicks { get; init; }

        /// <summary>
        /// Email attachments (only for EMAIL channel).
        /// </summary>
        public List<EmailAttachment>? Attachments { get; init; }

        /// <summary>
        /// Reply-to email address (only for EMAIL channel).
        /// </summary>
        public string? ReplyTo { get; init; }

        /// <summary>
        /// Custom sender information (optional override of default).
        /// </summary>
        public SenderInfo? From { get; init; }
    }

    /// <summary>
    /// Represents sender information for email notifications.
    /// </summary>
    public class SenderInfo
    {
        /// <summary>
        /// Display name of the sender.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Email address of the sender.
        /// </summary>
        public string? Email { get; init; }
    }

    /// <summary>
    /// Represents retry strategy configuration for delivery attempts.
    /// </summary>
    public class RetryConfiguration
    {
        /// <summary>
        /// Maximum number of delivery attempts (default: 3).
        /// </summary>
        public int MaxAttempts { get; init; } = 3;

        /// <summary>
        /// Multiplier for exponential backoff between retries (default: 2).
        /// </summary>
        public double BackoffMultiplier { get; init; } = 2.0;
    }

    /// <summary>
    /// Represents source information for auditing notification triggers.
    /// </summary>
    public class NotificationSource
    {
        /// <summary>
        /// Service name that triggered the notification (e.g., 'AdminService', 'PartnerService', 'OrderService').
        /// </summary>
        public required string Service { get; init; }

        /// <summary>
        /// Action that triggered the notification (e.g., 'PARTNER_APPROVED', 'ORDER_PLACED', 'OTP_GENERATED').
        /// </summary>
        public required string Action { get; init; }

        /// <summary>
        /// Admin ID if this notification was triggered by an admin action.
        /// </summary>
        public string? AdminId { get; init; }

        /// <summary>
        /// IP address from which the action was triggered (for security auditing).
        /// </summary>
        public string? IpAddress { get; init; }
    }

    /// <summary>
    /// Enumeration for notification channels.
    /// </summary>
    public enum NotificationChannel
    {
        /// <summary>Email notification via email provider.</summary>
        Email = 0,

        /// <summary>SMS notification via SMS provider.</summary>
        Sms = 1,

        /// <summary>In-app notification stored in user's notification center.</summary>
        InApp = 2
    }

    /// <summary>
    /// Enumeration for target audience types.
    /// </summary>
    public enum NotificationAudience
    {
        /// <summary>System administrators.</summary>
        Admin = 0,

        /// <summary>Catering service partners/owners.</summary>
        Partner = 1,

        /// <summary>End users/customers.</summary>
        User = 2
    }

    /// <summary>
    /// Enumeration for notification priority levels.
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>Low priority - can be delayed or batched.</summary>
        Low = 0,

        /// <summary>Normal priority - standard delivery.</summary>
        Normal = 1,

        /// <summary>High priority - expedited delivery.</summary>
        High = 2,

        /// <summary>Immediate priority - critical, send immediately (e.g., OTP, security alerts).</summary>
        Immediate = 3
    }
}
