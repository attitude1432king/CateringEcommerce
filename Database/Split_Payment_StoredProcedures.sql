-- ===============================================
-- Split Payment Stored Procedures
-- ===============================================

-- ===============================================
-- 1. Initialize Order Payment Summary (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_InitializeOrderPayment(
    p_OrderId BIGINT,
    p_TotalAmount DECIMAL(18,2),
    p_CommissionRate DECIMAL(5,2),
    p_AdvancePercentage DECIMAL(5,2) DEFAULT 30.00
)
RETURNS TABLE (
    c_paymentsummaryid BIGINT,
    c_orderid BIGINT,
    c_totalamount DECIMAL(18,2),
    c_advanceamount DECIMAL(18,2),
    c_finalamount DECIMAL(18,2),
    c_commissionamount DECIMAL(18,2)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_AdvanceAmount DECIMAL(18,2);
    v_FinalAmount DECIMAL(18,2);
    v_CommissionAmount DECIMAL(18,2);
BEGIN
    -- Calculations (same logic)
    v_AdvanceAmount := (p_TotalAmount * p_AdvancePercentage / 100);
    v_FinalAmount := (p_TotalAmount - v_AdvanceAmount);
    v_CommissionAmount := (p_TotalAmount * p_CommissionRate / 100);

    -- Insert or Update
    IF EXISTS (
        SELECT 1 FROM t_sys_order_payment_summary WHERE c_orderid = p_OrderId
    ) THEN

        UPDATE t_sys_order_payment_summary
        SET c_totalamount = p_TotalAmount,
            c_advancepercentage = p_AdvancePercentage,
            c_advanceamount = v_AdvanceAmount,
            c_finalamount = v_FinalAmount,
            c_commissionrate = p_CommissionRate,
            c_commissionamount = v_CommissionAmount,
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

    ELSE

        INSERT INTO t_sys_order_payment_summary (
            c_orderid, c_totalamount, c_advancepercentage, c_advanceamount,
            c_finalamount, c_commissionrate, c_commissionamount,
            c_escrowstatus, c_vendorpayoutstatus
        )
        VALUES (
            p_OrderId, p_TotalAmount, p_AdvancePercentage, v_AdvanceAmount,
            v_FinalAmount, p_CommissionRate, v_CommissionAmount,
            'PENDING', 'PENDING'
        );

    END IF;

    -- Return result (same output)
    RETURN QUERY
    SELECT
        c_paymentsummaryid,
        c_orderid,
        c_totalamount,
        c_advanceamount,
        c_finalamount,
        c_commissionamount
    FROM t_sys_order_payment_summary
    WHERE c_orderid = p_OrderId;

END;
$$;

-- ===============================================
-- 2. Process Advance Payment (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_ProcessAdvancePayment(
    p_OrderId BIGINT,
    p_UserId BIGINT,
    p_CateringOwnerId BIGINT,
    p_Amount DECIMAL(18,2),
    p_PaymentMethod VARCHAR(50),
    p_PaymentGateway VARCHAR(50),
    p_GatewayTransactionId VARCHAR(200),
    p_GatewayPaymentId VARCHAR(200),
    p_GatewaySignature VARCHAR(500),
    p_IsEMI BOOLEAN DEFAULT FALSE,
    p_EMITenure INT DEFAULT NULL,
    p_EMIBank VARCHAR(100) DEFAULT NULL,
    p_EMIRate DECIMAL(5,2) DEFAULT NULL,
    p_EMIAmount DECIMAL(18,2) DEFAULT NULL
)
RETURNS TABLE (
    c_transactionid BIGINT,
    c_orderid BIGINT,
    c_amount DECIMAL(18,2),
    c_paymentstatus VARCHAR,
    c_completeddate TIMESTAMP,
    c_advanceamount DECIMAL(18,2),
    c_finalamount DECIMAL(18,2),
    c_escrowstatus VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_TransactionId BIGINT;
BEGIN
    -- PostgreSQL auto transaction (function runs in transaction)
    
    BEGIN
        -- Insert Payment Transaction (RETURNING replaces SCOPE_IDENTITY)
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod, c_paymentgateway,
            c_gateway_transactionid, c_gateway_paymentid, c_gateway_signature,
            c_paymentstatus, c_is_emi, c_emi_tenure, c_emi_bank, c_emi_rate, c_emi_amount,
            c_completeddate
        )
        VALUES (
            p_OrderId, p_UserId, p_CateringOwnerId,
            'ADVANCE', p_Amount, p_PaymentMethod, p_PaymentGateway,
            p_GatewayTransactionId, p_GatewayPaymentId, p_GatewaySignature,
            'SUCCESS', p_IsEMI, p_EMITenure, p_EMIBank, p_EMIRate, p_EMIAmount,
            NOW()
        )
        RETURNING c_transactionid INTO v_TransactionId;

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_advancepaid = 1,
            c_advancepaiddate = NOW(),
            c_escrowstatus = 'HELD',
            c_escrowamount = p_Amount,
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Add to Escrow Ledger
        INSERT INTO t_sys_escrow_ledger (
            c_orderid, c_transactionid, c_transactiontype, c_amount,
            c_fromentity, c_toentity, c_status, c_description
        )
        VALUES (
            p_OrderId, v_TransactionId, 'CREDIT', p_Amount,
            'CUSTOMER', 'ADMIN', 'COMPLETED', 'Advance payment received from customer'
        );

        -- Update Order Status
        UPDATE t_sys_orders
        SET c_orderstatus = 'Confirmed',
            c_paymentstatus = 'Advance Paid',
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Return result (same as MSSQL SELECT)
        RETURN QUERY
        SELECT
            t.c_transactionid,
            t.c_orderid,
            t.c_amount,
            t.c_paymentstatus,
            t.c_completeddate,
            ps.c_advanceamount,
            ps.c_finalamount,
            ps.c_escrowstatus
        FROM t_sys_payment_transactions t
        INNER JOIN t_sys_order_payment_summary ps 
            ON t.c_orderid = ps.c_orderid
        WHERE t.c_transactionid = v_TransactionId;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE; -- same as THROW
    END;

END;
$$;

-- ===============================================
-- 3. Process Final Payment (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_ProcessFinalPayment(
    p_OrderId BIGINT,
    p_UserId BIGINT,
    p_CateringOwnerId BIGINT,
    p_Amount DECIMAL(18,2),
    p_PaymentMethod VARCHAR(50),
    p_PaymentGateway VARCHAR(50) DEFAULT NULL,
    p_GatewayTransactionId VARCHAR(200) DEFAULT NULL,
    p_GatewayPaymentId VARCHAR(200) DEFAULT NULL,
    p_GatewaySignature VARCHAR(500) DEFAULT NULL
)
RETURNS TABLE (
    c_transactionid BIGINT,
    c_orderid BIGINT,
    c_amount DECIMAL(18,2),
    c_paymentstatus VARCHAR,
    c_completeddate TIMESTAMP,
    c_paymentcompleted INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_TransactionId BIGINT;
BEGIN

    BEGIN
        -- Validate advance payment is done
        IF NOT EXISTS (
            SELECT 1 FROM t_sys_order_payment_summary
            WHERE c_orderid = p_OrderId AND c_advancepaid = 1
        ) THEN
            RAISE EXCEPTION 'Advance payment not completed';
        END IF;

        -- Insert Payment Transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod, c_paymentgateway,
            c_gateway_transactionid, c_gateway_paymentid, c_gateway_signature,
            c_paymentstatus, c_completeddate
        )
        VALUES (
            p_OrderId, p_UserId, p_CateringOwnerId,
            'FINAL', p_Amount, p_PaymentMethod, p_PaymentGateway,
            p_GatewayTransactionId, p_GatewayPaymentId, p_GatewaySignature,
            'SUCCESS', NOW()
        )
        RETURNING c_transactionid INTO v_TransactionId;

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_finalpaid = 1,
            c_finalpaiddate = NOW(),
            c_paymentcompleted = 1,
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Add to Escrow if not CASH
        IF p_PaymentMethod <> 'CASH' THEN

            UPDATE t_sys_order_payment_summary
            SET c_escrowamount = c_escrowamount + p_Amount
            WHERE c_orderid = p_OrderId;

            INSERT INTO t_sys_escrow_ledger (
                c_orderid, c_transactionid, c_transactiontype, c_amount,
                c_fromentity, c_toentity, c_status, c_description
            )
            VALUES (
                p_OrderId, v_TransactionId, 'CREDIT', p_Amount,
                'CUSTOMER', 'ADMIN', 'COMPLETED', 'Final payment received from customer'
            );

        END IF;

        -- Update Order Status
        UPDATE t_sys_orders
        SET c_orderstatus = 'Completed',
            c_paymentstatus = 'Fully Paid',
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Return result (same output)
        RETURN QUERY
        SELECT
            t.c_transactionid,
            t.c_orderid,
            t.c_amount,
            t.c_paymentstatus,
            t.c_completeddate,
            ps.c_paymentcompleted
        FROM t_sys_payment_transactions t
        INNER JOIN t_sys_order_payment_summary ps 
            ON t.c_orderid = ps.c_orderid
        WHERE t.c_transactionid = v_TransactionId;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;

END;
$$;

-- ===============================================
-- 4. Release Advance to Vendor (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_ReleaseAdvanceToVendor(
    p_OrderId BIGINT,
    p_ApprovedBy BIGINT
)
RETURNS TABLE (
    c_orderid BIGINT,
    c_vendoradvanceamount DECIMAL(18,2),
    c_vendoradvancereleaseddate TIMESTAMP,
    c_vendorpayoutstatus VARCHAR,
    c_escrowstatus VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_AdvanceAmount DECIMAL(18,2);
BEGIN

    BEGIN
        -- Get advance amount
        SELECT c_advanceamount INTO v_AdvanceAmount
        FROM t_sys_order_payment_summary
        WHERE c_orderid = p_OrderId
          AND c_advancepaid = 1
          AND c_vendoradvancereleased = 0;

        -- Validation (same behavior)
        IF v_AdvanceAmount IS NULL THEN
            RAISE EXCEPTION 'Advance not paid or already released';
        END IF;

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_vendoradvancereleased = 1,
            c_vendoradvanceamount = v_AdvanceAmount,
            c_vendoradvancereleaseddate = NOW(),
            c_vendorpayoutstatus = 'ADVANCE_RELEASED',
            c_escrowstatus = 'RELEASED_TO_VENDOR',
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Add to Escrow Ledger
        INSERT INTO t_sys_escrow_ledger (
            c_orderid, c_transactiontype, c_amount,
            c_fromentity, c_toentity, c_status, c_requiresapproval,
            c_approvedby, c_approveddate, c_description
        )
        VALUES (
            p_OrderId, 'RELEASE', v_AdvanceAmount,
            'ADMIN', 'VENDOR', 'COMPLETED', 1,
            p_ApprovedBy, NOW(), 'Advance amount released to vendor'
        );

        -- Return result (same as MSSQL SELECT)
        RETURN QUERY
        SELECT
            c_orderid,
            c_vendoradvanceamount,
            c_vendoradvancereleaseddate,
            c_vendorpayoutstatus,
            c_escrowstatus
        FROM t_sys_order_payment_summary
        WHERE c_orderid = p_OrderId;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;

END;
$$;

-- ===============================================
-- 5. Process Final Vendor Payout (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_ProcessFinalVendorPayout(
    p_OrderId BIGINT,
    p_ProcessedBy BIGINT
)
RETURNS TABLE (
    c_orderid BIGINT,
    c_commissionamount DECIMAL(18,2),
    c_vendorfinalpayout DECIMAL(18,2),
    c_vendorpayoutstatus VARCHAR,
    c_vendorfinalpayoutdate TIMESTAMP
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_FinalAmount DECIMAL(18,2);
    v_CommissionAmount DECIMAL(18,2);
    v_VendorPayout DECIMAL(18,2);
    v_CashPayment DECIMAL(18,2) := 0;
BEGIN

    BEGIN
        -- Get payment details
        SELECT
            c_finalamount,
            c_commissionamount
        INTO
            v_FinalAmount,
            v_CommissionAmount
        FROM t_sys_order_payment_summary
        WHERE c_orderid = p_OrderId
          AND c_finalpaid = 1;

        -- Validate
        IF v_FinalAmount IS NULL THEN
            RAISE EXCEPTION 'Final payment not completed';
        END IF;

        -- Check if final payment was CASH
        IF EXISTS (
            SELECT 1 FROM t_sys_payment_transactions
            WHERE c_orderid = p_OrderId
              AND c_transactiontype = 'FINAL'
              AND c_paymentmethod = 'CASH'
        ) THEN
            v_CashPayment := v_FinalAmount;
            v_VendorPayout := 0;
        ELSE
            -- Calculate vendor payout
            v_VendorPayout := v_FinalAmount - v_CommissionAmount;
        END IF;

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_commissionpaid = 1,
            c_vendorfinalpayout = v_VendorPayout,
            c_vendorfinalpayoutdate = NOW(),
            c_vendorpayoutstatus = 'COMPLETED',
            c_modifieddate = NOW()
        WHERE c_orderid = p_OrderId;

        -- Add commission transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod,
            c_paymentstatus, c_completeddate
        )
        SELECT
            p_OrderId, c_userid, c_cateringownerid,
            'COMMISSION', v_CommissionAmount, 'INTERNAL',
            'SUCCESS', NOW()
        FROM t_sys_orders
        WHERE c_orderid = p_OrderId;

        -- If payout needed (non-cash)
        IF v_VendorPayout > 0 THEN
            INSERT INTO t_sys_escrow_ledger (
                c_orderid, c_transactiontype, c_amount,
                c_fromentity, c_toentity, c_status, c_requiresapproval,
                c_approvedby, c_approveddate, c_description
            )
            VALUES (
                p_OrderId, 'RELEASE', v_VendorPayout,
                'ADMIN', 'VENDOR', 'COMPLETED', 1,
                p_ProcessedBy, NOW(), 'Final payout after commission deduction'
            );
        END IF;

        -- Return result (same output)
        RETURN QUERY
        SELECT
            c_orderid,
            c_commissionamount,
            c_vendorfinalpayout,
            c_vendorpayoutstatus,
            c_vendorfinalpayoutdate
        FROM t_sys_order_payment_summary
        WHERE c_orderid = p_OrderId;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;

END;
$$;

-- ===============================================
-- 6. Get Available EMI Plans (PostgreSQL)
-- ===============================================

CREATE OR REPLACE FUNCTION sp_GetEMIPlans(
    p_OrderAmount DECIMAL(18,2)
)
RETURNS TABLE (
    c_emiplanid BIGINT,
    c_bankname VARCHAR,
    c_bankcode VARCHAR,
    c_tenure INT,
    c_interestrate DECIMAL(5,2),
    c_processingfee DECIMAL(18,2),
    c_minordervalue DECIMAL(18,2),
    c_maxordervalue DECIMAL(18,2),
    MonthlyEMI DECIMAL(18,2),
    c_termsandconditions TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_emiplanid,
        c_bankname,
        c_bankcode,
        c_tenure,
        c_interestrate,
        c_processingfee,
        c_minordervalue,
        c_maxordervalue,

        -- EMI Calculation (same formula)
        CAST(
            ((p_OrderAmount + c_processingfee) *
            (1 + (c_interestrate / 100 / 12)) *
            POWER(1 + (c_interestrate / 100 / 12), c_tenure)) /
            (POWER(1 + (c_interestrate / 100 / 12), c_tenure) - 1)
        AS DECIMAL(18,2)) AS MonthlyEMI,

        c_termsandconditions

    FROM t_sys_emi_plans
    WHERE c_isactive = TRUE
        AND p_OrderAmount >= c_minordervalue
        AND (p_OrderAmount <= c_maxordervalue OR c_maxordervalue IS NULL)
    ORDER BY c_bankname, c_tenure;

END;
$$;

-- ===============================================
-- 7. Get Payment Summary
-- ===============================================

CREATE OR REPLACE PROCEDURE sp_GetPaymentSummary(
    IN p_OrderId BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN

    -- 1. Payment Summary
    SELECT
        ps.*,
        o.c_ordernumber,
        o.c_orderstatus,
        o.c_paymentstatus,
        o.c_userid,
        o.c_cateringownerid,
        co.c_catering_name AS VendorName
    FROM t_sys_order_payment_summary ps
    INNER JOIN t_sys_orders o 
        ON ps.c_orderid = o.c_orderid
    INNER JOIN t_sys_catering_owner co 
        ON o.c_cateringownerid = co.c_ownerid
    WHERE ps.c_orderid = p_OrderId;

    -- 2. Payment Transactions
    SELECT *
    FROM t_sys_payment_transactions
    WHERE c_orderid = p_OrderId
    ORDER BY c_createddate DESC;

    -- 3. Escrow Ledger
    SELECT *
    FROM t_sys_escrow_ledger
    WHERE c_orderid = p_OrderId
    ORDER BY c_createddate DESC;

END;
$$;
