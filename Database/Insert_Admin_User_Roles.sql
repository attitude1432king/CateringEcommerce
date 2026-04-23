DO $$
DECLARE
    v_super_admin_role_id BIGINT;
    v_catering_admin_role_id BIGINT;
    v_marketing_admin_role_id BIGINT;
    v_permission_count INTEGER;
    v_total_assignments INTEGER;
    v_active_assignments INTEGER;
BEGIN

-- ============================================
-- Step 1: Get Role IDs
-- ============================================
RAISE NOTICE 'Fetching role IDs...';

SELECT c_role_id INTO v_super_admin_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'SUPER_ADMIN' AND c_is_active = TRUE
LIMIT 1;

SELECT c_role_id INTO v_catering_admin_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = TRUE
LIMIT 1;

SELECT c_role_id INTO v_marketing_admin_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'MARKETING_ADMIN' AND c_is_active = TRUE
LIMIT 1;

RAISE NOTICE 'SUPER_ADMIN: %', v_super_admin_role_id;
RAISE NOTICE 'CATERING_ADMIN: %', v_catering_admin_role_id;
RAISE NOTICE 'MARKETING_ADMIN: %', v_marketing_admin_role_id;

-- ============================================
-- Step 2: Assign SUPER_ADMIN to Admin ID = 1
-- ============================================
RAISE NOTICE 'Assigning SUPER_ADMIN to Admin ID = 1';

IF NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles
    WHERE c_adminid = 1 AND c_role_id = v_super_admin_role_id
) THEN

    INSERT INTO t_sys_admin_user_roles (
        c_adminid,
        c_role_id,
        c_assigned_by,
        c_is_active,
        c_assigned_date
    )
    VALUES (
        1,
        v_super_admin_role_id,
        1,
        TRUE,
        CURRENT_TIMESTAMP
    );

    RAISE NOTICE 'Inserted SUPER_ADMIN role';

ELSE

    UPDATE t_sys_admin_user_roles
    SET c_is_active = TRUE
    WHERE c_adminid = 1 AND c_role_id = v_super_admin_role_id;

    RAISE NOTICE 'Updated existing SUPER_ADMIN role';

END IF;

-- ============================================
-- Step 3: Verification Queries
-- ============================================
RAISE NOTICE 'Run manual queries for verification';

-- Query 1: Roles
RAISE NOTICE 'SELECT r.c_role_code FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = TRUE;';

-- Query 2: Permissions (LIMIT instead of TOP)
RAISE NOTICE 'SELECT p.c_permission_code FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = TRUE
LIMIT 10;';

-- ============================================
-- Step 4: Permission Count
-- ============================================
SELECT COUNT(DISTINCT p.c_permission_code)
INTO v_permission_count
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = TRUE AND p.c_is_active = TRUE;

RAISE NOTICE 'Total permissions: %', v_permission_count;

-- ============================================
-- Step 5: Summary
-- ============================================
SELECT COUNT(*) INTO v_total_assignments
FROM t_sys_admin_user_roles;

SELECT COUNT(*) INTO v_active_assignments
FROM t_sys_admin_user_roles
WHERE c_is_active = TRUE;

RAISE NOTICE 'Total assignments: %', v_total_assignments;
RAISE NOTICE 'Active assignments: %', v_active_assignments;

RAISE NOTICE 'Admin role assignment completed successfully';

END $$;