# 运输单品类数据结构优化说明 (Transportation Order Category Data Structure Optimization)

## 问题背景

在之前的系统设计中，运输单的品类(Category)、重量(Weight)和金额(Amount)信息作为拼接字符串存储在 `TransportationOrders.ItemCategories` 字段中，格式类似：

```
纸类: 10.5 kg; 塑料: 5.2 kg; 金属: 8.0 kg
```

这种设计存在以下问题：

1. **数据难以查询和统计**：无法直接通过SQL查询特定品类的数据
2. **数据对齐困难**：品类名称、重量、单价、金额之间的关系不清晰
3. **扩展性差**：如果需要添加新字段（如折扣、备注等），需要修改字符串格式
4. **数据完整性无法保证**：没有外键约束，容易出现数据不一致

## 解决方案

创建独立的 `TransportationOrderCategories` 表来存储品类详细信息，类似于 `AppointmentCategories` 表的设计。

### 数据库设计

#### TransportationOrderCategories 表结构

| 字段名 | 类型 | 说明 |
|--------|------|------|
| CategoryID | INT (主键) | 品类明细ID |
| TransportOrderID | INT (外键) | 运输单ID |
| CategoryKey | NVARCHAR(50) | 品类键名（如：paper, plastic, metal） |
| CategoryName | NVARCHAR(50) | 品类名称（如：纸类、塑料、金属） |
| Weight | DECIMAL(10,2) | 该品类的重量（kg） |
| PricePerKg | DECIMAL(10,2) | 该品类的单价（元/kg） |
| TotalAmount | DECIMAL(10,2) | 该品类的总金额（元） |
| CreatedDate | DATETIME2 | 创建时间 |

#### 关键特性

1. **级联删除**：删除运输单时，自动删除对应的品类明细（ON DELETE CASCADE）
2. **数据约束**：
   - Weight > 0（重量必须大于0）
   - PricePerKg >= 0（单价必须大于等于0）
   - TotalAmount >= 0（总金额必须大于等于0）
3. **索引优化**：
   - TransportOrderID 索引（用于快速查询某个运输单的所有品类）
   - CategoryKey 索引（用于按品类统计）

## 数据流程

### 1. 创建运输单时

```
回收员暂存点 → 前端构建JSON → 后端解析 → 保存到TransportationOrderCategories表
```

前端发送的数据格式：
```json
[
  {
    "categoryKey": "paper",
    "categoryName": "纸类",
    "weight": 10.5,
    "pricePerKg": 2.0,
    "totalAmount": 21.0
  },
  {
    "categoryKey": "plastic",
    "categoryName": "塑料",
    "weight": 5.2,
    "pricePerKg": 1.5,
    "totalAmount": 7.8
  }
]
```

### 2. 创建入库单时

```
运输单完成 → 读取TransportationOrderCategories → 构建入库单ItemCategories → 保存到WarehouseReceipts
```

系统会：
1. 优先从 `TransportationOrderCategories` 表读取结构化数据
2. 如果表不存在或没有数据，使用 `TransportationOrders.ItemCategories` 字段（向后兼容）
3. 将结构化数据转换为JSON格式存储到 `WarehouseReceipts.ItemCategories`

### 3. 入库到库存时

```
入库单 → 解析ItemCategories JSON → 按品类分别写入Inventory表
```

## 部署步骤

### 1. 创建数据库表

执行以下SQL脚本：

```bash
Database/CreateTransportationOrderCategoriesTable.sql
```

或手动执行：

```sql
USE RecyclingDB;
GO

CREATE TABLE [dbo].[TransportationOrderCategories] (
    [CategoryID] INT PRIMARY KEY IDENTITY(1,1),
    [TransportOrderID] INT NOT NULL,
    [CategoryKey] NVARCHAR(50) NOT NULL,
    [CategoryName] NVARCHAR(50) NOT NULL,
    [Weight] DECIMAL(10, 2) NOT NULL,
    [PricePerKg] DECIMAL(10, 2) NOT NULL,
    [TotalAmount] DECIMAL(10, 2) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_TransportationOrderCategories_TransportationOrders 
        FOREIGN KEY ([TransportOrderID]) 
        REFERENCES [dbo].[TransportationOrders]([TransportOrderID]) 
        ON DELETE CASCADE,
    
    CONSTRAINT CK_TransportationOrderCategories_Weight 
        CHECK ([Weight] > 0),
    CONSTRAINT CK_TransportationOrderCategories_PricePerKg 
        CHECK ([PricePerKg] >= 0),
    CONSTRAINT CK_TransportationOrderCategories_TotalAmount 
        CHECK ([TotalAmount] >= 0)
);

CREATE INDEX IX_TransportationOrderCategories_TransportOrderID 
    ON [dbo].[TransportationOrderCategories]([TransportOrderID]);
CREATE INDEX IX_TransportationOrderCategories_CategoryKey 
    ON [dbo].[TransportationOrderCategories]([CategoryKey]);
```

### 2. 代码部署

以下文件已更新，需要重新编译和部署：

#### 新增文件
- `recycling.Model/TransportationOrderCategories.cs`
- `recycling.DAL/TransportationOrderCategoriesDAL.cs`
- `Database/CreateTransportationOrderCategoriesTable.sql`

#### 修改文件
- `recycling.DAL/TransportationOrderDAL.cs`
- `recycling.BLL/TransportationOrderBLL.cs`
- `recycling.BLL/WarehouseReceiptBLL.cs`
- `recycling.Web.UI/Controllers/StaffController.cs`
- `recycling.Web.UI/Views/Staff/StoragePointManagement.cshtml`

### 3. 验证步骤

1. **创建运输单测试**
   - 登录回收员账号
   - 在暂存点管理页面创建运输单
   - 检查数据库中 `TransportationOrderCategories` 表是否有对应记录

2. **查看品类数据**
   ```sql
   SELECT t.OrderNumber, c.CategoryName, c.Weight, c.PricePerKg, c.TotalAmount
   FROM TransportationOrders t
   INNER JOIN TransportationOrderCategories c ON t.TransportOrderID = c.TransportOrderID
   ORDER BY t.CreatedDate DESC, c.CategoryID;
   ```

3. **创建入库单测试**
   - 运输单完成后，基地工作人员创建入库单
   - 检查入库单的 `ItemCategories` 字段是否包含正确的JSON数据

4. **库存验证**
   - 入库单确认后，检查 `Inventory` 表中的品类记录是否正确

## 向后兼容性

系统设计保持了向后兼容：

1. **TransportationOrders.ItemCategories 字段保留**：仍然存储品类信息的文本表示，用于快速显示
2. **自动降级**：如果 `TransportationOrderCategories` 表不存在，系统会使用 `ItemCategories` 字段
3. **双写策略**：创建运输单时，同时写入两处：
   - `TransportationOrders.ItemCategories`（文本格式）
   - `TransportationOrderCategories` 表（结构化数据）

## 数据迁移（可选）

如果需要将现有运输单的品类数据迁移到新表，可以编写迁移脚本。但由于：
1. 旧数据的 `ItemCategories` 字段格式不统一
2. 缺少单价信息（需要从 `RecyclableItems` 表获取当前价格）

建议：
- **新运输单**：自动使用新的结构化存储
- **旧运输单**：保持原样，不强制迁移

## 收益

1. **数据一致性**：品类、重量、价格、金额始终对齐
2. **查询性能**：可以直接按品类统计，无需解析字符串或JSON
3. **扩展性**：未来可以轻松添加新字段（如折扣、税率等）
4. **维护性**：数据库约束确保数据完整性
5. **报表能力**：可以方便地生成品类分析报表

## 示例查询

### 1. 查询某个运输单的所有品类
```sql
SELECT CategoryName, Weight, PricePerKg, TotalAmount
FROM TransportationOrderCategories
WHERE TransportOrderID = 123;
```

### 2. 统计某个时间段内各品类的总重量和总金额
```sql
SELECT 
    c.CategoryName,
    SUM(c.Weight) AS TotalWeight,
    SUM(c.TotalAmount) AS TotalAmount
FROM TransportationOrderCategories c
INNER JOIN TransportationOrders t ON c.TransportOrderID = t.TransportOrderID
WHERE t.CreatedDate BETWEEN '2026-01-01' AND '2026-01-31'
GROUP BY c.CategoryName
ORDER BY TotalWeight DESC;
```

### 3. 查询某个回收员的品类统计
```sql
SELECT 
    c.CategoryName,
    COUNT(DISTINCT c.TransportOrderID) AS OrderCount,
    SUM(c.Weight) AS TotalWeight,
    AVG(c.PricePerKg) AS AvgPrice,
    SUM(c.TotalAmount) AS TotalAmount
FROM TransportationOrderCategories c
INNER JOIN TransportationOrders t ON c.TransportOrderID = t.TransportOrderID
WHERE t.RecyclerID = 456
GROUP BY c.CategoryName;
```

## 技术说明

### 前端实现要点

1. **数据收集**：从暂存点汇总数据中提取品类详细信息
2. **JSON构建**：构建包含 categoryKey, categoryName, weight, pricePerKg, totalAmount 的数组
3. **数据传输**：通过AJAX将JSON字符串发送到后端

### 后端实现要点

1. **JSON解析**：使用 Newtonsoft.Json 解析前端发送的JSON数据
2. **数据验证**：确保重量、单价、金额等字段的有效性
3. **事务处理**：使用数据库事务确保运输单和品类明细同时创建成功
4. **错误处理**：解析失败时降级使用原有的字符串格式

## 注意事项

1. **单价获取**：前端需要从服务器获取各品类的当前单价
2. **金额计算**：TotalAmount = Weight * PricePerKg，建议在前端计算后传递
3. **数据同步**：确保 `ItemCategories` 字段和 `TransportationOrderCategories` 表的数据一致性
4. **性能考虑**：对于大量运输单，`TransportationOrderCategories` 表会增长较快，需要定期归档历史数据

## 后续优化建议

1. **历史数据归档**：定期将超过一年的品类明细数据归档到历史表
2. **缓存优化**：对于频繁查询的品类统计数据，可以考虑添加缓存
3. **报表功能**：基于新的结构化数据，开发更详细的品类分析报表
4. **数据分析**：可以分析不同时间段、不同地区的品类回收趋势
