-- =============================================
-- Master Data Management - Audit Columns Migration
-- Adds audit tracking columns to existing master data tables
-- =============================================

-- =============================================
-- t_sys_city - Add audit columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_display_order')
BEGIN
    ALTER TABLE t_sys_city ADD c_display_order INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_created_date')
BEGIN
    ALTER TABLE t_sys_city ADD c_created_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_created_by')
BEGIN
    ALTER TABLE t_sys_city ADD c_created_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_modified_date')
BEGIN
    ALTER TABLE t_sys_city ADD c_modified_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_modified_by')
BEGIN
    ALTER TABLE t_sys_city ADD c_modified_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_city') AND name = 'c_is_deleted')
BEGIN
    ALTER TABLE t_sys_city ADD c_is_deleted BIT NOT NULL DEFAULT 0;
END

-- Create index for t_sys_city
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_City_Active' AND object_id = OBJECT_ID('t_sys_city'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_City_Active
        ON t_sys_city(c_is_active, c_stateid, c_display_order)
        WHERE c_is_deleted = 0;
END

GO

-- =============================================
-- t_sys_food_category - Add audit columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_food_category') AND name = 'c_display_order')
BEGIN
    ALTER TABLE t_sys_food_category ADD c_display_order INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_food_category') AND name = 'c_modified_date')
BEGIN
    ALTER TABLE t_sys_food_category ADD c_modified_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_food_category') AND name = 'c_modified_by')
BEGIN
    ALTER TABLE t_sys_food_category ADD c_modified_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_food_category') AND name = 'c_is_deleted')
BEGIN
    ALTER TABLE t_sys_food_category ADD c_is_deleted BIT NOT NULL DEFAULT 0;
END

-- Create index for t_sys_food_category
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodCategory_Active' AND object_id = OBJECT_ID('t_sys_food_category'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FoodCategory_Active
        ON t_sys_food_category(c_is_active, c_display_order)
        WHERE c_is_deleted = 0;
END

GO

-- =============================================
-- t_sys_catering_type_master - Add audit columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_display_order')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_display_order INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_created_date')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_created_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_created_by')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_created_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_modified_date')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_modified_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_modified_by')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_modified_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_type_master') AND name = 'c_is_deleted')
BEGIN
    ALTER TABLE t_sys_catering_type_master ADD c_is_deleted BIT NOT NULL DEFAULT 0;
END

-- Create index for t_sys_catering_type_master
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CateringType_Active' AND object_id = OBJECT_ID('t_sys_catering_type_master'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_CateringType_Active
        ON t_sys_catering_type_master(c_is_active, c_category_id, c_display_order)
        WHERE c_is_deleted = 0;
END

GO

-- =============================================
-- t_sys_catering_theme_types - Add audit columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_display_order')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_display_order INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_created_date')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_created_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_created_by')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_created_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_modified_date')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_modified_date DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_modified_by')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_modified_by BIGINT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_catering_theme_types') AND name = 'c_is_deleted')
BEGIN
    ALTER TABLE t_sys_catering_theme_types ADD c_is_deleted BIT NOT NULL DEFAULT 0;
END

-- Create index for t_sys_catering_theme_types
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ThemeType_Active' AND object_id = OBJECT_ID('t_sys_catering_theme_types'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ThemeType_Active
        ON t_sys_catering_theme_types(c_is_active, c_display_order)
        WHERE c_is_deleted = 0;
END

GO

-- =============================================
-- Verification Queries
-- =============================================
PRINT 'Verifying t_sys_city columns:';
SELECT name, TYPE_NAME(system_type_id) as data_type, is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID('t_sys_city')
AND name IN ('c_display_order', 'c_created_date', 'c_created_by', 'c_modified_date', 'c_modified_by', 'c_is_deleted');

PRINT 'Verifying t_sys_food_category columns:';
SELECT name, TYPE_NAME(system_type_id) as data_type, is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID('t_sys_food_category')
AND name IN ('c_display_order', 'c_modified_date', 'c_modified_by', 'c_is_deleted');

PRINT 'Verifying t_sys_catering_type_master columns:';
SELECT name, TYPE_NAME(system_type_id) as data_type, is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID('t_sys_catering_type_master')
AND name IN ('c_display_order', 'c_created_date', 'c_created_by', 'c_modified_date', 'c_modified_by', 'c_is_deleted');

PRINT 'Verifying t_sys_catering_theme_types columns:';
SELECT name, TYPE_NAME(system_type_id) as data_type, is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID('t_sys_catering_theme_types')
AND name IN ('c_display_order', 'c_created_date', 'c_created_by', 'c_modified_date', 'c_modified_by', 'c_is_deleted');
