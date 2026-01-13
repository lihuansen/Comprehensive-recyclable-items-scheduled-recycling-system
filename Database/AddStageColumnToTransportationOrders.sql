-- ==============================================================================
-- 添加Stage字段到运输单表
-- Add Stage Column to TransportationOrders Table
-- 
-- 用途: 确保TransportationOrders表包含Stage字段用于实时显示运输阶段
-- Purpose: Ensure TransportationOrders table has Stage column for real-time stage display
--
-- 说明: Stage字段用于实时跟踪运输阶段状态，与TransportStage字段功能相同但更新
-- Note: Stage column is used for real-time stage tracking, same function as TransportStage but newer
--
-- 执行说明 / Execution Instructions:
-- 1. 在SQL Server Management Studio (SSMS)中打开此脚本
-- 2. 连接到您的数据库服务器
-- 3. 确保选择了正确的数据库（RecyclingSystemDB 或 RecyclingDB）
-- 4. 点击"执行"按钮运行脚本
--
-- 注意: 此脚本可以安全地多次执行，不会重复添加字段或影响现有数据
-- Note: This script can be safely executed multiple times
--
-- 创建日期: 2026-01-13
-- ==============================================================================

-- 检查数据库是否存在并使用
-- Check if database exists and use it
-- Note: The system supports two database names for backward compatibility
-- RecyclingSystemDB is the newer standard, RecyclingDB is the legacy name
IF DB_ID('RecyclingSystemDB') IS NOT NULL
BEGIN
    USE RecyclingSystemDB;
    PRINT '使用数据库: RecyclingSystemDB';
END
ELSE IF DB_ID('RecyclingDB') IS NOT NULL
BEGIN
    USE RecyclingDB;
    PRINT '使用数据库: RecyclingDB';
END
ELSE
BEGIN
    PRINT '❌ 错误: 找不到 RecyclingSystemDB 或 RecyclingDB 数据库！';
    PRINT '❌ Error: Cannot find RecyclingSystemDB or RecyclingDB database!';
    RAISERROR('Database not found', 16, 1);
    RETURN;
END
GO

PRINT '';
PRINT '======================================================================';
PRINT '开始添加Stage字段...';
PRINT 'Starting to add Stage column...';
PRINT '======================================================================';
PRINT '';

-- ==============================================================================
-- 验证 TransportationOrders 表是否存在
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    PRINT '❌ 错误: TransportationOrders 表不存在！';
    PRINT '❌ Error: TransportationOrders table does not exist!';
    PRINT '';
    RETURN;
END

PRINT '✓ TransportationOrders 表已存在';
PRINT '';

-- ==============================================================================
-- 添加 Stage 字段（如果不存在）
-- ==============================================================================
PRINT '检查 Stage 字段...';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'Stage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [Stage] NVARCHAR(50) NULL;
    
    PRINT '  ✓ Stage 字段添加成功';
    PRINT '  ✓ Stage column added successfully';
END
ELSE
BEGIN
    PRINT '  ℹ Stage 字段已存在，跳过';
    PRINT '  ℹ Stage column already exists, skipping';
END
GO

-- ==============================================================================
-- 添加 Stage 值约束（如果不存在）
-- ==============================================================================
PRINT '';
PRINT '检查 Stage 约束...';
-- Note: Using Chinese values as they are required by the application business logic
-- The Stage column uses the same values as TransportStage for consistency
-- Both standardized and legacy terms are supported for backward compatibility with existing data
-- New records should use standardized terms, but legacy terms are accepted to avoid breaking existing workflows
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_TransportationOrders_Stage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD CONSTRAINT CK_TransportationOrders_Stage 
        CHECK ([Stage] IS NULL OR [Stage] IN (
            N'确认收货地点',  -- Confirm pickup location (standardized term)
            N'确认取货地点',  -- Confirm pickup location (legacy term - kept for compatibility)
            N'到达收货地点',  -- Arrive at pickup location (standardized term)
            N'到达取货地点',  -- Arrive at pickup location (legacy term - kept for compatibility)
            N'装货完成',      -- Loading completed (standardized term)
            N'装货完毕',      -- Loading completed (legacy term - kept for compatibility)
            N'确认送货地点',  -- Confirm delivery location
            N'到达送货地点'   -- Arrive at delivery location
        ));
    
    PRINT '  ✓ Stage 约束添加成功';
    PRINT '  ✓ Stage constraint added successfully';
END
ELSE
BEGIN
    PRINT '  ℹ Stage 约束已存在，跳过';
    PRINT '  ℹ Stage constraint already exists, skipping';
END
GO

-- ==============================================================================
-- 验证Stage字段已成功添加
-- ==============================================================================
PRINT '';
PRINT '======================================================================';
PRINT '验证Stage字段...';
PRINT 'Verifying Stage column...';
PRINT '======================================================================';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'Stage')
BEGIN
    PRINT '✓ Stage 字段验证通过！';
    PRINT '✓ Stage column verification passed!';
    PRINT '';
    
    -- 显示字段信息
    SELECT 
        COLUMN_NAME AS '字段名',
        DATA_TYPE AS '数据类型',
        CHARACTER_MAXIMUM_LENGTH AS '最大长度',
        IS_NULLABLE AS '可空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TransportationOrders'
        AND TABLE_SCHEMA = 'dbo'
        AND COLUMN_NAME = 'Stage';
END
ELSE
BEGIN
    PRINT '❌ 错误: Stage字段仍然缺失！';
    PRINT '❌ Error: Stage column is still missing!';
    PRINT '';
END

PRINT '';
PRINT '======================================================================';
PRINT '设置完成！';
PRINT 'Setup completed!';
PRINT '======================================================================';
PRINT '';
PRINT 'Stage字段说明 / Stage Column Description:';
PRINT '';
PRINT 'Stage字段用于实时跟踪运输阶段，允许的值包括:';
PRINT 'The Stage column tracks the real-time transportation stage, allowed values:';
PRINT '';
PRINT '1. NULL - 初始状态（待接单、已接单、已完成）';
PRINT '   NULL - Initial state (Pending, Accepted, Completed)';
PRINT '';
PRINT '2. 确认收货地点 / 确认取货地点 - 确认收货位置';
PRINT '   Confirm Pickup Location - Confirming pickup location';
PRINT '';
PRINT '3. 到达收货地点 / 到达取货地点 - 已到达收货位置';
PRINT '   Arrive at Pickup - Arrived at pickup location';
PRINT '';
PRINT '4. 装货完成 / 装货完毕 - 装货已完成';
PRINT '   Loading Completed - Loading finished';
PRINT '';
PRINT '5. 确认送货地点 - 确认送货位置';
PRINT '   Confirm Delivery Location - Confirming delivery location';
PRINT '';
PRINT '6. 到达送货地点 - 已到达送货位置';
PRINT '   Arrive at Delivery - Arrived at delivery location';
PRINT '';
PRINT '注意: Stage字段与TransportStage字段功能相同，但Stage是更新的字段';
PRINT 'Note: Stage column has the same function as TransportStage, but Stage is newer';
PRINT '';
PRINT '下一步操作 / Next Steps:';
PRINT '1. 重新编译项目（如果需要）';
PRINT '   Rebuild project (if needed)';
PRINT '2. 重启 Web 应用';
PRINT '   Restart web application';
PRINT '3. 测试运输完成功能:';
PRINT '   Test transportation completion:';
PRINT '   - 以运输人员身份登录';
PRINT '     Login as transporter';
PRINT '   - 完成整个运输流程直到"运输完成"';
PRINT '     Complete entire transport workflow until "Transportation Completed"';
PRINT '   - 验证数据正确保存到数据库';
PRINT '     Verify data is correctly saved to database';
PRINT '';
PRINT '======================================================================';
GO
