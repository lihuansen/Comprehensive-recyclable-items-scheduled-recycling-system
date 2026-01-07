# 仓库管理数据来源更新文档
Warehouse Management Data Source Update Documentation

## 任务概述 | Task Overview

### 问题描述 | Problem Description
系统中管理员端的仓库管理功能原本显示的是暂存点（Inventory表）的数据，但这些数据只是临时保存的物品，并非真正入库的数据。真正的入库数据应该来自WarehouseReceipts表（入库单），只有在运输完成并正式入库后，才算是真正的仓库库存。

The admin warehouse management feature was originally showing data from the Inventory table (temporary storage point), which only contains temporarily stored items, not actually warehoused data. The real warehouse data should come from the WarehouseReceipts table (warehouse receipts), as items are only truly warehoused after transport completion and formal warehouse entry.

### 解决方案 | Solution
将管理员端仓库管理的数据来源从Inventory表改为WarehouseReceipts表，同时保持显示：
- 各类别物品的总重量
- 各类别物品的总回收金额
- 数据看板显示仓库的总重量和总回收金额

Change the admin warehouse management data source from Inventory table to WarehouseReceipts table, while maintaining display of:
- Total weight per category
- Total recycling amount per category
- Dashboard showing total warehouse weight and total recycling amount

## 技术实现 | Technical Implementation

### 1. 数据访问层 (DAL) 新增方法

#### 文件：`recycling.DAL/WarehouseReceiptDAL.cs`

**新增辅助方法：**
```csharp
private Dictionary<string, decimal> LoadCategoryPrices(SqlConnection conn)
```
- 预加载所有活动类别的价格
- 避免N+1查询问题
- 按SortOrder排序，确保一致性

**新增方法1：GetWarehouseSummary()**
```csharp
public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetWarehouseSummary()
```
- 从WarehouseReceipts表读取所有已入库的记录
- 解析ItemCategories JSON字段
- 按类别聚合重量
- 从RecyclableItems表获取价格并计算总金额
- 返回类别汇总数据

**数据流程：**
1. 查询所有状态为"已入库"的WarehouseReceipts记录
2. 解析每条记录的ItemCategories JSON字段
3. 按categoryKey聚合重量
4. 预加载所有类别价格（一次查询）
5. 计算每个类别的总价 = 总重量 × 单价
6. 返回汇总结果

**新增方法2：GetWarehouseDetailWithRecycler()**
```csharp
public PagedResult<InventoryDetailViewModel> GetWarehouseDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null)
```
- 从WarehouseReceipts表读取入库单明细
- 关联Recyclers表获取回收员信息
- 关联TransportationOrders表获取运输单信息
- 支持按类别过滤
- 支持分页查询
- 计算每条记录的价格

**数据流程：**
1. 查询所有已入库的WarehouseReceipts及其关联信息
2. 解析每条记录的ItemCategories JSON
3. 展开为明细记录
4. 应用类别过滤（如果指定）
5. 预加载所有类别价格（一次查询）
6. 计算每条明细的价格
7. 应用分页并返回结果

### 2. 业务逻辑层 (BLL) 新增方法

#### 文件：`recycling.BLL/WarehouseReceiptBLL.cs`

**新增方法：**
```csharp
public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetWarehouseSummary()
public PagedResult<InventoryDetailViewModel> GetWarehouseDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null)
```
- 添加参数验证
- 调用DAL层对应方法
- 处理异常

### 3. 控制器 (Controller) 方法修改

#### 文件：`recycling.Web.UI/Controllers/StaffController.cs`

**修改方法1：GetInventorySummary()**
```csharp
// 修改前
var inventoryBll = new InventoryBLL();
var summary = inventoryBll.GetInventorySummary(null);

// 修改后
var summary = _warehouseReceiptBLL.GetWarehouseSummary();
```
- 从使用InventoryBLL改为使用WarehouseReceiptBLL
- 数据来源从Inventory表改为WarehouseReceipts表
- 返回格式保持不变，前端无需修改

**修改方法2：GetInventoryDetail()**
```csharp
// 修改前
var inventoryBll = new InventoryBLL();
var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey);

// 修改后
var result = _warehouseReceiptBLL.GetWarehouseDetailWithRecycler(page, pageSize, categoryKey);
```
- 从使用InventoryBLL改为使用WarehouseReceiptBLL
- 数据来源从Inventory表改为WarehouseReceipts表
- 返回格式保持不变，前端无需修改

### 4. 数据看板更新

#### 文件：`recycling.DAL/AdminDAL.cs`

**修改GetDashboardStatistics()方法：**
```csharp
// 修改前
cmd = new SqlCommand(@"
    SELECT CategoryKey, CategoryName, ISNULL(SUM(Weight), 0) AS TotalWeight
    FROM Inventory
    GROUP BY CategoryKey, CategoryName
    ORDER BY TotalWeight DESC", conn);

// 修改后
var warehouseReceiptDAL = new WarehouseReceiptDAL();
var warehouseSummary = warehouseReceiptDAL.GetWarehouseSummary();

var inventoryStats = new List<Dictionary<string, object>>();
foreach (var item in warehouseSummary)
{
    inventoryStats.Add(new Dictionary<string, object>
    {
        ["CategoryKey"] = item.CategoryKey,
        ["CategoryName"] = item.CategoryName,
        ["TotalWeight"] = item.TotalWeight
    });
}
```
- 从直接查询Inventory表改为调用WarehouseReceiptDAL
- 库存统计显示入库后的真实数据

### 5. 视图更新

#### 文件：`recycling.Web.UI/Views/Staff/WarehouseManagement.cshtml`

**更新页面描述：**
```html
<!-- 修改前 -->
<p>查看库存统计与明细，了解每一单的回收员负责情况</p>

<!-- 修改后 -->
<p>查看仓库入库统计与明细，了解每一单的入库情况和回收员负责情况</p>
```
- 更新描述文字以反映新的数据来源

## 数据结构说明 | Data Structure

### ItemCategories JSON格式
WarehouseReceipts表的ItemCategories字段存储JSON格式的类别信息：

```json
[
  {
    "categoryKey": "paper",
    "categoryName": "纸类",
    "weight": 20.5
  },
  {
    "categoryKey": "plastic",
    "categoryName": "塑料",
    "weight": 15.0
  },
  {
    "categoryKey": "metal",
    "categoryName": "金属",
    "weight": 10.0
  }
]
```

### 价格计算
价格从RecyclableItems表获取：
- Category字段匹配categoryKey
- 使用PricePerKg（每公斤价格）
- 计算公式：总价 = 重量 × 单价

## 性能优化 | Performance Optimization

### 避免N+1查询
**问题：** 原始实现对每个类别都执行一次价格查询

**解决方案：**
1. 添加LoadCategoryPrices()辅助方法
2. 一次查询加载所有类别价格到Dictionary
3. 使用内存查找代替数据库查询

**效果：**
- 查询次数从 N+1 减少到 2（N为类别数量）
- 显著提升性能，特别是类别较多时

### 代码复用
**问题：** 价格加载逻辑在两个方法中重复

**解决方案：**
1. 提取LoadCategoryPrices()为私有辅助方法
2. 两个方法共用同一实现
3. 确保逻辑一致性

## 影响范围 | Impact Scope

### 受影响的功能
1. **管理员仓库管理页面** (`/Staff/WarehouseManagement`)
   - 显示入库后的数据而非暂存点数据
   - 类别汇总显示入库物品的重量和金额
   - 明细列表显示入库单号和入库时间

2. **超级管理员数据看板** (`/Staff/DataDashboard`)
   - 库存统计部分显示入库后的数据
   - 反映真实的仓库库存情况

### 不受影响的功能
1. **回收员暂存点管理** (`/Staff/StoragePointManagement`)
   - 继续使用Inventory表
   - 显示回收员自己的暂存点数据
   - 不受此次更改影响

2. **其他管理功能**
   - 订单管理
   - 用户管理
   - 运输单管理
   - 均不受影响

## 数据对比 | Data Comparison

### Inventory表（暂存点）
- 订单完成后写入
- 回收员暂存点的临时数据
- 运输前的物品
- 运输到基地后会清零

### WarehouseReceipts表（入库单）
- 运输完成入库后写入
- 基地仓库的正式数据
- 已到达基地的物品
- 永久保存的入库记录

## 测试建议 | Testing Recommendations

### 功能测试
1. **仓库管理页面**
   - 登录管理员账号
   - 访问"仓库管理"页面
   - 验证显示的是入库单数据
   - 检查总重量和总金额显示
   - 测试类别筛选功能
   - 测试分页功能

2. **数据看板**
   - 登录超级管理员账号
   - 访问"数据看板"
   - 查看库存统计表格
   - 验证数据与入库单一致

### 数据验证
```sql
-- 查询入库单汇总（验证数据正确性）
SELECT 
    CategoryKey = JSON_VALUE(value, '$.categoryKey'),
    CategoryName = JSON_VALUE(value, '$.categoryName'),
    TotalWeight = SUM(CAST(JSON_VALUE(value, '$.weight') AS DECIMAL(10,2)))
FROM WarehouseReceipts
CROSS APPLY OPENJSON(ItemCategories)
WHERE Status = N'已入库'
GROUP BY JSON_VALUE(value, '$.categoryKey'), JSON_VALUE(value, '$.categoryName')
ORDER BY TotalWeight DESC
```

### 性能测试
- 测试大量入库单时的查询速度
- 验证LoadCategoryPrices只执行一次
- 检查分页功能的响应时间

## 回滚方案 | Rollback Plan

如需回滚到原来的数据源：

1. 恢复StaffController.cs的两个方法：
```csharp
var inventoryBll = new InventoryBLL();
var summary = inventoryBll.GetInventorySummary(null);
var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey);
```

2. 恢复AdminDAL.cs的查询：
```csharp
cmd = new SqlCommand(@"
    SELECT CategoryKey, CategoryName, ISNULL(SUM(Weight), 0) AS TotalWeight
    FROM Inventory
    GROUP BY CategoryKey, CategoryName
    ORDER BY TotalWeight DESC", conn);
```

3. 恢复页面描述文字

## 安全性 | Security

### 安全扫描结果
✅ CodeQL扫描通过，无安全漏洞

### 安全特性保持
- ✅ 防伪令牌验证
- ✅ 会话验证
- ✅ 权限检查
- ✅ SQL参数化查询
- ✅ 异常处理

## 未来改进建议 | Future Improvements

### 1. 强类型JSON反序列化
当前使用Dictionary<string, object>，建议创建专用类：
```csharp
public class ItemCategoryJson
{
    public string categoryKey { get; set; }
    public string categoryName { get; set; }
    public decimal weight { get; set; }
}
```

### 2. 数据库级分页
当前在内存中分页，大数据量时可能影响性能。建议：
- 在SQL层面实现分页
- 只加载当前页需要的数据

### 3. 缓存优化
对于类别价格等不常变化的数据，可以考虑：
- 添加内存缓存
- 减少数据库查询

## 相关文档 | Related Documentation

- [入库单表结构](./Database/CreateWarehouseReceiptsTable.sql)
- [基地管理实现指南](./BASE_MANAGEMENT_IMPLEMENTATION_GUIDE.md)
- [仓库数据直接显示修复](./WAREHOUSE_DIRECT_DISPLAY_FIX_CN.md)
- [数据库架构](./DATABASE_SCHEMA.md)

## 联系信息 | Contact

如有问题或建议，请通过以下方式联系：
- GitHub Issues
- Pull Request评论

---

**最后更新：** 2026-01-07
**版本：** 1.0.0
