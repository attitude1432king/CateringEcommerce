-- =============================================
-- Financial Strategy Implementation
-- Date: 2026-01-30
-- Purpose: Implement all missing components from the financial strategy document
-- =============================================


-- =============================================
-- SECTION 1: Order Modifications Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_order_modifications (
    c_modification_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_modification_type VARCHAR(50) NOT NULL,

    -- Guest Count Changes
    c_original_guest_count BIGINT,
    c_modified_guest_count BIGINT,
    c_guest_count_change BIGINT,

    -- Menu Changes
    c_menu_change_details TEXT,

    -- Financial Impact
    c_original_amount DECIMAL(18,2),
    c_additional_amount DECIMAL(18,2) NOT NULL,
    c_pricing_multiplier DECIMAL(5,2) DEFAULT 1.00,

    -- Request Details
    c_modification_reason VARCHAR(500) NOT NULL,
    c_requested_by BIGINT NOT NULL,
    c_requested_by_type VARCHAR(20) NOT NULL,
    c_request_date TIMESTAMP DEFAULT NOW(),

    -- Approval Workflow
    c_requires_approval BOOLEAN DEFAULT TRUE,
    c_approved_by BIGINT,
    c_approved_by_type VARCHAR(20),
    c_approval_date TIMESTAMP,
    c_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_rejection_reason VARCHAR(500),

    -- Payment Linkage
    c_payment_collected BOOLEAN DEFAULT FALSE,
    c_payment_transaction_id BIGINT,

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_order_modifications_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid) ON DELETE CASCADE,
    CONSTRAINT ck_order_modifications_type CHECK (c_modification_type IN ('GUEST_COUNT_INCREASE', 'GUEST_COUNT_DECREASE', 'MENU_CHANGE', 'SERVICE_EXTENSION', 'DECORATION_UPGRADE', 'OTHER')),
    CONSTRAINT ck_order_modifications_requested_type CHECK (c_requested_by_type IN ('CUSTOMER', 'PARTNER', 'ADMIN')),
    CONSTRAINT ck_order_modifications_approved_type CHECK (c_approved_by_type IS NULL OR c_approved_by_type IN ('CUSTOMER', 'PARTNER', 'ADMIN')),
    CONSTRAINT ck_order_modifications_status CHECK (c_status IN ('Pending', 'Approved', 'Rejected', 'Paid', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS ix_order_modifications_order ON t_sys_order_modifications(c_orderid);
CREATE INDEX IF NOT EXISTS ix_order_modifications_status ON t_sys_order_modifications(c_status, c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_order_modifications_type ON t_sys_order_modifications(c_modification_type);

-- =============================================
-- SECTION 3: Cancellation & Refund Policy Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_cancellation_requests (
    c_cancellation_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,

    -- Timing Analysis
    c_event_date TIMESTAMP NOT NULL,
    c_cancellation_request_date TIMESTAMP DEFAULT NOW(),
    c_hours_before_event INTEGER NOT NULL,
    c_days_before_event INTEGER NOT NULL,

    -- Policy Applied
    c_policy_tier VARCHAR(20) NOT NULL,
    c_refund_percentage DECIMAL(5,2) NOT NULL,

    -- Financial Breakdown
    c_order_total_amount DECIMAL(18,2) NOT NULL,
    c_advance_paid DECIMAL(18,2) NOT NULL,
    c_refund_amount DECIMAL(18,2) NOT NULL,
    c_retention_amount DECIMAL(18,2) NOT NULL,
    c_platform_commission_forfeited DECIMAL(18,2) DEFAULT 0,

    -- Partner Compensation
    c_partner_compensation DECIMAL(18,2) DEFAULT 0,

    -- Reason & Evidence
    c_cancellation_reason VARCHAR(1000) NOT NULL,
    c_is_force_majeure BOOLEAN DEFAULT FALSE,
    c_force_majeure_evidence TEXT,

    -- Approval & Processing
    c_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_admin_approved_by BIGINT,
    c_admin_approval_date TIMESTAMP,
    c_admin_notes VARCHAR(1000),
    c_partner_response VARCHAR(1000),
    c_partner_response_date TIMESTAMP,

    -- Refund Processing
    c_refund_initiated_date TIMESTAMP,
    c_refund_completed_date TIMESTAMP,
    c_refund_transaction_id VARCHAR(200),
    c_refund_method VARCHAR(50),

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_cancellation_requests_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_cancellation_requests_user FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_cancellation_requests_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT ck_cancellation_requests_policy_tier CHECK (c_policy_tier IN ('FULL_REFUND', 'PARTIAL_REFUND', 'NO_REFUND', 'FORCE_MAJEURE')),
    CONSTRAINT ck_cancellation_requests_status CHECK (c_status IN ('Pending', 'Approved', 'Rejected', 'Refunded', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS ix_cancellation_requests_order ON t_sys_cancellation_requests(c_orderid);
CREATE INDEX IF NOT EXISTS ix_cancellation_requests_status ON t_sys_cancellation_requests(c_status, c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_cancellation_requests_policy_tier ON t_sys_cancellation_requests(c_policy_tier);

-- =============================================
-- SECTION 4: Complaint & Dispute Management
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_order_complaints (
    c_complaint_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,

    -- Complaint Type & Category
    c_complaint_type VARCHAR(50) NOT NULL,
    c_severity VARCHAR(20) NOT NULL,
    c_complaint_summary VARCHAR(200) NOT NULL,
    c_complaint_details TEXT NOT NULL,

    -- Specific Issue Details
    c_affected_items TEXT,
    c_affected_item_count INTEGER DEFAULT 0,
    c_total_item_count INTEGER DEFAULT 0,
    c_guest_complaint_count INTEGER DEFAULT 0,
    c_total_guest_count INTEGER NOT NULL,

    -- Evidence
    c_photo_evidence_paths TEXT,
    c_video_evidence_paths TEXT,
    c_witness_statements TEXT,
    c_timestamp_evidence TEXT,

    -- Timing
    c_issue_occurred_at TIMESTAMP,
    c_reported_at TIMESTAMP DEFAULT NOW(),
    c_is_reported_during_event BOOLEAN DEFAULT FALSE,

    -- Partner Response
    c_partner_notified_date TIMESTAMP,
    c_partner_response TEXT,
    c_partner_response_date TIMESTAMP,
    c_partner_admitted_fault BOOLEAN,
    c_partner_offered_replacement BOOLEAN DEFAULT FALSE,
    c_partner_provided_replacement BOOLEAN DEFAULT FALSE,

    -- Resolution
    c_status VARCHAR(20) NOT NULL DEFAULT 'Open',
    c_resolution_type VARCHAR(50),
    c_refund_percentage DECIMAL(5,2) DEFAULT 0,
    c_refund_amount DECIMAL(18,2) DEFAULT 0,
    c_goodwill_credit DECIMAL(18,2) DEFAULT 0,

    -- Validity Assessment
    c_is_valid_complaint BOOLEAN,
    c_validity_reason VARCHAR(500),
    c_severity_factor DECIMAL(3,2) DEFAULT 1.0,

    -- Admin Review
    c_reviewed_by BIGINT,
    c_reviewed_date TIMESTAMP,
    c_admin_notes TEXT,
    c_resolution_notes TEXT,
    c_resolved_date TIMESTAMP,

    -- Fraud Detection
    c_is_flagged_suspicious BOOLEAN DEFAULT FALSE,
    c_customer_complaint_history_count INTEGER DEFAULT 0,

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_order_complaints_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_order_complaints_user FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_order_complaints_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT ck_order_complaints_type CHECK (c_complaint_type IN ('FOOD_COLD', 'FOOD_QUALITY', 'QUANTITY_SHORT', 'LATE_ARRIVAL', 'PARTIAL_ISSUE', 'SETUP_POOR', 'NO_SHOW', 'PARTNER_NO_SHOW', 'OTHER')),
    CONSTRAINT ck_order_complaints_severity CHECK (c_severity IN ('CRITICAL', 'MAJOR', 'MINOR')),
    CONSTRAINT ck_order_complaints_status CHECK (c_status IN ('Open', 'Under_Investigation', 'Resolved', 'Rejected', 'Escalated')),
    CONSTRAINT ck_order_complaints_resolution_type CHECK (c_resolution_type IS NULL OR c_resolution_type IN ('FULL_REFUND', 'PARTIAL_REFUND', 'REPLACEMENT', 'GOODWILL_CREDIT', 'NO_RESOLUTION'))
);

CREATE INDEX IF NOT EXISTS ix_order_complaints_order ON t_sys_order_complaints(c_orderid);
CREATE INDEX IF NOT EXISTS ix_order_complaints_status ON t_sys_order_complaints(c_status, c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_order_complaints_type ON t_sys_order_complaints(c_complaint_type);
CREATE INDEX IF NOT EXISTS ix_order_complaints_severity ON t_sys_order_complaints(c_severity);
CREATE INDEX IF NOT EXISTS ix_order_complaints_user ON t_sys_order_complaints(c_userid);

-- =============================================
-- SECTION 5: Partner Security Deposit
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_partner_security_deposits (
    c_deposit_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL UNIQUE,

    -- Deposit Details
    c_deposit_amount DECIMAL(18,2) NOT NULL DEFAULT 25000.00,
    c_deposit_paid BOOLEAN DEFAULT FALSE,
    c_deposit_paid_date TIMESTAMP,
    c_payment_method VARCHAR(50),
    c_transaction_id VARCHAR(200),

    -- Current Balance
    c_current_balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    c_holds_amount DECIMAL(18,2) DEFAULT 0,
    c_available_balance DECIMAL(18,2) DEFAULT 0,

    -- Deposit Status
    c_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    c_is_active BOOLEAN DEFAULT FALSE,

    -- Refund Details
    c_refund_requested BOOLEAN DEFAULT FALSE,
    c_refund_request_date TIMESTAMP,
    c_refund_approved BOOLEAN DEFAULT FALSE,
    c_refund_processed_date TIMESTAMP,
    c_refund_amount DECIMAL(18,2),
    c_refund_transaction_id VARCHAR(200),

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_partner_security_deposits_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT ck_partner_security_deposits_status CHECK (c_status IN ('Pending', 'Active', 'Depleted', 'Refunded'))
);

CREATE INDEX IF NOT EXISTS ix_partner_security_deposits_status ON t_sys_partner_security_deposits(c_status);

-- =============================================
-- SECTION 6: Security Deposit Transactions
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_deposit_transactions (
    c_transaction_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_deposit_id BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_orderid BIGINT, -- Null for deposit payment/refund

    -- Transaction Type
    c_transaction_type VARCHAR(50) NOT NULL, -- 'DEPOSIT', 'DEDUCTION', 'REFUND', 'HOLD', 'RELEASE_HOLD', 'TOP_UP'
    c_amount DECIMAL(18,2) NOT NULL,
    c_balance_before DECIMAL(18,2) NOT NULL,
    c_balance_after DECIMAL(18,2) NOT NULL,

    -- Reason & Reference
    c_reason VARCHAR(500) NOT NULL,
    c_reference_type VARCHAR(50), -- 'PARTNER_NO_SHOW', 'COMPLAINT_REFUND', 'CANCELLATION_COMPENSATION', 'INITIAL_DEPOSIT', 'REFUND_TO_PARTNER'
    c_reference_id BIGINT, -- Complaint ID, Cancellation ID, etc.

    -- Approval (for deductions)
    c_approved_by BIGINT,
    c_approval_date TIMESTAMP,

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_deposit_transactions_deposit FOREIGN KEY (c_deposit_id) REFERENCES t_sys_partner_security_deposits(c_deposit_id),
    CONSTRAINT fk_deposit_transactions_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT fk_deposit_transactions_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT ck_deposit_transactions_type CHECK (c_transaction_type IN ('DEPOSIT', 'DEDUCTION', 'REFUND', 'HOLD', 'RELEASE_HOLD', 'TOP_UP'))
);

CREATE INDEX IF NOT EXISTS ix_deposit_transactions_deposit ON t_sys_deposit_transactions(c_deposit_id, c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_deposit_transactions_owner ON t_sys_deposit_transactions(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_deposit_transactions_order ON t_sys_deposit_transactions(c_orderid) WHERE c_orderid IS NOT NULL;

-- =============================================
-- SECTION 7: Commission Tier Tracking (Partner Partnership Tiers)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_partnership_tiers (
    c_tier_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    -- Current Tier
    c_tier_name VARCHAR(50) NOT NULL, -- 'FOUNDER_PARTNER', 'LAUNCH_PARTNER', 'EARLY_ADOPTER', 'STANDARD', 'PREMIUM'
    c_current_commission_rate DECIMAL(5,2) NOT NULL,

    -- Tier Assignment
    c_tier_start_date DATE NOT NULL,
    c_tier_lock_end_date DATE, -- Lock-in period expiry
    c_is_lock_period_active BOOLEAN DEFAULT TRUE,
    c_days_remaining_in_lock INTEGER DEFAULT 0,

    -- Qualification Criteria
    c_joining_date DATE NOT NULL,
    c_joining_order_number INTEGER NOT NULL, -- 1-20 = Founder, 21-100 = Launch, etc.
    c_required_orders_for_lock INTEGER NOT NULL, -- 5 for Founder, 3 for Launch, etc.
    c_completed_orders_count INTEGER DEFAULT 0,
    c_lock_qualified BOOLEAN DEFAULT FALSE,
    c_lock_qualified_date DATE,

    -- Performance-Based Commission Adjustment
    c_monthly_order_count INTEGER DEFAULT 0,
    c_average_rating DECIMAL(3,2) DEFAULT 0,
    c_qualifies_for_reduced_commission BOOLEAN DEFAULT FALSE,
    c_performance_commission_rate DECIMAL(5,2), -- e.g., 10% for high performers

    -- Next Tier Transition
    c_next_tier_name VARCHAR(50),
    c_next_tier_commission_rate DECIMAL(5,2),
    c_next_tier_effective_date DATE,
    c_transition_notice_sent BOOLEAN DEFAULT FALSE,
    c_transition_notice_sent_date DATE,

    -- Badges & Benefits
    c_has_founder_badge BOOLEAN DEFAULT FALSE,
    c_has_featured_listing BOOLEAN DEFAULT FALSE,
    c_has_priority_support BOOLEAN DEFAULT FALSE,
    c_has_account_manager BOOLEAN DEFAULT FALSE,

    -- Audit
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_partnership_tiers_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT ck_partnership_tiers_tier_name CHECK (c_tier_name IN ('FOUNDER_PARTNER', 'LAUNCH_PARTNER', 'EARLY_ADOPTER', 'STANDARD', 'PREMIUM')),
    CONSTRAINT uq_partnership_tiers_owner UNIQUE (c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_partnership_tiers_tier ON t_sys_partnership_tiers(c_tier_name);
CREATE INDEX IF NOT EXISTS ix_partnership_tiers_lock_end_date ON t_sys_partnership_tiers(c_tier_lock_end_date) WHERE c_is_lock_period_active = TRUE;

-- =============================================
-- SECTION 8: Commission Tier History
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_commission_tier_history (
    c_history_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    -- Change Details
    c_old_tier_name VARCHAR(50) NOT NULL,
    c_new_tier_name VARCHAR(50) NOT NULL,
    c_old_commission_rate DECIMAL(5,2) NOT NULL,
    c_new_commission_rate DECIMAL(5,2) NOT NULL,

    -- Reason & Timing
    c_change_reason VARCHAR(100) NOT NULL, -- 'LOCK_PERIOD_EXPIRED', 'PERFORMANCE_UPGRADE', 'PERFORMANCE_DOWNGRADE', 'ADMIN_OVERRIDE'
    c_effective_date DATE NOT NULL,
    c_notice_period_days INTEGER DEFAULT 60,
    c_notice_sent_date DATE,

    -- Partner Communication
    c_partner_notified BOOLEAN DEFAULT FALSE,
    c_partner_acknowledged BOOLEAN DEFAULT FALSE,
    c_partner_acknowledgment_date TIMESTAMP,

    -- Audit
    c_changed_by BIGINT,
    c_createddate TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_commission_tier_history_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_commission_tier_history_owner ON t_sys_commission_tier_history(c_ownerid, c_createddate DESC);
CREATE INDEX IF NOT EXISTS ix_commission_tier_history_effective_date ON t_sys_commission_tier_history(c_effective_date);

-- =============================================
-- SECTION 9: Create Automated Lock Job Flag Table
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_auto_lock_jobs (
    c_job_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_job_name VARCHAR(100) NOT NULL,
    c_job_type VARCHAR(50) NOT NULL, -- 'GUEST_COUNT_LOCK', 'MENU_LOCK', 'PRE_EVENT_PAYMENT_CHARGE', 'POST_EVENT_RELEASE'
    c_enabled BOOLEAN DEFAULT TRUE,
    c_last_run_date TIMESTAMP,
    c_next_run_date TIMESTAMP,
    c_run_frequency_minutes INTEGER DEFAULT 60, -- Run every 60 minutes
    c_orders_processed_last_run INTEGER DEFAULT 0,
    c_createddate TIMESTAMP DEFAULT NOW()
);

-- Insert default jobs
INSERT INTO t_sys_auto_lock_jobs (c_job_name, c_job_type, c_enabled, c_run_frequency_minutes)
VALUES
    ('Auto-Lock Guest Count (5 days before)', 'GUEST_COUNT_LOCK',          TRUE, 60),
    ('Auto-Lock Menu (3 days before)',         'MENU_LOCK',                 TRUE, 60),
    ('Auto-Charge Pre-Event Payment (48h before)', 'PRE_EVENT_PAYMENT_CHARGE', TRUE, 120),
    ('Auto-Release Post-Event Payment (3 days after)', 'POST_EVENT_RELEASE', TRUE, 360)
ON CONFLICT DO NOTHING;