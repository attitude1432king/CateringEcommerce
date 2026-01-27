/*
====================================================
CATERING DELIVERY SYSTEM - DATABASE TABLES
====================================================
Created: 2026-01-20
Purpose: Separate delivery tables for Sample and Event delivery
====================================================
*/

-- ====================================================
-- 1. SAMPLE DELIVERY (Third-party real-time tracking)
-- ====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_sample_delivery')
BEGIN
    CREATE TABLE t_sys_sample_delivery (
        c_sample_delivery_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_orderid BIGINT NOT NULL,
        c_userid BIGINT NOT NULL,
        c_ownerid BIGINT NOT NULL,

        -- Third-party provider info
        c_provider NVARCHAR(50) NULL,          -- Dunzo / Porter / Shadowfax
        c_tracking_url NVARCHAR(1000) NULL,
        c_tracking_id NVARCHAR(200) NULL,

        -- Status tracking
        c_delivery_status INT NOT NULL DEFAULT 1,
        /*
            1 = Requested
            2 = PickedUp
            3 = InTransit
            4 = Delivered
            5 = Failed
        */

        -- Timestamps
        c_created_at DATETIME NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME NULL,

        -- Indexes
        CONSTRAINT FK_SampleDelivery_Order FOREIGN KEY (c_orderid)
            REFERENCES t_sys_catering_order(c_orderid),
        CONSTRAINT FK_SampleDelivery_User FOREIGN KEY (c_userid)
            REFERENCES t_sys_user(c_userid),
        CONSTRAINT FK_SampleDelivery_Owner FOREIGN KEY (c_ownerid)
            REFERENCES t_sys_catering_owner(c_ownerid)
    );

    CREATE INDEX IX_SampleDelivery_Order ON t_sys_sample_delivery(c_orderid);
    CREATE INDEX IX_SampleDelivery_Status ON t_sys_sample_delivery(c_delivery_status);

    PRINT 'Table t_sys_sample_delivery created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_sample_delivery already exists';
END
GO

-- ====================================================
-- 2. EVENT CATERING DELIVERY (Status-based, NO GPS)
-- ====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_event_delivery')
BEGIN
    CREATE TABLE t_sys_event_delivery (
        c_event_delivery_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_orderid BIGINT NOT NULL,
        c_ownerid BIGINT NOT NULL,

        -- Vehicle & Driver Information
        c_vehicle_number NVARCHAR(50) NULL,
        c_driver_name NVARCHAR(100) NULL,
        c_driver_phone NVARCHAR(20) NULL,

        -- Status-driven delivery (NO GPS)
        c_delivery_status INT NOT NULL DEFAULT 1,
        /*
            1 = Preparation Started
            2 = Vehicle Ready
            3 = Dispatched
            4 = Arrived At Venue
            5 = Event Completed
        */

        -- Scheduling & Timing
        c_scheduled_dispatch_time DATETIME NULL,
        c_actual_dispatch_time DATETIME NULL,
        c_arrived_time DATETIME NULL,
        c_completed_time DATETIME NULL,

        -- Notes
        c_notes NVARCHAR(500) NULL,

        -- System timestamps
        c_created_at DATETIME NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME NULL,

        -- Indexes
        CONSTRAINT FK_EventDelivery_Order FOREIGN KEY (c_orderid)
            REFERENCES t_sys_catering_order(c_orderid),
        CONSTRAINT FK_EventDelivery_Owner FOREIGN KEY (c_ownerid)
            REFERENCES t_sys_catering_owner(c_ownerid)
    );

    CREATE INDEX IX_EventDelivery_Order ON t_sys_event_delivery(c_orderid);
    CREATE INDEX IX_EventDelivery_Status ON t_sys_event_delivery(c_delivery_status);
    CREATE INDEX IX_EventDelivery_Owner ON t_sys_event_delivery(c_ownerid);

    PRINT 'Table t_sys_event_delivery created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_event_delivery already exists';
END
GO

-- ====================================================
-- 3. EVENT DELIVERY STATUS HISTORY (Audit Log)
-- ====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_event_delivery_history')
BEGIN
    CREATE TABLE t_sys_event_delivery_history (
        c_history_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_event_delivery_id BIGINT NOT NULL,
        c_orderid BIGINT NOT NULL,

        -- Status change tracking
        c_previous_status INT NULL,
        c_new_status INT NOT NULL,
        c_changed_by_userid BIGINT NULL,    -- Admin/Partner who made the change
        c_changed_by_type NVARCHAR(20) NULL, -- 'Partner', 'Admin', 'System'

        -- Details
        c_notes NVARCHAR(500) NULL,
        c_changed_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_EventDeliveryHistory_EventDelivery FOREIGN KEY (c_event_delivery_id)
            REFERENCES t_sys_event_delivery(c_event_delivery_id)
    );

    CREATE INDEX IX_EventDeliveryHistory_EventDelivery ON t_sys_event_delivery_history(c_event_delivery_id);
    CREATE INDEX IX_EventDeliveryHistory_Order ON t_sys_event_delivery_history(c_orderid);

    PRINT 'Table t_sys_event_delivery_history created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_event_delivery_history already exists';
END
GO

PRINT 'Delivery tables setup completed successfully';
GO
