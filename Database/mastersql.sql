CREATE TABLE t_sys_user (
    c_userid BIGINT IDENTITY(1,1) PRIMARY KEY, 
    c_name NVARCHAR(100) NOT NULL,
    c_mobile VARCHAR(15) NOT NULL UNIQUE,
    c_email NVARCHAR(100) NULL,
    c_password_hash NVARCHAR(255) NULL,         -- Optional (if using password login)
    c_googleid NVARCHAR(255) NULL,             -- Optional for Google login
    c_isemailverified BIT NULL, 
    c_isphoneverified BIT NULL,                -- 1 if OTP verified
    c_isactive BIT NULL,                -- 1 if OTP verified
    c_description NVARCHAR(4000) NULL, 
    c_stateid INT NULL,
    c_cityid INT NULL, 
    c_picture NVARCHAR(MAX) NULL,
    c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);


--------------------------------------------------------------


CREATE TABLE t_sys_state (
    c_stateid INT PRIMARY KEY,
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

CREATE TABLE t_sys_city (
  c_cityid   INT PRIMARY KEY,
  c_cityname VARCHAR(100) NOT NULL,
  c_stateid  INT NOT NULL
);


INSERT INTO t_sys_city (c_cityid, c_cityname, c_stateid) VALUES
-- Andhra Pradesh (1)
 (101, 'Amaravati', 1),
 (102, 'Visakhapatnam', 1),
 (103, 'Vijayawada', 1),
 (104, 'Tirupati', 1),

-- Arunachal Pradesh (2)
 (201, 'Itanagar', 2),
 (202, 'Tawang', 2),
 (203, 'Bomdila', 2),
 (204, 'Naharlagun', 2),

-- Assam (3)
 (301, 'Dispur', 3),
 (302, 'Guwahati', 3),
 (303, 'Jorhat', 3),
 (304, 'Dibrugarh', 3),

-- Bihar (4)
 (401, 'Patna', 4),
 (402, 'Gaya', 4),
 (403, 'Bhagalpur', 4),
 (404, 'Muzaffarpur', 4),

-- Chhattisgarh (5)
 (501, 'Raipur', 5),
 (502, 'Bilaspur', 5),
 (503, 'Durg', 5),
 (504, 'Korba', 5),

-- Goa (6)
 (601, 'Panaji', 6),
 (602, 'Margao', 6),
 (603, 'Vasco da Gama', 6),
 (604, 'Ponda', 6),

-- Gujarat (7)
 (701, 'Gandhinagar', 7),
 (702, 'Ahmedabad', 7),
 (703, 'Surat', 7),
 (704, 'Vadodara', 7),

-- Haryana (8)
 (801, 'Chandigarh', 8),
 (802, 'Faridabad', 8),
 (803, 'Gurugram', 8),
 (804, 'Panipat', 8),

-- Himachal Pradesh (9)
 (901, 'Shimla', 9),
 (902, 'Manali', 9),
 (903, 'Dharamshala', 9),
 (904, 'Solan', 9),

-- Jharkhand (10)
 (1001, 'Ranchi', 10),
 (1002, 'Dhanbad', 10),
 (1003, 'Jamshedpur', 10),
 (1004, 'Bokaro Steel City', 10),

-- Karnataka (11)
 (1101, 'Bengaluru', 11),
 (1102, 'Mysore', 11),
 (1103, 'Mangalore', 11),
 (1104, 'Hubballi', 11),

-- Kerala (12)
 (1201, 'Thiruvananthapuram', 12),
 (1202, 'Kochi', 12),
 (1203, 'Kozhikode', 12),
 (1204, 'Thrissur', 12),

-- Madhya Pradesh (13)
 (1301, 'Bhopal', 13),
 (1302, 'Indore', 13),
 (1303, 'Jabalpur', 13),
 (1304, 'Gwalior', 13),

-- Maharashtra (14)
 (1401, 'Mumbai', 14),
 (1402, 'Pune', 14),
 (1403, 'Nagpur', 14),
 (1404, 'Nashik', 14),

-- Manipur (15)
 (1501, 'Imphal', 15),
 (1502, 'Churachandpur', 15),
 (1503, 'Ukhrul', 15),
 (1504, 'Thoubal', 15),

-- Meghalaya (16)
 (1601, 'Shillong', 16),
 (1602, 'Tura', 16),
 (1603, 'Jowai', 16),
 (1604, 'Nongpoh', 16),

-- Mizoram (17)
 (1701, 'Aizawl', 17),
 (1702, 'Lunglei', 17),
 (1703, 'Kolasib', 17),
 (1704, 'Saiha', 17),

-- Nagaland (18)
 (1801, 'Kohima', 18),
 (1802, 'Dimapur', 18),
 (1803, 'Mokokchung', 18),
 (1804, 'Tuensang', 18),

-- Odisha (19)
 (1901, 'Bhubaneswar', 19),
 (1902, 'Cuttack', 19),
 (1903, 'Rourkela', 19),
 (1904, 'Puri', 19),

-- Punjab (20)
 (2001, 'Chandigarh', 20),
 (2002, 'Ludhiana', 20),
 (2003, 'Amritsar', 20),
 (2004, 'Jalandhar', 20),

-- Rajasthan (21)
 (2101, 'Jaipur', 21),
 (2102, 'Jodhpur', 21),
 (2103, 'Udaipur', 21),
 (2104, 'Kota', 21),

-- Sikkim (22)
 (2201, 'Gangtok', 22),
 (2202, 'Namchi', 22),
 (2203, 'Geyzing', 22),
 (2204, 'Mangan', 22),

-- Tamil Nadu (23)
 (2301, 'Chennai', 23),
 (2302, 'Coimbatore', 23),
 (2303, 'Madurai', 23),
 (2304, 'Tiruchirappalli', 23),

-- Telangana (24)
 (2401, 'Hyderabad', 24),
 (2402, 'Warangal', 24),
 (2403, 'Karimnagar', 24),
 (2404, 'Nizamabad', 24),

-- Tripura (25)
 (2501, 'Agartala', 25),
 (2502, 'Dharmanagar', 25),
 (2503, 'Udaipur', 25),
 (2504, 'Bishalgarh', 25),

-- Uttar Pradesh (26)
 (2601, 'Lucknow', 26),
 (2602, 'Kanpur', 26),
 (2603, 'Varanasi', 26),
 (2604, 'Agra', 26),

-- Uttarakhand (27)
 (2701, 'Dehradun', 27),
 (2702, 'Haridwar', 27),
 (2703, 'Nainital', 27),
 (2704, 'Rishikesh', 27),

-- West Bengal (28)
 (2801, 'Kolkata', 28),
 (2802, 'Asansol', 28),
 (2803, 'Darjeeling', 28),
 (2804, 'Siliguri', 28),

-- Union Territories
-- Andaman & Nicobar (29)
 (2901, 'Port Blair', 29),

-- Chandigarh (30) -- same as capital
 (3001, 'Chandigarh', 30),

-- Dadra & Nagar Haveli & Daman & Diu (31)
 (3101, 'Silvassa', 31),
 (3102, 'Daman', 31),
 (3103, 'Diu', 31),

-- Delhi (32)
 (3201, 'New Delhi', 32),
 (3202, 'Delhi', 32),

-- Jammu & Kashmir (33)
 (3301, 'Srinagar', 33),
 (3302, 'Jammu', 33),

-- Ladakh (34)
 (3401, 'Leh', 34),
 (3402, 'Kargil', 34),

-- Lakshadweep (35)
 (3501, 'Kavaratti', 35),

-- Puducherry (36)
 (3601, 'Puducherry', 36),
 (3602, 'Karaikal', 36);

-----------------------------------------------


CREATE TABLE t_sys_catering_owner (
    c_ownerid BIGINT IDENTITY(1,1) PRIMARY KEY,
     -- Business Info
	c_catering_name NVARCHAR(200) NOT NULL,
    c_owner_name NVARCHAR(200) NOT NULL,
    c_email NVARCHAR(256) NOT NULL,
    c_mobile NVARCHAR(15) NOT NULL,
    c_password_hash NVARCHAR(512) NULL, -- Store encrypted password hash
	c_catering_number NVARCHAR(15) NOT NULL,
	c_std_number NVARCHAR(15) NULL,
	c_logo_path NVARCHAR(500),
    -- Verification Flags
    c_same_contact BIT DEFAULT 0,
    c_isactive BIT DEFAULT 1,
    c_email_verified BIT DEFAULT 0,
    c_phone_verified BIT DEFAULT 0,
    c_verified_by_admin BIT DEFAULT 0,
	c_isonline BIT DEFAULT 0,
	c_isfeatured BIT DEFAULT 0, -- Featured on homepage
	-- Contact / Support
    c_support_contact_number NVARCHAR(15) NULL,
    c_alternate_email NVARCHAR(256) NULL,
    c_whatsapp_number NVARCHAR(15) NULL,
	c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);



CREATE TABLE t_sys_catering_owner_addresses (
    c_addressid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_building NVARCHAR(50) NOT NULL,
    c_street NVARCHAR(100) NULL,
	c_area NVARCHAR(256) NULL,
	c_stateid INT NULL,
    c_cityid INT NULL,
    c_pincode NVARCHAR(10) NOT NULL,
    c_latitude NVARCHAR(100) NULL,
    c_longitude NVARCHAR(100) NULL,
    c_mapurl NVARCHAR(500),
	c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);


CREATE TABLE t_sys_catering_owner_compliance (
    c_complianceid BIGINT IDENTITY(1,1) PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
    c_fssai_number NVARCHAR(20) NOT NULL,
    c_fssai_expiry_date DATE NOT NULL,
    c_fssai_certificate_path NVARCHAR(500) NOT NULL,
    c_gst_applicable BIT DEFAULT 0,
    c_gst_number NVARCHAR(20),
    c_gst_certificate_path NVARCHAR(500),
	c_pan_name NVARCHAR(100) NOT NULL,
    c_pan_number NVARCHAR(20) NOT NULL,
    c_pan_file_path NVARCHAR(500),
	c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);

CREATE TABLE t_sys_catering_owner_bankdetails (
    c_bankid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_account_number NVARCHAR(30) NOT NULL,
    c_account_holder_name NVARCHAR(200) NOT NULL,
    c_ifsc_code NVARCHAR(20) NOT NULL,
    c_cheque_path NVARCHAR(500),
    c_upi_id NVARCHAR(100),
	c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);


CREATE TABLE t_sys_catering_owner_operations (
    c_operationid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_cuisine_types NVARCHAR(500), -- comma-separated or JSON
    c_service_types NVARCHAR(300), -- comma-separated or JSON
	c_event_types NVARCHAR(300), -- comma-separated or JSON
	c_food_types NVARCHAR(300), -- comma-separated or JSON
    c_min_dish_order DECIMAL(10, 2),
    c_delivery_available BIT DEFAULT 0,
    c_delivery_radius_km INT,
    c_serving_time_slots NVARCHAR(200), -- e.g., "Breakfast,Lunch"
	c_createddate DATETIME NULL,
    c_modifieddate DATETIME NULL
);	


CREATE TABLE t_sys_catering_media_uploads (
    c_media_id BIGINT IDENTITY(1,1) PRIMARY KEY,
	c_ownerid BIGINT NOT NULL,
	c_reference_id BIGINT NOT NULL,
	c_document_type_id INT NULL, 
	c_extension NVARCHAR(20) NOT NULL,
    c_file_name VARCHAR(255) NOT NULL,
    c_file_path TEXT NOT NULL,
    c_document_type_id INT,
	c_is_deleted BIT DEFAULT 0,
    c_uploaded_at DATETIME NULL,
	c_updated_at DATETIME NULL,	
);

CREATE TABLE t_sys_catering_document_types (
    c_document_type_id INT PRIMARY KEY,                         -- Unique identifier
    c_document_type NVARCHAR(50) NOT NULL,                      -- PascalCase or friendly name (e.g., 'Food', 'Menu')
    c_description NVARCHAR(1000) NULL,                          -- Optional description (e.g., 'Visuals of food items')
    c_is_active BIT NOT NULL DEFAULT 1                          -- Logical delete / active status flag
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


	


CREATE TABLE t_sys_catering_type_category (
	c_category_id INT PRIMARY KEY,
	c_category_name NVARCHAR(100) NOT NULL,  -- e.g., "Food Preference", "Event Type"
	c_category_code NVARCHAR(10) NOT NULL   -- Optional: for filtering ("food", "event", etc.)
);


INSERT INTO t_sys_catering_type_category(c_category_id, c_category_name, c_category_code) VALUES 

  (1, 'Food Type', 'FT'),
  (2, 'Cuisine Type', 'CT'),
  (3, 'Event Type', 'ET'),
  (4, 'Service Type', 'ST')
  (5, 'Serving Slots', 'SS');



CREATE TABLE t_sys_catering_type_master (
	c_type_id INT IDENTITY(1,1) PRIMARY KEY,
	c_category_id INT,
	c_type_name NVARCHAR(100),
	c_description NVARCHAR(1000) NULL, 
	c_is_active BIT DEFAULT 1
);

INSERT INTO t_sys_catering_type_master ( c_category_id, c_type_name, c_description) VALUES

	-- Food Type
   (1, 'Vegetarian', 'Pure veg (no eggs/meat/fish)'),
   (1, 'Non-Vegetarian', 'Includes meat, poultry, seafood'),
   (1, 'Eggetarian', 'Vegetarian with eggs'),
   (1, 'Vegan', 'No animal products at all'),
   (1, 'Jain Food', 'Strict vegetarian, no root vegetables'),
   (1, 'Satvik', 'Ayurvedic-style pure veg, no onion/garlic'),
   (1, 'Organic Food', 'Prepared using organic ingredients'),
   (1, 'Gluten-Free', 'Safe for gluten allergy'),
   (1, 'Keto', 'High-fat, low-carb meals'),
   (1, 'Diabetic-Friendly', 'Low sugar, balanced meals'),
   
   -- Cuisine Type
	(2, 'South Indian', 'Idli, Dosa, Vada, Sambar, etc.'),
	(2, 'North Indian', 'Paneer, Dal, Paratha, Chole, etc.'),
	(2, 'Gujarati', 'Thepla, Farsan, Dal-Dhokli, etc.'),
	(2, 'Rajasthani', 'Daal Baati, Gatte, etc.'),
	(2, 'Maharashtrian', 'Poha, Sabudana, Puran Poli, etc.'),
	(2, 'Bengali', 'Fish curry, Sweets, etc.'),
	(2, 'Punjabi', 'Butter Chicken, Naan, etc.'),
	(2, 'Mughlai', 'Rich gravies, kebabs, etc.'),
	(2, 'Chinese', 'Chinese style (Manchurian, Fried Rice), etc.'),
	(2, 'Continental', 'Pasta, Salad, Sandwich, etc.'),
	(2, 'Street Food', 'Chaat, Pav Bhaji, Vada Pav, etc.'),
	(2, 'Fusion', 'Indo-western creative dishes'),
	
    -- Event Type
	(3, 'Wedding', 'Large-scale, buffet/multi-course for weddings'),
	(3, 'Birthday Party', 'Small to medium gatherings'),
	(3, 'Office/Corporate', 'Formal meals or snacks for office events'),
	(3, 'Festival', 'Themed food for Diwali, Eid, Navratri, etc.'),
	(3, 'Housewarming/(Griha Pravesh) ', 'Traditional dishes'),
	(3, 'Religious/Pooja', 'Satvik/Jain food for rituals'),
	(3, 'Baby Shower', 'Regional sweet and snack items'),
	(3, 'Funeral', 'Simple, traditional vegetarian meals'),
	
	--Service Type
	(4, 'Buffet Style',     'Self-service from food stations'),
	(4, 'Plate Service',    'Table service with plated meals'),
	(4, 'Live Counters',    'Food made fresh in front of gues'),
	(4, 'Tiffin Services',  'Daily/weekly home delivery meals'),
	(4, 'Self-Pickup',      'Customer collects food from kitchen'),
	(4, 'Delivery-Only',    'No dine-in, only delivery'),
	(4, 'Full-Service',     'Food + staff + setup + clean'),
	(4, 'Drop-off',         'Food delivered, no service staff'),
	
    -- Serving Slots
	(5, 'Breakfast', 'Morning meal service usually between 7 AM to 10 AM'),
	(5, 'Brunch',  'Late morning meal between breakfast and lunch'),
	(5, 'Lunch',  'Afternoon meal service usually between 12 PM to 3 PM'),
	(5, 'High Tea', 'Light evening snacks served with tea/coffee, 4 PM to 6 PM'),
	(5, 'Dinner',  'Evening meal service usually between 7 PM to 10 PM'),
	(5, 'Snacks',  'Light food items served anytime outside main meals'),
	(5, 'Midnight Meal', 'Late night food service after 10 PM till early morning');


CREATE TABLE t_sys_food_category (
  c_categoryid BIGINT PRIMARY KEY IDENTITY(1,1),  
  c_categoryname NVARCHAR(100) NOT NULL, 
  c_description NVARCHAR(500) NULL, 
  c_is_active BIT NULL, 
  c_is_global BIT NULL, 
  c_createdby NVARCHAR(50) NULL, 
  c_createddate GETDATE() 
);

INSERT INTO t_sys_food_category (c_categoryname, c_description, c_is_active, c_is_global, c_createdby, c_createddate) VALUES

    ('Starter', 'Appetizers served before the main course', 1, 1, NULL, GETDATE()),
    ('Soup', 'Various vegetarian and non-vegetarian soups', 1, 1, NULL, GETDATE()),
    ('Salad', 'Fresh salads including veg and non-veg options', 1, 1, NULL, GETDATE()),
    ('Main Course', 'Primary food items served in the meal', 1, 1, NULL, GETDATE()),
    ('Dal & Rice', 'Dal items with rice varieties', 1, 1, NULL, GETDATE()),
    ('Roti / Bread', 'Indian breads such as roti, naan, paratha', 1, 1, NULL, GETDATE()),
    ('Sweet Regular', 'Standard sweet dishes', 1, 1, NULL, GETDATE()),
    ('Sweet Special', 'Premium or special sweet dishes', 1, 1, NULL, GETDATE()),
    ('Dessert', 'Ice creams, pastries, and other desserts', 1, 1, NULL, GETDATE()),
    ('Juice / Beverages', 'Fruit juices and soft drinks', 1, 1, NULL, GETDATE()),
    ('Chat / Street Food', 'Indian street food items', 1, 1, NULL, GETDATE()),
    ('Chinese', 'Chinese cuisine items', 1, 1, NULL, GETDATE()),
    ('Italian', 'Italian cuisine items', 1, 1, NULL, GETDATE()),
    ('Special Counter', 'Live counters or special stalls', 1, 1, NULL, GETDATE()),
    ('Pan Counter', 'Pan and mouth freshener counters', 1, 1, NULL, GETDATE()),
    ('Mocktail / Coffee Bar', 'Mocktails, coffee, tea, and hot beverages', 1, 1, NULL, GETDATE()),
    ('Bengali Counter', 'Bengali food special items', 1, 1, NULL, GETDATE());


CREATE TABLE t_map_partner_category (
  c_mapid BIGINT PRIMARY KEY IDENTITY(1,1),  
  c_ownerid BIGINT NOT NULL, 
  c_categoryid BIGINT NOT NULL, 
  c_is_enabled BIT NULL, 
  c_createddate DATETIME NULL 
);


CREATE TABLE t_sys_catering_packages (
  c_packageid BIGINT PRIMARY KEY IDENTITY(1,1), 
  c_ownerid BIGINT NOT NULL, 
  c_packagename NVARCHAR(100) NULL, 
  c_description NVARCHAR(1000) NULL, 
  c_price DECIMAL(10, 2) NULL, 
  c_is_active BIT NULL, 
  c_is_deleted BIT DEFAULT 0,
  c_created_date GETDATE(), 
  c_modified_date DATETIME NULL, 
);

CREATE TABLE t_sys_catering_package_items (
  c_itemid BIGINT PRIMARY KEY IDENTITY(1,1),
  c_packageid BIGINT NOT NULL, 
  c_categoryid INT NOT NULL, 
  c_quantity INT NULL, 
  c_created_date GETDATE(), 
  c_modified_date DATETIME NULL
);



-- Menu Table – stores food items offered by partners
CREATE TABLE t_sys_fooditems (
    c_foodid BIGINT PRIMARY KEY IDENTITY(1,1),  -- Primary Key
    c_ownerid BIGINT NOT NULL,                  -- FK → Partner
    c_foodname NVARCHAR(200) NOT NULL,          -- Name of food item
    c_description NVARCHAR(MAX) NULL,           -- Description/details
    c_categoryid BIGINT NULL,                 -- Category (Starter, Main, Dessert, etc.)
    c_cuisinetypeid BIGINT NULL,              -- Cuisine type (Indian, Chinese, Italian, etc.)
    c_price DECIMAL(10,2) NOT NULL,             -- Price
    c_ispackage_item BIT NULL,
    c_isveg BIT NULL,
    c_islive_counter BIT DEFAULT 0,
    c_status BIT NULL,
	c_is_deleted BIT DEFAULT 0,
    c_createddate DATETIME NULL, 
    c_modifieddate DATETIME NULL, 
    c_issample_tasted BIT NOT NULL
);

CREATE TABLE t_sys_catering_decorations (
  c_decoration_id BIGINT PRIMARY KEY IDENTITY(1,1), 
  c_ownerid BIGINT NOT NULL, 
  c_packageids VARCHAR(100) NULL, 
  c_decoration_name NVARCHAR(200) NOT NULL, 
  c_description NVARCHAR(MAX) NULL, 
  c_theme_id INT NOT NULL, 
  c_price DECIMAL(10, 2) NULL, 
  c_status BIT NULL, 
  c_is_deleted BIT DEFAULT 0,
  c_createddate GETDATE(),
  c_modifieddate DATETIME NULL
);


CREATE TABLE t_sys_catering_theme_types
(
    c_theme_id     BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_theme_name   NVARCHAR(150) NOT NULL,
    c_description  NVARCHAR(500) NULL,
    c_isactive     BIT NOT NULL
);

INSERT INTO t_sys_catering_theme_types (c_theme_name, c_description, c_isactive) VALUES

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
  c_staffid BIGINT PRIMARY KEY IDENTITY(1,1),  
  c_ownerid BIGINT NOT NULL, 
  c_fullname NVARCHAR(150) NOT NULL, 
  c_gender NVARCHAR(10) NOT NULL, 
  c_contact_number NVARCHAR(15) NOT NULL, 
  c_role NVARCHAR(100) NOT NULL, 
  c_other_role NVARCHAR(100) NULL, 
  c_expertise_categoryid BIGINT NULL, 
  c_experience_years DECIMAL(4, 1) NULL, 
  c_salary_type NVARCHAR(20) NOT NULL, 
  c_salary_amount DECIMAL(10, 2) NOT NULL, 
  c_profile_path NVARCHAR(500) NULL, 
  c_identity_doc_path NVARCHAR(500) NULL, 
  c_resume_doc_path NVARCHAR(500) NULL, 
  c_availability BIT NULL, 
  c_is_deleted BIT DEFAULT 0,
  c_createddate GETDATE(), 
  c_modifieddate DATETIME NULL, 
);


CREATE TABLE t_sys_catering_review (
    c_reviewid BIGINT IDENTITY(1,1) PRIMARY KEY,

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
    c_review_title NVARCHAR(200) NULL,
    c_review_comment NVARCHAR(2000) NULL,

    -- Review Status & Moderation
    c_is_verified BIT DEFAULT 1,                -- Verified order-based review
    c_is_visible BIT DEFAULT 1,                 -- Visible to customers
    c_is_reported BIT DEFAULT 0,                -- Reported by vendor/user
    c_admin_status NVARCHAR(20) DEFAULT 'Approved', 
        -- Approved / Pending / Rejected

    -- Metadata
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME NULL
);


CREATE TABLE t_sys_catering_discount (
    c_discountid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,

    c_discount_name NVARCHAR(200) NOT NULL,
    c_discount_description NVARCHAR(500) NULL,

    c_discount_type INT NOT NULL,   -- Enum
    c_discount_mode INT NOT NULL,   -- Enum
	
    c_discount_value DECIMAL(10,2) NOT NULL,
    c_min_order_value DECIMAL(10,2) NULL,
    c_max_discount_value DECIMAL(10,2) NULL,
	c_discount_code NVARCHAR(50) NOT NULL UNIQUE,

    c_max_uses_per_order INT DEFAULT 1,
    c_max_uses_per_user INT DEFAULT 1,
    c_is_stackable BIT DEFAULT 0,

    c_startdate DATE NOT NULL,
    c_enddate DATE NOT NULL,

    c_isactive BIT DEFAULT 1,
    c_isautodisable BIT DEFAULT 1,
    c_status INT DEFAULT 1,         -- 1=Active,2=Expired,3=Disabled
	
	c_is_deleted BIT DEFAULT 0,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_updateddate DATETIME NULL
);



CREATE TABLE t_map_discount_fooditem (
    c_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_discountid BIGINT NOT NULL,
    c_foodid BIGINT NOT NULL,
	c_isactive BIT NOT NULL DEFAULT 1,
);

CREATE TABLE t_map_discount_package (
    c_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_discountid BIGINT NOT NULL,
    c_packageid BIGINT NOT NULL,
	c_isactive BIT NOT NULL DEFAULT 1,
);

-- Global Status Table
CREATE TABLE t_catering_availability_global (
    c_id INT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_global_status VARCHAR(20) NOT NULL DEFAULT 'OPEN' CHECK (c_global_status IN ('OPEN', 'CLOSED')),
    c_closure_reason NVARCHAR(255) NULL,
    c_modifieddate DATETIME DEFAULT GETDATE()
);

-- Date-Specific Availability Table
CREATE TABLE t_catering_availability_dates (
    c_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_date DATE NOT NULL,
    c_status VARCHAR(20) NOT NULL CHECK (c_status IN ('CLOSED', 'FULLY_BOOKED', 'OPEN')),
    c_note NVARCHAR(255) NULL,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME NULL
);

-- Indexes for Performance
CREATE INDEX IX_Availability_Dates_Range
ON t_catering_availability_dates (c_ownerid, c_date)
INCLUDE (c_status);
-- logic: fast lookup for "Is owner X available on date Y?" returning just the status.


-- Banner Management Table
CREATE TABLE t_sys_catering_banners (
    c_bannerid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_title NVARCHAR(200) NOT NULL,
    c_description NVARCHAR(MAX) NULL,
    c_link_url NVARCHAR(500) NULL,
    c_display_order INT NOT NULL DEFAULT 0,
    c_isactive BIT NOT NULL DEFAULT 1,
    c_start_date DATETIME NULL,
    c_end_date DATETIME NULL,
    c_click_count INT NOT NULL DEFAULT 0,
    c_view_count INT NOT NULL DEFAULT 0,
    c_is_deleted BIT NOT NULL DEFAULT 0,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME NULL
);

-- Index for better query performance
CREATE INDEX IX_Banners_Owner_Active
ON t_sys_catering_banners (c_ownerid, c_isactive, c_display_order)
WHERE c_is_deleted = 0;

------------------------------------------ Below Tables are executing is pending ---- --------------------------------

CREATE TABLE t_sys_catering_discount_usage (
    c_usage_id BIGINT IDENTITY(1,1) PRIMARY KEY,

    -- Discount Reference
    c_discount_id BIGINT NOT NULL,
    c_discount_code NVARCHAR(50) NOT NULL,

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
    c_used_at DATETIME DEFAULT GETDATE()
);


-- Order Table – stores orders placed by users
CREATE TABLE t_sys_order (
    c_orderid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,
    c_total_amount DECIMAL(10,2) NOT NULL,
    c_statusid BIGINT NOT NULL, -- Pending, Accepted, Preparing, Delivered, Cancelled
    c_created_at DATETIME NULL 
);

-- Order Items Table – stores which items are in an order
CREATE TABLE t_sys_order_items (
    c_order_itemid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_orderid BIGINT NOT NULL,
    c_foodid BIGINT NOT NULL,
    c_quantity INT NOT NULL,
    c_price DECIMAL(10,2) NOT NULL,
);

-- Payment Table – stores payment details
CREATE TABLE t_sys_payment (
    c_paymentid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_orderid BIGINT NOT NULL,
    c_payment_method NVARCHAR(50) NOT NULL, -- UPI, Card, COD
    c_payment_statusid BIGINT NOT NULL, 'Pending', -- Paid, Failed
    c_transaction_id NVARCHAR(100),
    c_paid_amount DECIMAL(10,2),
    c_paid_at DATETIME NOT NULL,
);

--Order History Table – tracks order status changes
CREATE TABLE t_sys_order_history (
    c_historyid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_orderid BIGINT NOT NULL,
    c_status NVARCHAR(50) NOT NULL,
    c_changed_at DATETIME NULL,
);

--Feedback Table – user feedback for orders
CREATE TABLE t_sys_feedback (
    c_feedbackid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_orderid BIGINT NOT NULL,
    c_userid BIGINT NOT NULL,
    c_rating INT CHECK (c_rating BETWEEN 1 AND 5),
    c_comment NVARCHAR(MAX),
    c_created_at DATETIME NULL
);

--optional: Coupon Table – discounts & promotions

CREATE TABLE t_sys_coupon (
    c_couponid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_code NVARCHAR(50) UNIQUE NOT NULL,
    c_discount_percent INT,
    c_valid_from DATETIME,
    c_valid_to DATETIME,
    c_is_active BIT DEFAULT 1
);


--Status Master Table
CREATE TABLE t_sys_statusmaster (
    c_statusid BIGINT PRIMARY KEY IDENTITY(1,1),  -- Primary Key
    c_statusname NVARCHAR(50) NOT NULL,           -- e.g., Active, Inactive, Pending, Completed
    c_description NVARCHAR(200) NULL,             -- Optional detail
    c_createddate DATETIME DEFAULT GETDATE()
);

-- Homepage Statistics Table (for caching homepage stats)
CREATE TABLE t_sys_homepage_stats (
    c_statsid INT PRIMARY KEY IDENTITY(1,1),
    c_total_events_catered INT NOT NULL DEFAULT 0,
    c_total_catering_partners INT NOT NULL DEFAULT 0,
    c_total_happy_customers INT NOT NULL DEFAULT 0,
    c_satisfaction_rate DECIMAL(5,2) NOT NULL DEFAULT 0,
    c_last_updated DATETIME DEFAULT GETDATE()
);

-- Insert initial homepage stats record
INSERT INTO t_sys_homepage_stats (c_total_events_catered, c_total_catering_partners, c_total_happy_customers, c_satisfaction_rate, c_last_updated)
VALUES (5000, 500, 50000, 98.00, GETDATE());


-- Partner Agreement Table
CREATE TABLE t_sys_catering_owner_agreement (
    c_agreementid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_agreement_text NVARCHAR(MAX) NOT NULL,
    c_agreement_accepted BIT NOT NULL DEFAULT 0,
    c_signature_data NVARCHAR(MAX) NULL, -- Base64 encoded signature image
    c_signature_path NVARCHAR(500) NULL, -- File path to signature image
    c_agreement_pdf_path NVARCHAR(500) NULL, -- File path to generated agreement PDF
    c_ip_address NVARCHAR(50) NULL,
    c_user_agent NVARCHAR(500) NULL,
    c_accepted_date DATETIME NULL,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME NULL,
    CONSTRAINT FK_Agreement_Owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);


-- Notification Templates Table
CREATE TABLE t_notification_templates (
    c_template_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_template_code VARCHAR(100) NOT NULL UNIQUE,
    c_channel VARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP
    c_audience VARCHAR(20) NOT NULL, -- ADMIN, PARTNER, USER, ALL
    c_category VARCHAR(50) NOT NULL, -- OTP, ORDER_CONFIRMATION, etc.

    c_name NVARCHAR(200) NOT NULL,
    c_description NVARCHAR(500),

    -- Template Content
    c_subject NVARCHAR(500) NULL, -- For EMAIL only
    c_body NVARCHAR(MAX) NOT NULL,
    c_text_body NVARCHAR(MAX) NULL, -- Plain text version for EMAIL

    -- Metadata
    c_language VARCHAR(10) NOT NULL DEFAULT 'en', -- en, hi, es, etc.
    c_version INT NOT NULL DEFAULT 1,
    c_is_active BIT NOT NULL DEFAULT 1,
    c_is_default BIT NOT NULL DEFAULT 0, -- Default template for this code

    -- Placeholders
    c_placeholders NVARCHAR(MAX), -- JSON array of placeholder names
    c_sample_data NVARCHAR(MAX), -- JSON object with sample data for preview

    -- Audit
    c_created_by VARCHAR(100),
    c_created_date DATETIME DEFAULT GETDATE(),
    c_updated_by VARCHAR(100),
    c_updated_date DATETIME DEFAULT GETDATE(),
    c_last_used_date DATETIME NULL,
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
    c_version_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_template_id BIGINT NOT NULL,
    c_version INT NOT NULL,
    c_subject NVARCHAR(500),
    c_body NVARCHAR(MAX) NOT NULL,
    c_text_body NVARCHAR(MAX),
    c_changed_by VARCHAR(100),
    c_changed_date DATETIME DEFAULT GETDATE(),
    c_change_notes NVARCHAR(500),

    CONSTRAINT FK_template_version_template FOREIGN KEY (c_template_id)
        REFERENCES t_notification_templates(c_template_id)
);

CREATE INDEX IX_template_versions ON t_notification_template_versions(c_template_id, c_version);

-- In-App Notifications Table
CREATE TABLE t_inapp_notifications (
    c_notification_id VARCHAR(50) PRIMARY KEY,
    c_user_id VARCHAR(50) NOT NULL,
    c_user_type VARCHAR(20) NOT NULL, -- ADMIN, PARTNER, USER

    c_title NVARCHAR(200) NOT NULL,
    c_message NVARCHAR(1000) NOT NULL,
    c_category VARCHAR(50),
    c_priority INT DEFAULT 5,

    c_action_url VARCHAR(500),
    c_icon_url VARCHAR(500),
    c_data NVARCHAR(MAX), -- JSON

    c_is_read BIT DEFAULT 0,
    c_read_at DATETIME NULL,

    c_created_at DATETIME DEFAULT GETDATE(),
    c_expires_at DATETIME NULL,

    INDEX IX_inapp_user_unread (c_user_id, c_user_type, c_is_read),
    INDEX IX_inapp_created (c_created_at DESC)
);

-- Notification Delivery Log
CREATE TABLE t_notification_delivery_log (
    c_delivery_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_notification_id VARCHAR(50) NOT NULL,
    c_channel VARCHAR(20) NOT NULL, -- EMAIL, SMS, INAPP

    c_recipient NVARCHAR(200) NOT NULL, -- Email/Phone/UserId
    c_status VARCHAR(20) NOT NULL, -- QUEUED, SENT, DELIVERED, FAILED, BOUNCED

    c_provider VARCHAR(50), -- SendGrid, Twilio, SignalR
    c_provider_message_id VARCHAR(200),

    c_sent_at DATETIME NULL,
    c_delivered_at DATETIME NULL,
    c_opened_at DATETIME NULL,
    c_clicked_at DATETIME NULL,

    c_error_message NVARCHAR(MAX),
    c_retry_count INT DEFAULT 0,
    c_cost DECIMAL(10, 4) DEFAULT 0,

    c_created_at DATETIME DEFAULT GETDATE(),

    INDEX IX_delivery_notification (c_notification_id),
    INDEX IX_delivery_status (c_channel, c_status),
    INDEX IX_delivery_created (c_created_at DESC)
);

-- Notification Queue (for debugging/retry)
CREATE TABLE t_notification_queue (
    c_queue_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_message_id VARCHAR(50) NOT NULL UNIQUE,
    c_routing_key VARCHAR(200) NOT NULL,

    c_payload NVARCHAR(MAX) NOT NULL, -- JSON
    c_priority INT DEFAULT 5,

    c_status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, PROCESSING, COMPLETED, FAILED
    c_retry_count INT DEFAULT 0,
    c_max_retries INT DEFAULT 3,

    c_next_retry_at DATETIME NULL,
    c_error_message NVARCHAR(MAX),

    c_enqueued_at DATETIME DEFAULT GETDATE(),
    c_processed_at DATETIME NULL,

    INDEX IX_queue_status (c_status, c_next_retry_at)
);

-- Template Usage Statistics
CREATE TABLE t_notification_template_stats (
    c_stat_id BIGINT PRIMARY KEY IDENTITY(1,1),
    c_template_id BIGINT NOT NULL,
    c_date DATE NOT NULL,
    c_channel VARCHAR(20) NOT NULL,

    c_sent_count INT DEFAULT 0,
    c_delivered_count INT DEFAULT 0,
    c_failed_count INT DEFAULT 0,
    c_opened_count INT DEFAULT 0,
    c_clicked_count INT DEFAULT 0,

    c_total_cost DECIMAL(10, 2) DEFAULT 0,

    CONSTRAINT FK_template_stats_template FOREIGN KEY (c_template_id)
        REFERENCES t_notification_templates(c_template_id),

    UNIQUE (c_template_id, c_date, c_channel),
    INDEX IX_template_stats_date (c_date DESC)
);