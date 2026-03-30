/*
 * Database Migration Script: Admin Temporary Password Flag
 * Purpose: Add c_is_temporary_password column to t_sys_admin
 *          to support forced password change on first login
 *          for admin-created sub-accounts.
 * Date: 2026-03-14
 */

USE [CateringDB];
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_admin]')
      AND name = 'c_is_temporary_password'
)
BEGIN
    ALTER TABLE [dbo].[t_sys_admin]
    ADD [c_is_temporary_password] BIT NOT NULL DEFAULT 0;

    PRINT 'Column [c_is_temporary_password] added to [t_sys_admin].';
END
ELSE
BEGIN
    PRINT 'Column [c_is_temporary_password] already exists in [t_sys_admin].';
END
GO

PRINT '============================================================';
PRINT 'AdminTempPassword Migration Complete';
PRINT '============================================================';
GO
