-- =============================================
-- Supervisor Management System - Database Schema
-- Two-Portal Strategy: Careers Portal + Registration Portal
-- =============================================

-- =============================================
-- 1. CORE SUPERVISOR TABLE (SHARED BASE)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor (
    -- Primary Key
    c_supervisor_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

    -- Supervisor Type (CRITICAL DISTINCTION)
    c_supervisor_type VARCHAR(20) NOT NULL,
    CONSTRAINT ck_supervisor_type CHECK (c_supervisor_type IN ('CAREER', 'REGISTERED')),

    -- Basic Information
    c_full_name VARCHAR(100) NOT NULL,
    c_email VARCHAR(100) NOT NULL UNIQUE,
    c_phone VARCHAR(15) NOT NULL,
    c_alternate_phone VARCHAR(15) NULL,
    c_date_of_birth DATE NULL,
    c_gender VARCHAR(10) NULL,

    -- Address
    c_address_line1 VARCHAR(500) NULL,
    c_cityid INTEGER NOT NULL,
    c_stateid INTEGER NOT NULL,
    c_pincode VARCHAR(10) NOT NULL,
    c_locality VARCHAR(100) NULL,

    -- Identity & Documents
    c_identity_type VARCHAR(20) NULL,
    c_identity_number VARCHAR(50) NULL,
    c_identity_proof_url VARCHAR(500) NULL,
    c_photo_url VARCHAR(500) NULL,        
    c_address_url VARCHAR(500) NULL,
    c_resume_url VARCHAR(500) NULL,

    -- Experience Details
    c_has_prior_experience BOOLEAN NULL,
    c_prior_experience_details VARCHAR(500) NULL,
    c_specialization VARCHAR(200) NULL,
    c_languages_known VARCHAR(200) NULL,

    -- Status & Lifecycle
    c_current_status VARCHAR(30) NOT NULL DEFAULT 'APPLIED',
    CONSTRAINT ck_current_status CHECK (c_current_status IN (
        'APPLIED', 'RESUME_SCREENED', 'INTERVIEW_SCHEDULED', 'INTERVIEW_PASSED',
        'BACKGROUND_VERIFICATION', 'TRAINING', 'CERTIFIED', 'PROBATION',
        'ACTIVE', 'SUSPENDED', 'DEACTIVATED', 'REJECTED', 'BLACKLISTED'
    )),
    c_status_reason VARCHAR(500) NULL,

    -- Authority Level
    c_authority_level VARCHAR(20) NOT NULL DEFAULT 'BASIC',
    CONSTRAINT ck_authority_level CHECK (c_authority_level IN ('BASIC', 'INTERMEDIATE', 'ADVANCED', 'FULL')),
    c_can_release_payment BOOLEAN NOT NULL DEFAULT FALSE,
    c_can_approve_refund BOOLEAN NOT NULL DEFAULT FALSE,
    c_can_mentor_others BOOLEAN NOT NULL DEFAULT FALSE,

    -- Availability
    c_is_available BOOLEAN NOT NULL DEFAULT TRUE,
    c_availability_calendar TEXT NULL,
    c_preferred_event_types VARCHAR(500) NULL,
    c_max_events_per_month INTEGER NULL DEFAULT 10,

    -- Banking (Post-Activation Only)
    c_bank_account_number VARCHAR(50) NULL,
    c_bank_branch_name VARCHAR(100) NULL,
    c_bank_account_type VARCHAR(20) NULL,
    c_bank_ifsc VARCHAR(20) NULL,
    c_bank_account_holder_name VARCHAR(100) NULL,
    c_bank_name VARCHAR(100) NULL,
    c_cancelled_cheque_url VARCHAR(500) NULL,

    -- Compensation (Type-Specific)
    c_compensation_type VARCHAR(20) NULL,
    c_monthly_salary DECIMAL(18,2) NULL,
    c_per_event_rate DECIMAL(18,2) NULL,
    c_incentive_percentage DECIMAL(5,2) NULL DEFAULT 0,

    -- Performance Metrics
    c_total_events_supervised INTEGER NOT NULL DEFAULT 0,
    c_average_rating DECIMAL(3,2) NULL,
    c_total_ratings_received INTEGER NOT NULL DEFAULT 0,
    c_complaints_count INTEGER NOT NULL DEFAULT 0,
    c_dispute_resolution_count INTEGER NOT NULL DEFAULT 0,

    -- Training & Certification
    c_training_completed_date TIMESTAMP NULL,
    c_certification_date TIMESTAMP NULL,
    c_certification_valid_until DATE NULL,
    c_certification_status VARCHAR(20) NULL,

    -- Probation (Careers Only)
    c_probation_start_date DATE NULL,
    c_probation_end_date DATE NULL,
    c_is_probation_passed BOOLEAN NULL,

    -- Admin Controls
    c_assigned_admin_id BIGINT NULL,
    c_suspended_by BIGINT NULL,
    c_suspension_date TIMESTAMP NULL,
    c_suspension_reason VARCHAR(500) NULL,

    -- Agreement & Compliance
    c_agreement_signed BOOLEAN NOT NULL DEFAULT FALSE,
    c_agreement_signed_date TIMESTAMP NULL,
    c_agreement_url VARCHAR(500) NULL,
    c_background_check_status VARCHAR(20) NULL,
    c_background_check_date TIMESTAMP NULL,

    -- Audit Fields
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_createdby BIGINT NULL,
    c_modifieddate TIMESTAMP NULL,
    c_modifiedby BIGINT NULL,
    c_last_login_date TIMESTAMP NULL,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS idx_supervisor_type ON t_sys_supervisor (c_supervisor_type);
CREATE INDEX IF NOT EXISTS idx_current_status ON t_sys_supervisor (c_current_status);
CREATE INDEX IF NOT EXISTS idx_is_available ON t_sys_supervisor (c_is_available);
CREATE INDEX IF NOT EXISTS idx_email ON t_sys_supervisor (c_email);

-- =============================================
-- CAREERS APPLICATION TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_careers_application (
    c_application_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_supervisor_id BIGINT NOT NULL,

    -- Application Details
    c_application_number VARCHAR(50) NOT NULL UNIQUE,
    c_applied_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_source VARCHAR(50),
    c_referral_code VARCHAR(50),

    -- Resume Screening
    c_resume_screened BOOLEAN NOT NULL DEFAULT FALSE,
    c_resume_screened_by BIGINT,
    c_resume_screened_date TIMESTAMP,
    c_resume_screening_notes TEXT,
    c_resume_screening_status VARCHAR(20),

    -- Interview Stage
    c_interview_scheduled BOOLEAN NOT NULL DEFAULT FALSE,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR(20),
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_interview_feedback TEXT,
    c_interview_score DECIMAL(5,2),
    c_interview_result VARCHAR(20),

    -- Background Verification
    c_background_verification_initiated BOOLEAN NOT NULL DEFAULT FALSE,
    c_background_verification_agency VARCHAR(100),
    c_background_verification_date TIMESTAMP,
    c_background_verification_result VARCHAR(20),
    c_background_verification_report_url VARCHAR(500),

    -- Training Stage
    c_training_batch_id BIGINT,
    c_training_start_date DATE,
    c_training_end_date DATE,
    c_training_attendance_percentage DECIMAL(5,2),
    c_training_completed BOOLEAN NOT NULL DEFAULT FALSE,

    -- Certification Stage
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL(5,2),
    c_certification_passed BOOLEAN NOT NULL DEFAULT FALSE,
    c_certification_certificate_url VARCHAR(500),

    -- Probation Stage
    c_probation_assigned BOOLEAN NOT NULL DEFAULT FALSE,
    c_probation_start_date DATE,
    c_probation_duration_days INT DEFAULT 90,
    c_probation_supervisor_id BIGINT,
    c_probation_evaluation_date DATE,
    c_probation_evaluation_notes TEXT,
    c_probation_passed BOOLEAN,

    -- Final Decision
    c_final_decision VARCHAR(20),
    c_final_decision_date TIMESTAMP,
    c_final_decision_by BIGINT,
    c_rejection_reason TEXT,

    -- Onboarding
    c_onboarding_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_joining_date DATE,
    c_offer_letter_url VARCHAR(500),
    c_employee_id VARCHAR(50),

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    -- Foreign Key
    CONSTRAINT fk_careers_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_application_number
ON t_sys_careers_application (c_application_number);

CREATE INDEX IF NOT EXISTS idx_supervisor
ON t_sys_careers_application (c_supervisor_id);

-- =============================================
-- SUPERVISOR REGISTRATION TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor_registration (
    c_registration_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_supervisor_id BIGINT NOT NULL,

    -- Registration Details
    c_registration_number VARCHAR(50) NOT NULL UNIQUE,
    c_registered_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_source VARCHAR(50),
    c_referral_code VARCHAR(50),

    -- Document Verification (MANDATORY)
    c_document_verification_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    c_document_verified_by BIGINT,
    c_document_verified_date TIMESTAMP,
    c_document_rejection_reason TEXT,
    c_doc_verification_status VARCHAR(20) NULL,

    -- Short Interview (MANDATORY)
    c_interview_scheduled BOOLEAN NOT NULL DEFAULT FALSE,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR(20) DEFAULT 'VIDEO_CALL',
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_interview_notes TEXT,
    c_interview_result VARCHAR(20),

    -- Training Module (MANDATORY)
    c_training_module_assigned BOOLEAN NOT NULL DEFAULT FALSE,
    c_training_module_id BIGINT,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL(5,2),
    c_training_passed BOOLEAN NOT NULL DEFAULT FALSE,

    -- Certification Test (MANDATORY)
    c_certification_test_assigned BOOLEAN NOT NULL DEFAULT FALSE,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL(5,2),
    c_certification_test_passed BOOLEAN NOT NULL DEFAULT FALSE,
    c_certification_certificate_url VARCHAR(500),

    -- Activation
    c_activation_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,

    -- Agreement
    c_agreement_accepted BOOLEAN NOT NULL DEFAULT FALSE,
    c_agreement_accepted_date TIMESTAMP,
    c_agreement_url VARCHAR(500),
    c_agreement_ip_address VARCHAR(50),

    -- Availability Setup
    c_availability_configured BOOLEAN NOT NULL DEFAULT FALSE,
    c_preferred_cities JSONB,
    c_preferred_localities JSONB,
    c_available_days_per_week INT,

    -- Banking Setup (Post-Activation)
    c_bank_details_submitted BOOLEAN NOT NULL DEFAULT FALSE,
    c_bank_details_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_bank_verification_date TIMESTAMP,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    -- Constraints
    CONSTRAINT fk_registration_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT chk_document_status
        CHECK (c_document_verification_status IN ('PENDING', 'VERIFIED', 'REJECTED')),

    CONSTRAINT chk_activation_status
        CHECK (c_activation_status IN ('PENDING', 'ACTIVATED', 'REJECTED', 'SUSPENDED'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_registration_number
ON t_sys_supervisor_registration (c_registration_number);

CREATE INDEX IF NOT EXISTS idx_supervisor_registration
ON t_sys_supervisor_registration (c_supervisor_id);

CREATE INDEX IF NOT EXISTS idx_activation_status
ON t_sys_supervisor_registration (c_activation_status);

-- =============================================
-- SUPERVISOR ASSIGNMENT TABLE (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor_assignment (
    c_assignment_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_supervisor_id BIGINT NOT NULL,

    -- Assignment Details
    c_assignment_number VARCHAR(50) NOT NULL UNIQUE,
    c_assigned_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_assigned_by BIGINT NOT NULL,
    c_assignment_source VARCHAR(20) NOT NULL,

    -- Event Details
    c_event_date DATE NOT NULL,
    c_event_time TIME,
    c_event_type VARCHAR(100),
    c_event_location VARCHAR(200),
    c_estimated_guests INT,
    c_event_value DECIMAL(18,2),

    -- Assignment Status
    c_status VARCHAR(30) NOT NULL DEFAULT 'ASSIGNED',

    -- Supervisor Actions
    c_accepted_date TIMESTAMP,
    c_rejection_reason TEXT,
    c_check_in_time TIMESTAMP,
    c_check_in_location TEXT,
    c_check_out_time TIMESTAMP,

    -- Pre-Event Verification
    c_pre_event_verification_status VARCHAR(20) NULL DEFAULT 'PENDING',
    c_pre_event_verification_date TIMESTAMP NULL,
    c_menu_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_menu_vs_contract_match BOOLEAN NULL,
    c_menu_verification_notes VARCHAR(1000) NULL,
    c_menu_verification_photos TEXT NULL,
    c_raw_material_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_raw_material_quality_ok BOOLEAN NULL,
    c_raw_material_quantity_ok BOOLEAN NULL,
    c_raw_material_notes VARCHAR(1000) NULL,
    c_raw_material_photos TEXT NULL,
    c_guest_count_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    c_confirmed_guest_count INTEGER NULL,
    c_locked_guest_count INTEGER NULL,
    c_guest_count_mismatch BOOLEAN NULL,
    c_guest_count_confirmation_date TIMESTAMP NULL,
    c_pre_event_evidence_submitted BOOLEAN NOT NULL DEFAULT FALSE,
    c_pre_event_evidence_urls TEXT NULL,
    c_pre_event_evidence_timestamp TIMESTAMP NULL,
    c_pre_event_checklist_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_pre_event_checklist_completion_date TIMESTAMP NULL,
    c_pre_event_issues_found BOOLEAN NOT NULL DEFAULT FALSE,
    c_pre_event_issues_description VARCHAR(2000) NULL,

    -- In-Event Supervision
    c_during_event_monitoring_active BOOLEAN NOT NULL DEFAULT FALSE,
    c_monitoring_started_time TIMESTAMP NULL,
    c_monitoring_ended_time TIMESTAMP NULL,
    c_food_serving_monitored BOOLEAN NOT NULL DEFAULT FALSE,
    c_food_serving_quality_rating INTEGER NULL,
    c_food_serving_temperature_ok BOOLEAN NULL,
    c_food_serving_presentation_ok BOOLEAN NULL,
    c_food_serving_notes VARCHAR(1000) NULL,
    c_food_serving_photos TEXT NULL,
    c_actual_guest_count INTEGER NULL,
    c_guest_count_variance INTEGER NULL,
    c_guest_count_tracking_log TEXT NULL,
    c_extra_quantity_requested BOOLEAN NOT NULL DEFAULT FALSE,
    c_extra_quantity_details TEXT NULL,
    c_client_approval_required BOOLEAN NULL,
    c_client_approval_method VARCHAR(20) NULL,
    c_client_otp_sent BOOLEAN NULL,
    c_client_otp_verified BOOLEAN NULL,
    c_client_otp_verification_time TIMESTAMP NULL,
    c_client_approval_status VARCHAR(20) NULL,
    c_client_approval_signature_url VARCHAR(500) NULL,
    c_live_issues_reported BOOLEAN NOT NULL DEFAULT FALSE,
    c_live_issues_log TEXT NULL,

    -- Post-Event Supervision
    c_post_event_report_submitted BOOLEAN NOT NULL DEFAULT FALSE,
    c_post_event_report_date TIMESTAMP NULL,
    c_client_feedback_collected BOOLEAN NOT NULL DEFAULT FALSE,
    c_client_overall_satisfaction INTEGER NULL,
    c_client_food_quality_rating INTEGER NULL,
    c_client_food_quantity_rating INTEGER NULL,
    c_client_service_rating INTEGER NULL,
    c_client_presentation_rating INTEGER NULL,
    c_client_feedback_comments VARCHAR(2000) NULL,
    c_client_feedback_timestamp TIMESTAMP NULL,
    c_vendor_feedback_collected BOOLEAN NOT NULL DEFAULT FALSE,
    c_vendor_cooperation_rating INTEGER NULL,
    c_vendor_preparation_rating INTEGER NULL,
    c_vendor_feedback_comments VARCHAR(1000) NULL,
    c_issues_recorded BOOLEAN NOT NULL DEFAULT FALSE,
    c_issues_count INTEGER NOT NULL DEFAULT 0,
    c_issues_summary TEXT NULL,
    c_final_payment_request_raised BOOLEAN NOT NULL DEFAULT FALSE,
    c_final_payment_request_date TIMESTAMP NULL,
    c_final_payment_amount DECIMAL(18,2) NULL,
    c_final_payment_breakdown TEXT NULL,
    c_event_completion_report_url VARCHAR(500) NULL,
    c_event_completion_summary VARCHAR(2000) NULL,
    c_event_completion_photos TEXT NULL,
    c_event_completion_signature_url VARCHAR(500) NULL,
    c_report_verified_by_admin BIGINT NULL,
    c_report_verification_date TIMESTAMP NULL,

    -- Event Supervision Report
    c_quality_check_done BOOLEAN NOT NULL DEFAULT FALSE,
    c_quality_rating INT,
    c_quantity_verified BOOLEAN NOT NULL DEFAULT FALSE,
    c_quantity_notes TEXT,
    c_issues_reported TEXT,
    c_photos_uploaded JSONB,

    -- Payment & Settlement
    c_payment_release_requested BOOLEAN NOT NULL DEFAULT FALSE,
    c_payment_release_approved BOOLEAN NOT NULL DEFAULT FALSE,
    c_payment_approved_by BIGINT,
    c_payment_approval_date TIMESTAMP,
    c_extra_charges_amount DECIMAL(18,2) DEFAULT 0,
    c_extra_charges_reason TEXT,

    -- Supervisor Compensation
    c_supervisor_payout_amount DECIMAL(18,2),
    c_supervisor_payout_status VARCHAR(20) DEFAULT 'PENDING',
    c_supervisor_payout_date TIMESTAMP,

    -- Rating & Review
    c_vendor_rating INT,
    c_vendor_feedback TEXT,
    c_admin_rating INT,
    c_admin_feedback TEXT,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT ck_client_approval_method 
        CHECK (c_client_approval_method IN ('IN_APP', 'OTP', 'SIGNATURE', NULL)),
            
    CONSTRAINT ck_client_approval_status 
        CHECK (c_client_approval_status IN ('PENDING', 'APPROVED', 'REJECTED', NULL)),

    -- Constraints
    CONSTRAINT fk_assignment_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT chk_assignment_status
        CHECK (c_status IN (
            'ASSIGNED', 'ACCEPTED', 'REJECTED', 'CHECKED_IN',
            'IN_PROGRESS', 'COMPLETED', 'CANCELLED', 'NO_SHOW'
        )),
    
    CONSTRAINT ck_pre_event_verification_status 
        CHECK (c_pre_event_verification_status IN ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'ISSUES_FOUND')),

    CONSTRAINT chk_payout_status
        CHECK (c_supervisor_payout_status IN ('PENDING', 'PROCESSED', 'PAID', 'FAILED'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_assignment_order
ON t_sys_supervisor_assignment (c_orderid);

CREATE INDEX IF NOT EXISTS idx_assignment_supervisor
ON t_sys_supervisor_assignment (c_supervisor_id);

CREATE INDEX IF NOT EXISTS idx_assignment_event_date
ON t_sys_supervisor_assignment (c_event_date);

CREATE INDEX IF NOT EXISTS idx_assignment_status
ON t_sys_supervisor_assignment (c_status);

-- =============================================
-- SUPERVISOR ACTION LOG (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor_action_log (
    c_log_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_supervisor_id BIGINT NOT NULL,
    c_assignment_id BIGINT,
    c_orderid BIGINT,

    -- Action Details
    c_action_type VARCHAR(50) NOT NULL,
    c_action_description TEXT NOT NULL,
    c_action_data JSONB,
    c_action_result VARCHAR(20),

    -- Authorization Check
    c_authority_level_required VARCHAR(20),
    c_authority_check_passed BOOLEAN NOT NULL DEFAULT TRUE,
    c_override_by_admin BIGINT,
    c_override_reason TEXT,

    -- Evidence
    c_evidence_urls JSONB,
    c_gps_location TEXT,
    c_ip_address VARCHAR(50),
    c_device_info VARCHAR(200),

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT fk_log_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT fk_log_assignment
        FOREIGN KEY (c_assignment_id)
        REFERENCES t_sys_supervisor_assignment(c_assignment_id),

    CONSTRAINT chk_action_type
        CHECK (c_action_type IN (
            'CHECK_IN', 'CHECK_OUT', 'QUALITY_CHECK', 'QUANTITY_CHECK',
            'PAYMENT_RELEASE_REQUEST', 'REFUND_REQUEST', 'ISSUE_REPORTED',
            'PHOTO_UPLOADED', 'EXTRA_CHARGE_REQUESTED', 'DISPUTE_RAISED',
            'STATUS_CHANGED', 'ASSIGNMENT_ACCEPTED', 'ASSIGNMENT_REJECTED'
        ))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_log_supervisor
ON t_sys_supervisor_action_log (c_supervisor_id);

CREATE INDEX IF NOT EXISTS idx_log_assignment
ON t_sys_supervisor_action_log (c_assignment_id);

CREATE INDEX IF NOT EXISTS idx_log_action_type
ON t_sys_supervisor_action_log (c_action_type);

CREATE INDEX IF NOT EXISTS idx_log_created_date
ON t_sys_supervisor_action_log (c_createddate);

-- =============================================
-- SUPERVISOR TRAINING MODULES (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor_training_module (
    c_module_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_module_code VARCHAR(50) NOT NULL UNIQUE,
    c_module_name VARCHAR(200) NOT NULL,
    c_module_description TEXT,
    c_module_type VARCHAR(20) NOT NULL, -- 'CAREER', 'REGISTERED', 'BOTH'
    c_is_mandatory BOOLEAN NOT NULL DEFAULT TRUE,
    c_duration_hours DECIMAL(5,2),
    c_content_url VARCHAR(500),
    c_video_url VARCHAR(500),
    c_passing_score DECIMAL(5,2) DEFAULT 70.00,
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_module_type
        CHECK (c_module_type IN ('CAREER', 'REGISTERED', 'BOTH'))
);

-- =============================================
-- SUPERVISOR TRAINING PROGRESS (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_supervisor_training_progress (
    c_progress_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_supervisor_id BIGINT NOT NULL,
    c_module_id BIGINT NOT NULL,

    c_started_date TIMESTAMP,
    c_completed_date TIMESTAMP,
    c_completion_percentage DECIMAL(5,2) DEFAULT 0,
    c_test_score DECIMAL(5,2),
    c_passed BOOLEAN NOT NULL DEFAULT FALSE,
    c_attempts_count INT NOT NULL DEFAULT 0,
    c_last_attempt_date TIMESTAMP,

    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Foreign Keys
    CONSTRAINT fk_progress_supervisor
        FOREIGN KEY (c_supervisor_id)
        REFERENCES t_sys_supervisor(c_supervisor_id),

    CONSTRAINT fk_progress_module
        FOREIGN KEY (c_module_id)
        REFERENCES t_sys_supervisor_training_module(c_module_id),

    -- Prevent duplicate progress records
    CONSTRAINT uq_supervisor_module UNIQUE (c_supervisor_id, c_module_id)
);

-- Index for performance
CREATE INDEX IF NOT EXISTS idx_progress_supervisor_module
ON t_sys_supervisor_training_progress (c_supervisor_id, c_module_id);

-- =============================================
-- ASSIGNMENT ELIGIBILITY RULES (PostgreSQL)
-- =============================================

CREATE TABLE IF NOT EXISTS t_sys_assignment_eligibility_rule (
    c_rule_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_rule_name VARCHAR(100) NOT NULL,
    c_rule_description TEXT,

    -- Condition
    c_event_type VARCHAR(100),
    c_min_event_value DECIMAL(18,2),
    c_max_event_value DECIMAL(18,2),
    c_min_guest_count INT,
    c_is_new_vendor BOOLEAN,
    c_is_vip_event BOOLEAN,

    -- Required Supervisor Type
    c_required_supervisor_type VARCHAR(20), -- 'CAREER', 'REGISTERED', 'EITHER'
    c_required_authority_level VARCHAR(20), -- 'BASIC', 'INTERMEDIATE', 'ADVANCED', 'FULL'
    c_required_min_experience INT,
    c_required_min_rating DECIMAL(3,2),

    -- Priority
    c_priority INT NOT NULL DEFAULT 100,
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_createdby BIGINT,
    c_modifieddate TIMESTAMP,
    c_modifiedby BIGINT,

    -- Constraints
    CONSTRAINT chk_supervisor_type
        CHECK (c_required_supervisor_type IN ('CAREER', 'REGISTERED', 'EITHER')),

    CONSTRAINT chk_authority_level
        CHECK (c_required_authority_level IN ('BASIC', 'INTERMEDIATE', 'ADVANCED', 'FULL'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_rule_priority
ON t_sys_assignment_eligibility_rule (c_priority);

CREATE INDEX IF NOT EXISTS idx_rule_active
ON t_sys_assignment_eligibility_rule (c_is_active);
