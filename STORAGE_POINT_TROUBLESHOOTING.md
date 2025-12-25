# 暂存点管理功能故障排查指南

## 问题：显示"网络问题，请重试"或"网络错误，请重试"

### 可能原因及解决方案

#### 1. Inventory表未创建

**症状**: 
- 页面显示错误信息："数据库错误，请确保Inventory表已创建"
- 浏览器控制台显示SQL相关错误

**解决方案**:
1. 连接到SQL Server数据库
2. 执行以下脚本创建Inventory表：

```sql
-- 运行文件: Database/CreateInventoryTable.sql
-- 或直接执行以下SQL

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Inventory] (
        [InventoryID] INT PRIMARY KEY IDENTITY(1,1),
        [OrderID] INT NOT NULL,
        [CategoryKey] NVARCHAR(50) NOT NULL,
        [CategoryName] NVARCHAR(50) NOT NULL,
        [Weight] DECIMAL(10, 2) NOT NULL,
        [Price] DECIMAL(10, 2) NULL,
        [RecyclerID] INT NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [FK_Inventory_Appointments] FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT [FK_Inventory_Recyclers] FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT [CK_Inventory_Weight] CHECK ([Weight] > 0),
        CONSTRAINT [CK_Inventory_Price] CHECK ([Price] IS NULL OR [Price] >= 0)
    );

    CREATE INDEX [IX_Inventory_OrderID] ON [dbo].[Inventory]([OrderID]);
    CREATE INDEX [IX_Inventory_RecyclerID] ON [dbo].[Inventory]([RecyclerID]);
    CREATE INDEX [IX_Inventory_CategoryKey] ON [dbo].[Inventory]([CategoryKey]);
    CREATE INDEX [IX_Inventory_CreatedDate] ON [dbo].[Inventory]([CreatedDate]);
END
GO
```

3. 验证表创建成功：
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Inventory'
```

#### 2. 数据库连接问题

**症状**:
- 错误信息包含"连接超时"或"无法连接到数据库"
- 其他功能也无法正常使用

**解决方案**:
1. 检查 `Web.config` 中的连接字符串：
```xml
<connectionStrings>
    <add name="RecyclingSystemDB" 
         connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

2. 确认SQL Server服务正在运行
3. 测试数据库连接是否正常

#### 3. 没有库存数据

**症状**:
- 页面正常加载，但显示"暂无库存数据"
- 统计显示全部为0

**这不是错误！** 这是正常情况，表示：
- Inventory表存在但为空
- 该回收员还没有完成过订单
- 或者完成的订单中没有记录重量

**解决方案**:
1. 完成一个订单来测试功能：
   - 以回收员身份登录
   - 接收并处理订单
   - 确保订单中有类别和重量信息
   - 完成订单（点击"完成订单"按钮）
2. 刷新暂存点管理页面，应该能看到库存数据

#### 4. 权限问题

**症状**:
- 显示"权限不足"或"未登录"

**解决方案**:
1. 确保以回收员身份登录（不是管理员或其他角色）
2. 如果已登录，尝试退出后重新登录
3. 检查Session是否过期

#### 5. CSRF Token问题

**症状**:
- HTTP 400或403错误
- 错误信息提到"验证失败"

**解决方案**:
1. 清除浏览器缓存
2. 刷新页面重新获取token
3. 确保`@Html.AntiForgeryToken()`正确生成

#### 6. JavaScript错误

**症状**:
- 页面加载后没有反应
- 浏览器控制台显示JavaScript错误

**解决方案**:
1. 打开浏览器开发者工具（F12）
2. 查看Console标签中的错误信息
3. 确认jQuery已正确加载
4. 检查是否有脚本冲突

## 调试步骤

### 步骤1: 检查浏览器控制台
1. 按F12打开开发者工具
2. 切换到Console标签
3. 重新加载页面
4. 查看是否有错误信息

**预期输出**:
```
GetStoragePointSummary 响应: {success: true, data: [...]}
```

**如果看到错误**:
- 记录完整的错误消息
- 查看Network标签中的请求详情
- 检查返回的状态码（200, 400, 500等）

### 步骤2: 检查网络请求
1. 在开发者工具中切换到Network标签
2. 重新加载页面
3. 找到`GetStoragePointSummary`请求
4. 查看：
   - 状态码（应该是200）
   - 响应内容
   - 请求参数

### 步骤3: 直接测试数据库
运行以下SQL查询测试数据：

```sql
-- 检查表是否存在
SELECT * FROM sys.tables WHERE name = 'Inventory'

-- 查看表结构
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory'

-- 查看数据（如果有）
SELECT TOP 10 * FROM Inventory ORDER BY CreatedDate DESC

-- 检查特定回收员的数据
SELECT * FROM Inventory WHERE RecyclerID = <YOUR_RECYCLER_ID>
```

### 步骤4: 检查应用程序日志
查看 `System.Diagnostics.Debug.WriteLine` 输出：
- 在Visual Studio中运行项目
- 查看Output窗口中的调试信息
- 寻找以"SQL错误:"或"获取库存汇总错误:"开头的消息

## 常见问题FAQ

### Q1: 为什么我看不到任何库存数据？
**A**: 库存数据来自已完成的订单。只有当订单状态变更为"已完成"时，系统才会自动将订单中的回收物品信息写入库存表。确保：
1. 订单包含类别和重量信息
2. 订单已正确完成
3. `ExecuteOrderCompletion`方法成功调用了`AddInventoryFromOrder`

### Q2: 我完成了订单，但库存中没有显示？
**A**: 检查以下几点：
1. 订单完成时是否有错误提示？
2. 订单的`AppointmentCategories`表中是否有数据？
```sql
SELECT * FROM AppointmentCategories WHERE AppointmentID = <YOUR_ORDER_ID>
```
3. 检查是否有外键约束错误（RecyclerID是否有效）

### Q3: 不同回收员看到的数据是一样的？
**A**: 这是数据隔离失败。检查：
1. 登录的回收员信息是否正确存储在Session中
2. `GetInventorySummary`和`GetInventoryList`方法是否正确传递了RecyclerID
3. SQL查询中的WHERE条件是否正确

### Q4: 页面加载很慢？
**A**: 可能是数据量太大。优化方案：
1. 检查是否有大量库存数据
2. 添加日期范围过滤
3. 优化索引（已在表创建脚本中包含）
4. 考虑减少`pageSize`参数

## 联系支持

如果以上方法都无法解决问题，请提供以下信息：
1. 完整的错误消息（包括浏览器控制台输出）
2. Network标签中的请求/响应详情
3. 数据库版本和配置
4. 是否能够成功完成订单
5. 其他功能是否正常工作

## 相关文档

- [暂存点管理功能实现说明](STORAGE_POINT_MANAGEMENT_IMPLEMENTATION.md)
- [完成总结](STORAGE_POINT_COMPLETION_SUMMARY.md)
- [数据库架构](DATABASE_SCHEMA.md)
