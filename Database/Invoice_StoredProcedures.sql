-- =============================================
-- Invoice System Stored Procedures
-- Version: 1.0.0
-- Date: 2026-02-20
-- Description: Complete set of stored procedures for invoice operations
-- =============================================

USE CateringDB;
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'Creating Invoice Stored Procedures...';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- =============================================
-- SP 1: Generate Invoice Number
-- =============================================
IF OBJECT_ID('sp_GenerateInvoiceNumber', 'P') IS NOT NULL
    DROP PROCEDURE sp_GenerateInvoiceNumber;
GO

CREATE PROCEDURE sp_GenerateInvoiceNumber
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InvoiceNumber VARCHAR(50);
    DECLARE @Prefix VARCHAR(10);
    DECLARE @DatePart VARCHAR(8);
    DECLARE @Sequence INT;

    -- Get prefix from settings
    SELECT @Prefix = c_value FROM t_sys_settings WHERE c_key = 'INVOICE.PREFIX';
    IF @Prefix IS NULL SET @Prefix = 'INV';

    -- Get date part (YYYYMMDD)
    SET @DatePart = CONVERT(VARCHAR(8), GETDATE(), 112);

    -- Get next sequence number for today
    SELECT @Sequence = ISNULL(MAX(CAST(RIGHT(c_invoice_number, 5) AS INT)), 0) + 1
    FROM t_sys_invoice
    WHERE c_invoice_number LIKE @Prefix + '-' + @DatePart + '%';

    -- Generate invoice number
    SET @InvoiceNumber = @Prefix + '-' + @DatePart + '-' + RIGHT('00000' + CAST(@Sequence AS VARCHAR), 5);

    SELECT @InvoiceNumber AS InvoiceNumber;
END
GO

PRINT '✓ sp_GenerateInvoiceNumber created';
GO

-- =============================================
-- SP 2: Generate Invoice (Auto-generation)
-- =============================================
IF OBJECT_ID('sp_GenerateInvoice', 'P') IS NOT NULL
    DROP PROCEDURE sp_GenerateInvoice;
GO

CREATE PROCEDURE sp_GenerateInvoice
    @OrderId BIGINT,
    @InvoiceType INT, -- 1=BOOKING, 2=PRE_EVENT, 3=FINAL
    @TriggeredBy BIGINT = NULL,
    @TriggeredByType VARCHAR(20) = 'SYSTEM',
    @ExtraGuestCount INT = 0,
    @ExtraGuestCharges DECIMAL(18,2) = 0,
    @AddonCharges DECIMAL(18,2) = 0,
    @OvertimeCharges DECIMAL(18,2) = 0,
    @OtherCharges DECIMAL(18,2) = 0,
    @InvoiceId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @InvoiceNumber VARCHAR(50);
        DECLARE @IsProforma BIT = 0;
        DECLARE @StageType VARCHAR(20);
        DECLARE @Percentage DECIMAL(5,2);
        DECLARE @DueDays INT;
        DECLARE @DueDate DATETIME;
        DECLARE @Subtotal DECIMAL(18,2);
        DECLARE @CgstRate DECIMAL(5,2);
        DECLARE @SgstRate DECIMAL(5,2);
        DECLARE @CgstAmount DECIMAL(18,2);
        DECLARE @SgstAmount DECIMAL(18,2);
        DECLARE @TotalTax DECIMAL(18,2);
        DECLARE @TotalAmount DECIMAL(18,2);
        DECLARE @UserId BIGINT;
        DECLARE @OwnerId BIGINT;
        DECLARE @OrderTotal DECIMAL(18,2);
        DECLARE @CompanyGstin VARCHAR(15);
        DECLARE @PlaceOfSupply VARCHAR(100);

        -- Get order details
        SELECT
            @UserId = c_userid,
            @OwnerId = c_ownerid,
            @OrderTotal = c_total_amount
        FROM t_sys_orders
        WHERE c_orderid = @OrderId;

        IF @UserId IS NULL
        BEGIN
            RAISERROR('Order not found', 16, 1);
            RETURN;
        END

        -- Check if invoice already exists
        IF EXISTS (SELECT 1 FROM t_sys_invoice WHERE c_order_id = @OrderId AND c_invoice_type = @InvoiceType AND c_is_deleted = 0)
        BEGIN
            -- Return existing invoice
            SELECT @InvoiceId = c_invoice_id FROM t_sys_invoice
            WHERE c_order_id = @OrderId AND c_invoice_type = @InvoiceType AND c_is_deleted = 0;

            COMMIT TRANSACTION;
            RETURN;
        END

        -- Get settings
        SELECT @CgstRate = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'GST.CGST_RATE';
        SELECT @SgstRate = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'GST.SGST_RATE';
        SELECT @CompanyGstin = c_value FROM t_sys_settings WHERE c_key = 'GST.COMPANY_GSTIN';
        SELECT @PlaceOfSupply = c_value FROM t_sys_settings WHERE c_key = 'GST.PLACE_OF_SUPPLY';

        IF @CgstRate IS NULL SET @CgstRate = 9.00;
        IF @SgstRate IS NULL SET @SgstRate = 9.00;

        -- Determine invoice parameters based on type
        IF @InvoiceType = 1 -- BOOKING
        BEGIN
            SET @IsProforma = 1;
            SET @StageType = 'BOOKING';
            SELECT @Percentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_PERCENTAGE';
            SELECT @DueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_DUE_DAYS';
            IF @Percentage IS NULL SET @Percentage = 40.00;
            IF @DueDays IS NULL SET @DueDays = 7;
        END
        ELSE IF @InvoiceType = 2 -- PRE_EVENT
        BEGIN
            SET @IsProforma = 0;
            SET @StageType = 'PRE_EVENT';
            SELECT @Percentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_PERCENTAGE';
            SELECT @DueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_DUE_DAYS';
            IF @Percentage IS NULL SET @Percentage = 35.00;
            IF @DueDays IS NULL SET @DueDays = 3;
        END
        ELSE IF @InvoiceType = 3 -- FINAL
        BEGIN
            SET @IsProforma = 0;
            SET @StageType = 'FINAL';
            SELECT @Percentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_PERCENTAGE';
            SELECT @DueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_DUE_DAYS';
            IF @Percentage IS NULL SET @Percentage = 25.00;
            IF @DueDays IS NULL SET @DueDays = 7;
        END

        -- Calculate amounts
        SET @Subtotal = (@OrderTotal * @Percentage / 100);

        -- Add extra charges for FINAL invoice
        IF @InvoiceType = 3
        BEGIN
            SET @Subtotal = @Subtotal + @ExtraGuestCharges + @AddonCharges + @OvertimeCharges + @OtherCharges;
        END

        -- Calculate GST
        SET @CgstAmount = @Subtotal * @CgstRate / 100;
        SET @SgstAmount = @Subtotal * @SgstRate / 100;
        SET @TotalTax = @CgstAmount + @SgstAmount;
        SET @TotalAmount = @Subtotal + @TotalTax;

        -- Calculate due date
        SET @DueDate = DATEADD(DAY, @DueDays, GETDATE());

        -- Generate invoice number
        EXEC sp_GenerateInvoiceNumber @InvoiceNumber OUTPUT;

        -- Insert invoice
        INSERT INTO t_sys_invoice (
            c_order_id, c_userid, c_ownerid, c_invoice_type, c_is_proforma,
            c_invoice_number, c_invoice_date, c_due_date,
            c_subtotal, c_cgst_percent, c_sgst_percent, c_cgst_amount, c_sgst_amount,
            c_total_tax_amount, c_discount_amount, c_total_amount, c_amount_paid, c_balance_due,
            c_payment_stage_type, c_payment_percentage, c_status,
            c_company_gstin, c_place_of_supply, c_sac_code,
            c_createdby, c_createddate
        )
        VALUES (
            @OrderId, @UserId, @OwnerId, @InvoiceType, @IsProforma,
            @InvoiceNumber, GETDATE(), @DueDate,
            @Subtotal, @CgstRate, @SgstRate, @CgstAmount, @SgstAmount,
            @TotalTax, 0, @TotalAmount, 0, @TotalAmount,
            @StageType, @Percentage, 'UNPAID',
            @CompanyGstin, @PlaceOfSupply, '996331',
            @TriggeredBy, GETDATE()
        );

        SET @InvoiceId = SCOPE_IDENTITY();

        -- Create line items (simplified - will be enhanced with actual order items)
        DECLARE @ItemDescription VARCHAR(500);
        SET @ItemDescription = CASE
            WHEN @InvoiceType = 1 THEN 'Booking Advance Payment (40%)'
            WHEN @InvoiceType = 2 THEN 'Pre-Event Payment (35%)'
            WHEN @InvoiceType = 3 THEN 'Final Settlement (25%)' +
                CASE WHEN @ExtraGuestCharges > 0 THEN ' + Extra Guest Charges' ELSE '' END +
                CASE WHEN @AddonCharges > 0 THEN ' + Add-on Items' ELSE '' END +
                CASE WHEN @OvertimeCharges > 0 THEN ' + Overtime Charges' ELSE '' END
        END;

        INSERT INTO t_sys_invoice_line_items (
            c_invoice_id, c_item_type, c_description, c_quantity, c_unit_price, c_subtotal,
            c_tax_percent, c_cgst_percent, c_sgst_percent, c_tax_amount, c_cgst_amount, c_sgst_amount,
            c_discount_amount, c_total, c_sequence
        )
        VALUES (
            @InvoiceId, 'OTHER', @ItemDescription, 1, @Subtotal, @Subtotal,
            (@CgstRate + @SgstRate), @CgstRate, @SgstRate, @TotalTax, @CgstAmount, @SgstAmount,
            0, @TotalAmount, 1
        );

        -- Update payment schedule
        UPDATE t_sys_payment_schedule
        SET c_invoice_id = @InvoiceId,
            c_modifieddate = GETDATE()
        WHERE c_order_id = @OrderId AND c_stage_type = @StageType;

        -- Log audit
        INSERT INTO t_sys_invoice_audit_log (
            c_invoice_id, c_order_id, c_action, c_performed_by, c_performed_by_type,
            c_new_status, c_remarks, c_timestamp
        )
        VALUES (
            @InvoiceId, @OrderId, 'GENERATED', @TriggeredBy, @TriggeredByType,
            'UNPAID', 'Invoice auto-generated', GETDATE()
        );

        COMMIT TRANSACTION;

        SELECT @InvoiceId AS InvoiceId, @InvoiceNumber AS InvoiceNumber;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT '✓ sp_GenerateInvoice created';
GO

-- =============================================
-- SP 3: Get Invoice By ID
-- =============================================
IF OBJECT_ID('sp_GetInvoiceById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetInvoiceById;
GO

CREATE PROCEDURE sp_GetInvoiceById
    @InvoiceId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Main invoice details
    SELECT
        i.c_invoice_id AS InvoiceId,
        i.c_order_id AS OrderId,
        i.c_event_id AS EventId,
        i.c_userid AS UserId,
        i.c_ownerid AS CateringOwnerId,
        i.c_invoice_type AS InvoiceType,
        i.c_is_proforma AS IsProforma,
        i.c_invoice_number AS InvoiceNumber,
        i.c_invoice_date AS InvoiceDate,
        i.c_due_date AS DueDate,
        i.c_subtotal AS Subtotal,
        i.c_cgst_percent AS CgstPercent,
        i.c_sgst_percent AS SgstPercent,
        i.c_cgst_amount AS CgstAmount,
        i.c_sgst_amount AS SgstAmount,
        i.c_total_tax_amount AS TotalTaxAmount,
        i.c_discount_amount AS DiscountAmount,
        i.c_total_amount AS TotalAmount,
        i.c_amount_paid AS AmountPaid,
        i.c_balance_due AS BalanceDue,
        i.c_payment_stage_type AS PaymentStageType,
        i.c_payment_percentage AS PaymentPercentage,
        i.c_status AS Status,
        i.c_razorpay_order_id AS RazorpayOrderId,
        i.c_razorpay_payment_id AS RazorpayPaymentId,
        i.c_transaction_id AS TransactionId,
        i.c_payment_method AS PaymentMethod,
        i.c_payment_date AS PaymentDate,
        i.c_company_gstin AS CompanyGstin,
        i.c_customer_gstin AS CustomerGstin,
        i.c_place_of_supply AS PlaceOfSupply,
        i.c_sac_code AS SacCode,
        i.c_notes AS Notes,
        i.c_terms_and_conditions AS TermsAndConditions,
        i.c_internal_remarks AS InternalRemarks,
        i.c_pdf_path AS PdfPath,
        i.c_pdf_generated_date AS PdfGeneratedDate,
        i.c_createdby AS CreatedBy,
        i.c_createddate AS CreatedDate,
        i.c_modifiedby AS ModifiedBy,
        i.c_modifieddate AS ModifiedDate,
        i.c_version AS Version,
        i.c_parent_invoice_id AS ParentInvoiceId,
        -- Order details
        o.c_order_number AS OrderNumber,
        o.c_event_date AS EventDate,
        o.c_event_time AS EventTime,
        o.c_event_type AS EventType,
        o.c_event_location AS EventLocation,
        o.c_guest_count AS GuestCount,
        o.c_original_guest_count AS OriginalGuestCount,
        o.c_final_guest_count AS FinalGuestCount,
        o.c_guest_locked AS GuestCountLocked,
        o.c_menu_locked AS MenuLocked,
        -- User details
        u.c_fullname AS CustomerName,
        u.c_phone AS CustomerPhone,
        u.c_email AS CustomerEmail,
        -- Owner details
        co.c_business_name AS PartnerName,
        co.c_contact_phone AS PartnerPhone,
        co.c_contact_email AS PartnerEmail
    FROM t_sys_invoice i
    INNER JOIN t_sys_orders o ON i.c_order_id = o.c_orderid
    INNER JOIN t_sys_user u ON i.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON i.c_ownerid = co.c_ownerid
    WHERE i.c_invoice_id = @InvoiceId AND i.c_is_deleted = 0;

    -- Line items
    SELECT
        c_line_item_id AS LineItemId,
        c_invoice_id AS InvoiceId,
        c_item_type AS ItemType,
        c_item_id AS ItemId,
        c_description AS Description,
        c_hsn_sac_code AS HsnSacCode,
        c_quantity AS Quantity,
        c_unit_of_measure AS UnitOfMeasure,
        c_unit_price AS UnitPrice,
        c_subtotal AS Subtotal,
        c_tax_percent AS TaxPercent,
        c_cgst_percent AS CgstPercent,
        c_sgst_percent AS SgstPercent,
        c_tax_amount AS TaxAmount,
        c_cgst_amount AS CgstAmount,
        c_sgst_amount AS SgstAmount,
        c_discount_percent AS DiscountPercent,
        c_discount_amount AS DiscountAmount,
        c_total AS Total,
        c_sequence AS Sequence,
        c_createddate AS CreatedDate
    FROM t_sys_invoice_line_items
    WHERE c_invoice_id = @InvoiceId
    ORDER BY c_sequence;
END
GO

PRINT '✓ sp_GetInvoiceById created';
GO

-- =============================================
-- SP 4: Get Invoices By Order ID
-- =============================================
IF OBJECT_ID('sp_GetInvoicesByOrderId', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetInvoicesByOrderId;
GO

CREATE PROCEDURE sp_GetInvoicesByOrderId
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c_invoice_id AS InvoiceId,
        c_invoice_type AS InvoiceType,
        c_invoice_number AS InvoiceNumber,
        c_invoice_date AS InvoiceDate,
        c_due_date AS DueDate,
        c_total_amount AS TotalAmount,
        c_amount_paid AS AmountPaid,
        c_balance_due AS BalanceDue,
        c_status AS Status,
        c_payment_stage_type AS PaymentStageType,
        c_payment_percentage AS PaymentPercentage,
        c_is_proforma AS IsProforma,
        c_pdf_path AS PdfPath,
        c_createddate AS CreatedDate
    FROM t_sys_invoice
    WHERE c_order_id = @OrderId AND c_is_deleted = 0
    ORDER BY c_invoice_type;
END
GO

PRINT '✓ sp_GetInvoicesByOrderId created';
GO

-- =============================================
-- SP 5: Update Invoice Status
-- =============================================
IF OBJECT_ID('sp_UpdateInvoiceStatus', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateInvoiceStatus;
GO

CREATE PROCEDURE sp_UpdateInvoiceStatus
    @InvoiceId BIGINT,
    @NewStatus VARCHAR(20),
    @Remarks NVARCHAR(1000) = NULL,
    @UpdatedBy BIGINT = NULL,
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OldStatus VARCHAR(20);
        DECLARE @OrderId BIGINT;

        -- Get current status
        SELECT @OldStatus = c_status, @OrderId = c_order_id
        FROM t_sys_invoice
        WHERE c_invoice_id = @InvoiceId;

        IF @OldStatus IS NULL
        BEGIN
            SET @Success = 0;
            SET @ErrorMessage = 'Invoice not found';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate status transition
        IF @OldStatus = 'PAID' AND @NewStatus != 'PAID'
        BEGIN
            SET @Success = 0;
            SET @ErrorMessage = 'Cannot change status of paid invoice';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update status
        UPDATE t_sys_invoice
        SET c_status = @NewStatus,
            c_modifiedby = @UpdatedBy,
            c_modifieddate = GETDATE()
        WHERE c_invoice_id = @InvoiceId;

        -- Log audit
        INSERT INTO t_sys_invoice_audit_log (
            c_invoice_id, c_order_id, c_action, c_performed_by, c_performed_by_type,
            c_old_status, c_new_status, c_remarks, c_timestamp
        )
        VALUES (
            @InvoiceId, @OrderId, 'STATUS_CHANGED', @UpdatedBy, 'ADMIN',
            @OldStatus, @NewStatus, @Remarks, GETDATE()
        );

        SET @Success = 1;
        SET @ErrorMessage = NULL;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @Success = 0;
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ sp_UpdateInvoiceStatus created';
GO

-- =============================================
-- SP 6: Link Payment To Invoice
-- =============================================
IF OBJECT_ID('sp_LinkPaymentToInvoice', 'P') IS NOT NULL
    DROP PROCEDURE sp_LinkPaymentToInvoice;
GO

CREATE PROCEDURE sp_LinkPaymentToInvoice
    @InvoiceId BIGINT,
    @RazorpayOrderId VARCHAR(100),
    @RazorpayPaymentId VARCHAR(100),
    @AmountPaid DECIMAL(18,2),
    @PaymentMethod VARCHAR(50),
    @TransactionId VARCHAR(100) = NULL,
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentAmountPaid DECIMAL(18,2);
        DECLARE @TotalAmount DECIMAL(18,2);
        DECLARE @NewAmountPaid DECIMAL(18,2);
        DECLARE @NewBalanceDue DECIMAL(18,2);
        DECLARE @NewStatus VARCHAR(20);
        DECLARE @OrderId BIGINT;
        DECLARE @StageType VARCHAR(20);
        DECLARE @OldStatus VARCHAR(20);

        -- Get current invoice details
        SELECT
            @CurrentAmountPaid = c_amount_paid,
            @TotalAmount = c_total_amount,
            @OrderId = c_order_id,
            @StageType = c_payment_stage_type,
            @OldStatus = c_status
        FROM t_sys_invoice
        WHERE c_invoice_id = @InvoiceId;

        -- Calculate new amounts
        SET @NewAmountPaid = @CurrentAmountPaid + @AmountPaid;
        SET @NewBalanceDue = @TotalAmount - @NewAmountPaid;

        -- Determine new status
        IF @NewBalanceDue <= 0
            SET @NewStatus = 'PAID';
        ELSE IF @NewAmountPaid > 0
            SET @NewStatus = 'PARTIALLY_PAID';
        ELSE
            SET @NewStatus = 'UNPAID';

        -- Update invoice
        UPDATE t_sys_invoice
        SET c_razorpay_order_id = @RazorpayOrderId,
            c_razorpay_payment_id = @RazorpayPaymentId,
            c_transaction_id = @TransactionId,
            c_payment_method = @PaymentMethod,
            c_amount_paid = @NewAmountPaid,
            c_balance_due = @NewBalanceDue,
            c_status = @NewStatus,
            c_payment_date = GETDATE(),
            c_modifieddate = GETDATE()
        WHERE c_invoice_id = @InvoiceId;

        -- Update payment schedule
        IF @NewStatus = 'PAID'
        BEGIN
            UPDATE t_sys_payment_schedule
            SET c_status = 'PAID',
                c_modifieddate = GETDATE()
            WHERE c_order_id = @OrderId AND c_stage_type = @StageType;
        END

        -- Update order payment progress
        DECLARE @TotalPaid DECIMAL(18,2);
        DECLARE @OrderTotal DECIMAL(18,2);
        DECLARE @ProgressPercentage DECIMAL(5,2);

        SELECT @TotalPaid = SUM(c_amount_paid)
        FROM t_sys_invoice
        WHERE c_order_id = @OrderId AND c_is_deleted = 0;

        SELECT @OrderTotal = c_total_amount FROM t_sys_orders WHERE c_orderid = @OrderId;

        SET @ProgressPercentage = (@TotalPaid / @OrderTotal) * 100;

        UPDATE t_sys_orders
        SET c_total_paid_amount = @TotalPaid,
            c_payment_progress_percentage = @ProgressPercentage
        WHERE c_orderid = @OrderId;

        -- Log audit
        INSERT INTO t_sys_invoice_audit_log (
            c_invoice_id, c_order_id, c_action, c_performed_by_type,
            c_old_status, c_new_status, c_old_amount_paid, c_new_amount_paid,
            c_remarks, c_timestamp
        )
        VALUES (
            @InvoiceId, @OrderId, 'PAID', 'SYSTEM',
            @OldStatus, @NewStatus, @CurrentAmountPaid, @NewAmountPaid,
            'Payment received via ' + @PaymentMethod, GETDATE()
        );

        SET @Success = 1;
        SET @ErrorMessage = NULL;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @Success = 0;
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ sp_LinkPaymentToInvoice created';
GO

-- =============================================
-- SP 7: Create Payment Schedule
-- =============================================
IF OBJECT_ID('sp_CreatePaymentSchedule', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreatePaymentSchedule;
GO

CREATE PROCEDURE sp_CreatePaymentSchedule
    @OrderId BIGINT,
    @TotalAmount DECIMAL(18,2),
    @EventDate DATETIME,
    @Success BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check if schedule already exists
        IF EXISTS (SELECT 1 FROM t_sys_payment_schedule WHERE c_order_id = @OrderId)
        BEGIN
            SET @Success = 1;
            SET @ErrorMessage = 'Payment schedule already exists';
            COMMIT TRANSACTION;
            RETURN;
        END

        DECLARE @BookingPercentage DECIMAL(5,2);
        DECLARE @PreEventPercentage DECIMAL(5,2);
        DECLARE @FinalPercentage DECIMAL(5,2);
        DECLARE @GuestLockDays INT;
        DECLARE @BookingDueDays INT;
        DECLARE @PreEventDueDays INT;
        DECLARE @FinalDueDays INT;

        -- Get settings
        SELECT @BookingPercentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_PERCENTAGE';
        SELECT @PreEventPercentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_PERCENTAGE';
        SELECT @FinalPercentage = CAST(c_value AS DECIMAL(5,2)) FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_PERCENTAGE';
        SELECT @GuestLockDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.GUEST_LOCK_DAYS';
        SELECT @BookingDueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_DUE_DAYS';
        SELECT @PreEventDueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_DUE_DAYS';
        SELECT @FinalDueDays = CAST(c_value AS INT) FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_DUE_DAYS';

        -- Defaults if settings not found
        IF @BookingPercentage IS NULL SET @BookingPercentage = 40.00;
        IF @PreEventPercentage IS NULL SET @PreEventPercentage = 35.00;
        IF @FinalPercentage IS NULL SET @FinalPercentage = 25.00;
        IF @GuestLockDays IS NULL SET @GuestLockDays = 5;
        IF @BookingDueDays IS NULL SET @BookingDueDays = 7;
        IF @PreEventDueDays IS NULL SET @PreEventDueDays = 3;
        IF @FinalDueDays IS NULL SET @FinalDueDays = 7;

        DECLARE @BookingAmount DECIMAL(18,2) = @TotalAmount * @BookingPercentage / 100;
        DECLARE @PreEventAmount DECIMAL(18,2) = @TotalAmount * @PreEventPercentage / 100;
        DECLARE @FinalAmount DECIMAL(18,2) = @TotalAmount * @FinalPercentage / 100;

        DECLARE @GuestLockDate DATETIME = DATEADD(DAY, -@GuestLockDays, @EventDate);
        DECLARE @BookingDueDate DATETIME = DATEADD(DAY, @BookingDueDays, GETDATE());
        DECLARE @PreEventDueDate DATETIME = DATEADD(DAY, -@PreEventDueDays, @EventDate);
        DECLARE @FinalDueDate DATETIME = DATEADD(DAY, @FinalDueDays, @EventDate);

        -- Insert BOOKING stage
        INSERT INTO t_sys_payment_schedule (
            c_order_id, c_stage_type, c_stage_sequence, c_percentage, c_amount,
            c_due_date, c_trigger_event, c_auto_generate_date, c_status, c_createddate
        )
        VALUES (
            @OrderId, 'BOOKING', 1, @BookingPercentage, @BookingAmount,
            @BookingDueDate, 'ORDER_APPROVED', GETDATE(), 'PENDING', GETDATE()
        );

        -- Insert PRE_EVENT stage
        INSERT INTO t_sys_payment_schedule (
            c_order_id, c_stage_type, c_stage_sequence, c_percentage, c_amount,
            c_due_date, c_trigger_event, c_auto_generate_date, c_status, c_createddate
        )
        VALUES (
            @OrderId, 'PRE_EVENT', 2, @PreEventPercentage, @PreEventAmount,
            @PreEventDueDate, 'GUEST_LOCK_DATE', @GuestLockDate, 'PENDING', GETDATE()
        );

        -- Insert FINAL stage
        INSERT INTO t_sys_payment_schedule (
            c_order_id, c_stage_type, c_stage_sequence, c_percentage, c_amount,
            c_due_date, c_trigger_event, c_auto_generate_date, c_status, c_createddate
        )
        VALUES (
            @OrderId, 'FINAL', 3, @FinalPercentage, @FinalAmount,
            @FinalDueDate, 'EVENT_COMPLETED', NULL, 'PENDING', GETDATE()
        );

        -- Update order with lock dates
        UPDATE t_sys_orders
        SET c_guest_lock_date = @GuestLockDate,
            c_menu_lock_date = DATEADD(DAY, -3, @EventDate),
            c_original_guest_count = c_guest_count
        WHERE c_orderid = @OrderId;

        SET @Success = 1;
        SET @ErrorMessage = NULL;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @Success = 0;
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END
GO

PRINT '✓ sp_CreatePaymentSchedule created';
GO

-- =============================================
-- SP 8: Get Payment Schedule
-- =============================================
IF OBJECT_ID('sp_GetPaymentSchedule', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetPaymentSchedule;
GO

CREATE PROCEDURE sp_GetPaymentSchedule
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Order summary
    SELECT
        o.c_orderid AS OrderId,
        o.c_order_number AS OrderNumber,
        o.c_total_amount AS TotalOrderAmount,
        o.c_total_paid_amount AS TotalPaidAmount,
        (o.c_total_amount - o.c_total_paid_amount) AS TotalPendingAmount,
        o.c_payment_progress_percentage AS PaymentProgressPercentage
    FROM t_sys_orders o
    WHERE o.c_orderid = @OrderId;

    -- Payment stages
    SELECT
        ps.c_schedule_id AS ScheduleId,
        ps.c_order_id AS OrderId,
        ps.c_stage_type AS StageType,
        ps.c_stage_sequence AS StageSequence,
        ps.c_percentage AS Percentage,
        ps.c_amount AS Amount,
        ps.c_due_date AS DueDate,
        ps.c_trigger_event AS TriggerEvent,
        ps.c_auto_generate_date AS AutoGenerateDate,
        ps.c_invoice_id AS InvoiceId,
        ps.c_status AS Status,
        ps.c_reminder_sent_count AS ReminderSentCount,
        ps.c_last_reminder_date AS LastReminderDate,
        ps.c_next_reminder_date AS NextReminderDate,
        ps.c_createddate AS CreatedDate,
        ps.c_modifieddate AS ModifiedDate,
        -- Invoice details if exists
        i.c_invoice_number AS InvoiceNumber,
        i.c_status AS InvoiceStatus,
        i.c_total_amount AS InvoiceTotalAmount,
        i.c_balance_due AS InvoiceBalanceDue
    FROM t_sys_payment_schedule ps
    LEFT JOIN t_sys_invoice i ON ps.c_invoice_id = i.c_invoice_id AND i.c_is_deleted = 0
    WHERE ps.c_order_id = @OrderId
    ORDER BY ps.c_stage_sequence;
END
GO

PRINT '✓ sp_GetPaymentSchedule created';
GO

-- =============================================
-- SP 9: Get Orders For Auto Invoice Generation
-- =============================================
IF OBJECT_ID('sp_GetOrdersForAutoInvoiceGeneration', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetOrdersForAutoInvoiceGeneration;
GO

CREATE PROCEDURE sp_GetOrdersForAutoInvoiceGeneration
AS
BEGIN
    SET NOCOUNT ON;

    -- Find orders where auto-generate date has passed and invoice not yet generated
    SELECT DISTINCT
        ps.c_order_id AS OrderId,
        ps.c_stage_type AS StageType,
        ps.c_auto_generate_date AS AutoGenerateDate,
        o.c_order_number AS OrderNumber,
        o.c_event_date AS EventDate
    FROM t_sys_payment_schedule ps
    INNER JOIN t_sys_orders o ON ps.c_order_id = o.c_orderid
    WHERE ps.c_auto_generate_date <= GETDATE()
        AND ps.c_invoice_id IS NULL
        AND ps.c_status = 'PENDING'
        AND o.c_isactive = 1
        AND o.c_order_status NOT IN ('Cancelled', 'Rejected')
    ORDER BY ps.c_auto_generate_date;
END
GO

PRINT '✓ sp_GetOrdersForAutoInvoiceGeneration created';
GO

-- =============================================
-- SP 10: Get Overdue Invoices
-- =============================================
IF OBJECT_ID('sp_GetOverdueInvoices', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetOverdueInvoices;
GO

CREATE PROCEDURE sp_GetOverdueInvoices
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.c_invoice_id AS InvoiceId,
        i.c_invoice_number AS InvoiceNumber,
        i.c_invoice_type AS InvoiceType,
        i.c_invoice_date AS InvoiceDate,
        i.c_due_date AS DueDate,
        i.c_total_amount AS TotalAmount,
        i.c_balance_due AS BalanceDue,
        i.c_status AS Status,
        DATEDIFF(DAY, i.c_due_date, GETDATE()) AS DaysOverdue,
        o.c_order_number AS OrderNumber,
        u.c_fullname AS CustomerName,
        u.c_email AS CustomerEmail,
        u.c_phone AS CustomerPhone
    FROM t_sys_invoice i
    INNER JOIN t_sys_orders o ON i.c_order_id = o.c_orderid
    INNER JOIN t_sys_user u ON i.c_userid = u.c_userid
    WHERE i.c_status IN ('UNPAID', 'OVERDUE')
        AND i.c_due_date < GETDATE()
        AND i.c_is_deleted = 0
    ORDER BY i.c_due_date;
END
GO

PRINT '✓ sp_GetOverdueInvoices created';
GO

-- =============================================
-- SP 11: Log Invoice Audit
-- =============================================
IF OBJECT_ID('sp_LogInvoiceAudit', 'P') IS NOT NULL
    DROP PROCEDURE sp_LogInvoiceAudit;
GO

CREATE PROCEDURE sp_LogInvoiceAudit
    @InvoiceId BIGINT,
    @OrderId BIGINT,
    @Action VARCHAR(50),
    @PerformedBy BIGINT = NULL,
    @PerformedByType VARCHAR(20) = 'SYSTEM',
    @Remarks NVARCHAR(1000) = NULL,
    @OldStatus VARCHAR(20) = NULL,
    @NewStatus VARCHAR(20) = NULL,
    @IpAddress VARCHAR(50) = NULL,
    @UserAgent VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO t_sys_invoice_audit_log (
        c_invoice_id, c_order_id, c_action, c_performed_by, c_performed_by_type,
        c_old_status, c_new_status, c_remarks, c_ip_address, c_user_agent, c_timestamp
    )
    VALUES (
        @InvoiceId, @OrderId, @Action, @PerformedBy, @PerformedByType,
        @OldStatus, @NewStatus, @Remarks, @IpAddress, @UserAgent, GETDATE()
    );
END
GO

PRINT '✓ sp_LogInvoiceAudit created';
GO

-- =============================================
-- SP 12: Get Invoice Statistics
-- =============================================
IF OBJECT_ID('sp_GetInvoiceStatistics', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetInvoiceStatistics;
GO

CREATE PROCEDURE sp_GetInvoiceStatistics
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @OwnerId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Set defaults
    IF @StartDate IS NULL SET @StartDate = DATEADD(MONTH, -1, GETDATE());
    IF @EndDate IS NULL SET @EndDate = GETDATE();

    SELECT
        COUNT(*) AS TotalInvoices,
        SUM(CASE WHEN c_status = 'UNPAID' THEN 1 ELSE 0 END) AS UnpaidInvoices,
        SUM(CASE WHEN c_status = 'PAID' THEN 1 ELSE 0 END) AS PaidInvoices,
        SUM(CASE WHEN c_status = 'OVERDUE' THEN 1 ELSE 0 END) AS OverdueInvoices,
        SUM(c_total_amount) AS TotalInvoiceAmount,
        SUM(c_amount_paid) AS TotalPaidAmount,
        SUM(c_balance_due) AS TotalPendingAmount,
        SUM(CASE WHEN c_status IN ('UNPAID', 'OVERDUE') THEN c_balance_due ELSE 0 END) AS TotalOverdueAmount,
        SUM(CASE WHEN c_invoice_type = 1 THEN 1 ELSE 0 END) AS BookingInvoiceCount,
        SUM(CASE WHEN c_invoice_type = 2 THEN 1 ELSE 0 END) AS PreEventInvoiceCount,
        SUM(CASE WHEN c_invoice_type = 3 THEN 1 ELSE 0 END) AS FinalInvoiceCount,
        AVG(c_total_amount) AS AverageInvoiceAmount,
        CAST(SUM(CASE WHEN c_status = 'PAID' THEN 1 ELSE 0 END) AS FLOAT) / NULLIF(COUNT(*), 0) * 100 AS PaymentSuccessRate
    FROM t_sys_invoice
    WHERE c_invoice_date BETWEEN @StartDate AND @EndDate
        AND c_is_deleted = 0
        AND (@OwnerId IS NULL OR c_ownerid = @OwnerId);
END
GO

PRINT '✓ sp_GetInvoiceStatistics created';
GO

PRINT '';
PRINT '========================================';
PRINT 'All Invoice Stored Procedures Created!';
PRINT 'Total Procedures: 12';
PRINT 'Completion Time: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO
