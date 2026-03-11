-- =============================================
-- P1 & P2 Fixes Configuration ROLLBACK Script
-- =============================================
-- This script removes all settings added by P1_P2_Fixes_Configuration_Migration.sql
-- USE WITH CAUTION - Only run if you need to completely undo the migration
-- =============================================

USE [CateringDB]
GO

BEGIN TRANSACTION;

PRINT '========================================';
PRINT 'P1 & P2 Fixes Configuration ROLLBACK';
PRINT 'Started at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';
PRINT '';
PRINT '⚠️ WARNING: This will DELETE all settings added by the migration!';
PRINT 'Press Ctrl+C within 5 seconds to cancel...';
PRINT '';

WAITFOR DELAY '00:00:05';

PRINT 'Proceeding with rollback...';
PRINT '';

-- =============================================
-- Backup settings before deletion
-- =============================================
PRINT '1. Creating backup of settings to be deleted...';

SELECT
    c_setting_key,
    c_setting_value,
    c_category,
    c_value_type,
    c_display_name,
    c_description,
    c_is_sensitive,
    c_createddate,
    c_last_modified
INTO #SettingsBackup
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
);

DECLARE @BackupCount INT;
SELECT @BackupCount = COUNT(*) FROM #SettingsBackup;
PRINT '   ✓ Backed up ' + CAST(@BackupCount AS VARCHAR) + ' settings';
PRINT '';

-- Display backup
PRINT 'Settings to be deleted:';
SELECT c_setting_key, c_category, c_setting_value FROM #SettingsBackup ORDER BY c_category, c_setting_key;
PRINT '';

-- =============================================
-- Delete settings
-- =============================================
PRINT '2. Deleting settings added by migration...';

DELETE FROM t_sys_settings WHERE c_setting_key = 'CORS.PRODUCTION_ORIGIN';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted CORS.PRODUCTION_ORIGIN';

DELETE FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted PAYMENT.RAZORPAY_WEBHOOK_SECRET';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.OTP_EXPIRY_SECONDS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SYSTEM.OTP_EXPIRY_SECONDS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'OTP.EXPIRY_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted OTP.EXPIRY_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.ADMIN_LOGIN_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.ADMIN_LOGIN_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.ADMIN_LOGIN_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.USER_LOGIN_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.USER_LOGIN_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.USER_LOGIN_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.OTP_SEND_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_SEND_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.OTP_SEND_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.OTP_VERIFY_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.OTP_VERIFY_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.OTP_VERIFY_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.API_GENERAL_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.API_GENERAL_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.API_GENERAL_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_PERMITS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.FILE_UPLOAD_PERMITS';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SECURITY.FILE_UPLOAD_WINDOW_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SECURITY.FILE_UPLOAD_WINDOW_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.SESSION_TIMEOUT_MINUTES';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SYSTEM.SESSION_TIMEOUT_MINUTES';

DELETE FROM t_sys_settings WHERE c_setting_key = 'SYSTEM.COOKIE_EXPIRY_DAYS';
IF @@ROWCOUNT > 0 PRINT '   ✓ Deleted SYSTEM.COOKIE_EXPIRY_DAYS';

PRINT '';

-- =============================================
-- Verify rollback
-- =============================================
PRINT '3. Verifying rollback...';

DECLARE @RemainingCount INT;
SELECT @RemainingCount = COUNT(*)
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
);

IF @RemainingCount = 0
BEGIN
    PRINT '   ✓ All migration settings successfully removed';
    PRINT '';
    PRINT '========================================';
    PRINT 'ROLLBACK COMPLETED SUCCESSFULLY';
    PRINT '========================================';
    PRINT '';
    PRINT 'Backup of deleted settings is in #SettingsBackup temp table';
    PRINT 'To restore, run:';
    PRINT '';
    PRINT '   INSERT INTO t_sys_settings';
    PRINT '   SELECT * FROM #SettingsBackup';
    PRINT '';

    COMMIT TRANSACTION;
    PRINT '✓ Transaction committed';
END
ELSE
BEGIN
    PRINT '   ⚠️ ERROR: ' + CAST(@RemainingCount AS VARCHAR) + ' settings still exist!';
    PRINT '';
    PRINT 'Remaining settings:';
    SELECT c_setting_key FROM t_sys_settings
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
    );

    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ Transaction rolled back';
END

PRINT '';
PRINT 'Rollback completed at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';

-- Cleanup temp table
DROP TABLE #SettingsBackup;
GO

PRINT '';
PRINT '⚠️ IMPORTANT NOTES AFTER ROLLBACK:';
PRINT '';
PRINT '1. Application will fall back to default values or appsettings.json';
PRINT '2. Payment webhooks will FAIL until PAYMENT.RAZORPAY_WEBHOOK_SECRET is restored';
PRINT '3. CORS may not work in production without CORS.PRODUCTION_ORIGIN';
PRINT '4. Rate limiting will use hardcoded defaults';
PRINT '';
PRINT 'To restore settings, re-run: P1_P2_Fixes_Configuration_Migration.sql';
PRINT '';
GO
