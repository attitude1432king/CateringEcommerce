/*
 * Database Migration Script: Checkout Modernization - New Tables
 * Purpose: Create new tables for split payments, saved addresses, order modifications, and Google Maps integration
 * PostgreSQL Compatible Version
 * Date: 2026-01-16
 */

-- =============================================
-- Table: t_sys_user_addresses
-- Purpose: Store user's saved delivery addresses (max 5 per user)
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_user_addresses (
    c_address_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_address_label VARCHAR(50) NOT NULL,
    c_full_address VARCHAR(500) NOT NULL,
    c_landmark VARCHAR(200),
    c_city VARCHAR(100) NOT NULL,
    c_state VARCHAR(100) NOT NULL,
    c_pincode VARCHAR(10) NOT NULL,
    c_contact_person VARCHAR(100) NOT NULL,
    c_contact_phone VARCHAR(20) NOT NULL,
    c_is_default BOOLEAN NOT NULL DEFAULT FALSE,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_isactive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_user_addresses_userid FOREIGN KEY (c_userid)
        REFERENCES t_sys_user(c_userid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_user_addresses_userid
    ON t_sys_user_addresses(c_userid);
-- =============================================
-- Table: t_sys_order_payment_stages
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_order_payment_stages (
    c_payment_stage_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_stage_type VARCHAR(20) NOT NULL,
    c_stage_percentage NUMERIC(5,2) NOT NULL,
    c_stage_amount NUMERIC(18,2) NOT NULL,
    c_payment_method VARCHAR(50),
    c_payment_gateway VARCHAR(50),
    c_razorpay_order_id VARCHAR(100),
    c_razorpay_payment_id VARCHAR(100),
    c_transaction_id VARCHAR(100),
    c_upi_id VARCHAR(100),
    c_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_payment_date TIMESTAMP,
    c_due_date TIMESTAMP,
    c_reminder_sent_count INTEGER NOT NULL DEFAULT 0,
    c_last_reminder_date TIMESTAMP,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_payment_stages_orderid 
        FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE,

    CONSTRAINT chk_stage_type 
        CHECK (c_stage_type IN ('PreBooking', 'PostEvent')),

    CONSTRAINT chk_stage_status 
        CHECK (c_status IN ('Pending', 'Success', 'Failed', 'Refunded'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_payment_stages_orderid
ON t_sys_order_payment_stages (c_orderid, c_stage_type, c_status);

CREATE INDEX IF NOT EXISTS idx_payment_stages_status_duedate
ON t_sys_order_payment_stages (c_status, c_due_date);

-- =============================================
-- Table: t_sys_event_locations
-- Purpose: Store event location with Google Maps coordinates
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_event_locations (
    c_location_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_location_name VARCHAR(200) NOT NULL,
    c_formatted_address VARCHAR(500) NOT NULL,
    c_latitude DECIMAL(10,7) NOT NULL,
    c_longitude DECIMAL(10,7) NOT NULL,
    c_place_id VARCHAR(200),
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_event_locations_orderid FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_event_locations_orderid
    ON t_sys_event_locations(c_orderid);

CREATE INDEX IF NOT EXISTS ix_event_locations_coordinates
    ON t_sys_event_locations(c_latitude, c_longitude);

    -- =============================================
    -- Migration Complete
    -- =============================================

