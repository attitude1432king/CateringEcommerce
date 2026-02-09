-- =====================================================
-- Review System Enhancement Migration
-- Ensures all required columns exist for review submission
-- =====================================================

USE CateringDB;
GO

PRINT 'Starting Review System Enhancement Migration...';
GO

-- Add c_ishidden column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_ishidden')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_ishidden] BIT DEFAULT 0;

    PRINT 'Column c_ishidden added to t_sys_catering_review.';
END
ELSE
BEGIN
    PRINT 'Column c_ishidden already exists in t_sys_catering_review.';
END
GO

-- Add c_hidden_reason column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_reason')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_reason] NVARCHAR(500) NULL;

    PRINT 'Column c_hidden_reason added to t_sys_catering_review.';
END
ELSE
BEGIN
    PRINT 'Column c_hidden_reason already exists in t_sys_catering_review.';
END
GO

-- Add c_hidden_by column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_by')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_by] BIGINT NULL;

    PRINT 'Column c_hidden_by added to t_sys_catering_review.';
END
ELSE
BEGIN
    PRINT 'Column c_hidden_by already exists in t_sys_catering_review.';
END
GO

-- Add c_hidden_date column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N't_sys_catering_review') AND name = 'c_hidden_date')
BEGIN
    ALTER TABLE [dbo].[t_sys_catering_review]
    ADD [c_hidden_date] DATETIME NULL;

    PRINT 'Column c_hidden_date added to t_sys_catering_review.';
END
ELSE
BEGIN
    PRINT 'Column c_hidden_date already exists in t_sys_catering_review.';
END
GO

-- Ensure Owner Review Reply table exists for owner responses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N't_sys_owner_review_reply') AND type IN (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_owner_review_reply] (
        [c_replyid] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_reviewid] BIGINT NOT NULL,
        [c_ownerid] BIGINT NOT NULL,
        [c_reply_text] NVARCHAR(1000) NOT NULL,
        [c_reply_date] DATETIME DEFAULT GETDATE(),
        [c_createddate] DATETIME DEFAULT GETDATE(),
        [c_modifieddate] DATETIME NULL,

        CONSTRAINT FK_ReviewReply_Review FOREIGN KEY ([c_reviewid])
            REFERENCES [t_sys_catering_review]([c_reviewid]) ON DELETE CASCADE,
        CONSTRAINT FK_ReviewReply_Owner FOREIGN KEY ([c_ownerid])
            REFERENCES [t_sys_catering_owner]([c_ownerid])
    );

    PRINT 'Table t_sys_owner_review_reply created.';
END
ELSE
BEGIN
    PRINT 'Table t_sys_owner_review_reply already exists.';
END
GO

-- Create index for better performance on review queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Review_OwnerId_Visible' AND object_id = OBJECT_ID('t_sys_catering_review'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Review_OwnerId_Visible]
    ON [dbo].[t_sys_catering_review] ([c_ownerid], [c_is_visible], [c_ishidden])
    INCLUDE ([c_overall_rating], [c_createddate]);

    PRINT 'Index IX_Review_OwnerId_Visible created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Review_OwnerId_Visible already exists.';
END
GO

-- Create index for user's reviews
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Review_UserId' AND object_id = OBJECT_ID('t_sys_catering_review'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Review_UserId]
    ON [dbo].[t_sys_catering_review] ([c_userid])
    INCLUDE ([c_orderid], [c_overall_rating], [c_createddate]);

    PRINT 'Index IX_Review_UserId created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Review_UserId already exists.';
END
GO

-- Create index for order reviews
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Review_OrderId' AND object_id = OBJECT_ID('t_sys_catering_review'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Review_OrderId]
    ON [dbo].[t_sys_catering_review] ([c_orderid]);

    PRINT 'Index IX_Review_OrderId created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Review_OrderId already exists.';
END
GO

-- Update existing reviews to have c_ishidden = 0 if NULL
UPDATE [dbo].[t_sys_catering_review]
SET [c_ishidden] = 0
WHERE [c_ishidden] IS NULL;
GO

PRINT 'Review System Enhancement Migration completed successfully!';
GO
