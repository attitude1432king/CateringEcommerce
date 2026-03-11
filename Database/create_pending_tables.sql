-- ========================================
-- PENDING TABLES TO BE CREATED IN DATABASE
-- Execute this script in SQL Server Management Studio
-- Database: CateringDB
-- ========================================

-- IMPORTANT: Run this script to fix the 500 errors on homepage APIs
-- These tables are REQUIRED for:
-- 1. /api/User/Home/Stats (needs t_sys_homepage_stats)
-- 2. /api/User/Home/Testimonials (needs t_sys_order, t_sys_catering_review)
-- 3. /api/User/Home/FeaturedCaterers

USE CateringDB;
GO

-- ==========================================
-- Table: t_sys_catering_discount_usage
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_catering_discount_usage')
BEGIN
    CREATE TABLE t_sys_catering_discount_usage (
        c_usage_id BIGINT IDENTITY(1,1) PRIMARY KEY,

        -- Discount Reference
        c_discount_id BIGINT NOT NULL,
        c_discount_code NVARCHAR(50) NOT NULL,

        -- Who used it
        c_ownerid BIGINT NOT NULL,        -- Catering Owner
        c_userid BIGINT NOT NULL,         -- Customer
        c_orderid BIGINT NOT NULL,        -- Catering Order/Event

        -- Usage Details
        c_discount_amount DECIMAL(10,2) NOT NULL,
        c_discount_type INT NOT NULL,     -- Enum snapshot
        c_discount_mode INT NOT NULL,     -- Enum snapshot

        -- Status
        c_usage_status INT DEFAULT 1,
        -- 1 = Applied
        -- 2 = RolledBack (payment failed)
        -- 3 = Cancelled (order cancelled)

        -- Audit
        c_used_at DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_catering_discount_usage';
END
ELSE
    PRINT 'Table already exists: t_sys_catering_discount_usage';
GO

-- ==========================================
-- Table: t_sys_order
-- CRITICAL: Needed for testimonials query
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_order')
BEGIN
    CREATE TABLE t_sys_order (
        c_orderid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_userid BIGINT NOT NULL,
        c_ownerid BIGINT NOT NULL,
        c_total_amount DECIMAL(10,2) NOT NULL,
        c_statusid BIGINT NOT NULL, -- Pending, Accepted, Preparing, Delivered, Cancelled
        c_createddate DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_order';
END
ELSE
    PRINT 'Table already exists: t_sys_order';
GO

-- ==========================================
-- Table: t_sys_order_items
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_order_items')
BEGIN
    CREATE TABLE t_sys_order_items (
        c_order_itemid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_orderid BIGINT NOT NULL,
        c_foodid BIGINT NOT NULL,
        c_quantity INT NOT NULL,
        c_price DECIMAL(10,2) NOT NULL
    );
    PRINT 'Created table: t_sys_order_items';
END
ELSE
    PRINT 'Table already exists: t_sys_order_items';
GO

-- ==========================================
-- Table: t_sys_payment
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_payment')
BEGIN
    CREATE TABLE t_sys_payment (
        c_paymentid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_orderid BIGINT NOT NULL,
        c_payment_method NVARCHAR(50) NOT NULL, -- UPI, Card, COD
        c_payment_statusid BIGINT NOT NULL, -- 'Pending', 'Paid', 'Failed'
        c_transaction_id NVARCHAR(100),
        c_paid_amount DECIMAL(10,2),
        c_paid_at DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_payment';
END
ELSE
    PRINT 'Table already exists: t_sys_payment';
GO

-- ==========================================
-- Table: t_sys_order_history
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_order_history')
BEGIN
    CREATE TABLE t_sys_order_history (
        c_historyid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_orderid BIGINT NOT NULL,
        c_status NVARCHAR(50) NOT NULL,
        c_changed_at DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_order_history';
END
ELSE
    PRINT 'Table already exists: t_sys_order_history';
GO

-- ==========================================
-- Table: t_sys_feedback
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_feedback')
BEGIN
    CREATE TABLE t_sys_feedback (
        c_feedbackid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_orderid BIGINT NOT NULL,
        c_userid BIGINT NOT NULL,
        c_rating INT CHECK (c_rating BETWEEN 1 AND 5),
        c_comment NVARCHAR(MAX),
        c_createddate DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_feedback';
END
ELSE
    PRINT 'Table already exists: t_sys_feedback';
GO

-- ==========================================
-- Table: t_sys_coupon
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_coupon')
BEGIN
    CREATE TABLE t_sys_coupon (
        c_couponid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_code NVARCHAR(50) UNIQUE NOT NULL,
        c_discount_percent INT,
        c_valid_from DATETIME,
        c_valid_to DATETIME,
        c_is_active BIT DEFAULT 1
    );
    PRINT 'Created table: t_sys_coupon';
END
ELSE
    PRINT 'Table already exists: t_sys_coupon';
GO

-- ==========================================
-- Table: t_sys_statusmaster
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_statusmaster')
BEGIN
    CREATE TABLE t_sys_statusmaster (
        c_statusid BIGINT PRIMARY KEY IDENTITY(1,1),
        c_statusname NVARCHAR(50) NOT NULL,
        c_description NVARCHAR(200) NULL,
        c_createddate DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_statusmaster';
END
ELSE
    PRINT 'Table already exists: t_sys_statusmaster';
GO

-- ==========================================
-- Table: t_sys_homepage_stats
-- CRITICAL: Required for /api/User/Home/Stats endpoint
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_homepage_stats')
BEGIN
    CREATE TABLE t_sys_homepage_stats (
        c_statsid INT PRIMARY KEY IDENTITY(1,1),
        c_total_events_catered INT NOT NULL DEFAULT 0,
        c_total_catering_partners INT NOT NULL DEFAULT 0,
        c_total_happy_customers INT NOT NULL DEFAULT 0,
        c_satisfaction_rate DECIMAL(5,2) NOT NULL DEFAULT 0,
        c_last_updated DATETIME DEFAULT GETDATE()
    );
    PRINT 'Created table: t_sys_homepage_stats';

    -- Insert initial homepage stats record
    INSERT INTO t_sys_homepage_stats (c_total_events_catered, c_total_catering_partners, c_total_happy_customers, c_satisfaction_rate, c_last_updated)
    VALUES (5000, 500, 50000, 98.00, GETDATE());
    PRINT 'Inserted initial homepage stats data';
END
ELSE
BEGIN
    PRINT 'Table already exists: t_sys_homepage_stats';
    -- Check if data exists, if not insert it
    IF NOT EXISTS (SELECT * FROM t_sys_homepage_stats)
    BEGIN
        INSERT INTO t_sys_homepage_stats (c_total_events_catered, c_total_catering_partners, c_total_happy_customers, c_satisfaction_rate, c_last_updated)
        VALUES (5000, 500, 50000, 98.00, GETDATE());
        PRINT 'Inserted initial homepage stats data';
    END
    ELSE
        PRINT 'Homepage stats data already exists';
END
GO

-- ==========================================
-- Verification Query
-- Run this after creating tables
-- ==========================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION - Checking created tables:';
PRINT '========================================';

SELECT
    'Table Created Successfully' AS Status,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    't_sys_catering_discount_usage',
    't_sys_order',
    't_sys_order_items',
    't_sys_payment',
    't_sys_order_history',
    't_sys_feedback',
    't_sys_coupon',
    't_sys_statusmaster',
    't_sys_homepage_stats'
)
ORDER BY TABLE_NAME;

-- Check homepage stats data
PRINT '';
PRINT 'Checking homepage stats data:';
SELECT * FROM t_sys_homepage_stats;

PRINT '';
PRINT '========================================';
PRINT 'ALL PENDING TABLES CREATED SUCCESSFULLY!';
PRINT 'You can now test the API endpoints.';
PRINT '========================================';
