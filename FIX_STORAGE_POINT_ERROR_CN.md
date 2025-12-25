# 修复暂存点管理网络错误（状态：500）

## 问题描述

在测试中，进行回收员端的界面，点击**暂存点管理**，会弹出**网络问题，请重试（状态：500）**。

## 问题原因

此错误发生的根本原因是：**Inventory（库存）表未在数据库中创建**。

暂存点管理功能依赖 `Inventory` 表来存储和查询回收员的库存数据。当表不存在时，系统尝试查询数据时会触发 SQL 异常，导致 HTTP 500 错误。

## 快速解决方案

### 方法 1：使用自动化脚本（**推荐**）

#### Windows 用户：

1. 打开命令提示符（CMD）或 PowerShell
2. 进入 Database 目录：
   ```batch
   cd Database
   ```
3. 运行修复脚本：
   ```batch
   FixStoragePointManagement.bat
   ```

脚本会自动：
- ✅ 检查 SQL Server 连接
- ✅ 验证 RecyclingSystemDB 数据库存在
- ✅ 创建 Inventory 表（如果不存在）
- ✅ 验证表创建成功

#### Linux/macOS 用户：

1. 打开终端
2. 进入 Database 目录：
   ```bash
   cd Database
   ```
3. 赋予脚本执行权限：
   ```bash
   chmod +x FixStoragePointManagement.sh
   ```
4. 运行修复脚本：
   ```bash
   ./FixStoragePointManagement.sh
   ```

### 方法 2：使用 SQL Server Management Studio (SSMS)

1. 打开 SQL Server Management Studio
2. 连接到您的 SQL Server 实例
3. 在**对象资源管理器**中，找到并选择 `RecyclingSystemDB` 数据库
4. 打开新查询窗口
5. 打开文件：`Database/CreateInventoryTable.sql`
6. 点击**执行**按钮（或按 F5）

### 方法 3：使用命令行

```bash
cd Database
sqlcmd -S localhost -d RecyclingSystemDB -i CreateInventoryTable.sql
```

**注意**：如果您的 SQL Server 实例不是 `localhost`，请将上面的 `localhost` 替换为您的服务器名称。

## 验证修复是否成功

### 步骤 1：验证表已创建

在 SSMS 中执行以下查询：

```sql
-- 检查表是否存在
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Inventory';

-- 查看表结构
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Inventory'
ORDER BY ORDINAL_POSITION;
```

**预期结果**：应该返回 Inventory 表的信息和列定义。

### 步骤 2：测试暂存点管理功能

1. 启动 Web 应用程序
2. 以**回收员**身份登录系统
3. 点击导航栏中的**"暂存点管理"**
4. 页面应该正常加载

**预期结果**：
- ✅ 页面正常加载，**不**显示"网络问题，请重试"错误
- ✅ 如果还没有库存数据，显示"暂无库存数据"（这是正常的）
- ✅ 如果有库存数据，显示统计概览和类别卡片

### 步骤 3：生成测试数据（可选）

如果想查看有数据时的效果：

1. 以回收员身份登录
2. 在订单管理中接收一个订单
3. 确保订单包含**类别**和**重量**信息
4. 完成订单（点击"完成订单"按钮）
   - 完成订单时，系统会自动将数据写入 Inventory 表
5. 返回**暂存点管理**页面，刷新

应该能看到：
- 📊 统计概览：显示类别数、总重量、总价值
- 🗂️ 类别卡片：显示各类别的汇总信息
- 📝 点击卡片可查看该类别的详细库存记录

## 如果问题仍然存在

### 1. 检查数据库名称

确认 `Web.config` 中的数据库名称：

```xml
<connectionStrings>
    <add name="RecyclingDB" 
         connectionString="data source=.;initial catalog=RecyclingSystemDB;..." />
</connectionStrings>
```

**重要**：数据库名称是 `RecyclingSystemDB`（不是 `RecyclingDB`）。

如果您的数据库使用不同的名称，请：
- 修改 `FixStoragePointManagement.bat` 或 `.sh` 脚本中的 `DATABASE` 变量
- 或者在手动执行 SQL 时使用正确的数据库名称

### 2. 检查 SQL Server 连接

测试数据库连接：

```bash
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

如果失败，请确认：
- SQL Server 服务正在运行
- 您有足够的权限访问数据库
- 服务器名称正确

### 3. 查看详细错误信息

1. 打开浏览器开发者工具（按 F12）
2. 切换到 **Console（控制台）** 标签
3. 重新加载暂存点管理页面
4. 查看是否有 JavaScript 错误或详细的错误消息

也可以查看 **Network（网络）** 标签：
- 找到 `GetStoragePointSummary` 请求
- 查看响应内容
- 记录状态码和错误详情

### 4. 检查外键约束

Inventory 表有外键约束，确保以下表存在：
- `Appointments` 表（订单表）
- `Recyclers` 表（回收员表）

如果这些表不存在，请运行：

```bash
sqlcmd -S localhost -d RecyclingSystemDB -i Database/CreateAllTables.sql
```

### 5. 查看应用程序日志

如果在 Visual Studio 中运行应用程序：
1. 打开 **Output（输出）** 窗口
2. 在下拉菜单中选择 **Debug（调试）**
3. 查找包含 "SQL错误" 或 "获取库存汇总错误" 的消息

## 技术细节

### Inventory 表结构

```sql
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
```

### 数据流程

1. **订单完成时**：
   - 回收员点击"完成订单"
   - 系统调用 `AddInventoryFromOrder` 方法
   - 从 `AppointmentCategories` 表读取类别和重量数据
   - 将数据插入 `Inventory` 表

2. **查看库存时**：
   - 用户访问"暂存点管理"页面
   - 前端调用 `GetStoragePointSummary` API
   - 系统从 `Inventory` 表按 `RecyclerID` 查询数据
   - 按类别汇总并返回结果

3. **数据隔离**：
   - 每个回收员只能看到自己的库存数据
   - 通过 `RecyclerID` 进行过滤
   - 确保数据安全和隐私

## 相关文档

- [STORAGE_POINT_TROUBLESHOOTING.md](STORAGE_POINT_TROUBLESHOOTING.md) - 详细故障排查指南
- [STORAGE_POINT_FIX_README.md](Database/STORAGE_POINT_FIX_README.md) - 修复说明
- [DATABASE_SETUP_INSTRUCTIONS.md](Database/DATABASE_SETUP_INSTRUCTIONS.md) - 数据库设置指南
- [STORAGE_POINT_NETWORK_ERROR_SOLUTION.md](STORAGE_POINT_NETWORK_ERROR_SOLUTION.md) - 完整解决方案文档

## 总结

暂存点管理的"网络问题，请重试（状态：500）"错误是由于 **Inventory 表缺失**导致的。

**解决方法**：
1. ✅ 运行 `FixStoragePointManagement.bat` 或 `.sh` 脚本
2. ✅ 或手动执行 `CreateInventoryTable.sql` 脚本
3. ✅ 验证表创建成功
4. ✅ 测试暂存点管理功能

修复后，暂存点管理功能应该可以正常使用。如果仍有问题，请参考上面的故障排查步骤或查看相关文档。
