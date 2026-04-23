-- ============================================================
-- Contact Messages Migration
-- Creates t_sys_contact_messages for user-facing contact form
-- ============================================================

CREATE TABLE IF NOT EXISTS t_sys_contact_messages (
    c_messageid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_name VARCHAR(100) NOT NULL,
    c_email VARCHAR(256) NOT NULL,
    c_message VARCHAR(2000) NOT NULL,
    c_status VARCHAR(20) NOT NULL DEFAULT 'New',   -- New | Read | Replied
    c_ip_address VARCHAR(50) NULL,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS ix_contact_messages_status
ON t_sys_contact_messages (c_status, c_createddate DESC);

