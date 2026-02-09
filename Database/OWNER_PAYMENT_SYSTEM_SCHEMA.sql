/*
====================================================
CATERING OWNER PAYMENT & SETTLEMENT SYSTEM
====================================================
Correct Domain Model: Owner/Partner (NOT Vendor)

Tables:
- t_owner_payment
- t_owner_settlement
- t_owner_payout_schedule
- t_partner_approval_request

Author: System Architect
Date: 2026-01-30
====================================================
*/

USE CateringDB;
GO

-- =============================================
-- TABLE: t_owner_payment
-- Owner payment settlements for individual orders
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_owner_payment')
BEGIN
    CREATE TABLE t_owner_payment (
        c_owner_payment_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_owner_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,

        -- Settlement amounts
        c_settlement_amount DECIMAL(18,2) NOT NULL,
        c_platform_service_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
        c_net_settlement_amount DECIMAL(18,2) NOT NULL,

        -- Status tracking
        c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        -- PENDING, ESCROWED, RELEASED, FAILED, REFUNDED, CANCELLED

        -- Payment details
        c_payment_method VARCHAR(50) NULL,
        c_transaction_reference VARCHAR(100) NULL,

        -- Timestamps
        c_escrowed_at DATETIME2 NULL,
        c_released_at DATETIME2 NULL,
        c_failed_at DATETIME2 NULL,
        c_failure_reason NVARCHAR(500) NULL,

        -- Audit
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_created_by BIGINT NULL,
        c_updated_by BIGINT NULL,

        -- Foreign keys
        CONSTRAINT FK_OwnerPayment_Owner FOREIGN KEY (c_owner_id)
            REFERENCES t_sys_owner(c_owner_id),
        CONSTRAINT FK_OwnerPayment_Order FOREIGN KEY (c_order_id)
            REFERENCES t_order(c_order_id),

        -- Constraints
        CONSTRAINT CHK_OwnerPayment_Status CHECK (c_status IN ('PENDING', 'ESCROWED', 'RELEASED', 'FAILED', 'REFUNDED', 'CANCELLED')),
        CONSTRAINT CHK_OwnerPayment_Amounts CHECK (c_settlement_amount >= 0 AND c_platform_service_fee >= 0 AND c_net_settlement_amount >= 0)
    );

    CREATE INDEX IX_OwnerPayment_OwnerId ON t_owner_payment(c_owner_id);
    CREATE INDEX IX_OwnerPayment_OrderId ON t_owner_payment(c_order_id);
    CREATE INDEX IX_OwnerPayment_Status ON t_owner_payment(c_status);
    CREATE INDEX IX_OwnerPayment_ReleasedAt ON t_owner_payment(c_released_at);
END
GO

-- =============================================
-- TABLE: t_owner_settlement
-- Aggregated settlements for catering partners
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_owner_settlement')
BEGIN
    CREATE TABLE t_owner_settlement (
        c_settlement_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_owner_id BIGINT NOT NULL,

        -- Settlement period
        c_settlement_period_start DATETIME2 NOT NULL,
        c_settlement_period_end DATETIME2 NOT NULL,

        -- Settlement amounts
        c_total_gross_amount DECIMAL(18,2) NOT NULL,
        c_total_platform_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
        c_total_adjustments DECIMAL(18,2) NOT NULL DEFAULT 0,
        c_net_settlement_amount DECIMAL(18,2) NOT NULL,

        -- Status tracking
        c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        -- PENDING, PROCESSING, COMPLETED, FAILED, CANCELLED

        -- Processing details
        c_processed_at DATETIME2 NULL,
        c_payment_batch_id VARCHAR(100) NULL,
        c_bank_reference VARCHAR(100) NULL,

        -- Audit
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_created_by BIGINT NULL,
        c_updated_by BIGINT NULL,

        -- Foreign key
        CONSTRAINT FK_OwnerSettlement_Owner FOREIGN KEY (c_owner_id)
            REFERENCES t_sys_owner(c_owner_id),

        -- Constraints
        CONSTRAINT CHK_OwnerSettlement_Status CHECK (c_status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED', 'CANCELLED')),
        CONSTRAINT CHK_OwnerSettlement_Period CHECK (c_settlement_period_end > c_settlement_period_start)
    );

    CREATE INDEX IX_OwnerSettlement_OwnerId ON t_owner_settlement(c_owner_id);
    CREATE INDEX IX_OwnerSettlement_Status ON t_owner_settlement(c_status);
    CREATE INDEX IX_OwnerSettlement_Period ON t_owner_settlement(c_settlement_period_start, c_settlement_period_end);
END
GO

-- =============================================
-- TABLE: t_owner_payout_schedule
-- Scheduled payouts for catering partners
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_owner_payout_schedule')
BEGIN
    CREATE TABLE t_owner_payout_schedule (
        c_schedule_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_owner_id BIGINT NOT NULL,
        c_settlement_id BIGINT NULL,

        -- Schedule details
        c_scheduled_amount DECIMAL(18,2) NOT NULL,
        c_scheduled_date DATETIME2 NOT NULL,

        -- Release tracking
        c_is_released BIT NOT NULL DEFAULT 0,
        c_released_at DATETIME2 NULL,
        c_release_method VARCHAR(50) NULL,
        -- BANK_TRANSFER, UPI, WALLET, CHECK

        -- Transaction details
        c_transaction_id VARCHAR(100) NULL,
        c_bank_account_id BIGINT NULL,

        -- Status
        c_status VARCHAR(20) NOT NULL DEFAULT 'SCHEDULED',
        -- SCHEDULED, PROCESSING, RELEASED, FAILED, CANCELLED

        -- Audit
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

        -- Foreign keys
        CONSTRAINT FK_OwnerPayoutSchedule_Owner FOREIGN KEY (c_owner_id)
            REFERENCES t_sys_owner(c_owner_id),
        CONSTRAINT FK_OwnerPayoutSchedule_Settlement FOREIGN KEY (c_settlement_id)
            REFERENCES t_owner_settlement(c_settlement_id),

        -- Constraints
        CONSTRAINT CHK_OwnerPayoutSchedule_Status CHECK (c_status IN ('SCHEDULED', 'PROCESSING', 'RELEASED', 'FAILED', 'CANCELLED')),
        CONSTRAINT CHK_OwnerPayoutSchedule_Amount CHECK (c_scheduled_amount > 0)
    );

    CREATE INDEX IX_OwnerPayoutSchedule_OwnerId ON t_owner_payout_schedule(c_owner_id);
    CREATE INDEX IX_OwnerPayoutSchedule_ScheduledDate ON t_owner_payout_schedule(c_scheduled_date);
    CREATE INDEX IX_OwnerPayoutSchedule_Status ON t_owner_payout_schedule(c_status);
END
GO

-- =============================================
-- TABLE: t_partner_approval_request
-- Approval requests requiring catering partner response
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_partner_approval_request')
BEGIN
    CREATE TABLE t_partner_approval_request (
        c_approval_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_owner_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,

        -- Request details
        c_request_type VARCHAR(50) NOT NULL,
        -- MENU_CHANGE, GUEST_COUNT_INCREASE, SPECIAL_REQUEST, EVENT_MODIFICATION

        c_description NVARCHAR(1000) NOT NULL,
        c_request_data NVARCHAR(MAX) NULL, -- JSON payload

        -- Requester info
        c_requested_by_user_id BIGINT NOT NULL,
        c_requested_at DATETIME2 NOT NULL DEFAULT GETDATE(),

        -- Deadline tracking
        c_deadline DATETIME2 NOT NULL,
        c_response_time_hours INT NOT NULL DEFAULT 24,

        -- Status tracking
        c_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        -- PENDING, APPROVED, REJECTED, EXPIRED, CANCELLED

        -- Approval details
        c_approved_at DATETIME2 NULL,
        c_approved_by_owner_id BIGINT NULL,

        -- Rejection details
        c_rejected_at DATETIME2 NULL,
        c_rejection_reason NVARCHAR(500) NULL,

        -- Partner notes
        c_partner_notes NVARCHAR(1000) NULL,

        -- Audit
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

        -- Foreign keys
        CONSTRAINT FK_PartnerApproval_Owner FOREIGN KEY (c_owner_id)
            REFERENCES t_sys_owner(c_owner_id),
        CONSTRAINT FK_PartnerApproval_Order FOREIGN KEY (c_order_id)
            REFERENCES t_order(c_order_id),

        -- Constraints
        CONSTRAINT CHK_PartnerApproval_Status CHECK (c_status IN ('PENDING', 'APPROVED', 'REJECTED', 'EXPIRED', 'CANCELLED')),
        CONSTRAINT CHK_PartnerApproval_RequestType CHECK (c_request_type IN ('MENU_CHANGE', 'GUEST_COUNT_INCREASE', 'SPECIAL_REQUEST', 'EVENT_MODIFICATION'))
    );

    CREATE INDEX IX_PartnerApproval_OwnerId ON t_partner_approval_request(c_owner_id);
    CREATE INDEX IX_PartnerApproval_OrderId ON t_partner_approval_request(c_order_id);
    CREATE INDEX IX_PartnerApproval_Status ON t_partner_approval_request(c_status);
    CREATE INDEX IX_PartnerApproval_Deadline ON t_partner_approval_request(c_deadline);
END
GO

-- =============================================
-- TABLE: t_partner_response_history
-- Track all partner responses to complaints/issues
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_partner_response_history')
BEGIN
    CREATE TABLE t_partner_response_history (
        c_response_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_owner_id BIGINT NOT NULL,
        c_complaint_id BIGINT NULL,
        c_order_id BIGINT NOT NULL,

        -- Response details
        c_response_type VARCHAR(50) NOT NULL,
        -- COMPLAINT_RESPONSE, QUALITY_DISPUTE, SERVICE_ISSUE

        c_response_text NVARCHAR(2000) NOT NULL,
        c_response_status VARCHAR(20) NOT NULL DEFAULT 'SUBMITTED',
        -- SUBMITTED, UNDER_REVIEW, ACCEPTED, REJECTED

        -- Evidence
        c_evidence_urls NVARCHAR(MAX) NULL, -- JSON array
        c_evidence_count INT NOT NULL DEFAULT 0,

        -- Timestamps
        c_responded_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_reviewed_at DATETIME2 NULL,

        -- Audit
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

        -- Foreign keys
        CONSTRAINT FK_PartnerResponse_Owner FOREIGN KEY (c_owner_id)
            REFERENCES t_sys_owner(c_owner_id),
        CONSTRAINT FK_PartnerResponse_Order FOREIGN KEY (c_order_id)
            REFERENCES t_order(c_order_id)
    );

    CREATE INDEX IX_PartnerResponse_OwnerId ON t_partner_response_history(c_owner_id);
    CREATE INDEX IX_PartnerResponse_ComplaintId ON t_partner_response_history(c_complaint_id);
    CREATE INDEX IX_PartnerResponse_Status ON t_partner_response_history(c_response_status);
END
GO

-- =============================================
-- COMPUTED COLUMNS & TRIGGERS
-- =============================================

-- Trigger to update c_updated_at automatically
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_OwnerPayment_UpdateTimestamp')
BEGIN
    EXEC('
    CREATE TRIGGER TR_OwnerPayment_UpdateTimestamp
    ON t_owner_payment
    AFTER UPDATE
    AS
    BEGIN
        UPDATE t_owner_payment
        SET c_updated_at = GETDATE()
        WHERE c_owner_payment_id IN (SELECT c_owner_payment_id FROM inserted)
    END
    ')
END
GO

-- =============================================
-- SAMPLE DATA / DEFAULT VALUES
-- =============================================

-- Platform service fee configuration (can be stored in settings table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_platform_fee_config')
BEGIN
    CREATE TABLE t_platform_fee_config (
        c_config_id INT PRIMARY KEY IDENTITY(1,1),
        c_fee_type VARCHAR(50) NOT NULL,
        -- PERCENTAGE, FLAT_FEE, TIERED

        c_fee_value DECIMAL(18,2) NOT NULL,
        c_min_amount DECIMAL(18,2) NULL,
        c_max_amount DECIMAL(18,2) NULL,

        c_is_active BIT NOT NULL DEFAULT 1,
        c_created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        c_updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    -- Insert default platform service fee (e.g., 10%)
    INSERT INTO t_platform_fee_config (c_fee_type, c_fee_value)
    VALUES ('PERCENTAGE', 10.00);
END
GO

PRINT 'Owner Payment & Settlement System Schema Created Successfully';
PRINT 'Tables Created:';
PRINT '  - t_owner_payment';
PRINT '  - t_owner_settlement';
PRINT '  - t_owner_payout_schedule';
PRINT '  - t_partner_approval_request';
PRINT '  - t_partner_response_history';
PRINT '  - t_platform_fee_config';
PRINT '';
PRINT '✅ All tables use OWNER/PARTNER terminology (NO VENDOR)';
