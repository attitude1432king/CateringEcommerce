-- =============================================
-- Financial Strategy Implementation
-- Date: 2026-01-30
-- Purpose: Implement all missing components from the financial strategy document
-- =============================================

USE [CateringEcommerce];
GO

PRINT '================================================';
PRINT 'Financial Strategy Implementation - Starting';
PRINT '================================================';
PRINT '';

-- =============================================
-- SECTION 1: Enhance Orders Table with Guest Count Locking
-- =============================================

PRINT 'SECTION 1: Enhancing Orders Table...';

-- Add guest count locking columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_original_guest_count')
BEGIN
    ALTER TABLE t_sys_order ADD c_original_guest_count INT NULL;
    PRINT '  ✓ Added c_original_guest_count';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_locked_guest_count')
BEGIN
    ALTER TABLE t_sys_order ADD c_locked_guest_count INT NULL;
    PRINT '  ✓ Added c_locked_guest_count';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_guest_count_locked')
BEGIN
    ALTER TABLE t_sys_order ADD c_guest_count_locked BIT DEFAULT 0;
    PRINT '  ✓ Added c_guest_count_locked';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_guest_count_locked_date')
BEGIN
    ALTER TABLE t_sys_order ADD c_guest_count_locked_date DATETIME NULL;
    PRINT '  ✓ Added c_guest_count_locked_date';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_final_served_guest_count')
BEGIN
    ALTER TABLE t_sys_order ADD c_final_served_guest_count INT NULL;
    PRINT '  ✓ Added c_final_served_guest_count';
END

-- Add menu locking columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_menu_locked')
BEGIN
    ALTER TABLE t_sys_order ADD c_menu_locked BIT DEFAULT 0;
    PRINT '  ✓ Added c_menu_locked';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_menu_locked_date')
BEGIN
    ALTER TABLE t_sys_order ADD c_menu_locked_date DATETIME NULL;
    PRINT '  ✓ Added c_menu_locked_date';
END

PRINT 'SECTION 1: Completed';
PRINT '';

-- =============================================
-- SECTION 2: Order Modifications Table
-- =============================================

PRINT 'SECTION 2: Creating Order Modifications Table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_modifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_modifications] (
        [c_modification_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_orderid] BIGINT NOT NULL,
        [c_modification_type] VARCHAR(50) NOT NULL, -- 'GUEST_COUNT_INCREASE', 'GUEST_COUNT_DECREASE', 'MENU_CHANGE', 'SERVICE_EXTENSION', 'DECORATION_UPGRADE'

        -- Guest Count Changes
        [c_original_guest_count] INT NULL,
        [c_modified_guest_count] INT NULL,
        [c_guest_count_change] INT NULL, -- Can be negative

        -- Menu Changes
        [c_menu_change_details] NVARCHAR(MAX) NULL, -- JSON: Old items, New items

        -- Financial Impact
        [c_original_amount] DECIMAL(18,2) NULL,
        [c_additional_amount] DECIMAL(18,2) NOT NULL, -- Can be negative for decreases
        [c_pricing_multiplier] DECIMAL(5,2) DEFAULT 1.00, -- 1.0x, 1.2x, 1.3x, 1.5x based on timing

        -- Request Details
        [c_modification_reason] NVARCHAR(500) NOT NULL,
        [c_requested_by] BIGINT NOT NULL, -- Owner/Partner ID or User ID
        [c_requested_by_type] VARCHAR(20) NOT NULL, -- 'CUSTOMER', 'VENDOR', 'ADMIN'
        [c_request_date] DATETIME DEFAULT GETDATE(),

        -- Approval Workflow
        [c_requires_approval] BIT DEFAULT 1,
        [c_approved_by] BIGINT NULL, -- User ID who approved (if customer requested) or Vendor ID (if vendor requested)
        [c_approved_by_type] VARCHAR(20) NULL, -- 'CUSTOMER', 'VENDOR', 'ADMIN'
        [c_approval_date] DATETIME NULL,
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected', 'Paid', 'Cancelled'
        [c_rejection_reason] NVARCHAR(500) NULL,

        -- Payment Linkage
        [c_payment_collected] BIT DEFAULT 0,
        [c_payment_transaction_id] BIGINT NULL, -- Links to payment transactions table

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),
        [c_modified_date] DATETIME NULL,

        CONSTRAINT [FK_OrderModifications_Order] FOREIGN KEY ([c_orderid]) REFERENCES [t_sys_order]([c_orderid]) ON DELETE CASCADE,
        CONSTRAINT [CK_OrderModifications_ModificationType] CHECK ([c_modification_type] IN
            ('GUEST_COUNT_INCREASE', 'GUEST_COUNT_DECREASE', 'MENU_CHANGE', 'SERVICE_EXTENSION', 'DECORATION_UPGRADE', 'OTHER')),
        CONSTRAINT [CK_OrderModifications_RequestedByType] CHECK ([c_requested_by_type] IN ('CUSTOMER', 'VENDOR', 'ADMIN')),
        CONSTRAINT [CK_OrderModifications_ApprovedByType] CHECK ([c_approved_by_type] IS NULL OR [c_approved_by_type] IN ('CUSTOMER', 'VENDOR', 'ADMIN')),
        CONSTRAINT [CK_OrderModifications_Status] CHECK ([c_status] IN ('Pending', 'Approved', 'Rejected', 'Paid', 'Cancelled'))
    );

    CREATE INDEX IX_OrderModifications_Order ON t_sys_order_modifications(c_orderid);
    CREATE INDEX IX_OrderModifications_Status ON t_sys_order_modifications(c_status, c_created_date DESC);
    CREATE INDEX IX_OrderModifications_Type ON t_sys_order_modifications(c_modification_type);

    PRINT '  ✓ Table t_sys_order_modifications created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_order_modifications already exists';
END

PRINT 'SECTION 2: Completed';
PRINT '';

-- =============================================
-- SECTION 3: Cancellation & Refund Policy Table
-- =============================================

PRINT 'SECTION 3: Creating Cancellation Policy Table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_cancellation_requests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_cancellation_requests] (
        [c_cancellation_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_orderid] BIGINT NOT NULL,
        [c_userid] BIGINT NOT NULL,
        [c_ownerid] BIGINT NOT NULL,

        -- Timing Analysis
        [c_event_date] DATETIME NOT NULL,
        [c_cancellation_request_date] DATETIME DEFAULT GETDATE(),
        [c_hours_before_event] INT NOT NULL, -- Calculated: Hours between request and event
        [c_days_before_event] INT NOT NULL, -- Calculated: Days between request and event

        -- Policy Applied
        [c_policy_tier] VARCHAR(20) NOT NULL, -- 'FULL_REFUND' (>7 days), 'PARTIAL_REFUND' (3-7 days), 'NO_REFUND' (<48 hours)
        [c_refund_percentage] DECIMAL(5,2) NOT NULL, -- 100.00, 50.00, 0.00

        -- Financial Breakdown
        [c_order_total_amount] DECIMAL(18,2) NOT NULL,
        [c_advance_paid] DECIMAL(18,2) NOT NULL,
        [c_refund_amount] DECIMAL(18,2) NOT NULL,
        [c_retention_amount] DECIMAL(18,2) NOT NULL, -- Amount kept (for vendor/platform)
        [c_platform_commission_forfeited] DECIMAL(18,2) DEFAULT 0, -- Commission platform gives up

        -- Vendor Compensation
        [c_vendor_compensation] DECIMAL(18,2) DEFAULT 0, -- Amount vendor keeps for losses

        -- Reason & Evidence
        [c_cancellation_reason] NVARCHAR(1000) NOT NULL,
        [c_is_force_majeure] BIT DEFAULT 0, -- Natural disaster, medical emergency, etc.
        [c_force_majeure_evidence] NVARCHAR(MAX) NULL, -- JSON: Document paths, descriptions

        -- Approval & Processing
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected', 'Refunded'
        [c_admin_approved_by] BIGINT NULL,
        [c_admin_approval_date] DATETIME NULL,
        [c_admin_notes] NVARCHAR(1000) NULL,
        [c_vendor_response] NVARCHAR(1000) NULL,
        [c_vendor_response_date] DATETIME NULL,

        -- Refund Processing
        [c_refund_initiated_date] DATETIME NULL,
        [c_refund_completed_date] DATETIME NULL,
        [c_refund_transaction_id] VARCHAR(200) NULL,
        [c_refund_method] VARCHAR(50) NULL, -- 'ORIGINAL_PAYMENT_METHOD', 'BANK_TRANSFER', 'WALLET'

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),
        [c_modified_date] DATETIME NULL,

        CONSTRAINT [FK_CancellationRequests_Order] FOREIGN KEY ([c_orderid]) REFERENCES [t_sys_order]([c_orderid]),
        CONSTRAINT [FK_CancellationRequests_User] FOREIGN KEY ([c_userid]) REFERENCES [t_sys_user]([c_userid]),
        CONSTRAINT [FK_CancellationRequests_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [CK_CancellationRequests_PolicyTier] CHECK ([c_policy_tier] IN ('FULL_REFUND', 'PARTIAL_REFUND', 'NO_REFUND', 'FORCE_MAJEURE')),
        CONSTRAINT [CK_CancellationRequests_Status] CHECK ([c_status] IN ('Pending', 'Approved', 'Rejected', 'Refunded', 'Cancelled'))
    );

    CREATE INDEX IX_CancellationRequests_Order ON t_sys_cancellation_requests(c_orderid);
    CREATE INDEX IX_CancellationRequests_Status ON t_sys_cancellation_requests(c_status, c_created_date DESC);
    CREATE INDEX IX_CancellationRequests_PolicyTier ON t_sys_cancellation_requests(c_policy_tier);

    PRINT '  ✓ Table t_sys_cancellation_requests created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_cancellation_requests already exists';
END

PRINT 'SECTION 3: Completed';
PRINT '';

-- =============================================
-- SECTION 4: Complaint & Dispute Management
-- =============================================

PRINT 'SECTION 4: Creating Complaint Management Table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_complaints]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_complaints] (
        [c_complaint_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_orderid] BIGINT NOT NULL,
        [c_userid] BIGINT NOT NULL,
        [c_ownerid] BIGINT NOT NULL,

        -- Complaint Type & Category
        [c_complaint_type] VARCHAR(50) NOT NULL, -- 'FOOD_COLD', 'FOOD_QUALITY', 'QUANTITY_SHORT', 'LATE_ARRIVAL', 'PARTIAL_ISSUE', 'SETUP_POOR', 'NO_SHOW', 'OTHER'
        [c_severity] VARCHAR(20) NOT NULL, -- 'CRITICAL', 'MAJOR', 'MINOR'
        [c_complaint_summary] NVARCHAR(200) NOT NULL,
        [c_complaint_details] NVARCHAR(MAX) NOT NULL,

        -- Specific Issue Details
        [c_affected_items] NVARCHAR(MAX) NULL, -- JSON: List of menu items affected
        [c_affected_item_count] INT DEFAULT 0, -- e.g., 1 out of 10 items
        [c_total_item_count] INT DEFAULT 0,
        [c_guest_complaint_count] INT DEFAULT 0, -- How many guests complained
        [c_total_guest_count] INT NOT NULL,

        -- Evidence
        [c_photo_evidence_paths] NVARCHAR(MAX) NULL, -- JSON array of image paths
        [c_video_evidence_paths] NVARCHAR(MAX) NULL, -- JSON array of video paths
        [c_witness_statements] NVARCHAR(MAX) NULL, -- JSON array of witness descriptions
        [c_timestamp_evidence] VARCHAR(MAX) NULL, -- Arrival time, service time, etc.

        -- Timing
        [c_issue_occurred_at] DATETIME NULL, -- When the issue occurred
        [c_reported_at] DATETIME DEFAULT GETDATE(),
        [c_is_reported_during_event] BIT DEFAULT 0,

        -- Vendor Response
        [c_vendor_notified_date] DATETIME NULL,
        [c_vendor_response] NVARCHAR(MAX) NULL,
        [c_vendor_response_date] DATETIME NULL,
        [c_vendor_admitted_fault] BIT NULL,
        [c_vendor_offered_replacement] BIT DEFAULT 0,
        [c_vendor_provided_replacement] BIT DEFAULT 0,

        -- Resolution
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Open', -- 'Open', 'Under_Investigation', 'Resolved', 'Rejected', 'Escalated'
        [c_resolution_type] VARCHAR(50) NULL, -- 'FULL_REFUND', 'PARTIAL_REFUND', 'REPLACEMENT', 'GOODWILL_CREDIT', 'NO_RESOLUTION'
        [c_refund_percentage] DECIMAL(5,2) DEFAULT 0,
        [c_refund_amount] DECIMAL(18,2) DEFAULT 0,
        [c_goodwill_credit] DECIMAL(18,2) DEFAULT 0,

        -- Validity Assessment
        [c_is_valid_complaint] BIT NULL, -- True/False after investigation
        [c_validity_reason] NVARCHAR(500) NULL,
        [c_severity_factor] DECIMAL(3,2) DEFAULT 1.0, -- Multiplier: 0.5x (minor), 1.0x (normal), 1.5x (important), 2.0x (critical)

        -- Admin Review
        [c_reviewed_by] BIGINT NULL,
        [c_reviewed_date] DATETIME NULL,
        [c_admin_notes] NVARCHAR(MAX) NULL,
        [c_resolution_notes] NVARCHAR(MAX) NULL,
        [c_resolved_date] DATETIME NULL,

        -- Fraud Detection
        [c_is_flagged_suspicious] BIT DEFAULT 0,
        [c_customer_complaint_history_count] INT DEFAULT 0, -- How many complaints this customer has filed

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),
        [c_modified_date] DATETIME NULL,

        CONSTRAINT [FK_OrderComplaints_Order] FOREIGN KEY ([c_orderid]) REFERENCES [t_sys_order]([c_orderid]),
        CONSTRAINT [FK_OrderComplaints_User] FOREIGN KEY ([c_userid]) REFERENCES [t_sys_user]([c_userid]),
        CONSTRAINT [FK_OrderComplaints_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [CK_OrderComplaints_Type] CHECK ([c_complaint_type] IN
            ('FOOD_COLD', 'FOOD_QUALITY', 'QUANTITY_SHORT', 'LATE_ARRIVAL', 'PARTIAL_ISSUE', 'SETUP_POOR', 'NO_SHOW', 'VENDOR_NO_SHOW', 'OTHER')),
        CONSTRAINT [CK_OrderComplaints_Severity] CHECK ([c_severity] IN ('CRITICAL', 'MAJOR', 'MINOR')),
        CONSTRAINT [CK_OrderComplaints_Status] CHECK ([c_status] IN ('Open', 'Under_Investigation', 'Resolved', 'Rejected', 'Escalated')),
        CONSTRAINT [CK_OrderComplaints_ResolutionType] CHECK ([c_resolution_type] IS NULL OR [c_resolution_type] IN
            ('FULL_REFUND', 'PARTIAL_REFUND', 'REPLACEMENT', 'GOODWILL_CREDIT', 'NO_RESOLUTION'))
    );

    CREATE INDEX IX_OrderComplaints_Order ON t_sys_order_complaints(c_orderid);
    CREATE INDEX IX_OrderComplaints_Status ON t_sys_order_complaints(c_status, c_created_date DESC);
    CREATE INDEX IX_OrderComplaints_Type ON t_sys_order_complaints(c_complaint_type);
    CREATE INDEX IX_OrderComplaints_Severity ON t_sys_order_complaints(c_severity);
    CREATE INDEX IX_OrderComplaints_User ON t_sys_order_complaints(c_userid);

    PRINT '  ✓ Table t_sys_order_complaints created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_order_complaints already exists';
END

PRINT 'SECTION 4: Completed';
PRINT '';

-- =============================================
-- SECTION 5: Vendor Security Deposit
-- =============================================

PRINT 'SECTION 5: Creating Vendor Security Deposit Table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_vendor_security_deposits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_vendor_security_deposits] (
        [c_deposit_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_ownerid] BIGINT NOT NULL,

        -- Deposit Details
        [c_deposit_amount] DECIMAL(18,2) NOT NULL DEFAULT 25000.00,
        [c_deposit_paid] BIT DEFAULT 0,
        [c_deposit_paid_date] DATETIME NULL,
        [c_payment_method] VARCHAR(50) NULL,
        [c_transaction_id] VARCHAR(200) NULL,

        -- Current Balance
        [c_current_balance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [c_holds_amount] DECIMAL(18,2) DEFAULT 0, -- Amount currently held for pending orders
        [c_available_balance] DECIMAL(18,2) DEFAULT 0, -- Current balance - holds

        -- Deposit Status
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Active', 'Depleted', 'Refunded'
        [c_is_active] BIT DEFAULT 0,

        -- Refund Details
        [c_refund_requested] BIT DEFAULT 0,
        [c_refund_request_date] DATETIME NULL,
        [c_refund_approved] BIT DEFAULT 0,
        [c_refund_processed_date] DATETIME NULL,
        [c_refund_amount] DECIMAL(18,2) NULL,
        [c_refund_transaction_id] VARCHAR(200) NULL,

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),
        [c_modified_date] DATETIME NULL,

        CONSTRAINT [FK_VendorSecurityDeposits_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [CK_VendorSecurityDeposits_Status] CHECK ([c_status] IN ('Pending', 'Active', 'Depleted', 'Refunded')),
        CONSTRAINT [UQ_VendorSecurityDeposits_Owner] UNIQUE ([c_ownerid])
    );

    CREATE INDEX IX_VendorSecurityDeposits_Status ON t_sys_vendor_security_deposits(c_status);

    PRINT '  ✓ Table t_sys_vendor_security_deposits created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_vendor_security_deposits already exists';
END

PRINT 'SECTION 5: Completed';
PRINT '';

-- =============================================
-- SECTION 6: Security Deposit Transactions
-- =============================================

PRINT 'SECTION 6: Creating Security Deposit Transaction Log...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_deposit_transactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_deposit_transactions] (
        [c_transaction_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_deposit_id] BIGINT NOT NULL,
        [c_ownerid] BIGINT NOT NULL,
        [c_orderid] BIGINT NULL, -- Null for deposit payment/refund

        -- Transaction Type
        [c_transaction_type] VARCHAR(50) NOT NULL, -- 'DEPOSIT', 'DEDUCTION', 'REFUND', 'HOLD', 'RELEASE_HOLD', 'TOP_UP'
        [c_amount] DECIMAL(18,2) NOT NULL,
        [c_balance_before] DECIMAL(18,2) NOT NULL,
        [c_balance_after] DECIMAL(18,2) NOT NULL,

        -- Reason & Reference
        [c_reason] NVARCHAR(500) NOT NULL,
        [c_reference_type] VARCHAR(50) NULL, -- 'VENDOR_NO_SHOW', 'COMPLAINT_REFUND', 'CANCELLATION_COMPENSATION', 'INITIAL_DEPOSIT', 'REFUND_TO_VENDOR'
        [c_reference_id] BIGINT NULL, -- Complaint ID, Cancellation ID, etc.

        -- Approval (for deductions)
        [c_approved_by] BIGINT NULL,
        [c_approval_date] DATETIME NULL,

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),

        CONSTRAINT [FK_DepositTransactions_Deposit] FOREIGN KEY ([c_deposit_id]) REFERENCES [t_sys_vendor_security_deposits]([c_deposit_id]),
        CONSTRAINT [FK_DepositTransactions_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [FK_DepositTransactions_Order] FOREIGN KEY ([c_orderid]) REFERENCES [t_sys_order]([c_orderid]),
        CONSTRAINT [CK_DepositTransactions_Type] CHECK ([c_transaction_type] IN
            ('DEPOSIT', 'DEDUCTION', 'REFUND', 'HOLD', 'RELEASE_HOLD', 'TOP_UP'))
    );

    CREATE INDEX IX_DepositTransactions_Deposit ON t_sys_deposit_transactions(c_deposit_id, c_created_date DESC);
    CREATE INDEX IX_DepositTransactions_Owner ON t_sys_deposit_transactions(c_ownerid);
    CREATE INDEX IX_DepositTransactions_Order ON t_sys_deposit_transactions(c_orderid) WHERE c_orderid IS NOT NULL;

    PRINT '  ✓ Table t_sys_deposit_transactions created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_deposit_transactions already exists';
END

PRINT 'SECTION 6: Completed';
PRINT '';

-- =============================================
-- SECTION 7: Commission Tier Tracking (Vendor Partnership Tiers)
-- =============================================

PRINT 'SECTION 7: Creating Vendor Partnership Tier Table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_vendor_partnership_tiers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_vendor_partnership_tiers] (
        [c_tier_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_ownerid] BIGINT NOT NULL,

        -- Current Tier
        [c_tier_name] VARCHAR(50) NOT NULL, -- 'FOUNDER_PARTNER', 'LAUNCH_PARTNER', 'EARLY_ADOPTER', 'STANDARD', 'PREMIUM'
        [c_current_commission_rate] DECIMAL(5,2) NOT NULL,

        -- Tier Assignment
        [c_tier_start_date] DATE NOT NULL,
        [c_tier_lock_end_date] DATE NULL, -- Lock-in period expiry
        [c_is_lock_period_active] BIT DEFAULT 1,
        [c_days_remaining_in_lock] INT DEFAULT 0,

        -- Qualification Criteria
        [c_joining_date] DATE NOT NULL,
        [c_joining_order_number] INT NOT NULL, -- 1-20 = Founder, 21-100 = Launch, etc.
        [c_required_orders_for_lock] INT NOT NULL, -- 5 for Founder, 3 for Launch, etc.
        [c_completed_orders_count] INT DEFAULT 0,
        [c_lock_qualified] BIT DEFAULT 0,
        [c_lock_qualified_date] DATE NULL,

        -- Performance-Based Commission Adjustment
        [c_monthly_order_count] INT DEFAULT 0,
        [c_average_rating] DECIMAL(3,2) DEFAULT 0,
        [c_qualifies_for_reduced_commission] BIT DEFAULT 0,
        [c_performance_commission_rate] DECIMAL(5,2) NULL, -- e.g., 10% for high performers

        -- Next Tier Transition
        [c_next_tier_name] VARCHAR(50) NULL,
        [c_next_tier_commission_rate] DECIMAL(5,2) NULL,
        [c_next_tier_effective_date] DATE NULL,
        [c_transition_notice_sent] BIT DEFAULT 0,
        [c_transition_notice_sent_date] DATE NULL,

        -- Badges & Benefits
        [c_has_founder_badge] BIT DEFAULT 0,
        [c_has_featured_listing] BIT DEFAULT 0,
        [c_has_priority_support] BIT DEFAULT 0,
        [c_has_account_manager] BIT DEFAULT 0,

        -- Audit
        [c_created_date] DATETIME DEFAULT GETDATE(),
        [c_modified_date] DATETIME NULL,

        CONSTRAINT [FK_VendorPartnershipTiers_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [CK_VendorPartnershipTiers_TierName] CHECK ([c_tier_name] IN
            ('FOUNDER_PARTNER', 'LAUNCH_PARTNER', 'EARLY_ADOPTER', 'STANDARD', 'PREMIUM')),
        CONSTRAINT [UQ_VendorPartnershipTiers_Owner] UNIQUE ([c_ownerid])
    );

    CREATE INDEX IX_VendorPartnershipTiers_Tier ON t_sys_vendor_partnership_tiers(c_tier_name);
    CREATE INDEX IX_VendorPartnershipTiers_LockEndDate ON t_sys_vendor_partnership_tiers(c_tier_lock_end_date) WHERE c_is_lock_period_active = 1;

    PRINT '  ✓ Table t_sys_vendor_partnership_tiers created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_vendor_partnership_tiers already exists';
END

PRINT 'SECTION 7: Completed';
PRINT '';

-- =============================================
-- SECTION 8: Commission Tier History
-- =============================================

PRINT 'SECTION 8: Creating Commission Tier Change History...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_commission_tier_history]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_commission_tier_history] (
        [c_history_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_ownerid] BIGINT NOT NULL,

        -- Change Details
        [c_old_tier_name] VARCHAR(50) NOT NULL,
        [c_new_tier_name] VARCHAR(50) NOT NULL,
        [c_old_commission_rate] DECIMAL(5,2) NOT NULL,
        [c_new_commission_rate] DECIMAL(5,2) NOT NULL,

        -- Reason & Timing
        [c_change_reason] VARCHAR(100) NOT NULL, -- 'LOCK_PERIOD_EXPIRED', 'PERFORMANCE_UPGRADE', 'PERFORMANCE_DOWNGRADE', 'ADMIN_OVERRIDE'
        [c_effective_date] DATE NOT NULL,
        [c_notice_period_days] INT DEFAULT 60,
        [c_notice_sent_date] DATE NULL,

        -- Vendor Communication
        [c_vendor_notified] BIT DEFAULT 0,
        [c_vendor_acknowledged] BIT DEFAULT 0,
        [c_vendor_acknowledgment_date] DATETIME NULL,

        -- Audit
        [c_changed_by] BIGINT NULL,
        [c_created_date] DATETIME DEFAULT GETDATE(),

        CONSTRAINT [FK_CommissionTierHistory_Owner] FOREIGN KEY ([c_ownerid]) REFERENCES [t_sys_catering_owner]([c_ownerid])
    );

    CREATE INDEX IX_CommissionTierHistory_Owner ON t_sys_commission_tier_history(c_ownerid, c_created_date DESC);
    CREATE INDEX IX_CommissionTierHistory_EffectiveDate ON t_sys_commission_tier_history(c_effective_date);

    PRINT '  ✓ Table t_sys_commission_tier_history created';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_commission_tier_history already exists';
END

PRINT 'SECTION 8: Completed';
PRINT '';

-- =============================================
-- SECTION 9: Add Cancellation Policy Settings
-- =============================================

PRINT 'SECTION 9: Adding Cancellation Policy Settings...';

-- Cancellation policy tier definitions
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.FULL_REFUND_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('CANCELLATION.FULL_REFUND_DAYS', '7', 'BUSINESS', 'NUMBER', 'Full Refund Period (Days)', 'Days before event when 100% refund is allowed', 20, '7');
    PRINT '  ✓ Added CANCELLATION.FULL_REFUND_DAYS';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.PARTIAL_REFUND_DAYS_START')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('CANCELLATION.PARTIAL_REFUND_DAYS_START', '3', 'BUSINESS', 'NUMBER', 'Partial Refund Start (Days)', 'Starting days for 50% refund window', 21, '3');
    PRINT '  ✓ Added CANCELLATION.PARTIAL_REFUND_DAYS_START';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.PARTIAL_REFUND_DAYS_END')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('CANCELLATION.PARTIAL_REFUND_DAYS_END', '7', 'BUSINESS', 'NUMBER', 'Partial Refund End (Days)', 'Ending days for 50% refund window', 22, '7');
    PRINT '  ✓ Added CANCELLATION.PARTIAL_REFUND_DAYS_END';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.NO_REFUND_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('CANCELLATION.NO_REFUND_HOURS', '48', 'BUSINESS', 'NUMBER', 'No Refund Window (Hours)', 'Hours before event when no refund is given', 23, '48');
    PRINT '  ✓ Added CANCELLATION.NO_REFUND_HOURS';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.PARTIAL_REFUND_PERCENTAGE')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('CANCELLATION.PARTIAL_REFUND_PERCENTAGE', '50', 'BUSINESS', 'NUMBER', 'Partial Refund Percentage', 'Refund percentage for partial refund tier', 24, '50');
    PRINT '  ✓ Added CANCELLATION.PARTIAL_REFUND_PERCENTAGE';
END

-- Guest count locking settings
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'ORDER.GUEST_COUNT_LOCK_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('ORDER.GUEST_COUNT_LOCK_DAYS', '5', 'BUSINESS', 'NUMBER', 'Guest Count Lock Period (Days)', 'Days before event when guest count is locked', 25, '5');
    PRINT '  ✓ Added ORDER.GUEST_COUNT_LOCK_DAYS';
END

IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'ORDER.MENU_LOCK_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('ORDER.MENU_LOCK_DAYS', '3', 'BUSINESS', 'NUMBER', 'Menu Lock Period (Days)', 'Days before event when menu is locked', 26, '3');
    PRINT '  ✓ Added ORDER.MENU_LOCK_DAYS';
END

-- Vendor security deposit amount
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'VENDOR.SECURITY_DEPOSIT_AMOUNT')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('VENDOR.SECURITY_DEPOSIT_AMOUNT', '25000', 'BUSINESS', 'NUMBER', 'Vendor Security Deposit (₹)', 'Required security deposit amount from vendors', 27, '25000');
    PRINT '  ✓ Added VENDOR.SECURITY_DEPOSIT_AMOUNT';
END

-- Dispute resolution SLA
IF NOT EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'DISPUTE.RESOLUTION_SLA_HOURS')
BEGIN
    INSERT INTO t_sys_settings (c_setting_key, c_setting_value, c_category, c_value_type, c_display_name, c_description, c_display_order, c_default_value)
    VALUES ('DISPUTE.RESOLUTION_SLA_HOURS', '12', 'BUSINESS', 'NUMBER', 'Dispute Resolution SLA (Hours)', 'Maximum hours to resolve a customer complaint', 28, '12');
    PRINT '  ✓ Added DISPUTE.RESOLUTION_SLA_HOURS';
END

PRINT 'SECTION 9: Completed';
PRINT '';

-- =============================================
-- SECTION 10: Create Automated Lock Job Flag Table
-- =============================================

PRINT 'SECTION 10: Creating Auto-Lock Job Configuration...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_auto_lock_jobs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_auto_lock_jobs] (
        [c_job_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_job_name] VARCHAR(100) NOT NULL,
        [c_job_type] VARCHAR(50) NOT NULL, -- 'GUEST_COUNT_LOCK', 'MENU_LOCK', 'PRE_EVENT_PAYMENT_CHARGE', 'POST_EVENT_RELEASE'
        [c_enabled] BIT DEFAULT 1,
        [c_last_run_date] DATETIME NULL,
        [c_next_run_date] DATETIME NULL,
        [c_run_frequency_minutes] INT DEFAULT 60, -- Run every 60 minutes
        [c_orders_processed_last_run] INT DEFAULT 0,
        [c_created_date] DATETIME DEFAULT GETDATE()
    );

    -- Insert default jobs
    INSERT INTO t_sys_auto_lock_jobs (c_job_name, c_job_type, c_enabled, c_run_frequency_minutes)
    VALUES
        ('Auto-Lock Guest Count (5 days before)', 'GUEST_COUNT_LOCK', 1, 60),
        ('Auto-Lock Menu (3 days before)', 'MENU_LOCK', 1, 60),
        ('Auto-Charge Pre-Event Payment (48h before)', 'PRE_EVENT_PAYMENT_CHARGE', 1, 120),
        ('Auto-Release Post-Event Payment (3 days after)', 'POST_EVENT_RELEASE', 1, 360);

    PRINT '  ✓ Table t_sys_auto_lock_jobs created and seeded';
END
ELSE
BEGIN
    PRINT '  ℹ Table t_sys_auto_lock_jobs already exists';
END

PRINT 'SECTION 10: Completed';
PRINT '';

-- =============================================
-- SECTION 11: Summary & Next Steps
-- =============================================

PRINT '';
PRINT '================================================';
PRINT 'Financial Strategy Implementation - COMPLETED';
PRINT '================================================';
PRINT '';
PRINT 'Tables Created/Enhanced:';
PRINT '  ✓ t_sys_order (enhanced with guest count locking)';
PRINT '  ✓ t_sys_order_modifications (guest count/menu changes)';
PRINT '  ✓ t_sys_cancellation_requests (cancellation policy tracking)';
PRINT '  ✓ t_sys_order_complaints (complaint/dispute management)';
PRINT '  ✓ t_sys_vendor_security_deposits (vendor deposits)';
PRINT '  ✓ t_sys_deposit_transactions (deposit transaction log)';
PRINT '  ✓ t_sys_vendor_partnership_tiers (commission tiers)';
PRINT '  ✓ t_sys_commission_tier_history (tier change tracking)';
PRINT '  ✓ t_sys_auto_lock_jobs (automated job configuration)';
PRINT '';
PRINT 'Settings Added:';
PRINT '  ✓ Cancellation policy settings (7 days / 3-7 days / <48h)';
PRINT '  ✓ Guest count lock period (5 days)';
PRINT '  ✓ Menu lock period (3 days)';
PRINT '  ✓ Vendor security deposit (₹25,000)';
PRINT '  ✓ Dispute resolution SLA (12 hours)';
PRINT '';
PRINT 'Next Steps Required:';
PRINT '  1. Create stored procedures for:';
PRINT '     - sp_AutoLockGuestCount';
PRINT '     - sp_AutoLockMenu';
PRINT '     - sp_ProcessCancellationRequest';
PRINT '     - sp_ProcessComplaintResolution';
PRINT '     - sp_CalculateRefundAmount';
PRINT '  2. Implement background jobs in C# for auto-locking';
PRINT '  3. Create API endpoints for:';
PRINT '     - Guest count modification requests';
PRINT '     - Menu change requests';
PRINT '     - Cancellation requests';
PRINT '     - Complaint filing and resolution';
PRINT '  4. Update frontend to display:';
PRINT '     - Lock countdown timers';
PRINT '     - Cancellation policy warnings';
PRINT '     - Commission tier badges';
PRINT '';
PRINT '================================================';
GO
