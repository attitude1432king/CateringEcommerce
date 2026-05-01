-- =============================================
-- Supervisor Assignment Stored Procedures
-- Maps to: SupervisorAssignmentRepository.cs
-- 16 Stored Procedures for Assignment CRUD, Actions, Analytics, Search
-- (Excludes 4 already in Supervisor_Management_StoredProcedures.sql:
--  sp_FindEligibleSupervisors, sp_AssignSupervisorToEvent,
--  sp_SupervisorCheckIn, sp_RequestPaymentRelease)
-- =============================================

-- =============================================
-- ASSIGNMENT CREATION (PostgreSQL - FINAL)
-- =============================================

DROP PROCEDURE IF EXISTS sp_BulkAssignSupervisor;

CREATE OR REPLACE PROCEDURE sp_BulkAssignSupervisor(
    IN p_OrderId BIGINT,
    IN p_SupervisorId BIGINT,
    IN p_AssignedBy BIGINT,
    INOUT p_AssignmentId BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_AssignmentNumber VARCHAR(50);

    v_EventDate DATE;
    v_EventLocation VARCHAR(200);
    v_EventValue DECIMAL(18,2);
    v_GuestCount INT;
BEGIN
    -- Generate Assignment Number (same logic)
    v_AssignmentNumber :=
        'ASG-' || TO_CHAR(NOW(), 'YYYYMMDD') || '-' ||
        p_OrderId || '-' ||
        p_SupervisorId;

    -- Get event details
    SELECT
        c_event_date,
        c_delivery_address,
        c_total_amount,
        c_guest_count
    INTO
        v_EventDate,
        v_EventLocation,
        v_EventValue,
        v_GuestCount
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId;

    -- Insert assignment
    INSERT INTO t_sys_supervisor_assignment (
        c_orderid, c_supervisor_id, c_assignment_number, c_assigned_date,
        c_assigned_by, c_assignment_source, c_event_date, c_event_location,
        c_estimated_guests, c_event_value, c_status
    )
    VALUES (
        p_OrderId,
        p_SupervisorId,
        v_AssignmentNumber,
        NOW(),
        p_AssignedBy,
        'MANUAL',
        COALESCE(v_EventDate, NOW()),
        v_EventLocation,
        v_GuestCount,
        v_EventValue,
        'ASSIGNED'
    )
    RETURNING c_assignment_id INTO p_AssignmentId;

END;
$$;

-- =============================================
-- ASSIGNMENT RETRIEVAL
-- =============================================

DROP FUNCTION IF EXISTS sp_GetAssignmentById;

CREATE OR REPLACE FUNCTION sp_GetAssignmentById(
    p_AssignmentId BIGINT
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorEmail VARCHAR,
    SupervisorPhone VARCHAR,
    SupervisorType VARCHAR,
    AuthorityLevel INT,
    SupervisorPhoto VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_supervisor_type,
        s.c_authority_level,
        s.c_photo_url
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_assignment_id = p_AssignmentId;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetAssignmentsBySupervisor;

CREATE OR REPLACE FUNCTION sp_GetAssignmentsBySupervisor(
    p_SupervisorId BIGINT,
    p_Status VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a,
        s.c_full_name
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_supervisor_id = p_SupervisorId
      AND (p_Status IS NULL OR a.c_status = p_Status)
    ORDER BY a.c_event_date DESC;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetAssignmentsByOrder;

CREATE OR REPLACE FUNCTION sp_GetAssignmentsByOrder(
    p_OrderId BIGINT
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorEmail VARCHAR,
    SupervisorPhone VARCHAR,
    SupervisorType VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_supervisor_type
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_orderid = p_OrderId
    ORDER BY a.c_assigned_date DESC;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetAllAssignments;

CREATE OR REPLACE FUNCTION sp_GetAllAssignments(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorType VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a,
        s.c_full_name,
        s.c_supervisor_type
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (p_FromDate IS NULL OR a.c_event_date >= p_FromDate)
      AND (p_ToDate IS NULL OR a.c_event_date <= p_ToDate)
    ORDER BY a.c_event_date DESC;
END;
$$;

-- =============================================
-- SUPERVISOR ACTIONS
-- =============================================

DROP FUNCTION IF EXISTS sp_AcceptAssignment;

CREATE OR REPLACE FUNCTION sp_AcceptAssignment(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_status = 'ACCEPTED',
        c_accepted_date = NOW(),
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId
      AND c_supervisor_id = p_SupervisorId
      AND c_status = 'ASSIGNED';

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        p_AssignmentId,
        'ASSIGNMENT_ACCEPTED',
        'Assignment accepted by supervisor',
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_RejectAssignment;

CREATE OR REPLACE FUNCTION sp_RejectAssignment(
    p_AssignmentId BIGINT,
    p_SupervisorId BIGINT,
    p_Reason VARCHAR
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_status = 'REJECTED',
        c_rejection_reason = p_Reason,
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId
      AND c_supervisor_id = p_SupervisorId
      AND c_status = 'ASSIGNED';

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        p_AssignmentId,
        'ASSIGNMENT_REJECTED',
        'Assignment rejected. Reason: ' || p_Reason,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_ApprovePaymentRelease;

CREATE OR REPLACE FUNCTION sp_ApprovePaymentRelease(
    p_AssignmentId BIGINT,
    p_ApprovedBy BIGINT,
    p_Notes VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_payment_release_approved = 1,
        c_payment_approved_by = p_ApprovedBy,
        c_payment_approval_date = NOW(),
        c_supervisor_payout_status = 'PROCESSED',
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Get supervisor id
    SELECT c_supervisor_id INTO v_SupervisorId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        v_SupervisorId,
        p_AssignmentId,
        'PAYMENT_RELEASE_REQUEST',
        'Payment release approved by admin',
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

-- =============================================
-- ASSIGNMENT STATUS MANAGEMENT
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateAssignmentStatus;

CREATE OR REPLACE FUNCTION sp_UpdateAssignmentStatus(
    p_AssignmentId BIGINT,
    p_NewStatus VARCHAR,
    p_UpdatedBy BIGINT,
    p_Notes VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OldStatus VARCHAR(30);
    v_SupervisorId BIGINT;
BEGIN

    -- Get old status
    SELECT c_status INTO v_OldStatus
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Update status
    UPDATE t_sys_supervisor_assignment
    SET c_status = p_NewStatus,
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Get supervisor
    SELECT c_supervisor_id INTO v_SupervisorId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        v_SupervisorId,
        p_AssignmentId,
        'STATUS_CHANGED',
        'Assignment status: ' || v_OldStatus || ' -> ' || p_NewStatus,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_CancelAssignment;

CREATE OR REPLACE FUNCTION sp_CancelAssignment(
    p_AssignmentId BIGINT,
    p_CancelledBy BIGINT,
    p_Reason VARCHAR
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_status = 'CANCELLED',
        c_rejection_reason = p_Reason,
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Get supervisor
    SELECT c_supervisor_id INTO v_SupervisorId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        v_SupervisorId,
        p_AssignmentId,
        'STATUS_CHANGED',
        'Assignment cancelled. Reason: ' || p_Reason,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_CompleteAssignment;

CREATE OR REPLACE FUNCTION sp_CompleteAssignment(
    p_AssignmentId BIGINT,
    p_CompletedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_SupervisorId BIGINT;
BEGIN

    -- Update assignment
    UPDATE t_sys_supervisor_assignment
    SET c_status = 'COMPLETED',
        c_check_out_time = NOW(),
        c_modifieddate = NOW()
    WHERE c_assignment_id = p_AssignmentId;

    -- Increment supervisor event count
    UPDATE t_sys_supervisor s
    SET c_total_events_supervised = s.c_total_events_supervised + 1,
        c_modifieddate = NOW()
    FROM t_sys_supervisor_assignment a
    WHERE s.c_supervisor_id = a.c_supervisor_id
      AND a.c_assignment_id = p_AssignmentId;

    -- Get supervisor
    SELECT c_supervisor_id INTO v_SupervisorId
    FROM t_sys_supervisor_assignment
    WHERE c_assignment_id = p_AssignmentId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_assignment_id, c_action_type,
        c_action_description, c_action_result
    )
    VALUES (
        v_SupervisorId,
        p_AssignmentId,
        'CHECK_OUT',
        'Assignment completed',
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

-- =============================================
-- ANALYTICS & REPORTING
-- =============================================

DROP FUNCTION IF EXISTS sp_GetUpcomingAssignments;

CREATE OR REPLACE FUNCTION sp_GetUpcomingAssignments(
    p_DaysAhead INT DEFAULT 7
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorPhone VARCHAR,
    SupervisorType VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        a,
        s.c_full_name,
        s.c_phone,
        s.c_supervisor_type
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_event_date BETWEEN CURRENT_DATE
        AND (CURRENT_DATE + (p_DaysAhead || ' days')::INTERVAL)
      AND a.c_status IN ('ASSIGNED', 'ACCEPTED')
    ORDER BY a.c_event_date ASC, a.c_event_time ASC;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetOverdueAssignments;

CREATE OR REPLACE FUNCTION sp_GetOverdueAssignments()
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorPhone VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        a,
        s.c_full_name,
        s.c_phone
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_event_date < CURRENT_DATE
      AND a.c_status IN ('ASSIGNED', 'ACCEPTED')
    ORDER BY a.c_event_date ASC;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetAssignmentStatistics;

CREATE OR REPLACE FUNCTION sp_GetAssignmentStatistics(
    p_FromDate TIMESTAMP,
    p_ToDate TIMESTAMP
)
RETURNS TABLE (
    TotalAssignments BIGINT,
    Assigned BIGINT,
    Accepted BIGINT,
    Rejected BIGINT,
    CheckedIn BIGINT,
    InProgress BIGINT,
    Completed BIGINT,
    Cancelled BIGINT,
    NoShows BIGINT,
    TotalPayoutAmount DECIMAL,
    AvgQualityRating DECIMAL
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        COUNT(*) AS TotalAssignments,
        SUM(CASE WHEN c_status = 'ASSIGNED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'ACCEPTED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'REJECTED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'CHECKED_IN' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'IN_PROGRESS' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'COMPLETED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'CANCELLED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_status = 'NO_SHOW' THEN 1 ELSE 0 END),
        COALESCE(SUM(c_supervisor_payout_amount), 0),
        COALESCE(AVG(c_quality_rating::DECIMAL(3,2)), 0)
    FROM t_sys_supervisor_assignment
    WHERE c_event_date BETWEEN p_FromDate AND p_ToDate;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorWorkload;

CREATE OR REPLACE FUNCTION sp_GetSupervisorWorkload(
    p_FromDate TIMESTAMP,
    p_ToDate TIMESTAMP
)
RETURNS TABLE (
    SupervisorId BIGINT,
    FullName VARCHAR,
    SupervisorType VARCHAR,
    TotalAssignments BIGINT,
    Completed BIGINT,
    Upcoming BIGINT,
    AvgRating DECIMAL,
    TotalPayout DECIMAL
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        s.c_supervisor_id,
        s.c_full_name,
        s.c_supervisor_type,
        COUNT(a.c_assignment_id),
        SUM(CASE WHEN a.c_status = 'COMPLETED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN a.c_status IN ('ASSIGNED','ACCEPTED') THEN 1 ELSE 0 END),
        AVG(a.c_quality_rating::DECIMAL(3,2)),
        COALESCE(SUM(a.c_supervisor_payout_amount), 0)
    FROM t_sys_supervisor s
    LEFT JOIN t_sys_supervisor_assignment a 
        ON s.c_supervisor_id = a.c_supervisor_id
        AND a.c_event_date BETWEEN p_FromDate AND p_ToDate
    WHERE s.c_is_deleted = FALSE 
      AND s.c_current_status = 'ACTIVE'
    GROUP BY s.c_supervisor_id, s.c_full_name, s.c_supervisor_type
    ORDER BY COUNT(a.c_assignment_id) DESC;

END;
$$;

-- =============================================
-- SEARCH & FILTERING (PostgreSQL)
-- =============================================

DROP FUNCTION IF EXISTS sp_SearchAssignments;

CREATE OR REPLACE FUNCTION sp_SearchAssignments(
    p_SupervisorId BIGINT DEFAULT NULL,
    p_OrderId BIGINT DEFAULT NULL,
    p_Status VARCHAR DEFAULT NULL,
    p_EventDateFrom DATE DEFAULT NULL,
    p_EventDateTo DATE DEFAULT NULL,
    p_SupervisorType VARCHAR DEFAULT NULL,
    p_PaymentReleased BOOLEAN DEFAULT NULL
)
RETURNS TABLE (
    a t_sys_supervisor_assignment,
    SupervisorName VARCHAR,
    SupervisorType VARCHAR,
    AuthorityLevel INT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        a,
        s.c_full_name,
        s.c_supervisor_type,
        s.c_authority_level
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s 
        ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (p_SupervisorId IS NULL OR a.c_supervisor_id = p_SupervisorId)
      AND (p_OrderId IS NULL OR a.c_orderid = p_OrderId)
      AND (p_Status IS NULL OR a.c_status = p_Status)
      AND (p_EventDateFrom IS NULL OR a.c_event_date >= p_EventDateFrom)
      AND (p_EventDateTo IS NULL OR a.c_event_date <= p_EventDateTo)
      AND (p_SupervisorType IS NULL OR s.c_supervisor_type = p_SupervisorType)
      AND (p_PaymentReleased IS NULL OR a.c_payment_release_approved = p_PaymentReleased)
    ORDER BY a.c_event_date DESC;

END;
$$;

