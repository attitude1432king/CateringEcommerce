-- =============================================
-- Order Management Tables Creation Script
-- Created: 2026-01-15
-- Description: Creates tables for order management, payments, and order history
-- =============================================

USE [CateringDB]
GO

-- =============================================
-- Table: t_sys_orders
-- Description: Main orders table storing order details
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_orders](
        [c_orderid] [bigint] IDENTITY(1,1) NOT NULL,
        [c_userid] [bigint] NOT NULL,
        [c_ownerid] [bigint] NOT NULL,
        [c_order_number] [varchar](50) NOT NULL,
        [c_event_date] [datetime] NOT NULL,
        [c_event_time] [varchar](20) NOT NULL,
        [c_event_type] [varchar](100) NOT NULL,
        [c_event_location] [nvarchar](500) NOT NULL,
        [c_guest_count] [int] NOT NULL,
        [c_special_instructions] [nvarchar](1000) NULL,
        [c_delivery_address] [nvarchar](500) NOT NULL,
        [c_contact_person] [nvarchar](100) NOT NULL,
        [c_contact_phone] [varchar](20) NOT NULL,
        [c_contact_email] [varchar](100) NOT NULL,
        [c_base_amount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [c_tax_amount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [c_delivery_charges] [decimal](18, 2) NOT NULL DEFAULT 0,
        [c_discount_amount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [c_total_amount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [c_payment_method] [varchar](50) NOT NULL,
        [c_platform_commission] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [c_commission_rate] DECIMAL(5,2) NOT NULL DEFAULT 10.00,
        [c_payment_status] [varchar](20) NOT NULL DEFAULT 'Pending',
        [c_order_status] [varchar](20) NOT NULL DEFAULT 'Pending',
        [c_created_date] [datetime] NOT NULL DEFAULT GETDATE(),
        [c_updated_date] [datetime] NULL,
        [c_isactive] [bit] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_t_sys_orders] PRIMARY KEY CLUSTERED ([c_orderid] ASC),
        CONSTRAINT [UQ_t_sys_orders_order_number] UNIQUE NONCLUSTERED ([c_order_number] ASC),
        CONSTRAINT [FK_t_sys_orders_user] FOREIGN KEY([c_userid]) REFERENCES [dbo].[t_sys_user] ([c_userid]),
        CONSTRAINT [FK_t_sys_orders_catering] FOREIGN KEY([c_ownerid]) REFERENCES [dbo].[t_sys_catering_owner] ([c_ownerid])
    ) ON [PRIMARY]

    PRINT 'Table t_sys_orders created successfully.'
END
ELSE
BEGIN
    PRINT 'Table t_sys_orders already exists.'
END
GO

-- =============================================
-- Table: t_sys_order_items
-- Description: Stores individual order items (packages, food items, decorations)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_items]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_items](
        [c_order_item_id] [bigint] IDENTITY(1,1) NOT NULL,
        [c_orderid] [bigint] NOT NULL,
        [c_item_type] [varchar](20) NOT NULL,
        [c_item_id] [bigint] NOT NULL,
        [c_item_name] [nvarchar](200) NOT NULL,
        [c_quantity] [int] NOT NULL DEFAULT 1,
        [c_unit_price] [decimal](18, 2) NOT NULL,
        [c_total_price] [decimal](18, 2) NOT NULL,
        [c_package_selections] [nvarchar](max) NULL,
        [c_created_date] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_t_sys_order_items] PRIMARY KEY CLUSTERED ([c_order_item_id] ASC),
        CONSTRAINT [FK_t_sys_order_items_order] FOREIGN KEY([c_orderid]) REFERENCES [dbo].[t_sys_orders] ([c_orderid]) ON DELETE CASCADE
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    PRINT 'Table t_sys_order_items created successfully.'
END
ELSE
BEGIN
    PRINT 'Table t_sys_order_items already exists.'
END
GO

-- =============================================
-- Table: t_sys_order_status_history
-- Description: Tracks order status changes with timestamps
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_status_history]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_status_history](
        [c_history_id] [bigint] IDENTITY(1,1) NOT NULL,
        [c_orderid] [bigint] NOT NULL,
        [c_status] [varchar](20) NOT NULL,
        [c_remarks] [nvarchar](500) NULL,
        [c_updated_by] [bigint] NULL,
        [c_updated_date] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_t_sys_order_status_history] PRIMARY KEY CLUSTERED ([c_history_id] ASC),
        CONSTRAINT [FK_t_sys_order_status_history_order] FOREIGN KEY([c_orderid]) REFERENCES [dbo].[t_sys_orders] ([c_orderid]) ON DELETE CASCADE
    ) ON [PRIMARY]

    PRINT 'Table t_sys_order_status_history created successfully.'
END
ELSE
BEGIN
    PRINT 'Table t_sys_order_status_history already exists.'
END
GO

-- =============================================
-- Table: t_sys_order_payments
-- Description: Stores payment details including payment proofs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_order_payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_order_payments](
        [c_payment_id] [bigint] IDENTITY(1,1) NOT NULL,
        [c_orderid] [bigint] NOT NULL,
        [c_payment_method] [varchar](50) NOT NULL,
        [c_payment_gateway] [varchar](50) NULL,
        [c_transaction_id] [varchar](100) NULL,
        [c_payment_proof_path] [nvarchar](500) NULL,
        [c_amount] [decimal](18, 2) NOT NULL,
        [c_status] [varchar](20) NOT NULL DEFAULT 'Pending',
        [c_payment_date] [datetime] NULL,
        [c_verified_by] [bigint] NULL,
        [c_verified_date] [datetime] NULL,
        [c_created_date] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_t_sys_order_payments] PRIMARY KEY CLUSTERED ([c_payment_id] ASC),
        CONSTRAINT [FK_t_sys_order_payments_order] FOREIGN KEY([c_orderid]) REFERENCES [dbo].[t_sys_orders] ([c_orderid]) ON DELETE CASCADE
    ) ON [PRIMARY]

    PRINT 'Table t_sys_order_payments created successfully.'
END
ELSE
BEGIN
    PRINT 'Table t_sys_order_payments already exists.'
END
GO

-- =============================================
-- Create Indexes for Performance
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_t_sys_orders_userid' AND object_id = OBJECT_ID('t_sys_orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_t_sys_orders_userid] ON [dbo].[t_sys_orders]
    (
        [c_userid] ASC
    )
    INCLUDE([c_order_number], [c_order_status], [c_event_date], [c_created_date])
    PRINT 'Index IX_t_sys_orders_userid created successfully.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_t_sys_orders_cateringid' AND object_id = OBJECT_ID('t_sys_orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_t_sys_orders_cateringid] ON [dbo].[t_sys_orders]
    (
        [c_ownerid] ASC
    )
    INCLUDE([c_order_number], [c_order_status], [c_event_date], [c_created_date])
    PRINT 'Index IX_t_sys_orders_cateringid created successfully.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_t_sys_orders_status' AND object_id = OBJECT_ID('t_sys_orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_t_sys_orders_status] ON [dbo].[t_sys_orders]
    (
        [c_order_status] ASC,
        [c_created_date] DESC
    )
    PRINT 'Index IX_t_sys_orders_status created successfully.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_t_sys_orders_event_date' AND object_id = OBJECT_ID('t_sys_orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_t_sys_orders_event_date] ON [dbo].[t_sys_orders]
    (
        [c_event_date] ASC
    )
    INCLUDE([c_ownerid], [c_order_status])
    PRINT 'Index IX_t_sys_orders_event_date created successfully.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_t_sys_order_items_orderid' AND object_id = OBJECT_ID('t_sys_order_items'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_t_sys_order_items_orderid] ON [dbo].[t_sys_order_items]
    (
        [c_orderid] ASC
    )
    PRINT 'Index IX_t_sys_order_items_orderid created successfully.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT 'Order Management Tables Created Successfully!'
PRINT '=========================================='
PRINT 'Tables created:'
PRINT '  - t_sys_orders'
PRINT '  - t_sys_order_items'
PRINT '  - t_sys_order_status_history'
PRINT '  - t_sys_order_payments'
PRINT 'Indexes created for optimal query performance.'
PRINT '=========================================='
