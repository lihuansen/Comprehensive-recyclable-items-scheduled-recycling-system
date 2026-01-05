-- ==============================================================================
-- 为基地工作人员表添加通知追踪字段
-- Add Notification Tracking Fields to SortingCenterWorkers Table
-- 
-- 用途: 持久化存储工作人员已查看的运输和仓库消息数量，解决重新登录后
--       已读消息重复显示的问题
-- Purpose: Persist viewed transport and warehouse message counts to fix
--          the issue of read messages showing as unread after re-login
--
-- 作者: System
-- 创建日期: 2026-01-05
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- 添加通知追踪字段
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SortingCenterWorkers' 
               AND COLUMN_NAME = 'LastViewedTransportCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    ADD [LastViewedTransportCount] INT NOT NULL DEFAULT 0;
    
    PRINT '成功添加字段: LastViewedTransportCount';
END
ELSE
BEGIN
    PRINT '字段已存在: LastViewedTransportCount';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SortingCenterWorkers' 
               AND COLUMN_NAME = 'LastViewedWarehouseCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    ADD [LastViewedWarehouseCount] INT NOT NULL DEFAULT 0;
    
    PRINT '成功添加字段: LastViewedWarehouseCount';
END
ELSE
BEGIN
    PRINT '字段已存在: LastViewedWarehouseCount';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- LastViewedTransportCount  : 工作人员最后一次查看运输管理时的运输中订单数量
--                             用于计算新增的运输订单数（未读数 = 当前总数 - 该值）
-- LastViewedWarehouseCount  : 工作人员最后一次查看仓库管理时的待处理入库数量
--                             用于计算新增的待处理项目数（未读数 = 当前总数 - 该值）
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. 这些字段在用户登录时从数据库加载到Session
-- 2. 用户访问运输管理页面时，更新LastViewedTransportCount为当前运输中订单总数
-- 3. 用户访问仓库管理页面时，更新LastViewedWarehouseCount为当前待处理项目总数
-- 4. 徽章只显示新增数量（当前总数 - 已查看数量）
-- 5. 默认值为0，表示所有消息都是未读的
-- ==============================================================================

PRINT '通知追踪字段添加完成';
GO
