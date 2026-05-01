-- ========================================
-- Package Selection Feature - Test Data
-- ========================================
-- This script creates sample data for testing the Package Selection feature.
-- Run this AFTER creating the master tables from mastersql.sql

-- ========================================
-- 1. CREATE TEST CATERING OWNER
-- ========================================

INSERT INTO t_sys_catering_owner (
    c_catering_name,
    c_owner_name,
    c_email,
    c_mobile,
    c_catering_number,
    c_password_hash,
    c_isactive,
    c_verified_by_admin,
    c_isonline,
    c_createddate
)
SELECT 'Royal Caterers', 'Rajesh Kumar', 'rajesh@royalcaterers.com', '9876543210',
    '9876543210', 'hashed_password', TRUE, TRUE, TRUE, NOW()
WHERE NOT EXISTS (SELECT 1 FROM t_sys_catering_owner WHERE c_email = 'rajesh@royalcaterers.com');

-- ========================================
-- 2. CREATE TEST PACKAGES
-- ========================================

-- Package 1: Wedding Special
INSERT INTO t_sys_catering_packages (
    c_ownerid,
    c_packagename,
    c_description,
    c_price,
    c_is_active,
    c_is_deleted,
    c_createddate
)
SELECT c_ownerid, 'Wedding Special Package', 'Perfect for grand wedding ceremonies with premium selection',
    599.00, TRUE, FALSE, NOW()
FROM t_sys_catering_owner
WHERE c_email = 'rajesh@royalcaterers.com'
  AND NOT EXISTS (SELECT 1 FROM t_sys_catering_packages WHERE c_packagename = 'Wedding Special Package');

-- Package 2: Birthday Party Package
INSERT INTO t_sys_catering_packages (
    c_ownerid,
    c_packagename,
    c_description,
    c_price,
    c_is_active,
    c_is_deleted,
    c_createddate
)
SELECT c_ownerid, 'Birthday Party Package', 'Fun and delicious food for birthday celebrations',
    399.00, TRUE, FALSE, NOW()
FROM t_sys_catering_owner
WHERE c_email = 'rajesh@royalcaterers.com'
  AND NOT EXISTS (SELECT 1 FROM t_sys_catering_packages WHERE c_packagename = 'Birthday Party Package');

-- ========================================
-- 3. DEFINE PACKAGE CATEGORY MAPPINGS
-- ========================================

INSERT INTO t_sys_catering_package_items (c_packageid, c_categoryid, c_quantity, c_createddate)
SELECT cp.c_packageid, cat.c_categoryid, qty.quantity, NOW()
FROM (SELECT c_packageid FROM t_sys_catering_packages WHERE c_packagename = 'Wedding Special Package' LIMIT 1) cp,
     (VALUES (1, 2), (4, 3), (7, 1), (9, 1), (10, 1)) AS cat(c_categoryid, quantity)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_catering_package_items cpi
    WHERE cpi.c_packageid = cp.c_packageid AND cpi.c_categoryid = cat.c_categoryid
);

INSERT INTO t_sys_catering_package_items (c_packageid, c_categoryid, c_quantity, c_createddate)
SELECT cp.c_packageid, cat.c_categoryid, qty.quantity, NOW()
FROM (SELECT c_packageid FROM t_sys_catering_packages WHERE c_packagename = 'Birthday Party Package' LIMIT 1) cp,
     (VALUES (1, 3), (4, 2), (9, 2)) AS cat(c_categoryid, quantity)
WHERE NOT EXISTS (
    SELECT 1 FROM t_sys_catering_package_items cpi
    WHERE cpi.c_packageid = cp.c_packageid AND cpi.c_categoryid = cat.c_categoryid
);

-- ========================================
-- 4. CREATE FOOD ITEMS (Package-Eligible)
-- ========================================

INSERT INTO t_sys_fooditems (
    c_ownerid, c_foodname, c_description, c_categoryid,
    c_price, c_ispackage_item, c_status, c_is_deleted,
    c_createddate, c_issample_tasted
)
SELECT co.c_ownerid, food.name, food.description, food.categoryid,
    food.price, TRUE, TRUE, FALSE, NOW(), TRUE
FROM t_sys_catering_owner co,
     (VALUES
        (1, 'Paneer Tikka', 'Grilled cottage cheese cubes marinated in spices', 150.00),
        (1, 'Veg Spring Roll', 'Crispy vegetable spring rolls with sweet chili sauce', 120.00),
        (1, 'Hara Bhara Kabab', 'Spinach and peas patties served with mint chutney', 130.00),
        (1, 'Aloo Tikki', 'Spiced potato patties with chutneys', 100.00),
        (1, 'Corn Cheese Balls', 'Crispy corn and cheese fritters', 140.00),
        (4, 'Paneer Butter Masala', 'Cottage cheese in rich tomato gravy', 200.00),
        (4, 'Dal Makhani', 'Creamy black lentils cooked overnight', 180.00),
        (4, 'Veg Kolhapuri', 'Mixed vegetables in spicy Kolhapuri gravy', 190.00)
     ) AS food(categoryid, name, description, price)
WHERE co.c_email = 'rajesh@royalcaterers.com'
  AND NOT EXISTS (SELECT 1 FROM t_sys_fooditems WHERE c_ownerid = co.c_ownerid AND c_foodname = food.name);
    (@TestOwnerId, 'Kadhai Paneer', 'Cottage cheese with bell peppers in kadhai masala', 4, 210.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Chole Bhature', 'Spicy chickpea curry with fried bread', 4, 170.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Mushroom Masala', 'Button mushrooms in onion tomato gravy', 4, 195.00, 1, 1, 0, NOW(), 1);

PRINT 'Added 6 Main Course items';

-- CATEGORY 7: SWEET REGULAR
INSERT INTO t_sys_fooditems (
    c_ownerid, c_foodname, c_description, c_categoryid,
    c_price, c_ispackage_item, c_status, c_is_deleted,
    c_createddate, c_issample_tasted
)
VALUES
    (@TestOwnerId, 'Gulab Jamun', 'Traditional milk solid dumplings in sugar syrup', 7, 80.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Rasgulla', 'Soft cottage cheese balls in sugar syrup', 7, 70.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Jalebi', 'Crispy sweet spirals soaked in sugar syrup', 7, 75.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Rasmalai', 'Cottage cheese patties in sweetened milk', 7, 90.00, 1, 1, 0, NOW(), 1);

PRINT 'Added 4 Sweet Regular items';

-- CATEGORY 9: DESSERT
INSERT INTO t_sys_fooditems (
    c_ownerid, c_foodname, c_description, c_categoryid,
    c_price, c_ispackage_item, c_status, c_is_deleted,
    c_createddate, c_issample_tasted
)
VALUES
    (@TestOwnerId, 'Chocolate Ice Cream', 'Premium chocolate ice cream', 9, 60.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Vanilla Ice Cream', 'Classic vanilla ice cream', 9, 50.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Strawberry Ice Cream', 'Fresh strawberry ice cream', 9, 55.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Fruit Custard', 'Mixed fruits in creamy custard', 9, 65.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Chocolate Brownie', 'Rich chocolate brownie with vanilla sauce', 9, 80.00, 1, 1, 0, NOW(), 1);

PRINT 'Added 5 Dessert items';

-- CATEGORY 10: JUICE / BEVERAGES
INSERT INTO t_sys_fooditems (
    c_ownerid, c_foodname, c_description, c_categoryid,
    c_price, c_ispackage_item, c_status, c_is_deleted,
    c_createddate, c_issample_tasted
)
VALUES
    (@TestOwnerId, 'Fresh Lime Soda', 'Refreshing lime soda with mint', 10, 40.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Mango Shake', 'Thick mango milkshake', 10, 60.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Masala Chaas', 'Spiced buttermilk', 10, 35.00, 1, 1, 0, NOW(), 1),
    (@TestOwnerId, 'Virgin Mojito', 'Non-alcoholic mojito with fresh mint', 10, 70.00, 1, 1, 0, NOW(), 1);

PRINT 'Added 4 Beverage items';

-- ========================================
-- 5. ADD FOOD ITEM IMAGES
-- ========================================
-- Add sample food images for better UI display
-- These use placeholder.com images - replace with actual images in production

-- Starters Images
INSERT INTO t_sys_catering_media_uploads (
    c_ownerid, c_reference_id, c_document_type_id,
    c_extension, c_file_name, c_file_path,
    c_is_deleted, c_uploaded_at
)
SELECT
    @TestOwnerId,
    f.c_foodid,
    1, -- Food document type
    '.jpg',
    f.c_foodname + '_1.jpg',
    CASE f.c_foodname
        WHEN 'Paneer Tikka' THEN 'https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400'
        WHEN 'Veg Spring Roll' THEN 'https://images.unsplash.com/photo-1529006557810-274b9b2fc783?w=400'
        WHEN 'Hara Bhara Kabab' THEN 'https://images.unsplash.com/photo-1601050690597-df0568f70950?w=400'
        WHEN 'Aloo Tikki' THEN 'https://images.unsplash.com/photo-1626074353765-517a681e40be?w=400'
        WHEN 'Corn Cheese Balls' THEN 'https://images.unsplash.com/photo-1625937286074-9ca519d5d9df?w=400'
    END,
    0,
    NOW()
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
    AND f.c_categoryid = 1
    AND f.c_ispackage_item = 1;

PRINT 'Added Starter images';

-- Main Course Images
INSERT INTO t_sys_catering_media_uploads (
    c_ownerid, c_reference_id, c_document_type_id,
    c_extension, c_file_name, c_file_path,
    c_is_deleted, c_uploaded_at
)
SELECT
    @TestOwnerId,
    f.c_foodid,
    1,
    '.jpg',
    f.c_foodname + '_1.jpg',
    CASE f.c_foodname
        WHEN 'Paneer Butter Masala' THEN 'https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=400'
        WHEN 'Dal Makhani' THEN 'https://images.unsplash.com/photo-1546833998-877b37c2e5c6?w=400'
        WHEN 'Veg Kolhapuri' THEN 'https://images.unsplash.com/photo-1585032226651-759b368d7246?w=400'
        WHEN 'Kadhai Paneer' THEN 'https://images.unsplash.com/photo-1567188040759-fb8a883dc6d8?w=400'
        WHEN 'Chole Bhature' THEN 'https://images.unsplash.com/photo-1626132647523-66f5bf380027?w=400'
        WHEN 'Mushroom Masala' THEN 'https://images.unsplash.com/photo-1618385288512-26627e15df92?w=400'
    END,
    0,
    NOW()
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
    AND f.c_categoryid = 4
    AND f.c_ispackage_item = 1;

PRINT 'Added Main Course images';

-- Sweet Regular Images
INSERT INTO t_sys_catering_media_uploads (
    c_ownerid, c_reference_id, c_document_type_id,
    c_extension, c_file_name, c_file_path,
    c_is_deleted, c_uploaded_at
)
SELECT
    @TestOwnerId,
    f.c_foodid,
    1,
    '.jpg',
    f.c_foodname + '_1.jpg',
    CASE f.c_foodname
        WHEN 'Gulab Jamun' THEN 'https://images.unsplash.com/photo-1589301773859-bb024d3b9960?w=400'
        WHEN 'Rasgulla' THEN 'https://images.unsplash.com/photo-1606312619070-d48b4cdd6bf4?w=400'
        WHEN 'Jalebi' THEN 'https://images.unsplash.com/photo-1596040033229-a0b4e27b3aae?w=400'
        WHEN 'Rasmalai' THEN 'https://images.unsplash.com/photo-1590301157890-4810ed352733?w=400'
    END,
    0,
    NOW()
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
    AND f.c_categoryid = 7
    AND f.c_ispackage_item = 1;

PRINT 'Added Sweet images';

-- Dessert Images
INSERT INTO t_sys_catering_media_uploads (
    c_ownerid, c_reference_id, c_document_type_id,
    c_extension, c_file_name, c_file_path,
    c_is_deleted, c_uploaded_at
)
SELECT
    @TestOwnerId,
    f.c_foodid,
    1,
    '.jpg',
    f.c_foodname + '_1.jpg',
    CASE f.c_foodname
        WHEN 'Chocolate Ice Cream' THEN 'https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=400'
        WHEN 'Vanilla Ice Cream' THEN 'https://images.unsplash.com/photo-1570197788417-0e82375c9371?w=400'
        WHEN 'Strawberry Ice Cream' THEN 'https://images.unsplash.com/photo-1501443762994-82bd5dace89a?w=400'
        WHEN 'Fruit Custard' THEN 'https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400'
        WHEN 'Chocolate Brownie' THEN 'https://images.unsplash.com/photo-1607920592782-c1e36e7d7b15?w=400'
    END,
    0,
    NOW()
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
    AND f.c_categoryid = 9
    AND f.c_ispackage_item = 1;

PRINT 'Added Dessert images';

-- Beverage Images
INSERT INTO t_sys_catering_media_uploads (
    c_ownerid, c_reference_id, c_document_type_id,
    c_extension, c_file_name, c_file_path,
    c_is_deleted, c_uploaded_at
)
SELECT
    @TestOwnerId,
    f.c_foodid,
    1,
    '.jpg',
    f.c_foodname + '_1.jpg',
    CASE f.c_foodname
        WHEN 'Fresh Lime Soda' THEN 'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400'
        WHEN 'Mango Shake' THEN 'https://images.unsplash.com/photo-1610889556528-9a770e32642f?w=400'
        WHEN 'Masala Chaas' THEN 'https://images.unsplash.com/photo-1623065422902-30a2d299bbe4?w=400'
        WHEN 'Virgin Mojito' THEN 'https://images.unsplash.com/photo-1551538827-9c037cb4f32a?w=400'
    END,
    0,
    NOW()
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
    AND f.c_categoryid = 10
    AND f.c_ispackage_item = 1;

PRINT 'Added Beverage images';

-- ========================================
-- 6. CREATE SOME NON-PACKAGE ITEMS (Add-ons)
-- ========================================
-- These items are NOT included in packages (c_ispackage_item = 0)
-- Users can add these separately as extras

INSERT INTO t_sys_fooditems (
    c_ownerid, c_foodname, c_description, c_categoryid,
    c_price, c_ispackage_item, c_status, c_is_deleted,
    c_createddate, c_issample_tasted
)
VALUES
    (@TestOwnerId, 'Extra Naan (10 pcs)', 'Additional naan bread', 6, 100.00, 0, 1, 0, NOW(), 0),
    (@TestOwnerId, 'Extra Roti (15 pcs)', 'Additional rotis', 6, 80.00, 0, 1, 0, NOW(), 0),
    (@TestOwnerId, 'Special Biryani Rice', 'Premium biryani rice with raita', 5, 250.00, 0, 1, 0, NOW(), 0);

PRINT 'Added 3 Add-on items (non-package items)';

-- ========================================
-- 6. VERIFY DATA
-- ========================================

PRINT '';
PRINT '========================================';
PRINT 'DATA VERIFICATION';
PRINT '========================================';

PRINT 'Owner ID: ' + CAST(@TestOwnerId AS VARCHAR(10));
PRINT 'Package 1 (Wedding): ' + CAST(@PackageId1 AS VARCHAR(10));
PRINT 'Package 2 (Birthday): ' + CAST(@PackageId2 AS VARCHAR(10));

SELECT
    p.c_packageid AS PackageId,
    p.c_packagename AS PackageName,
    COUNT(pi.c_categoryid) AS TotalCategories
FROM t_sys_catering_packages p
LEFT JOIN t_sys_catering_package_items pi ON pi.c_packageid = p.c_packageid
WHERE p.c_ownerid = @TestOwnerId
GROUP BY p.c_packageid, p.c_packagename;

SELECT
    f.c_categoryid AS CategoryId,
    fc.c_categoryname AS CategoryName,
    COUNT(f.c_foodid) AS TotalPackageItems
FROM t_sys_fooditems f
INNER JOIN t_sys_food_category fc ON fc.c_categoryid = f.c_categoryid
WHERE f.c_ownerid = @TestOwnerId
  AND f.c_ispackage_item = 1
  AND f.c_status = 1
  AND f.c_is_deleted = FALSE
GROUP BY f.c_categoryid, fc.c_categoryname
ORDER BY f.c_categoryid;

-- ========================================
-- 7. SAMPLE API TEST QUERY
-- ========================================
-- This is the query the API will execute
-- Replace @PackageId and @CateringId with actual values

PRINT '';
PRINT '========================================';
PRINT 'SAMPLE API QUERY FOR PACKAGE SELECTION';
PRINT '========================================';
PRINT 'Package ID: ' + CAST(@PackageId1 AS VARCHAR(10));
PRINT 'Owner ID: ' + CAST(@TestOwnerId AS VARCHAR(10));
PRINT '';

-- Get Package Info
SELECT
    p.c_packageid AS PackageId,
    p.c_packagename AS PackageName,
    p.c_description AS Description,
    p.c_price AS Price
FROM t_sys_catering_packages p
WHERE p.c_packageid = @PackageId1
  AND p.c_ownerid = @TestOwnerId
  AND p.c_is_active = TRUE
  AND p.c_is_deleted = FALSE;

-- Get Categories with Quantities
SELECT
    pi.c_categoryid AS CategoryId,
    fc.c_categoryname AS CategoryName,
    fc.c_description AS CategoryDescription,
    pi.c_quantity AS AllowedQuantity
FROM t_sys_catering_package_items pi
INNER JOIN t_sys_food_category fc ON fc.c_categoryid = pi.c_categoryid
WHERE pi.c_packageid = @PackageId1
  AND fc.c_is_active = TRUE
ORDER BY fc.c_categoryname;

-- Get Food Items for Category 1 (Starters)
SELECT
    f.c_foodid AS FoodId,
    f.c_foodname AS FoodName,
    f.c_description AS Description,
    f.c_price AS Price
FROM t_sys_fooditems f
WHERE f.c_ownerid = @TestOwnerId
  AND f.c_categoryid = 1
  AND f.c_ispackage_item = 1
  AND f.c_status = 1
  AND f.c_is_deleted = FALSE
ORDER BY f.c_foodname;

PRINT '';
PRINT '========================================';
PRINT 'TEST DATA CREATION COMPLETED!';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Test the API endpoint: GET /api/User/Home/Catering/' + CAST(@TestOwnerId AS VARCHAR(10)) + '/Package/' + CAST(@PackageId1 AS VARCHAR(10)) + '/Selection';
PRINT '2. Open the frontend and test the PackageSelectionModal component';
PRINT '3. Verify that users can select items according to quantity restrictions';
PRINT '';

