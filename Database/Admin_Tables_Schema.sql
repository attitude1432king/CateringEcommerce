-- =============================================
-- Admin Module - Database Schema
-- PostgreSQL Compatible
-- Purpose: Admin authentication, activity logging, and management
-- =============================================

-- =============================================
-- 1. Admin Users Table
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_admin (
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
    c_is_temporary_password BOOLEAN NOT NULL DEFAULT FALSE
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

-- =============================================
-- 2. Admin Activity Log Table
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_admin_activity_log (
    c_logid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_adminid BIGINT NOT NULL,
    c_action VARCHAR(100) NOT NULL,
    c_details TEXT,
    c_ipaddress VARCHAR(50),
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_admin_activity_log_admin FOREIGN KEY (c_adminid)
        REFERENCES t_sys_admin(c_adminid)
);

-- Create index for better query performance
CREATE INDEX IF NOT EXISTS ix_admin_activity_log_adminid_date
    ON t_sys_admin_activity_log(c_adminid, c_createddate DESC);

-- =============================================
-- 3. Add missing columns to existing tables for admin features
-- =============================================
-- =============================================
-- 3. Default Admin User
-- Password: Admin@123 (Change this in production!)
-- Password Hash is SHA256 of "Admin@123"
-- =============================================
INSERT INTO t_sys_admin
(c_username, c_passwordhash, c_email, c_fullname, c_role, c_isactive)
VALUES
(
    'admin',
    'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=',
    'admin@cateringecommerce.com',
    'System Administrator',
    'Super Admin',
    TRUE
) ON CONFLICT (c_username) DO NOTHING;

-- =============================================
-- 4. Create Indexes for Performance
-- =============================================

-- Index on admin username for login
CREATE INDEX IF NOT EXISTS ix_admin_username
    ON t_sys_admin(c_username)
    INCLUDE (c_passwordhash, c_isactive, c_islocked);

-- Index on admin email
CREATE INDEX IF NOT EXISTS ix_admin_email
    ON t_sys_admin(c_email);

-- =============================================
-- 5. Sample Query Scripts
-- =============================================

-- Test the admin user
SELECT * FROM t_sys_admin;

-- Test the activity log
SELECT * FROM t_sys_admin_activity_log ORDER BY c_createddate DESC LIMIT 10;

-- =============================================
-- Admin module database schema setup completed
-- =============================================
-- Default Admin Credentials:
-- Username: admin
-- Password: Admin@123
-- ⚠️  IMPORTANT: Change the default password immediately in production!

