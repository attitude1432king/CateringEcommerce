-- =============================================
-- Supervisor Careers Application Stored Functions (PostgreSQL)
-- Maps to: CareersApplicationRepository.cs
-- 29 Functions for Careers Portal 6-Stage Pipeline
-- (Excludes sp_ProgressCareersApplication already in
--  Supervisor_Management_StoredProcedures.sql)
-- =============================================
-- NOTE: All routines are PostgreSQL FUNCTIONs (not PROCEDUREs) so they can be
-- invoked through SqlQueryTranslator.BuildFunctionCall as
-- `SELECT * FROM sp_xxx(@p1, @p2, ...)`.
-- =============================================


-- =============================================
-- APPLICATION SUBMISSION
-- =============================================

CREATE OR REPLACE FUNCTION sp_SubmitCareersApplication(
    p_firstname           VARCHAR(50),
    p_lastname            VARCHAR(50),
    p_email               VARCHAR(100),
    p_phone               VARCHAR(15),
    p_address             VARCHAR(500),
    p_dateofbirth         DATE,
    p_resumeurl           VARCHAR(500),
    p_coverletter         VARCHAR(2000),
    p_yearsofexperience   INTEGER,
    p_previousemployer    VARCHAR(200),
    p_references          VARCHAR(2000)
)
RETURNS TABLE (applicationid BIGINT)
LANGUAGE plpgsql AS $$
DECLARE
    v_supervisor_id   BIGINT;
    v_full_name       VARCHAR(100);
    v_app_number      VARCHAR(50);
    v_application_id  BIGINT;
BEGIN
    v_full_name := p_firstname || ' ' || p_lastname;

    INSERT INTO t_sys_supervisor (
        c_supervisor_type, c_full_name, c_email, c_phone,
        c_date_of_birth, c_address_line1, c_city, c_state, c_pincode,
        c_resume_url, c_years_of_experience, c_previous_employer,
        c_current_status, c_authority_level,
        c_compensation_type, c_createddate
    )
    VALUES (
        'CAREER', v_full_name, p_email, p_phone,
        p_dateofbirth, p_address, '', '', '',
        p_resumeurl, p_yearsofexperience, p_previousemployer,
        'APPLIED', 'BASIC',
        'MONTHLY_SALARY', NOW()
    )
    RETURNING c_supervisor_id INTO v_supervisor_id;

    v_app_number := 'CAR-' || TO_CHAR(NOW(), 'YYYYMMDD') || '-' || v_supervisor_id::VARCHAR;

    INSERT INTO t_sys_careers_application (
        c_supervisor_id, c_application_number, c_applied_date,
        c_source
    )
    VALUES (
        v_supervisor_id, v_app_number, NOW(),
        'WEBSITE'
    )
    RETURNING c_application_id INTO v_application_id;

    RETURN QUERY SELECT v_application_id;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetCareersApplicationById(
    p_applicationid BIGINT
)
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_date_of_birth                        DATE,
    c_resume_url                           VARCHAR,
    c_years_of_experience                  INTEGER,
    c_previous_employer                    VARCHAR,
    c_photo_url                            VARCHAR,
    c_current_status                       VARCHAR,
    c_authority_level                      VARCHAR,
    c_address_line1                        VARCHAR,
    c_supervisor_type                      VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone, s.c_date_of_birth,
           s.c_resume_url, s.c_years_of_experience, s.c_previous_employer,
           s.c_photo_url, s.c_current_status, s.c_authority_level,
           s.c_address_line1, s.c_supervisor_type
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_application_id = p_applicationid;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetCareersApplicationBySupervisorId(
    p_supervisorid BIGINT
)
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_date_of_birth                        DATE,
    c_resume_url                           VARCHAR,
    c_years_of_experience                  INTEGER,
    c_previous_employer                    VARCHAR,
    c_photo_url                            VARCHAR,
    c_current_status                       VARCHAR,
    c_authority_level                      VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone, s.c_date_of_birth,
           s.c_resume_url, s.c_years_of_experience, s.c_previous_employer,
           s.c_photo_url, s.c_current_status, s.c_authority_level
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_supervisor_id = p_supervisorid;
END;
$$;


-- =============================================
-- REJECTION
-- =============================================

CREATE OR REPLACE FUNCTION sp_RejectCareersApplication(
    p_applicationid BIGINT,
    p_rejectedby    BIGINT,
    p_reason        VARCHAR(1000)
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_final_decision = 'REJECTED',
        c_final_decision_date = NOW(),
        c_final_decision_by = p_rejectedby,
        c_rejection_reason = p_reason,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_current_status = 'REJECTED',
        c_status_reason = p_reason,
        c_modifieddate = NOW(),
        c_modifiedby = p_rejectedby
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


-- =============================================
-- STAGE 2: RESUME SCREENING
-- =============================================

CREATE OR REPLACE FUNCTION sp_SubmitResumeScreening(
    p_applicationid    BIGINT,
    p_screenedby       BIGINT,
    p_passed           BOOLEAN,
    p_resumescore      DECIMAL(5,2),
    p_screeningnotes   VARCHAR(1000)
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_resume_screened = TRUE,
        c_resume_screened_by = p_screenedby,
        c_resume_screened_date = NOW(),
        c_resume_screening_notes = p_screeningnotes,
        c_resume_screening_status = CASE WHEN p_passed THEN 'PASSED' ELSE 'FAILED' END,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    IF p_passed THEN
        UPDATE t_sys_supervisor s
        SET c_current_status = 'RESUME_SCREENED',
            c_modifieddate = NOW()
        FROM t_sys_careers_application a
        WHERE s.c_supervisor_id = a.c_supervisor_id
          AND a.c_application_id = p_applicationid;
    END IF;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsForResumeScreening()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_resume_url                           VARCHAR,
    c_years_of_experience                  INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone,
           s.c_resume_url, s.c_years_of_experience
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_resume_screened = FALSE
    ORDER BY a.c_applied_date ASC;
END;
$$;


-- =============================================
-- STAGE 3: INTERVIEW
-- =============================================

CREATE OR REPLACE FUNCTION sp_ScheduleInterview(
    p_applicationid       BIGINT,
    p_interviewdatetime   TIMESTAMP,
    p_interviewtype       VARCHAR(20),
    p_interviewername     VARCHAR(100),
    p_meetinglink         VARCHAR(500),
    p_scheduledby         BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_interview_scheduled = TRUE,
        c_interview_date = p_interviewdatetime,
        c_interview_mode = p_interviewtype,
        c_interviewer_id = p_scheduledby,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_SubmitInterviewResult(
    p_applicationid     BIGINT,
    p_interviewedby     BIGINT,
    p_passed            BOOLEAN,
    p_interviewscore    DECIMAL(5,2),
    p_interviewnotes    VARCHAR(2000)
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_interview_completed = TRUE,
        c_interview_feedback = p_interviewnotes,
        c_interview_score = p_interviewscore,
        c_interview_result = CASE WHEN p_passed THEN 'PASSED' ELSE 'FAILED' END,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    IF p_passed THEN
        UPDATE t_sys_supervisor s
        SET c_current_status = 'INTERVIEW_PASSED',
            c_modifieddate = NOW()
        FROM t_sys_careers_application a
        WHERE s.c_supervisor_id = a.c_supervisor_id
          AND a.c_application_id = p_applicationid;
    END IF;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsForInterview()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_resume_screening_status = 'PASSED'
      AND a.c_interview_completed = FALSE
    ORDER BY a.c_interview_date ASC;
END;
$$;


-- =============================================
-- STAGE 4: BACKGROUND VERIFICATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_InitiateBackgroundCheck(
    p_applicationid BIGINT,
    p_initiatedby   BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_background_verification_initiated = TRUE,
        c_background_verification_date = NOW(),
        c_background_verification_result = 'PENDING',
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_current_status = 'BACKGROUND_VERIFICATION',
        c_background_check_status = 'PENDING',
        c_modifieddate = NOW()
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_SubmitBackgroundCheckResult(
    p_applicationid           BIGINT,
    p_passed                  BOOLEAN,
    p_verificationagency      VARCHAR(100),
    p_verificationdate        TIMESTAMP,
    p_verificationreporturl   VARCHAR(500),
    p_notes                   VARCHAR(1000),
    p_submittedby             BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_background_verification_result = CASE WHEN p_passed THEN 'CLEAR' ELSE 'ISSUES_FOUND' END,
        c_background_verification_agency = p_verificationagency,
        c_background_verification_date = p_verificationdate,
        c_background_verification_report_url = p_verificationreporturl,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_background_check_status = CASE WHEN p_passed THEN 'PASSED' ELSE 'FAILED' END,
        c_background_check_date = p_verificationdate,
        c_modifieddate = NOW()
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsPendingBackgroundCheck()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_interview_result = 'PASSED'
      AND (a.c_background_verification_result = 'PENDING' OR a.c_background_verification_initiated = FALSE)
    ORDER BY a.c_applied_date ASC;
END;
$$;


-- =============================================
-- STAGE 5: TRAINING
-- =============================================

CREATE OR REPLACE FUNCTION sp_AssignTraining(
    p_applicationid BIGINT,
    p_moduleids     VARCHAR(500),
    p_assignedby    BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_supervisor_id BIGINT;
    v_module_id     BIGINT;
BEGIN
    SELECT c_supervisor_id INTO v_supervisor_id
    FROM t_sys_careers_application
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_careers_application
    SET c_training_start_date = CURRENT_DATE,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor
    SET c_current_status = 'TRAINING',
        c_modifieddate = NOW()
    WHERE c_supervisor_id = v_supervisor_id;

    FOR v_module_id IN
        SELECT trim(both ' ' FROM x)::BIGINT
        FROM unnest(string_to_array(p_moduleids, ',')) AS x
        WHERE trim(both ' ' FROM x) <> ''
    LOOP
        IF NOT EXISTS (
            SELECT 1 FROM t_sys_supervisor_training_progress
            WHERE c_supervisor_id = v_supervisor_id AND c_module_id = v_module_id
        ) THEN
            INSERT INTO t_sys_supervisor_training_progress (c_supervisor_id, c_module_id, c_started_date)
            VALUES (v_supervisor_id, v_module_id, NOW());
        END IF;
    END LOOP;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_RecordTrainingProgress(
    p_applicationid       BIGINT,
    p_moduleid            BIGINT,
    p_progresspercentage  INTEGER
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_supervisor_id   BIGINT;
    v_overall_progress DECIMAL(5,2);
BEGIN
    SELECT c_supervisor_id INTO v_supervisor_id
    FROM t_sys_careers_application
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor_training_progress
    SET c_completion_percentage = p_progresspercentage,
        c_last_attempt_date = NOW(),
        c_passed = CASE WHEN p_progresspercentage >= 100 THEN TRUE ELSE FALSE END,
        c_completed_date = CASE WHEN p_progresspercentage >= 100 THEN NOW() ELSE NULL END
    WHERE c_supervisor_id = v_supervisor_id AND c_module_id = p_moduleid;

    SELECT AVG(c_completion_percentage) INTO v_overall_progress
    FROM t_sys_supervisor_training_progress
    WHERE c_supervisor_id = v_supervisor_id;

    UPDATE t_sys_careers_application
    SET c_training_attendance_percentage = v_overall_progress,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_CompleteTraining(
    p_applicationid BIGINT,
    p_completedby   BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_training_completed = TRUE,
        c_training_end_date = CURRENT_DATE,
        c_training_attendance_percentage = 100.00,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_training_completed_date = NOW(),
        c_modifieddate = NOW()
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsInTraining()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_background_verification_result = 'CLEAR'
      AND a.c_training_completed = FALSE
      AND s.c_current_status = 'TRAINING'
    ORDER BY a.c_training_start_date ASC;
END;
$$;


-- =============================================
-- STAGE 6: CERTIFICATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_ScheduleCertificationExam(
    p_applicationid BIGINT,
    p_examdate      TIMESTAMP,
    p_scheduledby   BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_certification_test_date = p_examdate,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_SubmitCertificationResult(
    p_applicationid       BIGINT,
    p_passed              BOOLEAN,
    p_examscore           DECIMAL(5,2),
    p_examdate            TIMESTAMP,
    p_certificatenumber   VARCHAR(50),
    p_certificateurl      VARCHAR(500),
    p_evaluatedby         BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_certification_test_score = p_examscore,
        c_certification_passed = p_passed,
        c_certification_certificate_url = p_certificateurl,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    IF p_passed THEN
        UPDATE t_sys_supervisor s
        SET c_certification_date = p_examdate,
            c_certification_status = 'CERTIFIED',
            c_certification_valid_until = (p_examdate + INTERVAL '1 year')::DATE,
            c_current_status = 'CERTIFIED',
            c_modifieddate = NOW()
        FROM t_sys_careers_application a
        WHERE s.c_supervisor_id = a.c_supervisor_id
          AND a.c_application_id = p_applicationid;
    END IF;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsPendingCertification()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_training_completed = TRUE
      AND a.c_certification_passed = FALSE
    ORDER BY a.c_certification_test_date ASC;
END;
$$;


-- =============================================
-- STAGE 7: PROBATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_StartProbation(
    p_applicationid  BIGINT,
    p_probationdays  INTEGER,
    p_startedby      BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_probation_assigned = TRUE,
        c_probation_start_date = CURRENT_DATE,
        c_probation_duration_days = p_probationdays,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_current_status = 'PROBATION',
        c_probation_start_date = CURRENT_DATE,
        c_probation_end_date = (CURRENT_DATE + (p_probationdays || ' days')::INTERVAL)::DATE,
        c_modifieddate = NOW()
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_CompleteProbation(
    p_applicationid BIGINT,
    p_passed        BOOLEAN,
    p_evaluatedby   BIGINT,
    p_evaluation    VARCHAR(2000)
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE t_sys_careers_application
    SET c_probation_evaluation_date = CURRENT_DATE,
        c_probation_evaluation_notes = p_evaluation,
        c_probation_passed = p_passed,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_is_probation_passed = p_passed,
        c_modifieddate = NOW()
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    RETURN QUERY SELECT 1;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsInProbation()
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_probation_start_date_supervisor      DATE,
    c_probation_end_date                   DATE
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone,
           s.c_probation_start_date, s.c_probation_end_date
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_certification_passed = TRUE
      AND a.c_probation_passed IS NULL
      AND s.c_current_status = 'PROBATION'
    ORDER BY s.c_probation_end_date ASC;
END;
$$;


-- =============================================
-- FINAL ACTIVATION
-- =============================================

CREATE OR REPLACE FUNCTION sp_ActivateCareerSupervisor(
    p_applicationid BIGINT,
    p_activatedby   BIGINT
)
RETURNS TABLE (success INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_supervisor_id BIGINT;
BEGIN
    UPDATE t_sys_careers_application
    SET c_final_decision = 'ACCEPTED',
        c_final_decision_date = NOW(),
        c_final_decision_by = p_activatedby,
        c_onboarding_completed = TRUE,
        c_joining_date = CURRENT_DATE,
        c_modifieddate = NOW()
    WHERE c_application_id = p_applicationid;

    UPDATE t_sys_supervisor s
    SET c_current_status = 'ACTIVE',
        c_authority_level = 'INTERMEDIATE',
        c_is_available = TRUE,
        c_modifieddate = NOW(),
        c_modifiedby = p_activatedby
    FROM t_sys_careers_application a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_application_id = p_applicationid;

    SELECT c_supervisor_id INTO v_supervisor_id
    FROM t_sys_careers_application
    WHERE c_application_id = p_applicationid;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (v_supervisor_id, 'STATUS_CHANGED', 'Career supervisor activated after completing full pipeline', 'SUCCESS');

    RETURN QUERY SELECT 1;
END;
$$;


-- =============================================
-- WORKFLOW TRACKING
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetApplicationProgress(
    p_applicationid BIGINT
)
RETURNS TABLE (
    ApplicationId            BIGINT,
    ApplicationNumber        VARCHAR,
    ApplicationStage         VARCHAR,
    AppliedDate              TIMESTAMP,
    ResumeScreeningStage     VARCHAR,
    ResumeScreenedDate       TIMESTAMP,
    ResumeScreeningResult    VARCHAR,
    InterviewStage           VARCHAR,
    InterviewDate            TIMESTAMP,
    InterviewScore           DECIMAL(5,2),
    BackgroundCheckStage     VARCHAR,
    BackgroundCheckDate      TIMESTAMP,
    TrainingStage            VARCHAR,
    TrainingStartDate        DATE,
    TrainingEndDate          DATE,
    TrainingProgress         DECIMAL(5,2),
    CertificationStage       VARCHAR,
    CertificationDate        TIMESTAMP,
    CertificationScore       DECIMAL(5,2),
    ProbationStage           VARCHAR,
    ProbationStartDate       DATE,
    FinalDecision            VARCHAR,
    FinalDecisionDate        TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        a.c_application_id,
        a.c_application_number,
        'COMPLETED'::VARCHAR,
        a.c_applied_date,
        (CASE WHEN a.c_resume_screening_status = 'PASSED' THEN 'COMPLETED'
              WHEN a.c_resume_screening_status = 'FAILED' THEN 'FAILED'
              WHEN a.c_resume_screened = TRUE THEN 'REVIEWED'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_resume_screened_date,
        a.c_resume_screening_status,
        (CASE WHEN a.c_interview_result = 'PASSED' THEN 'COMPLETED'
              WHEN a.c_interview_result = 'FAILED' THEN 'FAILED'
              WHEN a.c_interview_scheduled = TRUE THEN 'SCHEDULED'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_interview_date,
        a.c_interview_score,
        (CASE WHEN a.c_background_verification_result = 'CLEAR' THEN 'COMPLETED'
              WHEN a.c_background_verification_result = 'ISSUES_FOUND' THEN 'FAILED'
              WHEN a.c_background_verification_initiated = TRUE THEN 'IN_PROGRESS'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_background_verification_date,
        (CASE WHEN a.c_training_completed = TRUE THEN 'COMPLETED'
              WHEN a.c_training_start_date IS NOT NULL THEN 'IN_PROGRESS'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_training_start_date,
        a.c_training_end_date,
        a.c_training_attendance_percentage,
        (CASE WHEN a.c_certification_passed = TRUE THEN 'COMPLETED'
              WHEN a.c_certification_test_date IS NOT NULL THEN 'SCHEDULED'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_certification_test_date,
        a.c_certification_test_score,
        (CASE WHEN a.c_probation_passed = TRUE THEN 'COMPLETED'
              WHEN a.c_probation_passed = FALSE THEN 'FAILED'
              WHEN a.c_probation_assigned = TRUE THEN 'IN_PROGRESS'
              ELSE 'PENDING' END)::VARCHAR,
        a.c_probation_start_date,
        a.c_final_decision,
        a.c_final_decision_date
    FROM t_sys_careers_application a
    WHERE a.c_application_id = p_applicationid;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationWorkflowStatus(
    p_applicationid BIGINT
)
RETURNS TABLE (
    ApplicationId        BIGINT,
    ApplicationNumber    VARCHAR,
    SupervisorId         BIGINT,
    CurrentStage         VARCHAR,
    FinalDecision        VARCHAR,
    AppliedDate          TIMESTAMP,
    LastModifiedDate     TIMESTAMP
)
LANGUAGE plpgsql AS $$
DECLARE
    v_current_stage VARCHAR(30);
BEGIN
    SELECT CASE
        WHEN a.c_final_decision = 'ACCEPTED' THEN 'ACTIVATED'
        WHEN a.c_final_decision = 'REJECTED' THEN 'REJECTED'
        WHEN a.c_probation_assigned = TRUE AND a.c_probation_passed IS NULL THEN 'PROBATION'
        WHEN a.c_certification_passed = TRUE THEN 'CERTIFICATION_PASSED'
        WHEN a.c_certification_test_date IS NOT NULL THEN 'CERTIFICATION'
        WHEN a.c_training_completed = TRUE THEN 'TRAINING_COMPLETED'
        WHEN a.c_training_start_date IS NOT NULL THEN 'TRAINING'
        WHEN a.c_background_verification_result = 'CLEAR' THEN 'BACKGROUND_CLEARED'
        WHEN a.c_background_verification_initiated = TRUE THEN 'BACKGROUND_CHECK'
        WHEN a.c_interview_result = 'PASSED' THEN 'INTERVIEW_PASSED'
        WHEN a.c_interview_scheduled = TRUE THEN 'INTERVIEW'
        WHEN a.c_resume_screening_status = 'PASSED' THEN 'RESUME_SCREENED'
        ELSE 'APPLIED'
    END
    INTO v_current_stage
    FROM t_sys_careers_application a
    WHERE a.c_application_id = p_applicationid;

    RETURN QUERY
    SELECT a.c_application_id,
           a.c_application_number,
           a.c_supervisor_id,
           v_current_stage,
           a.c_final_decision,
           a.c_applied_date,
           a.c_modifieddate
    FROM t_sys_careers_application a
    WHERE a.c_application_id = p_applicationid;
END;
$$;


-- =============================================
-- ADMIN QUERIES
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetAllCareersApplications(
    p_status VARCHAR(20)
)
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_current_status                       VARCHAR,
    c_supervisor_type                      VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (p_status IS NULL OR a.c_final_decision = p_status)
    ORDER BY a.c_applied_date DESC;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationsByStage(
    p_stage VARCHAR(30)
)
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (
        (p_stage = 'RESUME_SCREENING' AND a.c_resume_screened = FALSE)
        OR (p_stage = 'INTERVIEW' AND a.c_resume_screening_status = 'PASSED' AND a.c_interview_completed = FALSE)
        OR (p_stage = 'BACKGROUND_CHECK' AND a.c_interview_result = 'PASSED' AND (a.c_background_verification_result IN ('PENDING', 'NULL') OR a.c_background_verification_initiated = FALSE))
        OR (p_stage = 'TRAINING' AND a.c_background_verification_result = 'CLEAR' AND a.c_training_completed = FALSE)
        OR (p_stage = 'CERTIFICATION' AND a.c_training_completed = TRUE AND a.c_certification_passed = FALSE)
        OR (p_stage = 'PROBATION' AND a.c_certification_passed = TRUE AND a.c_probation_passed IS NULL)
        OR (p_stage = 'ACTIVATED' AND a.c_final_decision = 'ACCEPTED')
    )
    ORDER BY a.c_applied_date ASC;
END;
$$;


CREATE OR REPLACE FUNCTION sp_GetApplicationStatistics()
RETURNS TABLE (
    TotalApplications        BIGINT,
    Accepted                 BIGINT,
    Rejected                 BIGINT,
    InProgress               BIGINT,
    PendingResumeScreening   BIGINT,
    PendingInterview         BIGINT,
    PendingBackgroundCheck   BIGINT,
    InTraining               BIGINT,
    PendingCertification     BIGINT,
    InProbation              BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) AS TotalApplications,
        SUM(CASE WHEN c_final_decision = 'ACCEPTED' THEN 1 ELSE 0 END) AS Accepted,
        SUM(CASE WHEN c_final_decision = 'REJECTED' THEN 1 ELSE 0 END) AS Rejected,
        SUM(CASE WHEN c_final_decision IS NULL THEN 1 ELSE 0 END) AS InProgress,
        SUM(CASE WHEN c_resume_screened = FALSE THEN 1 ELSE 0 END) AS PendingResumeScreening,
        SUM(CASE WHEN c_resume_screening_status = 'PASSED' AND c_interview_completed = FALSE THEN 1 ELSE 0 END) AS PendingInterview,
        SUM(CASE WHEN c_interview_result = 'PASSED' AND (c_background_verification_result = 'PENDING' OR c_background_verification_initiated = FALSE) THEN 1 ELSE 0 END) AS PendingBackgroundCheck,
        SUM(CASE WHEN c_background_verification_result = 'CLEAR' AND c_training_completed = FALSE THEN 1 ELSE 0 END) AS InTraining,
        SUM(CASE WHEN c_training_completed = TRUE AND c_certification_passed = FALSE THEN 1 ELSE 0 END) AS PendingCertification,
        SUM(CASE WHEN c_certification_passed = TRUE AND c_probation_passed IS NULL THEN 1 ELSE 0 END) AS InProbation
    FROM t_sys_careers_application;
END;
$$;


CREATE OR REPLACE FUNCTION sp_SearchCareersApplications(
    p_name           VARCHAR(100),
    p_email          VARCHAR(100),
    p_phone          VARCHAR(15),
    p_status         VARCHAR(20),
    p_currentstage   VARCHAR(30),
    p_appliedfrom    TIMESTAMP,
    p_appliedto      TIMESTAMP
)
RETURNS TABLE (
    c_application_id                       BIGINT,
    c_supervisor_id                        BIGINT,
    c_application_number                   VARCHAR,
    c_applied_date                         TIMESTAMP,
    c_source                               VARCHAR,
    c_referral_code                        VARCHAR,
    c_resume_screened                      BOOLEAN,
    c_resume_screened_by                   BIGINT,
    c_resume_screened_date                 TIMESTAMP,
    c_resume_screening_notes               TEXT,
    c_resume_screening_status              VARCHAR,
    c_interview_scheduled                  BOOLEAN,
    c_interview_date                       TIMESTAMP,
    c_interview_mode                       VARCHAR,
    c_interviewer_id                       BIGINT,
    c_interview_completed                  BOOLEAN,
    c_interview_feedback                   TEXT,
    c_interview_score                      DECIMAL(5,2),
    c_interview_result                     VARCHAR,
    c_background_verification_initiated    BOOLEAN,
    c_background_verification_agency       VARCHAR,
    c_background_verification_date         TIMESTAMP,
    c_background_verification_result       VARCHAR,
    c_background_verification_report_url   VARCHAR,
    c_training_batch_id                    BIGINT,
    c_training_start_date                  DATE,
    c_training_end_date                    DATE,
    c_training_attendance_percentage       DECIMAL(5,2),
    c_training_completed                   BOOLEAN,
    c_certification_test_date              TIMESTAMP,
    c_certification_test_score             DECIMAL(5,2),
    c_certification_passed                 BOOLEAN,
    c_certification_certificate_url        VARCHAR,
    c_probation_assigned                   BOOLEAN,
    c_probation_start_date                 DATE,
    c_probation_duration_days              INTEGER,
    c_probation_supervisor_id              BIGINT,
    c_probation_evaluation_date            DATE,
    c_probation_evaluation_notes           TEXT,
    c_probation_passed                     BOOLEAN,
    c_final_decision                       VARCHAR,
    c_final_decision_date                  TIMESTAMP,
    c_final_decision_by                    BIGINT,
    c_rejection_reason                     TEXT,
    c_onboarding_completed                 BOOLEAN,
    c_joining_date                         DATE,
    c_offer_letter_url                     VARCHAR,
    c_employee_id                          VARCHAR,
    c_createddate                          TIMESTAMP,
    c_modifieddate                         TIMESTAMP,
    c_full_name                            VARCHAR,
    c_email                                VARCHAR,
    c_phone                                VARCHAR,
    c_current_status                       VARCHAR,
    c_supervisor_type                      VARCHAR,
    c_years_of_experience                  INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT a.c_application_id, a.c_supervisor_id, a.c_application_number, a.c_applied_date,
           a.c_source, a.c_referral_code,
           a.c_resume_screened, a.c_resume_screened_by, a.c_resume_screened_date,
           a.c_resume_screening_notes, a.c_resume_screening_status,
           a.c_interview_scheduled, a.c_interview_date, a.c_interview_mode, a.c_interviewer_id,
           a.c_interview_completed, a.c_interview_feedback, a.c_interview_score, a.c_interview_result,
           a.c_background_verification_initiated, a.c_background_verification_agency,
           a.c_background_verification_date, a.c_background_verification_result,
           a.c_background_verification_report_url,
           a.c_training_batch_id, a.c_training_start_date, a.c_training_end_date,
           a.c_training_attendance_percentage, a.c_training_completed,
           a.c_certification_test_date, a.c_certification_test_score, a.c_certification_passed,
           a.c_certification_certificate_url,
           a.c_probation_assigned, a.c_probation_start_date, a.c_probation_duration_days,
           a.c_probation_supervisor_id, a.c_probation_evaluation_date,
           a.c_probation_evaluation_notes, a.c_probation_passed,
           a.c_final_decision, a.c_final_decision_date, a.c_final_decision_by, a.c_rejection_reason,
           a.c_onboarding_completed, a.c_joining_date, a.c_offer_letter_url, a.c_employee_id,
           a.c_createddate, a.c_modifieddate,
           s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type,
           s.c_years_of_experience
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (p_name IS NULL OR s.c_full_name LIKE '%' || p_name || '%')
      AND (p_email IS NULL OR s.c_email LIKE '%' || p_email || '%')
      AND (p_phone IS NULL OR s.c_phone LIKE '%' || p_phone || '%')
      AND (p_status IS NULL OR a.c_final_decision = p_status)
      AND (p_appliedfrom IS NULL OR a.c_applied_date >= p_appliedfrom)
      AND (p_appliedto IS NULL OR a.c_applied_date <= p_appliedto)
    ORDER BY a.c_applied_date DESC;
END;
$$;
