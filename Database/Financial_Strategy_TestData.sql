-- =============================================
-- Financial Strategy - Sample Test Data
-- Purpose: Generate test data for testing all financial strategy features
-- =============================================

-- =============================================
-- TEST SCENARIO 1: Cancellation Policy Testing
-- =============================================

-- Create test users (if not exist)
INSERT INTO t_sys_user (c_username, c_email, c_password, c_phone, c_isactive, c_isemailverified)
SELECT 'Test Customer 1', 'test.customer1@example.com', 'hashed_password', '9876543210', TRUE, TRUE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_user WHERE c_email = 'test.customer1@example.com');

-- Create test vendor (if not exist)
INSERT INTO t_sys_catering_owner (c_catering_name, c_owner_name, c_email, c_mobile, c_password, c_address, c_city, c_state, c_pincode, c_isactive)
SELECT 'Test Catering Services', 'Test Vendor 1', 'test.vendor1@example.com', '9876543211', 'hashed_password',
        'Test Address', 'Mumbai', 'Maharashtra', '400001', TRUE
WHERE NOT EXISTS (SELECT 1 FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com');

-- Test Order 1: Event 10 days away (Full Refund scenario)
INSERT INTO t_sys_orders (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
SELECT
    (SELECT c_userid FROM t_sys_user WHERE c_email = 'test.customer1@example.com'),
    (SELECT c_ownerid FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com'),
    'TEST-ORD-001', NOW() + INTERVAL '10 days', '18:00',
    'Wedding', 'Test Venue 1', 100, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    50000.00, 15.00, 'Advance_Paid', 'Confirmed',
    100
WHERE NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_ordernumber = 'TEST-ORD-001');

-- Test Order 2: Event 5 days away (Partial Refund scenario)
INSERT INTO t_sys_orders (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
SELECT
    (SELECT c_userid FROM t_sys_user WHERE c_email = 'test.customer1@example.com'),
    (SELECT c_ownerid FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com'),
    'TEST-ORD-002', NOW() + INTERVAL '5 days', '19:00',
    'Birthday Party', 'Test Venue 2', 50, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    25000.00, 15.00, 'Advance_Paid', 'Confirmed',
    50
WHERE NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_ordernumber = 'TEST-ORD-002');

-- Test Order 3: Event 1 day away (No Refund scenario)
INSERT INTO t_sys_orders (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
SELECT
    (SELECT c_userid FROM t_sys_user WHERE c_email = 'test.customer1@example.com'),
    (SELECT c_ownerid FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com'),
    'TEST-ORD-003', NOW() + INTERVAL '1 day', '20:00',
    'Corporate Event', 'Test Venue 3', 200, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    100000.00, 15.00, 'Advance_Paid', 'Confirmed',
    200
WHERE NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_ordernumber = 'TEST-ORD-003');

-- Test Order 4: Event 5 days away (should be auto-locked)
INSERT INTO t_sys_orders (
    c_userid, c_cateringownerid, c_ordernumber, c_event_date, c_event_time,
    c_event_type, c_event_location, c_guest_count, c_delivery_address,
    c_contact_person, c_contact_phone, c_contact_email,
    c_total_amount, c_commission_rate, c_payment_status, c_order_status,
    c_original_guest_count
)
SELECT
    (SELECT c_userid FROM t_sys_user WHERE c_email = 'test.customer1@example.com'),
    (SELECT c_ownerid FROM t_sys_catering_owner WHERE c_email = 'test.vendor1@example.com'),
    'TEST-ORD-004', NOW() + INTERVAL '5 days', '18:00',
    'Wedding Reception', 'Test Venue 4', 150, 'Test Delivery Address',
    'Test Contact', '9876543210', 'test@example.com',
    75000.00, 15.00, 'Advance_Paid', 'Confirmed',
    150
WHERE NOT EXISTS (SELECT 1 FROM t_sys_orders WHERE c_ordernumber = 'TEST-ORD-004');
PRINT '';
PRINT '2. Test Auto Guest Count Lock:';
PRINT '   EXEC sp_AutoLockGuestCount';
PRINT '   -- Check results:';
PRINT '   SELECT c_ordernumber, c_guest_count, c_locked_guest_count, c_guest_count_locked, c_guest_count_locked_date';
PRINT '   FROM t_sys_orders WHERE c_ordernumber LIKE ''TEST-ORD-%''';
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

