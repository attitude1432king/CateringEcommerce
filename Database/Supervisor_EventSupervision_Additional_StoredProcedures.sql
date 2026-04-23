-- =============================================
-- Event Supervision Additional Stored Procedures
-- Maps to: EventSupervisionRepository.cs
-- 10 Stored Procedures (supplements existing 7 in
--   Supervisor_Event_Responsibilities_StoredProcedures.sql and
--   Supervisor_Photo_Validation_Migration.sql)
-- =============================================

-- =============================================
-- PRE-EVENT - GET & UPDATE
-- =============================================
DROP FUNCTION IF EXISTS sp_GetPreEventVerification;

CREATE OR REPLACE FUNCTION sp_GetPreEventVerification(
    p_AssignmentId BIGINT
)
RETURNS TABLE (
    AssignmentId BIGINT,
    SupervisorId BIGINT,
    OrderId BIGINT,
    VerificationStatus TEXT,
    VerificationDate TIMESTAMP,
    MenuVerified BOOLEAN,
    MenuVsContractMatch BOOLEAN,
    MenuVerificationNotes TEXT,
    MenuVerificationPhotos TEXT,
    RawMaterialVerified BOOLEAN,
    RawMaterialQualityOK BOOLEAN,
    RawMaterialQuantityOK BOOLEAN,
    RawMaterialNotes TEXT,
    RawMaterialPhotos TEXT,
    GuestCountConfirmed BOOLEAN,
    ConfirmedGuestCount INT,
    LockedGuestCount INT,
    PreEventEvidenceUrls TEXT,
    IssuesFound BOOLEAN,
    IssuesDescription TEXT,
    ChecklistCompleted BOOLEAN,
    ChecklistId BIGINT,
    ChecklistPhotos TEXT,
    FreshnessCheckDone BOOLEAN,
    HygieneCheckDone BOOLEAN,
    PackagingCheckDone BOOLEAN,
    TemperatureCheckDone BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        a.c_assignment_id,
        a.c_supervisor_id,
        a.c_orderid,
        a.c_pre_event_verification_status,
        a.c_pre_event_verification_date,
        a.c_menu_verified,
        a.c_menu_vs_contract_match,
        a.c_menu_verification_notes,
        a.c_menu_verification_photos,
        a.c_raw_material_verified,
        a.c_raw_material_quality_ok,
        a.c_raw_material_quantity_ok,
        a.c_raw_material_notes,
        a.c_raw_material_photos,
        a.c_guest_count_confirmed,
        a.c_confirmed_guest_count,
        a.c_locked_guest_count,
        a.c_pre_event_evidence_urls,
        a.c_pre_event_issues_found,
        a.c_pre_event_issues_description,
        a.c_pre_event_checklist_completed,
        pc.c_checklist_id,
        pc.c_checklist_photos,
        pc.c_freshness_check_done,
        pc.c_hygiene_check_done,
        pc.c_packaging_check_done,
        pc.c_temperature_check_done
    FROM t_sys_supervisor_assignment a
    LEFT JOIN t_sys_pre_event_checklist pc 
        ON a.c_assignment_id = pc.c_assignment_id
    WHERE a.c_assignment_id = p_AssignmentId;
END;
$$;

DROP FUNCTION IF EXISTS sp_UpdatePreEventChecklist;

CREATE OR REPLACE FUNCTION sp_UpdatePreEventChecklist(
    p_ChecklistId BIGINT,
    p_MenuVerified BOOLEAN,
    p_RawMaterialVerified BOOLEAN,
    p_GuestCountConfirmed BOOLEAN,
    p_ChecklistPhotos TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_AssignmentId BIGINT;
BEGIN

    -- Check if checklist exists
    IF p_ChecklistId > 0 AND EXISTS (
        SELECT 1 FROM t_sys_pre_event_checklist 
        WHERE c_checklist_id = p_ChecklistId
    ) THEN

        -- Update checklist
        UPDATE t_sys_pre_event_checklist
        SET c_checklist_photos = COALESCE(p_ChecklistPhotos, c_checklist_photos),
            c_freshness_check_done = p_MenuVerified,
            c_timestamp = NOW()
        WHERE c_checklist_id = p_ChecklistId;

        -- Get assignment id
        SELECT c_assignment_id INTO v_AssignmentId
        FROM t_sys_pre_event_checklist
        WHERE c_checklist_id = p_ChecklistId;

        -- Update assignment
        UPDATE t_sys_supervisor_assignment
        SET c_menu_verified = p_MenuVerified,
            c_raw_material_verified = p_RawMaterialVerified,
            c_guest_count_confirmed = p_GuestCountConfirmed,
            c_modifieddate = NOW()
        WHERE c_assignment_id = v_AssignmentId;

    END IF;

    RETURN QUERY SELECT TRUE;

END;
$$;

-- =============================================
-- DURING-EVENT - FOOD MONITORING & TRACKING
-- =============================================

DROP FUNCTION IF EXISTS sp_RecordFoodServingMonitor;

CREATE OR REPLACE FUNCTION sp_RecordFoodServingMonitor(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_QualityRating INT,
    p_TemperatureOK BOOLEAN,
    p_PresentationOK BOOLEAN,
    p_Notes TEXT DEFAULT NULL,
    p_Photos TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_food_serving_monitored = TRUE,
        c_food_serving_quality_rating = p_QualityRating,
        c_food_serving_temperature_ok = p_TemperatureOK,
        c_food_serving_presentation_ok = p_PresentationOK,
        c_food_serving_notes = p_Notes,
        c_food_serving_photos = p_Photos,
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Get order id
    SELECT c_orderid INTO v_OrderId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Insert tracking record
    INSERT INTO t_sys_during_event_tracking (
        c_assignment_id,
        c_supervisor_id,
        c_orderid,
        c_tracking_type,
        c_tracking_description,
        c_tracking_data,
        c_evidence_urls,
        c_timestamp
    )
    VALUES (
        p_AssignmentId,
        p_SupervisorId,
        v_OrderId,
        'FOOD_SERVING_CHECK',
        'Food serving quality check: Rating ' || p_QualityRating || '/5',
        json_build_object(
            'qualityRating', p_QualityRating,
            'temperatureOK', p_TemperatureOK,
            'presentationOK', p_PresentationOK
        )::TEXT,
        p_Photos,
        NOW()
    );

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id,
        c_assignment_id,
        c_action_type,
        c_action_description,
        c_action_result
    )
    VALUES (
        p_SupervisorId,
        p_AssignmentId,
        'QUALITY_CHECK',
        'Food serving monitored. Quality: ' || p_QualityRating || '/5',
        'SUCCESS'
    );

    RETURN QUERY SELECT TRUE;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetDuringEventTracking;

CREATE OR REPLACE FUNCTION sp_GetDuringEventTracking(
    p_AssignmentId BIGINT
)
RETURNS TABLE (
    TrackingId BIGINT,
    AssignmentId BIGINT,
    SupervisorId BIGINT,
    TrackingType TEXT,
    Description TEXT,
    TrackingData TEXT,
    GuestCount INT,
    GuestCountVariance INT,
    ExtraItemName TEXT,
    ExtraQuantity INT,
    ExtraCost DECIMAL(18,2),
    ExtraReason TEXT,
    ApprovalMethod TEXT,
    ApprovalStatus TEXT,
    EvidenceUrls TEXT,
    GPSLocation TEXT,
    TimestampValue TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        c_tracking_id,
        c_assignment_id,
        c_supervisor_id,
        c_tracking_type,
        c_tracking_description,
        c_tracking_data,
        c_guest_count,
        c_guest_count_variance,
        c_extra_item_name,
        c_extra_quantity,
        c_extra_cost,
        c_extra_reason,
        c_approval_method,
        c_approval_status,
        c_evidence_urls,
        c_gps_location,
        c_timestamp
    FROM t_sys_during_event_tracking
    WHERE c_assignment_id = p_AssignmentId
    ORDER BY c_timestamp ASC;
END;
$$;

-- =============================================
-- POST-EVENT - GET, UPDATE, VERIFY
-- =============================================

DROP FUNCTION IF EXISTS sp_GetPostEventReport;

CREATE OR REPLACE FUNCTION sp_GetPostEventReport(
    p_AssignmentId BIGINT
)
RETURNS TABLE (
    ReportId BIGINT,
    AssignmentId BIGINT,
    SupervisorId BIGINT,
    OrderId BIGINT,
    FinalGuestCount INT,
    EventRating INT,
    ClientName TEXT,
    ClientPhone TEXT,
    ClientSatisfactionRating INT,
    FoodQualityRating INT,
    FoodQuantityRating INT,
    ServiceQualityRating INT,
    PresentationRating INT,
    WouldRecommend BOOLEAN,
    ClientComments TEXT,
    ClientSignatureUrl TEXT,
    VendorPunctualityRating INT,
    VendorPreparationRating INT,
    VendorCooperationRating INT,
    VendorComments TEXT,
    IssuesCount INT,
    IssuesSummary TEXT,
    FinalPayableAmount DECIMAL(18,2),
    PaymentBreakdown TEXT,
    CompletionPhotos TEXT,
    CompletionVideos TEXT,
    ReportSummary TEXT,
    Recommendations TEXT,
    SupervisorNotes TEXT,
    ReportVerified BOOLEAN,
    VerifiedBy BIGINT,
    VerificationDate TIMESTAMP,
    VerificationNotes TEXT,
    CreatedDate TIMESTAMP,
    SubmittedDate TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        r.c_report_id,
        r.c_assignment_id,
        r.c_supervisor_id,
        r.c_orderid,
        r.c_final_guest_count,
        r.c_event_rating,
        r.c_client_name,
        r.c_client_phone,
        r.c_client_satisfaction_rating,
        r.c_food_quality_rating,
        r.c_food_quantity_rating,
        r.c_service_quality_rating,
        r.c_presentation_rating,
        r.c_would_recommend,
        r.c_client_comments,
        r.c_client_signature_url,
        r.c_vendor_punctuality_rating,
        r.c_vendor_preparation_rating,
        r.c_vendor_cooperation_rating,
        r.c_vendor_comments,
        r.c_total_issues_count,
        r.c_issues_summary,
        r.c_final_payable_amount,
        r.c_payment_breakdown,
        r.c_completion_photos,
        r.c_completion_videos,
        r.c_report_summary,
        r.c_recommendations,
        r.c_supervisor_notes,
        r.c_report_verified,
        r.c_verified_by,
        r.c_verification_date,
        r.c_verification_notes,
        r.c_createddate,
        r.c_submitted_date
    FROM t_sys_post_event_report r
    WHERE r.c_assignment_id = p_AssignmentId;
END;
$$;

DROP FUNCTION IF EXISTS sp_UpdatePostEventReport;

CREATE OR REPLACE FUNCTION sp_UpdatePostEventReport(
    p_ReportId BIGINT,
    p_ReportSummary TEXT,
    p_Recommendations TEXT DEFAULT NULL,
    p_SupervisorNotes TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_post_event_report
    SET c_report_summary = p_ReportSummary,
        c_recommendations = p_Recommendations,
        c_supervisor_notes = p_SupervisorNotes
    WHERE c_report_id = p_ReportId;

    RETURN QUERY SELECT TRUE;

END;
$$;

DROP FUNCTION IF EXISTS sp_VerifyPostEventReport;

CREATE OR REPLACE FUNCTION sp_VerifyPostEventReport(
    p_ReportId BIGINT,
    p_VerifiedBy BIGINT,
    p_VerificationNotes TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_AssignmentId BIGINT;
BEGIN

    -- Update report
    UPDATE t_sys_post_event_report
    SET c_report_verified = TRUE,
        c_verified_by = p_VerifiedBy,
        c_verification_date = NOW(),
        c_verification_notes = p_VerificationNotes
    WHERE c_report_id = p_ReportId;

    -- Get assignment id
    SELECT c_assignment_id INTO v_AssignmentId
    FROM t_sys_post_event_report
    WHERE c_report_id = p_ReportId;

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_report_verified_by_admin = p_VerifiedBy,
        c_report_verification_date = NOW(),
        c_modifieddate = NOW()
    WHERE c_assignment_id = v_AssignmentId;

    RETURN QUERY SELECT TRUE;

END;
$$;

-- =============================================
-- OTP MANAGEMENT
-- =============================================

DROP FUNCTION IF EXISTS sp_ResendClientOTP;

CREATE OR REPLACE FUNCTION sp_ResendClientOTP(
    p_AssignmentId BIGINT,
    p_OTPPurpose VARCHAR
)
RETURNS TABLE (
    NewOTPCode VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_NewOTPCode VARCHAR(10);
    v_OrderId BIGINT;
    v_SupervisorId BIGINT;
BEGIN

    -- Generate new 6-digit OTP
    v_NewOTPCode := LPAD((floor(random() * 1000000))::TEXT, 6, '0');

    -- Expire old OTPs
    UPDATE t_sys_client_otp_verification
    SET c_status = 'EXPIRED'
    WHERE c_assignment_id = p_AssignmentId
      AND c_otp_purpose = p_OTPPurpose
      AND c_status = 'SENT';

    -- Get order & supervisor
    SELECT
        c_orderid,
        c_supervisor_id
    INTO
        v_OrderId,
        v_SupervisorId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Insert new OTP
    INSERT INTO t_sys_client_otp_verification (
        c_assignment_id,
        c_orderid,
        c_supervisor_id,
        c_otp_code,
        c_otp_purpose,
        c_otp_sent_to,
        c_otp_sent_time,
        c_otp_expires_at,
        c_status
    )
    VALUES (
        p_AssignmentId,
        v_OrderId,
        v_SupervisorId,
        v_NewOTPCode,
        p_OTPPurpose,
        '',
        NOW(),
        NOW() + INTERVAL '10 minutes',
        'SENT'
    );

    RETURN QUERY SELECT v_NewOTPCode;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetOTPVerificationStatus;

CREATE OR REPLACE FUNCTION sp_GetOTPVerificationStatus(
    p_OTPCode VARCHAR
)
RETURNS TABLE (
    OTPId BIGINT,
    AssignmentId BIGINT,
    OrderId BIGINT,
    SupervisorId BIGINT,
    OTPCode VARCHAR,
    OTPPurpose TEXT,
    SentTo TEXT,
    SentTime TIMESTAMP,
    ExpiresAt TIMESTAMP,
    IsVerified BOOLEAN,
    VerifiedTime TIMESTAMP,
    VerificationAttempts INT,
    MaxAttempts INT,
    Status TEXT,
    ContextData TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_otp_id,
        c_assignment_id,
        c_orderid,
        c_supervisor_id,
        c_otp_code,
        c_otp_purpose,
        c_otp_sent_to,
        c_otp_sent_time,
        c_otp_expires_at,
        c_otp_verified,
        c_otp_verified_time,
        c_verification_attempts,
        c_max_attempts,
        c_status,
        c_context_data
    FROM t_sys_client_otp_verification
    WHERE c_otp_code = p_OTPCode
    ORDER BY c_otp_sent_time DESC;

END;
$$;

-- =============================================
-- EVIDENCE RETRIEVAL
-- =============================================

DROP FUNCTION IF EXISTS sp_GetAssignmentEvidence;

CREATE OR REPLACE FUNCTION sp_GetAssignmentEvidence(
    p_AssignmentId BIGINT
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    v_result JSON;
BEGIN

    SELECT json_build_object(

        -- Pre-event evidence
        'PreEventEvidence',
        a.c_pre_event_evidence_urls,

        -- During-event evidence (array)
        'DuringEventEvidence',
        (
            SELECT COALESCE(json_agg(c_evidence_urls), '[]'::json)
            FROM t_sys_during_event_tracking
            WHERE c_assignment_id = p_AssignmentId
              AND c_evidence_urls IS NOT NULL
        ),

        -- Post-event evidence
        'PostEventEvidence',
        r.c_completion_photos

    )
    INTO v_result
    FROM t_sys_supervisor_assignment a
    LEFT JOIN t_sys_post_event_report r
        ON a.c_assignment_id = r.c_assignment_id
    WHERE a.c_assignment_id = p_AssignmentId;

    RETURN v_result;

END;
$$;
