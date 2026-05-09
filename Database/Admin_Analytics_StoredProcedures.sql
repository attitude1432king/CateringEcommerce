-- ===============================================
-- Admin Analytics Stored Procedures
-- Comprehensive analytics for admin dashboard
-- ===============================================

CREATE OR REPLACE FUNCTION sp_admin_getdashboardmetrics(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate   TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    "TotalUsers"              INTEGER,
    "UsersChangePercent"      DECIMAL(5,2),
    "ActiveCaterings"         INTEGER,
    "CateringsChangePercent"  DECIMAL(5,2),
    "TotalOrders"             INTEGER,
    "OrdersChangePercent"     DECIMAL(5,2),
    "TotalRevenue"            DECIMAL(18,2),
    "RevenueChangePercent"    DECIMAL(5,2),
    "TotalCommission"         DECIMAL(18,2),
    "AverageOrderValue"       DECIMAL(18,2),
    "PendingApprovals"        INTEGER,
    "AverageRating"           DECIMAL(3,2),
    "PeriodStart"             DATE,
    "PeriodEnd"               DATE
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
    v_DaysDiff INTEGER;
    v_PrevFromDate DATE;
    v_PrevToDate   DATE;

    v_TotalUsers INTEGER;
    v_PrevTotalUsers INTEGER;
    v_ActiveCaterings INTEGER;
    v_PrevActiveCaterings INTEGER;
    v_TotalOrders INTEGER;
    v_PrevTotalOrders INTEGER;
    v_TotalRevenue DECIMAL(18,2);
    v_PrevTotalRevenue DECIMAL(18,2);
    v_TotalCommission DECIMAL(18,2);
    v_AvgOrderValue DECIMAL(18,2);
    v_PendingApprovals INTEGER;
    v_AvgRating DECIMAL(3,2);

    v_UsersChange DECIMAL(5,2);
    v_CateringsChange DECIMAL(5,2);
    v_OrdersChange DECIMAL(5,2);
    v_RevenueChange DECIMAL(5,2);
BEGIN
    -- Convert TIMESTAMP → DATE
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    v_DaysDiff     := v_ToDate - v_FromDate;
    v_PrevFromDate := v_FromDate - v_DaysDiff;
    v_PrevToDate   := v_FromDate;

    -- Users
    SELECT COUNT(*) INTO v_TotalUsers
    FROM t_sys_user
    WHERE c_createddate::DATE <= v_ToDate AND c_isactive = TRUE;

    SELECT COUNT(*) INTO v_PrevTotalUsers
    FROM t_sys_user
    WHERE c_createddate::DATE <= v_PrevToDate AND c_isactive = TRUE;

    -- Caterings
    SELECT COUNT(*) INTO v_ActiveCaterings
    FROM t_sys_catering_owner
    WHERE c_isactive = TRUE AND c_approval_status = 2;

    SELECT COUNT(*) INTO v_PrevActiveCaterings
    FROM t_sys_catering_owner
    WHERE c_createddate::DATE <= v_PrevToDate
      AND c_isactive = TRUE AND c_approval_status = 2;

    -- Orders
    SELECT COUNT(*) INTO v_TotalOrders
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate;

    SELECT COUNT(*) INTO v_PrevTotalOrders
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_PrevFromDate AND v_PrevToDate;

    -- Revenue
    SELECT COALESCE(SUM(c_total_amount), 0) INTO v_TotalRevenue
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
      AND c_payment_status IN ('Completed', 'Paid');

    SELECT COALESCE(SUM(c_total_amount), 0) INTO v_PrevTotalRevenue
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_PrevFromDate AND v_PrevToDate
      AND c_payment_status IN ('Completed', 'Paid');

    -- Commission
    SELECT COALESCE(SUM(c_commission_rate), 0) INTO v_TotalCommission
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
      AND c_payment_status IN ('Completed', 'Paid');

    -- Avg Order
    SELECT COALESCE(AVG(c_total_amount), 0) INTO v_AvgOrderValue
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate;

    -- Pending
    SELECT COUNT(*) INTO v_PendingApprovals
    FROM t_sys_catering_owner
    WHERE c_approval_status = 1;

    -- Rating
    SELECT COALESCE(AVG(c_overall_rating::DECIMAL(3,2)), 0) INTO v_AvgRating
    FROM t_sys_catering_review
    WHERE c_is_verified = TRUE;

    -- Changes
    v_UsersChange := CASE WHEN v_PrevTotalUsers > 0
        THEN (v_TotalUsers - v_PrevTotalUsers) * 100.0 / v_PrevTotalUsers ELSE 0 END;

    v_CateringsChange := CASE WHEN v_PrevActiveCaterings > 0
        THEN (v_ActiveCaterings - v_PrevActiveCaterings) * 100.0 / v_PrevActiveCaterings ELSE 0 END;

    v_OrdersChange := CASE WHEN v_PrevTotalOrders > 0
        THEN (v_TotalOrders - v_PrevTotalOrders) * 100.0 / v_PrevTotalOrders ELSE 0 END;

    v_RevenueChange := CASE WHEN v_PrevTotalRevenue > 0
        THEN (v_TotalRevenue - v_PrevTotalRevenue) * 100.0 / v_PrevTotalRevenue ELSE 0 END;

    RETURN QUERY
    SELECT
        v_TotalUsers,
        v_UsersChange,
        v_ActiveCaterings,
        v_CateringsChange,
        v_TotalOrders,
        v_OrdersChange,
        v_TotalRevenue,
        v_RevenueChange,
        v_TotalCommission,
        v_AvgOrderValue,
        v_PendingApprovals,
        v_AvgRating,
        v_FromDate,
        v_ToDate;
END;
$$;


-- ===============================================
-- 2. Get Revenue Chart Data
-- ===============================================
CREATE OR REPLACE FUNCTION sp_Admin_GetRevenueChart(
    p_FromDate    TIMESTAMP DEFAULT NULL,
    p_ToDate      TIMESTAMP DEFAULT NULL,
    p_Granularity VARCHAR(10) DEFAULT 'day'
)
RETURNS TABLE (
    Date        DATE,
    Label       TEXT,
    Revenue     DECIMAL(18,2),
    Commission  DECIMAL(18,2),
    OrderCount  BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
BEGIN
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
	    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    IF p_Granularity = 'day' THEN
        RETURN QUERY
        SELECT
            c_createddate::DATE AS Date,
            TO_CHAR(c_createddate::DATE, 'Mon DD') AS Label,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS Revenue,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0)::DECIMAL(18,2) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        GROUP BY c_createddate::DATE
        ORDER BY Date;

    ELSIF p_Granularity = 'week' THEN
        RETURN QUERY
        SELECT
            DATE_TRUNC('week', c_createddate)::DATE AS Date,
            'Week ' || TO_CHAR(c_createddate, 'IW') AS Label,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS Revenue,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0)::DECIMAL(18,2) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        GROUP BY DATE_TRUNC('week', c_createddate)::DATE, TO_CHAR(c_createddate, 'IW')
        ORDER BY Date;

    ELSIF p_Granularity = 'month' THEN
        RETURN QUERY
        SELECT
            DATE_TRUNC('month', c_createddate)::DATE AS Date,
            TO_CHAR(DATE_TRUNC('month', c_createddate), 'Mon YYYY') AS Label,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS Revenue,
            COALESCE(SUM(CASE WHEN c_payment_status IN ('Completed', 'Paid') THEN c_commission_rate ELSE 0 END), 0)::DECIMAL(18,2) AS Commission,
            COUNT(*) AS OrderCount
        FROM t_sys_orders
        WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        GROUP BY DATE_TRUNC('month', c_createddate)
        ORDER BY Date;
    END IF;
END;
$$;


-- ===============================================
-- 3. Get Order Status Distribution
-- ===============================================
CREATE OR REPLACE FUNCTION sp_Admin_GetOrderStatusDistribution(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate   TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    "Status"      TEXT,
    "Count"       BIGINT,
    "Percentage"  DECIMAL(5,2)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
BEGIN
    -- Convert TIMESTAMP → DATE safely
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    RETURN QUERY
    SELECT
        c_order_status::TEXT AS "Status",
        COUNT(*) AS "Count",
        (
            COUNT(*) * 100.0 / NULLIF((
                SELECT COUNT(*)
                FROM t_sys_orders
                WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
            ), 0)
        )::DECIMAL(5,2) AS "Percentage"
    FROM t_sys_orders
    WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
    GROUP BY c_order_status
    ORDER BY "Count" DESC;
END;
$$;


-- ===============================================
-- 4. Get Top Performing Partners
-- ===============================================

DROP FUNCTION sp_Admin_GetTopPerformingPartners(timestamp, timestamp, integer);

CREATE OR REPLACE FUNCTION sp_Admin_GetTopPerformingPartners(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate   TIMESTAMP DEFAULT NULL,
    p_Limit    INTEGER DEFAULT 10
)
RETURNS TABLE (
    "CateringOwnerId" BIGINT,
    "BusinessName"    TEXT,
    "ContactPerson"   TEXT,
    "City"            TEXT,
    "TotalOrders"     BIGINT,
    "TotalRevenue"    DECIMAL(18,2),
    "AverageRating"   DECIMAL(3,2),
    "UniqueCustomers" BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
BEGIN
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    RETURN QUERY
    SELECT
        co.c_ownerid AS "CateringOwnerId",
        co.c_catering_name::TEXT AS "BusinessName",
        co.c_mobile::TEXT AS "ContactPerson",
        c.c_cityname::TEXT AS "City",
        COUNT(o.c_orderid) AS "TotalOrders",
        COALESCE(SUM(CASE WHEN o.c_payment_status IN ('Completed', 'Paid') THEN o.c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS "TotalRevenue",
        COALESCE(AVG(r.c_overall_rating::DECIMAL(3,2)), 0) AS "AverageRating",
        COUNT(DISTINCT o.c_userid) AS "UniqueCustomers"
    FROM t_sys_catering_owner co
    LEFT JOIN t_sys_orders o ON co.c_ownerid = o.c_ownerid
        AND o.c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
    LEFT JOIN t_sys_catering_owner_addresses ad ON co.c_ownerid = ad.c_ownerid
    LEFT JOIN t_sys_catering_review r ON co.c_ownerid = r.c_ownerid AND r.c_is_verified = TRUE
    LEFT JOIN t_sys_city c ON ad.c_cityid = c.c_cityid
    WHERE co.c_isactive = TRUE AND co.c_approval_status = 2
    GROUP BY co.c_ownerid, co.c_catering_name, co.c_mobile, c.c_cityname
    HAVING COUNT(o.c_orderid) > 0
    ORDER BY "TotalRevenue" DESC
    LIMIT p_Limit;
END;
$$;

DROP FUNCTION sp_Admin_GetRecentOrders(integer);

CREATE OR REPLACE FUNCTION sp_Admin_GetRecentOrders(
    p_Limit INTEGER DEFAULT 10
)
RETURNS TABLE (
    "OrderId"       BIGINT,
    "OrderNumber"   TEXT,
    "CustomerName"  TEXT,
    "CustomerEmail" TEXT,
    "CateringName"  TEXT,
    "TotalAmount"   DECIMAL(18,2),
    "OrderStatus"   TEXT,
    "PaymentStatus" TEXT,
    "EventDate"     DATE,
    "OrderDate"     TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        o.c_orderid AS "OrderId",
        o.c_order_number::TEXT AS "OrderNumber",  -- ✅ FIX
        u.c_name::TEXT AS "CustomerName",
        u.c_email::TEXT AS "CustomerEmail",
        co.c_catering_name::TEXT AS "CateringName",
        o.c_total_amount AS "TotalAmount",
        o.c_order_status::TEXT AS "OrderStatus",
        o.c_payment_status::TEXT AS "PaymentStatus",
        o.c_event_date::DATE AS "EventDate",
        o.c_createddate AS "OrderDate"
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    ORDER BY o.c_createddate DESC
    LIMIT p_Limit;
END;
$$;


-- ===============================================
-- 6. Get Popular Food Categories
-- ===============================================
    
DROP FUNCTION sp_Admin_GetPopularCategories(timestamp, timestamp, integer);

CREATE OR REPLACE FUNCTION sp_Admin_GetPopularCategories(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate   TIMESTAMP DEFAULT NULL,
    p_Limit    INTEGER DEFAULT 10
)
RETURNS TABLE (
    "CategoryId"    INTEGER,
    "CategoryName"  TEXT,
    "OrderCount"    BIGINT,
    "TotalQuantity" DECIMAL(18,2),
    "TotalRevenue"  DECIMAL(18,2)
)
LANGUAGE plpgsql AS $$
DECLARE
    v_FromDate      DATE;
    v_ToDate        DATE;
    v_HasItemType   BOOLEAN;
    v_HasFoodId     BOOLEAN;
BEGIN
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    -- Check column existence using information_schema (replaces COL_LENGTH)
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 't_sys_order_items'
          AND column_name IN ('c_item_type', 'c_item_id')
        HAVING COUNT(*) = 2
    ) INTO v_HasItemType;

    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 't_sys_order_items'
          AND column_name = 'c_foodid'
    ) INTO v_HasFoodId;

    IF v_HasItemType THEN
        RETURN QUERY
        WITH OrderLines AS (
            SELECT
                oi.c_orderid AS "OrderId",
                UPPER(TRIM(oi.c_item_type)) AS "ItemType",
                oi.c_item_id AS "ItemId",
                oi.c_quantity AS "LineQuantity",
                COALESCE(oi.c_total_price, oi.c_unit_price * oi.c_quantity)::DECIMAL(18,2) AS "LineRevenue"
            FROM t_sys_order_items oi
            INNER JOIN t_sys_orders o ON oi.c_orderid = o.c_orderid
            WHERE o.c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        ),
        PackageCategoryWeight AS (
            SELECT
                pi.c_packageid,
                pi.c_categoryid,
                pi.c_quantity::DECIMAL(18,6) AS "CategoryQty",
                SUM(pi.c_quantity::DECIMAL(18,6)) OVER (PARTITION BY pi.c_packageid) AS "TotalCategoryQty"
            FROM t_sys_catering_package_items pi
        ),
        CategoryRevenue AS (
            SELECT
                ol.c_orderid AS "OrderId",
                fi.c_categoryid AS "CategoryId",
                ol."LineQuantity"::DECIMAL(18,2) AS "QuantityContribution",
                ol."LineRevenue"::DECIMAL(18,2) AS "RevenueContribution"
            FROM OrderLines ol
            INNER JOIN t_sys_fooditems fi ON fi.c_foodid = ol."ItemId"
            WHERE ol."ItemType" IN ('FOOD_ITEM', 'FOOD')
              AND fi.c_categoryid IS NOT NULL

            UNION ALL

            SELECT
                ol.c_orderid AS "OrderId",
                pcw.c_categoryid AS "CategoryId",
                (ol."LineQuantity" * CASE
                    WHEN pcw."TotalCategoryQty" > 0 THEN pcw."CategoryQty" / pcw."TotalCategoryQty"
                    ELSE 0
                END)::DECIMAL(18,2) AS "QuantityContribution",
                (ol."LineRevenue" * CASE
                    WHEN pcw."TotalCategoryQty" > 0 THEN pcw."CategoryQty" / pcw."TotalCategoryQty"
                    ELSE 0
                END)::DECIMAL(18,2) AS "RevenueContribution"
            FROM OrderLines ol
            INNER JOIN t_sys_catering_packages p ON p.c_packageid = ol."ItemId"
            INNER JOIN PackageCategoryWeight pcw ON pcw.c_packageid = p.c_packageid
            WHERE ol."ItemType" = 'PACKAGE'
        )
        SELECT
            fc.c_categoryid AS "CategoryId",
            fc.c_categoryname AS "CategoryName",
            COUNT(DISTINCT cr."OrderId") AS "OrderCount",
            COALESCE(SUM(cr."QuantityContribution"), 0)::DECIMAL(18,2) AS "TotalQuantity",
            COALESCE(SUM(cr."RevenueContribution"), 0)::DECIMAL(18,2) AS "TotalRevenue"
        FROM t_sys_food_category fc
        INNER JOIN CategoryRevenue cr ON cr."CategoryId" = fc.c_categoryid
        WHERE fc.c_isactive = TRUE
        GROUP BY fc.c_categoryid, fc.c_categoryname
        ORDER BY "TotalRevenue" DESC
        LIMIT p_Limit;

    ELSIF v_HasFoodId THEN
        RETURN QUERY
        SELECT
            fc.c_categoryid AS "CategoryId",
            fc.c_categoryname AS "CategoryName",
            COUNT(DISTINCT oi.c_orderid) AS "OrderCount",
            COALESCE(SUM(oi.c_quantity), 0)::DECIMAL(18,2) AS "TotalQuantity",
            COALESCE(SUM(oi.c_price * oi.c_quantity), 0)::DECIMAL(18,2) AS "TotalRevenue"
        FROM t_sys_food_category fc
        INNER JOIN t_sys_fooditems fi ON fi.c_categoryid = fc.c_categoryid
        INNER JOIN t_sys_order_items oi ON oi.c_foodid = fi.c_foodid
        INNER JOIN t_sys_orders o ON oi.c_orderid = o.c_orderid
        WHERE o.c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
          AND fc.c_isactive = TRUE
        GROUP BY fc.c_categoryid, fc.c_categoryname
        ORDER BY "TotalRevenue" DESC
        LIMIT p_Limit;

    ELSE
        -- Safety fallback when neither known order-item shape exists
        RETURN QUERY
        SELECT
            fc.c_categoryid AS "CategoryId",
            fc.c_categoryname AS "CategoryName",
            0::BIGINT AS "OrderCount",
            0::DECIMAL(18,2) AS "TotalQuantity",
            0::DECIMAL(18,2) AS "TotalRevenue"
        FROM t_sys_food_category fc
        WHERE fc.c_isactive = TRUE
        ORDER BY fc.c_categoryid
        LIMIT p_Limit;
    END IF;
END;
$$;


-- ===============================================
-- 7. Get User Growth Analytics
-- ===============================================

DROP FUNCTION sp_Admin_GetUserGrowth(timestamp, timestamp, varchar);

CREATE OR REPLACE FUNCTION sp_Admin_GetUserGrowth(
    p_FromDate    TIMESTAMP DEFAULT NULL,
    p_ToDate      TIMESTAMP DEFAULT NULL,
    p_Granularity VARCHAR(10) DEFAULT 'day'
)
RETURNS TABLE (
    "Date"            DATE,
    "Label"           TEXT,
    "NewUsers"        BIGINT,
    "CumulativeUsers" BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
BEGIN
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    IF p_Granularity = 'day' THEN
        RETURN QUERY
        SELECT
            c_createddate::DATE AS "Date",
            TO_CHAR(c_createddate::DATE, 'Mon DD') AS "Label",
            COUNT(*) AS "NewUsers",
            SUM(COUNT(*)) OVER (ORDER BY c_createddate::DATE) AS "CumulativeUsers"
        FROM t_sys_user
        WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        GROUP BY c_createddate::DATE
        ORDER BY "Date";

    ELSIF p_Granularity = 'month' THEN
        RETURN QUERY
        SELECT
            DATE_TRUNC('month', c_createddate)::DATE AS "Date",
            TO_CHAR(DATE_TRUNC('month', c_createddate), 'Mon YYYY') AS "Label",
            COUNT(*) AS "NewUsers",
            SUM(COUNT(*)) OVER (ORDER BY DATE_TRUNC('month', c_createddate)) AS "CumulativeUsers"
        FROM t_sys_user
        WHERE c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
        GROUP BY DATE_TRUNC('month', c_createddate)
        ORDER BY "Date";
    END IF;
END;
$$;


-- ===============================================
-- 8. Get Revenue by City
-- ===============================================

DROP FUNCTION sp_Admin_GetRevenueByCity(timestamp, timestamp, integer);

CREATE OR REPLACE FUNCTION sp_Admin_GetRevenueByCity(
    p_FromDate TIMESTAMP DEFAULT NULL,
    p_ToDate   TIMESTAMP DEFAULT NULL,
    p_Limit    INTEGER DEFAULT 10
)
RETURNS TABLE (
    "CityId"         INTEGER,
    "CityName"       TEXT,
    "TotalOrders"    BIGINT,
    "TotalRevenue"   DECIMAL(18,2),
    "ActivePartners" BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_FromDate DATE;
    v_ToDate   DATE;
BEGIN
    v_FromDate := COALESCE(p_FromDate::DATE, (NOW() - INTERVAL '30 days')::DATE);
    v_ToDate   := COALESCE(p_ToDate::DATE, NOW()::DATE);

    RETURN QUERY
    SELECT
        c.c_cityid AS "CityId",
        c.c_cityname AS "CityName",
        COUNT(o.c_orderid) AS "TotalOrders",
        COALESCE(SUM(CASE WHEN o.c_payment_status IN ('Completed', 'Paid') THEN o.c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS "TotalRevenue",
        COUNT(DISTINCT co.c_ownerid) AS "ActivePartners"
    FROM t_sys_city c
    INNER JOIN t_sys_catering_owner_addresses co ON c.c_cityid = co.c_cityid
    LEFT JOIN t_sys_orders o ON co.c_ownerid = o.c_ownerid
        AND o.c_createddate::DATE BETWEEN v_FromDate AND v_ToDate
    WHERE c.c_isactive = TRUE
    GROUP BY c.c_cityid, c.c_cityname
    HAVING COUNT(o.c_orderid) > 0
    ORDER BY "TotalRevenue" DESC
    LIMIT p_Limit;
END;
$$;