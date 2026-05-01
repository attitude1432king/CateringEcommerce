-- =============================================
-- Two-Factor Authentication & OAuth (PostgreSQL)
-- =============================================

-- =============================================
-- 1. USER 2FA
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_user_2fa (
    c_2fa_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,

    c_is_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    c_secret_key VARCHAR(100) NOT NULL,
    c_method VARCHAR(20) NOT NULL DEFAULT 'TOTP',

    c_setup_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_setup_date TIMESTAMP,
    c_verified_date TIMESTAMP,

    c_backup_codes TEXT,
    c_backup_codes_used INT NOT NULL DEFAULT 0,

    c_recovery_email VARCHAR(255),
    c_recovery_phone VARCHAR(20),

    c_failed_attempts INT NOT NULL DEFAULT 0,
    c_locked_until TIMESTAMP,
    c_last_verified TIMESTAMP,

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_user_2fa_user FOREIGN KEY (c_userid)
        REFERENCES t_sys_user(c_userid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_user_2fa_userid ON t_sys_user_2fa(c_userid);
CREATE INDEX IF NOT EXISTS idx_user_2fa_enabled ON t_sys_user_2fa(c_is_enabled);

-- =============================================
-- 2. OWNER 2FA
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_owner_2fa (
    c_2fa_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    c_is_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    c_secret_key VARCHAR(100) NOT NULL,
    c_method VARCHAR(20) NOT NULL DEFAULT 'TOTP',

    c_setup_completed BOOLEAN NOT NULL DEFAULT FALSE,
    c_setup_date TIMESTAMP,
    c_verified_date TIMESTAMP,

    c_backup_codes TEXT,
    c_backup_codes_used INT NOT NULL DEFAULT 0,

    c_recovery_email VARCHAR(255),
    c_recovery_phone VARCHAR(20),

    c_failed_attempts INT NOT NULL DEFAULT 0,
    c_locked_until TIMESTAMP,
    c_last_verified TIMESTAMP,

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_owner_2fa_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_owner_2fa_ownerid ON t_sys_owner_2fa(c_ownerid);
CREATE INDEX IF NOT EXISTS idx_owner_2fa_enabled ON t_sys_owner_2fa(c_is_enabled);

-- =============================================
-- 3. OAUTH PROVIDERS
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_oauth_provider (
    c_provider_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_provider_name VARCHAR(50) NOT NULL UNIQUE,
    c_client_id VARCHAR(255) NOT NULL,
    c_client_secret VARCHAR(500) NOT NULL,
    c_redirect_uri VARCHAR(500) NOT NULL,
    c_authorization_endpoint VARCHAR(500) NOT NULL,
    c_token_endpoint VARCHAR(500) NOT NULL,
    c_user_info_endpoint VARCHAR(500) NOT NULL,
    c_scope VARCHAR(500) NOT NULL,

    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_oauth_provider_name ON t_sys_oauth_provider(c_provider_name);
CREATE INDEX IF NOT EXISTS idx_oauth_active ON t_sys_oauth_provider(c_is_active);

-- =============================================
-- 4. USER OAUTH
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_user_oauth (
    c_oauth_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_provider_id BIGINT NOT NULL,

    c_provider_user_id VARCHAR(255) NOT NULL,
    c_provider_email VARCHAR(255),
    c_provider_name VARCHAR(255),
    c_provider_picture VARCHAR(500),

    c_access_token TEXT,
    c_refresh_token TEXT,
    c_token_expires_at TIMESTAMP,

    c_is_primary BOOLEAN NOT NULL DEFAULT FALSE,
    c_linked_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_last_login TIMESTAMP,

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_user_oauth_user FOREIGN KEY (c_userid)
        REFERENCES t_sys_user(c_userid) ON DELETE CASCADE,

    CONSTRAINT fk_user_oauth_provider FOREIGN KEY (c_provider_id)
        REFERENCES t_sys_oauth_provider(c_provider_id),

    CONSTRAINT uq_provider_user UNIQUE (c_provider_id, c_provider_user_id)
);

CREATE INDEX IF NOT EXISTS idx_user_oauth_userid ON t_sys_user_oauth(c_userid);
CREATE INDEX IF NOT EXISTS idx_user_oauth_provider ON t_sys_user_oauth(c_provider_id);

-- =============================================
-- 5. 2FA ATTEMPT LOG
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_2fa_attempt_log (
    c_log_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_user_type VARCHAR(20) NOT NULL,
    c_userid BIGINT NOT NULL,

    c_code_entered VARCHAR(10),
    c_method_used VARCHAR(20) NOT NULL,
    c_is_successful BOOLEAN NOT NULL,

    c_ip_address VARCHAR(50),
    c_user_agent VARCHAR(500),
    c_device_info VARCHAR(500),

    c_failure_reason VARCHAR(255),
    c_attempt_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_2fa_log_user ON t_sys_2fa_attempt_log(c_user_type, c_userid);
CREATE INDEX IF NOT EXISTS idx_2fa_log_date ON t_sys_2fa_attempt_log(c_attempt_date);

-- =============================================
-- 6. TRUSTED DEVICES
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_trusted_device (
    c_device_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_user_type VARCHAR(20) NOT NULL,
    c_userid BIGINT NOT NULL,

    c_device_token VARCHAR(100) NOT NULL UNIQUE,
    c_device_name VARCHAR(255),
    c_device_fingerprint VARCHAR(500),

    c_ip_address VARCHAR(50),
    c_user_agent VARCHAR(500),
    c_browser VARCHAR(100),
    c_os VARCHAR(100),

    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_trusted_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_expires_at TIMESTAMP NOT NULL,
    c_last_used TIMESTAMP,

    c_revoked_date TIMESTAMP,
    c_revoked_reason VARCHAR(255)
);

CREATE INDEX IF NOT EXISTS idx_trusted_device_user ON t_sys_trusted_device(c_user_type, c_userid);

-- =============================================
-- 7. OAUTH STATE
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_oauth_state (
    c_state_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_state_token VARCHAR(100) NOT NULL UNIQUE,
    c_provider_name VARCHAR(50) NOT NULL,

    c_redirect_url VARCHAR(500),
    c_additional_data TEXT,

    c_ip_address VARCHAR(50),
    c_user_agent VARCHAR(500),

    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_expires_at TIMESTAMP NOT NULL,
    c_used BOOLEAN NOT NULL DEFAULT FALSE,
    c_used_date TIMESTAMP
);

-- =============================================
-- 8. DEFAULT OAUTH PROVIDERS
-- =============================================
INSERT INTO t_sys_oauth_provider (
    c_provider_name, c_client_id, c_client_secret,
    c_redirect_uri, c_authorization_endpoint,
    c_token_endpoint, c_user_info_endpoint, c_scope
)
VALUES
('GOOGLE','YOUR_GOOGLE_CLIENT_ID','YOUR_GOOGLE_CLIENT_SECRET',
 'https://yourdomain.com/api/auth/google/callback',
 'https://accounts.google.com/o/oauth2/v2/auth',
 'https://oauth2.googleapis.com/token',
 'https://www.googleapis.com/oauth2/v2/userinfo',
 'openid email profile')

ON CONFLICT (c_provider_name) DO NOTHING;

-- =============================================
-- 9. CLEANUP FUNCTION (Converted from PROCEDURE)
-- =============================================
CREATE OR REPLACE FUNCTION sp_CleanupExpiredSecurityTokens()
RETURNS TABLE(deleted_states INT, deleted_devices INT)
LANGUAGE plpgsql
AS $$
BEGIN

    DELETE FROM t_sys_oauth_state
    WHERE c_expires_at < CURRENT_TIMESTAMP
       OR (c_used = TRUE AND c_used_date < CURRENT_TIMESTAMP - INTERVAL '1 hour');

    GET DIAGNOSTICS deleted_states = ROW_COUNT;

    DELETE FROM t_sys_trusted_device
    WHERE c_expires_at < CURRENT_TIMESTAMP OR c_is_active = FALSE;

    GET DIAGNOSTICS deleted_devices = ROW_COUNT;

    RETURN NEXT;
END;
$$;