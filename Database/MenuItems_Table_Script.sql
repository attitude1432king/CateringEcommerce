-- =====================================================
-- Create SysMenuItems Table
-- =====================================================
-- This table stores menu items/food items for catering services
-- Owner/Partner can manage their food catalog

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SysMenuItems')
BEGIN
    CREATE TABLE [dbo].[SysMenuItems]
    (
        -- Primary Key
        [c_menu_item_id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
        
        -- Owner/Catering Reference
        [c_ownerid] BIGINT NOT NULL,
        
        -- Item Information
        [c_item_name] NVARCHAR(255) NOT NULL,
        [c_description] NVARCHAR(1000) NULL,
        [c_category] NVARCHAR(100) NOT NULL,
        [c_price] DECIMAL(10, 2) NOT NULL,
        
        -- Item Properties
        [c_is_vegetarian] BIT NOT NULL DEFAULT 1,
        [c_is_spicy] BIT NOT NULL DEFAULT 0,
        [c_is_vegan] BIT NOT NULL DEFAULT 0,
        [c_is_gluten_free] BIT NOT NULL DEFAULT 0,
        
        -- Media
        [c_image_url] NVARCHAR(500) NULL,
        [c_thumbnail_url] NVARCHAR(500) NULL,
        
        -- Availability & Status
        [c_is_active] BIT NOT NULL DEFAULT 1,
        [c_is_seasonal] BIT NOT NULL DEFAULT 0,
        [c_availability_from_date] DATE NULL,
        [c_availability_to_date] DATE NULL,
        
        -- Nutritional/Additional Info
        [c_calories] INT NULL,
        [c_serving_size] NVARCHAR(100) NULL,
        [c_ingredients] NVARCHAR(MAX) NULL,
        [c_allergen_info] NVARCHAR(MAX) NULL,
        [c_preparation_time_minutes] INT NULL,
        
        -- Quantity Tracking
        [c_current_stock] INT NULL,
        [c_min_stock_level] INT NULL,
        [c_max_order_quantity] INT NULL,
        
        -- Ratings & Reviews
        [c_average_rating] DECIMAL(3, 2) NULL DEFAULT 0,
        [c_review_count] INT NULL DEFAULT 0,
        
        -- Metadata
        [c_sort_order] INT NULL,
        [c_display_priority] INT NULL,
        
        -- Audit Columns
        [c_created_date] DATETIME NOT NULL DEFAULT GETDATE(),
        [c_created_by] BIGINT NULL,
        [c_modified_date] DATETIME NULL,
        [c_modified_by] BIGINT NULL,
        [c_is_deleted] BIT NOT NULL DEFAULT 0,
        [c_deleted_date] DATETIME NULL,
        
        -- Constraints
        CONSTRAINT [FK_SysMenuItems_Owner] FOREIGN KEY ([c_ownerid]) 
            REFERENCES [dbo].[SysOwner]([c_ownerid])
            ON DELETE NO ACTION
            ON UPDATE NO ACTION,
        
        CONSTRAINT [CHK_MenuItems_Price] CHECK ([c_price] >= 0),
        CONSTRAINT [CHK_MenuItems_Rating] CHECK ([c_average_rating] >= 0 AND [c_average_rating] <= 5)
    );
    
    -- Create Indexes for Performance
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_OwnerId_Active] 
        ON [dbo].[SysMenuItems] ([c_ownerid], [c_is_active])
        INCLUDE ([c_item_name], [c_category], [c_price]);
    
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_Category] 
        ON [dbo].[SysMenuItems] ([c_category], [c_is_active]);
    
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_IsVegetarian] 
        ON [dbo].[SysMenuItems] ([c_is_vegetarian], [c_is_active]);
    
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_CreatedDate] 
        ON [dbo].[SysMenuItems] ([c_created_date] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_Price] 
        ON [dbo].[SysMenuItems] ([c_price])
        WHERE [c_is_active] = 1;
    
    CREATE NONCLUSTERED INDEX [IX_SysMenuItems_Rating] 
        ON [dbo].[SysMenuItems] ([c_average_rating] DESC)
        WHERE [c_is_active] = 1;
    
    PRINT 'SysMenuItems table created successfully.';
END
ELSE
BEGIN
    PRINT 'SysMenuItems table already exists.';
END;

-- =====================================================
-- Add Columns if they don't exist (for upgrades)
-- =====================================================

-- Helper script to check and add missing columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_item_name')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_item_name] NVARCHAR(255) NOT NULL DEFAULT 'New Item';
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_category')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_category] NVARCHAR(100) NOT NULL DEFAULT 'Uncategorized';
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_price')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_price] DECIMAL(10, 2) NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_image_url')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_image_url] NVARCHAR(500) NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_is_active')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_is_active] BIT NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[SysMenuItems]') AND name = 'c_is_vegetarian')
BEGIN
    ALTER TABLE [dbo].[SysMenuItems] ADD [c_is_vegetarian] BIT NOT NULL DEFAULT 1;
END;

-- =====================================================
-- Sample Data Insert (Optional)
-- =====================================================

/*
-- Insert sample menu items
INSERT INTO [dbo].[SysMenuItems] (
    [c_ownerid],
    [c_item_name],
    [c_description],
    [c_category],
    [c_price],
    [c_is_vegetarian],
    [c_is_spicy],
    [c_image_url],
    [c_is_active],
    [c_serving_size],
    [c_preparation_time_minutes]
)
VALUES 
    (1, 'Paneer Tikka', 'Succulent pieces of cottage cheese marinated in yogurt and spices', 'Appetizers', 250.00, 1, 0, '/images/paneer-tikka.jpg', 1, '6 pieces', 20),
    (1, 'Biryani', 'Fragrant rice cooked with meat or vegetables', 'Main Course', 300.00, 0, 0, '/images/biryani.jpg', 1, '2 servings', 45),
    (1, 'Gulab Jamun', 'Sweet milky dessert', 'Desserts', 80.00, 1, 0, '/images/gulab-jamun.jpg', 1, '2 pieces', 15);
*/
