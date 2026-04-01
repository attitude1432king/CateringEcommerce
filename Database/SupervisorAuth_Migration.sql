-- ============================================================
-- Supervisor Authentication Migration
-- Adds stored procedures required by SupervisorAuthController
-- ============================================================

-- sp_GetSupervisorForLogin
-- Returns credential fields only (used for login verification)
IF OBJECT_ID('dbo.sp_GetSupervisorForLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSupervisorForLogin;
GO

CREATE PROCEDURE dbo.sp_GetSupervisorForLogin
    @Identifier NVARCHAR(100)   -- email or phone
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        c_supervisor_id     AS SupervisorId,
        c_full_name         AS FullName,
        c_email             AS Email,
        c_phone             AS Phone,
        c_password_hash     AS PasswordHash,
        c_current_status    AS CurrentStatus,
        c_supervisor_type   AS SupervisorType,
        c_authority_level   AS AuthorityLevel
    FROM t_sys_supervisor
    WHERE
        (c_email = @Identifier OR c_phone = @Identifier)
        AND c_is_deleted = 0;
END
GO

-- sp_UpdateSupervisorLastLogin
IF OBJECT_ID('dbo.sp_UpdateSupervisorLastLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateSupervisorLastLogin;
GO

CREATE PROCEDURE dbo.sp_UpdateSupervisorLastLogin
    @SupervisorId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_supervisor
    SET c_last_login_date = GETUTCDATE(),
        c_modified_date   = GETUTCDATE()
    WHERE c_supervisor_id = @SupervisorId
      AND c_is_deleted = 0;
END
GO
