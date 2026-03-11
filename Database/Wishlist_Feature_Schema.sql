-- =============================================
-- WISHLIST / FAVORITES FEATURE
-- Database Schema
-- Created: February 5, 2026
-- =============================================

USE [CateringDB]
GO

PRINT 'Creating Wishlist/Favorites table...';
GO

-- =============================================
-- TABLE: USER FAVORITES
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_sys_user_favorites')
BEGIN
    CREATE TABLE t_sys_user_favorites (
        c_favorite_id BIGINT PRIMARY KEY IDENTITY(1,1),
        c_userid BIGINT NOT NULL,
        c_catering_id BIGINT NOT NULL,

        -- Tracking
        c_added_date DATETIME NOT NULL DEFAULT GETDATE(),
        c_is_active BIT NOT NULL DEFAULT 1,

        -- Audit
        c_removed_date DATETIME NULL,
        c_createddate DATETIME NOT NULL DEFAULT GETDATE(),

        -- Foreign Keys
        CONSTRAINT FK_favorites_user FOREIGN KEY (c_userid)
            REFERENCES t_sys_user(c_userid) ON DELETE CASCADE,
        CONSTRAINT FK_favorites_catering FOREIGN KEY (c_catering_id)
            REFERENCES t_sys_catering_owner(c_ownerid),

        -- Unique Constraint: One user can favorite one catering only once
        CONSTRAINT UQ_user_catering_favorite UNIQUE (c_userid, c_catering_id),

        -- Indexes for performance
        INDEX idx_user (c_userid),
        INDEX idx_catering (c_catering_id),
        INDEX idx_active (c_is_active),
        INDEX idx_added_date (c_added_date DESC)
    );
    PRINT '✓ Created table: t_sys_user_favorites';
END
ELSE
    PRINT '  Table t_sys_user_favorites already exists';
GO

-- =============================================
-- STORED PROCEDURE: ADD TO FAVORITES
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_AddFavorite')
    DROP PROCEDURE sp_User_AddFavorite;
GO

CREATE PROCEDURE sp_User_AddFavorite
    @UserId BIGINT,
    @CateringId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if already exists
    IF EXISTS (
        SELECT 1 FROM t_sys_user_favorites
        WHERE c_userid = @UserId AND c_catering_id = @CateringId
    )
    BEGIN
        -- If exists but inactive, reactivate
        UPDATE t_sys_user_favorites
        SET c_is_active = 1,
            c_added_date = GETDATE(),
            c_removed_date = NULL
        WHERE c_userid = @UserId
          AND c_catering_id = @CateringId
          AND c_is_active = 0;

        SELECT @UserId AS UserId, @CateringId AS CateringId, 'Reactivated' AS Action;
    END
    ELSE
    BEGIN
        -- Insert new favorite
        INSERT INTO t_sys_user_favorites (c_userid, c_catering_id)
        VALUES (@UserId, @CateringId);

        SELECT @UserId AS UserId, @CateringId AS CateringId, 'Added' AS Action;
    END
END
GO

-- =============================================
-- STORED PROCEDURE: REMOVE FROM FAVORITES
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_RemoveFavorite')
    DROP PROCEDURE sp_User_RemoveFavorite;
GO

CREATE PROCEDURE sp_User_RemoveFavorite
    @UserId BIGINT,
    @CateringId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t_sys_user_favorites
    SET c_is_active = 0,
        c_removed_date = GETDATE()
    WHERE c_userid = @UserId
      AND c_catering_id = @CateringId
      AND c_is_active = 1;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- STORED PROCEDURE: GET USER FAVORITES
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_GetFavorites')
    DROP PROCEDURE sp_User_GetFavorites;
GO

CREATE PROCEDURE sp_User_GetFavorites
    @UserId BIGINT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get favorites with catering details
    SELECT
        f.c_favorite_id AS FavoriteId,
        f.c_catering_id AS CateringId,
        c.c_catering_name AS CateringName,
        c.c_logo_path AS LogoUrl,
        c.c_cityid AS CityId,
        city.c_name AS CityName,
        c.c_min_order_value AS MinOrderValue,
        c.c_is_online AS IsOnline,
        c.c_is_verified AS IsVerified,
        f.c_added_date AS AddedDate,

        -- Calculate average rating
        (
            SELECT ISNULL(AVG(CAST(r.c_rating AS FLOAT)), 0)
            FROM t_sys_reviews r
            WHERE r.c_catering_id = c.c_ownerid
              AND r.c_is_approved = 1
        ) AS AverageRating,

        -- Get review count
        (
            SELECT COUNT(*)
            FROM t_sys_reviews r
            WHERE r.c_catering_id = c.c_ownerid
              AND r.c_is_approved = 1
        ) AS ReviewCount,

        -- Get total orders count
        (
            SELECT COUNT(*)
            FROM t_sys_orders o
            WHERE o.c_ownerid = c.c_ownerid
              AND o.c_order_status = 'Completed'
        ) AS CompletedOrders

    FROM t_sys_user_favorites f
    INNER JOIN t_sys_catering_owner c ON f.c_catering_id = c.c_ownerid
    LEFT JOIN t_sys_city city ON c.c_cityid = city.c_cityid
    WHERE f.c_userid = @UserId
      AND f.c_is_active = 1
      AND c.c_is_active = 1  -- Only show active caterings
    ORDER BY f.c_added_date DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Get total count
    SELECT COUNT(*) AS TotalCount
    FROM t_sys_user_favorites f
    INNER JOIN t_sys_catering_owner c ON f.c_catering_id = c.c_ownerid
    WHERE f.c_userid = @UserId
      AND f.c_is_active = 1
      AND c.c_is_active = 1;
END
GO

-- =============================================
-- STORED PROCEDURE: CHECK IF FAVORITE EXISTS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_IsFavorite')
    DROP PROCEDURE sp_User_IsFavorite;
GO

CREATE PROCEDURE sp_User_IsFavorite
    @UserId BIGINT,
    @CateringId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CASE WHEN EXISTS (
            SELECT 1 FROM t_sys_user_favorites
            WHERE c_userid = @UserId
              AND c_catering_id = @CateringId
              AND c_is_active = 1
        ) THEN 1 ELSE 0 END AS IsFavorite;
END
GO

-- =============================================
-- STORED PROCEDURE: GET FAVORITES COUNT
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_GetFavoritesCount')
    DROP PROCEDURE sp_User_GetFavoritesCount;
GO

CREATE PROCEDURE sp_User_GetFavoritesCount
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalFavorites
    FROM t_sys_user_favorites
    WHERE c_userid = @UserId
      AND c_is_active = 1;
END
GO

-- =============================================
-- STORED PROCEDURE: GET MULTIPLE FAVORITE STATUS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_GetFavoriteStatus')
    DROP PROCEDURE sp_User_GetFavoriteStatus;
GO

CREATE PROCEDURE sp_User_GetFavoriteStatus
    @UserId BIGINT,
    @CateringIds NVARCHAR(MAX)  -- Comma-separated list of catering IDs
AS
BEGIN
    SET NOCOUNT ON;

    -- Parse comma-separated IDs into table
    DECLARE @CateringIdTable TABLE (CateringId BIGINT);

    INSERT INTO @CateringIdTable (CateringId)
    SELECT CAST(value AS BIGINT)
    FROM STRING_SPLIT(@CateringIds, ',')
    WHERE value <> '';

    -- Return favorite status for each catering
    SELECT
        c.CateringId,
        CASE WHEN f.c_favorite_id IS NOT NULL THEN 1 ELSE 0 END AS IsFavorite
    FROM @CateringIdTable c
    LEFT JOIN t_sys_user_favorites f
        ON f.c_userid = @UserId
        AND f.c_catering_id = c.CateringId
        AND f.c_is_active = 1;
END
GO

-- =============================================
-- SUMMARY
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Wishlist/Favorites Feature Created:';
PRINT '========================================';
PRINT '1. t_sys_user_favorites - User favorites table';
PRINT '2. sp_User_AddFavorite - Add to favorites';
PRINT '3. sp_User_RemoveFavorite - Remove from favorites';
PRINT '4. sp_User_GetFavorites - Get user favorites with pagination';
PRINT '5. sp_User_IsFavorite - Check if specific catering is favorited';
PRINT '6. sp_User_GetFavoritesCount - Get total favorites count';
PRINT '7. sp_User_GetFavoriteStatus - Batch check favorite status';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Create FavoritesRepository in backend';
PRINT '2. Create FavoritesController with endpoints';
PRINT '3. Implement frontend wishlist page';
PRINT '4. Add heart icon to catering cards';
PRINT '';
GO
