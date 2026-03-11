-- =============================================
-- P1 & P2 Fixes Configuration Verification Script
-- =============================================
-- This script verifies that all required settings are configured correctly
-- Run this AFTER executing P1_P2_Fixes_Configuration_Migration.sql
-- =============================================

USE [CateringDB]
GO

SET NOCOUNT ON;

PRINT '========================================';
PRINT 'P1 & P2 Fixes Configuration Verification';
PRINT 'Verification started at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- Create temp table for verification results
-- =============================================
CREATE TABLE #VerificationResults (
    CheckNumber INT,
    CheckName VARCHAR(100),
    Status VARCHAR(20),
    Details VARCHAR(500),
    Severity VARCHAR(20)
);

-- =============================================
-- 1. CRITICAL SETTINGS CHECK
-- =============================================
PRINT '1. Checking CRITICAL settings...';
PRINT '';

-- Check Razorpay Webhook Secret
DECLARE @WebhookSecret VARCHAR(500);
SELECT @WebhookSecret = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET';

IF @WebhookSecret IS NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (1, 'Razorpay Webhook Secret', 'MISSING', 'Setting not found in database', 'CRITICAL');
    PRINT '   ❌ CRITICAL: PAYMENT.RAZORPAY_WEBHOOK_SECRET is MISSING';
    PRINT '      Application will REJECT all payment webhooks!';
END
ELSE IF @WebhookSecret = 'YOUR_WEBHOOK_SECRET_HERE'
BEGIN
    INSERT INTO #VerificationResults VALUES (1, 'Razorpay Webhook Secret', 'NOT CONFIGURED', 'Default placeholder value detected', 'CRITICAL');
    PRINT '   ⚠️ CRITICAL: PAYMENT.RAZORPAY_WEBHOOK_SECRET has PLACEHOLDER value';
    PRINT '      Update with actual secret from Razorpay Dashboard > Settings > Webhooks';
END
ELSE IF LEN(@WebhookSecret) < 10
BEGIN
    INSERT INTO #VerificationResults VALUES (1, 'Razorpay Webhook Secret', 'INVALID', 'Secret seems too short (length < 10)', 'ERROR');
    PRINT '   ⚠️ ERROR: PAYMENT.RAZORPAY_WEBHOOK_SECRET seems invalid (too short)';
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (1, 'Razorpay Webhook Secret', 'OK', 'Configured with ' + CAST(LEN(@WebhookSecret) AS VARCHAR) + ' characters', 'INFO');
    PRINT '   ✓ PAYMENT.RAZORPAY_WEBHOOK_SECRET is configured';
END

-- Check CORS Production Origin
DECLARE @CorsOrigin VARCHAR(500);
SELECT @CorsOrigin = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'CORS.PRODUCTION_ORIGIN';

IF @CorsOrigin IS NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (2, 'CORS Production Origin', 'MISSING', 'Setting not found in database', 'WARNING');
    PRINT '   ⚠️ WARNING: CORS.PRODUCTION_ORIGIN is MISSING';
END
ELSE IF @CorsOrigin = 'https://yourdomain.com'
BEGIN
    INSERT INTO #VerificationResults VALUES (2, 'CORS Production Origin', 'NOT CONFIGURED', 'Default placeholder value detected', 'WARNING');
    PRINT '   ⚠️ WARNING: CORS.PRODUCTION_ORIGIN has PLACEHOLDER value';
    PRINT '      Update with actual production domain before deploying';
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (2, 'CORS Production Origin', 'OK', @CorsOrigin, 'INFO');
    PRINT '   ✓ CORS.PRODUCTION_ORIGIN: ' + @CorsOrigin;
END

-- =============================================
-- 2. SECURITY SETTINGS CHECK
-- =============================================
PRINT '';
PRINT '2. Checking security rate limiting settings...';
PRINT '';

DECLARE @SettingKey VARCHAR(100), @SettingValue VARCHAR(500), @CheckCount INT = 3;

-- Security settings to verify
DECLARE SecuritySettingsCursor CURSOR FOR
SELECT c_setting_key, c_setting_value
FROM t_sys_settings
WHERE c_setting_key IN (
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
    'SECURITY.FILE_UPLOAD_WINDOW_MINUTES'
)
ORDER BY c_setting_key;

OPEN SecuritySettingsCursor;
FETCH NEXT FROM SecuritySettingsCursor INTO @SettingKey, @SettingValue;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @SettingValue IS NOT NULL
    BEGIN
        INSERT INTO #VerificationResults VALUES (@CheckCount, @SettingKey, 'OK', @SettingValue, 'INFO');
        PRINT '   ✓ ' + @SettingKey + ' = ' + @SettingValue;
    END
    ELSE
    BEGIN
        INSERT INTO #VerificationResults VALUES (@CheckCount, @SettingKey, 'MISSING', 'Setting not configured', 'WARNING');
        PRINT '   ⚠️ ' + @SettingKey + ' is MISSING';
    END

    SET @CheckCount = @CheckCount + 1;
    FETCH NEXT FROM SecuritySettingsCursor INTO @SettingKey, @SettingValue;
END

CLOSE SecuritySettingsCursor;
DEALLOCATE SecuritySettingsCursor;

-- =============================================
-- 3. OTP SETTINGS CHECK
-- =============================================
PRINT '';
PRINT '3. Checking OTP settings...';
PRINT '';

DECLARE @OtpExpirySeconds VARCHAR(50), @OtpExpiryMinutes VARCHAR(50);
SELECT @OtpExpirySeconds = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.OTP_EXPIRY_SECONDS';
SELECT @OtpExpiryMinutes = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'OTP.EXPIRY_MINUTES';

IF @OtpExpirySeconds IS NOT NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'OTP Expiry (Seconds)', 'OK', @OtpExpirySeconds + ' seconds', 'INFO');
    PRINT '   ✓ SYSTEM.OTP_EXPIRY_SECONDS = ' + @OtpExpirySeconds + ' seconds';
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'OTP Expiry (Seconds)', 'MISSING', 'Using default: 300 seconds', 'WARNING');
    PRINT '   ⚠️ SYSTEM.OTP_EXPIRY_SECONDS is MISSING (using default: 300)';
    SET @CheckCount = @CheckCount + 1;
END

IF @OtpExpiryMinutes IS NOT NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'OTP Expiry (Minutes)', 'OK', @OtpExpiryMinutes + ' minutes', 'INFO');
    PRINT '   ✓ OTP.EXPIRY_MINUTES = ' + @OtpExpiryMinutes + ' minutes';
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'OTP Expiry (Minutes)', 'MISSING', 'Using default: 10 minutes', 'WARNING');
    PRINT '   ⚠️ OTP.EXPIRY_MINUTES is MISSING (using default: 10)';
    SET @CheckCount = @CheckCount + 1;
END

-- =============================================
-- 4. PAYMENT GATEWAY SETTINGS CHECK
-- =============================================
PRINT '';
PRINT '4. Checking payment gateway settings...';
PRINT '';

DECLARE @RazorpayKeyId VARCHAR(500), @RazorpayKeySecret VARCHAR(500);
SELECT @RazorpayKeyId = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_ID';
SELECT @RazorpayKeySecret = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_KEY_SECRET';

IF @RazorpayKeyId IS NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Razorpay Key ID', 'MISSING', 'Payment gateway will not work', 'CRITICAL');
    PRINT '   ❌ CRITICAL: PAYMENT.RAZORPAY_KEY_ID is MISSING';
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Razorpay Key ID', 'OK', LEFT(@RazorpayKeyId, 10) + '...', 'INFO');
    PRINT '   ✓ PAYMENT.RAZORPAY_KEY_ID is configured';
    SET @CheckCount = @CheckCount + 1;
END

IF @RazorpayKeySecret IS NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Razorpay Key Secret', 'MISSING', 'Payment gateway will not work', 'CRITICAL');
    PRINT '   ❌ CRITICAL: PAYMENT.RAZORPAY_KEY_SECRET is MISSING';
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Razorpay Key Secret', 'OK', 'Configured (hidden for security)', 'INFO');
    PRINT '   ✓ PAYMENT.RAZORPAY_KEY_SECRET is configured';
    SET @CheckCount = @CheckCount + 1;
END

-- =============================================
-- 5. SESSION SETTINGS CHECK
-- =============================================
PRINT '';
PRINT '5. Checking session settings...';
PRINT '';

DECLARE @SessionTimeout VARCHAR(50), @CookieExpiry VARCHAR(50);
SELECT @SessionTimeout = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SESSION_TIMEOUT_MINUTES';
SELECT @CookieExpiry = c_setting_value FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.COOKIE_EXPIRY_DAYS';

IF @SessionTimeout IS NOT NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Session Timeout', 'OK', @SessionTimeout + ' minutes', 'INFO');
    PRINT '   ✓ SYSTEM.SESSION_TIMEOUT_MINUTES = ' + @SessionTimeout;
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Session Timeout', 'MISSING', 'Using default: 20 minutes', 'WARNING');
    PRINT '   ⚠️ SYSTEM.SESSION_TIMEOUT_MINUTES is MISSING (using default: 20)';
    SET @CheckCount = @CheckCount + 1;
END

IF @CookieExpiry IS NOT NULL
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Cookie Expiry', 'OK', @CookieExpiry + ' days', 'INFO');
    PRINT '   ✓ SYSTEM.COOKIE_EXPIRY_DAYS = ' + @CookieExpiry;
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Cookie Expiry', 'MISSING', 'Using default: 7 days', 'WARNING');
    PRINT '   ⚠️ SYSTEM.COOKIE_EXPIRY_DAYS is MISSING (using default: 7)';
    SET @CheckCount = @CheckCount + 1;
END

-- =============================================
-- 6. DATABASE COLUMN VERIFICATION (P1 Fix #1)
-- =============================================
PRINT '';
PRINT '6. Verifying database schema (P1 Fix #1)...';
PRINT '';

-- Check if c_orderid column exists in t_sys_orders
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_orders'
    AND COLUMN_NAME = 'c_orderid'
)
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Orders Table Column', 'OK', 'Column c_orderid exists', 'INFO');
    PRINT '   ✓ t_sys_orders.c_orderid column exists (P1 fix validated)';
    SET @CheckCount = @CheckCount + 1;
END
ELSE IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_orders'
    AND COLUMN_NAME = 'c_order_id'
)
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Orders Table Column', 'WARNING', 'Found c_order_id instead of c_orderid', 'WARNING');
    PRINT '   ⚠️ WARNING: t_sys_orders uses c_order_id (with underscore)';
    PRINT '      Code expects c_orderid (no underscore). Update stored procedures if needed.';
    SET @CheckCount = @CheckCount + 1;
END
ELSE
BEGIN
    INSERT INTO #VerificationResults VALUES (@CheckCount, 'Orders Table Column', 'ERROR', 'Order ID column not found', 'ERROR');
    PRINT '   ❌ ERROR: t_sys_orders primary key column not found!';
    SET @CheckCount = @CheckCount + 1;
END

-- Check if c_orderid exists in t_sys_invoice
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_invoice'
    AND COLUMN_NAME = 'c_orderid'
)
BEGIN
    PRINT '   ✓ t_sys_invoice.c_orderid foreign key exists';
END
ELSE IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_invoice'
    AND COLUMN_NAME = 'c_order_id'
)
BEGIN
    PRINT '   ⚠️ WARNING: t_sys_invoice uses c_order_id (verify foreign key constraint)';
END

-- =============================================
-- 7. SUMMARY REPORT
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION SUMMARY';
PRINT '========================================';
PRINT '';

-- Count results by severity
DECLARE @CriticalCount INT, @ErrorCount INT, @WarningCount INT, @InfoCount INT;

SELECT @CriticalCount = COUNT(*) FROM #VerificationResults WHERE Severity = 'CRITICAL' AND Status NOT IN ('OK');
SELECT @ErrorCount = COUNT(*) FROM #VerificationResults WHERE Severity = 'ERROR' AND Status NOT IN ('OK');
SELECT @WarningCount = COUNT(*) FROM #VerificationResults WHERE Severity = 'WARNING' AND Status NOT IN ('OK');
SELECT @InfoCount = COUNT(*) FROM #VerificationResults WHERE Severity = 'INFO' AND Status = 'OK';

PRINT 'Results:';
PRINT '  ❌ CRITICAL Issues: ' + CAST(@CriticalCount AS VARCHAR);
PRINT '  ⚠️  ERRORS:         ' + CAST(@ErrorCount AS VARCHAR);
PRINT '  ⚠️  WARNINGS:       ' + CAST(@WarningCount AS VARCHAR);
PRINT '  ✓  PASSED:         ' + CAST(@InfoCount AS VARCHAR);
PRINT '';

-- Show all non-OK results
IF EXISTS (SELECT 1 FROM #VerificationResults WHERE Status NOT IN ('OK'))
BEGIN
    PRINT 'Issues found:';
    PRINT '----------------------------------------';

    SELECT
        CASE
            WHEN Severity = 'CRITICAL' THEN '❌ CRITICAL'
            WHEN Severity = 'ERROR' THEN '⚠️  ERROR'
            WHEN Severity = 'WARNING' THEN '⚠️  WARNING'
            ELSE 'ℹ INFO'
        END AS Severity,
        CheckName,
        Status,
        Details
    FROM #VerificationResults
    WHERE Status NOT IN ('OK')
    ORDER BY
        CASE Severity
            WHEN 'CRITICAL' THEN 1
            WHEN 'ERROR' THEN 2
            WHEN 'WARNING' THEN 3
            ELSE 4
        END,
        CheckNumber;

    PRINT '';
END

-- Overall status
IF @CriticalCount > 0
BEGIN
    PRINT '========================================';
    PRINT '❌ VERIFICATION FAILED - CRITICAL ISSUES';
    PRINT '========================================';
    PRINT 'APPLICATION CANNOT START UNTIL CRITICAL ISSUES ARE RESOLVED!';
    PRINT '';
    PRINT 'Required actions:';

    SELECT DISTINCT
        '  ' + CAST(ROW_NUMBER() OVER (ORDER BY CheckNumber) AS VARCHAR) + '. ' + CheckName + ': ' + Details
    FROM #VerificationResults
    WHERE Severity = 'CRITICAL' AND Status NOT IN ('OK');
END
ELSE IF @ErrorCount > 0
BEGIN
    PRINT '========================================';
    PRINT '⚠️  VERIFICATION WARNING - ERRORS FOUND';
    PRINT '========================================';
    PRINT 'Application may have reduced functionality.';
END
ELSE IF @WarningCount > 0
BEGIN
    PRINT '========================================';
    PRINT '⚠️  VERIFICATION PASSED WITH WARNINGS';
    PRINT '========================================';
    PRINT 'Application will start but some features may use defaults.';
END
ELSE
BEGIN
    PRINT '========================================';
    PRINT '✓ VERIFICATION PASSED - ALL CHECKS OK';
    PRINT '========================================';
    PRINT 'All P1 & P2 fix configurations are correct!';
END

PRINT '';
PRINT 'Verification completed at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';

-- Cleanup
DROP TABLE #VerificationResults;

SET NOCOUNT OFF;
GO
