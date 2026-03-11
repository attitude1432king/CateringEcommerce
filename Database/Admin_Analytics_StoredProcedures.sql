-- ===============================================
-- Admin Analytics Stored Procedures
-- Comprehensive analytics for admin dashboard
-- ===============================================

USE CateringDB;
GO

-- ===============================================
-- 1. Get Admin Dashboard Metrics with Date Range
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetDashboardMetrics]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default date range: last 30 days
    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    -- Calculate previous period for comparison
    DECLARE @DaysDiff INT = DATEDIFF(DAY, @FromDate, @ToDate);
    DECLARE @PrevFromDate DATE = DATEADD(DAY, -@DaysDiff, @FromDate);
    DECLARE @PrevToDate DATE = @FromDate;

    -- Total Users (Current and Previous Period)
    DECLARE @TotalUsers INT, @PrevTotalUsers INT;
    SELECT @TotalUsers = COUNT(*) FROM t_sys_user WHERE c_createddate <= @ToDate AND c_isactive = 1;
    SELECT @PrevTotalUsers = COUNT(*) FROM t_sys_user WHERE c_createddate <= @PrevToDate AND c_isactive = 1;

    -- Active Caterings (Current and Previous Period)
    DECLARE @ActiveCaterings INT, @PrevActiveCaterings INT;
    SELECT @ActiveCaterings = COUNT(*) FROM t_sys_catering_owner WHERE c_isactive = 1 AND c_approval_status = 2;
    SELECT @PrevActiveCaterings = COUNT(*) FROM t_sys_catering_owner WHERE c_createddate <= @PrevToDate AND c_isactive = 1 AND c_approval_status = 2;

    -- Total Orders (Current and Previous Period)
    DECLARE @TotalOrders INT, @PrevTotalOrders INT;
    SELECT @TotalOrders = COUNT(*)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate;

    SELECT @PrevTotalOrders = COUNT(*)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @PrevFromDate AND @PrevToDate;

    -- Total Revenue (Current and Previous Period)
    DECLARE @TotalRevenue DECIMAL(18,2), @PrevTotalRevenue DECIMAL(18,2);
    SELECT @TotalRevenue = ISNULL(SUM(c_total_amount), 0)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        AND c_payment_status IN ('Completed', 'Paid');

    SELECT @PrevTotalRevenue = ISNULL(SUM(c_total_amount), 0)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @PrevFromDate AND @PrevToDate
        AND c_payment_status IN ('Completed', 'Paid');

    -- Total Commission
    DECLARE @TotalCommission DECIMAL(18,2);
    SELECT @TotalCommission = ISNULL(SUM(c_commission_rate), 0)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        AND c_payment_status IN ('Completed', 'Paid');

    -- Average Order Value
    DECLARE @AvgOrderValue DECIMAL(18,2);
    SELECT @AvgOrderValue = ISNULL(AVG(c_total_amount), 0)
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate;

    -- Pending Partner Approvals
    DECLARE @PendingApprovals INT;
    SELECT @PendingApprovals = COUNT(*)
    FROM t_sys_catering_owner
    WHERE c_approval_status = 1;

    -- Average Platform Rating
    DECLARE @AvgRating DECIMAL(3,2);
    SELECT @AvgRating = ISNULL(AVG(CAST(c_overall_rating AS DECIMAL(3,2))), 0)
    FROM t_sys_catering_review
    WHERE c_is_verified = 1;

    -- Calculate percentage changes
    DECLARE @UsersChange DECIMAL(5,2) =
        CASE WHEN @PrevTotalUsers > 0
        THEN ((@TotalUsers - @PrevTotalUsers) * 100.0 / @PrevTotalUsers)
        ELSE 0 END;

    DECLARE @CateringsChange DECIMAL(5,2) =
        CASE WHEN @PrevActiveCaterings > 0
        THEN ((@ActiveCaterings - @PrevActiveCaterings) * 100.0 / @PrevActiveCaterings)
        ELSE 0 END;

    DECLARE @OrdersChange DECIMAL(5,2) =
        CASE WHEN @PrevTotalOrders > 0
        THEN ((@TotalOrders - @PrevTotalOrders) * 100.0 / @PrevTotalOrders)
        ELSE 0 END;

    DECLARE @RevenueChange DECIMAL(5,2) =
        CASE WHEN @PrevTotalRevenue > 0
        THEN ((@TotalRevenue - @PrevTotalRevenue) * 100.0 / @PrevTotalRevenue)
        ELSE 0 END;

    -- Return Main Metrics
    SELECT
        @TotalUsers AS TotalUsers,
        @UsersChange AS UsersChangePercent,
        @ActiveCaterings AS ActiveCaterings,
        @CateringsChange AS CateringsChangePercent,
        @TotalOrders AS TotalOrders,
        @OrdersChange AS OrdersChangePercent,
        @TotalRevenue AS TotalRevenue,
        @RevenueChange AS RevenueChangePercent,
        @TotalCommission AS TotalCommission,
        @AvgOrderValue AS AverageOrderValue,
        @PendingApprovals AS PendingApprovals,
        @AvgRating AS AverageRating,
        @FromDate AS PeriodStart,
        @ToDate AS PeriodEnd;
END
GO

-- ===============================================
-- 2. Get Revenue Chart Data
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetRevenueChart]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Granularity VARCHAR(10) = 'day' -- 'day', 'week', 'month'
AS
BEGIN
    SET NOCOUNT ON;

    -- Default date range: last 30 days
    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    IF @Granularity = 'day'
    BEGIN
        SELECT
            CAST(c_createddate AS DATE) AS Date,
            FORMAT(CAST(c_createddate AS DATE), 'MMM dd') AS Label,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY CAST(c_createddate AS DATE)
        ORDER BY Date;
    END
    ELSE IF @Granularity = 'week'
    BEGIN
        SELECT
            DATEADD(WEEK, DATEDIFF(WEEK, 0, c_createddate), 0) AS Date,
            'Week ' + CAST(DATEPART(WEEK, c_createddate) AS VARCHAR(2)) AS Label,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY DATEADD(WEEK, DATEDIFF(WEEK, 0, c_createddate), 0), DATEPART(WEEK, c_createddate)
        ORDER BY Date;
    END
    ELSE IF @Granularity = 'month'
    BEGIN
        SELECT
            DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1) AS Date,
            FORMAT(DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1), 'MMM yyyy') AS Label,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY YEAR(c_createddate), MONTH(c_createddate)
        ORDER BY Date;
    END
END
GO

-- ===============================================
-- 3. Get Order Status Distribution
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetOrderStatusDistribution]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    SELECT
        c_order_status AS Status,
        COUNT(*) AS Count,
        CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM t_sys_orders WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate) AS DECIMAL(5,2)) AS Percentage
    FROM t_sys_orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    GROUP BY c_order_status
    ORDER BY Count DESC;
END
GO

-- ===============================================
-- 4. Get Top Performing Partners
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetTopPerformingPartners]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    SELECT TOP (@Limit)
        co.c_ownerid AS CateringOwnerId,
        co.c_catering_name AS BusinessName,
        co.c_mobile AS ContactPerson,
        c.c_cityname AS City,
        COUNT(o.c_orderid) AS TotalOrders,
        ISNULL(SUM(CASE WHEN o.c_payment_status IN ('Completed', 'Paid') THEN o.c_total_amount ELSE 0 END), 0) AS TotalRevenue,
        ISNULL(AVG(CAST(r.c_overall_rating AS DECIMAL(3,2))), 0) AS AverageRating,
        COUNT(DISTINCT o.c_userid) AS UniqueCustomers
    FROM t_sys_catering_owner co
    LEFT JOIN t_sys_orders o ON co.c_ownerid = o.c_ownerid
    LEFT JOIN t_sys_catering_owner_addresses ad ON co.c_ownerid = ad.c_ownerid
        AND CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    LEFT JOIN t_sys_catering_review r ON co.c_ownerid = r.c_ownerid AND r.c_is_verified = 1
    LEFT JOIN t_sys_city c ON ad.c_cityid = c.c_cityid
    WHERE co.c_isactive = 1 AND co.c_approval_status = 2
    GROUP BY co.c_ownerid, co.c_catering_name, co.c_mobile, c.c_cityname
    HAVING COUNT(o.c_orderid) > 0
    ORDER BY TotalRevenue DESC;
END
GO

-- ===============================================
-- 5. Get Recent Orders for Admin
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetRecentOrders]
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Limit)
        o.c_orderid AS OrderId,
        o.c_order_number AS OrderNumber,
        u.c_name AS CustomerName,
        u.c_email AS CustomerEmail,
        co.c_catering_name AS CateringName,
        o.c_total_amount AS TotalAmount,
        o.c_order_status AS OrderStatus,
        o.c_payment_status AS PaymentStatus,
        o.c_event_date AS EventDate,
        o.c_createddate AS OrderDate
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    ORDER BY o.c_createddate DESC;
END
GO

-- ===============================================
-- 6. Get Popular Food Categories
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetPopularCategories]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    DECLARE @HasItemType BIT = CASE
        WHEN COL_LENGTH('dbo.t_sys_order_items', 'c_item_type') IS NOT NULL
         AND COL_LENGTH('dbo.t_sys_order_items', 'c_item_id') IS NOT NULL
        THEN 1 ELSE 0 END;

    DECLARE @HasFoodId BIT = CASE
        WHEN COL_LENGTH('dbo.t_sys_order_items', 'c_foodid') IS NOT NULL
        THEN 1 ELSE 0 END;

    IF @HasItemType = 1
    BEGIN
        DECLARE @ModernSql NVARCHAR(MAX) = N'
            ;WITH OrderLines AS (
                SELECT
                    oi.c_orderid,
                    UPPER(LTRIM(RTRIM(oi.c_item_type))) AS ItemType,
                    oi.c_item_id AS ItemId,
                    oi.c_quantity AS LineQuantity,
                    CAST(
                        ISNULL(
                            oi.c_total_price,
                            oi.c_unit_price * oi.c_quantity
                        ) AS DECIMAL(18,2)
                    ) AS LineRevenue
                FROM t_sys_order_items oi
                INNER JOIN t_sys_orders o ON oi.c_orderid = o.c_orderid
                WHERE CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
            ),
            PackageCategoryWeight AS (
                SELECT
                    pi.c_packageid,
                    pi.c_categoryid,
                    CAST(pi.c_quantity AS DECIMAL(18,6)) AS CategoryQty,
                    SUM(CAST(pi.c_quantity AS DECIMAL(18,6))) OVER (PARTITION BY pi.c_packageid) AS TotalCategoryQty
                FROM t_sys_catering_package_items pi
            ),
            CategoryRevenue AS (
                SELECT
                    ol.c_orderid AS OrderId,
                    fi.c_categoryid AS CategoryId,
                    CAST(ol.LineQuantity AS DECIMAL(18,2)) AS QuantityContribution,
                    CAST(ol.LineRevenue AS DECIMAL(18,2)) AS RevenueContribution
                FROM OrderLines ol
                INNER JOIN t_sys_fooditems fi ON fi.c_foodid = ol.ItemId
                WHERE ol.ItemType IN (''FOOD_ITEM'', ''FOOD'')
                  AND fi.c_categoryid IS NOT NULL

                UNION ALL

                SELECT
                    ol.c_orderid AS OrderId,
                    pcw.c_categoryid AS CategoryId,
                    CAST(ol.LineQuantity * CASE
                        WHEN pcw.TotalCategoryQty > 0 THEN pcw.CategoryQty / pcw.TotalCategoryQty
                        ELSE 0
                    END AS DECIMAL(18,2)) AS QuantityContribution,
                    CAST(ol.LineRevenue * CASE
                        WHEN pcw.TotalCategoryQty > 0 THEN pcw.CategoryQty / pcw.TotalCategoryQty
                        ELSE 0
                    END AS DECIMAL(18,2)) AS RevenueContribution
                FROM OrderLines ol
                INNER JOIN t_sys_catering_packages p ON p.c_packageid = ol.ItemId
                INNER JOIN PackageCategoryWeight pcw ON pcw.c_packageid = p.c_packageid
                WHERE ol.ItemType = ''PACKAGE''
            )
            SELECT TOP (@Limit)
                fc.c_categoryid AS CategoryId,
                fc.c_categoryname AS CategoryName,
                COUNT(DISTINCT cr.OrderId) AS OrderCount,
                CAST(ISNULL(SUM(cr.QuantityContribution), 0) AS DECIMAL(18,2)) AS TotalQuantity,
                CAST(ISNULL(SUM(cr.RevenueContribution), 0) AS DECIMAL(18,2)) AS TotalRevenue
            FROM t_sys_food_category fc
            INNER JOIN CategoryRevenue cr ON cr.CategoryId = fc.c_categoryid
            WHERE fc.c_isactive = 1
            GROUP BY fc.c_categoryid, fc.c_categoryname
            ORDER BY TotalRevenue DESC;';

        EXEC sp_executesql
            @ModernSql,
            N'@FromDate DATE, @ToDate DATE, @Limit INT',
            @FromDate = @FromDate,
            @ToDate = @ToDate,
            @Limit = @Limit;
    END
    ELSE IF @HasFoodId = 1
    BEGIN
        DECLARE @LegacySql NVARCHAR(MAX) = N'
            SELECT TOP (@Limit)
                fc.c_categoryid AS CategoryId,
                fc.c_categoryname AS CategoryName,
                COUNT(DISTINCT oi.c_orderid) AS OrderCount,
                CAST(ISNULL(SUM(oi.c_quantity), 0) AS DECIMAL(18,2)) AS TotalQuantity,
                CAST(ISNULL(SUM(oi.c_price * oi.c_quantity), 0) AS DECIMAL(18,2)) AS TotalRevenue
            FROM t_sys_food_category fc
            INNER JOIN t_sys_fooditems fi ON fi.c_categoryid = fc.c_categoryid
            INNER JOIN t_sys_order_items oi ON oi.c_foodid = fi.c_foodid
            INNER JOIN t_sys_orders o ON oi.c_orderid = o.c_orderid
            WHERE CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
              AND fc.c_isactive = 1
            GROUP BY fc.c_categoryid, fc.c_categoryname
            ORDER BY TotalRevenue DESC;';

        EXEC sp_executesql
            @LegacySql,
            N'@FromDate DATE, @ToDate DATE, @Limit INT',
            @FromDate = @FromDate,
            @ToDate = @ToDate,
            @Limit = @Limit;
    END
    ELSE
    BEGIN
        -- Safety fallback when neither known order-item shape exists
        SELECT TOP (@Limit)
            fc.c_categoryid AS CategoryId,
            fc.c_categoryname AS CategoryName,
            CAST(0 AS INT) AS OrderCount,
            CAST(0 AS DECIMAL(18,2)) AS TotalQuantity,
            CAST(0 AS DECIMAL(18,2)) AS TotalRevenue
        FROM t_sys_food_category fc
        WHERE fc.c_isactive = 1
        ORDER BY fc.c_categoryid;
    END
END
GO

-- ===============================================
-- 7. Get User Growth Analytics
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetUserGrowth]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Granularity VARCHAR(10) = 'day'
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    IF @Granularity = 'day'
    BEGIN
        SELECT
            CAST(c_createddate AS DATE) AS Date,
            FORMAT(CAST(c_createddate AS DATE), 'MMM dd') AS Label,
            COUNT(*) AS NewUsers,
            SUM(COUNT(*)) OVER (ORDER BY CAST(c_createddate AS DATE)) AS CumulativeUsers
        FROM t_sys_user
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY CAST(c_createddate AS DATE)
        ORDER BY Date;
    END
    ELSE IF @Granularity = 'month'
    BEGIN
        SELECT
            DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1) AS Date,
            FORMAT(DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1), 'MMM yyyy') AS Label,
            COUNT(*) AS NewUsers,
            SUM(COUNT(*)) OVER (ORDER BY YEAR(c_createddate), MONTH(c_createddate)) AS CumulativeUsers
        FROM t_sys_user
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY YEAR(c_createddate), MONTH(c_createddate)
        ORDER BY Date;
    END
END
GO

-- ===============================================
-- 8. Get Revenue by City
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetRevenueByCity]
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = DATEADD(DAY, -30, GETDATE());

    IF @ToDate IS NULL
        SET @ToDate = GETDATE();

    SELECT TOP (@Limit)
        c.c_cityid AS CityId,
        c.c_cityname AS CityName,
        COUNT(o.c_orderid) AS TotalOrders,
        ISNULL(SUM(CASE WHEN o.c_payment_status IN ('Completed', 'Paid') THEN o.c_total_amount ELSE 0 END), 0) AS TotalRevenue,
        COUNT(DISTINCT co.c_ownerid) AS ActivePartners
    FROM t_sys_city c
    INNER JOIN t_sys_catering_owner_addresses co ON c.c_cityid = co.c_cityid
    LEFT JOIN t_sys_orders o ON co.c_ownerid = o.c_ownerid
        AND CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    WHERE c.c_isactive = 1
    GROUP BY c.c_cityid, c.c_cityname
    HAVING COUNT(o.c_orderid) > 0
    ORDER BY TotalRevenue DESC;
END
GO

PRINT 'Admin Analytics Stored Procedures created successfully!';
