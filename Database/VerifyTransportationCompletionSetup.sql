-- ==============================================================================
-- 验证运输完成功能所需的数据库设置
-- Verify Transportation Completion Database Setup
-- 
-- 用途: 验证TransportationOrders表包含运输完成功能所需的所有字段
-- Purpose: Verify TransportationOrders table has all required fields for completion
--
-- 说明: 此脚本检查但不修改数据库，用于诊断问题
-- Note: This script only checks, does not modify the database, for diagnostics
--
-- 执行说明 / Execution Instructions:
-- 1. 在SQL Server Management Studio (SSMS)中打开此脚本
-- 2. 连接到您的数据库服务器
-- 3. 确保选择了正确的数据库
-- 4. 点击"执行"按钮运行脚本
--
-- 创建日期: 2026-01-13
-- ==============================================================================

-- 检查数据库是否存在并使用
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
    PRINT '❌ 错误: 找不到数据库！';
    RETURN;
END
GO

PRINT '';
PRINT '======================================================================';
PRINT '运输完成功能数据库验证';
PRINT 'Transportation Completion Database Verification';
PRINT '======================================================================';
PRINT '';

-- ==============================================================================
-- 检查表是否存在
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    PRINT '❌ 错误: TransportationOrders 表不存在！';
    RETURN;
END

PRINT '✓ TransportationOrders 表存在';
PRINT '';

-- ==============================================================================
-- 检查运输完成所需的关键字段
-- ==============================================================================
PRINT '======================================================================';
PRINT '检查运输完成功能所需的字段:';
PRINT 'Checking required fields for transportation completion:';
PRINT '======================================================================';
PRINT '';

DECLARE @MissingFields TABLE (FieldName NVARCHAR(100), Description NVARCHAR(200));
DECLARE @HasIssues BIT = 0;

-- 检查Status字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'Status')
BEGIN
    INSERT INTO @MissingFields VALUES ('Status', N'状态字段（必需）');
    SET @HasIssues = 1;
END
ELSE
    PRINT '✓ Status 字段存在';

-- 检查DeliveryDate字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryDate')
BEGIN
    INSERT INTO @MissingFields VALUES ('DeliveryDate', N'送达时间（必需）');
    SET @HasIssues = 1;
END
ELSE
    PRINT '✓ DeliveryDate 字段存在';

-- 检查CompletedDate字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'CompletedDate')
BEGIN
    INSERT INTO @MissingFields VALUES ('CompletedDate', N'完成时间（必需）');
    SET @HasIssues = 1;
END
ELSE
    PRINT '✓ CompletedDate 字段存在';

-- 检查ActualWeight字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ActualWeight')
BEGIN
    INSERT INTO @MissingFields VALUES ('ActualWeight', N'实际重量（可选）');
    SET @HasIssues = 1;
END
ELSE
    PRINT '✓ ActualWeight 字段存在';

-- 检查TransportStage字段（可选，用于阶段跟踪）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
BEGIN
    PRINT '⚠ TransportStage 字段不存在（可选字段，但建议添加）';
END
ELSE
    PRINT '✓ TransportStage 字段存在';

-- 检查Stage字段（可选，用于实时阶段显示）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'Stage')
BEGIN
    PRINT '⚠ Stage 字段不存在（可选字段，但建议添加用于实时显示）';
    INSERT INTO @MissingFields VALUES ('Stage', N'运输阶段（推荐添加）');
END
ELSE
    PRINT '✓ Stage 字段存在';

PRINT '';

-- ==============================================================================
-- 显示缺失的字段
-- ==============================================================================
IF EXISTS (SELECT * FROM @MissingFields WHERE FieldName IN ('Status', 'DeliveryDate', 'CompletedDate', 'ActualWeight'))
BEGIN
    PRINT '======================================================================';
    PRINT '❌ 发现缺失的关键字段！';
    PRINT '❌ Critical fields are missing!';
    PRINT '======================================================================';
    PRINT '';
    SELECT FieldName AS '缺失字段', Description AS '说明' FROM @MissingFields
    WHERE FieldName IN ('Status', 'DeliveryDate', 'CompletedDate', 'ActualWeight');
    PRINT '';
    PRINT '请先执行基础建表脚本: Database/CreateTransportationOrdersTable.sql';
END
ELSE IF EXISTS (SELECT * FROM @MissingFields)
BEGIN
    PRINT '======================================================================';
    PRINT '⚠ 发现可选字段缺失';
    PRINT '⚠ Optional fields are missing';
    PRINT '======================================================================';
    PRINT '';
    SELECT FieldName AS '缺失字段', Description AS '说明' FROM @MissingFields;
    PRINT '';
    PRINT '建议执行以下脚本添加可选字段:';
    PRINT '1. Database/EnsureTransportStageColumns.sql - 添加TransportStage字段 (如果已存在此文件)';
    PRINT '2. Database/AddStageColumnToTransportationOrders.sql - 添加Stage字段 (此PR中包含)';
    PRINT '';
    PRINT 'Note: The Stage column script is included in this changeset.';
    PRINT 'The EnsureTransportStageColumns.sql script may already exist in your database setup.';
END
ELSE
BEGIN
    PRINT '======================================================================';
    PRINT '✓ 所有字段验证通过！';
    PRINT '✓ All fields verified successfully!';
    PRINT '======================================================================';
END

PRINT '';

-- ==============================================================================
-- 显示当前表结构
-- ==============================================================================
PRINT '======================================================================';
PRINT '当前 TransportationOrders 表结构:';
PRINT 'Current TransportationOrders table structure:';
PRINT '======================================================================';
PRINT '';

SELECT 
    COLUMN_NAME AS '字段名',
    DATA_TYPE AS '数据类型',
    CHARACTER_MAXIMUM_LENGTH AS '最大长度',
    IS_NULLABLE AS '可空',
    COLUMN_DEFAULT AS '默认值'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TransportationOrders'
    AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

PRINT '';

-- ==============================================================================
-- 测试数据查询（如果存在数据）
-- ==============================================================================
DECLARE @RecordCount INT;
SELECT @RecordCount = COUNT(*) FROM [dbo].[TransportationOrders];

IF @RecordCount > 0
BEGIN
    PRINT '======================================================================';
    PRINT '数据库中的运输单统计:';
    PRINT 'Transportation orders statistics:';
    PRINT '======================================================================';
    PRINT '';
    
    SELECT 
        Status AS '状态',
        COUNT(*) AS '数量'
    FROM [dbo].[TransportationOrders]
    GROUP BY Status
    ORDER BY Status;
    
    PRINT '';
    
    -- 如果Stage字段存在，显示阶段统计
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'Stage')
    BEGIN
        PRINT '运输阶段统计:';
        PRINT 'Transportation stage statistics:';
        PRINT '';
        
        SELECT 
            ISNULL(Stage, N'(无阶段)') AS '阶段',
            COUNT(*) AS '数量'
        FROM [dbo].[TransportationOrders]
        WHERE Status = N'运输中'
        GROUP BY Stage
        ORDER BY Stage;
        
        PRINT '';
    END
    
    -- 显示最近完成的运输单
    IF EXISTS (SELECT * FROM [dbo].[TransportationOrders] WHERE Status = N'已完成')
    BEGIN
        PRINT '最近完成的运输单（最多5条）:';
        PRINT 'Recently completed orders (up to 5):';
        PRINT '';
        
        SELECT TOP 5
            OrderNumber AS '运输单号',
            CompletedDate AS '完成时间',
            ActualWeight AS '实际重量',
            Status AS '状态'
        FROM [dbo].[TransportationOrders]
        WHERE Status = N'已完成'
        ORDER BY CompletedDate DESC;
        
        PRINT '';
    END
END
ELSE
BEGIN
    PRINT '======================================================================';
    PRINT 'ℹ 数据库中暂无运输单数据';
    PRINT 'ℹ No transportation orders in database yet';
    PRINT '======================================================================';
    PRINT '';
END

-- ==============================================================================
-- 功能验证总结
-- ==============================================================================
PRINT '======================================================================';
PRINT '运输完成功能验证总结:';
PRINT 'Transportation Completion Verification Summary:';
PRINT '======================================================================';
PRINT '';

IF @HasIssues = 1
BEGIN
    PRINT '❌ 运输完成功能可能无法正常工作';
    PRINT '❌ Transportation completion may not work properly';
    PRINT '';
    PRINT '请先执行必需的数据库设置脚本';
    PRINT 'Please execute required database setup scripts first';
END
ELSE IF EXISTS (SELECT * FROM @MissingFields)
BEGIN
    PRINT '⚠ 基本功能可用，但建议添加可选字段以获得完整功能';
    PRINT '⚠ Basic function available, but optional fields recommended for full features';
    PRINT '';
    PRINT '运输完成功能将正常工作，但无法跟踪详细的运输阶段';
    PRINT 'Completion will work, but detailed stage tracking unavailable';
END
ELSE
BEGIN
    PRINT '✓ 数据库设置完整，运输完成功能已就绪！';
    PRINT '✓ Database setup complete, transportation completion ready!';
    PRINT '';
    PRINT '运输完成功能说明:';
    PRINT 'Transportation completion functionality:';
    PRINT '1. 运输人员点击"运输完成"按钮';
    PRINT '   Transporter clicks "Complete Transportation" button';
    PRINT '2. 可选填写实际重量';
    PRINT '   Optionally enter actual weight';
    PRINT '3. 系统更新以下字段:';
    PRINT '   System updates following fields:';
    PRINT '   - Status → 已完成 (Completed)';
    PRINT '   - DeliveryDate → 当前时间';
    PRINT '   - CompletedDate → 当前时间';
    PRINT '   - ActualWeight → 填写的值（如有）';
    PRINT '   - TransportStage → NULL';
    PRINT '   - Stage → NULL';
END

PRINT '';
PRINT '======================================================================';
PRINT '验证完成！';
PRINT 'Verification completed!';
PRINT '======================================================================';
GO
