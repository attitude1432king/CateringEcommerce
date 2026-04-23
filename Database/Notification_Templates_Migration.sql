-- ============================================
-- Notification Templates Migration
-- Creates notification templates table and inserts all templates
-- for Email, SMS, and In-App notifications
-- ============================================

-- Create notification templates table if it does not already exist
CREATE TABLE IF NOT EXISTS t_sys_notification_templates (
    c_template_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_template_code VARCHAR(100) NOT NULL UNIQUE,
    c_template_name VARCHAR(200) NOT NULL,
    c_description VARCHAR(500),
    c_language VARCHAR(10) NOT NULL DEFAULT 'en',
    c_channel VARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP
    c_category VARCHAR(50) NOT NULL,
    c_subject VARCHAR(500),
    c_body TEXT NOT NULL,
    c_version INT NOT NULL DEFAULT 1,
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_usage_count INT NOT NULL DEFAULT 0,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_createdby BIGINT,
    c_modifieddate TIMESTAMP,
    c_modifiedby BIGINT
);

CREATE INDEX IF NOT EXISTS "IX_NotificationTemplates_Code"
    ON t_sys_notification_templates(c_template_code, c_is_active);

CREATE INDEX IF NOT EXISTS "IX_NotificationTemplates_Channel"
    ON t_sys_notification_templates(c_channel, c_language, c_is_active);
    
-- ============================================
-- USER REGISTRATION & AUTHENTICATION TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('USER_REGISTRATION_WELCOME_EMAIL', 'User Registration Welcome Email', 'en', 'EMAIL', 'USER_REGISTRATION',
'Welcome to Enyvora Catering, {{ user_name }}!',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h1{color:#e74c3c}</style></head>
<body>
<h1>Welcome {{ user_name }}!</h1>
<p>Thank you for registering with <strong>Enyvora Catering</strong>, your trusted partner for all catering needs.</p>
<p>You registered on <strong>{{ registration_date }}</strong>. Start exploring our wide range of catering services for weddings, corporate events, parties, and more!</p>
<p><a href="{{ app_url }}" style="background:#e74c3c;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Browse Caterings</a></p>
<p>Need help? Contact us at <strong>{{ support_email }}</strong> or call <strong>{{ support_phone }}</strong>.</p>
<p>Best regards,<br>Enyvora Catering Team</p>
</body>
</html>'),

('USER_REGISTRATION_WELCOME_SMS', 'User Registration Welcome SMS', 'en', 'SMS', 'USER_REGISTRATION',
NULL,
'Welcome {{ user_name }}! Your account with Enyvora Catering is now active. Explore delicious catering options and book your perfect event. Download our app today!'),

('USER_OTP_SMS', 'User OTP Verification', 'en', 'SMS', 'OTP',
NULL,
'Your OTP for Enyvora Catering is {{ otp }}. Valid for {{ validity_minutes }} minutes. Do not share this code with anyone. -Enyvora');

-- ============================================
-- ORDER LIFECYCLE TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('ORDER_CONFIRMATION_EMAIL', 'Order Confirmation Email', 'en', 'EMAIL', 'ORDER',
'Order Confirmed #{{ order_number }} - Enyvora Catering',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}table{width:100%;border-collapse:collapse}th,td{padding:10px;text-align:left;border-bottom:1px solid #ddd}th{background:#e74c3c;color:#fff}</style></head>
<body>
<h2 style="color:#e74c3c">Order Confirmed!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Thank you for placing your order with <strong>{{ catering_name }}</strong>. Your order has been confirmed!</p>
<table>
<tr><th>Order Details</th><th></th></tr>
<tr><td>Order Number</td><td><strong>{{ order_number }}</strong></td></tr>
<tr><td>Event Date</td><td>{{ event_date }} at {{ event_time }}</td></tr>
<tr><td>Event Location</td><td>{{ event_location }}</td></tr>
<tr><td>Guest Count</td><td>{{ guest_count }} guests</td></tr>
<tr><td>Total Amount</td><td><strong>Rs. {{ total_amount }}</strong></td></tr>
<tr><td>Payment Status</td><td>{{ payment_status }}</td></tr>
</table>
<p>The caterer will contact you shortly to confirm event details.</p>
<p><a href="{{ order_url }}" style="background:#e74c3c;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">View Order Details</a></p>
<p>For support, contact us at {{ support_email }}</p>
<p>Best regards,<br>Enyvora Catering Team</p>
</body>
</html>'),

('ORDER_CONFIRMATION_SMS', 'Order Confirmation SMS', 'en', 'SMS', 'ORDER',
NULL,
'Order #{{ order_number }} confirmed! Event: {{ event_date }} | Guests: {{ guest_count }} | Total: Rs.{{ total_amount }}. {{ catering_name }} will contact you soon. -Enyvora'),

('ORDER_CONFIRMATION_INAPP', 'Order Confirmation In-App', 'en', 'INAPP', 'ORDER',
'Order Confirmed #{{ order_number }}',
'Your order for {{ guest_count }} guests on {{ event_date }} has been confirmed. Total: Rs.{{ total_amount }}'),

('ORDER_ASSIGNED_PARTNER_EMAIL', 'Order Assigned to Partner Email', 'en', 'EMAIL', 'ORDER',
'New Order Assigned #{{ order_number }} - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}table{width:100%;border-collapse:collapse}th,td{padding:10px;text-align:left;border-bottom:1px solid #ddd}th{background:#27ae60;color:#fff}</style></head>
<body>
<h2 style="color:#27ae60">New Order Assigned!</h2>
<p>Dear <strong>{{ partner_name }}</strong>,</p>
<p>A new order has been assigned to your catering business!</p>
<table>
<tr><th>Order Details</th><th></th></tr>
<tr><td>Order Number</td><td><strong>{{ order_number }}</strong></td></tr>
<tr><td>Customer Name</td><td>{{ customer_name }}</td></tr>
<tr><td>Event Date</td><td>{{ event_date }} at {{ event_time }}</td></tr>
<tr><td>Event Location</td><td>{{ event_location }}</td></tr>
<tr><td>Guest Count</td><td>{{ guest_count }} guests</td></tr>
<tr><td>Event Type</td><td>{{ event_type }}</td></tr>
<tr><td>Total Amount</td><td><strong>Rs. {{ total_amount }}</strong></td></tr>
</table>
<p><strong>Action Required:</strong> Please contact the customer at <strong>{{ customer_phone }}</strong> to confirm event details.</p>
<p><a href="{{ partner_portal_url }}" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">View Order in Partner Portal</a></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('ORDER_ASSIGNED_PARTNER_SMS', 'Order Assigned to Partner SMS', 'en', 'SMS', 'ORDER',
NULL,
'New order #{{ order_number }}! Event: {{ event_date }} | {{ guest_count }} guests | {{ event_location }}. Contact {{ customer_name }} at {{ customer_phone }}. Login to view details.'),

('ORDER_ASSIGNED_PARTNER_INAPP', 'Order Assigned to Partner In-App', 'en', 'INAPP', 'ORDER',
'New Order #{{ order_number }}',
'New order assigned for {{ event_date }}. {{ guest_count }} guests at {{ event_location }}. Contact customer immediately.'),

('ORDER_STATUS_UPDATE_EMAIL', 'Order Status Update Email', 'en', 'EMAIL', 'ORDER',
'Order #{{ order_number }} - Status: {{ status }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#3498db">Order Status Updated</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your order <strong>#{{ order_number }}</strong> status has been updated:</p>
<p style="font-size:18px;color:#27ae60"><strong>{{ status }}</strong></p>
<p>{{ status_message }}</p>
<p><a href="{{ order_url }}" style="background:#3498db;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Track Order</a></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('ORDER_CANCELLATION_EMAIL', 'Order Cancellation Email', 'en', 'EMAIL', 'ORDER',
'Order Cancelled #{{ order_number }} - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#e74c3c">Order Cancelled</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your order <strong>#{{ order_number }}</strong> has been cancelled.</p>
<p><strong>Cancellation Reason:</strong> {{ cancellation_reason }}</p>
<p><strong>Refund Information:</strong><br>
Amount: Rs. {{ refund_amount }}<br>
Refund will be processed within <strong>5-7 business days</strong> to your original payment method.</p>
<p>If you have any questions, contact us at {{ support_email }}</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('ORDER_CANCELLATION_SMS', 'Order Cancellation SMS', 'en', 'SMS', 'ORDER',
NULL,
'Order #{{ order_number }} cancelled. Refund of Rs.{{ refund_amount }} will be processed in 5-7 business days. Contact support for queries. -Enyvora');

-- ============================================
-- PAYMENT TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('PAYMENT_SUCCESS_EMAIL', 'Payment Success Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Successful for Order #{{ order_number }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}table{width:100%;border-collapse:collapse}th,td{padding:10px;text-align:left;border-bottom:1px solid #ddd}</style></head>
<body>
<h2 style="color:#27ae60">Payment Received Successfully!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>We have received your payment for order <strong>#{{ order_number }}</strong>.</p>
<table>
<tr><td>Transaction ID</td><td><strong>{{ transaction_id }}</strong></td></tr>
<tr><td>Amount Paid</td><td><strong>Rs. {{ amount }}</strong></td></tr>
<tr><td>Payment Method</td><td>{{ payment_method }}</td></tr>
<tr><td>Payment Date</td><td>{{ payment_date }}</td></tr>
</table>
<p>Your order is now confirmed and will be delivered on <strong>{{ event_date }}</strong>.</p>
<p><a href="{{ receipt_url }}" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Download Receipt</a></p>
<p>Thank you for choosing Enyvora!</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('PAYMENT_SUCCESS_SMS', 'Payment Success SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Payment of Rs.{{ amount }} successful for order #{{ order_number }}. Transaction ID: {{ transaction_id }}. Event confirmed for {{ event_date }}. -Enyvora'),

('PAYMENT_SUCCESS_INAPP', 'Payment Success In-App', 'en', 'INAPP', 'PAYMENT',
'Payment Successful',
'Payment of Rs.{{ amount }} received successfully for order #{{ order_number }}. Transaction ID: {{ transaction_id }}'),

('PAYMENT_FAILED_EMAIL', 'Payment Failed Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Failed for Order #{{ order_number }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#e74c3c">Payment Failed</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Unfortunately, your payment for order <strong>#{{ order_number }}</strong> could not be processed.</p>
<p><strong>Failure Reason:</strong> {{ failure_reason }}</p>
<p><strong>Amount:</strong> Rs. {{ amount }}</p>
<p>Please try again or use a different payment method.</p>
<p><a href="{{ retry_payment_url }}" style="background:#e74c3c;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Retry Payment</a></p>
<p>If the issue persists, contact us at {{ support_email }}</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('PAYMENT_FAILED_SMS', 'Payment Failed SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Payment failed for order #{{ order_number }}. Amount: Rs.{{ amount }}. Please retry or contact support. -Enyvora'),

('PAYMENT_REMINDER_EMAIL', 'Payment Reminder Email', 'en', 'EMAIL', 'PAYMENT',
'Payment Due Reminder - Order #{{ order_number }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#f39c12">Payment Reminder</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>This is a friendly reminder that you have a pending payment for order <strong>#{{ order_number }}</strong>.</p>
<p><strong>Amount Due:</strong> Rs. {{ amount_due }}<br>
<strong>Due Date:</strong> {{ due_date }}<br>
<strong>Days Overdue:</strong> {{ days_overdue }}</p>
<p>Please make the payment at your earliest convenience to avoid service interruption.</p>
<p><a href="{{ payment_url }}" style="background:#f39c12;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Pay Now</a></p>
<p>If already paid, please ignore this reminder.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('PAYMENT_REMINDER_SMS', 'Payment Reminder SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Reminder: Rs.{{ amount_due }} pending for order #{{ order_number }}. Due: {{ due_date }}. Pay now to avoid delays. -Enyvora'),

('REFUND_INITIATED_EMAIL', 'Refund Initiated Email', 'en', 'EMAIL', 'PAYMENT',
'Refund Initiated for Order #{{ order_number }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#3498db">Refund Initiated</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your refund for order <strong>#{{ order_number }}</strong> has been initiated.</p>
<p><strong>Refund Amount:</strong> Rs. {{ refund_amount }}<br>
<strong>Original Transaction ID:</strong> {{ original_transaction_id }}<br>
<strong>Refund Reference:</strong> {{ refund_reference }}</p>
<p>The amount will be credited to your original payment method within <strong>5-7 business days</strong>.</p>
<p>If you don''t receive the refund within this timeframe, please contact your bank or us at {{ support_email }}.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('REFUND_INITIATED_SMS', 'Refund Initiated SMS', 'en', 'SMS', 'PAYMENT',
NULL,
'Refund of Rs.{{ refund_amount }} initiated for order #{{ order_number }}. Amount will be credited in 5-7 business days. -Enyvora');

-- ============================================
-- PARTNER MANAGEMENT TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('PARTNER_REGISTRATION_ACK_EMAIL', 'Partner Registration Acknowledgement Email', 'en', 'EMAIL', 'PARTNER',
'Registration Received - Enyvora Catering Partner Program',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#27ae60">Thank You for Registering!</h2>
<p>Dear <strong>{{ owner_name }}</strong>,</p>
<p>Thank you for registering your catering business <strong>"{{ catering_name }}"</strong> with Enyvora!</p>
<p><strong>Registration Date:</strong> {{ registration_date }}</p>
<p><strong>Next Steps:</strong></p>
<ul>
<li>Our team will review your application within <strong>24-48 hours</strong></li>
<li>We may contact you for additional information if needed</li>
<li>Once approved, you will receive login credentials to access the Partner Portal</li>
</ul>
<p>In the meantime, you can:</p>
<ul>
<li>Prepare high-quality photos of your dishes and services</li>
<li>Review our <a href="{{ terms_url }}">Partner Terms & Conditions</a></li>
<li>Plan your initial menu offerings</li>
</ul>
<p>For any questions, contact our Partner Support Team at <strong>{{ partner_support_email }}</strong> or call <strong>{{ partner_support_phone }}</strong>.</p>
<p>We look forward to partnering with you!</p>
<p>Best regards,<br>Enyvora Partner Onboarding Team</p>
</body>
</html>'),

('PARTNER_REGISTRATION_ACK_SMS', 'Partner Registration Acknowledgement SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Thank you {{ owner_name }}! Registration for "{{ catering_name }}" received. Our team will review and contact you within 24-48 hours. -Enyvora'),

('PARTNER_APPROVAL_EMAIL', 'Partner Approval Email', 'en', 'EMAIL', 'PARTNER',
'Congratulations! Your Catering Business Approved - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#27ae60">Congratulations! Welcome to Enyvora!</h2>
<p>Dear <strong>{{ owner_name }}</strong>,</p>
<p>Great news! Your catering business <strong>"{{ catering_name }}"</strong> has been approved and is now live on the Enyvora platform!</p>
<p><strong>Approval Date:</strong> {{ approval_date }}</p>
<p><strong>Your Partner Portal Access:</strong></p>
<ul>
<li>Login URL: <a href="{{ login_url }}">{{ login_url }}</a></li>
<li>Username: {{ username }}</li>
<li>Temporary Password: {{ temp_password }} (Change after first login)</li>
</ul>
<p><strong>Next Steps to Start Receiving Orders:</strong></p>
<ol>
<li>Login to Partner Portal and change your password</li>
<li>Upload menu items with attractive photos and pricing</li>
<li>Set your availability calendar</li>
<li>Configure service areas and delivery radius</li>
<li>Review and accept your first orders!</li>
</ol>
<p><strong>Resources:</strong></p>
<ul>
<li><a href="{{ partner_guide_url }}">Partner User Guide</a></li>
<li><a href="{{ best_practices_url }}">Best Practices for Success</a></li>
<li><a href="{{ support_url }}">Partner Support Center</a></li>
</ul>
<p><a href="{{ login_url }}" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Login to Partner Portal</a></p>
<p>For support, contact us at {{ partner_support_email }} or call {{ partner_support_phone }}.</p>
<p>Welcome aboard!<br>Enyvora Partner Success Team</p>
</body>
</html>'),

('PARTNER_APPROVAL_SMS', 'Partner Approval SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Congratulations {{ owner_name }}! "{{ catering_name }}" approved on Enyvora. Login at {{ login_url }} to start receiving orders. -Enyvora'),

('PARTNER_APPROVAL_INAPP', 'Partner Approval In-App', 'en', 'INAPP', 'PARTNER',
'Business Approved!',
'Congratulations! Your catering business has been approved. Start adding menu items to receive orders.'),

('PARTNER_REJECTION_EMAIL', 'Partner Rejection Email', 'en', 'EMAIL', 'PARTNER',
'Application Status Update - Enyvora Partner Program',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#e74c3c">Application Status Update</h2>
<p>Dear <strong>{{ owner_name }}</strong>,</p>
<p>Thank you for your interest in joining Enyvora as a catering partner with <strong>"{{ catering_name }}"</strong>.</p>
<p>After careful review, we regret to inform you that we are unable to approve your application at this time.</p>
<p><strong>Reason:</strong> {{ rejection_reason }}</p>
<p><strong>What you can do:</strong></p>
<ul>
<li>Address the issues mentioned above</li>
<li>Re-apply after {{ reapply_duration }}</li>
<li>Contact our team at {{ partner_support_email }} for clarification</li>
</ul>
<p>We appreciate your interest and hope to work with you in the future.</p>
<p>Best regards,<br>Enyvora Partner Team</p>
</body>
</html>'),

('PARTNER_REJECTION_SMS', 'Partner Rejection SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Dear {{ owner_name }}, your application for "{{ catering_name }}" could not be approved at this time. Check your email for details. -Enyvora'),

('PARTNER_INFO_REQUEST_EMAIL', 'Partner Information Request Email', 'en', 'EMAIL', 'PARTNER',
'Additional Information Required - Enyvora Partner Application',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#f39c12">Additional Information Required</h2>
<p>Dear <strong>{{ owner_name }}</strong>,</p>
<p>We are reviewing your application for <strong>"{{ catering_name }}"</strong> and need some additional information to proceed.</p>
<p><strong>Information Requested:</strong></p>
<p>{{ info_requested }}</p>
<p><strong>Please provide the requested information by:</strong> {{ deadline_date }}</p>
<p><a href="{{ upload_url }}" style="background:#f39c12;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Submit Information</a></p>
<p>If you have any questions, contact us at {{ partner_support_email }}</p>
<p>Best regards,<br>Enyvora Verification Team</p>
</body>
</html>'),

('PARTNER_INFO_REQUEST_SMS', 'Partner Information Request SMS', 'en', 'SMS', 'PARTNER',
NULL,
'Additional info needed for "{{ catering_name }}" application. Check email for details. Deadline: {{ deadline_date }}. -Enyvora'),

('PARTNER_DEACTIVATION_EMAIL', 'Partner Deactivation Email', 'en', 'EMAIL', 'PARTNER',
'Account Deactivated - Enyvora Partner',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#e74c3c">Account Deactivated</h2>
<p>Dear <strong>{{ owner_name }}</strong>,</p>
<p>Your partner account for <strong>"{{ catering_name }}"</strong> has been deactivated.</p>
<p><strong>Deactivation Reason:</strong> {{ deactivation_reason }}</p>
<p><strong>Deactivation Date:</strong> {{ deactivation_date }}</p>
<p>You will no longer receive new orders. Pending orders must be fulfilled as committed.</p>
<p>To appeal this decision or discuss reactivation, contact {{ partner_support_email }}</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>');

-- ============================================
-- DELIVERY TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('SAMPLE_DELIVERY_SCHEDULED_EMAIL', 'Sample Delivery Scheduled Email', 'en', 'EMAIL', 'DELIVERY',
'Sample Delivery Scheduled for {{ delivery_date }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#3498db">Sample Delivery Confirmed!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your sample delivery for order <strong>#{{ order_number }}</strong> has been scheduled.</p>
<p><strong>Delivery Details:</strong><br>
Date: {{ delivery_date }}<br>
Time: {{ delivery_time }}<br>
Address: {{ delivery_address }}</p>
<p><strong>Sample Items:</strong><br>{{ sample_items }}</p>
<p>Please ensure someone is available to receive the sample at the scheduled time.</p>
<p>After tasting, you can confirm or modify your final order.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('SAMPLE_DELIVERY_SCHEDULED_SMS', 'Sample Delivery Scheduled SMS', 'en', 'SMS', 'DELIVERY',
NULL,
'Sample delivery scheduled for {{ delivery_date }} at {{ delivery_time }}. Address: {{ delivery_address }}. -Enyvora'),

('EVENT_DELIVERY_SCHEDULED_EMAIL', 'Event Delivery Scheduled Email', 'en', 'EMAIL', 'DELIVERY',
'Event Delivery Confirmed for {{ delivery_date }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#27ae60">Event Delivery Scheduled!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your order <strong>#{{ order_number }}</strong> delivery has been confirmed.</p>
<p><strong>Delivery Details:</strong><br>
Date: {{ delivery_date }}<br>
Time: {{ delivery_time }}<br>
Location: {{ event_location }}<br>
Contact Person: {{ contact_person }}</p>
<p><strong>Order Summary:</strong><br>
Guest Count: {{ guest_count }}<br>
Menu: {{ menu_type }}<br>
Special Instructions: {{ special_instructions }}</p>
<p>Our team will arrive 30 minutes before the scheduled time for setup.</p>
<p>For any last-minute changes, contact {{ caterer_phone }}</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('EVENT_DELIVERY_SCHEDULED_SMS', 'Event Delivery Scheduled SMS', 'en', 'SMS', 'DELIVERY',
NULL,
'Event delivery confirmed for {{ delivery_date }} at {{ delivery_time }}. Location: {{ event_location }}. Contact: {{ caterer_phone }} -Enyvora'),

('DELIVERY_REMINDER_SMS', 'Delivery Reminder SMS', 'en', 'SMS', 'DELIVERY',
NULL,
'Reminder: Your order #{{ order_number }} will be delivered tomorrow {{ delivery_date }} at {{ delivery_time }}. Address: {{ delivery_address }}. -Enyvora'),

('DELIVERY_REMINDER_INAPP', 'Delivery Reminder In-App', 'en', 'INAPP', 'DELIVERY',
'Delivery Tomorrow',
'Your order #{{ order_number }} will be delivered tomorrow at {{ delivery_time }}. Please be available.'),

('DELIVERY_COMPLETED_EMAIL', 'Delivery Completed Email', 'en', 'EMAIL', 'DELIVERY',
'Order Delivered Successfully #{{ order_number }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#27ae60">Order Delivered Successfully!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>Your order <strong>#{{ order_number }}</strong> has been delivered successfully.</p>
<p>We hope you enjoyed our service!</p>
<p><strong>Please take a moment to rate your experience:</strong></p>
<p><a href="{{ review_url }}" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Leave a Review</a></p>
<p>Your feedback helps us improve and helps other customers make informed decisions.</p>
<p>Thank you for choosing Enyvora!</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('DELIVERY_COMPLETED_SMS', 'Delivery Completed SMS', 'en', 'SMS', 'DELIVERY',
NULL,
'Order #{{ order_number }} delivered! Rate your experience: {{ review_url }} Thank you for choosing Enyvora!');

-- ============================================
-- ADMIN NOTIFICATION TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('ADMIN_NEW_PARTNER_REGISTRATION_INAPP', 'Admin - New Partner Registration In-App', 'en', 'INAPP', 'ADMIN',
'New Partner Registration',
'New catering partner "{{ catering_name }}" registered by {{ owner_name }} ({{ mobile }}). Review application now.'),

('ADMIN_NEW_ORDER_INAPP', 'Admin - New Order In-App', 'en', 'INAPP', 'ADMIN',
'New Order Placed',
'Order #{{ order_number }} placed by {{ customer_name }} for Rs.{{ amount }}. Partner: {{ catering_name }}'),

('ADMIN_PAYMENT_FAILED_INAPP', 'Admin - Payment Failed In-App', 'en', 'INAPP', 'ADMIN',
'Payment Failed Alert',
'Payment of Rs.{{ amount }} failed for order #{{ order_number }}. Customer: {{ customer_name }}. Reason: {{ reason }}'),

('ADMIN_HIGH_VALUE_ORDER_INAPP', 'Admin - High Value Order In-App', 'en', 'INAPP', 'ADMIN',
'High Value Order Alert',
'High value order #{{ order_number }} - Rs.{{ amount }}. Customer: {{ customer_name }}, Partner: {{ catering_name }}'),

('ADMIN_REVIEW_MODERATION_INAPP', 'Admin - Review Moderation In-App', 'en', 'INAPP', 'ADMIN',
'Review Needs Moderation',
'{{ customer_name }} rated {{ partner_name }} {{ rating }} stars. Review content may need moderation.'),

('ADMIN_PARTNER_DEACTIVATION_REQUEST_INAPP', 'Admin - Partner Deactivation Request', 'en', 'INAPP', 'ADMIN',
'Partner Deactivation Request',
'{{ partner_name }} requested account deactivation. Reason: {{ reason }}');

-- ============================================
-- REVIEW & RATING TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('REVIEW_REQUEST_EMAIL', 'Review Request Email', 'en', 'EMAIL', 'REVIEW',
'How was your experience with {{ catering_name }}?',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#3498db">How was your experience?</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p>We hope your event on <strong>{{ event_date }}</strong> was a great success!</p>
<p>Your feedback about <strong>{{ catering_name }}</strong> is very valuable to us and helps other customers make informed decisions.</p>
<p><strong>Please rate your experience:</strong></p>
<p><a href="{{ review_url }}" style="background:#3498db;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Write a Review</a></p>
<p>It will only take a minute!</p>
<p>Thank you,<br>Enyvora Team</p>
</body>
</html>'),

('REVIEW_REQUEST_SMS', 'Review Request SMS', 'en', 'SMS', 'REVIEW',
NULL,
'How was {{ catering_name }}? Rate your experience: {{ review_url }} Your feedback helps others! -Enyvora'),

('REVIEW_PARTNER_NOTIFICATION_INAPP', 'Partner - New Review In-App', 'en', 'INAPP', 'REVIEW',
'New Review Received',
'{{ customer_name }} rated you {{ rating }} stars for order #{{ order_number }}. View review to respond.'),

('REVIEW_PARTNER_NOTIFICATION_EMAIL', 'Partner - New Review Email', 'en', 'EMAIL', 'REVIEW',
'New Review Received from {{ customer_name }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#3498db">New Review Received!</h2>
<p>Dear Partner,</p>
<p><strong>{{ customer_name }}</strong> left a review for order <strong>#{{ order_number }}</strong>:</p>
<p><strong>Rating:</strong> {{ rating }} / 5 stars</p>
<p><strong>Review:</strong><br>{{ review_text }}</p>
<p><a href="{{ respond_url }}" style="background:#3498db;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Respond to Review</a></p>
<p>Responding to reviews shows you value customer feedback and builds trust!</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('REVIEW_RESPONSE_USER_EMAIL', 'User - Partner Responded to Review', 'en', 'EMAIL', 'REVIEW',
'{{ partner_name }} Responded to Your Review',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#27ae60">Partner Responded to Your Review!</h2>
<p>Dear <strong>{{ customer_name }}</strong>,</p>
<p><strong>{{ partner_name }}</strong> responded to your review for order <strong>#{{ order_number }}</strong>:</p>
<p><strong>Their Response:</strong><br>{{ partner_response }}</p>
<p><a href="{{ view_url }}" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">View Full Conversation</a></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>');

-- ============================================
-- SYSTEM & PROMOTIONAL TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('SYSTEM_MAINTENANCE_EMAIL', 'System Maintenance Announcement Email', 'en', 'EMAIL', 'SYSTEM',
'Scheduled Maintenance - {{ maintenance_date }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#f39c12">Scheduled System Maintenance</h2>
<p>Dear User,</p>
<p>We will be performing scheduled maintenance to improve our services.</p>
<p><strong>Maintenance Window:</strong><br>
Date: {{ maintenance_date }}<br>
Start Time: {{ start_time }}<br>
End Time: {{ end_time }}<br>
Duration: Approximately {{ duration }} hours</p>
<p><strong>Impact:</strong><br>
During this time, the platform may be temporarily unavailable. We apologize for any inconvenience.</p>
<p>Thank you for your patience!<br>Enyvora Team</p>
</body>
</html>'),

('SYSTEM_MAINTENANCE_SMS', 'System Maintenance SMS', 'en', 'SMS', 'SYSTEM',
NULL,
'System maintenance on {{ maintenance_date }} from {{ start_time }} to {{ end_time }}. Platform may be temporarily unavailable. -Enyvora'),

('PROMOTION_EMAIL', 'Promotional Email Template', 'en', 'EMAIL', 'PROMOTION',
'{{ promotion_title }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;color:#333}</style></head>
<body>
<h2 style="color:#e74c3c">{{ promotion_title }}</h2>
<div>{{ promotion_message }}</div>
<p><a href="{{ promotion_url }}" style="background:#e74c3c;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">{{ cta_text }}</a></p>
<p><small>To unsubscribe from promotional emails, <a href="{{ unsubscribe_url }}">click here</a></small></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('PROMOTION_SMS', 'Promotional SMS Template', 'en', 'SMS', 'PROMOTION',
NULL,
'{{ promotion_message }} {{ promotion_url }} -Enyvora');

-- ============================================
-- SUPERVISOR LIFECYCLE TEMPLATES
-- ============================================

INSERT INTO t_sys_notification_templates (c_template_code, c_template_name, c_language, c_channel, c_category, c_subject, c_body)
VALUES
('SUPERVISOR_REQUEST_APPROVED_EMAIL', 'Supervisor Request Approved Email', 'en', 'EMAIL', 'SUPERVISOR',
'Your Supervisor Application Has Been Approved - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#27ae60}</style></head>
<body>
<h2>Congratulations, {{ supervisor_name }}!</h2>
<p>We are pleased to inform you that your supervisor application has been <strong>approved</strong>.</p>
<p><strong>Status:</strong> {{ supervisor_status }}</p>
<p>You can now log in to your supervisor portal and start accepting event assignments.</p>
<p><strong>Your Details:</strong><br>
Name: {{ supervisor_name }}<br>
Email: {{ supervisor_email }}<br>
Phone: {{ supervisor_phone }}</p>
<p><a href="{{ app_url }}/supervisor/login" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Login to Supervisor Portal</a></p>
<p>For any questions, contact us at <strong>{{ support_email }}</strong>.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('SUPERVISOR_REQUEST_REJECTED_EMAIL', 'Supervisor Request Rejected Email', 'en', 'EMAIL', 'SUPERVISOR',
'Supervisor Application Update - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#e74c3c}</style></head>
<body>
<h2>Application Status Update</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>Thank you for your interest in joining Enyvora as a supervisor.</p>
<p>After careful review, we regret to inform you that your application has been <strong>rejected</strong>.</p>
<p><strong>Reason:</strong> {{ status_reason }}</p>
<p>You may address the issues mentioned above and re-apply in the future.</p>
<p>If you have any questions, contact us at <strong>{{ support_email }}</strong>.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('SUPERVISOR_REQUEST_UNDER_REVIEW_EMAIL', 'Supervisor Request Under Review Email', 'en', 'EMAIL', 'SUPERVISOR',
'Your Supervisor Application is Under Review - Enyvora',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#f39c12}</style></head>
<body>
<h2>Application Under Review</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>Your supervisor application is currently <strong>under review</strong> by our team.</p>
<p><strong>Status:</strong> {{ supervisor_status }}</p>
<p>Our verification team is reviewing your submitted documents and credentials. This process typically takes 2-3 business days.</p>
<p>We will notify you once a decision has been made.</p>
<p>For any questions, contact us at <strong>{{ support_email }}</strong>.</p>
<p>Best regards,<br>Enyvora Verification Team</p>
</body>
</html>'),

('SUPERVISOR_INFO_REQUESTED_EMAIL', 'Supervisor Info Requested Email', 'en', 'EMAIL', 'SUPERVISOR',
'Additional Information Required - Supervisor Application',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#f39c12}</style></head>
<body>
<h2>Additional Information Required</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>We are reviewing your supervisor application and require some additional information to proceed.</p>
<p><strong>Information Requested:</strong><br>{{ status_reason }}</p>
<p>Please provide the requested information at your earliest convenience so we can continue processing your application.</p>
<p>You can reply to this email or contact us at <strong>{{ support_email }}</strong>.</p>
<p>Best regards,<br>Enyvora Verification Team</p>
</body>
</html>'),

('SUPERVISOR_ASSIGNED_EVENT_EMAIL', 'Supervisor Assigned to Event Email', 'en', 'EMAIL', 'SUPERVISOR',
'New Event Assignment - {{ event_name }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#3498db}table{width:100%;border-collapse:collapse}th,td{padding:10px;text-align:left;border-bottom:1px solid #ddd}th{background:#3498db;color:#fff}</style></head>
<body>
<h2>New Event Assignment</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>You have been assigned to supervise a new event.</p>
<table>
<tr><th>Event Details</th><th></th></tr>
<tr><td>Event Name</td><td><strong>{{ event_name }}</strong></td></tr>
<tr><td>Event Date</td><td>{{ event_date }}</td></tr>
<tr><td>Event Location</td><td>{{ event_location }}</td></tr>
<tr><td>Client Name</td><td>{{ client_name }}</td></tr>
<tr><td>Monitoring Start</td><td>{{ monitoring_start_time }}</td></tr>
<tr><td>Monitoring End</td><td>{{ monitoring_end_time }}</td></tr>
</table>
<p><strong>Action Required:</strong> Please confirm your availability and review the event details in the Supervisor Portal.</p>
<p><a href="{{ app_url }}/supervisor/assignments" style="background:#3498db;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">View Assignment Details</a></p>
<p>For any questions, contact us at <strong>{{ support_email }}</strong>.</p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('SUPERVISOR_EVENT_LIVE_STATUS_EMAIL', 'Supervisor Event Live Status Update Email', 'en', 'EMAIL', 'SUPERVISOR',
'Event Live Update - {{ event_name }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#27ae60}</style></head>
<body>
<h2>Event Live Status Update</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>This is a status update for the event you are supervising:</p>
<p><strong>Event:</strong> {{ event_name }}<br>
<strong>Date:</strong> {{ event_date }}<br>
<strong>Location:</strong> {{ event_location }}<br>
<strong>Client:</strong> {{ client_name }}</p>
<p><strong>Current Status:</strong> {{ supervisor_status }}</p>
<p>Please ensure all monitoring checkpoints are completed on time. Upload photos and status reports via the Supervisor Portal.</p>
<p><a href="{{ app_url }}/supervisor/event-execution" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">Go to Event Execution</a></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>'),

('SUPERVISOR_EVENT_COMPLETED_EMAIL', 'Supervisor Event Completed Email', 'en', 'EMAIL', 'SUPERVISOR',
'Event Completed - {{ event_name }}',
'<!DOCTYPE html>
<html>
<head><style>body{font-family:Arial,sans-serif;line-height:1.6;color:#333}h2{color:#27ae60}</style></head>
<body>
<h2>Event Completed Successfully!</h2>
<p>Dear <strong>{{ supervisor_name }}</strong>,</p>
<p>The event you supervised has been marked as <strong>completed</strong>.</p>
<p><strong>Event:</strong> {{ event_name }}<br>
<strong>Date:</strong> {{ event_date }}<br>
<strong>Location:</strong> {{ event_location }}<br>
<strong>Client:</strong> {{ client_name }}<br>
<strong>Monitoring:</strong> {{ monitoring_start_time }} - {{ monitoring_end_time }}</p>
<p>Thank you for your dedication! Your payment for this assignment will be processed shortly.</p>
<p>Please ensure all final reports and photos are uploaded in the Supervisor Portal.</p>
<p><a href="{{ app_url }}/supervisor/earnings" style="background:#27ae60;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px">View Earnings</a></p>
<p>Best regards,<br>Enyvora Team</p>
</body>
</html>');

DO $$
BEGIN
    RAISE NOTICE 'All notification templates inserted successfully';
    RAISE NOTICE 'Total templates created: 60+ templates covering Email, SMS, and In-App channels (including 7 Supervisor templates)';
END $$;
