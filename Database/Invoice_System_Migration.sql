-- =============================================
-- Invoice & Payment System Migration
-- Version: 1.0.0
-- Date: 2026-02-20
-- Author: Tech Lead - CateringDB
-- Description: Complete invoice, payment schedule, and audit system implementation
-- =============================================

USE CateringDB;
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'Invoice System Migration - Starting...';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- =============================================
-- STEP 1: Create Backup Tables (Safety First)
-- =============================================
PRINT '';
PRINT 'STEP 1: Creating backup tables...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders_backup_20260220]') AND type in (N'U'))
BEGIN
    SELECT * INTO t_sys_orders_backup_20260220 FROM t_sys_orders;
    PRINT '  ✓ t_sys_orders backed up';
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payment_stages_backup_20260220]') AND type in (N'U'))
BEGIN
    SELECT * INTO t_sys_order_payment_stages_backup_20260220 FROM t_sys_order_payment_stages;
    PRINT '  ✓ t_sys_order_payment_stages backed up';
END
GO

-- =============================================
-- STEP 2: Create t_sys_invoice Table
-- =============================================
PRINT '';
PRINT 'STEP 2: Creating t_sys_invoice table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_invoice](
        -- Primary Key
        [c_invoice_id] BIGINT IDENTITY(1,1) NOT NULL,

        -- Order & Event Reference
        [c_order_id] BIGINT NOT NULL,
        [c_event_id] BIGINT NULL,
        [c_userid] BIGINT NOT NULL,
        [c_ownerid] BIGINT NOT NULL,

        -- Invoice Type & Classification
        [c_invoice_type] INT NOT NULL, -- 1=BOOKING, 2=PRE_EVENT, 3=FINAL
        [c_is_proforma] BIT NOT NULL DEFAULT 0, -- TRUE for booking invoice
        [c_invoice_number] VARCHAR(50) NOT NULL UNIQUE,
        [c_invoice_date] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_due_date] DATETIME NULL,

        -- Financial Details
        [c_subtotal] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_cgst_percent] DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        [c_sgst_percent] DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        [c_cgst_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_sgst_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_total_tax_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_discount_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_total_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_amount_paid] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [c_balance_due] DECIMAL(18,2) NOT NULL DEFAULT 0.00,

        -- Payment Stage Information
        [c_payment_stage_type] VARCHAR(20) NOT NULL, -- BOOKING/PRE_EVENT/FINAL
        [c_payment_percentage] DECIMAL(5,2) NOT NULL, -- 40.00/35.00/25.00

        -- Invoice Status
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'UNPAID', -- DRAFT/UNPAID/PARTIALLY_PAID/PAID/OVERDUE/EXPIRED/CANCELLED

        -- Payment Gateway Integration
        [c_razorpay_order_id] VARCHAR(100) NULL,
        [c_razorpay_payment_id] VARCHAR(100) NULL,
        [c_transaction_id] VARCHAR(100) NULL,
        [c_payment_method] VARCHAR(50) NULL, -- Online/UPI/Card/NetBanking
        [c_payment_date] DATETIME NULL,

        -- GST Compliance (India)
        [c_company_gstin] VARCHAR(15) NULL, -- Your company GSTIN
        [c_customer_gstin] VARCHAR(15) NULL, -- Customer GSTIN (if B2B)
        [c_place_of_supply] VARCHAR(100) NULL, -- State name for GST
        [c_sac_code] VARCHAR(20) NOT NULL DEFAULT '996331', -- Catering services SAC code

        -- Additional Information
        [c_notes] NVARCHAR(MAX) NULL,
        [c_terms_and_conditions] NVARCHAR(MAX) NULL,
        [c_internal_remarks] NVARCHAR(500) NULL, -- Admin notes

        -- PDF Storage
        [c_pdf_path] NVARCHAR(500) NULL,
        [c_pdf_generated_date] DATETIME NULL,

        -- Audit Fields
        [c_createdby] BIGINT NULL,
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_modifiedby] BIGINT NULL,
        [c_modifieddate] DATETIME NULL,
        [c_is_deleted] BIT NOT NULL DEFAULT 0,
        [c_deleted_date] DATETIME NULL,

        -- Version Control (for regeneration tracking)
        [c_version] INT NOT NULL DEFAULT 1,
        [c_parent_invoice_id] BIGINT NULL, -- Reference to previous version if regenerated

        CONSTRAINT [PK_t_sys_invoice] PRIMARY KEY CLUSTERED ([c_invoice_id] ASC),
        CONSTRAINT [FK_invoice_order] FOREIGN KEY([c_order_id]) REFERENCES [dbo].[t_sys_orders] ([c_orderid]),
        CONSTRAINT [FK_invoice_user] FOREIGN KEY([c_userid]) REFERENCES [dbo].[t_sys_user] ([c_userid]),
        CONSTRAINT [FK_invoice_owner] FOREIGN KEY([c_ownerid]) REFERENCES [dbo].[t_sys_catering_owner] ([c_ownerid]),
        CONSTRAINT [CHK_invoice_type] CHECK ([c_invoice_type] IN (1, 2, 3)), -- 1=BOOKING, 2=PRE_EVENT, 3=FINAL
        CONSTRAINT [CHK_invoice_status] CHECK ([c_status] IN ('DRAFT', 'UNPAID', 'PARTIALLY_PAID', 'PAID', 'OVERDUE', 'EXPIRED', 'CANCELLED')),
        CONSTRAINT [CHK_invoice_stage_type] CHECK ([c_payment_stage_type] IN ('BOOKING', 'PRE_EVENT', 'FINAL')),
        CONSTRAINT [CHK_invoice_percentage] CHECK ([c_payment_percentage] IN (40.00, 35.00, 25.00)),
        CONSTRAINT [CHK_invoice_amounts] CHECK ([c_total_amount] = [c_subtotal] + [c_total_tax_amount] - [c_discount_amount]),
        CONSTRAINT [CHK_invoice_balance] CHECK ([c_balance_due] = [c_total_amount] - [c_amount_paid])
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    PRINT '  ✓ t_sys_invoice table created';
END
ELSE
BEGIN
    PRINT '  ⚠ t_sys_invoice table already exists';
END
GO

-- =============================================
-- STEP 3: Create t_sys_invoice_line_items Table
-- =============================================
PRINT '';
PRINT 'STEP 3: Creating t_sys_invoice_line_items table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_line_items]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_invoice_line_items](
        -- Primary Key
        [c_line_item_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_invoice_id] BIGINT NOT NULL,

        -- Item Details
        [c_item_type] VARCHAR(30) NOT NULL, -- PACKAGE/FOOD_ITEM/DECORATION/EXTRA_GUEST/ADDON/OVERTIME/DELIVERY/OTHER
        [c_item_id] BIGINT NULL, -- Reference to actual item (package/food/decoration)
        [c_description] NVARCHAR(500) NOT NULL,
        [c_hsn_sac_code] VARCHAR(20) NULL, -- HSN/SAC code for the item

        -- Quantity & Pricing
        [c_quantity] DECIMAL(10,2) NOT NULL DEFAULT 1,
        [c_unit_of_measure] VARCHAR(20) NULL, -- pcs, kg, plate, hours, etc.
        [c_unit_price] DECIMAL(18,2) NOT NULL,
        [c_subtotal] DECIMAL(18,2) NOT NULL, -- quantity * unit_price

        -- Tax Details
        [c_tax_percent] DECIMAL(5,2) NOT NULL DEFAULT 18.00,
        [c_cgst_percent] DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        [c_sgst_percent] DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        [c_tax_amount] DECIMAL(18,2) NOT NULL,
        [c_cgst_amount] DECIMAL(18,2) NOT NULL,
        [c_sgst_amount] DECIMAL(18,2) NOT NULL,

        -- Discount (if applicable at line level)
        [c_discount_percent] DECIMAL(5,2) NOT NULL DEFAULT 0.00,
        [c_discount_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,

        -- Total
        [c_total] DECIMAL(18,2) NOT NULL, -- subtotal + tax - discount

        -- Audit
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_sequence] INT NOT NULL DEFAULT 1, -- Display order

        CONSTRAINT [PK_t_sys_invoice_line_items] PRIMARY KEY CLUSTERED ([c_line_item_id] ASC),
        CONSTRAINT [FK_line_item_invoice] FOREIGN KEY([c_invoice_id]) REFERENCES [dbo].[t_sys_invoice] ([c_invoice_id]) ON DELETE CASCADE,
        CONSTRAINT [CHK_line_item_type] CHECK ([c_item_type] IN ('PACKAGE', 'FOOD_ITEM', 'DECORATION', 'EXTRA_GUEST', 'ADDON', 'OVERTIME', 'DELIVERY', 'STAFF', 'OTHER')),
        CONSTRAINT [CHK_line_item_quantity] CHECK ([c_quantity] > 0),
        CONSTRAINT [CHK_line_item_subtotal] CHECK ([c_subtotal] = [c_quantity] * [c_unit_price]),
        CONSTRAINT [CHK_line_item_total] CHECK ([c_total] = [c_subtotal] + [c_tax_amount] - [c_discount_amount])
    ) ON [PRIMARY];

    PRINT '  ✓ t_sys_invoice_line_items table created';
END
ELSE
BEGIN
    PRINT '  ⚠ t_sys_invoice_line_items table already exists';
END
GO

-- =============================================
-- STEP 4: Create t_sys_payment_schedule Table
-- =============================================
PRINT '';
PRINT 'STEP 4: Creating t_sys_payment_schedule table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_payment_schedule]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_payment_schedule](
        -- Primary Key
        [c_schedule_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_order_id] BIGINT NOT NULL,

        -- Stage Information
        [c_stage_type] VARCHAR(20) NOT NULL, -- BOOKING/PRE_EVENT/FINAL
        [c_stage_sequence] INT NOT NULL, -- 1, 2, 3
        [c_percentage] DECIMAL(5,2) NOT NULL, -- 40.00, 35.00, 25.00
        [c_amount] DECIMAL(18,2) NOT NULL,

        -- Timeline
        [c_due_date] DATETIME NULL,
        [c_trigger_event] VARCHAR(50) NOT NULL, -- ORDER_APPROVED/GUEST_LOCK_DATE/EVENT_COMPLETED
        [c_auto_generate_date] DATETIME NULL, -- When invoice should auto-generate

        -- Invoice Link
        [c_invoice_id] BIGINT NULL, -- Links to generated invoice

        -- Status
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING/PAID/OVERDUE/CANCELLED

        -- Reminder Tracking
        [c_reminder_sent_count] INT NOT NULL DEFAULT 0,
        [c_last_reminder_date] DATETIME NULL,
        [c_next_reminder_date] DATETIME NULL,

        -- Audit
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_modifieddate] DATETIME NULL,

        CONSTRAINT [PK_t_sys_payment_schedule] PRIMARY KEY CLUSTERED ([c_schedule_id] ASC),
        CONSTRAINT [FK_schedule_order] FOREIGN KEY([c_order_id]) REFERENCES [dbo].[t_sys_orders] ([c_orderid]) ON DELETE CASCADE,
        CONSTRAINT [FK_schedule_invoice] FOREIGN KEY([c_invoice_id]) REFERENCES [dbo].[t_sys_invoice] ([c_invoice_id]),
        CONSTRAINT [CHK_schedule_stage_type] CHECK ([c_stage_type] IN ('BOOKING', 'PRE_EVENT', 'FINAL')),
        CONSTRAINT [CHK_schedule_sequence] CHECK ([c_stage_sequence] IN (1, 2, 3)),
        CONSTRAINT [CHK_schedule_percentage] CHECK ([c_percentage] IN (40.00, 35.00, 25.00)),
        CONSTRAINT [CHK_schedule_status] CHECK ([c_status] IN ('PENDING', 'PAID', 'OVERDUE', 'CANCELLED')),
        CONSTRAINT [UQ_schedule_order_stage] UNIQUE ([c_order_id], [c_stage_type]) -- One schedule per stage per order
    ) ON [PRIMARY];

    PRINT '  ✓ t_sys_payment_schedule table created';
END
ELSE
BEGIN
    PRINT '  ⚠ t_sys_payment_schedule table already exists';
END
GO

-- =============================================
-- STEP 5: Create t_sys_invoice_audit_log Table
-- =============================================
PRINT '';
PRINT 'STEP 5: Creating t_sys_invoice_audit_log table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_audit_log]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_invoice_audit_log](
        [c_audit_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_invoice_id] BIGINT NOT NULL,
        [c_order_id] BIGINT NOT NULL,

        -- Action Details
        [c_action] VARCHAR(50) NOT NULL, -- GENERATED/VIEWED/DOWNLOADED/PAID/CANCELLED/REGENERATED/STATUS_CHANGED
        [c_performed_by] BIGINT NULL, -- User/Admin ID
        [c_performed_by_type] VARCHAR(20) NULL, -- USER/ADMIN/OWNER/SYSTEM

        -- Status Change Tracking
        [c_old_status] VARCHAR(20) NULL,
        [c_new_status] VARCHAR(20) NULL,
        [c_old_amount_paid] DECIMAL(18,2) NULL,
        [c_new_amount_paid] DECIMAL(18,2) NULL,

        -- Request Information
        [c_ip_address] VARCHAR(50) NULL,
        [c_user_agent] VARCHAR(500) NULL,

        -- Additional Context
        [c_remarks] NVARCHAR(1000) NULL,
        [c_metadata] NVARCHAR(MAX) NULL, -- JSON for additional data

        -- Timestamp
        [c_timestamp] DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT [PK_t_sys_invoice_audit_log] PRIMARY KEY CLUSTERED ([c_audit_id] ASC),
        CONSTRAINT [FK_audit_invoice] FOREIGN KEY([c_invoice_id]) REFERENCES [dbo].[t_sys_invoice] ([c_invoice_id]),
        CONSTRAINT [FK_audit_order] FOREIGN KEY([c_order_id]) REFERENCES [dbo].[t_sys_orders] ([c_orderid])
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    PRINT '  ✓ t_sys_invoice_audit_log table created';
END
ELSE
BEGIN
    PRINT '  ⚠ t_sys_invoice_audit_log table already exists';
END
GO

-- =============================================
-- STEP 6: Alter t_sys_orders Table (Add New Columns)
-- =============================================
PRINT '';
PRINT 'STEP 6: Altering t_sys_orders table...';

-- Add guest_lock_date
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_guest_lock_date')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_guest_lock_date] DATETIME NULL;
    PRINT '  ✓ Column c_guest_lock_date added';
END

-- Add menu_lock_date
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_menu_lock_date')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_menu_lock_date] DATETIME NULL;
    PRINT '  ✓ Column c_menu_lock_date added';
END

-- Add guest_locked flag
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_guest_locked')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_guest_locked] BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ Column c_guest_locked added';
END

-- Add menu_locked flag
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_menu_locked')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_menu_locked] BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ Column c_menu_locked added';
END

-- Add original_guest_count (to track changes after lock)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_original_guest_count')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_original_guest_count] INT NULL;
    PRINT '  ✓ Column c_original_guest_count added';
END

-- Add final_guest_count
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_final_guest_count')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_final_guest_count] INT NULL;
    PRINT '  ✓ Column c_final_guest_count added';
END

-- Add extra_charges
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_extra_charges')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_extra_charges] DECIMAL(18,2) NOT NULL DEFAULT 0.00;
    PRINT '  ✓ Column c_extra_charges added';
END

-- Add total_paid_amount (for quick reference)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_total_paid_amount')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_total_paid_amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00;
    PRINT '  ✓ Column c_total_paid_amount added';
END

-- Add payment_progress_percentage
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_payment_progress_percentage')
BEGIN
    ALTER TABLE t_sys_orders ADD [c_payment_progress_percentage] DECIMAL(5,2) NOT NULL DEFAULT 0.00;
    PRINT '  ✓ Column c_payment_progress_percentage added';
END

GO

-- =============================================
-- STEP 7: Create Indexes for Performance
-- =============================================
PRINT '';
PRINT 'STEP 7: Creating indexes...';

-- Invoice indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_order_id' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_order_id ON t_sys_invoice(c_order_id)
    INCLUDE (c_invoice_type, c_status, c_total_amount, c_balance_due);
    PRINT '  ✓ Index IX_invoice_order_id created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_type' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_type ON t_sys_invoice(c_invoice_type);
    PRINT '  ✓ Index IX_invoice_type created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_status' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_status ON t_sys_invoice(c_status)
    INCLUDE (c_due_date, c_total_amount);
    PRINT '  ✓ Index IX_invoice_status created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_user_id' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_user_id ON t_sys_invoice(c_userid);
    PRINT '  ✓ Index IX_invoice_user_id created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_owner_id' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_owner_id ON t_sys_invoice(c_ownerid);
    PRINT '  ✓ Index IX_invoice_owner_id created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_razorpay_order' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_razorpay_order ON t_sys_invoice(c_razorpay_order_id);
    PRINT '  ✓ Index IX_invoice_razorpay_order created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_invoice_due_date' AND object_id = OBJECT_ID('t_sys_invoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_invoice_due_date ON t_sys_invoice(c_due_date)
    WHERE c_status IN ('UNPAID', 'OVERDUE');
    PRINT '  ✓ Index IX_invoice_due_date created';
END

-- Payment Schedule indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_schedule_order_id' AND object_id = OBJECT_ID('t_sys_payment_schedule'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_schedule_order_id ON t_sys_payment_schedule(c_order_id);
    PRINT '  ✓ Index IX_schedule_order_id created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_schedule_due_date' AND object_id = OBJECT_ID('t_sys_payment_schedule'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_schedule_due_date ON t_sys_payment_schedule(c_due_date)
    WHERE c_status = 'PENDING';
    PRINT '  ✓ Index IX_schedule_due_date created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_schedule_auto_generate' AND object_id = OBJECT_ID('t_sys_payment_schedule'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_schedule_auto_generate ON t_sys_payment_schedule(c_auto_generate_date)
    WHERE c_invoice_id IS NULL AND c_status = 'PENDING';
    PRINT '  ✓ Index IX_schedule_auto_generate created';
END

-- Invoice Line Items indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_line_items_invoice_id' AND object_id = OBJECT_ID('t_sys_invoice_line_items'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_line_items_invoice_id ON t_sys_invoice_line_items(c_invoice_id);
    PRINT '  ✓ Index IX_line_items_invoice_id created';
END

-- Audit Log indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_audit_invoice_id' AND object_id = OBJECT_ID('t_sys_invoice_audit_log'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_audit_invoice_id ON t_sys_invoice_audit_log(c_invoice_id, c_timestamp DESC);
    PRINT '  ✓ Index IX_audit_invoice_id created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_audit_action' AND object_id = OBJECT_ID('t_sys_invoice_audit_log'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_audit_action ON t_sys_invoice_audit_log(c_action, c_timestamp DESC);
    PRINT '  ✓ Index IX_audit_action created';
END

GO

-- =============================================
-- STEP 8: Insert Default Settings
-- =============================================
PRINT '';
PRINT 'STEP 8: Inserting default settings...';

-- Payment percentages
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_PERCENTAGE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.BOOKING_PERCENTAGE', '40.00', 'Booking advance payment percentage', 'Payment', 'decimal', 0, GETDATE());
    PRINT '  ✓ PAYMENT.BOOKING_PERCENTAGE setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_PERCENTAGE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.PRE_EVENT_PERCENTAGE', '35.00', 'Pre-event payment percentage', 'Payment', 'decimal', 0, GETDATE());
    PRINT '  ✓ PAYMENT.PRE_EVENT_PERCENTAGE setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_PERCENTAGE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.FINAL_PERCENTAGE', '25.00', 'Final settlement payment percentage', 'Payment', 'decimal', 0, GETDATE());
    PRINT '  ✓ PAYMENT.FINAL_PERCENTAGE setting added';
END

-- Lock dates
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.GUEST_LOCK_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.GUEST_LOCK_DAYS', '5', 'Days before event to lock guest count', 'Payment', 'int', 0, GETDATE());
    PRINT '  ✓ PAYMENT.GUEST_LOCK_DAYS setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.MENU_LOCK_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.MENU_LOCK_DAYS', '3', 'Days before event to lock menu', 'Payment', 'int', 0, GETDATE());
    PRINT '  ✓ PAYMENT.MENU_LOCK_DAYS setting added';
END

-- Due dates
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_DUE_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.BOOKING_DUE_DAYS', '7', 'Days to pay booking invoice', 'Payment', 'int', 0, GETDATE());
    PRINT '  ✓ PAYMENT.BOOKING_DUE_DAYS setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_DUE_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.PRE_EVENT_DUE_DAYS', '3', 'Days to pay pre-event invoice (before event)', 'Payment', 'int', 0, GETDATE());
    PRINT '  ✓ PAYMENT.PRE_EVENT_DUE_DAYS setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_DUE_DAYS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('PAYMENT.FINAL_DUE_DAYS', '7', 'Days to pay final invoice (after event)', 'Payment', 'int', 0, GETDATE());
    PRINT '  ✓ PAYMENT.FINAL_DUE_DAYS setting added';
END

-- GST settings
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.ENABLED', 'true', 'Enable GST calculations', 'GST', 'boolean', 0, GETDATE());
    PRINT '  ✓ GST.ENABLED setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.CGST_RATE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.CGST_RATE', '9.00', 'CGST rate percentage', 'GST', 'decimal', 0, GETDATE());
    PRINT '  ✓ GST.CGST_RATE setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.SGST_RATE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.SGST_RATE', '9.00', 'SGST rate percentage', 'GST', 'decimal', 0, GETDATE());
    PRINT '  ✓ GST.SGST_RATE setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.SAC_CODE')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.SAC_CODE', '996331', 'SAC code for outdoor catering services', 'GST', 'string', 0, GETDATE());
    PRINT '  ✓ GST.SAC_CODE setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.COMPANY_GSTIN')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.COMPANY_GSTIN', 'UPDATE_WITH_YOUR_GSTIN', 'Company GSTIN number', 'GST', 'string', 0, GETDATE());
    PRINT '  ✓ GST.COMPANY_GSTIN setting added (UPDATE REQUIRED)';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'GST.PLACE_OF_SUPPLY')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('GST.PLACE_OF_SUPPLY', 'Maharashtra', 'Default place of supply for GST', 'GST', 'string', 0, GETDATE());
    PRINT '  ✓ GST.PLACE_OF_SUPPLY setting added';
END

-- Invoice settings
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'INVOICE.PREFIX')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('INVOICE.PREFIX', 'INV', 'Invoice number prefix', 'Invoice', 'string', 0, GETDATE());
    PRINT '  ✓ INVOICE.PREFIX setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'INVOICE.AUTO_GENERATE_ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('INVOICE.AUTO_GENERATE_ENABLED', 'true', 'Enable automatic invoice generation', 'Invoice', 'boolean', 0, GETDATE());
    PRINT '  ✓ INVOICE.AUTO_GENERATE_ENABLED setting added';
END

-- Notification settings
IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'NOTIFICATION.INVOICE_EMAIL_ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('NOTIFICATION.INVOICE_EMAIL_ENABLED', 'true', 'Send invoice emails to customers', 'Notification', 'boolean', 0, GETDATE());
    PRINT '  ✓ NOTIFICATION.INVOICE_EMAIL_ENABLED setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'NOTIFICATION.INVOICE_SMS_ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('NOTIFICATION.INVOICE_SMS_ENABLED', 'true', 'Send invoice SMS to customers', 'Notification', 'boolean', 0, GETDATE());
    PRINT '  ✓ NOTIFICATION.INVOICE_SMS_ENABLED setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'NOTIFICATION.PAYMENT_REMINDER_ENABLED')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('NOTIFICATION.PAYMENT_REMINDER_ENABLED', 'true', 'Send payment reminders', 'Notification', 'boolean', 0, GETDATE());
    PRINT '  ✓ NOTIFICATION.PAYMENT_REMINDER_ENABLED setting added';
END

IF NOT EXISTS (SELECT * FROM t_sys_settings WHERE c_key = 'NOTIFICATION.REMINDER_INTERVALS')
BEGIN
    INSERT INTO t_sys_settings (c_key, c_value, c_description, c_category, c_data_type, c_is_encrypted, c_createddate)
    VALUES ('NOTIFICATION.REMINDER_INTERVALS', '7,3,1', 'Days before due date to send reminders (comma-separated)', 'Notification', 'string', 0, GETDATE());
    PRINT '  ✓ NOTIFICATION.REMINDER_INTERVALS setting added';
END

GO

-- =============================================
-- STEP 9: Create Helper Function for Invoice Number Generation
-- =============================================
PRINT '';
PRINT 'STEP 9: Creating helper functions...';

IF OBJECT_ID('fn_GenerateInvoiceNumber', 'FN') IS NOT NULL
    DROP FUNCTION fn_GenerateInvoiceNumber;
GO

CREATE FUNCTION fn_GenerateInvoiceNumber()
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @Prefix VARCHAR(10) = 'INV';
    DECLARE @DatePart VARCHAR(8) = CONVERT(VARCHAR(8), GETDATE(), 112); -- YYYYMMDD
    DECLARE @Sequence INT;

    -- Get next sequence number for today
    SELECT @Sequence = ISNULL(MAX(CAST(RIGHT(c_invoice_number, 5) AS INT)), 0) + 1
    FROM t_sys_invoice
    WHERE c_invoice_number LIKE @Prefix + '-' + @DatePart + '%';

    -- Return formatted invoice number
    RETURN @Prefix + '-' + @DatePart + '-' + RIGHT('00000' + CAST(@Sequence AS VARCHAR), 5);
END
GO

PRINT '  ✓ Function fn_GenerateInvoiceNumber created';
GO

-- =============================================
-- STEP 10: Verification Queries
-- =============================================
PRINT '';
PRINT 'STEP 10: Running verification...';

DECLARE @InvoiceTableExists BIT = 0;
DECLARE @LineItemsTableExists BIT = 0;
DECLARE @ScheduleTableExists BIT = 0;
DECLARE @AuditTableExists BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice]') AND type in (N'U'))
    SET @InvoiceTableExists = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_line_items]') AND type in (N'U'))
    SET @LineItemsTableExists = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_payment_schedule]') AND type in (N'U'))
    SET @ScheduleTableExists = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_audit_log]') AND type in (N'U'))
    SET @AuditTableExists = 1;

PRINT '  ✓ t_sys_invoice: ' + CASE WHEN @InvoiceTableExists = 1 THEN 'EXISTS' ELSE 'MISSING' END;
PRINT '  ✓ t_sys_invoice_line_items: ' + CASE WHEN @LineItemsTableExists = 1 THEN 'EXISTS' ELSE 'MISSING' END;
PRINT '  ✓ t_sys_payment_schedule: ' + CASE WHEN @ScheduleTableExists = 1 THEN 'EXISTS' ELSE 'MISSING' END;
PRINT '  ✓ t_sys_invoice_audit_log: ' + CASE WHEN @AuditTableExists = 1 THEN 'EXISTS' ELSE 'MISSING' END;

PRINT '';
PRINT '========================================';
PRINT 'Migration Status: ' + CASE
    WHEN @InvoiceTableExists = 1 AND @LineItemsTableExists = 1 AND @ScheduleTableExists = 1 AND @AuditTableExists = 1
    THEN 'SUCCESS ✓'
    ELSE 'INCOMPLETE - Please review errors above'
END;
PRINT 'Completion Time: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- =============================================
-- ROLLBACK SCRIPT (COMMENTED - KEEP FOR REFERENCE)
-- =============================================
/*
USE CateringDB;
GO

PRINT 'ROLLING BACK Invoice System Migration...';

-- Drop tables in reverse dependency order
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_audit_log]') AND type in (N'U'))
    DROP TABLE t_sys_invoice_audit_log;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_payment_schedule]') AND type in (N'U'))
    DROP TABLE t_sys_payment_schedule;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice_line_items]') AND type in (N'U'))
    DROP TABLE t_sys_invoice_line_items;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_invoice]') AND type in (N'U'))
    DROP TABLE t_sys_invoice;

-- Remove added columns from t_sys_orders
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_guest_lock_date')
    ALTER TABLE t_sys_orders DROP COLUMN c_guest_lock_date;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_menu_lock_date')
    ALTER TABLE t_sys_orders DROP COLUMN c_menu_lock_date;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_guest_locked')
    ALTER TABLE t_sys_orders DROP COLUMN c_guest_locked;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_orders') AND name = 'c_menu_locked')
    ALTER TABLE t_sys_orders DROP COLUMN c_menu_locked;

-- Drop function
IF OBJECT_ID('fn_GenerateInvoiceNumber', 'FN') IS NOT NULL
    DROP FUNCTION fn_GenerateInvoiceNumber;

-- Delete settings
DELETE FROM t_sys_settings WHERE c_key LIKE 'PAYMENT.%' OR c_key LIKE 'GST.%' OR c_key LIKE 'INVOICE.%' OR c_key LIKE 'NOTIFICATION.%';

PRINT 'Rollback completed.';
GO
*/
