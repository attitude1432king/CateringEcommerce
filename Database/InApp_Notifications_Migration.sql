-- ============================================
-- In-App Notifications Migration
-- Creates tables for in-app notification system
-- ============================================

USE CateringDB;
GO

PRINT 'Starting In-App Notifications Migration...';
GO

-- ============================================
-- TABLE: t_sys_notifications (In-App Notifications)
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N't_sys_notifications') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[t_sys_notifications] (
        [c_notification_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_notification_uuid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

        -- Recipient Information
        [c_userid] NVARCHAR(50) NOT NULL,  -- User ID (can be numeric or string)
        [c_user_type] NVARCHAR(20) NOT NULL DEFAULT 'USER', -- USER, OWNER, ADMIN, SUPERVISOR

        -- Notification Content
        [c_title] NVARCHAR(200) NOT NULL,
        [c_message] NVARCHAR(1000) NOT NULL,
        [c_category] NVARCHAR(50) NOT NULL, -- ORDER, PAYMENT, REVIEW, SYSTEM, etc.

        -- Priority & Actions
        [c_priority] INT NOT NULL DEFAULT 1, -- 1=Low, 2=Normal, 3=High, 4=Urgent
        [c_action_url] NVARCHAR(500) NULL, -- Deep link URL
        [c_action_label] NVARCHAR(100) NULL, -- Button label (e.g., "View Order")
        [c_icon_url] NVARCHAR(500) NULL, -- Icon/image URL

        -- Additional Data (JSON)
        [c_data] NVARCHAR(MAX) NULL, -- JSON object with extra data

        -- Status
        [c_is_read] BIT NOT NULL DEFAULT 0,
        [c_read_at] DATETIME NULL,
        [c_is_deleted] BIT NOT NULL DEFAULT 0,
        [c_deleted_at] DATETIME NULL,

        -- Audit
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_expires_at] DATETIME NULL, -- Auto-delete after this date

        -- Indexes
        CONSTRAINT [UQ_Notification_UUID] UNIQUE ([c_notification_uuid])
    );

    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_UserType]
        ON [dbo].[t_sys_notifications] ([c_userid], [c_user_type], [c_is_read], [c_is_deleted])
        INCLUDE ([c_createddate], [c_priority]);

    CREATE NONCLUSTERED INDEX [IX_Notifications_Created]
        ON [dbo].[t_sys_notifications] ([c_createddate] DESC)
        WHERE [c_is_deleted] = 0;

    CREATE NONCLUSTERED INDEX [IX_Notifications_Category]
        ON [dbo].[t_sys_notifications] ([c_category], [c_user_type])
        WHERE [c_is_deleted] = 0;

    PRINT 'Table t_sys_notifications created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_notifications already exists';
END
GO

-- ============================================
-- TABLE: t_sys_notification_preferences (User Preferences)
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N't_sys_notification_preferences') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[t_sys_notification_preferences] (
        [c_preference_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_userid] NVARCHAR(50) NOT NULL,
        [c_user_type] NVARCHAR(20) NOT NULL DEFAULT 'USER',

        -- Channel Preferences
        [c_email_enabled] BIT NOT NULL DEFAULT 1,
        [c_sms_enabled] BIT NOT NULL DEFAULT 1,
        [c_inapp_enabled] BIT NOT NULL DEFAULT 1,
        [c_push_enabled] BIT NOT NULL DEFAULT 1,

        -- Category Preferences (JSON)
        [c_category_preferences] NVARCHAR(MAX) NULL, -- JSON: {"ORDER": true, "PAYMENT": false, ...}

        -- Quiet Hours
        [c_quiet_hours_enabled] BIT NOT NULL DEFAULT 0,
        [c_quiet_hours_start] TIME NULL, -- e.g., 22:00
        [c_quiet_hours_end] TIME NULL, -- e.g., 08:00

        -- Audit
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_modifieddate] DATETIME NULL,

        -- Unique constraint
        CONSTRAINT [UQ_NotificationPreferences_User] UNIQUE ([c_userid], [c_user_type])
    );

    PRINT 'Table t_sys_notification_preferences created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_notification_preferences already exists';
END
GO

-- ============================================
-- STORED PROCEDURE: sp_GetUserNotifications
-- ============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_GetUserNotifications') AND type = 'P')
    DROP PROCEDURE sp_GetUserNotifications;
GO

CREATE PROCEDURE sp_GetUserNotifications
    @UserId NVARCHAR(50),
    @UserType NVARCHAR(20) = 'USER',
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @IncludeRead BIT = 1,
    @CategoryFilter NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        c_notification_uuid AS NotificationId,
        c_title AS Title,
        c_message AS Message,
        c_category AS Category,
        c_priority AS Priority,
        c_action_url AS ActionUrl,
        c_action_label AS ActionLabel,
        c_icon_url AS IconUrl,
        c_data AS Data,
        c_is_read AS IsRead,
        c_read_at AS ReadAt,
        c_createddate AS CreatedAt,
        c_expires_at AS ExpiresAt
    FROM t_sys_notifications
    WHERE c_userid = @UserId
      AND c_user_type = @UserType
      AND c_is_deleted = 0
      AND (@IncludeRead = 1 OR c_is_read = 0)
      AND (@CategoryFilter IS NULL OR c_category = @CategoryFilter)
      AND (c_expires_at IS NULL OR c_expires_at > GETDATE())
    ORDER BY
        c_priority DESC,
        c_createddate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Return total count
    SELECT COUNT(*) AS TotalCount
    FROM t_sys_notifications
    WHERE c_userid = @UserId
      AND c_user_type = @UserType
      AND c_is_deleted = 0
      AND (@IncludeRead = 1 OR c_is_read = 0)
      AND (@CategoryFilter IS NULL OR c_category = @CategoryFilter)
      AND (c_expires_at IS NULL OR c_expires_at > GETDATE());

    -- Return unread count
    SELECT COUNT(*) AS UnreadCount
    FROM t_sys_notifications
    WHERE c_userid = @UserId
      AND c_user_type = @UserType
      AND c_is_deleted = 0
      AND c_is_read = 0
      AND (c_expires_at IS NULL OR c_expires_at > GETDATE());
END
GO

PRINT 'Stored procedure sp_GetUserNotifications created successfully';
GO

-- ============================================
-- STORED PROCEDURE: sp_MarkNotificationAsRead
-- ============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_MarkNotificationAsRead') AND type = 'P')
    DROP PROCEDURE sp_MarkNotificationAsRead;
GO

CREATE PROCEDURE sp_MarkNotificationAsRead
    @NotificationUuid UNIQUEIDENTIFIER,
    @UserId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_notifications
    SET c_is_read = 1,
        c_read_at = GETDATE()
    WHERE c_notification_uuid = @NotificationUuid
      AND c_userid = @UserId
      AND c_is_read = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Stored procedure sp_MarkNotificationAsRead created successfully';
GO

-- ============================================
-- STORED PROCEDURE: sp_MarkAllNotificationsAsRead
-- ============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_MarkAllNotificationsAsRead') AND type = 'P')
    DROP PROCEDURE sp_MarkAllNotificationsAsRead;
GO

CREATE PROCEDURE sp_MarkAllNotificationsAsRead
    @UserId NVARCHAR(50),
    @UserType NVARCHAR(20) = 'USER'
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_notifications
    SET c_is_read = 1,
        c_read_at = GETDATE()
    WHERE c_userid = @UserId
      AND c_user_type = @UserType
      AND c_is_read = 0
      AND c_is_deleted = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Stored procedure sp_MarkAllNotificationsAsRead created successfully';
GO

-- ============================================
-- STORED PROCEDURE: sp_DeleteOldNotifications
-- ============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_DeleteOldNotifications') AND type = 'P')
    DROP PROCEDURE sp_DeleteOldNotifications;
GO

CREATE PROCEDURE sp_DeleteOldNotifications
    @DaysToKeep INT = 90
AS
BEGIN
    SET NOCOUNT ON;

    -- Soft delete old read notifications
    UPDATE t_sys_notifications
    SET c_is_deleted = 1,
        c_deleted_at = GETDATE()
    WHERE c_is_read = 1
      AND c_createddate < DATEADD(DAY, -@DaysToKeep, GETDATE())
      AND c_is_deleted = 0;

    SELECT @@ROWCOUNT AS DeletedCount;

    -- Hard delete expired notifications
    DELETE FROM t_sys_notifications
    WHERE c_expires_at IS NOT NULL
      AND c_expires_at < GETDATE();

    SELECT @@ROWCOUNT AS ExpiredCount;
END
GO

PRINT 'Stored procedure sp_DeleteOldNotifications created successfully';
GO

-- ============================================
-- Insert Default Notification Preferences for existing users
-- ============================================

-- Insert default preferences for existing users (if needed)
INSERT INTO t_sys_notification_preferences (c_userid, c_user_type, c_email_enabled, c_sms_enabled, c_inapp_enabled, c_push_enabled)
SELECT
    CAST(u.c_userid AS NVARCHAR(50)),
    'USER',
    1, 1, 1, 1
FROM t_sys_user u
WHERE NOT EXISTS (
    SELECT 1
    FROM t_sys_notification_preferences
    WHERE c_userid = CAST(u.c_userid AS NVARCHAR(50))
      AND c_user_type = 'USER'
);

PRINT 'Default notification preferences created for existing users';
GO

PRINT 'In-App Notifications Migration completed successfully!';
GO
