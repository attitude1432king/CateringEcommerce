-- ======================================================
-- Admin Partner Approval Flow - Enum Column Type Fix
-- ======================================================
-- Purpose: Fix c_approval_status and c_priority columns to use INT (enum values)
-- instead of VARCHAR (string values)
--
-- IMPORTANT:
-- - This migration fixes the incorrect VARCHAR columns to INT columns
-- - Uses enum integer values: ApprovalStatus (1,2,3,4,5), PriorityStatus (0,1,2,3)
-- - Converts existing string values to their corresponding enum integers
-- ======================================================

USE [CateringEcommerce];
GO

PRINT '======================================================';
PRINT 'Starting Admin Partner Approval Enum Fix Migration';
PRINT '======================================================';
PRINT '';

-- ======================================================
-- Step 1: Check if columns need migration
-- ======================================================
DECLARE @approval_status_type NVARCHAR(50);
DECLARE @priority_type NVARCHAR(50);

SELECT @approval_status_type = DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 't_sys_catering_owner'
AND COLUMN_NAME = 'c_approval_status';

SELECT @priority_type = DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 't_sys_catering_owner'
AND COLUMN_NAME = 'c_priority';

PRINT 'Current column types:';
PRINT '  c_approval_status: ' + ISNULL(@approval_status_type, 'NOT FOUND');
PRINT '  c_priority: ' + ISNULL(@priority_type, 'NOT FOUND');
PRINT '';

-- ======================================================
-- Step 2: Fix c_approval_status column (VARCHAR → INT)
-- ======================================================
IF @approval_status_type = 'varchar' OR @approval_status_type = 'nvarchar'
BEGIN
    PRINT 'Migrating c_approval_status from VARCHAR to INT...';

    -- Create temporary column
    ALTER TABLE t_sys_catering_owner
    ADD c_approval_status_int INT NULL;

    -- Convert existing VARCHAR values to INT enum values
    -- Mapping:
    --   'PENDING' → 1 (ApprovalStatus.Pending)
    --   'APPROVED' → 2 (ApprovalStatus.Approved)
    --   'REJECTED' → 3 (ApprovalStatus.Rejected)
    --   'UNDER_REVIEW' → 4 (ApprovalStatus.UnderReview)
    --   'INFO_REQUESTED' → 5 (ApprovalStatus.Info_Requested)
    --   Default → 1 (Pending)
    UPDATE t_sys_catering_owner
    SET c_approval_status_int = CASE
        WHEN c_approval_status = 'PENDING' THEN 1
        WHEN c_approval_status = 'APPROVED' THEN 2
        WHEN c_approval_status = 'REJECTED' THEN 3
        WHEN c_approval_status = 'UNDER_REVIEW' THEN 4
        WHEN c_approval_status = 'INFO_REQUESTED' THEN 5
        ELSE 1  -- Default to Pending
    END;

    -- Drop old VARCHAR column
    ALTER TABLE t_sys_catering_owner
    DROP COLUMN c_approval_status;

    -- Rename new column to original name
    EXEC sp_rename 't_sys_catering_owner.c_approval_status_int', 'c_approval_status', 'COLUMN';

    -- Set default constraint
    ALTER TABLE t_sys_catering_owner
    ADD CONSTRAINT DF_catering_owner_approval_status DEFAULT 1 FOR c_approval_status;

    -- Create index
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_catering_owner_approval_status')
    BEGIN
        CREATE INDEX IX_catering_owner_approval_status ON t_sys_catering_owner(c_approval_status);
    END

    PRINT '  ✓ c_approval_status migrated to INT successfully';
    PRINT '';
END
ELSE IF @approval_status_type = 'int'
BEGIN
    PRINT '  ✓ c_approval_status is already INT type - no migration needed';
    PRINT '';
END
ELSE
BEGIN
    PRINT '  ⚠ c_approval_status column not found - will be created';

    ALTER TABLE t_sys_catering_owner
    ADD c_approval_status INT NOT NULL DEFAULT 1;

    CREATE INDEX IX_catering_owner_approval_status ON t_sys_catering_owner(c_approval_status);

    PRINT '  ✓ c_approval_status column created';
    PRINT '';
END

-- ======================================================
-- Step 3: Fix c_priority column (VARCHAR → INT)
-- ======================================================
IF @priority_type = 'varchar' OR @priority_type = 'nvarchar'
BEGIN
    PRINT 'Migrating c_priority from VARCHAR to INT...';

    -- Create temporary column
    ALTER TABLE t_sys_catering_owner
    ADD c_priority_int INT NULL;

    -- Convert existing VARCHAR values to INT enum values
    -- Mapping:
    --   'LOW' → 0 (PriorityStatus.Low)
    --   'NORMAL' → 1 (PriorityStatus.Normal)
    --   'HIGH' → 2 (PriorityStatus.High)
    --   'URGENT' → 3 (PriorityStatus.Urgent)
    --   Default → 1 (Normal)
    UPDATE t_sys_catering_owner
    SET c_priority_int = CASE
        WHEN c_priority = 'LOW' THEN 0
        WHEN c_priority = 'NORMAL' THEN 1
        WHEN c_priority = 'HIGH' THEN 2
        WHEN c_priority = 'URGENT' THEN 3
        ELSE 1  -- Default to Normal
    END;

    -- Drop old VARCHAR column
    ALTER TABLE t_sys_catering_owner
    DROP COLUMN c_priority;

    -- Rename new column to original name
    EXEC sp_rename 't_sys_catering_owner.c_priority_int', 'c_priority', 'COLUMN';

    -- Set default constraint
    ALTER TABLE t_sys_catering_owner
    ADD CONSTRAINT DF_catering_owner_priority DEFAULT 1 FOR c_priority;

    PRINT '  ✓ c_priority migrated to INT successfully';
    PRINT '';
END
ELSE IF @priority_type = 'int'
BEGIN
    PRINT '  ✓ c_priority is already INT type - no migration needed';
    PRINT '';
END
ELSE
BEGIN
    PRINT '  ⚠ c_priority column not found - will be created';

    ALTER TABLE t_sys_catering_owner
    ADD c_priority INT NOT NULL DEFAULT 1;

    PRINT '  ✓ c_priority column created';
    PRINT '';
END

-- ======================================================
-- Step 4: Verify migration
-- ======================================================
PRINT 'Verifying migration...';
PRINT '';

SELECT @approval_status_type = DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 't_sys_catering_owner'
AND COLUMN_NAME = 'c_approval_status';

SELECT @priority_type = DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 't_sys_catering_owner'
AND COLUMN_NAME = 'c_priority';

PRINT 'Final column types:';
PRINT '  c_approval_status: ' + @approval_status_type;
PRINT '  c_priority: ' + @priority_type;
PRINT '';

-- Show sample data
PRINT 'Sample data (first 10 rows):';
SELECT TOP 10
    c_ownerid,
    c_catering_name,
    c_approval_status,
    c_priority,
    c_createddate
FROM t_sys_catering_owner
ORDER BY c_ownerid DESC;

-- ======================================================
-- Summary
-- ======================================================
PRINT '';
PRINT '======================================================';
PRINT 'Migration completed successfully!';
PRINT '======================================================';
PRINT '';
PRINT 'Column Mappings:';
PRINT '';
PRINT 'c_approval_status (INT):';
PRINT '  1 = Pending';
PRINT '  2 = Approved';
PRINT '  3 = Rejected';
PRINT '  4 = Under Review';
PRINT '  5 = Info Requested';
PRINT '';
PRINT 'c_priority (INT):';
PRINT '  0 = Low';
PRINT '  1 = Normal';
PRINT '  2 = High';
PRINT '  3 = Urgent';
PRINT '';
PRINT '✓ All changes have been applied successfully';
PRINT '';

GO
