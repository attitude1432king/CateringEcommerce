-- =============================================
-- Supervisor Core Stored Procedures
-- Maps to: SupervisorRepository.cs
-- 24 Stored Procedures for Supervisor CRUD, Authority, Status, Dashboard, Availability, Search
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- BASIC CRUD
-- =============================================

CREATE OR ALTER PROCEDURE sp_CheckEmailExists
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM t_sys_supervisor
            WHERE c_email = @Email
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS [Exists];
END
GO

CREATE OR ALTER PROCEDURE sp_CheckPhoneExists
    @Phone VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM t_sys_supervisor
            WHERE c_phone = @Phone
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS [Exists];
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorById
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_supervisor_id = @SupervisorId AND c_is_deleted = 0;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorByEmail
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_email = @Email AND c_is_deleted = 0;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorByPhone
    @Phone VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_phone = @Phone AND c_is_deleted = 0;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllSupervisors
    @SupervisorType VARCHAR(20) = NULL,
    @Status VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_is_deleted = 0
      AND (@SupervisorType IS NULL OR c_supervisor_type = @SupervisorType)
      AND (@Status IS NULL OR c_current_status = @Status)
    ORDER BY c_createddate DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateSupervisor
    @SupervisorId BIGINT,
    @FirstName NVARCHAR(50) = NULL,
    @LastName NVARCHAR(50) = NULL,
    @Email VARCHAR(100) = NULL,
    @Phone VARCHAR(15) = NULL,
    @Address NVARCHAR(500) = NULL,
    @ZoneId BIGINT = NULL,
    @EmergencyContactName NVARCHAR(100) = NULL,
    @EmergencyContactPhone VARCHAR(15) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FullName NVARCHAR(100);
    IF @FirstName IS NOT NULL AND @LastName IS NOT NULL
        SET @FullName = @FirstName + ' ' + @LastName;

    UPDATE t_sys_supervisor
    SET c_full_name = ISNULL(@FullName, c_full_name),
        c_email = ISNULL(@Email, c_email),
        c_phone = ISNULL(@Phone, c_phone),
        c_address_line1 = ISNULL(@Address, c_address_line1),
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId AND c_is_deleted = 0;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteSupervisor
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_is_deleted = 1,
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId;

    SELECT 1 AS Success;
END
GO

-- =============================================
-- AUTHORITY MANAGEMENT
-- =============================================

CREATE OR ALTER PROCEDURE sp_UpdateAuthorityLevel
    @SupervisorId BIGINT,
    @NewAuthorityLevel VARCHAR(20),
    @UpdatedBy BIGINT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldLevel VARCHAR(20);
    SELECT @OldLevel = c_authority_level FROM t_sys_supervisor WHERE c_supervisor_id = @SupervisorId;

    UPDATE t_sys_supervisor
    SET c_authority_level = @NewAuthorityLevel,
        c_can_release_payment = CASE WHEN @NewAuthorityLevel = 'FULL' THEN 1 ELSE 0 END,
        c_can_approve_refund = CASE WHEN @NewAuthorityLevel IN ('ADVANCED', 'FULL') THEN 1 ELSE 0 END,
        c_can_mentor_others = CASE WHEN @NewAuthorityLevel IN ('ADVANCED', 'FULL') THEN 1 ELSE 0 END,
        c_modifieddate = GETDATE(),
        c_modifiedby = @UpdatedBy
    WHERE c_supervisor_id = @SupervisorId;

    -- Log the authority change
    INSERT INTO t_sys_supervisor_action_log (
        c_supervisor_id, c_action_type, c_action_description, c_action_result
    )
    VALUES (
        @SupervisorId, 'STATUS_CHANGED',
        'Authority level changed from ' + @OldLevel + ' to ' + @NewAuthorityLevel + '. Reason: ' + @Reason,
        'SUCCESS'
    );

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GrantPermission
    @SupervisorId BIGINT,
    @PermissionType VARCHAR(50),
    @GrantedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Update specific permission flags based on type
    IF @PermissionType = 'RELEASE_PAYMENT'
        UPDATE t_sys_supervisor SET c_can_release_payment = 1, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;
    ELSE IF @PermissionType = 'APPROVE_REFUND'
        UPDATE t_sys_supervisor SET c_can_approve_refund = 1, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;
    ELSE IF @PermissionType = 'MENTOR_OTHERS'
        UPDATE t_sys_supervisor SET c_can_mentor_others = 1, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Permission granted: ' + @PermissionType, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_RevokePermission
    @SupervisorId BIGINT,
    @PermissionType VARCHAR(50),
    @RevokedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PermissionType = 'RELEASE_PAYMENT'
        UPDATE t_sys_supervisor SET c_can_release_payment = 0, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;
    ELSE IF @PermissionType = 'APPROVE_REFUND'
        UPDATE t_sys_supervisor SET c_can_approve_refund = 0, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;
    ELSE IF @PermissionType = 'MENTOR_OTHERS'
        UPDATE t_sys_supervisor SET c_can_mentor_others = 0, c_modifieddate = GETDATE() WHERE c_supervisor_id = @SupervisorId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Permission revoked: ' + @PermissionType, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

-- =============================================
-- STATUS MANAGEMENT
-- =============================================

CREATE OR ALTER PROCEDURE sp_UpdateSupervisorStatus
    @SupervisorId BIGINT,
    @NewStatus VARCHAR(30),
    @UpdatedBy BIGINT,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus VARCHAR(30);
    SELECT @OldStatus = c_current_status FROM t_sys_supervisor WHERE c_supervisor_id = @SupervisorId;

    UPDATE t_sys_supervisor
    SET c_current_status = @NewStatus,
        c_status_reason = @Notes,
        c_modifieddate = GETDATE(),
        c_modifiedby = @UpdatedBy
    WHERE c_supervisor_id = @SupervisorId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Status changed from ' + @OldStatus + ' to ' + @NewStatus, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_ActivateSupervisor
    @SupervisorId BIGINT,
    @ActivatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_current_status = 'ACTIVE',
        c_is_available = 1,
        c_modifieddate = GETDATE(),
        c_modifiedby = @ActivatedBy
    WHERE c_supervisor_id = @SupervisorId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Supervisor activated', 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_SuspendSupervisor
    @SupervisorId BIGINT,
    @SuspendedBy BIGINT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_current_status = 'SUSPENDED',
        c_is_available = 0,
        c_suspended_by = @SuspendedBy,
        c_suspension_date = GETDATE(),
        c_suspension_reason = @Reason,
        c_status_reason = @Reason,
        c_modifieddate = GETDATE(),
        c_modifiedby = @SuspendedBy
    WHERE c_supervisor_id = @SupervisorId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Supervisor suspended. Reason: ' + @Reason, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_TerminateSupervisor
    @SupervisorId BIGINT,
    @TerminatedBy BIGINT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_current_status = 'DEACTIVATED',
        c_is_available = 0,
        c_status_reason = @Reason,
        c_modifieddate = GETDATE(),
        c_modifiedby = @TerminatedBy
    WHERE c_supervisor_id = @SupervisorId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, 'STATUS_CHANGED', 'Supervisor terminated. Reason: ' + @Reason, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

-- =============================================
-- DASHBOARD & ANALYTICS
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetSupervisorDashboard
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Supervisor profile
    SELECT
        s.c_supervisor_id AS SupervisorId,
        s.c_full_name AS FullName,
        s.c_email AS Email,
        s.c_phone AS Phone,
        s.c_supervisor_type AS SupervisorType,
        s.c_current_status AS CurrentStatus,
        s.c_authority_level AS AuthorityLevel,
        s.c_total_events_supervised AS TotalEventsSupervised,
        s.c_average_rating AS AverageRating,
        s.c_photo_url AS PhotoUrl,
        s.c_is_available AS IsAvailable,
        -- Upcoming assignments
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = @SupervisorId AND c_status IN ('ASSIGNED', 'ACCEPTED')
           AND c_event_date >= CAST(GETDATE() AS DATE)) AS UpcomingAssignments,
        -- Active assignments (today)
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = @SupervisorId AND c_status IN ('CHECKED_IN', 'IN_PROGRESS')
           AND c_event_date = CAST(GETDATE() AS DATE)) AS ActiveAssignments,
        -- Completed this month
        (SELECT COUNT(*) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = @SupervisorId AND c_status = 'COMPLETED'
           AND c_event_date >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)) AS CompletedThisMonth,
        -- Pending payments
        (SELECT ISNULL(SUM(c_supervisor_payout_amount), 0) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = @SupervisorId AND c_supervisor_payout_status = 'PENDING') AS PendingPayments,
        -- Total earned
        (SELECT ISNULL(SUM(c_supervisor_payout_amount), 0) FROM t_sys_supervisor_assignment
         WHERE c_supervisor_id = @SupervisorId AND c_supervisor_payout_status = 'PAID') AS TotalEarned
    FROM t_sys_supervisor s
    WHERE s.c_supervisor_id = @SupervisorId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorPerformanceReport
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.c_supervisor_id AS SupervisorId,
        s.c_full_name AS FullName,
        s.c_supervisor_type AS SupervisorType,
        s.c_authority_level AS AuthorityLevel,
        COUNT(a.c_assignment_id) AS TotalAssignments,
        SUM(CASE WHEN a.c_status = 'COMPLETED' THEN 1 ELSE 0 END) AS CompletedAssignments,
        SUM(CASE WHEN a.c_status = 'NO_SHOW' THEN 1 ELSE 0 END) AS NoShows,
        AVG(CAST(a.c_quality_rating AS DECIMAL(3,2))) AS AvgQualityRating,
        ISNULL(SUM(a.c_supervisor_payout_amount), 0) AS TotalPayout
    FROM t_sys_supervisor s
    LEFT JOIN t_sys_supervisor_assignment a ON s.c_supervisor_id = a.c_supervisor_id
        AND a.c_event_date BETWEEN @FromDate AND @ToDate
    WHERE s.c_is_deleted = 0
    GROUP BY s.c_supervisor_id, s.c_full_name, s.c_supervisor_type, s.c_authority_level
    ORDER BY CompletedAssignments DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorStatistics
    @SupervisorType VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalSupervisors,
        SUM(CASE WHEN c_current_status = 'ACTIVE' THEN 1 ELSE 0 END) AS Active,
        SUM(CASE WHEN c_current_status = 'SUSPENDED' THEN 1 ELSE 0 END) AS Suspended,
        SUM(CASE WHEN c_current_status = 'APPLIED' THEN 1 ELSE 0 END) AS Applied,
        SUM(CASE WHEN c_current_status = 'TRAINING' THEN 1 ELSE 0 END) AS InTraining,
        SUM(CASE WHEN c_current_status = 'CERTIFIED' THEN 1 ELSE 0 END) AS Certified,
        SUM(CASE WHEN c_current_status = 'PROBATION' THEN 1 ELSE 0 END) AS OnProbation,
        SUM(CASE WHEN c_supervisor_type = 'CAREER' THEN 1 ELSE 0 END) AS CareerSupervisors,
        SUM(CASE WHEN c_supervisor_type = 'REGISTERED' THEN 1 ELSE 0 END) AS RegisteredSupervisors,
        SUM(CASE WHEN c_authority_level = 'BASIC' THEN 1 ELSE 0 END) AS BasicAuthority,
        SUM(CASE WHEN c_authority_level = 'INTERMEDIATE' THEN 1 ELSE 0 END) AS IntermediateAuthority,
        SUM(CASE WHEN c_authority_level = 'ADVANCED' THEN 1 ELSE 0 END) AS AdvancedAuthority,
        SUM(CASE WHEN c_authority_level = 'FULL' THEN 1 ELSE 0 END) AS FullAuthority
    FROM t_sys_supervisor
    WHERE c_is_deleted = 0
      AND (@SupervisorType IS NULL OR c_supervisor_type = @SupervisorType);
END
GO

-- =============================================
-- AVAILABILITY & SCHEDULING
-- =============================================

CREATE OR ALTER PROCEDURE sp_UpdateSupervisorAvailability
    @SupervisorId BIGINT,
    @AvailabilityData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_availability_calendar = @AvailabilityData,
        c_modifieddate = GETDATE()
    WHERE c_supervisor_id = @SupervisorId;

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorAvailability
    @SupervisorId BIGINT,
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c_supervisor_id AS SupervisorId,
           c_availability_calendar AS AvailabilityData,
           c_is_available AS IsAvailable,
           c_max_events_per_month AS MaxEventsPerMonth,
           c_preferred_event_types AS PreferredEventTypes
    FROM t_sys_supervisor
    WHERE c_supervisor_id = @SupervisorId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAvailableSupervisors
    @EventDate DATE,
    @EventType NVARCHAR(100),
    @ZoneId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.*
    FROM t_sys_supervisor s
    WHERE s.c_is_deleted = 0
      AND s.c_current_status = 'ACTIVE'
      AND s.c_is_available = 1
      AND (@ZoneId IS NULL OR s.c_locality IS NOT NULL)
      -- Exclude supervisors already assigned on this date
      AND s.c_supervisor_id NOT IN (
          SELECT a.c_supervisor_id
          FROM t_sys_supervisor_assignment a
          WHERE a.c_event_date = @EventDate
            AND a.c_status NOT IN ('CANCELLED', 'REJECTED')
      )
    ORDER BY s.c_average_rating DESC, s.c_total_events_supervised DESC;
END
GO

-- =============================================
-- SEARCH & FILTERING
-- =============================================

CREATE OR ALTER PROCEDURE sp_SearchSupervisors
    @Name NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Phone VARCHAR(15) = NULL,
    @SupervisorType VARCHAR(20) = NULL,
    @AuthorityLevel VARCHAR(20) = NULL,
    @Status VARCHAR(30) = NULL,
    @ZoneId BIGINT = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_is_deleted = 0
      AND (@Name IS NULL OR c_full_name LIKE '%' + @Name + '%')
      AND (@Email IS NULL OR c_email LIKE '%' + @Email + '%')
      AND (@Phone IS NULL OR c_phone LIKE '%' + @Phone + '%')
      AND (@SupervisorType IS NULL OR c_supervisor_type = @SupervisorType)
      AND (@AuthorityLevel IS NULL OR c_authority_level = @AuthorityLevel)
      AND (@Status IS NULL OR c_current_status = @Status)
      AND (@IsActive IS NULL OR (c_current_status = 'ACTIVE') = @IsActive)
    ORDER BY c_createddate DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorsByZone
    @ZoneId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_is_deleted = 0
      AND c_current_status = 'ACTIVE'
    ORDER BY c_average_rating DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorsByAuthority
    @AuthorityLevel VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM t_sys_supervisor
    WHERE c_is_deleted = 0
      AND c_authority_level = @AuthorityLevel
      AND c_current_status = 'ACTIVE'
    ORDER BY c_total_events_supervised DESC;
END
GO

PRINT '================================================';
PRINT 'Supervisor Core Stored Procedures Created';
PRINT '24 Stored Procedures for SupervisorRepository.cs';
PRINT '================================================';
GO
