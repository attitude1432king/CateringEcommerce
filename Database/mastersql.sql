CREATE TABLE IF NOT EXISTS t_sys_user (
    c_userid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_name VARCHAR(100) NOT NULL,
    c_mobile VARCHAR(15) NOT NULL UNIQUE,
    c_email VARCHAR(100),
    c_password_hash VARCHAR(255),
    c_googleid VARCHAR(255),
    c_isemailverified BOOLEAN,
    c_isphoneverified BOOLEAN,
    c_isactive BOOLEAN,
    c_description TEXT,
    c_stateid INTEGER,
    c_cityid INTEGER,
    c_isblocked BOOLEAN NOT NULL DEFAULT FALSE,
    c_block_reason VARCHAR(500),
    c_last_login TIMESTAMP,
    c_picture TEXT,
    c_deleted_date TIMESTAMP NULL,
    c_deleted_by BIGINT NULL,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    c_createddate TIMESTAMP,
    c_modifieddate TIMESTAMP
);


--------------------------------------------------------------


CREATE TABLE IF NOT EXISTS t_sys_state (
    c_stateid INTEGER PRIMARY KEY,
    c_statename VARCHAR(100) NOT NULL
);

INSERT INTO t_sys_state (c_stateid, c_statename) VALUES
(1, 'Andhra Pradesh'),
(2, 'Arunachal Pradesh'),
(3, 'Assam'),
(4, 'Bihar'),
(5, 'Chhattisgarh'),
(6, 'Goa'),
(7, 'Gujarat'),
(8, 'Haryana'),
(9, 'Himachal Pradesh'),
(10, 'Jharkhand'),
(11, 'Karnataka'),
(12, 'Kerala'),
(13, 'Madhya Pradesh'),
(14, 'Maharashtra'),
(15, 'Manipur'),
(16, 'Meghalaya'),
(17, 'Mizoram'),
(18, 'Nagaland'),
(19, 'Odisha'),
(20, 'Punjab'),
(21, 'Rajasthan'),
(22, 'Sikkim'),
(23, 'Tamil Nadu'),
(24, 'Telangana'),
(25, 'Tripura'),
(26, 'Uttar Pradesh'),
(27, 'Uttarakhand'),
(28, 'West Bengal'),

-- Union Territories
(29, 'Andaman and Nicobar Islands'),
(30, 'Chandigarh'),
(31, 'Dadra and Nagar Haveli and Daman and Diu'),
(32, 'Delhi'),
(33, 'Jammu and Kashmir'),
(34, 'Ladakh'),
(35, 'Lakshadweep'),
(36, 'Puducherry');

----------------------------------------------------

CREATE TABLE IF NOT EXISTS t_sys_city (
    c_cityid INTEGER PRIMARY KEY,
    c_cityname VARCHAR(100) NOT NULL,
    c_stateid INTEGER NOT NULL,
    c_isactive BOOLEAN DEFAULT TRUE,
    c_createdby INTEGER,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifiedby INTEGER,
    c_modifieddate TIMESTAMP
);


INSERT INTO t_sys_city (c_cityid, c_cityname, c_stateid, c_createdby) VALUES
-- Andhra Pradesh (1)
 (101, 'Amaravati', 1, 1),
 (102, 'Visakhapatnam', 1, 1),
 (103, 'Vijayawada', 1, 1),
 (104, 'Tirupati', 1, 1),

-- Arunachal Pradesh (2)
 (201, 'Itanagar', 2, 1),
 (202, 'Tawang', 2, 1),
 (203, 'Bomdila', 2, 1),
 (204, 'Naharlagun', 2, 1),

-- Assam (3)
 (301, 'Dispur', 3, 1),
 (302, 'Guwahati', 3, 1),
 (303, 'Jorhat', 3, 1),
 (304, 'Dibrugarh', 3, 1),

-- Bihar (4)
 (401, 'Patna', 4, 1),
 (402, 'Gaya', 4, 1),
 (403, 'Bhagalpur', 4, 1),
 (404, 'Muzaffarpur', 4, 1),

-- Chhattisgarh (5)
 (501, 'Raipur', 5, 1),
 (502, 'Bilaspur', 5, 1),
 (503, 'Durg', 5, 1),
 (504, 'Korba', 5, 1),

-- Goa (6)
 (601, 'Panaji', 6, 1),
 (602, 'Margao', 6, 1),
 (603, 'Vasco da Gama', 6, 1),
 (604, 'Ponda', 6, 1),

-- Gujarat (7)
 (701, 'Gandhinagar', 7, 1),
 (702, 'Ahmedabad', 7, 1),
 (703, 'Surat', 7, 1),
 (704, 'Vadodara', 7, 1),

-- Haryana (8)
 (801, 'Chandigarh', 8, 1),
 (802, 'Faridabad', 8, 1),
 (803, 'Gurugram', 8, 1),
 (804, 'Panipat', 8, 1),

-- Himachal Pradesh (9)
 (901, 'Shimla', 9, 1),
 (902, 'Manali', 9, 1),
 (903, 'Dharamshala', 9, 1),
 (904, 'Solan', 9, 1),

-- Jharkhand (10)
 (1001, 'Ranchi', 10, 1),
 (1002, 'Dhanbad', 10, 1),
 (1003, 'Jamshedpur', 10, 1),
 (1004, 'Bokaro Steel City', 10, 1),

-- Karnataka (11)
 (1101, 'Bengaluru', 11, 1),
 (1102, 'Mysore', 11, 1),
 (1103, 'Mangalore', 11, 1),
 (1104, 'Hubballi', 11, 1),

-- Kerala (12)
 (1201, 'Thiruvananthapuram', 12, 1),
 (1202, 'Kochi', 12, 1),
 (1203, 'Kozhikode', 12, 1),
 (1204, 'Thrissur', 12, 1),

-- Madhya Pradesh (13)
 (1301, 'Bhopal', 13, 1),
 (1302, 'Indore', 13, 1),
 (1303, 'Jabalpur', 13, 1),
 (1304, 'Gwalior', 13, 1),

-- Maharashtra (14)
 (1401, 'Mumbai', 14, 1),
 (1402, 'Pune', 14, 1),
 (1403, 'Nagpur', 14, 1),
 (1404, 'Nashik', 14, 1),

-- Manipur (15)
 (1501, 'Imphal', 15, 1),
 (1502, 'Churachandpur', 15, 1),
 (1503, 'Ukhrul', 15, 1),
 (1504, 'Thoubal', 15, 1),

-- Meghalaya (16)
 (1601, 'Shillong', 16, 1),
 (1602, 'Tura', 16, 1),
 (1603, 'Jowai', 16, 1),
 (1604, 'Nongpoh', 16, 1),

-- Mizoram (17)
 (1701, 'Aizawl', 17, 1),
 (1702, 'Lunglei', 17, 1),
 (1703, 'Kolasib', 17, 1),
 (1704, 'Saiha', 17, 1),

-- Nagaland (18)
 (1801, 'Kohima', 18, 1),
 (1802, 'Dimapur', 18, 1),
 (1803, 'Mokokchung', 18, 1),
 (1804, 'Tuensang', 18, 1),

-- Odisha (19)
 (1901, 'Bhubaneswar', 19, 1),
 (1902, 'Cuttack', 19, 1),
 (1903, 'Rourkela', 19, 1),
 (1904, 'Puri', 19, 1),

-- Punjab (20)
 (2001, 'Chandigarh', 20, 1),
 (2002, 'Ludhiana', 20, 1),
 (2003, 'Amritsar', 20, 1),
 (2004, 'Jalandhar', 20, 1),

-- Rajasthan (21)
 (2101, 'Jaipur', 21, 1),
 (2102, 'Jodhpur', 21, 1),
 (2103, 'Udaipur', 21, 1),
 (2104, 'Kota', 21, 1),

-- Sikkim (22)
 (2201, 'Gangtok', 22, 1),
 (2202, 'Namchi', 22, 1),
 (2203, 'Geyzing', 22, 1),
 (2204, 'Mangan', 22, 1),

-- Tamil Nadu (23)
 (2301, 'Chennai', 23, 1),
 (2302, 'Coimbatore', 23, 1),
 (2303, 'Madurai', 23, 1),
 (2304, 'Tiruchirappalli', 23, 1),

-- Telangana (24)
 (2401, 'Hyderabad', 24, 1),
 (2402, 'Warangal', 24, 1),
 (2403, 'Karimnagar', 24, 1),
 (2404, 'Nizamabad', 24, 1),

-- Tripura (25)
 (2501, 'Agartala', 25, 1),
 (2502, 'Dharmanagar', 25, 1),
 (2503, 'Udaipur', 25, 1),
 (2504, 'Bishalgarh', 25, 1),

-- Uttar Pradesh (26)
 (2601, 'Lucknow', 26, 1),
 (2602, 'Kanpur', 26, 1),
 (2603, 'Varanasi', 26, 1),
 (2604, 'Agra', 26, 1),

-- Uttarakhand (27)
 (2701, 'Dehradun', 27, 1),
 (2702, 'Haridwar', 27, 1),
 (2703, 'Nainital', 27, 1),
 (2704, 'Rishikesh', 27, 1),

-- West Bengal (28)
 (2801, 'Kolkata', 28, 1),
 (2802, 'Asansol', 28, 1),
 (2803, 'Darjeeling', 28, 1),
 (2804, 'Siliguri', 28, 1),

-- Union Territories
-- Andaman & Nicobar (29)
 (2901, 'Port Blair', 29, 1),

-- Chandigarh (30) -- same as capital
 (3001, 'Chandigarh', 30, 1),

-- Dadra & Nagar Haveli & Daman & Diu (31)
 (3101, 'Silvassa', 31, 1),
 (3102, 'Daman', 31, 1),
 (3103, 'Diu', 31, 1),

-- Delhi (32)
 (3201, 'New Delhi', 32, 1),
 (3202, 'Delhi', 32, 1),

-- Jammu & Kashmir (33)
 (3301, 'Srinagar', 33, 1),
 (3302, 'Jammu', 33, 1),

-- Ladakh (34)
 (3401, 'Leh', 34, 1),
 (3402, 'Kargil', 34, 1),

-- Lakshadweep (35)
 (3501, 'Kavaratti', 35, 1),

-- Puducherry (36)
 (3601, 'Puducherry', 36, 1),
 (3602, 'Karaikal', 36, 1);

-----------------------------------------------


CREATE TABLE IF NOT EXISTS t_sys_catering_owner (
	c_ownerid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_catering_name VARCHAR(200) NOT NULL,
	c_owner_name VARCHAR(200) NOT NULL,
	c_partnernumber VARCHAR(50) NULL,
	c_email VARCHAR(256) NOT NULL,
	c_mobile VARCHAR(15) NOT NULL,
	c_password_hash VARCHAR(512),
	c_catering_number VARCHAR(15) NOT NULL,
	c_std_number VARCHAR(15),
	c_logo_path VARCHAR(500),
	c_same_contact BOOLEAN DEFAULT FALSE,
	c_isactive BOOLEAN DEFAULT TRUE,
	c_email_verified BOOLEAN DEFAULT FALSE,
	c_phone_verified BOOLEAN DEFAULT FALSE,
	c_approval_status INTEGER DEFAULT 1,
	c_approved_date TIMESTAMP,
	c_approved_by BIGINT,
	c_priority INTEGER DEFAULT 1,
	c_isblocked BOOLEAN NOT NULL DEFAULT FALSE,
	c_block_reason VARCHAR(500),
	c_rejection_reason VARCHAR(1000),
	c_internal_notes TEXT,
	c_isfeatured BOOLEAN DEFAULT FALSE,
	c_support_contact_number VARCHAR(15),
	c_alternate_email VARCHAR(256),
	c_whatsapp_number VARCHAR(15),
	c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
	c_reviewed_date TIMESTAMP,
	c_reviewed_by BIGINT,
	c_deleted_by BIGINT,
	c_deleted_date TIMESTAMP,
	c_createddate TIMESTAMP,
	c_modifieddate TIMESTAMP
);



CREATE TABLE IF NOT EXISTS t_sys_catering_owner_addresses (
	c_addressid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_building VARCHAR(50) NOT NULL,
	c_street VARCHAR(100),
	c_area VARCHAR(256),
	c_stateid INTEGER,
	c_cityid INTEGER,
	c_pincode VARCHAR(10) NOT NULL,
	c_latitude VARCHAR(100),
	c_longitude VARCHAR(100),
	c_mapurl VARCHAR(500),
	c_createddate TIMESTAMP,
	c_modifieddate TIMESTAMP
);


CREATE TABLE IF NOT EXISTS t_sys_catering_owner_compliance (
	c_complianceid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_fssai_number VARCHAR(20) NOT NULL,
	c_fssai_expiry_date DATE NOT NULL,
	c_fssai_certificate_path VARCHAR(500) NOT NULL,
	c_gst_applicable BOOLEAN DEFAULT FALSE,
	c_gst_number VARCHAR(20),
	c_gst_certificate_path VARCHAR(500),
	c_pan_name VARCHAR(100) NOT NULL,
	c_pan_number VARCHAR(20) NOT NULL,
	c_pan_file_path VARCHAR(500),
	c_createddate TIMESTAMP,
	c_modifieddate TIMESTAMP
);

CREATE TABLE IF NOT EXISTS t_sys_catering_owner_bankdetails (
	c_bankid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_account_number VARCHAR(30) NOT NULL,
	c_account_holder_name VARCHAR(200) NOT NULL,
	c_ifsc_code VARCHAR(20) NOT NULL,
	c_cheque_path VARCHAR(500),
	c_upi_id VARCHAR(100),
	c_createddate TIMESTAMP,
	c_modifieddate TIMESTAMP
);


CREATE TABLE IF NOT EXISTS t_sys_catering_owner_operations (
	c_operationid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_cuisine_types VARCHAR(500),
	c_service_types VARCHAR(300),
	c_event_types VARCHAR(300),
	c_food_types VARCHAR(300),
	c_min_guest_count INTEGER,
	c_delivery_available BOOLEAN DEFAULT FALSE,
	c_delivery_radius_km INTEGER,
	c_daily_booking_capacity INTEGER,
	c_serving_time_slots VARCHAR(200),
	c_createddate TIMESTAMP,
	c_modifieddate TIMESTAMP
);


CREATE TABLE IF NOT EXISTS t_sys_catering_media_uploads (
	c_media_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_reference_id BIGINT NULL,
	c_document_type_id INTEGER,
	c_extension VARCHAR(20) NOT NULL,
	c_file_name VARCHAR(255) NOT NULL,
	c_file_path TEXT NOT NULL,
	c_is_deleted BOOLEAN DEFAULT FALSE,
	c_uploaded_at TIMESTAMP,
	c_modifieddate TIMESTAMP
);

CREATE TABLE IF NOT EXISTS t_sys_catering_document_types (
    c_document_type_id INTEGER PRIMARY KEY,
    c_document_type VARCHAR(50) NOT NULL,
    c_description VARCHAR(1000),
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE
);


INSERT INTO t_sys_catering_document_types
(c_document_type_id, c_document_type, c_description) VALUES
(0,  'Logo',           'Company logo and brand identity assets'),
(1,  'Food',           'Media related to dishes or menu items'),
(2,  'Kitchen',        'Photos or videos of kitchen and food prep areas'),
(3,  'EventSetup',     'Visuals of event setup, counters, and table arrangements'),
(4,  'Staff',          'Photos or documents related to staff members'),
(5,  'FoodMaking',     'Behind-the-scenes content of food preparation'),
(6,  'Menu',           'Digital or printable catering menus'),
(7,  'Promo',          'Promotional or marketing content'),
(8,  'Banner',         'Banners for website or email campaigns'),
(9,  'Packaging',      'Product packaging used in catering'),
(10, 'ChefProfile',    'Photos and profiles of chefs and kitchen staff'),
(11, 'Recipe',         'Instructions or documentation of recipes'),
(12, 'ClientReview',   'Client feedback, testimonials, or review images'),
(13, 'QuoteTemplate',  'Templates for quotes or pricing documents'),
(14, 'ServiceCatalog', 'Catalog of catering services and packages'),
(15, 'Instruction',    'Usage, serving, or reheating instructions'),
(16, 'Brand',          'Branding materials such as logos and guidelines'),
(17, 'Portfolio',      'Showcase of completed events and past work');


CREATE TABLE IF NOT EXISTS t_sys_catering_type_category (
	c_categoryid INTEGER PRIMARY KEY,
	c_categoryname VARCHAR(100) NOT NULL,
	c_category_code VARCHAR(10) NOT NULL
);


INSERT INTO t_sys_catering_type_category(c_categoryid, c_categoryname, c_category_code) VALUES 

  (1, 'Food Type', 'FT'),
  (2, 'Cuisine Type', 'CT'),
  (3, 'Event Type', 'ET'),
  (4, 'Service Type', 'ST'),
  (5, 'Serving Slots', 'SS');


CREATE TABLE IF NOT EXISTS t_sys_catering_type_master (
	c_typeid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_categoryid INTEGER,
	c_type_name VARCHAR(100),
	c_description VARCHAR(500),
	c_isactive BOOLEAN DEFAULT TRUE,
	c_createdby INTEGER,
	c_createddate TIMESTAMP DEFAULT NOW(),
	c_modifiedby INTEGER,
	c_modifieddate TIMESTAMP
);

INSERT INTO t_sys_catering_type_master ( c_categoryid, c_type_name, c_description, c_createdby) VALUES

	-- Food Type
   (1, 'Vegetarian', 'Pure veg (no eggs/meat/fish)', 1),
   (1, 'Non-Vegetarian', 'Includes meat, poultry, seafood', 1),
   (1, 'Eggetarian', 'Vegetarian with eggs', 1),
   (1, 'Vegan', 'No animal products at all', 1),
   (1, 'Jain Food', 'Strict vegetarian, no root vegetables', 1),
   (1, 'Satvik', 'Ayurvedic-style pure veg, no onion/garlic', 1),
   (1, 'Organic Food', 'Prepared using organic ingredients', 1),
   (1, 'Gluten-Free', 'Safe for gluten allergy', 1),
   (1, 'Keto', 'High-fat, low-carb meals', 1),
   (1, 'Diabetic-Friendly', 'Low sugar, balanced meals', 1),
   
   -- Cuisine Type
	(2, 'South Indian', 'Idli, Dosa, Vada, Sambar, etc.', 1),
	(2, 'North Indian', 'Paneer, Dal, Paratha, Chole, etc.', 1),
	(2, 'Gujarati', 'Thepla, Farsan, Dal-Dhokli, etc.', 1),
	(2, 'Rajasthani', 'Daal Baati, Gatte, etc.',1),
	(2, 'Maharashtrian', 'Poha, Sabudana, Puran Poli, etc.', 1),
	(2, 'Bengali', 'Fish curry, Sweets, etc.', 1),
	(2, 'Punjabi', 'Butter Chicken, Naan, etc.', 1),
	(2, 'Mughlai', 'Rich gravies, kebabs, etc.', 1),
	(2, 'Chinese', 'Chinese style (Manchurian, Fried Rice), etc.', 1),
	(2, 'Continental', 'Pasta, Salad, Sandwich, etc.', 1),
	(2, 'Street Food', 'Chaat, Pav Bhaji, Vada Pav, etc.', 1),
	(2, 'Fusion', 'Indo-western creative dishes', 1),
	
    -- Event Type
	(3, 'Wedding', 'Large-scale, buffet/multi-course for weddings', 1),
	(3, 'Birthday Party', 'Small to medium gatherings', 1),
	(3, 'Office/Corporate', 'Formal meals or snacks for office events', 1),
	(3, 'Festival', 'Themed food for Diwali, Eid, Navratri, etc.', 1),
	(3, 'Housewarming/(Griha Pravesh) ', 'Traditional dishes', 1),
	(3, 'Religious/Pooja', 'Satvik/Jain food for rituals', 1),
	(3, 'Baby Shower', 'Regional sweet and snack items', 1),
	(3, 'Funeral', 'Simple, traditional vegetarian meals', 1),
	
	--Service Type
	(4, 'Buffet Style',     'Self-service from food stations', 1),
	(4, 'Plate Service',    'Table service with plated meals', 1),
	(4, 'Live Counters',    'Food made fresh in front of gues', 1),
	(4, 'Tiffin Services',  'Daily/weekly home delivery meals', 1),
	(4, 'Self-Pickup',      'Customer collects food from kitchen', 1),
	(4, 'Delivery-Only',    'No dine-in, only delivery', 1),
	(4, 'Full-Service',     'Food + staff + setup + clean', 1),
	(4, 'Drop-off',         'Food delivered, no service staff', 1),
	
    -- Serving Slots
	(5, 'Breakfast', 'Morning meal service usually between 7 AM to 10 AM', 1),
	(5, 'Brunch',  'Late morning meal between breakfast and lunch', 1),
	(5, 'Lunch',  'Afternoon meal service usually between 12 PM to 3 PM', 1),
	(5, 'High Tea', 'Light evening snacks served with tea/coffee, 4 PM to 6 PM', 1),
	(5, 'Dinner',  'Evening meal service usually between 7 PM to 10 PM', 1),
	(5, 'Snacks',  'Light food items served anytime outside main meals', 1),
	(5, 'Midnight Meal', 'Late night food service after 10 PM till early morning', 1);


CREATE TABLE t_sys_food_category (
  c_categoryid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,  
  c_categoryname VARCHAR(100) NOT NULL, 
  c_description VARCHAR(500) NULL, 
  c_isactive BOOLEAN NULL, 
  c_is_global BOOLEAN NULL, 
  c_createdby INT NULL, 
  c_createddate TIMESTAMP DEFAULT NOW(),
  c_modifiedby INT NULL, 
  c_modifieddate TIMESTAMP NULL  
);

INSERT INTO t_sys_food_category (c_categoryname, c_description, c_isactive, c_is_global, c_createdby, c_createddate) VALUES

    ('Starter', 'Appetizers served before the main course', TRUE, TRUE, NULL, NOW()),
    ('Soup', 'Various vegetarian and non-vegetarian soups', TRUE, TRUE, NULL, NOW()),
    ('Salad', 'Fresh salads including veg and non-veg options', TRUE, TRUE, NULL, NOW()),
    ('Main Course', 'Primary food items served in the meal', TRUE, TRUE, NULL, NOW()),
    ('Dal & Rice', 'Dal items with rice varieties', TRUE, TRUE, NULL, NOW()),
    ('Roti / Bread', 'Indian breads such as roti, naan, paratha', TRUE, TRUE, NULL, NOW()),
    ('Sweet Regular', 'Standard sweet dishes', TRUE, TRUE, NULL, NOW()),
    ('Sweet Special', 'Premium or special sweet dishes', TRUE, TRUE, NULL, NOW()),
    ('Dessert', 'Ice creams, pastries, and other desserts', TRUE, TRUE, NULL, NOW()),
    ('Juice / Beverages', 'Fruit juices and soft drinks', TRUE, TRUE, NULL, NOW()),
    ('Chat / Street Food', 'Indian street food items', TRUE, TRUE, NULL, NOW()),
    ('Chinese', 'Chinese cuisine items', TRUE, TRUE, NULL, NOW()),
    ('Italian', 'Italian cuisine items', TRUE, TRUE, NULL, NOW()),
    ('Special Items', 'Live counters or special stalls', TRUE, TRUE, NULL, NOW()),
    ('Pan Counter', 'Pan and mouth freshener counters', TRUE, TRUE, NULL, NOW()),
    ('Mocktail / Coffee Bar', 'Mocktails, coffee, tea, and hot beverages', TRUE, TRUE, NULL, NOW()),
    ('Bengali Counter', 'Bengali food special items', TRUE, TRUE, NULL, NOW());


CREATE TABLE t_sys_guest_category (
	c_guest_category_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	c_categoryname VARCHAR(100),
	c_description VARCHAR(500) NULL, 
	c_isactive BOOLEAN DEFAULT TRUE,
    c_createdby INT NULL, 
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifiedby INT NULL, 
    c_modifieddate TIMESTAMP NULL
); 

-- Active + display order (used in UI filtering)
CREATE INDEX IF NOT EXISTS ix_guest_category_active
ON t_sys_guest_category(c_isactive);

-- Fast lookup by category name
CREATE INDEX IF NOT EXISTS ix_guest_category_name
ON t_sys_guest_category(c_categoryname);

-- =============================================
-- 3. Insert Default Data (Safe + Idempotent)
-- =============================================
INSERT INTO t_sys_guest_category 
(c_categoryname, c_description, c_isactive, c_createdby)
VALUES
('50-100 Guests', 'Small gatherings and intimate events', TRUE, 1),
('100-200 Guests', 'Medium-sized events and celebrations', TRUE, 1),
('200-500 Guests', 'Large events and parties', TRUE, 1),
('500-1000 Guests', 'Very large events and corporate functions', TRUE, 1),
('1000+ Guests', 'Mass gatherings and mega events', TRUE, 1);

CREATE TABLE t_map_partner_category (
  c_mapid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,  
  c_ownerid BIGINT NOT NULL, 
  c_categoryid BIGINT NOT NULL, 
  c_is_enabled BOOLEAN NULL, 
  c_createddate TIMESTAMP NULL 
);


CREATE TABLE t_sys_catering_packages (
  c_packageid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
  c_ownerid BIGINT NOT NULL, 
  c_packagename VARCHAR(100) NULL, 
  c_description VARCHAR(1000) NULL, 
  c_price DECIMAL(10, 2) NULL, 
  c_is_active BOOLEAN NULL, 
  c_is_deleted BOOLEAN DEFAULT FALSE,
  c_createddate TIMESTAMP DEFAULT NOW(), 
  c_modifieddate TIMESTAMP NULL 
);

CREATE TABLE t_sys_catering_package_items (
  c_itemid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  c_packageid BIGINT NOT NULL, 
  c_categoryid BIGINT NOT NULL, 
  c_quantity INT NULL, 
  c_createddate TIMESTAMP DEFAULT NOW(), 
  c_modifieddate TIMESTAMP NULL
);



-- Menu Table – stores food items offered by partners
CREATE TABLE t_sys_fooditems (
    c_foodid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,  -- Primary Key
    c_ownerid BIGINT NOT NULL,                  -- FK → Partner
    c_foodname VARCHAR(200) NOT NULL,          -- Name of food item
    c_description TEXT NULL,           -- Description/details
    c_categoryid BIGINT NULL,                 -- Category (Starter, Main, Dessert, etc.)
    c_cuisinetypeid BIGINT NULL,              -- Cuisine type (Indian, Chinese, Italian, etc.)
    c_price DECIMAL(10,2) NOT NULL,             -- Price
    c_ispackage_item BOOLEAN NULL,
    c_isveg BOOLEAN NULL,
    c_islive_counter BOOLEAN DEFAULT FALSE,
    c_status BOOLEAN NULL,
	c_is_deleted BOOLEAN DEFAULT FALSE,
    c_createddate TIMESTAMP NULL, 
    c_modifieddate TIMESTAMP NULL, 
    c_issample_tasted BOOLEAN NOT NULL
);

CREATE TABLE t_sys_catering_decorations (
  c_decoration_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
  c_ownerid BIGINT NOT NULL, 
  c_packageids VARCHAR(100) NULL, 
  c_decoration_name VARCHAR(200) NOT NULL, 
  c_description TEXT NULL, 
  c_theme_id INT NOT NULL, 
  c_price DECIMAL(10, 2) NULL, 
  c_status BOOLEAN NULL, 
  c_is_deleted BOOLEAN DEFAULT FALSE,
  c_createddate TIMESTAMP DEFAULT NOW(),
  c_modifieddate TIMESTAMP NULL
);


CREATE TABLE t_sys_catering_theme_types
(
    c_theme_id    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_theme_name  VARCHAR(150) NOT NULL,
    c_description TEXT NULL,
    c_isactive    BOOLEAN DEFAULT TRUE,
    c_createdby   INT NULL, 
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifiedby  INT NULL, 
    c_modifieddate TIMESTAMP NULL
);

INSERT INTO t_sys_catering_theme_types (c_theme_name, c_description, c_createdby) VALUES

    ('Royal Rajasthani Theme', 'Colorful umbrellas, mirror work, ethnic props, and traditional Rajasthani décor.', 1),
    ('South Indian Temple Theme', 'Banana leaves, brass lamps, and floral garlands inspired by temple décor.', 1),
    ('Gujarati Garba Theme', 'Vibrant bandhani patterns, colorful drapes, and traditional handicrafts.', 1),
    ('Punjabi Dhaba Theme', 'Rustic food stalls, lanterns, charpai seating, and Punjabi folk touches.', 1),
    ('Mughal Nawabi Theme', 'Golden drapes, chandeliers, velvet table covers, and royal feel.', 1),
    ('Kashmiri Theme', 'Soft pastel flowers, phoolon ka chadar, and walnut wood-inspired setup.', 1),
    ('Bengali Traditional Theme', 'Red-white color palette, alpana art, and diyas for elegance.', 1),
    ('Kerala Backwater Theme', 'Coconut leaves, banana leaf counters, and traditional wooden décor.', 1),
    ('Royal Palace Theme', 'Luxurious chandeliers, golden décor, and regal stage design.', 1),
    ('Modern Minimalist Theme', 'Simple elegant décor with neutral tones and sleek counters.', 1),
    ('Crystal & Mirror Theme', 'Mirror counters, chandeliers, and glass textures.', 1),
    ('LED & Neon Theme', 'Glow-in-dark theme with neon borders and LED counters.', 1),
    ('Bohemian Chic Theme', 'Earthy tones, jute, macramé, and simple floral patterns.', 1),
    ('Rustic Wooden Theme', 'Wooden furniture, lanterns, and warm light tones.', 1),
    ('Garden Floral Theme', 'Green foliage walls and full floral counter setup.', 1),
    ('Bollywood Glam Theme', 'Movie posters, spotlights, and celebrity-style design.', 1),
    ('Desi Village Theme', 'Mud hut look, clay pots, hay bales, and folk elements.', 1),
    ('Royal Peacock Theme', 'Blue and green peacock-feather-inspired décor.', 1),
    ('Moroccan Theme', 'Arabic lanterns, lattice patterns, and rich fabric textures.', 1),
    ('Rajwadi Theme', 'Red and gold tone décor with traditional cushions and royal props.', 1),
    ('Banarasi Theme', 'Banarasi silk textures, lotus motifs, and diya lights.', 1),
    ('Wedding Grand Floral Theme', 'Heavy floral canopy and lavish stage background.', 1),
    ('Engagement Elegance Theme', 'Soft pastel tones with crystal lighting.', 1),
    ('Birthday Carnival Theme', 'Colorful backdrops and joyful props for family functions.', 1),
    ('Corporate Elegant Theme', 'Professional neutral design with minimal decoration.', 1),
    ('Festive Diwali Theme', 'Marigolds, diyas, and golden illumination.', 1),
    ('Navratri Theme', 'Multicolor drapes and goddess-inspired motifs.', 1),
    ('Christmas Winter Wonderland', 'White drapes, snow theme, and crystal lights.', 1),
    ('Mocktail Bar Theme', 'LED bottle display and vibrant bar setup.', 1),
    ('Fruit Counter Theme', 'Fresh fruit art, natural décor, and display design.', 1),
    ('Chaat Counter Theme', 'Street-food style chaat counter with colorful signage.', 1),
    ('South Indian Counter Theme', 'Banana leaf setup and brass utensil decoration.', 1),
    ('Live Tandoor BBQ Theme', 'Smoke-effect grill counter with rustic appearance.', 1),
    ('Dessert Counter Theme', 'Soft pastel setup with ice blue or chocolate tone décor.', 1);


CREATE TABLE t_sys_catering_staff (
  c_staffid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,  
  c_ownerid BIGINT NOT NULL, 
  c_fullname VARCHAR(150) NOT NULL, 
  c_gender VARCHAR(10) NOT NULL, 
  c_contact_number VARCHAR(15) NOT NULL, 
  c_role VARCHAR(100) NOT NULL, 
  c_other_role VARCHAR(100) NULL, 
  c_expertise_categoryid BIGINT NULL, 
  c_experience_years DECIMAL(4, 1) NULL, 
  c_salary_type VARCHAR(20) NOT NULL, 
  c_salary_amount DECIMAL(10, 2) NOT NULL, 
  c_profile_path VARCHAR(500) NULL, 
  c_identity_doc_path VARCHAR(500) NULL, 
  c_resume_doc_path VARCHAR(500) NULL, 
  c_availability BOOLEAN NULL, 
  c_is_deleted BOOLEAN DEFAULT FALSE,
  c_createddate TIMESTAMP DEFAULT NOW(), 
  c_modifieddate TIMESTAMP NULL 
);


CREATE TABLE t_sys_catering_review (
    c_reviewid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    -- Foreign Keys
    c_ownerid BIGINT NOT NULL,                  -- FK → Catering Owner
    c_userid BIGINT NOT NULL,                   -- FK → User who gave review
    c_orderid BIGINT NOT NULL,                  -- FK → Catering Order/Event

    -- Rating Fields (multi-dimensional)
    c_overall_rating DECIMAL(2,1) NOT NULL,     -- e.g., 4.5
    c_food_quality_rating DECIMAL(2,1) NULL,
    c_hygiene_rating DECIMAL(2,1) NULL,
    c_staff_behavior_rating DECIMAL(2,1) NULL,
    c_decoration_rating DECIMAL(2,1) NULL,
    c_punctuality_rating DECIMAL(2,1) NULL,

    -- Review Content
    c_review_title VARCHAR(200) NULL,
    c_review_comment TEXT NULL,
    c_owner_reply TEXT NULL,          -- Owner response to review
    c_owner_reply_date TIMESTAMP NULL,           -- When owner replied

    -- Review Status & Moderation
    c_is_verified BOOLEAN DEFAULT TRUE,                -- Verified order-based review
    c_is_visible BOOLEAN DEFAULT TRUE,                 -- Visible to customers
    c_is_reported BOOLEAN DEFAULT FALSE,                -- Reported by vendor/user
    c_ishidden BOOLEAN NOT NULL DEFAULT FALSE,          -- Admin hidden flag
    c_hidden_reason VARCHAR(500) NULL,         -- Why review was hidden
    c_hidden_by BIGINT NULL,                    -- Admin who hid review
    c_hidden_date TIMESTAMP NULL,                -- When review was hidden
    c_admin_status VARCHAR(20) DEFAULT 'Approved', 
        -- Approved / Pending / Rejected

    -- Metadata
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL
);

-- Ensure Owner Review Reply table exists for owner responses
CREATE TABLE IF NOT EXISTS t_sys_owner_review_reply (
    c_replyid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_reviewid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_reply_text VARCHAR(1000) NOT NULL,
    c_reply_date TIMESTAMP DEFAULT NOW(),
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL,

    CONSTRAINT fk_review_reply_review FOREIGN KEY (c_reviewid)
        REFERENCES t_sys_catering_review (c_reviewid) ON DELETE CASCADE,
    CONSTRAINT fk_review_reply_owner FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner (c_ownerid)
);

-- Create index for better performance on review queries
CREATE INDEX IF NOT EXISTS ix_review_ownerid_visible
ON t_sys_catering_review (c_ownerid, c_is_visible, c_ishidden);

-- Create index for user's reviews
CREATE INDEX IF NOT EXISTS ix_review_userid
ON t_sys_catering_review (c_userid);

-- Create index for order reviews
CREATE INDEX IF NOT EXISTS ix_review_orderid
ON t_sys_catering_review (c_orderid);


CREATE TABLE IF NOT EXISTS t_sys_catering_discount (
    c_discountid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    c_discount_name VARCHAR(200) NOT NULL,
    c_discount_description TEXT,

    c_discount_type INT NOT NULL,   -- Enum
    c_discount_mode INT NOT NULL,   -- Enum

    c_discount_value NUMERIC(10,2) NOT NULL,
    c_min_order_value NUMERIC(10,2),
    c_max_discount_value NUMERIC(10,2),

    c_discount_code VARCHAR(50) NOT NULL UNIQUE,

    c_max_uses_per_order INT DEFAULT 1,
    c_max_uses_per_user INT DEFAULT 1,
    c_is_stackable BOOLEAN DEFAULT FALSE,

    c_startdate DATE NOT NULL,
    c_enddate DATE NOT NULL,

    c_isactive BOOLEAN DEFAULT TRUE,
    c_isautodisable BOOLEAN DEFAULT TRUE,
    c_status INT DEFAULT 1, -- 1=Active,2=Expired,3=Disabled

    c_is_deleted BOOLEAN DEFAULT FALSE,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_updateddate TIMESTAMP
);



CREATE TABLE t_map_discount_fooditem (
    c_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_discountid BIGINT NOT NULL,
    c_foodid BIGINT NOT NULL,
	c_isactive BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE t_map_discount_package (
    c_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_discountid BIGINT NOT NULL,
    c_packageid BIGINT NOT NULL,
	c_isactive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Global Status Table
CREATE TABLE t_catering_availability_global (
    c_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_global_status INT NOT NULL DEFAULT 1 CHECK (c_global_status IN (1, 2)),
    c_closure_reason VARCHAR(255) NULL,
    c_modifieddate TIMESTAMP DEFAULT NOW()
);

-- Date-Specific Availability Table
CREATE TABLE t_catering_availability_dates (
    c_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_date DATE NOT NULL,
    c_status INT NOT NULL CHECK (c_status IN (1, 2, 3)),
    c_note VARCHAR(255) NULL,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS idx_availability_dates_range
ON t_catering_availability_dates (c_ownerid, c_date)
INCLUDE (c_status);


-- Banner Management Table
CREATE TABLE t_sys_catering_banners (
    c_bannerid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_title VARCHAR(200) NOT NULL,
    c_description TEXT NULL,
    c_link_url VARCHAR(500) NULL,
    c_display_order INT NOT NULL DEFAULT 0,
    c_isactive BOOLEAN NOT NULL DEFAULT TRUE,
    c_start_date TIMESTAMP NULL,
    c_end_date TIMESTAMP NULL,
    c_click_count INT NOT NULL DEFAULT 0,
    c_view_count INT NOT NULL DEFAULT 0,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL
);

-- Index for better query performance
CREATE INDEX IX_Banners_Owner_Active
ON t_sys_catering_banners (c_ownerid, c_isactive, c_display_order)
WHERE c_is_deleted = FALSE;

------------------------------------------ Below Tables are executing is pending ---- --------------------------------

CREATE TABLE t_sys_catering_discount_usage (
    c_usage_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    -- Discount Reference
    c_discount_id BIGINT NOT NULL,
    c_discount_code VARCHAR(50) NOT NULL,

    -- Who used it
    c_ownerid BIGINT NOT NULL,        -- Catering Owner
    c_userid BIGINT NOT NULL,         -- Customer
    c_orderid BIGINT NOT NULL,        -- Catering Order/Event

    -- Usage Details
    c_discount_amount DECIMAL(10,2) NOT NULL,
    c_discount_type INT NOT NULL,     -- Enum snapshot
    c_discount_mode INT NOT NULL,     -- Enum snapshot

    -- Status
    c_usage_status INT DEFAULT 1,
    -- 1 = Applied
    -- 2 = RolledBack (payment failed)
    -- 3 = Cancelled (order cancelled)

    -- Audit
    c_used_at TIMESTAMP DEFAULT NOW()
);

--optional: Coupon Table – discounts & promotions

-- Homepage Statistics Table (for caching homepage stats)
CREATE TABLE t_sys_homepage_stats (
    c_statsid INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_total_events_catered INT NOT NULL DEFAULT 0,
    c_total_catering_partners INT NOT NULL DEFAULT 0,
    c_total_happy_customers INT NOT NULL DEFAULT 0,
    c_satisfaction_rate DECIMAL(5,2) NOT NULL DEFAULT 0,
    c_last_updated TIMESTAMP DEFAULT NOW()
);

-- Insert initial homepage stats record
INSERT INTO t_sys_homepage_stats (c_total_events_catered, c_total_catering_partners, c_total_happy_customers, c_satisfaction_rate, c_last_updated)
VALUES (5000, 500, 50000, 98.00, NOW());


-- Create Partner Agreement Table
CREATE TABLE IF NOT EXISTS t_sys_catering_owner_agreement (
    c_agreementid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_agreement_text TEXT NOT NULL,
    c_agreement_accepted BOOLEAN NOT NULL DEFAULT FALSE,
    c_signature_data TEXT NULL, -- Base64 encoded signature image
    c_signature_path VARCHAR(500) NULL, -- File path to signature image
    c_agreement_pdf_path VARCHAR(500) NULL, -- File path to generated agreement PDF
    c_ip_address VARCHAR(50) NULL,
    c_user_agent VARCHAR(500) NULL,
    c_accepted_date TIMESTAMP NULL,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL,
    CONSTRAINT fk_agreement_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);


-- Notification Templates Table
CREATE TABLE t_notification_templates (
    c_template_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_template_code VARCHAR(100) NOT NULL UNIQUE,
    c_channel VARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP
    c_audience VARCHAR(20) NOT NULL, -- ADMIN, PARTNER, USER, ALL
    c_category VARCHAR(50) NOT NULL, -- OTP, ORDER_CONFIRMATION, etc.

    c_name VARCHAR(200) NOT NULL,
    c_description VARCHAR(500),

    -- Template Content
    c_subject VARCHAR(500) NULL, -- For EMAIL only
    c_body TEXT NOT NULL,
    c_text_body TEXT NULL, -- Plain text version for EMAIL

    -- Metadata
    c_language VARCHAR(10) NOT NULL DEFAULT 'en', -- en, hi, es, etc.
    c_version INT NOT NULL DEFAULT 1,
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,
    c_is_default BOOLEAN NOT NULL DEFAULT FALSE, -- Default template for this code

    -- Placeholders
    c_placeholders TEXT, -- JSON array of placeholder names
    c_sample_data TEXT, -- JSON object with sample data for preview

    -- Audit
    c_createdby VARCHAR(100),
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_updated_by VARCHAR(100),
    c_modifieddate TIMESTAMP DEFAULT NOW(),
    c_last_used_date TIMESTAMP NULL,
    c_usage_count INT DEFAULT 0,

    -- Constraints
    CONSTRAINT CK_template_channel CHECK (c_channel IN ('EMAIL', 'SMS', 'INAPP')),
    CONSTRAINT CK_template_audience CHECK (c_audience IN ('ADMIN', 'PARTNER', 'USER', 'ALL')),
    CONSTRAINT CK_template_language CHECK (c_language IN ('en', 'hi', 'es', 'fr', 'de'))
);

-- Indexes
CREATE INDEX IX_templates_code_language ON t_notification_templates(c_template_code, c_language, c_is_active);
CREATE INDEX IX_templates_channel_audience ON t_notification_templates(c_channel, c_audience);
CREATE INDEX IX_templates_active ON t_notification_templates(c_is_active);

-- Template Version History
CREATE TABLE t_notification_template_versions (
    c_version_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_template_id BIGINT NOT NULL,
    c_version INT NOT NULL,
    c_subject VARCHAR(500),
    c_body TEXT NOT NULL,
    c_text_body TEXT,
    c_changed_by VARCHAR(100),
    c_changed_date TIMESTAMP DEFAULT NOW(),
    c_change_notes VARCHAR(500),

    CONSTRAINT FK_template_version_template FOREIGN KEY (c_template_id)
        REFERENCES t_notification_templates(c_template_id)
);

CREATE INDEX IX_template_versions ON t_notification_template_versions(c_template_id, c_version);

CREATE TABLE IF NOT EXISTS t_inapp_notifications (
    c_notification_id VARCHAR(50) PRIMARY KEY,
    c_userid VARCHAR(50) NOT NULL,
    c_user_type VARCHAR(20) NOT NULL, -- ADMIN, PARTNER, USER

    c_title VARCHAR(200) NOT NULL,
    c_message VARCHAR(1000) NOT NULL,
    c_category VARCHAR(50),
    c_priority INT DEFAULT 5,

    c_action_url VARCHAR(500),
    c_icon_url VARCHAR(500),
    c_data JSONB, -- better for PostgreSQL

    c_is_read BOOLEAN DEFAULT FALSE,
    c_read_at TIMESTAMP NULL,

    c_createddate TIMESTAMP DEFAULT NOW(),
    c_expires_at TIMESTAMP NULL
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_inapp_user_unread 
ON t_inapp_notifications (c_userid, c_user_type, c_is_read);

CREATE INDEX IF NOT EXISTS idx_inapp_created 
ON t_inapp_notifications (c_createddate DESC);

CREATE TABLE IF NOT EXISTS t_notification_delivery_log (
    c_delivery_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_notification_id VARCHAR(50) NOT NULL,
    c_channel VARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP

    c_recipient VARCHAR(200) NOT NULL, -- Email/Phone/UserId
    c_status VARCHAR(20) NOT NULL, -- QUEUED, SENT, DELIVERED, FAILED, BOUNCED

    c_provider VARCHAR(50),
    c_provider_message_id VARCHAR(200),

    c_sent_at TIMESTAMP NULL,
    c_delivered_at TIMESTAMP NULL,
    c_opened_at TIMESTAMP NULL,
    c_clicked_at TIMESTAMP NULL,

    c_error_message TEXT,
    c_retry_count INT DEFAULT 0,
    c_cost NUMERIC(10,4) DEFAULT 0,

    c_createddate TIMESTAMP DEFAULT NOW()
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_delivery_notification 
ON t_notification_delivery_log (c_notification_id);

CREATE INDEX IF NOT EXISTS idx_delivery_status 
ON t_notification_delivery_log (c_channel, c_status);

CREATE INDEX IF NOT EXISTS idx_delivery_created 
ON t_notification_delivery_log (c_createddate DESC);

-- Notification Queue (for debugging/retry)
CREATE TABLE IF NOT EXISTS t_notification_queue (
    c_queue_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_message_id VARCHAR(50) NOT NULL UNIQUE,
    c_routing_key VARCHAR(200) NOT NULL,

    c_payload JSONB NOT NULL, -- JSON optimized for PostgreSQL
    c_priority INT DEFAULT 5,

    c_status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, PROCESSING, COMPLETED, FAILED
    c_retry_count INT DEFAULT 0,
    c_max_retries INT DEFAULT 3,

    c_next_retry_at TIMESTAMP NULL,
    c_error_message TEXT,

    c_enqueued_at TIMESTAMP DEFAULT NOW(),
    c_processed_at TIMESTAMP NULL
);

-- Index
CREATE INDEX IF NOT EXISTS idx_queue_status 
ON t_notification_queue (c_status, c_next_retry_at);

-- Template Usage Statistics
CREATE TABLE IF NOT EXISTS t_notification_template_stats (
    c_stat_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_template_id BIGINT NOT NULL,
    c_date DATE NOT NULL,
    c_channel VARCHAR(20) NOT NULL,

    c_sent_count INT DEFAULT 0,
    c_delivered_count INT DEFAULT 0,
    c_failed_count INT DEFAULT 0,
    c_opened_count INT DEFAULT 0,
    c_clicked_count INT DEFAULT 0,

    c_total_cost NUMERIC(10, 2) DEFAULT 0,

    CONSTRAINT fk_template_stats_template 
        FOREIGN KEY (c_template_id)
        REFERENCES t_notification_templates(c_template_id),

    CONSTRAINT uq_template_stats UNIQUE (c_template_id, c_date, c_channel)
);

-- Index
CREATE INDEX IF NOT EXISTS idx_template_stats_date 
ON t_notification_template_stats (c_date DESC);


/* =============================================
   Owner Support Tickets Migration
   Creates t_sys_support_tickets table
   and t_sys_support_ticket_messages table
   ============================================= */

CREATE TABLE IF NOT EXISTS t_sys_support_tickets (
    c_ticket_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ticket_number VARCHAR(20) NOT NULL,
    c_ownerid BIGINT NOT NULL,

    -- Ticket Info
    c_subject VARCHAR(200) NOT NULL,
    c_description VARCHAR(2000) NOT NULL,
    c_category VARCHAR(50) NOT NULL,
    c_priority VARCHAR(20) NOT NULL DEFAULT 'Medium',
    c_status VARCHAR(20) NOT NULL DEFAULT 'Open',

    -- Related entities (optional)
    c_related_order_id BIGINT NULL,

    -- Resolution
    c_resolved_by VARCHAR(100) NULL,
    c_resolution_notes VARCHAR(2000) NULL,
    c_resolved_date TIMESTAMP NULL,

    -- Metadata
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL,

    CONSTRAINT uq_ticket_number UNIQUE (c_ticket_number)
);

CREATE INDEX IF NOT EXISTS ix_support_tickets_ownerid ON t_sys_support_tickets (c_ownerid);
CREATE INDEX IF NOT EXISTS ix_support_tickets_status ON t_sys_support_tickets (c_status);
CREATE INDEX IF NOT EXISTS ix_support_tickets_category ON t_sys_support_tickets (c_category);
CREATE INDEX IF NOT EXISTS ix_support_tickets_createddate ON t_sys_support_tickets (c_createddate DESC);

CREATE TABLE IF NOT EXISTS t_sys_support_ticket_messages (
    c_message_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ticket_id BIGINT NOT NULL,
    c_sender_type VARCHAR(20) NOT NULL,
    c_sender_id BIGINT NOT NULL,
    c_message_text VARCHAR(2000) NOT NULL,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_ticket_messages_ticket FOREIGN KEY (c_ticket_id)
        REFERENCES t_sys_support_tickets (c_ticket_id)
);

CREATE INDEX IF NOT EXISTS ix_ticket_messages_ticketid ON t_sys_support_ticket_messages (c_ticket_id);

-- ============================================================
-- Contact Messages Migration
-- Creates t_sys_contact_messages for user-facing contact form
-- ============================================================

CREATE TABLE IF NOT EXISTS t_sys_contact_messages (
    c_messageid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_name VARCHAR(100) NOT NULL,
    c_email VARCHAR(256) NOT NULL,
    c_message VARCHAR(2000) NOT NULL,
    c_status VARCHAR(20) NOT NULL DEFAULT 'New',   -- New | Read | Replied
    c_ip_address VARCHAR(50) NULL,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS ix_contact_messages_status
ON t_sys_contact_messages (c_status, c_createddate DESC);

