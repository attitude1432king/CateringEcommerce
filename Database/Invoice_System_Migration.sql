/* =============================================
   Invoice & Payment System Migration
   Version: 1.0.0
   Date: 2026-02-20
   Author: Tech Lead - CateringDB
   Description: Complete invoice, payment schedule, and audit system implementation
   ================================================ */

-- STEP 2: Create t_sys_invoice Table
CREATE TABLE IF NOT EXISTS t_sys_invoice (
    -- Primary Key
    c_invoice_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    -- Order & Event Reference
    c_orderid BIGINT NOT NULL,
    c_event_id BIGINT NULL,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,

    -- Invoice Type & Classification
    c_invoice_type INTEGER NOT NULL, -- 1=BOOKING, 2=PRE_EVENT, 3=FINAL
    c_is_proforma BOOLEAN NOT NULL DEFAULT FALSE, -- TRUE for booking invoice
    c_invoice_number VARCHAR(50) NOT NULL UNIQUE,
    c_invoice_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_due_date TIMESTAMP NULL,

    -- Financial Details
    c_subtotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_cgst_percent DECIMAL(5,2) NOT NULL DEFAULT 9.00,
    c_sgst_percent DECIMAL(5,2) NOT NULL DEFAULT 9.00,
    c_cgst_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_sgst_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_total_tax_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_total_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_amount_paid DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    c_balance_due DECIMAL(18,2) NOT NULL DEFAULT 0.00,

    -- Payment Stage Information
    c_payment_stage_type VARCHAR(20) NOT NULL, -- BOOKING/PRE_EVENT/FINAL
    c_payment_percentage DECIMAL(5,2) NOT NULL, -- 40.00/35.00/25.00

    -- Invoice Status
    c_status VARCHAR(20) NOT NULL DEFAULT 'UNPAID', -- DRAFT/UNPAID/PARTIALLY_PAID/PAID/OVERDUE/EXPIRED/CANCELLED

    -- Payment Gateway Integration
    c_razorpay_order_id VARCHAR(100) NULL,
    c_razorpay_payment_id VARCHAR(100) NULL,
    c_transaction_id VARCHAR(100) NULL,
    c_payment_method VARCHAR(50) NULL, -- Online/UPI/Card/NetBanking
    c_payment_date TIMESTAMP NULL,

    -- GST Compliance (India)
    c_company_gstin VARCHAR(15) NULL, -- Your company GSTIN
    c_customer_gstin VARCHAR(15) NULL, -- Customer GSTIN (if B2B)
    c_place_of_supply VARCHAR(100) NULL, -- State name for GST
    c_sac_code VARCHAR(20) NOT NULL DEFAULT '996331', -- Catering services SAC code

    -- Additional Information
    c_notes TEXT NULL,
    c_terms_and_conditions TEXT NULL,
    c_internal_remarks VARCHAR(500) NULL, -- Admin notes

    -- PDF Storage
    c_pdf_path VARCHAR(500) NULL,
    c_pdf_generated_date TIMESTAMP NULL,

    -- Audit Fields
    c_createdby BIGINT NULL,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifiedby BIGINT NULL,
    c_modifieddate TIMESTAMP NULL,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    c_deleted_date TIMESTAMP NULL,

    -- Version Control (for regeneration tracking)
    c_version INTEGER NOT NULL DEFAULT 1,
    c_parent_invoice_id BIGINT NULL, -- Reference to previous version if regenerated

    CONSTRAINT fk_invoice_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders (c_orderid),
    CONSTRAINT fk_invoice_user FOREIGN KEY (c_userid) REFERENCES t_sys_user (c_userid),
    CONSTRAINT fk_invoice_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner (c_ownerid),
    CONSTRAINT chk_invoice_type CHECK (c_invoice_type IN (1, 2, 3)),
    CONSTRAINT chk_invoice_status CHECK (c_status IN ('DRAFT', 'UNPAID', 'PARTIALLY_PAID', 'PAID', 'OVERDUE', 'EXPIRED', 'CANCELLED')),
    CONSTRAINT chk_invoice_stage_type CHECK (c_payment_stage_type IN ('BOOKING', 'PRE_EVENT', 'FINAL')),
    CONSTRAINT chk_invoice_percentage CHECK (c_payment_percentage IN (40.00, 35.00, 25.00)),
    CONSTRAINT chk_invoice_amounts CHECK (c_total_amount = c_subtotal + c_total_tax_amount - c_discount_amount),
    CONSTRAINT chk_invoice_balance CHECK (c_balance_due = c_total_amount - c_amount_paid)
);

-- STEP 3: Create t_sys_invoice_line_items Table
CREATE TABLE IF NOT EXISTS t_sys_invoice_line_items (
    -- Primary Key
    c_line_item_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_invoice_id BIGINT NOT NULL,

    -- Item Details
    c_item_type VARCHAR(30) NOT NULL, -- PACKAGE/FOOD_ITEM/DECORATION/EXTRA_GUEST/ADDON/OVERTIME/DELIVERY/OTHER
    c_item_id BIGINT NULL, -- Reference to actual item (package/food/decoration)
    c_description VARCHAR(500) NOT NULL,
    c_hsn_sac_code VARCHAR(20) NULL, -- HSN/SAC code for the item

    -- Quantity & Pricing
    c_quantity DECIMAL(10,2) NOT NULL DEFAULT 1,
    c_unit_of_measure VARCHAR(20) NULL, -- pcs, kg, plate, hours, etc.
    c_unit_price DECIMAL(18,2) NOT NULL,
    c_subtotal DECIMAL(18,2) NOT NULL, -- quantity * unit_price

    -- Tax Details
    c_tax_percent DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    c_cgst_percent DECIMAL(5,2) NOT NULL DEFAULT 9.00,
    c_sgst_percent DECIMAL(5,2) NOT NULL DEFAULT 9.00,
    c_tax_amount DECIMAL(18,2) NOT NULL,
    c_cgst_amount DECIMAL(18,2) NOT NULL,
    c_sgst_amount DECIMAL(18,2) NOT NULL,

    -- Discount (if applicable at line level)
    c_discount_percent DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    c_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,

    -- Total
    c_total DECIMAL(18,2) NOT NULL, -- subtotal + tax - discount

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_sequence INTEGER NOT NULL DEFAULT 1, -- Display order

    CONSTRAINT fk_line_item_invoice FOREIGN KEY (c_invoice_id) REFERENCES t_sys_invoice (c_invoice_id) ON DELETE CASCADE,
    CONSTRAINT chk_line_item_type CHECK (c_item_type IN ('PACKAGE', 'FOOD_ITEM', 'DECORATION', 'EXTRA_GUEST', 'ADDON', 'OVERTIME', 'DELIVERY', 'STAFF', 'OTHER')),
    CONSTRAINT chk_line_item_quantity CHECK (c_quantity > 0),
    CONSTRAINT chk_line_item_subtotal CHECK (c_subtotal = c_quantity * c_unit_price),
    CONSTRAINT chk_line_item_total CHECK (c_total = c_subtotal + c_tax_amount - c_discount_amount)
);

-- STEP 4: Create t_sys_payment_schedule Table
CREATE TABLE IF NOT EXISTS t_sys_payment_schedule (
    -- Primary Key
    c_schedule_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,

    -- Stage Information
    c_stage_type VARCHAR(20) NOT NULL, -- BOOKING/PRE_EVENT/FINAL
    c_stage_sequence INTEGER NOT NULL, -- 1, 2, 3
    c_percentage DECIMAL(5,2) NOT NULL, -- 40.00, 35.00, 25.00
    c_amount DECIMAL(18,2) NOT NULL,

    -- Timeline
    c_due_date TIMESTAMP NULL,
    c_trigger_event VARCHAR(50) NOT NULL, -- ORDER_APPROVED/GUEST_LOCK_DATE/EVENT_COMPLETED
    c_auto_generate_date TIMESTAMP NULL, -- When invoice should auto-generate

    -- Invoice Link
    c_invoice_id BIGINT NULL, -- Links to generated invoice

    -- Status
    c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING/PAID/OVERDUE/CANCELLED

    -- Reminder Tracking
    c_reminder_sent_count INTEGER NOT NULL DEFAULT 0,
    c_last_reminder_date TIMESTAMP NULL,
    c_next_reminder_date TIMESTAMP NULL,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL,

    CONSTRAINT fk_schedule_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders (c_orderid) ON DELETE CASCADE,
    CONSTRAINT fk_schedule_invoice FOREIGN KEY (c_invoice_id) REFERENCES t_sys_invoice (c_invoice_id),
    CONSTRAINT chk_schedule_stage_type CHECK (c_stage_type IN ('BOOKING', 'PRE_EVENT', 'FINAL')),
    CONSTRAINT chk_schedule_sequence CHECK (c_stage_sequence IN (1, 2, 3)),
    CONSTRAINT chk_schedule_percentage CHECK (c_percentage IN (40.00, 35.00, 25.00)),
    CONSTRAINT chk_schedule_status CHECK (c_status IN ('PENDING', 'PAID', 'OVERDUE', 'CANCELLED')),
    CONSTRAINT uq_schedule_order_stage UNIQUE (c_orderid, c_stage_type)
);

-- STEP 5: Create t_sys_invoice_audit_log Table
CREATE TABLE IF NOT EXISTS t_sys_invoice_audit_log (
    c_audit_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_invoice_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    -- Action Details
    c_action VARCHAR(50) NOT NULL, -- GENERATED/VIEWED/DOWNLOADED/PAID/CANCELLED/REGENERATED/STATUS_CHANGED
    c_performed_by BIGINT NULL, -- User/Admin ID
    c_performed_by_type VARCHAR(20) NULL, -- USER/ADMIN/OWNER/SYSTEM

    -- Status Change Tracking
    c_old_status VARCHAR(20) NULL,
    c_new_status VARCHAR(20) NULL,
    c_old_amount_paid DECIMAL(18,2) NULL,
    c_new_amount_paid DECIMAL(18,2) NULL,

    -- Request Information
    c_ip_address VARCHAR(50) NULL,
    c_user_agent VARCHAR(500) NULL,

    -- Additional Context
    c_remarks VARCHAR(1000) NULL,
    c_metadata TEXT NULL, -- JSON for additional data

    -- Timestamp
    c_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_audit_invoice FOREIGN KEY (c_invoice_id) REFERENCES t_sys_invoice (c_invoice_id),
    CONSTRAINT fk_audit_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders (c_orderid)
);

-- STEP 6: Alter t_sys_orders Table (Add New Columns)
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_guest_lock_date TIMESTAMP NULL;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_menu_lock_date TIMESTAMP NULL;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_guest_locked BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_menu_locked BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_original_guest_count INTEGER NULL;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_final_guest_count INTEGER NULL;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_extra_charges DECIMAL(18,2) NOT NULL DEFAULT 0.00;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_total_paid_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00;
ALTER TABLE t_sys_orders ADD COLUMN IF NOT EXISTS c_payment_progress_percentage DECIMAL(5,2) NOT NULL DEFAULT 0.00;

-- STEP 7: Create Indexes for Performance
CREATE INDEX IF NOT EXISTS ix_invoice_order_id ON t_sys_invoice (c_orderid);
CREATE INDEX IF NOT EXISTS ix_invoice_type ON t_sys_invoice (c_invoice_type);
CREATE INDEX IF NOT EXISTS ix_invoice_status ON t_sys_invoice (c_status);
CREATE INDEX IF NOT EXISTS ix_invoice_user_id ON t_sys_invoice (c_userid);
CREATE INDEX IF NOT EXISTS ix_invoice_owner_id ON t_sys_invoice (c_ownerid);
CREATE INDEX IF NOT EXISTS ix_invoice_razorpay_order ON t_sys_invoice (c_razorpay_order_id);
CREATE INDEX IF NOT EXISTS ix_invoice_due_date ON t_sys_invoice (c_due_date) WHERE c_status IN ('UNPAID', 'OVERDUE');

CREATE INDEX IF NOT EXISTS ix_schedule_order_id ON t_sys_payment_schedule (c_orderid);
CREATE INDEX IF NOT EXISTS ix_schedule_due_date ON t_sys_payment_schedule (c_due_date) WHERE c_status = 'PENDING';
CREATE INDEX IF NOT EXISTS ix_schedule_auto_generate ON t_sys_payment_schedule (c_auto_generate_date) WHERE c_invoice_id IS NULL AND c_status = 'PENDING';

CREATE INDEX IF NOT EXISTS ix_line_items_invoice_id ON t_sys_invoice_line_items (c_invoice_id);

CREATE INDEX IF NOT EXISTS ix_audit_invoice_id ON t_sys_invoice_audit_log (c_invoice_id, c_timestamp DESC);
CREATE INDEX IF NOT EXISTS ix_audit_action ON t_sys_invoice_audit_log (c_action, c_timestamp DESC);

-- =============================================
-- STEP 8: Create Helper Function for Invoice Number Generation
-- =============================================

CREATE OR REPLACE FUNCTION fn_GenerateInvoiceNumber()
RETURNS VARCHAR(50)
LANGUAGE plpgsql
AS $$
DECLARE
    v_Prefix     VARCHAR(10) := 'INV';
    v_DatePart   VARCHAR(8);
    v_Sequence   INTEGER;
BEGIN
    -- Format date as YYYYMMDD
    v_DatePart := TO_CHAR(NOW(), 'YYYYMMDD');

    -- Get next sequence number for today
    SELECT COALESCE(MAX(CAST(RIGHT(c_invoice_number, 5) AS INTEGER)), 0) + 1
    INTO v_Sequence
    FROM t_sys_invoice
    WHERE c_invoice_number LIKE v_Prefix || '-' || v_DatePart || '%';

    -- Return formatted invoice number
    RETURN v_Prefix || '-' || v_DatePart || '-' || LPAD(v_Sequence::TEXT, 5, '0');
END;
$$;
