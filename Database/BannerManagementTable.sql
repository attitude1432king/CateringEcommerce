-- =============================================
-- Banner Management Table
-- Description: Stores banner images that partners can upload to display on user homepage
-- =============================================

CREATE TABLE t_sys_catering_banners (
    c_bannerid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_ownerid BIGINT NOT NULL,
    c_title NVARCHAR(200) NOT NULL,
    c_description NVARCHAR(500) NULL,
    c_linkurl NVARCHAR(500) NULL,
    c_display_order INT NOT NULL DEFAULT 0,
    c_isactive BIT NOT NULL DEFAULT 1,
    c_startdate DATETIME NULL,
    c_enddate DATETIME NULL,
    c_clickcount INT NOT NULL DEFAULT 0,
    c_viewcount INT NOT NULL DEFAULT 0,
    c_createddate DATETIME NOT NULL DEFAULT GETDATE(),
    c_modifieddate BIGINT NULL,
    c_is_deleted BIT NOT NULL DEFAULT 0,
);

-- Index for better query performance
CREATE INDEX IX_Banner_OwnerPKID ON t_sys_catering_banners(c_ownerid) WHERE IsDeleted = 0;
CREATE INDEX IX_Banner_IsActive ON t_sys_catering_banners(c_isactive, c_startdate, c_enddate) WHERE IsDeleted = 0;
CREATE INDEX IX_Banner_DisplayOrder ON t_sys_catering_banners(c_display_order, c_ownerid) WHERE IsDeleted = 0;


-- Index for better query performance
CREATE INDEX IX_Banner_OwnerPKID ON t_sys_catering_banners(OwnerPKID) WHERE IsDeleted = 0;
CREATE INDEX IX_Banner_IsActive ON t_sys_catering_banners(IsActive, StartDate, EndDate) WHERE IsDeleted = 0;
CREATE INDEX IX_Banner_DisplayOrder ON t_sys_catering_banners(DisplayOrder, OwnerPKID) WHERE IsDeleted = 0;

GO
