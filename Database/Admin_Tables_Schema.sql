-- =============================================
-- Admin Module - Database Schema
-- Purpose: Admin authentication, activity logging, and management
-- =============================================

USE [CateringDB];
GO

-- =============================================
-- 1. Admin Users Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin')
BEGIN
    CREATE TABLE [dbo].[t_sys_admin]
    (
        [c_adminid] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_username] NVARCHAR(50) NOT NULL UNIQUE,
        [c_passwordhash] NVARCHAR(255) NOT NULL,
        [c_email] NVARCHAR(100) NOT NULL UNIQUE,
        [c_fullname] NVARCHAR(100) NOT NULL,
        [c_role] NVARCHAR(50) NOT NULL DEFAULT 'System Admin', -- 'System Admin' or 'Super Admin'
        [c_profilephoto] NVARCHAR(500) NULL,
        [c_isactive] BIT NOT NULL DEFAULT 1,
        [c_failedloginattempts] INT NOT NULL DEFAULT 0,
        [c_islocked] BIT NOT NULL DEFAULT 0,
        [c_lockeduntil] DATETIME NULL,
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_createdby] BIGINT NULL,
        [c_lastlogin] DATETIME NULL,
        [c_lastmodified] DATETIME NULL,
        [c_modifiedby] BIGINT NULL
    );

    PRINT 'Table t_sys_admin created successfully.';
END
ELSE
BEGIN
    PRINT 'Table t_sys_admin already exists.';
END
GO

-- =============================================
-- 2. Admin Activity Log Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_admin_activity_log')
BEGIN
    CREATE TABLE [dbo].[t_sys_admin_activity_log]
    (
        [c_logid] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_adminid] BIGINT NOT NULL,
        [c_action] NVARCHAR(100) NOT NULL, -- LOGIN, LOGOUT, UPDATE_CATERING_STATUS, DELETE_REVIEW, etc.
        [c_details] NVARCHAR(MAX) NULL,
        [c_ipaddress] NVARCHAR(50) NULL,
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_AdminActivityLog_Admin FOREIGN KEY ([c_adminid])
            REFERENCES [dbo].[t_sys_admin]([c_adminid])
    );

    -- Create index for better query performance
    CREATE NONCLUSTERED INDEX IX_AdminActivityLog_AdminId_Date
        ON [dbo].[t_sys_admin_activity_log]([c_adminid], [c_createddate] DESC);

    PRINT 'Table t_sys_admin_activity_log created successfully.';
END
ELSE
BEGIN
    PRINT 'Table t_sys_admin_activity_log already exists.';
END
GO

-- =============================================
-- 3. Add missing columns to existing tables for admin features
-- =============================================

-- Add columns to t_sys_catering_owner for admin management
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_approved_date')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_approved_date] DATETIME NULL;
    PRINT 'Column c_approved_date added to t_sys_catering_owner.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_isblocked')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_isblocked] BIT NOT NULL DEFAULT 0;
    PRINT 'Column c_isblocked added to t_sys_catering_owner.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_block_reason')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_block_reason] NVARCHAR(500) NULL;
    PRINT 'Column c_block_reason added to t_sys_catering_owner.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_isdeleted')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_isdeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Column c_isdeleted added to t_sys_catering_owner.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_deleted_by')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_deleted_by] BIGINT NULL;
    PRINT 'Column c_deleted_by added to t_sys_catering_owner.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_deleted_date')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_owner]
    ADD [c_deleted_date] DATETIME NULL;
    PRINT 'Column c_deleted_date added to t_sys_catering_owner.';
END
GO

-- Add columns to t_sys_user for admin management
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_user') AND name = 'c_isblocked')
BEGIN
    ALTER TABLE [dbo].[t_sys_user]
    ADD [c_isblocked] BIT NOT NULL DEFAULT 0;
    PRINT 'Column c_isblocked added to t_sys_user.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_user') AND name = 'c_block_reason')
BEGIN
    ALTER TABLE [dbo].[t_sys_user]
    ADD [c_block_reason] NVARCHAR(500) NULL;
    PRINT 'Column c_block_reason added to t_sys_user.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_user') AND name = 'c_last_login')
BEGIN
    ALTER TABLE [dbo].[t_sys_user]
    ADD [c_last_login] DATETIME NULL;
    PRINT 'Column c_last_login added to t_sys_user.';
END
GO

-- Add columns to t_sys_catering_review for moderation
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_ishidden')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_ishidden] BIT NOT NULL DEFAULT 0;
    PRINT 'Column c_ishidden added to t_sys_catering_review.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_reason')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_reason] NVARCHAR(500) NULL;
    PRINT 'Column c_hidden_reason added to t_sys_catering_review.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_by')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_by] BIGINT NULL;
    PRINT 'Column c_hidden_by added to t_sys_catering_review.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_date')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_date] DATETIME NULL;
    PRINT 'Column c_hidden_date added to t_sys_catering_review.';
END
GO

-- Add commission tracking columns to orders table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_orders') AND name = 'c_platform_commission')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_platform_commission] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column c_platform_commission added to t_sys_orders.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_orders') AND name = 'c_commission_rate')
BEGIN
    ALTER TABLE [dbo].[t_sys_orders]
    ADD [c_commission_rate] DECIMAL(5,2) NOT NULL DEFAULT 10.00; -- Default 10% commission
    PRINT 'Column c_commission_rate added to t_sys_orders.';
END
GO

-- =============================================
-- 4. Insert Default Admin User
-- Password: Admin@123 (Change this in production!)
-- Password Hash is SHA256 of "Admin@123"
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[t_sys_admin] WHERE c_username = 'admin')
BEGIN
    INSERT INTO [dbo].[t_sys_admin]
    (c_username, c_passwordhash, c_email, c_fullname, c_role, c_isactive)
    VALUES
    ('admin',
     'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- SHA256 hash of "Admin@123"
     'admin@cateringecommerce.com',
     'System Administrator',
     'Super Admin',
     1);

    PRINT 'Default admin user created successfully.';
    PRINT 'Username: admin';
    PRINT 'Password: Admin@123 (CHANGE THIS IMMEDIATELY IN PRODUCTION!)';
END
ELSE
BEGIN
    PRINT 'Admin user already exists.';
END
GO

-- =============================================
-- 5. Create Indexes for Performance
-- =============================================

-- Index on admin username for login
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Admin_Username')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Admin_Username
        ON [dbo].[t_sys_admin]([c_username])
        INCLUDE ([c_passwordhash], [c_isactive], [c_islocked]);
    PRINT 'Index IX_Admin_Username created.';
END
GO

-- =============================================
-- 6. Sample Query Scripts
-- =============================================

-- Test the admin user
SELECT * FROM [dbo].[t_sys_admin];

-- Test the activity log
SELECT TOP 10 * FROM [dbo].[t_sys_admin_activity_log] ORDER BY c_createddate DESC;

PRINT '============================================='
PRINT 'Admin module database schema setup completed!'
PRINT '============================================='
PRINT ''
PRINT 'Default Admin Credentials:'
PRINT 'Username: admin'
PRINT 'Password: Admin@123'
PRINT ''
PRINT '⚠️ IMPORTANT: Change the default password immediately in production!'
PRINT '============================================='
GO
