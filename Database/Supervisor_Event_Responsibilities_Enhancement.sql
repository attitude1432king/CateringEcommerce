-- =============================================
-- Event Supervisor Responsibilities - Enhanced Schema
-- Pre-Event, During-Event, Post-Event Tracking
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Enhancing Supervisor Assignment Table';
PRINT 'Adding Detailed Event Supervision Tracking';
PRINT '================================================';
PRINT '';

-- =============================================
-- 1. PRE-EVENT VERIFICATION (Before Event Starts)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor_assignment') AND name = 'c_pre_event_verification_status')
BEGIN
    ALTER TABLE t_sys_supervisor_assignment ADD
        -- Pre-Event Verification Status
        c_pre_event_verification_status VARCHAR(20) NULL DEFAULT 'PENDING',
        c_pre_event_verification_date DATETIME NULL,

        -- Menu Verification
        c_menu_verified BIT NOT NULL DEFAULT 0,
        c_menu_vs_contract_match BIT NULL,
        c_menu_verification_notes NVARCHAR(1000) NULL,
        c_menu_verification_photos TEXT NULL, -- JSON array of photo URLs

        -- Raw Material & Quantity Verification
        c_raw_material_verified BIT NOT NULL DEFAULT 0,
        c_raw_material_quality_ok BIT NULL,
        c_raw_material_quantity_ok BIT NULL,
        c_raw_material_notes NVARCHAR(1000) NULL,
        c_raw_material_photos TEXT NULL, -- JSON array

        -- Guest Count Lock Confirmation
        c_guest_count_confirmed BIT NOT NULL DEFAULT 0,
        c_confirmed_guest_count INT NULL,
        c_locked_guest_count INT NULL,
        c_guest_count_mismatch BIT NULL,
        c_guest_count_confirmation_date DATETIME NULL,

        -- Pre-Event Evidence
        c_pre_event_evidence_submitted BIT NOT NULL DEFAULT 0,
        c_pre_event_evidence_urls TEXT NULL, -- JSON array: {type: 'photo/video', url: '...', timestamp: '...'}
        c_pre_event_evidence_timestamp DATETIME NULL,

        -- Pre-Event Checklist Completion
        c_pre_event_checklist_completed BIT NOT NULL DEFAULT 0,
        c_pre_event_checklist_completion_date DATETIME NULL,
        c_pre_event_issues_found BIT NOT NULL DEFAULT 0,
        c_pre_event_issues_description NVARCHAR(2000) NULL,

        CHECK (c_pre_event_verification_status IN ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'ISSUES_FOUND'));

    PRINT '✓ Added pre-event verification columns';
END
ELSE
    PRINT '  Pre-event columns already exist';
GO

-- =============================================
-- 2. DURING-EVENT MONITORING
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor_assignment') AND name = 'c_during_event_monitoring_active')
BEGIN
    ALTER TABLE t_sys_supervisor_assignment ADD
        -- Monitoring Status
        c_during_event_monitoring_active BIT NOT NULL DEFAULT 0,
        c_monitoring_started_time DATETIME NULL,
        c_monitoring_ended_time DATETIME NULL,

        -- Food Serving Monitoring
        c_food_serving_monitored BIT NOT NULL DEFAULT 0,
        c_food_serving_quality_rating INT NULL, -- 1-5
        c_food_serving_temperature_ok BIT NULL,
        c_food_serving_presentation_ok BIT NULL,
        c_food_serving_notes NVARCHAR(1000) NULL,
        c_food_serving_photos TEXT NULL, -- JSON array

        -- Guest Count Tracking (Real-time)
        c_actual_guest_count INT NULL,
        c_guest_count_variance INT NULL, -- Difference from confirmed count
        c_guest_count_tracking_log TEXT NULL, -- JSON: [{timestamp, count, notes}]

        -- Extra Quantity Requests
        c_extra_quantity_requested BIT NOT NULL DEFAULT 0,
        c_extra_quantity_details TEXT NULL, -- JSON: [{item, quantity, reason, timestamp}]

        -- Client Approval for Additional Cost
        c_client_approval_required BIT NULL,
        c_client_approval_method VARCHAR(20) NULL, -- 'IN_APP', 'OTP', 'SIGNATURE'
        c_client_otp_sent BIT NULL,
        c_client_otp_verified BIT NULL,
        c_client_otp_verification_time DATETIME NULL,
        c_client_approval_status VARCHAR(20) NULL, -- 'PENDING', 'APPROVED', 'REJECTED'
        c_client_approval_signature_url VARCHAR(500) NULL,

        -- Live Issue Tracking
        c_live_issues_reported BIT NOT NULL DEFAULT 0,
        c_live_issues_log TEXT NULL, -- JSON: [{timestamp, issue, severity, action_taken}]

        CHECK (c_client_approval_method IN ('IN_APP', 'OTP', 'SIGNATURE', NULL)),
        CHECK (c_client_approval_status IN ('PENDING', 'APPROVED', 'REJECTED', NULL));

    PRINT '✓ Added during-event monitoring columns';
END
ELSE
    PRINT '  During-event columns already exist';
GO

-- =============================================
-- 3. POST-EVENT COMPLETION
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor_assignment') AND name = 'c_post_event_report_submitted')
BEGIN
    ALTER TABLE t_sys_supervisor_assignment ADD
        -- Post-Event Report
        c_post_event_report_submitted BIT NOT NULL DEFAULT 0,
        c_post_event_report_date DATETIME NULL,

        -- Structured Feedback Collection
        c_client_feedback_collected BIT NOT NULL DEFAULT 0,
        c_client_overall_satisfaction INT NULL, -- 1-5
        c_client_food_quality_rating INT NULL, -- 1-5
        c_client_food_quantity_rating INT NULL, -- 1-5
        c_client_service_rating INT NULL, -- 1-5
        c_client_presentation_rating INT NULL, -- 1-5
        c_client_feedback_comments NVARCHAR(2000) NULL,
        c_client_feedback_timestamp DATETIME NULL,

        -- Vendor Feedback
        c_vendor_feedback_collected BIT NOT NULL DEFAULT 0,
        c_vendor_cooperation_rating INT NULL, -- 1-5
        c_vendor_preparation_rating INT NULL, -- 1-5
        c_vendor_feedback_comments NVARCHAR(1000) NULL,

        -- Issues Recording
        c_issues_recorded BIT NOT NULL DEFAULT 0,
        c_issues_count INT NOT NULL DEFAULT 0,
        c_issues_summary TEXT NULL, -- JSON: [{issue_type, severity, description, resolution}]

        -- Final Payment Request
        c_final_payment_request_raised BIT NOT NULL DEFAULT 0,
        c_final_payment_request_date DATETIME NULL,
        c_final_payment_amount DECIMAL(18,2) NULL,
        c_final_payment_breakdown TEXT NULL, -- JSON: {base_amount, extra_charges, deductions}

        -- Event Completion Report
        c_event_completion_report_url VARCHAR(500) NULL,
        c_event_completion_summary NVARCHAR(2000) NULL,
        c_event_completion_photos TEXT NULL, -- JSON array
        c_event_completion_signature_url VARCHAR(500) NULL, -- Client signature
        c_report_verified_by_admin BIGINT NULL,
        c_report_verification_date DATETIME NULL;

    PRINT '✓ Added post-event completion columns';
END
ELSE
    PRINT '  Post-event columns already exist';
GO

-- =============================================
-- 4. CREATE PRE-EVENT VERIFICATION CHECKLIST TABLE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_pre_event_checklist')
BEGIN
    CREATE TABLE t_sys_pre_event_checklist (
        c_checklist_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_assignment_id BIGINT NOT NULL,
        c_supervisor_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,

        -- Menu Verification Items
        c_menu_items_received TEXT NULL, -- JSON array of menu items
        c_menu_items_verified TEXT NULL, -- JSON array of verified items
        c_missing_items TEXT NULL, -- JSON array
        c_substituted_items TEXT NULL, -- JSON: [{original, substitute, reason}]

        -- Quantity Verification
        c_expected_portions INT NULL,
        c_verified_portions INT NULL,
        c_portion_variance_acceptable BIT NULL,

        -- Quality Verification
        c_freshness_check_done BIT NOT NULL DEFAULT 0,
        c_hygiene_check_done BIT NOT NULL DEFAULT 0,
        c_packaging_check_done BIT NOT NULL DEFAULT 0,
        c_temperature_check_done BIT NOT NULL DEFAULT 0,
        c_quality_issues_found BIT NOT NULL DEFAULT 0,
        c_quality_issues_details NVARCHAR(1000) NULL,

        -- Photos & Evidence
        c_checklist_photos TEXT NULL, -- JSON array
        c_timestamp DATETIME NOT NULL DEFAULT GETDATE(),

        -- Sign-off
        c_supervisor_signed_off BIT NOT NULL DEFAULT 0,
        c_vendor_signed_off BIT NOT NULL DEFAULT 0,
        c_vendor_signature_url VARCHAR(500) NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        FOREIGN KEY (c_order_id) REFERENCES t_sys_order(c_orderid),
        INDEX idx_assignment (c_assignment_id)
    );
    PRINT '✓ Created table: t_sys_pre_event_checklist';
END
ELSE
    PRINT '  Table t_sys_pre_event_checklist already exists';
GO

-- =============================================
-- 5. CREATE DURING-EVENT TRACKING TABLE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_during_event_tracking')
BEGIN
    CREATE TABLE t_sys_during_event_tracking (
        c_tracking_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_assignment_id BIGINT NOT NULL,
        c_supervisor_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,

        -- Tracking Type
        c_tracking_type VARCHAR(30) NOT NULL,
        CHECK (c_tracking_type IN (
            'GUEST_COUNT_UPDATE', 'FOOD_SERVING_CHECK', 'EXTRA_QUANTITY_REQUEST',
            'CLIENT_APPROVAL', 'ISSUE_REPORTED', 'QUALITY_CHECK'
        )),

        -- Details
        c_tracking_description NVARCHAR(1000) NOT NULL,
        c_tracking_data TEXT NULL, -- JSON: complete tracking payload

        -- Guest Count Specific
        c_guest_count INT NULL,
        c_guest_count_variance INT NULL,

        -- Extra Quantity Specific
        c_extra_item_name NVARCHAR(200) NULL,
        c_extra_quantity INT NULL,
        c_extra_cost DECIMAL(18,2) NULL,
        c_extra_reason NVARCHAR(500) NULL,

        -- Client Approval Specific
        c_approval_method VARCHAR(20) NULL, -- 'IN_APP', 'OTP', 'SIGNATURE'
        c_otp_code VARCHAR(10) NULL,
        c_otp_sent_time DATETIME NULL,
        c_otp_verified BIT NULL,
        c_approval_status VARCHAR(20) NULL, -- 'PENDING', 'APPROVED', 'REJECTED'
        c_approval_timestamp DATETIME NULL,

        -- Evidence
        c_evidence_urls TEXT NULL, -- JSON array
        c_gps_location VARCHAR(100) NULL,

        -- Timestamp
        c_timestamp DATETIME NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        INDEX idx_assignment (c_assignment_id),
        INDEX idx_tracking_type (c_tracking_type),
        INDEX idx_timestamp (c_timestamp)
    );
    PRINT '✓ Created table: t_sys_during_event_tracking';
END
ELSE
    PRINT '  Table t_sys_during_event_tracking already exists';
GO

-- =============================================
-- 6. CREATE POST-EVENT COMPLETION REPORT TABLE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_post_event_report')
BEGIN
    CREATE TABLE t_sys_post_event_report (
        c_report_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_assignment_id BIGINT NOT NULL,
        c_supervisor_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,

        -- Event Summary
        c_event_started_time DATETIME NULL,
        c_event_ended_time DATETIME NULL,
        c_event_duration_minutes INT NULL,
        c_final_guest_count INT NULL,
        c_event_rating INT NULL, -- Supervisor's overall rating 1-5

        -- Client Feedback (Structured)
        c_client_name NVARCHAR(100) NULL,
        c_client_phone VARCHAR(15) NULL,
        c_client_email VARCHAR(100) NULL,
        c_client_satisfaction_rating INT NULL, -- 1-5
        c_food_quality_rating INT NULL,
        c_food_quantity_rating INT NULL,
        c_service_quality_rating INT NULL,
        c_presentation_rating INT NULL,
        c_value_for_money_rating INT NULL,
        c_would_recommend BIT NULL,
        c_client_comments NVARCHAR(2000) NULL,
        c_client_signature_url VARCHAR(500) NULL,

        -- Vendor Performance
        c_vendor_punctuality_rating INT NULL, -- 1-5
        c_vendor_preparation_rating INT NULL,
        c_vendor_cooperation_rating INT NULL,
        c_vendor_hygiene_rating INT NULL,
        c_vendor_comments NVARCHAR(1000) NULL,

        -- Issues Summary
        c_total_issues_count INT NOT NULL DEFAULT 0,
        c_critical_issues_count INT NOT NULL DEFAULT 0,
        c_major_issues_count INT NOT NULL DEFAULT 0,
        c_minor_issues_count INT NOT NULL DEFAULT 0,
        c_issues_summary TEXT NULL, -- JSON: [{type, severity, description, resolution, timestamp}]
        c_all_issues_resolved BIT NULL,

        -- Financial Summary
        c_base_order_amount DECIMAL(18,2) NULL,
        c_extra_charges_amount DECIMAL(18,2) NULL DEFAULT 0,
        c_deductions_amount DECIMAL(18,2) NULL DEFAULT 0,
        c_final_payable_amount DECIMAL(18,2) NULL,
        c_payment_breakdown TEXT NULL, -- JSON

        -- Completion Evidence
        c_completion_photos TEXT NULL, -- JSON array
        c_completion_videos TEXT NULL, -- JSON array
        c_waste_photos TEXT NULL, -- For quantity verification
        c_leftover_notes NVARCHAR(1000) NULL,

        -- Report Details
        c_report_summary NVARCHAR(2000) NOT NULL,
        c_recommendations NVARCHAR(1000) NULL,
        c_supervisor_notes NVARCHAR(1000) NULL,
        c_report_pdf_url VARCHAR(500) NULL,

        -- Verification
        c_report_verified BIT NOT NULL DEFAULT 0,
        c_verified_by BIGINT NULL,
        c_verification_date DATETIME NULL,
        c_verification_notes NVARCHAR(500) NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_submitted_date DATETIME NULL,

        FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        INDEX idx_assignment (c_assignment_id),
        INDEX idx_created_date (c_createddate)
    );
    PRINT '✓ Created table: t_sys_post_event_report';
END
ELSE
    PRINT '  Table t_sys_post_event_report already exists';
GO

-- =============================================
-- 7. CREATE CLIENT OTP VERIFICATION TABLE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_client_otp_verification')
BEGIN
    CREATE TABLE t_sys_client_otp_verification (
        c_otp_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_assignment_id BIGINT NOT NULL,
        c_order_id BIGINT NOT NULL,
        c_supervisor_id BIGINT NOT NULL,

        -- OTP Details
        c_otp_code VARCHAR(10) NOT NULL,
        c_otp_purpose VARCHAR(50) NOT NULL, -- 'EXTRA_QUANTITY_APPROVAL', 'PAYMENT_RELEASE', 'MENU_CHANGE'
        c_otp_sent_to VARCHAR(15) NOT NULL, -- Client phone number
        c_otp_sent_time DATETIME NOT NULL DEFAULT GETDATE(),
        c_otp_expires_at DATETIME NOT NULL,

        -- Verification
        c_otp_verified BIT NOT NULL DEFAULT 0,
        c_otp_verified_time DATETIME NULL,
        c_verification_attempts INT NOT NULL DEFAULT 0,
        c_max_attempts INT NOT NULL DEFAULT 3,

        -- Context Data
        c_context_data TEXT NULL, -- JSON: {item_name, quantity, amount, etc.}
        c_client_ip_address VARCHAR(50) NULL,

        -- Status
        c_status VARCHAR(20) NOT NULL DEFAULT 'SENT',
        CHECK (c_status IN ('SENT', 'VERIFIED', 'EXPIRED', 'FAILED')),

        FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
        INDEX idx_otp_code (c_otp_code),
        INDEX idx_assignment (c_assignment_id),
        INDEX idx_status (c_status)
    );
    PRINT '✓ Created table: t_sys_client_otp_verification';
END
ELSE
    PRINT '  Table t_sys_client_otp_verification already exists';
GO

PRINT '';
PRINT '================================================';
PRINT 'Event Supervision Responsibilities Enhancement Complete';
PRINT '================================================';
PRINT '';
PRINT 'Tables Enhanced/Created:';
PRINT '1. t_sys_supervisor_assignment - Added 60+ new columns for detailed tracking';
PRINT '2. t_sys_pre_event_checklist - Pre-event verification tracking';
PRINT '3. t_sys_during_event_tracking - Real-time event monitoring';
PRINT '4. t_sys_post_event_report - Comprehensive completion report';
PRINT '5. t_sys_client_otp_verification - OTP-based client approval';
PRINT '';
GO
