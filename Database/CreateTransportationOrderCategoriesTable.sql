-- ==============================================================================
-- 运输单品类明细表（TransportationOrderCategories）建表脚本
-- Transportation Order Categories Detail Table Creation Script
-- 
-- 用途: 存储运输单的品类详细信息，确保品类、重量、价格、金额对齐
-- Purpose: Store detailed category information for transportation orders, 
--          ensuring proper alignment of categories, weights, prices, and amounts
--
-- 作者: System
-- 创建日期: 2026-01-16
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- TransportationOrderCategories 表（运输单品类明细表）
-- 实体类: recycling.Model.TransportationOrderCategories
-- 用途: 存储每个运输单的品类详细信息，替代TransportationOrders表中的ItemCategories字段
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrderCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TransportationOrderCategories] (
        [CategoryID] INT PRIMARY KEY IDENTITY(1,1),          -- 品类明细ID（自增主键）
        [TransportOrderID] INT NOT NULL,                     -- 运输单ID（外键）
        [CategoryKey] NVARCHAR(50) NOT NULL,                 -- 品类键名（如：paper, plastic, metal）
        [CategoryName] NVARCHAR(50) NOT NULL,                -- 品类名称（如：纸类、塑料、金属）
        [Weight] DECIMAL(10, 2) NOT NULL,                    -- 该品类的重量（kg）
        [PricePerKg] DECIMAL(10, 2) NOT NULL,                -- 该品类的单价（元/kg）
        [TotalAmount] DECIMAL(10, 2) NOT NULL,               -- 该品类的总金额（元）= Weight * PricePerKg
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),  -- 创建时间
        
        -- 外键约束
        CONSTRAINT FK_TransportationOrderCategories_TransportationOrders 
            FOREIGN KEY ([TransportOrderID]) 
            REFERENCES [dbo].[TransportationOrders]([TransportOrderID]) 
            ON DELETE CASCADE,
        
        -- 检查约束
        CONSTRAINT CK_TransportationOrderCategories_Weight 
            CHECK ([Weight] > 0),
        CONSTRAINT CK_TransportationOrderCategories_PricePerKg 
            CHECK ([PricePerKg] >= 0),
        CONSTRAINT CK_TransportationOrderCategories_TotalAmount 
            CHECK ([TotalAmount] >= 0)
    );

    -- 创建索引
    CREATE INDEX IX_TransportationOrderCategories_TransportOrderID 
        ON [dbo].[TransportationOrderCategories]([TransportOrderID]);
    CREATE INDEX IX_TransportationOrderCategories_CategoryKey 
        ON [dbo].[TransportationOrderCategories]([CategoryKey]);

    PRINT 'TransportationOrderCategories 表创建成功';
END
ELSE
BEGIN
    PRINT 'TransportationOrderCategories 表已存在';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- CategoryID           : 品类明细唯一标识（主键，自增）
-- TransportOrderID     : 所属运输单ID
-- CategoryKey          : 品类键名，用于系统内部标识（如：paper, plastic, metal）
-- CategoryName         : 品类显示名称（如：纸类、塑料、金属）
-- Weight               : 该品类的重量（公斤）
-- PricePerKg           : 该品类的单价（元/公斤）
-- TotalAmount          : 该品类的总金额（元）= Weight * PricePerKg
-- CreatedDate          : 创建时间
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. 每个运输单可以有多个品类明细
-- 2. Weight 必须大于0
-- 3. PricePerKg 必须大于等于0（某些品类可能免费回收）
-- 4. TotalAmount 应等于 Weight * PricePerKg，由应用层计算并存储
-- 5. 当删除运输单时，对应的品类明细会自动级联删除（ON DELETE CASCADE）
-- 6. CategoryKey 和 CategoryName 应保持一致性，从 RecyclableItems 表获取
-- ==============================================================================

-- ==============================================================================
-- 示例数据 / Sample Data
-- ==============================================================================
-- 假设运输单ID为1，包含三个品类：
-- INSERT INTO [dbo].[TransportationOrderCategories] 
--     (TransportOrderID, CategoryKey, CategoryName, Weight, PricePerKg, TotalAmount)
-- VALUES 
--     (1, 'paper', N'纸类', 20.50, 2.00, 41.00),
--     (1, 'plastic', N'塑料', 15.00, 1.50, 22.50),
--     (1, 'metal', N'金属', 10.00, 3.00, 30.00);
-- ==============================================================================

-- ==============================================================================
-- 数据迁移说明 / Data Migration Notes
-- ==============================================================================
-- 如果需要从现有的 TransportationOrders.ItemCategories 字段迁移数据：
-- 1. ItemCategories 字段当前存储格式：JSON字符串，如：
--    [{"categoryKey":"paper","categoryName":"纸类","weight":10.5}]
-- 2. 需要解析 JSON 并插入到此表
-- 3. 迁移脚本需要处理价格信息（从 RecyclableItems 表获取）
-- 4. 迁移后，ItemCategories 字段可以保留作为备份或删除
-- ==============================================================================
