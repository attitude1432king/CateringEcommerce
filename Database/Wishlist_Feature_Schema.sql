/* =============================================
   WISHLIST / FAVORITES FEATURE (POSTGRESQL)
   ============================================= */

-- =============================================
-- TABLE: USER FAVORITES
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_user_favorites (
    c_favorite_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_userid BIGINT NOT NULL,
    c_ownerid BIGINT NOT NULL,

    -- Tracking
    c_added_date TIMESTAMP NOT NULL DEFAULT NOW(),
    c_is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit
    c_removed_date TIMESTAMP NULL,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Foreign Keys
    CONSTRAINT fk_favorites_user FOREIGN KEY (c_userid)
        REFERENCES t_sys_user (c_userid) ON DELETE CASCADE,

    CONSTRAINT fk_favorites_catering FOREIGN KEY (c_ownerid)
        REFERENCES t_sys_catering_owner (c_ownerid),

    -- Unique Constraint
    CONSTRAINT uq_user_catering_favorite UNIQUE (c_userid, c_ownerid)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_user ON t_sys_user_favorites (c_userid);
CREATE INDEX IF NOT EXISTS idx_catering ON t_sys_user_favorites (c_ownerid);
CREATE INDEX IF NOT EXISTS idx_active ON t_sys_user_favorites (c_is_active);
CREATE INDEX IF NOT EXISTS idx_added_date ON t_sys_user_favorites (c_added_date DESC);

-- =============================================
-- FUNCTION: CHECK IS FAVORITE
-- =============================================
CREATE OR REPLACE FUNCTION sp_User_AddFavorite(
    p_UserId BIGINT,
    p_CateringId BIGINT
)
RETURNS TABLE (
    UserId BIGINT,
    CateringId BIGINT,
    Action VARCHAR(20)
)
LANGUAGE plpgsql AS $$
BEGIN
    -- Check if already exists
    IF EXISTS (
        SELECT 1 FROM t_sys_user_favorites
        WHERE c_userid = p_UserId 
          AND c_ownerid = p_CateringId
    ) THEN

        -- If exists but inactive, reactivate
        UPDATE t_sys_user_favorites
        SET c_is_active = TRUE,
            c_added_date = NOW(),
            c_removed_date = NULL
        WHERE c_userid = p_UserId
          AND c_ownerid = p_CateringId
          AND c_is_active = FALSE;

        RETURN QUERY
        SELECT p_UserId, p_CateringId, 'Reactivated';

    ELSE
        -- Insert new favorite
        INSERT INTO t_sys_user_favorites (c_userid, c_ownerid)
        VALUES (p_UserId, p_CateringId);

        RETURN QUERY
        SELECT p_UserId, p_CateringId, 'Added';
    END IF;
END;
$$;

-- =============================================
-- FUNCTION: GET FAVORITES COUNT
-- =============================================
CREATE OR REPLACE FUNCTION sp_User_RemoveFavorite(
    p_UserId BIGINT,
    p_CateringId BIGINT
)
RETURNS TABLE (RowsAffected INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    v_RowCount INTEGER;
BEGIN
    UPDATE t_sys_user_favorites
    SET c_is_active = FALSE,
        c_removed_date = NOW()
    WHERE c_userid = p_UserId
      AND c_ownerid = p_CateringId
      AND c_is_active = TRUE;

    GET DIAGNOSTICS v_RowCount = ROW_COUNT;

    RETURN QUERY
    SELECT v_RowCount;
END;
$$;

-- =============================================
-- FUNCTION: GET MULTIPLE FAVORITE STATUS
-- =============================================
CREATE OR REPLACE FUNCTION sp_User_GetFavorites(
    p_UserId BIGINT,
    p_PageNumber INT DEFAULT 1,
    p_PageSize INT DEFAULT 20
)
RETURNS TABLE (
    FavoriteId BIGINT,
    CateringId BIGINT,
    CateringName TEXT,
    LogoUrl TEXT,
    CityId BIGINT,
    CityName TEXT,
    MinOrderValue NUMERIC,
    IsOnline BOOLEAN,
    IsVerified BOOLEAN,
    AddedDate TIMESTAMP,
    AverageRating NUMERIC,
    ReviewCount BIGINT,
    CompletedOrders BIGINT,
    TotalCount BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_Offset INT := (p_PageNumber - 1) * p_PageSize;
BEGIN
    RETURN QUERY
    SELECT
        f.c_favorite_id,
        f.c_ownerid,
        c.c_catering_name,
        c.c_logo_path,
        c.c_cityid,
        city.c_name,
        c.c_min_order_value,
        c.c_is_online,
        c.c_is_verified,
        f.c_added_date,

        -- Average Rating
        COALESCE((
            SELECT AVG(r.c_rating::NUMERIC)
            FROM t_sys_reviews r
            WHERE r.c_ownerid = c.c_ownerid
              AND r.c_is_approved = TRUE
        ), 0),

        -- Review Count
        (
            SELECT COUNT(*)
            FROM t_sys_reviews r
            WHERE r.c_ownerid = c.c_ownerid
              AND r.c_is_approved = TRUE
        ),

        -- Completed Orders
        (
            SELECT COUNT(*)
            FROM t_sys_orders o
            WHERE o.c_ownerid = c.c_ownerid
              AND o.c_order_status = 'Completed'
        ),

        -- Total Count (same for all rows)
        COUNT(*) OVER()

    FROM t_sys_user_favorites f
    INNER JOIN t_sys_catering_owner c ON f.c_ownerid = c.c_ownerid
    LEFT JOIN t_sys_city city ON c.c_cityid = city.c_cityid
    WHERE f.c_userid = p_UserId
      AND f.c_is_active = TRUE
      AND c.c_is_active = TRUE
    ORDER BY f.c_added_date DESC
    OFFSET v_Offset
    LIMIT p_PageSize;
END;
$$;

-- =============================================
-- STORED PROCEDURE: GET FAVORITES COUNT
-- =============================================

CREATE OR REPLACE FUNCTION sp_User_GetFavoritesCount(
    p_UserId BIGINT
)
RETURNS TABLE (TotalFavorites BIGINT)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT COUNT(*)
    FROM t_sys_user_favorites
    WHERE c_userid = p_UserId
      AND c_is_active = TRUE;
END;
$$;

-- =============================================
-- STORED PROCEDURE: GET MULTIPLE FAVORITE STATUS
-- =============================================


CREATE OR REPLACE FUNCTION sp_User_GetFavoriteStatus(
    p_UserId BIGINT,
    p_CateringIds TEXT  -- Comma-separated list
)
RETURNS TABLE (
    CateringId BIGINT,
    IsFavorite INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        c_id::BIGINT AS CateringId,
        CASE WHEN f.c_favorite_id IS NOT NULL THEN 1 ELSE 0 END
    FROM unnest(string_to_array(p_CateringIds, ',')) AS c_id
    LEFT JOIN t_sys_user_favorites f
        ON f.c_userid = p_UserId
        AND f.c_ownerid = c_id::BIGINT
        AND f.c_is_active = TRUE;
END;
$$;