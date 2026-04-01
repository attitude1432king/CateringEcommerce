IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N't_sys_catering_owner_operations')
      AND name = 'c_daily_booking_capacity'
)
BEGIN
    ALTER TABLE t_sys_catering_owner_operations
    ADD c_daily_booking_capacity INT NULL;

    PRINT 'Column c_daily_booking_capacity added to t_sys_catering_owner_operations.';
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N't_catering_availability_global')
      AND name = 'c_global_status'
      AND system_type_id IN (167, 175, 231, 239)
)
BEGIN
    DECLARE @GlobalConstraintSql NVARCHAR(MAX) = N'';

    SELECT @GlobalConstraintSql = @GlobalConstraintSql +
        N'ALTER TABLE t_catering_availability_global DROP CONSTRAINT [' + dc.name + N'];'
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N't_catering_availability_global')
      AND c.name = 'c_global_status';

    SELECT @GlobalConstraintSql = @GlobalConstraintSql +
        N'ALTER TABLE t_catering_availability_global DROP CONSTRAINT [' + cc.name + N'];'
    FROM sys.check_constraints cc
    WHERE cc.parent_object_id = OBJECT_ID(N't_catering_availability_global')
      AND cc.definition LIKE '%c_global_status%';

    IF (@GlobalConstraintSql <> N'')
        EXEC sp_executesql @GlobalConstraintSql;

    ALTER TABLE t_catering_availability_global
    ADD c_global_status_int INT NULL;

    UPDATE t_catering_availability_global
    SET c_global_status_int = CASE UPPER(LTRIM(RTRIM(ISNULL(c_global_status, 'OPEN'))))
        WHEN 'OPEN' THEN 1
        WHEN 'CLOSED' THEN 2
        ELSE 1
    END;

    ALTER TABLE t_catering_availability_global DROP COLUMN c_global_status;

    EXEC sp_rename
        't_catering_availability_global.c_global_status_int',
        'c_global_status',
        'COLUMN';

    ALTER TABLE t_catering_availability_global
    ALTER COLUMN c_global_status INT NOT NULL;

    ALTER TABLE t_catering_availability_global
    ADD CONSTRAINT CHK_t_catering_availability_global_status
        CHECK (c_global_status IN (1, 2));

    ALTER TABLE t_catering_availability_global
    ADD CONSTRAINT DF_t_catering_availability_global_status
        DEFAULT (1) FOR c_global_status;

    PRINT 't_catering_availability_global.c_global_status converted from text to int enum values.';
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N't_catering_availability_dates')
      AND name = 'c_status'
      AND system_type_id IN (167, 175, 231, 239)
)
BEGIN
    DECLARE @DateConstraintSql NVARCHAR(MAX) = N'';

    SELECT @DateConstraintSql = @DateConstraintSql +
        N'ALTER TABLE t_catering_availability_dates DROP CONSTRAINT [' + dc.name + N'];'
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N't_catering_availability_dates')
      AND c.name = 'c_status';

    SELECT @DateConstraintSql = @DateConstraintSql +
        N'ALTER TABLE t_catering_availability_dates DROP CONSTRAINT [' + cc.name + N'];'
    FROM sys.check_constraints cc
    WHERE cc.parent_object_id = OBJECT_ID(N't_catering_availability_dates')
      AND cc.definition LIKE '%c_status%';

    IF (@DateConstraintSql <> N'')
        EXEC sp_executesql @DateConstraintSql;

    ALTER TABLE t_catering_availability_dates
    ADD c_status_int INT NULL;

    UPDATE t_catering_availability_dates
    SET c_status_int = CASE UPPER(LTRIM(RTRIM(ISNULL(c_status, 'OPEN'))))
        WHEN 'OPEN' THEN 1
        WHEN 'CLOSED' THEN 2
        WHEN 'FULLY_BOOKED' THEN 3
        ELSE 1
    END;

    ALTER TABLE t_catering_availability_dates DROP COLUMN c_status;

    EXEC sp_rename
        't_catering_availability_dates.c_status_int',
        'c_status',
        'COLUMN';

    ALTER TABLE t_catering_availability_dates
    ALTER COLUMN c_status INT NOT NULL;

    ALTER TABLE t_catering_availability_dates
    ADD CONSTRAINT CHK_t_catering_availability_dates_status
        CHECK (c_status IN (1, 2, 3));

    PRINT 't_catering_availability_dates.c_status converted from text to int enum values.';
END
GO

IF EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS')
BEGIN
    UPDATE t_sys_settings
    SET c_setting_value = '5',
        c_default_value = '5',
        c_modifieddate = GETDATE()
    WHERE c_setting_key = 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS';

    PRINT 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS updated to 5.';
END
ELSE
BEGIN
    INSERT INTO t_sys_settings
    (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_display_order,
        c_default_value,
        c_validation_regex
    )
    VALUES
    (
        'BUSINESS.MIN_ADVANCE_BOOKING_DAYS',
        '5',
        'BUSINESS',
        'NUMBER',
        'Min Advance Booking Days',
        'Minimum days in advance for catering orders',
        17,
        '5',
        '^[1-9][0-9]*$'
    );

    PRINT 'BUSINESS.MIN_ADVANCE_BOOKING_DAYS inserted with value 5.';
END
GO

IF EXISTS (SELECT 1 FROM t_sys_settings WHERE c_setting_key = 'BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY')
BEGIN
    UPDATE t_sys_settings
    SET c_modifieddate = GETDATE()
    WHERE c_setting_key = 'BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY';
END
ELSE
BEGIN
    INSERT INTO t_sys_settings
    (
        c_setting_key,
        c_setting_value,
        c_category,
        c_value_type,
        c_display_name,
        c_description,
        c_display_order,
        c_default_value,
        c_validation_regex
    )
    VALUES
    (
        'BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY',
        '1',
        'BUSINESS',
        'NUMBER',
        'Default Daily Booking Capacity',
        'Fallback booking capacity when a catering profile has no specific daily booking capacity configured',
        18,
        '1',
        '^[1-9][0-9]*$'
    );

    PRINT 'BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY inserted with value 1.';
END
GO
