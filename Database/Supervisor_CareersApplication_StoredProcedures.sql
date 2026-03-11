-- =============================================
-- Supervisor Careers Application Stored Procedures
-- Maps to: CareersApplicationRepository.cs
-- 29 Stored Procedures for Careers Portal 6-Stage Pipeline
-- (Excludes sp_ProgressCareersApplication already in
--  Supervisor_Management_StoredProcedures.sql)
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- APPLICATION SUBMISSION
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitCareersApplication
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email VARCHAR(100),
    @Phone VARCHAR(15),
    @Address NVARCHAR(500),
    @DateOfBirth DATE,
    @ResumeUrl VARCHAR(500),
    @CoverLetter NVARCHAR(2000) = NULL,
    @YearsOfExperience INT,
    @PreviousEmployer NVARCHAR(200) = NULL,
    @References NVARCHAR(2000) = NULL,
    @ApplicationId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Create supervisor record
        DECLARE @SupervisorId BIGINT;
        DECLARE @FullName NVARCHAR(100) = @FirstName + ' ' + @LastName;

        INSERT INTO t_sys_supervisor (
            c_supervisor_type, c_full_name, c_email, c_phone,
            c_date_of_birth, c_address_line1, c_city, c_state, c_pincode,
            c_resume_url, c_years_of_experience, c_previous_employer,
            c_current_status, c_authority_level,
            c_compensation_type, c_createddate
        )
        VALUES (
            'CAREER', @FullName, @Email, @Phone,
            @DateOfBirth, @Address, '', '', '',
            @ResumeUrl, @YearsOfExperience, @PreviousEmployer,
            'APPLIED', 'BASIC',
            'MONTHLY_SALARY', GETDATE()
        );

        SET @SupervisorId = SCOPE_IDENTITY();

        -- Create careers application record
        DECLARE @AppNumber VARCHAR(50) = 'CAR-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(@SupervisorId AS VARCHAR(10));

        INSERT INTO t_sys_careers_application (
            c_supervisor_id, c_application_number, c_applied_date,
            c_source
        )
        VALUES (
            @SupervisorId, @AppNumber, GETDATE(),
            'WEBSITE'
        );

        SET @ApplicationId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_GetCareersApplicationById
    @ApplicationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone, s.c_date_of_birth,
           s.c_resume_url, s.c_years_of_experience, s.c_previous_employer,
           s.c_photo_url, s.c_current_status, s.c_authority_level,
           s.c_address_line1, s.c_supervisor_type
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetCareersApplicationBySupervisorId
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone, s.c_date_of_birth,
           s.c_resume_url, s.c_years_of_experience, s.c_previous_employer,
           s.c_photo_url, s.c_current_status, s.c_authority_level
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_supervisor_id = @SupervisorId;
END
GO

-- =============================================
-- REJECTION
-- =============================================

CREATE OR ALTER PROCEDURE sp_RejectCareersApplication
    @ApplicationId BIGINT,
    @RejectedBy BIGINT,
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_final_decision = 'REJECTED',
        c_final_decision_date = GETDATE(),
        c_final_decision_by = @RejectedBy,
        c_rejection_reason = @Reason,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    -- Update supervisor status
    UPDATE s
    SET s.c_current_status = 'REJECTED',
        s.c_status_reason = @Reason,
        s.c_modifieddate = GETDATE(),
        s.c_modified_by = @RejectedBy
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

-- =============================================
-- STAGE 2: RESUME SCREENING
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitResumeScreening
    @ApplicationId BIGINT,
    @ScreenedBy BIGINT,
    @Passed BIT,
    @ResumeScore DECIMAL(5,2),
    @ScreeningNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_resume_screened = 1,
        c_resume_screened_by = @ScreenedBy,
        c_resume_screened_date = GETDATE(),
        c_resume_screening_notes = @ScreeningNotes,
        c_resume_screening_status = CASE WHEN @Passed = 1 THEN 'PASSED' ELSE 'FAILED' END,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    -- Update supervisor status if passed
    IF @Passed = 1
    BEGIN
        UPDATE s
        SET s.c_current_status = 'RESUME_SCREENED',
            s.c_modifieddate = GETDATE()
        FROM t_sys_supervisor s
        INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
        WHERE a.c_application_id = @ApplicationId;
    END

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsForResumeScreening
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_resume_url, s.c_years_of_experience
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_resume_screened = 0
    ORDER BY a.c_applied_date ASC;
END
GO

-- =============================================
-- STAGE 3: INTERVIEW
-- =============================================

CREATE OR ALTER PROCEDURE sp_ScheduleInterview
    @ApplicationId BIGINT,
    @InterviewDateTime DATETIME,
    @InterviewType VARCHAR(20),
    @InterviewerName NVARCHAR(100),
    @MeetingLink VARCHAR(500) = NULL,
    @ScheduledBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_interview_scheduled = 1,
        c_interview_date = @InterviewDateTime,
        c_interview_mode = @InterviewType,
        c_interviewer_id = @ScheduledBy,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitInterviewResult
    @ApplicationId BIGINT,
    @InterviewedBy BIGINT,
    @Passed BIT,
    @InterviewScore DECIMAL(5,2),
    @InterviewNotes NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_interview_completed = 1,
        c_interview_feedback = @InterviewNotes,
        c_interview_score = @InterviewScore,
        c_interview_result = CASE WHEN @Passed = 1 THEN 'PASSED' ELSE 'FAILED' END,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    IF @Passed = 1
    BEGIN
        UPDATE s
        SET s.c_current_status = 'INTERVIEW_PASSED',
            s.c_modifieddate = GETDATE()
        FROM t_sys_supervisor s
        INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
        WHERE a.c_application_id = @ApplicationId;
    END

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsForInterview
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_resume_screening_status = 'PASSED'
      AND a.c_interview_completed = 0
    ORDER BY a.c_interview_date ASC;
END
GO

-- =============================================
-- STAGE 4: BACKGROUND VERIFICATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_InitiateBackgroundCheck
    @ApplicationId BIGINT,
    @InitiatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_background_verification_initiated = 1,
        c_background_verification_date = GETDATE(),
        c_background_verification_result = 'PENDING',
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    UPDATE s
    SET s.c_current_status = 'BACKGROUND_VERIFICATION',
        s.c_background_check_status = 'PENDING',
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitBackgroundCheckResult
    @ApplicationId BIGINT,
    @Passed BIT,
    @VerificationAgency VARCHAR(100),
    @VerificationDate DATETIME,
    @VerificationReportUrl VARCHAR(500) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @SubmittedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_background_verification_result = CASE WHEN @Passed = 1 THEN 'CLEAR' ELSE 'ISSUES_FOUND' END,
        c_background_verification_agency = @VerificationAgency,
        c_background_verification_date = @VerificationDate,
        c_background_verification_report_url = @VerificationReportUrl,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    UPDATE s
    SET s.c_background_check_status = CASE WHEN @Passed = 1 THEN 'PASSED' ELSE 'FAILED' END,
        s.c_background_check_date = @VerificationDate,
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsPendingBackgroundCheck
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_interview_result = 'PASSED'
      AND (a.c_background_verification_result = 'PENDING' OR a.c_background_verification_initiated = 0)
    ORDER BY a.c_applied_date ASC;
END
GO

-- =============================================
-- STAGE 5: TRAINING
-- =============================================

CREATE OR ALTER PROCEDURE sp_AssignTraining
    @ApplicationId BIGINT,
    @ModuleIds VARCHAR(500),
    @AssignedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_careers_application WHERE c_application_id = @ApplicationId;

    -- Update application
    UPDATE t_sys_careers_application
    SET c_training_start_date = CAST(GETDATE() AS DATE),
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    -- Update supervisor status
    UPDATE t_sys_supervisor
    SET c_current_status = 'TRAINING',
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId;

    -- Insert training progress for each module
    DECLARE @ModuleId BIGINT;
    DECLARE @Pos INT = 1;
    DECLARE @Len INT;

    WHILE @Pos <= LEN(@ModuleIds)
    BEGIN
        SET @Len = CHARINDEX(',', @ModuleIds + ',', @Pos) - @Pos;
        SET @ModuleId = CAST(SUBSTRING(@ModuleIds, @Pos, @Len) AS BIGINT);

        IF NOT EXISTS (SELECT 1 FROM t_sys_supervisor_training_progress WHERE c_supervisor_id = @SupervisorId AND c_module_id = @ModuleId)
        BEGIN
            INSERT INTO t_sys_supervisor_training_progress (c_supervisor_id, c_module_id, c_started_date)
            VALUES (@SupervisorId, @ModuleId, GETDATE());
        END

        SET @Pos = @Pos + @Len + 1;
    END

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_RecordTrainingProgress
    @ApplicationId BIGINT,
    @ModuleId BIGINT,
    @ProgressPercentage INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_careers_application WHERE c_application_id = @ApplicationId;

    UPDATE t_sys_supervisor_training_progress
    SET c_completion_percentage = @ProgressPercentage,
        c_last_attempt_date = GETDATE(),
        c_passed = CASE WHEN @ProgressPercentage >= 100 THEN 1 ELSE 0 END,
        c_completed_date = CASE WHEN @ProgressPercentage >= 100 THEN GETDATE() ELSE NULL END
    WHERE c_supervisor_id = @SupervisorId AND c_module_id = @ModuleId;

    -- Update overall training attendance in application
    DECLARE @OverallProgress DECIMAL(5,2);
    SELECT @OverallProgress = AVG(c_completion_percentage)
    FROM t_sys_supervisor_training_progress
    WHERE c_supervisor_id = @SupervisorId;

    UPDATE t_sys_careers_application
    SET c_training_attendance_percentage = @OverallProgress,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_CompleteTraining
    @ApplicationId BIGINT,
    @CompletedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_training_completed = 1,
        c_training_end_date = CAST(GETDATE() AS DATE),
        c_training_attendance_percentage = 100.00,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    -- Update supervisor training date
    UPDATE s
    SET s.c_training_completed_date = GETDATE(),
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsInTraining
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_background_verification_result = 'CLEAR'
      AND a.c_training_completed = 0
      AND s.c_current_status = 'TRAINING'
    ORDER BY a.c_training_start_date ASC;
END
GO

-- =============================================
-- STAGE 6: CERTIFICATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_ScheduleCertificationExam
    @ApplicationId BIGINT,
    @ExamDate DATETIME,
    @ScheduledBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_certification_test_date = @ExamDate,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitCertificationResult
    @ApplicationId BIGINT,
    @Passed BIT,
    @ExamScore DECIMAL(5,2),
    @ExamDate DATETIME,
    @CertificateNumber VARCHAR(50) = NULL,
    @CertificateUrl VARCHAR(500) = NULL,
    @EvaluatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_certification_test_score = @ExamScore,
        c_certification_passed = @Passed,
        c_certification_certificate_url = @CertificateUrl,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    IF @Passed = 1
    BEGIN
        UPDATE s
        SET s.c_certification_date = @ExamDate,
            s.c_certification_status = 'CERTIFIED',
            s.c_certification_valid_until = DATEADD(YEAR, 1, @ExamDate),
            s.c_current_status = 'CERTIFIED',
            s.c_modifieddate = GETDATE()
        FROM t_sys_supervisor s
        INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
        WHERE a.c_application_id = @ApplicationId;
    END

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsPendingCertification
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_training_completed = 1
      AND a.c_certification_passed = 0
    ORDER BY a.c_certification_test_date ASC;
END
GO

-- =============================================
-- STAGE 7: PROBATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_StartProbation
    @ApplicationId BIGINT,
    @ProbationDays INT,
    @StartedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_probation_assigned = 1,
        c_probation_start_date = CAST(GETDATE() AS DATE),
        c_probation_duration_days = @ProbationDays,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    UPDATE s
    SET s.c_current_status = 'PROBATION',
        s.c_probation_start_date = CAST(GETDATE() AS DATE),
        s.c_probation_end_date = DATEADD(DAY, @ProbationDays, CAST(GETDATE() AS DATE)),
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_CompleteProbation
    @ApplicationId BIGINT,
    @Passed BIT,
    @EvaluatedBy BIGINT,
    @Evaluation NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_careers_application
    SET c_probation_evaluation_date = CAST(GETDATE() AS DATE),
        c_probation_evaluation_notes = @Evaluation,
        c_probation_passed = @Passed,
        c_modifieddate = GETDATE()
    WHERE c_application_id = @ApplicationId;

    UPDATE s
    SET s.c_is_probation_passed = @Passed,
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_application_id = @ApplicationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsInProbation
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_probation_start_date, s.c_probation_end_date
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_certification_passed = 1
      AND a.c_probation_passed IS NULL
      AND s.c_current_status = 'PROBATION'
    ORDER BY s.c_probation_end_date ASC;
END
GO

-- =============================================
-- FINAL ACTIVATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_ActivateCareerSupervisor
    @ApplicationId BIGINT,
    @ActivatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Update application
        UPDATE t_sys_careers_application
        SET c_final_decision = 'ACCEPTED',
            c_final_decision_date = GETDATE(),
            c_final_decision_by = @ActivatedBy,
            c_onboarding_completed = 1,
            c_joining_date = CAST(GETDATE() AS DATE),
            c_modifieddate = GETDATE()
        WHERE c_application_id = @ApplicationId;

        -- Update supervisor to ACTIVE with elevated authority
        UPDATE s
        SET s.c_current_status = 'ACTIVE',
            s.c_authority_level = 'INTERMEDIATE',
            s.c_is_available = 1,
            s.c_modifieddate = GETDATE(),
            s.c_modified_by = @ActivatedBy
        FROM t_sys_supervisor s
        INNER JOIN t_sys_careers_application a ON s.c_supervisor_id = a.c_supervisor_id
        WHERE a.c_application_id = @ApplicationId;

        -- Log activation
        DECLARE @SupervisorId BIGINT;
        SELECT @SupervisorId = c_supervisor_id FROM t_sys_careers_application WHERE c_application_id = @ApplicationId;

        INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
        VALUES (@SupervisorId, 'STATUS_CHANGED', 'Career supervisor activated after completing full pipeline', 'SUCCESS');

        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- WORKFLOW TRACKING
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetApplicationProgress
    @ApplicationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.c_application_id AS ApplicationId,
        a.c_application_number AS ApplicationNumber,
        -- Stage 1: Application
        'COMPLETED' AS ApplicationStage,
        a.c_applied_date AS AppliedDate,
        -- Stage 2: Resume Screening
        CASE WHEN a.c_resume_screening_status = 'PASSED' THEN 'COMPLETED'
             WHEN a.c_resume_screening_status = 'FAILED' THEN 'FAILED'
             WHEN a.c_resume_screened = 1 THEN 'REVIEWED'
             ELSE 'PENDING' END AS ResumeScreeningStage,
        a.c_resume_screened_date AS ResumeScreenedDate,
        a.c_resume_screening_status AS ResumeScreeningResult,
        -- Stage 3: Interview
        CASE WHEN a.c_interview_result = 'PASSED' THEN 'COMPLETED'
             WHEN a.c_interview_result = 'FAILED' THEN 'FAILED'
             WHEN a.c_interview_scheduled = 1 THEN 'SCHEDULED'
             ELSE 'PENDING' END AS InterviewStage,
        a.c_interview_date AS InterviewDate,
        a.c_interview_score AS InterviewScore,
        -- Stage 4: Background Check
        CASE WHEN a.c_background_verification_result = 'CLEAR' THEN 'COMPLETED'
             WHEN a.c_background_verification_result = 'ISSUES_FOUND' THEN 'FAILED'
             WHEN a.c_background_verification_initiated = 1 THEN 'IN_PROGRESS'
             ELSE 'PENDING' END AS BackgroundCheckStage,
        a.c_background_verification_date AS BackgroundCheckDate,
        -- Stage 5: Training
        CASE WHEN a.c_training_completed = 1 THEN 'COMPLETED'
             WHEN a.c_training_start_date IS NOT NULL THEN 'IN_PROGRESS'
             ELSE 'PENDING' END AS TrainingStage,
        a.c_training_start_date AS TrainingStartDate,
        a.c_training_end_date AS TrainingEndDate,
        a.c_training_attendance_percentage AS TrainingProgress,
        -- Stage 6: Certification
        CASE WHEN a.c_certification_passed = 1 THEN 'COMPLETED'
             WHEN a.c_certification_test_date IS NOT NULL THEN 'SCHEDULED'
             ELSE 'PENDING' END AS CertificationStage,
        a.c_certification_test_date AS CertificationDate,
        a.c_certification_test_score AS CertificationScore,
        -- Stage 7: Probation
        CASE WHEN a.c_probation_passed = 1 THEN 'COMPLETED'
             WHEN a.c_probation_passed = 0 THEN 'FAILED'
             WHEN a.c_probation_assigned = 1 THEN 'IN_PROGRESS'
             ELSE 'PENDING' END AS ProbationStage,
        a.c_probation_start_date AS ProbationStartDate,
        -- Final
        a.c_final_decision AS FinalDecision,
        a.c_final_decision_date AS FinalDecisionDate
    FROM t_sys_careers_application a
    WHERE a.c_application_id = @ApplicationId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationWorkflowStatus
    @ApplicationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStage VARCHAR(30);

    SELECT @CurrentStage = CASE
        WHEN a.c_final_decision = 'ACCEPTED' THEN 'ACTIVATED'
        WHEN a.c_final_decision = 'REJECTED' THEN 'REJECTED'
        WHEN a.c_probation_assigned = 1 AND a.c_probation_passed IS NULL THEN 'PROBATION'
        WHEN a.c_certification_passed = 1 THEN 'CERTIFICATION_PASSED'
        WHEN a.c_certification_test_date IS NOT NULL THEN 'CERTIFICATION'
        WHEN a.c_training_completed = 1 THEN 'TRAINING_COMPLETED'
        WHEN a.c_training_start_date IS NOT NULL THEN 'TRAINING'
        WHEN a.c_background_verification_result = 'CLEAR' THEN 'BACKGROUND_CLEARED'
        WHEN a.c_background_verification_initiated = 1 THEN 'BACKGROUND_CHECK'
        WHEN a.c_interview_result = 'PASSED' THEN 'INTERVIEW_PASSED'
        WHEN a.c_interview_scheduled = 1 THEN 'INTERVIEW'
        WHEN a.c_resume_screening_status = 'PASSED' THEN 'RESUME_SCREENED'
        ELSE 'APPLIED'
    END
    FROM t_sys_careers_application a
    WHERE a.c_application_id = @ApplicationId;

    SELECT a.c_application_id AS ApplicationId,
           a.c_application_number AS ApplicationNumber,
           a.c_supervisor_id AS SupervisorId,
           @CurrentStage AS CurrentStage,
           a.c_final_decision AS FinalDecision,
           a.c_applied_date AS AppliedDate,
           a.c_modifieddate AS LastModifiedDate
    FROM t_sys_careers_application a
    WHERE a.c_application_id = @ApplicationId;
END
GO

-- =============================================
-- ADMIN QUERIES
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetAllCareersApplications
    @Status VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (@Status IS NULL OR a.c_final_decision = @Status)
    ORDER BY a.c_applied_date DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationsByStage
    @Stage VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (
        (@Stage = 'RESUME_SCREENING' AND a.c_resume_screened = 0)
        OR (@Stage = 'INTERVIEW' AND a.c_resume_screening_status = 'PASSED' AND a.c_interview_completed = 0)
        OR (@Stage = 'BACKGROUND_CHECK' AND a.c_interview_result = 'PASSED' AND a.c_background_verification_result IN ('PENDING', 'NULL') OR a.c_background_verification_initiated = 0)
        OR (@Stage = 'TRAINING' AND a.c_background_verification_result = 'CLEAR' AND a.c_training_completed = 0)
        OR (@Stage = 'CERTIFICATION' AND a.c_training_completed = 1 AND a.c_certification_passed = 0)
        OR (@Stage = 'PROBATION' AND a.c_certification_passed = 1 AND a.c_probation_passed IS NULL)
        OR (@Stage = 'ACTIVATED' AND a.c_final_decision = 'ACCEPTED')
    )
    ORDER BY a.c_applied_date ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetApplicationStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalApplications,
        SUM(CASE WHEN c_final_decision = 'ACCEPTED' THEN 1 ELSE 0 END) AS Accepted,
        SUM(CASE WHEN c_final_decision = 'REJECTED' THEN 1 ELSE 0 END) AS Rejected,
        SUM(CASE WHEN c_final_decision IS NULL THEN 1 ELSE 0 END) AS InProgress,
        SUM(CASE WHEN c_resume_screened = 0 THEN 1 ELSE 0 END) AS PendingResumeScreening,
        SUM(CASE WHEN c_resume_screening_status = 'PASSED' AND c_interview_completed = 0 THEN 1 ELSE 0 END) AS PendingInterview,
        SUM(CASE WHEN c_interview_result = 'PASSED' AND (c_background_verification_result = 'PENDING' OR c_background_verification_initiated = 0) THEN 1 ELSE 0 END) AS PendingBackgroundCheck,
        SUM(CASE WHEN c_background_verification_result = 'CLEAR' AND c_training_completed = 0 THEN 1 ELSE 0 END) AS InTraining,
        SUM(CASE WHEN c_training_completed = 1 AND c_certification_passed = 0 THEN 1 ELSE 0 END) AS PendingCertification,
        SUM(CASE WHEN c_certification_passed = 1 AND c_probation_passed IS NULL THEN 1 ELSE 0 END) AS InProbation
    FROM t_sys_careers_application;
END
GO

CREATE OR ALTER PROCEDURE sp_SearchCareersApplications
    @Name NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Phone VARCHAR(15) = NULL,
    @Status VARCHAR(20) = NULL,
    @CurrentStage VARCHAR(30) = NULL,
    @AppliedFrom DATETIME = NULL,
    @AppliedTo DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type,
           s.c_years_of_experience
    FROM t_sys_careers_application a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (@Name IS NULL OR s.c_full_name LIKE '%' + @Name + '%')
      AND (@Email IS NULL OR s.c_email LIKE '%' + @Email + '%')
      AND (@Phone IS NULL OR s.c_phone LIKE '%' + @Phone + '%')
      AND (@Status IS NULL OR a.c_final_decision = @Status)
      AND (@AppliedFrom IS NULL OR a.c_applied_date >= @AppliedFrom)
      AND (@AppliedTo IS NULL OR a.c_applied_date <= @AppliedTo)
    ORDER BY a.c_applied_date DESC;
END
GO

PRINT '================================================';
PRINT 'Supervisor Careers Application Stored Procedures Created';
PRINT '29 Stored Procedures for CareersApplicationRepository.cs';
PRINT '================================================';
GO
