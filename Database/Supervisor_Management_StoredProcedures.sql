-- =============================================
-- Supervisor Management System - Stored Procedures
-- Business Logic for Both Portals
-- =============================================
-- =============================================
-- FUNCTION: Check Supervisor Authority
-- =============================================

-- Drop existing function
DROP FUNCTION IF EXISTS sp_CheckSupervisorAuthority;

-- Create function (PostgreSQL)
CREATE OR REPLACE FUNCTION sp_CheckSupervisorAuthority(
    p_SupervisorId BIGINT,
    p_RequiredAction VARCHAR
)
RETURNS TABLE (
    IsAuthorized BOOLEAN,
    AuthorityLevel VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorType VARCHAR(20);
    v_CanReleasePayment BOOLEAN;
    v_CanApproveRefund BOOLEAN;
    v_Status VARCHAR(30);
    v_AuthorityLevel VARCHAR(20);
    v_IsAuthorized BOOLEAN := FALSE;
BEGIN

    -- Fetch supervisor details
    SELECT
        c_supervisor_type,
        c_authority_level,
        c_can_release_payment,
        c_can_approve_refund,
        c_current_status
    INTO
        v_SupervisorType,
        v_AuthorityLevel,
        v_CanReleasePayment,
        v_CanApproveRefund,
        v_Status
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId;

    -- If not active → return false
    IF v_Status IS NULL OR v_Status <> 'ACTIVE' THEN
        RETURN QUERY SELECT FALSE, v_AuthorityLevel;
        RETURN;
    END IF;

    -- PAYMENT_RELEASE
    IF p_RequiredAction = 'PAYMENT_RELEASE' THEN
        IF v_SupervisorType = 'CAREER' AND v_CanReleasePayment = TRUE THEN
            v_IsAuthorized := TRUE;
        END IF;
    END IF;

    -- REFUND_APPROVAL
    IF p_RequiredAction = 'REFUND_APPROVAL' THEN
        IF v_SupervisorType = 'CAREER' AND v_CanApproveRefund = TRUE THEN
            v_IsAuthorized := TRUE;
        END IF;
    END IF;

    -- MENTOR_ACCESS
    IF p_RequiredAction = 'MENTOR_ACCESS' THEN
        IF v_SupervisorType = 'CAREER'
           AND v_AuthorityLevel IN ('ADVANCED', 'FULL') THEN
            v_IsAuthorized := TRUE;
        END IF;
    END IF;

    -- QUALITY_CHECK
    IF p_RequiredAction = 'QUALITY_CHECK' THEN
        v_IsAuthorized := TRUE;
    END IF;

    -- EXTRA_PAYMENT_REQUEST
    IF p_RequiredAction = 'EXTRA_PAYMENT_REQUEST' THEN
        v_IsAuthorized := TRUE;
    END IF;

    -- Return result
    RETURN QUERY
    SELECT v_IsAuthorized, v_AuthorityLevel;

END;
$$;

-- =============================================
-- FUNCTION: Progress Careers Application Status
-- =============================================

CREATE OR REPLACE FUNCTION sp_ProgressCareersApplication(
    p_ApplicationId BIGINT,
    p_NewStatus VARCHAR,
    p_AdminId BIGINT,
    p_Notes TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
    v_CurrentStatus VARCHAR(30);
BEGIN
    BEGIN
        -- Get application details
        SELECT c_supervisor_id
        INTO v_SupervisorId
        FROM t_sys_careers_application
        WHERE c_application_id = p_ApplicationId;

        SELECT c_current_status
        INTO v_CurrentStatus
        FROM t_sys_supervisor
        WHERE c_supervisor_id = v_SupervisorId;

        -- Validate status progression
        IF p_NewStatus = 'RESUME_SCREENED' AND v_CurrentStatus <> 'APPLIED' THEN
            RETURN QUERY SELECT FALSE, 'Invalid status progression. Current status: ' || v_CurrentStatus;
            RETURN;
        END IF;

        -- Update supervisor status
        UPDATE t_sys_supervisor
        SET c_current_status = p_NewStatus,
            c_status_reason = p_Notes,
            c_modifieddate = NOW(),
            c_modifiedby = p_AdminId
        WHERE c_supervisor_id = v_SupervisorId;

        -- Update application workflow based on new status
        IF p_NewStatus = 'RESUME_SCREENED' THEN
            UPDATE t_sys_careers_application
            SET c_resume_screened = TRUE,
                c_resume_screened_by = p_AdminId,
                c_resume_screened_date = NOW(),
                c_resume_screening_notes = p_Notes,
                c_resume_screening_status = 'PASSED'
            WHERE c_application_id = p_ApplicationId;
        END IF;

        IF p_NewStatus = 'INTERVIEW_PASSED' THEN
            UPDATE t_sys_careers_application
            SET c_interview_completed = TRUE,
                c_interview_result = 'PASSED',
                c_interview_feedback = p_Notes
            WHERE c_application_id = p_ApplicationId;
        END IF;

        IF p_NewStatus = 'CERTIFIED' THEN
            UPDATE t_sys_careers_application
            SET c_certification_passed = TRUE
            WHERE c_application_id = p_ApplicationId;

            UPDATE t_sys_supervisor
            SET c_certification_status = 'CERTIFIED',
                c_certification_date = NOW()
            WHERE c_supervisor_id = v_SupervisorId;
        END IF;

        IF p_NewStatus = 'ACTIVE' THEN
            UPDATE t_sys_careers_application
            SET c_probation_passed = TRUE,
                c_onboarding_completed = TRUE
            WHERE c_application_id = p_ApplicationId;

            UPDATE t_sys_supervisor
            SET c_authority_level = 'FULL',
                c_can_release_payment = TRUE,
                c_can_approve_refund = TRUE,
                c_can_mentor_others = TRUE
            WHERE c_supervisor_id = v_SupervisorId;
        END IF;

        RETURN QUERY SELECT TRUE, 'Status updated successfully to: ' || p_NewStatus;

    EXCEPTION
        WHEN OTHERS THEN
            RETURN QUERY SELECT FALSE, SQLERRM;
    END;
END;
$$;

-- =============================================
-- FUNCTION: Progress Registration Portal Status
-- =============================================

DROP FUNCTION IF EXISTS sp_progress_registration_status;
CREATE OR REPLACE FUNCTION sp_ProgressRegistrationStatus(
    p_RegistrationId BIGINT,
    p_StageCompleted VARCHAR,
    p_AdminId BIGINT,
    p_Notes TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
    v_AllStagesComplete BOOLEAN := FALSE;
BEGIN
    BEGIN
        SELECT c_supervisor_id
        INTO v_SupervisorId
        FROM t_sys_supervisor_registration
        WHERE c_registration_id = p_RegistrationId;

        IF p_StageCompleted = 'DOCUMENT_VERIFIED' THEN
            UPDATE t_sys_supervisor_registration
            SET c_document_verification_status = 'VERIFIED',
                c_document_verified_by = p_AdminId,
                c_document_verified_date = NOW()
            WHERE c_registration_id = p_RegistrationId;
        END IF;

        IF p_StageCompleted = 'INTERVIEW_PASSED' THEN
            UPDATE t_sys_supervisor_registration
            SET c_interview_completed = TRUE,
                c_interview_result = 'PASSED',
                c_interview_notes = p_Notes
            WHERE c_registration_id = p_RegistrationId;
        END IF;

        IF p_StageCompleted = 'TRAINING_PASSED' THEN
            UPDATE t_sys_supervisor_registration
            SET c_training_passed = TRUE,
                c_training_completed_date = NOW()
            WHERE c_registration_id = p_RegistrationId;

            UPDATE t_sys_supervisor
            SET c_training_completed_date = NOW()
            WHERE c_supervisor_id = v_SupervisorId;
        END IF;

        IF p_StageCompleted = 'CERTIFIED' THEN
            UPDATE t_sys_supervisor_registration
            SET c_certification_test_passed = TRUE
            WHERE c_registration_id = p_RegistrationId;

            UPDATE t_sys_supervisor
            SET c_certification_status = 'CERTIFIED',
                c_certification_date = NOW(),
                c_current_status = 'CERTIFIED'
            WHERE c_supervisor_id = v_SupervisorId;
        END IF;

        SELECT CASE
            WHEN c_document_verification_status = 'VERIFIED'
             AND c_interview_result = 'PASSED'
             AND c_training_passed = TRUE
             AND c_certification_test_passed = TRUE
            THEN TRUE
            ELSE FALSE
        END
        INTO v_AllStagesComplete
        FROM t_sys_supervisor_registration
        WHERE c_registration_id = p_RegistrationId;

        IF v_AllStagesComplete THEN
            UPDATE t_sys_supervisor_registration
            SET c_activation_status = 'ACTIVATED',
                c_activated_date = NOW(),
                c_activated_by = p_AdminId
            WHERE c_registration_id = p_RegistrationId;

            UPDATE t_sys_supervisor
            SET c_current_status = 'ACTIVE',
                c_authority_level = 'BASIC',
                c_can_release_payment = FALSE,
                c_can_approve_refund = FALSE,
                c_can_mentor_others = FALSE
            WHERE c_supervisor_id = v_SupervisorId;

            RETURN QUERY SELECT TRUE, 'Registration completed and supervisor activated';
        ELSE
            RETURN QUERY SELECT TRUE, 'Stage completed: ' || p_StageCompleted;
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RETURN QUERY SELECT FALSE, SQLERRM;
    END;
END;
$$;

-- =============================================
-- SP 4: Find Eligible Supervisors for Assignment
-- =============================================

CREATE OR REPLACE FUNCTION sp_FindEligibleSupervisors(
    p_OrderId BIGINT,
    p_EventDate DATE,
    p_EventType TEXT,
    p_EventValue NUMERIC(18,2),
    p_GuestCount INT,
    p_City TEXT,
    p_Locality TEXT DEFAULT NULL,
    p_IsVIPEvent BOOLEAN DEFAULT FALSE,
    p_IsNewVendor BOOLEAN DEFAULT FALSE
)
RETURNS TABLE (
    SupervisorId BIGINT,
    SupervisorType TEXT,
    FullName TEXT,
    Email TEXT,
    Phone TEXT,
    AuthorityLevel TEXT,
    Experience INT,
    Rating NUMERIC,
    TotalEvents INT,
    City TEXT,
    Locality TEXT,
    PerEventRate NUMERIC,
    CompensationType TEXT,
    PriorityScore INT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_RequiredSupervisorType TEXT := 'EITHER';
    v_RequiredAuthorityLevel TEXT := 'BASIC';
    v_RequiredMinExperience INT := 0;
BEGIN

    SELECT
        COALESCE(c_required_supervisor_type, 'EITHER'),
        COALESCE(c_required_authority_level, 'BASIC'),
        COALESCE(c_required_min_experience, 0)
    INTO
        v_RequiredSupervisorType,
        v_RequiredAuthorityLevel,
        v_RequiredMinExperience
    FROM t_sys_assignment_eligibility_rule
    WHERE c_is_active = TRUE
        AND (c_event_type IS NULL OR c_event_type = p_EventType)
        AND (c_min_event_value IS NULL OR c_min_event_value <= p_EventValue)
        AND (c_max_event_value IS NULL OR c_max_event_value >= p_EventValue)
        AND (c_min_guest_count IS NULL OR c_min_guest_count <= p_GuestCount)
        AND (c_is_vip_event IS NULL OR c_is_vip_event = p_IsVIPEvent)
        AND (c_is_new_vendor IS NULL OR c_is_new_vendor = p_IsNewVendor)
    ORDER BY c_priority ASC
    LIMIT 1;

    RETURN QUERY
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
        CASE
            WHEN s.c_supervisor_type = 'CAREER' THEN 1
            WHEN s.c_supervisor_type = 'REGISTERED' THEN 2
            ELSE 3
        END AS PriorityScore
    FROM t_sys_supervisor s
    WHERE s.c_current_status = 'ACTIVE'
        AND s.c_is_available = TRUE
        AND s.c_city = p_City
        AND (p_Locality IS NULL OR s.c_locality = p_Locality)
        AND s.c_years_of_experience >= v_RequiredMinExperience
        AND (
            v_RequiredSupervisorType = 'EITHER'
            OR s.c_supervisor_type = v_RequiredSupervisorType
        )
        AND (
            s.c_authority_level = v_RequiredAuthorityLevel
            OR (
                v_RequiredAuthorityLevel = 'BASIC' AND s.c_authority_level IN ('BASIC','INTERMEDIATE','ADVANCED','FULL')
            )
            OR (
                v_RequiredAuthorityLevel = 'INTERMEDIATE' AND s.c_authority_level IN ('INTERMEDIATE','ADVANCED','FULL')
            )
            OR (
                v_RequiredAuthorityLevel = 'ADVANCED' AND s.c_authority_level IN ('ADVANCED','FULL')
            )
            OR (
                v_RequiredAuthorityLevel = 'FULL' AND s.c_authority_level = 'FULL'
            )
        )
        AND NOT EXISTS (
            SELECT 1 FROM t_sys_supervisor_assignment sa
            WHERE sa.c_supervisor_id = s.c_supervisor_id
                AND sa.c_event_date = p_EventDate
                AND sa.c_status NOT IN ('REJECTED', 'CANCELLED')
        )
    ORDER BY PriorityScore ASC, s.c_average_rating DESC, s.c_total_events_supervised DESC;

END;
$$;

-- =============================================
-- SP 5: Assign Supervisor to Event
-- =============================================

CREATE OR REPLACE FUNCTION sp_AssignSupervisorToEvent(
    p_OrderId BIGINT,
    p_SupervisorId BIGINT,
    p_AssignedBy BIGINT
)
RETURNS TABLE (
    AssignmentNumber TEXT,
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorStatus VARCHAR(30);
    v_EventDate DATE;
    v_EventType TEXT;
    v_EventValue NUMERIC(18,2);
    v_GuestCount INT;
    v_AssignmentNumber TEXT;
BEGIN
    BEGIN
        -- Validate supervisor is active
        SELECT c_current_status
        INTO v_SupervisorStatus
        FROM t_sys_supervisor
        WHERE c_supervisor_id = p_SupervisorId;

        IF v_SupervisorStatus <> 'ACTIVE' THEN
            RETURN QUERY SELECT NULL::TEXT, FALSE, 'Supervisor is not active. Status: ' || v_SupervisorStatus;
            RETURN;
        END IF;

        -- Get event details
        SELECT
            c_event_date,
            c_event_type,
            c_total_amount,
            c_guest_count
        INTO
            v_EventDate,
            v_EventType,
            v_EventValue,
            v_GuestCount
        FROM t_sys_orders
        WHERE c_orderid = p_OrderId;

        -- Generate assignment number
        v_AssignmentNumber := 'SA-' || TO_CHAR(NOW(), 'YYYYMMDD') || '-' || LPAD(p_OrderId::TEXT, 6, '0');

        -- Create assignment
        INSERT INTO t_sys_supervisor_assignment (
            c_orderid, c_supervisor_id, c_assignment_number, c_assigned_by,
            c_assignment_source, c_event_date, c_event_type, c_event_value,
            c_estimated_guests, c_status
        )
        VALUES (
            p_OrderId, p_SupervisorId, v_AssignmentNumber, p_AssignedBy,
            'MANUAL', v_EventDate, v_EventType, v_EventValue,
            v_GuestCount, 'ASSIGNED'
        );

        -- Update order
        UPDATE t_sys_orders
        SET c_supervisor_id = p_SupervisorId,
            c_supervisor_assigned_date = NOW(),
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        RETURN QUERY SELECT v_AssignmentNumber, TRUE, 'Supervisor assigned successfully';

    EXCEPTION
        WHEN OTHERS THEN
            RETURN QUERY SELECT NULL::TEXT, FALSE, SQLERRM;
    END;
END;
$$;

-- =============================================
-- SP 6: Supervisor Check-In to Event
-- =============================================

CREATE OR REPLACE FUNCTION sp_SupervisorCheckIn(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_CheckInLocation TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_Status VARCHAR(30);
BEGIN
    BEGIN
        SELECT c_status
        INTO v_Status
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = p_AssignmentId
          AND c_supervisor_id = p_SupervisorId;

        IF v_Status NOT IN ('ASSIGNED', 'ACCEPTED') THEN
            RETURN QUERY SELECT FALSE, 'Cannot check-in. Current status: ' || v_Status;
            RETURN;
        END IF;

        UPDATE t_sys_supervisor_assignment
        SET c_status = 'CHECKED_IN',
            c_check_in_time = NOW(),
            c_check_in_location = p_CheckInLocation,
            c_modifieddate = NOW()
        WHERE c_assignment_id = p_AssignmentId;

        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id, c_assignment_id, c_action_type,
            c_action_description, c_gps_location
        )
        VALUES (
            p_SupervisorId, p_AssignmentId, 'CHECK_IN',
            'Supervisor checked in to event', p_CheckInLocation
        );

        RETURN QUERY SELECT TRUE, 'Check-in successful';

    EXCEPTION
        WHEN OTHERS THEN
            RETURN QUERY SELECT FALSE, SQLERRM;
    END;
END;
$$;

-- =============================================
-- SP 7: Request Payment Release (With Authority Check)
-- =============================================

CREATE OR REPLACE FUNCTION sp_RequestPaymentRelease(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_ExtraChargesAmount NUMERIC(18,2) DEFAULT 0,
    p_ExtraChargesReason TEXT DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT,
    RequiresAdminApproval BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorType TEXT;
    v_CanReleasePayment BOOLEAN;
    v_AuthorityLevel TEXT;
    v_OrderId BIGINT;
BEGIN
    BEGIN
        SELECT
            c_supervisor_type,
            c_can_release_payment,
            c_authority_level
        INTO
            v_SupervisorType,
            v_CanReleasePayment,
            v_AuthorityLevel
        FROM t_sys_supervisor
        WHERE c_supervisor_id = p_SupervisorId;

        SELECT c_orderid
        INTO v_OrderId
        FROM t_sys_supervisor_assignment
        WHERE c_assignment_id = p_AssignmentId;

        IF v_SupervisorType = 'CAREER' AND v_CanReleasePayment = TRUE THEN

            UPDATE t_sys_supervisor_assignment
            SET c_payment_release_requested = 1,
                c_payment_release_approved = 1,
                c_payment_approved_by = p_SupervisorId,
                c_payment_approval_date = NOW(),
                c_extra_charges_amount = p_ExtraChargesAmount,
                c_extra_charges_reason = p_ExtraChargesReason
            WHERE c_assignment_id = p_AssignmentId;

            UPDATE t_sys_orders
            SET c_payment_status = 'Released',
                c_modifieddate = NOW()
            WHERE c_orderid = v_OrderId;

            INSERT INTO t_sys_supervisor_action_log (
                c_supervisor_id, c_assignment_id, c_orderid, c_action_type,
                c_action_description, c_authority_level_required, c_authority_check_passed
            )
            VALUES (
                p_SupervisorId, p_AssignmentId, v_OrderId, 'PAYMENT_RELEASE_REQUEST',
                'Payment release requested. Amount: ' || p_ExtraChargesAmount,
                'FULL', v_CanReleasePayment
            );

            RETURN QUERY SELECT TRUE,
                'Payment released successfully (Careers Supervisor Authority)',
                FALSE;

        ELSE

            UPDATE t_sys_supervisor_assignment
            SET c_payment_release_requested = 1,
                c_extra_charges_amount = p_ExtraChargesAmount,
                c_extra_charges_reason = p_ExtraChargesReason
            WHERE c_assignment_id = p_AssignmentId;

            INSERT INTO t_sys_supervisor_action_log (
                c_supervisor_id, c_assignment_id, c_orderid, c_action_type,
                c_action_description, c_authority_level_required, c_authority_check_passed
            )
            VALUES (
                p_SupervisorId, p_AssignmentId, v_OrderId, 'PAYMENT_RELEASE_REQUEST',
                'Payment release requested. Amount: ' || p_ExtraChargesAmount,
                'FULL', v_CanReleasePayment
            );

            RETURN QUERY SELECT TRUE,
                'Payment release requested. Awaiting admin approval (Registered Supervisor - Limited Authority)',
                TRUE;

        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RETURN QUERY SELECT FALSE, SQLERRM, FALSE;
    END;
END;
$$;
