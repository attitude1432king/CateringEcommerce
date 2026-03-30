/*
 * Database Migration Script: Sample Tasting Orders
 * Purpose: Create t_sys_sample_orders and t_sys_sample_order_items
 *          following project column-prefix convention (c_ prefix)
 *          with mandatory c_createddate and c_modifieddate audit columns.
 * Date: 2026-03-14
 */

USE [CateringDB];
GO

-- ============================================================
-- 1. t_sys_sample_orders — Main sample tasting order entity
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_sample_orders]') AND type = N'U'
)
BEGIN
    CREATE TABLE [dbo].[t_sys_sample_orders] (

        -- Primary Key
        [c_sample_order_id]              BIGINT          NOT NULL IDENTITY(1,1),

        -- Foreign Keys
        [c_userid]                        BIGINT          NOT NULL,       -- FK → t_sys_user.c_userid
        [c_ownerid]                   BIGINT          NOT NULL,       -- FK → t_sys_catering_owner.c_ownerid
        [c_delivery_address_id]           BIGINT          NULL,           -- FK → t_sys_user_addresses.c_address_id

        -- Pricing
        [c_sample_price_total]            DECIMAL(10,2)   NOT NULL,
        [c_delivery_charge]               DECIMAL(10,2)   NOT NULL DEFAULT 0,
        [c_total_amount]                  DECIMAL(10,2)   NOT NULL,

        -- Status
        [c_status]                        VARCHAR(50)     NOT NULL DEFAULT 'SAMPLE_REQUESTED',
        [c_payment_status]                VARCHAR(50)     NOT NULL DEFAULT 'PENDING',
        [c_is_paid]                       BIT             NOT NULL DEFAULT 0,

        -- Addresses & Coordinates
        [c_pickup_address]                NVARCHAR(500)   NOT NULL,
        [c_pickup_latitude]               DECIMAL(10,8)   NULL,
        [c_pickup_longitude]              DECIMAL(11,8)   NULL,
        [c_delivery_latitude]             DECIMAL(10,8)   NULL,
        [c_delivery_longitude]            DECIMAL(11,8)   NULL,

        -- Payment Reference
        [c_payment_id]                    BIGINT          NULL,
        [c_payment_gateway_order_id]      VARCHAR(100)    NULL,
        [c_payment_gateway_transaction_id] VARCHAR(100)   NULL,

        -- Partner Response
        [c_partner_response_date]         DATETIME        NULL,
        [c_rejection_reason]              NVARCHAR(500)   NULL,

        -- Third-Party Delivery
        [c_delivery_provider]             VARCHAR(50)     NULL,   -- 'DUNZO','PORTER','SHADOWFAX'
        [c_delivery_partner_order_id]     VARCHAR(100)    NULL,
        [c_delivery_partner_name]         VARCHAR(200)    NULL,
        [c_delivery_partner_phone]        VARCHAR(20)     NULL,
        [c_delivery_vehicle_number]       VARCHAR(50)     NULL,
        [c_estimated_pickup_time]         DATETIME        NULL,
        [c_actual_pickup_time]            DATETIME        NULL,
        [c_estimated_delivery_time]       DATETIME        NULL,
        [c_actual_delivery_time]          DATETIME        NULL,

        -- Customer Feedback & Conversion
        [c_client_feedback]               NVARCHAR(1000)  NULL,
        [c_taste_rating]                  INT             NULL,
        [c_hygiene_rating]                INT             NULL,
        [c_overall_rating]                INT             NULL,
        [c_feedback_date]                 DATETIME        NULL,
        [c_converted_to_event_order]      BIT             NOT NULL DEFAULT 0,
        [c_event_order_id]                BIGINT          NULL,   -- FK → t_sys_orders.c_orderid
        [c_conversion_date]               DATETIME        NULL,

        -- Mandatory Audit Columns
        [c_createddate]                   DATETIME        NOT NULL DEFAULT GETDATE(),
        [c_modifieddate]                  DATETIME        NOT NULL DEFAULT GETDATE(),

        -- Soft Delete / Active
        [c_is_active]                     BIT             NOT NULL DEFAULT 1,
        [c_is_deleted]                    BIT             NOT NULL DEFAULT 0,

        -- Constraints
        CONSTRAINT [PK_t_sys_sample_orders]
            PRIMARY KEY CLUSTERED ([c_sample_order_id] ASC),

        CONSTRAINT [FK_t_sys_sample_orders_user]
            FOREIGN KEY ([c_userid])
            REFERENCES [dbo].[t_sys_user] ([c_userid]),

        CONSTRAINT [FK_t_sys_sample_orders_catering]
            FOREIGN KEY ([c_ownerid])
            REFERENCES [dbo].[t_sys_catering_owner] ([c_ownerid]),

        CONSTRAINT [FK_t_sys_sample_orders_delivery_addr]
            FOREIGN KEY ([c_delivery_address_id])
            REFERENCES [dbo].[t_sys_user_addresses] ([c_address_id]),

        CONSTRAINT [FK_t_sys_sample_orders_event_order]
            FOREIGN KEY ([c_event_order_id])
            REFERENCES [dbo].[t_sys_orders] ([c_orderid]),

        CONSTRAINT [CK_t_sys_sample_orders_status]
            CHECK ([c_status] IN (
                'SAMPLE_REQUESTED','SAMPLE_ACCEPTED','SAMPLE_REJECTED',
                'SAMPLE_PREPARING','READY_FOR_PICKUP','IN_TRANSIT',
                'DELIVERED','REFUNDED'
            )),

        CONSTRAINT [CK_t_sys_sample_orders_taste_rating]
            CHECK ([c_taste_rating] IS NULL OR [c_taste_rating] BETWEEN 1 AND 5),

        CONSTRAINT [CK_t_sys_sample_orders_hygiene_rating]
            CHECK ([c_hygiene_rating] IS NULL OR [c_hygiene_rating] BETWEEN 1 AND 5),

        CONSTRAINT [CK_t_sys_sample_orders_overall_rating]
            CHECK ([c_overall_rating] IS NULL OR [c_overall_rating] BETWEEN 1 AND 5)
    );

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_orders_userid]
        ON [dbo].[t_sys_sample_orders] ([c_userid]);

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_orders_catering_id]
        ON [dbo].[t_sys_sample_orders] ([c_ownerid]);

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_orders_status]
        ON [dbo].[t_sys_sample_orders] ([c_status]);

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_orders_createddate]
        ON [dbo].[t_sys_sample_orders] ([c_createddate] DESC);

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_orders_payment_status]
        ON [dbo].[t_sys_sample_orders] ([c_payment_status]);

    PRINT 'Table [t_sys_sample_orders] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_sample_orders] already exists.';
END
GO

-- ============================================================
-- 2. t_sys_sample_order_items — Line items for sample orders
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_sample_order_items]') AND type = N'U'
)
BEGIN
    CREATE TABLE [dbo].[t_sys_sample_order_items] (

        -- Primary Key
        [c_sample_item_id]    BIGINT          NOT NULL IDENTITY(1,1),

        -- Foreign Key
        [c_sample_order_id]   BIGINT          NOT NULL,   -- FK → t_sys_sample_orders.c_sample_order_id
        [c_menu_item_id]      BIGINT          NOT NULL,   -- FK → t_sys_fooditems.c_foodid

        -- Item Details (snapshot — not live-linked to t_sys_fooditems)
        [c_menu_item_name]    NVARCHAR(200)   NOT NULL,
        [c_sample_price]      DECIMAL(10,2)   NOT NULL,
        [c_sample_quantity]   INT             NOT NULL DEFAULT 1,
        [c_category]          VARCHAR(100)    NULL,
        [c_description]       NVARCHAR(500)   NULL,
        [c_image_url]         NVARCHAR(500)   NULL,
        [c_cuisine_type]      VARCHAR(100)    NULL,
        [c_is_veg]            BIT             NULL,

        -- Source Tracking
        [c_is_from_package]   BIT             NOT NULL DEFAULT 0,
        [c_package_id]        BIGINT          NULL,

        -- Mandatory Audit Columns
        [c_createddate]       DATETIME        NOT NULL DEFAULT GETDATE(),
        [c_modifieddate]      DATETIME        NOT NULL DEFAULT GETDATE(),

        -- Constraints
        CONSTRAINT [PK_t_sys_sample_order_items]
            PRIMARY KEY CLUSTERED ([c_sample_item_id] ASC),

        CONSTRAINT [FK_t_sys_sample_order_items_order]
            FOREIGN KEY ([c_sample_order_id])
            REFERENCES [dbo].[t_sys_sample_orders] ([c_sample_order_id])
            ON DELETE CASCADE,

        CONSTRAINT [FK_t_sys_sample_order_items_fooditem]
            FOREIGN KEY ([c_menu_item_id])
            REFERENCES [dbo].[t_sys_fooditems] ([c_foodid])
    );

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_order_items_order_id]
        ON [dbo].[t_sys_sample_order_items] ([c_sample_order_id]);

    CREATE NONCLUSTERED INDEX [IX_t_sys_sample_order_items_menu_item_id]
        ON [dbo].[t_sys_sample_order_items] ([c_menu_item_id]);

    PRINT 'Table [t_sys_sample_order_items] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [t_sys_sample_order_items] already exists.';
END
GO

PRINT '============================================================';
PRINT 'Sample Orders Migration Complete';
PRINT 'Tables: t_sys_sample_orders, t_sys_sample_order_items';
PRINT '============================================================';
GO
