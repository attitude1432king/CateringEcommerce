-- =====================================================
-- VENDOR TO PARTNER MIGRATION ROLLBACK SCRIPT
-- =====================================================
-- Purpose: Rollback partner naming back to vendor
-- Date: 2026-01-31
-- Author: Automated Refactoring
--
-- ⚠️ USE WITH CAUTION ⚠️
-- This script reverses the vendor-to-partner migration
-- Only use if you need to rollback the changes
-- =====================================================

USE [CateringEcommerce];
GO

PRINT '========================================';
PRINT 'Starting Rollback: Partner to Vendor';
PRINT 'Date: ' + CAST(GETDATE() AS VARCHAR(50));
PRINT '========================================';
GO

-- =====================================================
-- STEP 1: ROLLBACK TABLE RENAMES
-- =====================================================
PRINT '';
PRINT 'STEP 1: Rolling back Table Renames...';
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partner_security_deposits')
BEGIN
    PRINT '  - Renaming t_sys_partner_security_deposits back to t_sys_vendor_security_deposits...';
    EXEC sp_rename 't_sys_partner_security_deposits', 't_sys_vendor_security_deposits';
    PRINT '    ✓ Table renamed';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partnership_tiers')
BEGIN
    PRINT '  - Renaming t_sys_partnership_tiers back to t_sys_vendor_partnership_tiers...';
    EXEC sp_rename 't_sys_partnership_tiers', 't_sys_vendor_partnership_tiers';
    PRINT '    ✓ Table renamed';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_partner_payout_requests')
BEGIN
    PRINT '  - Renaming t_sys_partner_payout_requests back to t_sys_vendor_payout_requests...';
    EXEC sp_rename 't_sys_partner_payout_requests', 't_sys_vendor_payout_requests';
    PRINT '    ✓ Table renamed';
END
GO

-- =====================================================
-- STEP 2: ROLLBACK COLUMN RENAMES - t_sys_order_complaints
-- =====================================================
PRINT 'STEP 2: Rolling back t_sys_order_complaints...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_response')
    EXEC sp_rename 't_sys_order_complaints.c_partner_response', 'c_vendor_response', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_response_date')
    EXEC sp_rename 't_sys_order_complaints.c_partner_response_date', 'c_vendor_response_date', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_notified_date')
    EXEC sp_rename 't_sys_order_complaints.c_partner_notified_date', 'c_vendor_notified_date', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_admitted_fault')
    EXEC sp_rename 't_sys_order_complaints.c_partner_admitted_fault', 'c_vendor_admitted_fault', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_offered_replacement')
    EXEC sp_rename 't_sys_order_complaints.c_partner_offered_replacement', 'c_vendor_offered_replacement', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_provided_replacement')
    EXEC sp_rename 't_sys_order_complaints.c_partner_provided_replacement', 'c_vendor_provided_replacement', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_complaints') AND name = 'c_partner_penalty_amount')
    EXEC sp_rename 't_sys_order_complaints.c_partner_penalty_amount', 'c_vendor_penalty_amount', 'COLUMN';
GO

-- =====================================================
-- STEP 3: ROLLBACK COLUMN RENAMES - t_sys_order_payment_summary
-- =====================================================
PRINT 'STEP 3: Rolling back t_sys_order_payment_summary...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partnerpayoutstatus')
    EXEC sp_rename 't_sys_order_payment_summary.c_partnerpayoutstatus', 'c_vendorpayoutstatus', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partneradvancereleased')
    EXEC sp_rename 't_sys_order_payment_summary.c_partneradvancereleased', 'c_vendoradvancereleased', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partneradvanceamount')
    EXEC sp_rename 't_sys_order_payment_summary.c_partneradvanceamount', 'c_vendoradvanceamount', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partneradvancereleaseddate')
    EXEC sp_rename 't_sys_order_payment_summary.c_partneradvancereleaseddate', 'c_vendoradvancereleaseddate', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partnerfinalpayout')
    EXEC sp_rename 't_sys_order_payment_summary.c_partnerfinalpayout', 'c_vendorfinalpayout', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_payment_summary') AND name = 'c_partnerfinalpayoutdate')
    EXEC sp_rename 't_sys_order_payment_summary.c_partnerfinalpayoutdate', 'c_vendorfinalpayoutdate', 'COLUMN';
GO

-- =====================================================
-- STEP 4: ROLLBACK COLUMN RENAMES - Supervisor Tables
-- =====================================================
PRINT 'STEP 4: Rolling back Supervisor tables...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_event_assignments') AND name = 'c_partner_rating')
    EXEC sp_rename 't_supervisor_event_assignments.c_partner_rating', 'c_vendor_rating', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_event_assignments') AND name = 'c_partner_feedback')
    EXEC sp_rename 't_supervisor_event_assignments.c_partner_feedback', 'c_vendor_feedback', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_supervisor_availability_rules') AND name = 'c_is_new_partner')
    EXEC sp_rename 't_supervisor_availability_rules.c_is_new_partner', 'c_is_new_vendor', 'COLUMN';
GO

-- =====================================================
-- STEP 5: ROLLBACK COLUMN RENAMES - t_sys_order_cancellations
-- =====================================================
PRINT 'STEP 5: Rolling back t_sys_order_cancellations...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_partner_compensation')
    EXEC sp_rename 't_sys_order_cancellations.c_partner_compensation', 'c_vendor_compensation', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_partner_response')
    EXEC sp_rename 't_sys_order_cancellations.c_partner_response', 'c_vendor_response', 'COLUMN';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order_cancellations') AND name = 'c_partner_response_date')
    EXEC sp_rename 't_sys_order_cancellations.c_partner_response_date', 'c_vendor_response_date', 'COLUMN';
GO

PRINT '';
PRINT '========================================';
PRINT 'Rollback Complete!';
PRINT 'Date: ' + CAST(GETDATE() AS VARCHAR(50));
PRINT '========================================';
GO
