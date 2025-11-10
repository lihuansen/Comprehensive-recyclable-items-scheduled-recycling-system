# 验证检查清单 (Verification Checklist)

## 部署前检查 (Pre-Deployment Checklist)

在部署这些修复之前，请按照以下步骤进行验证：

### 1. 数据库设置 ✓

#### 步骤 1.1: 执行数据库脚本

选择以下任一方法：

**方法 A: 使用自动化脚本（推荐）**
```batch
cd Database
SetupRequiredTables.bat
```

**方法 B: 使用 PowerShell**
```powershell
cd Database
.\SetupRequiredTables.ps1
```

**方法 C: 手动执行 SQL**
```sql
-- 在 SSMS 中依次执行：
1. CreateUserFeedbackTable.sql
2. CreateAdminContactMessagesTable.sql
```

#### 步骤 1.2: 验证表已创建

在 SQL Server Management Studio 中运行：

```sql
-- 检查表是否存在
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM 
    INFORMATION_SCHEMA.TABLES 
WHERE 
    TABLE_NAME IN ('UserFeedback', 'AdminContactConversations', 'AdminContactMessages')
ORDER BY 
    TABLE_NAME;
```

**预期结果：** 应该返回3行记录

#### 步骤 1.3: 验证表结构

```sql
-- 查看 UserFeedback 表结构
EXEC sp_columns 'UserFeedback';

-- 查看 AdminContactConversations 表结构
EXEC sp_columns 'AdminContactConversations';

-- 查看 AdminContactMessages 表结构
EXEC sp_columns 'AdminContactMessages';
```

**预期结果：** 每个表都应显示完整的列定义

### 2. 代码验证 ✓

#### 步骤 2.1: 验证文件更改

确认以下文件已正确修改：

- [ ] `Database/CreateUserFeedbackTable.sql` - 新创建
- [ ] `Database/SetupRequiredTables.bat` - 新创建
- [ ] `Database/SetupRequiredTables.ps1` - 新创建
- [ ] `Database/DATABASE_SETUP_INSTRUCTIONS.md` - 新创建
- [ ] `Database/README.md` - 已更新
- [ ] `ISSUE_FIX_SUMMARY.md` - 新创建
- [ ] `recycling.Web.UI/Views/Home/ContactAdmin.cshtml` - 已修改（移除冗余按钮）

#### 步骤 2.2: 验证 UI 更改

打开 `recycling.Web.UI/Views/Home/ContactAdmin.cshtml` 并检查：

1. **第 265-275 行**（聊天输入区域）
   - ✓ 应该只有"发送"和"结束对话"两个按钮
   - ✗ 不应该有"返回"按钮

2. **第 247-249 行**（侧边栏）
   - ✓ 应该保留"返回问题反馈"按钮

### 3. 功能测试 ✓

#### 测试 3.1: 问题反馈功能

**测试步骤：**
1. 启动应用程序
2. 使用测试账户登录
3. 导航到：首页 → 问题反馈 (或直接访问 `/Home/Feedback`)
4. 填写反馈表单：
   - 选择反馈类型：问题反馈
   - 反馈主题：测试反馈功能
   - 详细描述：这是一个测试反馈，用于验证功能是否正常工作
   - 联系邮箱：test@example.com（可选）
5. 点击"提交反馈"按钮

**预期结果：**
- ✓ 显示成功消息："反馈提交成功！感谢您的反馈"
- ✓ 表单自动清空
- ✓ 2秒后自动跳转到首页

**数据库验证：**
```sql
-- 查询最新的反馈记录
SELECT TOP 5 
    FeedbackID,
    FeedbackType,
    Subject,
    Description,
    Status,
    CreatedDate
FROM 
    UserFeedback 
ORDER BY 
    CreatedDate DESC;
```

**预期结果：** 应该看到刚才提交的测试反馈记录

#### 测试 3.2: 联系管理员功能

**测试步骤：**
1. 在问题反馈页面，点击底部的"联系我们"链接
2. 验证页面加载（不应该有数据库错误）
3. 等待自动创建会话
4. 在消息输入框中输入："测试消息"
5. 点击"发送"按钮

**预期结果：**
- ✓ 页面正确加载，没有错误提示
- ✓ 会话自动创建
- ✓ 消息成功发送
- ✓ 消息显示在聊天窗口中
- ✓ 消息输入框自动清空

**数据库验证：**
```sql
-- 查询最新的会话
SELECT TOP 5 
    ConversationID,
    UserID,
    StartTime,
    UserEnded,
    AdminEnded
FROM 
    AdminContactConversations 
ORDER BY 
    StartTime DESC;

-- 查询最新的消息
SELECT TOP 5 
    MessageID,
    SenderType,
    Content,
    SentTime
FROM 
    AdminContactMessages 
ORDER BY 
    SentTime DESC;
```

**预期结果：** 
- 会话表中应该有新的会话记录
- 消息表中应该有刚才发送的测试消息

#### 测试 3.3: UI 改进验证

**测试步骤：**
1. 在"联系管理员"页面
2. 检查聊天输入区域（页面底部）

**预期结果：**
- ✓ 输入区域只有3个元素：
  1. 消息输入框
  2. "发送"按钮
  3. "结束对话"按钮
- ✗ 输入区域**不应该有**"返回"按钮

**测试步骤：**
1. 检查左侧边栏

**预期结果：**
- ✓ 侧边栏应该有"返回问题反馈"按钮
- ✓ 点击该按钮应该跳转到 `/Home/Feedback`

### 4. 错误处理测试 ✓

#### 测试 4.1: 反馈表单验证

**测试步骤：**
1. 访问问题反馈页面
2. 不填写任何内容，直接点击"提交反馈"

**预期结果：**
- ✓ 浏览器应该显示必填字段提示
- ✓ 表单不应该提交

**测试步骤：**
1. 填写反馈表单，但主题只输入1个字符
2. 点击"提交反馈"

**预期结果：**
- ✓ 应该显示验证错误（主题太短）

#### 测试 4.2: 未登录状态测试

**测试步骤：**
1. 退出登录
2. 尝试访问 `/Home/Feedback`

**预期结果：**
- ✓ 应该重定向到登录页面

**测试步骤：**
1. 退出登录
2. 尝试访问 `/Home/ContactAdmin`

**预期结果：**
- ✓ 应该重定向到登录选择页面

### 5. 浏览器兼容性测试 ✓

在以下浏览器中测试功能：

- [ ] Chrome/Edge (最新版)
- [ ] Firefox (最新版)
- [ ] Safari (如果在 Mac 上)

**测试内容：**
- 问题反馈页面显示正常
- 表单提交正常
- 联系管理员页面显示正常
- 消息发送正常
- 按钮点击正常

### 6. 性能测试 ✓

#### 测试 6.1: 数据库查询性能

```sql
-- 测试反馈查询性能
SET STATISTICS TIME ON;
SELECT * FROM UserFeedback WHERE Status = N'待处理' ORDER BY CreatedDate DESC;
SET STATISTICS TIME OFF;

-- 测试会话查询性能
SET STATISTICS TIME ON;
SELECT * FROM AdminContactConversations WHERE UserEnded = 0 OR AdminEnded = 0;
SET STATISTICS TIME OFF;
```

**预期结果：** 查询应该在100毫秒内完成

#### 测试 6.2: 索引验证

```sql
-- 查看 UserFeedback 表的索引
EXEC sp_helpindex 'UserFeedback';

-- 查看 AdminContactConversations 表的索引
EXEC sp_helpindex 'AdminContactConversations';

-- 查看 AdminContactMessages 表的索引
EXEC sp_helpindex 'AdminContactMessages';
```

**预期结果：** 每个表都应该有适当的索引

### 7. 安全检查 ✓

#### 检查 7.1: SQL 注入防护

验证代码使用参数化查询：

- [ ] `FeedbackDAL.cs` - 使用 `SqlParameter`
- [ ] `AdminContactDAL.cs` - 使用 `SqlParameter`

#### 检查 7.2: 输入验证

验证以下验证规则：

- [ ] 反馈类型：只允许预定义的4种类型
- [ ] 反馈主题：最大100字符
- [ ] 详细描述：10-1000字符
- [ ] 邮箱：有效的邮箱格式（可选）
- [ ] 消息内容：非空，最大2000字符

#### 检查 7.3: XSS 防护

验证输出编码：

- [ ] 反馈内容在显示时正确编码
- [ ] 消息内容在显示时正确编码
- [ ] JavaScript 使用 `escapeHtml()` 函数

### 8. 文档检查 ✓

验证以下文档已创建/更新：

- [ ] `Database/CreateUserFeedbackTable.sql` - SQL 脚本文档齐全
- [ ] `Database/SetupRequiredTables.bat` - 包含中英文注释
- [ ] `Database/SetupRequiredTables.ps1` - 包含帮助信息
- [ ] `Database/DATABASE_SETUP_INSTRUCTIONS.md` - 详细的设置说明
- [ ] `Database/README.md` - 更新了快速开始部分
- [ ] `ISSUE_FIX_SUMMARY.md` - 完整的问题分析和解决方案

## 部署步骤 (Deployment Steps)

### 1. 备份数据库

```sql
-- 备份当前数据库
BACKUP DATABASE RecyclingDB 
TO DISK = 'C:\Backups\RecyclingDB_BeforeUpdate_20231110.bak'
WITH FORMAT, COMPRESSION;
```

### 2. 部署数据库更改

```batch
cd Database
SetupRequiredTables.bat
```

或手动执行 SQL 脚本。

### 3. 部署代码更改

1. 停止 IIS 应用程序池
2. 部署新的代码文件
3. 启动 IIS 应用程序池

### 4. 验证部署

按照上面的功能测试步骤验证所有功能正常工作。

### 5. 监控

部署后监控以下内容：

- 应用程序日志
- 数据库错误日志
- 用户反馈提交率
- 管理员联系功能使用率

## 回滚计划 (Rollback Plan)

如果部署出现问题：

### 1. 回滚数据库

```sql
-- 删除新创建的表（如果需要）
DROP TABLE IF EXISTS AdminContactMessages;
DROP TABLE IF EXISTS AdminContactConversations;
DROP TABLE IF EXISTS UserFeedback;

-- 恢复数据库备份
RESTORE DATABASE RecyclingDB 
FROM DISK = 'C:\Backups\RecyclingDB_BeforeUpdate_20231110.bak'
WITH REPLACE;
```

### 2. 回滚代码

1. 停止 IIS 应用程序池
2. 恢复之前的代码版本
3. 启动 IIS 应用程序池

## 验证完成 (Verification Complete)

完成所有测试后，在此签字确认：

- 数据库设置：[ ] 完成
- 功能测试：[ ] 通过
- 错误处理：[ ] 验证
- 性能测试：[ ] 通过
- 安全检查：[ ] 完成
- 文档检查：[ ] 完成

**验证人员：** _______________  
**验证日期：** _______________  
**备注：** _______________________________________

## 常见问题 (Troubleshooting)

### 问题：数据库脚本执行失败

**解决方案：**
1. 检查数据库连接字符串
2. 确认有足够的数据库权限
3. 查看 SQL Server 错误日志

### 问题：反馈提交失败

**解决方案：**
1. 检查 `UserFeedback` 表是否存在
2. 查看应用程序日志
3. 验证数据库连接

### 问题：联系管理员功能报错

**解决方案：**
1. 检查 `AdminContactConversations` 表是否存在
2. 检查 `AdminContactMessages` 表是否存在
3. 查看浏览器控制台错误
4. 查看应用程序日志

## 联系支持

如果遇到任何问题，请联系技术支持团队。
