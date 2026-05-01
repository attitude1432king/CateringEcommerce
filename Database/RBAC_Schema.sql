-- =====================================================
-- RBAC (Role-Based Access Control) Database Schema
-- Enterprise-Grade Permission System
-- =====================================================

-- =====================================================
-- 1. ADMIN USERS TABLE
-- Compatibility table used by RBAC/admin management code paths
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_users (
    c_adminid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_username VARCHAR(50) NOT NULL UNIQUE,
    c_passwordhash VARCHAR(255) NOT NULL,
    c_email VARCHAR(100) NOT NULL UNIQUE,
    c_fullname VARCHAR(100) NOT NULL,
    c_role VARCHAR(50) NOT NULL DEFAULT 'System Admin',
    c_role_id BIGINT,
    c_mobile VARCHAR(20),
    c_profilephoto VARCHAR(500),
    c_force_password_reset BOOLEAN DEFAULT FALSE,
    c_is_temporary_password BOOLEAN NOT NULL DEFAULT FALSE,
    c_isactive BOOLEAN NOT NULL DEFAULT TRUE,
    c_failedloginattempts INTEGER NOT NULL DEFAULT 0,
    c_islocked BOOLEAN NOT NULL DEFAULT FALSE,
    c_lockeduntil TIMESTAMP,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_createdby BIGINT,
    c_lastlogin TIMESTAMP,
    c_lastmodified TIMESTAMP,
    c_modifiedby BIGINT
);

CREATE INDEX IF NOT EXISTS idx_admin_users_role_id
ON t_sys_admin_users (c_role_id);

CREATE INDEX IF NOT EXISTS idx_admin_users_isactive
ON t_sys_admin_users (c_isactive);

CREATE INDEX IF NOT EXISTS idx_admin_users_createddate
ON t_sys_admin_users (c_createddate DESC);

CREATE UNIQUE INDEX IF NOT EXISTS idx_admin_users_username
ON t_sys_admin_users (c_username);

CREATE UNIQUE INDEX IF NOT EXISTS idx_admin_users_email
ON t_sys_admin_users (c_email);

-- =====================================================
-- 2. ROLES TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_roles (
    c_role_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_role_code VARCHAR(50) NOT NULL UNIQUE,
    c_role_name VARCHAR(100) NOT NULL,
    c_description VARCHAR(500),
    c_color VARCHAR(20) DEFAULT '#6366f1',
    c_is_system_role BOOLEAN DEFAULT FALSE,
    c_is_active BOOLEAN DEFAULT TRUE,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_createdby BIGINT,
    c_modifieddate TIMESTAMP,
    c_updated_by BIGINT
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_admin_roles_code
ON t_sys_admin_roles (c_role_code);

CREATE INDEX IF NOT EXISTS idx_admin_roles_active
ON t_sys_admin_roles (c_is_active);

-- =====================================================
-- 3. PERMISSIONS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_permissions (
    c_permission_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_permission_code VARCHAR(50) NOT NULL UNIQUE,
    c_permission_name VARCHAR(100) NOT NULL,
    c_description VARCHAR(500),
    c_module VARCHAR(50),  -- CATERING, USER, REVIEW, EARNINGS, etc.
    c_action VARCHAR(50),  -- VIEW, CREATE, EDIT, DELETE, BLOCK, etc.
    c_is_active BOOLEAN DEFAULT TRUE,
    c_createddate TIMESTAMP DEFAULT NOW()
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_permissions_module
ON t_sys_admin_permissions (c_module);

CREATE INDEX IF NOT EXISTS idx_permissions_active
ON t_sys_admin_permissions (c_is_active);

-- =====================================================
-- 4. ROLE-PERMISSION JUNCTION TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_role_permissions (
    c_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_role_id BIGINT NOT NULL,
    c_permission_id BIGINT NOT NULL,
    c_assigned_date TIMESTAMP DEFAULT NOW(),
    c_assigned_by BIGINT,

    CONSTRAINT fk_role_permissions_role 
        FOREIGN KEY (c_role_id) 
        REFERENCES t_sys_admin_roles(c_role_id) ON DELETE CASCADE,

    CONSTRAINT fk_role_permissions_permission 
        FOREIGN KEY (c_permission_id) 
        REFERENCES t_sys_admin_permissions(c_permission_id) ON DELETE CASCADE,

    CONSTRAINT uq_role_permission UNIQUE (c_role_id, c_permission_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_role_permissions_role
ON t_sys_admin_role_permissions (c_role_id);

CREATE INDEX IF NOT EXISTS idx_role_permissions_permission
ON t_sys_admin_role_permissions (c_permission_id);

-- =====================================================
-- 5. ADMIN USER-ROLE JUNCTION TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_user_roles (
    c_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_adminid BIGINT NOT NULL,
    c_role_id BIGINT NOT NULL,
    c_assigned_date TIMESTAMP DEFAULT NOW(),
    c_assigned_by BIGINT,
    c_is_active BOOLEAN DEFAULT TRUE,

    CONSTRAINT fk_user_roles_admin 
        FOREIGN KEY (c_adminid) 
        REFERENCES t_sys_admin_users(c_adminid) ON DELETE CASCADE,

    CONSTRAINT fk_user_roles_role 
        FOREIGN KEY (c_role_id) 
        REFERENCES t_sys_admin_roles(c_role_id) ON DELETE CASCADE,

    CONSTRAINT uq_admin_role UNIQUE (c_adminid, c_role_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_user_roles_admin
ON t_sys_admin_user_roles (c_adminid);

CREATE INDEX IF NOT EXISTS idx_user_roles_role
ON t_sys_admin_user_roles (c_role_id);

CREATE INDEX IF NOT EXISTS idx_user_roles_active
ON t_sys_admin_user_roles (c_is_active);

-- =====================================================
-- 6. AUDIT LOG TABLE (Enhanced)
-- =====================================================
CREATE TABLE IF NOT EXISTS t_sys_admin_audit_logs (
    c_audit_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_adminid BIGINT NOT NULL,
    c_admin_name VARCHAR(200),
    c_action VARCHAR(200) NOT NULL,
    c_module VARCHAR(50),  -- CATERING, USER, REVIEW, etc.
    c_target_id BIGINT,
    c_target_type VARCHAR(50),
    c_details TEXT,
    c_ip_address VARCHAR(50),
    c_user_agent VARCHAR(500),
    c_timestamp TIMESTAMP DEFAULT NOW(),
    c_status VARCHAR(20),
    c_error_message TEXT
);

-- Indexes (separate statements)
CREATE INDEX IF NOT EXISTS idx_admin_audit_adminid
ON t_sys_admin_audit_logs (c_adminid);

CREATE INDEX IF NOT EXISTS idx_admin_audit_timestamp
ON t_sys_admin_audit_logs (c_timestamp DESC);

CREATE INDEX IF NOT EXISTS idx_admin_audit_module
ON t_sys_admin_audit_logs (c_module);

CREATE INDEX IF NOT EXISTS idx_admin_audit_action
ON t_sys_admin_audit_logs (c_action);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE table_name = 't_sys_admin_users'
          AND constraint_name = 'fk_admin_users_role'
    ) THEN
        ALTER TABLE t_sys_admin_users
        ADD CONSTRAINT fk_admin_users_role
            FOREIGN KEY (c_role_id)
            REFERENCES t_sys_admin_roles(c_role_id);
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 't_sys_admin'
    ) THEN
        INSERT INTO t_sys_admin_users (
            c_adminid,
            c_username,
            c_passwordhash,
            c_email,
            c_fullname,
            c_role,
            c_role_id,
            c_mobile,
            c_profilephoto,
            c_force_password_reset,
            c_is_temporary_password,
            c_isactive,
            c_failedloginattempts,
            c_islocked,
            c_lockeduntil,
            c_createddate,
            c_createdby,
            c_lastlogin,
            c_lastmodified,
            c_modifiedby
        )
        OVERRIDING SYSTEM VALUE
        SELECT
            a.c_adminid,
            a.c_username,
            a.c_passwordhash,
            a.c_email,
            a.c_fullname,
            a.c_role,
            a.c_role_id,
            a.c_mobile,
            a.c_profilephoto,
            COALESCE(a.c_force_password_reset, FALSE),
            COALESCE(a.c_is_temporary_password, FALSE),
            COALESCE(a.c_isactive, TRUE),
            COALESCE(a.c_failedloginattempts, 0),
            COALESCE(a.c_islocked, FALSE),
            a.c_lockeduntil,
            COALESCE(a.c_createddate, NOW()),
            a.c_createdby,
            a.c_lastlogin,
            a.c_lastmodified,
            a.c_modifiedby
        FROM t_sys_admin a
        WHERE NOT EXISTS (
            SELECT 1
            FROM t_sys_admin_users u
            WHERE u.c_adminid = a.c_adminid
        );

        PERFORM setval(
            pg_get_serial_sequence('t_sys_admin_users', 'c_adminid'),
            GREATEST(COALESCE((SELECT MAX(c_adminid) FROM t_sys_admin_users), 0), 1),
            TRUE
        );
    END IF;
END $$;

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

-- =====================================================
-- SEED DEFAULT ROLES
-- =====================================================

-- SUPER_ADMIN Role
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

-- CATERING_ADMIN Role
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

-- MARKETING_ADMIN Role
INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role)
SELECT 'MARKETING_ADMIN', 'Marketing Manager', 'Manages discounts, banners, and promotional campaigns', '#ec4899', FALSE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_admin_roles WHERE c_role_code = 'MARKETING_ADMIN');

INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id)
SELECT
    (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'MARKETING_ADMIN'),
    c_permission_id
FROM t_sys_admin_permissions
WHERE c_permission_code IN ('DISCOUNT_VIEW', 'DISCOUNT_CREATE', 'DISCOUNT_EDIT', 'DISCOUNT_DELETE', 'BANNER_MANAGE', 'CAMPAIGN_CREATE')
    AND NOT EXISTS (
        SELECT 1 FROM t_sys_admin_role_permissions rp
        WHERE rp.c_role_id = (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'MARKETING_ADMIN')
        AND rp.c_permission_id = t_sys_admin_permissions.c_permission_id
    );

