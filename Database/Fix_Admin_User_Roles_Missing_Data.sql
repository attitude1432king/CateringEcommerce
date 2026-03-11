-- =====================================================
-- Fix Missing Admin User Roles Records
-- =====================================================
-- This script assigns roles to existing admin users
-- who don't have any role assignments
-- =====================================================

USE CateringDB;
GO

PRINT '=====================================================';
PRINT 'Starting Admin User Roles Fix Script';
PRINT '=====================================================';
PRINT '';

-- =====================================================
-- Step 1: Check current state
-- =====================================================
PRINT 'Step 1: Checking current state...';
PRINT '';

-- Count admins without roles
DECLARE @AdminsWithoutRoles INT;
SELECT @AdminsWithoutRoles = COUNT(*)
FROM t_sys_admin_users a
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles ur
    WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = 1
);

PRINT 'Admins without roles: ' + CAST(@AdminsWithoutRoles AS VARCHAR(10));

-- Show admin details
PRINT '';
PRINT 'Admin Users:';
SELECT
    c_adminid AS AdminId,
    c_full_name AS FullName,
    c_email AS Email,
    c_username AS Username,
    c_is_active AS IsActive
FROM t_sys_admin_users
ORDER BY c_adminid;

PRINT '';
PRINT 'Available Roles:';
SELECT
    c_role_id AS RoleId,
    c_role_code AS RoleCode,
    c_role_name AS RoleName,
    c_is_system_role AS IsSystemRole,
    c_is_active AS IsActive
FROM t_sys_admin_roles
WHERE c_is_active = 1
ORDER BY c_role_id;

PRINT '';
PRINT 'Current Admin-Role Assignments:';
SELECT
    ur.c_id AS Id,
    ur.c_adminid AS AdminId,
    a.c_full_name AS AdminName,
    ur.c_role_id AS RoleId,
    r.c_role_code AS RoleCode,
    r.c_role_name AS RoleName,
    ur.c_is_active AS IsActive
FROM t_sys_admin_user_roles ur
INNER JOIN t_sys_admin_users a ON ur.c_adminid = a.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
ORDER BY ur.c_adminid, ur.c_role_id;

-- =====================================================
-- Step 2: Get SUPER_ADMIN role ID
-- =====================================================
PRINT '';
PRINT 'Step 2: Getting SUPER_ADMIN role ID...';

DECLARE @SuperAdminRoleId BIGINT;
SELECT @SuperAdminRoleId = c_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'SUPER_ADMIN' AND c_is_active = 1;

IF @SuperAdminRoleId IS NULL
BEGIN
    PRINT '❌ ERROR: SUPER_ADMIN role not found!';
    PRINT 'Please run RBAC_Schema.sql first to create the SUPER_ADMIN role.';
    RETURN;
END

PRINT '✓ SUPER_ADMIN role found: ID = ' + CAST(@SuperAdminRoleId AS VARCHAR(10));

-- =====================================================
-- Step 3: Assign SUPER_ADMIN role to Admin ID = 1
-- =====================================================
PRINT '';
PRINT 'Step 3: Assigning SUPER_ADMIN role to Admin ID = 1...';

-- Check if admin exists
IF NOT EXISTS (SELECT 1 FROM t_sys_admin_users WHERE c_adminid = 1)
BEGIN
    PRINT '❌ ERROR: Admin with ID = 1 not found!';
    PRINT 'Please ensure you have at least one admin user in t_sys_admin_users table.';
    RETURN;
END

-- Check if assignment already exists
IF EXISTS (
    SELECT 1
    FROM t_sys_admin_user_roles
    WHERE c_adminid = 1 AND c_role_id = @SuperAdminRoleId
)
BEGIN
    -- Update existing record to active
    UPDATE t_sys_admin_user_roles
    SET c_is_active = 1
    WHERE c_adminid = 1 AND c_role_id = @SuperAdminRoleId;

    PRINT '✓ Updated existing role assignment to active';
END
ELSE
BEGIN
    -- Insert new record
    INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
    VALUES (1, @SuperAdminRoleId, 1, 1);

    PRINT '✓ Inserted new role assignment for Admin ID = 1';
END

-- =====================================================
-- Step 4: Assign roles to other admins (if any)
-- =====================================================
PRINT '';
PRINT 'Step 4: Assigning roles to other admins...';

-- Get CATERING_ADMIN role ID (for secondary admins)
DECLARE @CateringAdminRoleId BIGINT;
SELECT @CateringAdminRoleId = c_role_id
FROM t_sys_admin_roles
WHERE c_role_code = 'CATERING_ADMIN' AND c_is_active = 1;

-- Assign CATERING_ADMIN to all other active admins who don't have a role
IF @CateringAdminRoleId IS NOT NULL
BEGIN
    INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by, c_is_active)
    SELECT
        a.c_adminid,
        @CateringAdminRoleId,
        1, -- Assigned by Super Admin
        1  -- Active
    FROM t_sys_admin_users a
    WHERE a.c_adminid > 1 -- Not the first admin
      AND a.c_is_active = 1
      AND NOT EXISTS (
          SELECT 1
          FROM t_sys_admin_user_roles ur
          WHERE ur.c_adminid = a.c_adminid AND ur.c_is_active = 1
      );

    DECLARE @OtherAdminsAssigned INT = @@ROWCOUNT;
    PRINT '✓ Assigned CATERING_ADMIN role to ' + CAST(@OtherAdminsAssigned AS VARCHAR(10)) + ' other admin(s)';
END
ELSE
BEGIN
    PRINT '⚠ CATERING_ADMIN role not found. Skipping assignment for other admins.';
END

-- =====================================================
-- Step 5: Verify the fix
-- =====================================================
PRINT '';
PRINT 'Step 5: Verifying the fix...';
PRINT '';

-- Test Query 1: Get admin roles
PRINT 'Test Query 1: Get roles for Admin ID = 1';
SELECT r.c_role_code AS RoleCode
FROM t_sys_admin_roles r
INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND r.c_is_active = 1;

PRINT '';

-- Test Query 2: Get admin permissions
PRINT 'Test Query 2: Get permissions for Admin ID = 1';
SELECT DISTINCT p.c_permission_code AS PermissionCode
FROM t_sys_admin_permissions p
INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
WHERE ur.c_adminid = 1 AND ur.c_is_active = 1 AND p.c_is_active = 1
ORDER BY p.c_permission_code;

PRINT '';

-- Final summary
PRINT '=====================================================';
PRINT 'Final Summary:';
PRINT '=====================================================';

SELECT
    a.c_adminid AS AdminId,
    a.c_full_name AS AdminName,
    a.c_email AS Email,
    r.c_role_code AS RoleCode,
    r.c_role_name AS RoleName,
    ur.c_is_active AS RoleIsActive,
    ur.c_assigned_date AS AssignedDate,
    (SELECT COUNT(*)
     FROM t_sys_admin_role_permissions rp
     WHERE rp.c_role_id = r.c_role_id) AS PermissionCount
FROM t_sys_admin_users a
INNER JOIN t_sys_admin_user_roles ur ON a.c_adminid = ur.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
WHERE ur.c_is_active = 1
ORDER BY a.c_adminid, r.c_role_code;

PRINT '';
PRINT '✅ Admin User Roles fix completed successfully!';
PRINT '';

GO
