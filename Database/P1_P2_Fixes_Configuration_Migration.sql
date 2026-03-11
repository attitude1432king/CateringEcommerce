-- =============================================
-- P1 & P2 Fixes Configuration Migration
-- =============================================
-- This script adds required configuration settings for all P1 and P2 fixes
-- Version: 1.0
-- Date: 2026-02-20
-- Author: Production Tech Lead Analysis
--
-- IMPORTANT: Update placeholder values before running in production!
-- =============================================

USE [CateringDB]
GO

BEGIN TRANSACTION;

PRINT '========================================';
PRINT 'P1 & P2 Fixes Configuration Migration';
PRINT 'Started at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';

-- =============================================
-- 1. CORS CONFIGURATION (P2 Fix #1)
-- =============================================
PRINT '';
PRINT '1. Configuring CORS settings...';

-- Production CORS Origin
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CORS.PRODUCTION_ORIGIN')
BEGIN
    INSERT INTO t_sys_settings (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_is_sensitive,
        c_createddate,
        c_last_modified
    )
    VALUES (
        'CORS.PRODUCTION_ORIGIN',
        'https://yourdomain.com', -- ⚠️ UPDATE THIS WITH YOUR PRODUCTION DOMAIN
        'SECURITY',
        'STRING',
        'Production CORS Origin',
        'Production frontend domain for CORS policy. Update this with your actual production domain.',
        0,
        GETDATE(),
        GETDATE()
    );
    PRINT '   ✓ CORS.PRODUCTION_ORIGIN configured (UPDATE VALUE BEFORE PRODUCTION!)';
END
ELSE
BEGIN
    PRINT '   ℹ CORS.PRODUCTION_ORIGIN already exists';
END

-- =============================================
-- 2. RAZORPAY WEBHOOK SECRET (P2 Fix #3)
-- ⚠️ CRITICAL - REQUIRED FOR PAYMENT SECURITY
-- =============================================
PRINT '';
PRINT '2. Configuring Razorpay webhook secret...';

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET')
BEGIN
    INSERT INTO t_sys_settings (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_is_sensitive,
        c_createddate,
        c_last_modified
    )
    VALUES (
        'PAYMENT.RAZORPAY_WEBHOOK_SECRET',
        'YOUR_WEBHOOK_SECRET_HERE', -- ⚠️ CRITICAL: Get this from Razorpay Dashboard → Settings → Webhooks
        'PAYMENT',
        'STRING',
        'Razorpay Webhook Secret',
        'CRITICAL: Webhook signing secret from Razorpay. Required for webhook signature verification. Get from: Razorpay Dashboard > Settings > Webhooks > Secret',
        1, -- Sensitive data
        GETDATE(),
        GETDATE()
    );
    PRINT '   ✓ PAYMENT.RAZORPAY_WEBHOOK_SECRET configured (⚠️ UPDATE VALUE IMMEDIATELY!)';
    PRINT '   ⚠️ CRITICAL: Application will REJECT all webhooks until this is configured!';
END
ELSE
BEGIN
    PRINT '   ℹ PAYMENT.RAZORPAY_WEBHOOK_SECRET already exists';
END

-- =============================================
-- 3. OTP SETTINGS (Supporting P2 Fix #2)
-- =============================================
PRINT '';
PRINT '3. Configuring OTP settings...';

-- OTP Expiry Time
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.OTP_EXPIRY_SECONDS')
BEGIN
    INSERT INTO t_sys_settings (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_is_sensitive,
        c_createddate,
        c_last_modified
    )
    VALUES (
        'SYSTEM.OTP_EXPIRY_SECONDS',
        '300', -- 5 minutes
        'SYSTEM',
        'INT',
        'OTP Expiry Duration',
        'Duration in seconds for OTP validity. Default: 300 seconds (5 minutes)',
        0,
        GETDATE(),
        GETDATE()
    );
    PRINT '   ✓ SYSTEM.OTP_EXPIRY_SECONDS set to 300 seconds';
END
ELSE
BEGIN
    PRINT '   ℹ SYSTEM.OTP_EXPIRY_SECONDS already exists';
END

-- OTP Expiry (Minutes - for compatibility)
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'OTP.EXPIRY_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_is_sensitive,
        c_createddate,
        c_last_modified
    )
    VALUES (
        'OTP.EXPIRY_MINUTES',
        '10',
        'SYSTEM',
        'INT',
        'OTP Expiry (Minutes)',
        'OTP validity duration in minutes for SMS service',
        0,
        GETDATE(),
        GETDATE()
    );
    PRINT '   ✓ OTP.EXPIRY_MINUTES set to 10 minutes';
END
ELSE
BEGIN
    PRINT '   ℹ OTP.EXPIRY_MINUTES already exists';
END

-- =============================================
-- 4. SECURITY RATE LIMITING SETTINGS
-- =============================================
PRINT '';
PRINT '4. Configuring security rate limiting...';

-- Admin Login Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.ADMIN_LOGIN_PERMITS', '3', 'SECURITY', 'INT', 'Admin Login Permits', 'Max admin login attempts per window', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.ADMIN_LOGIN_PERMITS set to 3';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.ADMIN_LOGIN_WINDOW_MINUTES', '15', 'SECURITY', 'INT', 'Admin Login Window', 'Time window in minutes for rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.ADMIN_LOGIN_WINDOW_MINUTES set to 15';
END

-- User Login Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.USER_LOGIN_PERMITS', '5', 'SECURITY', 'INT', 'User Login Permits', 'Max user login attempts per window', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.USER_LOGIN_PERMITS set to 5';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.USER_LOGIN_WINDOW_MINUTES', '10', 'SECURITY', 'INT', 'User Login Window', 'Time window in minutes for user login rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.USER_LOGIN_WINDOW_MINUTES set to 10';
END

-- OTP Send Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.OTP_SEND_PERMITS', '3', 'SECURITY', 'INT', 'OTP Send Permits', 'Max OTP send attempts per window (prevents SMS spam)', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.OTP_SEND_PERMITS set to 3';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.OTP_SEND_WINDOW_MINUTES', '60', 'SECURITY', 'INT', 'OTP Send Window', 'Time window in minutes for OTP send rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.OTP_SEND_WINDOW_MINUTES set to 60';
END

-- OTP Verify Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.OTP_VERIFY_PERMITS', '5', 'SECURITY', 'INT', 'OTP Verify Permits', 'Max OTP verification attempts per window (prevents brute force)', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.OTP_VERIFY_PERMITS set to 5';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.OTP_VERIFY_WINDOW_MINUTES', '5', 'SECURITY', 'INT', 'OTP Verify Window', 'Time window in minutes for OTP verification rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.OTP_VERIFY_WINDOW_MINUTES set to 5';
END

-- General API Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.API_GENERAL_PERMITS', '100', 'SECURITY', 'INT', 'API General Permits', 'Max API requests per window (prevents DoS)', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.API_GENERAL_PERMITS set to 100';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.API_GENERAL_WINDOW_MINUTES', '1', 'SECURITY', 'INT', 'API General Window', 'Time window in minutes for general API rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.API_GENERAL_WINDOW_MINUTES set to 1';
END

-- File Upload Rate Limiting
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_PERMITS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.FILE_UPLOAD_PERMITS', '10', 'SECURITY', 'INT', 'File Upload Permits', 'Max file upload attempts per window (prevents abuse)', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.FILE_UPLOAD_PERMITS set to 10';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_WINDOW_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SECURITY.FILE_UPLOAD_WINDOW_MINUTES', '10', 'SECURITY', 'INT', 'File Upload Window', 'Time window in minutes for file upload rate limiting', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SECURITY.FILE_UPLOAD_WINDOW_MINUTES set to 10';
END

-- =============================================
-- 5. SESSION AND COOKIE SETTINGS
-- =============================================
PRINT '';
PRINT '5. Configuring session settings...';

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SESSION_TIMEOUT_MINUTES')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SYSTEM.SESSION_TIMEOUT_MINUTES', '20', 'SYSTEM', 'INT', 'Session Timeout', 'Session idle timeout in minutes', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SYSTEM.SESSION_TIMEOUT_MINUTES set to 20';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.COOKIE_EXPIRY_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_is_sensitive, c_createddate, c_last_modified)
    VALUES ('SYSTEM.COOKIE_EXPIRY_DAYS', '7', 'SYSTEM', 'INT', 'Cookie Expiry Days', 'Authentication cookie expiry in days', 0, GETDATE(), GETDATE());
    PRINT '   ✓ SYSTEM.COOKIE_EXPIRY_DAYS set to 7';
END

-- =============================================
-- 6. VERIFY EXISTING CRITICAL SETTINGS
-- =============================================
PRINT '';
PRINT '6. Verifying existing critical settings...';

-- Check JWT settings
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.KEY')
BEGIN
    PRINT '   ⚠️ WARNING: JWT.KEY not found in t_sys_settings!';
    PRINT '   Action: Add JWT.KEY to t_sys_settings or configure in appsettings.json';
END
ELSE
BEGIN
    PRINT '   ✓ JWT.KEY exists';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.ISSUER')
BEGIN
    PRINT '   ℹ JWT.ISSUER not in t_sys_settings (may be in appsettings.json)';
END
ELSE
BEGIN
    PRINT '   ✓ JWT.ISSUER exists';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'JWT.AUDIENCE')
BEGIN
    PRINT '   ℹ JWT.AUDIENCE not in t_sys_settings (may be in appsettings.json)';
END
ELSE
BEGIN
    PRINT '   ✓ JWT.AUDIENCE exists';
END

-- Check Razorpay primary credentials
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_ID')
BEGIN
    PRINT '   ⚠️ WARNING: PAYMENT.RAZORPAY_KEY_ID not found!';
    PRINT '   Action: Add Razorpay credentials to t_sys_settings';
END
ELSE
BEGIN
    PRINT '   ✓ PAYMENT.RAZORPAY_KEY_ID exists';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_SECRET')
BEGIN
    PRINT '   ⚠️ WARNING: PAYMENT.RAZORPAY_KEY_SECRET not found!';
END
ELSE
BEGIN
    PRINT '   ✓ PAYMENT.RAZORPAY_KEY_SECRET exists';
END

-- Check Email settings
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'EMAIL.SMTP_HOST')
BEGIN
    PRINT '   ℹ EMAIL.SMTP_HOST not in t_sys_settings (may be in appsettings.json)';
END
ELSE
BEGIN
    PRINT '   ✓ EMAIL.SMTP_HOST exists';
END

-- Check SMS settings
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'SMS.PROVIDER')
BEGIN
    PRINT '   ℹ SMS.PROVIDER not in t_sys_settings (may be in appsettings.json)';
END
ELSE
BEGIN
    PRINT '   ✓ SMS.PROVIDER exists';
END

-- =============================================
-- 7. COMMIT TRANSACTION
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Migration Summary:';
PRINT '========================================';

SELECT
    c_category AS Category,
    COUNT(*) AS SettingCount
FROM t_sys_settings
WHERE c_setting_key IN (
    'CORS.PRODUCTION_ORIGIN',
    'PAYMENT.RAZORPAY_WEBHOOK_SECRET',
    'SYSTEM.OTP_EXPIRY_SECONDS',
    'OTP.EXPIRY_MINUTES',
    'SECURITY.ADMIN_LOGIN_PERMITS',
    'SECURITY.ADMIN_LOGIN_WINDOW_MINUTES',
    'SECURITY.USER_LOGIN_PERMITS',
    'SECURITY.USER_LOGIN_WINDOW_MINUTES',
    'SECURITY.OTP_SEND_PERMITS',
    'SECURITY.OTP_SEND_WINDOW_MINUTES',
    'SECURITY.OTP_VERIFY_PERMITS',
    'SECURITY.OTP_VERIFY_WINDOW_MINUTES',
    'SECURITY.API_GENERAL_PERMITS',
    'SECURITY.API_GENERAL_WINDOW_MINUTES',
    'SECURITY.FILE_UPLOAD_PERMITS',
    'SECURITY.FILE_UPLOAD_WINDOW_MINUTES',
    'SYSTEM.SESSION_TIMEOUT_MINUTES',
    'SYSTEM.COOKIE_EXPIRY_DAYS'
)
GROUP BY c_category
ORDER BY c_category;

PRINT '';
PRINT '⚠️ CRITICAL ACTIONS REQUIRED:';
PRINT '1. Update CORS.PRODUCTION_ORIGIN with your production domain';
PRINT '2. Update PAYMENT.RAZORPAY_WEBHOOK_SECRET from Razorpay Dashboard';
PRINT '   (Dashboard > Settings > Webhooks > Secret)';
PRINT '';
PRINT 'Migration completed at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);

COMMIT TRANSACTION;

PRINT '✓ Transaction committed successfully!';
PRINT '========================================';
GO
