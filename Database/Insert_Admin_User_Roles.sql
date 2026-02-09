-- =====================================================
-- INSERT Query for t_sys_admin_user_roles
-- Maps Admin Users to Roles
-- =====================================================

USE CateringEcommerce;
GO

PRINT '=====================================================';
PRINT 'Inserting Admin User Role Assignments';
PRINT '=====================================================';
PRINT '';

-- =====================================================
-- Step 1: Get Role IDs
-- =====================================================
DECLARE @SuperAdminRoleId BIGINT;
DECLARE @CateringAdminRoleId BIGINT;
DECLARE @MarketingAdminRoleId BIGINT;

SELECT @SuperAdminRoleId = c_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'SUPER_ADMIN' AND c_is_active = 1;

SELECT @CateringAdminRoleId = c_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = 1;

SELECT @MarketingAdminRoleId = c_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'MARKETING_ADMIN' AND c_is_active = 1;

PRINT 'Role IDs:';
PRINT '  SUPER_ADMIN: ' + CAST(@SuperAdminRoleId AS VARCHAR(10));
PRINT '  CATERING_ADMIN: ' + ISNULL(CAST(@CateringAdminRoleId AS VARCHAR(10)), 'NULL');
PRINT '  MARKETING_ADMIN: ' + ISNULL(CAST(@MarketingAdminRoleId AS VARCHAR(10)), 'NULL');
PRINT '';

-- =====================================================
-- Step 2: Insert Admin User Role Assignments
-- =====================================================

-- METHOD 1: Assign SUPER_ADMIN to Admin ID = 1 (First Admin)
-- This is the most common scenario
PRINT 'Method 1: Assigning SUPER_ADMIN to Admin ID = 1';

IF NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles
    WHERE c_adminid = 1 AND c_role_id = @SuperAdminRoleId
)
BEGIN
    INSERT INTO t_sys_admin_user_roles (
        c_adminid,
        c_role_id,
        c_assigned_by,
        c_is_active,
        c_assigned_date
    )
    VALUES (
        1,                      -- Admin ID (change as needed)
        @SuperAdminRoleId,      -- SUPER_ADMIN Role ID
        1,                      -- Assigned by Admin ID = 1 (self-assign)
        1,                      -- Is Active
        GETDATE()               -- Assigned Date
    );

    PRINT '✓ Assigned SUPER_ADMIN role to Admin ID = 1';
END
ELSE
BEGIN
    -- Update existing record to active
    UPDATE t_sys_admin_user_roles
    SET c_is_active = 1
    WHERE c_adminid = 1 AND c_role_id = @SuperAdminRoleId;

    PRINT '✓ Updated existing SUPER_ADMIN role assignment to active';
END

PRINT '';

-- =====================================================
-- METHOD 2: Bulk Insert for Multiple Admins
-- Uncomment and modify as needed
-- =====================================================
/*
PRINT 'Method 2: Bulk insert for multiple admins';

-- Assign roles to multiple admins at once
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT AdminId, RoleId, 1, 1
FROM (VALUES
    -- (AdminId, RoleId)
    (1, @SuperAdminRoleId),          -- Admin 1 = SUPER_ADMIN
    (2, @CateringAdminRoleId),       -- Admin 2 = CATERING_ADMIN
    (3, @CateringAdminRoleId),       -- Admin 3 = CATERING_ADMIN
    (4, @MarketingAdminRoleId)       -- Admin 4 = MARKETING_ADMIN
) AS Assignments(AdminId, RoleId)
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles ur
    WHERE ur.c_adminid = Assignments.AdminId
    AND ur.c_role_id = Assignments.RoleId
);

PRINT '✓ Bulk insert completed';
*/

-- =====================================================
-- METHOD 3: Assign Role to All Existing Admins
-- Assigns CATERING_ADMIN to all admins who don't have a role
-- =====================================================
/*
PRINT 'Method 3: Auto-assign CATERING_ADMIN to all admins without roles';

INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
SELECT
    a.c_adminid,
    @CateringAdminRoleId,
    1,  -- Assigned by Admin ID = 1
    1   -- Active
FROM t_sys_admin_users a
WHERE a.c_is_active = 1
  AND NOT EXISTS (
      SELECT 1
      FROM t_sys_admin_user_roles ur
      WHERE ur.c_adminid = a.c_adminid
      AND ur.c_is_active = 1
  );

PRINT '✓ Auto-assigned CATERING_ADMIN to admins without roles';
*/

-- =====================================================
-- Step 3: Verify the Inserts
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT 'Verification:';
PRINT '=====================================================';
PRINT '';

-- Show all admin-role assignments
PRINT 'Current Admin-Role Assignments:';
SELECT
    ur.c_id AS Id,
    ur.c_adminid AS AdminId,
    a.c_full_name AS AdminName,
    a.c_email AS Email,
    ur.c_role_id AS RoleId,
    r.c_role_code AS RoleCode,
    r.c_role_name AS RoleName,
    ur.c_is_active AS IsActive,
    ur.c_assigned_date AS AssignedDate
FROM t_sys_admin_user_roles ur
INNER JOIN t_sys_admin_users a ON ur.c_adminid = a.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
ORDER BY ur.c_adminid, ur.c_is_active DESC;

PRINT '';

-- =====================================================
-- Step 4: Test the Queries from the User
-- =====================================================
PRINT '=====================================================';
PRINT 'Testing Original Queries:';
PRINT '=====================================================';
PRINT '';

-- Query 1: Get roles for Admin ID = 1
PRINT 'Query 1: Get roles for Admin ID = 1';
SELECT r.c_role_code AS RoleCode
FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND r.c_is_active = 1;

PRINT '';

-- Query 2: Get permissions for Admin ID = 1
PRINT 'Query 2: Get permissions for Admin ID = 1 (showing first 10)';
SELECT TOP 10
    p.c_permission_code AS PermissionCode,
    p.c_permission_name AS PermissionName,
    p.c_module AS Module
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND p.c_is_active = 1
ORDER BY p.c_module, p.c_permission_code;

PRINT '';

-- Query 3: Count total permissions
DECLARE @PermissionCount INT;
SELECT @PermissionCount = COUNT(DISTINCT p.c_permission_code)
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND p.c_is_active = 1;

PRINT 'Total permissions for Admin ID = 1: ' + CAST(@PermissionCount AS VARCHAR(10));
PRINT '';

-- =====================================================
-- Summary
-- =====================================================
PRINT '=====================================================';
PRINT 'Summary:';
PRINT '=====================================================';

DECLARE @TotalAssignments INT;
DECLARE @ActiveAssignments INT;

SELECT @TotalAssignments = COUNT(*)
FROM t_sys_admin_user_roles;

SELECT @ActiveAssignments = COUNT(*)
FROM t_sys_admin_user_roles
WHERE c_is_active = 1;

PRINT 'Total admin-role assignments: ' + CAST(@TotalAssignments AS VARCHAR(10));
PRINT 'Active assignments: ' + CAST(@ActiveAssignments AS VARCHAR(10));
PRINT '';
PRINT '✅ Admin User Role assignments completed successfully!';

GO
