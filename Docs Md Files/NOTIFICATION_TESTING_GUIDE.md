# Notification System - End-to-End Testing Guide

## 🎯 Testing Overview

This guide provides comprehensive test scenarios for the notification system covering all 20+ notification types across Email, SMS, and In-App channels.

---

## ✅ Pre-Testing Checklist

### 1. Database Setup
```sql
-- Run the notification templates migration
-- File: Database/Notification_Templates_Migration.sql
USE CateringEcommerceDB;
GO

-- Execute the migration script
-- Verify templates created
SELECT COUNT(*) FROM t_sys_notification_templates;
-- Expected: 60+ templates
```

### 2. Configuration Verification

**Check `appsettings.json`**:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Enyvora Catering",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromPhoneNumber": "+1234567890"
  },
  "RabbitMQ": {
    "Enabled": false  // Set to true if using RabbitMQ
  }
}
```

### 3. Test Accounts

Create test accounts for each user type:

| Type | Email | Phone | Purpose |
|------|-------|-------|---------|
| User | test-user@example.com | +919876543210 | Test user notifications |
| Partner | test-partner@example.com | +919876543211 | Test partner notifications |
| Admin | test-admin@example.com | +919876543212 | Test admin notifications |

---

## 🧪 Test Scenarios

### Test Group 1: User Registration & Authentication

#### Test 1.1: User Registration Welcome
**Endpoint**: `POST /api/User/Auth/verify-otp`

**Request**:
```json
{
  "currentAction": "signup",
  "phoneNumber": "+919876543210",
  "name": "Test User",
  "otp": "1234",
  "isPartnerLogin": false
}
```

**Expected Notifications**:
- ✉️ Email: `USER_REGISTRATION_WELCOME_EMAIL` (if email available)
- 📱 SMS: `USER_REGISTRATION_WELCOME_SMS`
- 🔔 In-App: `USER_REGISTRATION_WELCOME_INAPP`

**Verification**:
```sql
-- Check notification logs
SELECT TOP 5 * FROM t_sys_notification_logs
WHERE c_template_code LIKE 'USER_REGISTRATION_WELCOME%'
ORDER BY c_created_date DESC;
```

**Manual Checks**:
- [ ] Email received with welcome message
- [ ] SMS received with registration confirmation
- [ ] In-app notification visible in user dashboard
- [ ] All placeholders ({{user_name}}, {{registration_date}}) rendered correctly

---

#### Test 1.2: OTP Sending
**Endpoint**: `POST /api/User/Auth/send-otp`

**Request**:
```json
{
  "currentAction": "login",
  "phoneNumber": "+919876543210",
  "isPartnerLogin": false
}
```

**Expected Notifications**:
- 📱 SMS: `USER_OTP_SMS`

**Verification**:
- [ ] OTP SMS received within 30 seconds
- [ ] OTP code is 6 digits
- [ ] Validity message included (e.g., "Valid for 10 minutes")

---

### Test Group 2: Partner Registration & Approval

#### Test 2.1: Partner Registration Acknowledgement
**Endpoint**: `POST /api/Auth/Owner/Register`

**Request**: (Use full partner registration payload)

**Expected Notifications**:
- ✉️ Email: `PARTNER_REGISTRATION_ACK_EMAIL`
- 📱 SMS: `PARTNER_REGISTRATION_ACK_SMS`
- 🔔 Admin In-App: `NEW_PARTNER_REGISTRATION`

**Verification**:
- [ ] Partner receives acknowledgement email
- [ ] Partner receives acknowledgement SMS
- [ ] Admin panel shows new partner notification
- [ ] Notification includes business name, owner name, contact details

---

#### Test 2.2: Partner Approval
**Endpoint**: `PUT /api/admin/partner-requests/{ownerId}/approve`

**Request**:
```json
{
  "remarks": "All documents verified. Approved."
}
```

**Expected Notifications**:
- ✉️ Email: `PARTNER_APPROVAL_EMAIL`
- 📱 SMS: `PARTNER_APPROVAL_SMS`
- 🔔 Admin In-App: `PARTNER_REQUEST_APPROVED`

**Verification**:
- [ ] Partner receives approval email with login credentials
- [ ] Partner receives approval SMS
- [ ] Email includes partner guide URL and support contact
- [ ] Admin notification created

---

#### Test 2.3: Partner Rejection
**Endpoint**: `PUT /api/admin/partner-requests/{ownerId}/reject`

**Request**:
```json
{
  "remarks": "Incomplete FSSAI documentation"
}
```

**Expected Notifications**:
- ✉️ Email: `PARTNER_REJECTION_EMAIL`
- 📱 SMS: Not sent (rejection is email-only for detailed explanation)

**Verification**:
- [ ] Partner receives rejection email
- [ ] Email includes rejection reason
- [ ] Email mentions reapplication timeline

---

#### Test 2.4: Partner Info Request
**Endpoint**: `PUT /api/admin/partner-requests/{ownerId}/request-info`

**Request**:
```json
{
  "remarks": "Please upload updated GST certificate"
}
```

**Expected Notifications**:
- ✉️ Email: `PARTNER_INFO_REQUEST_EMAIL`
- 📱 SMS: `PARTNER_INFO_REQUEST_SMS`

**Verification**:
- [ ] Partner receives info request email
- [ ] Email specifies what information is needed
- [ ] Email includes deadline (typically 7 days)
- [ ] Upload URL provided

---

### Test Group 3: Order Notifications

#### Test 3.1: Order Confirmation
**Endpoint**: `POST /api/User/Orders/Create`

**Request**: (Use full order creation payload)

**Expected Notifications**:
- ✉️ Email to User: `ORDER_CONFIRMATION_EMAIL`
- 📱 SMS to User: `ORDER_CONFIRMATION_SMS`
- 🔔 In-App to User: `ORDER_CONFIRMATION_INAPP`
- 🔔 Admin In-App: `ADMIN_NEW_ORDER`

**Verification**:
- [ ] User receives order confirmation email
- [ ] User receives order confirmation SMS
- [ ] User sees in-app notification
- [ ] Admin panel shows new order alert
- [ ] All order details correct (order number, event date, amount, etc.)

---

#### Test 3.2: Order Cancellation
**Endpoint**: `POST /api/User/Orders/{orderId}/Cancel`

**Request**:
```json
{
  "reason": "Event postponed"
}
```

**Expected Notifications**:
- ✉️ Email to User: `ORDER_CANCELLATION_EMAIL`
- 📱 SMS to User: `ORDER_CANCELLATION_SMS`
- ✉️ Email to Partner: `ORDER_CANCELLATION_PARTNER_EMAIL`
- 📱 SMS to Partner: `ORDER_CANCELLATION_PARTNER_SMS`
- 🔔 Admin In-App: Admin alert

**Verification**:
- [ ] User receives cancellation confirmation
- [ ] User informed about refund timeline (5-7 days)
- [ ] Partner notified about cancellation
- [ ] Admin alert created

---

### Test Group 4: Payment Notifications

#### Test 4.1: Payment Success
**Endpoint**: `POST /api/User/PaymentGateway/VerifyPayment`

**Request**:
```json
{
  "orderId": "12345",
  "razorpayOrderId": "order_xyz",
  "razorpayPaymentId": "pay_abc",
  "razorpaySignature": "valid_signature"
}
```

**Expected Notifications**:
- ✉️ Email: `PAYMENT_SUCCESS_EMAIL`
- 📱 SMS: `PAYMENT_SUCCESS_SMS`
- 🔔 In-App: `PAYMENT_SUCCESS_INAPP`

**Verification**:
- [ ] User receives payment confirmation email
- [ ] Email includes transaction ID and amount
- [ ] SMS received within 1 minute
- [ ] In-app notification shows payment success

---

#### Test 4.2: Payment Failed
**Endpoint**: `POST /api/User/PaymentGateway/VerifyPayment` (with invalid signature)

**Expected Notifications**:
- ✉️ Email: `PAYMENT_FAILED_EMAIL`
- 📱 SMS: `PAYMENT_FAILED_SMS`
- 🔔 Admin In-App: `ADMIN_PAYMENT_FAILED`

**Verification**:
- [ ] User receives payment failure email
- [ ] Email includes retry link
- [ ] SMS includes support contact
- [ ] Admin notified of payment failure

---

#### Test 4.3: Refund Initiated
**Endpoint**: `POST /api/User/PaymentGateway/ProcessRefund`

**Request**:
```json
{
  "paymentId": "pay_abc",
  "amount": 5000,
  "reason": "Order cancelled"
}
```

**Expected Notifications**:
- ✉️ Email: `REFUND_INITIATED_EMAIL`
- 📱 SMS: Not sent (refund is email-only for detailed info)
- 🔔 In-App: `REFUND_INITIATED_INAPP`

**Verification**:
- [ ] User receives refund initiation email
- [ ] Email includes refund amount and timeline
- [ ] Email mentions it will be credited to original payment method

---

### Test Group 5: Delivery Notifications

#### Test 5.1: Delivery Scheduled
**Endpoint**: `PUT /api/Owner/EventDelivery/update-status`

**Request**:
```json
{
  "eventDeliveryId": 123,
  "newStatus": "Scheduled",
  "remarks": "Delivery scheduled for tomorrow at 2 PM"
}
```

**Expected Notifications**:
- ✉️ Email: `EVENT_DELIVERY_SCHEDULED_EMAIL`
- 📱 SMS: `EVENT_DELIVERY_SCHEDULED_SMS`
- 🔔 In-App: `EVENT_DELIVERY_SCHEDULED_INAPP`

**Verification**:
- [ ] User receives delivery scheduled email
- [ ] Email includes delivery date, time, and address
- [ ] SMS received with key delivery details

---

#### Test 5.2: Out for Delivery
**Endpoint**: `PUT /api/Owner/EventDelivery/update-status`

**Request**:
```json
{
  "eventDeliveryId": 123,
  "newStatus": "OutForDelivery",
  "remarks": "Delivery vehicle departed"
}
```

**Expected Notifications**:
- 📱 SMS: `DELIVERY_OUT_FOR_DELIVERY_SMS`
- 🔔 In-App: `DELIVERY_OUT_FOR_DELIVERY_INAPP`

**Verification**:
- [ ] User receives "out for delivery" SMS
- [ ] In-app notification shows live delivery status

---

#### Test 5.3: Delivery Completed
**Endpoint**: `PUT /api/Owner/EventDelivery/update-status`

**Request**:
```json
{
  "eventDeliveryId": 123,
  "newStatus": "Delivered",
  "remarks": "Delivered successfully"
}
```

**Expected Notifications**:
- ✉️ Email: `DELIVERY_COMPLETED_EMAIL`
- 🔔 In-App: `DELIVERY_COMPLETED_INAPP`

**Verification**:
- [ ] User receives delivery completion email
- [ ] Email asks for feedback/review
- [ ] In-app notification marks delivery as complete

---

#### Test 5.4: Delivery Delayed
**Endpoint**: `PUT /api/Owner/EventDelivery/update-status`

**Request**:
```json
{
  "eventDeliveryId": 123,
  "newStatus": "Delayed",
  "remarks": "Traffic delay, expected by 3 PM"
}
```

**Expected Notifications**:
- 📱 SMS: `DELIVERY_DELAYED_SMS`
- 🔔 In-App: `DELIVERY_DELAYED_INAPP`

**Verification**:
- [ ] User receives delay notification SMS
- [ ] SMS includes new estimated time
- [ ] Partner support contact provided

---

### Test Group 6: Admin Notifications

#### Test 6.1: Admin Panel Notifications
**Access**: Admin dashboard at `/admin/notifications`

**Test Scenarios**:
1. New partner registration → Admin notification appears
2. New order placed → Admin notification appears
3. Payment failed → Admin notification appears
4. Partner approved → Admin notification appears

**Verification**:
- [ ] All admin notifications appear in real-time
- [ ] Notifications have proper icons and colors
- [ ] Click notification navigates to relevant page
- [ ] Unread count updates correctly
- [ ] Mark as read functionality works

---

## 🔧 Testing Tools

### 1. Postman Collection

Create a Postman collection with all endpoints:

**Collection Structure**:
```
📁 Notification System Tests
  📁 1. User Registration
    - Send OTP
    - Verify OTP (Signup)
  📁 2. Partner Management
    - Partner Registration
    - Approve Partner
    - Reject Partner
    - Request Info
  📁 3. Orders
    - Create Order
    - Cancel Order
  📁 4. Payments
    - Payment Success
    - Payment Failed
    - Process Refund
  📁 5. Delivery
    - Schedule Delivery
    - Out for Delivery
    - Delivery Completed
    - Delivery Delayed
```

### 2. SQL Queries for Verification

```sql
-- Check all notification logs for today
SELECT
    c_template_code,
    c_channel,
    c_recipient_email,
    c_recipient_phone,
    c_status,
    c_created_date
FROM t_sys_notification_logs
WHERE CAST(c_created_date AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY c_created_date DESC;

-- Check notification success rate by channel
SELECT
    c_channel,
    COUNT(*) as Total,
    SUM(CASE WHEN c_status = 'Sent' THEN 1 ELSE 0 END) as Sent,
    SUM(CASE WHEN c_status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    CAST(SUM(CASE WHEN c_status = 'Sent' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as SuccessRate
FROM t_sys_notification_logs
GROUP BY c_channel;

-- Check template usage
SELECT
    c_template_code,
    COUNT(*) as UsageCount
FROM t_sys_notification_logs
GROUP BY c_template_code
ORDER BY UsageCount DESC;
```

### 3. Email Testing Tools

**For Development**:
- **MailHog**: Local email testing (http://localhost:8025)
- **Mailtrap**: Free email testing service (https://mailtrap.io)

**Setup MailHog**:
```bash
docker run -d -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

Update `appsettings.Development.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "localhost",
    "SmtpPort": 1025,
    "SenderEmail": "test@enyvora.com",
    "SenderName": "Enyvora Catering"
  }
}
```

### 4. SMS Testing

**Twilio Test Credentials**:
```json
{
  "Twilio": {
    "AccountSid": "test",
    "AuthToken": "test",
    "FromPhoneNumber": "+15005550006"  // Twilio test number
  }
}
```

**Note**: With test credentials, SMS won't be sent but will be logged.

---

## 📊 Test Results Template

### Test Execution Log

| Test ID | Scenario | Channel | Status | Notes |
|---------|----------|---------|--------|-------|
| 1.1 | User Registration | Email | ✅ Pass | - |
| 1.1 | User Registration | SMS | ✅ Pass | - |
| 1.1 | User Registration | In-App | ✅ Pass | - |
| 2.1 | Partner Registration | Email | ✅ Pass | - |
| 2.2 | Partner Approval | Email | ✅ Pass | - |
| 3.1 | Order Confirmation | Email | ✅ Pass | - |
| 4.1 | Payment Success | Email | ✅ Pass | - |
| 5.1 | Delivery Scheduled | SMS | ✅ Pass | - |

### Performance Metrics

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| Email Delivery Time | < 30s | 15s | ✅ |
| SMS Delivery Time | < 10s | 5s | ✅ |
| In-App Notification Latency | < 2s | 1s | ✅ |
| Notification Success Rate | > 95% | 98% | ✅ |
| Template Rendering Errors | 0 | 0 | ✅ |

---

## 🐛 Common Issues & Fixes

### Issue 1: Email Not Received

**Possible Causes**:
- SMTP credentials incorrect
- Email in spam folder
- Gmail "Less secure apps" blocked

**Fix**:
1. Check `appsettings.json` SMTP settings
2. For Gmail, use App Password instead of regular password
3. Check spam folder
4. Verify email logs in database

### Issue 2: SMS Not Sent

**Possible Causes**:
- Twilio credentials incorrect
- Phone number format wrong
- Insufficient Twilio credits

**Fix**:
1. Verify Twilio Account SID and Auth Token
2. Use E.164 format for phone numbers (+91XXXXXXXXXX)
3. Check Twilio console for errors

### Issue 3: Template Placeholders Not Rendering

**Possible Causes**:
- Missing data in notification payload
- Template syntax error

**Fix**:
1. Check notification data dictionary has all required keys
2. Verify template in database matches expected format
3. Check logs for template rendering errors

### Issue 4: Admin Notifications Not Appearing

**Possible Causes**:
- SignalR not configured
- Admin user not connected to notification hub

**Fix**:
1. Verify SignalR is registered in Program.cs
2. Check browser console for SignalR connection errors
3. Ensure admin is logged in

---

## ✅ Final Checklist

### Pre-Production
- [ ] All 20+ notification types tested
- [ ] Email delivery working (Gmail/SendGrid)
- [ ] SMS delivery working (Twilio)
- [ ] In-app notifications appearing in real-time
- [ ] All templates render correctly without {{placeholders}}
- [ ] Error handling works (graceful degradation)
- [ ] Notification logs being created
- [ ] Admin panel showing all notifications
- [ ] RabbitMQ integration tested (if enabled)
- [ ] Performance metrics meet targets
- [ ] No spam complaints
- [ ] Unsubscribe links work (for promotional emails)

### Production Readiness
- [ ] Production SMTP credentials configured
- [ ] Production Twilio credentials configured
- [ ] RabbitMQ production instance configured (if used)
- [ ] Monitoring set up (notification success rate)
- [ ] Alerts configured for critical failures
- [ ] Rate limiting configured
- [ ] User notification preferences implemented
- [ ] Multi-language support tested (if applicable)

---

## 📈 Continuous Monitoring

### Key Metrics to Track

1. **Delivery Success Rate**: Target > 95%
2. **Average Delivery Time**: Target < 30s for email, < 10s for SMS
3. **Template Error Rate**: Target = 0%
4. **User Engagement**: Email open rate, click-through rate
5. **Opt-out Rate**: Target < 5%

### Monitoring Queries

```sql
-- Daily notification metrics
SELECT
    CAST(c_created_date AS DATE) as Date,
    c_channel as Channel,
    COUNT(*) as Total,
    SUM(CASE WHEN c_status = 'Sent' THEN 1 ELSE 0 END) as Sent,
    SUM(CASE WHEN c_status = 'Failed' THEN 1 ELSE 0 END) as Failed
FROM t_sys_notification_logs
WHERE c_created_date >= DATEADD(day, -7, GETDATE())
GROUP BY CAST(c_created_date AS DATE), c_channel
ORDER BY Date DESC, Channel;
```

---

**Testing Status**: Ready for execution
**Last Updated**: January 2026
**Version**: 1.0
