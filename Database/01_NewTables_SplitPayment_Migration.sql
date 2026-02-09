/*
 * Database Migration Script: Checkout Modernization - New Tables
 * Purpose: Create new tables for split payments, saved addresses, order modifications, and Google Maps integration
 * Author: Claude Code
 * Date: 2026-01-16
 */

USE CateringDB;
GO

-- =============================================
-- Table: t_sys_user_addresses
-- Purpose: Store user's saved delivery addresses (max 5 per user)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_user_addresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_user_addresses] (
        [c_address_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_userid] BIGINT NOT NULL,
        [c_address_label] NVARCHAR(50) NOT NULL, -- Home, Office, Other
        [c_full_address] NVARCHAR(500) NOT NULL,
        [c_landmark] NVARCHAR(200) NULL,
        [c_city] NVARCHAR(100) NOT NULL,
        [c_state] NVARCHAR(100) NOT NULL,
        [c_pincode] VARCHAR(10) NOT NULL,
        [c_contact_person] NVARCHAR(100) NOT NULL,
        [c_contact_phone] VARCHAR(20) NOT NULL,
        [c_is_default] BIT NOT NULL DEFAULT 0,
        [c_created_date] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_isactive] BIT NOT NULL DEFAULT 1,

        CONSTRAINT [PK_t_sys_user_addresses] PRIMARY KEY CLUSTERED ([c_address_id] ASC),
        CONSTRAINT [FK_user_addresses_userid] FOREIGN KEY ([c_userid])
            REFERENCES [dbo].[t_sys_user]([c_pkid]) ON DELETE CASCADE
    );

    -- Create index for faster lookups by user
    CREATE NONCLUSTERED INDEX [IX_user_addresses_userid]
        ON [dbo].[t_sys_user_addresses]([c_userid])
        INCLUDE ([c_is_default], [c_isactive]);

    PRINT 'Table [t_sys_user_addresses] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_user_addresses] already exists.';
END
GO

-- =============================================
-- Table: t_sys_order_payment_stages
-- Purpose: Track split payments (40% pre-booking, 60% post-event)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payment_stages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_payment_stages] (
        [c_payment_stage_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_orderid] BIGINT NOT NULL,
        [c_stage_type] VARCHAR(20) NOT NULL, -- PreBooking, PostEvent
        [c_stage_percentage] DECIMAL(5,2) NOT NULL, -- 40.00, 60.00
        [c_stage_amount] DECIMAL(18,2) NOT NULL,
        [c_payment_method] VARCHAR(50) NULL, -- Online, COD, UPI, Card, Wallet
        [c_payment_gateway] VARCHAR(50) NULL, -- Razorpay, PhonePe
        [c_razorpay_order_id] VARCHAR(100) NULL,
        [c_razorpay_payment_id] VARCHAR(100) NULL,
        [c_transaction_id] VARCHAR(100) NULL,
        [c_upi_id] VARCHAR(100) NULL,
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Success, Failed, Refunded
        [c_payment_date] DATETIME NULL,
        [c_due_date] DATETIME NULL, -- For post-event payments
        [c_reminder_sent_count] INT NOT NULL DEFAULT 0,
        [c_last_reminder_date] DATETIME NULL,
        [c_created_date] DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT [PK_t_sys_order_payment_stages] PRIMARY KEY CLUSTERED ([c_payment_stage_id] ASC),
        CONSTRAINT [FK_payment_stages_orderid] FOREIGN KEY ([c_orderid])
            REFERENCES [dbo].[t_sys_orders]([c_orderid]) ON DELETE CASCADE,
        CONSTRAINT [CHK_stage_type] CHECK ([c_stage_type] IN ('PreBooking', 'PostEvent')),
        CONSTRAINT [CHK_stage_status] CHECK ([c_status] IN ('Pending', 'Success', 'Failed', 'Refunded'))
    );

    -- Index for faster lookups by order
    CREATE NONCLUSTERED INDEX [IX_payment_stages_orderid]
        ON [dbo].[t_sys_order_payment_stages]([c_orderid])
        INCLUDE ([c_stage_type], [c_status]);

    -- Index for background job: find pending payments by due date
    CREATE NONCLUSTERED INDEX [IX_payment_stages_status_duedate]
        ON [dbo].[t_sys_order_payment_stages]([c_status], [c_due_date])
        INCLUDE ([c_orderid], [c_reminder_sent_count], [c_last_reminder_date]);

    PRINT 'Table [t_sys_order_payment_stages] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_order_payment_stages] already exists.';
END
GO

-- =============================================
-- Table: t_sys_order_modifications
-- Purpose: Track order modifications during event (plate count increases, item additions)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_modifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_modifications] (
        [c_modification_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_orderid] BIGINT NOT NULL,
        [c_modification_type] VARCHAR(50) NOT NULL, -- GuestCountIncrease, ItemAddition, ServiceExtension, DecorationUpgrade
        [c_original_guest_count] INT NULL,
        [c_modified_guest_count] INT NULL,
        [c_additional_amount] DECIMAL(18,2) NOT NULL,
        [c_modification_reason] NVARCHAR(500) NOT NULL,
        [c_requested_by] BIGINT NOT NULL, -- Owner/Partner ID
        [c_approved_by] BIGINT NULL, -- User ID who approved
        [c_status] VARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, Paid
        [c_payment_stage_id] BIGINT NULL, -- Links to post-event payment stage
        [c_created_date] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_approved_date] DATETIME NULL,

        CONSTRAINT [PK_t_sys_order_modifications] PRIMARY KEY CLUSTERED ([c_modification_id] ASC),
        CONSTRAINT [FK_modifications_orderid] FOREIGN KEY ([c_orderid])
            REFERENCES [dbo].[t_sys_orders]([c_orderid]) ON DELETE CASCADE,
        CONSTRAINT [FK_modifications_payment_stage] FOREIGN KEY ([c_payment_stage_id])
            REFERENCES [dbo].[t_sys_order_payment_stages]([c_payment_stage_id]),
        CONSTRAINT [FK_modifications_requested_by] FOREIGN KEY ([c_requested_by])
            REFERENCES [dbo].[t_sys_catering_owner]([c_ownerid]),
        CONSTRAINT [FK_modifications_approved_by] FOREIGN KEY ([c_approved_by])
            REFERENCES [dbo].[t_sys_user]([c_userid]),
        CONSTRAINT [CHK_modification_status] CHECK ([c_status] IN ('Pending', 'Approved', 'Rejected', 'Paid'))
    );

    -- Index for faster lookups by order
    CREATE NONCLUSTERED INDEX [IX_modifications_orderid]
        ON [dbo].[t_sys_order_modifications]([c_orderid])
        INCLUDE ([c_status], [c_created_date]);

    -- Index for finding pending modifications
    CREATE NONCLUSTERED INDEX [IX_modifications_status]
        ON [dbo].[t_sys_order_modifications]([c_status])
        INCLUDE ([c_orderid], [c_requested_by], [c_created_date]);

    PRINT 'Table [t_sys_order_modifications] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_order_modifications] already exists.';
END
GO

-- =============================================
-- Table: t_sys_event_locations
-- Purpose: Store event location with Google Maps coordinates
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_event_locations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_event_locations] (
        [c_location_id] BIGINT IDENTITY(1,1) NOT NULL,
        [c_orderid] BIGINT NOT NULL,
        [c_location_name] NVARCHAR(200) NOT NULL, -- "Hotel Taj, Pune"
        [c_formatted_address] NVARCHAR(500) NOT NULL, -- Google Maps formatted address
        [c_latitude] DECIMAL(10,7) NOT NULL,
        [c_longitude] DECIMAL(10,7) NOT NULL,
        [c_place_id] VARCHAR(200) NULL, -- Google Place ID
        [c_created_date] DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT [PK_t_sys_event_locations] PRIMARY KEY CLUSTERED ([c_location_id] ASC),
        CONSTRAINT [FK_event_locations_orderid] FOREIGN KEY ([c_orderid])
            REFERENCES [dbo].[t_sys_orders]([c_orderid]) ON DELETE CASCADE
    );

    -- Index for faster lookups by order
    CREATE NONCLUSTERED INDEX [IX_event_locations_orderid]
        ON [dbo].[t_sys_event_locations]([c_orderid]);

    -- Index for spatial queries (if needed in future for nearby searches)
    CREATE NONCLUSTERED INDEX [IX_event_locations_coordinates]
        ON [dbo].[t_sys_event_locations]([c_latitude], [c_longitude]);

    PRINT 'Table [t_sys_event_locations] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_event_locations] already exists.';
END
GO

-- =============================================
-- Summary
-- =============================================
PRINT '';
PRINT '================================================';
PRINT 'Migration completed successfully!';
PRINT 'New tables created:';
PRINT '  1. t_sys_user_addresses (Saved delivery addresses)';
PRINT '  2. t_sys_order_payment_stages (Split payment tracking)';
PRINT '  3. t_sys_order_modifications (Order modifications)';
PRINT '  4. t_sys_event_locations (Google Maps location data)';
PRINT '================================================';
GO
