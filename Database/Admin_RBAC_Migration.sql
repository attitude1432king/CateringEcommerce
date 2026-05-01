-- =====================================================
-- Admin RBAC Migration Script
-- Purpose: Migrate t_sys_admin to use one-to-one role assignment
-- Date: 2026-01-24
-- =====================================================
-- =====================================================
-- STEP 2: Ensure RBAC roles exist
-- =====================================================

-- Ensure SUPER_ADMIN role exists
INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
SELECT 'SUPER_ADMIN', 'Super Administrator', 'Full system access - can manage roles, permissions, and all admins', '#dc2626', TRUE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN');

INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
SELECT
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN'),
    c_permission_id
FROM t_sys_admin_permissions
WHERE c_is_active = TRUE
    AND NOT EXISTS (
        SELECT 1 FROM t_sys_admin_role_permissions rp
        WHERE rp.c_role_id = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN')
        AND rp.c_permission_id = t_sys_admin_permissions.c_permission_id
    );

-- Ensure CATERING_ADMIN role exists
INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
SELECT 'CATERING_ADMIN', 'Catering Manager', 'Manages catering providers, menus, and verification', '#6366f1', FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN');

INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
SELECT
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN'),
    c_permission_id
FROM t_sys_admin_permissions
WHERE c_permission_code IN ('CATERING_VIEW', 'CATERING_VERIFY', 'CATERING_BLOCK', 'CATERING_EDIT')
    AND NOT EXISTS (
        SELECT 1 FROM t_sys_admin_role_permissions rp
        WHERE rp.c_role_id = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN')
        AND rp.c_permission_id = t_sys_admin_permissions.c_permission_id
    );

-- Ensure SYSTEM_ADMIN role exists (default for unmapped admins)
INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
SELECT 'SYSTEM_ADMIN', 'System Administrator', 'General admin with basic permissions', '#10b981', FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'SYSTEM_ADMIN');

INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
SELECT
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SYSTEM_ADMIN'),
    c_permission_id
FROM t_sys_admin_permissions
WHERE c_permission_code IN ('CATERING_VIEW', 'USER_VIEW', 'REVIEW_VIEW', 'EVENT_VIEW')
    AND NOT EXISTS (
        SELECT 1 FROM t_sys_admin_role_permissions rp
        WHERE rp.c_role_id = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SYSTEM_ADMIN')
        AND rp.c_permission_id = t_sys_admin_permissions.c_permission_id
    );

-- =====================================================
-- STEP 3: Migrate existing role data
-- =====================================================

UPDATE t_sys_admin a
SET c_role_id = r.c_role_id
FROM t_sys_admin_roles r
WHERE r.c_role_code = 'SUPER_ADMIN'
    AND a.c_role = 'Super Admin'
    AND a.c_role_id IS NULL;

UPDATE t_sys_admin a
SET c_role_id = r.c_role_id
FROM t_sys_admin_roles r
WHERE r.c_role_code = 'SYSTEM_ADMIN'
    AND a.c_role = 'System Admin'
    AND a.c_role_id IS NULL;

-- Assign default role to any remaining admins without role_id
UPDATE t_sys_admin a
SET c_role_id = r.c_role_id
FROM t_sys_admin_roles r
WHERE r.c_role_code = 'SYSTEM_ADMIN'
    AND a.c_role_id IS NULL;

-- =====================================================
-- STEP 4: Add Foreign Key constraint
-- =====================================================

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'fk_admin_role'
          AND table_name = 't_sys_admin'
    ) THEN
        ALTER TABLE t_sys_admin DROP CONSTRAINT fk_admin_role;
    END IF;
END $$;

ALTER TABLE t_sys_admin
ADD CONSTRAINT fk_admin_role
    FOREIGN KEY (c_role_id) REFERENCES t_sys_admin_roles(c_role_id);

-- =====================================================
-- STEP 5: Create index for performance
-- =====================================================

CREATE INDEX IF NOT EXISTS ix_admin_roleid
    ON t_sys_admin(c_role_id);

-- =====================================================
-- STEP 6: Verification queries
-- =====================================================

-- Count admins by role
SELECT
    r.c_role_name AS Role,
    r.c_role_code AS RoleCode,
    COUNT(a.c_adminid) AS AdminCount,
    r.c_color AS Color
FROM t_sys_admin_roles r
LEFT JOIN t_sys_admin a ON a.c_role_id = r.c_role_id
WHERE r.c_is_active = TRUE
GROUP BY r.c_role_name, r.c_role_code, r.c_color
ORDER BY AdminCount DESC;

-- Verify Super Admin exists and list them
DO $$
DECLARE
    v_SuperAdminCount INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_SuperAdminCount
    FROM t_sys_admin a
    JOIN t_sys_admin_roles r ON a.c_role_id = r.c_role_id
    WHERE r.c_role_code = 'SUPER_ADMIN'
      AND a.c_isactive = TRUE;

    IF v_SuperAdminCount > 0 THEN
        RAISE NOTICE 'Active Super Admin count: %', v_SuperAdminCount;
    ELSE
        RAISE NOTICE 'WARNING: No active Super Admin found!';
        RAISE NOTICE 'Please create at least one Super Admin manually.';
    END IF;

    RAISE NOTICE '================================================';
    RAISE NOTICE 'Admin RBAC Migration Completed Successfully!';
    RAISE NOTICE '================================================';
    RAISE NOTICE '';
    RAISE NOTICE 'Next Steps:';
    RAISE NOTICE '1. Verify the migration results above';
    RAISE NOTICE '2. Test admin login with new role system';
    RAISE NOTICE '3. (Optional) Deprecate c_role column after verifying new system works';
    RAISE NOTICE '';
    RAISE NOTICE 'Notes:';
    RAISE NOTICE '- Old c_role column kept for backward compatibility';
    RAISE NOTICE '- Synonym t_sys_admin_users points to t_sys_admin';
    RAISE NOTICE '- All admins now have one-to-one role assignment via c_role_id';
    RAISE NOTICE '================================================';
END $$;