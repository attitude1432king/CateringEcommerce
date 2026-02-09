-- =============================================
-- Master Data Management - Guest Category Migration
-- Creates t_sys_guest_category table
-- =============================================

-- Create Guest Category Table
CREATE TABLE t_sys_guest_category (
    c_guest_category_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_category_name NVARCHAR(100) NOT NULL,
    c_description NVARCHAR(500) NULL,
    c_display_order INT NOT NULL DEFAULT 0,
    c_is_active BIT NOT NULL DEFAULT 1,
    c_created_date DATETIME NOT NULL DEFAULT GETDATE(),
    c_created_by BIGINT NULL,
    c_modified_date DATETIME NULL,
    c_modified_by BIGINT NULL,
    c_is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_GuestCategory_Name UNIQUE (c_category_name)
);

-- Create index for active status and display order
CREATE NONCLUSTERED INDEX IX_GuestCategory_Active
    ON t_sys_guest_category(c_is_active, c_display_order)
    WHERE c_is_deleted = 0;

-- Insert default data
INSERT INTO t_sys_guest_category (c_category_name, c_description, c_display_order, c_is_active)
VALUES
    ('50-100 Guests', 'Small gatherings and intimate events', 1, 1),
    ('100-200 Guests', 'Medium-sized events and celebrations', 2, 1),
    ('200-500 Guests', 'Large events and parties', 3, 1),
    ('500-1000 Guests', 'Very large events and corporate functions', 4, 1),
    ('1000+ Guests', 'Mass gatherings and mega events', 5, 1);

GO

-- Verification query
SELECT * FROM t_sys_guest_category ORDER BY c_display_order;
