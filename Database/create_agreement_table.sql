-- =============================================
-- Create Partner Agreement Table
-- =============================================
-- This script creates the t_sys_catering_owner_agreement table
-- Safe to run multiple times - uses IF NOT EXISTS check
-- =============================================

USE CateringDB;
GO

PRINT '========================================';
PRINT 'Creating Partner Agreement Table';
PRINT '========================================';
PRINT '';

-- Create Partner Agreement Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_catering_owner_agreement]') AND type in (N'U'))
BEGIN
    CREATE TABLE t_sys_catering_owner_agreement (
        c_agreementid BIGINT IDENTITY(1,1) PRIMARY KEY,
        c_ownerid BIGINT NOT NULL,
        c_agreement_text NVARCHAR(MAX) NOT NULL,
        c_agreement_accepted BIT NOT NULL DEFAULT 0,
        c_signature_data NVARCHAR(MAX) NULL, -- Base64 encoded signature image
        c_signature_path NVARCHAR(500) NULL, -- File path to signature image
        c_agreement_pdf_path NVARCHAR(500) NULL, -- File path to generated agreement PDF
        c_ip_address NVARCHAR(50) NULL,
        c_user_agent NVARCHAR(500) NULL,
        c_accepted_date DATETIME NULL,
        c_createddate DATETIME DEFAULT GETDATE(),
        c_modifieddate DATETIME NULL,
        CONSTRAINT FK_Agreement_Owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid)
    );
    PRINT '✓ Created table: t_sys_catering_owner_agreement';
END
ELSE
BEGIN
    PRINT '✓ Table already exists: t_sys_catering_owner_agreement';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Verification';
PRINT '========================================';
PRINT '';

-- Verify the table was created
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_catering_owner_agreement]') AND type in (N'U'))
BEGIN
    PRINT '✓ SUCCESS: t_sys_catering_owner_agreement table is ready!';
    PRINT '';

    -- Show table structure
    PRINT 'Table Structure:';
    PRINT '----------------------------------------';
    SELECT
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        CHARACTER_MAXIMUM_LENGTH as MaxLength,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_sys_catering_owner_agreement'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '✗ ERROR: Failed to create t_sys_catering_owner_agreement table';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Script execution completed!';
PRINT '========================================';
