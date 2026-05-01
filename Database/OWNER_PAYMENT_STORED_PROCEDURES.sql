/*
====================================================
OWNER PAYMENT SYSTEM - STORED PROCEDURES
====================================================
Correct Domain Model: Owner/Partner (NOT Vendor)

Procedures for:
- Creating owner payments
- Releasing settlements
- Partner approvals
- Payment calculations

Author: System Architect
Date: 2026-01-30
====================================================
*/
-- =============================================
-- SP: Create Owner Payment for Order
-- PostgreSQL Version (Final - C# Compatible)
-- =============================================

CREATE OR REPLACE FUNCTION sp_CreateOwnerPayment(
    p_order_id BIGINT,
    p_owner_id BIGINT,
    p_settlement_amount NUMERIC(18,2),
    p_platform_service_fee NUMERIC(18,2)
)
RETURNS TABLE (
    "OwnerPaymentId" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_net_settlement NUMERIC(18,2);
BEGIN

    -- Calculate net settlement
    v_net_settlement := p_settlement_amount - p_platform_service_fee;

    -- Insert and return ID directly
    RETURN QUERY
    INSERT INTO t_owner_payment (
        c_owner_id,
        c_orderid,
        c_settlement_amount,
        c_platform_service_fee,
        c_net_settlement_amount,
        c_status,
        c_createddate,
        c_modifieddate
    )
    VALUES (
        p_owner_id,
        p_order_id,
        p_settlement_amount,
        p_platform_service_fee,
        v_net_settlement,
        'PENDING',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING c_owner_payment_id;

END;
$$;

-- =============================================
-- SP: Escrow Owner Payment (Admin holds funds)
-- =============================================

CREATE OR REPLACE FUNCTION sp_EscrowOwnerPayment(
    p_owner_payment_id BIGINT,
    p_transaction_reference VARCHAR(100)
)
RETURNS TABLE (
    "RowsAffected" INT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rows_affected INT;
BEGIN

    -- Update escrow status
    UPDATE t_owner_payment
    SET c_status = 'ESCROWED',
        c_escrowed_at = CURRENT_TIMESTAMP,
        c_transaction_reference = p_transaction_reference,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_owner_payment_id = p_owner_payment_id
      AND c_status = 'PENDING';

    -- Get affected rows (equivalent to @@ROWCOUNT)
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;

    -- Return result
    RETURN QUERY SELECT v_rows_affected;

END;
$$;

-- =============================================
-- SP: Release Settlement to Catering Partner
-- =============================================

CREATE OR REPLACE FUNCTION sp_ReleaseOwnerSettlement(
    p_owner_payment_id BIGINT,
    p_payment_method VARCHAR(50),
    p_released_by BIGINT
)
RETURNS TABLE (
    "Success" INT,
    "Message" VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_owner_id BIGINT;
    v_net_settlement NUMERIC(18,2);
BEGIN

    -- Get owner details
    SELECT 
        c_owner_id,
        c_net_settlement_amount
    INTO 
        v_owner_id,
        v_net_settlement
    FROM t_owner_payment
    WHERE c_owner_payment_id = p_owner_payment_id
      AND c_status = 'ESCROWED';

    -- Validation
    IF v_owner_id IS NULL THEN
        RETURN QUERY 
        SELECT 0, 'Payment not found or not escrowed';
        RETURN;
    END IF;

    -- Update payment status
    UPDATE t_owner_payment
    SET c_status = 'RELEASED',
        c_released_at = CURRENT_TIMESTAMP,
        c_payment_method = p_payment_method,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_owner_payment_id = p_owner_payment_id;

    -- Create payout schedule entry
    INSERT INTO t_owner_payout_schedule (
        c_owner_id,
        c_scheduled_amount,
        c_scheduled_date,
        c_is_released,
        c_released_at,
        c_release_method,
        c_status
    )
    VALUES (
        v_owner_id,
        v_net_settlement,
        CURRENT_TIMESTAMP,
        1,
        CURRENT_TIMESTAMP,
        p_payment_method,
        'RELEASED'
    );

    -- Success response
    RETURN QUERY 
    SELECT 1, 'Settlement released to catering partner successfully';

END;
$$;

-- =============================================
-- SP: Calculate Platform Service Fee
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_CalculatePlatformServiceFee(
    p_gross_amount NUMERIC(18,2),
    p_owner_id BIGINT
)
RETURNS TABLE (
    "PlatformServiceFee" NUMERIC,
    "NetSettlementAmount" NUMERIC
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_fee_percentage NUMERIC(5,2);
    v_service_fee NUMERIC(18,2);
    v_net_settlement NUMERIC(18,2);
BEGIN

    -- Get platform fee configuration
    SELECT c_fee_value
    INTO v_fee_percentage
    FROM t_platform_fee_config
    WHERE c_fee_type = 'PERCENTAGE'
      AND c_is_active = TRUE
    LIMIT 1;

    -- Default to 10% if not configured
    IF v_fee_percentage IS NULL THEN
        v_fee_percentage := 10.00;
    END IF;

    -- Calculate fee
    v_service_fee := p_gross_amount * (v_fee_percentage / 100);
    v_net_settlement := p_gross_amount - v_service_fee;

    -- Return result (same as MSSQL SELECT)
    RETURN QUERY
    SELECT v_service_fee, v_net_settlement;

END;
$$;

-- =============================================
-- SP: Create Partner Approval Request
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_CreatePartnerApprovalRequest(
    p_owner_id BIGINT,
    p_order_id BIGINT,
    p_request_type VARCHAR(50),
    p_description VARCHAR(1000),
    p_request_data TEXT,
    p_requested_by_user_id BIGINT,
    p_response_time_hours INT DEFAULT 24
)
RETURNS TABLE (
    "ApprovalId" BIGINT,
    "Deadline" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_deadline TIMESTAMP;
BEGIN

    -- Calculate deadline
    v_deadline := CURRENT_TIMESTAMP + (p_response_time_hours || ' hours')::INTERVAL;

    -- Insert and return values
    RETURN QUERY
    INSERT INTO t_partner_approval_request (
        c_owner_id,
        c_orderid,
        c_request_type,
        c_description,
        c_request_data,
        c_requested_by_user_id,
        c_requested_at,
        c_deadline,
        c_response_time_hours,
        c_status,
        c_createddate,
        c_modifieddate
    )
    VALUES (
        p_owner_id,
        p_order_id,
        p_request_type,
        p_description,
        p_request_data,
        p_requested_by_user_id,
        CURRENT_TIMESTAMP,
        v_deadline,
        p_response_time_hours,
        'PENDING',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING 
        c_approval_id,
        v_deadline;

END;
$$;

-- =============================================
-- SP: Approve Partner Request
-- =============================================

CREATE OR REPLACE FUNCTION sp_ApprovePartnerRequest(
    p_approval_id BIGINT,
    p_approved_by_owner_id BIGINT,
    p_partner_notes VARCHAR(1000) DEFAULT NULL
)
RETURNS TABLE (
    "Success" INT,
    "Message" VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rows_affected INT;
BEGIN

    -- Update approval
    UPDATE t_partner_approval_request
    SET c_status = 'APPROVED',
        c_approved_at = CURRENT_TIMESTAMP,
        c_approved_by_owner_id = p_approved_by_owner_id,
        c_partner_notes = p_partner_notes,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_approval_id = p_approval_id
      AND c_status = 'PENDING'
      AND c_deadline > CURRENT_TIMESTAMP;

    -- Get affected rows
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;

    -- Return result
    IF v_rows_affected > 0 THEN
        RETURN QUERY SELECT 1, 'Request approved by catering partner';
    ELSE
        RETURN QUERY SELECT 0, 'Request not found, already processed, or expired';
    END IF;

END;
$$;

-- =============================================
-- SP: Reject Partner Request
-- =============================================

CREATE OR REPLACE FUNCTION sp_RejectPartnerRequest(
    p_approval_id BIGINT,
    p_owner_id BIGINT,
    p_rejection_reason VARCHAR(500)
)
RETURNS TABLE (
    "Success" INT,
    "Message" VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rows_affected INT;
BEGIN

    -- Update rejection
    UPDATE t_partner_approval_request
    SET c_status = 'REJECTED',
        c_rejected_at = CURRENT_TIMESTAMP,
        c_rejection_reason = p_rejection_reason,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_approval_id = p_approval_id
      AND c_owner_id = p_owner_id
      AND c_status = 'PENDING';

    -- Get affected rows
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;

    -- Return result
    IF v_rows_affected > 0 THEN
        RETURN QUERY SELECT 1, 'Request rejected by catering partner';
    ELSE
        RETURN QUERY SELECT 0, 'Request not found or already processed';
    END IF;

END;
$$;

-- =============================================
-- SP: Mark Expired Partner Approvals
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_MarkExpiredPartnerApprovals()
RETURNS TABLE ("ExpiredCount" INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
BEGIN

    UPDATE t_partner_approval_request
    SET c_status = 'EXPIRED',
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_status = 'PENDING'
      AND c_deadline < CURRENT_TIMESTAMP;

    GET DIAGNOSTICS v_count = ROW_COUNT;

    RETURN QUERY SELECT v_count;

END;
$$;

-- =============================================
-- SP: Get Pending Partner Approvals
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetPendingPartnerApprovals(
    p_owner_id BIGINT
)
RETURNS TABLE (
    "ApprovalId" BIGINT,
    "OrderId" BIGINT,
    "OrderNumber" VARCHAR,
    "RequestType" VARCHAR,
    "Description" VARCHAR,
    "RequestData" TEXT,
    "RequestedAt" TIMESTAMP,
    "Deadline" TIMESTAMP,
    "ResponseTimeHours" INT,
    "Status" VARCHAR,
    "RequestedByCustomer" VARCHAR,
    "MinutesRemaining" INT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        pa.c_approval_id,
        pa.c_orderid,
        o.c_order_number,
        pa.c_request_type,
        pa.c_description,
        pa.c_request_data,
        pa.c_requested_at,
        pa.c_deadline,
        pa.c_response_time_hours,
        pa.c_status,
        (u.c_first_name || ' ' || u.c_last_name),
        EXTRACT(EPOCH FROM (pa.c_deadline - CURRENT_TIMESTAMP)) / 60
    FROM t_partner_approval_request pa
    INNER JOIN t_order o ON pa.c_orderid = o.c_orderid
    INNER JOIN t_sys_user u ON pa.c_requested_by_user_id = u.c_userid
    WHERE pa.c_owner_id = p_owner_id
      AND pa.c_status = 'PENDING'
      AND pa.c_deadline > CURRENT_TIMESTAMP
    ORDER BY pa.c_deadline ASC;

END;
$$;

-- =============================================
-- SP: Get Owner Settlement History
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetOwnerSettlementHistory(
    p_owner_id BIGINT,
    p_status VARCHAR DEFAULT NULL,
    p_from_date TIMESTAMP DEFAULT NULL,
    p_to_date TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    "OwnerPaymentId" BIGINT,
    "OrderId" BIGINT,
    "OrderNumber" VARCHAR,
    "SettlementAmount" NUMERIC,
    "PlatformServiceFee" NUMERIC,
    "NetSettlementAmount" NUMERIC,
    "Status" VARCHAR,
    "PaymentMethod" VARCHAR,
    "TransactionReference" VARCHAR,
    "EscrowedAt" TIMESTAMP,
    "ReleasedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        op.c_owner_payment_id,
        op.c_orderid,
        o.c_order_number,
        op.c_settlement_amount,
        op.c_platform_service_fee,
        op.c_net_settlement_amount,
        op.c_status,
        op.c_payment_method,
        op.c_transaction_reference,
        op.c_escrowed_at,
        op.c_released_at,
        op.c_createddate
    FROM t_owner_payment op
    INNER JOIN t_order o ON op.c_orderid = o.c_orderid
    WHERE op.c_owner_id = p_owner_id
      AND (p_status IS NULL OR op.c_status = p_status)
      AND (p_from_date IS NULL OR op.c_createddate >= p_from_date)
      AND (p_to_date IS NULL OR op.c_createddate <= p_to_date)
    ORDER BY op.c_createddate DESC;

END;
$$;

-- =============================================
-- SP: Get Partner Earnings Summary
-- PostgreSQL Version
-- =============================================

CREATE OR REPLACE FUNCTION sp_GetPartnerEarningsSummary(
    p_owner_id BIGINT,
    p_period_start TIMESTAMP,
    p_period_end TIMESTAMP
)
RETURNS TABLE (
    "TotalOrders" BIGINT,
    "TotalGrossEarnings" NUMERIC,
    "TotalPlatformFees" NUMERIC,
    "TotalNetEarnings" NUMERIC,
    "ReleasedAmount" NUMERIC,
    "EscrowedAmount" NUMERIC,
    "PendingAmount" NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        COUNT(*),
        SUM(c_settlement_amount),
        SUM(c_platform_service_fee),
        SUM(c_net_settlement_amount),
        SUM(CASE WHEN c_status = 'RELEASED' THEN c_net_settlement_amount ELSE 0 END),
        SUM(CASE WHEN c_status = 'ESCROWED' THEN c_net_settlement_amount ELSE 0 END),
        SUM(CASE WHEN c_status = 'PENDING' THEN c_net_settlement_amount ELSE 0 END)
    FROM t_owner_payment
    WHERE c_owner_id = p_owner_id
      AND c_createddate BETWEEN p_period_start AND p_period_end;

END;
$$;
