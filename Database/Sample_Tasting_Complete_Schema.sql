-- ============================================================
-- SAMPLE TASTING COMPLETE DATABASE SCHEMA
-- Version: 1.0
-- Date: February 7, 2026
-- Purpose: Paid sample tasting with live delivery tracking
-- ============================================================

USE [CateringEcommerce]
GO

-- ============================================================
-- 1. SAMPLE ORDERS TABLE (Main Order Entity)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sample_Orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Sample_Orders] (
        [SampleOrderID] BIGINT PRIMARY KEY IDENTITY(1,1),
        [UserID] BIGINT NOT NULL,
        [CateringID] BIGINT NOT NULL,

        -- Pricing (NEVER derived from package price)
        [SamplePriceTotal] DECIMAL(10,2) NOT NULL,
        [DeliveryCharge] DECIMAL(10,2) NOT NULL DEFAULT 0,
        [TotalAmount] DECIMAL(10,2) NOT NULL,

        -- Status Management
        [Status] VARCHAR(50) NOT NULL DEFAULT 'SAMPLE_REQUESTED',
        [PaymentStatus] VARCHAR(50) NOT NULL DEFAULT 'PENDING',
        [IsPaid] BIT NOT NULL DEFAULT 0,

        -- Addresses
        [DeliveryAddressID] BIGINT NOT NULL,
        [PickupAddress] NVARCHAR(500) NOT NULL,
        [PickupLatitude] DECIMAL(10,8) NULL,
        [PickupLongitude] DECIMAL(11,8) NULL,
        [DeliveryLatitude] DECIMAL(10,8) NULL,
        [DeliveryLongitude] DECIMAL(11,8) NULL,

        -- Payment Reference
        [PaymentID] BIGINT NULL,
        [PaymentGatewayOrderID] VARCHAR(100) NULL,
        [PaymentGatewayTransactionID] VARCHAR(100) NULL,

        -- Partner Response
        [PartnerResponseDate] DATETIME NULL,
        [RejectionReason] NVARCHAR(500) NULL,

        -- Third-Party Delivery Integration
        [DeliveryProvider] VARCHAR(50) NULL, -- 'DUNZO', 'PORTER', 'SHADOWFAX'
        [DeliveryPartnerOrderID] VARCHAR(100) NULL,
        [DeliveryPartnerName] VARCHAR(200) NULL,
        [DeliveryPartnerPhone] VARCHAR(20) NULL,
        [DeliveryVehicleNumber] VARCHAR(50) NULL,
        [EstimatedPickupTime] DATETIME NULL,
        [ActualPickupTime] DATETIME NULL,
        [EstimatedDeliveryTime] DATETIME NULL,
        [ActualDeliveryTime] DATETIME NULL,

        -- Customer Feedback & Conversion
        [ClientFeedback] NVARCHAR(1000) NULL,
        [TasteRating] INT NULL, -- 1-5
        [HygieneRating] INT NULL, -- 1-5
        [OverallRating] INT NULL, -- 1-5
        [FeedbackDate] DATETIME NULL,
        [ConvertedToEventOrder] BIT DEFAULT 0,
        [EventOrderID] BIGINT NULL,
        [ConversionDate] DATETIME NULL,

        -- Audit Fields
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [LastModifiedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,

        -- Foreign Keys
        CONSTRAINT [FK_Sample_Orders_Users] FOREIGN KEY ([UserID])
            REFERENCES [dbo].[Users]([PkID]),
        CONSTRAINT [FK_Sample_Orders_Owner] FOREIGN KEY ([CateringID])
            REFERENCES [dbo].[Owner]([PkID]),
        CONSTRAINT [FK_Sample_Orders_Addresses] FOREIGN KEY ([DeliveryAddressID])
            REFERENCES [dbo].[User_Addresses]([PkID]),
        CONSTRAINT [FK_Sample_Orders_Events] FOREIGN KEY ([EventOrderID])
            REFERENCES [dbo].[Orders]([PkID]),

        -- Constraints
        CONSTRAINT [CK_Sample_Orders_TasteRating] CHECK ([TasteRating] BETWEEN 1 AND 5),
        CONSTRAINT [CK_Sample_Orders_HygieneRating] CHECK ([HygieneRating] BETWEEN 1 AND 5),
        CONSTRAINT [CK_Sample_Orders_OverallRating] CHECK ([OverallRating] BETWEEN 1 AND 5),
        CONSTRAINT [CK_Sample_Orders_Status] CHECK ([Status] IN (
            'SAMPLE_REQUESTED', 'SAMPLE_ACCEPTED', 'SAMPLE_REJECTED',
            'SAMPLE_PREPARING', 'READY_FOR_PICKUP', 'IN_TRANSIT',
            'DELIVERED', 'REFUNDED'
        ))
    );

    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_Sample_Orders_UserID]
        ON [dbo].[Sample_Orders]([UserID]);
    CREATE NONCLUSTERED INDEX [IX_Sample_Orders_CateringID]
        ON [dbo].[Sample_Orders]([CateringID]);
    CREATE NONCLUSTERED INDEX [IX_Sample_Orders_Status]
        ON [dbo].[Sample_Orders]([Status]);
    CREATE NONCLUSTERED INDEX [IX_Sample_Orders_CreatedDate]
        ON [dbo].[Sample_Orders]([CreatedDate] DESC);
    CREATE NONCLUSTERED INDEX [IX_Sample_Orders_PaymentStatus]
        ON [dbo].[Sample_Orders]([PaymentStatus]);

    PRINT 'Table Sample_Orders created successfully';
END
GO

-- ============================================================
-- 2. SAMPLE ORDER ITEMS TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sample_Order_Items]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Sample_Order_Items] (
        [SampleItemID] BIGINT PRIMARY KEY IDENTITY(1,1),
        [SampleOrderID] BIGINT NOT NULL,

        -- Item Reference
        [MenuItemID] BIGINT NOT NULL,
        [MenuItemName] NVARCHAR(200) NOT NULL,

        -- CRITICAL: Sample pricing is PER ITEM, NOT from package
        [SamplePrice] DECIMAL(10,2) NOT NULL,
        [SampleQuantity] INT NOT NULL DEFAULT 1, -- Fixed, no client control

        -- Item Details (Snapshot for historical record)
        [Category] VARCHAR(100) NULL,
        [Description] NVARCHAR(500) NULL,
        [ImageUrl] NVARCHAR(500) NULL,
        [CuisineType] VARCHAR(100) NULL,
        [IsVeg] BIT NULL,

        -- Source Tracking
        [IsFromPackage] BIT DEFAULT 0,
        [PackageID] BIGINT NULL,

        -- Audit
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),

        -- Foreign Keys
        CONSTRAINT [FK_Sample_Items_Orders] FOREIGN KEY ([SampleOrderID])
            REFERENCES [dbo].[Sample_Orders]([SampleOrderID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Sample_Items_FoodItems] FOREIGN KEY ([MenuItemID])
            REFERENCES [dbo].[FoodItems]([PkID])
    );

    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_Sample_Items_SampleOrderID]
        ON [dbo].[Sample_Order_Items]([SampleOrderID]);
    CREATE NONCLUSTERED INDEX [IX_Sample_Items_MenuItemID]
        ON [dbo].[Sample_Order_Items]([MenuItemID]);

    PRINT 'Table Sample_Order_Items created successfully';
END
GO

-- ============================================================
-- 3. SAMPLE DELIVERY TRACKING TABLE (Live Tracking)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sample_Delivery_Tracking]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Sample_Delivery_Tracking] (
        [TrackingID] BIGINT PRIMARY KEY IDENTITY(1,1),
        [SampleOrderID] BIGINT NOT NULL,

        -- Live Tracking Data (Like Swiggy/Zomato)
        [DeliveryStatus] VARCHAR(50) NOT NULL,
        [Latitude] DECIMAL(10,8) NULL,
        [Longitude] DECIMAL(11,8) NULL,

        -- Delivery Partner Info
        [PartnerName] VARCHAR(200) NULL,
        [PartnerPhone] VARCHAR(20) NULL,
        [VehicleNumber] VARCHAR(50) NULL,
        [VehicleType] VARCHAR(50) NULL,

        -- ETA Tracking
        [EstimatedArrival] DATETIME NULL,
        [DistanceRemaining] DECIMAL(10,2) NULL, -- in KM

        -- Status Update Details
        [StatusMessage] NVARCHAR(500) NULL,
        [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(),

        -- Additional Tracking
        [Speed] DECIMAL(5,2) NULL, -- km/h
        [BatteryLevel] INT NULL, -- For electric vehicles

        -- Foreign Keys
        CONSTRAINT [FK_Tracking_SampleOrders] FOREIGN KEY ([SampleOrderID])
            REFERENCES [dbo].[Sample_Orders]([SampleOrderID]) ON DELETE CASCADE,

        -- Constraints
        CONSTRAINT [CK_Tracking_DeliveryStatus] CHECK ([DeliveryStatus] IN (
            'PICKUP_ASSIGNED', 'PICKED_UP', 'IN_TRANSIT', 'DELIVERED', 'FAILED'
        ))
    );

    -- Indexes for real-time queries
    CREATE NONCLUSTERED INDEX [IX_Tracking_SampleOrderID]
        ON [dbo].[Sample_Delivery_Tracking]([SampleOrderID]);
    CREATE NONCLUSTERED INDEX [IX_Tracking_Timestamp]
        ON [dbo].[Sample_Delivery_Tracking]([Timestamp] DESC);
    CREATE NONCLUSTERED INDEX [IX_Tracking_Status]
        ON [dbo].[Sample_Delivery_Tracking]([DeliveryStatus]);

    PRINT 'Table Sample_Delivery_Tracking created successfully';
END
GO

-- ============================================================
-- 4. SAMPLE REFUNDS TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sample_Refunds]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Sample_Refunds] (
        [RefundID] BIGINT PRIMARY KEY IDENTITY(1,1),
        [SampleOrderID] BIGINT NOT NULL,

        -- Refund Details
        [RefundAmount] DECIMAL(10,2) NOT NULL,
        [RefundReason] VARCHAR(100) NOT NULL,
        [RefundStatus] VARCHAR(50) NOT NULL DEFAULT 'PENDING',

        -- Payment Gateway Integration
        [PaymentGatewayRefundID] VARCHAR(100) NULL,
        [RefundMethod] VARCHAR(50) NULL, -- 'ORIGINAL_PAYMENT', 'WALLET', etc.

        -- Timing
        [RefundInitiatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [RefundCompletedDate] DATETIME NULL,
        [ExpectedRefundDate] DATETIME NULL,

        -- Additional Info
        [Notes] NVARCHAR(500) NULL,
        [ProcessedBy] VARCHAR(100) NULL, -- System or Admin ID
        [IsAutoRefund] BIT DEFAULT 0, -- True for partner rejection

        -- Foreign Keys
        CONSTRAINT [FK_Refunds_SampleOrders] FOREIGN KEY ([SampleOrderID])
            REFERENCES [dbo].[Sample_Orders]([SampleOrderID]),

        -- Constraints
        CONSTRAINT [CK_Refund_Status] CHECK ([RefundStatus] IN (
            'PENDING', 'PROCESSING', 'COMPLETED', 'FAILED'
        )),
        CONSTRAINT [CK_Refund_Reason] CHECK ([RefundReason] IN (
            'PARTNER_REJECTED', 'DELIVERY_FAILED', 'CUSTOMER_REQUEST',
            'QUALITY_ISSUE', 'SYSTEM_ERROR'
        ))
    );

    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_Refunds_SampleOrderID]
        ON [dbo].[Sample_Refunds]([SampleOrderID]);
    CREATE NONCLUSTERED INDEX [IX_Refunds_Status]
        ON [dbo].[Sample_Refunds]([RefundStatus]);

    PRINT 'Table Sample_Refunds created successfully';
END
GO

-- ============================================================
-- 5. MENU ITEM SAMPLE PRICING TABLE (Critical - NOT package-based)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Menu_Item_Sample_Pricing]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Menu_Item_Sample_Pricing] (
        [ID] BIGINT PRIMARY KEY IDENTITY(1,1),
        [MenuItemID] BIGINT NOT NULL,

        -- CRITICAL: Sample-specific pricing, NEVER derived from package
        [SamplePrice] DECIMAL(10,2) NOT NULL,
        [SampleQuantity] INT NOT NULL DEFAULT 1, -- Fixed sample size (e.g., 250g)

        -- Availability
        [IsAvailableForSample] BIT NOT NULL DEFAULT 1,
        [MinOrderQuantity] INT NULL, -- If sample leads to min order requirement

        -- Partner Settings
        [OwnerID] BIGINT NOT NULL,
        [IsPartnerApproved] BIT DEFAULT 1,

        -- Audit
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [LastModifiedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatedBy] BIGINT NULL,
        [ModifiedBy] BIGINT NULL,

        -- Foreign Keys
        CONSTRAINT [FK_SamplePricing_FoodItems] FOREIGN KEY ([MenuItemID])
            REFERENCES [dbo].[FoodItems]([PkID]) ON DELETE CASCADE,
        CONSTRAINT [FK_SamplePricing_Owner] FOREIGN KEY ([OwnerID])
            REFERENCES [dbo].[Owner]([PkID]),

        -- Unique Constraint
        CONSTRAINT [UQ_MenuItem_Sample] UNIQUE([MenuItemID])
    );

    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_SamplePricing_MenuItemID]
        ON [dbo].[Menu_Item_Sample_Pricing]([MenuItemID]);
    CREATE NONCLUSTERED INDEX [IX_SamplePricing_OwnerID]
        ON [dbo].[Menu_Item_Sample_Pricing]([OwnerID]);
    CREATE NONCLUSTERED INDEX [IX_SamplePricing_Available]
        ON [dbo].[Menu_Item_Sample_Pricing]([IsAvailableForSample]);

    PRINT 'Table Menu_Item_Sample_Pricing created successfully';
END
GO

-- ============================================================
-- 6. SYSTEM SAMPLE CONFIGURATION TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[System_Sample_Configuration]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[System_Sample_Configuration] (
        [ConfigID] INT PRIMARY KEY IDENTITY(1,1),

        -- Item Selection Limits (ABUSE PREVENTION)
        [MaxSampleItemsAllowed] INT NOT NULL DEFAULT 3,
        [MinSampleItemsRequired] INT NOT NULL DEFAULT 1,

        -- Pricing Model
        [PricingModel] VARCHAR(50) NOT NULL DEFAULT 'PER_ITEM',
        [FixedSampleFee] DECIMAL(10,2) NULL, -- If FIXED_FEE model

        -- Delivery Configuration
        [DefaultDeliveryProvider] VARCHAR(50) NOT NULL DEFAULT 'DUNZO',
        [EnableLiveTracking] BIT NOT NULL DEFAULT 1,
        [DeliveryChargeFlat] DECIMAL(10,2) DEFAULT 50,
        [FreeDeliveryAbove] DECIMAL(10,2) DEFAULT 500,

        -- Business Rules
        [RequirePartnerApproval] BIT NOT NULL DEFAULT 1,
        [AutoRejectAfterHours] INT DEFAULT 24,
        [AllowSameDaySampling] BIT DEFAULT 1,

        -- Abuse Prevention
        [MaxSamplesPerUserPerMonth] INT DEFAULT 2,
        [SampleCooldownHours] INT DEFAULT 24,
        [RequireMinSpendForSample] DECIMAL(10,2) NULL,

        -- Conversion Tracking
        [ShowConversionCTAAfterDelivery] BIT NOT NULL DEFAULT 1,
        [ConversionDiscountPercent] DECIMAL(5,2) NULL,

        -- System Settings
        [IsActive] BIT NOT NULL DEFAULT 1,
        [EffectiveFrom] DATETIME NOT NULL DEFAULT GETDATE(),
        [EffectiveTo] DATETIME NULL,
        [LastModifiedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedBy] VARCHAR(100) NULL,

        -- Constraints
        CONSTRAINT [CK_Config_PricingModel] CHECK ([PricingModel] IN ('PER_ITEM', 'FIXED_FEE')),
        CONSTRAINT [CK_Config_MaxItems] CHECK ([MaxSampleItemsAllowed] BETWEEN 1 AND 5),
        CONSTRAINT [CK_Config_DeliveryProvider] CHECK ([DefaultDeliveryProvider] IN (
            'DUNZO', 'PORTER', 'SHADOWFAX'
        ))
    );

    PRINT 'Table System_Sample_Configuration created successfully';

    -- Insert default configuration
    INSERT INTO [dbo].[System_Sample_Configuration] (
        [MaxSampleItemsAllowed],
        [MinSampleItemsRequired],
        [PricingModel],
        [DefaultDeliveryProvider],
        [EnableLiveTracking],
        [RequirePartnerApproval],
        [MaxSamplesPerUserPerMonth],
        [ShowConversionCTAAfterDelivery]
    ) VALUES (
        3, -- Max 3 items
        1, -- Min 1 item
        'PER_ITEM', -- Per-item pricing
        'DUNZO', -- Default delivery
        1, -- Live tracking enabled
        1, -- Partner approval required
        2, -- Max 2 samples per user per month
        1 -- Show conversion CTA
    );

    PRINT 'Default sample configuration inserted';
END
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

-- Get Active Sample Configuration
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_GetActiveSampleConfiguration]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM [dbo].[System_Sample_Configuration]
    WHERE [IsActive] = 1
      AND [EffectiveFrom] <= GETDATE()
      AND ([EffectiveTo] IS NULL OR [EffectiveTo] >= GETDATE())
    ORDER BY [ConfigID] DESC;
END
GO

-- Check User Sample Eligibility (Abuse Prevention)
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_CheckUserSampleEligibility]
    @UserID BIGINT,
    @CateringID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaxSamplesPerMonth INT;
    DECLARE @CooldownHours INT;
    DECLARE @SampleCountThisMonth INT;
    DECLARE @LastSampleDate DATETIME;
    DECLARE @HoursSinceLastSample INT;

    -- Get configuration
    SELECT
        @MaxSamplesPerMonth = [MaxSamplesPerUserPerMonth],
        @CooldownHours = [SampleCooldownHours]
    FROM [dbo].[System_Sample_Configuration]
    WHERE [IsActive] = 1;

    -- Check samples this month
    SELECT @SampleCountThisMonth = COUNT(*)
    FROM [dbo].[Sample_Orders]
    WHERE [UserID] = @UserID
      AND [CreatedDate] >= DATEADD(MONTH, -1, GETDATE())
      AND [Status] NOT IN ('SAMPLE_REJECTED', 'REFUNDED');

    -- Check last sample date
    SELECT TOP 1 @LastSampleDate = [CreatedDate]
    FROM [dbo].[Sample_Orders]
    WHERE [UserID] = @UserID
      AND [CateringID] = @CateringID
    ORDER BY [CreatedDate] DESC;

    SET @HoursSinceLastSample = DATEDIFF(HOUR, @LastSampleDate, GETDATE());

    -- Return eligibility
    SELECT
        CASE
            WHEN @SampleCountThisMonth >= @MaxSamplesPerMonth THEN 0
            WHEN @HoursSinceLastSample < @CooldownHours THEN 0
            ELSE 1
        END AS [IsEligible],
        @SampleCountThisMonth AS [SampleCountThisMonth],
        @MaxSamplesPerMonth AS [MaxAllowed],
        @HoursSinceLastSample AS [HoursSinceLastSample],
        @CooldownHours AS [CooldownRequired];
END
GO

-- Get Sample-Eligible Menu Items for Catering
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_GetSampleEligibleItems]
    @CateringID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        fi.[PkID] AS [MenuItemID],
        fi.[Name] AS [ItemName],
        fi.[Category],
        fi.[Description],
        fi.[ImagePath],
        fi.[IsVeg],
        fi.[CuisineType],
        sp.[SamplePrice],
        sp.[SampleQuantity],
        sp.[IsAvailableForSample]
    FROM [dbo].[FoodItems] fi
    INNER JOIN [dbo].[Menu_Item_Sample_Pricing] sp
        ON fi.[PkID] = sp.[MenuItemID]
    WHERE sp.[OwnerID] = @CateringID
      AND sp.[IsAvailableForSample] = 1
      AND fi.[IsActive] = 1
    ORDER BY fi.[Category], fi.[Name];
END
GO

PRINT '============================================================';
PRINT 'Sample Tasting Database Schema Created Successfully!';
PRINT '============================================================';
PRINT 'Tables Created: 6';
PRINT 'Stored Procedures Created: 3';
PRINT 'Status: Ready for Application Integration';
PRINT '============================================================';
GO
