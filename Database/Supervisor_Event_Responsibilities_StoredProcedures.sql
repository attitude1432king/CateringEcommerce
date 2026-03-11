-- =============================================
-- Event Supervisor Responsibilities - Stored Procedures
-- Pre-Event, During-Event, Post-Event Workflows
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Creating Event Supervision Workflow Procedures';
PRINT '================================================';
PRINT '';

-- =============================================
-- SP 1: Submit Pre-Event Verification
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitPreEventVerification
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @MenuVerified BIT,
    @MenuVsContractMatch BIT,
    @RawMaterialVerified BIT,
    @RawMaterialQualityOK BIT,
    @RawMaterialQuantityOK BIT,
    @GuestCountConfirmed BIT,
    @ConfirmedGuestCount INT,
    @PreEventEvidenceUrls NVARCHAR(MAX), -- JSON array
    @IssuesFound BIT,
    @IssuesDescription NVARCHAR(2000) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OrderId BIGINT;
        DECLARE @LockedGuestCount INT;

        -- Get order details
        SELECT @OrderId = c_order_id
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId;

        SELECT @LockedGuestCount = c_locked_guest_count
        FROM t_sys_order
        WHERE c_orderid = @OrderId;

        -- Check if guest count matches locked count
        DECLARE @GuestCountMismatch BIT = 0;
        IF @ConfirmedGuestCount != @LockedGuestCount
            SET @GuestCountMismatch = 1;

        -- Update assignment with pre-event verification
        UPDATE t_sys_supervisor_assignment
        SET c_pre_event_verification_status = CASE WHEN @IssuesFound = 1 THEN 'ISSUES_FOUND' ELSE 'COMPLETED' END,
            c_pre_event_verification_date = GETDATE(),
            c_menu_verified = @MenuVerified,
            c_menu_vs_contract_match = @MenuVsContractMatch,
            c_raw_material_verified = @RawMaterialVerified,
            c_raw_material_quality_ok = @RawMaterialQualityOK,
            c_raw_material_quantity_ok = @RawMaterialQuantityOK,
            c_guest_count_confirmed = @GuestCountConfirmed,
            c_confirmed_guest_count = @ConfirmedGuestCount,
            c_locked_guest_count = @LockedGuestCount,
            c_guest_count_mismatch = @GuestCountMismatch,
            c_guest_count_confirmation_date = GETDATE(),
            c_pre_event_evidence_submitted = 1,
            c_pre_event_evidence_urls = @PreEventEvidenceUrls,
            c_pre_event_evidence_timestamp = GETDATE(),
            c_pre_event_checklist_completed = 1,
            c_pre_event_checklist_completion_date = GETDATE(),
            c_pre_event_issues_found = @IssuesFound,
            c_pre_event_issues_description = @IssuesDescription,
            c_modifieddate = GETDATE()
        WHERE c_assignment_id = @AssignmentId;

        -- Log action
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id, c_assignment_id, c_order_id, c_action_type,
            c_action_description, c_action_data, c_evidence_urls
        )
        VALUES (
            @SupervisorId, @AssignmentId, @OrderId, 'PRE_EVENT_VERIFICATION',
            'Pre-event verification completed. Issues found: ' + CAST(@IssuesFound AS VARCHAR),
            JSON_OBJECT('menu_verified', @MenuVerified, 'material_verified', @RawMaterialVerified, 'guest_count', @ConfirmedGuestCount),
            @PreEventEvidenceUrls
        );

        SET @Success = 1;
        SET @Message = CASE
            WHEN @IssuesFound = 1 THEN 'Pre-event verification completed with issues. Admin notified.'
            WHEN @GuestCountMismatch = 1 THEN 'Verification completed. Guest count mismatch detected!'
            ELSE 'Pre-event verification completed successfully'
        END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_SubmitPreEventVerification';

-- =============================================
-- SP 2: Request Extra Quantity with Client Approval
-- =============================================

CREATE OR ALTER PROCEDURE sp_RequestExtraQuantity
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @ItemName NVARCHAR(200),
    @ExtraQuantity INT,
    @ExtraCost DECIMAL(18,2),
    @Reason NVARCHAR(500),
    @ClientPhone VARCHAR(15),
    @ApprovalMethod VARCHAR(20), -- 'IN_APP', 'OTP', 'SIGNATURE'
    @OTPCode VARCHAR(10) OUTPUT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OrderId BIGINT;

        SELECT @OrderId = c_order_id
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId;

        -- Generate OTP if OTP method
        IF @ApprovalMethod = 'OTP'
        BEGIN
            SET @OTPCode = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS VARCHAR), 6);

            -- Insert OTP record
            INSERT INTO t_sys_client_otp_verification (
                c_assignment_id, c_order_id, c_supervisor_id,
                c_otp_code, c_otp_purpose, c_otp_sent_to,
                c_otp_expires_at, c_context_data
            )
            VALUES (
                @AssignmentId, @OrderId, @SupervisorId,
                @OTPCode, 'EXTRA_QUANTITY_APPROVAL', @ClientPhone,
                DATEADD(MINUTE, 10, GETDATE()),
                JSON_OBJECT('item', @ItemName, 'quantity', @ExtraQuantity, 'cost', @ExtraCost, 'reason', @Reason)
            );
        END

        -- Log extra quantity request
        INSERT INTO t_sys_during_event_tracking (
            c_assignment_id, c_supervisor_id, c_order_id,
            c_tracking_type, c_tracking_description,
            c_extra_item_name, c_extra_quantity, c_extra_cost, c_extra_reason,
            c_approval_method, c_approval_status, c_otp_code
        )
        VALUES (
            @AssignmentId, @SupervisorId, @OrderId,
            'EXTRA_QUANTITY_REQUEST',
            'Extra quantity requested: ' + @ItemName + ' x ' + CAST(@ExtraQuantity AS VARCHAR),
            @ItemName, @ExtraQuantity, @ExtraCost, @Reason,
            @ApprovalMethod, 'PENDING', @OTPCode
        );

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_extra_quantity_requested = 1,
            c_client_approval_required = 1,
            c_client_approval_method = @ApprovalMethod,
            c_client_otp_sent = CASE WHEN @ApprovalMethod = 'OTP' THEN 1 ELSE 0 END,
            c_client_approval_status = 'PENDING'
        WHERE c_assignment_id = @AssignmentId;

        SET @Success = 1;
        SET @Message = CASE
            WHEN @ApprovalMethod = 'OTP' THEN 'OTP sent to client: ' + @ClientPhone + '. Code: ' + @OTPCode
            ELSE 'Approval request sent to client'
        END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_RequestExtraQuantity';

-- =============================================
-- SP 3: Verify Client OTP
-- =============================================

CREATE OR ALTER PROCEDURE sp_VerifyClientOTP
    @AssignmentId BIGINT,
    @OTPCode VARCHAR(10),
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OTPId BIGINT;
        DECLARE @ExpiresAt DATETIME;
        DECLARE @AlreadyVerified BIT;
        DECLARE @Attempts INT;
        DECLARE @MaxAttempts INT;

        -- Get OTP details
        SELECT
            @OTPId = c_otp_id,
            @ExpiresAt = c_otp_expires_at,
            @AlreadyVerified = c_otp_verified,
            @Attempts = c_verification_attempts,
            @MaxAttempts = c_max_attempts
        FROM t_sys_client_otp_verification
        WHERE c_assignment_id = @AssignmentId
            AND c_otp_code = @OTPCode
            AND c_status = 'SENT';

        -- Check if OTP exists
        IF @OTPId IS NULL
        BEGIN
            SET @Success = 0;
            SET @Message = 'Invalid OTP code';
            ROLLBACK;
            RETURN;
        END

        -- Check if already verified
        IF @AlreadyVerified = 1
        BEGIN
            SET @Success = 0;
            SET @Message = 'OTP already verified';
            ROLLBACK;
            RETURN;
        END

        -- Check if expired
        IF @ExpiresAt < GETDATE()
        BEGIN
            UPDATE t_sys_client_otp_verification
            SET c_status = 'EXPIRED'
            WHERE c_otp_id = @OTPId;

            SET @Success = 0;
            SET @Message = 'OTP has expired';
            ROLLBACK;
            RETURN;
        END

        -- Check max attempts
        IF @Attempts >= @MaxAttempts
        BEGIN
            UPDATE t_sys_client_otp_verification
            SET c_status = 'FAILED'
            WHERE c_otp_id = @OTPId;

            SET @Success = 0;
            SET @Message = 'Maximum verification attempts exceeded';
            ROLLBACK;
            RETURN;
        END

        -- Verify OTP
        UPDATE t_sys_client_otp_verification
        SET c_otp_verified = 1,
            c_otp_verified_time = GETDATE(),
            c_status = 'VERIFIED'
        WHERE c_otp_id = @OTPId;

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_client_otp_verified = 1,
            c_client_otp_verification_time = GETDATE(),
            c_client_approval_status = 'APPROVED'
        WHERE c_assignment_id = @AssignmentId;

        -- Update tracking record
        UPDATE t_sys_during_event_tracking
        SET c_otp_verified = 1,
            c_approval_status = 'APPROVED',
            c_approval_timestamp = GETDATE()
        WHERE c_assignment_id = @AssignmentId
            AND c_otp_code = @OTPCode;

        SET @Success = 1;
        SET @Message = 'Client approval verified successfully';

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_VerifyClientOTP';

-- =============================================
-- SP 4: Update Guest Count (During Event)
-- =============================================

CREATE OR ALTER PROCEDURE sp_UpdateGuestCount
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @ActualGuestCount INT,
    @Notes NVARCHAR(500) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OrderId BIGINT;
        DECLARE @ConfirmedCount INT;
        DECLARE @Variance INT;

        SELECT @OrderId = c_order_id, @ConfirmedCount = c_confirmed_guest_count
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId;

        SET @Variance = @ActualGuestCount - @ConfirmedCount;

        -- Log guest count update
        INSERT INTO t_sys_during_event_tracking (
            c_assignment_id, c_supervisor_id, c_order_id,
            c_tracking_type, c_tracking_description,
            c_guest_count, c_guest_count_variance
        )
        VALUES (
            @AssignmentId, @SupervisorId, @OrderId,
            'GUEST_COUNT_UPDATE',
            'Guest count updated to ' + CAST(@ActualGuestCount AS VARCHAR) + '. Variance: ' + CAST(@Variance AS VARCHAR),
            @ActualGuestCount, @Variance
        );

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_actual_guest_count = @ActualGuestCount,
            c_guest_count_variance = @Variance
        WHERE c_assignment_id = @AssignmentId;

        SET @Success = 1;
        SET @Message = 'Guest count updated. Variance: ' + CAST(@Variance AS VARCHAR);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_UpdateGuestCount';

-- =============================================
-- SP 5: Submit Post-Event Report
-- =============================================

CREATE OR ALTER PROCEDURE sp_SubmitPostEventReport
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @FinalGuestCount INT,
    @ClientSatisfactionRating INT,
    @FoodQualityRating INT,
    @FoodQuantityRating INT,
    @ServiceQualityRating INT,
    @ClientComments NVARCHAR(2000),
    @IssuesCount INT,
    @IssuesSummary NVARCHAR(MAX), -- JSON
    @FinalPayableAmount DECIMAL(18,2),
    @ReportSummary NVARCHAR(2000),
    @CompletionPhotos NVARCHAR(MAX), -- JSON array
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OrderId BIGINT;
        DECLARE @EventStartTime DATETIME;
        DECLARE @EventEndTime DATETIME;
        DECLARE @DurationMinutes INT;

        SELECT
            @OrderId = c_order_id,
            @EventStartTime = c_check_in_time,
            @EventEndTime = GETDATE()
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = @AssignmentId;

        SET @DurationMinutes = DATEDIFF(MINUTE, @EventStartTime, @EventEndTime);

        -- Insert post-event report
        INSERT INTO t_sys_post_event_report (
            c_assignment_id, c_supervisor_id, c_order_id,
            c_event_started_time, c_event_ended_time, c_event_duration_minutes,
            c_final_guest_count,
            c_client_satisfaction_rating, c_food_quality_rating, c_food_quantity_rating,
            c_service_quality_rating, c_client_comments,
            c_total_issues_count, c_issues_summary,
            c_final_payable_amount, c_report_summary, c_completion_photos,
            c_submitted_date
        )
        VALUES (
            @AssignmentId, @SupervisorId, @OrderId,
            @EventStartTime, @EventEndTime, @DurationMinutes,
            @FinalGuestCount,
            @ClientSatisfactionRating, @FoodQualityRating, @FoodQuantityRating,
            @ServiceQualityRating, @ClientComments,
            @IssuesCount, @IssuesSummary,
            @FinalPayableAmount, @ReportSummary, @CompletionPhotos,
            GETDATE()
        );

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_post_event_report_submitted = 1,
            c_post_event_report_date = GETDATE(),
            c_client_feedback_collected = 1,
            c_client_overall_satisfaction = @ClientSatisfactionRating,
            c_client_food_quality_rating = @FoodQualityRating,
            c_client_food_quantity_rating = @FoodQuantityRating,
            c_issues_recorded = 1,
            c_issues_count = @IssuesCount,
            c_final_payment_request_raised = 1,
            c_final_payment_request_date = GETDATE(),
            c_final_payment_amount = @FinalPayableAmount,
            c_event_completion_summary = @ReportSummary,
            c_event_completion_photos = @CompletionPhotos,
            c_status = 'COMPLETED',
            c_modifieddate = GETDATE()
        WHERE c_assignment_id = @AssignmentId;

        -- Log action
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id, c_assignment_id, c_order_id, c_action_type,
            c_action_description, c_evidence_urls
        )
        VALUES (
            @SupervisorId, @AssignmentId, @OrderId, 'POST_EVENT_REPORT',
            'Event completion report submitted. Final amount: ₹' + CAST(@FinalPayableAmount AS VARCHAR),
            @CompletionPhotos
        );

        SET @Success = 1;
        SET @Message = 'Post-event report submitted successfully';

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ Created: sp_SubmitPostEventReport';

-- =============================================
-- SP 6: Get Event Supervision Summary
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetEventSupervisionSummary
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Main assignment details
    SELECT
        sa.c_assignment_id AS AssignmentId,
        sa.c_assignment_number AS AssignmentNumber,
        sa.c_event_date AS EventDate,
        sa.c_event_type AS EventType,
        sa.c_status AS Status,

        -- Pre-Event
        sa.c_pre_event_verification_status AS PreEventStatus,
        sa.c_menu_verified AS MenuVerified,
        sa.c_raw_material_verified AS MaterialVerified,
        sa.c_guest_count_confirmed AS GuestCountConfirmed,
        sa.c_confirmed_guest_count AS ConfirmedGuestCount,
        sa.c_pre_event_issues_found AS PreEventIssuesFound,

        -- During-Event
        sa.c_actual_guest_count AS ActualGuestCount,
        sa.c_guest_count_variance AS GuestCountVariance,
        sa.c_extra_quantity_requested AS ExtraQuantityRequested,
        sa.c_client_approval_status AS ClientApprovalStatus,

        -- Post-Event
        sa.c_post_event_report_submitted AS ReportSubmitted,
        sa.c_client_overall_satisfaction AS ClientSatisfaction,
        sa.c_issues_count AS IssuesCount,
        sa.c_final_payment_amount AS FinalPaymentAmount,

        -- Supervisor Details
        s.c_full_name AS SupervisorName,
        s.c_supervisor_type AS SupervisorType

    FROM t_sys_supervisor_assignment sa
    INNER JOIN t_sys_supervisor s ON sa.c_supervisor_id = s.c_supervisor_id
    WHERE sa.c_assignment_id = @AssignmentId;

    -- During-event tracking log
    SELECT
        c_tracking_type AS TrackingType,
        c_tracking_description AS Description,
        c_guest_count AS GuestCount,
        c_extra_item_name AS ExtraItem,
        c_extra_cost AS ExtraCost,
        c_approval_status AS ApprovalStatus,
        c_timestamp AS Timestamp
    FROM t_sys_during_event_tracking
    WHERE c_assignment_id = @AssignmentId
    ORDER BY c_timestamp DESC;

    -- Post-event report (if exists)
    SELECT
        c_final_guest_count AS FinalGuestCount,
        c_client_satisfaction_rating AS SatisfactionRating,
        c_food_quality_rating AS FoodQualityRating,
        c_service_quality_rating AS ServiceQualityRating,
        c_total_issues_count AS IssuesCount,
        c_final_payable_amount AS FinalAmount,
        c_report_summary AS Summary,
        c_submitted_date AS SubmittedDate
    FROM t_sys_post_event_report
    WHERE c_assignment_id = @AssignmentId;
END
GO

PRINT '✓ Created: sp_GetEventSupervisionSummary';

PRINT '';
PRINT '================================================';
PRINT 'Event Supervision Workflow Procedures Created';
PRINT '================================================';
PRINT '';
PRINT 'Procedures Created:';
PRINT '1. sp_SubmitPreEventVerification';
PRINT '2. sp_RequestExtraQuantity (with OTP)';
PRINT '3. sp_VerifyClientOTP';
PRINT '4. sp_UpdateGuestCount';
PRINT '5. sp_SubmitPostEventReport';
PRINT '6. sp_GetEventSupervisionSummary';
PRINT '';
GO
