DO $$
DECLARE
    v_admins_without_roles INTEGER;
    v_super_admin_role_id BIGINT;
    v_catering_admin_role_id BIGINT;
    v_other_admins_assigned INTEGER;
BEGIN

-- ============================================
-- Step 1: Checking current state
-- ============================================
RAISE NOTICE 'Step 1: Checking current state...';

SELECT COUNT(*)
INTO v_admins_without_roles
FROM t_sys_admin_users a
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles ur
    WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = TRUE
);

RAISE NOTICE 'Admins without roles: %', v_admins_without_roles;

-- ============================================
-- Step 2: Get SUPER_ADMIN role ID
-- ============================================
RAISE NOTICE 'Step 2: Getting SUPER_ADMIN role ID...';

SELECT c_role_id
INTO v_super_admin_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'SUPER_ADMIN' AND c_is_active = TRUE
LIMIT 1;

IF v_super_admin_role_id IS NULL THEN
    RAISE EXCEPTION 'ERROR: SUPER_ADMIN role not found!';
END IF;

RAISE NOTICE 'SUPER_ADMIN role found: ID = %', v_super_admin_role_id;

-- ============================================
-- Step 3: Assign SUPER_ADMIN to Admin ID = 1
-- ============================================
RAISE NOTICE 'Step 3: Assigning SUPER_ADMIN role...';

IF NOT EXISTS (
    SELECT 1 FROM t_sys_admin_users WHERE c_adminid = 1
) THEN
    RAISE EXCEPTION 'ERROR: Admin with ID = 1 not found!';
END IF;

IF EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles
    WHERE c_adminid = 1 AND c_role_id = v_super_admin_role_id
) THEN

    UPDATE t_sys_admin_user_roles
    SET c_is_active = TRUE
    WHERE c_adminid = 1 AND c_role_id = v_super_admin_role_id;

    RAISE NOTICE 'Updated existing role assignment';

ELSE

    INSERT INTO t_sys_admin_user_roles
    (c_adminid, c_role_id, c_assigned_by, c_is_active)
    VALUES (1, v_super_admin_role_id, 1, TRUE);

    RAISE NOTICE 'Inserted new role assignment';

END IF;

-- ============================================
-- Step 4: Assign roles to other admins
-- ============================================
RAISE NOTICE 'Step 4: Assigning roles to other admins...';

SELECT c_role_id
INTO v_catering_admin_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = TRUE
LIMIT 1;

IF v_catering_admin_role_id IS NOT NULL THEN

    INSERT INTO t_sys_admin_user_roles
    (c_adminid, c_role_id, c_assigned_by, c_is_active)
    SELECT
        a.c_adminid,
        v_catering_admin_role_id,
        1,
        TRUE
    FROM t_sys_admin_users a
    WHERE a.c_adminid > 1
      AND a.c_isactive = TRUE
      AND NOT EXISTS (
          SELECT 1
          FROM t_sys_admin_user_roles ur
          WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = TRUE
      );

    GET DIAGNOSTICS v_other_admins_assigned = ROW_COUNT;

    RAISE NOTICE 'Assigned CATERING_ADMIN to % admins', v_other_admins_assigned;

ELSE
    RAISE NOTICE 'CATERING_ADMIN role not found, skipping...';
END IF;

-- ============================================
-- Step 5: Verification Output
-- ============================================
RAISE NOTICE 'Step 5: Verification';

-- You cannot PRINT tables like SQL Server,
-- so run these manually after execution:

RAISE NOTICE 'Run this query manually to verify:';
RAISE NOTICE 'SELECT * FROM t_sys_admin_user_roles;';

END $$;