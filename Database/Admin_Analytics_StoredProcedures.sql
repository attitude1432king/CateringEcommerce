-- ===============================================
-- Admin Analytics Stored Procedures
-- Comprehensive analytics for admin dashboard
-- ===============================================

USE CateringEcommerce;
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
    SELECT @TotalUsers = COUNT(*) FROM tbl_User WHERE c_createddate <= @ToDate AND c_isactive = 1;
    SELECT @PrevTotalUsers = COUNT(*) FROM tbl_User WHERE c_createddate <= @PrevToDate AND c_isactive = 1;

    -- Active Caterings (Current and Previous Period)
    DECLARE @ActiveCaterings INT, @PrevActiveCaterings INT;
    SELECT @ActiveCaterings = COUNT(*) FROM tbl_CateringOwners WHERE c_isactive = 1 AND c_isapproved = 1;
    SELECT @PrevActiveCaterings = COUNT(*) FROM tbl_CateringOwners WHERE c_createddate <= @PrevToDate AND c_isactive = 1 AND c_isapproved = 1;

    -- Total Orders (Current and Previous Period)
    DECLARE @TotalOrders INT, @PrevTotalOrders INT;
    SELECT @TotalOrders = COUNT(*)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate;

    SELECT @PrevTotalOrders = COUNT(*)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @PrevFromDate AND @PrevToDate;

    -- Total Revenue (Current and Previous Period)
    DECLARE @TotalRevenue DECIMAL(18,2), @PrevTotalRevenue DECIMAL(18,2);
    SELECT @TotalRevenue = ISNULL(SUM(c_totalamount), 0)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        AND c_paymentstatus IN ('Completed', 'Paid');

    SELECT @PrevTotalRevenue = ISNULL(SUM(c_totalamount), 0)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @PrevFromDate AND @PrevToDate
        AND c_paymentstatus IN ('Completed', 'Paid');

    -- Total Commission
    DECLARE @TotalCommission DECIMAL(18,2);
    SELECT @TotalCommission = ISNULL(SUM(c_commissionamount), 0)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        AND c_paymentstatus IN ('Completed', 'Paid');

    -- Average Order Value
    DECLARE @AvgOrderValue DECIMAL(18,2);
    SELECT @AvgOrderValue = ISNULL(AVG(c_totalamount), 0)
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate;

    -- Pending Partner Approvals
    DECLARE @PendingApprovals INT;
    SELECT @PendingApprovals = COUNT(*)
    FROM tbl_CateringOwners
    WHERE c_approvalstatus = 'Pending';

    -- Average Platform Rating
    DECLARE @AvgRating DECIMAL(3,2);
    SELECT @AvgRating = ISNULL(AVG(CAST(c_rating AS DECIMAL(3,2))), 0)
    FROM tbl_Reviews
    WHERE c_isactive = 1;

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
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_totalamount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_commissionamount ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM tbl_Orders
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY CAST(c_createddate AS DATE)
        ORDER BY Date;
    END
    ELSE IF @Granularity = 'week'
    BEGIN
        SELECT
            DATEADD(WEEK, DATEDIFF(WEEK, 0, c_createddate), 0) AS Date,
            'Week ' + CAST(DATEPART(WEEK, c_createddate) AS VARCHAR(2)) AS Label,
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_totalamount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_commissionamount ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM tbl_Orders
        WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY DATEADD(WEEK, DATEDIFF(WEEK, 0, c_createddate), 0), DATEPART(WEEK, c_createddate)
        ORDER BY Date;
    END
    ELSE IF @Granularity = 'month'
    BEGIN
        SELECT
            DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1) AS Date,
            FORMAT(DATEFROMPARTS(YEAR(c_createddate), MONTH(c_createddate), 1), 'MMM yyyy') AS Label,
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_totalamount ELSE 0 END), 0) AS Revenue,
            ISNULL(SUM(CASE WHEN c_paymentstatus IN ('Completed', 'Paid') THEN c_commissionamount ELSE 0 END), 0) AS Commission,
            COUNT(*) AS OrderCount
        FROM tbl_Orders
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
        c_orderstatus AS Status,
        COUNT(*) AS Count,
        CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM tbl_Orders WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate) AS DECIMAL(5,2)) AS Percentage
    FROM tbl_Orders
    WHERE CAST(c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    GROUP BY c_orderstatus
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
        co.c_cateringownerid AS CateringOwnerId,
        co.c_businessname AS BusinessName,
        co.c_contactperson AS ContactPerson,
        c.c_cityname AS City,
        COUNT(o.c_orderid) AS TotalOrders,
        ISNULL(SUM(CASE WHEN o.c_paymentstatus IN ('Completed', 'Paid') THEN o.c_totalamount ELSE 0 END), 0) AS TotalRevenue,
        ISNULL(AVG(CAST(r.c_rating AS DECIMAL(3,2))), 0) AS AverageRating,
        COUNT(DISTINCT o.c_userid) AS UniqueCustomers
    FROM tbl_CateringOwners co
    LEFT JOIN tbl_Orders o ON co.c_cateringownerid = o.c_cateringownerid
        AND CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    LEFT JOIN tbl_Reviews r ON co.c_cateringownerid = r.c_cateringownerid AND r.c_isactive = 1
    LEFT JOIN tbl_City c ON co.c_cityid = c.c_cityid
    WHERE co.c_isactive = 1 AND co.c_isapproved = 1
    GROUP BY co.c_cateringownerid, co.c_businessname, co.c_contactperson, c.c_cityname
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
        o.c_ordernumber AS OrderNumber,
        u.c_firstname + ' ' + u.c_lastname AS CustomerName,
        u.c_email AS CustomerEmail,
        co.c_businessname AS CateringName,
        o.c_totalamount AS TotalAmount,
        o.c_orderstatus AS OrderStatus,
        o.c_paymentstatus AS PaymentStatus,
        o.c_eventdate AS EventDate,
        o.c_createddate AS OrderDate
    FROM tbl_Orders o
    INNER JOIN tbl_User u ON o.c_userid = u.c_userid
    INNER JOIN tbl_CateringOwners co ON o.c_cateringownerid = co.c_cateringownerid
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

    SELECT TOP (@Limit)
        fc.c_foodcategoryid AS CategoryId,
        fc.c_categoryname AS CategoryName,
        COUNT(DISTINCT oi.c_orderid) AS OrderCount,
        SUM(oi.c_quantity) AS TotalQuantity,
        ISNULL(SUM(oi.c_price * oi.c_quantity), 0) AS TotalRevenue
    FROM tbl_FoodCategory fc
    INNER JOIN tbl_MenuItem mi ON fc.c_foodcategoryid = mi.c_foodcategoryid
    INNER JOIN tbl_OrderItem oi ON mi.c_menuitemid = oi.c_menuitemid
    INNER JOIN tbl_Orders o ON oi.c_orderid = o.c_orderid
    WHERE CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
        AND fc.c_isactive = 1
    GROUP BY fc.c_foodcategoryid, fc.c_categoryname
    ORDER BY TotalRevenue DESC;
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
        FROM tbl_User
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
        FROM tbl_User
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
        ISNULL(SUM(CASE WHEN o.c_paymentstatus IN ('Completed', 'Paid') THEN o.c_totalamount ELSE 0 END), 0) AS TotalRevenue,
        COUNT(DISTINCT co.c_cateringownerid) AS ActivePartners
    FROM tbl_City c
    INNER JOIN tbl_CateringOwners co ON c.c_cityid = co.c_cityid
    LEFT JOIN tbl_Orders o ON co.c_cateringownerid = o.c_cateringownerid
        AND CAST(o.c_createddate AS DATE) BETWEEN @FromDate AND @ToDate
    WHERE c.c_isactive = 1
    GROUP BY c.c_cityid, c.c_cityname
    HAVING COUNT(o.c_orderid) > 0
    ORDER BY TotalRevenue DESC;
END
GO

PRINT 'Admin Analytics Stored Procedures created successfully!';
