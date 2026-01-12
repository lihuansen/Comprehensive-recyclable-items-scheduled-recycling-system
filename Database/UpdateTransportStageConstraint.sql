-- ==============================================================================
-- Update TransportStage Constraint to Accept User-Friendly Terminology
-- 更新运输阶段约束以接受用户友好的术语
-- 
-- Purpose: Update the constraint to accept "装货完成" (loading completed) 
--          as a synonym for "装货完毕"
-- 用途: 更新约束以接受"装货完成"作为"装货完毕"的同义词
--
-- Date: 2026-01-12
-- ==============================================================================

USE RecyclingDB;
GO

-- Drop existing constraint if it exists
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_TransportationOrders_TransportStage')
BEGIN
    ALTER TABLE [dbo].[TransportationOrders]
    DROP CONSTRAINT CK_TransportationOrders_TransportStage;
    
    PRINT 'Dropped existing TransportStage constraint';
END
GO

-- Add updated constraint with "装货完成" as an allowed value
ALTER TABLE [dbo].[TransportationOrders]
ADD CONSTRAINT CK_TransportationOrders_TransportStage 
    CHECK ([TransportStage] IS NULL OR [TransportStage] IN (
        N'确认取货地点',   -- Confirm pickup location
        N'到达取货地点',   -- Arrive at pickup location
        N'装货完毕',       -- Loading completed (formal)
        N'装货完成',       -- Loading completed (casual, user-friendly)
        N'确认送货地点',   -- Confirm delivery location
        N'到达送货地点'    -- Arrive at delivery location
    ));

PRINT 'Updated TransportStage constraint to accept user-friendly terminology';
GO

-- ==============================================================================
-- Notes / 说明
-- ==============================================================================
-- Both "装货完毕" and "装货完成" are now accepted as they are synonyms
-- meaning "loading completed". The application will use "装货完成" going forward
-- as it is more user-friendly, but "装货完毕" is kept for backward compatibility
-- with existing data.
--
-- "装货完毕"和"装货完成"现在都被接受，因为它们是同义词，表示"装载完成"。
-- 应用程序将使用"装货完成"，因为它更用户友好，但保留"装货完毕"以向后兼容现有数据。
-- ==============================================================================
