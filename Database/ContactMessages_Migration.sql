-- ============================================================
-- Contact Messages Migration
-- Creates t_sys_contact_messages for user-facing contact form
-- ============================================================

CREATE TABLE t_sys_contact_messages (
    c_messageid   BIGINT        IDENTITY(1,1) PRIMARY KEY,
    c_name        NVARCHAR(100) NOT NULL,
    c_email       NVARCHAR(256) NOT NULL,
    c_message     NVARCHAR(2000) NOT NULL,
    c_status      NVARCHAR(20)  NOT NULL DEFAULT 'New',   -- New | Read | Replied
    c_ip_address  NVARCHAR(50)  NULL,
    c_createddate DATETIME      NOT NULL DEFAULT GETDATE(),
    c_modifieddate DATETIME     NULL
);

CREATE INDEX IX_contact_messages_status
    ON t_sys_contact_messages (c_status, c_createddate DESC);
