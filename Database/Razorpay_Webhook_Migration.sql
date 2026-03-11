-- =============================================
-- RAZORPAY WEBHOOK CONFIGURATION MIGRATION
-- =============================================
-- Purpose: Add webhook secret setting for Razorpay webhook signature verification
-- Date: 2026-02-21
-- =============================================

USE CateringDB;
GO

-- =============================================
-- Add Razorpay Webhook Secret Setting
-- =============================================

-- Check if the setting already exists
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_description, c_is_encrypted, c_createddate)
    VALUES (
        'PAYMENT.RAZORPAY_WEBHOOK_SECRET',
        'YOUR_WEBHOOK_SECRET_HERE', -- REPLACE THIS WITH YOUR ACTUAL WEBHOOK SECRET FROM RAZORPAY DASHBOARD
        'Razorpay webhook secret for signature verification. Get this from Razorpay Dashboard > Settings > Webhooks',
        0, -- Set to 1 if you want to encrypt this value
        GETDATE()
    );

    PRINT 'Razorpay webhook secret setting added successfully.';
    PRINT '⚠️ IMPORTANT: Update the setting value with your actual webhook secret from Razorpay Dashboard!';
END
ELSE
BEGIN
    PRINT 'Razorpay webhook secret setting already exists. No changes made.';
END
GO

-- =============================================
-- VERIFICATION QUERY
-- =============================================
-- Run this to verify the setting was added correctly
SELECT
    c_setting_key,
    c_setting_value,
    c_description,
    c_is_encrypted,
    c_createddate
FROM t_sys_settings
WHERE c_setting_key = 'PAYMENT.RAZORPAY_WEBHOOK_SECRET';
GO

PRINT '';
PRINT '=============================================';
PRINT 'NEXT STEPS:';
PRINT '=============================================';
PRINT '1. Log in to Razorpay Dashboard (https://dashboard.razorpay.com/)';
PRINT '2. Navigate to Settings > Webhooks';
PRINT '3. Create a new webhook with URL: https://yourdomain.com/api/webhooks/razorpay';
PRINT '4. Select events: payment.authorized, payment.captured, payment.failed';
PRINT '5. Copy the generated webhook secret';
PRINT '6. Update the setting in this table with the actual secret:';
PRINT '   UPDATE t_sys_settings ';
PRINT '   SET c_setting_value = ''YOUR_ACTUAL_SECRET_HERE'' ';
PRINT '   WHERE c_setting_key = ''PAYMENT.RAZORPAY_WEBHOOK_SECRET'';';
PRINT '=============================================';
GO
