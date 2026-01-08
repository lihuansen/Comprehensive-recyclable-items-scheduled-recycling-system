# Warehouse Inventory System Redesign

## Overview
This document describes the redesign of the warehouse inventory management system to properly handle the lifecycle of goods from storage point → transportation → base warehouse.

## Problem Statement
The original system had issues with inventory state tracking:
1. When goods were in transit ("运输中"), they were still visible in the storage point
2. When warehouse receipts were created, the Appointment status was incorrectly changed to "已入库"
3. The inventory flow didn't properly reflect the physical movement of goods

## Solution

### Inventory State Machine
We introduced a new "InTransit" state to properly track inventory lifecycle:

```
StoragePoint → InTransit → Warehouse
```

### State Transitions

1. **Order Completion** → `InventoryType = "StoragePoint"`
   - When a recycler completes an order, inventory items are created with `InventoryType = "StoragePoint"`
   - These items are visible in the storage point view

2. **Transport Starts** → `InventoryType = "InTransit"`
   - When `TransportationOrders.Status` changes from "已接单" to "运输中"
   - All inventory items for that recycler are moved from "StoragePoint" to "InTransit"
   - Items are NOT visible in either storage point or warehouse

3. **Transport Completes** → Status remains "InTransit"
   - When `TransportationOrders.Status` changes to "已完成"
   - Inventory items remain in "InTransit" state
   - Items are still NOT visible in either storage point or warehouse

4. **Warehouse Receipt Created** → `InventoryType = "Warehouse"`
   - When a `WarehouseReceipts` record is created with `Status = "已入库"`
   - All inventory items for that recycler are moved from "InTransit" to "Warehouse"
   - Items are now visible in the warehouse view

### Key Changes

#### 1. TransportationOrderDAL.StartTransportation
**File:** `recycling.DAL/TransportationOrderDAL.cs`

Added transaction-based logic to:
- Update transport order status to "运输中"
- Move all inventory items from "StoragePoint" to "InTransit" for the recycler

```csharp
// Move inventory from StoragePoint to InTransit
UPDATE Inventory 
SET InventoryType = N'InTransit'
WHERE RecyclerID = @RecyclerID 
  AND InventoryType = N'StoragePoint'
```

#### 2. WarehouseReceiptDAL.CreateWarehouseReceipt
**File:** `recycling.DAL/WarehouseReceiptDAL.cs`

Changes:
- **Removed** incorrect Appointment status update (no longer changes from "已完成" to "已入库")
- **Changed** inventory transfer from "StoragePoint" → "Warehouse" to "InTransit" → "Warehouse"
- **Only** updates `WarehouseReceipts.Status` to "已入库"

```csharp
// Move inventory from InTransit to Warehouse
UPDATE Inventory 
SET InventoryType = N'Warehouse'
WHERE RecyclerID = @RecyclerID 
  AND InventoryType = N'InTransit'
```

#### 3. StoragePointDAL.GetStoragePointSummary & GetStoragePointDetail
**File:** `recycling.DAL/StoragePointDAL.cs`

Changes:
- **Changed** from querying `Appointments` table to querying `Inventory` table
- **Only** shows items where `InventoryType = 'StoragePoint'`
- This ensures items in transit or warehouse are not visible in storage point

```csharp
// Query from Inventory table
SELECT CategoryKey, CategoryName, SUM(Weight), SUM(Price)
FROM Inventory
WHERE RecyclerID = @RecyclerID 
  AND InventoryType = N'StoragePoint'
GROUP BY CategoryKey, CategoryName
```

## Data Model

### Inventory Table
| Field | Type | Description |
|-------|------|-------------|
| InventoryID | int | Primary key |
| OrderID | int | Reference to Appointment |
| RecyclerID | int | Reference to Recycler |
| CategoryKey | string(50) | Category identifier |
| CategoryName | string(50) | Category display name |
| Weight | decimal | Weight in kg |
| Price | decimal? | Calculated price |
| CreatedDate | datetime2 | Original creation timestamp |
| **InventoryType** | **string(20)** | **"StoragePoint", "InTransit", or "Warehouse"** |

### Status Values

#### Appointments.Status
- "已完成" - Order completed (remains unchanged throughout the process)
- **Note:** No longer changes to "已入库"

#### TransportationOrders.Status
- "待接单" - Awaiting acceptance
- "已接单" - Accepted
- "运输中" - In transit
- "已完成" - Transport completed
- "已取消" - Cancelled

#### WarehouseReceipts.Status
- "已入库" - Warehoused

#### Inventory.InventoryType
- "StoragePoint" - Items at storage point
- "InTransit" - Items being transported (not visible)
- "Warehouse" - Items at warehouse

## Benefits

1. **Accurate State Tracking**: Inventory state now accurately reflects physical location
2. **No Double Counting**: Items cannot appear in both storage point and warehouse
3. **Proper Visibility**: During transport, items are not visible in either location
4. **Preserves History**: Appointment status remains "已完成" for historical tracking
5. **Correct Status Updates**: Only warehouse receipt status is "已入库", not appointment status

## Migration Notes

For existing data in production:
1. Identify any inventory items that should be "InTransit" (associated with transport orders in "运输中" status)
2. Update their `InventoryType` from "StoragePoint" to "InTransit"
3. No changes needed to Appointment records

## Testing Checklist

- [ ] Complete an order → Verify inventory appears in storage point
- [ ] Start transportation → Verify inventory disappears from storage point
- [ ] Check during transport → Verify inventory not visible in warehouse
- [ ] Complete transport → Verify inventory still not visible
- [ ] Create warehouse receipt → Verify inventory appears in warehouse
- [ ] Verify Appointment.Status remains "已完成" throughout process
- [ ] Verify WarehouseReceipts.Status is "已入库"
