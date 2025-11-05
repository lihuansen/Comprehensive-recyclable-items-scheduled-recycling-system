-- 测试订单评价功能的数据库脚本
-- Test script for OrderReviews functionality

-- 1. 检查 OrderReviews 表是否存在
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT '✓ OrderReviews 表已存在'
END
ELSE
BEGIN
    PRINT '✗ 错误：OrderReviews 表不存在！请先运行 CreateOrderReviewsTable.sql'
END
GO

-- 2. 检查表结构
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT ''
    PRINT '=== OrderReviews 表结构 ==='
    
    SELECT 
        COLUMN_NAME AS '列名',
        DATA_TYPE AS '数据类型',
        CHARACTER_MAXIMUM_LENGTH AS '最大长度',
        IS_NULLABLE AS '可为空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OrderReviews'
    ORDER BY ORDINAL_POSITION;
END
GO

-- 3. 检查外键约束
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT ''
    PRINT '=== 外键约束 ==='
    
    SELECT 
        fk.name AS '约束名',
        OBJECT_NAME(fk.parent_object_id) AS '表名',
        COL_NAME(fc.parent_object_id, fc.parent_column_id) AS '列名',
        OBJECT_NAME(fk.referenced_object_id) AS '引用表',
        COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS '引用列'
    FROM sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc 
        ON fk.object_id = fc.constraint_object_id
    WHERE OBJECT_NAME(fk.parent_object_id) = 'OrderReviews';
END
GO

-- 4. 检查索引
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT ''
    PRINT '=== 索引 ==='
    
    SELECT 
        i.name AS '索引名',
        i.type_desc AS '索引类型',
        COL_NAME(ic.object_id, ic.column_id) AS '列名'
    FROM sys.indexes AS i
    INNER JOIN sys.index_columns AS ic 
        ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    WHERE i.object_id = OBJECT_ID('OrderReviews')
    ORDER BY i.name, ic.key_ordinal;
END
GO

-- 5. 测试插入中文数据
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT ''
    PRINT '=== 测试中文数据插入 ==='
    
    -- 检查是否有测试数据，如果有则跳过
    IF NOT EXISTS (SELECT 1 FROM OrderReviews WHERE ReviewText LIKE '%测试%')
    BEGIN
        -- 获取一个有效的测试数据（假设存在相关记录）
        DECLARE @TestOrderID INT
        DECLARE @TestUserID INT
        DECLARE @TestRecyclerID INT
        
        -- 尝试获取一个已完成的订单用于测试
        SELECT TOP 1 
            @TestOrderID = AppointmentID,
            @TestUserID = UserID,
            @TestRecyclerID = ISNULL(RecyclerID, 1)  -- 如果没有回收员，使用1
        FROM Appointments 
        WHERE Status = N'已完成' 
            AND RecyclerID IS NOT NULL
            AND NOT EXISTS (
                SELECT 1 FROM OrderReviews 
                WHERE OrderID = Appointments.AppointmentID
            );
        
        IF @TestOrderID IS NOT NULL
        BEGIN
            -- 插入测试评价（包含中文）
            INSERT INTO OrderReviews (OrderID, UserID, RecyclerID, StarRating, ReviewText, CreatedDate)
            VALUES (@TestOrderID, @TestUserID, @TestRecyclerID, 5, N'这是一条测试评价，包含中文字符：优秀、满意、五星好评！', GETDATE());
            
            -- 验证插入的数据
            IF EXISTS (
                SELECT 1 FROM OrderReviews 
                WHERE ReviewText LIKE N'%测试评价%' 
                    AND ReviewText LIKE N'%中文%'
                    AND ReviewText NOT LIKE '%??%'  -- 确保没有乱码
            )
            BEGIN
                PRINT '✓ 中文数据插入成功，无乱码'
                
                -- 删除测试数据
                DELETE FROM OrderReviews WHERE ReviewText LIKE N'%测试评价%';
                PRINT '✓ 测试数据已清理'
            END
            ELSE
            BEGIN
                PRINT '✗ 警告：中文数据可能存在乱码问题'
            END
        END
        ELSE
        BEGIN
            PRINT '⚠ 跳过：没有找到合适的已完成订单用于测试'
        END
    END
    ELSE
    BEGIN
        PRINT '⚠ 跳过：已存在测试数据'
    END
END
GO

-- 6. 统计现有评价数据
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    PRINT ''
    PRINT '=== 评价数据统计 ==='
    
    SELECT 
        COUNT(*) AS '总评价数',
        AVG(CAST(StarRating AS FLOAT)) AS '平均评分',
        COUNT(DISTINCT RecyclerID) AS '被评价回收员数',
        COUNT(DISTINCT UserID) AS '评价用户数'
    FROM OrderReviews;
    
    PRINT ''
    PRINT '=== 评分分布 ==='
    SELECT 
        StarRating AS '星级',
        COUNT(*) AS '数量'
    FROM OrderReviews
    GROUP BY StarRating
    ORDER BY StarRating DESC;
END
GO

PRINT ''
PRINT '=== 测试完成 ==='
PRINT '如果所有检查都通过，订单评价功能已准备就绪。'
GO
