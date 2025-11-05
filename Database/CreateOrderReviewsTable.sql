-- 创建订单评价表（OrderReviews）
-- 用于存储用户对回收员的评价信息

-- 检查表是否已存在，如果不存在则创建
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderReviews] (
        [ReviewID] INT PRIMARY KEY IDENTITY(1,1),        -- 评价ID（自增主键）
        [OrderID] INT NOT NULL,                          -- 订单ID（外键，关联到 Appointments）
        [UserID] INT NOT NULL,                           -- 用户ID（外键，关联到 Users）
        [RecyclerID] INT NOT NULL,                       -- 回收员ID（外键，关联到 Recyclers）
        [StarRating] INT NOT NULL,                       -- 星级评分（1-5星）
        [ReviewText] NVARCHAR(500) NULL,                 -- 评价文字内容（支持中文）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 评价创建时间
        
        -- 外键约束
        CONSTRAINT [FK_OrderReviews_Appointments] FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT [FK_OrderReviews_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]),
        CONSTRAINT [FK_OrderReviews_Recyclers] FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
            
        -- 检查约束：确保评分在1-5之间
        CONSTRAINT [CK_OrderReviews_StarRating] CHECK ([StarRating] >= 1 AND [StarRating] <= 5)
    );

    -- 创建索引以提升查询性能
    CREATE INDEX [IX_OrderReviews_OrderID] ON [dbo].[OrderReviews]([OrderID]);
    CREATE INDEX [IX_OrderReviews_UserID] ON [dbo].[OrderReviews]([UserID]);
    CREATE INDEX [IX_OrderReviews_RecyclerID] ON [dbo].[OrderReviews]([RecyclerID]);
    CREATE INDEX [IX_OrderReviews_CreatedDate] ON [dbo].[OrderReviews]([CreatedDate]);

    PRINT 'OrderReviews 表创建成功';
END
ELSE
BEGIN
    PRINT 'OrderReviews 表已存在，跳过创建';
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
WHERE TABLE_NAME = 'OrderReviews'
ORDER BY ORDINAL_POSITION;
GO
