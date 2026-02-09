-- =============================================
-- Owner Review Reply Columns Migration
-- Adds owner_reply and owner_reply_date to t_sys_catering_review
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_review') AND name = 'c_owner_reply')
BEGIN
    ALTER TABLE t_sys_catering_review ADD c_owner_reply NVARCHAR(2000) NULL;
    PRINT 'Added column c_owner_reply to t_sys_catering_review';
END
ELSE
    PRINT 'Column c_owner_reply already exists';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_review') AND name = 'c_owner_reply_date')
BEGIN
    ALTER TABLE t_sys_catering_review ADD c_owner_reply_date DATETIME NULL;
    PRINT 'Added column c_owner_reply_date to t_sys_catering_review';
END
ELSE
    PRINT 'Column c_owner_reply_date already exists';
GO
