-- ============================================================
-- Supervisor Authentication Migration
-- Adds stored procedures required by SupervisorAuthController
-- ============================================================

-- sp_GetSupervisorForLogin
-- Returns credential fields only (used for login verification)
CREATE OR REPLACE FUNCTION sp_GetSupervisorForLogin(
    p_Identifier VARCHAR
)
RETURNS TABLE (
    SupervisorId BIGINT,
    FullName VARCHAR,
    Email VARCHAR,
    Phone VARCHAR,
    PasswordHash VARCHAR,
    CurrentStatus VARCHAR,
    SupervisorType VARCHAR,
    AuthorityLevel VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_supervisor_id     AS SupervisorId,
        c_full_name         AS FullName,
        c_email             AS Email,
        c_phone             AS Phone,
        c_password_hash     AS PasswordHash,
        c_current_status    AS CurrentStatus,
        c_supervisor_type   AS SupervisorType,
        c_authority_level   AS AuthorityLevel
    FROM t_sys_supervisor
    WHERE (c_email = p_Identifier OR c_phone = p_Identifier)
      AND c_is_deleted = FALSE
    LIMIT 1;

END;
$$;

-- sp_UpdateSupervisorLastLogin
CREATE OR REPLACE FUNCTION sp_UpdateSupervisorLastLogin(
    p_SupervisorId BIGINT
)
RETURNS TABLE (Success INT)
LANGUAGE plpgsql
AS $$
BEGIN

    UPDATE t_sys_supervisor
    SET c_last_login_date = NOW(),
        c_modified_date   = NOW()
    WHERE c_supervisor_id = p_SupervisorId
      AND c_is_deleted = FALSE;

    RETURN QUERY SELECT 1 AS Success;

END;
$$;
