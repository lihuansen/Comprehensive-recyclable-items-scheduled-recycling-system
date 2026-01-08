# 仓库库存转移实现 / Warehouse Inventory Transfer Implementation

> **最新更新**: 暂存点清空问题已修复。详见 [STORAGE_POINT_CLEARING_FIX.md](./STORAGE_POINT_CLEARING_FIX.md)  
> **Latest Update**: Storage point clearing issue has been fixed. See [STORAGE_POINT_CLEARING_FIX.md](./STORAGE_POINT_CLEARING_FIX.md)

## 概述 / Overview

本实现解决了入库单创建后，暂存点库存转移到仓库管理的需求。

This implementation addresses the requirement to transfer storage point inventory to warehouse management after warehouse receipt creation.

## 问题描述 / Problem Description

### 原有流程 / Original Flow
1. 回收员完成订单 → 物品进入暂存点（Inventory表）
2. 创建入库单 → 暂存点库存被**删除**
3. 管理员和基地工作人员查看仓库管理 → 从WarehouseReceipts表（JSON格式）获取数据

### 问题 / Issues
- 暂存点库存被删除后无法追踪
- 管理员和基地工作人员的仓库管理使用JSON解析，而非标准化的Inventory表结构
- 无法区分暂存点库存和仓库库存

## 解决方案 / Solution

### 1. 添加 InventoryType 字段
**文件**: `Database/AddInventoryTypeColumn.sql`

在Inventory表中添加`InventoryType`字段：
- `StoragePoint`: 暂存点库存（回收员端）
- `Warehouse`: 仓库库存（管理员端和基地工作人员端）

```sql
ALTER TABLE [dbo].[Inventory]
ADD [InventoryType] NVARCHAR(20) NOT NULL DEFAULT N'StoragePoint';

ALTER TABLE [dbo].[Inventory]
ADD CONSTRAINT [CK_Inventory_InventoryType] 
    CHECK ([InventoryType] IN (N'StoragePoint', N'Warehouse'));
```

### 2. 修改入库单创建逻辑
**文件**: `recycling.DAL/WarehouseReceiptDAL.cs`

**之前 (Before)**:
```csharp
// 删除暂存点库存
DELETE FROM Inventory WHERE RecyclerID = @RecyclerID
```

**之后 (After)**:
```csharp
// 将暂存点库存转移到仓库
UPDATE Inventory 
SET InventoryType = N'Warehouse',
    CreatedDate = @TransferDate
WHERE RecyclerID = @RecyclerID 
  AND InventoryType = N'StoragePoint'
```

### 3. 更新数据访问层方法
**文件**: `recycling.DAL/InventoryDAL.cs`

所有Inventory查询方法都添加了`inventoryType`参数：

- `GetInventoryList(recyclerId, pageIndex, pageSize, inventoryType)`
- `GetInventorySummary(recyclerId, inventoryType)`
- `GetInventoryDetailWithRecycler(pageIndex, pageSize, categoryKey, inventoryType)`

### 4. 更新业务逻辑层
**文件**: `recycling.BLL/InventoryBLL.cs`

业务逻辑层方法同步添加`inventoryType`参数，默认值：
- 查询列表：`StoragePoint`（暂存点查询用）
- 查询汇总：`Warehouse`（仓库管理用）

### 5. 更新控制器
**文件**: `recycling.Web.UI/Controllers/StaffController.cs`

#### 管理员仓库管理
```csharp
// GetInventorySummary() - 使用 inventoryType="Warehouse"
var inventoryBll = new InventoryBLL();
var summary = inventoryBll.GetInventorySummary(null, "Warehouse");

// GetInventoryDetail() - 使用 inventoryType="Warehouse"
var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey, "Warehouse");
```

#### 基地工作人员仓库管理
```csharp
// GetBaseWarehouseInventorySummary() - 使用 inventoryType="Warehouse"
var summary = inventoryBll.GetInventorySummary(null, "Warehouse");

// GetBaseWarehouseInventoryDetail() - 使用 inventoryType="Warehouse"
var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey, "Warehouse");
```

### 6. 更新数据看板统计
**文件**: `recycling.DAL/AdminDAL.cs`

所有统计查询都添加了`WHERE InventoryType = N'Warehouse'`过滤：
- 今日总重量
- 本月总重量
- 历史总重量
- 每日类别重量
- 7天趋势
- 类别分布

## 数据流程 / Data Flow

```
1. 回收员完成订单
   Recycler completes order
   ↓
2. 写入Inventory表 (InventoryType='StoragePoint')
   Write to Inventory table with StoragePoint type
   ↓
3. 创建运输单
   Create transportation order
   ↓
4. 运输单完成
   Transportation order completed
   ↓
5. 基地工作人员创建入库单
   Base worker creates warehouse receipt
   ↓
6. UPDATE Inventory SET InventoryType='Warehouse'
   Transfer inventory to warehouse
   ↓
7. 管理员/基地工作人员查看仓库管理
   Admin/Base worker views warehouse management
   ↓
8. 查询 Inventory WHERE InventoryType='Warehouse'
   Query warehouse inventory
```

## 优势 / Benefits

### 1. 数据完整性
✅ 库存记录不被删除，完整保留
✅ 可追踪每笔库存的来源（RecyclerID）
✅ 保留时间戳（CreatedDate）

### 2. 统一数据结构
✅ 管理员和基地工作人员使用相同的Inventory表结构
✅ 不再依赖JSON解析
✅ 查询性能更好

### 3. 清晰的业务逻辑
✅ 明确区分暂存点和仓库
✅ 回收员查看：`InventoryType='StoragePoint'`
✅ 管理员/基地工作人员查看：`InventoryType='Warehouse'`

### 4. 易于扩展
✅ 可以添加更多InventoryType（如"已出库"、"已销售"等）
✅ 可以添加库存流转记录
✅ 支持库存审计

## 受影响的功能 / Affected Features

### ✅ 已更新
- [x] 入库单创建 (WarehouseReceipts.Create)
- [x] 管理员仓库管理 (Admin WarehouseManagement)
- [x] 基地工作人员仓库管理 (Base Worker WarehouseManagement)
- [x] 数据看板统计 (Dashboard Statistics)
- [x] 暂存点清空 (StoragePoint Clear) - **已修复** ✅ (2026-01-08)

### 🔍 需要测试
- [ ] 完整的订单到入库流程
- [ ] 多个回收员同时入库
- [ ] 仓库库存统计准确性
- [x] 回收员暂存点不显示已转移的库存 - **已修复** ✅

## 数据库迁移 / Database Migration

### 执行步骤
1. 运行SQL脚本：`Database/AddInventoryTypeColumn.sql`
2. 脚本会：
   - 添加InventoryType列（默认值='StoragePoint'）
   - 添加检查约束
   - 创建索引
3. 现有数据会自动设置为'StoragePoint'

### 回滚方案
如需回滚到原来的实现：
```sql
-- 删除InventoryType列
ALTER TABLE Inventory DROP CONSTRAINT CK_Inventory_InventoryType;
DROP INDEX IX_Inventory_InventoryType ON Inventory;
ALTER TABLE Inventory DROP COLUMN InventoryType;
```

然后还原代码到之前的DELETE逻辑。

## 测试建议 / Testing Recommendations

### 单元测试
1. 测试InventoryType默认值为'StoragePoint'
2. 测试仓库转移UPDATE操作
3. 测试按InventoryType过滤查询

### 集成测试
1. 完整流程测试：
   - 回收员完成订单
   - 查看暂存点（应显示库存）
   - 创建入库单
   - 查看暂存点（应为空）
   - 查看仓库管理（应显示库存）
2. 多回收员测试：
   - 多个回收员各自完成订单
   - 分别创建入库单
   - 验证各自的库存正确转移

### 性能测试
1. 大量库存记录的查询性能
2. 索引是否有效
3. 统计查询的响应时间

## 安全考虑 / Security Considerations

✅ 保持现有的安全特性：
- 防伪令牌验证 (AntiForgeryToken)
- 会话验证
- 角色权限检查
- SQL参数化查询

## 相关文档 / Related Documentation

- `STORAGE_POINT_CLEARING_FIX.md` - **暂存点清空问题修复（最新）** ✅
- `WAREHOUSE_UPDATE_QUICKREF.md` - 仓库数据源更新快速参考
- `CLEAR_STORAGE_POINT_IMPLEMENTATION.md` - 暂存点清空实现
- `BASE_MANAGEMENT_IMPLEMENTATION_GUIDE.md` - 基地管理实现指南
- `Database/CreateWarehouseReceiptsTable.sql` - 入库单表创建脚本
- `Database/CreateInventoryTable.sql` - 库存表创建脚本

---

**实现日期**: 2026-01-08
**状态**: ✅ 已完成
**版本**: v1.0
