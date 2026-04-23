-- =============================================
-- Supervisor Registration Stored Procedures
-- Maps to: RegistrationRepository.cs
-- 25 Stored Procedures for Registration Portal Workflow
-- =============================================

-- =============================================
-- REGISTRATION SUBMISSION
-- =============================================

CREATE OR REPLACE FUNCTION sp_SubmitSupervisorRegistration(
    p_FirstName VARCHAR,
    p_LastName VARCHAR,
    p_Email VARCHAR,
    p_Phone VARCHAR,
    p_Address TEXT,
    p_Pincode VARCHAR,
    p_StateID INT,
    p_CityID INT,
    p_DateOfBirth DATE,
    p_IDProofType VARCHAR,
    p_IDProofNumber VARCHAR,
    p_HasPriorExperience BOOLEAN,
    p_IDProofUrl TEXT DEFAULT NULL,
    p_AddressProofUrl TEXT DEFAULT NULL,
    p_PhotoUrl TEXT DEFAULT NULL,
    p_PriorExperienceDetails TEXT DEFAULT NULL
)
RETURNS TABLE (RegistrationId BIGINT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
    v_FullName TEXT;
    v_RegNumber TEXT;
BEGIN

    -- Preserve variable declaration (same as MS SQL)
    v_FullName := p_FirstName || ' ' || p_LastName;

    INSERT INTO t_sys_supervisor (
        c_supervisor_type, c_full_name, c_email, c_phone,
        c_date_of_birth, c_address_line1, c_cityid, c_stateid, c_pincode,
        c_identity_type, c_identity_number,
        c_has_prior_experience, c_prior_experience_details,
        c_current_status, c_authority_level,
        c_createddate
    )
    VALUES (
        'REGISTERED',
        v_FullName,
        p_Email, p_Phone,
        p_DateOfBirth, p_Address, p_CityID, p_StateID, p_Pincode,
        p_IDProofType, p_IDProofNumber,
        p_HasPriorExperience, p_PriorExperienceDetails,
        'APPLIED', 'BASIC',
        NOW()
    )
    RETURNING c_supervisor_id INTO v_SupervisorId;

    v_RegNumber := 'REG-' || TO_CHAR(NOW(), 'YYYYMMDD') || '-' || v_SupervisorId;

    INSERT INTO t_sys_supervisor_registration (
        c_supervisor_id, c_registration_number, c_registered_date,
        c_source, c_document_verification_status
    )
    VALUES (
        v_SupervisorId, v_RegNumber, NOW(),
        'WEBSITE', 'PENDING'
    )
    RETURNING c_registration_id INTO RegistrationId;

    RETURN QUERY SELECT RegistrationId;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationById(
    p_RegistrationId BIGINT
)
RETURNS TABLE (
    RegistrationId BIGINT,
    SupervisorId BIGINT,
    RegistrationNumber VARCHAR,
    RegisteredDate TIMESTAMP,
    Source VARCHAR,
    ReferralCode VARCHAR,

    DocumentVerificationStatus VARCHAR,
    DocumentVerifiedBy BIGINT,
    DocumentVerifiedDate TIMESTAMP,
    DocumentRejectionReason VARCHAR,

    InterviewScheduled BOOLEAN,
    InterviewDate TIMESTAMP,
    InterviewMode VARCHAR,
    InterviewerId BIGINT,
    InterviewCompleted BOOLEAN,
    InterviewNotes VARCHAR,
    InterviewResult VARCHAR,

    TrainingModuleAssigned BOOLEAN,
    TrainingStartedDate TIMESTAMP,
    TrainingCompletedDate TIMESTAMP,
    TrainingCompletionPercentage NUMERIC,
    TrainingPassed BOOLEAN,

    CertificationTestAssigned BOOLEAN,
    CertificationTestDate TIMESTAMP,
    CertificationTestScore NUMERIC,
    CertificationTestPassed BOOLEAN,
    CertificationCertificateUrl VARCHAR,

    ActivationStatus VARCHAR,
    ActivatedDate TIMESTAMP,
    ActivatedBy BIGINT,
    RejectionReason VARCHAR,

    BankDetailsSubmitted BOOLEAN,
    BankDetailsVerified BOOLEAN,
    BankVerificationDate TIMESTAMP,

    CreatedDate TIMESTAMP,
    ModifiedDate TIMESTAMP,

    FullName VARCHAR,
    Email VARCHAR,
    Phone VARCHAR,
    DateOfBirth DATE,
    IdentityType VARCHAR,
    IdentityNumber VARCHAR,
    IDProofUrl VARCHAR,
    PhotoUrl VARCHAR,
    CurrentStatus VARCHAR,
    AuthorityLevel VARCHAR,
    Address VARCHAR,
    SupervisorType VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,

        r.c_document_verification_status,
        r.c_document_verified_by,
        r.c_document_verified_date,
        r.c_document_rejection_reason,

        r.c_interview_scheduled,
        r.c_interview_date,
        r.c_interview_mode,
        r.c_interviewer_id,
        r.c_interview_completed,
        r.c_interview_notes,
        r.c_interview_result,

        r.c_training_module_assigned,
        r.c_training_started_date,
        r.c_training_completed_date,
        r.c_training_completion_percentage,
        r.c_training_passed,

        r.c_certification_test_assigned,
        r.c_certification_test_date,
        r.c_certification_test_score,
        r.c_certification_test_passed,
        r.c_certification_certificate_url,

        r.c_activation_status,
        r.c_activated_date,
        r.c_activated_by,
        r.c_rejection_reason,

        r.c_bank_details_submitted,
        r.c_bank_details_verified,
        r.c_bank_verification_date,

        r.c_createddate,
        r.c_modifieddate,

        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_date_of_birth,
        s.c_identity_type,
        s.c_identity_number,
        s.c_identity_proof_url,
        s.c_photo_url,
        s.c_current_status,
        s.c_authority_level,
        s.c_address_line1,
        s.c_supervisor_type

    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_registration_id = p_RegistrationId;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationBySupervisorId(
    p_SupervisorId BIGINT
)
RETURNS TABLE (
    RegistrationId BIGINT,
    SupervisorId BIGINT,
    RegistrationNumber VARCHAR,
    RegisteredDate TIMESTAMP,
    Source VARCHAR,
    ReferralCode VARCHAR,

    DocumentVerificationStatus VARCHAR,
    DocumentVerifiedBy BIGINT,
    DocumentVerifiedDate TIMESTAMP,
    DocumentRejectionReason VARCHAR,

    InterviewScheduled BOOLEAN,
    InterviewDate TIMESTAMP,
    InterviewMode VARCHAR,
    InterviewerId BIGINT,
    InterviewCompleted BOOLEAN,
    InterviewNotes VARCHAR,
    InterviewResult VARCHAR,

    TrainingModuleAssigned BOOLEAN,
    TrainingStartedDate TIMESTAMP,
    TrainingCompletedDate TIMESTAMP,
    TrainingCompletionPercentage NUMERIC,
    TrainingPassed BOOLEAN,

    CertificationTestAssigned BOOLEAN,
    CertificationTestDate TIMESTAMP,
    CertificationTestScore NUMERIC,
    CertificationTestPassed BOOLEAN,
    CertificationCertificateUrl VARCHAR,

    ActivationStatus VARCHAR,
    ActivatedDate TIMESTAMP,
    ActivatedBy BIGINT,
    RejectionReason VARCHAR,

    BankDetailsSubmitted BOOLEAN,
    BankDetailsVerified BOOLEAN,
    BankVerificationDate TIMESTAMP,

    CreatedDate TIMESTAMP,
    ModifiedDate TIMESTAMP,

    FullName VARCHAR,
    Email VARCHAR,
    Phone VARCHAR,
    DateOfBirth DATE,
    IdentityType VARCHAR,
    IdentityNumber VARCHAR,
    IDProofUrl VARCHAR,
    PhotoUrl VARCHAR,
    CurrentStatus VARCHAR,
    AuthorityLevel VARCHAR,
    Address VARCHAR,
    SupervisorType VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,

        r.c_document_verification_status,
        r.c_document_verified_by,
        r.c_document_verified_date,
        r.c_document_rejection_reason,

        r.c_interview_scheduled,
        r.c_interview_date,
        r.c_interview_mode,
        r.c_interviewer_id,
        r.c_interview_completed,
        r.c_interview_notes,
        r.c_interview_result,

        r.c_training_module_assigned,
        r.c_training_started_date,
        r.c_training_completed_date,
        r.c_training_completion_percentage,
        r.c_training_passed,

        r.c_certification_test_assigned,
        r.c_certification_test_date,
        r.c_certification_test_score,
        r.c_certification_test_passed,
        r.c_certification_certificate_url,

        r.c_activation_status,
        r.c_activated_date,
        r.c_activated_by,
        r.c_rejection_reason,

        r.c_bank_details_submitted,
        r.c_bank_details_verified,
        r.c_bank_verification_date,

        r.c_createddate,
        r.c_modifieddate,

        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_date_of_birth,
        s.c_identity_type,
        s.c_identity_number,
        s.c_identity_proof_url,
        s.c_photo_url,
        s.c_current_status,
        s.c_authority_level,
        s.c_address_line1,
        s.c_supervisor_type

    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_supervisor_id = p_SupervisorId;

END;
$$;

-- =============================================
-- STAGE PROGRESSION
-- =============================================

CREATE OR REPLACE FUNCTION sp_RejectRegistration(
    p_RegistrationId BIGINT,
    p_RejectedBy BIGINT,
    p_Reason TEXT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_activation_status = 'REJECTED',
        c_rejection_reason = p_Reason,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    UPDATE t_sys_supervisor s
    SET c_current_status = 'REJECTED',
        c_status_reason = p_Reason,
        c_modifieddate = NOW(),
        c_modifiedby = p_RejectedBy
    FROM t_sys_supervisor_registration r
    WHERE s.c_supervisor_id = r.c_supervisor_id
      AND r.c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

-- =============================================
-- STAGE 1: DOCUMENT VERIFICATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_SubmitIdentityProofDocuments(
    p_RegistrationId BIGINT,
    p_IDProofUrl VARCHAR DEFAULT NULL,
    p_AddressProofUrl VARCHAR DEFAULT NULL,
    p_PhotoUrl VARCHAR DEFAULT NULL,
    p_CancelledChequeUrl VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success BOOLEAN)
LANGUAGE plpgsql
AS $$
DECLARE
    v_RowCount INT;
BEGIN

    UPDATE t_sys_supervisor s
    SET c_identity_proof_url = COALESCE(NULLIF(p_IDProofUrl, ''), s.c_identity_proof_url),
        c_address_url = COALESCE(NULLIF(p_AddressProofUrl, ''), s.c_address_url),
        c_photo_url = COALESCE(NULLIF(p_PhotoUrl, ''), s.c_photo_url),
        c_cancelled_cheque_url = COALESCE(NULLIF(p_CancelledChequeUrl, ''), s.c_cancelled_cheque_url),
        c_modifieddate = NOW()
    FROM t_sys_supervisor_registration r
    WHERE s.c_supervisor_id = r.c_supervisor_id
      AND r.c_registration_id = p_RegistrationId;

    GET DIAGNOSTICS v_RowCount = ROW_COUNT;

    UPDATE t_sys_supervisor_registration
    SET c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT (v_RowCount > 0);

END;
$$;

CREATE OR REPLACE FUNCTION sp_SubmitDocumentVerification(
    p_RegistrationId BIGINT,
    p_VerifiedBy BIGINT,
    p_Passed BOOLEAN,
    p_IDProofVerified BOOLEAN,
    p_AddressProofVerified BOOLEAN,
    p_PhotoVerified BOOLEAN,
    p_VerificationNotes TEXT DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_document_verification_status = CASE WHEN p_Passed THEN 'VERIFIED' ELSE 'REJECTED' END,
        c_doc_verification_status      = CASE WHEN p_Passed THEN 'VERIFIED' ELSE 'REJECTED' END,
        c_document_verified_by = p_VerifiedBy,
        c_document_verified_date = NOW(),
        c_document_rejection_reason = CASE WHEN NOT p_Passed THEN p_VerificationNotes ELSE NULL END,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationsPendingDocumentVerification()
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_document_verified_by BIGINT,
    c_document_verified_date TIMESTAMP,
    c_document_rejection_reason VARCHAR,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR,
    c_identity_type VARCHAR,
    c_identity_number VARCHAR,
    c_identity_proof_url VARCHAR,
    c_photo_url VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,
        r.c_document_verification_status,
        r.c_document_verified_by,
        r.c_document_verified_date,
        r.c_document_rejection_reason,
        r.c_modifieddate,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_identity_type,
        s.c_identity_number,
        s.c_identity_proof_url,
        s.c_photo_url
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_document_verification_status = 'PENDING'
    ORDER BY r.c_registered_date ASC;

END;
$$;

-- =============================================
-- STAGE 2: INTERVIEW
-- =============================================

CREATE OR REPLACE FUNCTION sp_ScheduleQuickInterview(
    p_RegistrationId BIGINT,
    p_InterviewDateTime TIMESTAMP,
    p_InterviewType VARCHAR,
    p_InterviewerName VARCHAR,
    p_ScheduledBy BIGINT,
    p_MeetingLink VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_interview_scheduled = 1,
        c_interview_date = p_InterviewDateTime,
        c_interview_mode = p_InterviewType,
        c_interviewer_id = p_ScheduledBy,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_SubmitQuickInterviewResult(
    p_RegistrationId BIGINT,
    p_InterviewedBy BIGINT,
    p_Passed BOOLEAN,
    p_Score DECIMAL(5,2),
    p_Notes TEXT DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_interview_completed = 1,
        c_interview_result = CASE WHEN p_Passed THEN 'PASSED' ELSE 'FAILED' END,
        c_interview_notes = p_Notes,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationsPendingInterview()
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,
        r.c_document_verification_status,
        r.c_interview_scheduled,
        r.c_interview_date,
        r.c_interview_mode,
        r.c_interviewer_id,
        r.c_interview_completed,
        r.c_interview_notes,
        r.c_interview_result,
        r.c_training_module_assigned,
        r.c_training_started_date,
        r.c_training_completed_date,
        r.c_training_completion_percentage,
        r.c_training_passed,
        r.c_certification_test_assigned,
        r.c_certification_test_date,
        r.c_certification_test_score,
        r.c_certification_test_passed,
        r.c_certification_certificate_url,
        r.c_activation_status,
        r.c_activated_date,
        r.c_activated_by,
        r.c_rejection_reason,
        r.c_bank_details_submitted,
        r.c_bank_details_verified,
        r.c_bank_verification_date,
        r.c_createddate,
        r.c_modifieddate,
        s.c_full_name,
        s.c_email,
        s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_document_verification_status = 'VERIFIED'
      AND r.c_interview_completed = 0
    ORDER BY r.c_interview_date ASC;

END;
$$;

-- =============================================
-- STAGE 3: TRAINING
-- =============================================

CREATE OR REPLACE FUNCTION sp_AssignCondensedTraining(
    p_RegistrationId BIGINT,
    p_ModuleIds VARCHAR,
    p_AssignedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
    v_ModuleId BIGINT;
BEGIN

    SELECT c_supervisor_id
    INTO v_SupervisorId
    FROM t_sys_supervisor_registration
    WHERE c_registration_id = p_RegistrationId;

    -- Mark training as assigned
    UPDATE t_sys_supervisor_registration
    SET c_training_module_assigned = 1,
        c_training_started_date = NOW(),
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    -- Insert training progress using string split (PostgreSQL way)
    FOR v_ModuleId IN
        SELECT CAST(value AS BIGINT)
        FROM unnest(string_to_array(p_ModuleIds, ',')) AS value
    LOOP
        IF NOT EXISTS (
            SELECT 1
            FROM t_sys_supervisor_training_progress
            WHERE c_supervisor_id = v_SupervisorId
              AND c_module_id = v_ModuleId
        )
        THEN
            INSERT INTO t_sys_supervisor_training_progress (
                c_supervisor_id, c_module_id, c_started_date
            )
            VALUES (
                v_SupervisorId, v_ModuleId, NOW()
            );
        END IF;
    END LOOP;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_CompleteCondensedTraining(
    p_RegistrationId BIGINT,
    p_CompletedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_training_passed = 1,
        c_training_completed_date = NOW(),
        c_training_completion_percentage = 100.00,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    -- Update supervisor training date
    UPDATE t_sys_supervisor s
    SET c_training_completed_date = NOW(),
        c_modifieddate = NOW()
    FROM t_sys_supervisor_registration r
    WHERE s.c_supervisor_id = r.c_supervisor_id
      AND r.c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationsPendingTraining()
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,
        r.c_document_verification_status,
        r.c_interview_scheduled,
        r.c_interview_date,
        r.c_interview_mode,
        r.c_interviewer_id,
        r.c_interview_completed,
        r.c_interview_notes,
        r.c_interview_result,
        r.c_training_module_assigned,
        r.c_training_started_date,
        r.c_training_completed_date,
        r.c_training_completion_percentage,
        r.c_training_passed,
        r.c_certification_test_assigned,
        r.c_certification_test_date,
        r.c_certification_test_score,
        r.c_certification_test_passed,
        r.c_certification_certificate_url,
        r.c_activation_status,
        r.c_activated_date,
        r.c_activated_by,
        r.c_rejection_reason,
        r.c_bank_details_submitted,
        r.c_bank_details_verified,
        r.c_bank_verification_date,
        r.c_createddate,
        r.c_modifieddate,
        s.c_full_name,
        s.c_email,
        s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_interview_result = 'PASSED'
      AND r.c_training_passed = 0
    ORDER BY r.c_registered_date ASC;

END;
$$;

-- =============================================
-- STAGE 4: CERTIFICATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_ScheduleQuickCertification(
    p_RegistrationId BIGINT,
    p_ExamDate TIMESTAMP,
    p_ScheduledBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_certification_test_assigned = 1,
        c_certification_test_date = p_ExamDate,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_SubmitQuickCertificationResult(
    p_RegistrationId BIGINT,
    p_Passed BOOLEAN,
    p_ExamScore DECIMAL(5,2),
    p_ExamDate TIMESTAMP,
    p_EvaluatedBy BIGINT,
    p_CertificateNumber VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor_registration
    SET c_certification_test_passed = p_Passed,
        c_certification_test_score = p_ExamScore,
        c_certification_certificate_url = p_CertificateNumber,
        c_modifieddate = NOW()
    WHERE c_registration_id = p_RegistrationId;

    -- Update supervisor certification
    IF p_Passed THEN
        UPDATE t_sys_supervisor s
        SET c_certification_date = p_ExamDate,
            c_certification_status = 'CERTIFIED',
            c_certification_valid_until = p_ExamDate + INTERVAL '1 year',
            c_modifieddate = NOW()
        FROM t_sys_supervisor_registration r
        WHERE s.c_supervisor_id = r.c_supervisor_id
          AND r.c_registration_id = p_RegistrationId;
    END IF;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationsPendingCertification()
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id,
        r.c_supervisor_id,
        r.c_registration_number,
        r.c_registered_date,
        r.c_source,
        r.c_referral_code,
        r.c_document_verification_status,
        r.c_interview_scheduled,
        r.c_interview_date,
        r.c_interview_mode,
        r.c_interviewer_id,
        r.c_interview_completed,
        r.c_interview_notes,
        r.c_interview_result,
        r.c_training_module_assigned,
        r.c_training_started_date,
        r.c_training_completed_date,
        r.c_training_completion_percentage,
        r.c_training_passed,
        r.c_certification_test_assigned,
        r.c_certification_test_date,
        r.c_certification_test_score,
        r.c_certification_test_passed,
        r.c_certification_certificate_url,
        r.c_activation_status,
        r.c_activated_date,
        r.c_activated_by,
        r.c_rejection_reason,
        r.c_bank_details_submitted,
        r.c_bank_details_verified,
        r.c_bank_verification_date,
        r.c_createddate,
        r.c_modifieddate,
        s.c_full_name,
        s.c_email,
        s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_training_passed = 1
      AND r.c_certification_test_passed = 0
    ORDER BY r.c_certification_test_date ASC;

END;
$$;

-- =============================================
-- BANKING DETAILS
-- =============================================

CREATE OR REPLACE FUNCTION sp_SubmitBankingDetails(
    p_SupervisorId BIGINT,
    p_AccountHolderName VARCHAR,
    p_BankName VARCHAR,
    p_AccountNumber VARCHAR,
    p_IFSCCode VARCHAR,
    p_BranchName VARCHAR,
    p_AccountType VARCHAR,
    p_CancelledChequeUrl VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor
    SET c_bank_account_holder_name = p_AccountHolderName,
        c_bank_name = p_BankName,
        c_bank_account_number = p_AccountNumber,
        c_bank_ifsc = p_IFSCCode,
        c_bank_branch_name = p_BranchName,
        c_bank_account_type = p_AccountType,
        c_modifieddate = NOW()
    WHERE c_supervisor_id = p_SupervisorId;

    -- Mark banking details as submitted in registration
    UPDATE t_sys_supervisor_registration
    SET c_bank_details_submitted = 1,
        c_modifieddate = NOW()
    WHERE c_supervisor_id = p_SupervisorId;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetBankingDetails(
    p_SupervisorId BIGINT
)
RETURNS TABLE (
    SupervisorId BIGINT,
    AccountHolderName VARCHAR,
    BankName VARCHAR,
    AccountNumber VARCHAR,
    IFSCCode VARCHAR,
    BranchName VARCHAR,
    AccountType VARCHAR,
    CancelledChequeUrl VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_supervisor_id AS SupervisorId,
        c_bank_account_holder_name AS AccountHolderName,
        c_bank_name AS BankName,
        c_bank_account_number AS AccountNumber,
        c_bank_ifsc AS IFSCCode,
        COALESCE(c_bank_branch_name, '') AS BranchName,
        COALESCE(c_bank_account_type, '') AS AccountType,
        COALESCE(c_cancelled_cheque_url, '') AS CancelledChequeUrl
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId;

END;
$$;

-- =============================================
-- FINAL ACTIVATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_ActivateRegisteredSupervisor(
    p_RegistrationId BIGINT,
    p_ActivatedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    BEGIN
        -- Update registration status
        UPDATE t_sys_supervisor_registration
        SET c_activation_status = 'ACTIVATED',
            c_activated_date = NOW(),
            c_activated_by = p_ActivatedBy,
            c_modifieddate = NOW()
        WHERE c_registration_id = p_RegistrationId;

        -- Update supervisor to ACTIVE
        UPDATE t_sys_supervisor s
        SET c_current_status = 'ACTIVE',
            c_is_available = 1,
            c_modifieddate = NOW(),
            c_modifiedby = p_ActivatedBy
        FROM t_sys_supervisor_registration r
        WHERE s.c_supervisor_id = r.c_supervisor_id
          AND r.c_registration_id = p_RegistrationId;

        RETURN QUERY SELECT 1 AS Success;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;

END;
$$;

-- =============================================
-- WORKFLOW TRACKING
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetRegistrationProgress(
    p_RegistrationId BIGINT
)
RETURNS TABLE (
    RegistrationId BIGINT,
    RegistrationNumber VARCHAR,
    DocumentVerificationStage VARCHAR,
    DocumentVerifiedDate TIMESTAMP,
    InterviewStage VARCHAR,
    InterviewDate TIMESTAMP,
    TrainingStage VARCHAR,
    TrainingCompletedDate TIMESTAMP,
    CertificationStage VARCHAR,
    CertificationDate TIMESTAMP,
    ActivationStatus VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.c_registration_id AS RegistrationId,
        r.c_registration_number AS RegistrationNumber,
        -- Stage 1
        CASE
            WHEN r.c_document_verification_status = 'VERIFIED' THEN 'COMPLETED'
            WHEN r.c_document_verification_status = 'REJECTED' THEN 'REJECTED'
            ELSE 'PENDING'
        END AS DocumentVerificationStage,
        r.c_document_verified_date AS DocumentVerifiedDate,
        -- Stage 2
        CASE
            WHEN r.c_interview_result = 'PASSED' THEN 'COMPLETED'
            WHEN r.c_interview_result = 'FAILED' THEN 'FAILED'
            WHEN r.c_interview_scheduled = TRUE THEN 'SCHEDULED'
            ELSE 'PENDING'
        END AS InterviewStage,
        r.c_interview_date AS InterviewDate,
        -- Stage 3
        CASE
            WHEN r.c_training_passed = TRUE THEN 'COMPLETED'
            WHEN r.c_training_module_assigned = TRUE THEN 'IN_PROGRESS'
            ELSE 'PENDING'
        END AS TrainingStage,
        r.c_training_completed_date AS TrainingCompletedDate,
        -- Stage 4
        CASE
            WHEN r.c_certification_test_passed = TRUE THEN 'COMPLETED'
            WHEN r.c_certification_test_assigned = TRUE THEN 'SCHEDULED'
            ELSE 'PENDING'
        END AS CertificationStage,
        r.c_certification_test_date AS CertificationDate,
        -- Overall
        r.c_activation_status AS ActivationStatus
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = p_RegistrationId;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationWorkflowStatus(
    p_RegistrationId BIGINT
)
RETURNS TABLE (
    RegistrationId BIGINT,
    RegistrationNumber VARCHAR,
    SupervisorId BIGINT,
    CurrentStage VARCHAR,
    ActivationStatus VARCHAR,
    RegisteredDate TIMESTAMP,
    LastModifiedDate TIMESTAMP
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_CurrentStage VARCHAR;
BEGIN

    SELECT
        CASE
            WHEN r.c_activation_status = 'ACTIVATED' THEN 'ACTIVATED'
            WHEN r.c_certification_test_passed = TRUE THEN 'CERTIFICATION_PASSED'
            WHEN r.c_certification_test_assigned = TRUE THEN 'CERTIFICATION'
            WHEN r.c_training_passed = TRUE THEN 'TRAINING_PASSED'
            WHEN r.c_training_module_assigned = TRUE THEN 'TRAINING'
            WHEN r.c_interview_result = 'PASSED' THEN 'INTERVIEW_PASSED'
            WHEN r.c_interview_scheduled = TRUE THEN 'INTERVIEW'
            WHEN r.c_document_verification_status = 'VERIFIED' THEN 'DOCUMENT_VERIFIED'
            ELSE 'DOCUMENT_VERIFICATION'
        END
    INTO v_CurrentStage
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = p_RegistrationId;

    RETURN QUERY
    SELECT
        r.c_registration_id AS RegistrationId,
        r.c_registration_number AS RegistrationNumber,
        r.c_supervisor_id AS SupervisorId,
        v_CurrentStage AS CurrentStage,
        r.c_activation_status AS ActivationStatus,
        r.c_registered_date AS RegisteredDate,
        r.c_modifieddate AS LastModifiedDate
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = p_RegistrationId;

END;
$$;

-- =============================================
-- ADMIN QUERIES
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetAllRegistrations(
    p_Status VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR,
    c_current_status VARCHAR,
    c_supervisor_type VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.*,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_current_status,
        s.c_supervisor_type
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (p_Status IS NULL OR r.c_activation_status = p_Status)
    ORDER BY r.c_registered_date DESC;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationsByStage(
    p_Stage VARCHAR
)
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.*,
        s.c_full_name,
        s.c_email,
        s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (
        (p_Stage = 'DOCUMENT_VERIFICATION' AND r.c_document_verification_status = 'PENDING')
        OR (p_Stage = 'INTERVIEW' AND r.c_document_verification_status = 'VERIFIED' AND r.c_interview_completed = FALSE)
        OR (p_Stage = 'TRAINING' AND r.c_interview_result = 'PASSED' AND r.c_training_passed = FALSE)
        OR (p_Stage = 'CERTIFICATION' AND r.c_training_passed = TRUE AND r.c_certification_test_passed = FALSE)
        OR (p_Stage = 'ACTIVATED' AND r.c_activation_status = 'ACTIVATED')
    )
    ORDER BY r.c_registered_date ASC;

END;
$$;

CREATE OR REPLACE FUNCTION sp_GetRegistrationStatistics()
RETURNS TABLE (
    TotalRegistrations BIGINT,
    Activated BIGINT,
    Rejected BIGINT,
    Pending BIGINT,
    PendingDocVerification BIGINT,
    PendingInterview BIGINT,
    PendingTraining BIGINT,
    PendingCertification BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        COUNT(*) AS TotalRegistrations,
        SUM(CASE WHEN c_activation_status = 'ACTIVATED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_activation_status = 'REJECTED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_activation_status = 'PENDING' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_document_verification_status = 'PENDING' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_document_verification_status = 'VERIFIED' AND c_interview_completed = FALSE THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_interview_result = 'PASSED' AND c_training_passed = FALSE THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_training_passed = TRUE AND c_certification_test_passed = FALSE THEN 1 ELSE 0 END)
    FROM t_sys_supervisor_registration;

END;
$$;

CREATE OR REPLACE FUNCTION sp_SearchRegistrations(
    p_Name VARCHAR DEFAULT NULL,
    p_Email VARCHAR DEFAULT NULL,
    p_Phone VARCHAR DEFAULT NULL,
    p_Status VARCHAR DEFAULT NULL,
    p_CurrentStage VARCHAR DEFAULT NULL,
    p_ZoneId BIGINT DEFAULT NULL,
    p_RegisteredFrom TIMESTAMP DEFAULT NULL,
    p_RegisteredTo TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    c_registration_id BIGINT,
    c_supervisor_id BIGINT,
    c_registration_number VARCHAR,
    c_registered_date TIMESTAMP,
    c_source VARCHAR,
    c_referral_code VARCHAR,
    c_document_verification_status VARCHAR,
    c_interview_scheduled BOOLEAN,
    c_interview_date TIMESTAMP,
    c_interview_mode VARCHAR,
    c_interviewer_id BIGINT,
    c_interview_completed BOOLEAN,
    c_interview_notes TEXT,
    c_interview_result VARCHAR,
    c_training_module_assigned BOOLEAN,
    c_training_started_date TIMESTAMP,
    c_training_completed_date TIMESTAMP,
    c_training_completion_percentage DECIMAL,
    c_training_passed BOOLEAN,
    c_certification_test_assigned BOOLEAN,
    c_certification_test_date TIMESTAMP,
    c_certification_test_score DECIMAL,
    c_certification_test_passed BOOLEAN,
    c_certification_certificate_url VARCHAR,
    c_activation_status VARCHAR,
    c_activated_date TIMESTAMP,
    c_activated_by BIGINT,
    c_rejection_reason TEXT,
    c_bank_details_submitted BOOLEAN,
    c_bank_details_verified BOOLEAN,
    c_bank_verification_date TIMESTAMP,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP,
    c_full_name VARCHAR,
    c_email VARCHAR,
    c_phone VARCHAR,
    c_current_status VARCHAR,
    c_supervisor_type VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        r.*,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_current_status,
        s.c_supervisor_type
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s
        ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (p_Name IS NULL OR s.c_full_name ILIKE '%' || p_Name || '%')
      AND (p_Email IS NULL OR s.c_email ILIKE '%' || p_Email || '%')
      AND (p_Phone IS NULL OR s.c_phone ILIKE '%' || p_Phone || '%')
      AND (p_Status IS NULL OR r.c_activation_status = p_Status)
      AND (p_RegisteredFrom IS NULL OR r.c_registered_date >= p_RegisteredFrom)
      AND (p_RegisteredTo IS NULL OR r.c_registered_date <= p_RegisteredTo)
    ORDER BY r.c_registered_date DESC;

END;
$$;
