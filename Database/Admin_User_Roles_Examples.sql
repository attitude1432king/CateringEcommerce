-- =====================================================
-- t_sys_admin_user_roles - INSERT Query Examples
-- Maps Admin Users to Roles
-- =====================================================

-- =====================================================
-- EXAMPLE 1: Assign Single Role to Single Admin
-- =====================================================

-- Assign SUPER_ADMIN role to Admin ID = 1
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
VALUES (
    1,                                                                            -- Admin ID
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN'), -- Role ID
    1,                                                                            -- Assigned by (Admin ID = 1)
    TRUE                                                                          -- Is Active
);

-- =====================================================
-- EXAMPLE 2: Assign Multiple Roles to One Admin
-- (An admin can have multiple roles)
-- =====================================================

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT 2, c_role_id, 1, TRUE
FROM t_sys_admin_roles
WHERE c_role_code IN ('CATERING_ADMIN', 'MARKETING_ADMIN')
  AND c_is_active = TRUE;

-- =====================================================
-- EXAMPLE 3: Assign Same Role to Multiple Admins
-- =====================================================

-- Assign CATERING_ADMIN to multiple admins
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT role_id.c_role_id, admin_id.admin_id, 1, TRUE
FROM (VALUES (3), (4), (5)) AS admin_id(admin_id)
CROSS JOIN (
    SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN'
) AS role_id;

-- =====================================================
-- EXAMPLE 4: Bulk Insert with Role Mapping
-- =====================================================

-- Map multiple admins to their respective roles
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT m.AdminId, r.c_role_id, 1, TRUE
FROM (
    VALUES
        -- (AdminId, RoleCode)
        (1, 'SUPER_ADMIN'),
        (2, 'CATERING_ADMIN'),
        (3, 'CATERING_ADMIN'),
        (4, 'MARKETING_ADMIN'),
        (5, 'CATERING_ADMIN')
) AS m(AdminId, RoleCode)
INNER JOIN t_sys_admin_roles r ON r.c_role_code = m.RoleCode AND r.c_is_active = TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles ur
    WHERE ur.c_adminid = m.AdminId AND ur.c_role_id = r.c_role_id
);

-- =====================================================
-- EXAMPLE 5: Auto-Assign Role to All Admins Without Roles
-- =====================================================

-- Assign CATERING_ADMIN to all admins who don't have any role
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT
    a.c_adminid,
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = TRUE),
    1,    -- Assigned by Admin ID = 1
    TRUE  -- Active
FROM t_sys_admin_users a
WHERE a.c_isactive = TRUE
  AND NOT EXISTS (
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = TRUE
  );

-- =====================================================
-- EXAMPLE 6: UPSERT (Insert or Update)
-- If record exists, update to active; otherwise insert new
-- =====================================================

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT 1, c_role_id, 1, TRUE
FROM t_sys_admin_roles
WHERE c_role_code = 'SUPER_ADMIN'
ON CONFLICT (c_adminid, c_role_id) DO UPDATE SET
    c_is_active     = TRUE,
    c_assigned_date = NOW();

-- =====================================================
-- EXAMPLE 7: PostgreSQL Compatible UPSERT
-- =====================================================

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT
    1           AS AdminId,
    r.c_role_id AS RoleId,
    1           AS AssignedBy,
    TRUE        AS is_active
FROM t_sys_admin_roles r
WHERE r.c_role_code = 'SUPER_ADMIN'
ON CONFLICT (c_adminid, c_role_id) DO UPDATE SET
    c_is_active     = TRUE,
    c_assigned_date = NOW();

-- =====================================================
-- EXAMPLE 8: Role Assignment with Validation
-- =====================================================

-- Only insert if:
-- 1. Admin exists and is active
-- 2. Role exists and is active
-- 3. Assignment doesn't already exist

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT
    a.c_adminid,
    r.c_role_id,
    1,    -- Assigned by
    TRUE  -- Active
FROM t_sys_admin_users a
CROSS JOIN t_sys_admin_roles r
WHERE a.c_adminid = 1                 -- Specific admin
  AND a.c_isactive = TRUE             -- Admin must be active
  AND r.c_role_code = 'SUPER_ADMIN'  -- Specific role
  AND r.c_is_active = TRUE           -- Role must be active
  AND NOT EXISTS (                   -- No duplicate assignment
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid
        AND ur.c_role_id = r.c_role_id
  );

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- 1. Show all admin-role mappings
SELECT
    ur.c_id,
    a.c_adminid,
    a.c_fullname AS AdminName,
    r.c_role_code,
    r.c_role_name,
    ur.c_is_active,
    ur.c_assigned_date
FROM t_sys_admin_user_roles ur
INNER JOIN t_sys_admin_users a ON ur.c_adminid = a.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
ORDER BY a.c_adminid, r.c_role_code;

-- 2. Count roles per admin
SELECT
    a.c_adminid,
    a.c_fullname,
    COUNT(ur.c_role_id) AS RoleCount,
    STRING_AGG(r.c_role_code, ', ') AS Roles
FROM t_sys_admin_users a
LEFT JOIN t_sys_admin_user_roles ur ON a.c_adminid = ur.c_adminid AND ur.c_is_active = TRUE
LEFT JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
WHERE a.c_isactive = TRUE
GROUP BY a.c_adminid, a.c_fullname
ORDER BY a.c_adminid;

-- 3. Admins without any role (need fixing)
SELECT
    a.c_adminid,
    a.c_fullname,
    a.c_email
FROM t_sys_admin_users a
WHERE a.c_isactive = TRUE
  AND NOT EXISTS (
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = TRUE
  );

-- 4. Get roles for Admin ID = 1
SELECT r.c_role_code
FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = TRUE AND r.c_is_active = TRUE;

-- 5. Get permissions for Admin ID = 1
SELECT DISTINCT p.c_permission_code
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = TRUE AND p.c_is_active = TRUE;