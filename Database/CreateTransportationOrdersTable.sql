-- ==============================================================================
-- 运输单表（TransportationOrders）建表脚本
-- Transportation Orders Table Creation Script
-- 
-- 用途: 存储回收员联系运输人员的运输单信息
-- Purpose: Store transportation orders created by recyclers to contact transporters
--
-- 作者: System
-- 创建日期: 2026-01-03
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- TransportationOrders 表（运输单表）
-- 实体类: recycling.Model.TransportationOrders
-- 用途: 存储从回收员暂存点到基地的运输单信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TransportationOrders] (
        [TransportOrderID] INT PRIMARY KEY IDENTITY(1,1),    -- 运输单ID（自增主键）
        [OrderNumber] NVARCHAR(50) NOT NULL UNIQUE,          -- 运输单号（唯一，格式：TO+年月日+序号）
        [RecyclerID] INT NOT NULL,                           -- 回收员ID（外键）
        [TransporterID] INT NOT NULL,                        -- 运输人员ID（外键）
        [PickupAddress] NVARCHAR(200) NOT NULL,              -- 取货地址
        [DestinationAddress] NVARCHAR(200) NOT NULL,         -- 目的地地址（基地地址）
        [ContactPerson] NVARCHAR(50) NOT NULL,               -- 联系人（回收员姓名）
        [ContactPhone] NVARCHAR(20) NOT NULL,                -- 联系电话
        [EstimatedWeight] DECIMAL(10, 2) NOT NULL,           -- 预估总重量（kg）
        [ActualWeight] DECIMAL(10, 2) NULL,                  -- 实际重量（kg）
        [ItemCategories] NVARCHAR(MAX) NULL,                 -- 物品类别（JSON格式存储）
        [SpecialInstructions] NVARCHAR(500) NULL,            -- 特殊说明
        [Status] NVARCHAR(20) NOT NULL DEFAULT N'待接单',    -- 状态（待接单、已接单、运输中、已完成、已取消）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),  -- 创建时间
        [AcceptedDate] DATETIME2 NULL,                       -- 接单时间
        [PickupDate] DATETIME2 NULL,                         -- 取货时间
        [DeliveryDate] DATETIME2 NULL,                       -- 送达时间
        [CompletedDate] DATETIME2 NULL,                      -- 完成时间
        [CancelledDate] DATETIME2 NULL,                      -- 取消时间
        [CancelReason] NVARCHAR(200) NULL,                   -- 取消原因
        [TransporterNotes] NVARCHAR(500) NULL,               -- 运输人员备注
        [RecyclerRating] INT NULL,                           -- 回收员评分（1-5）
        [RecyclerReview] NVARCHAR(500) NULL,                 -- 回收员评价
        
        -- 约束
        CONSTRAINT FK_TransportationOrders_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT FK_TransportationOrders_Transporters FOREIGN KEY ([TransporterID]) 
            REFERENCES [dbo].[Transporters]([TransporterID]),
        CONSTRAINT CK_TransportationOrders_Status 
            CHECK ([Status] IN (N'待接单', N'已接单', N'运输中', N'已完成', N'已取消')),
        CONSTRAINT CK_TransportationOrders_RecyclerRating 
            CHECK ([RecyclerRating] IS NULL OR ([RecyclerRating] >= 1 AND [RecyclerRating] <= 5)),
        CONSTRAINT CK_TransportationOrders_EstimatedWeight 
            CHECK ([EstimatedWeight] > 0),
        CONSTRAINT CK_TransportationOrders_ActualWeight 
            CHECK ([ActualWeight] IS NULL OR [ActualWeight] > 0)
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_TransportationOrders_OrderNumber ON [dbo].[TransportationOrders]([OrderNumber]);
    CREATE INDEX IX_TransportationOrders_RecyclerID ON [dbo].[TransportationOrders]([RecyclerID]);
    CREATE INDEX IX_TransportationOrders_TransporterID ON [dbo].[TransportationOrders]([TransporterID]);
    CREATE INDEX IX_TransportationOrders_Status ON [dbo].[TransportationOrders]([Status]);
    CREATE INDEX IX_TransportationOrders_CreatedDate ON [dbo].[TransportationOrders]([CreatedDate] DESC);

    PRINT 'TransportationOrders 表创建成功';
END
ELSE
BEGIN
    PRINT 'TransportationOrders 表已存在';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- TransportOrderID      : 运输单唯一标识（主键，自增）
-- OrderNumber           : 运输单号，格式为 TO+YYYYMMDD+序号，如 TO202601030001
-- RecyclerID            : 发起运输的回收员ID
-- TransporterID         : 负责运输的运输人员ID
-- PickupAddress         : 回收员暂存点地址（取货地址）
-- DestinationAddress    : 目的地地址（通常是分拣中心或基地）
-- ContactPerson         : 联系人姓名（回收员姓名）
-- ContactPhone          : 联系电话（回收员电话）
-- EstimatedWeight       : 预估总重量（公斤）
-- ActualWeight          : 实际运输重量（公斤），由运输人员在完成后填写
-- ItemCategories        : 物品类别JSON，例如：[{"categoryName":"纸类","weight":10.5},{"categoryName":"塑料","weight":5.2}]
-- SpecialInstructions   : 特殊说明或备注信息
-- Status                : 运输单状态
-- CreatedDate           : 运输单创建时间
-- AcceptedDate          : 运输人员接单时间
-- PickupDate            : 运输人员取货时间
-- DeliveryDate          : 运输人员送达时间
-- CompletedDate         : 运输单完成时间
-- CancelledDate         : 取消时间
-- CancelReason          : 取消原因
-- TransporterNotes      : 运输人员备注信息
-- RecyclerRating        : 回收员对运输服务的评分（1-5分）
-- RecyclerReview        : 回收员对运输服务的评价文字
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. OrderNumber 格式为 TO+YYYYMMDD+4位序号，例如：TO202601030001
-- 2. 运输单状态流转：
--    待接单 -> 已接单 -> 运输中 -> 已完成
--    任何状态都可以转为 已取消（需填写取消原因）
-- 3. RecyclerID 和 TransporterID 必须存在于对应的表中
-- 4. EstimatedWeight 必须大于0
-- 5. ActualWeight 只有在完成后由运输人员填写
-- 6. ItemCategories 以JSON格式存储物品类别和重量信息
-- 7. 只有状态为"已完成"的运输单才能评分
-- ==============================================================================

-- ==============================================================================
-- 示例数据 / Sample Data
-- ==============================================================================
-- INSERT INTO [dbo].[TransportationOrders] 
--     (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress, 
--      ContactPerson, ContactPhone, EstimatedWeight, ItemCategories, Status)
-- VALUES 
--     (N'TO202601030001', 1, 1, N'深圳市罗湖区XX街道XX号', N'深圳市罗湖区分拣中心',
--      N'张回收员', N'13800138000', 45.50, 
--      N'[{"categoryName":"纸类","weight":20.5},{"categoryName":"塑料","weight":15.0},{"categoryName":"金属","weight":10.0}]',
--      N'待接单');
-- ==============================================================================
