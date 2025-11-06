-- 创建库存表（Inventory）
-- 用于存储回收员的库存管理信息

-- 检查表是否已存在，如果不存在则创建
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Inventory] (
        [InventoryID] INT PRIMARY KEY IDENTITY(1,1),     -- 库存ID（自增主键）
        [OrderID] INT NOT NULL,                          -- 订单ID（外键，关联到 Appointments）
        [CategoryKey] NVARCHAR(50) NOT NULL,             -- 品类键名（如 "glass", "metal"）
        [CategoryName] NVARCHAR(50) NOT NULL,            -- 品类名称（如 "玻璃", "金属"）
        [Weight] DECIMAL(10, 2) NOT NULL,                -- 重量（单位：kg）
        [Price] DECIMAL(10, 2) NULL,                     -- 价格（回收价格）
        [RecyclerID] INT NOT NULL,                       -- 回收员ID（外键，关联到 Recyclers）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        
        -- 外键约束
        CONSTRAINT [FK_Inventory_Appointments] FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT [FK_Inventory_Recyclers] FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
            
        -- 检查约束：确保重量为正数
        CONSTRAINT [CK_Inventory_Weight] CHECK ([Weight] > 0),
        -- 检查约束：确保价格为非负数（如果不为NULL）
        CONSTRAINT [CK_Inventory_Price] CHECK ([Price] IS NULL OR [Price] >= 0)
    );

    -- 创建索引以提升查询性能
    CREATE INDEX [IX_Inventory_OrderID] ON [dbo].[Inventory]([OrderID]);
    CREATE INDEX [IX_Inventory_RecyclerID] ON [dbo].[Inventory]([RecyclerID]);
    CREATE INDEX [IX_Inventory_CategoryKey] ON [dbo].[Inventory]([CategoryKey]);
    CREATE INDEX [IX_Inventory_CreatedDate] ON [dbo].[Inventory]([CreatedDate]);

    PRINT 'Inventory 表创建成功';
END
ELSE
BEGIN
    PRINT 'Inventory 表已存在，跳过创建';
END
GO

-- 验证表结构
SELECT 
    COLUMN_NAME AS '列名',
    DATA_TYPE AS '数据类型',
    CHARACTER_MAXIMUM_LENGTH AS '最大长度',
    IS_NULLABLE AS '可为空',
    COLUMN_DEFAULT AS '默认值'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Inventory'
ORDER BY ORDINAL_POSITION;
GO
