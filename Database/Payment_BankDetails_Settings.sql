-- =============================================
-- Payment Bank Details Settings Migration
-- Adds platform bank account details for bank transfer payments
-- =============================================
-- Created: 2026-02-21
-- Description: Adds configurable bank details for displaying to customers during bank transfer payment option
-- =============================================

USE [YourDatabaseName]; -- Replace with your database name
GO

PRINT 'Starting Payment Bank Details Settings Migration...';
GO

-- =============================================
-- Add PAYMENT.BANK_* Settings
-- =============================================

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_NAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.BANK_NAME', 'HDFC Bank', 'PAYMENT', 'STRING', 'Bank Name', 'Name of the bank for receiving payments via bank transfer', 21, 'HDFC Bank');
    PRINT 'Added PAYMENT.BANK_NAME setting';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_ACCOUNT_NAME')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.BANK_ACCOUNT_NAME', 'Enyvora Catering Services', 'PAYMENT', 'STRING', 'Account Name', 'Account holder name for receiving payments', 22, 'Enyvora Catering Services');
    PRINT 'Added PAYMENT.BANK_ACCOUNT_NAME setting';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_ACCOUNT_NUMBER')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_is_sensitive)
    VALUES ('PAYMENT.BANK_ACCOUNT_NUMBER', '1234567890123456', 'PAYMENT', 'STRING', 'Account Number', 'Bank account number (masked for non-admin users)', 23, '1234567890123456', 0);
    PRINT 'Added PAYMENT.BANK_ACCOUNT_NUMBER setting';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_IFSC_CODE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value, c_validation_regex)
    VALUES ('PAYMENT.BANK_IFSC_CODE', 'HDFC0001234', 'PAYMENT', 'STRING', 'IFSC Code', 'Bank IFSC code for NEFT/RTGS transfers', 24, 'HDFC0001234', '^[A-Z]{4}0[A-Z0-9]{6}$');
    PRINT 'Added PAYMENT.BANK_IFSC_CODE setting';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'PAYMENT.BANK_TRANSFER_ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('PAYMENT.BANK_TRANSFER_ENABLED', 'true', 'PAYMENT', 'BOOLEAN', 'Enable Bank Transfer', 'Allow customers to pay via bank transfer', 25, 'true');
    PRINT 'Added PAYMENT.BANK_TRANSFER_ENABLED setting';
END

GO

PRINT 'Payment Bank Details Settings Migration completed successfully!';
PRINT '';
PRINT 'IMPORTANT: Please update the bank details in the Admin Settings page:';
PRINT '  - PAYMENT.BANK_NAME';
PRINT '  - PAYMENT.BANK_ACCOUNT_NAME';
PRINT '  - PAYMENT.BANK_ACCOUNT_NUMBER';
PRINT '  - PAYMENT.BANK_IFSC_CODE';
PRINT '';
GO
