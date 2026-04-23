-- =============================================
-- Financial Strategy Stored Procedures
-- Date: 2026-01-30
-- Purpose: Business logic implementation for financial strategy
-- =============================================


-- =============================================
-- SP 1: Auto-Lock Guest Count
-- =============================================

CREATE OR REPLACE FUNCTION sp_AutoLockGuestCount()
RETURNS TABLE (OrdersLocked INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_LockDays       INTEGER;
    v_ProcessedCount INTEGER := 0;
BEGIN
    -- Get lock period from settings (default 5 days)
    SELECT CAST(c_setting_value AS INTEGER) INTO v_LockDays
    FROM t_sys_settings
    WHERE c_setting_key = 'ORDER.GUEST_COUNT_LOCK_DAYS';

    IF v_LockDays IS NULL THEN
        v_LockDays := 5;
    END IF;

    -- Lock guest count for all orders that are 5 days away and not yet locked
    UPDATE t_sys_orders
    SET c_guest_count_locked      = TRUE,
        c_locked_guest_count      = c_guest_count,
        c_original_guest_count    = COALESCE(c_original_guest_count, c_guest_count),
        c_guest_lock_date = NOW(),
        c_modifieddate            = NOW()
    WHERE c_guest_count_locked = FALSE
      AND c_order_status NOT IN ('Cancelled', 'Completed', 'Rejected')
      AND (c_event_date::DATE - NOW()::DATE) <= v_LockDays
      AND c_event_date >= NOW();

    GET DIAGNOSTICS v_ProcessedCount = ROW_COUNT;

    -- Update job status
    UPDATE t_sys_auto_lock_jobs
    SET c_last_run_date              = NOW(),
        c_next_run_date              = NOW() + (c_run_frequency_minutes || ' minutes')::INTERVAL,
        c_orders_processed_last_run  = v_ProcessedCount
    WHERE c_job_type = 'GUEST_COUNT_LOCK';

    RETURN QUERY SELECT v_ProcessedCount;
END;
$$;


-- =============================================
-- SP 2: Auto-Lock Menu
-- =============================================

CREATE OR REPLACE FUNCTION sp_AutoLockMenu()
RETURNS TABLE (OrdersLocked INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_LockDays       INTEGER;
    v_ProcessedCount INTEGER := 0;
BEGIN
    -- Get lock period from settings (default 3 days)
    SELECT CAST(c_setting_value AS INTEGER) INTO v_LockDays
    FROM t_sys_settings
    WHERE c_setting_key = 'ORDER.MENU_LOCK_DAYS';

    IF v_LockDays IS NULL THEN
        v_LockDays := 3;
    END IF;

    -- Lock menu for all orders that are 3 days away and not yet locked
    UPDATE t_sys_orders
    SET c_menu_locked      = TRUE,
        c_menu_lock_date = NOW(),
        c_modifieddate     = NOW()
    WHERE c_menu_locked = FALSE
      AND c_order_status NOT IN ('Cancelled', 'Completed', 'Rejected')
      AND (c_event_date::DATE - NOW()::DATE) <= v_LockDays
      AND c_event_date >= NOW();

    GET DIAGNOSTICS v_ProcessedCount = ROW_COUNT;

    -- Update job status
    UPDATE t_sys_auto_lock_jobs
    SET c_last_run_date             = NOW(),
        c_next_run_date             = NOW() + (c_run_frequency_minutes || ' minutes')::INTERVAL,
        c_orders_processed_last_run = v_ProcessedCount
    WHERE c_job_type = 'MENU_LOCK';

    RETURN QUERY SELECT v_ProcessedCount;
END;
$$;


-- =============================================
-- SP 3: Calculate Refund Amount (Cancellation Policy)
-- =============================================

CREATE OR REPLACE FUNCTION sp_CalculateCancellationRefund(
    p_OrderId                BIGINT,
    OUT p_RefundPercentage   DECIMAL(5,2),
    OUT p_RefundAmount       DECIMAL(18,2),
    OUT p_PolicyTier         VARCHAR(20),
    OUT p_PartnerCompensation DECIMAL(18,2)
)
RETURNS RECORD
LANGUAGE plpgsql AS $$
DECLARE
    v_EventDate                 TIMESTAMP;
    v_OrderTotal                DECIMAL(18,2);
    v_AdvancePaid               DECIMAL(18,2);
    v_HoursBeforeEvent          INTEGER;
    v_DaysBeforeEvent           INTEGER;
    v_FullRefundDays            INTEGER := 7;
    v_PartialRefundStartDays    INTEGER := 3;
    v_PartialRefundEndDays      INTEGER := 7;
    v_NoRefundHours             INTEGER := 48;
    v_PartialRefundPercentage   DECIMAL(5,2) := 50.00;
BEGIN
    -- Get settings
    SELECT CAST(c_setting_value AS INTEGER) INTO v_FullRefundDays
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.FULL_REFUND_DAYS';

    SELECT CAST(c_setting_value AS INTEGER) INTO v_NoRefundHours
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.NO_REFUND_HOURS';

    SELECT CAST(c_setting_value AS DECIMAL(5,2)) INTO v_PartialRefundPercentage
    FROM t_sys_settings WHERE c_setting_key = 'CANCELLATION.PARTIAL_REFUND_PERCENTAGE';

    -- Get order details
    SELECT c_event_date, c_total_amount
    INTO v_EventDate, v_OrderTotal
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId;

    -- Calculate time difference
    v_HoursBeforeEvent := EXTRACT(EPOCH FROM (v_EventDate - NOW())) / 3600;
    v_DaysBeforeEvent  := v_EventDate::DATE - NOW()::DATE;

    -- Get advance paid from payment summary
    SELECT c_advanceamount INTO v_AdvancePaid
    FROM t_sys_order_payment_summary
    WHERE c_orderid = p_OrderId;

    IF v_AdvancePaid IS NULL THEN
        v_AdvancePaid := v_OrderTotal * 0.5; -- Assume 50% if not in summary
    END IF;

    -- Determine policy tier and refund percentage
    IF v_DaysBeforeEvent > v_FullRefundDays THEN
        -- >7 days: 100% refund
        p_PolicyTier         := 'FULL_REFUND';
        p_RefundPercentage   := 100.00;
        p_PartnerCompensation := 0;

    ELSIF v_DaysBeforeEvent >= v_PartialRefundStartDays AND v_DaysBeforeEvent <= v_PartialRefundEndDays THEN
        -- 3-7 days: 50% refund
        p_PolicyTier         := 'PARTIAL_REFUND';
        p_RefundPercentage   := v_PartialRefundPercentage;
        p_PartnerCompensation := v_AdvancePaid * (1 - (v_PartialRefundPercentage / 100.0));

    ELSIF v_HoursBeforeEvent < v_NoRefundHours THEN
        -- <48 hours: No refund
        p_PolicyTier         := 'NO_REFUND';
        p_RefundPercentage   := 0.00;
        p_PartnerCompensation := v_AdvancePaid;

    ELSE
        -- Default to partial
        p_PolicyTier         := 'PARTIAL_REFUND';
        p_RefundPercentage   := v_PartialRefundPercentage;
        p_PartnerCompensation := v_AdvancePaid * (1 - (v_PartialRefundPercentage / 100.0));
    END IF;

    -- Calculate refund amount
    p_RefundAmount := v_AdvancePaid * (p_RefundPercentage / 100.0);
END;
$$;


-- =============================================
-- SP 4: Process Cancellation Request
-- =============================================

CREATE OR REPLACE FUNCTION sp_ProcessCancellationRequest(
    p_OrderId              BIGINT,
    p_UserId               BIGINT,
    p_CancellationReason   VARCHAR(1000),
    p_IsForceMajeure       BOOLEAN DEFAULT FALSE,
    p_ForceMajeureEvidence TEXT DEFAULT NULL
)
RETURNS TABLE (
    CancellationId      BIGINT,
    PolicyTier          VARCHAR(20),
    RefundPercentage    DECIMAL(5,2),
    RefundAmount        DECIMAL(18,2),
    PartnerCompensation DECIMAL(18,2),
    DaysBeforeEvent     INTEGER,
    Message             TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_OwnerId                    BIGINT;
    v_EventDate                  TIMESTAMP;
    v_HoursBeforeEvent           INTEGER;
    v_DaysBeforeEvent            INTEGER;
    v_PolicyTier                 VARCHAR(20);
    v_RefundPercentage           DECIMAL(5,2);
    v_RefundAmount               DECIMAL(18,2);
    v_PartnerCompensation        DECIMAL(18,2);
    v_OrderTotal                 DECIMAL(18,2);
    v_AdvancePaid                DECIMAL(18,2);
    v_CommissionRate             DECIMAL(5,2);
    v_PlatformCommissionForfeited DECIMAL(18,2);
    v_CancellationId             BIGINT;
    v_RefundResult               RECORD;
BEGIN
    -- Get order details
    SELECT c_ownerid, c_event_date, c_total_amount, c_commission_rate
    INTO v_OwnerId, v_EventDate, v_OrderTotal, v_CommissionRate
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId AND c_userid = p_UserId;

    IF v_OwnerId IS NULL THEN
        RAISE EXCEPTION 'Order not found or does not belong to this user' USING ERRCODE = 'P0001';
    END IF;

    -- Calculate refund via SP 3
    SELECT p_RefundPercentage, p_RefundAmount, p_PolicyTier, p_PartnerCompensation
    INTO v_RefundPercentage, v_RefundAmount, v_PolicyTier, v_PartnerCompensation
    FROM sp_CalculateCancellationRefund(p_OrderId);

    SELECT c_advanceamount INTO v_AdvancePaid
    FROM t_sys_order_payment_summary
    WHERE c_orderid = p_OrderId;

    IF v_AdvancePaid IS NULL THEN
        v_AdvancePaid := v_OrderTotal * 0.5;
    END IF;

    v_HoursBeforeEvent := EXTRACT(EPOCH FROM (v_EventDate - NOW())) / 3600;
    v_DaysBeforeEvent  := v_EventDate::DATE - NOW()::DATE;

    -- Force majeure overrides policy
    IF p_IsForceMajeure THEN
        v_PolicyTier                  := 'FORCE_MAJEURE';
        v_RefundPercentage            := 50.00; -- Split 50-50 for force majeure
        v_RefundAmount                := v_AdvancePaid * 0.5;
        v_PartnerCompensation         := v_AdvancePaid * 0.5;
        v_PlatformCommissionForfeited := v_AdvancePaid * (v_CommissionRate / 100.0);
    ELSE
        v_PlatformCommissionForfeited := 0; -- Platform keeps commission for normal cancellations
    END IF;

    -- Insert cancellation request
    INSERT INTO t_sys_cancellation_requests (
        c_orderid, c_userid, c_ownerid,
        c_event_date, c_hours_before_event, c_days_before_event,
        c_policy_tier, c_refund_percentage,
        c_order_total_amount, c_advance_paid,
        c_refund_amount, c_retention_amount,
        c_partner_compensation, c_platform_commission_forfeited,
        c_cancellation_reason, c_is_force_majeure, c_force_majeure_evidence,
        c_status
    )
    VALUES (
        p_OrderId, p_UserId, v_OwnerId,
        v_EventDate, v_HoursBeforeEvent, v_DaysBeforeEvent,
        v_PolicyTier, v_RefundPercentage,
        v_OrderTotal, v_AdvancePaid,
        v_RefundAmount, (v_AdvancePaid - v_RefundAmount),
        v_PartnerCompensation, v_PlatformCommissionForfeited,
        p_CancellationReason, p_IsForceMajeure, p_ForceMajeureEvidence,
        'Pending'
    )
    RETURNING c_cancellation_id INTO v_CancellationId;

    -- Update order status
    UPDATE t_sys_orders
    SET c_order_status = 'Cancellation_Requested',
        c_modifieddate = NOW()
    WHERE c_orderid = p_OrderId;

    -- Add to order history
    INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
    VALUES (
        p_OrderId,
        'Cancellation_Requested',
        'Customer requested cancellation. Policy: ' || v_PolicyTier || ', Refund: ' || v_RefundPercentage::TEXT || '%',
        NOW()
    );

    RETURN QUERY SELECT
        v_CancellationId,
        v_PolicyTier,
        v_RefundPercentage,
        v_RefundAmount,
        v_PartnerCompensation,
        v_DaysBeforeEvent,
        'Your cancellation request has been submitted. Expected refund: ₹' || v_RefundAmount::TEXT;

EXCEPTION WHEN OTHERS THEN
    RAISE;
END;
$$;


-- =============================================
-- SP 5: Request Guest Count Change
-- =============================================

CREATE OR REPLACE FUNCTION sp_RequestGuestCountChange(
    p_OrderId      BIGINT,
    p_UserId       BIGINT,
    p_NewGuestCount INTEGER,
    p_ChangeReason VARCHAR(500)
)
RETURNS TABLE (
    ModificationId        BIGINT,
    GuestCountChange      INTEGER,
    AdditionalAmount      DECIMAL(18,2),
    PricingMultiplier     DECIMAL(5,2),
    RequiresPartnerApproval BOOLEAN,
    Message               TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_CurrentGuestCount INTEGER;
    v_LockedGuestCount  INTEGER;
    v_IsLocked          BOOLEAN;
    v_EventDate         TIMESTAMP;
    v_DaysBeforeEvent   INTEGER;
    v_PricePerGuest     DECIMAL(18,2);
    v_AdditionalAmount  DECIMAL(18,2);
    v_PricingMultiplier DECIMAL(5,2);
    v_GuestCountChange  INTEGER;
    v_OwnerId           BIGINT;
    v_RequiresApproval  BOOLEAN := FALSE;
    v_ModificationId    BIGINT;
BEGIN
    -- Get order details
    SELECT
        c_guest_count,
        c_locked_guest_count,
        c_guest_count_locked,
        c_event_date,
        c_ownerid,
        c_total_amount / c_guest_count
    INTO
        v_CurrentGuestCount,
        v_LockedGuestCount,
        v_IsLocked,
        v_EventDate,
        v_OwnerId,
        v_PricePerGuest
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId AND c_userid = p_UserId;

    IF v_OwnerId IS NULL THEN
        RAISE EXCEPTION 'Order not found or does not belong to this user' USING ERRCODE = 'P0001';
    END IF;

    v_GuestCountChange := p_NewGuestCount - COALESCE(v_LockedGuestCount, v_CurrentGuestCount);

    -- Check if decrease is allowed
    IF v_GuestCountChange < 0 THEN
        IF v_IsLocked THEN
            RAISE EXCEPTION 'Guest count decrease is not allowed after lock period' USING ERRCODE = 'P0002';
        END IF;

        v_DaysBeforeEvent := v_EventDate::DATE - NOW()::DATE;
        IF v_DaysBeforeEvent <= 7 THEN
            RAISE EXCEPTION 'Guest count decrease is only allowed more than 7 days before event' USING ERRCODE = 'P0003';
        END IF;

        v_RequiresApproval := TRUE; -- Partner must approve decrease
    END IF;

    -- Calculate pricing multiplier based on timing
    v_DaysBeforeEvent := v_EventDate::DATE - NOW()::DATE;

    IF v_DaysBeforeEvent > 7 THEN
        v_PricingMultiplier := 1.0;
    ELSIF v_DaysBeforeEvent BETWEEN 5 AND 7 THEN
        v_PricingMultiplier := 1.2;
    ELSIF v_DaysBeforeEvent BETWEEN 3 AND 4 THEN
        v_PricingMultiplier := 1.3;
    ELSIF v_DaysBeforeEvent >= 2 THEN
        v_PricingMultiplier := 1.5;
    ELSE
        RAISE EXCEPTION 'Guest count changes not allowed within 48 hours of event' USING ERRCODE = 'P0004';
    END IF;

    -- Calculate additional amount
    v_AdditionalAmount := (v_PricePerGuest * ABS(v_GuestCountChange)) * v_PricingMultiplier;

    IF v_GuestCountChange < 0 THEN
        v_AdditionalAmount := -v_AdditionalAmount; -- Negative for decreases
    END IF;

    -- Check if partner approval required for increases after lock
    IF v_IsLocked AND v_GuestCountChange > 0 THEN
        IF (p_NewGuestCount - v_LockedGuestCount) > (v_LockedGuestCount * 0.10) THEN -- >10% increase
            v_RequiresApproval := TRUE;
        END IF;
    END IF;

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
        p_OrderId,
        CASE WHEN v_GuestCountChange > 0 THEN 'GUEST_COUNT_INCREASE' ELSE 'GUEST_COUNT_DECREASE' END,
        v_CurrentGuestCount, p_NewGuestCount, v_GuestCountChange,
        v_CurrentGuestCount * v_PricePerGuest, v_AdditionalAmount, v_PricingMultiplier,
        p_ChangeReason,
        p_UserId, 'CUSTOMER',
        v_RequiresApproval,
        CASE WHEN v_RequiresApproval THEN 'Pending' ELSE 'Approved' END
    )
    RETURNING c_modification_id INTO v_ModificationId;

    -- If no approval required and increase, update immediately
    IF NOT v_RequiresApproval AND v_GuestCountChange > 0 THEN
        UPDATE t_sys_orders
        SET c_guest_count   = p_NewGuestCount,
            c_total_amount  = c_total_amount + v_AdditionalAmount,
            c_modifieddate  = NOW()
        WHERE c_orderid = p_OrderId;

        UPDATE t_sys_order_modifications
        SET c_status          = 'Approved',
            c_approved_by     = v_OwnerId,
            c_approved_by_type = 'PARTNER',
            c_approval_date   = NOW()
        WHERE c_modification_id = v_ModificationId;
    END IF;

    RETURN QUERY SELECT
        v_ModificationId,
        v_GuestCountChange,
        v_AdditionalAmount,
        v_PricingMultiplier,
        v_RequiresApproval,
        CASE
            WHEN v_RequiresApproval         THEN 'Request sent to partner for approval'
            WHEN v_GuestCountChange > 0     THEN 'Guest count updated. Additional amount: ₹' || v_AdditionalAmount::TEXT || ' to be paid'
            ELSE                                 'Refund amount: ₹' || ABS(v_AdditionalAmount)::TEXT
        END;

EXCEPTION WHEN OTHERS THEN
    RAISE;
END;
$$;


-- =============================================
-- SP 6: File Customer Complaint
-- =============================================

CREATE OR REPLACE FUNCTION sp_FileCustomerComplaint(
    p_OrderId          BIGINT,
    p_UserId           BIGINT,
    p_ComplaintType    VARCHAR(50),
    p_ComplaintSummary VARCHAR(200),
    p_ComplaintDetails TEXT,
    p_PhotoEvidence    TEXT DEFAULT NULL,  -- JSON array
    p_AffectedItems    TEXT DEFAULT NULL,  -- JSON array
    p_IssueOccurredAt  TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    ComplaintId    BIGINT,
    Severity       VARCHAR(20),
    Message        TEXT,
    AdditionalInfo TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_OwnerId              BIGINT;
    v_TotalGuestCount      INTEGER;
    v_Severity             VARCHAR(20);
    v_ComplaintHistoryCount INTEGER;
    v_IsSuspicious         BOOLEAN := FALSE;
    v_ComplaintId          BIGINT;
BEGIN
    -- Get order details
    SELECT c_ownerid, c_guest_count
    INTO v_OwnerId, v_TotalGuestCount
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId AND c_userid = p_UserId;

    IF v_OwnerId IS NULL THEN
        RAISE EXCEPTION 'Order not found or does not belong to this user' USING ERRCODE = 'P0001';
    END IF;

    -- Check customer complaint history
    SELECT COUNT(*) INTO v_ComplaintHistoryCount
    FROM t_sys_order_complaints
    WHERE c_userid = p_UserId
      AND c_createddate >= NOW() - INTERVAL '12 months';

    IF v_ComplaintHistoryCount >= 3 THEN
        v_IsSuspicious := TRUE; -- Flag customers with 3+ complaints in last 12 months
    END IF;

    -- Determine severity
    IF p_ComplaintType IN ('NO_SHOW', 'PARTNER_NO_SHOW', 'FOOD_QUALITY') THEN
        v_Severity := 'CRITICAL';
    ELSIF p_ComplaintType IN ('QUANTITY_SHORT', 'LATE_ARRIVAL', 'SETUP_POOR') THEN
        v_Severity := 'MAJOR';
    ELSE
        v_Severity := 'MINOR';
    END IF;

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
        p_OrderId, p_UserId, v_OwnerId,
        p_ComplaintType, v_Severity,
        p_ComplaintSummary, p_ComplaintDetails,
        p_AffectedItems, 0, v_TotalGuestCount,
        p_PhotoEvidence, COALESCE(p_IssueOccurredAt, NOW()),
        CASE WHEN p_IssueOccurredAt IS NULL OR p_IssueOccurredAt >= NOW() - INTERVAL '4 hours' THEN TRUE ELSE FALSE END,
        v_IsSuspicious, v_ComplaintHistoryCount,
        'Open'
    )
    RETURNING c_complaint_id INTO v_ComplaintId;

    -- TODO: Send notification to partner

    RETURN QUERY SELECT
        v_ComplaintId,
        v_Severity,
        'Your complaint has been filed. Our team will review it within 12 hours.'::TEXT,
        CASE WHEN v_IsSuspicious
            THEN 'This complaint will undergo additional verification due to your complaint history.'
            ELSE ''
        END::TEXT;

EXCEPTION WHEN OTHERS THEN
    RAISE;
END;
$$;


-- =============================================
-- SP 7: Calculate Complaint Refund
-- =============================================

CREATE OR REPLACE FUNCTION sp_CalculateComplaintRefund(
    p_ComplaintId        BIGINT,
    OUT p_RefundAmount   NUMERIC(18,2)
)
LANGUAGE plpgsql AS $$
DECLARE
    v_ComplaintType       VARCHAR(50);
    v_AffectedItemCount   INTEGER;
    v_TotalItemCount      INTEGER;
    v_SeverityFactor      NUMERIC(3,2);
    v_OrderTotal          NUMERIC(18,2);
    v_ItemValue           NUMERIC(18,2);
    v_MaxRefundPercentage NUMERIC(5,2) := 15.00;
BEGIN
    -- Get complaint details
    SELECT
        c.c_complaint_type,
        c.c_affected_item_count,
        c.c_total_item_count,
        o.c_total_amount
    INTO
        v_ComplaintType,
        v_AffectedItemCount,
        v_TotalItemCount,
        v_OrderTotal
    FROM t_sys_order_complaints c
    INNER JOIN t_sys_orders o ON c.c_orderid = o.c_orderid
    WHERE c.c_complaint_id = p_ComplaintId;

    -- 100% refund for no-show
    IF v_ComplaintType IN ('NO_SHOW', 'PARTNER_NO_SHOW') THEN
        p_RefundAmount := v_OrderTotal;
        RETURN;
    END IF;

    -- Severity factor
    v_SeverityFactor := 1.0;

    IF v_ComplaintType = 'FOOD_QUALITY' THEN
        v_SeverityFactor := 2.0;
    ELSIF v_ComplaintType IN ('QUANTITY_SHORT', 'LATE_ARRIVAL') THEN
        v_SeverityFactor := 1.5;
    ELSIF v_ComplaintType = 'SETUP_POOR' THEN
        v_SeverityFactor := 1.0;
    ELSIF v_ComplaintType IN ('FOOD_COLD', 'PARTIAL_ISSUE') THEN
        v_SeverityFactor := 0.5;
    END IF;

    -- Item value
    IF v_TotalItemCount > 0 THEN
        v_ItemValue := v_OrderTotal / v_TotalItemCount;
    ELSE
        v_ItemValue := v_OrderTotal * 0.1;
    END IF;

    -- Refund calculation
    p_RefundAmount := (v_ItemValue / v_OrderTotal) * v_SeverityFactor * v_OrderTotal;

    -- Cap refund
    IF (p_RefundAmount / v_OrderTotal) > (v_MaxRefundPercentage / 100.0) THEN
        p_RefundAmount := v_OrderTotal * (v_MaxRefundPercentage / 100.0);
    END IF;

END;
$$;