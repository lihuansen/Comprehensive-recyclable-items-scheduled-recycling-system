-- ==============================================================================
-- 从基地工作人员表移除通知追踪字段
-- Remove Notification Tracking Fields from SortingCenterWorkers Table
-- 
-- 用途: 删除用于持久化存储工作人员已查看的运输和仓库消息数量的字段
-- Purpose: Remove the fields used to persist viewed transport and warehouse 
--          message counts for workers
--
-- 作者: System
-- 创建日期: 2026-01-14
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- 删除通知追踪字段
-- ==============================================================================
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SortingCenterWorkers' 
           AND COLUMN_NAME = 'LastViewedTransportCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    DROP COLUMN [LastViewedTransportCount];
    
    PRINT '成功删除字段: LastViewedTransportCount';
END
ELSE
BEGIN
    PRINT '字段不存在: LastViewedTransportCount';
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SortingCenterWorkers' 
           AND COLUMN_NAME = 'LastViewedWarehouseCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    DROP COLUMN [LastViewedWarehouseCount];
    
    PRINT '成功删除字段: LastViewedWarehouseCount';
END
ELSE
BEGIN
    PRINT '字段不存在: LastViewedWarehouseCount';
END
GO

PRINT '通知追踪字段删除完成';
GO
