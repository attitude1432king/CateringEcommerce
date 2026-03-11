-- =============================================
-- Admin Orders Management - Stored Procedures
-- Description: Complete order management for admin panel
-- Version: 1.0
-- Date: 2026-02-19
-- =============================================

USE [CateringDB]
GO

-- =============================================
-- 1. Get Orders with Pagination and Filtering
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetOrders]
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchTerm NVARCHAR(200) = NULL,
    @OrderStatus NVARCHAR(50) = NULL,
    @PaymentStatus NVARCHAR(50) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @UserId BIGINT = NULL,
    @CateringOwnerId BIGINT = NULL,
    @MinAmount DECIMAL(18, 2) = NULL,
    @MaxAmount DECIMAL(18, 2) = NULL,
    @SortBy NVARCHAR(50) = 'CreatedDate',
    @SortOrder NVARCHAR(10) = 'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Main query with filtering
    IF OBJECT_ID('tempdb..#FilteredOrders') IS NOT NULL
        DROP TABLE #FilteredOrders;

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
        ISNULL(loc.c_cityname, '') AS c_city_name,
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
    INTO #FilteredOrders
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    OUTER APPLY (
        SELECT TOP 1 c.c_cityname
        FROM t_sys_catering_owner_addresses coa
        LEFT JOIN t_sys_city c ON c.c_cityid = coa.c_cityid
        WHERE coa.c_ownerid = co.c_ownerid
        ORDER BY coa.c_addressid DESC
    ) loc
    WHERE 1=1
        -- Search filter
        AND (@SearchTerm IS NULL OR
             o.c_order_number LIKE '%' + @SearchTerm + '%' OR
             u.c_name LIKE '%' + @SearchTerm + '%' OR
             co.c_catering_name LIKE '%' + @SearchTerm + '%' OR
             co.c_owner_name LIKE '%' + @SearchTerm + '%' OR
             ISNULL(loc.c_cityname, '') LIKE '%' + @SearchTerm + '%')
        -- Status filters
        AND (@OrderStatus IS NULL OR o.c_order_status = @OrderStatus)
        AND (@PaymentStatus IS NULL OR o.c_payment_status = @PaymentStatus)
        -- Date range filter
        AND (@StartDate IS NULL OR o.c_createddate >= @StartDate)
        AND (@EndDate IS NULL OR o.c_createddate <= @EndDate)
        -- User/Catering filters
        AND (@UserId IS NULL OR o.c_userid = @UserId)
        AND (@CateringOwnerId IS NULL OR o.c_ownerid = @CateringOwnerId)
        -- Amount filters
        AND (@MinAmount IS NULL OR o.c_total_amount >= @MinAmount)
        AND (@MaxAmount IS NULL OR o.c_total_amount <= @MaxAmount);

    SELECT @TotalCount = COUNT(*) FROM #FilteredOrders;

    SELECT *
    FROM #FilteredOrders
    ORDER BY
        CASE WHEN @SortBy = 'CreatedDate' AND @SortOrder = 'ASC' THEN c_createddate END ASC,
        CASE WHEN @SortBy = 'CreatedDate' AND @SortOrder = 'DESC' THEN c_createddate END DESC,
        CASE WHEN @SortBy = 'EventDate' AND @SortOrder = 'ASC' THEN c_event_date END ASC,
        CASE WHEN @SortBy = 'EventDate' AND @SortOrder = 'DESC' THEN c_event_date END DESC,
        CASE WHEN @SortBy = 'TotalAmount' AND @SortOrder = 'ASC' THEN c_total_amount END ASC,
        CASE WHEN @SortBy = 'TotalAmount' AND @SortOrder = 'DESC' THEN c_total_amount END DESC,
        c_createddate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- 2. Get Order Details by ID
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetOrderById]
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

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
        CAST(NULL AS DECIMAL(18,2)) AS c_advance_amount,
        CAST(NULL AS DECIMAL(18,2)) AS c_balance_amount,
        o.c_platform_commission AS c_commission_amount,
        o.c_commission_rate AS c_commission_percentage
    FROM t_sys_orders o
    INNER JOIN t_sys_user u ON o.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON o.c_ownerid = co.c_ownerid
    WHERE o.c_orderid = @OrderId;
END
GO

-- =============================================
-- 3. Update Order Status
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_UpdateOrderStatus]
    @OrderId BIGINT,
    @NewStatus NVARCHAR(50),
    @Remarks NVARCHAR(MAX) = NULL,
    @UpdatedBy BIGINT,
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Verify order exists
        IF NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_orderid = @OrderId)
        BEGIN
            SET @ErrorMessage = 'Order not found';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate status transition
        DECLARE @CurrentStatus NVARCHAR(50);
        SELECT @CurrentStatus = c_order_status FROM t_sys_orders WHERE c_orderid = @OrderId;

        -- Prevent invalid status transitions (add your business logic here)
        IF @CurrentStatus = 'Cancelled' AND @NewStatus != 'Cancelled'
        BEGIN
            SET @ErrorMessage = 'Cannot change status of a cancelled order';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus = 'Completed' AND @NewStatus NOT IN ('Completed', 'Refunded')
        BEGIN
            SET @ErrorMessage = 'Cannot change status of a completed order';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update order status
        UPDATE t_sys_orders
        SET c_order_status = @NewStatus,
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Insert status history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        VALUES (@OrderId, @NewStatus, COALESCE(@Remarks, 'Status updated by admin ID: ' + CAST(@UpdatedBy AS NVARCHAR)), GETDATE());

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;

        -- Log error
        INSERT INTO t_sys_error_log (c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date)
        VALUES (ERROR_MESSAGE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_SEVERITY(), GETDATE());
    END CATCH
END
GO

-- =============================================
-- 4. Get Order Statistics
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_GetOrderStats]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalOrders,
        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS PendingOrders,
        SUM(CASE WHEN c_order_status = 'Confirmed' THEN 1 ELSE 0 END) AS ConfirmedOrders,
        SUM(CASE WHEN c_order_status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressOrders,
        SUM(CASE WHEN c_order_status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOrders,
        SUM(CASE WHEN c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders,
        COALESCE(SUM(CASE WHEN c_order_status NOT IN ('Cancelled', 'Refunded') THEN c_total_amount ELSE 0 END), 0) AS TotalRevenue,
        COALESCE(SUM(CASE WHEN c_order_status = 'Pending' THEN c_total_amount ELSE 0 END), 0) AS PendingRevenue,
        COALESCE(SUM(CASE WHEN CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END), 0) AS TodayOrders,
        COALESCE(SUM(CASE WHEN YEAR(c_createddate) = YEAR(GETDATE()) AND MONTH(c_createddate) = MONTH(GETDATE()) THEN 1 ELSE 0 END), 0) AS ThisMonthOrders
    FROM t_sys_orders;
END
GO

-- =============================================
-- 5. Cancel Order (Admin Initiated)
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Admin_CancelOrder]
    @OrderId BIGINT,
    @AdminId BIGINT,
    @CancellationReason NVARCHAR(MAX),
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Verify order exists and is not already cancelled
        DECLARE @CurrentStatus NVARCHAR(50);
        SELECT @CurrentStatus = c_order_status FROM t_sys_orders WHERE c_orderid = @OrderId;

        IF @CurrentStatus IS NULL
        BEGIN
            SET @ErrorMessage = 'Order not found';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus = 'Cancelled'
        BEGIN
            SET @ErrorMessage = 'Order is already cancelled';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus = 'Completed'
        BEGIN
            SET @ErrorMessage = 'Cannot cancel a completed order';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update order status to Cancelled
        UPDATE t_sys_orders
        SET c_order_status = 'Cancelled',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Insert cancellation history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        VALUES (
            @OrderId,
            'Cancelled',
            'Order cancelled by admin (ID: ' + CAST(@AdminId AS NVARCHAR) + '). Reason: ' + @CancellationReason,
            GETDATE()
        );

        -- Note: Refund processing should be handled separately via the cancellation/refund flow
        -- This procedure only marks the order as cancelled

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;

        -- Log error
        INSERT INTO t_sys_error_log (c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date)
        VALUES (ERROR_MESSAGE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_SEVERITY(), GETDATE());
    END CATCH
END
GO

-- =============================================
-- Verification
-- =============================================

PRINT '================================================';
PRINT 'Admin Orders Management - DEPLOYMENT COMPLETE';
PRINT '================================================';
PRINT 'Stored Procedures Created:';
PRINT '  ✅ sp_Admin_GetOrders';
PRINT '  ✅ sp_Admin_GetOrderById';
PRINT '  ✅ sp_Admin_UpdateOrderStatus';
PRINT '  ✅ sp_Admin_GetOrderStats';
PRINT '  ✅ sp_Admin_CancelOrder';
PRINT '';
PRINT 'Next Steps:';
PRINT '  - Register IAdminOrderRepository in DI container';
PRINT '  - Create AdminOrdersController.cs';
PRINT '  - Update frontend to use new endpoints';
PRINT '================================================';
GO
