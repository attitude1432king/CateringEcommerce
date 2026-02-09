-- ======================================================
-- Partner Registration Approval System - Database Schema
-- ======================================================
-- Uses existing tables:
-- - t_sys_catering_owner (main partner data)
-- - t_sys_catering_owner_addresses
-- - t_sys_catering_owner_compliance
-- - t_sys_catering_owner_bankdetails
-- - t_sys_catering_owner_operations
--
-- Creates 3 new support tables:
-- 1. t_sys_partner_request_actions - Audit trail of all actions
-- 2. t_sys_partner_request_communications - Email/SMS communication log
-- 3. t_sys_admin_notifications - Admin notification system
-- ======================================================

-- Drop tables if they exist (in reverse order due to foreign keys)
IF OBJECT_ID('t_sys_admin_notifications', 'U') IS NOT NULL
    DROP TABLE t_sys_admin_notifications;

IF OBJECT_ID('t_sys_partner_request_communications', 'U') IS NOT NULL
    DROP TABLE t_sys_partner_request_communications;

IF OBJECT_ID('t_sys_partner_request_actions', 'U') IS NOT NULL
    DROP TABLE t_sys_partner_request_actions;

GO

-- ======================================================
-- Add columns to existing t_sys_catering_owner if not exists
-- ======================================================
-- These columns track the approval workflow status
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_approval_status')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_approval_status VARCHAR(50) DEFAULT 'PENDING';
    -- Values: PENDING, UNDER_REVIEW, APPROVED, REJECTED, INFO_REQUESTED, INCOMPLETE
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_reviewed_date')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_reviewed_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_reviewed_by')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_reviewed_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_approved_date')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_approved_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_approved_by')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_approved_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_rejection_reason')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_rejection_reason NVARCHAR(1000) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_internal_notes')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_internal_notes NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_owner') AND name = 'c_priority')
BEGIN
    ALTER TABLE t_sys_catering_owner
    ADD c_priority VARCHAR(20) DEFAULT 'NORMAL';
    -- Values: NORMAL, HIGH, URGENT
END

GO

-- Create index on approval_status for filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_catering_owner_approval_status' AND object_id = OBJECT_ID('t_sys_catering_owner'))
BEGIN
    CREATE INDEX IX_catering_owner_approval_status ON t_sys_catering_owner(c_approval_status);
END

GO

-- ======================================================
-- Table 1: Partner Request Actions Log (Audit Trail)
-- ======================================================
CREATE TABLE t_sys_partner_request_actions (
    c_action_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_ownerid BIGINT NOT NULL, -- References t_sys_catering_owner
    c_adminid BIGINT NOT NULL,
    c_action_type VARCHAR(50) NOT NULL,
        -- Values: SUBMITTED, VIEWED, STATUS_CHANGED, APPROVED, REJECTED, INFO_REQUESTED, DOCUMENT_UPLOADED
    c_old_status VARCHAR(50),
    c_new_status VARCHAR(50),
    c_remarks NVARCHAR(1000),
    c_action_date DATETIME DEFAULT GETDATE(),
    c_ip_address VARCHAR(50),

    CONSTRAINT FK_partner_action_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT FK_partner_action_admin FOREIGN KEY (c_adminid)
        REFERENCES t_sys_admin(c_adminid)
);

-- Indexes for performance
CREATE INDEX IX_partner_actions_ownerid ON t_sys_partner_request_actions(c_ownerid);
CREATE INDEX IX_partner_actions_admin_id ON t_sys_partner_request_actions(c_adminid);
CREATE INDEX IX_partner_actions_date ON t_sys_partner_request_actions(c_action_date DESC);

GO

-- ======================================================
-- Table 2: Partner Request Communications Log
-- ======================================================
CREATE TABLE t_sys_partner_request_communications (
    c_communication_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_ownerid BIGINT NOT NULL, -- References t_sys_catering_owner
    c_adminid BIGINT NOT NULL,
    c_communication_type VARCHAR(20) NOT NULL, -- EMAIL, SMS, BOTH, WHATSAPP
    c_subject NVARCHAR(200),
    c_message NVARCHAR(MAX) NOT NULL,
    c_sent_to_email VARCHAR(100),
    c_sent_to_phone VARCHAR(15),
    c_email_sent BIT DEFAULT 0,
    c_sms_sent BIT DEFAULT 0,
    c_email_status VARCHAR(50), -- SENT, FAILED, BOUNCED
    c_sms_status VARCHAR(50), -- SENT, FAILED, DELIVERED
    c_sent_date DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_partner_comm_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner(c_ownerid),
    CONSTRAINT FK_partner_comm_admin FOREIGN KEY (c_adminid)
        REFERENCES t_sys_admin(c_adminid)
);

-- Indexes for performance
CREATE INDEX IX_partner_comms_ownerid ON t_sys_partner_request_communications(c_ownerid);
CREATE INDEX IX_partner_comms_date ON t_sys_partner_request_communications(c_sent_date DESC);

GO

-- ======================================================
-- Table 3: Admin Notifications
-- ======================================================
CREATE TABLE t_sys_admin_notifications (
    c_notification_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_adminid BIGINT NULL, -- NULL for all admins
    c_notification_type VARCHAR(50) NOT NULL,
        -- PARTNER_REQUEST_NEW, PARTNER_REQUEST_UPDATE, DOCUMENT_UPLOADED
    c_title NVARCHAR(200) NOT NULL,
    c_message NVARCHAR(500),
    c_entity_id BIGINT, -- Partner/Owner ID or other entity ID
    c_entity_type VARCHAR(50), -- PARTNER_REQUEST, CATERING, ORDER, etc.
    c_link VARCHAR(500), -- Deep link to the request
    c_is_read BIT DEFAULT 0,
    c_read_date DATETIME NULL,
    c_created_date DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_notification_admin FOREIGN KEY (c_adminid)
        REFERENCES t_sys_admin(c_adminid)
);

-- Indexes for performance
CREATE INDEX IX_admin_notifications_admin_id ON t_sys_admin_notifications(c_adminid);
CREATE INDEX IX_admin_notifications_is_read ON t_sys_admin_notifications(c_is_read);
CREATE INDEX IX_admin_notifications_created_date ON t_sys_admin_notifications(c_created_date DESC);
CREATE INDEX IX_admin_notifications_type ON t_sys_admin_notifications(c_notification_type);

GO

-- ======================================================
-- Success Message
-- ======================================================
PRINT '✅ Partner Approval System tables created successfully!';
PRINT '';
PRINT 'Modified existing table:';
PRINT '  - t_sys_catering_owner (added approval workflow columns)';
PRINT '';
PRINT 'New tables created:';
PRINT '  1. t_sys_partner_request_actions';
PRINT '  2. t_sys_partner_request_communications';
PRINT '  3. t_sys_admin_notifications';
PRINT '';
PRINT 'All indexes created successfully.';
