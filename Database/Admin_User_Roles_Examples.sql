-- =====================================================
-- t_sys_admin_user_roles - INSERT Query Examples
-- Maps Admin Users to Roles
-- =====================================================

USE CateringDB;
GO

-- =====================================================
-- EXAMPLE 1: Assign Single Role to Single Admin
-- =====================================================

-- Assign SUPER_ADMIN role to Admin ID = 1
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
VALUES (
    1,                                                               -- Admin ID
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN'),  -- Role ID
    1,                                                               -- Assigned by (Admin ID = 1)
    1                                                                -- Is Active (1 = true)
);

-- =====================================================
-- EXAMPLE 2: Assign Multiple Roles to One Admin
-- (An admin can have multiple roles)
-- =====================================================

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT 2, c_role_id, 1, 1
FROM t_sys_admin_roles
WHERE c_role_code IN ('CATERING_ADMIN', 'MARKETING_ADMIN')
  AND c_is_active = 1;

-- =====================================================
-- EXAMPLE 3: Assign Same Role to Multiple Admins
-- =====================================================

DECLARE @CateringRoleId BIGINT = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN');

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
VALUES
    (3, @CateringRoleId, 1, 1),   -- Admin 3
    (4, @CateringRoleId, 1, 1),   -- Admin 4
    (5, @CateringRoleId, 1, 1);   -- Admin 5

-- =====================================================
-- EXAMPLE 4: Bulk Insert with Role Mapping
-- =====================================================

-- Map multiple admins to their respective roles
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT AdminId, RoleId, 1, 1
FROM (
    VALUES
        -- (AdminId, RoleCode)
        (1, 'SUPER_ADMIN'),
        (2, 'CATERING_ADMIN'),
        (3, 'CATERING_ADMIN'),
        (4, 'MARKETING_ADMIN'),
        (5, 'CATERING_ADMIN')
) AS Mapping(AdminId, RoleCode)
CROSS APPLY (
    SELECT c_role_id AS RoleId
    FROM t_sys_admin_roles
    WHERE c_role_code = Mapping.RoleCode AND c_is_active = 1
) AS Roles
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles ur
    WHERE ur.c_adminid = Mapping.AdminId AND ur.c_role_id = Roles.RoleId
);

-- =====================================================
-- EXAMPLE 5: Auto-Assign Role to All Admins Without Roles
-- =====================================================

-- Assign CATERING_ADMIN to all admins who don't have any role
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT
    a.c_adminid,
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = 1),
    1,  -- Assigned by Admin ID = 1
    1   -- Active
FROM t_sys_admin_users a
WHERE a.c_isactive = 1
  AND NOT EXISTS (
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = 1
  );

-- =====================================================
-- EXAMPLE 6: UPSERT (Insert or Update)
-- If record exists, update to active; otherwise insert new
-- =====================================================

DECLARE @AdminId BIGINT = 1;
DECLARE @RoleId BIGINT = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN');

IF EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles
    WHERE c_adminid = @AdminId AND c_role_id = @RoleId
)
BEGIN
    -- Update existing record
    UPDATE t_sys_admin_user_roles
    SET c_is_active = 1, c_assigned_date = GETDATE()
    WHERE c_adminid = @AdminId AND c_role_id = @RoleId;
END
ELSE
BEGIN
    -- Insert new record
    INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
    VALUES (@AdminId, @RoleId, @AdminId, 1);
END

-- =====================================================
-- EXAMPLE 7: MERGE (Modern UPSERT)
-- =====================================================

MERGE INTO t_sys_admin_user_roles AS target
USING (
    SELECT
        1 AS AdminId,
        r.c_role_id AS RoleId,
        1 AS AssignedBy
    FROM t_sys_admin_roles r
    WHERE r.c_role_code = 'SUPER_ADMIN'
) AS source
ON target.c_adminid = source.AdminId AND target.c_role_id = source.RoleId
WHEN MATCHED THEN
    UPDATE SET c_is_active = 1, c_assigned_date = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (c_adminid, c_role_id, c_assigned_by, c_is_active)
    VALUES (source.AdminId, source.RoleId, source.AssignedBy, 1);

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
    1,  -- Assigned by
    1   -- Active
FROM t_sys_admin_users a
CROSS JOIN t_sys_admin_roles r
WHERE a.c_adminid = 1                    -- Specific admin
  AND a.c_isactive = 1                  -- Admin must be active
  AND r.c_role_code = 'SUPER_ADMIN'      -- Specific role
  AND r.c_is_active = 1                  -- Role must be active
  AND NOT EXISTS (                       -- No duplicate assignment
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
LEFT JOIN t_sys_admin_user_roles ur ON a.c_adminid = ur.c_adminid AND ur.c_is_active = 1
LEFT JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
WHERE a.c_isactive = 1
GROUP BY a.c_adminid, a.c_fullname
ORDER BY a.c_adminid;

-- 3. Admins without any role (need fixing)
SELECT
    a.c_adminid,
    a.c_fullname,
    a.c_email
FROM t_sys_admin_users a
WHERE a.c_isactive = 1
  AND NOT EXISTS (
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = 1
  );

-- 4. Test the original queries from user
-- Get roles for Admin ID = 1
SELECT r.c_role_code
FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND r.c_is_active = 1;

-- Get permissions for Admin ID = 1
SELECT DISTINCT p.c_permission_code
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND p.c_is_active = 1;

GO
