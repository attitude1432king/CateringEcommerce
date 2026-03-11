-- =============================================
-- Supervisor Management System - Stored Procedures
-- Business Logic for Both Portals
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Creating Supervisor Management Stored Procedures';
PRINT '================================================';
PRINT '';

-- =============================================
-- SP 1: Check Authority Level for Action
-- =============================================

CREATE OR ALTER PROCEDURE sp_CheckSupervisorAuthority
    @SupervisorId BIGINT,
    @RequiredAction VARCHAR(50), -- 'PAYMENT_RELEASE', 'REFUND_APPROVAL', 'MENTOR_ACCESS'
    @IsAuthorized BIT OUTPUT,
    @AuthorityLevel VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SupervisorType VARCHAR(20);
    DECLARE @CanReleasePayment BIT;
    DECLARE @CanApproveRefund BIT;
    DECLARE @Status VARCHAR(30);

    -- Get supervisor details
    SELECT
        @SupervisorType = c_supervisor_type,
        @AuthorityLevel = c_authority_level,
        @CanReleasePayment = c_can_release_payment,
        @CanApproveRefund = c_can_approve_refund,
        @Status = c_current_status
    FROM t_sys_supervisor
    WHERE c_supervisor_id = @SupervisorId;

    -- Default: Not authorized
    SET @IsAuthorized = 0;

    -- Check if supervisor is active
    IF @Status != 'ACTIVE'
    BEGIN
        RETURN;
    END

    -- Check specific action permissions
    IF @RequiredAction = 'PAYMENT_RELEASE'
    BEGIN
        -- Only CAREER supervisors with FULL authority can release payments
        IF @SupervisorType = 'CAREER' AND @CanReleasePayment = 1
            SET @IsAuthorized = 1;
    END

    IF @RequiredAction = 'REFUND_APPROVAL'
    BEGIN
        -- Only CAREER supervisors can approve refunds
        IF @SupervisorType = 'CAREER' AND @CanApproveRefund = 1
            SET @IsAuthorized = 1;
    END

    IF @RequiredAction = 'MENTOR_ACCESS'
    BEGIN
        -- Only CAREER supervisors with ADVANCED or FULL authority
        IF @SupervisorType = 'CAREER' AND @AuthorityLevel IN ('ADVANCED', 'FULL')
            SET @IsAuthorized = 1;
    END

    IF @RequiredAction = 'QUALITY_CHECK'
    BEGIN
        -- Both types can do quality checks
        SET @IsAuthorized = 1;
    END

    IF @RequiredAction = 'EXTRA_PAYMENT_REQUEST'
    BEGIN
        -- Both types can request extra payments (but not approve)
        SET @IsAuthorized = 1;
    END
END
GO

PRINT '✓ Created: sp_CheckSupervisorAuthority';

-- =============================================
-- SP 2: Progress Careers Application Status
-- =============================================

CREATE OR ALTER PROCEDURE sp_ProgressCareersApplication
    @ApplicationId BIGINT,
    @NewStatus VARCHAR(30),
    @AdminId BIGINT,
    @Notes NVARCHAR(1000) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SupervisorId BIGINT;
        DECLARE @CurrentStatus VARCHAR(30);

        -- Get application details
        SELECT @SupervisorId = c_supervisor_id
        FROM t_sys_careers_application
        WHERE c_application_id = @ApplicationId;

        SELECT @CurrentStatus = c_current_status
        FROM t_sys_supervisor
        WHERE c_supervisor_id = @SupervisorId;

        -- Validate status progression
        IF @NewStatus = 'RESUME_SCREENED' AND @CurrentStatus != 'APPLIED'
        BEGIN
            SET @Success = 0;
            SET @Message = 'Invalid status progression. Current status: ' + @CurrentStatus;
            ROLLBACK;
            RETURN;
        END

        -- Update supervisor status
        UPDATE t_sys_supervisor
        SET c_current_status = @NewStatus,
            c_status_reason = @Notes,
            c_modifieddate = GETDATE(),
            c_modified_by = @AdminId
        WHERE c_supervisor_id = @SupervisorId;

        -- Update application workflow based on new status
        IF @NewStatus = 'RESUME_SCREENED'
        BEGIN
            UPDATE t_sys_careers_application
            SET c_resume_screened = 1,
                c_resume_screened_by = @AdminId,
                c_resume_screened_date = GETDATE(),
                c_resume_screening_notes = @Notes,
                c_resume_screening_status = 'PASSED'
            WHERE c_application_id = @ApplicationId;
        END

        IF @NewStatus = 'INTERVIEW_PASSED'
        BEGIN
            UPDATE t_sys_careers_application
            SET c_interview_completed = 1,
                c_interview_result = 'PASSED',
                c_interview_feedback = @Notes
            WHERE c_application_id = @ApplicationId;
        END

        IF @NewStatus = 'CERTIFIED'
        BEGIN
            UPDATE t_sys_careers_application
            SET c_certification_passed = 1
            WHERE c_application_id = @ApplicationId;

            UPDATE t_sys_supervisor
            SET c_certification_status = 'CERTIFIED',
                c_certification_date = GETDATE()
            WHERE c_supervisor_id = @SupervisorId;
        END

        IF @NewStatus = 'ACTIVE'
        BEGIN
            UPDATE t_sys_careers_application
            SET c_probation_passed = 1,
                c_onboarding_completed = 1
            WHERE c_application_id = @ApplicationId;

            -- Grant full authority to careers supervisors
            UPDATE t_sys_supervisor
            SET c_authority_level = 'FULL',
                c_can_release_payment = 1,
                c_can_approve_refund = 1,
                c_can_mentor_others = 1
            WHERE c_supervisor_id = @SupervisorId;
        END

        SET @Success = 1;
        SET @Message = 'Status updated successfully to: ' + @NewStatus;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_ProgressCareersApplication';

-- =============================================
-- SP 3: Progress Registration Portal Status
-- =============================================

CREATE OR ALTER PROCEDURE sp_ProgressRegistrationStatus
    @RegistrationId BIGINT,
    @StageCompleted VARCHAR(30), -- 'DOCUMENT_VERIFIED', 'INTERVIEW_PASSED', 'TRAINING_PASSED', 'CERTIFIED'
    @AdminId BIGINT,
    @Notes NVARCHAR(1000) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SupervisorId BIGINT;
        DECLARE @AllStagesComplete BIT = 0;

        SELECT @SupervisorId = c_supervisor_id
        FROM t_sys_supervisor_registration
        WHERE c_registration_id = @RegistrationId;

        -- Update registration stage
        IF @StageCompleted = 'DOCUMENT_VERIFIED'
        BEGIN
            UPDATE t_sys_supervisor_registration
            SET c_document_verification_status = 'VERIFIED',
                c_document_verified_by = @AdminId,
                c_document_verified_date = GETDATE()
            WHERE c_registration_id = @RegistrationId;
        END

        IF @StageCompleted = 'INTERVIEW_PASSED'
        BEGIN
            UPDATE t_sys_supervisor_registration
            SET c_interview_completed = 1,
                c_interview_result = 'PASSED',
                c_interview_notes = @Notes
            WHERE c_registration_id = @RegistrationId;
        END

        IF @StageCompleted = 'TRAINING_PASSED'
        BEGIN
            UPDATE t_sys_supervisor_registration
            SET c_training_passed = 1,
                c_training_completed_date = GETDATE()
            WHERE c_registration_id = @RegistrationId;

            UPDATE t_sys_supervisor
            SET c_training_completed_date = GETDATE()
            WHERE c_supervisor_id = @SupervisorId;
        END

        IF @StageCompleted = 'CERTIFIED'
        BEGIN
            UPDATE t_sys_supervisor_registration
            SET c_certification_test_passed = 1
            WHERE c_registration_id = @RegistrationId;

            UPDATE t_sys_supervisor
            SET c_certification_status = 'CERTIFIED',
                c_certification_date = GETDATE(),
                c_current_status = 'CERTIFIED'
            WHERE c_supervisor_id = @SupervisorId;
        END

        -- Check if all stages are complete
        SELECT @AllStagesComplete = CASE
            WHEN c_document_verification_status = 'VERIFIED'
                AND c_interview_result = 'PASSED'
                AND c_training_passed = 1
                AND c_certification_test_passed = 1
            THEN 1
            ELSE 0
        END
        FROM t_sys_supervisor_registration
        WHERE c_registration_id = @RegistrationId;

        -- Auto-activate if all stages complete
        IF @AllStagesComplete = 1
        BEGIN
            UPDATE t_sys_supervisor_registration
            SET c_activation_status = 'ACTIVATED',
                c_activated_date = GETDATE(),
                c_activated_by = @AdminId
            WHERE c_registration_id = @RegistrationId;

            UPDATE t_sys_supervisor
            SET c_current_status = 'ACTIVE',
                c_authority_level = 'BASIC', -- Limited authority for registered supervisors
                c_can_release_payment = 0, -- CANNOT release payments
                c_can_approve_refund = 0, -- CANNOT approve refunds
                c_can_mentor_others = 0
            WHERE c_supervisor_id = @SupervisorId;

            SET @Message = 'Registration completed and supervisor activated';
        END
        ELSE
        BEGIN
            SET @Message = 'Stage completed: ' + @StageCompleted;
        END

        SET @Success = 1;
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_ProgressRegistrationStatus';

-- =============================================
-- SP 4: Find Eligible Supervisors for Assignment
-- =============================================

CREATE OR ALTER PROCEDURE sp_FindEligibleSupervisors
    @OrderId BIGINT,
    @EventDate DATE,
    @EventType NVARCHAR(100),
    @EventValue DECIMAL(18,2),
    @GuestCount INT,
    @City NVARCHAR(50),
    @Locality NVARCHAR(100) = NULL,
    @IsVIPEvent BIT = 0,
    @IsNewVendor BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Step 1: Check eligibility rules
    DECLARE @RequiredSupervisorType VARCHAR(20) = 'EITHER';
    DECLARE @RequiredAuthorityLevel VARCHAR(20) = 'BASIC';
    DECLARE @RequiredMinExperience INT = 0;

    -- Find matching rule (highest priority)
    SELECT TOP 1
        @RequiredSupervisorType = ISNULL(c_required_supervisor_type, 'EITHER'),
        @RequiredAuthorityLevel = ISNULL(c_required_authority_level, 'BASIC'),
        @RequiredMinExperience = ISNULL(c_required_min_experience, 0)
    FROM t_sys_assignment_eligibility_rule
    WHERE c_is_active = 1
        AND (c_event_type IS NULL OR c_event_type = @EventType)
        AND (c_min_event_value IS NULL OR c_min_event_value <= @EventValue)
        AND (c_max_event_value IS NULL OR c_max_event_value >= @EventValue)
        AND (c_min_guest_count IS NULL OR c_min_guest_count <= @GuestCount)
        AND (c_is_vip_event IS NULL OR c_is_vip_event = @IsVIPEvent)
        AND (c_is_new_vendor IS NULL OR c_is_new_vendor = @IsNewVendor)
    ORDER BY c_priority ASC;

    -- Step 2: Find eligible supervisors
    SELECT
        s.c_supervisor_id AS SupervisorId,
        s.c_supervisor_type AS SupervisorType,
        s.c_full_name AS FullName,
        s.c_email AS Email,
        s.c_phone AS Phone,
        s.c_authority_level AS AuthorityLevel,
        s.c_years_of_experience AS Experience,
        s.c_average_rating AS Rating,
        s.c_total_events_supervised AS TotalEvents,
        s.c_city AS City,
        s.c_locality AS Locality,
        s.c_per_event_rate AS PerEventRate,
        s.c_compensation_type AS CompensationType,
        -- Priority score (lower is better)
        CASE
            WHEN s.c_supervisor_type = 'CAREER' THEN 1
            WHEN s.c_supervisor_type = 'REGISTERED' THEN 2
            ELSE 3
        END AS PriorityScore
    FROM t_sys_supervisor s
    WHERE s.c_current_status = 'ACTIVE'
        AND s.c_is_available = 1
        AND s.c_city = @City
        AND (@Locality IS NULL OR s.c_locality = @Locality)
        AND s.c_years_of_experience >= @RequiredMinExperience
        AND (
            @RequiredSupervisorType = 'EITHER'
            OR s.c_supervisor_type = @RequiredSupervisorType
        )
        AND (
            s.c_authority_level = @RequiredAuthorityLevel
            OR s.c_authority_level IN (
                CASE @RequiredAuthorityLevel
                    WHEN 'BASIC' THEN 'BASIC,INTERMEDIATE,ADVANCED,FULL'
                    WHEN 'INTERMEDIATE' THEN 'INTERMEDIATE,ADVANCED,FULL'
                    WHEN 'ADVANCED' THEN 'ADVANCED,FULL'
                    WHEN 'FULL' THEN 'FULL'
                END
            )
        )
        AND NOT EXISTS (
            -- Not already assigned to another event on same date
            SELECT 1 FROM t_sys_supervisor_assignment sa
            WHERE sa.c_supervisor_id = s.c_supervisor_id
                AND sa.c_event_date = @EventDate
                AND sa.c_status NOT IN ('REJECTED', 'CANCELLED')
        )
    ORDER BY PriorityScore ASC, s.c_average_rating DESC, s.c_total_events_supervised DESC;
END
GO

PRINT '✓ Created: sp_FindEligibleSupervisors';

-- =============================================
-- SP 5: Assign Supervisor to Event
-- =============================================

CREATE OR ALTER PROCEDURE sp_AssignSupervisorToEvent
    @OrderId BIGINT,
    @SupervisorId BIGINT,
    @AssignedBy BIGINT, -- Admin ID
    @AssignmentNumber VARCHAR(50) OUTPUT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SupervisorStatus VARCHAR(30);
        DECLARE @EventDate DATE;
        DECLARE @EventType NVARCHAR(100);
        DECLARE @EventValue DECIMAL(18,2);
        DECLARE @GuestCount INT;

        -- Validate supervisor is active
        SELECT @SupervisorStatus = c_current_status
        FROM t_sys_supervisor
        WHERE c_supervisor_id = @SupervisorId;

        IF @SupervisorStatus != 'ACTIVE'
        BEGIN
            SET @Success = 0;
            SET @Message = 'Supervisor is not active. Status: ' + @SupervisorStatus;
            ROLLBACK;
            RETURN;
        END

        -- Get event details
        SELECT
            @EventDate = c_event_date,
            @EventType = c_event_type,
            @EventValue = c_total_amount,
            @GuestCount = c_guest_count
        FROM t_sys_order
        WHERE c_orderid = @OrderId;

        -- Generate assignment number
        SET @AssignmentNumber = 'SA-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('000000' + CAST(@OrderId AS VARCHAR), 6);

        -- Create assignment
        INSERT INTO t_sys_supervisor_assignment (
            c_order_id, c_supervisor_id, c_assignment_number, c_assigned_by,
            c_assignment_source, c_event_date, c_event_type, c_event_value,
            c_estimated_guests, c_status
        )
        VALUES (
            @OrderId, @SupervisorId, @AssignmentNumber, @AssignedBy,
            'MANUAL', @EventDate, @EventType, @EventValue,
            @GuestCount, 'ASSIGNED'
        );

        -- Update order with supervisor assignment
        UPDATE t_sys_order
        SET c_supervisor_id = @SupervisorId,
            c_supervisor_assigned_date = GETDATE(),
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        SET @Success = 1;
        SET @Message = 'Supervisor assigned successfully';
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_AssignSupervisorToEvent';

-- =============================================
-- SP 6: Supervisor Check-In to Event
-- =============================================

CREATE OR ALTER PROCEDURE sp_SupervisorCheckIn
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @CheckInLocation VARCHAR(500) = NULL, -- GPS coordinates
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Status VARCHAR(30);

        SELECT @Status = c_status
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId
            AND c_supervisor_id = @SupervisorId;

        IF @Status NOT IN ('ASSIGNED', 'ACCEPTED')
        BEGIN
            SET @Success = 0;
            SET @Message = 'Cannot check-in. Current status: ' + @Status;
            ROLLBACK;
            RETURN;
        END

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_status = 'CHECKED_IN',
            c_check_in_time = GETDATE(),
            c_check_in_location = @CheckInLocation,
            c_modifieddate = GETDATE()
        WHERE c_assignment_id = @AssignmentId;

        -- Log action
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id, c_assignment_id, c_action_type,
            c_action_description, c_gps_location
        )
        VALUES (
            @SupervisorId, @AssignmentId, 'CHECK_IN',
            'Supervisor checked in to event', @CheckInLocation
        );

        SET @Success = 1;
        SET @Message = 'Check-in successful';
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_SupervisorCheckIn';

-- =============================================
-- SP 7: Request Payment Release (With Authority Check)
-- =============================================

CREATE OR ALTER PROCEDURE sp_RequestPaymentRelease
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @ExtraChargesAmount DECIMAL(18,2) = 0,
    @ExtraChargesReason NVARCHAR(500) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT,
    @RequiresAdminApproval BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @SupervisorType VARCHAR(20);
        DECLARE @CanReleasePayment BIT;
        DECLARE @AuthorityLevel VARCHAR(20);
        DECLARE @OrderId BIGINT;

        -- Get supervisor details
        SELECT
            @SupervisorType = c_supervisor_type,
            @CanReleasePayment = c_can_release_payment,
            @AuthorityLevel = c_authority_level
        FROM t_sys_supervisor
        WHERE c_supervisor_id = @SupervisorId;

        SELECT @OrderId = c_order_id
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId;

        -- CAREER supervisors with FULL authority can release payment directly
        IF @SupervisorType = 'CAREER' AND @CanReleasePayment = 1
        BEGIN
            SET @RequiresAdminApproval = 0;

            UPDATE t_sys_supervisor_assignment
            SET c_payment_release_requested = 1,
                c_payment_release_approved = 1,
                c_payment_approved_by = @SupervisorId,
                c_payment_approval_date = GETDATE(),
                c_extra_charges_amount = @ExtraChargesAmount,
                c_extra_charges_reason = @ExtraChargesReason
            WHERE c_assignment_id = @AssignmentId;

            -- Update order payment status
            UPDATE t_sys_order
            SET c_payment_status = 'Released',
                c_modifieddate = GETDATE()
            WHERE c_orderid = @OrderId;

            SET @Message = 'Payment released successfully (Careers Supervisor Authority)';
        END
        ELSE
        BEGIN
            -- REGISTERED supervisors can only REQUEST payment release
            SET @RequiresAdminApproval = 1;

            UPDATE t_sys_supervisor_assignment
            SET c_payment_release_requested = 1,
                c_extra_charges_amount = @ExtraChargesAmount,
                c_extra_charges_reason = @ExtraChargesReason
            WHERE c_assignment_id = @AssignmentId;

            SET @Message = 'Payment release requested. Awaiting admin approval (Registered Supervisor - Limited Authority)';
        END

        -- Log action
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id, c_assignment_id, c_order_id, c_action_type,
            c_action_description, c_authority_level_required, c_authority_check_passed
        )
        VALUES (
            @SupervisorId, @AssignmentId, @OrderId, 'PAYMENT_RELEASE_REQUEST',
            'Payment release requested. Amount: ' + CAST(@ExtraChargesAmount AS VARCHAR),
            'FULL', @CanReleasePayment
        );

        SET @Success = 1;
    END TRY
    BEGIN CATCH
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_RequestPaymentRelease';

PRINT '';
PRINT '================================================';
PRINT 'Supervisor Management Stored Procedures Created';
PRINT '================================================';
PRINT '';
GO
