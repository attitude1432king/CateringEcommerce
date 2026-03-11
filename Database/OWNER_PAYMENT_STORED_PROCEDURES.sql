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

USE CateringDB;
GO

-- =============================================
-- SP: Create Owner Payment for Order
-- =============================================

CREATE OR ALTER PROCEDURE sp_CreateOwnerPayment
    @p_order_id BIGINT,
    @p_owner_id BIGINT,
    @p_settlement_amount DECIMAL(18,2),
    @p_platform_service_fee DECIMAL(18,2),
    @p_owner_payment_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_net_settlement DECIMAL(18,2);
    SET @v_net_settlement = @p_settlement_amount - @p_platform_service_fee;

    INSERT INTO t_owner_payment (
        c_owner_id,
        c_order_id,
        c_settlement_amount,
        c_platform_service_fee,
        c_net_settlement_amount,
        c_status,
        c_createddate,
        c_modifieddate
    )
    VALUES (
        @p_owner_id,
        @p_order_id,
        @p_settlement_amount,
        @p_platform_service_fee,
        @v_net_settlement,
        'PENDING',
        GETDATE(),
        GETDATE()
    );

    SET @p_owner_payment_id = SCOPE_IDENTITY();

    SELECT @p_owner_payment_id AS OwnerPaymentId;
END
GO

-- =============================================
-- SP: Escrow Owner Payment (Admin holds funds)
-- =============================================

CREATE OR ALTER PROCEDURE sp_EscrowOwnerPayment
    @p_owner_payment_id BIGINT,
    @p_transaction_reference VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_owner_payment
    SET c_status = 'ESCROWED',
        c_escrowed_at = GETDATE(),
        c_transaction_reference = @p_transaction_reference,
        c_modifieddate = GETDATE()
    WHERE c_owner_payment_id = @p_owner_payment_id
      AND c_status = 'PENDING';

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- SP: Release Settlement to Catering Partner
-- =============================================

CREATE OR ALTER PROCEDURE sp_ReleaseOwnerSettlement
    @p_owner_payment_id BIGINT,
    @p_payment_method VARCHAR(50),
    @p_released_by BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_owner_id BIGINT;
    DECLARE @v_net_settlement DECIMAL(18,2);

    -- Get owner details
    SELECT @v_owner_id = c_owner_id,
           @v_net_settlement = c_net_settlement_amount
    FROM t_owner_payment
    WHERE c_owner_payment_id = @p_owner_payment_id
      AND c_status = 'ESCROWED';

    IF @v_owner_id IS NULL
    BEGIN
        -- Payment not found or not in ESCROWED status
        SELECT 0 AS Success, 'Payment not found or not escrowed' AS Message;
        RETURN;
    END

    -- Update payment status
    UPDATE t_owner_payment
    SET c_status = 'RELEASED',
        c_released_at = GETDATE(),
        c_payment_method = @p_payment_method,
        c_modifieddate = GETDATE()
    WHERE c_owner_payment_id = @p_owner_payment_id;

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
        @v_owner_id,
        @v_net_settlement,
        GETDATE(),
        1,
        GETDATE(),
        @p_payment_method,
        'RELEASED'
    );

    SELECT 1 AS Success, 'Settlement released to catering partner successfully' AS Message;
END
GO

-- =============================================
-- SP: Calculate Platform Service Fee
-- =============================================

CREATE OR ALTER PROCEDURE sp_CalculatePlatformServiceFee
    @p_gross_amount DECIMAL(18,2),
    @p_owner_id BIGINT,
    @p_service_fee DECIMAL(18,2) OUTPUT,
    @p_net_settlement DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_fee_percentage DECIMAL(5,2);

    -- Get platform fee configuration
    SELECT TOP 1 @v_fee_percentage = c_fee_value
    FROM t_platform_fee_config
    WHERE c_fee_type = 'PERCENTAGE'
      AND c_is_active = 1;

    -- Default to 10% if not configured
    IF @v_fee_percentage IS NULL
        SET @v_fee_percentage = 10.00;

    -- Calculate fee
    SET @p_service_fee = @p_gross_amount * (@v_fee_percentage / 100);
    SET @p_net_settlement = @p_gross_amount - @p_service_fee;

    SELECT @p_service_fee AS PlatformServiceFee,
           @p_net_settlement AS NetSettlementAmount;
END
GO

-- =============================================
-- SP: Create Partner Approval Request
-- =============================================

CREATE OR ALTER PROCEDURE sp_CreatePartnerApprovalRequest
    @p_owner_id BIGINT,
    @p_order_id BIGINT,
    @p_request_type VARCHAR(50),
    @p_description NVARCHAR(1000),
    @p_request_data NVARCHAR(MAX),
    @p_requested_by_user_id BIGINT,
    @p_response_time_hours INT = 24,
    @p_approval_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_deadline DATETIME2;
    SET @v_deadline = DATEADD(HOUR, @p_response_time_hours, GETDATE());

    INSERT INTO t_partner_approval_request (
        c_owner_id,
        c_order_id,
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
        @p_owner_id,
        @p_order_id,
        @p_request_type,
        @p_description,
        @p_request_data,
        @p_requested_by_user_id,
        GETDATE(),
        @v_deadline,
        @p_response_time_hours,
        'PENDING',
        GETDATE(),
        GETDATE()
    );

    SET @p_approval_id = SCOPE_IDENTITY();

    SELECT @p_approval_id AS ApprovalId, @v_deadline AS Deadline;
END
GO

-- =============================================
-- SP: Approve Partner Request
-- =============================================

CREATE OR ALTER PROCEDURE sp_ApprovePartnerRequest
    @p_approval_id BIGINT,
    @p_approved_by_owner_id BIGINT,
    @p_partner_notes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_partner_approval_request
    SET c_status = 'APPROVED',
        c_approved_at = GETDATE(),
        c_approved_by_owner_id = @p_approved_by_owner_id,
        c_partner_notes = @p_partner_notes,
        c_modifieddate = GETDATE()
    WHERE c_approval_id = @p_approval_id
      AND c_status = 'PENDING'
      AND c_deadline > GETDATE();

    IF @@ROWCOUNT > 0
        SELECT 1 AS Success, 'Request approved by catering partner' AS Message;
    ELSE
        SELECT 0 AS Success, 'Request not found, already processed, or expired' AS Message;
END
GO

-- =============================================
-- SP: Reject Partner Request
-- =============================================

CREATE OR ALTER PROCEDURE sp_RejectPartnerRequest
    @p_approval_id BIGINT,
    @p_owner_id BIGINT,
    @p_rejection_reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_partner_approval_request
    SET c_status = 'REJECTED',
        c_rejected_at = GETDATE(),
        c_rejection_reason = @p_rejection_reason,
        c_modifieddate = GETDATE()
    WHERE c_approval_id = @p_approval_id
      AND c_owner_id = @p_owner_id
      AND c_status = 'PENDING';

    IF @@ROWCOUNT > 0
        SELECT 1 AS Success, 'Request rejected by catering partner' AS Message;
    ELSE
        SELECT 0 AS Success, 'Request not found or already processed' AS Message;
END
GO

-- =============================================
-- SP: Mark Expired Approval Requests
-- =============================================

CREATE OR ALTER PROCEDURE sp_MarkExpiredPartnerApprovals
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_partner_approval_request
    SET c_status = 'EXPIRED',
        c_modifieddate = GETDATE()
    WHERE c_status = 'PENDING'
      AND c_deadline < GETDATE();

    SELECT @@ROWCOUNT AS ExpiredCount;
END
GO

-- =============================================
-- SP: Get Pending Approvals for Owner
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetPendingPartnerApprovals
    @p_owner_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pa.c_approval_id AS ApprovalId,
        pa.c_order_id AS OrderId,
        o.c_order_number AS OrderNumber,
        pa.c_request_type AS RequestType,
        pa.c_description AS Description,
        pa.c_request_data AS RequestData,
        pa.c_requested_at AS RequestedAt,
        pa.c_deadline AS Deadline,
        pa.c_response_time_hours AS ResponseTimeHours,
        pa.c_status AS Status,
        u.c_first_name + ' ' + u.c_last_name AS RequestedByCustomer,
        DATEDIFF(MINUTE, GETDATE(), pa.c_deadline) AS MinutesRemaining
    FROM t_partner_approval_request pa
    INNER JOIN t_order o ON pa.c_order_id = o.c_order_id
    INNER JOIN t_sys_user u ON pa.c_requested_by_user_id = u.c_userid
    WHERE pa.c_owner_id = @p_owner_id
      AND pa.c_status = 'PENDING'
      AND pa.c_deadline > GETDATE()
    ORDER BY pa.c_deadline ASC;
END
GO

-- =============================================
-- SP: Get Owner Settlement History
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetOwnerSettlementHistory
    @p_owner_id BIGINT,
    @p_status VARCHAR(20) = NULL,
    @p_from_date DATETIME2 = NULL,
    @p_to_date DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        op.c_owner_payment_id AS OwnerPaymentId,
        op.c_order_id AS OrderId,
        o.c_order_number AS OrderNumber,
        op.c_settlement_amount AS SettlementAmount,
        op.c_platform_service_fee AS PlatformServiceFee,
        op.c_net_settlement_amount AS NetSettlementAmount,
        op.c_status AS Status,
        op.c_payment_method AS PaymentMethod,
        op.c_transaction_reference AS TransactionReference,
        op.c_escrowed_at AS EscrowedAt,
        op.c_released_at AS ReleasedAt,
        op.c_createddate AS CreatedAt
    FROM t_owner_payment op
    INNER JOIN t_order o ON op.c_order_id = o.c_order_id
    WHERE op.c_owner_id = @p_owner_id
      AND (@p_status IS NULL OR op.c_status = @p_status)
      AND (@p_from_date IS NULL OR op.c_createddate >= @p_from_date)
      AND (@p_to_date IS NULL OR op.c_createddate <= @p_to_date)
    ORDER BY op.c_createddate DESC;
END
GO

-- =============================================
-- SP: Get Partner Earnings Summary
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetPartnerEarningsSummary
    @p_owner_id BIGINT,
    @p_period_start DATETIME2,
    @p_period_end DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalOrders,
        SUM(c_settlement_amount) AS TotalGrossEarnings,
        SUM(c_platform_service_fee) AS TotalPlatformFees,
        SUM(c_net_settlement_amount) AS TotalNetEarnings,
        SUM(CASE WHEN c_status = 'RELEASED' THEN c_net_settlement_amount ELSE 0 END) AS ReleasedAmount,
        SUM(CASE WHEN c_status = 'ESCROWED' THEN c_net_settlement_amount ELSE 0 END) AS EscrowedAmount,
        SUM(CASE WHEN c_status = 'PENDING' THEN c_net_settlement_amount ELSE 0 END) AS PendingAmount
    FROM t_owner_payment
    WHERE c_owner_id = @p_owner_id
      AND c_createddate BETWEEN @p_period_start AND @p_period_end;
END
GO

PRINT 'Owner Payment Stored Procedures Created Successfully';
PRINT '';
PRINT 'Procedures Created:';
PRINT '  - sp_CreateOwnerPayment';
PRINT '  - sp_EscrowOwnerPayment';
PRINT '  - sp_ReleaseOwnerSettlement';
PRINT '  - sp_CalculatePlatformServiceFee';
PRINT '  - sp_CreatePartnerApprovalRequest';
PRINT '  - sp_ApprovePartnerRequest';
PRINT '  - sp_RejectPartnerRequest';
PRINT '  - sp_MarkExpiredPartnerApprovals';
PRINT '  - sp_GetPendingPartnerApprovals';
PRINT '  - sp_GetOwnerSettlementHistory';
PRINT '  - sp_GetPartnerEarningsSummary';
PRINT '';
PRINT '✅ All procedures use OWNER/PARTNER terminology (NO VENDOR)';
