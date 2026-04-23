-- =============================================
-- Transaction Management Fix - CRITICAL SECURITY UPDATE
-- Description: Wraps critical operations in atomic transactions
--              to prevent data corruption and revenue leakage
-- Version: 1.0
-- Date: 2026-02-19
-- Priority: P0 - MUST DEPLOY BEFORE PRODUCTION
-- =============================================

-- =============================================
-- Create Error Log Table (if not exists)
-- =============================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_name = 't_sys_error_log'
    ) THEN

        CREATE TABLE t_sys_error_log (
            c_error_id BIGSERIAL PRIMARY KEY,
            c_error_message TEXT NOT NULL,
            c_error_procedure VARCHAR(200),
            c_error_line INT,
            c_error_severity INT,
            c_error_state INT,
            c_error_number INT,
            c_additional_info TEXT,
            c_error_date TIMESTAMP DEFAULT NOW(),
            c_resolved BOOLEAN DEFAULT FALSE,
            c_resolved_date TIMESTAMP,
            c_resolved_by BIGINT
        );

        CREATE INDEX idx_errorlog_date ON t_sys_error_log (c_error_date DESC);
        CREATE INDEX idx_errorlog_resolved ON t_sys_error_log (c_resolved);

        RAISE NOTICE 'Table t_sys_error_log created successfully';

    ELSE
        RAISE NOTICE 'Table t_sys_error_log already exists';
    END IF;
END
$$;

-- =============================================
-- 1. Process Refund with Transaction
-- CRITICAL FIX: Lines 156-191 in CancellationRepository.cs
-- BEFORE: Separate queries, no transaction = revenue leakage
-- AFTER: Atomic transaction with rollback on failure
-- =============================================

CREATE OR REPLACE FUNCTION sp_ProcessRefundTransaction(
    p_CancellationId BIGINT,
    p_RefundTransactionId VARCHAR,
    p_RefundMethod VARCHAR,
    OUT p_Success BOOLEAN,
    OUT p_ErrorMessage VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OrderId BIGINT;
    v_RefundAmount DECIMAL(18,2);
    v_CateringOwnerId BIGINT;
    v_UserId BIGINT;
    v_TransactionId BIGINT;
BEGIN
    p_Success := FALSE;
    p_ErrorMessage := NULL;

    BEGIN
        -- Verify cancellation request exists and is approved
        IF NOT EXISTS (
            SELECT 1
            FROM t_sys_cancellation_requests
            WHERE c_cancellation_id = p_CancellationId
              AND c_status = 'Approved'
        ) THEN
            p_ErrorMessage := 'Cancellation request not found or not approved';
            RETURN;
        END IF;

        -- Get order details
        SELECT
            cr.c_orderid,
            cr.c_refund_amount,
            cr.c_userid,
            o.c_cateringownerid
        INTO
            v_OrderId,
            v_RefundAmount,
            v_UserId,
            v_CateringOwnerId
        FROM t_sys_cancellation_requests cr
        INNER JOIN t_sys_orders o ON cr.c_orderid = o.c_orderid
        WHERE cr.c_cancellation_id = p_CancellationId;

        -- 1. Update cancellation request status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Refunded',
            c_refund_initiated_date = NOW(),
            c_refund_completed_date = NOW(),
            c_refund_transaction_id = p_RefundTransactionId,
            c_refund_method = p_RefundMethod,
            c_modifieddate = NOW()
        WHERE c_cancellation_id = p_CancellationId
          AND c_status = 'Approved';

        IF NOT FOUND THEN
            p_ErrorMessage := 'Failed to update cancellation request status';
            RETURN;
        END IF;

        -- 2. Update order status
        UPDATE t_sys_orders
        SET c_order_status = 'Cancelled',
            c_payment_status = 'Refunded',
            c_modifieddate = NOW()
        WHERE c_orderid = v_OrderId;

        IF NOT FOUND THEN
            p_ErrorMessage := 'Failed to update order status';
            RETURN;
        END IF;

        -- 3. Insert order status history
        INSERT INTO t_sys_order_status_history (
            c_orderid, c_status, c_remarks, c_modifieddate
        )
        VALUES (
            v_OrderId,
            'Cancelled',
            'Order cancelled and refund processed. Transaction ID: ' || p_RefundTransactionId,
            NOW()
        );

        -- 4. Update payment summary
        IF EXISTS (
            SELECT 1 FROM t_sys_order_payment_summary WHERE c_orderid = v_OrderId
        ) THEN
            UPDATE t_sys_order_payment_summary
            SET c_refundamount = v_RefundAmount,
                c_refundstatus = 'COMPLETED',
                c_refunddate = NOW(),
                c_modifieddate = NOW()
            WHERE c_orderid = v_OrderId;
        END IF;

        -- 5. Record refund transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid,
            c_userid,
            c_cateringownerid,
            c_transactiontype,
            c_amount,
            c_paymentmethod,
            c_paymentgateway,
            c_gateway_transactionid,
            c_paymentstatus,
            c_completeddate,
            c_createddate
        )
        VALUES (
            v_OrderId,
            v_UserId,
            v_CateringOwnerId,
            'REFUND',
            v_RefundAmount,
            p_RefundMethod,
            'Razorpay',
            p_RefundTransactionId,
            'SUCCESS',
            NOW(),
            NOW()
        )
        RETURNING c_transactionid INTO v_TransactionId;

        -- 6. Update escrow ledger
        INSERT INTO t_sys_escrow_ledger (
            c_orderid,
            c_transactionid,
            c_transactiontype,
            c_amount,
            c_fromentity,
            c_toentity,
            c_status,
            c_description,
            c_createddate
        )
        VALUES (
            v_OrderId,
            v_TransactionId,
            'DEBIT',
            v_RefundAmount,
            'ADMIN',
            'CUSTOMER',
            'COMPLETED',
            'Refund processed for cancelled order',
            NOW()
        );

        p_Success := TRUE;

    EXCEPTION
        WHEN OTHERS THEN
            p_ErrorMessage := SQLERRM;
            p_Success := FALSE;

            INSERT INTO t_sys_error_log (
                c_error_message,
                c_error_procedure,
                c_error_line,
                c_error_severity,
                c_error_date
            )
            VALUES (
                SQLERRM,
                'sp_ProcessRefundTransaction',
                0,
                0,
                NOW()
            );
    END;
END;
$$;

-- =============================================
-- 2. Update Payment Stage with Order Status (Transaction Wrapper)
-- CRITICAL FIX: Payment verification must update both payment stage AND order status atomically
-- =============================================

CREATE OR REPLACE FUNCTION sp_UpdatePaymentStageWithOrderStatus(
    p_PaymentStageId BIGINT,
    p_OrderId BIGINT,
    p_StageType VARCHAR,
    p_Status VARCHAR,
    p_PaymentMethod VARCHAR,
    p_PaymentGateway VARCHAR,
    p_RazorpayOrderId VARCHAR,
    p_RazorpayPaymentId VARCHAR,
    p_TransactionId VARCHAR,
    p_UpiId VARCHAR,
    p_NewOrderStatus VARCHAR,
    OUT p_Success BOOLEAN,
    OUT p_ErrorMessage VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_Success := FALSE;
    p_ErrorMessage := NULL;

    BEGIN
        -- 1. Update payment stage status
        UPDATE t_sys_order_payment_stages
        SET c_status = p_Status,
            c_payment_method = p_PaymentMethod,
            c_payment_gateway = p_PaymentGateway,
            c_razorpay_order_id = p_RazorpayOrderId,
            c_razorpay_payment_id = p_RazorpayPaymentId,
            c_transaction_id = p_TransactionId,
            c_upi_id = p_UpiId,
            c_payment_date = NOW()
        WHERE c_payment_stage_id = p_PaymentStageId;

        IF NOT FOUND THEN
            p_ErrorMessage := 'Failed to update payment stage';
            RETURN;
        END IF;

        -- 2. Update order status
        IF p_StageType IN ('PreBooking', 'Full') AND p_NewOrderStatus IS NOT NULL THEN
            UPDATE t_sys_orders
            SET c_order_status = p_NewOrderStatus,
                c_payment_status = CASE
                    WHEN p_StageType = 'Full' THEN 'Fully Paid'
                    ELSE 'Advance Paid'
                END,
                c_modifieddate = NOW()
            WHERE c_orderid = p_OrderId;

            IF NOT FOUND THEN
                p_ErrorMessage := 'Failed to update order status';
                RETURN;
            END IF;

            -- 3. Insert order status history
            INSERT INTO t_sys_order_status_history (
                c_orderid, c_status, c_remarks, c_modifieddate
            )
            VALUES (
                p_OrderId,
                p_NewOrderStatus,
                'Order confirmed after successful ' || p_StageType ||
                ' payment via ' || p_PaymentGateway ||
                ' (Payment ID: ' || p_RazorpayPaymentId || ')',
                NOW()
            );
        END IF;

        p_Success := TRUE;

    EXCEPTION
        WHEN OTHERS THEN
            p_ErrorMessage := SQLERRM;
            p_Success := FALSE;

            INSERT INTO t_sys_error_log (
                c_error_message,
                c_error_procedure,
                c_error_line,
                c_error_severity,
                c_error_date
            )
            VALUES (
                'CRITICAL: Payment verified but DB update failed - OrderId: ' ||
                p_OrderId || ', Error: ' || SQLERRM,
                'sp_UpdatePaymentStageWithOrderStatus',
                0,
                0,
                NOW()
            );
    END;
END;
$$;

-- =============================================
-- 3. Approve Cancellation with Transaction
-- CRITICAL FIX: Admin approval must be atomic
-- =============================================

CREATE OR REPLACE FUNCTION sp_ApproveCancellationTransaction(
    p_CancellationId BIGINT,
    p_AdminId BIGINT,
    p_AdminNotes VARCHAR,
    OUT p_Success BOOLEAN,
    OUT p_ErrorMessage VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_Success := FALSE;
    p_ErrorMessage := NULL;

    BEGIN
        -- Verify cancellation exists and is pending
        IF NOT EXISTS (
            SELECT 1
            FROM t_sys_cancellation_requests
            WHERE c_cancellation_id = p_CancellationId
              AND c_status = 'Pending'
        ) THEN
            p_ErrorMessage := 'Cancellation request not found or already processed';
            RETURN;
        END IF;

        -- Update cancellation status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Approved',
            c_admin_approved_by = p_AdminId,
            c_admin_approval_date = NOW(),
            c_admin_notes = p_AdminNotes,
            c_modifieddate = NOW()
        WHERE c_cancellation_id = p_CancellationId
          AND c_status = 'Pending';

        IF NOT FOUND THEN
            p_ErrorMessage := 'Failed to approve cancellation request';
            RETURN;
        END IF;

        -- Update order status
        UPDATE t_sys_orders
        SET c_order_status = 'Cancellation_Approved',
            c_modifieddate = NOW()
        WHERE c_orderid = (
            SELECT c_orderid
            FROM t_sys_cancellation_requests
            WHERE c_cancellation_id = p_CancellationId
        );

        -- Insert status history
        INSERT INTO t_sys_order_status_history (
            c_orderid, c_status, c_remarks, c_modifieddate
        )
        SELECT
            c_orderid,
            'Cancellation_Approved',
            'Cancellation request approved by admin. Refund will be processed.',
            NOW()
        FROM t_sys_cancellation_requests
        WHERE c_cancellation_id = p_CancellationId;

        p_Success := TRUE;

    EXCEPTION
        WHEN OTHERS THEN
            p_ErrorMessage := SQLERRM;
            p_Success := FALSE;
    END;
END;
$$;

-- =============================================
-- 4. Reject Cancellation with Transaction
-- =============================================

CREATE OR REPLACE FUNCTION sp_RejectCancellationTransaction(
    p_CancellationId BIGINT,
    p_AdminId BIGINT,
    p_RejectionReason VARCHAR,
    OUT p_Success BOOLEAN,
    OUT p_ErrorMessage VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_Success := FALSE;
    p_ErrorMessage := NULL;

    BEGIN
        -- Update cancellation status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Rejected',
            c_admin_approved_by = p_AdminId,
            c_admin_approval_date = NOW(),
            c_admin_notes = p_RejectionReason,
            c_modifieddate = NOW()
        WHERE c_cancellation_id = p_CancellationId
          AND c_status = 'Pending';

        IF NOT FOUND THEN
            p_ErrorMessage := 'Failed to reject cancellation request';
            RETURN;
        END IF;

        -- Insert status history
        INSERT INTO t_sys_order_status_history (
            c_orderid, c_status, c_remarks, c_modifieddate
        )
        SELECT
            c_orderid,
            'Cancellation_Rejected',
            'Cancellation request rejected. Reason: ' || p_RejectionReason,
            NOW()
        FROM t_sys_cancellation_requests
        WHERE c_cancellation_id = p_CancellationId;

        p_Success := TRUE;

    EXCEPTION
        WHEN OTHERS THEN
            p_ErrorMessage := SQLERRM;
            p_Success := FALSE;
    END;
END;
$$;
