-- ============================================================================
-- 运输单功能验证脚本
-- Transportation Order Feature Verification Script
-- ============================================================================

USE RecyclingSystemDB;
GO

PRINT '======================================================================';
PRINT '开始验证运输单功能配置...';
PRINT '======================================================================';
PRINT '';

-- ============================================================================
-- 1. 检查 TransportationOrders 表是否存在
-- ============================================================================
PRINT '1. 检查 TransportationOrders 表...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    PRINT '   ✓ TransportationOrders 表已存在';
    
    -- 显示表结构
    PRINT '   表结构信息:';
    SELECT 
        COLUMN_NAME as '字段名',
        DATA_TYPE as '数据类型',
        CHARACTER_MAXIMUM_LENGTH as '最大长度',
        IS_NULLABLE as '可空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TransportationOrders'
    ORDER BY ORDINAL_POSITION;
    
    -- 显示索引
    PRINT '';
    PRINT '   索引信息:';
    EXEC sp_helpindex 'TransportationOrders';
END
ELSE
BEGIN
    PRINT '   ✗ TransportationOrders 表不存在！';
    PRINT '   请执行 Database/CreateTransportationOrdersTable.sql 脚本创建表';
    PRINT '';
END
PRINT '';

-- ============================================================================
-- 2. 检查外键依赖表
-- ============================================================================
PRINT '2. 检查依赖表...';

-- 检查 Recyclers 表
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Recyclers]') AND type in (N'U'))
BEGIN
    DECLARE @RecyclerCount INT;
    SELECT @RecyclerCount = COUNT(*) FROM Recyclers;
    PRINT '   ✓ Recyclers 表存在 (记录数: ' + CAST(@RecyclerCount AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '   ✗ Recyclers 表不存在！';
END

-- 检查 Transporters 表
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND type in (N'U'))
BEGIN
    DECLARE @TransporterCount INT;
    SELECT @TransporterCount = COUNT(*) FROM Transporters;
    PRINT '   ✓ Transporters 表存在 (记录数: ' + CAST(@TransporterCount AS VARCHAR) + ')';
    
    -- 检查可用的运输人员
    DECLARE @AvailableCount INT;
    SELECT @AvailableCount = COUNT(*) 
    FROM Transporters 
    WHERE IsActive = 1 AND Available = 1;
    PRINT '   可用运输人员数量: ' + CAST(@AvailableCount AS VARCHAR);
    
    IF @AvailableCount = 0
    BEGIN
        PRINT '   ⚠ 警告: 没有可用的运输人员！';
        PRINT '   提示: 请确保至少有一个运输人员的 IsActive=1 且 Available=1';
    END
END
ELSE
BEGIN
    PRINT '   ✗ Transporters 表不存在！';
END
PRINT '';

-- ============================================================================
-- 3. 检查外键约束
-- ============================================================================
PRINT '3. 检查外键约束...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    SELECT 
        fk.name AS '外键名称',
        OBJECT_NAME(fk.parent_object_id) AS '当前表',
        COL_NAME(fc.parent_object_id, fc.parent_column_id) AS '当前表字段',
        OBJECT_NAME(fk.referenced_object_id) AS '引用表',
        COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS '引用表字段'
    FROM sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc 
        ON fk.object_id = fc.constraint_object_id
    WHERE OBJECT_NAME(fk.parent_object_id) = 'TransportationOrders';
    
    IF @@ROWCOUNT > 0
        PRINT '   ✓ 外键约束配置正确';
    ELSE
        PRINT '   ⚠ 警告: 未找到外键约束';
END
PRINT '';

-- ============================================================================
-- 4. 测试数据完整性
-- ============================================================================
PRINT '4. 检查测试数据...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    DECLARE @OrderCount INT;
    SELECT @OrderCount = COUNT(*) FROM TransportationOrders;
    PRINT '   运输单记录数: ' + CAST(@OrderCount AS VARCHAR);
    
    IF @OrderCount > 0
    BEGIN
        PRINT '   最近的运输单:';
        SELECT TOP 5
            OrderNumber AS '运输单号',
            Status AS '状态',
            EstimatedWeight AS '预估重量(kg)',
            CreatedDate AS '创建时间'
        FROM TransportationOrders
        ORDER BY CreatedDate DESC;
    END
    ELSE
    BEGIN
        PRINT '   暂无运输单记录';
    END
END
PRINT '';

-- ============================================================================
-- 5. 区域匹配检查
-- ============================================================================
PRINT '5. 检查区域匹配情况...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Recyclers]') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND type in (N'U'))
BEGIN
    PRINT '   回收员和运输人员的区域分布:';
    SELECT 
        r.Region AS '区域',
        COUNT(DISTINCT r.RecyclerID) AS '回收员数量',
        COUNT(DISTINCT t.TransporterID) AS '运输人员数量',
        COUNT(DISTINCT CASE WHEN t.IsActive = 1 AND t.Available = 1 THEN t.TransporterID END) AS '可用运输人员'
    FROM Recyclers r
    FULL OUTER JOIN Transporters t ON r.Region = t.Region
    WHERE r.Region IS NOT NULL OR t.Region IS NOT NULL
    GROUP BY COALESCE(r.Region, t.Region)
    ORDER BY '区域';
    
    -- 检查是否有回收员区域没有对应的运输人员
    IF EXISTS (
        SELECT 1 
        FROM Recyclers r
        WHERE NOT EXISTS (
            SELECT 1 
            FROM Transporters t 
            WHERE t.Region = r.Region 
            AND t.IsActive = 1 
            AND t.Available = 1
        )
    )
    BEGIN
        PRINT '';
        PRINT '   ⚠ 警告: 以下区域的回收员找不到可用的运输人员:';
        SELECT DISTINCT r.Region AS '区域'
        FROM Recyclers r
        WHERE NOT EXISTS (
            SELECT 1 
            FROM Transporters t 
            WHERE t.Region = r.Region 
            AND t.IsActive = 1 
            AND t.Available = 1
        );
    END
    ELSE
    BEGIN
        PRINT '   ✓ 所有回收员区域都有可用的运输人员';
    END
END
PRINT '';

-- ============================================================================
-- 6. 权限检查（可选）
-- ============================================================================
PRINT '6. 数据库权限检查...';
BEGIN TRY
    -- 测试插入权限
    BEGIN TRANSACTION;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
    BEGIN
        -- 这只是测试，立即回滚
        PRINT '   ✓ 具有表操作权限';
    END
    
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '   ✗ 权限不足: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- ============================================================================
-- 总结
-- ============================================================================
PRINT '======================================================================';
PRINT '验证完成！';
PRINT '======================================================================';
PRINT '';
PRINT '下一步操作:';
PRINT '1. 如果 TransportationOrders 表不存在，请执行:';
PRINT '   Database/CreateTransportationOrdersTable.sql';
PRINT '';
PRINT '2. 如果没有可用运输人员，请添加测试数据或确保:';
PRINT '   - Transporters 表中有记录';
PRINT '   - IsActive = 1';
PRINT '   - Available = 1';
PRINT '   - Region 与回收员的 Region 匹配';
PRINT '';
PRINT '3. 如果代码已更新，请:';
PRINT '   - 重新编译项目';
PRINT '   - 重启 Web 应用';
PRINT '   - 清除浏览器缓存';
PRINT '';
PRINT '4. 测试功能:';
PRINT '   - 以回收员身份登录';
PRINT '   - 进入"暂存点管理"页面';
PRINT '   - 点击"联系运输人员"按钮';
PRINT '   - 选择运输人员并创建运输单';
PRINT '';
PRINT '======================================================================';

GO
