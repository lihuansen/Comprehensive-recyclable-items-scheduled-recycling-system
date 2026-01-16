# 运输单品类对齐功能 - 快速部署和测试指南

## 快速部署步骤

### 1. 创建数据库表（必须）

在SQL Server Management Studio中连接到 `RecyclingDB` 数据库，执行以下脚本：

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

PRINT '表创建成功！';
GO
```

或直接执行脚本文件：
```bash
Database/CreateTransportationOrderCategoriesTable.sql
```

### 2. 编译和部署代码

1. 在Visual Studio中打开解决方案
2. 清理解决方案（Clean Solution）
3. 重新生成解决方案（Rebuild Solution）
4. 发布到IIS或运行调试

### 3. 验证部署

检查数据库表是否创建成功：
```sql
-- 检查表是否存在
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'TransportationOrderCategories';
-- 结果应该是 1

-- 查看表结构
EXEC sp_help 'TransportationOrderCategories';
```

## 快速测试步骤

### 测试1：创建运输单

1. **登录回收员账号**
   - 打开浏览器，访问系统
   - 使用回收员账号登录

2. **准备测试数据**
   - 确保回收员账号有暂存点库存
   - 如果没有，先完成一个预约订单

3. **创建运输单**
   - 进入"暂存点管理"页面
   - 点击"联系运输"按钮
   - 选择一个运输人员
   - 填写基地联系人信息
   - 提交创建运输单

4. **验证数据库**
   ```sql
   -- 查看最新的运输单
   SELECT TOP 1 * FROM TransportationOrders 
   ORDER BY CreatedDate DESC;
   
   -- 查看对应的品类明细（使用上面查询到的TransportOrderID）
   SELECT * FROM TransportationOrderCategories 
   WHERE TransportOrderID = <运输单ID>
   ORDER BY CategoryID;
   ```

5. **预期结果**
   - `TransportationOrders` 表有新记录
   - `TransportationOrderCategories` 表有对应的品类明细记录
   - 品类明细包含：CategoryKey, CategoryName, Weight, PricePerKg, TotalAmount
   - TotalAmount = Weight * PricePerKg

### 测试2：查看运输单品类数据

```sql
-- 查看运输单及其品类明细
SELECT 
    t.OrderNumber,
    t.RecyclerID,
    t.EstimatedWeight,
    t.ItemTotalValue,
    c.CategoryName,
    c.Weight,
    c.PricePerKg,
    c.TotalAmount
FROM TransportationOrders t
LEFT JOIN TransportationOrderCategories c ON t.TransportOrderID = c.TransportOrderID
WHERE t.OrderNumber = '<运输单号>'
ORDER BY c.CategoryID;
```

### 测试3：完成运输并创建入库单

1. **运输人员接单并完成运输**
   - 登录运输人员账号
   - 接单 → 确认收货地点 → 到达收货地点 → 装货完毕 → 确认送货地点 → 到达送货地点 → 完成运输

2. **基地工作人员创建入库单**
   - 登录基地工作人员账号
   - 进入"基地仓库管理"页面
   - 找到完成的运输单
   - 创建入库单

3. **验证入库单数据**
   ```sql
   -- 查看入库单
   SELECT TOP 1 * FROM WarehouseReceipts 
   ORDER BY CreatedDate DESC;
   
   -- 查看入库单的ItemCategories字段（应该是JSON格式）
   SELECT ItemCategories FROM WarehouseReceipts 
   WHERE ReceiptID = <入库单ID>;
   ```

4. **预期结果**
   - `ItemCategories` 字段包含JSON格式的品类数据
   - JSON格式类似：
     ```json
     [
       {
         "categoryKey": "paper",
         "categoryName": "纸类",
         "weight": 10.5,
         "pricePerKg": 2.0,
         "totalAmount": 21.0
       }
     ]
     ```

### 测试4：确认入库并查看库存

1. **确认入库**
   - 基地工作人员点击"确认入库"按钮

2. **验证库存数据**
   ```sql
   -- 查看最新的库存记录
   SELECT * FROM Inventory 
   WHERE InventoryType = 'BaseWarehouse'
   ORDER BY UpdatedDate DESC;
   ```

3. **预期结果**
   - `Inventory` 表中有对应的品类记录
   - 每个品类单独一条记录

## 常见问题排查

### 问题1：TransportationOrderCategories 表没有数据

**可能原因**：
- 表不存在（系统会降级使用旧格式）
- 前端没有发送JSON格式的品类数据

**排查步骤**：
1. 检查表是否存在
   ```sql
   SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'TransportationOrderCategories';
   ```

2. 查看应用日志（Debug输出）
   - 搜索 "成功解析"、"保存了"、"品类明细" 等关键词
   - 查看是否有错误消息

3. 检查浏览器开发者工具
   - Network标签，找到CreateTransportationOrder请求
   - 查看Form Data中itemCategories的值
   - 应该是JSON数组格式，而不是简单字符串

### 问题2：ItemCategories 格式不正确

**可能原因**：
- 前端数据构建有误
- JSON解析失败

**排查步骤**：
1. 查看浏览器控制台是否有JavaScript错误
2. 在浏览器开发者工具中设置断点，查看 `storagePointData.itemCategoriesJson` 的值
3. 验证JSON格式是否正确

### 问题3：入库单的ItemCategories字段为旧格式

**可能原因**：
- TransportationOrderCategories 表不存在
- 运输单是在新代码部署前创建的

**解决方法**：
- 如果是测试，删除旧运输单，重新创建
- 如果是生产环境，旧运输单保持旧格式，新运输单会使用新格式

## 性能验证

### 批量插入性能测试

创建一个包含多个品类的运输单，验证批量插入的性能：

```sql
-- 查看品类明细插入的执行时间
SET STATISTICS TIME ON;

-- 执行查询
SELECT * FROM TransportationOrderCategories 
WHERE TransportOrderID = <运输单ID>;

SET STATISTICS TIME OFF;
```

### 查询性能测试

```sql
-- 测试按品类统计的查询性能
SET STATISTICS TIME ON;

SELECT 
    c.CategoryName,
    COUNT(DISTINCT c.TransportOrderID) AS OrderCount,
    SUM(c.Weight) AS TotalWeight,
    AVG(c.PricePerKg) AS AvgPrice,
    SUM(c.TotalAmount) AS TotalAmount
FROM TransportationOrderCategories c
INNER JOIN TransportationOrders t ON c.TransportOrderID = t.TransportOrderID
WHERE t.CreatedDate >= DATEADD(month, -1, GETDATE())
GROUP BY c.CategoryName
ORDER BY TotalWeight DESC;

SET STATISTICS TIME OFF;
```

## 数据对齐验证

验证品类、重量、金额是否正确对齐：

```sql
-- 验证数据对齐
SELECT 
    t.OrderNumber,
    c.CategoryName,
    c.Weight,
    c.PricePerKg,
    c.TotalAmount,
    -- 计算验证
    c.Weight * c.PricePerKg AS CalculatedAmount,
    -- 差异（应该接近0）
    ABS(c.TotalAmount - (c.Weight * c.PricePerKg)) AS Difference
FROM TransportationOrders t
INNER JOIN TransportationOrderCategories c ON t.TransportOrderID = c.TransportOrderID
WHERE t.CreatedDate >= DATEADD(day, -7, GETDATE())
ORDER BY t.CreatedDate DESC, c.CategoryID;

-- 检查是否有对齐错误（差异大于0.01的记录）
SELECT COUNT(*) AS ErrorCount
FROM TransportationOrderCategories
WHERE ABS(TotalAmount - (Weight * PricePerKg)) > 0.01;
-- 结果应该是 0
```

## 回滚步骤（如果需要）

如果部署后发现问题需要回滚：

### 1. 停止应用
停止IIS应用池或Web服务

### 2. 恢复旧代码
使用Git恢复到上一个提交

### 3. 保留数据库表（推荐）
表可以保留，不影响旧代码运行。系统会自动检测表是否存在。

### 4. 删除数据库表（可选）
```sql
-- 仅在确定不需要时执行
DROP TABLE IF EXISTS TransportationOrderCategories;
```

## 监控建议

上线后需要监控的指标：

1. **TransportationOrderCategories表的记录数**
   ```sql
   SELECT COUNT(*) FROM TransportationOrderCategories;
   ```

2. **有品类明细的运输单比例**
   ```sql
   SELECT 
       COUNT(DISTINCT c.TransportOrderID) AS WithDetails,
       (SELECT COUNT(*) FROM TransportationOrders) AS TotalOrders,
       CAST(COUNT(DISTINCT c.TransportOrderID) * 100.0 / 
            (SELECT COUNT(*) FROM TransportationOrders) AS DECIMAL(5,2)) AS Percentage
   FROM TransportationOrderCategories c;
   ```

3. **数据对齐错误率**
   ```sql
   SELECT 
       COUNT(*) AS TotalRecords,
       SUM(CASE WHEN ABS(TotalAmount - (Weight * PricePerKg)) > 0.01 THEN 1 ELSE 0 END) AS ErrorRecords,
       CAST(SUM(CASE WHEN ABS(TotalAmount - (Weight * PricePerKg)) > 0.01 THEN 1 ELSE 0 END) * 100.0 / 
            COUNT(*) AS DECIMAL(5,2)) AS ErrorRate
   FROM TransportationOrderCategories;
   ```

## 支持联系

如有问题，请查看：
- 详细文档：`TRANSPORTATION_ORDER_CATEGORIES_OPTIMIZATION.md`
- 数据库脚本：`Database/CreateTransportationOrderCategoriesTable.sql`
- 应用日志：IIS日志或应用Debug输出
