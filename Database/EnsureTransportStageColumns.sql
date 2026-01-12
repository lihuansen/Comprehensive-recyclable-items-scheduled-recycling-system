-- ==============================================================================
-- 确保运输阶段跟踪字段存在 - 完整设置脚本
-- Ensure Transport Stage Tracking Columns Exist - Complete Setup Script
-- 
-- 用途: 一键式脚本，确保TransportationOrders表包含所有运输阶段跟踪必需的字段
-- Purpose: One-click script to ensure TransportationOrders table has all required fields for transport stage tracking
--
-- 执行说明 / Execution Instructions:
-- 1. 在SQL Server Management Studio (SSMS)中打开此脚本
-- 2. 连接到您的数据库服务器
-- 3. 确保选择了正确的数据库（RecyclingSystemDB 或 RecyclingDB）
-- 4. 点击"执行"按钮运行脚本
-- 5. 检查输出消息确认所有字段已成功添加
--
-- 注意: 此脚本可以安全地多次执行，不会重复添加字段或影响现有数据
-- Note: This script can be safely executed multiple times without duplicating fields or affecting existing data
--
-- 创建日期: 2026-01-12
-- ==============================================================================

-- 检查数据库是否存在并使用
-- Check if database exists and use it
IF DB_ID('RecyclingSystemDB') IS NOT NULL
BEGIN
    USE RecyclingSystemDB;
END
ELSE IF DB_ID('RecyclingDB') IS NOT NULL
BEGIN
    PRINT '警告: RecyclingSystemDB 数据库不存在，使用 RecyclingDB';
    PRINT 'Warning: RecyclingSystemDB database does not exist, using RecyclingDB';
    USE RecyclingDB;
END
ELSE
BEGIN
    PRINT '❌ 错误: 找不到 RecyclingSystemDB 或 RecyclingDB 数据库！';
    PRINT '❌ Error: Cannot find RecyclingSystemDB or RecyclingDB database!';
    PRINT '   请确认数据库名称并手动修改脚本开头的 USE 语句';
    PRINT '   Please confirm the database name and manually modify the USE statement at the beginning of the script';
    RAISERROR('Database not found', 16, 1);
    RETURN;
END
GO

PRINT '======================================================================';
PRINT '开始确保运输阶段跟踪字段存在...';
PRINT 'Starting to ensure transport stage tracking fields exist...';
PRINT '======================================================================';
PRINT '';

-- ==============================================================================
-- 步骤 1: 验证 TransportationOrders 表是否存在
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    PRINT '❌ 错误: TransportationOrders 表不存在！';
    PRINT '   请先执行 Database/CreateTransportationOrdersTable.sql 脚本创建表';
    PRINT '';
    PRINT '======================================================================';
    RETURN;
END

PRINT '✓ TransportationOrders 表已存在';
PRINT '';

-- ==============================================================================
-- 步骤 2: 添加 BaseContactPerson 字段（如果不存在）
-- ==============================================================================
PRINT '检查 BaseContactPerson 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPerson')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [BaseContactPerson] NVARCHAR(50) NULL;
    
    PRINT '  ✓ BaseContactPerson 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ BaseContactPerson 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 3: 添加 BaseContactPhone 字段（如果不存在）
-- ==============================================================================
PRINT '检查 BaseContactPhone 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPhone')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [BaseContactPhone] NVARCHAR(20) NULL;
    
    PRINT '  ✓ BaseContactPhone 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ BaseContactPhone 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 4: 添加 ItemTotalValue 字段（如果不存在）
-- ==============================================================================
PRINT '检查 ItemTotalValue 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ItemTotalValue')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ItemTotalValue] DECIMAL(10, 2) NULL DEFAULT 0;
    
    PRINT '  ✓ ItemTotalValue 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ ItemTotalValue 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 5: 修改 ContactPerson 为可空（如果需要）
-- ==============================================================================
PRINT '检查 ContactPerson 字段是否为可空...';
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') 
    AND name = 'ContactPerson' 
    AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ALTER COLUMN [ContactPerson] NVARCHAR(50) NULL;
    
    PRINT '  ✓ ContactPerson 字段修改为可空成功';
END
ELSE
BEGIN
    PRINT '  ℹ ContactPerson 字段已经是可空，跳过';
END
GO

-- ==============================================================================
-- 步骤 6: 修改 ContactPhone 为可空（如果需要）
-- ==============================================================================
PRINT '检查 ContactPhone 字段是否为可空...';
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') 
    AND name = 'ContactPhone' 
    AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ALTER COLUMN [ContactPhone] NVARCHAR(20) NULL;
    
    PRINT '  ✓ ContactPhone 字段修改为可空成功';
END
ELSE
BEGIN
    PRINT '  ℹ ContactPhone 字段已经是可空，跳过';
END
GO

-- ==============================================================================
-- 步骤 7: 添加 TransportStage 字段（核心字段）
-- ==============================================================================
PRINT '检查 TransportStage 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [TransportStage] NVARCHAR(50) NULL;
    
    PRINT '  ✓ TransportStage 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ TransportStage 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 8: 添加 PickupConfirmedDate 字段
-- ==============================================================================
PRINT '检查 PickupConfirmedDate 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'PickupConfirmedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [PickupConfirmedDate] DATETIME2 NULL;
    
    PRINT '  ✓ PickupConfirmedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ PickupConfirmedDate 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 9: 添加 ArrivedAtPickupDate 字段
-- ==============================================================================
PRINT '检查 ArrivedAtPickupDate 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtPickupDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ArrivedAtPickupDate] DATETIME2 NULL;
    
    PRINT '  ✓ ArrivedAtPickupDate 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ ArrivedAtPickupDate 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 10: 添加 LoadingCompletedDate 字段
-- ==============================================================================
PRINT '检查 LoadingCompletedDate 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'LoadingCompletedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [LoadingCompletedDate] DATETIME2 NULL;
    
    PRINT '  ✓ LoadingCompletedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ LoadingCompletedDate 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 11: 添加 DeliveryConfirmedDate 字段
-- ==============================================================================
PRINT '检查 DeliveryConfirmedDate 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryConfirmedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [DeliveryConfirmedDate] DATETIME2 NULL;
    
    PRINT '  ✓ DeliveryConfirmedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ DeliveryConfirmedDate 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 12: 添加 ArrivedAtDeliveryDate 字段
-- ==============================================================================
PRINT '检查 ArrivedAtDeliveryDate 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtDeliveryDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ArrivedAtDeliveryDate] DATETIME2 NULL;
    
    PRINT '  ✓ ArrivedAtDeliveryDate 字段添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ ArrivedAtDeliveryDate 字段已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 13: 添加 TransportStage 值约束（如果不存在）
-- ==============================================================================
PRINT '检查 TransportStage 约束...';
-- Note: Using Chinese values as they are required by the application business logic
-- The application uses these specific Chinese strings to track transport stages
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_TransportationOrders_TransportStage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD CONSTRAINT CK_TransportationOrders_TransportStage 
        CHECK ([TransportStage] IS NULL OR [TransportStage] IN (
            N'确认取货地点',  -- Confirm pickup location
            N'到达取货地点',  -- Arrive at pickup location
            N'装货完毕',      -- Loading completed
            N'确认送货地点',  -- Confirm delivery location
            N'到达送货地点'   -- Arrive at delivery location
        ));
    
    PRINT '  ✓ TransportStage 约束添加成功';
END
ELSE
BEGIN
    PRINT '  ℹ TransportStage 约束已存在，跳过';
END
GO

-- ==============================================================================
-- 步骤 14: 验证所有字段已成功添加
-- ==============================================================================
PRINT '';
PRINT '======================================================================';
PRINT '验证所有字段...';
PRINT '======================================================================';

DECLARE @MissingFields TABLE (FieldName NVARCHAR(100));

-- 检查所有必需字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
    INSERT INTO @MissingFields VALUES ('TransportStage');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'PickupConfirmedDate')
    INSERT INTO @MissingFields VALUES ('PickupConfirmedDate');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtPickupDate')
    INSERT INTO @MissingFields VALUES ('ArrivedAtPickupDate');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'LoadingCompletedDate')
    INSERT INTO @MissingFields VALUES ('LoadingCompletedDate');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryConfirmedDate')
    INSERT INTO @MissingFields VALUES ('DeliveryConfirmedDate');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtDeliveryDate')
    INSERT INTO @MissingFields VALUES ('ArrivedAtDeliveryDate');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPerson')
    INSERT INTO @MissingFields VALUES ('BaseContactPerson');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPhone')
    INSERT INTO @MissingFields VALUES ('BaseContactPhone');
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ItemTotalValue')
    INSERT INTO @MissingFields VALUES ('ItemTotalValue');

IF EXISTS (SELECT * FROM @MissingFields)
BEGIN
    PRINT '❌ 错误: 以下字段仍然缺失:';
    SELECT FieldName AS '缺失字段' FROM @MissingFields;
    PRINT '';
    PRINT '请检查数据库权限或联系数据库管理员';
END
ELSE
BEGIN
    PRINT '✓ 所有必需字段验证通过！';
    PRINT '';
    PRINT '当前 TransportationOrders 表包含以下字段:';
    SELECT 
        COLUMN_NAME AS '字段名',
        DATA_TYPE AS '数据类型',
        CHARACTER_MAXIMUM_LENGTH AS '最大长度',
        IS_NULLABLE AS '可空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TransportationOrders'
        AND TABLE_SCHEMA = 'dbo'
    ORDER BY ORDINAL_POSITION;
END

PRINT '';
PRINT '======================================================================';
PRINT '设置完成！';
PRINT 'Setup completed!';
PRINT '======================================================================';
PRINT '';
PRINT '运输阶段工作流程说明 / Transport Stage Workflow:';
PRINT '1. 待接单 (Pending)';
PRINT '2. 已接单 (Accepted) - 运输人员点击"接单"';
PRINT '3. 运输中 (In Transit) - 从确认取货地点开始';
PRINT '   3.1 确认取货地点 (Confirm Pickup Location)';
PRINT '   3.2 到达取货地点 (Arrive at Pickup)';
PRINT '   3.3 装货完毕 (Loading Completed)';
PRINT '   3.4 确认送货地点 (Confirm Delivery Location)';
PRINT '   3.5 到达送货地点 (Arrive at Delivery)';
PRINT '4. 已完成 (Completed) - 运输完成';
PRINT '';
PRINT '下一步操作 / Next Steps:';
PRINT '1. 重新编译项目';
PRINT '2. 重启 Web 应用';
PRINT '3. 清除浏览器缓存';
PRINT '4. 测试运输功能:';
PRINT '   - 以运输人员身份登录';
PRINT '   - 查看运输管理页面';
PRINT '   - 接单并测试各个运输阶段';
PRINT '';
PRINT '======================================================================';
GO
