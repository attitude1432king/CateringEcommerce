-- Cart Table for User Session Cart (Event-Based)
-- Users can only have one caterer in cart at a time
-- Cart is event-based: it stores the selected caterer, package, guest count, and event details

CREATE TABLE IF NOT EXISTS t_sys_user_cart (
    c_cartid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL, -- Selected caterer
    c_packageid BIGINT NULL, -- Selected package (can be null if selecting individual items)
    c_guest_count INTEGER NOT NULL DEFAULT 50,
    c_event_date TIMESTAMP NULL,
    c_event_type VARCHAR(100) NULL, -- Wedding, Birthday, Corporate, etc.
    c_event_location VARCHAR(500) NULL,
    c_special_requirements TEXT NULL,
    c_decoration_id BIGINT NULL, -- Selected decoration theme (optional)
    c_base_amount DECIMAL(10,2) NULL,
    c_decoration_amount DECIMAL(10,2) NULL DEFAULT 0,
    c_tax_amount DECIMAL(10,2) NULL DEFAULT 0,
    c_total_amount DECIMAL(10,2) NULL,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP NULL,
    CONSTRAINT fk_cart_user FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_cart_owner FOREIGN KEY (c_ownerid) REFERENCES t_sys_catering_owner(c_ownerid),
    -- User can only have one active cart at a time
    CONSTRAINT uq_user_cart UNIQUE (c_userid)
);

-- Cart Additional Food Items Table
-- Stores food items that are NOT included in the package (add-ons)
CREATE TABLE IF NOT EXISTS t_sys_cart_food_items (
    c_cart_item_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_cartid BIGINT NOT NULL,
    c_foodid BIGINT NOT NULL,
    c_quantity INTEGER NOT NULL DEFAULT 1,
    c_price DECIMAL(10,2) NOT NULL,
    c_createddate TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_cart_item_cart FOREIGN KEY (c_cartid) REFERENCES t_sys_user_cart(c_cartid) ON DELETE CASCADE,
    CONSTRAINT fk_cart_item_food FOREIGN KEY (c_foodid) REFERENCES t_sys_fooditems(c_foodid)
);

-- Cart Decorations Table
-- Stores one or more selected decoration themes for a cart
CREATE TABLE IF NOT EXISTS t_sys_cart_decorations (
    c_cart_decoration_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_cartid BIGINT NOT NULL,
    c_decoration_id BIGINT NOT NULL,
    c_price DECIMAL(10,2) NOT NULL DEFAULT 0,
    c_createddate TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_cart_decoration_cart FOREIGN KEY (c_cartid) REFERENCES t_sys_user_cart(c_cartid) ON DELETE CASCADE,
    CONSTRAINT fk_cart_decoration_decoration FOREIGN KEY (c_decoration_id) REFERENCES t_sys_catering_decorations(c_decoration_id),
    CONSTRAINT uq_cart_decoration UNIQUE (c_cartid, c_decoration_id)
);

-- Add indexes for better performance
CREATE INDEX IF NOT EXISTS ix_cart_userid ON t_sys_user_cart(c_userid);
CREATE INDEX IF NOT EXISTS ix_cart_ownerid ON t_sys_user_cart(c_ownerid);
CREATE INDEX IF NOT EXISTS ix_cart_item_cartid ON t_sys_cart_food_items(c_cartid);
CREATE INDEX IF NOT EXISTS ix_cart_decoration_cartid ON t_sys_cart_decorations(c_cartid);


-- =============================================
-- Table: t_sys_cart_decorations
-- Purpose: Store decoration selections in shopping carts
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_cart_decorations (
    c_cart_decoration_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_cartid BIGINT NOT NULL,
    c_decoration_id BIGINT NOT NULL,
    c_price DECIMAL(10,2) NOT NULL DEFAULT 0,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_cart_decorations_cart
        FOREIGN KEY (c_cartid) REFERENCES t_sys_user_cart(c_cartid) ON DELETE CASCADE,
    CONSTRAINT fk_cart_decorations_decoration
        FOREIGN KEY (c_decoration_id) REFERENCES t_sys_catering_decorations(c_decoration_id),
    CONSTRAINT uq_cart_decorations_cart_decoration
        UNIQUE (c_cartid, c_decoration_id)
);

CREATE INDEX IF NOT EXISTS ix_cart_decorations_cartid
    ON t_sys_cart_decorations(c_cartid);