# 仓库库存转移功能测试指南
# Warehouse Inventory Transfer Feature Test Guide

## 测试前准备 / Pre-Test Setup

### 1. 数据库准备
```sql
-- 1.1 运行迁移脚本
USE RecyclingDB;
GO

-- 执行 Database/AddInventoryTypeColumn.sql

-- 1.2 验证表结构
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Inventory' 
  AND COLUMN_NAME = 'InventoryType';

-- 预期结果: 
-- InventoryType, nvarchar(20), NO, 'StoragePoint'

-- 1.3 验证约束
SELECT 
    CONSTRAINT_NAME, 
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE CONSTRAINT_NAME = 'CK_Inventory_InventoryType';

-- 预期结果:
-- CK_Inventory_InventoryType, ([InventoryType]='Warehouse' OR [InventoryType]='StoragePoint')
```

### 2. 测试账号准备
需要以下角色的测试账号：
- **回收员账号** (Recycler)
- **运输员账号** (Transporter) 
- **基地工作人员账号** (Sorting Center Worker)
- **管理员账号** (Admin)

## 测试场景 / Test Scenarios

### 场景 1: 完整的订单到仓库流程

#### 步骤 1: 回收员完成订单
1. 登录回收员账号
2. 接单并完成一个订单
3. 确认订单包含多个类别的物品（例如：纸类20kg，塑料15kg）

**验证点**:
```sql
-- 检查Inventory表是否有记录，且InventoryType为'StoragePoint'
SELECT 
    InventoryID, 
    CategoryKey, 
    CategoryName, 
    Weight, 
    InventoryType, 
    RecyclerID,
    CreatedDate
FROM Inventory
WHERE RecyclerID = [回收员ID]
  AND InventoryType = 'StoragePoint';

-- 预期结果: 应该有2条记录（纸类和塑料），InventoryType都是'StoragePoint'
```

#### 步骤 2: 查看回收员暂存点
1. 在回收员端访问"暂存点管理"页面
2. 验证显示的库存数据

**验证点**:
- ✅ 应该显示刚才完成订单的物品
- ✅ 总重量应该是35kg (20+15)
- ✅ 类别应该显示"纸类"和"塑料"

#### 步骤 3: 创建运输单
1. 登录运输员账号
2. 创建运输单并关联到回收员的订单
3. 将运输单状态更新为"运输中"，然后"已完成"

#### 步骤 4: 创建入库单
1. 登录基地工作人员账号
2. 访问"仓库管理"页面
3. 在"待入库运输单"列表中找到刚才完成的运输单
4. 点击"创建入库单"按钮
5. 填写入库信息：
   - 确认重量
   - 确认物品类别
   - 添加备注（可选）
6. 提交创建入库单

**验证点**:
```sql
-- 1. 检查入库单是否创建成功
SELECT 
    ReceiptID,
    ReceiptNumber,
    TransportOrderID,
    RecyclerID,
    TotalWeight,
    ItemCategories,
    Status,
    CreatedDate
FROM WarehouseReceipts
WHERE RecyclerID = [回收员ID]
ORDER BY CreatedDate DESC;

-- 预期结果: 应该有一条新的入库单记录，Status='已入库'

-- 2. **关键验证**: 检查Inventory表中的InventoryType是否已更新
SELECT 
    InventoryID, 
    CategoryKey, 
    CategoryName, 
    Weight, 
    InventoryType, 
    RecyclerID,
    CreatedDate
FROM Inventory
WHERE RecyclerID = [回收员ID]
ORDER BY CreatedDate DESC;

-- 预期结果: 
-- - 之前的库存记录仍然存在（未被删除）
-- - InventoryType已从'StoragePoint'更新为'Warehouse'
-- - CreatedDate保持原值（未更新）
-- - 所有其他字段保持不变
```

#### 步骤 5: 验证回收员暂存点
1. 重新登录回收员账号
2. 访问"暂存点管理"页面

**验证点**:
- ✅ 暂存点应该为空（不显示已转移到仓库的物品）
- ✅ 汇总重量应该为0

```sql
-- 验证查询：只查StoragePoint类型的库存
SELECT * FROM Inventory 
WHERE RecyclerID = [回收员ID] 
  AND InventoryType = 'StoragePoint';

-- 预期结果: 空结果集（0行）
```

#### 步骤 6: 验证管理员仓库管理
1. 登录管理员账号
2. 访问"仓库管理"页面

**验证点**:
- ✅ 应该显示刚才入库的物品
- ✅ 汇总卡片应该显示：
  - 纸类: 20kg
  - 塑料: 15kg
- ✅ 明细表格应该显示2条记录
- ✅ 每条记录应该包含回收员信息

```sql
-- 验证查询：管理员查看的仓库库存
SELECT 
    i.CategoryKey,
    i.CategoryName,
    i.Weight,
    i.Price,
    r.Username AS RecyclerName,
    i.CreatedDate,
    i.InventoryType
FROM Inventory i
LEFT JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
WHERE i.InventoryType = 'Warehouse'
ORDER BY i.CreatedDate DESC;

-- 预期结果: 应该包含刚才转移的库存记录
```

#### 步骤 7: 验证基地工作人员仓库管理
1. 登录基地工作人员账号
2. 访问"仓库管理"页面

**验证点**:
- ✅ 应该显示与管理员相同的仓库库存
- ✅ 数据一致性：总重量、类别、价格都相同

### 场景 2: 多个回收员同时入库

#### 测试目标
验证多个回收员的库存不会混淆

#### 步骤
1. 准备3个回收员账号（Recycler A, B, C）
2. 每个回收员完成不同的订单：
   - Recycler A: 纸类30kg
   - Recycler B: 塑料25kg
   - Recycler C: 金属40kg
3. 分别为这3个回收员创建运输单和入库单

**验证点**:
```sql
-- 验证每个回收员的库存都正确转移
SELECT 
    r.Username AS RecyclerName,
    i.CategoryName,
    i.Weight,
    i.InventoryType
FROM Inventory i
JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
WHERE i.RecyclerID IN ([Recycler_A_ID], [Recycler_B_ID], [Recycler_C_ID])
ORDER BY i.RecyclerID, i.CreatedDate;

-- 预期结果:
-- Recycler A | 纸类 | 30 | Warehouse
-- Recycler B | 塑料 | 25 | Warehouse
-- Recycler C | 金属 | 40 | Warehouse

-- 验证仓库总库存
SELECT 
    CategoryName,
    SUM(Weight) AS TotalWeight
FROM Inventory
WHERE InventoryType = 'Warehouse'
GROUP BY CategoryName;

-- 预期结果:
-- 纸类: 30kg
-- 塑料: 25kg
-- 金属: 40kg
```

### 场景 3: 数据看板统计验证

#### 步骤
1. 登录管理员账号
2. 访问"数据看板"页面
3. 查看各项统计数据

**验证点**:
- ✅ "今日总重量"应该只统计今天转移到仓库的库存
- ✅ "本月总重量"应该只统计本月的仓库库存
- ✅ "历史总重量"应该只统计所有Warehouse类型的库存
- ✅ "类别分布"图表应该只显示仓库库存的分布

```sql
-- 验证数据看板统计查询

-- 1. 今日总重量
SELECT ISNULL(SUM(Weight), 0) AS TodayTotal
FROM Inventory 
WHERE InventoryType = 'Warehouse' 
  AND CreatedDate >= CAST(GETDATE() AS DATE) 
  AND CreatedDate < DATEADD(day, 1, CAST(GETDATE() AS DATE));

-- 2. 本月总重量
SELECT ISNULL(SUM(Weight), 0) AS MonthTotal
FROM Inventory 
WHERE InventoryType = 'Warehouse'
  AND CreatedDate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1) 
  AND CreatedDate < DATEADD(month, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1));

-- 3. 历史总重量
SELECT ISNULL(SUM(Weight), 0) AS AllTimeTotal
FROM Inventory 
WHERE InventoryType = 'Warehouse';

-- 4. 类别分布
SELECT 
    CategoryName, 
    SUM(Weight) AS TotalWeight
FROM Inventory
WHERE InventoryType = 'Warehouse'
GROUP BY CategoryName
ORDER BY TotalWeight DESC;
```

### 场景 4: 边界条件测试

#### 测试 4.1: 空库存情况
**步骤**:
1. 确保某个回收员的暂存点为空
2. 尝试创建入库单

**预期行为**:
- ✅ 系统应该允许创建入库单（即使暂存点为空）
- ✅ Inventory表不会有任何更新操作

#### 测试 4.2: 重复创建入库单
**步骤**:
1. 对同一个运输单尝试创建第二次入库单

**预期行为**:
- ✅ 系统应该拒绝创建
- ✅ 显示错误消息："该运输单已创建入库单"

#### 测试 4.3: 未完成的运输单
**步骤**:
1. 尝试为状态不是"已完成"的运输单创建入库单

**预期行为**:
- ✅ 系统应该拒绝创建
- ✅ 显示错误消息："只能为已完成的运输单创建入库单"

## 性能测试 / Performance Testing

### 测试 1: 大量库存查询
```sql
-- 创建测试数据（如果需要）
-- 插入10000条仓库库存记录

-- 测试查询性能
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

-- 1. 汇总查询
SELECT 
    CategoryName,
    SUM(Weight) AS TotalWeight,
    SUM(ISNULL(Price, 0)) AS TotalPrice
FROM Inventory
WHERE InventoryType = 'Warehouse'
GROUP BY CategoryName;

-- 2. 分页查询
SELECT 
    i.InventoryID,
    i.CategoryKey,
    i.CategoryName,
    i.Weight,
    i.Price,
    r.Username AS RecyclerName
FROM Inventory i
LEFT JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
WHERE i.InventoryType = 'Warehouse'
ORDER BY i.CreatedDate DESC
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;

-- 验证索引是否生效
-- 查询计划应该使用 IX_Inventory_InventoryType 索引
```

**性能基准**:
- 汇总查询: < 100ms
- 分页查询: < 50ms
- 索引扫描: 应该使用索引，而非表扫描

## 回滚测试 / Rollback Testing

### 测试回滚脚本
```sql
-- 1. 备份当前数据
SELECT * INTO Inventory_Backup FROM Inventory;

-- 2. 执行回滚
ALTER TABLE Inventory DROP CONSTRAINT CK_Inventory_InventoryType;
DROP INDEX IX_Inventory_InventoryType ON Inventory;
ALTER TABLE Inventory DROP COLUMN InventoryType;

-- 3. 验证回滚成功
SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'InventoryType';
-- 预期结果: 空结果集（列已删除）

-- 4. 恢复数据（如果需要）
DROP TABLE Inventory;
SELECT * INTO Inventory FROM Inventory_Backup;
-- 然后重新运行 AddInventoryTypeColumn.sql
```

## 常见问题排查 / Troubleshooting

### 问题 1: 暂存点仍然显示已转移的物品
**原因**: 可能的原因包括：
- 数据库迁移脚本未执行
- InventoryType字段未正确更新
- 前端缓存问题

**排查步骤**:
```sql
-- 检查是否有StoragePoint类型的库存
SELECT * FROM Inventory 
WHERE RecyclerID = [回收员ID] 
  AND InventoryType = 'StoragePoint';

-- 如果有记录，检查是否应该被转移
SELECT * FROM WarehouseReceipts 
WHERE RecyclerID = [回收员ID];
```

### 问题 2: 仓库管理看不到库存
**原因**: 可能的原因包括：
- InventoryType过滤条件错误
- 入库单创建失败
- 权限问题

**排查步骤**:
```sql
-- 检查是否有Warehouse类型的库存
SELECT * FROM Inventory WHERE InventoryType = 'Warehouse';

-- 检查入库单
SELECT * FROM WarehouseReceipts ORDER BY CreatedDate DESC;

-- 检查最近的Inventory更新
SELECT TOP 10 * FROM Inventory ORDER BY CreatedDate DESC;
```

### 问题 3: 数据统计不准确
**原因**: 可能的原因包括：
- 查询条件缺少InventoryType过滤
- 数据类型不匹配

**排查步骤**:
```sql
-- 对比不同查询的结果
-- 查询1: 所有库存
SELECT SUM(Weight) FROM Inventory;

-- 查询2: 只查仓库库存
SELECT SUM(Weight) FROM Inventory WHERE InventoryType = 'Warehouse';

-- 查询3: 只查暂存点库存
SELECT SUM(Weight) FROM Inventory WHERE InventoryType = 'StoragePoint';
```

## 测试检查清单 / Test Checklist

### 功能测试
- [ ] 回收员完成订单后，库存记录创建在Inventory表，InventoryType='StoragePoint'
- [ ] 回收员可以在暂存点管理看到自己的库存
- [ ] 创建入库单后，Inventory记录的InventoryType更新为'Warehouse'
- [ ] 创建入库单后，CreatedDate字段保持原值
- [ ] 回收员暂存点不再显示已转移的库存
- [ ] 管理员可以在仓库管理看到所有Warehouse类型的库存
- [ ] 基地工作人员可以在仓库管理看到所有Warehouse类型的库存
- [ ] 多个回收员的库存不会混淆
- [ ] 数据看板统计只统计Warehouse类型的库存

### 边界条件测试
- [ ] 空暂存点时可以创建入库单
- [ ] 不能重复创建入库单
- [ ] 只能为已完成的运输单创建入库单
- [ ] InventoryType只能是'StoragePoint'或'Warehouse'

### 性能测试
- [ ] 大量库存记录时查询性能良好
- [ ] 索引正确使用
- [ ] 分页查询响应快速

### 安全测试
- [ ] SQL注入防护（参数化查询）
- [ ] 权限检查正确
- [ ] 会话验证正确
- [ ] 防伪令牌验证正确

### 兼容性测试
- [ ] 现有数据迁移正确
- [ ] 不影响其他功能
- [ ] 回滚脚本可用

---

**文档版本**: v1.0
**最后更新**: 2026-01-08
**测试负责人**: [填写姓名]
