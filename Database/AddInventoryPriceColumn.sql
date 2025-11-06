-- =============================================
-- Script: Add Price column to Inventory table
-- Description: Adds a Price column to store the recycling price for inventory items
-- Author: System
-- Date: 2025-11-06
-- =============================================

USE RecyclingSystemDB;
GO

-- Check if the Price column already exists
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Inventory' 
    AND COLUMN_NAME = 'Price'
)
BEGIN
    PRINT 'Adding Price column to Inventory table...';
    
    -- Add the Price column
    ALTER TABLE Inventory
    ADD Price DECIMAL(10, 2) NULL;
    
    PRINT 'Price column added successfully to Inventory table.';
END
ELSE
BEGIN
    PRINT 'Price column already exists in Inventory table. No changes made.';
END
GO

-- Display the updated table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Inventory'
ORDER BY ORDINAL_POSITION;
GO
