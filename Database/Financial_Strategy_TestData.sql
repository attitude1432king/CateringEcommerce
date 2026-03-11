-- =============================================
-- Financial Strategy - Sample Test Data
-- Purpose: Generate test data for testing all financial strategy features
-- =============================================

USE [CateringDB];
GO

PRINT '================================================';
PRINT 'Creating Sample Test Data';
PRINT '================================================';
PRINT '';

-- =============================================
-- TEST SCENARIO 1: Cancellation Policy Testing
-- =============================================

PRINT 'Creating test orders for cancellation scenarios...';

-- Create test users (if not exist)
IF NOT EXISTS (SELECT 1 FROM t_sys_user WHERE c_email = 'test.customer1@example.com')
BEGIN
    INSERT INTO t_sys_user (c_username, c_email, c_password, c_phone, c_isactive, c_isemailverified)
    VALUES ('Test Customer 1', 'test.customer1@example.com', 'hashed_password', '9876543210', 1, 1);
    PRINT '  ✓ Test customer 1 created';
END

-- Create test vendor (if not exist)
IF NOT EXISTS (SELECT 1 FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com')
BEGIN
    INSERT INTO t_sys_catering_owner (c_catering_name, c_owner_name, c_email, c_mobile, c_password, c_address, c_city, c_state, c_pincode, c_isactive)
    VALUES ('Test Catering Services', 'Test Vendor 1', 'test.vendor1@example.com', '9876543211', 'hashed_password',
            'Test Address', 'Mumbai', 'Maharashtra', '400001', 1);
    PRINT '  ✓ Test vendor 1 created';
END

DECLARE @TestUserId BIGINT = (SELECT c_userid FROM t_sys_user WHERE c_email = 'test.customer1@example.com');
DECLARE @TestVendorId BIGINT = (SELECT c_ownerid FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com');

-- Test Order 1: Event 10 days away (Full Refund scenario)
INSERT INTO t_sys_order (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
VALUES (
    @TestUserId, @TestVendorId, 'TEST-ORD-001', DATEADD(DAY, 10, GETDATE()), '18:00',
    'Wedding', 'Test Venue 1', 100, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    50000.00, 15.00, 'Advance_Paid', 'Confirmed',
    100
);
PRINT '  ✓ Test Order 1 created (10 days away - Full Refund eligible)';

-- Initialize payment summary for Order 1
DECLARE @Order1Id BIGINT = (SELECT c_orderid FROM t_sys_order WHERE c_ordernumber = 'TEST-ORD-001');
EXEC sp_InitializeOrderPayment @OrderId = @Order1Id, @TotalAmount = 50000, @AdvancePercentage = 50, @CommissionRate = 15;

-- Test Order 2: Event 5 days away (Partial Refund scenario)
INSERT INTO t_sys_order (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
VALUES (
    @TestUserId, @TestVendorId, 'TEST-ORD-002', DATEADD(DAY, 5, GETDATE()), '19:00',
    'Birthday Party', 'Test Venue 2', 50, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    25000.00, 15.00, 'Advance_Paid', 'Confirmed',
    50
);
PRINT '  ✓ Test Order 2 created (5 days away - Partial Refund eligible)';

DECLARE @Order2Id BIGINT = (SELECT c_orderid FROM t_sys_order WHERE c_ordernumber = 'TEST-ORD-002');
EXEC sp_InitializeOrderPayment @OrderId = @Order2Id, @TotalAmount = 25000, @AdvancePercentage = 50, @CommissionRate = 15;

-- Test Order 3: Event 1 day away (No Refund scenario)
INSERT INTO t_sys_order (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
VALUES (
    @TestUserId, @TestVendorId, 'TEST-ORD-003', DATEADD(DAY, 1, GETDATE()), '20:00',
    'Corporate Event', 'Test Venue 3', 200, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    100000.00, 15.00, 'Advance_Paid', 'Confirmed',
    200
);
PRINT '  ✓ Test Order 3 created (1 day away - No Refund)';

DECLARE @Order3Id BIGINT = (SELECT c_orderid FROM t_sys_order WHERE c_ordernumber = 'TEST-ORD-003');
EXEC sp_InitializeOrderPayment @OrderId = @Order3Id, @TotalAmount = 100000, @AdvancePercentage = 50, @CommissionRate = 15;

PRINT '';

-- =============================================
-- TEST SCENARIO 2: Guest Count Locking
-- =============================================

PRINT 'Creating test orders for guest count locking...';

-- Test Order 4: Event 5 days away (should be auto-locked)
INSERT INTO t_sys_order (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
VALUES (
    @TestUserId, @TestVendorId, 'TEST-ORD-004', DATEADD(DAY, 5, GETDATE()), '18:00',
    'Wedding Reception', 'Test Venue 4', 150, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    75000.00, 15.00, 'Advance_Paid', 'Confirmed',
    150
);
PRINT '  ✓ Test Order 4 created (5 days away - Guest count lock eligible)';

PRINT '';

-- =============================================
-- TEST SCENARIO 3: Vendor Partnership Tiers
-- =============================================

PRINT 'Creating vendor partnership tier data...';

-- Create Founder Partner tier for test vendor
IF NOT EXISTS (SELECT 1 FROM t_sys_vendor_partnership_tiers WHERE c_ownerid = @TestVendorId)
BEGIN
    INSERT INTO t_sys_vendor_partnership_tiers (
        c_ownerid, c_tier_name, c_current_commission_rate,
        c_tier_start_date, c_tier_lock_end_date, c_is_lock_period_active,
        c_joining_date, c_joining_order_number, c_required_orders_for_lock,
        c_completed_orders_count, c_lock_qualified, c_lock_qualified_date,
        c_monthly_order_count, c_average_rating,
        c_next_tier_name, c_next_tier_commission_rate, c_next_tier_effective_date,
        c_has_founder_badge, c_has_featured_listing, c_has_priority_support
    )
    VALUES (
        @TestVendorId, 'FOUNDER_PARTNER', 8.00,
        GETDATE(), DATEADD(MONTH, 12, GETDATE()), 1,
        GETDATE(), 5, 5,
        3, 0, NULL,
        10, 4.5,
        'LAUNCH_PARTNER', 10.00, DATEADD(MONTH, 12, GETDATE()),
        1, 1, 1
    );
    PRINT '  ✓ Vendor partnership tier created (Founder Partner - 8%)';
END

-- Create security deposit
IF NOT EXISTS (SELECT 1 FROM t_sys_vendor_security_deposits WHERE c_ownerid = @TestVendorId)
BEGIN
    INSERT INTO t_sys_vendor_security_deposits (
        c_ownerid, c_deposit_amount, c_deposit_paid, c_deposit_paid_date,
        c_payment_method, c_transaction_id,
        c_current_balance, c_holds_amount, c_available_balance,
        c_status, c_is_active
    )
    VALUES (
        @TestVendorId, 25000.00, 1, GETDATE(),
        'BANK_TRANSFER', 'TEST-TXN-001',
        25000.00, 0, 25000.00,
        'Active', 1
    );
    PRINT '  ✓ Vendor security deposit created (₹25,000)';

    -- Log deposit transaction
    DECLARE @DepositId BIGINT = (SELECT c_deposit_id FROM t_sys_vendor_security_deposits WHERE c_ownerid = @TestVendorId);
    INSERT INTO t_sys_deposit_transactions (
        c_deposit_id, c_ownerid, c_transaction_type, c_amount,
        c_balance_before, c_balance_after, c_reason, c_reference_type
    )
    VALUES (
        @DepositId, @TestVendorId, 'DEPOSIT', 25000.00,
        0, 25000.00, 'Initial security deposit payment', 'INITIAL_DEPOSIT'
    );
END

PRINT '';

-- =============================================
-- TEST QUERIES - Run these to test features
-- =============================================

PRINT '================================================';
PRINT 'Test Data Created Successfully!';
PRINT '================================================';
PRINT '';
PRINT 'Test Queries to Run:';
PRINT '';
PRINT '1. Test Cancellation Policy Calculation:';
PRINT '   EXEC sp_CalculateCancellationRefund @OrderId = ' + CAST(@Order1Id AS VARCHAR);
PRINT '';
PRINT '2. Test Auto Guest Count Lock:';
PRINT '   EXEC sp_AutoLockGuestCount';
PRINT '   -- Check results:';
PRINT '   SELECT c_ordernumber, c_guest_count, c_locked_guest_count, c_guest_count_locked, c_guest_count_locked_date';
PRINT '   FROM t_sys_order WHERE c_ordernumber LIKE ''TEST-ORD-%''';
PRINT '';
PRINT '3. Test Cancellation Request:';
PRINT '   EXEC sp_ProcessCancellationRequest';
PRINT '       @OrderId = ' + CAST(@Order1Id AS VARCHAR) + ',';
PRINT '       @UserId = ' + CAST(@TestUserId AS VARCHAR) + ',';
PRINT '       @CancellationReason = ''Testing cancellation policy''';
PRINT '';
PRINT '4. Test Guest Count Change Request:';
PRINT '   EXEC sp_RequestGuestCountChange';
PRINT '       @OrderId = ' + CAST(@Order2Id AS VARCHAR) + ',';
PRINT '       @UserId = ' + CAST(@TestUserId AS VARCHAR) + ',';
PRINT '       @NewGuestCount = 60,';
PRINT '       @ChangeReason = ''More guests confirmed''';
PRINT '';
PRINT '5. View Vendor Partnership Tier:';
PRINT '   SELECT * FROM t_sys_vendor_partnership_tiers WHERE c_ownerid = ' + CAST(@TestVendorId AS VARCHAR);
PRINT '';
PRINT '6. View Security Deposit:';
PRINT '   SELECT * FROM t_sys_vendor_security_deposits WHERE c_ownerid = ' + CAST(@TestVendorId AS VARCHAR);
PRINT '';
PRINT '================================================';
GO
