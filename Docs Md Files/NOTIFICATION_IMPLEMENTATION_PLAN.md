# Comprehensive Notification System Implementation Plan

## Executive Summary

**Current Status**: 10% Complete (5 of 50+ notification types implemented)
**Target**: 100% Complete with Email, SMS, and In-App notifications
**Timeline**: 15-20 days

---

## Phase 1: Foundation & Templates (Days 1-3)

### 1.1 Database Setup

**Migration File**: `Database/Notification_Templates_Migration.sql`

```sql
-- Notification Templates Table (if not exists)
CREATE TABLE IF NOT EXISTS t_sys_notification_templates (
    c_template_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_template_code NVARCHAR(100) NOT NULL UNIQUE,
    c_template_name NVARCHAR(200) NOT NULL,
    c_language NVARCHAR(10) NOT NULL DEFAULT 'en',
    c_channel NVARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP
    c_category NVARCHAR(50) NOT NULL,
    c_subject NVARCHAR(500) NULL,
    c_body NVARCHAR(MAX) NOT NULL,
    c_version INT NOT NULL DEFAULT 1,
    c_is_active BIT NOT NULL DEFAULT 1,
    c_usage_count INT NOT NULL DEFAULT 0,
    c_created_date DATETIME NOT NULL DEFAULT GETDATE(),
    c_modified_date DATETIME NULL
);

-- Insert all notification templates
INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
-- USER REGISTRATION & AUTHENTICATION
('USER_REGISTRATION_WELCOME_EMAIL', 'User Registration Welcome Email', 'en', 'EMAIL', 'USER_REGISTRATION',
'Welcome to Enyvora Catering, {{ user_name }}!',
'<h1>Welcome {{ user_name }}!</h1><p>Thank you for registering with Enyvora Catering...</p>'),

('USER_REGISTRATION_WELCOME_SMS', 'User Registration Welcome SMS', 'en', 'SMS', 'USER_REGISTRATION',
NULL,
'Welcome {{ user_name }}! Your account has been created successfully. Explore delicious catering options at Enyvora.'),

('USER_OTP_SMS', 'User OTP Verification', 'en', 'SMS', 'OTP',
NULL,
'Your OTP for Enyvora Catering is {{ otp }}. Valid for {{ validity_minutes }} minutes. Do not share this code.'),

-- ORDER LIFECYCLE
('ORDER_CONFIRMATION_EMAIL', 'Order Confirmation Email', 'en', 'EMAIL', 'ORDER',
'Order Confirmed #{{ order_number }}',
'<h2>Order Confirmed!</h2><p>Dear {{ customer_name }},</p><p>Your order #{{ order_number }} has been confirmed...</p>'),

('ORDER_CONFIRMATION_SMS', 'Order Confirmation SMS', 'en', 'SMS', 'ORDER',
NULL,
'Order #{{ order_number }} confirmed! Event: {{ event_date }} | Guests: {{ guest_count }} | Total: Rs.{{ total_amount }}'),

('ORDER_CONFIRMATION_INAPP', 'Order Confirmation In-App', 'en', 'INAPP', 'ORDER',
'Order Confirmed',
'Your order #{{ order_number }} has been confirmed for {{ event_date }}'),

('ORDER_ASSIGNED_PARTNER_EMAIL', 'Order Assigned to Partner', 'en', 'EMAIL', 'ORDER',
'New Order Assigned #{{ order_number }}',
'<h2>New Order Assigned</h2><p>Dear {{ partner_name }},</p><p>A new order #{{ order_number }} has been assigned to you...</p>'),

('ORDER_ASSIGNED_PARTNER_SMS', 'Order Assigned SMS', 'en', 'SMS', 'ORDER',
NULL,
'New order #{{ order_number }} assigned! Event: {{ event_date }} | Guests: {{ guest_count }} | Location: {{ location }}'),

('ORDER_STATUS_UPDATE_EMAIL', 'Order Status Update', 'en', 'EMAIL', 'ORDER',
'Order #{{ order_number }} - {{ status }}',
'<h2>Order Status Updated</h2><p>Dear {{ customer_name }},</p><p>Your order status: {{ status }}</p>'),

('ORDER_CANCELLATION_EMAIL', 'Order Cancellation Email', 'en', 'EMAIL', 'ORDER',
'Order Cancelled #{{ order_number }}',
'<h2>Order Cancelled</h2><p>Dear {{ customer_name }},</p><p>Your order #{{ order_number }} has been cancelled...</p>'),

('ORDER_CANCELLATION_SMS', 'Order Cancellation SMS', 'en', 'SMS', 'ORDER',
NULL,
'Order #{{ order_number }} cancelled. Refund will be processed within 5-7 business days.'),

-- PAYMENT
('PAYMENT_SUCCESS_EMAIL', 'Payment Success Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Successful for Order #{{ order_number }}',
'<h2>Payment Received!</h2><p>Dear {{ customer_name }},</p><p>Payment of Rs.{{ amount }} received for order #{{ order_number }}...</p>'),

('PAYMENT_SUCCESS_SMS', 'Payment Success SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Payment of Rs.{{ amount }} successful for order #{{ order_number }}. Transaction ID: {{ transaction_id }}'),

('PAYMENT_FAILED_EMAIL', 'Payment Failed Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Failed for Order #{{ order_number }}',
'<h2>Payment Failed</h2><p>Dear {{ customer_name }},</p><p>Payment failed for order #{{ order_number }}. Reason: {{ reason }}</p>'),

('PAYMENT_FAILED_SMS', 'Payment Failed SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Payment failed for order #{{ order_number }}. Please retry or contact support.'),

('PAYMENT_REMINDER_EMAIL', 'Payment Reminder Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Due for Order #{{ order_number }}',
'<h2>Payment Reminder</h2><p>Dear {{ customer_name }},</p><p>Pending payment of Rs.{{ amount }} due on {{ due_date }}</p>'),

('PAYMENT_REMINDER_SMS', 'Payment Reminder SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Reminder: Rs.{{ amount }} pending for order #{{ order_number }}. Due: {{ due_date }}. Pay now to avoid delays.'),

('REFUND_INITIATED_EMAIL', 'Refund Initiated Email', 'en', 'EMAIL', 'PAYMENT',
'Refund Initiated for Order #{{ order_number }}',
'<h2>Refund Initiated</h2><p>Dear {{ customer_name }},</p><p>Refund of Rs.{{ amount }} initiated. Will be credited in 5-7 business days.</p>'),

-- PARTNER MANAGEMENT
('PARTNER_REGISTRATION_ACK_EMAIL', 'Partner Registration Acknowledgement', 'en', 'EMAIL', 'PARTNER',
'Registration Received - Enyvora Catering Partner',
'<h2>Thank You for Registering!</h2><p>Dear {{ owner_name }},</p><p>Your catering business "{{ catering_name }}" registration received...</p>'),

('PARTNER_REGISTRATION_ACK_SMS', 'Partner Registration SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Thank you {{ owner_name }}! Your registration received. Our team will review and contact you within 24-48 hours.'),

('PARTNER_APPROVAL_EMAIL', 'Partner Approval Email', 'en', 'EMAIL', 'PARTNER',
'Congratulations! Your Catering Business Approved',
'<h2>Welcome to Enyvora!</h2><p>Dear {{ owner_name }},</p><p>Your business "{{ catering_name }}" has been approved...</p>'),

('PARTNER_APPROVAL_SMS', 'Partner Approval SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Congratulations {{ owner_name }}! Your catering business approved. Login now to start receiving orders.'),

('PARTNER_REJECTION_EMAIL', 'Partner Rejection Email', 'en', 'EMAIL', 'PARTNER',
'Application Update - Enyvora Catering',
'<h2>Application Status</h2><p>Dear {{ owner_name }},</p><p>Thank you for your interest. Unfortunately, we cannot approve your application at this time. Reason: {{ reason }}</p>'),

('PARTNER_INFO_REQUEST_EMAIL', 'Partner Info Request Email', 'en', 'EMAIL', 'PARTNER',
'Additional Information Required',
'<h2>Information Request</h2><p>Dear {{ owner_name }},</p><p>We need additional information: {{ info_requested }}</p>'),

('PARTNER_DEACTIVATION_EMAIL', 'Partner Deactivation Email', 'en', 'EMAIL', 'PARTNER',
'Account Deactivated - Enyvora Catering',
'<h2>Account Deactivated</h2><p>Dear {{ owner_name }},</p><p>Your account has been deactivated. Reason: {{ reason }}</p>'),

-- DELIVERY
('SAMPLE_DELIVERY_SCHEDULED_EMAIL', 'Sample Delivery Scheduled', 'en', 'EMAIL', 'DELIVERY',
'Sample Delivery Scheduled for {{ delivery_date }}',
'<h2>Sample Delivery Confirmed</h2><p>Dear {{ customer_name }},</p><p>Sample delivery scheduled for {{ delivery_date }} at {{ delivery_time }}</p>'),

('EVENT_DELIVERY_SCHEDULED_EMAIL', 'Event Delivery Scheduled', 'en', 'EMAIL', 'DELIVERY',
'Event Delivery Confirmed for {{ delivery_date }}',
'<h2>Delivery Scheduled</h2><p>Dear {{ customer_name }},</p><p>Your order will be delivered on {{ delivery_date }} at {{ delivery_time }}</p>'),

('DELIVERY_REMINDER_SMS', 'Delivery Reminder SMS', 'en', 'SMS', 'DELIVERY',
NULL,
'Reminder: Your order #{{ order_number }} will be delivered tomorrow at {{ delivery_time }}. Address: {{ address }}'),

('DELIVERY_COMPLETED_EMAIL', 'Delivery Completed Email', 'en', 'EMAIL', 'DELIVERY',
'Order Delivered Successfully #{{ order_number }}',
'<h2>Delivery Complete</h2><p>Dear {{ customer_name }},</p><p>Your order has been delivered. Hope you enjoyed our service!</p>'),

-- ADMIN NOTIFICATIONS
('ADMIN_NEW_PARTNER_REGISTRATION', 'Admin - New Partner Registration', 'en', 'INAPP', 'ADMIN',
'New Partner Registration',
'New catering partner "{{ catering_name }}" registered. Contact: {{ owner_name }} ({{ mobile }})'),

('ADMIN_NEW_ORDER', 'Admin - New Order Placed', 'en', 'INAPP', 'ADMIN',
'New Order Placed',
'Order #{{ order_number }} placed by {{ customer_name }}. Amount: Rs.{{ amount }}'),

('ADMIN_PAYMENT_FAILED', 'Admin - Payment Failed Alert', 'en', 'INAPP', 'ADMIN',
'Payment Failed Alert',
'Payment failed for order #{{ order_number }}. Customer: {{ customer_name }}'),

('ADMIN_REVIEW_MODERATION', 'Admin - Review Moderation Required', 'en', 'INAPP', 'ADMIN',
'Review Needs Moderation',
'New review by {{ customer_name }} for {{ partner_name }}. Rating: {{ rating }} stars'),

-- REVIEWS
('REVIEW_PARTNER_NOTIFICATION', 'Partner - New Review Received', 'en', 'INAPP', 'REVIEW',
'New Review Received',
'{{ customer_name }} rated you {{ rating }} stars. View details to respond.'),

('REVIEW_RESPONSE_USER_EMAIL', 'User - Partner Responded to Review', 'en', 'EMAIL', 'REVIEW',
'{{ partner_name }} Responded to Your Review',
'<h2>Partner Response</h2><p>Dear {{ customer_name }},</p><p>{{ partner_name }} responded to your review...</p>'),

-- SYSTEM ANNOUNCEMENTS
('SYSTEM_MAINTENANCE_EMAIL', 'System Maintenance Announcement', 'en', 'EMAIL', 'SYSTEM',
'Scheduled Maintenance - {{ maintenance_date }}',
'<h2>Scheduled Maintenance</h2><p>Our system will be under maintenance on {{ maintenance_date }} from {{ start_time }} to {{ end_time }}</p>'),

('PROMOTION_EMAIL', 'Promotional Email', 'en', 'EMAIL', 'PROMOTION',
'{{ promotion_title }}',
'<h2>{{ promotion_title }}</h2><p>{{ promotion_message }}</p>');
```

### 1.2 Create Notification Helper Service

**File**: `CateringEcommerce.BAL/Helpers/NotificationHelper.cs`

```csharp
using CateringEcommerce.Domain.Models.Notification;
using CateringEcommerce.Domain.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Helpers
{
    public class NotificationHelper
    {
        private readonly ILogger<NotificationHelper> _logger;

        public NotificationHelper(ILogger<NotificationHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends multi-channel notification (Email + SMS + In-App)
        /// </summary>
        public async Task SendMultiChannelNotificationAsync(
            string templateCodePrefix, // e.g., "ORDER_CONFIRMATION"
            string audience, // "USER", "PARTNER", "ADMIN"
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

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Sends single channel notification
        /// </summary>
        private async Task SendNotificationAsync(
            string templateCode,
            NotificationChannel channel,
            string audience,
            string recipientId,
            string? recipientEmail,
            string? recipientPhone,
            Dictionary<string, object> data,
            NotificationPriority priority,
            CancellationToken cancellationToken)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    Channel = channel,
                    Audience = audience,
                    Priority = priority,
                    Category = ExtractCategory(templateCode),
                    Recipient = new NotificationRecipient
                    {
                        Id = recipientId,
                        Email = recipientEmail,
                        Phone = recipientPhone
                    },
                    TemplateCode = templateCode,
                    Data = data,
                    Options = new NotificationOptions
                    {
                        TrackOpens = channel == NotificationChannel.Email,
                        TrackClicks = channel == NotificationChannel.Email
                    }
                };

                // Publish to RabbitMQ queue based on channel
                // (This will be handled by the existing queue consumers)
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
            }
        }

        private string ExtractCategory(string templateCode)
        {
            // Extract category from template code
            // E.g., "ORDER_CONFIRMATION_EMAIL" -> "ORDER"
            var parts = templateCode.Split('_');
            return parts.Length > 0 ? parts[0] : "GENERAL";
        }

        private async Task PublishToQueueAsync(NotificationMessage notification, CancellationToken cancellationToken)
        {
            // TODO: Implement RabbitMQ publishing
            // This should use the existing RabbitMQ infrastructure
            await Task.CompletedTask;
        }
    }
}
```

---

## Phase 2: Controller Integration (Days 4-10)

### 2.1 User Controllers

#### **File**: `CateringEcommerce.API/Controllers/User/AuthController.cs`

**Events to Implement**:
1. ✅ OTP Send (Already implemented)
2. ❌ User Registration Welcome (MISSING)
3. ❌ Password Reset (MISSING)

**Implementation**:
```csharp
// After successful registration
await _notificationHelper.SendMultiChannelNotificationAsync(
    "USER_REGISTRATION_WELCOME",
    "USER",
    userId.ToString(),
    email,
    mobile,
    new Dictionary<string, object>
    {
        { "user_name", userName },
        { "registration_date", DateTime.Now.ToString("dd MMM yyyy") }
    },
    sendEmail: true,
    sendSms: true,
    sendInApp: true
);
```

#### **File**: `CateringEcommerce.API/Controllers/User/OrdersController.cs`

**Events to Implement**:
1. ✅ Order Confirmation (Migrate from legacy)
2. ✅ Order Cancellation (Migrate from legacy)
3. ❌ Order Status Updates (MISSING)
4. ❌ Delivery Scheduled (MISSING)

#### **File**: `CateringEcommerce.API/Controllers/User/PaymentGatewayController.cs`

**Events to Implement**:
1. ❌ Payment Success (MISSING)
2. ❌ Payment Failed (MISSING)
3. ❌ Refund Initiated (MISSING)

### 2.2 Partner Controllers

#### **File**: `CateringEcommerce.API/Controllers/Owner/RegistrationController.cs`

**Events to Implement**:
1. ❌ Partner Registration Acknowledgement (MISSING - added admin notification only)
2. ❌ Partner needs to receive email/SMS too

**Implementation**:
```csharp
// After successful registration (line 167)
await _notificationHelper.SendMultiChannelNotificationAsync(
    "PARTNER_REGISTRATION_ACK",
    "PARTNER",
    ownerPkid.ToString(),
    registrationData.Email,
    registrationData.Mobile,
    new Dictionary<string, object>
    {
        { "owner_name", registrationData.OwnerName },
        { "catering_name", registrationData.CateringName },
        { "registration_date", DateTime.Now.ToString("dd MMM yyyy") }
    }
);

// Send admin notification (already added)
```

#### **File**: `CateringEcommerce.API/Controllers/Owner/OrdersController.cs`

**Events to Implement**:
1. ❌ Order Assigned to Partner (MISSING)
2. ❌ Order Modification Requests (MISSING)

### 2.3 Admin Controllers

#### **File**: `CateringEcommerce.API/Controllers/Admin/AdminPartnerRequestsController.cs`

**Events to Implement**:
1. ✅ Partner Approval (Code exists, needs integration)
2. ✅ Partner Rejection (Code exists, needs integration)
3. ✅ Info Request (Code exists, needs integration)

**Integrate existing methods**:
```csharp
// Replace legacy SendApprovalCommunication() with:
await _notificationHelper.SendMultiChannelNotificationAsync(
    "PARTNER_APPROVAL",
    "PARTNER",
    ownerId.ToString(),
    partnerEmail,
    partnerMobile,
    new Dictionary<string, object>
    {
        { "owner_name", ownerName },
        { "catering_name", cateringName },
        { "approval_date", DateTime.Now.ToString("dd MMM yyyy") },
        { "login_url", "https://yourapp.com/partner/login" }
    }
);
```

---

## Phase 3: Background Jobs & Scheduled Notifications (Days 11-12)

### 3.1 Payment Reminder Job (Migrate)

**File**: `CateringEcommerce.BAL/Jobs/PaymentReminderJob.cs`

**Current**: Uses legacy NotificationService
**Migration**: Use NotificationHelper with templates

### 3.2 Delivery Reminder Job (NEW)

**File**: `CateringEcommerce.BAL/Jobs/DeliveryReminderJob.cs`

**Schedule**: Daily at 8:00 AM
**Logic**: Send reminder 1 day before event delivery

### 3.3 Review Request Job (NEW)

**File**: `CateringEcommerce.BAL/Jobs/ReviewRequestJob.cs`

**Schedule**: Daily at 10:00 AM
**Logic**: Send review request 1 day after event completion

---

## Phase 4: Testing & Validation (Days 13-15)

### 4.1 Unit Tests

- Test NotificationHelper
- Test template rendering
- Test multi-channel delivery

### 4.2 Integration Tests

- Test end-to-end notification flow
- Test queue processing
- Test provider failover

### 4.3 Manual Testing

- Register user → Verify email/SMS/in-app
- Place order → Verify all parties notified
- Cancel order → Verify notifications
- Partner approval → Verify email/SMS

---

## Phase 5: Monitoring & Analytics (Days 16-17)

### 5.1 Notification Dashboard

- Delivery success rates by channel
- Failed notification alerts
- Template usage statistics

### 5.2 Logging & Alerts

- Failed delivery alerts to admin
- Provider downtime alerts
- Rate limit warnings

---

## Implementation Priority Matrix

### P0 - Critical (Implement First)
1. Order Confirmation (Email + SMS + In-App) - User & Partner
2. Payment Success/Failed (Email + SMS) - User
3. Partner Registration Acknowledgement (Email + SMS) - Partner
4. Partner Approval/Rejection (Email + SMS) - Partner
5. Admin notifications for critical events

### P1 - High Priority
1. Order Status Updates
2. Delivery Scheduled/Reminder
3. Payment Reminders
4. Order Cancellation notifications

### P2 - Medium Priority
1. Review notifications
2. Profile update confirmations
3. System maintenance announcements

### P3 - Low Priority
1. Promotional emails
2. Newsletter
3. Analytics notifications

---

## Key Implementation Files

### Backend
- **NEW**: `CateringEcommerce.BAL/Helpers/NotificationHelper.cs`
- **MODIFY**: All controller files (32 controllers)
- **MODIFY**: `PaymentReminderJob.cs` (migrate to new architecture)
- **NEW**: `DeliveryReminderJob.cs`
- **NEW**: `ReviewRequestJob.cs`

### Database
- **NEW**: `Database/Notification_Templates_Migration.sql`
- **MODIFY**: Insert all 40+ templates

### Configuration
- **VERIFY**: `appsettings.json` - RabbitMQ, Email, SMS settings
- **CONFIGURE**: Template cache settings
- **CONFIGURE**: Rate limits

---

## Success Metrics

- **Delivery Rate**: >95% for all channels
- **Latency**: <5 seconds from trigger to queue
- **Coverage**: 100% of critical business events
- **User Satisfaction**: Opt-out rate <5%

---

## Rollback Plan

If notification system causes issues:
1. Disable RabbitMQ consumers
2. Fallback to legacy NotificationService
3. Investigate queue backlog
4. Resume after fix

---

## Dependencies

- RabbitMQ running and accessible
- SendGrid API key configured
- Twilio API credentials configured
- SignalR hubs registered in Program.cs
- Database migration executed

---

## Notes

- All notifications should be asynchronous (fire-and-forget)
- Never block controller responses waiting for notifications
- Log all notification failures for debugging
- Respect user notification preferences (when implemented)
- Support multi-language templates (future enhancement)
