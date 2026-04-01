-- =============================================
-- Supervisor Schema Fixes Migration
-- Adds missing columns referenced in code but
-- absent from t_sys_supervisor and
-- t_sys_supervisor_registration
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Supervisor Schema Fixes Migration';
PRINT '================================================';
PRINT '';

-- =============================================
-- 1. t_sys_supervisor — missing columns
-- =============================================

-- c_bank_branch_name — collected in Step 6 registration form, accepted by SP but never persisted
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor') AND name = 'c_bank_branch_name')
BEGIN
    ALTER TABLE t_sys_supervisor ADD c_bank_branch_name NVARCHAR(100) NULL;
    PRINT '✓ Added column: t_sys_supervisor.c_bank_branch_name';
END
ELSE
    PRINT '  Column t_sys_supervisor.c_bank_branch_name already exists';
GO

-- c_bank_account_type — collected in Step 6 registration form (SAVINGS/CURRENT), accepted by SP but never persisted
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor') AND name = 'c_bank_account_type')
BEGIN
    ALTER TABLE t_sys_supervisor ADD c_bank_account_type VARCHAR(20) NULL;
    PRINT '✓ Added column: t_sys_supervisor.c_bank_account_type';
END
ELSE
    PRINT '  Column t_sys_supervisor.c_bank_account_type already exists';
GO

-- c_modifiedby — original schema definition was missing this column;
-- all stored procedures and repository UPDATE queries reference c_modifiedby for audit tracking
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor') AND name = 'c_modifiedby')
BEGIN
    ALTER TABLE t_sys_supervisor ADD c_modifiedby BIGINT NULL;
    PRINT '✓ Added column: t_sys_supervisor.c_modifiedby';
END
ELSE
    PRINT '  Column t_sys_supervisor.c_modifiedby already exists';
GO

-- =============================================
-- 2. t_sys_supervisor_registration — c_doc_verification_status alias column
--    AdminSupervisorRepository reads r.c_doc_verification_status but the
--    original column is c_document_verification_status
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_supervisor_registration') AND name = 'c_doc_verification_status')
BEGIN
    ALTER TABLE t_sys_supervisor_registration ADD c_doc_verification_status VARCHAR(20) NULL;
    PRINT '✓ Added column: t_sys_supervisor_registration.c_doc_verification_status';
END
ELSE
    PRINT '  Column t_sys_supervisor_registration.c_doc_verification_status already exists';
GO

-- Backfill from the original column for existing rows
UPDATE t_sys_supervisor_registration
SET    c_doc_verification_status = c_document_verification_status
WHERE  c_doc_verification_status IS NULL
  AND  c_document_verification_status IS NOT NULL;

PRINT '✓ Backfilled c_doc_verification_status from c_document_verification_status';
GO

-- =============================================
-- Summary
-- =============================================

PRINT '';
PRINT '================================================';
PRINT 'Migration completed successfully';
PRINT 'Columns added / verified:';
PRINT '  t_sys_supervisor.c_bank_branch_name';
PRINT '  t_sys_supervisor.c_bank_account_type';
PRINT '  t_sys_supervisor.c_modifiedby';
PRINT '  t_sys_supervisor_registration.c_doc_verification_status';
PRINT '================================================';
GO
