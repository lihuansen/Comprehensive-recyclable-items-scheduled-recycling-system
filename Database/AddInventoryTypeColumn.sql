-- ================================================================
-- Add InventoryType column to Inventory table
-- Purpose: Distinguish between storage point inventory and warehouse inventory
-- ================================================================

USE RecyclingDB;
GO

-- Check if the column already exists
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') 
    AND name = 'InventoryType'
)
BEGIN
    PRINT 'Adding InventoryType column to Inventory table...';
    
    -- Add the new column with a default value
    ALTER TABLE [dbo].[Inventory]
    ADD [InventoryType] NVARCHAR(20) NOT NULL DEFAULT N'StoragePoint';
    
    -- Add check constraint to ensure only valid values
    ALTER TABLE [dbo].[Inventory]
    ADD CONSTRAINT [CK_Inventory_InventoryType] 
        CHECK ([InventoryType] IN (N'StoragePoint', N'Warehouse'));
    
    -- Create index for better query performance
    CREATE INDEX [IX_Inventory_InventoryType] ON [dbo].[Inventory]([InventoryType]);
    
    PRINT 'InventoryType column added successfully.';
    PRINT 'Default value set to "StoragePoint" for all existing records.';
END
ELSE
BEGIN
    PRINT 'InventoryType column already exists in Inventory table.';
END
GO

-- ================================================================
-- Column Description:
-- InventoryType: Indicates the location/type of inventory
--   - 'StoragePoint': Items in recycler's temporary storage point (暂存点)
--   - 'Warehouse': Items that have been warehoused at the base (仓库)
-- ================================================================
