-- =============================================
-- Transaction Management Fix - CRITICAL SECURITY UPDATE
-- Description: Wraps critical operations in atomic transactions
--              to prevent data corruption and revenue leakage
-- Version: 1.0
-- Date: 2026-02-19
-- Priority: P0 - MUST DEPLOY BEFORE PRODUCTION
-- =============================================

USE [CateringDB]
GO

-- =============================================
-- 1. Process Refund with Transaction
-- CRITICAL FIX: Lines 156-191 in CancellationRepository.cs
-- BEFORE: Separate queries, no transaction = revenue leakage
-- AFTER: Atomic transaction with rollback on failure
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessRefundTransaction]
    @CancellationId BIGINT,
    @RefundTransactionId NVARCHAR(200),
    @RefundMethod NVARCHAR(50),
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    DECLARE @OrderId BIGINT;
    DECLARE @RefundAmount DECIMAL(18, 2);
    DECLARE @CateringOwnerId BIGINT;
    DECLARE @UserId BIGINT;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Verify cancellation request exists and is approved
        IF NOT EXISTS (
            SELECT 1
            FROM t_sys_cancellation_requests
            WHERE c_cancellation_id = @CancellationId
              AND c_status = 'Approved'
        )
        BEGIN
            SET @ErrorMessage = 'Cancellation request not found or not approved';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get order details
        SELECT
            @OrderId = cr.c_orderid,
            @RefundAmount = cr.c_refund_amount,
            @UserId = cr.c_userid,
            @CateringOwnerId = o.c_cateringownerid
        FROM t_sys_cancellation_requests cr
        INNER JOIN t_sys_orders o ON cr.c_orderid = o.c_orderid
        WHERE cr.c_cancellation_id = @CancellationId;

        -- 1. Update cancellation request status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Refunded',
            c_refund_initiated_date = GETDATE(),
            c_refund_completed_date = GETDATE(),
            c_refund_transaction_id = @RefundTransactionId,
            c_refund_method = @RefundMethod,
            c_modifieddate = GETDATE()
        WHERE c_cancellation_id = @CancellationId
          AND c_status = 'Approved';

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = 'Failed to update cancellation request status';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 2. Update order status to Cancelled
        UPDATE t_sys_orders
        SET c_order_status = 'Cancelled',
            c_payment_status = 'Refunded',
            c_modifieddate = GETDATE()
        WHERE c_orderid = @OrderId;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = 'Failed to update order status';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 3. Insert order status history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        VALUES (
            @OrderId,
            'Cancelled',
            'Order cancelled and refund processed. Transaction ID: ' + @RefundTransactionId,
            GETDATE()
        );

        -- 4. Update payment summary if exists
        IF EXISTS (SELECT 1 FROM t_sys_order_payment_summary WHERE c_orderid = @OrderId)
        BEGIN
            UPDATE t_sys_order_payment_summary
            SET c_refundamount = @RefundAmount,
                c_refundstatus = 'COMPLETED',
                c_refunddate = GETDATE(),
                c_modifieddate = GETDATE()
            WHERE c_orderid = @OrderId;
        END

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
            @OrderId,
            @UserId,
            @CateringOwnerId,
            'REFUND',
            @RefundAmount,
            @RefundMethod,
            'Razorpay',
            @RefundTransactionId,
            'SUCCESS',
            GETDATE(),
            GETDATE()
        );

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
            @OrderId,
            SCOPE_IDENTITY(),
            'DEBIT',
            @RefundAmount,
            'ADMIN',
            'CUSTOMER',
            'COMPLETED',
            'Refund processed for cancelled order',
            GETDATE()
        );

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;

        -- Log error for debugging
        INSERT INTO t_sys_error_log (c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date)
        VALUES (
            ERROR_MESSAGE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_SEVERITY(),
            GETDATE()
        );
    END CATCH
END
GO

-- =============================================
-- 2. Update Payment Stage with Order Status (Transaction Wrapper)
-- CRITICAL FIX: Payment verification must update both payment stage AND order status atomically
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_UpdatePaymentStageWithOrderStatus]
    @PaymentStageId BIGINT,
    @OrderId BIGINT,
    @StageType NVARCHAR(20),
    @Status NVARCHAR(20),
    @PaymentMethod NVARCHAR(50),
    @PaymentGateway NVARCHAR(50),
    @RazorpayOrderId NVARCHAR(100),
    @RazorpayPaymentId NVARCHAR(100),
    @TransactionId NVARCHAR(100),
    @UpiId NVARCHAR(100),
    @NewOrderStatus NVARCHAR(20),
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Update payment stage status
        UPDATE t_sys_order_payment_stages
        SET c_status = @Status,
            c_payment_method = @PaymentMethod,
            c_payment_gateway = @PaymentGateway,
            c_razorpay_order_id = @RazorpayOrderId,
            c_razorpay_payment_id = @RazorpayPaymentId,
            c_transaction_id = @TransactionId,
            c_upi_id = @UpiId,
            c_payment_date = GETDATE()
        WHERE c_payment_stage_id = @PaymentStageId;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = 'Failed to update payment stage';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 2. Update order status (only for PreBooking or Full payment types)
        IF @StageType IN ('PreBooking', 'Full') AND @NewOrderStatus IS NOT NULL
        BEGIN
            UPDATE t_sys_orders
            SET c_order_status = @NewOrderStatus,
                c_payment_status = CASE
                    WHEN @StageType = 'Full' THEN 'Fully Paid'
                    ELSE 'Advance Paid'
                END,
                c_modifieddate = GETDATE()
            WHERE c_orderid = @OrderId;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @ErrorMessage = 'Failed to update order status';
                ROLLBACK TRANSACTION;
                RETURN;
            END

            -- 3. Insert order status history
            INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
            VALUES (
                @OrderId,
                @NewOrderStatus,
                'Order confirmed after successful ' + @StageType + ' payment via ' + @PaymentGateway + ' (Payment ID: ' + @RazorpayPaymentId + ')',
                GETDATE()
            );
        END

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;

        -- Log critical error
        INSERT INTO t_sys_error_log (c_error_message, c_error_procedure, c_error_line, c_error_severity, c_error_date)
        VALUES (
            'CRITICAL: Payment verified but DB update failed - OrderId: ' + CAST(@OrderId AS NVARCHAR) + ', Error: ' + ERROR_MESSAGE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_SEVERITY(),
            GETDATE()
        );
    END CATCH
END
GO

-- =============================================
-- 3. Create Error Log Table (if not exists)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[t_sys_error_log]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[t_sys_error_log] (
        [c_error_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [c_error_message] NVARCHAR(MAX) NOT NULL,
        [c_error_procedure] NVARCHAR(200),
        [c_error_line] INT,
        [c_error_severity] INT,
        [c_error_state] INT,
        [c_error_number] INT,
        [c_additional_info] NVARCHAR(MAX),
        [c_error_date] DATETIME DEFAULT GETDATE(),
        [c_resolved] BIT DEFAULT 0,
        [c_resolved_date] DATETIME,
        [c_resolved_by] BIGINT
    );

    CREATE NONCLUSTERED INDEX [IX_ErrorLog_Date] ON [dbo].[t_sys_error_log]([c_error_date] DESC);
    CREATE NONCLUSTERED INDEX [IX_ErrorLog_Resolved] ON [dbo].[t_sys_error_log]([c_resolved]);

    PRINT 'Table t_sys_error_log created successfully';
END
ELSE
BEGIN
    PRINT 'Table t_sys_error_log already exists';
END
GO

-- =============================================
-- 4. Approve Cancellation with Transaction
-- CRITICAL FIX: Admin approval must be atomic
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_ApproveCancellationTransaction]
    @CancellationId BIGINT,
    @AdminId BIGINT,
    @AdminNotes NVARCHAR(500),
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Verify cancellation exists and is pending
        IF NOT EXISTS (
            SELECT 1
            FROM t_sys_cancellation_requests
            WHERE c_cancellation_id = @CancellationId
              AND c_status = 'Pending'
        )
        BEGIN
            SET @ErrorMessage = 'Cancellation request not found or already processed';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update cancellation status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Approved',
            c_admin_approved_by = @AdminId,
            c_admin_approval_date = GETDATE(),
            c_admin_notes = @AdminNotes,
            c_modifieddate = GETDATE()
        WHERE c_cancellation_id = @CancellationId
          AND c_status = 'Pending';

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = 'Failed to approve cancellation request';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update order status to show cancellation is in progress
        UPDATE t_sys_orders
        SET c_order_status = 'Cancellation_Approved',
            c_modifieddate = GETDATE()
        WHERE c_orderid = (SELECT c_orderid FROM t_sys_cancellation_requests WHERE c_cancellation_id = @CancellationId);

        -- Insert status history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        SELECT
            c_orderid,
            'Cancellation_Approved',
            'Cancellation request approved by admin. Refund will be processed.',
            GETDATE()
        FROM t_sys_cancellation_requests
        WHERE c_cancellation_id = @CancellationId;

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;
    END CATCH
END
GO

-- =============================================
-- 5. Reject Cancellation with Transaction
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_RejectCancellationTransaction]
    @CancellationId BIGINT,
    @AdminId BIGINT,
    @RejectionReason NVARCHAR(500),
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Update cancellation status
        UPDATE t_sys_cancellation_requests
        SET c_status = 'Rejected',
            c_admin_approved_by = @AdminId,
            c_admin_approval_date = GETDATE(),
            c_admin_notes = @RejectionReason,
            c_modifieddate = GETDATE()
        WHERE c_cancellation_id = @CancellationId
          AND c_status = 'Pending';

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = 'Failed to reject cancellation request';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert status history
        INSERT INTO t_sys_order_status_history (c_orderid, c_status, c_remarks, c_modifieddate)
        SELECT
            c_orderid,
            'Cancellation_Rejected',
            'Cancellation request rejected. Reason: ' + @RejectionReason,
            GETDATE()
        FROM t_sys_cancellation_requests
        WHERE c_cancellation_id = @CancellationId;

        COMMIT TRANSACTION;
        SET @Success = 1;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;
    END CATCH
END
GO

-- =============================================
-- 6. Verification Queries
-- =============================================

PRINT '================================================';
PRINT 'Transaction Management Fix - DEPLOYMENT COMPLETE';
PRINT '================================================';
PRINT 'Stored Procedures Created:';
PRINT '  ✅ sp_ProcessRefundTransaction';
PRINT '  ✅ sp_UpdatePaymentStageWithOrderStatus';
PRINT '  ✅ sp_ApproveCancellationTransaction';
PRINT '  ✅ sp_RejectCancellationTransaction';
PRINT '';
PRINT 'Tables Created:';
PRINT '  ✅ t_sys_error_log';
PRINT '';
PRINT 'CRITICAL: Update application code to use new stored procedures';
PRINT 'Files to update:';
PRINT '  - CateringDB.BAL/Base/Order/CancellationRepository.cs';
PRINT '  - CateringDB.BAL/Base/User/PaymentStageService.cs';
PRINT '================================================';
GO

-- =============================================
-- 7. Test Transaction Rollback
-- =============================================

-- Uncomment to test rollback functionality
/*
DECLARE @Success BIT, @ErrorMessage NVARCHAR(500);

-- Test with invalid cancellation ID (should rollback)
EXEC sp_ProcessRefundTransaction
    @CancellationId = 999999,
    @RefundTransactionId = 'TEST_REFUND_123',
    @RefundMethod = 'Razorpay',
    @Success = @Success OUTPUT,
    @ErrorMessage = @ErrorMessage OUTPUT;

SELECT @Success AS Success, @ErrorMessage AS ErrorMessage;

-- Verify no data was changed
SELECT * FROM t_sys_cancellation_requests WHERE c_cancellation_id = 999999;
*/
