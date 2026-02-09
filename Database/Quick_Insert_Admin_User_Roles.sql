-- =====================================================
-- QUICK INSERT for t_sys_admin_user_roles
-- Single query to assign SUPER_ADMIN to Admin ID = 1
-- =====================================================

USE CateringEcommerce;
GO

-- Simple INSERT: Assign SUPER_ADMIN role to Admin ID = 1
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active, c_assigned_date)
SELECT
    1 AS c_adminid,                                                      -- Admin ID
    r.c_role_id,                                                         -- Role ID from t_sys_admin_roles
    1 AS c_assigned_by,                                                  -- Assigned by Admin ID = 1
    1 AS c_is_active,                                                    -- Active = True
    GETDATE() AS c_assigned_date                                         -- Current timestamp
FROM t_sys_admin_roles r
WHERE r.c_role_code = 'SUPER_ADMIN'                                     -- Get SUPER_ADMIN role
  AND r.c_is_active = 1                                                  -- Only active roles
  AND NOT EXISTS (                                                       -- Avoid duplicates
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = 1 AND ur.c_role_id = r.c_role_id
  );

-- Verify the insert
SELECT
    ur.c_adminid AS AdminId,
    a.c_full_name AS AdminName,
    r.c_role_code AS RoleCode,
    r.c_role_name AS RoleName,
    ur.c_is_active AS IsActive
FROM t_sys_admin_user_roles ur
INNER JOIN t_sys_admin_users a ON ur.c_adminid = a.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
WHERE ur.c_adminid = 1;

GO

-- =====================================================
-- TEST: Verify the queries now return data
-- =====================================================

-- Query 1: Get admin roles
SELECT r.c_role_code
FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND r.c_is_active = 1;

-- Query 2: Get admin permissions
SELECT DISTINCT p.c_permission_code
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND p.c_is_active = 1;

GO
