-- =====================================================
-- RBAC (Role-Based Access Control) Database Schema
-- Enterprise-Grade Permission System
-- =====================================================

USE CateringDB;
GO

-- =====================================================
-- 1. ROLES TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_roles')
BEGIN
    CREATE TABLE t_sys_admin_roles (
        c_role_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_role_code NVARCHAR(50) NOT NULL UNIQUE,
        c_role_name NVARCHAR(100) NOT NULL,
        c_description NVARCHAR(500),
        c_color NVARCHAR(20) DEFAULT '#6366f1',
        c_is_system_role BIT DEFAULT 0,  -- Cannot be deleted if true
        c_is_active BIT DEFAULT 1,
        c_createddate DATETIME DEFAULT GETDATE(),
        c_created_by BIGINT,
        c_modifieddate DATETIME,
        c_updated_by BIGINT,

        INDEX IX_role_code (c_role_code),
        INDEX IX_active (c_is_active)
    );
END
GO

-- =====================================================
-- 2. PERMISSIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_permissions')
BEGIN
    CREATE TABLE t_sys_admin_permissions (
        c_permission_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_permission_code NVARCHAR(50) NOT NULL UNIQUE,
        c_permission_name NVARCHAR(100) NOT NULL,
        c_description NVARCHAR(500),
        c_module NVARCHAR(50),  -- CATERING, USER, REVIEW, EARNINGS, etc.
        c_action NVARCHAR(50),  -- VIEW, CREATE, EDIT, DELETE, BLOCK, etc.
        c_is_active BIT DEFAULT 1,
        c_createddate DATETIME DEFAULT GETDATE(),

        INDEX IX_permission_code (c_permission_code),
        INDEX IX_module (c_module),
        INDEX IX_active (c_is_active)
    );
END
GO

-- =====================================================
-- 3. ROLE-PERMISSION JUNCTION TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_role_permissions')
BEGIN
    CREATE TABLE t_sys_admin_role_permissions (
        c_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_role_id BIGINT NOT NULL,
        c_permission_id BIGINT NOT NULL,
        c_assigned_date DATETIME DEFAULT GETDATE(),
        c_assigned_by BIGINT,

        FOREIGN KEY (c_role_id) REFERENCES t_sys_admin_roles(c_role_id) ON DELETE CASCADE,
        FOREIGN KEY (c_permission_id) REFERENCES t_sys_admin_permissions(c_permission_id) ON DELETE CASCADE,
        UNIQUE (c_role_id, c_permission_id),

        INDEX IX_role_id (c_role_id),
        INDEX IX_permission_id (c_permission_id)
    );
END
GO

-- =====================================================
-- 4. ADMIN USER-ROLE JUNCTION TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_user_roles')
BEGIN
    CREATE TABLE t_sys_admin_user_roles (
        c_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_adminid BIGINT NOT NULL,
        c_role_id BIGINT NOT NULL,
        c_assigned_date DATETIME DEFAULT GETDATE(),
        c_assigned_by BIGINT,
        c_is_active BIT DEFAULT 1,

        FOREIGN KEY (c_adminid) REFERENCES t_sys_admin_users(c_adminid) ON DELETE CASCADE,
        FOREIGN KEY (c_role_id) REFERENCES t_sys_admin_roles(c_role_id) ON DELETE CASCADE,
        UNIQUE (c_adminid, c_role_id),

        INDEX IX_admin_id (c_adminid),
        INDEX IX_role_id (c_role_id),
        INDEX IX_active (c_is_active)
    );
END
GO

-- =====================================================
-- 5. AUDIT LOG TABLE (Enhanced)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_audit_logs')
BEGIN
    CREATE TABLE t_sys_admin_audit_logs (
        c_audit_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_adminid BIGINT NOT NULL,
        c_admin_name NVARCHAR(200),
        c_action NVARCHAR(200) NOT NULL,
        c_module NVARCHAR(50),  -- CATERING, USER, REVIEW, etc.
        c_target_id BIGINT,  -- ID of affected entity
        c_target_type NVARCHAR(50),  -- Entity type
        c_details NVARCHAR(MAX),  -- JSON or text details
        c_ip_address NVARCHAR(50),
        c_user_agent NVARCHAR(500),
        c_timestamp DATETIME DEFAULT GETDATE(),
        c_status NVARCHAR(20),  -- SUCCESS, FAILED, UNAUTHORIZED
        c_error_message NVARCHAR(MAX),

        INDEX IX_admin_id (c_adminid),
        INDEX IX_timestamp (c_timestamp DESC),
        INDEX IX_module (c_module),
        INDEX IX_action (c_action)
    );
END
GO

-- =====================================================
-- SEED DEFAULT PERMISSIONS
-- =====================================================

-- Catering Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CATERING_VIEW', 'View Caterings', 'View catering list and details', 'CATERING', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CATERING_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CATERING_VERIFY', 'Verify Caterings', 'Approve/reject catering applications', 'CATERING', 'VERIFY'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CATERING_VERIFY');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CATERING_BLOCK', 'Block Caterings', 'Block/unblock caterings', 'CATERING', 'BLOCK'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CATERING_BLOCK');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CATERING_EDIT', 'Edit Caterings', 'Edit catering information', 'CATERING', 'EDIT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CATERING_EDIT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CATERING_DELETE', 'Delete Caterings', 'Soft delete caterings', 'CATERING', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CATERING_DELETE');

-- User Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'USER_VIEW', 'View Users', 'View user list and details', 'USER', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'USER_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'USER_BLOCK', 'Block Users', 'Block/unblock user accounts', 'USER', 'BLOCK'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'USER_BLOCK');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'USER_EDIT', 'Edit Users', 'Edit user information', 'USER', 'EDIT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'USER_EDIT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'USER_DELETE', 'Delete Users', 'Delete user accounts', 'USER', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'USER_DELETE');

-- Review Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'REVIEW_VIEW', 'View Reviews', 'View all reviews', 'REVIEW', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'REVIEW_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'REVIEW_MODERATE', 'Moderate Reviews', 'Hide/unhide reviews', 'REVIEW', 'MODERATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'REVIEW_MODERATE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'REVIEW_DELETE', 'Delete Reviews', 'Permanently delete reviews', 'REVIEW', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'REVIEW_DELETE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'REVIEW_RESPOND', 'Respond to Reviews', 'Post official responses', 'REVIEW', 'RESPOND'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'REVIEW_RESPOND');

-- Finance Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EARNINGS_VIEW', 'View Earnings', 'View earnings dashboard', 'EARNINGS', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EARNINGS_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EARNINGS_EXPORT', 'Export Reports', 'Export financial reports', 'EARNINGS', 'EXPORT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EARNINGS_EXPORT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'PAYOUT_APPROVE', 'Approve Payouts', 'Approve payout requests', 'PAYOUT', 'APPROVE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'PAYOUT_APPROVE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'COMMISSION_CONFIGURE', 'Configure Commission', 'Set commission rates', 'COMMISSION', 'CONFIGURE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'COMMISSION_CONFIGURE');

-- Marketing Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'DISCOUNT_VIEW', 'View Discounts', 'View discount campaigns', 'DISCOUNT', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'DISCOUNT_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'DISCOUNT_CREATE', 'Create Discounts', 'Create new discounts', 'DISCOUNT', 'CREATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'DISCOUNT_CREATE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'DISCOUNT_EDIT', 'Edit Discounts', 'Modify existing discounts', 'DISCOUNT', 'EDIT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'DISCOUNT_EDIT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'DISCOUNT_DELETE', 'Delete Discounts', 'Remove discounts', 'DISCOUNT', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'DISCOUNT_DELETE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'BANNER_MANAGE', 'Manage Banners', 'Create/edit promotional banners', 'BANNER', 'MANAGE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'BANNER_MANAGE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'CAMPAIGN_CREATE', 'Create Campaigns', 'Launch marketing campaigns', 'CAMPAIGN', 'CREATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'CAMPAIGN_CREATE');

-- Event Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EVENT_VIEW', 'View Events', 'View events', 'EVENT', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EVENT_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EVENT_ASSIGN', 'Assign Events', 'Assign staff to events', 'EVENT', 'ASSIGN'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EVENT_ASSIGN');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EVENT_UPDATE', 'Update Events', 'Update event status', 'EVENT', 'UPDATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EVENT_UPDATE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'EVENT_NOTES', 'Event Notes', 'Add notes to events', 'EVENT', 'NOTES'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'EVENT_NOTES');

-- System Admin Permissions
INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ROLE_CREATE', 'Create Roles', 'Create new admin roles', 'SYSTEM', 'CREATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ROLE_CREATE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ROLE_EDIT', 'Edit Roles', 'Modify role definitions', 'SYSTEM', 'EDIT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ROLE_EDIT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ROLE_DELETE', 'Delete Roles', 'Remove roles', 'SYSTEM', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ROLE_DELETE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'PERMISSION_ASSIGN', 'Assign Permissions', 'Assign permissions to roles', 'SYSTEM', 'ASSIGN'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'PERMISSION_ASSIGN');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ADMIN_CREATE', 'Create Admins', 'Create admin users', 'ADMIN', 'CREATE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ADMIN_CREATE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ADMIN_EDIT', 'Edit Admins', 'Edit admin details', 'ADMIN', 'EDIT'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ADMIN_EDIT');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ADMIN_DELETE', 'Delete Admins', 'Remove admin users', 'ADMIN', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ADMIN_DELETE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'ADMIN_ASSIGN_ROLE', 'Assign Roles', 'Assign roles to admins', 'ADMIN', 'ASSIGN_ROLE'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'ADMIN_ASSIGN_ROLE');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'AUDIT_VIEW', 'View Audit Logs', 'Access audit trail', 'AUDIT', 'VIEW'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'AUDIT_VIEW');

INSERT INTO t_sys_admin_permissions (c_permission_code, c_permission_name, c_description, c_module, c_action)
SELECT 'SYSTEM_CONFIG', 'System Config', 'Configure system settings', 'SYSTEM', 'CONFIG'
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_permissions WHERE c_permission_code = 'SYSTEM_CONFIG');

GO

-- =====================================================
-- SEED DEFAULT ROLES
-- =====================================================

-- SUPER_ADMIN Role
DECLARE @SuperAdminRoleId BIGINT;

IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('SUPER_ADMIN', 'Super Administrator', 'Full system access - can manage roles, permissions, and all admins', '#dc2626', 1);

    SET @SuperAdminRoleId = SCOPE_IDENTITY();

    -- Assign ALL permissions to SUPER_ADMIN
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @SuperAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_is_active = 1;
END
ELSE
BEGIN
    SELECT @SuperAdminRoleId = c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN';
END

-- CATERING_ADMIN Role
DECLARE @CateringAdminRoleId BIGINT;

IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'CATERING_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('CATERING_ADMIN', 'Catering Manager', 'Manages catering providers, menus, and verification', '#6366f1', 0);

    SET @CateringAdminRoleId = SCOPE_IDENTITY();

    -- Assign catering permissions
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @CateringAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_permission_code IN ('CATERING_VIEW', 'CATERING_VERIFY', 'CATERING_BLOCK', 'CATERING_EDIT');
END

-- MARKETING_ADMIN Role
DECLARE @MarketingAdminRoleId BIGINT;

IF NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'MARKETING_ADMIN')
BEGIN
    INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
    VALUES ('MARKETING_ADMIN', 'Marketing Manager', 'Manages discounts, banners, and promotional campaigns', '#ec4899', 0);

    SET @MarketingAdminRoleId = SCOPE_IDENTITY();

    -- Assign marketing permissions
    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
    SELECT @MarketingAdminRoleId, c_permission_id
    FROM t_sys_admin_permissions
    WHERE c_permission_code IN ('DISCOUNT_VIEW', 'DISCOUNT_CREATE', 'DISCOUNT_EDIT', 'DISCOUNT_DELETE', 'BANNER_MANAGE', 'CAMPAIGN_CREATE');
END

GO

PRINT 'RBAC Schema and seed data created successfully!';
