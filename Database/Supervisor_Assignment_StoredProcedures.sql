-- =============================================
-- Supervisor Assignment Stored Procedures
-- Maps to: SupervisorAssignmentRepository.cs
-- 16 Stored Procedures for Assignment CRUD, Actions, Analytics, Search
-- (Excludes 4 already in Supervisor_Management_StoredProcedures.sql:
--  sp_FindEligibleSupervisors, sp_AssignSupervisorToEvent,
--  sp_SupervisorCheckIn, sp_RequestPaymentRelease)
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- ASSIGNMENT CREATION
-- =============================================

CREATE OR ALTER PROCEDURE sp_BulkAssignSupervisor
    @OrderId BIGINT,
    @SupervisorId BIGINT,
    @AssignedBy BIGINT,
    @AssignmentId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AssignmentNumber VARCHAR(50) = 'ASG-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(@OrderId AS VARCHAR(10)) + '-' + CAST(@SupervisorId AS VARCHAR(10));

    -- Get event details from order
    DECLARE @EventDate DATE, @EventLocation NVARCHAR(200), @EventValue DECIMAL(18,2), @GuestCount INT;

    SELECT @EventDate = c_event_date, @EventLocation = c_delivery_address,
           @EventValue = c_total_amount, @GuestCount = c_guest_count
    FROM t_sys_order
    WHERE c_orderid = @OrderId;

    INSERT INTO t_sys_supervisor_assignment (
        c_order_id, c_supervisor_id, c_assignment_number, c_assigned_date,
        c_assigned_by, c_assignment_source, c_event_date, c_event_location,
        c_estimated_guests, c_event_value, c_status
    )
    VALUES (
        @OrderId, @SupervisorId, @AssignmentNumber, GETDATE(),
        @AssignedBy, 'MANUAL', ISNULL(@EventDate, GETDATE()), @EventLocation,
        @GuestCount, @EventValue, 'ASSIGNED'
    );

    SET @AssignmentId = SCOPE_IDENTITY();
END
GO

-- =============================================
-- ASSIGNMENT RETRIEVAL
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetAssignmentById
    @AssignmentId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_email AS SupervisorEmail,
           s.c_phone AS SupervisorPhone, s.c_supervisor_type AS SupervisorType,
           s.c_authority_level AS AuthorityLevel, s.c_photo_url AS SupervisorPhoto
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_assignment_id = @AssignmentId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAssignmentsBySupervisor
    @SupervisorId BIGINT,
    @Status VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_supervisor_id = @SupervisorId
      AND (@Status IS NULL OR a.c_status = @Status)
    ORDER BY a.c_event_date DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAssignmentsByOrder
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_email AS SupervisorEmail,
           s.c_phone AS SupervisorPhone, s.c_supervisor_type AS SupervisorType
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_order_id = @OrderId
    ORDER BY a.c_assigned_date DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllAssignments
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_supervisor_type AS SupervisorType
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (@FromDate IS NULL OR a.c_event_date >= @FromDate)
      AND (@ToDate IS NULL OR a.c_event_date <= @ToDate)
    ORDER BY a.c_event_date DESC;
END
GO

-- =============================================
-- SUPERVISOR ACTIONS
-- =============================================

CREATE OR ALTER PROCEDURE sp_AcceptAssignment
    @AssignmentId BIGINT,
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_assignment
    SET c_status = 'ACCEPTED',
        c_accepted_date = GETDATE(),
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId
      AND c_supervisor_id = @SupervisorId
      AND c_status = 'ASSIGNED';

    -- Log action
    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'ASSIGNMENT_ACCEPTED', 'Assignment accepted by supervisor', 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_RejectAssignment
    @AssignmentId BIGINT,
    @SupervisorId BIGINT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_assignment
    SET c_status = 'REJECTED',
        c_rejection_reason = @Reason,
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId
      AND c_supervisor_id = @SupervisorId
      AND c_status = 'ASSIGNED';

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'ASSIGNMENT_REJECTED', 'Assignment rejected. Reason: ' + @Reason, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_ApprovePaymentRelease
    @AssignmentId BIGINT,
    @ApprovedBy BIGINT,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_assignment
    SET c_payment_release_approved = 1,
        c_payment_approved_by = @ApprovedBy,
        c_payment_approval_date = GETDATE(),
        c_supervisor_payout_status = 'PROCESSED',
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId;

    -- Log action
    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_supervisor_assignment WHERE c_assignment_id = @AssignmentId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'PAYMENT_RELEASE_REQUEST', 'Payment release approved by admin', 'SUCCESS');

    SELECT 1 AS Success;
END
GO

-- =============================================
-- ASSIGNMENT STATUS MANAGEMENT
-- =============================================

CREATE OR ALTER PROCEDURE sp_UpdateAssignmentStatus
    @AssignmentId BIGINT,
    @NewStatus VARCHAR(30),
    @UpdatedBy BIGINT,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus VARCHAR(30);
    SELECT @OldStatus = c_status FROM t_sys_supervisor_assignment WHERE c_assignment_id = @AssignmentId;

    UPDATE t_sys_supervisor_assignment
    SET c_status = @NewStatus,
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_supervisor_assignment WHERE c_assignment_id = @AssignmentId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'STATUS_CHANGED', 'Assignment status: ' + @OldStatus + ' -> ' + @NewStatus, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_CancelAssignment
    @AssignmentId BIGINT,
    @CancelledBy BIGINT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_assignment
    SET c_status = 'CANCELLED',
        c_rejection_reason = @Reason,
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_supervisor_assignment WHERE c_assignment_id = @AssignmentId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'STATUS_CHANGED', 'Assignment cancelled. Reason: ' + @Reason, 'SUCCESS');

    SELECT 1 AS Success;
END
GO

CREATE OR ALTER PROCEDURE sp_CompleteAssignment
    @AssignmentId BIGINT,
    @CompletedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor_assignment
    SET c_status = 'COMPLETED',
        c_check_out_time = GETDATE(),
        c_modifieddate = GETDATE()
    WHERE c_assignment_id = @AssignmentId;

    -- Increment supervisor's completed event count
    UPDATE s
    SET s.c_total_events_supervised = s.c_total_events_supervised + 1,
        s.c_modifieddate = GETDATE()
    FROM t_sys_supervisor s
    INNER JOIN t_sys_supervisor_assignment a ON s.c_supervisor_id = a.c_supervisor_id
    WHERE a.c_assignment_id = @AssignmentId;

    DECLARE @SupervisorId BIGINT;
    SELECT @SupervisorId = c_supervisor_id FROM t_sys_supervisor_assignment WHERE c_assignment_id = @AssignmentId;

    INSERT INTO t_sys_supervisor_action_log (c_supervisor_id, c_assignment_id, c_action_type, c_action_description, c_action_result)
    VALUES (@SupervisorId, @AssignmentId, 'CHECK_OUT', 'Assignment completed', 'SUCCESS');

    SELECT 1 AS Success;
END
GO

-- =============================================
-- ANALYTICS & REPORTING
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetUpcomingAssignments
    @DaysAhead INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_phone AS SupervisorPhone,
           s.c_supervisor_type AS SupervisorType
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_event_date BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE))
      AND a.c_status IN ('ASSIGNED', 'ACCEPTED')
    ORDER BY a.c_event_date ASC, a.c_event_time ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetOverdueAssignments
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_phone AS SupervisorPhone
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE a.c_event_date < CAST(GETDATE() AS DATE)
      AND a.c_status IN ('ASSIGNED', 'ACCEPTED')
    ORDER BY a.c_event_date ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAssignmentStatistics
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalAssignments,
        SUM(CASE WHEN c_status = 'ASSIGNED' THEN 1 ELSE 0 END) AS Assigned,
        SUM(CASE WHEN c_status = 'ACCEPTED' THEN 1 ELSE 0 END) AS Accepted,
        SUM(CASE WHEN c_status = 'REJECTED' THEN 1 ELSE 0 END) AS Rejected,
        SUM(CASE WHEN c_status = 'CHECKED_IN' THEN 1 ELSE 0 END) AS CheckedIn,
        SUM(CASE WHEN c_status = 'IN_PROGRESS' THEN 1 ELSE 0 END) AS InProgress,
        SUM(CASE WHEN c_status = 'COMPLETED' THEN 1 ELSE 0 END) AS Completed,
        SUM(CASE WHEN c_status = 'CANCELLED' THEN 1 ELSE 0 END) AS Cancelled,
        SUM(CASE WHEN c_status = 'NO_SHOW' THEN 1 ELSE 0 END) AS NoShows,
        ISNULL(SUM(c_supervisor_payout_amount), 0) AS TotalPayoutAmount,
        ISNULL(AVG(CAST(c_quality_rating AS DECIMAL(3,2))), 0) AS AvgQualityRating
    FROM t_sys_supervisor_assignment
    WHERE c_event_date BETWEEN @FromDate AND @ToDate;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSupervisorWorkload
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.c_supervisor_id AS SupervisorId,
        s.c_full_name AS FullName,
        s.c_supervisor_type AS SupervisorType,
        COUNT(a.c_assignment_id) AS TotalAssignments,
        SUM(CASE WHEN a.c_status = 'COMPLETED' THEN 1 ELSE 0 END) AS Completed,
        SUM(CASE WHEN a.c_status IN ('ASSIGNED', 'ACCEPTED') THEN 1 ELSE 0 END) AS Upcoming,
        AVG(CAST(a.c_quality_rating AS DECIMAL(3,2))) AS AvgRating,
        ISNULL(SUM(a.c_supervisor_payout_amount), 0) AS TotalPayout
    FROM t_sys_supervisor s
    LEFT JOIN t_sys_supervisor_assignment a ON s.c_supervisor_id = a.c_supervisor_id
        AND a.c_event_date BETWEEN @FromDate AND @ToDate
    WHERE s.c_is_deleted = 0 AND s.c_current_status = 'ACTIVE'
    GROUP BY s.c_supervisor_id, s.c_full_name, s.c_supervisor_type
    ORDER BY TotalAssignments DESC;
END
GO

-- =============================================
-- SEARCH & FILTERING
-- =============================================

CREATE OR ALTER PROCEDURE sp_SearchAssignments
    @SupervisorId BIGINT = NULL,
    @OrderId BIGINT = NULL,
    @Status VARCHAR(30) = NULL,
    @EventDateFrom DATE = NULL,
    @EventDateTo DATE = NULL,
    @SupervisorType VARCHAR(20) = NULL,
    @PaymentReleased BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, s.c_full_name AS SupervisorName, s.c_supervisor_type AS SupervisorType,
           s.c_authority_level AS AuthorityLevel
    FROM t_sys_supervisor_assignment a
    INNER JOIN t_sys_supervisor s ON a.c_supervisor_id = s.c_supervisor_id
    WHERE (@SupervisorId IS NULL OR a.c_supervisor_id = @SupervisorId)
      AND (@OrderId IS NULL OR a.c_order_id = @OrderId)
      AND (@Status IS NULL OR a.c_status = @Status)
      AND (@EventDateFrom IS NULL OR a.c_event_date >= @EventDateFrom)
      AND (@EventDateTo IS NULL OR a.c_event_date <= @EventDateTo)
      AND (@SupervisorType IS NULL OR s.c_supervisor_type = @SupervisorType)
      AND (@PaymentReleased IS NULL OR a.c_payment_release_approved = @PaymentReleased)
    ORDER BY a.c_event_date DESC;
END
GO

PRINT '================================================';
PRINT 'Supervisor Assignment Stored Procedures Created';
PRINT '16 Stored Procedures for SupervisorAssignmentRepository.cs';
PRINT '================================================';
GO
