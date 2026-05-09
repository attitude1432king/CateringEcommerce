-- =============================================
-- Invoice System Stored Procedures
-- Version: 1.0.0
-- Date: 2026-02-20
-- Description: Complete set of stored procedures for invoice operations
-- =============================================

CREATE OR REPLACE FUNCTION sp_GenerateInvoiceNumber()
RETURNS VARCHAR
LANGUAGE plpgsql
AS $$
DECLARE
    InvoiceNumber VARCHAR(50);
    Prefix VARCHAR(10);
    DatePart VARCHAR(8);
    Sequence INT;
BEGIN
    SELECT c_value INTO Prefix
    FROM t_sys_settings
    WHERE c_key = 'INVOICE.PREFIX'
    LIMIT 1;

    IF Prefix IS NULL THEN Prefix := 'INV'; END IF;

    DatePart := TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDD');

    SELECT COALESCE(MAX(CAST(RIGHT(c_invoice_number, 5) AS INT)), 0) + 1
    INTO Sequence
    FROM t_sys_invoice
    WHERE c_invoice_number LIKE Prefix || '-' || DatePart || '%';

    InvoiceNumber := Prefix || '-' || DatePart || '-' || LPAD(Sequence::TEXT, 5, '0');

    RETURN InvoiceNumber;
END;
$$;

-- =============================================
-- SP 2: Generate Invoice (Auto-generation)
-- =============================================

DROP PROCEDURE IF EXISTS sp_GenerateInvoice;
DROP FUNCTION IF EXISTS sp_GenerateInvoice(BIGINT, INT, BIGINT, VARCHAR, INT, NUMERIC, NUMERIC, NUMERIC, NUMERIC);

CREATE OR REPLACE FUNCTION sp_GenerateInvoice(
    p_OrderId BIGINT,
    p_InvoiceType INT,
    p_TriggeredBy BIGINT DEFAULT NULL,
    p_TriggeredByType VARCHAR(20) DEFAULT 'SYSTEM',
    p_ExtraGuestCount INT DEFAULT 0,
    p_ExtraGuestCharges NUMERIC(18,2) DEFAULT 0,
    p_AddonCharges NUMERIC(18,2) DEFAULT 0,
    p_OvertimeCharges NUMERIC(18,2) DEFAULT 0,
    p_OtherCharges NUMERIC(18,2) DEFAULT 0
)
RETURNS TABLE (InvoiceId BIGINT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_InvoiceNumber VARCHAR(50);
    v_IsProforma BOOLEAN := FALSE;
    v_StageType VARCHAR(20);
    v_Percentage NUMERIC(5,2);
    v_DueDays INT;
    v_DueDate TIMESTAMP;
    v_Subtotal NUMERIC(18,2);
    v_CgstRate NUMERIC(5,2);
    v_SgstRate NUMERIC(5,2);
    v_CgstAmount NUMERIC(18,2);
    v_SgstAmount NUMERIC(18,2);
    v_TotalTax NUMERIC(18,2);
    v_TotalAmount NUMERIC(18,2);
    v_UserId BIGINT;
    v_OwnerId BIGINT;
    v_OrderTotal NUMERIC(18,2);
    v_CompanyGstin VARCHAR(15);
    v_PlaceOfSupply VARCHAR(100);
    v_ItemDescription VARCHAR(500);
    v_InvoiceId BIGINT;
BEGIN

    -- Get order details
    SELECT c_userid, c_ownerid, c_total_amount
    INTO v_UserId, v_OwnerId, v_OrderTotal
    FROM t_sys_orders
    WHERE c_orderid = p_OrderId;

    IF v_UserId IS NULL THEN
        RAISE EXCEPTION 'Order not found';
    END IF;

    -- Check if invoice already exists
    SELECT c_invoice_id INTO v_InvoiceId
    FROM t_sys_invoice
    WHERE c_orderid = p_OrderId
      AND c_invoice_type = p_InvoiceType
      AND c_is_deleted = FALSE
    LIMIT 1;

    IF v_InvoiceId IS NOT NULL THEN
        RETURN QUERY SELECT v_InvoiceId;
        RETURN;
    END IF;

    -- Get settings
    SELECT c_value::NUMERIC INTO v_CgstRate FROM t_sys_settings WHERE c_key = 'GST.CGST_RATE';
    SELECT c_value::NUMERIC INTO v_SgstRate FROM t_sys_settings WHERE c_key = 'GST.SGST_RATE';
    SELECT c_value INTO v_CompanyGstin FROM t_sys_settings WHERE c_key = 'GST.COMPANY_GSTIN';
    SELECT c_value INTO v_PlaceOfSupply FROM t_sys_settings WHERE c_key = 'GST.PLACE_OF_SUPPLY';

    IF v_CgstRate IS NULL THEN v_CgstRate := 9.00; END IF;
    IF v_SgstRate IS NULL THEN v_SgstRate := 9.00; END IF;

    -- Invoice type logic
    IF p_InvoiceType = 1 THEN
        v_IsProforma := TRUE;
        v_StageType := 'BOOKING';

        SELECT c_value::NUMERIC INTO v_Percentage FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_PERCENTAGE';
        SELECT c_value::INT INTO v_DueDays FROM t_sys_settings WHERE c_key = 'PAYMENT.BOOKING_DUE_DAYS';

        IF v_Percentage IS NULL THEN v_Percentage := 40.00; END IF;
        IF v_DueDays IS NULL THEN v_DueDays := 7; END IF;

    ELSIF p_InvoiceType = 2 THEN
        v_IsProforma := FALSE;
        v_StageType := 'PRE_EVENT';

        SELECT c_value::NUMERIC INTO v_Percentage FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_PERCENTAGE';
        SELECT c_value::INT INTO v_DueDays FROM t_sys_settings WHERE c_key = 'PAYMENT.PRE_EVENT_DUE_DAYS';

        IF v_Percentage IS NULL THEN v_Percentage := 35.00; END IF;
        IF v_DueDays IS NULL THEN v_DueDays := 3; END IF;

    ELSIF p_InvoiceType = 3 THEN
        v_IsProforma := FALSE;
        v_StageType := 'FINAL';

        SELECT c_value::NUMERIC INTO v_Percentage FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_PERCENTAGE';
        SELECT c_value::INT INTO v_DueDays FROM t_sys_settings WHERE c_key = 'PAYMENT.FINAL_DUE_DAYS';

        IF v_Percentage IS NULL THEN v_Percentage := 25.00; END IF;
        IF v_DueDays IS NULL THEN v_DueDays := 7; END IF;
    END IF;

    -- Calculate subtotal
    v_Subtotal := (v_OrderTotal * v_Percentage / 100);

    IF p_InvoiceType = 3 THEN
        v_Subtotal := v_Subtotal + p_ExtraGuestCharges + p_AddonCharges + p_OvertimeCharges + p_OtherCharges;
    END IF;

    -- Tax calculation
    v_CgstAmount := v_Subtotal * v_CgstRate / 100;
    v_SgstAmount := v_Subtotal * v_SgstRate / 100;
    v_TotalTax := v_CgstAmount + v_SgstAmount;
    v_TotalAmount := v_Subtotal + v_TotalTax;

    -- Due date
    v_DueDate := CURRENT_TIMESTAMP + (v_DueDays || ' days')::INTERVAL;

    -- Generate invoice number (sp_GenerateInvoiceNumber is a FUNCTION returning VARCHAR)
    v_InvoiceNumber := sp_GenerateInvoiceNumber();

    -- Insert invoice
    INSERT INTO t_sys_invoice (
        c_orderid, c_userid, c_ownerid, c_invoice_type, c_is_proforma,
        c_invoice_number, c_invoice_date, c_due_date,
        c_subtotal, c_cgst_percent, c_sgst_percent, c_cgst_amount, c_sgst_amount,
        c_total_tax_amount, c_discount_amount, c_total_amount, c_amount_paid, c_balance_due,
        c_payment_stage_type, c_payment_percentage, c_status,
        c_company_gstin, c_place_of_supply, c_sac_code,
        c_createdby, c_createddate
    )
    VALUES (
        p_OrderId, v_UserId, v_OwnerId, p_InvoiceType, v_IsProforma,
        v_InvoiceNumber, CURRENT_TIMESTAMP, v_DueDate,
        v_Subtotal, v_CgstRate, v_SgstRate, v_CgstAmount, v_SgstAmount,
        v_TotalTax, 0, v_TotalAmount, 0, v_TotalAmount,
        v_StageType, v_Percentage, 'UNPAID',
        v_CompanyGstin, v_PlaceOfSupply, '996331',
        p_TriggeredBy, CURRENT_TIMESTAMP
    )
    RETURNING c_invoice_id INTO v_InvoiceId;

    -- Line item description
    v_ItemDescription :=
        CASE
            WHEN p_InvoiceType = 1 THEN 'Booking Advance Payment (40%)'
            WHEN p_InvoiceType = 2 THEN 'Pre-Event Payment (35%)'
            WHEN p_InvoiceType = 3 THEN
                'Final Settlement (25%)' ||
                CASE WHEN p_ExtraGuestCharges > 0 THEN ' + Extra Guest Charges' ELSE '' END ||
                CASE WHEN p_AddonCharges > 0 THEN ' + Add-on Items' ELSE '' END ||
                CASE WHEN p_OvertimeCharges > 0 THEN ' + Overtime Charges' ELSE '' END
        END;

    -- Insert line item
    INSERT INTO t_sys_invoice_line_items (
        c_invoice_id, c_item_type, c_description, c_quantity, c_unit_price, c_subtotal,
        c_tax_percent, c_cgst_percent, c_sgst_percent, c_tax_amount, c_cgst_amount, c_sgst_amount,
        c_discount_amount, c_total, c_sequence
    )
    VALUES (
        v_InvoiceId, 'OTHER', v_ItemDescription, 1, v_Subtotal, v_Subtotal,
        (v_CgstRate + v_SgstRate), v_CgstRate, v_SgstRate, v_TotalTax, v_CgstAmount, v_SgstAmount,
        0, v_TotalAmount, 1
    );

    -- Update payment schedule
    UPDATE t_sys_payment_schedule
    SET c_invoice_id = v_InvoiceId,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_orderid = p_OrderId AND c_stage_type = v_StageType;

    -- Audit log
    INSERT INTO t_sys_invoice_audit_log (
        c_invoice_id, c_orderid, c_action, c_performed_by, c_performed_by_type,
        c_new_status, c_remarks, c_timestamp
    )
    VALUES (
        v_InvoiceId, p_OrderId, 'GENERATED', p_TriggeredBy, p_TriggeredByType,
        'UNPAID', 'Invoice auto-generated', CURRENT_TIMESTAMP
    );

    RETURN QUERY SELECT v_InvoiceId;
END;
$$;
-- =============================================
-- SP: Get Invoice By Id
-- =============================================

DROP FUNCTION IF EXISTS sp_GetInvoiceById(BIGINT);

CREATE OR REPLACE FUNCTION sp_GetInvoiceById(
    p_InvoiceId BIGINT
)
RETURNS SETOF REFCURSOR
LANGUAGE plpgsql
AS $$
DECLARE
    ref1 REFCURSOR;
    ref2 REFCURSOR;
BEGIN

    -- Cursor 1: Main invoice details
    OPEN ref1 FOR
    SELECT
        i.c_invoice_id AS "InvoiceId",
        i.c_orderid AS "OrderId",
        i.c_event_id AS "EventId",
        i.c_userid AS "UserId",
        i.c_ownerid AS "CateringOwnerId",
        i.c_invoice_type AS "InvoiceType",
        i.c_is_proforma AS "IsProforma",
        i.c_invoice_number AS "InvoiceNumber",
        i.c_invoice_date AS "InvoiceDate",
        i.c_due_date AS "DueDate",
        i.c_subtotal AS "Subtotal",
        i.c_cgst_percent AS "CgstPercent",
        i.c_sgst_percent AS "SgstPercent",
        i.c_cgst_amount AS "CgstAmount",
        i.c_sgst_amount AS "SgstAmount",
        i.c_total_tax_amount AS "TotalTaxAmount",
        i.c_discount_amount AS "DiscountAmount",
        i.c_total_amount AS "TotalAmount",
        i.c_amount_paid AS "AmountPaid",
        i.c_balance_due AS "BalanceDue",
        i.c_payment_stage_type AS "PaymentStageType",
        i.c_payment_percentage AS "PaymentPercentage",
        i.c_status AS "Status",
        i.c_razorpay_order_id AS "RazorpayOrderId",
        i.c_razorpay_payment_id AS "RazorpayPaymentId",
        i.c_transaction_id AS "TransactionId",
        i.c_payment_method AS "PaymentMethod",
        i.c_payment_date AS "PaymentDate",
        i.c_company_gstin AS "CompanyGstin",
        i.c_customer_gstin AS "CustomerGstin",
        i.c_place_of_supply AS "PlaceOfSupply",
        i.c_sac_code AS "SacCode",
        i.c_notes AS "Notes",
        i.c_terms_and_conditions AS "TermsAndConditions",
        i.c_internal_remarks AS "InternalRemarks",
        i.c_pdf_path AS "PdfPath",
        i.c_pdf_generated_date AS "PdfGeneratedDate",
        i.c_createdby AS "CreatedBy",
        i.c_createddate AS "CreatedDate",
        i.c_modifiedby AS "ModifiedBy",
        i.c_modifieddate AS "ModifiedDate",
        i.c_version AS "Version",
        i.c_parent_invoice_id AS "ParentInvoiceId",
        -- Order details
        o.c_order_number AS "OrderNumber",
        o.c_event_date AS "EventDate",
        o.c_event_time AS "EventTime",
        o.c_event_type AS "EventType",
        o.c_event_location AS "EventLocation",
        o.c_guest_count AS "GuestCount",
        o.c_original_guest_count AS "OriginalGuestCount",
        o.c_final_guest_count AS "FinalGuestCount",
        o.c_guest_locked AS "GuestCountLocked",
        o.c_menu_locked AS "MenuLocked",
        -- User details
        u.c_fullname AS "CustomerName",
        u.c_phone AS "CustomerPhone",
        u.c_email AS "CustomerEmail",
        -- Owner details
        co.c_business_name AS "PartnerName",
        co.c_contact_phone AS "PartnerPhone",
        co.c_contact_email AS "PartnerEmail"
    FROM t_sys_invoice i
    INNER JOIN t_sys_orders o ON i.c_orderid = o.c_orderid
    INNER JOIN t_sys_user u ON i.c_userid = u.c_userid
    INNER JOIN t_sys_catering_owner co ON i.c_ownerid = co.c_ownerid
    WHERE i.c_invoice_id = p_InvoiceId
      AND i.c_is_deleted = FALSE;

    RETURN NEXT ref1;

    -- Cursor 2: Line items
    OPEN ref2 FOR
    SELECT
        c_line_item_id AS "LineItemId",
        c_invoice_id AS "InvoiceId",
        c_item_type AS "ItemType",
        c_item_id AS "ItemId",
        c_description AS "Description",
        c_hsn_sac_code AS "HsnSacCode",
        c_quantity AS "Quantity",
        c_unit_of_measure AS "UnitOfMeasure",
        c_unit_price AS "UnitPrice",
        c_subtotal AS "Subtotal",
        c_tax_percent AS "TaxPercent",
        c_cgst_percent AS "CgstPercent",
        c_sgst_percent AS "SgstPercent",
        c_tax_amount AS "TaxAmount",
        c_cgst_amount AS "CgstAmount",
        c_sgst_amount AS "SgstAmount",
        c_discount_percent AS "DiscountPercent",
        c_discount_amount AS "DiscountAmount",
        c_total AS "Total",
        c_sequence AS "Sequence",
        c_createddate AS "CreatedDate"
    FROM t_sys_invoice_line_items
    WHERE c_invoice_id = p_InvoiceId
    ORDER BY c_sequence;

    RETURN NEXT ref2;

END;
$$;

-- =============================================
-- SP 4: Get Invoices By Order ID
-- =============================================

DROP FUNCTION IF EXISTS sp_GetInvoicesByOrderId(BIGINT);

CREATE OR REPLACE FUNCTION sp_GetInvoicesByOrderId(
    p_OrderId BIGINT
)
RETURNS TABLE (
    "InvoiceId" BIGINT,
    "InvoiceType" INT,
    "InvoiceNumber" VARCHAR,
    "InvoiceDate" TIMESTAMP,
    "DueDate" TIMESTAMP,
    "TotalAmount" NUMERIC,
    "AmountPaid" NUMERIC,
    "BalanceDue" NUMERIC,
    "Status" VARCHAR,
    "PaymentStageType" VARCHAR,
    "PaymentPercentage" NUMERIC,
    "IsProforma" BOOLEAN,
    "PdfPath" VARCHAR,
    "CreatedDate" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        c_invoice_id,
        c_invoice_type,
        c_invoice_number,
        c_invoice_date,
        c_due_date,
        c_total_amount,
        c_amount_paid,
        c_balance_due,
        c_status,
        c_payment_stage_type,
        c_payment_percentage,
        c_is_proforma,
        c_pdf_path,
        c_createddate
    FROM t_sys_invoice
    WHERE c_orderid = p_OrderId
      AND c_is_deleted = FALSE
    ORDER BY c_invoice_type;

END;
$$;

-- =============================================
-- SP 5: Update Invoice Status
-- =============================================

DROP FUNCTION IF EXISTS sp_UpdateInvoiceStatus(BIGINT, VARCHAR, VARCHAR, BIGINT);

CREATE OR REPLACE FUNCTION sp_UpdateInvoiceStatus(
    p_InvoiceId BIGINT,
    p_NewStatus VARCHAR(20),
    p_Remarks VARCHAR(1000) DEFAULT NULL,
    p_UpdatedBy BIGINT DEFAULT NULL
)
RETURNS TABLE (
    "Success" BOOLEAN,
    "ErrorMessage" VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_OldStatus VARCHAR(20);
    v_OrderId BIGINT;
BEGIN

    -- Get current status
    SELECT c_status, c_orderid
    INTO v_OldStatus, v_OrderId
    FROM t_sys_invoice
    WHERE c_invoice_id = p_InvoiceId;

    IF v_OldStatus IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invoice not found';
        RETURN;
    END IF;

    -- Validate status transition
    IF v_OldStatus = 'PAID' AND p_NewStatus <> 'PAID' THEN
        RETURN QUERY SELECT FALSE, 'Cannot change status of paid invoice';
        RETURN;
    END IF;

    -- Update status
    UPDATE t_sys_invoice
    SET c_status = p_NewStatus,
        c_modifiedby = p_UpdatedBy,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_invoice_id = p_InvoiceId;

    -- Log audit
    INSERT INTO t_sys_invoice_audit_log (
        c_invoice_id, c_orderid, c_action, c_performed_by, c_performed_by_type,
        c_old_status, c_new_status, c_remarks, c_timestamp
    )
    VALUES (
        p_InvoiceId, v_OrderId, 'STATUS_CHANGED', p_UpdatedBy, 'ADMIN',
        v_OldStatus, p_NewStatus, p_Remarks, CURRENT_TIMESTAMP
    );

    -- Success
    RETURN QUERY SELECT TRUE, NULL;

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 6: Link Payment To Invoice
-- PostgreSQL Version
-- =============================================

DROP FUNCTION IF EXISTS sp_LinkPaymentToInvoice(
    BIGINT, VARCHAR, VARCHAR, NUMERIC, VARCHAR, VARCHAR
);

CREATE OR REPLACE FUNCTION sp_LinkPaymentToInvoice(
    p_InvoiceId BIGINT,
    p_RazorpayOrderId VARCHAR(100),
    p_RazorpayPaymentId VARCHAR(100),
    p_AmountPaid NUMERIC(18,2),
    p_PaymentMethod VARCHAR(50),
    p_TransactionId VARCHAR(100) DEFAULT NULL
)
RETURNS TABLE (
    "Success" BOOLEAN,
    "ErrorMessage" VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_CurrentAmountPaid NUMERIC(18,2);
    v_TotalAmount NUMERIC(18,2);
    v_NewAmountPaid NUMERIC(18,2);
    v_NewBalanceDue NUMERIC(18,2);
    v_NewStatus VARCHAR(20);
    v_OrderId BIGINT;
    v_StageType VARCHAR(20);
    v_OldStatus VARCHAR(20);

    v_TotalPaid NUMERIC(18,2);
    v_OrderTotal NUMERIC(18,2);
    v_ProgressPercentage NUMERIC(5,2);
BEGIN

    -- Get current invoice details
    SELECT
        c_amount_paid,
        c_total_amount,
        c_orderid,
        c_payment_stage_type,
        c_status
    INTO
        v_CurrentAmountPaid,
        v_TotalAmount,
        v_OrderId,
        v_StageType,
        v_OldStatus
    FROM t_sys_invoice
    WHERE c_invoice_id = p_InvoiceId;

    IF v_CurrentAmountPaid IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invoice not found';
        RETURN;
    END IF;

    -- Calculate new amounts
    v_NewAmountPaid := v_CurrentAmountPaid + p_AmountPaid;
    v_NewBalanceDue := v_TotalAmount - v_NewAmountPaid;

    -- Determine new status
    IF v_NewBalanceDue <= 0 THEN
        v_NewStatus := 'PAID';
    ELSIF v_NewAmountPaid > 0 THEN
        v_NewStatus := 'PARTIALLY_PAID';
    ELSE
        v_NewStatus := 'UNPAID';
    END IF;

    -- Update invoice
    UPDATE t_sys_invoice
    SET c_razorpay_order_id = p_RazorpayOrderId,
        c_razorpay_payment_id = p_RazorpayPaymentId,
        c_transaction_id = p_TransactionId,
        c_payment_method = p_PaymentMethod,
        c_amount_paid = v_NewAmountPaid,
        c_balance_due = v_NewBalanceDue,
        c_status = v_NewStatus,
        c_payment_date = CURRENT_TIMESTAMP,
        c_modifieddate = CURRENT_TIMESTAMP
    WHERE c_invoice_id = p_InvoiceId;

    -- Update payment schedule if fully paid
    IF v_NewStatus = 'PAID' THEN
        UPDATE t_sys_payment_schedule
        SET c_status = 'PAID',
            c_modifieddate = CURRENT_TIMESTAMP
        WHERE c_orderid = v_OrderId
          AND c_stage_type = v_StageType;
    END IF;

    -- Calculate order payment progress
    SELECT COALESCE(SUM(c_amount_paid), 0)
    INTO v_TotalPaid
    FROM t_sys_invoice
    WHERE c_orderid = v_OrderId
      AND c_is_deleted = FALSE;

    SELECT c_total_amount
    INTO v_OrderTotal
    FROM t_sys_orders
    WHERE c_orderid = v_OrderId;

    v_ProgressPercentage := (v_TotalPaid / v_OrderTotal) * 100;

    -- Update order
    UPDATE t_sys_orders
    SET c_total_paid_amount = v_TotalPaid,
        c_payment_progress_percentage = v_ProgressPercentage
    WHERE c_orderid = v_OrderId;

    -- Log audit
    INSERT INTO t_sys_invoice_audit_log (
        c_invoice_id, c_orderid, c_action, c_performed_by_type,
        c_old_status, c_new_status, c_old_amount_paid, c_new_amount_paid,
        c_remarks, c_timestamp
    )
    VALUES (
        p_InvoiceId, v_OrderId, 'PAID', 'SYSTEM',
        v_OldStatus, v_NewStatus, v_CurrentAmountPaid, v_NewAmountPaid,
        'Payment received via ' || p_PaymentMethod,
        CURRENT_TIMESTAMP
    );

    RETURN QUERY SELECT TRUE, NULL;

EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM;
END;
$$;

-- =============================================
-- SP 8: Get Payment Schedule
-- =============================================

DROP FUNCTION IF EXISTS sp_GetPaymentSchedule(BIGINT);

CREATE OR REPLACE FUNCTION sp_GetPaymentSchedule(
    p_OrderId BIGINT
)
RETURNS SETOF REFCURSOR
LANGUAGE plpgsql
AS $$
DECLARE
    ref1 REFCURSOR;
    ref2 REFCURSOR;
BEGIN

    -- Cursor 1: Order summary
    OPEN ref1 FOR
    SELECT
        o.c_orderid AS "OrderId",
        o.c_order_number AS "OrderNumber",
        o.c_total_amount AS "TotalOrderAmount",
        o.c_total_paid_amount AS "TotalPaidAmount",
        (o.c_total_amount - o.c_total_paid_amount) AS "TotalPendingAmount",
        o.c_payment_progress_percentage AS "PaymentProgressPercentage"
    FROM t_sys_orders o
    WHERE o.c_orderid = p_OrderId;

    RETURN NEXT ref1;

    -- Cursor 2: Payment stages
    OPEN ref2 FOR
    SELECT
        ps.c_schedule_id AS "ScheduleId",
        ps.c_orderid AS "OrderId",
        ps.c_stage_type AS "StageType",
        ps.c_stage_sequence AS "StageSequence",
        ps.c_percentage AS "Percentage",
        ps.c_amount AS "Amount",
        ps.c_due_date AS "DueDate",
        ps.c_trigger_event AS "TriggerEvent",
        ps.c_auto_generate_date AS "AutoGenerateDate",
        ps.c_invoice_id AS "InvoiceId",
        ps.c_status AS "Status",
        ps.c_reminder_sent_count AS "ReminderSentCount",
        ps.c_last_reminder_date AS "LastReminderDate",
        ps.c_next_reminder_date AS "NextReminderDate",
        ps.c_createddate AS "CreatedDate",
        ps.c_modifieddate AS "ModifiedDate",
        -- Invoice details
        i.c_invoice_number AS "InvoiceNumber",
        i.c_status AS "InvoiceStatus",
        i.c_total_amount AS "InvoiceTotalAmount",
        i.c_balance_due AS "InvoiceBalanceDue"
    FROM t_sys_payment_schedule ps
    LEFT JOIN t_sys_invoice i 
        ON ps.c_invoice_id = i.c_invoice_id 
       AND i.c_is_deleted = FALSE
    WHERE ps.c_orderid = p_OrderId
    ORDER BY ps.c_stage_sequence;

    RETURN NEXT ref2;

END;
$$;

-- =============================================
-- SP 9: Get Orders For Auto Invoice Generation
-- =============================================

DROP FUNCTION IF EXISTS sp_GetOrdersForAutoInvoiceGeneration();

CREATE OR REPLACE FUNCTION sp_GetOrdersForAutoInvoiceGeneration()
RETURNS TABLE (
    "OrderId" BIGINT,
    "StageType" VARCHAR,
    "AutoGenerateDate" TIMESTAMP,
    "OrderNumber" VARCHAR,
    "EventDate" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT DISTINCT
        ps.c_orderid,
        ps.c_stage_type,
        ps.c_auto_generate_date,
        o.c_order_number,
        o.c_event_date
    FROM t_sys_payment_schedule ps
    INNER JOIN t_sys_orders o 
        ON ps.c_orderid = o.c_orderid
    WHERE ps.c_auto_generate_date <= CURRENT_TIMESTAMP
        AND ps.c_invoice_id IS NULL
        AND ps.c_status = 'PENDING'
        AND o.c_isactive = TRUE
        AND o.c_order_status NOT IN ('Cancelled', 'Rejected')
    ORDER BY ps.c_auto_generate_date;

END;
$$;

-- =============================================
-- SP 10: Get Overdue Invoices
-- =============================================

DROP FUNCTION IF EXISTS sp_GetOverdueInvoices();

CREATE OR REPLACE FUNCTION sp_GetOverdueInvoices()
RETURNS TABLE (
    "InvoiceId" BIGINT,
    "InvoiceNumber" VARCHAR,
    "InvoiceType" INT,
    "InvoiceDate" TIMESTAMP,
    "DueDate" TIMESTAMP,
    "TotalAmount" NUMERIC,
    "BalanceDue" NUMERIC,
    "Status" VARCHAR,
    "DaysOverdue" INT,
    "OrderNumber" VARCHAR,
    "CustomerName" VARCHAR,
    "CustomerEmail" VARCHAR,
    "CustomerPhone" VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN

    RETURN QUERY
    SELECT
        i.c_invoice_id,
        i.c_invoice_number,
        i.c_invoice_type,
        i.c_invoice_date,
        i.c_due_date,
        i.c_total_amount,
        i.c_balance_due,
        i.c_status,
        (CURRENT_DATE - i.c_due_date::DATE) AS "DaysOverdue",
        o.c_order_number,
        u.c_fullname,
        u.c_email,
        u.c_phone
    FROM t_sys_invoice i
    INNER JOIN t_sys_orders o 
        ON i.c_orderid = o.c_orderid
    INNER JOIN t_sys_user u 
        ON i.c_userid = u.c_userid
    WHERE i.c_status IN ('UNPAID', 'OVERDUE')
        AND i.c_due_date < CURRENT_TIMESTAMP
        AND i.c_is_deleted = FALSE
    ORDER BY i.c_due_date;

END;
$$;

-- =============================================
-- SP 11: Log Invoice Audit
-- =============================================

DROP PROCEDURE IF EXISTS sp_LogInvoiceAudit;

CREATE OR REPLACE PROCEDURE sp_LogInvoiceAudit(
    IN p_InvoiceId BIGINT,
    IN p_OrderId BIGINT,
    IN p_Action VARCHAR(50),
    IN p_PerformedBy BIGINT DEFAULT NULL,
    IN p_PerformedByType VARCHAR(20) DEFAULT 'SYSTEM',
    IN p_Remarks VARCHAR(1000) DEFAULT NULL,
    IN p_OldStatus VARCHAR(20) DEFAULT NULL,
    IN p_NewStatus VARCHAR(20) DEFAULT NULL,
    IN p_IpAddress VARCHAR(50) DEFAULT NULL,
    IN p_UserAgent VARCHAR(500) DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN

    INSERT INTO t_sys_invoice_audit_log (
        c_invoice_id, c_orderid, c_action, c_performed_by, c_performed_by_type,
        c_old_status, c_new_status, c_remarks, c_ip_address, c_user_agent, c_timestamp
    )
    VALUES (
        p_InvoiceId, p_OrderId, p_Action, p_PerformedBy, p_PerformedByType,
        p_OldStatus, p_NewStatus, p_Remarks, p_IpAddress, p_UserAgent,
        CURRENT_TIMESTAMP
    );

END;
$$;

-- =============================================
-- SP 12: Get Invoice Statistics
-- =============================================

DROP FUNCTION IF EXISTS sp_GetInvoiceStatistics(TIMESTAMP, TIMESTAMP, BIGINT);

CREATE OR REPLACE FUNCTION sp_GetInvoiceStatistics(
    p_StartDate TIMESTAMP DEFAULT NULL,
    p_EndDate TIMESTAMP DEFAULT NULL,
    p_OwnerId BIGINT DEFAULT NULL
)
RETURNS TABLE (
    "TotalInvoices" BIGINT,
    "UnpaidInvoices" BIGINT,
    "PaidInvoices" BIGINT,
    "OverdueInvoices" BIGINT,
    "TotalInvoiceAmount" NUMERIC,
    "TotalPaidAmount" NUMERIC,
    "TotalPendingAmount" NUMERIC,
    "TotalOverdueAmount" NUMERIC,
    "BookingInvoiceCount" BIGINT,
    "PreEventInvoiceCount" BIGINT,
    "FinalInvoiceCount" BIGINT,
    "AverageInvoiceAmount" NUMERIC,
    "PaymentSuccessRate" NUMERIC
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_StartDate TIMESTAMP;
    v_EndDate TIMESTAMP;
BEGIN

    -- Set defaults
    v_StartDate := COALESCE(p_StartDate, CURRENT_TIMESTAMP - INTERVAL '1 month');
    v_EndDate := COALESCE(p_EndDate, CURRENT_TIMESTAMP);

    RETURN QUERY
    SELECT
        COUNT(*) AS "TotalInvoices",
        SUM(CASE WHEN c_status = 'UNPAID' THEN 1 ELSE 0 END) AS "UnpaidInvoices",
        SUM(CASE WHEN c_status = 'PAID' THEN 1 ELSE 0 END) AS "PaidInvoices",
        SUM(CASE WHEN c_status = 'OVERDUE' THEN 1 ELSE 0 END) AS "OverdueInvoices",
        SUM(c_total_amount) AS "TotalInvoiceAmount",
        SUM(c_amount_paid) AS "TotalPaidAmount",
        SUM(c_balance_due) AS "TotalPendingAmount",
        SUM(CASE WHEN c_status IN ('UNPAID', 'OVERDUE') THEN c_balance_due ELSE 0 END) AS "TotalOverdueAmount",
        SUM(CASE WHEN c_invoice_type = 1 THEN 1 ELSE 0 END) AS "BookingInvoiceCount",
        SUM(CASE WHEN c_invoice_type = 2 THEN 1 ELSE 0 END) AS "PreEventInvoiceCount",
        SUM(CASE WHEN c_invoice_type = 3 THEN 1 ELSE 0 END) AS "FinalInvoiceCount",
        AVG(c_total_amount) AS "AverageInvoiceAmount",
        (SUM(CASE WHEN c_status = 'PAID' THEN 1 ELSE 0 END)::NUMERIC 
            / NULLIF(COUNT(*), 0)) * 100 AS "PaymentSuccessRate"
    FROM t_sys_invoice
    WHERE c_invoice_date BETWEEN v_StartDate AND v_EndDate
        AND c_is_deleted = FALSE
        AND (p_OwnerId IS NULL OR c_ownerid = p_OwnerId);

END;
$$;