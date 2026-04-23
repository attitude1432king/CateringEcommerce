/*
 * Database Table: Banner Management
 * Purpose: Stores banner images that partners can upload to display on user homepage
 * PostgreSQL Compatible Version
 */

-- =============================================
-- Table: t_sys_catering_banners
-- Purpose: Banner management for catering owners
-- =============================================
CREATE TABLE IF NOT EXISTS t_sys_catering_banners (
    c_bannerid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_ownerid BIGINT NOT NULL,
    c_title VARCHAR(200) NOT NULL,
    c_description VARCHAR(500),
    c_linkurl VARCHAR(500),
    c_display_order INTEGER NOT NULL DEFAULT 0,
    c_isactive BOOLEAN NOT NULL DEFAULT TRUE,
    c_startdate TIMESTAMP,
    c_enddate TIMESTAMP,
    c_clickcount INTEGER NOT NULL DEFAULT 0,
    c_viewcount INTEGER NOT NULL DEFAULT 0,
    c_createddate TIMESTAMP NOT NULL DEFAULT NOW(),
    c_modifieddate TIMESTAMP,
    c_is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS ix_banner_ownerid ON t_sys_catering_banners(c_ownerid) WHERE c_is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS ix_banner_isactive ON t_sys_catering_banners(c_isactive, c_startdate, c_enddate) WHERE c_is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS ix_banner_displayorder ON t_sys_catering_banners(c_display_order, c_ownerid) WHERE c_is_deleted = FALSE;

