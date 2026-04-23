-- =============================================
-- Order Management Tables
-- PostgreSQL Compatible Version
-- Created: 2026-01-15
-- Description: Creates tables for order management, payments, and order history
-- =============================================

-- =============================================
-- Table: t_sys_orders
-- Description: Main orders table storing order details
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_orders (
    c_orderid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_order_number VARCHAR(50) NOT NULL UNIQUE,
    c_event_date TIMESTAMP NOT NULL,
    c_event_time VARCHAR(20) NOT NULL,
    c_event_type VARCHAR(100) NOT NULL,
    c_event_location VARCHAR(500) NOT NULL,
    c_guest_count INTEGER NOT NULL,
    c_special_instructions VARCHAR(1000),
    c_delivery_address VARCHAR(500) NOT NULL,
    c_contact_person VARCHAR(100) NOT NULL,
    c_contact_phone VARCHAR(20) NOT NULL,
    c_contact_email VARCHAR(100) NOT NULL,
    c_base_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    c_tax_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    c_delivery_charges DECIMAL(18, 2) NOT NULL DEFAULT 0,
    c_discount_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    c_total_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    c_payment_method VARCHAR(50) NOT NULL,
    c_payment_split_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    c_paymentmode VARCHAR(50) DEFAULT 'SPLIT',
    c_escrowstatus VARCHAR(50) DEFAULT 'PENDING',
    c_advanceamount DECIMAL(18,2),
    c_advancepercentage DECIMAL(5,2) DEFAULT 30.00,
    c_prebooking_amount DECIMAL(18, 2),
    c_postevent_amount DECIMAL(18, 2),
    c_prebooking_status VARCHAR(20),
    c_postevent_status VARCHAR(20),
    c_event_latitude DECIMAL(10, 7),
    c_event_longitude DECIMAL(10, 7),
    c_event_place_id VARCHAR(200),
    c_saved_address_id BIGINT,
    c_decoration_id BIGINT,
    c_original_guest_count INTEGER NULL,
    c_locked_guest_count INTEGER NULL,
    c_guest_count_locked BOOLEAN DEFAULT FALSE,
    c_guest_lock_date TIMESTAMP,    
    c_final_guest_count INTEGER NULL,
    c_menu_locked BOOLEAN DEFAULT FALSE,
    c_menu_lock_date TIMESTAMP,
    c_platform_commission DECIMAL(18,2) NOT NULL DEFAULT 0,
    c_commission_rate DECIMAL(5,2) NOT NULL DEFAULT 10.00,
    c_extra_charges DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_total_paid_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_payment_progress_percentage DECIMAL(5,2) NOT NULL DEFAULT 0.00
    c_payment_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_order_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,
    c_isactive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_orders_user FOREIGN KEY(c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_orders_catering FOREIGN KEY(c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT fk_orders_decoration FOREIGN KEY(c_decoration_id) REFERENCES t_sys_catering_decorations(c_decoration_id)
);

CREATE INDEX IF NOT EXISTS ix_orders_userid ON t_sys_orders(c_userid);
CREATE INDEX IF NOT EXISTS ix_orders_ownerid ON t_sys_orders(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_orders_status ON t_sys_orders(c_order_status);

-- =============================================
-- Table: t_sys_order_items
-- Description: Stores individual order items (packages, food items, decorations)
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_order_items (
    c_order_item_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_item_type VARCHAR(20) NOT NULL,
    c_item_id BIGINT NOT NULL,
    c_item_name VARCHAR(200) NOT NULL,
    c_quantity INTEGER NOT NULL DEFAULT 1,
    c_unit_price DECIMAL(18, 2) NOT NULL,
    c_total_price DECIMAL(18, 2) NOT NULL,
    c_package_selections TEXT,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_order_items_order FOREIGN KEY(c_orderid) REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_order_items_orderid ON t_sys_order_items(c_orderid);

-- =============================================
-- Table: t_sys_order_status_history
-- Description: Tracks order status changes with timestamps
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_order_status_history (
    c_history_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_status VARCHAR(20) NOT NULL,
    c_remarks VARCHAR(500),
    c_updated_by BIGINT,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_order_status_history_order FOREIGN KEY(c_orderid) REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_order_status_history_orderid ON t_sys_order_status_history(c_orderid);

-- =============================================
-- Table: t_sys_order_payments
-- Description: Stores payment details including payment proofs
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_order_payments (
        c_payment_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
        c_orderid BIGINT NOT NULL,
        c_payment_method VARCHAR(50) NOT NULL,
        c_payment_gateway VARCHAR(50),
        c_transaction_id VARCHAR(100),
        c_payment_stage_type VARCHAR(20),
        c_razorpay_order_id VARCHAR(100),
        c_razorpay_payment_id VARCHAR(100),
        c_upi_id VARCHAR(100),
        c_payment_proof_path VARCHAR(500),
        c_amount DECIMAL(18, 2) NOT NULL,
        c_paid_amount DECIMAL(18, 2),
        c_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
        c_payment_date TIMESTAMP,
        c_verified_by BIGINT,
        c_verified_date TIMESTAMP,
        c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
        CONSTRAINT fk_order_payments_order FOREIGN KEY(c_orderid) REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS ix_order_payments_orderid ON t_sys_order_payments(c_orderid);
    CREATE INDEX IF NOT EXISTS ix_order_payments_status ON t_sys_order_payments(c_status);

    -- =============================================
    -- Additional Indexes for Performance
    -- =============================================
    CREATE INDEX IF NOT EXISTS ix_orders_event_date ON t_sys_orders(c_event_date DESC);
    CREATE INDEX IF NOT EXISTS ix_orders_createddate ON t_sys_orders(c_createddate DESC);
    CREATE INDEX IF NOT EXISTS ix_orders_payment_status ON t_sys_orders(c_payment_status);

    -- =============================================
    -- Order Management Tables Created Successfully
    -- =============================================
    -- Tables created:
    --   - t_sys_orders
    --   - t_sys_order_items
    --   - t_sys_order_status_history
    --   - t_sys_order_payments
    -- Indexes created for optimal query performance.

