-- ==============================================================================
-- 添加 ReceiptID 列到 Inventory 表
-- Add ReceiptID Column to Inventory Table
-- 
-- 用途: 记录库存记录对应的入库单编号，方便追溯来源
-- Purpose: Store the associated warehouse receipt ID for inventory records
--
-- 创建日期: 2026-03-13
-- ==============================================================================

USE RecyclingDB;
GO

-- 添加 ReceiptID 列
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND name = 'ReceiptID')
BEGIN
    ALTER TABLE [dbo].[Inventory]
    ADD [ReceiptID] INT NULL;

    PRINT 'ReceiptID 列已成功添加到 Inventory 表';
END
ELSE
BEGIN
    PRINT 'ReceiptID 列已存在于 Inventory 表中';
END
GO
