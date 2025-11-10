# 数据库设置说明 (Database Setup Instructions)

## 概述 (Overview)

本文档说明如何正确设置数据库，以确保所有功能正常工作。

## 必需的数据库表 (Required Database Tables)

### 1. 用户反馈表 (User Feedback Table)

**表名**: `UserFeedback`

**用途**: 存储用户提交的问题反馈、功能建议、投诉举报等信息

**创建脚本**: `CreateUserFeedbackTable.sql`

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
-- 或使用命令行工具执行
sqlcmd -S localhost -d RecyclingDB -i CreateUserFeedbackTable.sql
```

### 2. 管理员联系表 (Admin Contact Tables)

**表名**: `AdminContactConversations` 和 `AdminContactMessages`

**用途**: 存储用户与管理员之间的对话记录

**创建脚本**: `CreateAdminContactMessagesTable.sql`

**执行方法**:
```sql
-- 在 SQL Server Management Studio 中运行以下脚本
sqlcmd -S localhost -d RecyclingDB -i CreateAdminContactMessagesTable.sql
```

## 数据库设置步骤 (Setup Steps)

### 方法1: 使用 SQL Server Management Studio (SSMS)

1. 打开 SQL Server Management Studio
2. 连接到您的数据库服务器
3. 选择 `RecyclingDB` 数据库（或您的数据库名称）
4. 依次打开并执行以下脚本：
   - `CreateUserFeedbackTable.sql`
   - `CreateAdminContactMessagesTable.sql`
   - `CreateHomepageCarouselTable.sql`（如果需要）
   - `CreateOrderReviewsTable.sql`（如果需要）
   - `CreateInventoryTable.sql`（如果需要）

### 方法2: 使用命令行 (Command Line)

```bash
# 进入 Database 目录
cd Database

# 依次执行必需的脚本
sqlcmd -S localhost -d RecyclingDB -i CreateUserFeedbackTable.sql
sqlcmd -S localhost -d RecyclingDB -i CreateAdminContactMessagesTable.sql

# 执行其他可选脚本（如果需要）
sqlcmd -S localhost -d RecyclingDB -i CreateHomepageCarouselTable.sql
sqlcmd -S localhost -d RecyclingDB -i CreateOrderReviewsTable.sql
sqlcmd -S localhost -d RecyclingDB -i CreateInventoryTable.sql
```

### 方法3: 使用批处理脚本 (Batch Script)

创建一个批处理文件 `SetupDatabase.bat`:

```batch
@echo off
echo 开始设置数据库表...
echo.

echo 创建用户反馈表...
sqlcmd -S localhost -d RecyclingDB -i CreateUserFeedbackTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 用户反馈表创建失败
    pause
    exit /b 1
)

echo 创建管理员联系表...
sqlcmd -S localhost -d RecyclingDB -i CreateAdminContactMessagesTable.sql
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
        'UserFeedback', 
        'AdminContactConversations', 
        'AdminContactMessages'
    )
ORDER BY 
    TABLE_NAME;

-- 查看表结构
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
    <add name="RecyclingDB" 
         connectionString="Data Source=localhost;Initial Catalog=RecyclingDB;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

## 功能测试 (Feature Testing)

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
