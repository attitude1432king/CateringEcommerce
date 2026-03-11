-- =============================================
-- Settings & Config Feature - Database Migration
-- Description: Complete schema for system settings,
--              commission configuration, and email templates
-- Version: 1.0
-- Date: 2026-01-27
-- =============================================

USE [CateringDB]
GO

-- =============================================
-- SECTION 1: System Settings Table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_settings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_settings] (
        [c_setting_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_setting_key] NVARCHAR(100) NOT NULL UNIQUE,
        [c_setting_value] NVARCHAR(MAX) NOT NULL,
        [c_category] NVARCHAR(50) NOT NULL, -- 'SYSTEM', 'EMAIL', 'PAYMENT', 'BUSINESS', 'NOTIFICATION'
        [c_value_type] NVARCHAR(20) NOT NULL DEFAULT 'STRING', -- 'STRING', 'NUMBER', 'BOOLEAN', 'JSON', 'ENCRYPTED'
        [c_display_name] NVARCHAR(200) NOT NULL,
        [c_description] NVARCHAR(500),
        [c_is_sensitive] BIT DEFAULT 0,
        [c_is_readonly] BIT DEFAULT 0,
        [c_display_order] INT DEFAULT 0,
        [c_validation_regex] NVARCHAR(500),
        [c_default_value] NVARCHAR(MAX),
        [c_is_active] BIT DEFAULT 1,
        [c_createddate] DATETIME DEFAULT GETDATE(),
        [c_createdby] BIGINT,
        [c_modifieddate] DATETIME,
        [c_modifiedby] BIGINT
    );

    CREATE NONCLUSTERED INDEX [IX_SysSettings_Category] ON [dbo].[t_sys_settings]([c_category]);
    CREATE NONCLUSTERED INDEX [IX_SysSettings_Active] ON [dbo].[t_sys_settings]([c_is_active]);

    PRINT 'Table t_sys_settings created successfully';
END
GO

-- =============================================
-- SECTION 2: Settings History Table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_settings_history]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_settings_history] (
        [c_history_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_setting_id] BIGINT NOT NULL,
        [c_setting_key] NVARCHAR(100) NOT NULL,
        [c_old_value] NVARCHAR(MAX),
        [c_new_value] NVARCHAR(MAX),
        [c_changed_by] BIGINT NOT NULL,
        [c_changed_by_name] NVARCHAR(200),
        [c_change_date] DATETIME DEFAULT GETDATE(),
        [c_change_reason] NVARCHAR(500),
        [c_ip_address] NVARCHAR(50)
    );

    CREATE NONCLUSTERED INDEX [IX_SettingsHistory_SettingId] ON [dbo].[t_sys_settings_history]([c_setting_id]);
    CREATE NONCLUSTERED INDEX [IX_SettingsHistory_ChangeDate] ON [dbo].[t_sys_settings_history]([c_change_date] DESC);

    PRINT 'Table t_sys_settings_history created successfully';
END
GO

-- =============================================
-- SECTION 3: Commission Configuration Table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_commission_config]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_commission_config] (
        [c_config_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_config_name] NVARCHAR(200) NOT NULL,
        [c_config_type] NVARCHAR(50) NOT NULL, -- 'GLOBAL', 'CATERING_SPECIFIC', 'TIERED'
        [c_ownerid] BIGINT NULL,
        [c_commission_rate] DECIMAL(5, 2) NOT NULL,
        [c_fixed_fee] DECIMAL(10, 2) DEFAULT 0,
        [c_min_order_value] DECIMAL(10, 2),
        [c_max_order_value] DECIMAL(10, 2),
        [c_is_active] BIT DEFAULT 1,
        [c_effective_from] DATE NOT NULL,
        [c_effective_to] DATE NULL,
        [c_createddate] DATETIME DEFAULT GETDATE(),
        [c_createdby] BIGINT,
        [c_modifieddate] DATETIME,
        [c_modifiedby] BIGINT
    );

    CREATE NONCLUSTERED INDEX [IX_CommissionConfig_Type] ON [dbo].[t_sys_commission_config]([c_config_type]);
    CREATE NONCLUSTERED INDEX [IX_CommissionConfig_Active] ON [dbo].[t_sys_commission_config]([c_is_active]);
    CREATE NONCLUSTERED INDEX [IX_CommissionConfig_EffectiveDates] ON [dbo].[t_sys_commission_config]([c_effective_from], [c_effective_to]);
    CREATE NONCLUSTERED INDEX [IX_CommissionConfig_CateringOwner] ON [dbo].[t_sys_commission_config]([c_ownerid]) WHERE [c_ownerid] IS NOT NULL;

    PRINT 'Table t_sys_commission_config created successfully';
END
GO

-- =============================================
-- SECTION 4: Template Variables Table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_template_variables]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_template_variables] (
        [c_variable_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_template_code] NVARCHAR(100) NOT NULL,
        [c_variable_name] NVARCHAR(100) NOT NULL,
        [c_variable_key] NVARCHAR(100) NOT NULL, -- e.g., '{{ customer_name }}'
        [c_description] NVARCHAR(500),
        [c_example_value] NVARCHAR(200),
        CONSTRAINT [UQ_TemplateVariables] UNIQUE ([c_template_code], [c_variable_key])
    );

    CREATE NONCLUSTERED INDEX [IX_TemplateVariables_TemplateCode] ON [dbo].[t_sys_template_variables]([c_template_code]);

    PRINT 'Table t_sys_template_variables created successfully';
END
GO

-- =============================================
-- SECTION 5: Seed Data - SYSTEM Settings
-- =============================================

-- App Configuration
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.APP_NAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.APP_NAME', 'Catering Ecommerce Platform', 'SYSTEM', 'STRING', 'Application Name', 'Name of the application displayed to users', 1, 'Catering Ecommerce Platform');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAINTENANCE_MODE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.MAINTENANCE_MODE', 'false', 'SYSTEM', 'BOOLEAN', 'Maintenance Mode', 'Enable maintenance mode to prevent user access', 2, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAINTENANCE_MESSAGE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.MAINTENANCE_MESSAGE', 'System is under maintenance. Please check back soon.', 'SYSTEM', 'STRING', 'Maintenance Message', 'Message displayed during maintenance mode', 3, 'System is under maintenance. Please check back soon.');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SUPPORT_EMAIL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.SUPPORT_EMAIL', 'support@cateringecommerce.com', 'SYSTEM', 'STRING', 'Support Email', 'Email address for customer support', 4, 'support@cateringecommerce.com');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SUPPORT_PHONE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.SUPPORT_PHONE', '+1-800-CATERING', 'SYSTEM', 'STRING', 'Support Phone', 'Phone number for customer support', 5, '+1-800-CATERING');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SESSION_TIMEOUT_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.SESSION_TIMEOUT_MINUTES', '30', 'SYSTEM', 'NUMBER', 'Session Timeout (Minutes)', 'User session timeout duration in minutes', 6, '30', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.MAX_LOGIN_ATTEMPTS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.MAX_LOGIN_ATTEMPTS', '5', 'SYSTEM', 'NUMBER', 'Max Login Attempts', 'Maximum failed login attempts before account lockout', 7, '5', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.ACCOUNT_LOCKOUT_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.ACCOUNT_LOCKOUT_MINUTES', '15', 'SYSTEM', 'NUMBER', 'Account Lockout Duration (Minutes)', 'Duration for which account is locked after max failed attempts', 8, '15', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.PASSWORD_MIN_LENGTH')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.PASSWORD_MIN_LENGTH', '8', 'SYSTEM', 'NUMBER', 'Minimum Password Length', 'Minimum length required for user passwords', 9, '8', '^[6-9]|[1-9][0-9]$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.PASSWORD_REQUIRE_SPECIAL_CHAR')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.PASSWORD_REQUIRE_SPECIAL_CHAR', 'true', 'SYSTEM', 'BOOLEAN', 'Require Special Characters in Password', 'Whether passwords must contain special characters', 10, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.ENABLE_TWO_FACTOR_AUTH')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.ENABLE_TWO_FACTOR_AUTH', 'false', 'SYSTEM', 'BOOLEAN', 'Enable Two-Factor Authentication', 'Require 2FA for admin and owner accounts', 11, 'false');
END

PRINT 'SYSTEM settings seeded successfully';
GO

-- =============================================
-- SECTION 6: Seed Data - EMAIL Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_HOST')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.SMTP_HOST', 'smtp.gmail.com', 'EMAIL', 'STRING', 'SMTP Host', 'SMTP server hostname', 1, 'smtp.gmail.com');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_PORT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('EMAIL.SMTP_PORT', '587', 'EMAIL', 'NUMBER', 'SMTP Port', 'SMTP server port number', 2, '587', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_USERNAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('EMAIL.SMTP_USERNAME', 'noreply@cateringecommerce.com', 'EMAIL', 'STRING', 'SMTP Username', 'SMTP authentication username', 3, 'noreply@cateringecommerce.com', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_PASSWORD')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('EMAIL.SMTP_PASSWORD', 'change_this_password', 'EMAIL', 'ENCRYPTED', 'SMTP Password', 'SMTP authentication password', 4, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.ENABLE_SSL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.ENABLE_SSL', 'true', 'EMAIL', 'BOOLEAN', 'Enable SSL', 'Use SSL/TLS for SMTP connection', 5, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.FROM_ADDRESS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.FROM_ADDRESS', 'noreply@cateringecommerce.com', 'EMAIL', 'STRING', 'From Email Address', 'Email address used in From field', 6, 'noreply@cateringecommerce.com');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.FROM_NAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.FROM_NAME', 'Catering Ecommerce', 'EMAIL', 'STRING', 'From Name', 'Display name used in From field', 7, 'Catering Ecommerce');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.REPLY_TO_ADDRESS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.REPLY_TO_ADDRESS', 'support@cateringecommerce.com', 'EMAIL', 'STRING', 'Reply-To Email Address', 'Email address for replies', 8, 'support@cateringecommerce.com');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.BCC_ADMIN')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('EMAIL.BCC_ADMIN', '', 'EMAIL', 'STRING', 'BCC Admin Email', 'BCC all emails to admin (leave blank to disable)', 9, '');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.MAX_RETRY_ATTEMPTS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('EMAIL.MAX_RETRY_ATTEMPTS', '3', 'EMAIL', 'NUMBER', 'Max Retry Attempts', 'Maximum retry attempts for failed emails', 10, '3', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.RETRY_DELAY_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('EMAIL.RETRY_DELAY_MINUTES', '5', 'EMAIL', 'NUMBER', 'Retry Delay (Minutes)', 'Delay between retry attempts in minutes', 11, '5', '^[0-9]+$');
END

PRINT 'EMAIL settings seeded successfully';
GO

-- =============================================
-- SECTION 7: Seed Data - PAYMENT Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_ID')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('PAYMENT.RAZORPAY_KEY_ID', 'rzp_test_xxxxxxxxxxxxxxxx', 'PAYMENT', 'STRING', 'Razorpay Key ID', 'Razorpay API Key ID', 1, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_SECRET')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('PAYMENT.RAZORPAY_KEY_SECRET', 'xxxxxxxxxxxxxxxxxxxxxxxx', 'PAYMENT', 'ENCRYPTED', 'Razorpay Key Secret', 'Razorpay API Key Secret', 2, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('PAYMENT.RAZORPAY_WEBHOOK_SECRET', 'whsec_xxxxxxxxxxxxxxxx', 'PAYMENT', 'ENCRYPTED', 'Razorpay Webhook Secret', 'Razorpay Webhook Secret for verifying webhook signatures', 3, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.PAYMENT_TIMEOUT_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('PAYMENT.PAYMENT_TIMEOUT_MINUTES', '15', 'PAYMENT', 'NUMBER', 'Payment Timeout (Minutes)', 'Time limit for completing payment', 4, '15', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_WALLET')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.ENABLE_WALLET', 'true', 'PAYMENT', 'BOOLEAN', 'Enable Wallet Payments', 'Allow customers to pay using wallet', 5, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_UPI')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.ENABLE_UPI', 'true', 'PAYMENT', 'BOOLEAN', 'Enable UPI Payments', 'Allow customers to pay using UPI', 6, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.ENABLE_NETBANKING')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.ENABLE_NETBANKING', 'true', 'PAYMENT', 'BOOLEAN', 'Enable Net Banking', 'Allow customers to pay using net banking', 7, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.REFUND_PROCESSING_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('PAYMENT.REFUND_PROCESSING_DAYS', '7', 'PAYMENT', 'NUMBER', 'Refund Processing Days', 'Number of business days to process refunds', 8, '7', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.AUTO_REFUND_ON_CANCEL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.AUTO_REFUND_ON_CANCEL', 'true', 'PAYMENT', 'BOOLEAN', 'Auto Refund on Cancel', 'Automatically initiate refund on order cancellation', 9, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.CURRENCY')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_readonly)
    VALUES ('PAYMENT.CURRENCY', 'INR', 'PAYMENT', 'STRING', 'Currency Code', 'Currency code for all transactions', 10, 'INR', 1);
END

PRINT 'PAYMENT settings seeded successfully';
GO

-- =============================================
-- SECTION 8: Seed Data - BUSINESS Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.DEFAULT_COMMISSION_RATE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.DEFAULT_COMMISSION_RATE', '15', 'BUSINESS', 'NUMBER', 'Default Commission Rate (%)', 'Default commission rate percentage for catering partners', 1, '15', '^[0-9]+(\\.[0-9]+)?$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ORDER_VALUE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_ORDER_VALUE', '500', 'BUSINESS', 'NUMBER', 'Minimum Order Value', 'Minimum order value in rupees', 2, '500', '^[0-9]+(\\.[0-9]+)?$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_ADVANCE_BOOKING_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MAX_ADVANCE_BOOKING_DAYS', '90', 'BUSINESS', 'NUMBER', 'Max Advance Booking Days', 'Maximum days in advance customers can book', 3, '90', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_ADVANCE_BOOKING_HOURS', '24', 'BUSINESS', 'NUMBER', 'Min Advance Booking Hours', 'Minimum hours in advance required for booking', 4, '24', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.CANCELLATION_WINDOW_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.CANCELLATION_WINDOW_HOURS', '48', 'BUSINESS', 'NUMBER', 'Cancellation Window (Hours)', 'Hours before event when free cancellation is allowed', 5, '48', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.CANCELLATION_FEE_PERCENTAGE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.CANCELLATION_FEE_PERCENTAGE', '20', 'BUSINESS', 'NUMBER', 'Cancellation Fee (%)', 'Cancellation fee percentage after free cancellation window', 6, '20', '^[0-9]+(\\.[0-9]+)?$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_GUESTS_PER_ORDER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MAX_GUESTS_PER_ORDER', '1000', 'BUSINESS', 'NUMBER', 'Max Guests Per Order', 'Maximum number of guests allowed per order', 7, '1000', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_REVIEWS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('BUSINESS.ENABLE_REVIEWS', 'true', 'BUSINESS', 'BOOLEAN', 'Enable Reviews', 'Allow customers to leave reviews', 8, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.AUTO_APPROVE_REVIEWS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('BUSINESS.AUTO_APPROVE_REVIEWS', 'false', 'BUSINESS', 'BOOLEAN', 'Auto Approve Reviews', 'Automatically approve customer reviews without moderation', 9, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_DISCOUNTS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('BUSINESS.ENABLE_DISCOUNTS', 'true', 'BUSINESS', 'BOOLEAN', 'Enable Discounts', 'Allow catering partners to create discount offers', 10, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.PARTNER_PAYOUT_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.PARTNER_PAYOUT_DAYS', '7', 'BUSINESS', 'NUMBER', 'Partner Payout Days', 'Days after order completion to process partner payout', 11, '7', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.ENABLE_PARTNER_APPROVAL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('BUSINESS.ENABLE_PARTNER_APPROVAL', 'true', 'BUSINESS', 'BOOLEAN', 'Enable Partner Approval', 'Require admin approval for new partner registrations', 12, 'true');
END

PRINT 'BUSINESS settings seeded successfully';
GO

-- =============================================
-- SECTION 9: Seed Data - NOTIFICATION Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_EMAIL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ENABLE_EMAIL', 'true', 'NOTIFICATION', 'BOOLEAN', 'Enable Email Notifications', 'Send notifications via email', 1, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_SMS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ENABLE_SMS', 'false', 'NOTIFICATION', 'BOOLEAN', 'Enable SMS Notifications', 'Send notifications via SMS', 2, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_IN_APP')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ENABLE_IN_APP', 'true', 'NOTIFICATION', 'BOOLEAN', 'Enable In-App Notifications', 'Send notifications within the application', 3, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ENABLE_PUSH')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ENABLE_PUSH', 'false', 'NOTIFICATION', 'BOOLEAN', 'Enable Push Notifications', 'Send push notifications to mobile devices', 4, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ORDER_CONFIRMATION')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ORDER_CONFIRMATION', 'true', 'NOTIFICATION', 'BOOLEAN', 'Order Confirmation Notifications', 'Send notifications on order confirmation', 5, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.ORDER_STATUS_UPDATES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.ORDER_STATUS_UPDATES', 'true', 'NOTIFICATION', 'BOOLEAN', 'Order Status Update Notifications', 'Send notifications on order status changes', 6, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.PAYMENT_CONFIRMATIONS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.PAYMENT_CONFIRMATIONS', 'true', 'NOTIFICATION', 'BOOLEAN', 'Payment Confirmation Notifications', 'Send notifications on payment confirmation', 7, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.CANCELLATION_CONFIRMATIONS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.CANCELLATION_CONFIRMATIONS', 'true', 'NOTIFICATION', 'BOOLEAN', 'Cancellation Confirmation Notifications', 'Send notifications on order cancellation', 8, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.PARTNER_APPROVAL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.PARTNER_APPROVAL', 'true', 'NOTIFICATION', 'BOOLEAN', 'Partner Approval Notifications', 'Send notifications on partner approval/rejection', 9, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.REVIEW_REMINDERS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.REVIEW_REMINDERS', 'true', 'NOTIFICATION', 'BOOLEAN', 'Review Reminder Notifications', 'Send reminders to customers to leave reviews', 10, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.EVENT_REMINDERS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('NOTIFICATION.EVENT_REMINDERS', 'true', 'NOTIFICATION', 'BOOLEAN', 'Event Reminder Notifications', 'Send reminders before event date', 11, 'true');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'NOTIFICATION.EVENT_REMINDER_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('NOTIFICATION.EVENT_REMINDER_HOURS', '24', 'NOTIFICATION', 'NUMBER', 'Event Reminder Hours', 'Hours before event to send reminder notification', 12, '24', '^[0-9]+$');
END

PRINT 'NOTIFICATION settings seeded successfully';
GO

-- =============================================
-- SECTION 10: Seed Data - Default Commission Configs
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_commission_config WHERE c_config_type = 'GLOBAL')
BEGIN
    INSERT INTO t_sys_commission_config (c_config_name, c_config_type, c_commission_rate, c_fixed_fee, c_is_active, c_effective_from)
    VALUES ('Default Global Commission', 'GLOBAL', 15.00, 0.00, 1, '2026-01-01');

    PRINT 'Default global commission config created';
END
GO

-- =============================================
-- SECTION 11: Seed Data - Template Variables (Common)
-- =============================================

-- Common variables used across all templates
DECLARE @CommonTemplates TABLE (TemplateCode NVARCHAR(100));
INSERT INTO @CommonTemplates VALUES
    ('USER_REGISTRATION'), ('USER_EMAIL_VERIFICATION'), ('USER_PASSWORD_RESET'),
    ('ORDER_CONFIRMATION'), ('ORDER_STATUS_UPDATE'), ('PAYMENT_SUCCESS'),
    ('PAYMENT_FAILED'), ('ORDER_CANCELLED'), ('REFUND_INITIATED'),
    ('OWNER_REGISTRATION'), ('PARTNER_APPROVED'), ('PARTNER_REJECTED'),
    ('ADMIN_USER_CREATED');

-- Insert common variables for each template
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    tc.TemplateCode,
    'Customer Name',
    '{{ customer_name }}',
    'Full name of the customer or user',
    'John Doe'
FROM @CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc.TemplateCode AND c_variable_key = '{{ customer_name }}'
);

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    tc.TemplateCode,
    'App Name',
    '{{ app_name }}',
    'Application name',
    'Catering Ecommerce Platform'
FROM @CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc.TemplateCode AND c_variable_key = '{{ app_name }}'
);

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    tc.TemplateCode,
    'Support Email',
    '{{ support_email }}',
    'Customer support email address',
    'support@cateringecommerce.com'
FROM @CommonTemplates tc
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = tc.TemplateCode AND c_variable_key = '{{ support_email }}'
);

-- Order-specific variables
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
VALUES
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
    ('REFUND_INITIATED', 'Refund Reason', '{{ refund_reason }}', 'Reason for refund', 'Order cancelled by customer');

-- Authentication variables
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
VALUES
    ('USER_EMAIL_VERIFICATION', 'Verification Link', '{{ verification_link }}', 'Email verification link', 'https://example.com/verify?token=xxxxx'),
    ('USER_PASSWORD_RESET', 'Reset Link', '{{ reset_link }}', 'Password reset link', 'https://example.com/reset?token=xxxxx'),
    ('USER_PASSWORD_RESET', 'Expiry Time', '{{ expiry_time }}', 'Link expiry time', '1 hour');

-- Partner-specific variables
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
VALUES
    ('PARTNER_APPROVED', 'Business Name', '{{ business_name }}', 'Catering business name', 'Delicious Caterers'),
    ('PARTNER_APPROVED', 'Owner Name', '{{ owner_name }}', 'Owner full name', 'Jane Smith'),
    ('PARTNER_APPROVED', 'Login Link', '{{ login_link }}', 'Partner login page link', 'https://example.com/owner/login'),
    ('PARTNER_REJECTED', 'Business Name', '{{ business_name }}', 'Catering business name', 'Delicious Caterers'),
    ('PARTNER_REJECTED', 'Rejection Reason', '{{ rejection_reason }}', 'Reason for rejection', 'Incomplete documentation');

-- Supervisor-specific variables
DECLARE @SupervisorTemplates TABLE (TemplateCode NVARCHAR(100));
INSERT INTO @SupervisorTemplates VALUES
    ('SUPERVISOR_REQUEST_APPROVED'), ('SUPERVISOR_REQUEST_REJECTED'),
    ('SUPERVISOR_REQUEST_UNDER_REVIEW'), ('SUPERVISOR_INFO_REQUESTED'),
    ('SUPERVISOR_ASSIGNED_EVENT'), ('SUPERVISOR_EVENT_LIVE_STATUS'),
    ('SUPERVISOR_EVENT_COMPLETED');

-- Common supervisor variables across all supervisor templates
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    st.TemplateCode,
    'Supervisor Name',
    '{{ supervisor_name }}',
    'Full name of the supervisor',
    'Rahul Sharma'
FROM @SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st.TemplateCode AND c_variable_key = '{{ supervisor_name }}'
);

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    st.TemplateCode,
    'Supervisor Email',
    '{{ supervisor_email }}',
    'Email address of the supervisor',
    'rahul.sharma@example.com'
FROM @SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st.TemplateCode AND c_variable_key = '{{ supervisor_email }}'
);

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    st.TemplateCode,
    'Supervisor Phone',
    '{{ supervisor_phone }}',
    'Phone number of the supervisor',
    '+91-9876543210'
FROM @SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st.TemplateCode AND c_variable_key = '{{ supervisor_phone }}'
);

INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
SELECT
    st.TemplateCode,
    'Supervisor Status',
    '{{ supervisor_status }}',
    'Current status of the supervisor application or assignment',
    'Approved'
FROM @SupervisorTemplates st
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_template_variables
    WHERE c_template_code = st.TemplateCode AND c_variable_key = '{{ supervisor_status }}'
);

-- Event-specific supervisor variables (for assignment/event templates)
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
VALUES
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Name', '{{ event_name }}', 'Name of the catering event', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Date', '{{ event_date }}', 'Date of the event', '15-Mar-2026'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Event Location', '{{ event_location }}', 'Location of the event', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Client Name', '{{ client_name }}', 'Name of the client who booked the event', 'Amit Sharma'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Monitoring Start Time', '{{ monitoring_start_time }}', 'Start time for supervisor monitoring', '10:00 AM'),
    ('SUPERVISOR_ASSIGNED_EVENT', 'Monitoring End Time', '{{ monitoring_end_time }}', 'End time for supervisor monitoring', '06:00 PM'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Name', '{{ event_name }}', 'Name of the catering event', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Date', '{{ event_date }}', 'Date of the event', '15-Mar-2026'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Event Location', '{{ event_location }}', 'Location of the event', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_EVENT_LIVE_STATUS', 'Client Name', '{{ client_name }}', 'Name of the client who booked the event', 'Amit Sharma'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Event Name', '{{ event_name }}', 'Name of the catering event', 'Wedding Reception - Sharma Family'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Event Date', '{{ event_date }}', 'Date of the event', '15-Mar-2026'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Event Location', '{{ event_location }}', 'Location of the event', 'Grand Banquet Hall, Mumbai'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Client Name', '{{ client_name }}', 'Name of the client who booked the event', 'Amit Sharma'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Monitoring Start Time', '{{ monitoring_start_time }}', 'Start time for supervisor monitoring', '10:00 AM'),
    ('SUPERVISOR_EVENT_COMPLETED', 'Monitoring End Time', '{{ monitoring_end_time }}', 'End time for supervisor monitoring', '06:00 PM');

-- Status reason variable for rejection/info request templates
INSERT INTO t_sys_template_variables (c_template_code, c_variable_name, c_variable_key, c_description, c_example_value)
VALUES
    ('SUPERVISOR_REQUEST_REJECTED', 'Status Reason', '{{ status_reason }}', 'Reason for rejection', 'Incomplete documentation submitted'),
    ('SUPERVISOR_INFO_REQUESTED', 'Status Reason', '{{ status_reason }}', 'Information requested from supervisor', 'Please upload a valid ID proof and address verification document');

PRINT 'Template variables seeded successfully (including Supervisor variables)';
GO

-- =============================================
-- SECTION 12: Seed Data - JWT Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.KEY')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive, c_is_readonly)
    VALUES ('JWT.KEY', 'u83$9Sj@q!5#LmQzTNfT^PwBzEoRk1At', 'JWT', 'STRING', 'JWT Signing Key', 'Secret key used to sign JWT tokens', 1, '', 1, 0);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.ISSUER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('JWT.ISSUER', 'https://localhost:44368', 'JWT', 'STRING', 'JWT Issuer', 'Issuer URL for JWT tokens', 2, 'https://localhost:44368');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.AUDIENCE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('JWT.AUDIENCE', 'https://localhost:5173/', 'JWT', 'STRING', 'JWT Audience', 'Audience URL for JWT tokens', 3, 'https://localhost:5173/');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.EXPIRE_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('JWT.EXPIRE_MINUTES', '60', 'JWT', 'NUMBER', 'JWT Expiry (Minutes)', 'JWT token expiration time in minutes', 4, '60', '^[1-9][0-9]*$');
END

PRINT 'JWT settings seeded successfully';
GO

-- =============================================
-- SECTION 13: Seed Data - TWILIO Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'TWILIO.ACCOUNT_SID')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('TWILIO.ACCOUNT_SID', 'ACa0e64157a3eeacc75d11d3ca0a45dc58', 'TWILIO', 'STRING', 'Twilio Account SID', 'Twilio account identifier', 1, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'TWILIO.AUTH_TOKEN')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('TWILIO.AUTH_TOKEN', '32d72da8576aab7007b38891731caf34', 'TWILIO', 'STRING', 'Twilio Auth Token', 'Twilio authentication token', 2, '', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'TWILIO.FROM_PHONE_NUMBER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('TWILIO.FROM_PHONE_NUMBER', '+918160182327', 'TWILIO', 'STRING', 'Twilio From Phone Number', 'Phone number used for sending SMS', 3, '');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'TWILIO.VERIFY_SERVICE_SID')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('TWILIO.VERIFY_SERVICE_SID', 'VA36c73036f1c116da2e88220cdcf48834', 'TWILIO', 'STRING', 'Twilio Verify Service SID', 'Twilio Verify service identifier for OTP', 4, '', 1);
END

PRINT 'TWILIO settings seeded successfully';
GO

-- =============================================
-- SECTION 14: Seed Data - Additional SYSTEM Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.ENCRYPTION_KEY')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive, c_is_readonly)
    VALUES ('SYSTEM.ENCRYPTION_KEY', 'p3d$Rsj@q!5#ShArP@#$^PwJ@!M@taJ!', 'SYSTEM', 'STRING', 'Encryption Key', 'Master encryption key for AES operations', 20, '', 1, 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.API_BASE_URL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SYSTEM.API_BASE_URL', 'https://localhost:44368', 'SYSTEM', 'STRING', 'API Base URL', 'Base URL for the API server', 21, 'https://localhost:44368');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.OTP_EXPIRY_SECONDS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.OTP_EXPIRY_SECONDS', '300', 'SYSTEM', 'NUMBER', 'OTP Expiry (Seconds)', 'OTP code expiration time in seconds', 22, '300', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.COOKIE_EXPIRY_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.COOKIE_EXPIRY_DAYS', '7', 'SYSTEM', 'NUMBER', 'Cookie Expiry (Days)', 'Authentication cookie expiration in days', 23, '7', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.GEO_CACHE_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SYSTEM.GEO_CACHE_HOURS', '6', 'SYSTEM', 'NUMBER', 'Geo Cache Duration (Hours)', 'Duration to cache geolocation data in hours', 24, '6', '^[1-9][0-9]*$');
END

PRINT 'Additional SYSTEM settings seeded successfully';
GO

-- =============================================
-- SECTION 15: Seed Data - SECURITY Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.ADMIN_LOGIN_PERMITS', '3', 'SECURITY', 'NUMBER', 'Admin Login Attempts', 'Maximum admin login attempts per window', 1, '3', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.ADMIN_LOGIN_WINDOW_MINUTES', '15', 'SECURITY', 'NUMBER', 'Admin Login Window (Minutes)', 'Time window for admin login rate limiting', 2, '15', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.USER_LOGIN_PERMITS', '5', 'SECURITY', 'NUMBER', 'User Login Attempts', 'Maximum user login attempts per window', 3, '5', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.USER_LOGIN_WINDOW_MINUTES', '10', 'SECURITY', 'NUMBER', 'User Login Window (Minutes)', 'Time window for user login rate limiting', 4, '10', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.OTP_SEND_PERMITS', '3', 'SECURITY', 'NUMBER', 'OTP Send Limit', 'Maximum OTP send attempts per window', 5, '3', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.OTP_SEND_WINDOW_MINUTES', '60', 'SECURITY', 'NUMBER', 'OTP Send Window (Minutes)', 'Time window for OTP send rate limiting', 6, '60', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.OTP_VERIFY_PERMITS', '5', 'SECURITY', 'NUMBER', 'OTP Verify Limit', 'Maximum OTP verification attempts per window', 7, '5', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.OTP_VERIFY_WINDOW_MINUTES', '5', 'SECURITY', 'NUMBER', 'OTP Verify Window (Minutes)', 'Time window for OTP verification rate limiting', 8, '5', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.API_GENERAL_PERMITS', '100', 'SECURITY', 'NUMBER', 'API Rate Limit', 'Maximum API requests per window', 9, '100', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.API_GENERAL_WINDOW_MINUTES', '1', 'SECURITY', 'NUMBER', 'API Rate Window (Minutes)', 'Time window for API general rate limiting', 10, '1', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.FILE_UPLOAD_PERMITS', '10', 'SECURITY', 'NUMBER', 'File Upload Limit', 'Maximum file uploads per window', 11, '10', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('SECURITY.FILE_UPLOAD_WINDOW_MINUTES', '10', 'SECURITY', 'NUMBER', 'File Upload Window (Minutes)', 'Time window for file upload rate limiting', 12, '10', '^[1-9][0-9]*$');
END

PRINT 'SECURITY settings seeded successfully';
GO

-- =============================================
-- SECTION 16: Seed Data - Additional BUSINESS Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_WITHDRAWAL_AMOUNT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_WITHDRAWAL_AMOUNT', '500', 'BUSINESS', 'NUMBER', 'Min Withdrawal Amount', 'Minimum amount for partner withdrawal requests', 13, '500', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_ADDRESSES_PER_USER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MAX_ADDRESSES_PER_USER', '5', 'BUSINESS', 'NUMBER', 'Max Addresses Per User', 'Maximum saved addresses allowed per user', 14, '5', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_GUESTS_PER_ORDER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_GUESTS_PER_ORDER', '50', 'BUSINESS', 'NUMBER', 'Min Guests Per Order', 'Minimum number of guests required per order', 15, '50', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_PAYMENT_PERCENT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_ADVANCE_PAYMENT_PERCENT', '30', 'BUSINESS', 'NUMBER', 'Min Advance Payment (%)', 'Minimum advance payment percentage required', 16, '30', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_ADVANCE_BOOKING_DAYS', '3', 'BUSINESS', 'NUMBER', 'Min Advance Booking Days', 'Minimum days in advance for catering orders', 17, '3', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MIN_ADVANCE_BOOKING_HOURS', '24', 'BUSINESS', 'NUMBER', 'Min Advance Booking Hours', 'Minimum hours in advance for event bookings', 18, '24', '^[1-9][0-9]*$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MAX_ADVANCE_BOOKING_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('BUSINESS.MAX_ADVANCE_BOOKING_DAYS', '90', 'BUSINESS', 'NUMBER', 'Max Advance Booking Days', 'Maximum days in advance for event bookings', 19, '90', '^[1-9][0-9]*$');
END

PRINT 'Additional BUSINESS settings seeded successfully';
GO

-- =============================================
-- SECTION 17: Seed Data - RABBITMQ Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'RABBITMQ.ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('RABBITMQ.ENABLED', 'false', 'RABBITMQ', 'BOOLEAN', 'RabbitMQ Enabled', 'Enable RabbitMQ message queue integration', 1, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'RABBITMQ.HOSTNAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('RABBITMQ.HOSTNAME', 'localhost', 'RABBITMQ', 'STRING', 'RabbitMQ Hostname', 'RabbitMQ server hostname', 2, 'localhost');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'RABBITMQ.PORT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('RABBITMQ.PORT', '5672', 'RABBITMQ', 'NUMBER', 'RabbitMQ Port', 'RabbitMQ server port number', 3, '5672', '^[0-9]+$');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'RABBITMQ.USERNAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('RABBITMQ.USERNAME', 'guest', 'RABBITMQ', 'STRING', 'RabbitMQ Username', 'RabbitMQ authentication username', 4, 'guest', 1);
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'RABBITMQ.PASSWORD')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('RABBITMQ.PASSWORD', 'guest', 'RABBITMQ', 'STRING', 'RabbitMQ Password', 'RabbitMQ authentication password', 5, 'guest', 1);
END

PRINT 'RABBITMQ settings seeded successfully';
GO

-- =============================================
-- SECTION 18: Seed Data - APP Settings (URLs, Contact, Branding)
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_PORTAL_URL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.PARTNER_PORTAL_URL', 'https://partner.enyvora.com/login', 'APP', 'STRING', 'Partner Portal URL', 'Login URL for the partner portal', 1, 'https://partner.enyvora.com/login');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_GUIDE_URL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.PARTNER_GUIDE_URL', 'https://enyvora.com/partner-guide', 'APP', 'STRING', 'Partner Guide URL', 'URL for the partner onboarding guide', 2, 'https://enyvora.com/partner-guide');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.BEST_PRACTICES_URL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.BEST_PRACTICES_URL', 'https://enyvora.com/best-practices', 'APP', 'STRING', 'Best Practices URL', 'URL for partner best practices page', 3, 'https://enyvora.com/best-practices');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.SUPPORT_URL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.SUPPORT_URL', 'https://enyvora.com/support', 'APP', 'STRING', 'Support URL', 'URL for the main support page', 4, 'https://enyvora.com/support');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_SUPPORT_EMAIL')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.PARTNER_SUPPORT_EMAIL', 'support@enyvora.com', 'APP', 'STRING', 'Partner Support Email', 'Email address for partner support inquiries', 5, 'support@enyvora.com');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.PARTNER_SUPPORT_PHONE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.PARTNER_SUPPORT_PHONE', '+91-1234567890', 'APP', 'STRING', 'Partner Support Phone', 'Phone number for partner support', 6, '+91-1234567890');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'APP.REAPPLY_DURATION')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('APP.REAPPLY_DURATION', '30 days', 'APP', 'STRING', 'Reapply Duration', 'Duration after rejection before partner can reapply', 7, '30 days');
END

PRINT 'APP settings seeded successfully';
GO

-- =============================================
-- SECTION 19: MSG91 Provider Settings
-- =============================================

PRINT 'Seeding MSG91 settings...';
GO

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.AUTH_KEY')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('MSG91.AUTH_KEY', '', 'MSG91', 'ENCRYPTED', 1, 'MSG91 Auth Key', 'MSG91 API authentication key (keep secret)', 1, '');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.SENDER_ID')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('MSG91.SENDER_ID', 'CATAPP', 'MSG91', 'STRING', 0, 'MSG91 Sender ID', '6-character alphanumeric sender ID registered with MSG91', 2, 'CATAPP');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.TEMPLATE_ID')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('MSG91.TEMPLATE_ID', '', 'MSG91', 'STRING', 0, 'MSG91 OTP Template ID', 'DLT-approved OTP template ID from MSG91 dashboard', 3, '');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'MSG91.ROUTE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('MSG91.ROUTE', '4', 'MSG91', 'STRING', 0, 'MSG91 Route', '4 = transactional (OTP), 1 = promotional', 4, '4');
END

PRINT 'MSG91 settings seeded successfully';
GO

-- =============================================
-- SECTION 20: OTP Service Settings
-- =============================================

PRINT 'Seeding OTP service settings...';
GO

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SMS.PROVIDER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('SMS.PROVIDER', 'MSG91', 'SMS', 'STRING', 0, 'Active SMS Provider', 'Active OTP SMS provider: MSG91 or TWILIO. Requires app restart to take effect.', 1, 'MSG91');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.LENGTH')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('OTP.LENGTH', '6', 'OTP', 'STRING', 0, 'OTP Length', 'Number of digits in generated OTP (default: 6)', 1, '6');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.EXPIRY_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('OTP.EXPIRY_MINUTES', '10', 'OTP', 'STRING', 0, 'OTP Expiry (minutes)', 'Time in minutes before OTP expires (default: 10)', 2, '10');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.MAX_VERIFY_ATTEMPTS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('OTP.MAX_VERIFY_ATTEMPTS', '5', 'OTP', 'STRING', 0, 'OTP Max Verify Attempts', 'Maximum wrong OTP attempts before entry is locked (default: 5)', 3, '5');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.BYPASS_VERIFICATION')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('OTP.BYPASS_VERIFICATION', 'false', 'OTP', 'STRING', 0, 'Bypass OTP Verification', 'Set to true only for development/testing to skip actual OTP check. MUST be false in production.', 4, 'false');
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.MESSAGE_TEMPLATE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('OTP.MESSAGE_TEMPLATE', 'Your {AppName} OTP is {OTP}. Valid for {EXPIRY} minutes. Do not share this code with anyone.', 'OTP', 'STRING', 0, 'OTP SMS Template', 'SMS template for OTP delivery. Placeholders: {AppName}, {OTP}, {EXPIRY}', 5, 'Your {AppName} OTP is {OTP}. Valid for {EXPIRY} minutes. Do not share this code with anyone.');
END

PRINT 'OTP service settings seeded successfully';
GO

-- =============================================
-- SECTION 21: Public Stats Settings
-- =============================================

PRINT 'Seeding public stats settings...';
GO

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'STATS.AVG_GROWTH_PERCENT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_is_sensitive, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('STATS.AVG_GROWTH_PERCENT', '150', 'STATS', 'INT', 0, 'Avg Partner Growth (%)', 'Average revenue growth percentage shown on the Partner Login page stat counter', 1, '150');
END

PRINT 'Public stats settings seeded successfully';
GO

-- =============================================
-- SECTION 22: Summary
-- =============================================

PRINT '================================================';
PRINT 'Settings & Config Migration Completed Successfully';
PRINT '================================================';
PRINT 'Tables Created:';
PRINT '  - t_sys_settings';
PRINT '  - t_sys_settings_history';
PRINT '  - t_sys_commission_config';
PRINT '  - t_sys_template_variables';
PRINT '';
PRINT 'Settings Seeded:';
PRINT '  - SYSTEM: 16 settings';
PRINT '  - EMAIL: 11 settings';
PRINT '  - PAYMENT: 10 settings';
PRINT '  - BUSINESS: 16 settings';
PRINT '  - NOTIFICATION: 12 settings';
PRINT '  - JWT: 4 settings';
PRINT '  - TWILIO: 4 settings';
PRINT '  - SECURITY: 12 settings';
PRINT '  - RABBITMQ: 5 settings';
PRINT '  - APP: 7 settings';
PRINT '  - MSG91: 4 settings';
PRINT '  - SMS: 1 setting';
PRINT '  - OTP: 5 settings';
PRINT '  - STATS: 1 setting';
PRINT '  Total: 111 settings';
PRINT '';
PRINT 'Commission Configs: 1 default global config';
PRINT 'Template Variables: Common + specific variables';
PRINT '================================================';
GO
