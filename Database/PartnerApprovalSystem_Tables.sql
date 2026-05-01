/*
 * Database Migration Script: Partner Registration Approval System
 * Purpose: Create support tables for partner approval workflow
 * PostgreSQL Compatible Version
 */

-- =============================================
-- Table: t_sys_partner_request_actions
-- Purpose: Audit trail of all approval actions
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_partner_request_actions (
    c_action_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_adminid BIGINT NOT NULL,
    c_action_type VARCHAR(50) NOT NULL,
    c_old_status VARCHAR(50),
    c_new_status VARCHAR(50),
    c_remarks VARCHAR(1000),
    c_action_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_ip_address VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS ix_partner_actions_ownerid ON t_sys_partner_request_actions(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_partner_actions_admin_id ON t_sys_partner_request_actions(c_adminid);
CREATE INDEX IF NOT EXISTS ix_partner_actions_date ON t_sys_partner_request_actions(c_action_date DESC);

        -- =============================================
        -- Table: t_sys_partner_request_communications
        -- Purpose: Email/SMS communication log for partner requests
        -- =============================================
        CREATE TABLE IF NOT EXISTS t_sys_partner_request_communications (
            c_communication_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            c_ownerid BIGINT NOT NULL,
            c_adminid BIGINT NOT NULL,
            c_communication_type VARCHAR(20) NOT NULL,
            c_subject VARCHAR(200),
            c_message TEXT NOT NULL,
            c_sent_to_email VARCHAR(100),
            c_sent_to_phone VARCHAR(15),
            c_email_sent BOOLEAN DEFAULT FALSE,
            c_sms_sent BOOLEAN DEFAULT FALSE,
            c_email_status VARCHAR(50),
            c_sms_status VARCHAR(50),
            c_sent_date TIMESTAMP NOT NULL DEFAULT NOW()
        );

        CREATE INDEX IF NOT EXISTS ix_partner_comms_ownerid ON t_sys_partner_request_communications(c_ownerid);

        -- =============================================
        -- Table: t_sys_admin_notifications
        -- Purpose: Admin notification system for partner requests
        -- =============================================
        CREATE TABLE IF NOT EXISTS t_sys_admin_notifications (
            c_notification_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            c_adminid BIGINT,
            c_notification_type VARCHAR(50) NOT NULL,
            c_title VARCHAR(200) NOT NULL,
            c_message VARCHAR(500),
            c_entity_id BIGINT,
            c_entity_type VARCHAR(50),
            c_link VARCHAR(500),
            c_is_read BOOLEAN DEFAULT FALSE,
            c_read_date TIMESTAMP,
            c_createddate TIMESTAMP NOT NULL DEFAULT NOW()
        );

        CREATE INDEX IF NOT EXISTS ix_admin_notifications_admin_id ON t_sys_admin_notifications(c_adminid);
        CREATE INDEX IF NOT EXISTS ix_admin_notifications_is_read ON t_sys_admin_notifications(c_is_read);
        CREATE INDEX IF NOT EXISTS ix_admin_notifications_created_date ON t_sys_admin_notifications(c_createddate DESC);
        CREATE INDEX IF NOT EXISTS ix_admin_notifications_type ON t_sys_admin_notifications(c_notification_type);

        -- =============================================
        -- Migration Complete
        -- =============================================

