-- =============================================
-- Update Agreement Table - Add PDF Columns
-- =============================================
-- Run this if you already created the agreement table
-- and need to add the new PDF-related columns
-- =============================================

USE CateringDB;
GO

PRINT '========================================';
PRINT 'Updating Agreement Table Schema';
PRINT '========================================';
PRINT '';

-- Check if table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_catering_owner_agreement]') AND type in (N'U'))
BEGIN
    PRINT '✓ Table t_sys_catering_owner_agreement exists';
    PRINT '';

    -- Add c_signature_path column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_catering_owner_agreement]') AND name = 'c_signature_path')
    BEGIN
        ALTER TABLE t_sys_catering_owner_agreement
        ADD c_signature_path NVARCHAR(500) NULL;
        PRINT '✓ Added column: c_signature_path';
    END
    ELSE
    BEGIN
        PRINT '✓ Column already exists: c_signature_path';
    END

    -- Add c_agreement_pdf_path column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_catering_owner_agreement]') AND name = 'c_agreement_pdf_path')
    BEGIN
        ALTER TABLE t_sys_catering_owner_agreement
        ADD c_agreement_pdf_path NVARCHAR(500) NULL;
        PRINT '✓ Added column: c_agreement_pdf_path';
    END
    ELSE
    BEGIN
        PRINT '✓ Column already exists: c_agreement_pdf_path';
    END

    PRINT '';
    PRINT '========================================';
    PRINT 'Verification';
    PRINT '========================================';
    PRINT '';

    -- Show updated table structure
    PRINT 'Updated Table Structure:';
    PRINT '----------------------------------------';
    SELECT
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        CHARACTER_MAXIMUM_LENGTH as MaxLength,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_catering_owner_agreement'
    ORDER BY ORDINAL_POSITION;

    PRINT '';
    PRINT '✓ SUCCESS: Agreement table updated successfully!';
END
ELSE
BEGIN
    PRINT '✗ ERROR: Table t_sys_catering_owner_agreement does not exist';
    PRINT 'Please run create_agreement_table.sql first';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Script execution completed!';
PRINT '========================================';
