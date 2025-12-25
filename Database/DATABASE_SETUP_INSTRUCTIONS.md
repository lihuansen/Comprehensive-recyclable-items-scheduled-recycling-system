# 数据库设置说明 (Database Setup Instructions)

## 概述 (Overview)

本文档说明如何正确设置数据库，以确保所有功能正常工作。

## 必需的数据库表 (Required Database Tables)

### 0. 管理员操作日志表 (Admin Operation Logs Table)

**表名**: `AdminOperationLogs`

**用途**: 记录管理员的所有操作日志，用于日志管理功能

**创建脚本**: `CreateAdminOperationLogsTable.sql`

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
-- 或使用命令行工具执行
sqlcmd -S localhost -d RecyclingSystemDB -i CreateAdminOperationLogsTable.sql
```

**验证表创建**:
```sql
SELECT COUNT(*) AS RecordCount FROM AdminOperationLogs;
-- 如果返回 0，表示表已创建成功但没有数据
-- 管理员进行任何操作后，可以在此表中看到记录
```

### 1. 用户反馈表 (User Feedback Table)

**表名**: `UserFeedback`

**用途**: 存储用户提交的问题反馈、功能建议、投诉举报等信息

**创建脚本**: `CreateUserFeedbackTable.sql`

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
-- 或使用命令行工具执行
sqlcmd -S localhost -d RecyclingSystemDB -i CreateUserFeedbackTable.sql
```

### 3. 库存表 (Inventory Table) - **暂存点管理必需**

**表名**: `Inventory`

**用途**: 存储回收员的库存管理信息，用于暂存点管理功能

**创建脚本**: `CreateInventoryTable.sql`

**重要性**: ⚠️ **此表是暂存点管理功能的必需表**。如果不创建此表，回收员端的"暂存点管理"功能将无法使用，会显示"网络问题，请重试"错误。

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
-- 或使用命令行工具执行
sqlcmd -S localhost -d RecyclingSystemDB -i CreateInventoryTable.sql
```

**验证表创建**:
```sql
-- 检查表是否存在
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Inventory';

-- 查看表结构
EXEC sp_help 'Inventory';

-- 查看索引
SELECT name, type_desc FROM sys.indexes WHERE object_id = OBJECT_ID('Inventory');
```

### 4. 管理员联系表 (Admin Contact Tables)

**表名**: `AdminContactConversations` 和 `AdminContactMessages`

**用途**: 存储用户与管理员之间的对话记录

**创建脚本**: `CreateAdminContactMessagesTable.sql`

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
sqlcmd -S localhost -d RecyclingSystemDB -i CreateAdminContactMessagesTable.sql
```

## 数据库设置步骤 (Setup Steps)

### 方法1: 使用 SQL Server Management Studio (SSMS)

1. 打开 SQL Server Management Studio
2. 连接到您的数据库服务器
3. 选择 `RecyclingSystemDB` 数据库（或您的数据库名称）
4. 依次打开并执行以下脚本：
   - `CreateAdminOperationLogsTable.sql`（管理员日志表 - **推荐首先执行**）
   - `CreateInventoryTable.sql`（库存表 - **暂存点管理必需**）
   - `CreateUserFeedbackTable.sql`
   - `CreateAdminContactMessagesTable.sql`
   - `CreateHomepageCarouselTable.sql`（如果需要）
   - `CreateOrderReviewsTable.sql`（如果需要）

### 方法2: 使用命令行 (Command Line)

```bash
# 进入 Database 目录
cd Database

# 首先执行管理员操作日志表脚本
sqlcmd -S localhost -d RecyclingSystemDB -i CreateAdminOperationLogsTable.sql

# 执行库存表脚本（暂存点管理必需）
sqlcmd -S localhost -d RecyclingSystemDB -i CreateInventoryTable.sql

# 依次执行其他必需的脚本
sqlcmd -S localhost -d RecyclingSystemDB -i CreateUserFeedbackTable.sql
sqlcmd -S localhost -d RecyclingSystemDB -i CreateAdminContactMessagesTable.sql

# 执行其他可选脚本（如果需要）
sqlcmd -S localhost -d RecyclingSystemDB -i CreateHomepageCarouselTable.sql
sqlcmd -S localhost -d RecyclingSystemDB -i CreateOrderReviewsTable.sql
```

### 方法3: 使用批处理脚本 (Batch Script)

创建一个批处理文件 `SetupDatabase.bat`:

```batch
@echo off
echo 开始设置数据库表...
echo.

echo 创建用户反馈表...
sqlcmd -S localhost -d RecyclingSystemDB -i CreateUserFeedbackTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 用户反馈表创建失败
    pause
    exit /b 1
)

echo 创建管理员联系表...
sqlcmd -S localhost -d RecyclingSystemDB -i CreateAdminContactMessagesTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 管理员联系表创建失败
    pause
    exit /b 1
)

echo.
echo 数据库表创建完成！
pause
```

然后双击运行 `SetupDatabase.bat`。

## 验证数据库表 (Verify Database Tables)

执行以下 SQL 查询以验证表是否创建成功：

```sql
-- 检查表是否存在
SELECT 
    TABLE_NAME,
    CREATE_DATE = (SELECT create_date FROM sys.tables WHERE name = TABLE_NAME)
FROM 
    INFORMATION_SCHEMA.TABLES 
WHERE 
    TABLE_TYPE = 'BASE TABLE' 
    AND TABLE_NAME IN (
        'AdminOperationLogs',
        'Inventory', 
        'UserFeedback', 
        'AdminContactConversations', 
        'AdminContactMessages'
    )
ORDER BY 
    TABLE_NAME;

-- 查看表结构
EXEC sp_help 'Inventory';
EXEC sp_help 'UserFeedback';
EXEC sp_help 'AdminContactConversations';
EXEC sp_help 'AdminContactMessages';
```

## 常见问题 (Troubleshooting)

### 问题1: "对象名 'UserFeedback' 无效"

**原因**: `UserFeedback` 表未创建

**解决方案**: 执行 `CreateUserFeedbackTable.sql` 脚本

### 问题2: "对象名 'AdminContactConversations' 无效"

**原因**: `AdminContactConversations` 表未创建

**解决方案**: 执行 `CreateAdminContactMessagesTable.sql` 脚本

### 问题3: 连接字符串错误

**检查**: 确保 Web.config 中的连接字符串正确：

```xml
<connectionStrings>
    <add name="RecyclingSystemDB" 
         connectionString="Data Source=localhost;Initial Catalog=RecyclingSystemDB;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 问题4: 暂存点管理显示"网络问题，请重试"

**原因**: `Inventory` 表未创建

**症状**: 
- 回收员点击"暂存点管理"后显示错误
- 浏览器控制台显示SQL相关错误
- 错误信息提示"数据库错误，请确保Inventory表已创建"

**解决方案**: 
1. 执行 `CreateInventoryTable.sql` 脚本创建Inventory表
2. 验证表创建成功：
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Inventory';
```
3. 刷新暂存点管理页面

**相关文档**: 参见 `STORAGE_POINT_TROUBLESHOOTING.md` 获取详细的故障排查步骤

## 功能测试 (Feature Testing)

### 测试日志管理功能

1. 使用管理员账户登录
2. 执行一些管理操作（如导出用户数据、更新回收员信息等）
3. 导航到 "日志管理" 页面
4. 验证日志是否正确记录到数据库：

```sql
-- 查看最近的操作日志
SELECT TOP 20 
    LogID,
    AdminUsername AS '管理员',
    Module AS '模块',
    OperationType AS '操作类型',
    Description AS '描述',
    OperationTime AS '操作时间',
    Result AS '结果'
FROM AdminOperationLogs 
ORDER BY OperationTime DESC;

-- 查看今日操作统计
SELECT 
    Module AS '模块',
    OperationType AS '操作类型',
    COUNT(*) AS '操作次数'
FROM AdminOperationLogs 
WHERE CAST(OperationTime AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY Module, OperationType
ORDER BY COUNT(*) DESC;
```

### 测试用户反馈功能

1. 登录用户账户
2. 导航到 "问题反馈" 页面
3. 填写并提交反馈表单
4. 验证反馈是否成功保存到数据库：

```sql
SELECT TOP 10 * FROM UserFeedback ORDER BY CreatedDate DESC;
```

### 测试管理员联系功能

1. 在 "问题反馈" 页面点击 "联系我们"
2. 开始与管理员对话
3. 发送消息
4. 验证消息是否保存到数据库：

```sql
SELECT TOP 10 * FROM AdminContactConversations ORDER BY StartTime DESC;
SELECT TOP 10 * FROM AdminContactMessages ORDER BY SentTime DESC;
```

### 测试暂存点管理功能

⚠️ **前提条件**: Inventory表必须已创建

1. 以回收员身份登录系统
2. 完成一个订单（确保订单包含类别和重量信息）
3. 点击导航栏中的 "暂存点管理"
4. 验证页面能正常加载，不显示错误
5. 如果有数据，验证统计数据是否正确显示
6. 点击类别卡片，验证详细信息是否正确
7. 验证数据是否保存到数据库：

```sql
-- 查看最新的库存记录
SELECT TOP 10 
    i.InventoryID,
    'AP' + RIGHT('000000' + CAST(i.OrderID AS VARCHAR(6)), 6) AS '订单编号',
    i.CategoryName AS '类别',
    i.Weight AS '重量(kg)',
    i.Price AS '价值(元)',
    r.Username AS '回收员',
    i.CreatedDate AS '入库时间'
FROM Inventory i
LEFT JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
ORDER BY i.CreatedDate DESC;

-- 查看库存汇总（按类别）
SELECT 
    CategoryName AS '类别',
    SUM(Weight) AS '总重量(kg)',
    SUM(ISNULL(Price, 0)) AS '总价值(元)',
    COUNT(*) AS '记录数'
FROM Inventory
GROUP BY CategoryKey, CategoryName
ORDER BY CategoryName;

-- 查看特定回收员的库存
SELECT * FROM Inventory WHERE RecyclerID = <YOUR_RECYCLER_ID>;
```

## 数据库维护 (Database Maintenance)

### 定期清理旧数据

```sql
-- 清理90天前的已完成反馈
DELETE FROM UserFeedback 
WHERE Status = N'已完成' 
  AND CreatedDate < DATEADD(DAY, -90, GETDATE());

-- 清理180天前的已结束对话
DELETE FROM AdminContactConversations 
WHERE UserEnded = 1 
  AND AdminEnded = 1 
  AND UserEndedTime < DATEADD(DAY, -180, GETDATE());
```

## 备份建议 (Backup Recommendations)

定期备份重要数据：

```sql
-- 备份用户反馈
SELECT * INTO UserFeedback_Backup_20231110 FROM UserFeedback;

-- 备份管理员对话
SELECT * INTO AdminContactConversations_Backup_20231110 FROM AdminContactConversations;
SELECT * INTO AdminContactMessages_Backup_20231110 FROM AdminContactMessages;
```

## 联系支持 (Contact Support)

如果遇到数据库设置问题，请联系技术支持团队。
