-- ==============================================================================
-- 入库单表（WarehouseReceipts）建表脚本
-- Warehouse Receipts Table Creation Script
-- 
-- 用途: 存储运输至基地的可回收物入库记录
-- Purpose: Store warehouse receipt records for recyclable items delivered to base
--
-- 作者: System
-- 创建日期: 2026-01-04
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- WarehouseReceipts 表（入库单表）
-- 实体类: recycling.Model.WarehouseReceipts
-- 用途: 记录从运输单到基地仓库的入库信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WarehouseReceipts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WarehouseReceipts] (
        [ReceiptID] INT PRIMARY KEY IDENTITY(1,1),            -- 入库单ID（自增主键）
        [ReceiptNumber] NVARCHAR(50) NOT NULL UNIQUE,         -- 入库单号（唯一，格式：WR+年月日+序号）
        [TransportOrderID] INT NOT NULL,                      -- 运输单ID（外键）
        [RecyclerID] INT NOT NULL,                            -- 回收员ID（外键）
        [WorkerID] INT NOT NULL,                              -- 处理入库的基地人员ID（外键）
        [TotalWeight] DECIMAL(10, 2) NOT NULL,                -- 入库总重量（kg）
        [ItemCategories] NVARCHAR(MAX) NULL,                  -- 物品类别（JSON格式存储）
        [Status] NVARCHAR(20) NOT NULL DEFAULT N'已入库',     -- 状态（已入库、已取消）
        [Notes] NVARCHAR(500) NULL,                           -- 备注信息
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),   -- 创建时间（入库时间）
        [CreatedBy] INT NOT NULL,                             -- 创建人（基地人员ID）
        
        -- 约束
        CONSTRAINT FK_WarehouseReceipts_TransportationOrders FOREIGN KEY ([TransportOrderID]) 
            REFERENCES [dbo].[TransportationOrders]([TransportOrderID]),
        CONSTRAINT FK_WarehouseReceipts_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT FK_WarehouseReceipts_Workers FOREIGN KEY ([WorkerID]) 
            REFERENCES [dbo].[SortingCenterWorkers]([WorkerID]),
        CONSTRAINT CK_WarehouseReceipts_Status 
            CHECK ([Status] IN (N'已入库', N'已取消')),
        CONSTRAINT CK_WarehouseReceipts_TotalWeight 
            CHECK ([TotalWeight] > 0)
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_WarehouseReceipts_ReceiptNumber ON [dbo].[WarehouseReceipts]([ReceiptNumber]);
    CREATE INDEX IX_WarehouseReceipts_TransportOrderID ON [dbo].[WarehouseReceipts]([TransportOrderID]);
    CREATE INDEX IX_WarehouseReceipts_RecyclerID ON [dbo].[WarehouseReceipts]([RecyclerID]);
    CREATE INDEX IX_WarehouseReceipts_WorkerID ON [dbo].[WarehouseReceipts]([WorkerID]);
    CREATE INDEX IX_WarehouseReceipts_Status ON [dbo].[WarehouseReceipts]([Status]);
    CREATE INDEX IX_WarehouseReceipts_CreatedDate ON [dbo].[WarehouseReceipts]([CreatedDate] DESC);

    PRINT 'WarehouseReceipts 表创建成功';
END
ELSE
BEGIN
    PRINT 'WarehouseReceipts 表已存在';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- ReceiptID            : 入库单唯一标识（主键，自增）
-- ReceiptNumber        : 入库单号，格式为 WR+YYYYMMDD+序号，如 WR202601040001
-- TransportOrderID     : 对应的运输单ID
-- RecyclerID           : 回收员ID（来源）
-- WorkerID             : 处理入库的基地人员ID
-- TotalWeight          : 入库总重量（公斤）
-- ItemCategories       : 物品类别JSON，例如：[{"categoryKey":"paper","categoryName":"纸类","weight":10.5}]
-- Status               : 入库单状态（已入库、已取消）
-- Notes                : 备注信息
-- CreatedDate          : 入库时间
-- CreatedBy            : 创建人（基地人员ID）
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. ReceiptNumber 格式为 WR+YYYYMMDD+4位序号，例如：WR202601040001
-- 2. 每个运输单只能创建一次入库单
-- 3. 入库成功后，对应暂存点的重量要清零
-- 4. TotalWeight 必须大于0
-- 5. ItemCategories 以JSON格式存储物品类别和重量信息
-- 6. 入库单创建后状态为"已入库"
-- ==============================================================================

-- ==============================================================================
-- 示例数据 / Sample Data
-- ==============================================================================
-- INSERT INTO [dbo].[WarehouseReceipts] 
--     (ReceiptNumber, TransportOrderID, RecyclerID, WorkerID, TotalWeight, 
--      ItemCategories, Status, CreatedBy)
-- VALUES 
--     (N'WR202601040001', 1, 1, 1, 45.50, 
--      N'[{"categoryKey":"paper","categoryName":"纸类","weight":20.5},{"categoryKey":"plastic","categoryName":"塑料","weight":15.0},{"categoryKey":"metal","categoryName":"金属","weight":10.0}]',
--      N'已入库', 1);
-- ==============================================================================
