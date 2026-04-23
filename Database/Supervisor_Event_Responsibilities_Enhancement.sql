-- =============================================
-- Event Supervisor Responsibilities - Enhanced Schema
-- Pre-Event, During-Event, Post-Event Tracking
-- =============================================
-- =============================================
-- 4. CREATE PRE-EVENT VERIFICATION CHECKLIST TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_pre_event_checklist (
    c_checklist_id BIGSERIAL PRIMARY KEY,

    c_assignment_id BIGINT NOT NULL,
    c_supervisor_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    -- Menu Verification
    c_menu_items_received TEXT NULL,
    c_menu_items_verified TEXT NULL,
    c_missing_items TEXT NULL,
    c_substituted_items TEXT NULL,

    -- Quantity Verification
    c_expected_portions INTEGER NULL,
    c_verified_portions INTEGER NULL,
    c_portion_variance_acceptable BOOLEAN NULL,

    -- Quality Verification
    c_freshness_check_done BOOLEAN NOT NULL DEFAULT FALSE,
    c_hygiene_check_done BOOLEAN NOT NULL DEFAULT FALSE,
    c_packaging_check_done BOOLEAN NOT NULL DEFAULT FALSE,
    c_temperature_check_done BOOLEAN NOT NULL DEFAULT FALSE,
    c_quality_issues_found BOOLEAN NOT NULL DEFAULT FALSE,
    c_quality_issues_details VARCHAR(1000) NULL,

    -- Photos & Evidence
    c_checklist_photos TEXT NULL,

    -- Timestamp
    c_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Sign-off
    c_supervisor_signed_off BOOLEAN NOT NULL DEFAULT FALSE,
    c_vendor_signed_off BOOLEAN NOT NULL DEFAULT FALSE,
    c_vendor_signature_url VARCHAR(500) NULL,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Foreign Keys
    CONSTRAINT fk_checklist_assignment
        FOREIGN KEY (c_assignment_id)
        REFERENCES t_sys_supervisor_assignment(c_assignment_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_checklist_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT fk_checklist_order
        FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid)
);

-- Index
CREATE INDEX IF NOT EXISTS idx_pre_event_assignment
ON t_sys_pre_event_checklist(c_assignment_id);

-- =============================================
-- 5. CREATE DURING-EVENT TRACKING TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_during_event_tracking (
    c_tracking_id BIGSERIAL PRIMARY KEY,
    c_assignment_id BIGINT NOT NULL,
    c_supervisor_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    -- Tracking Type
    c_tracking_type VARCHAR(30) NOT NULL CHECK (
        c_tracking_type IN (
            'GUEST_COUNT_UPDATE', 'FOOD_SERVING_CHECK', 'EXTRA_QUANTITY_REQUEST',
            'CLIENT_APPROVAL', 'ISSUE_REPORTED', 'QUALITY_CHECK'
        )
    ),

    -- Details
    c_tracking_description TEXT NOT NULL,
    c_tracking_data TEXT NULL, -- JSON payload

    -- Guest Count
    c_guest_count INT NULL,
    c_guest_count_variance INT NULL,

    -- Extra Quantity
    c_extra_item_name VARCHAR(200) NULL,
    c_extra_quantity INT NULL,
    c_extra_cost DECIMAL(18,2) NULL,
    c_extra_reason VARCHAR(500) NULL,

    -- Client Approval
    c_approval_method VARCHAR(20) NULL,
    c_otp_code VARCHAR(10) NULL,
    c_otp_sent_time TIMESTAMP NULL,
    c_otp_verified BOOLEAN NULL,
    c_approval_status VARCHAR(20) NULL,
    c_approval_timestamp TIMESTAMP NULL,

    -- Evidence
    c_evidence_urls TEXT NULL,
    c_gps_location VARCHAR(100) NULL,

    -- Timestamp
    c_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
    FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_tracking_assignment ON t_sys_during_event_tracking(c_assignment_id);
CREATE INDEX IF NOT EXISTS idx_tracking_type ON t_sys_during_event_tracking(c_tracking_type);
CREATE INDEX IF NOT EXISTS idx_tracking_timestamp ON t_sys_during_event_tracking(c_timestamp);

-- =============================================
-- 6. CREATE POST-EVENT COMPLETION REPORT TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_post_event_report (
    c_report_id BIGSERIAL PRIMARY KEY,
    c_assignment_id BIGINT NOT NULL,
    c_supervisor_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    -- Event Summary
    c_event_started_time TIMESTAMP NULL,
    c_event_ended_time TIMESTAMP NULL,
    c_event_duration_minutes INT NULL,
    c_final_guest_count INT NULL,
    c_event_rating INT NULL,

    -- Client Feedback
    c_client_name VARCHAR(100) NULL,
    c_client_phone VARCHAR(15) NULL,
    c_client_email VARCHAR(100) NULL,
    c_client_satisfaction_rating INT NULL,
    c_food_quality_rating INT NULL,
    c_food_quantity_rating INT NULL,
    c_service_quality_rating INT NULL,
    c_presentation_rating INT NULL,
    c_value_for_money_rating INT NULL,
    c_would_recommend BOOLEAN NULL,
    c_client_comments TEXT NULL,
    c_client_signature_url VARCHAR(500) NULL,

    -- Vendor Performance
    c_vendor_punctuality_rating INT NULL,
    c_vendor_preparation_rating INT NULL,
    c_vendor_cooperation_rating INT NULL,
    c_vendor_hygiene_rating INT NULL,
    c_vendor_comments TEXT NULL,

    -- Issues Summary
    c_total_issues_count INT NOT NULL DEFAULT 0,
    c_critical_issues_count INT NOT NULL DEFAULT 0,
    c_major_issues_count INT NOT NULL DEFAULT 0,
    c_minor_issues_count INT NOT NULL DEFAULT 0,
    c_issues_summary TEXT NULL,
    c_all_issues_resolved BOOLEAN NULL,

    -- Financial Summary
    c_base_order_amount DECIMAL(18,2) NULL,
    c_extra_charges_amount DECIMAL(18,2) NULL DEFAULT 0,
    c_deductions_amount DECIMAL(18,2) NULL DEFAULT 0,
    c_final_payable_amount DECIMAL(18,2) NULL,
    c_payment_breakdown TEXT NULL,

    -- Completion Evidence
    c_completion_photos TEXT NULL,
    c_completion_videos TEXT NULL,
    c_waste_photos TEXT NULL,
    c_leftover_notes TEXT NULL,

    -- Report Details
    c_report_summary TEXT NOT NULL,
    c_recommendations TEXT NULL,
    c_supervisor_notes TEXT NULL,
    c_report_pdf_url VARCHAR(500) NULL,

    -- Verification
    c_report_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_verified_by BIGINT NULL,
    c_verification_date TIMESTAMP NULL,
    c_verification_notes VARCHAR(500) NULL,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_submitted_date TIMESTAMP NULL,

    FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
    FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_post_event_assignment ON t_sys_post_event_report(c_assignment_id);
CREATE INDEX IF NOT EXISTS idx_post_event_created ON t_sys_post_event_report(c_createddate);

-- =============================================
-- 7. CREATE CLIENT OTP VERIFICATION TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_client_otp_verification (
    c_otp_id BIGSERIAL PRIMARY KEY,

    c_assignment_id BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,
    c_supervisor_id BIGINT NOT NULL,

    -- OTP Details
    c_otp_code VARCHAR(10) NOT NULL,
    c_otp_purpose VARCHAR(50) NOT NULL,
    c_otp_sent_to VARCHAR(15) NOT NULL,
    c_otp_sent_time TIMESTAMP NOT NULL DEFAULT NOW(),
    c_otp_expires_at TIMESTAMP NOT NULL,

    -- Verification
    c_otp_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_otp_verified_time TIMESTAMP NULL,
    c_verification_attempts INT NOT NULL DEFAULT 0,
    c_max_attempts INT NOT NULL DEFAULT 3,

    -- Context Data
    c_context_data TEXT NULL,
    c_client_ip_address VARCHAR(50) NULL,

    -- Status
    c_status VARCHAR(20) NOT NULL DEFAULT 'SENT',

    -- Constraints
    CONSTRAINT chk_otp_status
        CHECK (c_status IN ('SENT', 'VERIFIED', 'EXPIRED', 'FAILED')),

    CONSTRAINT fk_otp_assignment
        FOREIGN KEY (c_assignment_id)
        REFERENCES t_sys_supervisor_assignment(c_assignment_id),

    CONSTRAINT fk_otp_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT fk_otp_order
        FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_otp_code
ON t_sys_client_otp_verification(c_otp_code);

CREATE INDEX IF NOT EXISTS idx_otp_assignment
ON t_sys_client_otp_verification(c_assignment_id);

CREATE INDEX IF NOT EXISTS idx_otp_status
ON t_sys_client_otp_verification(c_status);