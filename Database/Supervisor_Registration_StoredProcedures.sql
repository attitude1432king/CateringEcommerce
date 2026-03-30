-- =============================================
-- Supervisor Registration Stored Procedures
-- Maps to: RegistrationRepository.cs
-- 25 Stored Procedures for Registration Portal Workflow
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- REGISTRATION SUBMISSION
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitSupervisorRegistration
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email VARCHAR(100),
    @Phone VARCHAR(15),
    @Address NVARCHAR(500),
    @Pincode VARCHAR(10),
    @StateID INT,
    @CityID INT,
    @DateOfBirth DATE,
    @IDProofType VARCHAR(20),
    @IDProofNumber VARCHAR(50),
    @IDProofUrl NVARCHAR(500) = NULL,
    @AddressProofUrl NVARCHAR(500) = NULL,
    @PhotoUrl NVARCHAR(500) = NULL,
    @HasPriorExperience BIT,
    @PriorExperienceDetails NVARCHAR(500) = NULL,
    @RegistrationId BIGINT OUTPUT
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
            c_date_of_birth, c_address_line1, c_cityid, c_stateid, c_pincode,
            c_identity_type, c_identity_number, c_has_prior_experience, c_prior_experience_details, c_current_status, c_authority_level,
            c_createddate
        )
        VALUES (
            'REGISTERED', @FullName, @Email, @Phone,
            @DateOfBirth, @Address, @CityID, @StateID, @Pincode,
            @IDProofType, @IDProofNumber, @HasPriorExperience, @PriorExperienceDetails, 'APPLIED', 'BASIC',
            GETDATE()
        );

        SET @SupervisorId = SCOPE_IDENTITY();

        -- Create registration record
        DECLARE @RegNumber VARCHAR(50) = 'REG-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(@SupervisorId AS VARCHAR(10));

        INSERT INTO t_sys_supervisor_registration (
            c_supervisor_id, c_registration_number, c_registered_date,
            c_source, c_document_verification_status
        )
        VALUES (
            @SupervisorId, @RegNumber, GETDATE(),
            'WEBSITE', 'PENDING'
        );

        SET @RegistrationId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationById
    @RegistrationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.c_registration_id                 AS RegistrationId,
        r.c_supervisor_id                   AS SupervisorId,
        r.c_registration_number             AS RegistrationNumber,
        r.c_registered_date                 AS RegisteredDate,
        r.c_source                          AS Source,
        r.c_referral_code                   AS ReferralCode,
        -- Document Verification
        r.c_document_verification_status    AS DocumentVerificationStatus,
        r.c_document_verified_by            AS DocumentVerifiedBy,
        r.c_document_verified_date          AS DocumentVerifiedDate,
        r.c_document_rejection_reason       AS DocumentRejectionReason,
        -- Interview
        r.c_interview_scheduled             AS InterviewScheduled,
        r.c_interview_date                  AS InterviewDate,
        r.c_interview_mode                  AS InterviewMode,
        r.c_interviewer_id                  AS InterviewerId,
        r.c_interview_completed             AS InterviewCompleted,
        r.c_interview_notes                 AS InterviewNotes,
        r.c_interview_result                AS InterviewResult,
        -- Training
        r.c_training_module_assigned        AS TrainingModuleAssigned,
        r.c_training_started_date           AS TrainingStartedDate,
        r.c_training_completed_date         AS TrainingCompletedDate,
        r.c_training_completion_percentage  AS TrainingCompletionPercentage,
        r.c_training_passed                 AS TrainingPassed,
        -- Certification
        r.c_certification_test_assigned     AS CertificationTestAssigned,
        r.c_certification_test_date         AS CertificationTestDate,
        r.c_certification_test_score        AS CertificationTestScore,
        r.c_certification_test_passed       AS CertificationTestPassed,
        r.c_certification_certificate_url   AS CertificationCertificateUrl,
        -- Activation
        r.c_activation_status               AS ActivationStatus,
        r.c_activated_date                  AS ActivatedDate,
        r.c_activated_by                    AS ActivatedBy,
        r.c_rejection_reason                AS RejectionReason,
        -- Banking
        r.c_bank_details_submitted          AS BankDetailsSubmitted,
        r.c_bank_details_verified           AS BankDetailsVerified,
        r.c_bank_verification_date          AS BankVerificationDate,
        -- Audit
        r.c_createddate                     AS CreatedDate,
        r.c_modifieddate                    AS ModifiedDate,
        -- From supervisor table
        s.c_full_name                       AS FullName,
        s.c_email                           AS Email,
        s.c_phone                           AS Phone,
        s.c_date_of_birth                   AS DateOfBirth,
        s.c_identity_type                   AS IdentityType,
        s.c_identity_number                 AS IdentityNumber,
        s.c_identity_proof_url              AS IDProofUrl,
        s.c_photo_url                       AS PhotoUrl,
        s.c_current_status                  AS CurrentStatus,
        s.c_authority_level                 AS AuthorityLevel,
        s.c_address_line1                   AS Address,
        s.c_supervisor_type                 AS SupervisorType
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_registration_id = @RegistrationId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationBySupervisorId
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.c_registration_id                 AS RegistrationId,
        r.c_supervisor_id                   AS SupervisorId,
        r.c_registration_number             AS RegistrationNumber,
        r.c_registered_date                 AS RegisteredDate,
        r.c_source                          AS Source,
        r.c_referral_code                   AS ReferralCode,
        -- Document Verification
        r.c_document_verification_status    AS DocumentVerificationStatus,
        r.c_document_verified_by            AS DocumentVerifiedBy,
        r.c_document_verified_date          AS DocumentVerifiedDate,
        r.c_document_rejection_reason       AS DocumentRejectionReason,
        -- Interview
        r.c_interview_scheduled             AS InterviewScheduled,
        r.c_interview_date                  AS InterviewDate,
        r.c_interview_mode                  AS InterviewMode,
        r.c_interviewer_id                  AS InterviewerId,
        r.c_interview_completed             AS InterviewCompleted,
        r.c_interview_notes                 AS InterviewNotes,
        r.c_interview_result                AS InterviewResult,
        -- Training
        r.c_training_module_assigned        AS TrainingModuleAssigned,
        r.c_training_started_date           AS TrainingStartedDate,
        r.c_training_completed_date         AS TrainingCompletedDate,
        r.c_training_completion_percentage  AS TrainingCompletionPercentage,
        r.c_training_passed                 AS TrainingPassed,
        -- Certification
        r.c_certification_test_assigned     AS CertificationTestAssigned,
        r.c_certification_test_date         AS CertificationTestDate,
        r.c_certification_test_score        AS CertificationTestScore,
        r.c_certification_test_passed       AS CertificationTestPassed,
        r.c_certification_certificate_url   AS CertificationCertificateUrl,
        -- Activation
        r.c_activation_status               AS ActivationStatus,
        r.c_activated_date                  AS ActivatedDate,
        r.c_activated_by                    AS ActivatedBy,
        r.c_rejection_reason                AS RejectionReason,
        -- Banking
        r.c_bank_details_submitted          AS BankDetailsSubmitted,
        r.c_bank_details_verified           AS BankDetailsVerified,
        r.c_bank_verification_date          AS BankVerificationDate,
        -- Audit
        r.c_createddate                     AS CreatedDate,
        r.c_modifieddate                    AS ModifiedDate,
        -- From supervisor table
        s.c_full_name                       AS FullName,
        s.c_email                           AS Email,
        s.c_phone                           AS Phone,
        s.c_date_of_birth                   AS DateOfBirth,
        s.c_identity_type                   AS IdentityType,
        s.c_identity_number                 AS IdentityNumber,
        s.c_identity_proof_url              AS IDProofUrl,
        s.c_photo_url                       AS PhotoUrl,
        s.c_current_status                  AS CurrentStatus,
        s.c_authority_level                 AS AuthorityLevel,
        s.c_address_line1                   AS Address,
        s.c_supervisor_type                 AS SupervisorType
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_supervisor_id = @SupervisorId;
END
GO

-- =============================================
-- STAGE PROGRESSION
-- =============================================

CREATE OR ALTER PROCEDURE sp_RejectRegistration
    @RegistrationId BIGINT,
    @RejectedBy BIGINT,
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_activation_status = 'REJECTED',
        c_rejection_reason = @Reason,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    -- Update supervisor status
    UPDATE s
    SET s.c_current_status = 'REJECTED',
        s.c_status_reason = @Reason,
        s.c_modifieddate = GETDATE(),
        s.c_modifiedby = @RejectedBy
    FROM t_sys_supervisor s
    INNER JOIN t_sys_supervisor_registration r ON s.c_supervisor_id = r.c_supervisor_id
    WHERE r.c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

-- =============================================
-- STAGE 1: DOCUMENT VERIFICATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitIdentityProofDocuments
    @RegistrationId BIGINT,
    @IDProofUrl VARCHAR(500) = NULL,
    @AddressProofUrl VARCHAR(500) = NULL,
    @PhotoUrl VARCHAR(500) = NULL,
    @CancelledChequeUrl VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE s
    SET s.c_identity_proof_url = COALESCE(NULLIF(@IDProofUrl, ''), s.c_identity_proof_url),
        s.c_address_url = COALESCE(NULLIF(@AddressProofUrl, ''), s.c_address_url),
        s.c_photo_url = COALESCE(NULLIF(@PhotoUrl, ''), s.c_photo_url),
        s.c_cancelled_cheque_url = COALESCE(NULLIF(@CancelledChequeUrl, ''), s.c_cancelled_cheque_url),
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_supervisor_registration r ON s.c_supervisor_id = r.c_supervisor_id
    WHERE r.c_registration_id = @RegistrationId;

    UPDATE t_sys_supervisor_registration
    SET c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    SELECT CASE WHEN @@ROWCOUNT > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitDocumentVerification
    @RegistrationId BIGINT,
    @VerifiedBy BIGINT,
    @Passed BIT,
    @IDProofVerified BIT,
    @AddressProofVerified BIT,
    @PhotoVerified BIT,
    @VerificationNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_document_verification_status = CASE WHEN @Passed = 1 THEN 'VERIFIED' ELSE 'REJECTED' END,
        c_doc_verification_status       = CASE WHEN @Passed = 1 THEN 'VERIFIED' ELSE 'REJECTED' END,
        c_document_verified_by = @VerifiedBy,
        c_document_verified_date = GETDATE(),
        c_document_rejection_reason = CASE WHEN @Passed = 0 THEN @VerificationNotes ELSE NULL END,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationsPendingDocumentVerification
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_identity_type, s.c_identity_number, s.c_identity_proof_url,
           s.c_photo_url
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_document_verification_status = 'PENDING'
    ORDER BY r.c_registered_date ASC;
END
GO

-- =============================================
-- STAGE 2: INTERVIEW
-- =============================================

CREATE OR ALTER PROCEDURE sp_ScheduleQuickInterview
    @RegistrationId BIGINT,
    @InterviewDateTime DATETIME,
    @InterviewType VARCHAR(20),
    @InterviewerName NVARCHAR(100),
    @MeetingLink VARCHAR(500) = NULL,
    @ScheduledBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_interview_scheduled = 1,
        c_interview_date = @InterviewDateTime,
        c_interview_mode = @InterviewType,
        c_interviewer_id = @ScheduledBy,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitQuickInterviewResult
    @RegistrationId BIGINT,
    @InterviewedBy BIGINT,
    @Passed BIT,
    @Score DECIMAL(5,2),
    @Notes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_interview_completed = 1,
        c_interview_result = CASE WHEN @Passed = 1 THEN 'PASSED' ELSE 'FAILED' END,
        c_interview_notes = @Notes,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationsPendingInterview
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_document_verification_status = 'VERIFIED'
      AND r.c_interview_completed = 0
    ORDER BY r.c_interview_date ASC;
END
GO

-- =============================================
-- STAGE 3: TRAINING
-- =============================================

CREATE OR ALTER PROCEDURE sp_AssignCondensedTraining
    @RegistrationId BIGINT,
    @ModuleIds VARCHAR(500),
    @AssignedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_supervisor_registration WHERE c_registration_id = @RegistrationId;

    -- Mark training as assigned
    UPDATE t_sys_supervisor_registration
    SET c_training_module_assigned = 1,
        c_training_started_date = GETDATE(),
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

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

CREATE OR ALTER PROCEDURE sp_CompleteCondensedTraining
    @RegistrationId BIGINT,
    @CompletedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_training_passed = 1,
        c_training_completed_date = GETDATE(),
        c_training_completion_percentage = 100.00,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    -- Update supervisor training date
    UPDATE s
    SET s.c_training_completed_date = GETDATE(),
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_supervisor_registration r ON s.c_supervisor_id = r.c_supervisor_id
    WHERE r.c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationsPendingTraining
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_interview_result = 'PASSED'
      AND r.c_training_passed = 0
    ORDER BY r.c_registered_date ASC;
END
GO

-- =============================================
-- STAGE 4: CERTIFICATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_ScheduleQuickCertification
    @RegistrationId BIGINT,
    @ExamDate DATETIME,
    @ScheduledBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_certification_test_assigned = 1,
        c_certification_test_date = @ExamDate,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SubmitQuickCertificationResult
    @RegistrationId BIGINT,
    @Passed BIT,
    @ExamScore DECIMAL(5,2),
    @ExamDate DATETIME,
    @CertificateNumber VARCHAR(50) = NULL,
    @EvaluatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_registration
    SET c_certification_test_passed = @Passed,
        c_certification_test_score = @ExamScore,
        c_certification_certificate_url = @CertificateNumber,
        c_modifieddate = GETDATE()
    WHERE c_registration_id = @RegistrationId;

    -- Update supervisor certification
    IF @Passed = 1
    BEGIN
        UPDATE s
        SET s.c_certification_date = @ExamDate,
            s.c_certification_status = 'CERTIFIED',
            s.c_certification_valid_until = DATEADD(YEAR, 1, @ExamDate),
            s.c_modifieddate = GETDATE()
        FROM t_sys_supervisor s
        INNER JOIN t_sys_supervisor_registration r ON s.c_supervisor_id = r.c_supervisor_id
        WHERE r.c_registration_id = @RegistrationId;
    END

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationsPendingCertification
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE r.c_training_passed = 1
      AND r.c_certification_test_passed = 0
    ORDER BY r.c_certification_test_date ASC;
END
GO

-- =============================================
-- BANKING DETAILS
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitBankingDetails
    @SupervisorId BIGINT,
    @AccountHolderName NVARCHAR(100),
    @BankName NVARCHAR(100),
    @AccountNumber VARCHAR(50),
    @IFSCCode VARCHAR(20),
    @BranchName NVARCHAR(100),
    @AccountType VARCHAR(20),
    @CancelledChequeUrl VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_bank_account_holder_name = @AccountHolderName,
        c_bank_name = @BankName,
        c_bank_account_number = @AccountNumber,
        c_bank_ifsc = @IFSCCode,
        c_bank_branch_name = @BranchName,
        c_bank_account_type = @AccountType,
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId;

    -- Mark banking details as submitted in registration
    UPDATE t_sys_supervisor_registration
    SET c_bank_details_submitted = 1,
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetBankingDetails
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c_supervisor_id AS SupervisorId,
           c_bank_account_holder_name AS AccountHolderName,
           c_bank_name AS BankName,
           c_bank_account_number AS AccountNumber,
           c_bank_ifsc AS IFSCCode,
           ISNULL(c_bank_branch_name, '') AS BranchName,
           ISNULL(c_bank_account_type, '') AS AccountType,
           ISNULL(c_cancelled_cheque_url, '') AS CancelledChequeUrl
    FROM t_sys_supervisor
    WHERE c_supervisor_id = @SupervisorId;
END
GO

-- =============================================
-- FINAL ACTIVATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_ActivateRegisteredSupervisor
    @RegistrationId BIGINT,
    @ActivatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Update registration status
        UPDATE t_sys_supervisor_registration
        SET c_activation_status = 'ACTIVATED',
            c_activated_date = GETDATE(),
            c_activated_by = @ActivatedBy,
            c_modifieddate = GETDATE()
        WHERE c_registration_id = @RegistrationId;

        -- Update supervisor to ACTIVE
        UPDATE s
        SET s.c_current_status = 'ACTIVE',
            s.c_is_available = 1,
            s.c_modifieddate = GETDATE(),
            s.c_modifiedby = @ActivatedBy
        FROM t_sys_supervisor s
        INNER JOIN t_sys_supervisor_registration r ON s.c_supervisor_id = r.c_supervisor_id
        WHERE r.c_registration_id = @RegistrationId;

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

CREATE OR ALTER PROCEDURE sp_GetRegistrationProgress
    @RegistrationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.c_registration_id AS RegistrationId,
        r.c_registration_number AS RegistrationNumber,
        -- Stage 1: Document Verification
        CASE WHEN r.c_document_verification_status = 'VERIFIED' THEN 'COMPLETED'
             WHEN r.c_document_verification_status = 'REJECTED' THEN 'REJECTED'
             ELSE 'PENDING' END AS DocumentVerificationStage,
        r.c_document_verified_date AS DocumentVerifiedDate,
        -- Stage 2: Interview
        CASE WHEN r.c_interview_result = 'PASSED' THEN 'COMPLETED'
             WHEN r.c_interview_result = 'FAILED' THEN 'FAILED'
             WHEN r.c_interview_scheduled = 1 THEN 'SCHEDULED'
             ELSE 'PENDING' END AS InterviewStage,
        r.c_interview_date AS InterviewDate,
        -- Stage 3: Training
        CASE WHEN r.c_training_passed = 1 THEN 'COMPLETED'
             WHEN r.c_training_module_assigned = 1 THEN 'IN_PROGRESS'
             ELSE 'PENDING' END AS TrainingStage,
        r.c_training_completed_date AS TrainingCompletedDate,
        -- Stage 4: Certification
        CASE WHEN r.c_certification_test_passed = 1 THEN 'COMPLETED'
             WHEN r.c_certification_test_assigned = 1 THEN 'SCHEDULED'
             ELSE 'PENDING' END AS CertificationStage,
        r.c_certification_test_date AS CertificationDate,
        -- Overall
        r.c_activation_status AS ActivationStatus
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = @RegistrationId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationWorkflowStatus
    @RegistrationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStage VARCHAR(30);

    SELECT @CurrentStage = CASE
        WHEN r.c_activation_status = 'ACTIVATED' THEN 'ACTIVATED'
        WHEN r.c_certification_test_passed = 1 THEN 'CERTIFICATION_PASSED'
        WHEN r.c_certification_test_assigned = 1 THEN 'CERTIFICATION'
        WHEN r.c_training_passed = 1 THEN 'TRAINING_PASSED'
        WHEN r.c_training_module_assigned = 1 THEN 'TRAINING'
        WHEN r.c_interview_result = 'PASSED' THEN 'INTERVIEW_PASSED'
        WHEN r.c_interview_scheduled = 1 THEN 'INTERVIEW'
        WHEN r.c_document_verification_status = 'VERIFIED' THEN 'DOCUMENT_VERIFIED'
        ELSE 'DOCUMENT_VERIFICATION'
    END
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = @RegistrationId;

    SELECT r.c_registration_id AS RegistrationId,
           r.c_registration_number AS RegistrationNumber,
           r.c_supervisor_id AS SupervisorId,
           @CurrentStage AS CurrentStage,
           r.c_activation_status AS ActivationStatus,
           r.c_registered_date AS RegisteredDate,
           r.c_modifieddate AS LastModifiedDate
    FROM t_sys_supervisor_registration r
    WHERE r.c_registration_id = @RegistrationId;
END
GO

-- =============================================
-- ADMIN QUERIES
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetAllRegistrations
    @Status VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (@Status IS NULL OR r.c_activation_status = @Status)
    ORDER BY r.c_registered_date DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationsByStage
    @Stage VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (
        (@Stage = 'DOCUMENT_VERIFICATION' AND r.c_document_verification_status = 'PENDING')
        OR (@Stage = 'INTERVIEW' AND r.c_document_verification_status = 'VERIFIED' AND r.c_interview_completed = 0)
        OR (@Stage = 'TRAINING' AND r.c_interview_result = 'PASSED' AND r.c_training_passed = 0)
        OR (@Stage = 'CERTIFICATION' AND r.c_training_passed = 1 AND r.c_certification_test_passed = 0)
        OR (@Stage = 'ACTIVATED' AND r.c_activation_status = 'ACTIVATED')
    )
    ORDER BY r.c_registered_date ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetRegistrationStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalRegistrations,
        SUM(CASE WHEN c_activation_status = 'ACTIVATED' THEN 1 ELSE 0 END) AS Activated,
        SUM(CASE WHEN c_activation_status = 'REJECTED' THEN 1 ELSE 0 END) AS Rejected,
        SUM(CASE WHEN c_activation_status = 'PENDING' THEN 1 ELSE 0 END) AS Pending,
        SUM(CASE WHEN c_document_verification_status = 'PENDING' THEN 1 ELSE 0 END) AS PendingDocVerification,
        SUM(CASE WHEN c_document_verification_status = 'VERIFIED' AND c_interview_completed = 0 THEN 1 ELSE 0 END) AS PendingInterview,
        SUM(CASE WHEN c_interview_result = 'PASSED' AND c_training_passed = 0 THEN 1 ELSE 0 END) AS PendingTraining,
        SUM(CASE WHEN c_training_passed = 1 AND c_certification_test_passed = 0 THEN 1 ELSE 0 END) AS PendingCertification
    FROM t_sys_supervisor_registration;
END
GO

CREATE OR ALTER PROCEDURE sp_SearchRegistrations
    @Name NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Phone VARCHAR(15) = NULL,
    @Status VARCHAR(20) = NULL,
    @CurrentStage VARCHAR(30) = NULL,
    @ZoneId BIGINT = NULL,
    @RegisteredFrom DATETIME = NULL,
    @RegisteredTo DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, s.c_full_name, s.c_email, s.c_phone,
           s.c_current_status, s.c_supervisor_type
    FROM t_sys_supervisor_registration r
    INNER JOIN t_sys_supervisor s ON r.c_supervisor_id = s.c_supervisor_id
    WHERE (@Name IS NULL OR s.c_full_name LIKE '%' + @Name + '%')
      AND (@Email IS NULL OR s.c_email LIKE '%' + @Email + '%')
      AND (@Phone IS NULL OR s.c_phone LIKE '%' + @Phone + '%')
      AND (@Status IS NULL OR r.c_activation_status = @Status)
      AND (@RegisteredFrom IS NULL OR r.c_registered_date >= @RegisteredFrom)
      AND (@RegisteredTo IS NULL OR r.c_registered_date <= @RegisteredTo)
    ORDER BY r.c_registered_date DESC;
END
GO

PRINT '================================================';
PRINT 'Supervisor Registration Stored Procedures Created';
PRINT '25 Stored Procedures for RegistrationRepository.cs';
PRINT '================================================';
GO
