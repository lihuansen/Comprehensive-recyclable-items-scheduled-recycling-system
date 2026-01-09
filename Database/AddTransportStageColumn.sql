-- ==============================================================================
-- 添加运输阶段跟踪字段
-- Add Transport Stage Tracking Column
-- 
-- 用途: 为运输单表添加TransportStage字段，用于跟踪运输中的详细阶段
-- Purpose: Add TransportStage column to track detailed stages during transportation
--
-- 作者: System
-- 创建日期: 2026-01-09
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- 添加 TransportStage 字段和相关时间戳字段
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'TransportStage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [TransportStage] NVARCHAR(50) NULL; -- 运输阶段（确认取货地点、到达取货地点、装货完毕、确认送货地点、到达送货地点）
    
    PRINT 'TransportStage 字段添加成功';
END
ELSE
BEGIN
    PRINT 'TransportStage 字段已存在';
END
GO

-- 添加取货地点确认时间
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'PickupConfirmedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [PickupConfirmedDate] DATETIME2 NULL; -- 确认取货地点时间
    
    PRINT 'PickupConfirmedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT 'PickupConfirmedDate 字段已存在';
END
GO

-- 添加到达取货地点时间（重命名 PickupDate 的语义，但为了兼容性保留原字段，添加新字段）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtPickupDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ArrivedAtPickupDate] DATETIME2 NULL; -- 到达取货地点时间
    
    PRINT 'ArrivedAtPickupDate 字段添加成功';
END
ELSE
BEGIN
    PRINT 'ArrivedAtPickupDate 字段已存在';
END
GO

-- 添加装货完毕时间
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'LoadingCompletedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [LoadingCompletedDate] DATETIME2 NULL; -- 装货完毕时间
    
    PRINT 'LoadingCompletedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT 'LoadingCompletedDate 字段已存在';
END
GO

-- 添加送货地点确认时间
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'DeliveryConfirmedDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [DeliveryConfirmedDate] DATETIME2 NULL; -- 确认送货地点时间
    
    PRINT 'DeliveryConfirmedDate 字段添加成功';
END
ELSE
BEGIN
    PRINT 'DeliveryConfirmedDate 字段已存在';
END
GO

-- 添加到达送货地点时间（重命名 DeliveryDate 的语义，但为了兼容性保留原字段，添加新字段）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ArrivedAtDeliveryDate')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ArrivedAtDeliveryDate] DATETIME2 NULL; -- 到达送货地点时间
    
    PRINT 'ArrivedAtDeliveryDate 字段添加成功';
END
ELSE
BEGIN
    PRINT 'ArrivedAtDeliveryDate 字段已存在';
END
GO

-- 添加约束检查运输阶段的值
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_TransportationOrders_TransportStage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD CONSTRAINT CK_TransportationOrders_TransportStage 
        CHECK ([TransportStage] IS NULL OR [TransportStage] IN (
            N'确认取货地点', 
            N'到达取货地点', 
            N'装货完毕', 
            N'确认送货地点', 
            N'到达送货地点'
        ));
    
    PRINT 'TransportStage 约束添加成功';
END
ELSE
BEGIN
    PRINT 'TransportStage 约束已存在';
END
GO

PRINT '运输阶段跟踪字段添加完成';
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- TransportStage            : 运输阶段（运输中状态的详细子阶段）
--                             可选值：确认取货地点、到达取货地点、装货完毕、确认送货地点、到达送货地点
-- PickupConfirmedDate       : 确认取货地点的时间
-- ArrivedAtPickupDate       : 到达取货地点的时间
-- LoadingCompletedDate      : 装货完毕的时间
-- DeliveryConfirmedDate     : 确认送货地点的时间
-- ArrivedAtDeliveryDate     : 到达送货地点的时间
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. TransportStage 仅在 Status = '运输中' 时有效
-- 2. 运输阶段流转顺序：
--    确认取货地点 -> 到达取货地点 -> 装货完毕 -> 确认送货地点 -> 到达送货地点 -> 完成运输
-- 3. 每个阶段都会记录对应的时间戳
-- 4. 当运输单状态变为"已完成"时，TransportStage 应该为 NULL 或 '到达送货地点'
-- ==============================================================================
