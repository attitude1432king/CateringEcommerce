-- =====================================================
-- VENDOR TO PARTNER MIGRATION SCRIPT
-- =====================================================
-- Purpose: Rename all vendor-related tables and columns to partner
-- Date: 2026-01-31
-- Author: Automated Refactoring
--
-- ⚠️ CRITICAL WARNING ⚠️
-- 1. BACKUP YOUR DATABASE BEFORE RUNNING THIS SCRIPT!
-- 2. Test on development/staging environment first
-- 3. Run during maintenance window (minimal user activity)
-- 4. This script makes irreversible schema changes
-- =====================================================

USE [CateringEcommerce];
GO

PRINT '========================================';
PRINT 'Starting Vendor to Partner Migration';
PRINT 'Date: ' + CAST(GETDATE() AS VARCHAR(50));
PRINT '========================================';
GO

-- =====================================================
-- STEP 1: TABLE RENAMES
-- =====================================================
PRINT '';
PRINT 'STEP 1: Renaming Tables...';
GO

-- Check and rename t_sys_vendor_security_deposits
IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_vendor_security_deposits')
BEGIN
    PRINT '  - Renaming t_sys_vendor_security_deposits to t_sys_partner_security_deposits...';
    EXEC sp_rename 't_sys_vendor_security_deposits', 't_sys_partner_security_deposits';
    PRINT '    ✓ Table renamed successfully';
END
ELSE
BEGIN
    PRINT '  ⚠️ Warning: t_sys_vendor_security_deposits not found (may already be renamed)';
END
GO

-- Check and rename t_sys_vendor_partnership_tiers
IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_vendor_partnership_tiers')
BEGIN
    PRINT '  - Renaming t_sys_vendor_partnership_tiers to t_sys_partnership_tiers...';
    EXEC sp_rename 't_sys_vendor_partnership_tiers', 't_sys_partnership_tiers';
    PRINT '    ✓ Table renamed successfully';
END
ELSE
BEGIN
    PRINT '  ⚠️ Warning: t_sys_vendor_partnership_tiers not found (may already be renamed)';
END
GO

-- Check and rename t_sys_vendor_payout_requests
IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_vendor_payout_requests')
BEGIN
    PRINT '  - Renaming t_sys_vendor_payout_requests to t_sys_partner_payout_requests...';
    EXEC sp_rename 't_sys_vendor_payout_requests', 't_sys_partner_payout_requests';
    PRINT '    ✓ Table renamed successfully';
END
ELSE
BEGIN
    PRINT '  ⚠️ Warning: t_sys_vendor_payout_requests not found (may already be renamed)';
END
GO

PRINT 'STEP 1 Complete: Tables renamed';
PRINT '';
GO

-- =====================================================
-- STEP 2: COLUMN RENAMES - t_sys_order_complaints
-- =====================================================
PRINT 'STEP 2: Renaming Columns in t_sys_order_complaints...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_response')
BEGIN
    PRINT '  - Renaming c_vendor_response to c_partner_response...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_response', 'c_partner_response', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_response_date')
BEGIN
    PRINT '  - Renaming c_vendor_response_date to c_partner_response_date...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_response_date', 'c_partner_response_date', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_notified_date')
BEGIN
    PRINT '  - Renaming c_vendor_notified_date to c_partner_notified_date...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_notified_date', 'c_partner_notified_date', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_admitted_fault')
BEGIN
    PRINT '  - Renaming c_vendor_admitted_fault to c_partner_admitted_fault...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_admitted_fault', 'c_partner_admitted_fault', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_offered_replacement')
BEGIN
    PRINT '  - Renaming c_vendor_offered_replacement to c_partner_offered_replacement...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_offered_replacement', 'c_partner_offered_replacement', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_provided_replacement')
BEGIN
    PRINT '  - Renaming c_vendor_provided_replacement to c_partner_provided_replacement...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_provided_replacement', 'c_partner_provided_replacement', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_vendor_penalty_amount')
BEGIN
    PRINT '  - Renaming c_vendor_penalty_amount to c_partner_penalty_amount...';
    EXEC sp_rename 't_sys_order_complaints.c_vendor_penalty_amount', 'c_partner_penalty_amount', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

PRINT 'Columns in t_sys_order_complaints renamed';
PRINT '';
GO

-- =====================================================
-- STEP 3: COLUMN RENAMES - t_sys_order_payment_summary
-- =====================================================
PRINT 'STEP 3: Renaming Columns in t_sys_order_payment_summary...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendorpayoutstatus')
BEGIN
    PRINT '  - Renaming c_vendorpayoutstatus to c_partnerpayoutstatus...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendorpayoutstatus', 'c_partnerpayoutstatus', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendoradvancereleased')
BEGIN
    PRINT '  - Renaming c_vendoradvancereleased to c_partneradvancereleased...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendoradvancereleased', 'c_partneradvancereleased', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendoradvanceamount')
BEGIN
    PRINT '  - Renaming c_vendoradvanceamount to c_partneradvanceamount...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendoradvanceamount', 'c_partneradvanceamount', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendoradvancereleaseddate')
BEGIN
    PRINT '  - Renaming c_vendoradvancereleaseddate to c_partneradvancereleaseddate...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendoradvancereleaseddate', 'c_partneradvancereleaseddate', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendorfinalpayout')
BEGIN
    PRINT '  - Renaming c_vendorfinalpayout to c_partnerfinalpayout...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendorfinalpayout', 'c_partnerfinalpayout', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_vendorfinalpayoutdate')
BEGIN
    PRINT '  - Renaming c_vendorfinalpayoutdate to c_partnerfinalpayoutdate...';
    EXEC sp_rename 't_sys_order_payment_summary.c_vendorfinalpayoutdate', 'c_partnerfinalpayoutdate', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

PRINT 'Columns in t_sys_order_payment_summary renamed';
PRINT '';
GO

-- =====================================================
-- STEP 4: COLUMN RENAMES - Supervisor Tables
-- =====================================================
PRINT 'STEP 4: Renaming Columns in Supervisor Tables...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_event_assignments') AND name = 'c_vendor_rating')
BEGIN
    PRINT '  - Renaming c_vendor_rating to c_partner_rating...';
    EXEC sp_rename 't_supervisor_event_assignments.c_vendor_rating', 'c_partner_rating', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_event_assignments') AND name = 'c_vendor_feedback')
BEGIN
    PRINT '  - Renaming c_vendor_feedback to c_partner_feedback...';
    EXEC sp_rename 't_supervisor_event_assignments.c_vendor_feedback', 'c_partner_feedback', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_availability_rules') AND name = 'c_is_new_vendor')
BEGIN
    PRINT '  - Renaming c_is_new_vendor to c_is_new_partner...';
    EXEC sp_rename 't_supervisor_availability_rules.c_is_new_vendor', 'c_is_new_partner', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

PRINT 'Columns in Supervisor tables renamed';
PRINT '';
GO

-- =====================================================
-- STEP 5: COLUMN RENAMES - t_sys_order_cancellations
-- =====================================================
PRINT 'STEP 5: Renaming Columns in t_sys_order_cancellations...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_vendor_compensation')
BEGIN
    PRINT '  - Renaming c_vendor_compensation to c_partner_compensation...';
    EXEC sp_rename 't_sys_order_cancellations.c_vendor_compensation', 'c_partner_compensation', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_vendor_response')
BEGIN
    PRINT '  - Renaming c_vendor_response to c_partner_response...';
    EXEC sp_rename 't_sys_order_cancellations.c_vendor_response', 'c_partner_response', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_vendor_response_date')
BEGIN
    PRINT '  - Renaming c_vendor_response_date to c_partner_response_date...';
    EXEC sp_rename 't_sys_order_cancellations.c_vendor_response_date', 'c_partner_response_date', 'COLUMN';
    PRINT '    ✓ Column renamed';
END
GO

PRINT 'Columns in t_sys_order_cancellations renamed';
PRINT '';
GO

-- =====================================================
-- VERIFICATION
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT 'Migration Verification';
PRINT '========================================';
GO

-- Verify renamed tables exist
PRINT 'Checking renamed tables...';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partner_security_deposits')
    PRINT '  ✓ t_sys_partner_security_deposits exists';
ELSE
    PRINT '  ✗ ERROR: t_sys_partner_security_deposits not found!';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partnership_tiers')
    PRINT '  ✓ t_sys_partnership_tiers exists';
ELSE
    PRINT '  ✗ ERROR: t_sys_partnership_tiers not found!';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partner_payout_requests')
    PRINT '  ✓ t_sys_partner_payout_requests exists';
ELSE
    PRINT '  ✗ ERROR: t_sys_partner_payout_requests not found!';

PRINT '';
PRINT 'Checking for any remaining "vendor" column names...';
SELECT
    t.name AS TableName,
    c.name AS ColumnName
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
WHERE c.name LIKE '%vendor%'
    AND t.name NOT LIKE 'AspNet%'
ORDER BY t.name, c.name;

PRINT '';
PRINT '========================================';
PRINT 'Migration Complete!';
PRINT 'Date: ' + CAST(GETDATE() AS VARCHAR(50));
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Review any remaining "vendor" columns listed above';
PRINT '2. Update stored procedures, views, and functions that reference old names';
PRINT '3. Update application code to use new table/column names';
PRINT '4. Run comprehensive integration tests';
PRINT '5. Update database documentation';
PRINT '';
GO
