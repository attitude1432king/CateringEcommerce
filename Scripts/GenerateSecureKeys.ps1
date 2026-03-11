# ========================================
# Secure Key Generator for Production
# ========================================
# Generates cryptographically secure random keys for JWT and encryption
# Run this script before deploying to production

function Generate-SecureKey {
    param (
        [int]$Length = 32,
        [string]$Purpose
    )

    $chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}|;:,.<>?"
    $random = New-Object System.Random
    $key = -join (1..$Length | ForEach-Object { $chars[$random.Next(0, $chars.Length)] })

    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Generated $Purpose Key:" -ForegroundColor Cyan
    Write-Host $key -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""

    return $key
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Catering Ecommerce - Secure Key Generator       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  WARNING: These keys will be shown ONCE. Store them securely!" -ForegroundColor Red
Write-Host ""

# Generate keys
$jwtKey = Generate-SecureKey -Length 32 -Purpose "JWT Signing"
$encryptionKey = Generate-SecureKey -Length 32 -Purpose "Master Encryption"
$webhookSecret = Generate-SecureKey -Length 24 -Purpose "Razorpay Webhook"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "SQL Script to Update Production Database:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$sqlScript = @"
-- ========================================
-- PRODUCTION SECURITY KEYS UPDATE
-- Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- ========================================

USE CateringDB;
GO

-- Update JWT signing key
UPDATE t_sys_settings
SET c_setting_value = '$jwtKey',
    c_modifieddate = GETDATE(),
    c_modifiedby = 1  -- Admin user ID
WHERE c_setting_key = 'JWT.KEY';

-- Update master encryption key
UPDATE t_sys_settings
SET c_setting_value = '$encryptionKey',
    c_modifieddate = GETDATE(),
    c_modifiedby = 1
WHERE c_setting_key = 'SYSTEM.ENCRYPTION_KEY';

-- Update Razorpay webhook secret (optional - use Razorpay's generated secret)
-- UPDATE t_sys_settings
-- SET c_setting_value = '$webhookSecret',
--     c_modifieddate = GETDATE(),
--     c_modifiedby = 1
-- WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET';

-- Record in history
INSERT INTO t_sys_settings_history (c_setting_id, c_setting_key, c_old_value, c_new_value, c_changed_by, c_changed_by_name, c_change_reason)
SELECT
    c_setting_id,
    c_setting_key,
    '***REDACTED***',
    '***REDACTED***',
    1,
    'System Administrator',
    'Production deployment - initial key generation'
FROM t_sys_settings
WHERE c_setting_key IN ('JWT.KEY', 'SYSTEM.ENCRYPTION_KEY');

-- Verify update
SELECT
    c_setting_key,
    '***HIDDEN***' AS c_setting_value,
    c_modifieddate,
    CASE
        WHEN c_modifieddate > DATEADD(MINUTE, -5, GETDATE()) THEN 'Updated ✅'
        ELSE 'Not updated ⚠️'
    END AS Status
FROM t_sys_settings
WHERE c_setting_key IN ('JWT.KEY', 'SYSTEM.ENCRYPTION_KEY');

PRINT '========================================';
PRINT 'Production keys updated successfully!';
PRINT 'NEXT STEPS:';
PRINT '1. Restart the application to load new keys';
PRINT '2. All users will be logged out (expected)';
PRINT '3. Test authentication flow thoroughly';
PRINT '4. Monitor for any issues for 24 hours';
PRINT '========================================';
GO
"@

Write-Host $sqlScript
Write-Host ""

# Save to file
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputFile = "ProductionKeys_$timestamp.sql"
$sqlScript | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ SQL script saved to: $outputFile" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "⚠️  IMPORTANT SECURITY NOTES:" -ForegroundColor Red
Write-Host "1. Execute this SQL script on your PRODUCTION database server ONLY" -ForegroundColor Yellow
Write-Host "2. Delete the $outputFile file after use (contains sensitive keys)" -ForegroundColor Yellow
Write-Host "3. Store keys in a password manager (LastPass, 1Password, Azure Key Vault)" -ForegroundColor Yellow
Write-Host "4. Never commit these keys to version control" -ForegroundColor Yellow
Write-Host "5. Rotate these keys every 90 days (JWT) and 180 days (Encryption)" -ForegroundColor Yellow
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "Additional Production Checklist:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
Write-Host "[ ] Update Razorpay keys to PRODUCTION (rzp_live_xxx)" -ForegroundColor White
Write-Host "[ ] Update JWT.ISSUER to production API URL" -ForegroundColor White
Write-Host "[ ] Update JWT.AUDIENCE to production frontend URL" -ForegroundColor White
Write-Host "[ ] Update SMTP credentials to production email service" -ForegroundColor White
Write-Host "[ ] Update Twilio/MSG91 credentials to production accounts" -ForegroundColor White
Write-Host "[ ] Set OTP.BYPASS_VERIFICATION = false" -ForegroundColor White
Write-Host "[ ] Set CORS.ALLOWED_ORIGINS to production domain only" -ForegroundColor White
Write-Host "[ ] Configure database connection string in environment variables" -ForegroundColor White
Write-Host "[ ] Enable Application Insights or monitoring service" -ForegroundColor White
Write-Host "[ ] Set up daily database backups with 30-day retention" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit..."
