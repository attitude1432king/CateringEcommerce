/*
 * Database Migration Script: Catering Delivery System
 * Purpose: Separate delivery tables for Sample and Event delivery
 * PostgreSQL Compatible Version
 * Date: 2026-01-20
 */

-- =============================================
-- Table: t_sys_sample_delivery
-- Purpose: Third-party provider delivery tracking for sample orders
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_sample_delivery (
    c_sample_delivery_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_provider VARCHAR(50),
    c_tracking_url VARCHAR(1000),
    c_tracking_id VARCHAR(200),
    c_delivery_status INTEGER NOT NULL DEFAULT 1,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,
    CONSTRAINT fk_sample_delivery_order FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_sample_delivery_user FOREIGN KEY (c_userid)
        REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_sample_delivery_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_sample_delivery_order ON t_sys_sample_delivery(c_orderid);

-- =============================================
-- Table: t_sys_event_delivery
-- Purpose: Status-based delivery tracking for event catering (no GPS)
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_event_delivery (
    c_event_delivery_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_vehicle_number VARCHAR(50),
    c_driver_name VARCHAR(100),
    c_driver_phone VARCHAR(20),
    c_delivery_status INTEGER NOT NULL DEFAULT 1,
    c_scheduled_dispatch_time TIMESTAMP,
    c_actual_dispatch_time TIMESTAMP,
    c_arrived_time TIMESTAMP,
    c_completed_time TIMESTAMP,
    c_notes VARCHAR(500),
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,
    CONSTRAINT fk_event_delivery_order FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_event_delivery_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_event_delivery_order ON t_sys_event_delivery(c_orderid);
CREATE INDEX IF NOT EXISTS ix_event_delivery_status ON t_sys_event_delivery(c_delivery_status);

-- =============================================
-- Table: t_sys_event_delivery_history
-- Purpose: Audit log for event delivery status changes
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_event_delivery_history (
    c_history_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_event_delivery_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,
    c_previous_status INTEGER,
    c_new_status INTEGER NOT NULL,
    c_changed_by_userid BIGINT,
    c_changed_by_type VARCHAR(20),
    c_notes VARCHAR(500),
    c_changed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_event_delivery_history_event_delivery FOREIGN KEY (c_event_delivery_id)
        REFERENCES t_sys_event_delivery(c_event_delivery_id)
);

CREATE INDEX IF NOT EXISTS ix_event_delivery_history_event_delivery ON t_sys_event_delivery_history(c_event_delivery_id);
CREATE INDEX IF NOT EXISTS ix_event_delivery_history_order ON t_sys_event_delivery_history(c_orderid);

-- =============================================
-- Migration Complete
-- =============================================

