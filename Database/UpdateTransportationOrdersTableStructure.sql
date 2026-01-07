-- ==============================================================================
-- 更新运输单表结构
-- Update TransportationOrders Table Structure
-- 
-- 用途: 添加基地联系人信息和物品总金额字段，修改联系人字段为可选
-- Purpose: Add base contact information and item total value fields, make contact fields optional
--
-- 作者: System
-- 创建日期: 2026-01-07
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- 步骤 1: 修改现有字段，将 ContactPerson 和 ContactPhone 改为可选
-- Step 1: Modify existing fields to make ContactPerson and ContactPhone optional
-- ==============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    -- 修改 ContactPerson 为可选字段
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ContactPerson')
    BEGIN
        ALTER TABLE [dbo].[TransportationOrders]
        ALTER COLUMN [ContactPerson] NVARCHAR(50) NULL;
        
        PRINT 'ContactPerson 字段已修改为可选';
    END

    -- 修改 ContactPhone 为可选字段
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ContactPhone')
    BEGIN
        ALTER TABLE [dbo].[TransportationOrders]
        ALTER COLUMN [ContactPhone] NVARCHAR(20) NULL;
        
        PRINT 'ContactPhone 字段已修改为可选';
    END
END
GO

-- ==============================================================================
-- 步骤 2: 添加新字段 - 基地联系人姓名
-- Step 2: Add new field - Base Contact Person Name
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPerson')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [BaseContactPerson] NVARCHAR(50) NULL;
    
    PRINT 'BaseContactPerson 字段添加成功';
END
ELSE
BEGIN
    PRINT 'BaseContactPerson 字段已存在';
END
GO

-- ==============================================================================
-- 步骤 3: 添加新字段 - 基地联系人电话
-- Step 3: Add new field - Base Contact Phone
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'BaseContactPhone')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [BaseContactPhone] NVARCHAR(20) NULL;
    
    PRINT 'BaseContactPhone 字段添加成功';
END
ELSE
BEGIN
    PRINT 'BaseContactPhone 字段已存在';
END
GO

-- ==============================================================================
-- 步骤 4: 添加新字段 - 物品总金额
-- Step 4: Add new field - Item Total Value
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND name = 'ItemTotalValue')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    ADD [ItemTotalValue] DECIMAL(10, 2) NOT NULL DEFAULT 0;
    
    PRINT 'ItemTotalValue 字段添加成功';
END
ELSE
BEGIN
    PRINT 'ItemTotalValue 字段已存在';
END
GO

-- ==============================================================================
-- 验证修改结果
-- Verify modifications
-- ==============================================================================
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TransportationOrders'
    AND COLUMN_NAME IN ('ContactPerson', 'ContactPhone', 'BaseContactPerson', 'BaseContactPhone', 'ItemTotalValue')
ORDER BY ORDINAL_POSITION;
GO

PRINT '==============================================================================';
PRINT '运输单表结构更新完成';
PRINT 'Transportation Orders table structure update completed';
PRINT '==============================================================================';
PRINT '';
PRINT '新增字段说明 / New Fields Description:';
PRINT '1. ContactPerson - 修改为可选字段（回收员联系人）';
PRINT '2. ContactPhone - 修改为可选字段（回收员联系电话）';
PRINT '3. BaseContactPerson - 基地人员联系人姓名（可编辑）';
PRINT '4. BaseContactPhone - 基地人员联系电话（可编辑）';
PRINT '5. ItemTotalValue - 物品总金额（自动计算）';
PRINT '==============================================================================';
