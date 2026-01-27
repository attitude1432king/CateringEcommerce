/*
 * Database Migration Script: Checkout Modernization - Alter Existing Tables
 * Purpose: Add new columns to t_sys_orders and t_sys_order_payments for split payment and location support
 * Author: Claude Code
 * Date: 2026-01-16
 */

USE CateringDB;
GO

-- =============================================
-- Alter Table: t_sys_orders
-- Purpose: Add split payment and Google Maps location columns
-- =============================================
PRINT 'Altering table [t_sys_orders]...';

-- Add c_payment_split_enabled column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_payment_split_enabled')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_payment_split_enabled] BIT NOT NULL DEFAULT 0;
    PRINT '  - Added column: c_payment_split_enabled';
END
ELSE
BEGIN
    PRINT '  - Column c_payment_split_enabled already exists';
END

-- Add c_prebooking_amount column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_prebooking_amount')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_prebooking_amount] DECIMAL(18,2) NULL;
    PRINT '  - Added column: c_prebooking_amount';
END
ELSE
BEGIN
    PRINT '  - Column c_prebooking_amount already exists';
END

-- Add c_postevent_amount column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_postevent_amount')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_postevent_amount] DECIMAL(18,2) NULL;
    PRINT '  - Added column: c_postevent_amount';
END
ELSE
BEGIN
    PRINT '  - Column c_postevent_amount already exists';
END

-- Add c_prebooking_status column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_prebooking_status')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_prebooking_status] VARCHAR(20) NULL; -- Pending, Paid, Failed
    PRINT '  - Added column: c_prebooking_status';
END
ELSE
BEGIN
    PRINT '  - Column c_prebooking_status already exists';
END

-- Add c_postevent_status column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_postevent_status')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_postevent_status] VARCHAR(20) NULL; -- Pending, Paid, Failed
    PRINT '  - Added column: c_postevent_status';
END
ELSE
BEGIN
    PRINT '  - Column c_postevent_status already exists';
END

-- Add c_event_latitude column (Google Maps)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_event_latitude')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_event_latitude] DECIMAL(10,7) NULL;
    PRINT '  - Added column: c_event_latitude';
END
ELSE
BEGIN
    PRINT '  - Column c_event_latitude already exists';
END

-- Add c_event_longitude column (Google Maps)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_event_longitude')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_event_longitude] DECIMAL(10,7) NULL;
    PRINT '  - Added column: c_event_longitude';
END
ELSE
BEGIN
    PRINT '  - Column c_event_longitude already exists';
END

-- Add c_event_place_id column (Google Place ID)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_event_place_id')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_event_place_id] VARCHAR(200) NULL;
    PRINT '  - Added column: c_event_place_id';
END
ELSE
BEGIN
    PRINT '  - Column c_event_place_id already exists';
END

-- Add c_saved_address_id column (reference to saved address)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'c_saved_address_id')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_saved_address_id] BIGINT NULL;
    PRINT '  - Added column: c_saved_address_id';
END
ELSE
BEGIN
    PRINT '  - Column c_saved_address_id already exists';
END

-- Create index for split payment queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND name = 'IX_orders_split_payment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_orders_split_payment]
        ON [dbo].[t_sys_orders]([c_payment_split_enabled], [c_prebooking_status], [c_postevent_status])
        INCLUDE ([c_orderid], [c_event_date]);
    PRINT '  - Created index: IX_orders_split_payment';
END
ELSE
BEGIN
    PRINT '  - Index IX_orders_split_payment already exists';
END

PRINT 'Table [t_sys_orders] alteration completed.';
PRINT '';
GO

-- =============================================
-- Alter Table: t_sys_order_payments
-- Purpose: Add Razorpay and payment stage columns
-- =============================================
PRINT 'Altering table [t_sys_order_payments]...';

-- Add c_payment_stage_type column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'c_payment_stage_type')
BEGIN
    ALTER TABLE [dbo].[t_sys_order_payments]
    ADD [c_payment_stage_type] VARCHAR(20) NULL; -- Full, PreBooking, PostEvent
    PRINT '  - Added column: c_payment_stage_type';
END
ELSE
BEGIN
    PRINT '  - Column c_payment_stage_type already exists';
END

-- Add c_razorpay_order_id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'c_razorpay_order_id')
BEGIN
    ALTER TABLE [dbo].[t_sys_order_payments]
    ADD [c_razorpay_order_id] VARCHAR(100) NULL;
    PRINT '  - Added column: c_razorpay_order_id';
END
ELSE
BEGIN
    PRINT '  - Column c_razorpay_order_id already exists';
END

-- Add c_razorpay_payment_id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'c_razorpay_payment_id')
BEGIN
    ALTER TABLE [dbo].[t_sys_order_payments]
    ADD [c_razorpay_payment_id] VARCHAR(100) NULL;
    PRINT '  - Added column: c_razorpay_payment_id';
END
ELSE
BEGIN
    PRINT '  - Column c_razorpay_payment_id already exists';
END

-- Add c_upi_id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'c_upi_id')
BEGIN
    ALTER TABLE [dbo].[t_sys_order_payments]
    ADD [c_upi_id] VARCHAR(100) NULL;
    PRINT '  - Added column: c_upi_id';
END
ELSE
BEGIN
    PRINT '  - Column c_upi_id already exists';
END

-- Create index for Razorpay payment lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'IX_payments_razorpay_order')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_payments_razorpay_order]
        ON [dbo].[t_sys_order_payments]([c_razorpay_order_id])
        INCLUDE ([c_orderid], [c_status]);
    PRINT '  - Created index: IX_payments_razorpay_order';
END
ELSE
BEGIN
    PRINT '  - Index IX_payments_razorpay_order already exists';
END

-- Create index for Razorpay payment ID lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND name = 'IX_payments_razorpay_payment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_payments_razorpay_payment]
        ON [dbo].[t_sys_order_payments]([c_razorpay_payment_id])
        INCLUDE ([c_orderid], [c_status]);
    PRINT '  - Created index: IX_payments_razorpay_payment';
END
ELSE
BEGIN
    PRINT '  - Index IX_payments_razorpay_payment already exists';
END

PRINT 'Table [t_sys_order_payments] alteration completed.';
PRINT '';
GO

-- =============================================
-- Add new order status values
-- =============================================
PRINT 'Adding new order status values...';
PRINT '  New statuses: AwaitingPrePayment, Closed';
PRINT '  (Ensure application code handles these new statuses)';
PRINT '';
GO

-- =============================================
-- Summary
-- =============================================
PRINT '================================================';
PRINT 'Migration completed successfully!';
PRINT '';
PRINT 'Modified tables:';
PRINT '  1. t_sys_orders - Added 9 columns:';
PRINT '     - c_payment_split_enabled';
PRINT '     - c_prebooking_amount';
PRINT '     - c_postevent_amount';
PRINT '     - c_prebooking_status';
PRINT '     - c_postevent_status';
PRINT '     - c_event_latitude';
PRINT '     - c_event_longitude';
PRINT '     - c_event_place_id';
PRINT '     - c_saved_address_id';
PRINT '';
PRINT '  2. t_sys_order_payments - Added 4 columns:';
PRINT '     - c_payment_stage_type';
PRINT '     - c_razorpay_order_id';
PRINT '     - c_razorpay_payment_id';
PRINT '     - c_upi_id';
PRINT '';
PRINT 'New indexes created for performance optimization.';
PRINT '================================================';
GO

-- =============================================
-- Optional: Update existing records
-- =============================================
/*
-- Uncomment if you want to set default values for existing records

-- Set existing orders to not use split payment
UPDATE [dbo].[t_sys_orders]
SET [c_payment_split_enabled] = 0
WHERE [c_payment_split_enabled] IS NULL;

-- Set existing payments to 'Full' payment stage
UPDATE [dbo].[t_sys_order_payments]
SET [c_payment_stage_type] = 'Full'
WHERE [c_payment_stage_type] IS NULL;

PRINT 'Updated existing records with default values.';
*/
GO
