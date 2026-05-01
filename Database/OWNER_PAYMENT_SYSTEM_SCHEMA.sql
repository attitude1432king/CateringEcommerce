-- =============================================
-- OWNER PAYMENT & SETTLEMENT SYSTEM (PostgreSQL)
-- =============================================

-- =============================================
-- TABLE: t_owner_payment
-- =============================================
CREATE TABLE IF NOT EXISTS t_owner_payment (
    c_owner_payment_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    c_settlement_amount DECIMAL(18,2) NOT NULL,
    c_platform_service_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    c_net_settlement_amount DECIMAL(18,2) NOT NULL,

    c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',

    c_payment_method VARCHAR(50),
    c_transaction_reference VARCHAR(100),

    c_escrowed_at TIMESTAMP,
    c_released_at TIMESTAMP,
    c_failed_at TIMESTAMP,
    c_failure_reason VARCHAR(500),

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_createdby BIGINT,
    c_updated_by BIGINT,

    CONSTRAINT fk_ownerpayment_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),

    CONSTRAINT fk_ownerpayment_order FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid),

    CONSTRAINT chk_ownerpayment_status CHECK 
    (c_status IN ('PENDING','ESCROWED','RELEASED','FAILED','REFUNDED','CANCELLED')),

    CONSTRAINT chk_ownerpayment_amounts CHECK 
    (c_settlement_amount >= 0 AND c_platform_service_fee >= 0 AND c_net_settlement_amount >= 0)
);

CREATE INDEX IF NOT EXISTS ix_ownerpayment_ownerid ON t_owner_payment(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_ownerpayment_orderid ON t_owner_payment(c_orderid);
CREATE INDEX IF NOT EXISTS ix_ownerpayment_status ON t_owner_payment(c_status);
CREATE INDEX IF NOT EXISTS ix_ownerpayment_releasedat ON t_owner_payment(c_released_at);

-- =============================================
-- TABLE: t_owner_settlement
-- =============================================
CREATE TABLE IF NOT EXISTS t_owner_settlement (
    c_settlement_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    c_settlement_period_start TIMESTAMP NOT NULL,
    c_settlement_period_end TIMESTAMP NOT NULL,

    c_total_gross_amount DECIMAL(18,2) NOT NULL,
    c_total_platform_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    c_total_adjustments DECIMAL(18,2) NOT NULL DEFAULT 0,
    c_net_settlement_amount DECIMAL(18,2) NOT NULL,

    c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',

    c_processed_at TIMESTAMP,
    c_payment_batch_id VARCHAR(100),
    c_bank_reference VARCHAR(100),

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_createdby BIGINT,
    c_updated_by BIGINT,

    CONSTRAINT fk_ownersettlement_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),

    CONSTRAINT chk_ownersettlement_status CHECK 
    (c_status IN ('PENDING','PROCESSING','COMPLETED','FAILED','CANCELLED')),

    CONSTRAINT chk_ownersettlement_period CHECK 
    (c_settlement_period_end > c_settlement_period_start)
);

CREATE INDEX IF NOT EXISTS ix_ownersettlement_ownerid ON t_owner_settlement(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_ownersettlement_status ON t_owner_settlement(c_status);
CREATE INDEX IF NOT EXISTS ix_ownersettlement_period 
ON t_owner_settlement(c_settlement_period_start, c_settlement_period_end);

-- =============================================
-- TABLE: t_owner_payout_schedule
-- =============================================
CREATE TABLE IF NOT EXISTS t_owner_payout_schedule (
    c_schedule_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_settlement_id BIGINT,

    c_scheduled_amount DECIMAL(18,2) NOT NULL,
    c_scheduled_date TIMESTAMP NOT NULL,

    c_is_released BOOLEAN NOT NULL DEFAULT FALSE,
    c_released_at TIMESTAMP,
    c_release_method VARCHAR(50),

    c_transaction_id VARCHAR(100),
    c_bank_account_id BIGINT,

    c_status VARCHAR(20) NOT NULL DEFAULT 'SCHEDULED',

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_ownerpayout_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),

    CONSTRAINT fk_ownerpayout_settlement FOREIGN KEY (c_settlement_id)
        REFERENCES t_owner_settlement(c_settlement_id),

    CONSTRAINT chk_ownerpayout_status CHECK 
    (c_status IN ('SCHEDULED','PROCESSING','RELEASED','FAILED','CANCELLED')),

    CONSTRAINT chk_ownerpayout_amount CHECK (c_scheduled_amount > 0)
);

CREATE INDEX IF NOT EXISTS ix_ownerpayout_ownerid ON t_owner_payout_schedule(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_ownerpayout_date ON t_owner_payout_schedule(c_scheduled_date);
CREATE INDEX IF NOT EXISTS ix_ownerpayout_status ON t_owner_payout_schedule(c_status);

-- =============================================
-- TABLE: t_partner_approval_request
-- =============================================
CREATE TABLE IF NOT EXISTS t_partner_approval_request (
    c_approval_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_orderid BIGINT NOT NULL,

    c_request_type VARCHAR(50) NOT NULL,
    c_description TEXT NOT NULL,
    c_request_data TEXT,

    c_requested_by_user_id BIGINT NOT NULL,
    c_requested_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    c_deadline TIMESTAMP NOT NULL,
    c_response_time_hours INT NOT NULL DEFAULT 24,

    c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',

    c_approved_at TIMESTAMP,
    c_approved_by_owner_id BIGINT,

    c_rejected_at TIMESTAMP,
    c_rejection_reason VARCHAR(500),

    c_partner_notes TEXT,

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_partnerapproval_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),

    CONSTRAINT fk_partnerapproval_order FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid),

    CONSTRAINT chk_partnerapproval_status CHECK 
    (c_status IN ('PENDING','APPROVED','REJECTED','EXPIRED','CANCELLED')),

    CONSTRAINT chk_partnerapproval_type CHECK 
    (c_request_type IN ('MENU_CHANGE','GUEST_COUNT_INCREASE','SPECIAL_REQUEST','EVENT_MODIFICATION'))
);

CREATE INDEX IF NOT EXISTS ix_partnerapproval_ownerid ON t_partner_approval_request(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_partnerapproval_orderid ON t_partner_approval_request(c_orderid);
CREATE INDEX IF NOT EXISTS ix_partnerapproval_status ON t_partner_approval_request(c_status);
CREATE INDEX IF NOT EXISTS ix_partnerapproval_deadline ON t_partner_approval_request(c_deadline);

-- =============================================
-- TABLE: t_partner_response_history
-- =============================================
CREATE TABLE IF NOT EXISTS t_partner_response_history (
    c_response_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_complaint_id BIGINT,
    c_orderid BIGINT NOT NULL,

    c_response_type VARCHAR(50) NOT NULL,
    c_response_text TEXT NOT NULL,
    c_response_status VARCHAR(20) NOT NULL DEFAULT 'SUBMITTED',

    c_evidence_urls TEXT,
    c_evidence_count INT NOT NULL DEFAULT 0,

    c_responded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_reviewed_at TIMESTAMP,

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_partnerresponse_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),

    CONSTRAINT fk_partnerresponse_order FOREIGN KEY (c_orderid)
        REFERENCES t_sys_orders(c_orderid)
);

CREATE INDEX IF NOT EXISTS ix_partnerresponse_ownerid ON t_partner_response_history(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_partnerresponse_complaintid ON t_partner_response_history(c_complaint_id);
CREATE INDEX IF NOT EXISTS ix_partnerresponse_status ON t_partner_response_history(c_response_status);

-- =============================================
-- TABLE: t_platform_fee_config
-- =============================================
CREATE TABLE IF NOT EXISTS t_platform_fee_config (
    c_config_id SERIAL PRIMARY KEY,
    c_fee_type VARCHAR(50) NOT NULL,
    c_fee_value DECIMAL(18,2) NOT NULL,
    c_min_amount DECIMAL(18,2),
    c_max_amount DECIMAL(18,2),
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO t_platform_fee_config (c_fee_type, c_fee_value)
VALUES ('PERCENTAGE', 10.00)
ON CONFLICT DO NOTHING;