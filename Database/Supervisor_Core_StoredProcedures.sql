-- =============================================
-- Supervisor Core Stored Procedures
-- Maps to: SupervisorRepository.cs
-- 24 Stored Procedures for Supervisor CRUD, Authority, Status, Dashboard, Availability, Search
-- =============================================

-- =============================================
-- BASIC CRUD
-- =============================================

DROP FUNCTION IF EXISTS sp_CheckEmailExists;

CREATE OR REPLACE FUNCTION sp_CheckEmailExists(
    p_Email VARCHAR
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM t_sys_supervisor
        WHERE c_email = p_Email
    ) INTO v_exists;

    RETURN v_exists;
END;
$$;

DROP FUNCTION IF EXISTS sp_CheckPhoneExists;

CREATE OR REPLACE FUNCTION sp_CheckPhoneExists(
    p_Phone VARCHAR
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM t_sys_supervisor
        WHERE c_phone = p_Phone
    ) INTO v_exists;

    RETURN v_exists;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorById;

CREATE OR REPLACE FUNCTION sp_GetSupervisorById(
    p_SupervisorId BIGINT
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId
      AND c_is_deleted = FALSE;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorByEmail;

CREATE OR REPLACE FUNCTION sp_GetSupervisorByEmail(
    p_Email VARCHAR
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_email = p_Email
      AND c_is_deleted = FALSE;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorByPhone;

CREATE OR REPLACE FUNCTION sp_GetSupervisorByPhone(
    p_Phone VARCHAR
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_phone = p_Phone
      AND c_is_deleted = FALSE;
END;
$$;

DROP FUNCTION IF EXISTS sp_GetAllSupervisors;

CREATE OR REPLACE FUNCTION sp_GetAllSupervisors(
    p_SupervisorType VARCHAR DEFAULT NULL,
    p_Status VARCHAR DEFAULT NULL
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_is_deleted = FALSE
      AND (p_SupervisorType IS NULL OR c_supervisor_type = p_SupervisorType)
      AND (p_Status IS NULL OR c_current_status = p_Status)
    ORDER BY c_createddate DESC;
END;
$$;

DROP FUNCTION IF EXISTS sp_UpdateSupervisor;

CREATE OR REPLACE FUNCTION sp_UpdateSupervisor(
    p_SupervisorId BIGINT,
    p_FirstName VARCHAR DEFAULT NULL,
    p_LastName VARCHAR DEFAULT NULL,
    p_Email VARCHAR DEFAULT NULL,
    p_Phone VARCHAR DEFAULT NULL,
    p_Address VARCHAR DEFAULT NULL,
    p_ZoneId BIGINT DEFAULT NULL,
    p_EmergencyContactName VARCHAR DEFAULT NULL,
    p_EmergencyContactPhone VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_FullName VARCHAR;
BEGIN

    -- Build full name
    IF p_FirstName IS NOT NULL AND p_LastName IS NOT NULL THEN
        v_FullName := p_FirstName || ' ' || p_LastName;
    END IF;

    -- Update
    UPDATE t_sys_supervisor
    SET c_full_name = COALESCE(v_FullName, c_full_name),
        c_email = COALESCE(p_Email, c_email),
        c_phone = COALESCE(p_Phone, c_phone),
        c_address_line1 = COALESCE(p_Address, c_address_line1),
        c_modifieddate = NOW()
    WHERE c_supervisor_id = p_SupervisorId
      AND c_is_deleted = FALSE;

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_DeleteSupervisor;

CREATE OR REPLACE FUNCTION sp_DeleteSupervisor(
    p_SupervisorId BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor
    SET c_is_deleted = TRUE,
        c_modifieddate = NOW()
    WHERE c_supervisor_id = p_SupervisorId;

    RETURN QUERY SELECT 1;

END;
$$;

-- =============================================
-- AUTHORITY MANAGEMENT
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateAuthorityLevel;

CREATE OR REPLACE FUNCTION sp_UpdateAuthorityLevel(
    p_SupervisorId BIGINT,
    p_NewAuthorityLevel VARCHAR,
    p_UpdatedBy BIGINT,
    p_Reason VARCHAR
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OldLevel VARCHAR;
BEGIN

    -- Get old level
    SELECT c_authority_level INTO v_OldLevel
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId;

    -- Update authority + permissions
    UPDATE t_sys_supervisor
    SET c_authority_level = p_NewAuthorityLevel,
        c_can_release_payment = CASE WHEN p_NewAuthorityLevel = 'FULL' THEN 1 ELSE 0 END,
        c_can_approve_refund = CASE WHEN p_NewAuthorityLevel IN ('ADVANCED','FULL') THEN 1 ELSE 0 END,
        c_can_mentor_others = CASE WHEN p_NewAuthorityLevel IN ('ADVANCED','FULL') THEN 1 ELSE 0 END,
        c_modifieddate = NOW(),
        c_modifiedby = p_UpdatedBy
    WHERE c_supervisor_id = p_SupervisorId;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Authority level changed from ' || v_OldLevel || ' to ' || p_NewAuthorityLevel || '. Reason: ' || p_Reason,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_GrantPermission;

CREATE OR REPLACE FUNCTION sp_GrantPermission(
    p_SupervisorId BIGINT,
    p_PermissionType VARCHAR,
    p_GrantedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Conditional updates
    IF p_PermissionType = 'RELEASE_PAYMENT' THEN
        UPDATE t_sys_supervisor
        SET c_can_release_payment = 1,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;

    ELSIF p_PermissionType = 'APPROVE_REFUND' THEN
        UPDATE t_sys_supervisor
        SET c_can_approve_refund = 1,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;

    ELSIF p_PermissionType = 'MENTOR_OTHERS' THEN
        UPDATE t_sys_supervisor
        SET c_can_mentor_others = 1,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;
    END IF;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Permission granted: ' || p_PermissionType,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_RevokePermission;

CREATE OR REPLACE FUNCTION sp_RevokePermission(
    p_SupervisorId BIGINT,
    p_PermissionType VARCHAR,
    p_RevokedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Conditional updates
    IF p_PermissionType = 'RELEASE_PAYMENT' THEN
        UPDATE t_sys_supervisor
        SET c_can_release_payment = 0,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;

    ELSIF p_PermissionType = 'APPROVE_REFUND' THEN
        UPDATE t_sys_supervisor
        SET c_can_approve_refund = 0,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;

    ELSIF p_PermissionType = 'MENTOR_OTHERS' THEN
        UPDATE t_sys_supervisor
        SET c_can_mentor_others = 0,
            c_modifieddate = NOW()
        WHERE c_supervisor_id = p_SupervisorId;
    END IF;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Permission revoked: ' || p_PermissionType,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

-- =============================================
-- STATUS MANAGEMENT
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateSupervisorStatus;

CREATE OR REPLACE FUNCTION sp_UpdateSupervisorStatus(
    p_SupervisorId BIGINT,
    p_NewStatus VARCHAR,
    p_UpdatedBy BIGINT,
    p_Notes VARCHAR DEFAULT NULL
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OldStatus VARCHAR;
BEGIN

    -- Get old status
    SELECT c_current_status INTO v_OldStatus
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId;

    -- Update
    UPDATE t_sys_supervisor
    SET c_current_status = p_NewStatus,
        c_status_reason = p_Notes,
        c_modifieddate = NOW(),
        c_modifiedby = p_UpdatedBy
    WHERE c_supervisor_id = p_SupervisorId;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Status changed from ' || v_OldStatus || ' to ' || p_NewStatus,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_ActivateSupervisor;

CREATE OR REPLACE FUNCTION sp_ActivateSupervisor(
    p_SupervisorId BIGINT,
    p_ActivatedBy BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Update
    UPDATE t_sys_supervisor
    SET c_current_status = 'ACTIVE',
        c_is_available = 1,
        c_modifieddate = NOW(),
        c_modifiedby = p_ActivatedBy
    WHERE c_supervisor_id = p_SupervisorId;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Supervisor activated',
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_SuspendSupervisor;

CREATE OR REPLACE FUNCTION sp_SuspendSupervisor(
    p_SupervisorId BIGINT,
    p_SuspendedBy BIGINT,
    p_Reason VARCHAR
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Update
    UPDATE t_sys_supervisor
    SET c_current_status = 'SUSPENDED',
        c_is_available = 0,
        c_suspended_by = p_SuspendedBy,
        c_suspension_date = NOW(),
        c_suspension_reason = p_Reason,
        c_status_reason = p_Reason,
        c_modifieddate = NOW(),
        c_modifiedby = p_SuspendedBy
    WHERE c_supervisor_id = p_SupervisorId;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Supervisor suspended. Reason: ' || p_Reason,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_TerminateSupervisor;

CREATE OR REPLACE FUNCTION sp_TerminateSupervisor(
    p_SupervisorId BIGINT,
    p_TerminatedBy BIGINT,
    p_Reason VARCHAR
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    -- Update
    UPDATE t_sys_supervisor
    SET c_current_status = 'DEACTIVATED',
        c_is_available = 0,
        c_status_reason = p_Reason,
        c_modifieddate = NOW(),
        c_modifiedby = p_TerminatedBy
    WHERE c_supervisor_id = p_SupervisorId;

    -- Log
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        p_SupervisorId,
        'STATUS_CHANGED',
        'Supervisor terminated. Reason: ' || p_Reason,
        'SUCCESS'
    );

    RETURN QUERY SELECT 1;

END;
$$;

-- =============================================
-- DASHBOARD & ANALYTICS
-- =============================================

DROP FUNCTION IF EXISTS sp_GetSupervisorDashboard;

CREATE OR REPLACE FUNCTION sp_GetSupervisorDashboard(
    p_SupervisorId BIGINT
)
RETURNS TABLE (
    SupervisorId BIGINT,
    FullName VARCHAR,
    Email VARCHAR,
    Phone VARCHAR,
    SupervisorType VARCHAR,
    CurrentStatus VARCHAR,
    AuthorityLevel VARCHAR,
    TotalEventsSupervised INT,
    AverageRating DECIMAL,
    PhotoUrl VARCHAR,
    IsAvailable BOOLEAN,
    UpcomingAssignments INT,
    ActiveAssignments INT,
    CompletedThisMonth INT,
    PendingPayments DECIMAL,
    TotalEarned DECIMAL
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        s.c_supervisor_id,
        s.c_full_name,
        s.c_email,
        s.c_phone,
        s.c_supervisor_type,
        s.c_current_status,
        s.c_authority_level,
        s.c_total_events_supervised,
        s.c_average_rating,
        s.c_photo_url,
        s.c_is_available,

        -- Upcoming
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = p_SupervisorId
           AND c_status IN ('ASSIGNED','ACCEPTED')
           AND c_event_date >= CURRENT_DATE),

        -- Active today
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = p_SupervisorId
           AND c_status IN ('CHECKED_IN','IN_PROGRESS')
           AND c_event_date = CURRENT_DATE),

        -- Completed this month
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = p_SupervisorId
           AND c_status = 'COMPLETED'
           AND c_event_date >= date_trunc('month', CURRENT_DATE)),

        -- Pending payments
        (SELECT COALESCE(SUM(c_supervisor_payout_amount),0)
         FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = p_SupervisorId
           AND c_supervisor_payout_status = 'PENDING'),

        -- Total earned
        (SELECT COALESCE(SUM(c_supervisor_payout_amount),0)
         FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = p_SupervisorId
           AND c_supervisor_payout_status = 'PAID')

    FROM t_sys_supervisor s
    WHERE s.c_supervisor_id = p_SupervisorId;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorPerformanceReport;

CREATE OR REPLACE FUNCTION sp_GetSupervisorPerformanceReport(
    p_FromDate TIMESTAMP,
    p_ToDate TIMESTAMP
)
RETURNS TABLE (
    SupervisorId BIGINT,
    FullName VARCHAR,
    SupervisorType VARCHAR,
    AuthorityLevel VARCHAR,
    TotalAssignments BIGINT,
    CompletedAssignments BIGINT,
    NoShows BIGINT,
    AvgQualityRating DECIMAL,
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
        s.c_authority_level,
        COUNT(a.c_assignment_id),
        SUM(CASE WHEN a.c_status = 'COMPLETED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN a.c_status = 'NO_SHOW' THEN 1 ELSE 0 END),
        AVG(a.c_quality_rating::DECIMAL(3,2)),
        COALESCE(SUM(a.c_supervisor_payout_amount),0)
    FROM t_sys_supervisor s
    LEFT JOIN t_sys_supervisor_assignment a 
        ON s.c_supervisor_id = a.c_supervisor_id
        AND a.c_event_date BETWEEN p_FromDate AND p_ToDate
    WHERE s.c_is_deleted = FALSE
    GROUP BY s.c_supervisor_id, s.c_full_name, s.c_supervisor_type, s.c_authority_level
    ORDER BY SUM(CASE WHEN a.c_status = 'COMPLETED' THEN 1 ELSE 0 END) DESC;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorStatistics;

CREATE OR REPLACE FUNCTION sp_GetSupervisorStatistics(
    p_SupervisorType VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    TotalSupervisors BIGINT,
    Active BIGINT,
    Suspended BIGINT,
    Applied BIGINT,
    InTraining BIGINT,
    Certified BIGINT,
    OnProbation BIGINT,
    CareerSupervisors BIGINT,
    RegisteredSupervisors BIGINT,
    BasicAuthority BIGINT,
    IntermediateAuthority BIGINT,
    AdvancedAuthority BIGINT,
    FullAuthority BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        COUNT(*),
        SUM(CASE WHEN c_current_status = 'ACTIVE' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_current_status = 'SUSPENDED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_current_status = 'APPLIED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_current_status = 'TRAINING' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_current_status = 'CERTIFIED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_current_status = 'PROBATION' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_supervisor_type = 'CAREER' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_supervisor_type = 'REGISTERED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_authority_level = 'BASIC' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_authority_level = 'INTERMEDIATE' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_authority_level = 'ADVANCED' THEN 1 ELSE 0 END),
        SUM(CASE WHEN c_authority_level = 'FULL' THEN 1 ELSE 0 END)
    FROM t_sys_supervisor
    WHERE c_is_deleted = FALSE
      AND (p_SupervisorType IS NULL OR c_supervisor_type = p_SupervisorType);

END;
$$;

-- =============================================
-- AVAILABILITY & SCHEDULING
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateSupervisorAvailability;

CREATE OR REPLACE FUNCTION sp_UpdateSupervisorAvailability(
    p_SupervisorId BIGINT,
    p_AvailabilityData TEXT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor
    SET c_availability_calendar = p_AvailabilityData,
        c_modifieddate = NOW()
    WHERE c_supervisor_id = p_SupervisorId;

    RETURN QUERY SELECT 1;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorAvailability;

CREATE OR REPLACE FUNCTION sp_GetSupervisorAvailability(
    p_SupervisorId BIGINT,
    p_Date DATE
)
RETURNS TABLE (
    SupervisorId BIGINT,
    AvailabilityData TEXT,
    IsAvailable BOOLEAN,
    MaxEventsPerMonth INT,
    PreferredEventTypes TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_supervisor_id,
        c_availability_calendar,
        c_is_available,
        c_max_events_per_month,
        c_preferred_event_types
    FROM t_sys_supervisor
    WHERE c_supervisor_id = p_SupervisorId;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetAvailableSupervisors;

CREATE OR REPLACE FUNCTION sp_GetAvailableSupervisors(
    p_EventDate DATE,
    p_EventType TEXT,
    p_ZoneId BIGINT DEFAULT NULL
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT s.*
    FROM t_sys_supervisor s
    WHERE s.c_is_deleted = FALSE
      AND s.c_current_status = 'ACTIVE'
      AND s.c_is_available = 1
      AND (p_ZoneId IS NULL OR s.c_locality IS NOT NULL)
      -- Exclude assigned supervisors
      AND s.c_supervisor_id NOT IN (
          SELECT a.c_supervisor_id
          FROM t_sys_supervisor_assignment a
          WHERE a.c_event_date = p_EventDate
            AND a.c_status NOT IN ('CANCELLED', 'REJECTED')
      )
    ORDER BY s.c_average_rating DESC,
             s.c_total_events_supervised DESC;

END;
$$;

-- =============================================
-- SEARCH & FILTERING
-- =============================================

DROP FUNCTION IF EXISTS sp_SearchSupervisors;

CREATE OR REPLACE FUNCTION sp_SearchSupervisors(
    p_Name VARCHAR DEFAULT NULL,
    p_Email VARCHAR DEFAULT NULL,
    p_Phone VARCHAR DEFAULT NULL,
    p_SupervisorType VARCHAR DEFAULT NULL,
    p_AuthorityLevel VARCHAR DEFAULT NULL,
    p_Status VARCHAR DEFAULT NULL,
    p_ZoneId BIGINT DEFAULT NULL,
    p_IsActive BOOLEAN DEFAULT NULL
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_is_deleted = FALSE
      AND (p_Name IS NULL OR c_full_name ILIKE '%' || p_Name || '%')
      AND (p_Email IS NULL OR c_email ILIKE '%' || p_Email || '%')
      AND (p_Phone IS NULL OR c_phone ILIKE '%' || p_Phone || '%')
      AND (p_SupervisorType IS NULL OR c_supervisor_type = p_SupervisorType)
      AND (p_AuthorityLevel IS NULL OR c_authority_level = p_AuthorityLevel)
      AND (p_Status IS NULL OR c_current_status = p_Status)
      AND (p_IsActive IS NULL OR (c_current_status = 'ACTIVE') = p_IsActive)
    ORDER BY c_createddate DESC;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorsByZone;

CREATE OR REPLACE FUNCTION sp_GetSupervisorsByZone(
    p_ZoneId BIGINT
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_is_deleted = FALSE
      AND c_current_status = 'ACTIVE'
    ORDER BY c_average_rating DESC;

END;
$$;

DROP FUNCTION IF EXISTS sp_GetSupervisorsByAuthority;

CREATE OR REPLACE FUNCTION sp_GetSupervisorsByAuthority(
    p_AuthorityLevel VARCHAR
)
RETURNS SETOF t_sys_supervisor
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT *
    FROM t_sys_supervisor
    WHERE c_is_deleted = FALSE
      AND c_authority_level = p_AuthorityLevel
      AND c_current_status = 'ACTIVE'
    ORDER BY c_total_events_supervised DESC;

END;
$$;

