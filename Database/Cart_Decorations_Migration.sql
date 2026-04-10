IF OBJECT_ID(N'[dbo].[t_sys_cart_decorations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[t_sys_cart_decorations] (
        [c_cart_decoration_id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [c_cartid] BIGINT NOT NULL,
        [c_decoration_id] BIGINT NOT NULL,
        [c_price] DECIMAL(10,2) NOT NULL DEFAULT 0,
        [c_createddate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_t_sys_cart_decorations_cart]
            FOREIGN KEY ([c_cartid]) REFERENCES [dbo].[t_sys_user_cart]([c_cartid]) ON DELETE CASCADE,
        CONSTRAINT [FK_t_sys_cart_decorations_decoration]
            FOREIGN KEY ([c_decoration_id]) REFERENCES [dbo].[t_sys_catering_decorations]([c_decoration_id]),
        CONSTRAINT [UQ_t_sys_cart_decorations_cart_decoration]
            UNIQUE ([c_cartid], [c_decoration_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_t_sys_cart_decorations_cartid'
      AND object_id = OBJECT_ID(N'[dbo].[t_sys_cart_decorations]')
)
BEGIN
    CREATE INDEX [IX_t_sys_cart_decorations_cartid]
        ON [dbo].[t_sys_cart_decorations]([c_cartid]);
END;
GO

INSERT INTO [dbo].[t_sys_cart_decorations] ([c_cartid], [c_decoration_id], [c_price], [c_createddate])
SELECT
    c.[c_cartid],
    c.[c_decoration_id],
    ISNULL(c.[c_decoration_amount], 0),
    ISNULL(c.[c_createddate], GETDATE())
FROM [dbo].[t_sys_user_cart] c
WHERE c.[c_decoration_id] IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[t_sys_cart_decorations] cd
      WHERE cd.[c_cartid] = c.[c_cartid]
        AND cd.[c_decoration_id] = c.[c_decoration_id]
  );
GO
