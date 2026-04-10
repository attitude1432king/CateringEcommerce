-- Cart Table for User Session Cart (Event-Based)
-- Users can only have one caterer in cart at a time
-- Cart is event-based: it stores the selected caterer, package, guest count, and event details

CREATE TABLE t_sys_user_cart (
    c_cartid BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL, -- Selected caterer
    c_packageid BIGINT NULL, -- Selected package (can be null if selecting individual items)
    c_guest_count INT NOT NULL DEFAULT 50,
    c_event_date DATETIME NULL,
    c_event_type NVARCHAR(100) NULL, -- Wedding, Birthday, Corporate, etc.
    c_event_location NVARCHAR(500) NULL,
    c_special_requirements NVARCHAR(MAX) NULL,
    c_decoration_id BIGINT NULL, -- Selected decoration theme (optional)
    c_base_amount DECIMAL(10,2) NULL,
    c_decoration_amount DECIMAL(10,2) NULL DEFAULT 0,
    c_tax_amount DECIMAL(10,2) NULL DEFAULT 0,
    c_total_amount DECIMAL(10,2) NULL,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME NULL,
    CONSTRAINT FK_Cart_User FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT FK_Cart_Owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    -- User can only have one active cart at a time
    CONSTRAINT UQ_User_Cart UNIQUE (c_userid)
);

-- Cart Additional Food Items Table
-- Stores food items that are NOT included in the package (add-ons)
CREATE TABLE t_sys_cart_food_items (
    c_cart_item_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_cartid BIGINT NOT NULL,
    c_foodid BIGINT NOT NULL,
    c_quantity INT NOT NULL DEFAULT 1,
    c_price DECIMAL(10,2) NOT NULL,
    c_createddate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_CartItem_Cart FOREIGN KEY (c_cartid) REFERENCES t_sys_user_cart(c_cartid) ON DELETE CASCADE,
    CONSTRAINT FK_CartItem_Food FOREIGN KEY (c_foodid) REFERENCES t_sys_fooditems(c_foodid)
);

-- Cart Decorations Table
-- Stores one or more selected decoration themes for a cart
CREATE TABLE t_sys_cart_decorations (
    c_cart_decoration_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    c_cartid BIGINT NOT NULL,
    c_decoration_id BIGINT NOT NULL,
    c_price DECIMAL(10,2) NOT NULL DEFAULT 0,
    c_createddate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_CartDecoration_Cart FOREIGN KEY (c_cartid) REFERENCES t_sys_user_cart(c_cartid) ON DELETE CASCADE,
    CONSTRAINT FK_CartDecoration_Decoration FOREIGN KEY (c_decoration_id) REFERENCES t_sys_catering_decorations(c_decoration_id),
    CONSTRAINT UQ_CartDecoration UNIQUE (c_cartid, c_decoration_id)
);

-- Add indexes for better performance
CREATE INDEX IX_Cart_UserID ON t_sys_user_cart(c_userid);
CREATE INDEX IX_Cart_OwnerID ON t_sys_user_cart(c_ownerid);
CREATE INDEX IX_CartItem_CartID ON t_sys_cart_food_items(c_cartid);
CREATE INDEX IX_CartDecoration_CartID ON t_sys_cart_decorations(c_cartid);
