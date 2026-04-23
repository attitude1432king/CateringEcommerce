-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =============================================
-- Table: t_sys_notifications
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_notifications (
    c_notification_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_notification_uuid UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    c_userid VARCHAR(50) NOT NULL,
    c_user_type VARCHAR(20) NOT NULL DEFAULT 'USER',
    c_title VARCHAR(200) NOT NULL,
    c_message VARCHAR(1000) NOT NULL,
    c_category VARCHAR(50) NOT NULL,
    c_priority INTEGER NOT NULL DEFAULT 1,
    c_action_url VARCHAR(500),
    c_action_label VARCHAR(100),
    c_icon_url VARCHAR(500),
    c_data TEXT,
    c_is_read BOOLEAN NOT NULL DEFAULT FALSE,
    c_read_at TIMESTAMP,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    c_deleted_at TIMESTAMP,
    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_expires_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_notifications_userid_usertype
ON t_sys_notifications(c_userid, c_user_type, c_is_read, c_is_deleted, c_createddate DESC);

CREATE INDEX IF NOT EXISTS ix_notifications_created
ON t_sys_notifications(c_createddate DESC)
WHERE c_is_deleted = FALSE;

CREATE INDEX IF NOT EXISTS ix_notifications_category
ON t_sys_notifications(c_category);

-- =============================================
-- Table: t_sys_notification_preferences
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_notification_preferences (
    c_preference_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid VARCHAR(50) NOT NULL,
    c_user_type VARCHAR(20) NOT NULL DEFAULT 'USER',
    c_email_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    c_sms_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    c_inapp_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    c_push_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    c_category_preferences TEXT,
    c_quiet_hours_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    c_quiet_hours_start TIME,
    c_quiet_hours_end TIME,
    c_createddate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    c_modifieddate TIMESTAMP,
    CONSTRAINT uq_notification_preferences_user UNIQUE (c_userid, c_user_type)
);

-- =============================================
-- FUNCTION: Mark Notification As Read
-- =============================================
CREATE OR REPLACE FUNCTION sp_MarkNotificationAsRead(
    p_notification_uuid UUID,
    p_userid VARCHAR
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE v_count INTEGER;
BEGIN
    UPDATE t_sys_notifications
    SET c_is_read = TRUE,
        c_read_at = CURRENT_TIMESTAMP
    WHERE c_notification_uuid = p_notification_uuid
      AND c_userid = p_userid
      AND c_is_read = FALSE;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count;
END;
$$;

-- =============================================
-- FUNCTION: Mark All Notifications As Read
-- =============================================
CREATE OR REPLACE FUNCTION sp_MarkAllNotificationsAsRead(
    p_userid VARCHAR,
    p_user_type VARCHAR DEFAULT 'USER'
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE v_count INTEGER;
BEGIN
    UPDATE t_sys_notifications
    SET c_is_read = TRUE,
        c_read_at = CURRENT_TIMESTAMP
    WHERE c_userid = p_userid
      AND c_user_type = p_user_type
      AND c_is_read = FALSE
      AND c_is_deleted = FALSE;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count;
END;
$$;

-- =============================================
-- FUNCTION: Delete Old Notifications
-- =============================================
CREATE OR REPLACE FUNCTION sp_DeleteOldNotifications(
    p_days_to_keep INTEGER DEFAULT 90
)
RETURNS TABLE(deleted_count INTEGER, expired_count INTEGER)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Soft delete
    UPDATE t_sys_notifications
    SET c_is_deleted = TRUE,
        c_deleted_at = CURRENT_TIMESTAMP
    WHERE c_is_read = TRUE
      AND c_createddate < CURRENT_TIMESTAMP - (p_days_to_keep || ' days')::INTERVAL
      AND c_is_deleted = FALSE;

    GET DIAGNOSTICS deleted_count = ROW_COUNT;

    -- Hard delete
    DELETE FROM t_sys_notifications
    WHERE c_expires_at IS NOT NULL
      AND c_expires_at < CURRENT_TIMESTAMP;

    GET DIAGNOSTICS expired_count = ROW_COUNT;

    RETURN NEXT;
END;
$$;

-- =============================================
-- Insert Default Preferences
-- =============================================
INSERT INTO t_sys_notification_preferences 
(c_userid, c_user_type, c_email_enabled, c_sms_enabled, c_inapp_enabled, c_push_enabled)
SELECT
    CAST(u.c_userid AS VARCHAR(50)),
    'USER',
    TRUE, TRUE, TRUE, TRUE
FROM t_sys_user u
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_notification_preferences p
    WHERE p.c_userid = CAST(u.c_userid AS VARCHAR(50))
      AND p.c_user_type = 'USER'
);