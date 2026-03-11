-- =============================================
-- Two-Factor Authentication & OAuth Integration
-- Database Schema
-- Created: February 4, 2026
-- =============================================

USE [CateringDB]
GO

PRINT 'Creating 2FA and OAuth tables...';
GO

-- =============================================
-- 1. USER TWO-FACTOR AUTHENTICATION
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_user_2fa')
BEGIN
    CREATE TABLE t_sys_user_2fa (
        c_2fa_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_userid BIGINT NOT NULL,

        -- 2FA Configuration
        c_is_enabled BIT NOT NULL DEFAULT 0,
        c_secret_key NVARCHAR(100) NOT NULL, -- Base32 encoded TOTP secret
        c_method VARCHAR(20) NOT NULL DEFAULT 'TOTP', -- TOTP, SMS, EMAIL

        -- Setup
        c_setup_completed BIT NOT NULL DEFAULT 0,
        c_setup_date DATETIME NULL,
        c_verified_date DATETIME NULL,

        -- Backup Codes (JSON array of hashed codes)
        c_backup_codes NVARCHAR(MAX) NULL,
        c_backup_codes_used INT NOT NULL DEFAULT 0,

        -- Recovery
        c_recovery_email NVARCHAR(255) NULL,
        c_recovery_phone NVARCHAR(20) NULL,

        -- Security
        c_failed_attempts INT NOT NULL DEFAULT 0,
        c_locked_until DATETIME NULL,
        c_last_verified DATETIME NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        CONSTRAINT FK_user_2fa_user FOREIGN KEY (c_userid)
            REFERENCES t_sys_user(c_userid) ON DELETE CASCADE,

        INDEX idx_user (c_userid),
        INDEX idx_enabled (c_is_enabled)
    );
    PRINT '✓ Created table: t_sys_user_2fa';
END
ELSE
    PRINT '  Table t_sys_user_2fa already exists';
GO

-- =============================================
-- 2. OWNER/PARTNER TWO-FACTOR AUTHENTICATION
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_owner_2fa')
BEGIN
    CREATE TABLE t_sys_owner_2fa (
        c_2fa_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_ownerid BIGINT NOT NULL,

        -- 2FA Configuration
        c_is_enabled BIT NOT NULL DEFAULT 0,
        c_secret_key NVARCHAR(100) NOT NULL, -- Base32 encoded TOTP secret
        c_method VARCHAR(20) NOT NULL DEFAULT 'TOTP', -- TOTP, SMS, EMAIL

        -- Setup
        c_setup_completed BIT NOT NULL DEFAULT 0,
        c_setup_date DATETIME NULL,
        c_verified_date DATETIME NULL,

        -- Backup Codes (JSON array of hashed codes)
        c_backup_codes NVARCHAR(MAX) NULL,
        c_backup_codes_used INT NOT NULL DEFAULT 0,

        -- Recovery
        c_recovery_email NVARCHAR(255) NULL,
        c_recovery_phone NVARCHAR(20) NULL,

        -- Security
        c_failed_attempts INT NOT NULL DEFAULT 0,
        c_locked_until DATETIME NULL,
        c_last_verified DATETIME NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        CONSTRAINT FK_owner_2fa_owner FOREIGN KEY (c_ownerid)
            REFERENCES t_sys_catering_owner(c_ownerid) ON DELETE CASCADE,

        INDEX idx_owner (c_ownerid),
        INDEX idx_enabled (c_is_enabled)
    );
    PRINT '✓ Created table: t_sys_owner_2fa';
END
ELSE
    PRINT '  Table t_sys_owner_2fa already exists';
GO

-- =============================================
-- 3. OAUTH PROVIDERS (USER ONLY)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_oauth_provider')
BEGIN
    CREATE TABLE t_sys_oauth_provider (
        c_provider_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_provider_name VARCHAR(50) NOT NULL UNIQUE, -- GOOGLE, FACEBOOK, APPLE
        c_client_id NVARCHAR(255) NOT NULL,
        c_client_secret NVARCHAR(500) NOT NULL, -- Encrypted
        c_redirect_uri NVARCHAR(500) NOT NULL,
        c_authorization_endpoint NVARCHAR(500) NOT NULL,
        c_token_endpoint NVARCHAR(500) NOT NULL,
        c_user_info_endpoint NVARCHAR(500) NOT NULL,
        c_scope NVARCHAR(500) NOT NULL,

        c_is_active BIT NOT NULL DEFAULT 1,
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        INDEX idx_provider_name (c_provider_name),
        INDEX idx_active (c_is_active)
    );
    PRINT '✓ Created table: t_sys_oauth_provider';
END
ELSE
    PRINT '  Table t_sys_oauth_provider already exists';
GO

-- =============================================
-- 4. OAUTH USER CONNECTIONS
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_user_oauth')
BEGIN
    CREATE TABLE t_sys_user_oauth (
        c_oauth_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_userid BIGINT NOT NULL,
        c_provider_id BIGINT NOT NULL,

        -- OAuth Data
        c_provider_user_id NVARCHAR(255) NOT NULL, -- User ID from OAuth provider
        c_provider_email NVARCHAR(255) NULL,
        c_provider_name NVARCHAR(255) NULL,
        c_provider_picture NVARCHAR(500) NULL,

        -- Tokens (Encrypted)
        c_access_token NVARCHAR(MAX) NULL,
        c_refresh_token NVARCHAR(MAX) NULL,
        c_token_expires_at DATETIME NULL,

        -- Linking
        c_is_primary BIT NOT NULL DEFAULT 0, -- Is this the primary login method?
        c_linked_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_last_login DATETIME NULL,

        -- Audit
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        CONSTRAINT FK_user_oauth_user FOREIGN KEY (c_userid)
            REFERENCES t_sys_user(c_userid) ON DELETE CASCADE,
        CONSTRAINT FK_user_oauth_provider FOREIGN KEY (c_provider_id)
            REFERENCES t_sys_oauth_provider(c_provider_id),

        -- One OAuth account can link to only one user
        CONSTRAINT UQ_provider_user UNIQUE (c_provider_id, c_provider_user_id),

        INDEX idx_user (c_userid),
        INDEX idx_provider (c_provider_id),
        INDEX idx_provider_user_id (c_provider_user_id),
        INDEX idx_primary (c_is_primary)
    );
    PRINT '✓ Created table: t_sys_user_oauth';
END
ELSE
    PRINT '  Table t_sys_user_oauth already exists';
GO

-- =============================================
-- 5. 2FA VERIFICATION ATTEMPTS LOG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_2fa_attempt_log')
BEGIN
    CREATE TABLE t_sys_2fa_attempt_log (
        c_log_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_user_type VARCHAR(20) NOT NULL, -- USER, OWNER
        c_userid BIGINT NOT NULL,

        -- Attempt Details
        c_code_entered NVARCHAR(10) NULL, -- Do NOT store actual code, just length/pattern
        c_method_used VARCHAR(20) NOT NULL, -- TOTP, BACKUP_CODE, SMS
        c_is_successful BIT NOT NULL,

        -- Context
        c_ip_address VARCHAR(50) NULL,
        c_user_agent NVARCHAR(500) NULL,
        c_device_info NVARCHAR(500) NULL,

        -- Result
        c_failure_reason NVARCHAR(255) NULL,
        c_attempt_date DATETIME NOT NULL DEFAULT GETDATE(),

        INDEX idx_user_type_id (c_user_type, c_userid),
        INDEX idx_attempt_date (c_attempt_date),
        INDEX idx_successful (c_is_successful)
    );
    PRINT '✓ Created table: t_sys_2fa_attempt_log';
END
ELSE
    PRINT '  Table t_sys_2fa_attempt_log already exists';
GO

-- =============================================
-- 6. TRUSTED DEVICES (OPTIONAL - "Remember this device")
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_trusted_device')
BEGIN
    CREATE TABLE t_sys_trusted_device (
        c_device_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_user_type VARCHAR(20) NOT NULL, -- USER, OWNER
        c_userid BIGINT NOT NULL,

        -- Device Identification
        c_device_token NVARCHAR(100) NOT NULL UNIQUE, -- Random token to identify device
        c_device_name NVARCHAR(255) NULL, -- "Chrome on Windows"
        c_device_fingerprint NVARCHAR(500) NULL, -- Browser fingerprint

        -- Device Info
        c_ip_address VARCHAR(50) NULL,
        c_user_agent NVARCHAR(500) NULL,
        c_browser NVARCHAR(100) NULL,
        c_os NVARCHAR(100) NULL,

        -- Trust Status
        c_is_active BIT NOT NULL DEFAULT 1,
        c_trusted_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_expires_at DATETIME NOT NULL, -- 30 days from trust date
        c_last_used DATETIME NULL,

        -- Revocation
        c_revoked_date DATETIME NULL,
        c_revoked_reason NVARCHAR(255) NULL,

        INDEX idx_user_type_id (c_user_type, c_userid),
        INDEX idx_device_token (c_device_token),
        INDEX idx_active (c_is_active),
        INDEX idx_expires (c_expires_at)
    );
    PRINT '✓ Created table: t_sys_trusted_device';
END
ELSE
    PRINT '  Table t_sys_trusted_device already exists';
GO

-- =============================================
-- 7. OAUTH STATE TOKENS (CSRF Protection)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_oauth_state')
BEGIN
    CREATE TABLE t_sys_oauth_state (
        c_state_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_state_token NVARCHAR(100) NOT NULL UNIQUE,
        c_provider_name VARCHAR(50) NOT NULL,

        -- State Data
        c_redirect_url NVARCHAR(500) NULL, -- Where to redirect after OAuth
        c_additional_data NVARCHAR(MAX) NULL, -- JSON for any extra data

        -- Security
        c_ip_address VARCHAR(50) NULL,
        c_user_agent NVARCHAR(500) NULL,

        -- Expiry
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_expires_at DATETIME NOT NULL, -- 10 minutes from creation
        c_used BIT NOT NULL DEFAULT 0,
        c_used_date DATETIME NULL,

        INDEX idx_state_token (c_state_token),
        INDEX idx_expires (c_expires_at),
        INDEX idx_used (c_used)
    );
    PRINT '✓ Created table: t_sys_oauth_state';
END
ELSE
    PRINT '  Table t_sys_oauth_state already exists';
GO

-- =============================================
-- INSERT DEFAULT OAUTH PROVIDERS
-- =============================================

-- Note: Client IDs and secrets should be configured via admin panel or environment variables
-- These are placeholder values

IF NOT EXISTS (SELECT * FROM t_sys_oauth_provider WHERE c_provider_name = 'GOOGLE')
BEGIN
    INSERT INTO t_sys_oauth_provider (
        c_provider_name,
        c_client_id,
        c_client_secret,
        c_redirect_uri,
        c_authorization_endpoint,
        c_token_endpoint,
        c_user_info_endpoint,
        c_scope
    )
    VALUES (
        'GOOGLE',
        'YOUR_GOOGLE_CLIENT_ID', -- Replace with actual client ID
        'YOUR_GOOGLE_CLIENT_SECRET', -- Replace with actual client secret
        'https://yourdomain.com/api/auth/google/callback',
        'https://accounts.google.com/o/oauth2/v2/auth',
        'https://oauth2.googleapis.com/token',
        'https://www.googleapis.com/oauth2/v2/userinfo',
        'openid email profile'
    );
    PRINT '✓ Inserted Google OAuth provider (configure client ID/secret)';
END

IF NOT EXISTS (SELECT * FROM t_sys_oauth_provider WHERE c_provider_name = 'FACEBOOK')
BEGIN
    INSERT INTO t_sys_oauth_provider (
        c_provider_name,
        c_client_id,
        c_client_secret,
        c_redirect_uri,
        c_authorization_endpoint,
        c_token_endpoint,
        c_user_info_endpoint,
        c_scope
    )
    VALUES (
        'FACEBOOK',
        'YOUR_FACEBOOK_APP_ID', -- Replace with actual app ID
        'YOUR_FACEBOOK_APP_SECRET', -- Replace with actual app secret
        'https://yourdomain.com/api/auth/facebook/callback',
        'https://www.facebook.com/v18.0/dialog/oauth',
        'https://graph.facebook.com/v18.0/oauth/access_token',
        'https://graph.facebook.com/me?fields=id,name,email,picture',
        'email public_profile'
    );
    PRINT '✓ Inserted Facebook OAuth provider (configure app ID/secret)';
END

GO

-- =============================================
-- CLEANUP JOB: Remove expired OAuth states and trusted devices
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CleanupExpiredSecurityTokens')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_CleanupExpiredSecurityTokens
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @DeletedStates INT, @DeletedDevices INT;

        -- Delete expired OAuth states
        DELETE FROM t_sys_oauth_state
        WHERE c_expires_at < GETDATE() OR (c_used = 1 AND c_used_date < DATEADD(HOUR, -1, GETDATE()));
        SET @DeletedStates = @@ROWCOUNT;

        -- Delete expired trusted devices
        DELETE FROM t_sys_trusted_device
        WHERE c_expires_at < GETDATE() OR c_is_active = 0;
        SET @DeletedDevices = @@ROWCOUNT;

        SELECT
            @DeletedStates AS DeletedOAuthStates,
            @DeletedDevices AS DeletedTrustedDevices;
    END
    ');
    PRINT '✓ Created stored procedure: sp_CleanupExpiredSecurityTokens';
END
GO

-- =============================================
-- SUMMARY
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Security Enhancement Tables Created:';
PRINT '========================================';
PRINT '1. t_sys_user_2fa - User 2FA settings';
PRINT '2. t_sys_owner_2fa - Partner 2FA settings';
PRINT '3. t_sys_oauth_provider - OAuth provider configs';
PRINT '4. t_sys_user_oauth - User OAuth connections';
PRINT '5. t_sys_2fa_attempt_log - 2FA verification logs';
PRINT '6. t_sys_trusted_device - Trusted device tracking';
PRINT '7. t_sys_oauth_state - OAuth CSRF protection';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Update OAuth provider credentials in t_sys_oauth_provider';
PRINT '2. Implement 2FA in application layer';
PRINT '3. Configure OAuth callbacks in application';
PRINT '4. Schedule sp_CleanupExpiredSecurityTokens as a background job';
PRINT '';
GO
