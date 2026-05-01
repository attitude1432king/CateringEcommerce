-- =============================================
-- Settings & Config Feature - Database Migration
-- Description: Complete schema for system settings,
--              commission configuration, and email templates
-- Version: 1.0
-- Date: 2026-01-27
-- =============================================

-- =============================================
-- SECTION 1: System Settings Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_settings (
    c_setting_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_setting_key VARCHAR(100) NOT NULL UNIQUE,
    c_setting_value TEXT NOT NULL,
    c_category VARCHAR(50) NOT NULL, -- 'SYSTEM', 'EMAIL', 'PAYMENT', 'BUSINESS', 'NOTIFICATION'
    c_value_type VARCHAR(20) NOT NULL DEFAULT 'STRING', -- 'STRING', 'NUMBER', 'BOOLEAN', 'JSON', 'ENCRYPTED'
    c_display_name VARCHAR(200) NOT NULL,
    c_description VARCHAR(500),
    c_is_sensitive BOOLEAN DEFAULT FALSE,
    c_is_readonly BOOLEAN DEFAULT FALSE,
    c_display_order INTEGER DEFAULT 0,
    c_validation_regex VARCHAR(500),
    c_default_value TEXT,
    c_is_active BOOLEAN DEFAULT TRUE,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_createdby BIGINT,
    c_modifieddate TIMESTAMP,
    c_modifiedby BIGINT
);

CREATE INDEX IF NOT EXISTS ix_settings_category ON t_sys_settings(c_category);
CREATE INDEX IF NOT EXISTS ix_settings_active ON t_sys_settings(c_is_active);

-- Production security cleanup: secrets must come from .NET configuration or environment variables, not t_sys_settings.
DELETE FROM t_sys_settings
WHERE c_setting_key IN (
    'EMAIL.SMTP_USERNAME',
    'EMAIL.SMTP_PASSWORD',
    'PAYMENT.RAZORPAY_KEY_ID',
    'PAYMENT.RAZORPAY_KEY_SECRET',
    'PAYMENT.RAZORPAY_WEBHOOK_SECRET',
    'JWT.KEY',
    'AWS_SNS.ACCESS_KEY',
    'AWS_SNS.SECRET_KEY',
    'SYSTEM.ENCRYPTION_KEY',
    'RABBITMQ.PASSWORD',
    'MSG91.AUTH_KEY'
);

-- =============================================
-- SECTION 2: Settings History Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_settings_history (
    c_history_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_setting_id BIGINT NOT NULL,
    c_setting_key VARCHAR(100) NOT NULL,
    c_old_value TEXT,
    c_new_value TEXT,
    c_changed_by BIGINT NOT NULL,
    c_changed_by_name VARCHAR(200),
    c_change_date TIMESTAMP DEFAULT NOW(),
    c_change_reason VARCHAR(500),
    c_ip_address VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS ix_settings_history_id ON t_sys_settings_history(c_setting_id);
CREATE INDEX IF NOT EXISTS ix_settings_history_date ON t_sys_settings_history(c_change_date DESC);

-- =============================================
-- SECTION 3: Commission Configuration Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_commission_config (
    c_config_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_config_name VARCHAR(200) NOT NULL,
    c_config_type VARCHAR(50) NOT NULL, -- 'GLOBAL', 'CATERING_SPECIFIC', 'TIERED'
    c_ownerid BIGINT NULL,
    c_commission_rate DECIMAL(5, 2) NOT NULL,
    c_fixed_fee DECIMAL(10, 2) DEFAULT 0,
    c_min_order_value DECIMAL(10, 2),
    c_max_order_value DECIMAL(10, 2),
    c_is_active BOOLEAN DEFAULT TRUE,
    c_effective_from DATE NOT NULL,
    c_effective_to DATE NULL,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_createdby BIGINT,
    c_modifieddate TIMESTAMP,
    c_modifiedby BIGINT
);

CREATE INDEX IF NOT EXISTS ix_commission_type ON t_sys_commission_config(c_config_type);
CREATE INDEX IF NOT EXISTS ix_commission_active ON t_sys_commission_config(c_is_active);
CREATE INDEX IF NOT EXISTS ix_commission_dates ON t_sys_commission_config(c_effective_from, c_effective_to);
CREATE INDEX IF NOT EXISTS ix_commission_owner ON t_sys_commission_config(c_ownerid) WHERE c_ownerid IS NOT NULL;

-- =============================================
-- SECTION 4: Template Variables Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_template_variables (
    c_variable_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_template_code VARCHAR(100) NOT NULL,
    c_variable_name VARCHAR(100) NOT NULL,
    c_variable_key VARCHAR(100) NOT NULL, -- e.g., '{{ customer_name }}'
    c_description VARCHAR(500),
    c_example_value VARCHAR(200),
    CONSTRAINT uq_template_variables UNIQUE (c_template_code, c_variable_key)
);

CREATE INDEX IF NOT EXISTS ix_template_variables_code ON t_sys_template_variables(c_template_code);

-- =============================================
-- SECTION 5: Seed Data - SYSTEM Settings
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.APP_NAME', 'Catering Ecommerce Platform', 'SYSTEM', 'STRING', 'Application Name', 'Name of the application displayed to users', 1, 'Catering Ecommerce Platform'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.APP_NAME');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.MAINTENANCE_MODE', 'false', 'SYSTEM', 'BOOLEAN', 'Maintenance Mode', 'Enable maintenance mode to prevent user access', 2, 'false'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAINTENANCE_MODE');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.MAINTENANCE_MESSAGE', 'System is under maintenance. Please check back soon.', 'SYSTEM', 'STRING', 'Maintenance Message', 'Message displayed during maintenance mode', 3, 'System is under maintenance. Please check back soon.'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAINTENANCE_MESSAGE');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.SUPPORT_EMAIL', 'support@cateringecommerce.com', 'SYSTEM', 'STRING', 'Support Email', 'Email address for customer support', 4, 'support@cateringecommerce.com'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SUPPORT_EMAIL');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.SUPPORT_PHONE', '+1-800-CATERING', 'SYSTEM', 'STRING', 'Support Phone', 'Phone number for customer support', 5, '+1-800-CATERING'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SUPPORT_PHONE');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.SESSION_TIMEOUT_MINUTES', '30', 'SYSTEM', 'NUMBER', 'Session Timeout (Minutes)', 'User session timeout duration in minutes', 6, '30', '^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SESSION_TIMEOUT_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.MAX_LOGIN_ATTEMPTS', '5', 'SYSTEM', 'NUMBER', 'Max Login Attempts', 'Maximum failed login attempts before account lockout', 7, '5', '^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAX_LOGIN_ATTEMPTS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.ACCOUNT_LOCKOUT_MINUTES', '15', 'SYSTEM', 'NUMBER', 'Account Lockout Duration (Minutes)', 'Duration for which account is locked after max failed attempts', 8, '15', '^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.ACCOUNT_LOCKOUT_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.PASSWORD_MIN_LENGTH', '8', 'SYSTEM', 'NUMBER', 'Minimum Password Length', 'Minimum length required for user passwords', 9, '8', '^[6-9]|[1-9][0-9]$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.PASSWORD_MIN_LENGTH');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.PASSWORD_REQUIRE_SPECIAL_CHAR', 'true', 'SYSTEM', 'BOOLEAN', 'Require Special Characters in Password', 'Whether passwords must contain special characters', 10, 'true'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.PASSWORD_REQUIRE_SPECIAL_CHAR');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.ENABLE_TWO_FACTOR_AUTH', 'false', 'SYSTEM', 'BOOLEAN', 'Enable Two-Factor Authentication', 'Require 2FA for admin and owner accounts', 11, 'false'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.ENABLE_TWO_FACTOR_AUTH');

-- =============================================
-- SECTION 6: Seed Data - EMAIL Settings
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.SMTP_HOST', 'smtp.gmail.com', 'EMAIL', 'STRING', 'SMTP Host', 'SMTP server hostname', 1, 'smtp.gmail.com'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_HOST');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'EMAIL.SMTP_PORT', '587', 'EMAIL', 'NUMBER', 'SMTP Port', 'SMTP server port number', 2, '587', '^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_PORT');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.ENABLE_SSL', 'true', 'EMAIL', 'BOOLEAN', 'Enable SSL', 'Use SSL/TLS for SMTP connection', 5, 'true'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.ENABLE_SSL');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.FROM_ADDRESS', 'noreply@cateringecommerce.com', 'EMAIL', 'STRING', 'From Email Address', 'Email address used in From field', 6, 'noreply@cateringecommerce.com'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.FROM_ADDRESS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.FROM_NAME', 'Catering Ecommerce', 'EMAIL', 'STRING', 'From Name', 'Display name used in From field', 7, 'Catering Ecommerce'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.FROM_NAME');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.REPLY_TO_ADDRESS', 'support@cateringecommerce.com', 'EMAIL', 'STRING', 'Reply-To Email Address', 'Email address for replies', 8, 'support@cateringecommerce.com'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.REPLY_TO_ADDRESS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'EMAIL.BCC_ADMIN', '', 'EMAIL', 'STRING', 'BCC Admin Email', 'BCC all emails to admin (leave blank to disable)', 9, ''
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.BCC_ADMIN');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'EMAIL.MAX_RETRY_ATTEMPTS', '3', 'EMAIL', 'NUMBER', 'Max Retry Attempts', 'Maximum retry attempts for failed emails', 10, '3', '^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.MAX_RETRY_ATTEMPTS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'EMAIL.RETRY_DELAY_MINUTES', '5', 'EMAIL', 'NUMBER', 'Retry Delay (Minutes)', 'Delay between retry attempts in minutes', 11, '5', '^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.RETRY_DELAY_MINUTES');

-- =============================================
-- SECTION 7: Seed Data - PAYMENT Settings
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'PAYMENT.PAYMENT_TIMEOUT_MINUTES', '15', 'PAYMENT', 'NUMBER', 'Payment Timeout (Minutes)', 'Time limit for completing payment', 4, '15', '^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.PAYMENT_TIMEOUT_MINUTES');

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'PAYMENT.ENABLE_WALLET', 'true', 'PAYMENT', 'BOOLEAN',
       'Enable Wallet Payments', 'Allow customers to pay using wallet', 5, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_WALLET'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'PAYMENT.ENABLE_UPI', 'true', 'PAYMENT', 'BOOLEAN',
       'Enable UPI Payments', 'Allow customers to pay using UPI', 6, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_UPI'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'PAYMENT.ENABLE_NETBANKING', 'true', 'PAYMENT', 'BOOLEAN',
       'Enable Net Banking', 'Allow customers to pay using net banking', 7, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_NETBANKING'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'PAYMENT.REFUND_PROCESSING_DAYS', '7', 'PAYMENT', 'NUMBER',
       'Refund Processing Days', 'Number of business days to process refunds', 8, '7', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.REFUND_PROCESSING_DAYS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'PAYMENT.AUTO_REFUND_ON_CANCEL', 'true', 'PAYMENT', 'BOOLEAN',
       'Auto Refund on Cancel', 'Automatically initiate refund on order cancellation', 9, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.AUTO_REFUND_ON_CANCEL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_is_readonly
)
SELECT 'PAYMENT.CURRENCY', 'INR', 'PAYMENT', 'STRING',
       'Currency Code', 'Currency code for all transactions', 10, 'INR', TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.CURRENCY'
);


-- STEP 8: Insert Default Settings
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.BOOKING_PERCENTAGE', '40.00', 'Booking advance payment percentage', 'PAYMENT', 'DECIMAL', 'Booking Percentage', 11, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.PRE_EVENT_PERCENTAGE', '35.00', 'Pre-event payment percentage', 'PAYMENT', 'DECIMAL', 'Pre-Event Percentage', 12, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.FINAL_PERCENTAGE', '25.00', 'Final settlement payment percentage', 'PAYMENT', 'DECIMAL', 'Final Percentage', 13, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.GUEST_LOCK_DAYS', '5', 'Days before event to lock guest count', 'PAYMENT', 'NUMBER', 'Guest Lock Days', 14, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.MENU_LOCK_DAYS', '3', 'Days before event to lock menu', 'PAYMENT', 'NUMBER', 'Menu Lock Days', 15, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;


-- Due dates
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.BOOKING_DUE_DAYS', '7', 'Days to pay booking invoice', 'PAYMENT', 'NUMBER', 'Booking Due Days', 16, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.PRE_EVENT_DUE_DAYS', '3', 'Days to pay pre-event invoice (before event)', 'PAYMENT', 'NUMBER', 'Pre-Event Due Days', 17, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive)
VALUES ('PAYMENT.FINAL_DUE_DAYS', '7', 'Days to pay final invoice (after event)', 'PAYMENT', 'NUMBER', 'Final Due Days', 18, FALSE)
ON CONFLICT (c_setting_key) DO NOTHING;

    -- =============================================
    -- Add PAYMENT.BANK_* Settings
    -- =============================================
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'PAYMENT.BANK_NAME', 'HDFC Bank', 'PAYMENT', 'STRING', 'Bank Name', 'Name of the bank for receiving payments via bank transfer', 19, 'HDFC Bank'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_NAME');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'PAYMENT.BANK_ACCOUNT_NAME', 'Enyvora Catering Services', 'PAYMENT', 'STRING', 'Account Name', 'Account holder name for receiving payments', 20, 'Enyvora Catering Services'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_ACCOUNT_NAME');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
SELECT 'PAYMENT.BANK_ACCOUNT_NUMBER', '1234567890123456', 'PAYMENT', 'STRING', 'Account Number', 'Bank account number (masked for non-admin users)', 21, '1234567890123456', FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_ACCOUNT_NUMBER');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'PAYMENT.BANK_IFSC_CODE', 'HDFC0001234', 'PAYMENT', 'STRING', 'IFSC Code', 'Bank IFSC code for NEFT/RTGS transfers', 22, 'HDFC0001234', '^[A-Z]{4}0[A-Z0-9]{6}$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_IFSC_CODE');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'PAYMENT.BANK_TRANSFER_ENABLED', 'true', 'PAYMENT', 'BOOLEAN', 'Enable Bank Transfer', 'Allow customers to pay via bank transfer', 23, 'true'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_TRANSFER_ENABLED');


-- =============================================
-- SECTION 8: Seed Data - BUSINESS Settings
-- =============================================

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.DEFAULT_COMMISSION_RATE', '15', 'BUSINESS', 'NUMBER',
       'Default Commission Rate (%)', 'Default commission rate percentage for catering partners',
       1, '15', '^[0-9]+(\.[0-9]+)?$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.DEFAULT_COMMISSION_RATE'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.MIN_ORDER_VALUE', '500', 'BUSINESS', 'NUMBER',
       'Minimum Order Value', 'Minimum order value in rupees',
       2, '500', '^[0-9]+(\.[0-9]+)?$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ORDER_VALUE'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.MAX_ADVANCE_BOOKING_DAYS', '90', 'BUSINESS', 'NUMBER',
       'Max Advance Booking Days', 'Maximum days in advance customers can book',
       3, '90', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_ADVANCE_BOOKING_DAYS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.MIN_ADVANCE_BOOKING_HOURS', '24', 'BUSINESS', 'NUMBER',
       'Min Advance Booking Hours', 'Minimum hours in advance required for booking',
       4, '24', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_HOURS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.CANCELLATION_WINDOW_HOURS', '48', 'BUSINESS', 'NUMBER',
       'Cancellation Window (Hours)', 'Hours before event when free cancellation is allowed',
       5, '48', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.CANCELLATION_WINDOW_HOURS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.CANCELLATION_FEE_PERCENTAGE', '20', 'BUSINESS', 'NUMBER',
       'Cancellation Fee (%)', 'Cancellation fee percentage after free cancellation window',
       6, '20', '^[0-9]+(\.[0-9]+)?$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.CANCELLATION_FEE_PERCENTAGE'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.MAX_GUESTS_PER_ORDER', '1000', 'BUSINESS', 'NUMBER',
       'Max Guests Per Order', 'Maximum number of guests allowed per order',
       7, '1000', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_GUESTS_PER_ORDER'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'BUSINESS.ENABLE_REVIEWS', 'true', 'BUSINESS', 'BOOLEAN',
       'Enable Reviews', 'Allow customers to leave reviews',
       8, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_REVIEWS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'BUSINESS.AUTO_APPROVE_REVIEWS', 'false', 'BUSINESS', 'BOOLEAN',
       'Auto Approve Reviews', 'Automatically approve customer reviews without moderation',
       9, 'false'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.AUTO_APPROVE_REVIEWS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'BUSINESS.ENABLE_DISCOUNTS', 'true', 'BUSINESS', 'BOOLEAN',
       'Enable Discounts', 'Allow catering partners to create discount offers',
       10, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_DISCOUNTS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'BUSINESS.PARTNER_PAYOUT_DAYS', '7', 'BUSINESS', 'NUMBER',
       'Partner Payout Days', 'Days after order completion to process partner payout',
       11, '7', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.PARTNER_PAYOUT_DAYS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'BUSINESS.ENABLE_PARTNER_APPROVAL', 'true', 'BUSINESS', 'BOOLEAN',
       'Enable Partner Approval', 'Require admin approval for new partner registrations',
       12, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_PARTNER_APPROVAL'
);

-- =============================================
-- SECTION 9: Seed Data - NOTIFICATION Settings
-- =============================================

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ENABLE_EMAIL', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Enable Email Notifications', 'Send notifications via email',
       1, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_EMAIL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ENABLE_SMS', 'false', 'NOTIFICATION', 'BOOLEAN',
       'Enable SMS Notifications', 'Send notifications via SMS',
       2, 'false'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_SMS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ENABLE_IN_APP', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Enable In-App Notifications', 'Send notifications within the application',
       3, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_IN_APP'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ENABLE_PUSH', 'false', 'NOTIFICATION', 'BOOLEAN',
       'Enable Push Notifications', 'Send push notifications to mobile devices',
       4, 'false'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_PUSH'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ORDER_CONFIRMATION', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Order Confirmation Notifications', 'Send notifications on order confirmation',
       5, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ORDER_CONFIRMATION'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.ORDER_STATUS_UPDATES', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Order Status Update Notifications', 'Send notifications on order status changes',
       6, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ORDER_STATUS_UPDATES'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.PAYMENT_CONFIRMATIONS', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Payment Confirmation Notifications', 'Send notifications on payment confirmation',
       7, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.PAYMENT_CONFIRMATIONS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.CANCELLATION_CONFIRMATIONS', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Cancellation Confirmation Notifications', 'Send notifications on order cancellation',
       8, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.CANCELLATION_CONFIRMATIONS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.PARTNER_APPROVAL', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Partner Approval Notifications', 'Send notifications on partner approval/rejection',
       9, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.PARTNER_APPROVAL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.REVIEW_REMINDERS', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Review Reminder Notifications', 'Send reminders to customers to leave reviews',
       10, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.REVIEW_REMINDERS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 'NOTIFICATION.EVENT_REMINDERS', 'true', 'NOTIFICATION', 'BOOLEAN',
       'Event Reminder Notifications', 'Send reminders before event date',
       11, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.EVENT_REMINDERS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value, c_validation_regex
)
SELECT 'NOTIFICATION.EVENT_REMINDER_HOURS', '24', 'NOTIFICATION', 'NUMBER',
       'Event Reminder Hours', 'Hours before event to send reminder notification',
       12, '24', '^[0-9]+$'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.EVENT_REMINDER_HOURS'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_default_value, c_is_sensitive)
SELECT 'NOTIFICATION.INVOICE_EMAIL_ENABLED', 'true', 'Send invoice emails to customers', 'NOTIFICATION', 'BOOLEAN', 'Invoice Email Notifications', 13, 'true', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.INVOICE_EMAIL_ENABLED'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_default_value, c_is_sensitive)
SELECT 'NOTIFICATION.INVOICE_SMS_ENABLED', 'true', 'Send invoice SMS to customers', 'NOTIFICATION', 'BOOLEAN', 'Invoice SMS Notifications', 14, 'true', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.INVOICE_SMS_ENABLED'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_default_value, c_is_sensitive)
SELECT 'NOTIFICATION.PAYMENT_REMINDER_ENABLED', 'true', 'Send payment reminders', 'NOTIFICATION', 'BOOLEAN', 'Payment Reminder Notifications', 15, 'true', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.PAYMENT_REMINDER_ENABLED'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_default_value, c_is_sensitive)
SELECT 'NOTIFICATION.REMINDER_INTERVALS', '7,3,1', 'Days before due date to send reminders (comma-separated)', 'NOTIFICATION', 'STRING', 'Reminder Intervals', 16, '7,3,1', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.REMINDER_INTERVALS'
);



-- =============================================
-- SECTION 10: Seed Data - GST Settings
-- =============================================
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.ENABLED', 'true', 'Enable GST calculations', 'GST', 'BOOLEAN', 'GST Enabled', 1, FALSE, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.ENABLED'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.CGST_RATE', '9.00', 'CGST rate percentage', 'GST', 'DECIMAL', 'CGST Rate', 2, FALSE, '9.00'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.CGST_RATE'
);
    
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.SGST_RATE', '9.00', 'SGST rate percentage', 'GST', 'DECIMAL', 'SGST Rate', 3, FALSE, '9.00'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.SGST_RATE'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.SAC_CODE', '996331', 'SAC code for outdoor catering services', 'GST', 'STRING', 'SAC Code', 4, FALSE, '996331'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.SAC_CODE'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.COMPANY_GSTIN', 'UPDATE_WITH_YOUR_GSTIN', 'Company GSTIN number', 'GST', 'STRING', 'Company GSTIN', 5, FALSE, 'UPDATE_WITH_YOUR_GSTIN'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.COMPANY_GSTIN'
);


INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'GST.PLACE_OF_SUPPLY', 'Surat', 'Default place of supply for GST', 'GST', 'STRING', 'Place of Supply', 6, FALSE, 'Surat'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'GST.PLACE_OF_SUPPLY'
);



-- =============================================
-- SECTION 11: Seed Data - Invoice settings
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'INVOICE.PREFIX', 'INV', 'Invoice number prefix', 'INVOICE', 'STRING', 'Invoice Prefix', 1, FALSE, 'INV'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'INVOICE.PREFIX'
);

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_category, c_value_type, c_display_name, c_display_order, c_is_sensitive, c_default_value)
SELECT 'INVOICE.AUTO_GENERATE_ENABLED', 'true', 'Enable automatic invoice generation', 'INVOICE', 'BOOLEAN', 'Auto Generate Invoices', 2, FALSE, 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'INVOICE.AUTO_GENERATE_ENABLED'
);


-- =============================================
-- SECTION 12: Seed Data - Default Commission Configs
-- =============================================

INSERT INTO t_sys_commission_config (
    c_config_name,
    c_config_type,
    c_commission_rate,
    c_fixed_fee,
    c_is_active,
    c_effective_from
)
SELECT 
    'Default Global Commission',
    'GLOBAL',
    15.00,
    0.00,
    TRUE,
    '2026-01-01'
WHERE NOT EXISTS (
    SELECT 1 
    FROM t_sys_commission_config 
    WHERE c_config_type = 'GLOBAL'
);
-- =============================================
-- TEMPLATE VARIABLES SEED (PostgreSQL)
-- =============================================

-- =============================================
-- COMMON TEMPLATES
-- =============================================

WITH CommonTemplates AS (
    SELECT * FROM (VALUES
        ('USER_REGISTRATION'), ('USER_EMAIL_VERIFICATION'), ('USER_PASSWORD_RESET'),
        ('ORDER_CONFIRMATION'), ('ORDER_STATUS_UPDATE'), ('PAYMENT_SUCCESS'),
        ('PAYMENT_FAILED'), ('ORDER_CANCELLED'), ('REFUND_INITIATED'),
        ('OWNER_REGISTRATION'), ('PARTNER_APPROVED'), ('PARTNER_REJECTED'),
        ('ADMIN_USER_CREATED')
    ) AS t("TemplateCode")
)

-- Customer Name
INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    tc."TemplateCode",
    'Customer Name',
    '{{ customer_name }}',
    'Full name of the customer or user',
    'John Doe'
FROM CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc."TemplateCode"
      AND c_variable_key = '{{ customer_name }}'
);

-- App Name
WITH CommonTemplates AS (
    SELECT * FROM (VALUES
        ('USER_REGISTRATION'), ('USER_EMAIL_VERIFICATION'), ('USER_PASSWORD_RESET'),
        ('ORDER_CONFIRMATION'), ('ORDER_STATUS_UPDATE'), ('PAYMENT_SUCCESS'),
        ('PAYMENT_FAILED'), ('ORDER_CANCELLED'), ('REFUND_INITIATED'),
        ('OWNER_REGISTRATION'), ('PARTNER_APPROVED'), ('PARTNER_REJECTED'),
        ('ADMIN_USER_CREATED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    tc."TemplateCode",
    'App Name',
    '{{ app_name }}',
    'Application name',
    'Catering Ecommerce Platform'
FROM CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc."TemplateCode"
      AND c_variable_key = '{{ app_name }}'
);

-- Support Email
WITH CommonTemplates AS (
    SELECT * FROM (VALUES
        ('USER_REGISTRATION'), ('USER_EMAIL_VERIFICATION'), ('USER_PASSWORD_RESET'),
        ('ORDER_CONFIRMATION'), ('ORDER_STATUS_UPDATE'), ('PAYMENT_SUCCESS'),
        ('PAYMENT_FAILED'), ('ORDER_CANCELLED'), ('REFUND_INITIATED'),
        ('OWNER_REGISTRATION'), ('PARTNER_APPROVED'), ('PARTNER_REJECTED'),
        ('ADMIN_USER_CREATED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    tc."TemplateCode",
    'Support Email',
    '{{ support_email }}',
    'Customer support email address',
    'support@cateringecommerce.com'
FROM CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc."TemplateCode"
      AND c_variable_key = '{{ support_email }}'
);

-- =============================================
-- ORDER / PAYMENT VARIABLES
-- =============================================

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT *
FROM (VALUES
    ('ORDER_CONFIRMATION', 'Order ID', '{{ order_id }}', 'Unique order identifier', 'ORD-2026-00123'),
    ('ORDER_CONFIRMATION', 'Order Date', '{{ order_date }}', 'Date when order was placed', '27-Jan-2026'),
    ('ORDER_CONFIRMATION', 'Event Date', '{{ event_date }}', 'Date of the catering event', '15-Feb-2026'),
    ('ORDER_CONFIRMATION', 'Total Amount', '{{ total_amount }}', 'Total order amount', '₹25,000'),
    ('ORDER_CONFIRMATION', 'Catering Name', '{{ catering_name }}', 'Name of the catering service', 'Delicious Caterers'),

    ('PAYMENT_SUCCESS', 'Order ID', '{{ order_id }}', 'Unique order identifier', 'ORD-2026-00123'),
    ('PAYMENT_SUCCESS', 'Payment ID', '{{ payment_id }}', 'Payment transaction ID', 'pay_XXXXXXXXXX'),
    ('PAYMENT_SUCCESS', 'Amount Paid', '{{ amount_paid }}', 'Amount paid', '₹25,000'),

    ('REFUND_INITIATED', 'Order ID', '{{ order_id }}', 'Unique order identifier', 'ORD-2026-00123'),
    ('REFUND_INITIATED', 'Refund Amount', '{{ refund_amount }}', 'Amount to be refunded', '₹20,000'),
    ('REFUND_INITIATED', 'Refund Reason', '{{ refund_reason }}', 'Reason for refund', 'Order cancelled by customer')
) AS v(c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables t
    WHERE t.c_template_code = v.c_template_code
      AND t.c_variable_key = v.c_variable_key
);

-- =============================================
-- AUTH VARIABLES
-- =============================================

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT *
FROM (VALUES
    ('USER_EMAIL_VERIFICATION', 'Verification Link', '{{ verification_link }}', 'Email verification link', 'https://example.com/verify?token=xxxxx'),
    ('USER_PASSWORD_RESET', 'Reset Link', '{{ reset_link }}', 'Password reset link', 'https://example.com/reset?token=xxxxx'),
    ('USER_PASSWORD_RESET', 'Expiry Time', '{{ expiry_time }}', 'Link expiry time', '1 hour')
) AS v(c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables t
    WHERE t.c_template_code = v.c_template_code
      AND t.c_variable_key = v.c_variable_key
);

-- =============================================
-- PARTNER VARIABLES
-- =============================================

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT *
FROM (VALUES
    ('PARTNER_APPROVED', 'Business Name', '{{ business_name }}', 'Catering business name', 'Delicious Caterers'),
    ('PARTNER_APPROVED', 'Owner Name', '{{ owner_name }}', 'Owner full name', 'Jane Smith'),
    ('PARTNER_APPROVED', 'Login Link', '{{ login_link }}', 'Partner login page link', 'https://example.com/owner/login'),

    ('PARTNER_REJECTED', 'Business Name', '{{ business_name }}', 'Catering business name', 'Delicious Caterers'),
    ('PARTNER_REJECTED', 'Rejection Reason', '{{ rejection_reason }}', 'Reason for rejection', 'Incomplete documentation')
) AS v(c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables t
    WHERE t.c_template_code = v.c_template_code
      AND t.c_variable_key = v.c_variable_key
);

-- =============================================
-- SUPERVISOR VARIABLES
-- =============================================

WITH SupervisorTemplates AS (
    SELECT * FROM (VALUES
        ('SUPERVISOR_REQUEST_APPROVED'),
        ('SUPERVISOR_REQUEST_REJECTED'),
        ('SUPERVISOR_REQUEST_UNDER_REVIEW'),
        ('SUPERVISOR_INFO_REQUESTED'),
        ('SUPERVISOR_ASSIGNED_EVENT'),
        ('SUPERVISOR_EVENT_LIVE_STATUS'),
        ('SUPERVISOR_EVENT_COMPLETED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    st."TemplateCode",
    'Supervisor Name',
    '{{ supervisor_name }}',
    'Full name of the supervisor',
    'Rahul Sharma'
FROM SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st."TemplateCode"
      AND c_variable_key = '{{ supervisor_name }}'
);

WITH SupervisorTemplates AS (
    SELECT * FROM (VALUES
        ('SUPERVISOR_REQUEST_APPROVED'),
        ('SUPERVISOR_REQUEST_REJECTED'),
        ('SUPERVISOR_REQUEST_UNDER_REVIEW'),
        ('SUPERVISOR_INFO_REQUESTED'),
        ('SUPERVISOR_ASSIGNED_EVENT'),
        ('SUPERVISOR_EVENT_LIVE_STATUS'),
        ('SUPERVISOR_EVENT_COMPLETED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    st."TemplateCode",
    'Supervisor Email',
    '{{ supervisor_email }}',
    'Email address of the supervisor',
    'rahul.sharma@example.com'
FROM SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st."TemplateCode"
      AND c_variable_key = '{{ supervisor_email }}'
);

WITH SupervisorTemplates AS (
    SELECT * FROM (VALUES
        ('SUPERVISOR_REQUEST_APPROVED'),
        ('SUPERVISOR_REQUEST_REJECTED'),
        ('SUPERVISOR_REQUEST_UNDER_REVIEW'),
        ('SUPERVISOR_INFO_REQUESTED'),
        ('SUPERVISOR_ASSIGNED_EVENT'),
        ('SUPERVISOR_EVENT_LIVE_STATUS'),
        ('SUPERVISOR_EVENT_COMPLETED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    st."TemplateCode",
    'Supervisor Phone',
    '{{ supervisor_phone }}',
    'Phone number of the supervisor',
    '+91-9876543210'
FROM SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st."TemplateCode"
      AND c_variable_key = '{{ supervisor_phone }}'
);

WITH SupervisorTemplates AS (
    SELECT * FROM (VALUES
        ('SUPERVISOR_REQUEST_APPROVED'),
        ('SUPERVISOR_REQUEST_REJECTED'),
        ('SUPERVISOR_REQUEST_UNDER_REVIEW'),
        ('SUPERVISOR_INFO_REQUESTED'),
        ('SUPERVISOR_ASSIGNED_EVENT'),
        ('SUPERVISOR_EVENT_LIVE_STATUS'),
        ('SUPERVISOR_EVENT_COMPLETED')
    ) AS t("TemplateCode")
)

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT
    st."TemplateCode",
    'Supervisor Status',
    '{{ supervisor_status }}',
    'Current status of the supervisor application or assignment',
    'Approved'
FROM SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st."TemplateCode"
      AND c_variable_key = '{{ supervisor_status }}'
);


INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT *
FROM (VALUES
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Name', '{{ event_name }}', 'Name of the catering event', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Date', '{{ event_date }}', 'Date of the event', '15-Mar-2026'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Location', '{{ event_location }}', 'Location of the event', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Client Name', '{{ client_name }}', 'Name of the client', 'Amit Sharma'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Monitoring Start Time', '{{ monitoring_start_time }}', 'Start time', '10:00 AM'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Monitoring End Time', '{{ monitoring_end_time }}', 'End time', '06:00 PM'),

    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Name', '{{ event_name }}', 'Event name', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Date', '{{ event_date }}', 'Event date', '15-Mar-2026'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Location', '{{ event_location }}', 'Event location', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Client Name', '{{ client_name }}', 'Client name', 'Amit Sharma'),

    ('SUPERVISOR_EVENT_COMPLETED', 'Event Name', '{{ event_name }}', 'Event name', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Event Date', '{{ event_date }}', 'Event date', '15-Mar-2026'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Event Location', '{{ event_location }}', 'Event location', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Client Name', '{{ client_name }}', 'Client name', 'Amit Sharma'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Monitoring Start Time', '{{ monitoring_start_time }}', 'Start time', '10:00 AM'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Monitoring End Time', '{{ monitoring_end_time }}', 'End time', '06:00 PM')
) AS v(c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables t
    WHERE t.c_template_code = v.c_template_code
      AND t.c_variable_key = v.c_variable_key
);

INSERT INTO t_sys_template_variables (
    c_template_code, c_variable_name, c_variable_key, c_description, c_example_value
)
SELECT *
FROM (VALUES
    ('SUPERVISOR_REQUEST_REJECTED', 'Status Reason', '{{ status_reason }}', 'Reason for rejection', 'Incomplete documentation submitted'),
    ('SUPERVISOR_INFO_REQUESTED', 'Status Reason', '{{ status_reason }}', 'Information requested', 'Please upload documents')
) AS v(c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables t
    WHERE t.c_template_code = v.c_template_code
      AND t.c_variable_key = v.c_variable_key
);


-- =============================================
-- SECTION 12: JWT SETTINGS
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive, c_is_readonly)
SELECT 'JWT.ISSUER','https://localhost:44368','JWT','STRING','JWT Issuer','Issuer URL for JWT tokens',2,'https://localhost:44368',FALSE,FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='JWT.ISSUER');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'JWT.AUDIENCE','https://localhost:5173/','JWT','STRING','JWT Audience','Audience URL for JWT tokens',3,'https://localhost:5173/'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='JWT.AUDIENCE');
    
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'JWT.EXPIRE_MINUTES','60','JWT','NUMBER','JWT Expiry (Minutes)','JWT token expiration time in minutes',4,'60','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='JWT.EXPIRE_MINUTES');


-- =============================================
-- SECTION 13: AWS SNS SETTINGS
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
SELECT 'AWS_SNS.REGION','ap-south-1','NOTIFICATION','STRING','AWS SNS Region','AWS SNS Region',3,'ap-south-1',FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='AWS_SNS.REGION');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
SELECT 'AWS_SNS.SENDER_ID','ENYVORA','NOTIFICATION','STRING','AWS SNS Sender ID','SMS Sender ID',4,'ENYVORA',FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='AWS_SNS.SENDER_ID');


-- =============================================
-- SECTION 14: SYSTEM SETTINGS
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
SELECT 'SYSTEM.API_BASE_URL','https://localhost:44368','SYSTEM','STRING','API Base URL','Base URL',21,'https://localhost:44368'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SYSTEM.API_BASE_URL');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.OTP_EXPIRY_SECONDS','300','SYSTEM','NUMBER','OTP Expiry','OTP expiry seconds',22,'300','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SYSTEM.OTP_EXPIRY_SECONDS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.COOKIE_EXPIRY_DAYS','7','SYSTEM','NUMBER','Cookie Expiry','Cookie expiry days',23,'7','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SYSTEM.COOKIE_EXPIRY_DAYS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SYSTEM.GEO_CACHE_HOURS','6','SYSTEM','NUMBER','Geo Cache','Geo cache duration',24,'6','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SYSTEM.GEO_CACHE_HOURS');


-- =============================================
-- SECTION 15: SECURITY SETTINGS
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SECURITY.ADMIN_LOGIN_PERMITS','3','SECURITY','NUMBER','Admin Login Attempts','Max admin login attempts',1,'3','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.ADMIN_LOGIN_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SECURITY.ADMIN_LOGIN_WINDOW_MINUTES','15','SECURITY','NUMBER','Admin Login Window','Login window',2,'15','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.ADMIN_LOGIN_WINDOW_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.USER_LOGIN_PERMITS','5','SECURITY','NUMBER','User Login Attempts','Max user login attempts',3,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.USER_LOGIN_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'SECURITY.USER_LOGIN_WINDOW_MINUTES','10','SECURITY','NUMBER','User Login Window','Login window',4,'10','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.USER_LOGIN_WINDOW_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.OTP_SEND_PERMITS','3','SECURITY','NUMBER','OTP Send Limit','OTP send limit',5,'3','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.OTP_SEND_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.OTP_SEND_WINDOW_MINUTES','60','SECURITY','NUMBER','OTP Send Window','OTP send window',6,'60','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.OTP_SEND_WINDOW_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.OTP_VERIFY_PERMITS','5','SECURITY','NUMBER','OTP Verify Limit','OTP verify limit',7,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.OTP_VERIFY_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.OTP_VERIFY_WINDOW_MINUTES','5','SECURITY','NUMBER','OTP Verify Window','OTP verify window',8,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.OTP_VERIFY_WINDOW_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.API_GENERAL_PERMITS','100','SECURITY','NUMBER','API Limit','API rate limit',9,'100','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.API_GENERAL_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)  
SELECT 'SECURITY.API_GENERAL_WINDOW_MINUTES','1','SECURITY','NUMBER','API Window','API window',10,'1','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.API_GENERAL_WINDOW_MINUTES');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)      
SELECT 'SECURITY.FILE_UPLOAD_PERMITS','10','SECURITY','NUMBER','Upload Limit','File upload limit',11,'10','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.FILE_UPLOAD_PERMITS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)      
SELECT 'SECURITY.FILE_UPLOAD_WINDOW_MINUTES','10','SECURITY','NUMBER','Upload Window','Upload window',12,'10','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='SECURITY.FILE_UPLOAD_WINDOW_MINUTES');


-- =============================================
-- SECTION 16: BUSINESS SETTINGS
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MIN_WITHDRAWAL_AMOUNT','500','BUSINESS','NUMBER','Min Withdrawal Amount','Minimum amount for withdrawal',13,'500','^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MIN_WITHDRAWAL_AMOUNT');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MAX_ADDRESSES_PER_USER','5','BUSINESS','NUMBER','Max Addresses','Max addresses per user',14,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MAX_ADDRESSES_PER_USER');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MIN_GUESTS_PER_ORDER','50','BUSINESS','NUMBER','Min Guests','Minimum guests per order',15,'50','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MIN_GUESTS_PER_ORDER');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MIN_ADVANCE_PAYMENT_PERCENT','30','BUSINESS','NUMBER','Advance Payment %','Minimum advance %',16,'30','^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MIN_ADVANCE_PAYMENT_PERCENT');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS','5','BUSINESS','NUMBER','Min Booking Days','Minimum booking days',17,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MIN_ADVANCE_BOOKING_DAYS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MIN_ADVANCE_BOOKING_HOURS','24','BUSINESS','NUMBER','Min Booking Hours','Minimum booking hours',18,'24','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MIN_ADVANCE_BOOKING_HOURS');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.MAX_ADVANCE_BOOKING_DAYS','90','BUSINESS','NUMBER','Max Booking Days','Max booking days',19,'90','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.MAX_ADVANCE_BOOKING_DAYS');

-- Cancellation policy tier definitions
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('CANCELLATION.FULL_REFUND_DAYS', '7', 'BUSINESS', 'NUMBER', 'Full Refund Period (Days)', 'Days before event when 100% refund is allowed', 20, '7')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('CANCELLATION.PARTIAL_REFUND_DAYS_START', '3', 'BUSINESS', 'NUMBER', 'Partial Refund Start (Days)', 'Starting days for 50% refund window', 21, '3')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('CANCELLATION.PARTIAL_REFUND_DAYS_END', '7', 'BUSINESS', 'NUMBER', 'Partial Refund End (Days)', 'Ending days for 50% refund window', 22, '7')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('CANCELLATION.NO_REFUND_HOURS', '48', 'BUSINESS', 'NUMBER', 'No Refund Window (Hours)', 'Hours before event when no refund is given', 23, '48')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('CANCELLATION.PARTIAL_REFUND_PERCENTAGE', '50', 'BUSINESS', 'NUMBER', 'Partial Refund Percentage', 'Refund percentage for partial refund tier', 24, '50')
ON CONFLICT (c_setting_key) DO NOTHING;

-- Guest count locking settings
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('ORDER.GUEST_COUNT_LOCK_DAYS', '5', 'BUSINESS', 'NUMBER', 'Guest Count Lock Period (Days)', 'Days before event when guest count is locked', 25, '5')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('ORDER.MENU_LOCK_DAYS', '3', 'BUSINESS', 'NUMBER', 'Menu Lock Period (Days)', 'Days before event when menu is locked', 26, '3')
ON CONFLICT (c_setting_key) DO NOTHING;

-- Partner security deposit amount
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('PARTNER.SECURITY_DEPOSIT_AMOUNT', '25000', 'BUSINESS', 'NUMBER', 'Partner Security Deposit (₹)', 'Required security deposit amount from partners', 27, '25000')
ON CONFLICT (c_setting_key) DO NOTHING;

-- Dispute resolution SLA
INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
VALUES ('DISPUTE.RESOLUTION_SLA_HOURS', '12', 'BUSINESS', 'NUMBER', 'Dispute Resolution SLA (Hours)', 'Maximum hours to resolve a customer complaint', 28, '12')
ON CONFLICT (c_setting_key) DO NOTHING;

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY','2','BUSINESS','NUMBER','Default Daily Booking Capacity','Fallback booking capacity when no specific capacity configured',29,'5','^[1-9][0-9]*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY');


-- =============================================
-- SECTION 17: RABBITMQ
-- =============================================

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'RABBITMQ.ENABLED','false','RABBITMQ','BOOLEAN','RabbitMQ Enabled','Enable MQ',1,'false','^(true|false)$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='RABBITMQ.ENABLED');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'RABBITMQ.HOSTNAME','localhost','RABBITMQ','STRING','Hostname','MQ Host',2,'localhost','^.*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='RABBITMQ.HOSTNAME');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'RABBITMQ.PORT','5672','RABBITMQ','NUMBER','Port','MQ Port',3,'5672','^[0-9]+$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='RABBITMQ.PORT');

INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
SELECT 'RABBITMQ.USERNAME','guest','RABBITMQ','STRING','Username','MQ User',4,'guest','^.*$'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key='RABBITMQ.USERNAME');

-- =============================================
-- SECTION 18: Seed Data - APP Settings (PostgreSQL)
-- =============================================

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.PARTNER_PORTAL_URL',
    'https://partner.enyvora.com/login',
    'APP',
    'STRING',
    'Partner Portal URL',
    'Login URL for the partner portal',
    1,
    'https://partner.enyvora.com/login'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_PORTAL_URL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.PARTNER_GUIDE_URL',
    'https://enyvora.com/partner-guide',
    'APP',
    'STRING',
    'Partner Guide URL',
    'URL for the partner onboarding guide',
    2,
    'https://enyvora.com/partner-guide'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_GUIDE_URL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.BEST_PRACTICES_URL',
    'https://enyvora.com/best-practices',
    'APP',
    'STRING',
    'Best Practices URL',
    'URL for partner best practices page',
    3,
    'https://enyvora.com/best-practices'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.BEST_PRACTICES_URL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.SUPPORT_URL',
    'https://enyvora.com/support',
    'APP',
    'STRING',
    'Support URL',
    'URL for the main support page',
    4,
    'https://enyvora.com/support'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.SUPPORT_URL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.PARTNER_SUPPORT_EMAIL',
    'support@enyvora.com',
    'APP',
    'STRING',
    'Partner Support Email',
    'Email address for partner support inquiries',
    5,
    'support@enyvora.com'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_SUPPORT_EMAIL'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.PARTNER_SUPPORT_PHONE',
    '+91-1234567890',
    'APP',
    'STRING',
    'Partner Support Phone',
    'Phone number for partner support',
    6,
    '+91-1234567890'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_SUPPORT_PHONE'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'APP.REAPPLY_DURATION',
    '30 days',
    'APP',
    'STRING',
    'Reapply Duration',
    'Duration after rejection before partner can reapply',
    7,
    '30 days'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.REAPPLY_DURATION'
);


-- =============================================
-- SECTION 19: MSG91 Provider Settings (PostgreSQL)
-- =============================================

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'MSG91.SENDER_ID',
    'CATAPP',
    'MSG91',
    'STRING',
    FALSE,
    'MSG91 Sender ID',
    '6-character alphanumeric sender ID registered with MSG91',
    2,
    'CATAPP'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.SENDER_ID'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'MSG91.TEMPLATE_ID',
    '',
    'MSG91',
    'STRING',
    FALSE,
    'MSG91 OTP Template ID',
    'DLT-approved OTP template ID from MSG91 dashboard',
    3,
    ''
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.TEMPLATE_ID'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'MSG91.ROUTE',
    '4',
    'MSG91',
    'STRING',
    FALSE,
    'MSG91 Route',
    '4 = transactional (OTP), 1 = promotional',
    4,
    '4'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.ROUTE'
);


-- =============================================
-- SECTION 20: OTP Service Settings (PostgreSQL)
-- =============================================

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'SMS.PROVIDER',
    'MSG91',
    'SMS',
    'STRING',
    FALSE,
    'Active SMS Provider',
    'OTP SMS provider: MSG91 (fixed). Order/system SMS uses AWS SNS.',
    1,
    'MSG91'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SMS.PROVIDER'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'OTP.LENGTH',
    '6',
    'OTP',
    'STRING',
    FALSE,
    'OTP Length',
    'Number of digits in generated OTP (default: 6)',
    1,
    '6'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.LENGTH'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'OTP.EXPIRY_MINUTES',
    '10',
    'OTP',
    'STRING',
    FALSE,
    'OTP Expiry (minutes)',
    'Time in minutes before OTP expires (default: 10)',
    2,
    '10'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.EXPIRY_MINUTES'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'OTP.MAX_VERIFY_ATTEMPTS',
    '5',
    'OTP',
    'STRING',
    FALSE,
    'OTP Max Verify Attempts',
    'Maximum wrong OTP attempts before entry is locked (default: 5)',
    3,
    '5'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.MAX_VERIFY_ATTEMPTS'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'OTP.BYPASS_VERIFICATION',
    'false',
    'OTP',
    'STRING',
    FALSE,
    'Bypass OTP Verification',
    'Set to true only for development/testing to skip actual OTP check. MUST be false in production.',
    4,
    'false'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.BYPASS_VERIFICATION'
);


INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'OTP.MESSAGE_TEMPLATE',
    'Your {AppName} OTP is {OTP}. Valid for {EXPIRY} minutes. Do not share this code with anyone.',
    'OTP',
    'STRING',
    FALSE,
    'OTP SMS Template',
    'SMS template for OTP delivery. Placeholders: {AppName}, {OTP}, {EXPIRY}',
    5,
    'Your {AppName} OTP is {OTP}. Valid for {EXPIRY} minutes. Do not share this code with anyone.'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.MESSAGE_TEMPLATE'
);


-- =============================================
-- SECTION 21: STATS
-- =============================================\

INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value
)
SELECT 
    'STATS.AVG_GROWTH_PERCENT',
    '150',
    'STATS',
    'INT',
    FALSE,
    'Avg Partner Growth (%)',
    'Average revenue growth percentage shown on the Partner Login page stat counter',
    1,
    '150'
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'STATS.AVG_GROWTH_PERCENT'
);


-- =============================================
-- 22. CORS CONFIGURATION
-- =============================================
INSERT INTO t_sys_settings (
    c_setting_key, c_setting_value, c_category, c_value_type,
    c_display_name, c_description, c_is_sensitive, c_createddate
)
SELECT 'CORS.PRODUCTION_ORIGIN', 'https://yourdomain.com', 'SECURITY', 'STRING',
       'Production CORS Origin',
       'Production frontend domain for CORS policy. Update this with your actual production domain.',
       FALSE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CORS.PRODUCTION_ORIGIN'
);


