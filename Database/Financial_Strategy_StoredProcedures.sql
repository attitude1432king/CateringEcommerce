-- =============================================
-- Financial Strategy Stored Procedures
-- Date: 2026-01-30
-- Purpose: Business logic implementation for financial strategy
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Creating Financial Strategy Stored Procedures';
PRINT '================================================';
PRINT '';

-- =============================================
-- SP 1: Auto-Lock Guest Count
-- =============================================

PRINT 'Creating sp_AutoLockGuestCount...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_AutoLockGuestCount]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LockDays INT;
    DECLARE @ProcessedCount INT = 0;

    -- Get lock period from settings (default 5 days)
    SELECT @LockDays = TRY_CAST(c_setting_value AS INT)
    FROM t_sys_settings
    WHERE c_setting_key = 'ORDER.GUEST_COUNT_LOCK_DAYS';

    IF @LockDays IS NULL SET @LockDays = 5;

    -- Lock guest count for all orders that are 5 days away and not yet locked
    UPDATE t_sys_orders
    SET c_guest_count_locked = 1,
        c_locked_guest_count = c_guest_count,
        c_original_guest_count = ISNULL(c_original_guest_count, c_guest_count),
        c_guest_count_locked_date = GETDATE(),
        c_modifieddate = GETDATE()
    WHERE c_guest_count_locked = 0
      AND c_order_status NOT IN ('Cancelled', 'Completed', 'Rejected')
      AND DATEDIFF(DAY, GETDATE(), c_event_date) <= @LockDays
      AND c_event_date >= GETDATE();

    SET @ProcessedCount = @@ROWCOUNT;

    -- Update job status
    UPDATE t_sys_auto_lock_jobs
    SET c_last_run_date = GETDATE(),
        c_next_run_date = DATEADD(MINUTE, c_run_frequency_minutes, GETDATE()),
        c_orders_processed_last_run = @ProcessedCount
    WHERE c_job_type = 'GUEST_COUNT_LOCK';

    SELECT @ProcessedCount AS OrdersLocked;
END
GO

PRINT '  ✓ sp_AutoLockGuestCount created';
PRINT '';

-- =============================================
-- SP 2: Auto-Lock Menu
-- =============================================

PRINT 'Creating sp_AutoLockMenu...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_AutoLockMenu]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LockDays INT;
    DECLARE @ProcessedCount INT = 0;

    -- Get lock period from settings (default 3 days)
    SELECT @LockDays = TRY_CAST(c_setting_value AS INT)
    FROM t_sys_settings
    WHERE c_setting_key = 'ORDER.MENU_LOCK_DAYS';

    IF @LockDays IS NULL SET @LockDays = 3;

    -- Lock menu for all orders that are 3 days away and not yet locked
    UPDATE t_sys_orders
    SET c_menu_locked = 1,
        c_menu_locked_date = GETDATE(),
        c_modifieddate = GETDATE()
    WHERE c_menu_locked = 0
      AND c_order_status NOT IN ('Cancelled', 'Completed', 'Rejected')
      AND DATEDIFF(DAY, GETDATE(), c_event_date) <= @LockDays
      AND c_event_date >= GETDATE();

    SET @ProcessedCount = @@ROWCOUNT;

    -- Update job status
    UPDATE t_sys_auto_lock_jobs
    SET c_last_run_date = GETDATE(),
        c_next_run_date = DATEADD(MINUTE, c_run_frequency_minutes, GETDATE()),
        c_orders_processed_last_run = @ProcessedCount
    WHERE c_job_type = 'MENU_LOCK';

    SELECT @ProcessedCount AS OrdersLocked;
END
GO

PRINT '  ✓ sp_AutoLockMenu created';
PRINT '';

-- =============================================
-- SP 3: Calculate Refund Amount (Cancellation Policy)
-- =============================================

PRINT 'Creating sp_CalculateCancellationRefund...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CalculateCancellationRefund]
    @OrderId BIGINT,
    @RefundPercentage DECIMAL(5,2) OUTPUT,
    @RefundAmount DECIMAL(18,2) OUTPUT,
    @PolicyTier VARCHAR(20) OUTPUT,
    @PartnerCompensation DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EventDate DATETIME;
    DECLARE @OrderTotal DECIMAL(18,2);
    DECLARE @AdvancePaid DECIMAL(18,2);
    DECLARE @HoursBeforeEvent INT;
    DECLARE @DaysBeforeEvent INT;
    DECLARE @FullRefundDays INT = 7;
    DECLARE @PartialRefundStartDays INT = 3;
    DECLARE @PartialRefundEndDays INT = 7;
    DECLARE @NoRefundHours INT = 48;
    DECLARE @PartialRefundPercentage DECIMAL(5,2) = 50.00;

    -- Get settings
    SELECT @FullRefundDays = TRY_CAST(c_setting_value AS INT)
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.FULL_REFUND_DAYS';

    SELECT @NoRefundHours = TRY_CAST(c_setting_value AS INT)
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.NO_REFUND_HOURS';

    SELECT @PartialRefundPercentage = TRY_CAST(c_setting_value AS DECIMAL(5,2))
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.PARTIAL_REFUND_PERCENTAGE';

    -- Get order details
    SELECT
        @EventDate = c_event_date,
        @OrderTotal = c_total_amount
    FROM t_sys_orders
    WHERE c_orderid = @OrderId;

    -- Calculate time difference
    SET @HoursBeforeEvent = DATEDIFF(HOUR, GETDATE(), @EventDate);
    SET @DaysBeforeEvent = DATEDIFF(DAY, GETDATE(), @EventDate);

    -- Get advance paid from payment summary
    SELECT @AdvancePaid = c_advanceamount
    FROM t_sys_order_payment_summary
    WHERE c_orderid = @OrderId;

    IF @AdvancePaid IS NULL SET @AdvancePaid = @OrderTotal * 0.5; -- Assume 50% if not in summary

    -- Determine policy tier and refund percentage
    IF @DaysBeforeEvent > @FullRefundDays
    BEGIN
        -- >7 days: 100% refund
        SET @PolicyTier = 'FULL_REFUND';
        SET @RefundPercentage = 100.00;
        SET @PartnerCompensation = 0;
    END
    ELSE IF @DaysBeforeEvent >= @PartialRefundStartDays AND @DaysBeforeEvent <= @PartialRefundEndDays
    BEGIN
        -- 3-7 days: 50% refund
        SET @PolicyTier = 'PARTIAL_REFUND';
        SET @RefundPercentage = @PartialRefundPercentage;
        SET @PartnerCompensation = @AdvancePaid * (1 - (@PartialRefundPercentage / 100.0));
    END
    ELSE IF @HoursBeforeEvent < @NoRefundHours
    BEGIN
        -- <48 hours: No refund
        SET @PolicyTier = 'NO_REFUND';
        SET @RefundPercentage = 0.00;
        SET @PartnerCompensation = @AdvancePaid;
    END
    ELSE
    BEGIN
        -- Default to partial
        SET @PolicyTier = 'PARTIAL_REFUND';
        SET @RefundPercentage = @PartialRefundPercentage;
        SET @PartnerCompensation = @AdvancePaid * (1 - (@PartialRefundPercentage / 100.0));
    END

    -- Calculate refund amount
    SET @RefundAmount = @AdvancePaid * (@RefundPercentage / 100.0);

    -- Return results
    SELECT
        @OrderId AS OrderId,
        @EventDate AS EventDate,
        @DaysBeforeEvent AS DaysBeforeEvent,
        @HoursBeforeEvent AS HoursBeforeEvent,
        @PolicyTier AS PolicyTier,
        @OrderTotal AS OrderTotal,
        @AdvancePaid AS AdvancePaid,
        @RefundPercentage AS RefundPercentage,
        @RefundAmount AS RefundAmount,
        @PartnerCompensation AS PartnerCompensation;
END
GO

PRINT '  ✓ sp_CalculateCancellationRefund created';
PRINT '';

-- =============================================
-- SP 4: Process Cancellation Request
-- =============================================

PRINT 'Creating sp_ProcessCancellationRequest...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessCancellationRequest]
    @OrderId BIGINT,
    @UserId BIGINT,
    @CancellationReason NVARCHAR(1000),
    @IsForceMajeure BIT = 0,
    @ForceMajeureEvidence NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @OwnerId BIGINT;
        DECLARE @EventDate DATETIME;
        DECLARE @HoursBeforeEvent INT;
        DECLARE @DaysBeforeEvent INT;
        DECLARE @PolicyTier VARCHAR(20);
        DECLARE @RefundPercentage DECIMAL(5,2);
        DECLARE @RefundAmount DECIMAL(18,2);
        DECLARE @PartnerCompensation DECIMAL(18,2);
        DECLARE @OrderTotal DECIMAL(18,2);
        DECLARE @AdvancePaid DECIMAL(18,2);
        DECLARE @CommissionRate DECIMAL(5,2);
        DECLARE @PlatformCommissionForfeited DECIMAL(18,2);

        -- Get order details
        SELECT
            @OwnerId = c_ownerid,
            @EventDate = c_event_date,
            @OrderTotal = c_total_amount,
            @CommissionRate = c_commission_rate
        FROM t_sys_orders
        WHERE c_orderid = @OrderId AND c_userid = @UserId;

        IF @OwnerId IS NULL
        BEGIN
            THROW 50001, 'Order not found or does not belong to this user', 1;
        END

        -- Calculate refund
        EXEC sp_CalculateCancellationRefund
            @OrderId = @OrderId,
            @RefundPercentage = @RefundPercentage OUTPUT,
            @RefundAmount = @RefundAmount OUTPUT,
            @PolicyTier = @PolicyTier OUTPUT,
            @PartnerCompensation = @PartnerCompensation OUTPUT;

        SELECT @AdvancePaid = c_advanceamount
        FROM t_sys_order_payment_summary
        WHERE c_orderid = @OrderId;

        IF @AdvancePaid IS NULL SET @AdvancePaid = @OrderTotal * 0.5;

        SET @HoursBeforeEvent = DATEDIFF(HOUR, GETDATE(), @EventDate);
        SET @DaysBeforeEvent = DATEDIFF(DAY, GETDATE(), @EventDate);

        -- Force majeure overrides policy
        IF @IsForceMajeure = 1
        BEGIN
            SET @PolicyTier = 'FORCE_MAJEURE';
            SET @RefundPercentage = 50.00; -- Split 50-50 for force majeure
            SET @RefundAmount = @AdvancePaid * 0.5;
            SET @PartnerCompensation = @AdvancePaid * 0.5;
            SET @PlatformCommissionForfeited = @AdvancePaid * (@CommissionRate / 100.0);
        END
        ELSE
        BEGIN
            SET @PlatformCommissionForfeited = 0; -- Platform keeps commission for normal cancellations
        END

        -- Insert cancellation request (supports both old and migrated compensation column names)
        DECLARE @CompensationColumn SYSNAME;
        SELECT TOP 1 @CompensationColumn = c.name
        FROM sys.columns c
        WHERE c.object_id = OBJECT_ID('dbo.t_sys_cancellation_requests')
          AND c.name LIKE 'c[_]%[_]compensation'
          AND c.name NOT LIKE 'c[_]platform[_]%'
        ORDER BY CASE WHEN c.name = 'c_partner_compensation' THEN 0 ELSE 1 END;

        DECLARE @InsertCancellationSql NVARCHAR(MAX) = N'
            INSERT INTO t_sys_cancellation_requests (
                c_orderid, c_userid, c_ownerid,
                c_event_date, c_hours_before_event, c_days_before_event,
                c_policy_tier, c_refund_percentage,
                c_order_total_amount, c_advance_paid,
                c_refund_amount, c_retention_amount,
                ' + QUOTENAME(@CompensationColumn) + N', c_platform_commission_forfeited,
                c_cancellation_reason, c_is_force_majeure, c_force_majeure_evidence,
                c_status
            )
            VALUES (
                @OrderId, @UserId, @OwnerId,
                @EventDate, @HoursBeforeEvent, @DaysBeforeEvent,
                @PolicyTier, @RefundPercentage,
                @OrderTotal, @AdvancePaid,
                @RefundAmount, (@AdvancePaid - @RefundAmount),
                @PartnerCompensation, @PlatformCommissionForfeited,
                @CancellationReason, @IsForceMajeure, @ForceMajeureEvidence,
                ''Pending''
            );
            SET @InsertedCancellationId = SCOPE_IDENTITY();';

        DECLARE @CancellationId BIGINT;

        EXEC sp_executesql
            @InsertCancellationSql,
            N'@OrderId BIGINT, @UserId BIGINT, @OwnerId BIGINT, @EventDate DATETIME, @HoursBeforeEvent INT, @DaysBeforeEvent INT, @PolicyTier VARCHAR(20), @RefundPercentage DECIMAL(5,2), @OrderTotal DECIMAL(18,2), @AdvancePaid DECIMAL(18,2), @RefundAmount DECIMAL(18,2), @PartnerCompensation DECIMAL(18,2), @PlatformCommissionForfeited DECIMAL(18,2), @CancellationReason NVARCHAR(1000), @IsForceMajeure BIT, @ForceMajeureEvidence NVARCHAR(MAX), @InsertedCancellationId BIGINT OUTPUT',
            @OrderId = @OrderId,
            @UserId = @UserId,
            @OwnerId = @OwnerId,
            @EventDate = @EventDate,
            @HoursBeforeEvent = @HoursBeforeEvent,
            @DaysBeforeEvent = @DaysBeforeEvent,
            @PolicyTier = @PolicyTier,
            @RefundPercentage = @RefundPercentage,
            @OrderTotal = @OrderTotal,
            @AdvancePaid = @AdvancePaid,
            @RefundAmount = @RefundAmount,
            @PartnerCompensation = @PartnerCompensation,
            @PlatformCommissionForfeited = @PlatformCommissionForfeited,
            @CancellationReason = @CancellationReason,
            @IsForceMajeure = @IsForceMajeure,
            @ForceMajeureEvidence = @ForceMajeureEvidence,
            @InsertedCancellationId = @CancellationId OUTPUT;

        -- Update order status
        UPDATE t_sys_orders
        SET c_order_status = 'Cancellation_Requested',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Add to order history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        VALUES (@OrderId, 'Cancellation_Requested',
                'Customer requested cancellation. Policy: ' + @PolicyTier + ', Refund: ' + CAST(@RefundPercentage AS VARCHAR) + '%',
                GETDATE());

        COMMIT TRANSACTION;

        -- Return cancellation details
        SELECT
            @CancellationId AS CancellationId,
            @PolicyTier AS PolicyTier,
            @RefundPercentage AS RefundPercentage,
            @RefundAmount AS RefundAmount,
            @PartnerCompensation AS PartnerCompensation,
            @DaysBeforeEvent AS DaysBeforeEvent,
            'Your cancellation request has been submitted. Expected refund: ₹' + CAST(@RefundAmount AS VARCHAR) AS Message;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '  ✓ sp_ProcessCancellationRequest created';
PRINT '';

-- =============================================
-- SP 5: Request Guest Count Increase
-- =============================================

PRINT 'Creating sp_RequestGuestCountChange...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_RequestGuestCountChange]
    @OrderId BIGINT,
    @UserId BIGINT,
    @NewGuestCount INT,
    @ChangeReason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CurrentGuestCount INT;
        DECLARE @LockedGuestCount INT;
        DECLARE @IsLocked BIT;
        DECLARE @EventDate DATETIME;
        DECLARE @DaysBeforeEvent INT;
        DECLARE @PricePerGuest DECIMAL(18,2);
        DECLARE @AdditionalAmount DECIMAL(18,2);
        DECLARE @PricingMultiplier DECIMAL(5,2);
        DECLARE @GuestCountChange INT;
        DECLARE @OwnerId BIGINT;
        DECLARE @RequiresApproval BIT = 0;

        -- Get order details
        SELECT
            @CurrentGuestCount = c_guest_count,
            @LockedGuestCount = c_locked_guest_count,
            @IsLocked = c_guest_count_locked,
            @EventDate = c_event_date,
            @OwnerId = c_ownerid,
            @PricePerGuest = c_total_amount / c_guest_count
        FROM t_sys_orders
        WHERE c_orderid = @OrderId AND c_userid = @UserId;

        IF @OwnerId IS NULL
        BEGIN
            THROW 50001, 'Order not found or does not belong to this user', 1;
        END

        SET @GuestCountChange = @NewGuestCount - ISNULL(@LockedGuestCount, @CurrentGuestCount);

        -- Check if decrease is allowed
        IF @GuestCountChange < 0
        BEGIN
            IF @IsLocked = 1
            BEGIN
                THROW 50002, 'Guest count decrease is not allowed after lock period', 1;
            END

            SET @DaysBeforeEvent = DATEDIFF(DAY, GETDATE(), @EventDate);
            IF @DaysBeforeEvent <= 7
            BEGIN
                THROW 50003, 'Guest count decrease is only allowed more than 7 days before event', 1;
            END

            SET @RequiresApproval = 1; -- Partner must approve decrease
        END

        -- Calculate pricing multiplier based on timing
        SET @DaysBeforeEvent = DATEDIFF(DAY, GETDATE(), @EventDate);

        IF @DaysBeforeEvent > 7
            SET @PricingMultiplier = 1.0;
        ELSE IF @DaysBeforeEvent BETWEEN 5 AND 7
            SET @PricingMultiplier = 1.2;
        ELSE IF @DaysBeforeEvent BETWEEN 3 AND 4
            SET @PricingMultiplier = 1.3;
        ELSE IF @DaysBeforeEvent >= 2
            SET @PricingMultiplier = 1.5;
        ELSE
            THROW 50004, 'Guest count changes not allowed within 48 hours of event', 1;

        -- Calculate additional amount
        SET @AdditionalAmount = (@PricePerGuest * ABS(@GuestCountChange)) * @PricingMultiplier;

        IF @GuestCountChange < 0
            SET @AdditionalAmount = -@AdditionalAmount; -- Negative for decreases

        -- Check if partner approval required for increases after lock
        IF @IsLocked = 1 AND @GuestCountChange > 0
        BEGIN
            IF (@NewGuestCount - @LockedGuestCount) > (@LockedGuestCount * 0.10) -- >10% increase
            BEGIN
                SET @RequiresApproval = 1;
            END
        END

        -- Insert modification request
        INSERT INTO t_sys_order_modifications (
            c_orderid, c_modification_type,
            c_original_guest_count, c_modified_guest_count, c_guest_count_change,
            c_original_amount, c_additional_amount, c_pricing_multiplier,
            c_modification_reason,
            c_requested_by, c_requested_by_type,
            c_requires_approval, c_status
        )
        VALUES (
            @OrderId, CASE WHEN @GuestCountChange > 0 THEN 'GUEST_COUNT_INCREASE' ELSE 'GUEST_COUNT_DECREASE' END,
            @CurrentGuestCount, @NewGuestCount, @GuestCountChange,
            @CurrentGuestCount * @PricePerGuest, @AdditionalAmount, @PricingMultiplier,
            @ChangeReason,
            @UserId, 'CUSTOMER',
            @RequiresApproval,
            CASE WHEN @RequiresApproval = 1 THEN 'Pending' ELSE 'Approved' END
        );

        DECLARE @ModificationId BIGINT = SCOPE_IDENTITY();

        -- If no approval required and increase, update immediately
        IF @RequiresApproval = 0 AND @GuestCountChange > 0
        BEGIN
            DECLARE @ApprovedByType VARCHAR(20) = 'PARTNER';

            IF EXISTS (
                SELECT 1
                FROM sys.check_constraints cc
                WHERE cc.parent_object_id = OBJECT_ID('dbo.t_sys_order_modifications')
                  AND cc.definition LIKE '%PARTNER%'
            )
                SET @ApprovedByType = 'PARTNER';

            UPDATE t_sys_orders
            SET c_guest_count = @NewGuestCount,
                c_total_amount = c_total_amount + @AdditionalAmount,
                c_modifieddate = GETDATE()
            WHERE c_orderid = @OrderId;

            UPDATE t_sys_order_modifications
            SET c_status = 'Approved',
                c_approved_by = @OwnerId,
                c_approved_by_type = @ApprovedByType,
                c_approval_date = GETDATE()
            WHERE c_modification_id = @ModificationId;
        END

        COMMIT TRANSACTION;

        -- Return modification details
        SELECT
            @ModificationId AS ModificationId,
            @GuestCountChange AS GuestCountChange,
            @AdditionalAmount AS AdditionalAmount,
            @PricingMultiplier AS PricingMultiplier,
            @RequiresApproval AS RequiresPartnerApproval,
            CASE
                WHEN @RequiresApproval = 1 THEN 'Request sent to partner for approval'
                WHEN @GuestCountChange > 0 THEN 'Guest count updated. Additional amount: ₹' + CAST(@AdditionalAmount AS VARCHAR) + ' to be paid'
                ELSE 'Refund amount: ₹' + CAST(ABS(@AdditionalAmount) AS VARCHAR)
            END AS Message;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '  ✓ sp_RequestGuestCountChange created';
PRINT '';

-- =============================================
-- SP 6: File Customer Complaint
-- =============================================

PRINT 'Creating sp_FileCustomerComplaint...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_FileCustomerComplaint]
    @OrderId BIGINT,
    @UserId BIGINT,
    @ComplaintType VARCHAR(50),
    @ComplaintSummary NVARCHAR(200),
    @ComplaintDetails NVARCHAR(MAX),
    @PhotoEvidence NVARCHAR(MAX) = NULL, -- JSON array
    @AffectedItems NVARCHAR(MAX) = NULL, -- JSON array
    @IssueOccurredAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @OwnerId BIGINT;
        DECLARE @TotalGuestCount INT;
        DECLARE @Severity VARCHAR(20);
        DECLARE @ComplaintHistoryCount INT;
        DECLARE @IsSuspicious BIT = 0;

        -- Get order details
        SELECT
            @OwnerId = c_ownerid,
            @TotalGuestCount = c_guest_count
        FROM t_sys_orders
        WHERE c_orderid = @OrderId AND c_userid = @UserId;

        IF @OwnerId IS NULL
        BEGIN
            THROW 50001, 'Order not found or does not belong to this user', 1;
        END

        -- Check customer complaint history
        SELECT @ComplaintHistoryCount = COUNT(*)
        FROM t_sys_order_complaints
        WHERE c_userid = @UserId
          AND c_createddate >= DATEADD(MONTH, -12, GETDATE());

        IF @ComplaintHistoryCount >= 3
            SET @IsSuspicious = 1; -- Flag customers with 3+ complaints in last 12 months

        -- Determine severity
        IF @ComplaintType IN ('NO_SHOW', 'PARTNER_NO_SHOW', 'FOOD_QUALITY')
           OR @ComplaintType LIKE '%[_]NO_SHOW'
            SET @Severity = 'CRITICAL';
        ELSE IF @ComplaintType IN ('QUANTITY_SHORT', 'LATE_ARRIVAL', 'SETUP_POOR')
            SET @Severity = 'MAJOR';
        ELSE
            SET @Severity = 'MINOR';

        -- Insert complaint
        INSERT INTO t_sys_order_complaints (
            c_orderid, c_userid, c_ownerid,
            c_complaint_type, c_severity,
            c_complaint_summary, c_complaint_details,
            c_affected_items, c_total_item_count, c_total_guest_count,
            c_photo_evidence_paths, c_issue_occurred_at,
            c_is_reported_during_event,
            c_is_flagged_suspicious, c_customer_complaint_history_count,
            c_status
        )
        VALUES (
            @OrderId, @UserId, @OwnerId,
            @ComplaintType, @Severity,
            @ComplaintSummary, @ComplaintDetails,
            @AffectedItems, 0, @TotalGuestCount,
            @PhotoEvidence, ISNULL(@IssueOccurredAt, GETDATE()),
            CASE WHEN @IssueOccurredAt IS NULL OR @IssueOccurredAt >= DATEADD(HOUR, -4, GETDATE()) THEN 1 ELSE 0 END,
            @IsSuspicious, @ComplaintHistoryCount,
            'Open'
        );

        DECLARE @ComplaintId BIGINT = SCOPE_IDENTITY();

        -- Notify partner
        -- TODO: Send notification to partner

        COMMIT TRANSACTION;

        -- Return complaint details
        SELECT
            @ComplaintId AS ComplaintId,
            @Severity AS Severity,
            'Your complaint has been filed. Our team will review it within 12 hours.' AS Message,
            CASE WHEN @IsSuspicious = 1 THEN 'This complaint will undergo additional verification due to your complaint history.' ELSE '' END AS AdditionalInfo;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '  ✓ sp_FileCustomerComplaint created';
PRINT '';

-- =============================================
-- SP 7: Calculate Complaint Refund
-- =============================================

PRINT 'Creating sp_CalculateComplaintRefund...';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CalculateComplaintRefund]
    @ComplaintId BIGINT,
    @RefundAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ComplaintType VARCHAR(50);
    DECLARE @AffectedItemCount INT;
    DECLARE @TotalItemCount INT;
    DECLARE @SeverityFactor DECIMAL(3,2);
    DECLARE @OrderTotal DECIMAL(18,2);
    DECLARE @ItemValue DECIMAL(18,2);
    DECLARE @MaxRefundPercentage DECIMAL(5,2) = 15.00;

    -- Get complaint details
    SELECT
        @ComplaintType = c.c_complaint_type,
        @AffectedItemCount = c.c_affected_item_count,
        @TotalItemCount = c.c_total_item_count,
        @OrderTotal = o.c_total_amount
    FROM t_sys_order_complaints c
    INNER JOIN t_sys_orders o ON c.c_orderid = o.c_orderid
    WHERE c.c_complaint_id = @ComplaintId;

    -- Determine severity factor
    IF @ComplaintType IN ('NO_SHOW', 'PARTNER_NO_SHOW')
       OR @ComplaintType LIKE '%[_]NO_SHOW'
    BEGIN
        SET @RefundAmount = @OrderTotal; -- 100% refund for no-show
        RETURN;
    END

    -- For partial issues, calculate based on item importance
    SET @SeverityFactor = 1.0; -- Default

    IF @ComplaintType = 'FOOD_QUALITY' SET @SeverityFactor = 2.0; -- Critical item
    ELSE IF @ComplaintType IN ('QUANTITY_SHORT', 'LATE_ARRIVAL') SET @SeverityFactor = 1.5; -- Important
    ELSE IF @ComplaintType = 'SETUP_POOR' SET @SeverityFactor = 1.0; -- Normal
    ELSE IF @ComplaintType IN ('FOOD_COLD', 'PARTIAL_ISSUE') SET @SeverityFactor = 0.5; -- Minor

    -- Calculate item value
    IF @TotalItemCount > 0
        SET @ItemValue = @OrderTotal / @TotalItemCount;
    ELSE
        SET @ItemValue = @OrderTotal * 0.1; -- Assume 10% if unknown

    -- Calculate refund: (Item Value / Total) × Severity Factor × Total
    SET @RefundAmount = (@ItemValue / @OrderTotal) * @SeverityFactor * @OrderTotal;

    -- Cap at maximum refund percentage
    IF (@RefundAmount / @OrderTotal) > (@MaxRefundPercentage / 100.0)
        SET @RefundAmount = @OrderTotal * (@MaxRefundPercentage / 100.0);

    SELECT @RefundAmount AS CalculatedRefundAmount;
END
GO

PRINT '  ✓ sp_CalculateComplaintRefund created';
PRINT '';

-- =============================================
-- Summary
-- =============================================

PRINT '';
PRINT '================================================';
PRINT 'Stored Procedures Created Successfully';
PRINT '================================================';
PRINT '';
PRINT 'Procedures Created:';
PRINT '  ✓ sp_AutoLockGuestCount';
PRINT '  ✓ sp_AutoLockMenu';
PRINT '  ✓ sp_CalculateCancellationRefund';
PRINT '  ✓ sp_ProcessCancellationRequest';
PRINT '  ✓ sp_RequestGuestCountChange';
PRINT '  ✓ sp_FileCustomerComplaint';
PRINT '  ✓ sp_CalculateComplaintRefund';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run Financial_Strategy_Implementation.sql first';
PRINT '  2. Test each stored procedure with sample data';
PRINT '  3. Create C# API endpoints that call these procedures';
PRINT '  4. Set up background jobs for auto-lock procedures';
PRINT '';
PRINT '================================================';
GO
