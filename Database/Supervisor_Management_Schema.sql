-- =============================================
-- Supervisor Management System - Database Schema
-- Two-Portal Strategy: Careers Portal + Registration Portal
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Creating Supervisor Management System Tables';
PRINT '================================================';
PRINT '';

-- =============================================
-- 1. CORE SUPERVISOR TABLE (SHARED BASE)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor')
BEGIN
    CREATE TABLE t_sys_supervisor (
        -- Primary Key
        c_supervisor_id BIGINT PRIMARY KEY IDENTITY(1,1),

        -- Supervisor Type (CRITICAL DISTINCTION)
        c_supervisor_type VARCHAR(20) NOT NULL, -- 'CAREER' or 'REGISTERED'
        CHECK (c_supervisor_type IN ('CAREER', 'REGISTERED')),

        -- Basic Information
        c_full_name NVARCHAR(100) NOT NULL,
        c_email VARCHAR(100) NOT NULL UNIQUE,
        c_phone VARCHAR(15) NOT NULL,
        c_alternate_phone VARCHAR(15) NULL,
        c_date_of_birth DATE NULL,
        c_gender VARCHAR(10) NULL,

        -- Address
        c_address_line1 NVARCHAR(500) NULL,
        c_cityid INT NOT NULL,
        c_stateid INT NOT NULL,
        c_pincode VARCHAR(10) NOT NULL,
        c_locality NVARCHAR(100) NULL, -- For assignment matching

        -- Identity & Documents
        c_identity_type VARCHAR(20) NULL, -- AADHAAR, PAN, PASSPORT, etc.
        c_identity_number VARCHAR(50) NULL,
        c_identity_proof_url VARCHAR(500) NULL,
        c_photo_url VARCHAR(500) NULL,        
        c_address_url VARCHAR(500) NULL,
        c_resume_url VARCHAR(500) NULL, -- Careers only

        -- Experience Details
        c_has_prior_experience BIT NULL,
        c_prior_experience_details NVARCHAR(500) NULL,
        c_specialization NVARCHAR(200) NULL, -- e.g., "Wedding Catering, Corporate Events"
        c_languages_known NVARCHAR(200) NULL, -- JSON array

        -- Status & Lifecycle
        c_current_status VARCHAR(30) NOT NULL DEFAULT 'APPLIED',
        CHECK (c_current_status IN (
            'APPLIED', 'RESUME_SCREENED', 'INTERVIEW_SCHEDULED', 'INTERVIEW_PASSED',
            'BACKGROUND_VERIFICATION', 'TRAINING', 'CERTIFIED', 'PROBATION',
            'ACTIVE', 'SUSPENDED', 'DEACTIVATED', 'REJECTED', 'BLACKLISTED'
        )),
        c_status_reason NVARCHAR(500) NULL,

        -- Authority Level
        c_authority_level VARCHAR(20) NOT NULL DEFAULT 'BASIC',
        CHECK (c_authority_level IN ('BASIC', 'INTERMEDIATE', 'ADVANCED', 'FULL')),
        c_can_release_payment BIT NOT NULL DEFAULT 0,
        c_can_approve_refund BIT NOT NULL DEFAULT 0,
        c_can_mentor_others BIT NOT NULL DEFAULT 0,

        -- Availability
        c_is_available BIT NOT NULL DEFAULT 1,
        c_availability_calendar TEXT NULL, -- JSON: { "2026-02-01": true, ... }
        c_preferred_event_types NVARCHAR(500) NULL, -- JSON array
        c_max_events_per_month INT NULL DEFAULT 10,

        -- Banking (Post-Activation Only)
        c_bank_account_number VARCHAR(50) NULL,
        c_bank_ifsc VARCHAR(20) NULL,
        c_bank_account_holder_name NVARCHAR(100) NULL,
        c_bank_name NVARCHAR(100) NULL,
        c_cancelled_cheque_url VARCHAR(500) NULL,

        -- Compensation (Type-Specific)
        c_compensation_type VARCHAR(20) NULL, -- 'MONTHLY_SALARY', 'PER_EVENT', 'HYBRID'
        c_monthly_salary DECIMAL(18,2) NULL, -- Careers only
        c_per_event_rate DECIMAL(18,2) NULL, -- Registration only
        c_incentive_percentage DECIMAL(5,2) NULL DEFAULT 0, -- Both

        -- Performance Metrics
        c_total_events_supervised INT NOT NULL DEFAULT 0,
        c_average_rating DECIMAL(3,2) NULL,
        c_total_ratings_received INT NOT NULL DEFAULT 0,
        c_complaints_count INT NOT NULL DEFAULT 0,
        c_dispute_resolution_count INT NOT NULL DEFAULT 0,

        -- Training & Certification
        c_training_completed_date DATETIME NULL,
        c_certification_date DATETIME NULL,
        c_certification_valid_until DATE NULL,
        c_certification_status VARCHAR(20) NULL, -- 'PENDING', 'CERTIFIED', 'EXPIRED'

        -- Probation (Careers Only)
        c_probation_start_date DATE NULL,
        c_probation_end_date DATE NULL,
        c_is_probation_passed BIT NULL,

        -- Admin Controls
        c_assigned_admin_id BIGINT NULL, -- Admin who manages this supervisor
        c_suspended_by BIGINT NULL,
        c_suspension_date DATETIME NULL,
        c_suspension_reason NVARCHAR(500) NULL,

        -- Agreement & Compliance
        c_agreement_signed BIT NOT NULL DEFAULT 0,
        c_agreement_signed_date DATETIME NULL,
        c_agreement_url VARCHAR(500) NULL,
        c_background_check_status VARCHAR(20) NULL, -- 'PENDING', 'PASSED', 'FAILED'
        c_background_check_date DATETIME NULL,

        -- Audit Fields
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_createdby BIGINT NULL,
        c_modifieddate DATETIME NULL,
        c_modifiedby BIGINT NULL,
        c_last_login_date DATETIME NULL,
        c_is_deleted BIT NOT NULL DEFAULT 0,

        -- Indexes
        INDEX idx_supervisor_type (c_supervisor_type),
        INDEX idx_current_status (c_current_status),
        INDEX idx_city_locality (c_city, c_locality),
        INDEX idx_is_available (c_is_available),
        INDEX idx_email (c_email)
    );
    PRINT '✓ Created table: t_sys_supervisor';
END
ELSE
    PRINT '  Table t_sys_supervisor already exists';
GO

-- =============================================
-- 2. CAREERS APPLICATION WORKFLOW
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_careers_application')
BEGIN
    CREATE TABLE t_sys_careers_application (
        c_application_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_supervisor_id BIGINT NOT NULL,

        -- Application Details
        c_application_number VARCHAR(50) NOT NULL UNIQUE,
        c_applied_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_source VARCHAR(50) NULL, -- 'WEBSITE', 'REFERRAL', 'LINKEDIN', etc.
        c_referral_code VARCHAR(50) NULL,

        -- Resume Screening
        c_resume_screened BIT NOT NULL DEFAULT 0,
        c_resume_screened_by BIGINT NULL,
        c_resume_screened_date DATETIME NULL,
        c_resume_screening_notes NVARCHAR(1000) NULL,
        c_resume_screening_status VARCHAR(20) NULL, -- 'PASSED', 'FAILED', 'ON_HOLD'

        -- Interview Stage
        c_interview_scheduled BIT NOT NULL DEFAULT 0,
        c_interview_date DATETIME NULL,
        c_interview_mode VARCHAR(20) NULL, -- 'IN_PERSON', 'VIDEO_CALL', 'PHONE'
        c_interviewer_id BIGINT NULL,
        c_interview_completed BIT NOT NULL DEFAULT 0,
        c_interview_feedback NVARCHAR(2000) NULL,
        c_interview_score DECIMAL(5,2) NULL, -- Out of 100
        c_interview_result VARCHAR(20) NULL, -- 'PASSED', 'FAILED'

        -- Background Verification
        c_background_verification_initiated BIT NOT NULL DEFAULT 0,
        c_background_verification_agency VARCHAR(100) NULL,
        c_background_verification_date DATETIME NULL,
        c_background_verification_result VARCHAR(20) NULL, -- 'CLEAR', 'ISSUES_FOUND', 'PENDING'
        c_background_verification_report_url VARCHAR(500) NULL,

        -- Training Stage
        c_training_batch_id BIGINT NULL,
        c_training_start_date DATE NULL,
        c_training_end_date DATE NULL,
        c_training_attendance_percentage DECIMAL(5,2) NULL,
        c_training_completed BIT NOT NULL DEFAULT 0,

        -- Certification Stage
        c_certification_test_date DATETIME NULL,
        c_certification_test_score DECIMAL(5,2) NULL,
        c_certification_passed BIT NOT NULL DEFAULT 0,
        c_certification_certificate_url VARCHAR(500) NULL,

        -- Probation Stage
        c_probation_assigned BIT NOT NULL DEFAULT 0,
        c_probation_start_date DATE NULL,
        c_probation_duration_days INT NULL DEFAULT 90,
        c_probation_supervisor_id BIGINT NULL, -- Mentor during probation
        c_probation_evaluation_date DATE NULL,
        c_probation_evaluation_notes NVARCHAR(2000) NULL,
        c_probation_passed BIT NULL,

        -- Final Decision
        c_final_decision VARCHAR(20) NULL, -- 'ACCEPTED', 'REJECTED', 'ON_HOLD'
        c_final_decision_date DATETIME NULL,
        c_final_decision_by BIGINT NULL,
        c_rejection_reason NVARCHAR(1000) NULL,

        -- Onboarding
        c_onboarding_completed BIT NOT NULL DEFAULT 0,
        c_joining_date DATE NULL,
        c_offer_letter_url VARCHAR(500) NULL,
        c_employee_id VARCHAR(50) NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        INDEX idx_application_number (c_application_number),
        INDEX idx_supervisor (c_supervisor_id)
    );
    PRINT '✓ Created table: t_sys_careers_application';
END
ELSE
    PRINT '  Table t_sys_careers_application already exists';
GO

-- =============================================
-- 3. REGISTRATION PORTAL WORKFLOW
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor_registration')
BEGIN
    CREATE TABLE t_sys_supervisor_registration (
        c_registration_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_supervisor_id BIGINT NOT NULL,

        -- Registration Details
        c_registration_number VARCHAR(50) NOT NULL UNIQUE,
        c_registered_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_source VARCHAR(50) NULL, -- 'WEBSITE', 'MOBILE_APP', 'REFERRAL'
        c_referral_code VARCHAR(50) NULL,

        -- Document Verification (MANDATORY)
        c_document_verification_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        CHECK (c_document_verification_status IN ('PENDING', 'VERIFIED', 'REJECTED')),
        c_document_verified_by BIGINT NULL,
        c_document_verified_date DATETIME NULL,
        c_document_rejection_reason NVARCHAR(500) NULL,

        -- Short Interview (MANDATORY)
        c_interview_scheduled BIT NOT NULL DEFAULT 0,
        c_interview_date DATETIME NULL,
        c_interview_mode VARCHAR(20) NULL DEFAULT 'VIDEO_CALL',
        c_interviewer_id BIGINT NULL,
        c_interview_completed BIT NOT NULL DEFAULT 0,
        c_interview_notes NVARCHAR(1000) NULL,
        c_interview_result VARCHAR(20) NULL, -- 'PASSED', 'FAILED'

        -- Training Module (MANDATORY)
        c_training_module_assigned BIT NOT NULL DEFAULT 0,
        c_training_module_id BIGINT NULL,
        c_training_started_date DATETIME NULL,
        c_training_completed_date DATETIME NULL,
        c_training_completion_percentage DECIMAL(5,2) NULL,
        c_training_passed BIT NOT NULL DEFAULT 0,

        -- Certification Test (MANDATORY)
        c_certification_test_assigned BIT NOT NULL DEFAULT 0,
        c_certification_test_date DATETIME NULL,
        c_certification_test_score DECIMAL(5,2) NULL,
        c_certification_test_passed BIT NOT NULL DEFAULT 0,
        c_certification_certificate_url VARCHAR(500) NULL,

        -- Activation
        c_activation_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        CHECK (c_activation_status IN ('PENDING', 'ACTIVATED', 'REJECTED', 'SUSPENDED')),
        c_activated_date DATETIME NULL,
        c_activated_by BIGINT NULL,
        c_rejection_reason NVARCHAR(1000) NULL,

        -- Agreement
        c_agreement_accepted BIT NOT NULL DEFAULT 0,
        c_agreement_accepted_date DATETIME NULL,
        c_agreement_url VARCHAR(500) NULL,
        c_agreement_ip_address VARCHAR(50) NULL,

        -- Availability Setup
        c_availability_configured BIT NOT NULL DEFAULT 0,
        c_preferred_cities NVARCHAR(500) NULL, -- JSON array
        c_preferred_localities NVARCHAR(500) NULL, -- JSON array
        c_available_days_per_week INT NULL,

        -- Banking Setup (Post-Activation)
        c_bank_details_submitted BIT NOT NULL DEFAULT 0,
        c_bank_details_verified BIT NOT NULL DEFAULT 0,
        c_bank_verification_date DATETIME NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        INDEX idx_registration_number (c_registration_number),
        INDEX idx_supervisor (c_supervisor_id),
        INDEX idx_activation_status (c_activation_status)
    );
    PRINT '✓ Created table: t_sys_supervisor_registration';
END
ELSE
    PRINT '  Table t_sys_supervisor_registration already exists';
GO

-- =============================================
-- 4. SUPERVISOR EVENT ASSIGNMENTS
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor_assignment')
BEGIN
    CREATE TABLE t_sys_supervisor_assignment (
        c_assignment_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_order_id BIGINT NOT NULL,
        c_supervisor_id BIGINT NOT NULL,

        -- Assignment Details
        c_assignment_number VARCHAR(50) NOT NULL UNIQUE,
        c_assigned_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_assigned_by BIGINT NOT NULL, -- Admin who assigned
        c_assignment_source VARCHAR(20) NOT NULL, -- 'MANUAL', 'AUTO_ASSIGNED'

        -- Event Details (Denormalized for Quick Access)
        c_event_date DATE NOT NULL,
        c_event_time TIME NULL,
        c_event_type NVARCHAR(100) NULL,
        c_event_location NVARCHAR(200) NULL,
        c_estimated_guests INT NULL,
        c_event_value DECIMAL(18,2) NULL,

        -- Assignment Status
        c_status VARCHAR(30) NOT NULL DEFAULT 'ASSIGNED',
        CHECK (c_status IN (
            'ASSIGNED', 'ACCEPTED', 'REJECTED', 'CHECKED_IN',
            'IN_PROGRESS', 'COMPLETED', 'CANCELLED', 'NO_SHOW'
        )),

        -- Supervisor Actions
        c_accepted_date DATETIME NULL,
        c_rejection_reason NVARCHAR(500) NULL,
        c_check_in_time DATETIME NULL,
        c_check_in_location VARCHAR(500) NULL, -- GPS coordinates
        c_check_out_time DATETIME NULL,

        -- Event Supervision Report
        c_quality_check_done BIT NOT NULL DEFAULT 0,
        c_quality_rating INT NULL, -- 1-5
        c_quantity_verified BIT NOT NULL DEFAULT 0,
        c_quantity_notes NVARCHAR(1000) NULL,
        c_issues_reported NVARCHAR(2000) NULL,
        c_photos_uploaded VARCHAR(2000) NULL, -- JSON array of URLs

        -- Payment & Settlement
        c_payment_release_requested BIT NOT NULL DEFAULT 0,
        c_payment_release_approved BIT NOT NULL DEFAULT 0,
        c_payment_approved_by BIGINT NULL,
        c_payment_approval_date DATETIME NULL,
        c_extra_charges_amount DECIMAL(18,2) NULL DEFAULT 0,
        c_extra_charges_reason NVARCHAR(500) NULL,

        -- Supervisor Compensation
        c_supervisor_payout_amount DECIMAL(18,2) NULL,
        c_supervisor_payout_status VARCHAR(20) NULL DEFAULT 'PENDING',
        CHECK (c_supervisor_payout_status IN ('PENDING', 'PROCESSED', 'PAID', 'FAILED')),
        c_supervisor_payout_date DATETIME NULL,

        -- Rating & Review
        c_vendor_rating INT NULL, -- Vendor rates supervisor (1-5)
        c_vendor_feedback NVARCHAR(1000) NULL,
        c_admin_rating INT NULL, -- Admin rates supervisor (1-5)
        c_admin_feedback NVARCHAR(1000) NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        INDEX idx_order (c_order_id),
        INDEX idx_supervisor (c_supervisor_id),
        INDEX idx_event_date (c_event_date),
        INDEX idx_status (c_status)
    );
    PRINT '✓ Created table: t_sys_supervisor_assignment';
END
ELSE
    PRINT '  Table t_sys_supervisor_assignment already exists';
GO

-- =============================================
-- 5. SUPERVISOR ACTION LOG (AUDIT TRAIL)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor_action_log')
BEGIN
    CREATE TABLE t_sys_supervisor_action_log (
        c_log_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_supervisor_id BIGINT NOT NULL,
        c_assignment_id BIGINT NULL,
        c_order_id BIGINT NULL,

        -- Action Details
        c_action_type VARCHAR(50) NOT NULL,
        CHECK (c_action_type IN (
            'CHECK_IN', 'CHECK_OUT', 'QUALITY_CHECK', 'QUANTITY_CHECK',
            'PAYMENT_RELEASE_REQUEST', 'REFUND_REQUEST', 'ISSUE_REPORTED',
            'PHOTO_UPLOADED', 'EXTRA_CHARGE_REQUESTED', 'DISPUTE_RAISED',
            'STATUS_CHANGED', 'ASSIGNMENT_ACCEPTED', 'ASSIGNMENT_REJECTED'
        )),

        c_action_description NVARCHAR(1000) NOT NULL,
        c_action_data TEXT NULL, -- JSON: complete action payload
        c_action_result VARCHAR(20) NULL, -- 'SUCCESS', 'FAILED', 'PENDING'

        -- Authorization Check
        c_authority_level_required VARCHAR(20) NULL,
        c_authority_check_passed BIT NOT NULL DEFAULT 1,
        c_override_by_admin BIGINT NULL, -- If admin overrode the action
        c_override_reason NVARCHAR(500) NULL,

        -- Evidence
        c_evidence_urls TEXT NULL, -- JSON array of photo/video URLs
        c_gps_location VARCHAR(100) NULL,
        c_ip_address VARCHAR(50) NULL,
        c_device_info VARCHAR(200) NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        FOREIGN KEY (c_assignment_id) REFERENCES t_sys_supervisor_assignment(c_assignment_id),
        INDEX idx_supervisor (c_supervisor_id),
        INDEX idx_assignment (c_assignment_id),
        INDEX idx_action_type (c_action_type),
        INDEX idx_created_date (c_createddate)
    );
    PRINT '✓ Created table: t_sys_supervisor_action_log';
END
ELSE
    PRINT '  Table t_sys_supervisor_action_log already exists';
GO

-- =============================================
-- 6. SUPERVISOR TRAINING MODULES
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor_training_module')
BEGIN
    CREATE TABLE t_sys_supervisor_training_module (
        c_module_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_module_code VARCHAR(50) NOT NULL UNIQUE,
        c_module_name NVARCHAR(200) NOT NULL,
        c_module_description NVARCHAR(1000) NULL,
        c_module_type VARCHAR(20) NOT NULL, -- 'CAREER', 'REGISTERED', 'BOTH'
        c_is_mandatory BIT NOT NULL DEFAULT 1,
        c_duration_hours DECIMAL(5,2) NULL,
        c_content_url VARCHAR(500) NULL,
        c_video_url VARCHAR(500) NULL,
        c_passing_score DECIMAL(5,2) NULL DEFAULT 70.00,
        c_is_active BIT NOT NULL DEFAULT 1,
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL
    );
    PRINT '✓ Created table: t_sys_supervisor_training_module';
END
ELSE
    PRINT '  Table t_sys_supervisor_training_module already exists';
GO

-- =============================================
-- 7. SUPERVISOR TRAINING PROGRESS
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_supervisor_training_progress')
BEGIN
    CREATE TABLE t_sys_supervisor_training_progress (
        c_progress_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_supervisor_id BIGINT NOT NULL,
        c_module_id BIGINT NOT NULL,
        c_started_date DATETIME NULL,
        c_completed_date DATETIME NULL,
        c_completion_percentage DECIMAL(5,2) NULL DEFAULT 0,
        c_test_score DECIMAL(5,2) NULL,
        c_passed BIT NOT NULL DEFAULT 0,
        c_attempts_count INT NOT NULL DEFAULT 0,
        c_last_attempt_date DATETIME NULL,
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (c_supervisor_id) REFERENCES t_sys_supervisor(c_supervisor_id),
        FOREIGN KEY (c_module_id) REFERENCES t_sys_supervisor_training_module(c_module_id),
        INDEX idx_supervisor_module (c_supervisor_id, c_module_id)
    );
    PRINT '✓ Created table: t_sys_supervisor_training_progress';
END
ELSE
    PRINT '  Table t_sys_supervisor_training_progress already exists';
GO

-- =============================================
-- 8. ASSIGNMENT ELIGIBILITY RULES (CONFIGURABLE)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_assignment_eligibility_rule')
BEGIN
    CREATE TABLE t_sys_assignment_eligibility_rule (
        c_rule_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_rule_name NVARCHAR(100) NOT NULL,
        c_rule_description NVARCHAR(500) NULL,

        -- Condition
        c_event_type NVARCHAR(100) NULL, -- 'WEDDING', 'CORPORATE', 'VIP', etc.
        c_min_event_value DECIMAL(18,2) NULL,
        c_max_event_value DECIMAL(18,2) NULL,
        c_min_guest_count INT NULL,
        c_is_new_vendor BIT NULL,
        c_is_vip_event BIT NULL,

        -- Required Supervisor Type
        c_required_supervisor_type VARCHAR(20) NULL, -- 'CAREER', 'REGISTERED', 'EITHER'
        c_required_authority_level VARCHAR(20) NULL, -- 'BASIC', 'INTERMEDIATE', 'ADVANCED', 'FULL'
        c_required_min_experience INT NULL,
        c_required_min_rating DECIMAL(3,2) NULL,

        -- Priority
        c_priority INT NOT NULL DEFAULT 100, -- Lower number = higher priority
        c_is_active BIT NOT NULL DEFAULT 1,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_createdby BIGINT NULL,
        c_modifieddate DATETIME NULL,
        c_modifiedby BIGINT NULL,

        INDEX idx_priority (c_priority),
        INDEX idx_is_active (c_is_active)
    );
    PRINT '✓ Created table: t_sys_assignment_eligibility_rule';
END
ELSE
    PRINT '  Table t_sys_assignment_eligibility_rule already exists';
GO

PRINT '';
PRINT '================================================';
PRINT 'Supervisor Management System Tables Created Successfully';
PRINT '================================================';
PRINT '';
PRINT 'Tables Created:';
PRINT '1. t_sys_supervisor (Main supervisor entity)';
PRINT '2. t_sys_careers_application (Careers portal workflow)';
PRINT '3. t_sys_supervisor_registration (Registration portal workflow)';
PRINT '4. t_sys_supervisor_assignment (Event assignments)';
PRINT '5. t_sys_supervisor_action_log (Audit trail)';
PRINT '6. t_sys_supervisor_training_module (Training modules)';
PRINT '7. t_sys_supervisor_training_progress (Training progress tracking)';
PRINT '8. t_sys_assignment_eligibility_rule (Configurable assignment rules)';
PRINT '';
GO
