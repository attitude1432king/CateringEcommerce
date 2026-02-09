-- ===============================================
-- Split Payment Stored Procedures
-- ===============================================

USE CateringEcommerce;
GO

-- ===============================================
-- 1. Initialize Order Payment Summary
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_InitializeOrderPayment]
    @OrderId BIGINT,
    @TotalAmount DECIMAL(18,2),
    @AdvancePercentage DECIMAL(5,2) = 30.00,
    @CommissionRate DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AdvanceAmount DECIMAL(18,2) = (@TotalAmount * @AdvancePercentage / 100);
    DECLARE @FinalAmount DECIMAL(18,2) = (@TotalAmount - @AdvanceAmount);
    DECLARE @CommissionAmount DECIMAL(18,2) = (@TotalAmount * @CommissionRate / 100);

    -- Insert or Update Payment Summary
    IF EXISTS (SELECT 1 FROM t_sys_order_payment_summary WHERE c_orderid = @OrderId)
    BEGIN
        UPDATE t_sys_order_payment_summary
        SET c_totalamount = @TotalAmount,
            c_advancepercentage = @AdvancePercentage,
            c_advanceamount = @AdvanceAmount,
            c_finalamount = @FinalAmount,
            c_commissionrate = @CommissionRate,
            c_commissionamount = @CommissionAmount,
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;
    END
    ELSE
    BEGIN
        INSERT INTO t_sys_order_payment_summary (
            c_orderid, c_totalamount, c_advancepercentage, c_advanceamount,
            c_finalamount, c_commissionrate, c_commissionamount,
            c_escrowstatus, c_vendorpayoutstatus
        )
        VALUES (
            @OrderId, @TotalAmount, @AdvancePercentage, @AdvanceAmount,
            @FinalAmount, @CommissionRate, @CommissionAmount,
            'PENDING', 'PENDING'
        );
    END

    SELECT
        c_paymentsummaryid, c_orderid, c_totalamount,
        c_advanceamount, c_finalamount, c_commissionamount
    FROM t_sys_order_payment_summary
    WHERE c_orderid = @OrderId;
END
GO

-- ===============================================
-- 2. Process Advance Payment
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessAdvancePayment]
    @OrderId BIGINT,
    @UserId BIGINT,
    @CateringOwnerId BIGINT,
    @Amount DECIMAL(18,2),
    @PaymentMethod VARCHAR(50),
    @PaymentGateway VARCHAR(50),
    @GatewayTransactionId VARCHAR(200),
    @GatewayPaymentId VARCHAR(200),
    @GatewaySignature VARCHAR(500),
    @IsEMI BIT = 0,
    @EMITenure INT = NULL,
    @EMIBank VARCHAR(100) = NULL,
    @EMIRate DECIMAL(5,2) = NULL,
    @EMIAmount DECIMAL(18,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Insert Payment Transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod, c_paymentgateway,
            c_gateway_transactionid, c_gateway_paymentid, c_gateway_signature,
            c_paymentstatus, c_is_emi, c_emi_tenure, c_emi_bank, c_emi_rate, c_emi_amount,
            c_completeddate
        )
        VALUES (
            @OrderId, @UserId, @CateringOwnerId,
            'ADVANCE', @Amount, @PaymentMethod, @PaymentGateway,
            @GatewayTransactionId, @GatewayPaymentId, @GatewaySignature,
            'SUCCESS', @IsEMI, @EMITenure, @EMIBank, @EMIRate, @EMIAmount,
            GETDATE()
        );

        DECLARE @TransactionId BIGINT = SCOPE_IDENTITY();

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_advancepaid = 1,
            c_advancepaiddate = GETDATE(),
            c_escrowstatus = 'HELD',
            c_escrowamount = @Amount,
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Add to Escrow Ledger
        INSERT INTO t_sys_escrow_ledger (
            c_orderid, c_transactionid, c_transactiontype, c_amount,
            c_fromentity, c_toentity, c_status, c_description
        )
        VALUES (
            @OrderId, @TransactionId, 'CREDIT', @Amount,
            'CUSTOMER', 'ADMIN', 'COMPLETED', 'Advance payment received from customer'
        );

        -- Update Order Status
        UPDATE t_sys_order
        SET c_orderstatus = 'Confirmed',
            c_paymentstatus = 'Advance Paid',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        COMMIT TRANSACTION;

        -- Return transaction details
        SELECT
            t.c_transactionid, t.c_orderid, t.c_amount,
            t.c_paymentstatus, t.c_completeddate,
            ps.c_advanceamount, ps.c_finalamount, ps.c_escrowstatus
        FROM t_sys_payment_transactions t
        INNER JOIN t_sys_order_payment_summary ps ON t.c_orderid = ps.c_orderid
        WHERE t.c_transactionid = @TransactionId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ===============================================
-- 3. Process Final Payment
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessFinalPayment]
    @OrderId BIGINT,
    @UserId BIGINT,
    @CateringOwnerId BIGINT,
    @Amount DECIMAL(18,2),
    @PaymentMethod VARCHAR(50),
    @PaymentGateway VARCHAR(50) = NULL,
    @GatewayTransactionId VARCHAR(200) = NULL,
    @GatewayPaymentId VARCHAR(200) = NULL,
    @GatewaySignature VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Validate advance payment is done
        IF NOT EXISTS (
            SELECT 1 FROM t_sys_order_payment_summary
            WHERE c_orderid = @OrderId AND c_advancepaid = 1
        )
        BEGIN
            THROW 50001, 'Advance payment not completed', 1;
        END

        -- Insert Payment Transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod, c_paymentgateway,
            c_gateway_transactionid, c_gateway_paymentid, c_gateway_signature,
            c_paymentstatus, c_completeddate
        )
        VALUES (
            @OrderId, @UserId, @CateringOwnerId,
            'FINAL', @Amount, @PaymentMethod, @PaymentGateway,
            @GatewayTransactionId, @GatewayPaymentId, @GatewaySignature,
            'SUCCESS', GETDATE()
        );

        DECLARE @TransactionId BIGINT = SCOPE_IDENTITY();

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_finalpaid = 1,
            c_finalpaiddate = GETDATE(),
            c_paymentcompleted = 1,
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Add to Escrow if online payment
        IF @PaymentMethod <> 'CASH'
        BEGIN
            UPDATE t_sys_order_payment_summary
            SET c_escrowamount = c_escrowamount + @Amount
            WHERE c_orderid = @OrderId;

            INSERT INTO t_sys_escrow_ledger (
                c_orderid, c_transactionid, c_transactiontype, c_amount,
                c_fromentity, c_toentity, c_status, c_description
            )
            VALUES (
                @OrderId, @TransactionId, 'CREDIT', @Amount,
                'CUSTOMER', 'ADMIN', 'COMPLETED', 'Final payment received from customer'
            );
        END

        -- Update Order Status
        UPDATE t_sys_order
        SET c_orderstatus = 'Completed',
            c_paymentstatus = 'Fully Paid',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        COMMIT TRANSACTION;

        -- Return transaction details
        SELECT
            t.c_transactionid, t.c_orderid, t.c_amount,
            t.c_paymentstatus, t.c_completeddate,
            ps.c_paymentcompleted
        FROM t_sys_payment_transactions t
        INNER JOIN t_sys_order_payment_summary ps ON t.c_orderid = ps.c_orderid
        WHERE t.c_transactionid = @TransactionId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ===============================================
-- 4. Release Advance to Vendor
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ReleaseAdvanceToVendor]
    @OrderId BIGINT,
    @ApprovedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @AdvanceAmount DECIMAL(18,2);

        -- Get advance amount
        SELECT @AdvanceAmount = c_advanceamount
        FROM t_sys_order_payment_summary
        WHERE c_orderid = @OrderId AND c_advancepaid = 1 AND c_vendoradvancereleased = 0;

        IF @AdvanceAmount IS NULL
        BEGIN
            THROW 50002, 'Advance not paid or already released', 1;
        END

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_vendoradvancereleased = 1,
            c_vendoradvanceamount = @AdvanceAmount,
            c_vendoradvancereleaseddate = GETDATE(),
            c_vendorpayoutstatus = 'ADVANCE_RELEASED',
            c_escrowstatus = 'RELEASED_TO_VENDOR',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Add to Escrow Ledger
        INSERT INTO t_sys_escrow_ledger (
            c_orderid, c_transactiontype, c_amount,
            c_fromentity, c_toentity, c_status, c_requiresapproval,
            c_approvedby, c_approveddate, c_description
        )
        VALUES (
            @OrderId, 'RELEASE', @AdvanceAmount,
            'ADMIN', 'VENDOR', 'COMPLETED', 1,
            @ApprovedBy, GETDATE(), 'Advance amount released to vendor'
        );

        COMMIT TRANSACTION;

        SELECT
            c_orderid, c_vendoradvanceamount, c_vendoradvancereleaseddate,
            c_vendorpayoutstatus, c_escrowstatus
        FROM t_sys_order_payment_summary
        WHERE c_orderid = @OrderId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ===============================================
-- 5. Process Final Vendor Payout
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessFinalVendorPayout]
    @OrderId BIGINT,
    @ProcessedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @FinalAmount DECIMAL(18,2);
        DECLARE @CommissionAmount DECIMAL(18,2);
        DECLARE @VendorPayout DECIMAL(18,2);
        DECLARE @CashPayment DECIMAL(18,2) = 0;

        -- Get payment details
        SELECT
            @FinalAmount = c_finalamount,
            @CommissionAmount = c_commissionamount
        FROM t_sys_order_payment_summary
        WHERE c_orderid = @OrderId AND c_finalpaid = 1;

        -- Check if final payment was cash (directly to vendor)
        IF EXISTS (
            SELECT 1 FROM t_sys_payment_transactions
            WHERE c_orderid = @OrderId
            AND c_transactiontype = 'FINAL'
            AND c_paymentmethod = 'CASH'
        )
        BEGIN
            SET @CashPayment = @FinalAmount;
            SET @VendorPayout = 0; -- No payout needed as customer paid directly
        END
        ELSE
        BEGIN
            -- Calculate vendor payout (final amount - commission)
            SET @VendorPayout = @FinalAmount - @CommissionAmount;
        END

        -- Update Payment Summary
        UPDATE t_sys_order_payment_summary
        SET c_commissionpaid = 1,
            c_vendorfinalpayout = @VendorPayout,
            c_vendorfinalpayoutdate = GETDATE(),
            c_vendorpayoutstatus = 'COMPLETED',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        -- Add commission transaction
        INSERT INTO t_sys_payment_transactions (
            c_orderid, c_userid, c_cateringownerid,
            c_transactiontype, c_amount, c_paymentmethod,
            c_paymentstatus, c_completeddate
        )
        SELECT
            @OrderId, c_userid, c_cateringownerid,
            'COMMISSION', @CommissionAmount, 'INTERNAL',
            'SUCCESS', GETDATE()
        FROM t_sys_order
        WHERE c_orderid = @OrderId;

        -- If vendor payout needed (online payment)
        IF @VendorPayout > 0
        BEGIN
            INSERT INTO t_sys_escrow_ledger (
                c_orderid, c_transactiontype, c_amount,
                c_fromentity, c_toentity, c_status, c_requiresapproval,
                c_approvedby, c_approveddate, c_description
            )
            VALUES (
                @OrderId, 'RELEASE', @VendorPayout,
                'ADMIN', 'VENDOR', 'COMPLETED', 1,
                @ProcessedBy, GETDATE(), 'Final payout after commission deduction'
            );
        END

        COMMIT TRANSACTION;

        SELECT
            c_orderid, c_commissionamount, c_vendorfinalpayout,
            c_vendorpayoutstatus, c_vendorfinalpayoutdate
        FROM t_sys_order_payment_summary
        WHERE c_orderid = @OrderId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ===============================================
-- 6. Get Available EMI Plans
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetEMIPlans]
    @OrderAmount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c_emiplanid, c_bankname, c_bankcode,
        c_tenure, c_interestrate, c_processingfee,
        c_minordervalue, c_maxordervalue,
        -- Calculate EMI amount
        CAST(
            ((@OrderAmount + c_processingfee) *
            (1 + (c_interestrate / 100 / 12)) *
            POWER(1 + (c_interestrate / 100 / 12), c_tenure)) /
            (POWER(1 + (c_interestrate / 100 / 12), c_tenure) - 1)
        AS DECIMAL(18,2)) AS MonthlyEMI,
        c_termsandconditions
    FROM t_sys_emi_plans
    WHERE c_isactive = 1
        AND @OrderAmount >= c_minordervalue
        AND (@OrderAmount <= c_maxordervalue OR c_maxordervalue IS NULL)
    ORDER BY c_bankname, c_tenure;
END
GO

-- ===============================================
-- 7. Get Payment Summary
-- ===============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPaymentSummary]
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Payment Summary
    SELECT
        ps.*,
        o.c_ordernumber, o.c_orderstatus, o.c_paymentstatus,
        o.c_userid, o.c_cateringownerid,
        co.c_catering_name AS VendorName
    FROM t_sys_order_payment_summary ps
    INNER JOIN t_sys_order o ON ps.c_orderid = o.c_orderid
    INNER JOIN t_sys_catering_owner co ON o.c_cateringownerid = co.c_ownerid
    WHERE ps.c_orderid = @OrderId;

    -- Payment Transactions
    SELECT *
    FROM t_sys_payment_transactions
    WHERE c_orderid = @OrderId
    ORDER BY c_createddate DESC;

    -- Escrow Ledger
    SELECT *
    FROM t_sys_escrow_ledger
    WHERE c_orderid = @OrderId
    ORDER BY c_createddate DESC;
END
GO

PRINT 'Split Payment Stored Procedures created successfully!';
