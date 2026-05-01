-- =============================================
-- Admin Orders Management - Stored Procedures
-- Description: Complete order management for admin panel
-- Version: 1.0
-- Date: 2026-02-19
-- =============================================


-- =============================================
-- 1. Get Orders with Pagination and Filtering
-- =============================================

CREATE OR REPLACE FUNCTION sp_Admin_GetOrders(
    p_PageNumber      INTEGER DEFAULT 1,
    p_PageSize        INTEGER DEFAULT 20,
    p_SearchTerm      VARCHAR(200) DEFAULT NULL,
    p_OrderStatus     VARCHAR(50) DEFAULT NULL,
    p_PaymentStatus   VARCHAR(50) DEFAULT NULL,
    p_StartDate       TIMESTAMP DEFAULT NULL,
    p_EndDate         TIMESTAMP DEFAULT NULL,
    p_UserId          BIGINT DEFAULT NULL,
    p_CateringOwnerId BIGINT DEFAULT NULL,
    p_MinAmount       NUMERIC(18,2) DEFAULT NULL,
    p_MaxAmount       NUMERIC(18,2) DEFAULT NULL,
    p_SortBy          VARCHAR(50) DEFAULT 'CreatedDate',
    p_SortOrder       VARCHAR(10) DEFAULT 'DESC',
    OUT p_TotalCount  INTEGER,
    OUT ref refcursor
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_Offset INTEGER;
BEGIN
    v_Offset := (p_PageNumber - 1) * p_PageSize;

    -- 1. Get total count
    SELECT COUNT(*) INTO p_TotalCount
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    WHERE TRUE
        AND (p_SearchTerm IS NULL OR
             o.c_order_number ILIKE '%' || p_SearchTerm || '%' OR
             u.c_name ILIKE '%' || p_SearchTerm || '%' OR
             co.c_catering_name ILIKE '%' || p_SearchTerm || '%')
        AND (p_OrderStatus IS NULL OR o.c_order_status = p_OrderStatus)
        AND (p_PaymentStatus IS NULL OR o.c_payment_status = p_PaymentStatus)
        AND (p_StartDate IS NULL OR o.c_createddate >= p_StartDate)
        AND (p_EndDate IS NULL OR o.c_createddate <= p_EndDate)
        AND (p_UserId IS NULL OR o.c_userid = p_UserId)
        AND (p_CateringOwnerId IS NULL OR o.c_ownerid = p_CateringOwnerId)
        AND (p_MinAmount IS NULL OR o.c_total_amount >= p_MinAmount)
        AND (p_MaxAmount IS NULL OR o.c_total_amount <= p_MaxAmount);

    -- 2. Open cursor for result set
    OPEN ref FOR
    SELECT
        o.c_orderid,
        o.c_order_number,
        o.c_userid,
        u.c_name AS c_user_name,
        u.c_email AS c_user_email,
        u.c_mobile AS c_user_phone,
        o.c_ownerid AS c_cateringownerid,
        co.c_catering_name,
        co.c_owner_name,
        o.c_event_date,
        o.c_event_type,
        o.c_guest_count,
        o.c_total_amount,
        o.c_order_status,
        o.c_payment_status,
        o.c_createddate,
        o.c_modifieddate,
        o.c_contact_person,
        o.c_contact_phone
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    WHERE TRUE
        AND (p_SearchTerm IS NULL OR
             o.c_order_number ILIKE '%' || p_SearchTerm || '%' OR
             u.c_name ILIKE '%' || p_SearchTerm || '%' OR
             co.c_catering_name ILIKE '%' || p_SearchTerm || '%')
        AND (p_OrderStatus IS NULL OR o.c_order_status = p_OrderStatus)
        AND (p_PaymentStatus IS NULL OR o.c_payment_status = p_PaymentStatus)
        AND (p_StartDate IS NULL OR o.c_createddate >= p_StartDate)
        AND (p_EndDate IS NULL OR o.c_createddate <= p_EndDate)
        AND (p_UserId IS NULL OR o.c_userid = p_UserId)
        AND (p_CateringOwnerId IS NULL OR o.c_ownerid = p_CateringOwnerId)
        AND (p_MinAmount IS NULL OR o.c_total_amount >= p_MinAmount)
        AND (p_MaxAmount IS NULL OR o.c_total_amount <= p_MaxAmount)
    ORDER BY o.c_createddate DESC
    LIMIT p_PageSize OFFSET v_Offset;

END;
$$;


-- =============================================
-- 2. Get Order Details by ID
-- =============================================

CREATE OR REPLACE FUNCTION sp_Admin_GetOrderById(
    p_OrderId BIGINT
)
RETURNS TABLE (
    c_orderid               BIGINT,
    c_order_number          TEXT,
    c_order_status          TEXT,
    c_createddate           TIMESTAMP,
    c_modifieddate          TIMESTAMP,
    c_userid                BIGINT,
    c_user_name             TEXT,
    c_user_email            TEXT,
    c_user_phone            TEXT,
    c_cateringownerid       BIGINT,
    c_catering_name         TEXT,
    c_owner_name            TEXT,
    c_owner_phone           TEXT,
    c_event_date            DATE,
    c_event_type            TEXT,
    c_guest_count           INTEGER,
    c_event_location        TEXT,
    c_venue_address         TEXT,
    c_contact_person        TEXT,
    c_contact_phone         TEXT,
    c_contact_email         TEXT,
    c_total_amount          DECIMAL(18,2),
    c_payment_status        TEXT,
    c_advance_amount        DECIMAL(18,2),
    c_balance_amount        DECIMAL(18,2),
    c_commission_amount     DECIMAL(18,2),
    c_commission_percentage DECIMAL(18,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        o.c_orderid,
        o.c_order_number,
        o.c_order_status,
        o.c_createddate,
        o.c_modifieddate,
        -- Customer info
        o.c_userid,
        u.c_name AS c_user_name,
        u.c_email AS c_user_email,
        u.c_mobile AS c_user_phone,
        -- Catering partner info
        o.c_ownerid AS c_cateringownerid,
        co.c_catering_name,
        co.c_owner_name,
        co.c_mobile AS c_owner_phone,
        -- Event details
        o.c_event_date,
        o.c_event_type,
        o.c_guest_count,
        o.c_event_location,
        o.c_delivery_address AS c_venue_address,
        o.c_contact_person,
        o.c_contact_phone,
        o.c_contact_email,
        -- Financial details
        o.c_total_amount,
        o.c_payment_status,
        NULL::DECIMAL(18,2) AS c_advance_amount,
        NULL::DECIMAL(18,2) AS c_balance_amount,
        o.c_platform_commission AS c_commission_amount,
        o.c_commission_rate AS c_commission_percentage
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    WHERE o.c_orderid = p_OrderId;
END;
$$;

CREATE OR REPLACE FUNCTION sp_Admin_UpdateOrderStatus(
    p_OrderId     BIGINT,
    p_NewStatus   VARCHAR(50),
    p_UpdatedBy   BIGINT,
    p_Remarks     TEXT DEFAULT NULL,
    OUT p_Success       BOOLEAN,
    OUT p_ErrorMessage  VARCHAR(500)
)
LANGUAGE plpgsql AS $$
DECLARE
    v_CurrentStatus VARCHAR(50);
BEGIN
    p_Success      := FALSE;
    p_ErrorMessage := NULL;

    -- Verify order exists
    IF NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_orderid = p_OrderId) THEN
        p_ErrorMessage := 'Order not found';
        RETURN;
    END IF;

    -- Get current status
    SELECT c_order_status INTO v_CurrentStatus
    FROM t_sys_orders WHERE c_orderid = p_OrderId;

    -- Prevent invalid transitions
    IF v_CurrentStatus = 'Cancelled' AND p_NewStatus != 'Cancelled' THEN
        p_ErrorMessage := 'Cannot change status of a cancelled order';
        RETURN;
    END IF;

    IF v_CurrentStatus = 'Completed' AND p_NewStatus NOT IN ('Completed', 'Refunded') THEN
        p_ErrorMessage := 'Cannot change status of a completed order';
        RETURN;
    END IF;

    BEGIN
        -- Update order
        UPDATE t_sys_orders
        SET c_order_status = p_NewStatus,
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Insert history
        INSERT INTO t_sys_order_status_history (
            c_orderid, c_status, c_remarks, c_modifieddate
        )
        VALUES (
            p_OrderId,
            p_NewStatus,
            COALESCE(p_Remarks, 'Status updated by admin ID: ' || p_UpdatedBy::TEXT),
            NOW()
        );

        p_Success := TRUE;

    EXCEPTION WHEN OTHERS THEN
        p_ErrorMessage := SQLERRM;
        p_Success      := FALSE;

        INSERT INTO t_sys_error_log (
            c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date
        )
        VALUES (
            SQLERRM, 'sp_Admin_UpdateOrderStatus', NULL, SQLSTATE, NOW()
        );
    END;
END;
$$;


-- =============================================
-- 4. Get Order Statistics
-- =============================================

CREATE OR REPLACE FUNCTION sp_Admin_GetOrderStats()
RETURNS TABLE (
    TotalOrders       BIGINT,
    PendingOrders     BIGINT,
    ConfirmedOrders   BIGINT,
    InProgressOrders  BIGINT,
    CompletedOrders   BIGINT,
    CancelledOrders   BIGINT,
    TotalRevenue      DECIMAL(18,2),
    PendingRevenue    DECIMAL(18,2),
    TodayOrders       BIGINT,
    ThisMonthOrders   BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) AS TotalOrders,
        SUM(CASE WHEN c_order_status = 'Pending'    THEN 1 ELSE 0 END) AS PendingOrders,
        SUM(CASE WHEN c_order_status = 'Confirmed'  THEN 1 ELSE 0 END) AS ConfirmedOrders,
        SUM(CASE WHEN c_order_status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressOrders,
        SUM(CASE WHEN c_order_status = 'Completed'  THEN 1 ELSE 0 END) AS CompletedOrders,
        SUM(CASE WHEN c_order_status = 'Cancelled'  THEN 1 ELSE 0 END) AS CancelledOrders,
        COALESCE(SUM(CASE WHEN c_order_status NOT IN ('Cancelled', 'Refunded') THEN c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS TotalRevenue,
        COALESCE(SUM(CASE WHEN c_order_status = 'Pending' THEN c_total_amount ELSE 0 END), 0)::DECIMAL(18,2) AS PendingRevenue,
        COALESCE(SUM(CASE WHEN c_createddate::DATE = NOW()::DATE THEN 1 ELSE 0 END), 0) AS TodayOrders,
        COALESCE(SUM(CASE WHEN EXTRACT(YEAR  FROM c_createddate) = EXTRACT(YEAR  FROM NOW())
                           AND EXTRACT(MONTH FROM c_createddate) = EXTRACT(MONTH FROM NOW())
                          THEN 1 ELSE 0 END), 0) AS ThisMonthOrders
    FROM t_sys_orders;
END;
$$;


-- =============================================
-- 5. Cancel Order (Admin Initiated)
-- =============================================

CREATE OR REPLACE FUNCTION sp_Admin_CancelOrder(
    p_OrderId             BIGINT,
    p_AdminId             BIGINT,
    p_CancellationReason  TEXT,
    OUT p_Success         BOOLEAN,
    OUT p_ErrorMessage    VARCHAR(500)
)
RETURNS RECORD
LANGUAGE plpgsql AS $$
DECLARE
    v_CurrentStatus VARCHAR(50);
BEGIN
    p_Success      := FALSE;
    p_ErrorMessage := NULL;

    -- Verify order exists and get current status
    SELECT c_order_status INTO v_CurrentStatus
    FROM t_sys_orders WHERE c_orderid = p_OrderId;

    IF v_CurrentStatus IS NULL THEN
        p_ErrorMessage := 'Order not found';
        RETURN;
    END IF;

    IF v_CurrentStatus = 'Cancelled' THEN
        p_ErrorMessage := 'Order is already cancelled';
        RETURN;
    END IF;

    IF v_CurrentStatus = 'Completed' THEN
        p_ErrorMessage := 'Cannot cancel a completed order';
        RETURN;
    END IF;

    BEGIN
        -- Update order status to Cancelled
        UPDATE t_sys_orders
        SET c_order_status = 'Cancelled',
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Insert cancellation history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        VALUES (
            p_OrderId,
            'Cancelled',
            'Order cancelled by admin (ID: ' || p_AdminId::TEXT || '). Reason: ' || p_CancellationReason,
            NOW()
        );

        -- Note: Refund processing should be handled separately via the cancellation/refund flow
        -- This procedure only marks the order as cancelled

        p_Success := TRUE;

    EXCEPTION WHEN OTHERS THEN
        p_ErrorMessage := SQLERRM;
        p_Success      := FALSE;

        -- Log error
        INSERT INTO t_sys_error_log (c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date)
        VALUES (SQLERRM, 'sp_Admin_CancelOrder', NULL, SQLSTATE, NOW());
    END;
END;
$$;