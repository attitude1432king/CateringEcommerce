-- =============================================
-- Migration: Add Soft Delete columns to t_sys_user
-- Date: 2026-02-14
-- Description: Adds soft delete support for admin user management
-- =============================================

-- Add soft delete columns if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_user') AND name = 'c_isdeleted')
BEGIN
    ALTER TABLE t_sys_user ADD c_isdeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added c_isdeleted column to t_sys_user';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_user') AND name = 'c_deleted_by')
BEGIN
    ALTER TABLE t_sys_user ADD c_deleted_by BIGINT NULL;
    PRINT 'Added c_deleted_by column to t_sys_user';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_user') AND name = 'c_deleted_date')
BEGIN
    ALTER TABLE t_sys_user ADD c_deleted_date DATETIME NULL;
    PRINT 'Added c_deleted_date column to t_sys_user';
END
GO

-- Ensure c_isactive has a default value
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_user') AND name = 'c_isactive')
BEGIN
    -- Update NULL values to 1 (active)
    UPDATE t_sys_user SET c_isactive = 1 WHERE c_isactive IS NULL;
    PRINT 'Updated NULL c_isactive values to 1';
END
GO

PRINT 'User_SoftDelete_Migration completed successfully';
GO
