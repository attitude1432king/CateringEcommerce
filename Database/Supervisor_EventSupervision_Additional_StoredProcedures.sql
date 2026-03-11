-- =============================================
-- Event Supervision Additional Stored Procedures
-- Maps to: EventSupervisionRepository.cs
-- 10 Stored Procedures (supplements existing 7 in
--   Supervisor_Event_Responsibilities_StoredProcedures.sql and
--   Supervisor_Photo_Validation_Migration.sql)
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- PRE-EVENT - GET & UPDATE
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetPreEventVerification
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.c_assignment_id AS AssignmentId,
        a.c_supervisor_id AS SupervisorId,
        a.c_order_id AS OrderId,
        a.c_pre_event_verification_status AS VerificationStatus,
        a.c_pre_event_verification_date AS VerificationDate,
        a.c_menu_verified AS MenuVerified,
        a.c_menu_vs_contract_match AS MenuVsContractMatch,
        a.c_menu_verification_notes AS MenuVerificationNotes,
        a.c_menu_verification_photos AS MenuVerificationPhotos,
        a.c_raw_material_verified AS RawMaterialVerified,
        a.c_raw_material_quality_ok AS RawMaterialQualityOK,
        a.c_raw_material_quantity_ok AS RawMaterialQuantityOK,
        a.c_raw_material_notes AS RawMaterialNotes,
        a.c_raw_material_photos AS RawMaterialPhotos,
        a.c_guest_count_confirmed AS GuestCountConfirmed,
        a.c_confirmed_guest_count AS ConfirmedGuestCount,
        a.c_locked_guest_count AS LockedGuestCount,
        a.c_pre_event_evidence_urls AS PreEventEvidenceUrls,
        a.c_pre_event_issues_found AS IssuesFound,
        a.c_pre_event_issues_description AS IssuesDescription,
        a.c_pre_event_checklist_completed AS ChecklistCompleted,
        -- Checklist details
        pc.c_checklist_id AS ChecklistId,
        pc.c_checklist_photos AS ChecklistPhotos,
        pc.c_freshness_check_done AS FreshnessCheckDone,
        pc.c_hygiene_check_done AS HygieneCheckDone,
        pc.c_packaging_check_done AS PackagingCheckDone,
        pc.c_temperature_check_done AS TemperatureCheckDone
    FROM t_sys_supervisor_assignment a
    LEFT JOIN t_sys_pre_event_checklist pc ON a.c_assignment_id = pc.c_assignment_id
    WHERE a.c_assignment_id = @AssignmentId;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdatePreEventChecklist
    @ChecklistId BIGINT,
    @MenuVerified BIT,
    @RawMaterialVerified BIT,
    @GuestCountConfirmed BIT,
    @ChecklistPhotos TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ChecklistId > 0 AND EXISTS (SELECT 1 FROM t_sys_pre_event_checklist WHERE c_checklist_id = @ChecklistId)
    BEGIN
        UPDATE t_sys_pre_event_checklist
        SET c_checklist_photos = ISNULL(@ChecklistPhotos, c_checklist_photos),
            c_freshness_check_done = @MenuVerified,
            c_timestamp = GETDATE()
        WHERE c_checklist_id = @ChecklistId;

        -- Also update the assignment
        DECLARE @AssignmentId BIGINT;
        SELECT @AssignmentId = c_assignment_id FROM t_sys_pre_event_checklist WHERE c_checklist_id = @ChecklistId;

        UPDATE t_sys_supervisor_assignment
        SET c_menu_verified = @MenuVerified,
            c_raw_material_verified = @RawMaterialVerified,
            c_guest_count_confirmed = @GuestCountConfirmed,
            c_modifieddate = GETDATE()
        WHERE c_assignment_id = @AssignmentId;
    END

    SELECT 1 AS Success;
END
GO

-- =============================================
-- DURING-EVENT - FOOD MONITORING & TRACKING
-- =============================================

CREATE OR ALTER PROCEDURE sp_RecordFoodServingMonitor
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @QualityRating INT,
    @TemperatureOK BIT,
    @PresentationOK BIT,
    @Notes NVARCHAR(1000) = NULL,
    @Photos TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_food_serving_monitored = 1,
        c_food_serving_quality_rating = @QualityRating,
        c_food_serving_temperature_ok = @TemperatureOK,
        c_food_serving_presentation_ok = @PresentationOK,
        c_food_serving_notes = @Notes,
        c_food_serving_photos = @Photos,
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId;

    -- Insert tracking record
    INSERT INTO t_sys_during_event_tracking (
        c_assignment_id, c_supervisor_id, c_order_id,
        c_tracking_type, c_tracking_description, c_tracking_data,
        c_evidence_urls, c_timestamp
    )
    SELECT
        @AssignmentId, @SupervisorId, c_order_id,
        'FOOD_SERVING_CHECK',
        'Food serving quality check: Rating ' + CAST(@QualityRating AS VARCHAR(2)) + '/5',
        '{"qualityRating":' + CAST(@QualityRating AS VARCHAR(2)) +
        ',"temperatureOK":' + CASE WHEN @TemperatureOK = 1 THEN 'true' ELSE 'false' END +
        ',"presentationOK":' + CASE WHEN @PresentationOK = 1 THEN 'true' ELSE 'false' END + '}',
        @Photos, GETDATE()
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = @AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'QUALITY_CHECK', 'Food serving monitored. Quality: ' + CAST(@QualityRating AS VARCHAR(2)) + '/5', 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetDuringEventTracking
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c_tracking_id AS TrackingId,
        c_assignment_id AS AssignmentId,
        c_supervisor_id AS SupervisorId,
        c_tracking_type AS TrackingType,
        c_tracking_description AS Description,
        c_tracking_data AS TrackingData,
        c_guest_count AS GuestCount,
        c_guest_count_variance AS GuestCountVariance,
        c_extra_item_name AS ExtraItemName,
        c_extra_quantity AS ExtraQuantity,
        c_extra_cost AS ExtraCost,
        c_extra_reason AS ExtraReason,
        c_approval_method AS ApprovalMethod,
        c_approval_status AS ApprovalStatus,
        c_evidence_urls AS EvidenceUrls,
        c_gps_location AS GPSLocation,
        c_timestamp AS Timestamp
    FROM t_sys_during_event_tracking
    WHERE c_assignment_id = @AssignmentId
    ORDER BY c_timestamp ASC;
END
GO

-- =============================================
-- POST-EVENT - GET, UPDATE, VERIFY
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetPostEventReport
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.c_report_id AS ReportId,
        r.c_assignment_id AS AssignmentId,
        r.c_supervisor_id AS SupervisorId,
        r.c_order_id AS OrderId,
        r.c_final_guest_count AS FinalGuestCount,
        r.c_event_rating AS EventRating,
        r.c_client_name AS ClientName,
        r.c_client_phone AS ClientPhone,
        r.c_client_satisfaction_rating AS ClientSatisfactionRating,
        r.c_food_quality_rating AS FoodQualityRating,
        r.c_food_quantity_rating AS FoodQuantityRating,
        r.c_service_quality_rating AS ServiceQualityRating,
        r.c_presentation_rating AS PresentationRating,
        r.c_would_recommend AS WouldRecommend,
        r.c_client_comments AS ClientComments,
        r.c_client_signature_url AS ClientSignatureUrl,
        r.c_vendor_punctuality_rating AS VendorPunctualityRating,
        r.c_vendor_preparation_rating AS VendorPreparationRating,
        r.c_vendor_cooperation_rating AS VendorCooperationRating,
        r.c_vendor_comments AS VendorComments,
        r.c_total_issues_count AS IssuesCount,
        r.c_issues_summary AS IssuesSummary,
        r.c_final_payable_amount AS FinalPayableAmount,
        r.c_payment_breakdown AS PaymentBreakdown,
        r.c_completion_photos AS CompletionPhotos,
        r.c_completion_videos AS CompletionVideos,
        r.c_report_summary AS ReportSummary,
        r.c_recommendations AS Recommendations,
        r.c_supervisor_notes AS SupervisorNotes,
        r.c_report_verified AS ReportVerified,
        r.c_verified_by AS VerifiedBy,
        r.c_verification_date AS VerificationDate,
        r.c_verification_notes AS VerificationNotes,
        r.c_createddate AS CreatedDate,
        r.c_submitted_date AS SubmittedDate
    FROM t_sys_post_event_report r
    WHERE r.c_assignment_id = @AssignmentId;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdatePostEventReport
    @ReportId BIGINT,
    @ReportSummary NVARCHAR(2000),
    @Recommendations NVARCHAR(1000) = NULL,
    @SupervisorNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_post_event_report
    SET c_report_summary = @ReportSummary,
        c_recommendations = @Recommendations,
        c_supervisor_notes = @SupervisorNotes
    WHERE c_report_id = @ReportId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_VerifyPostEventReport
    @ReportId BIGINT,
    @VerifiedBy BIGINT,
    @VerificationNotes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_post_event_report
    SET c_report_verified = 1,
        c_verified_by = @VerifiedBy,
        c_verification_date = GETDATE(),
        c_verification_notes = @VerificationNotes
    WHERE c_report_id = @ReportId;

    -- Also update the assignment
    UPDATE a
    SET a.c_report_verified_by_admin = @VerifiedBy,
        a.c_report_verification_date = GETDATE(),
        a.c_modifieddate = GETDATE()
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_post_event_report r ON a.c_assignment_id = r.c_assignment_id
    WHERE r.c_report_id = @ReportId;

    SELECT 1 AS Success;
END
GO

-- =============================================
-- OTP MANAGEMENT
-- =============================================

CREATE OR ALTER PROCEDURE sp_ResendClientOTP
    @AssignmentId BIGINT,
    @OTPPurpose VARCHAR(50),
    @NewOTPCode VARCHAR(10) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Generate a new 6-digit OTP
    SET @NewOTPCode = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS VARCHAR(6)), 6);

    -- Expire old OTPs for same assignment and purpose
    UPDATE t_sys_client_otp_verification
    SET c_status = 'EXPIRED'
    WHERE c_assignment_id = @AssignmentId
      AND c_otp_purpose = @OTPPurpose
      AND c_status = 'SENT';

    -- Get order and supervisor info
    DECLARE @OrderId BIGINT, @SupervisorId BIGINT, @ClientPhone VARCHAR(15);

    SELECT @OrderId = a.c_order_id, @SupervisorId = a.c_supervisor_id
    FROM t_sys_supervisor_assignment a
    WHERE a.c_assignment_id = @AssignmentId;

    -- Insert new OTP
    INSERT INTO t_sys_client_otp_verification (
        c_assignment_id, c_order_id, c_supervisor_id,
        c_otp_code, c_otp_purpose, c_otp_sent_to,
        c_otp_sent_time, c_otp_expires_at, c_status
    )
    VALUES (
        @AssignmentId, @OrderId, @SupervisorId,
        @NewOTPCode, @OTPPurpose, '',
        GETDATE(), DATEADD(MINUTE, 10, GETDATE()), 'SENT'
    );
END
GO

CREATE OR ALTER PROCEDURE sp_GetOTPVerificationStatus
    @OTPCode VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c_otp_id AS OTPId,
        c_assignment_id AS AssignmentId,
        c_order_id AS OrderId,
        c_supervisor_id AS SupervisorId,
        c_otp_code AS OTPCode,
        c_otp_purpose AS OTPPurpose,
        c_otp_sent_to AS SentTo,
        c_otp_sent_time AS SentTime,
        c_otp_expires_at AS ExpiresAt,
        c_otp_verified AS IsVerified,
        c_otp_verified_time AS VerifiedTime,
        c_verification_attempts AS VerificationAttempts,
        c_max_attempts AS MaxAttempts,
        c_status AS Status,
        c_context_data AS ContextData
    FROM t_sys_client_otp_verification
    WHERE c_otp_code = @OTPCode
    ORDER BY c_otp_sent_time DESC;
END
GO

-- =============================================
-- EVIDENCE RETRIEVAL
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetAssignmentEvidence
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Collect evidence from multiple sources
    SELECT
        a.c_pre_event_evidence_urls AS PreEventEvidence,
        -- Collect during-event evidence from tracking table
        (SELECT c_evidence_urls FROM t_sys_during_event_tracking
         WHERE c_assignment_id = @AssignmentId AND c_evidence_urls IS NOT NULL
         FOR JSON PATH) AS DuringEventEvidence,
        -- Post-event evidence from report
        r.c_completion_photos AS PostEventEvidence
    FROM t_sys_supervisor_assignment a
    LEFT JOIN t_sys_post_event_report r ON a.c_assignment_id = r.c_assignment_id
    WHERE a.c_assignment_id = @AssignmentId;
END
GO

PRINT '================================================';
PRINT 'Event Supervision Additional Stored Procedures Created';
PRINT '10 Stored Procedures for EventSupervisionRepository.cs';
PRINT '================================================';
GO
