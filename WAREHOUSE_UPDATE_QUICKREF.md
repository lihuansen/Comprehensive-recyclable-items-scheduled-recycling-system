# 仓库数据来源更新 - 快速参考

## 更改内容

### 问题
管理员看到的仓库数据来自暂存点（Inventory表），这只是临时数据，不是真正的仓库库存。

### 解决方案
**将数据来源改为入库单（WarehouseReceipts表）**

## 主要修改的文件

| 文件 | 修改内容 |
|------|---------|
| `recycling.DAL/WarehouseReceiptDAL.cs` | 新增方法获取入库单汇总和明细 |
| `recycling.BLL/WarehouseReceiptBLL.cs` | 新增业务逻辑层方法 |
| `recycling.Web.UI/Controllers/StaffController.cs` | 修改控制器使用新的数据源 |
| `recycling.DAL/AdminDAL.cs` | 更新数据看板统计 |
| `recycling.Web.UI/Views/Staff/WarehouseManagement.cshtml` | 更新页面描述 |

## 功能对比

### 修改前（使用Inventory表）
- ❌ 显示暂存点数据
- ❌ 包含未运输的物品
- ❌ 运输后会清零

### 修改后（使用WarehouseReceipts表）
- ✅ 显示入库单数据
- ✅ 只显示已入库的物品
- ✅ 永久保存的记录

## 技术要点

### 1. JSON解析
```csharp
// WarehouseReceipts表的ItemCategories字段是JSON格式
[
  {
    "categoryKey": "paper",
    "categoryName": "纸类", 
    "weight": 20.5
  }
]

// 代码中解析并聚合
var categories = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
```

### 2. 价格计算
```csharp
// 从RecyclableItems表获取单价
PricePerKg = 2.0元/公斤

// 计算总价
TotalPrice = Weight × PricePerKg
           = 20.5kg × 2.0元/kg
           = 41.0元
```

### 3. 性能优化
```csharp
// 避免N+1查询 - 预加载所有价格
var categoryPrices = LoadCategoryPrices(conn);

// 而不是每个类别查询一次
foreach (var category in categories)
{
    // 使用预加载的价格字典
    price = categoryPrices[category.Key];
}
```

## 受影响的页面

### 1. 管理员仓库管理
- **路径：** `/Staff/WarehouseManagement`
- **变化：** 显示入库单数据
- **用户：** 管理员、超级管理员

### 2. 数据看板
- **路径：** `/Staff/DataDashboard`
- **变化：** 库存统计显示入库数据
- **用户：** 超级管理员

## 测试要点

### 基本功能测试
1. ✅ 登录管理员账号
2. ✅ 访问仓库管理页面
3. ✅ 验证显示入库单数据
4. ✅ 检查总重量计算正确
5. ✅ 检查总金额计算正确
6. ✅ 测试类别筛选
7. ✅ 测试分页功能

### 数据验证
```sql
-- 验证入库单数据
SELECT * FROM WarehouseReceipts WHERE Status = N'已入库'

-- 验证类别汇总
SELECT 
    JSON_VALUE(value, '$.categoryKey') AS CategoryKey,
    SUM(CAST(JSON_VALUE(value, '$.weight') AS DECIMAL)) AS TotalWeight
FROM WarehouseReceipts
CROSS APPLY OPENJSON(ItemCategories)
WHERE Status = N'已入库'
GROUP BY JSON_VALUE(value, '$.categoryKey')
```

## 安全性

✅ **CodeQL扫描通过** - 无安全漏洞

保持的安全特性：
- 防伪令牌验证
- 会话验证  
- 权限检查
- SQL参数化查询

## 回滚方案

如需回滚，修改3处代码：

1. **StaffController.cs** - GetInventorySummary()
```csharp
// 改回
var inventoryBll = new InventoryBLL();
var summary = inventoryBll.GetInventorySummary(null);
```

2. **StaffController.cs** - GetInventoryDetail()
```csharp
// 改回
var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey);
```

3. **AdminDAL.cs** - GetDashboardStatistics()
```csharp
// 改回直接查询Inventory表的SQL
```

## 常见问题

**Q: 为什么要改数据源？**
A: 暂存点只是临时存放，入库单才是真正的仓库库存。

**Q: 会影响回收员吗？**
A: 不会，回收员的暂存点管理仍然使用Inventory表。

**Q: 数据会丢失吗？**
A: 不会，两个表的数据都保留，只是管理员看的数据源改变了。

**Q: 性能会变差吗？**
A: 不会，已经优化过查询，避免了N+1问题。

**Q: 如何验证功能正常？**
A: 创建一个入库单，然后在仓库管理页面查看是否显示。

## 相关文档

详细文档请查看：
- **完整文档：** `WAREHOUSE_DATA_SOURCE_UPDATE.md`
- **数据库脚本：** `Database/CreateWarehouseReceiptsTable.sql`
- **基地管理指南：** `BASE_MANAGEMENT_IMPLEMENTATION_GUIDE.md`

---

**更新时间：** 2026-01-07  
**状态：** ✅ 已完成
