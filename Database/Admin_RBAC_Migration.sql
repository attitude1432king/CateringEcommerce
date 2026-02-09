-- =====================================================
-- Admin RBAC Migration Script
-- Purpose: Migrate t_sys_admin to use one-to-one role assignment
-- Date: 2026-01-24
-- =====================================================

USE CateringEcommerce;
GO

PRINT '================================================';
PRINT 'Starting Admin RBAC Migration...';
PRINT '================================================';

-- =====================================================
-- STEP 1: Add new columns to t_sys_admin
-- =====================================================

PRINT 'Step 1: Adding new columns to t_sys_admin...';

-- Add c_role_id column (nullable initially for migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_admin') AND name = 'c_role_id')
BEGIN
    ALTER TABLE t_sys_admin ADD c_role_id BIGINT NULL;
    PRINT '  - Column c_role_id added';
END
ELSE
BEGIN
    PRINT '  - Column c_role_id already exists';
END

-- Add c_mobile column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_admin') AND name = 'c_mobile')
BEGIN
    ALTER TABLE t_sys_admin ADD c_mobile NVARCHAR(20) NULL;
    PRINT '  - Column c_mobile added';
END
ELSE
BEGIN
    PRINT '  - Column c_mobile already exists';
END

-- Add c_force_password_reset column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_admin') AND name = 'c_force_password_reset')
BEGIN
    ALTER TABLE t_sys_admin ADD c_force_password_reset BIT DEFAULT 0;
    PRINT '  - Column c_force_password_reset added';
END
ELSE
BEGIN
    PRINT '  - Column c_force_password_reset already exists';
END

GO

-- =====================================================
-- STEP 2: Ensure RBAC roles exist
-- =====================================================

PRINT '';
PRINT 'Step 2: Ensuring RBAC roles exist...';

-- Ensure SUPER_ADMIN role exists
IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('SUPER_ADMIN', 'Super Administrator', 'Full system access - can manage roles, permissions, and all admins', '#dc2626', 1);
    PRINT '  - SUPER_ADMIN role created';

    -- Assign all permissions to SUPER_ADMIN
    DECLARE @SuperAdminRoleId BIGINT = SCOPE_IDENTITY();
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @SuperAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_is_active = 1;
    PRINT '  - Permissions assigned to SUPER_ADMIN';
END
ELSE
BEGIN
    PRINT '  - SUPER_ADMIN role already exists';
END

-- Ensure CATERING_ADMIN role exists
IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('CATERING_ADMIN', 'Catering Manager', 'Manages catering providers, menus, and verification', '#6366f1', 0);
    PRINT '  - CATERING_ADMIN role created';

    -- Assign catering permissions
    DECLARE @CateringAdminRoleId BIGINT = SCOPE_IDENTITY();
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @CateringAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_permission_code IN ('CATERING_VIEW', 'CATERING_VERIFY', 'CATERING_BLOCK', 'CATERING_EDIT');
    PRINT '  - Permissions assigned to CATERING_ADMIN';
END
ELSE
BEGIN
    PRINT '  - CATERING_ADMIN role already exists';
END

-- Ensure SYSTEM_ADMIN role exists (default for unmapped admins)
IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'SYSTEM_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('SYSTEM_ADMIN', 'System Administrator', 'General admin with basic permissions', '#10b981', 0);
    PRINT '  - SYSTEM_ADMIN role created';

    -- Assign basic permissions
    DECLARE @SystemAdminRoleId BIGINT = SCOPE_IDENTITY();
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @SystemAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_permission_code IN ('CATERING_VIEW', 'USER_VIEW', 'REVIEW_VIEW', 'EVENT_VIEW');
    PRINT '  - Basic permissions assigned to SYSTEM_ADMIN';
END
ELSE
BEGIN
    PRINT '  - SYSTEM_ADMIN role already exists';
END

GO

-- =====================================================
-- STEP 3: Migrate existing role data
-- =====================================================

PRINT '';
PRINT 'Step 3: Migrating existing role assignments...';

-- Migrate 'Super Admin' string → SUPER_ADMIN role
UPDATE a
SET a.c_role_id = r.c_role_id
FROM t_sys_admin a
JOIN t_sys_admin_roles r ON r.c_role_code = 'SUPER_ADMIN'
WHERE a.c_role = 'Super Admin' AND a.c_role_id IS NULL;

DECLARE @SuperAdminMigrated INT = @@ROWCOUNT;
PRINT '  - Migrated ' + CAST(@SuperAdminMigrated AS NVARCHAR) + ' Super Admin users';

-- Migrate 'System Admin' string → SYSTEM_ADMIN role
UPDATE a
SET a.c_role_id = r.c_role_id
FROM t_sys_admin a
JOIN t_sys_admin_roles r ON r.c_role_code = 'SYSTEM_ADMIN'
WHERE a.c_role = 'System Admin' AND a.c_role_id IS NULL;

DECLARE @SystemAdminMigrated INT = @@ROWCOUNT;
PRINT '  - Migrated ' + CAST(@SystemAdminMigrated AS NVARCHAR) + ' System Admin users';

-- Assign default role to any remaining admins without role_id
UPDATE a
SET a.c_role_id = r.c_role_id
FROM t_sys_admin a
JOIN t_sys_admin_roles r ON r.c_role_code = 'SYSTEM_ADMIN'
WHERE a.c_role_id IS NULL;

DECLARE @DefaultRoleAssigned INT = @@ROWCOUNT;
IF @DefaultRoleAssigned > 0
BEGIN
    PRINT '  - Assigned default SYSTEM_ADMIN role to ' + CAST(@DefaultRoleAssigned AS NVARCHAR) + ' unmapped users';
END

GO

-- =====================================================
-- STEP 4: Verify all admins have valid role_id
-- =====================================================

PRINT '';
PRINT 'Step 4: Verifying data migration...';

DECLARE @AdminsWithoutRole INT;
SELECT @AdminsWithoutRole = COUNT(*) FROM t_sys_admin WHERE c_role_id IS NULL;

IF @AdminsWithoutRole > 0
BEGIN
    PRINT '  ⚠️ WARNING: ' + CAST(@AdminsWithoutRole AS NVARCHAR) + ' admin(s) still have NULL c_role_id!';
    PRINT '  Migration STOPPED. Please manually assign roles to these admins:';
    SELECT c_adminid, c_username, c_email, c_fullname, c_role
    FROM t_sys_admin
    WHERE c_role_id IS NULL;
    RAISERROR('Migration failed: Some admins have NULL c_role_id', 16, 1);
END
ELSE
BEGIN
    PRINT '  ✓ All admins have valid role assignments';
END

GO

-- =====================================================
-- STEP 5: Add Foreign Key constraint
-- =====================================================

PRINT '';
PRINT 'Step 5: Adding foreign key constraint...';

-- Drop FK if it already exists (for re-running script)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Admin_Role')
BEGIN
    ALTER TABLE t_sys_admin DROP CONSTRAINT FK_Admin_Role;
    PRINT '  - Existing FK_Admin_Role dropped';
END

-- Add FK constraint
ALTER TABLE t_sys_admin
ADD CONSTRAINT FK_Admin_Role
    FOREIGN KEY (c_role_id) REFERENCES t_sys_admin_roles(c_role_id);

PRINT '  ✓ Foreign key constraint FK_Admin_Role created';

GO

-- =====================================================
-- STEP 6: Make c_role_id NOT NULL
-- =====================================================

PRINT '';
PRINT 'Step 6: Making c_role_id mandatory...';

-- Make c_role_id NOT NULL
ALTER TABLE t_sys_admin ALTER COLUMN c_role_id BIGINT NOT NULL;

PRINT '  ✓ Column c_role_id is now NOT NULL';

GO

-- =====================================================
-- STEP 7: Create synonym for compatibility
-- =====================================================

PRINT '';
PRINT 'Step 7: Creating synonym t_sys_admin_users...';

-- Drop synonym if it exists
IF EXISTS (SELECT * FROM sys.synonyms WHERE name = 't_sys_admin_users')
BEGIN
    DROP SYNONYM t_sys_admin_users;
    PRINT '  - Existing synonym dropped';
END

-- Create synonym
CREATE SYNONYM t_sys_admin_users FOR t_sys_admin;

PRINT '  ✓ Synonym t_sys_admin_users created (points to t_sys_admin)';

GO

-- =====================================================
-- STEP 8: Update RBAC audit log foreign keys
-- =====================================================

PRINT '';
PRINT 'Step 8: Updating RBAC audit log foreign keys...';

-- The RBAC_Schema.sql references t_sys_admin_users which is now a synonym
-- FK constraint should work through synonym, but let's verify
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__t_sys_adm__c_adm__73852659')
BEGIN
    PRINT '  ✓ Audit log FK already exists (references work through synonym)';
END
ELSE
BEGIN
    PRINT '  - Note: Audit log FK will be created when RBAC_Schema.sql runs';
END

GO

-- =====================================================
-- STEP 9: Create index for performance
-- =====================================================

PRINT '';
PRINT 'Step 9: Creating indexes for performance...';

-- Index on c_role_id for JOINs
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Admin_RoleId' AND object_id = OBJECT_ID('t_sys_admin'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Admin_RoleId
        ON t_sys_admin(c_role_id)
        INCLUDE (c_username, c_email, c_fullname, c_isactive);
    PRINT '  ✓ Index IX_Admin_RoleId created';
END
ELSE
BEGIN
    PRINT '  - Index IX_Admin_RoleId already exists';
END

GO

-- =====================================================
-- STEP 10: Verification Report
-- =====================================================

PRINT '';
PRINT '================================================';
PRINT 'Migration Verification Report';
PRINT '================================================';

-- Count admins by role
PRINT '';
PRINT 'Admin users by role:';
SELECT
    r.c_role_name AS Role,
    r.c_role_code AS RoleCode,
    COUNT(a.c_adminid) AS AdminCount,
    r.c_color AS Color
FROM t_sys_admin_roles r
LEFT JOIN t_sys_admin a ON a.c_role_id = r.c_role_id
WHERE r.c_is_active = 1
GROUP BY r.c_role_name, r.c_role_code, r.c_color
ORDER BY AdminCount DESC;

-- Verify Super Admin exists
PRINT '';
DECLARE @SuperAdminCount INT;
SELECT @SuperAdminCount = COUNT(*)
FROM t_sys_admin a
JOIN t_sys_admin_roles r ON a.c_role_id = r.c_role_id
WHERE r.c_role_code = 'SUPER_ADMIN' AND a.c_isactive = 1;

IF @SuperAdminCount > 0
BEGIN
    PRINT '✓ Active Super Admin count: ' + CAST(@SuperAdminCount AS NVARCHAR);
    PRINT '';
    PRINT 'Super Admin users:';
    SELECT
        a.c_adminid AS AdminID,
        a.c_username AS Username,
        a.c_email AS Email,
        a.c_fullname AS FullName,
        CASE WHEN a.c_isactive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
    FROM t_sys_admin a
    JOIN t_sys_admin_roles r ON a.c_role_id = r.c_role_id
    WHERE r.c_role_code = 'SUPER_ADMIN';
END
ELSE
BEGIN
    PRINT '⚠️ WARNING: No active Super Admin found!';
    PRINT 'Please create at least one Super Admin manually.';
END

PRINT '';
PRINT '================================================';
PRINT '✓ Admin RBAC Migration Completed Successfully!';
PRINT '================================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Verify the migration results above';
PRINT '2. Test admin login with new role system';
PRINT '3. (Optional) Deprecate c_role column after verifying new system works';
PRINT '';
PRINT 'Notes:';
PRINT '- Old c_role column kept for backward compatibility';
PRINT '- Synonym t_sys_admin_users points to t_sys_admin';
PRINT '- All admins now have one-to-one role assignment via c_role_id';
PRINT '================================================';

GO
