/*
 * Database Migration Script: Sample Tasting Orders
 * Purpose: Create t_sys_sample_orders and t_sys_sample_order_items
 *          following project column-prefix convention (c_ prefix)
 *          with mandatory c_createddate and c_modifieddate audit columns.
 * Date: 2026-03-14
 */

-- ============================================================
-- 1. t_sys_sample_orders — Main sample tasting order entity
-- ============================================================
CREATE TABLE IF NOT EXISTS t_sys_sample_orders (

    -- Primary Key
    c_sample_order_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

    -- Foreign Keys
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_delivery_address_id BIGINT NULL,

    -- Pricing
    c_sample_price_total DECIMAL(10,2) NOT NULL,
    c_delivery_charge DECIMAL(10,2) NOT NULL DEFAULT 0,
    c_total_amount DECIMAL(10,2) NOT NULL,

    -- Status
    c_status VARCHAR(50) NOT NULL DEFAULT 'SAMPLE_REQUESTED',
    c_payment_status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    c_is_paid BOOLEAN NOT NULL DEFAULT FALSE,

    -- Addresses & Coordinates
    c_pickup_address VARCHAR(500) NOT NULL,
    c_pickup_latitude DECIMAL(10,8) NULL,
    c_pickup_longitude DECIMAL(11,8) NULL,
    c_delivery_latitude DECIMAL(10,8) NULL,
    c_delivery_longitude DECIMAL(11,8) NULL,

    -- Payment Reference
    c_payment_id BIGINT NULL,
    c_payment_gateway_order_id VARCHAR(100) NULL,
    c_payment_gateway_transaction_id VARCHAR(100) NULL,

    -- Partner Response
    c_partner_response_date TIMESTAMP NULL,
    c_rejection_reason VARCHAR(500) NULL,

    -- Third-Party Delivery
    c_delivery_provider VARCHAR(50) NULL,
    c_delivery_partner_order_id VARCHAR(100) NULL,
    c_delivery_partner_name VARCHAR(200) NULL,
    c_delivery_partner_phone VARCHAR(20) NULL,
    c_delivery_vehicle_number VARCHAR(50) NULL,
    c_estimated_pickup_time TIMESTAMP NULL,
    c_actual_pickup_time TIMESTAMP NULL,
    c_estimated_delivery_time TIMESTAMP NULL,
    c_actual_delivery_time TIMESTAMP NULL,

    -- Customer Feedback & Conversion
    c_client_feedback VARCHAR(1000) NULL,
    c_taste_rating INTEGER NULL,
    c_hygiene_rating INTEGER NULL,
    c_overall_rating INTEGER NULL,
    c_feedback_date TIMESTAMP NULL,
    c_converted_to_event_order BOOLEAN NOT NULL DEFAULT FALSE,
    c_event_order_id BIGINT NULL,
    c_conversion_date TIMESTAMP NULL,

    -- Mandatory Audit Columns
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Soft Delete / Active
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Constraints
    CONSTRAINT fk_sample_orders_user FOREIGN KEY (c_userid)
        REFERENCES t_sys_user (c_userid),
    CONSTRAINT fk_sample_orders_catering FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner (c_ownerid),
    CONSTRAINT fk_sample_orders_delivery_addr FOREIGN KEY (c_delivery_address_id)
        REFERENCES t_sys_user_addresses (c_address_id),
    CONSTRAINT fk_sample_orders_event_order FOREIGN KEY (c_event_order_id)
        REFERENCES t_sys_orders (c_orderid),
    CONSTRAINT ck_sample_orders_status CHECK (c_status IN (
        'SAMPLE_REQUESTED','SAMPLE_ACCEPTED','SAMPLE_REJECTED',
        'SAMPLE_PREPARING','READY_FOR_PICKUP','IN_TRANSIT',
        'DELIVERED','REFUNDED'
    )),
    CONSTRAINT ck_sample_orders_taste_rating CHECK (c_taste_rating IS NULL OR c_taste_rating BETWEEN 1 AND 5),
    CONSTRAINT ck_sample_orders_hygiene_rating CHECK (c_hygiene_rating IS NULL OR c_hygiene_rating BETWEEN 1 AND 5),
    CONSTRAINT ck_sample_orders_overall_rating CHECK (c_overall_rating IS NULL OR c_overall_rating BETWEEN 1 AND 5)
);

CREATE INDEX IF NOT EXISTS ix_sample_orders_userid ON t_sys_sample_orders (c_userid);
CREATE INDEX IF NOT EXISTS ix_sample_orders_catering_id ON t_sys_sample_orders (c_ownerid);
CREATE INDEX IF NOT EXISTS ix_sample_orders_status ON t_sys_sample_orders (c_status);
CREATE INDEX IF NOT EXISTS ix_sample_orders_createddate ON t_sys_sample_orders (c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_sample_orders_payment_status ON t_sys_sample_orders (c_payment_status);

-- ============================================================
-- 2. t_sys_sample_order_items — Line items for sample orders
-- ============================================================
CREATE TABLE IF NOT EXISTS t_sys_sample_order_items (

    -- Primary Key
    c_sample_item_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

    -- Foreign Key
    c_sample_order_id BIGINT NOT NULL,
    c_menu_item_id BIGINT NOT NULL,

    -- Item Details (snapshot — not live-linked to t_sys_fooditems)
    c_menu_item_name VARCHAR(200) NOT NULL,
    c_sample_price DECIMAL(10,2) NOT NULL,
    c_sample_quantity INTEGER NOT NULL DEFAULT 1,
    c_category VARCHAR(100) NULL,
    c_description VARCHAR(500) NULL,
    c_image_url VARCHAR(500) NULL,
    c_cuisine_type VARCHAR(100) NULL,
    c_is_veg BOOLEAN NULL,

    -- Source Tracking
    c_is_from_package BOOLEAN NOT NULL DEFAULT FALSE,
    c_package_id BIGINT NULL,

    -- Mandatory Audit Columns
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT fk_sample_order_items_order FOREIGN KEY (c_sample_order_id)
        REFERENCES t_sys_sample_orders (c_sample_order_id) ON DELETE CASCADE,
    CONSTRAINT fk_sample_order_items_fooditem FOREIGN KEY (c_menu_item_id)
        REFERENCES t_sys_fooditems (c_foodid)
);

CREATE INDEX IF NOT EXISTS ix_sample_order_items_order_id ON t_sys_sample_order_items (c_sample_order_id);
CREATE INDEX IF NOT EXISTS ix_sample_order_items_menu_item_id ON t_sys_sample_order_items (c_menu_item_id);

