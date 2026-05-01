-- =============================================
-- Event Supervisor Responsibilities - Stored Procedures
-- Pre-Event, During-Event, Post-Event Workflows
-- =============================================

-- ============================================= 
-- SP 1: Submit Pre-Event Verification 
-- =============================================
DROP FUNCTION IF EXISTS sp_SubmitPreEventVerification;

CREATE OR REPLACE FUNCTION sp_SubmitPreEventVerification(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_MenuVerified BOOLEAN,
    p_MenuVsContractMatch BOOLEAN,
    p_RawMaterialVerified BOOLEAN,
    p_RawMaterialQualityOK BOOLEAN,
    p_RawMaterialQuantityOK BOOLEAN,
    p_GuestCountConfirmed BOOLEAN,
    p_ConfirmedGuestCount INT,
    p_PreEventEvidenceUrls TEXT,
    p_IssuesFound BOOLEAN,
    p_IssuesDescription TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
    v_LockedGuestCount INT;
    v_GuestCountMismatch BOOLEAN := FALSE;
BEGIN

    -- Get order details
    SELECT c_orderid INTO v_OrderId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    SELECT c_locked_guest_count INTO v_LockedGuestCount
    FROM t_sys_orders
    WHERE c_orderid = v_OrderId;

    -- Check mismatch
    IF p_ConfirmedGuestCount <> v_LockedGuestCount THEN
        v_GuestCountMismatch := TRUE;
    END IF;

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_pre_event_verification_status = CASE 
            WHEN p_IssuesFound THEN 'ISSUES_FOUND' 
            ELSE 'COMPLETED' 
        END,
        c_pre_event_verification_date = NOW(),
        c_menu_verified = p_MenuVerified,
        c_menu_vs_contract_match = p_MenuVsContractMatch,
        c_raw_material_verified = p_RawMaterialVerified,
        c_raw_material_quality_ok = p_RawMaterialQualityOK,
        c_raw_material_quantity_ok = p_RawMaterialQuantityOK,
        c_guest_count_confirmed = p_GuestCountConfirmed,
        c_confirmed_guest_count = p_ConfirmedGuestCount,
        c_locked_guest_count = v_LockedGuestCount,
        c_guest_count_mismatch = v_GuestCountMismatch,
        c_guest_count_confirmation_date = NOW(),
        c_pre_event_evidence_submitted = TRUE,
        c_pre_event_evidence_urls = p_PreEventEvidenceUrls,
        c_pre_event_evidence_timestamp = NOW(),
        c_pre_event_checklist_completed = TRUE,
        c_pre_event_checklist_completion_date = NOW(),
        c_pre_event_issues_found = p_IssuesFound,
        c_pre_event_issues_description = p_IssuesDescription,
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_orderid, c_action_type,
        c_action_description, c_action_data, c_evidence_urls
    )
    VALUES (
        p_SupervisorId,
        p_AssignmentId,
        v_OrderId,
        'PRE_EVENT_VERIFICATION',
        'Pre-event verification completed. Issues found: ' || p_IssuesFound::TEXT,
        json_build_object(
            'menu_verified', p_MenuVerified,
            'material_verified', p_RawMaterialVerified,
            'guest_count', p_ConfirmedGuestCount
        ),
        p_PreEventEvidenceUrls
    );

    -- Return success
    RETURN QUERY
    SELECT TRUE,
        CASE
            WHEN p_IssuesFound THEN 'Pre-event verification completed with issues. Admin notified.'
            WHEN v_GuestCountMismatch THEN 'Verification completed. Guest count mismatch detected!'
            ELSE 'Pre-event verification completed successfully'
        END;

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY
        SELECT FALSE, SQLERRM;
END;
$$;

-- ============================================= 
-- SP 2: Request Extra Quantity with Client Approval 
-- =============================================

DROP FUNCTION IF EXISTS sp_RequestExtraQuantity;

CREATE OR REPLACE FUNCTION sp_RequestExtraQuantity(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_ItemName TEXT,
    p_ExtraQuantity INT,
    p_ExtraCost DECIMAL(18,2),
    p_Reason TEXT,
    p_ClientPhone VARCHAR,
    p_ApprovalMethod VARCHAR -- 'IN_APP', 'OTP', 'SIGNATURE'
)
RETURNS TABLE (
    OTPCode VARCHAR,
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
    v_OTPCode VARCHAR(10) := NULL;
BEGIN

    -- Get OrderId
    SELECT c_orderid INTO v_OrderId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Generate OTP if required
    IF p_ApprovalMethod = 'OTP' THEN
        v_OTPCode := LPAD((floor(random() * 1000000))::TEXT, 6, '0');

        -- Insert OTP record
        INSERT INTO t_sys_client_otp_verification (
            c_assignment_id,
            c_orderid,
            c_supervisor_id,
            c_otp_code,
            c_otp_purpose,
            c_otp_sent_to,
            c_otp_expires_at,
            c_context_data
        )
        VALUES (
            p_AssignmentId,
            v_OrderId,
            p_SupervisorId,
            v_OTPCode,
            'EXTRA_QUANTITY_APPROVAL',
            p_ClientPhone,
            NOW() + INTERVAL '10 minutes',
            json_build_object(
                'item', p_ItemName,
                'quantity', p_ExtraQuantity,
                'cost', p_ExtraCost,
                'reason', p_Reason
            )
        );
    END IF;

    -- Log request in tracking
    INSERT INTO t_sys_during_event_tracking (
        c_assignment_id,
        c_supervisor_id,
        c_orderid,
        c_tracking_type,
        c_tracking_description,
        c_extra_item_name,
        c_extra_quantity,
        c_extra_cost,
        c_extra_reason,
        c_approval_method,
        c_approval_status,
        c_otp_code
    )
    VALUES (
        p_AssignmentId,
        p_SupervisorId,
        v_OrderId,
        'EXTRA_QUANTITY_REQUEST',
        'Extra quantity requested: ' || p_ItemName || ' x ' || p_ExtraQuantity,
        p_ItemName,
        p_ExtraQuantity,
        p_ExtraCost,
        p_Reason,
        p_ApprovalMethod,
        'PENDING',
        v_OTPCode
    );

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_extra_quantity_requested = TRUE,
        c_client_approval_required = TRUE,
        c_client_approval_method = p_ApprovalMethod,
        c_client_otp_sent = CASE WHEN p_ApprovalMethod = 'OTP' THEN TRUE ELSE FALSE END,
        c_client_approval_status = 'PENDING'
    WHERE c_assignment_id = p_AssignmentId;

    -- Return result
    RETURN QUERY
    SELECT
        v_OTPCode,
        TRUE,
        CASE
            WHEN p_ApprovalMethod = 'OTP'
                THEN 'OTP sent to client: ' || p_ClientPhone || '. Code: ' || v_OTPCode
            ELSE 'Approval request sent to client'
        END;

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY
        SELECT NULL, FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 3: Verify Client OTP
-- =============================================

DROP FUNCTION IF EXISTS sp_VerifyClientOTP;

CREATE OR REPLACE FUNCTION sp_VerifyClientOTP(
    p_AssignmentId BIGINT,
    p_OTPCode VARCHAR
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OTPId BIGINT;
    v_ExpiresAt TIMESTAMP;
    v_AlreadyVerified BOOLEAN;
    v_Attempts INT;
    v_MaxAttempts INT;
BEGIN

    -- Get OTP details
    SELECT
        c_otp_id,
        c_otp_expires_at,
        c_otp_verified,
        c_verification_attempts,
        c_max_attempts
    INTO
        v_OTPId,
        v_ExpiresAt,
        v_AlreadyVerified,
        v_Attempts,
        v_MaxAttempts
    FROM t_sys_client_otp_verification
    WHERE c_assignment_id = p_AssignmentId
      AND c_otp_code = p_OTPCode
      AND c_status = 'SENT'
    LIMIT 1;

    -- Check if OTP exists
    IF v_OTPId IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invalid OTP code';
        RETURN;
    END IF;

    -- Check if already verified
    IF v_AlreadyVerified THEN
        RETURN QUERY SELECT FALSE, 'OTP already verified';
        RETURN;
    END IF;

    -- Check if expired
    IF v_ExpiresAt < NOW() THEN
        UPDATE t_sys_client_otp_verification
        SET c_status = 'EXPIRED'
        WHERE c_otp_id = v_OTPId;

        RETURN QUERY SELECT FALSE, 'OTP has expired';
        RETURN;
    END IF;

    -- Check max attempts
    IF v_Attempts >= v_MaxAttempts THEN
        UPDATE t_sys_client_otp_verification
        SET c_status = 'FAILED'
        WHERE c_otp_id = v_OTPId;

        RETURN QUERY SELECT FALSE, 'Maximum verification attempts exceeded';
        RETURN;
    END IF;

    -- Verify OTP
    UPDATE t_sys_client_otp_verification
    SET c_otp_verified = TRUE,
        c_otp_verified_time = NOW(),
        c_status = 'VERIFIED'
    WHERE c_otp_id = v_OTPId;

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_client_otp_verified = TRUE,
        c_client_otp_verification_time = NOW(),
        c_client_approval_status = 'APPROVED'
    WHERE c_assignment_id = p_AssignmentId;

    -- Update tracking
    UPDATE t_sys_during_event_tracking
    SET c_otp_verified = TRUE,
        c_approval_status = 'APPROVED',
        c_approval_timestamp = NOW()
    WHERE c_assignment_id = p_AssignmentId
      AND c_otp_code = p_OTPCode;

    RETURN QUERY SELECT TRUE, 'Client approval verified successfully';

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 4: Update Guest Count (During Event)
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateGuestCount;

CREATE OR REPLACE FUNCTION sp_UpdateGuestCount(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_ActualGuestCount INT,
    p_Notes TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
    v_ConfirmedCount INT;
    v_Variance INT;
BEGIN

    -- Get order + confirmed count
    SELECT 
        c_orderid,
        c_confirmed_guest_count
    INTO 
        v_OrderId,
        v_ConfirmedCount
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Calculate variance
    v_Variance := p_ActualGuestCount - COALESCE(v_ConfirmedCount, 0);

    -- Log tracking
    INSERT INTO t_sys_during_event_tracking (
        c_assignment_id,
        c_supervisor_id,
        c_orderid,
        c_tracking_type,
        c_tracking_description,
        c_guest_count,
        c_guest_count_variance
    )
    VALUES (
        p_AssignmentId,
        p_SupervisorId,
        v_OrderId,
        'GUEST_COUNT_UPDATE',
        'Guest count updated to ' || p_ActualGuestCount || 
        '. Variance: ' || v_Variance,
        p_ActualGuestCount,
        v_Variance
    );

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_actual_guest_count = p_ActualGuestCount,
        c_guest_count_variance = v_Variance
    WHERE c_assignment_id = p_AssignmentId;

    -- Return success
    RETURN QUERY
    SELECT TRUE,
           'Guest count updated. Variance: ' || v_Variance;

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY
        SELECT FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 5: Submit Post-Event Report
-- =============================================

DROP FUNCTION IF EXISTS sp_SubmitPostEventReport;

CREATE OR REPLACE FUNCTION sp_SubmitPostEventReport(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_FinalGuestCount INT,
    p_ClientSatisfactionRating INT,
    p_FoodQualityRating INT,
    p_FoodQuantityRating INT,
    p_ServiceQualityRating INT,
    p_ClientComments TEXT,
    p_IssuesCount INT,
    p_IssuesSummary TEXT,
    p_FinalPayableAmount DECIMAL(18,2),
    p_ReportSummary TEXT,
    p_CompletionPhotos TEXT
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
    v_EventStartTime TIMESTAMP;
    v_EventEndTime TIMESTAMP;
    v_DurationMinutes INT;
BEGIN

    -- Get event details
    SELECT
        c_orderid,
        c_check_in_time
    INTO
        v_OrderId,
        v_EventStartTime
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    v_EventEndTime := NOW();

    -- Calculate duration (in minutes)
    v_DurationMinutes := EXTRACT(EPOCH FROM (v_EventEndTime - v_EventStartTime)) / 60;

    -- Insert post-event report
    INSERT INTO t_sys_post_event_report (
        c_assignment_id, c_supervisor_id, c_orderid,
        c_event_started_time, c_event_ended_time, c_event_duration_minutes,
        c_final_guest_count,
        c_client_satisfaction_rating, c_food_quality_rating, c_food_quantity_rating,
        c_service_quality_rating, c_client_comments,
        c_total_issues_count, c_issues_summary,
        c_final_payable_amount, c_report_summary, c_completion_photos,
        c_submitted_date
    )
    VALUES (
        p_AssignmentId, p_SupervisorId, v_OrderId,
        v_EventStartTime, v_EventEndTime, v_DurationMinutes,
        p_FinalGuestCount,
        p_ClientSatisfactionRating, p_FoodQualityRating, p_FoodQuantityRating,
        p_ServiceQualityRating, p_ClientComments,
        p_IssuesCount, p_IssuesSummary,
        p_FinalPayableAmount, p_ReportSummary, p_CompletionPhotos,
        NOW()
    );

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_post_event_report_submitted = TRUE,
        c_post_event_report_date = NOW(),
        c_client_feedback_collected = TRUE,
        c_client_overall_satisfaction = p_ClientSatisfactionRating,
        c_client_food_quality_rating = p_FoodQualityRating,
        c_client_food_quantity_rating = p_FoodQuantityRating,
        c_issues_recorded = TRUE,
        c_issues_count = p_IssuesCount,
        c_final_payment_request_raised = TRUE,
        c_final_payment_request_date = NOW(),
        c_final_payment_amount = p_FinalPayableAmount,
        c_event_completion_summary = p_ReportSummary,
        c_event_completion_photos = p_CompletionPhotos,
        c_status = 'COMPLETED',
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_orderid,
        c_action_type, c_action_description, c_evidence_urls
    )
    VALUES (
        p_SupervisorId,
        p_AssignmentId,
        v_OrderId,
        'POST_EVENT_REPORT',
        'Event completion report submitted. Final amount: ₹' || p_FinalPayableAmount,
        p_CompletionPhotos
    );

    -- Return success
    RETURN QUERY
    SELECT TRUE, 'Post-event report submitted successfully';

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY
        SELECT FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 6: Get Event Supervision Summary
-- =============================================

DROP FUNCTION IF EXISTS sp_GetEventSupervisionSummary;

CREATE OR REPLACE FUNCTION sp_GetEventSupervisionSummary(
    p_AssignmentId BIGINT
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    v_result JSON;
BEGIN

    SELECT json_build_object(

        -- Main Assignment
        'assignment', (
            SELECT row_to_json(t)
            FROM (
                SELECT
                    sa.c_assignment_id AS "AssignmentId",
                    sa.c_assignment_number AS "AssignmentNumber",
                    sa.c_event_date AS "EventDate",
                    sa.c_event_type AS "EventType",
                    sa.c_status AS "Status",

                    -- Pre-Event
                    sa.c_pre_event_verification_status AS "PreEventStatus",
                    sa.c_menu_verified AS "MenuVerified",
                    sa.c_raw_material_verified AS "MaterialVerified",
                    sa.c_guest_count_confirmed AS "GuestCountConfirmed",
                    sa.c_confirmed_guest_count AS "ConfirmedGuestCount",
                    sa.c_pre_event_issues_found AS "PreEventIssuesFound",

                    -- During-Event
                    sa.c_actual_guest_count AS "ActualGuestCount",
                    sa.c_guest_count_variance AS "GuestCountVariance",
                    sa.c_extra_quantity_requested AS "ExtraQuantityRequested",
                    sa.c_client_approval_status AS "ClientApprovalStatus",

                    -- Post-Event
                    sa.c_post_event_report_submitted AS "ReportSubmitted",
                    sa.c_client_overall_satisfaction AS "ClientSatisfaction",
                    sa.c_issues_count AS "IssuesCount",
                    sa.c_final_payment_amount AS "FinalPaymentAmount",

                    -- Supervisor
                    s.c_full_name AS "SupervisorName",
                    s.c_supervisor_type AS "SupervisorType"

                FROM t_sys_supervisor_assignment sa
                INNER JOIN t_sys_supervisor s 
                    ON sa.c_supervisor_id = s.c_supervisor_id
                WHERE sa.c_assignment_id = p_AssignmentId
            ) t
        ),

        -- Tracking Logs
        'tracking', (
            SELECT COALESCE(json_agg(t), '[]')
            FROM (
                SELECT
                    c_tracking_type AS "TrackingType",
                    c_tracking_description AS "Description",
                    c_guest_count AS "GuestCount",
                    c_extra_item_name AS "ExtraItem",
                    c_extra_cost AS "ExtraCost",
                    c_approval_status AS "ApprovalStatus",
                    c_timestamp AS "Timestamp"
                FROM t_sys_during_event_tracking
                WHERE c_assignment_id = p_AssignmentId
                ORDER BY c_timestamp DESC
            ) t
        ),

        -- Post Event Report
        'post_event', (
            SELECT COALESCE(json_agg(t), '[]')
            FROM (
                SELECT
                    c_final_guest_count AS "FinalGuestCount",
                    c_client_satisfaction_rating AS "SatisfactionRating",
                    c_food_quality_rating AS "FoodQualityRating",
                    c_service_quality_rating AS "ServiceQualityRating",
                    c_total_issues_count AS "IssuesCount",
                    c_final_payable_amount AS "FinalAmount",
                    c_report_summary AS "Summary",
                    c_submitted_date AS "SubmittedDate"
                FROM t_sys_post_event_report
                WHERE c_assignment_id = p_AssignmentId
            ) t
        )

    ) INTO v_result;

    RETURN v_result;

END;
$$;


