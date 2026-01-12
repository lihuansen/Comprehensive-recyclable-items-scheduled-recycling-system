-- ==============================================================================
-- 检查运输阶段跟踪字段是否存在
-- Check if Transport Stage Tracking Columns Exist
-- 
-- 用途: 快速检查TransportationOrders表是否包含运输阶段跟踪所需的字段
-- Purpose: Quickly check if TransportationOrders table has required fields for transport stage tracking
--
-- 使用说明 / Instructions:
-- 1. 在SQL Server Management Studio (SSMS)中打开此脚本
-- 2. 连接到您的数据库服务器
-- 3. 确保选择了正确的数据库
-- 4. 点击"执行"按钮运行脚本
-- 5. 查看输出结果
--
-- 如果某些字段不存在，请运行 EnsureTransportStageColumns.sql 或 AddTransportStageColumn.sql
-- If some fields don't exist, please run EnsureTransportStageColumns.sql or AddTransportStageColumn.sql
--
-- 创建日期: 2026-01-12
-- ==============================================================================

USE RecyclingSystemDB;
GO

PRINT '======================================================================';
PRINT '检查运输阶段跟踪字段...';
PRINT 'Checking transport stage tracking fields...';
PRINT '======================================================================';
PRINT '';

-- 检查 TransportStage 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
BEGIN
    PRINT '✓ TransportStage 字段存在';
    PRINT '✓ TransportStage column exists';
END
ELSE
BEGIN
    PRINT '✗ TransportStage 字段不存在！';
    PRINT '✗ TransportStage column does NOT exist!';
END
PRINT '';

-- 检查 PickupConfirmedDate 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'PickupConfirmedDate')
BEGIN
    PRINT '✓ PickupConfirmedDate 字段存在';
    PRINT '✓ PickupConfirmedDate column exists';
END
ELSE
BEGIN
    PRINT '✗ PickupConfirmedDate 字段不存在！';
    PRINT '✗ PickupConfirmedDate column does NOT exist!';
END
PRINT '';

-- 检查 ArrivedAtPickupDate 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtPickupDate')
BEGIN
    PRINT '✓ ArrivedAtPickupDate 字段存在';
    PRINT '✓ ArrivedAtPickupDate column exists';
END
ELSE
BEGIN
    PRINT '✗ ArrivedAtPickupDate 字段不存在！';
    PRINT '✗ ArrivedAtPickupDate column does NOT exist!';
END
PRINT '';

-- 检查 LoadingCompletedDate 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'LoadingCompletedDate')
BEGIN
    PRINT '✓ LoadingCompletedDate 字段存在';
    PRINT '✓ LoadingCompletedDate column exists';
END
ELSE
BEGIN
    PRINT '✗ LoadingCompletedDate 字段不存在！';
    PRINT '✗ LoadingCompletedDate column does NOT exist!';
END
PRINT '';

-- 检查 DeliveryConfirmedDate 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryConfirmedDate')
BEGIN
    PRINT '✓ DeliveryConfirmedDate 字段存在';
    PRINT '✓ DeliveryConfirmedDate column exists';
END
ELSE
BEGIN
    PRINT '✗ DeliveryConfirmedDate 字段不存在！';
    PRINT '✗ DeliveryConfirmedDate column does NOT exist!';
END
PRINT '';

-- 检查 ArrivedAtDeliveryDate 字段
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtDeliveryDate')
BEGIN
    PRINT '✓ ArrivedAtDeliveryDate 字段存在';
    PRINT '✓ ArrivedAtDeliveryDate column exists';
END
ELSE
BEGIN
    PRINT '✗ ArrivedAtDeliveryDate 字段不存在！';
    PRINT '✗ ArrivedAtDeliveryDate column does NOT exist!';
END
PRINT '';

PRINT '======================================================================';
PRINT '检查完成 / Check complete';
PRINT '======================================================================';
PRINT '';
PRINT '如果有字段不存在，请运行以下脚本之一来添加这些字段：';
PRINT 'If any fields do not exist, please run one of the following scripts to add them:';
PRINT '  - EnsureTransportStageColumns.sql (推荐 / Recommended)';
PRINT '  - AddTransportStageColumn.sql';
PRINT '';

-- 显示统计信息
DECLARE @MissingCount INT = 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
    SET @MissingCount = @MissingCount + 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'PickupConfirmedDate')
    SET @MissingCount = @MissingCount + 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtPickupDate')
    SET @MissingCount = @MissingCount + 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'LoadingCompletedDate')
    SET @MissingCount = @MissingCount + 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryConfirmedDate')
    SET @MissingCount = @MissingCount + 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtDeliveryDate')
    SET @MissingCount = @MissingCount + 1;

IF @MissingCount = 0
BEGIN
    PRINT '✓✓✓ 所有必需字段都存在！系统可以正常使用完整的运输阶段跟踪功能。';
    PRINT '✓✓✓ All required fields exist! The system can use full transport stage tracking functionality.';
END
ELSE
BEGIN
    PRINT '⚠⚠⚠ 缺少 ' + CAST(@MissingCount AS VARCHAR) + ' 个字段！建议运行数据库迁移脚本。';
    PRINT '⚠⚠⚠ Missing ' + CAST(@MissingCount AS VARCHAR) + ' fields! Recommend running database migration script.';
    PRINT '';
    PRINT '当前系统处于向后兼容模式，可以工作但无法使用详细的运输阶段跟踪功能。';
    PRINT 'System is currently in backward compatibility mode, it works but cannot use detailed stage tracking.';
END
GO
