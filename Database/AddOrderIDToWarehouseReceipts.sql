-- ==============================================================================
-- 添加 OrderID 列到 WarehouseReceipts 表
-- Add OrderID Column to WarehouseReceipts Table
-- 
-- 用途: 记录入库单关联的运输单/订单ID，方便追溯来源
-- Purpose: Store the associated order ID for the warehouse receipt
--
-- 创建日期: 2026-03-13
-- ==============================================================================

USE RecyclingDB;
GO

-- 添加 OrderID 列
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WarehouseReceipts]') AND name = 'OrderID')
BEGIN
    ALTER TABLE [dbo].[WarehouseReceipts]
    ADD [OrderID] INT NULL;

    PRINT 'OrderID 列已成功添加到 WarehouseReceipts 表';
END
ELSE
BEGIN
    PRINT 'OrderID 列已存在于 WarehouseReceipts 表中';
END
GO
