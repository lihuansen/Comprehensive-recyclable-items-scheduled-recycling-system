-- ==============================================================================
-- 添加运输单指定基地工作人员字段 & 更新基地工作人员状态约束
-- Add AssignedWorkerID to TransportationOrders & Update SortingCenterWorkers CurrentStatus constraint
--
-- 用途: 
--   1. 运输单关联指定的基地工作人员
--   2. 基地工作人员状态增加 '空闲' 和 '工作中' 值
--
-- 作者: System
-- 创建日期: 2026-03-14
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- 1. 添加 AssignedWorkerID 列到 TransportationOrders 表
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'TransportationOrders' AND COLUMN_NAME = 'AssignedWorkerID'
)
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [AssignedWorkerID] INT NULL;

    PRINT 'AssignedWorkerID 列已添加到 TransportationOrders 表';
END
ELSE
BEGIN
    PRINT 'AssignedWorkerID 列已存在于 TransportationOrders 表';
END
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TransportationOrders_AssignedWorkerID')
BEGIN
    CREATE INDEX IX_TransportationOrders_AssignedWorkerID 
    ON [dbo].[TransportationOrders]([AssignedWorkerID]);
    PRINT 'AssignedWorkerID 索引创建成功';
END
GO

-- ==============================================================================
-- 2. 更新 SortingCenterWorkers 表的 CurrentStatus 约束
--    增加 '空闲' 和 '工作中' 状态值
-- ==============================================================================
IF EXISTS (
    SELECT * FROM sys.check_constraints 
    WHERE name = 'CK_SortingCenterWorkers_CurrentStatus'
)
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    DROP CONSTRAINT CK_SortingCenterWorkers_CurrentStatus;
    PRINT '旧的 CurrentStatus 约束已删除';
END
GO

ALTER TABLE [dbo].[SortingCenterWorkers]
ADD CONSTRAINT CK_SortingCenterWorkers_CurrentStatus 
CHECK ([CurrentStatus] IN (N'待命', N'分拣中', N'休息', N'离岗', N'离线', N'空闲', N'工作中'));
PRINT '新的 CurrentStatus 约束已创建（包含 空闲 和 工作中）';
GO

-- ==============================================================================
-- 3. 将现有 '待命' 状态的工作人员更新为 '空闲'
-- ==============================================================================
UPDATE [dbo].[SortingCenterWorkers]
SET [CurrentStatus] = N'空闲'
WHERE [CurrentStatus] = N'待命';
PRINT '已将现有待命状态更新为空闲';
GO

PRINT '迁移脚本执行完成';
GO
