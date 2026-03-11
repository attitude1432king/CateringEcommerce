-- =============================================
-- Owner Support Tickets Migration
-- Creates t_sys_support_tickets table
-- and t_sys_support_ticket_messages table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_support_tickets')
BEGIN
    CREATE TABLE t_sys_support_tickets (
        c_ticket_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_ticket_number NVARCHAR(20) NOT NULL,         -- e.g. TKT-20260201-001
        c_ownerid BIGINT NOT NULL,                      -- FK -> t_sys_catering_owner

        -- Ticket Info
        c_subject NVARCHAR(200) NOT NULL,
        c_description NVARCHAR(2000) NOT NULL,
        c_category NVARCHAR(50) NOT NULL,               -- Payment Issues, Orders & Bookings, Account & Settings, Technical Issue, Other
        c_priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Urgent
        c_status NVARCHAR(20) NOT NULL DEFAULT 'Open',     -- Open, InProgress, Resolved, Closed

        -- Related entities (optional)
        c_related_order_id BIGINT NULL,

        -- Resolution
        c_resolved_by NVARCHAR(100) NULL,
        c_resolution_notes NVARCHAR(2000) NULL,
        c_resolved_date DATETIME NULL,

        -- Metadata
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,

        CONSTRAINT UQ_TicketNumber UNIQUE (c_ticket_number)
    );

    CREATE INDEX IX_SupportTickets_OwnerId ON t_sys_support_tickets (c_ownerid);
    CREATE INDEX IX_SupportTickets_Status ON t_sys_support_tickets (c_status);
    CREATE INDEX IX_SupportTickets_Category ON t_sys_support_tickets (c_category);
    CREATE INDEX IX_SupportTickets_CreatedDate ON t_sys_support_tickets (c_createddate DESC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_support_ticket_messages')
BEGIN
    CREATE TABLE t_sys_support_ticket_messages (
        c_message_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_ticket_id BIGINT NOT NULL,                    -- FK -> t_sys_support_tickets
        c_sender_type NVARCHAR(20) NOT NULL,            -- Owner, Admin
        c_sender_id BIGINT NOT NULL,
        c_message_text NVARCHAR(2000) NOT NULL,
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TicketMessages_Ticket FOREIGN KEY (c_ticket_id)
            REFERENCES t_sys_support_tickets (c_ticket_id)
    );

    CREATE INDEX IX_TicketMessages_TicketId ON t_sys_support_ticket_messages (c_ticket_id);
END
GO
