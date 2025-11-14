# 用户联系请求功能实现文档

## 概述

本文档描述了重新设计的"联系我们"功能的实现。新的设计不再在用户点击"联系我们"时自动创建会话，而是记录用户的联系请求，由管理员主动发起对话。

## 设计理念

### 原有问题
- 用户点击"联系我们"后会自动创建聊天会话
- 包含历史对话功能，增加了复杂度
- 用户和管理员之间的交互不够明确

### 新设计
- 用户点击"联系我们"仅记录联系请求（标记状态为 true）
- 管理员可以查看所有待处理的联系请求
- 管理员主动选择要联系的用户并发起对话
- 移除历史对话功能，简化用户体验
- 聊天界面显示居中的系统消息"管理在线客服"

## 数据库设计

### UserContactRequests 表

```sql
CREATE TABLE [dbo].[UserContactRequests] (
    [RequestID] INT IDENTITY(1,1) PRIMARY KEY,
    [UserID] INT NOT NULL,
    [RequestStatus] BIT NOT NULL DEFAULT 1,  -- 1=待联系, 0=已处理
    [RequestTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ContactedTime] DATETIME2 NULL,
    [AdminID] INT NULL,
    CONSTRAINT FK_UserContactRequests_Users FOREIGN KEY ([UserID]) 
        REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
);
```

**字段说明：**
- `RequestID`: 请求的唯一标识符
- `UserID`: 发起请求的用户ID
- `RequestStatus`: 请求状态（true=待处理，false=已处理）
- `RequestTime`: 请求创建时间
- `ContactedTime`: 管理员处理时间
- `AdminID`: 处理该请求的管理员ID

**索引：**
- `IX_UserContactRequests_UserID`: 按用户ID查询
- `IX_UserContactRequests_RequestStatus`: 按状态过滤
- `IX_UserContactRequests_RequestTime`: 按时间排序

## 架构设计

### 层次结构

```
┌─────────────────────┐
│   View Layer        │  ContactAdmin.cshtml (用户端)
│                     │  UserContactManagement.cshtml (管理员端)
├─────────────────────┤
│  Controller Layer   │  HomeController.cs
│                     │  StaffController.cs
├─────────────────────┤
│   BLL Layer         │  UserContactRequestsBLL.cs
│                     │  AdminContactBLL.cs
├─────────────────────┤
│   DAL Layer         │  UserContactRequestsDAL.cs
│                     │  AdminContactDAL.cs
├─────────────────────┤
│   Model Layer       │  UserContactRequests.cs
│                     │  UserContactRequestViewModel.cs
│                     │  AdminContactMessages.cs
│                     │  AdminContactConversations.cs
└─────────────────────┘
```

## 业务流程

### 用户端流程

```
1. 用户登录系统
   ↓
2. 访问"问题反馈"页面
   ↓
3. 点击"联系我们"链接
   ↓
4. 系统创建 UserContactRequests 记录（RequestStatus = true）
   ↓
5. 显示确认消息："您的联系请求已提交，管理员会尽快与您联系"
   ↓
6. 用户等待管理员发起对话
   ↓
7. 管理员发起对话后，聊天界面自动激活
   ↓
8. 用户可以发送消息与管理员对话
```

### 管理员端流程

```
1. 管理员登录系统
   ↓
2. 访问"用户联系管理"页面
   ↓
3. 查看"待联系"标签页（默认显示）
   ↓
4. 看到所有请求联系的用户列表
   ↓
5. 点击某个用户记录
   ↓
6. 系统：
   - 标记请求为已处理（RequestStatus = false）
   - 创建 AdminContactConversations 记录
   - 发送系统消息"管理在线客服"
   ↓
7. 管理员可以在聊天界面与用户对话
```

## 关键实现

### 1. DAL 层 (UserContactRequestsDAL.cs)

**CreateContactRequest()**
```csharp
// 检查是否已有待处理请求，避免重复
// 如果没有，创建新请求
// 返回请求ID或0（已存在）
```

**GetPendingRequests()**
```csharp
// 联接 Users 表获取用户信息
// 只返回 RequestStatus = 1 的记录
// 按请求时间降序排列
```

**MarkAsContacted()**
```csharp
// 将 RequestStatus 设为 0
// 记录 ContactedTime 和 AdminID
```

### 2. BLL 层 (UserContactRequestsBLL.cs)

提供业务逻辑封装和输入验证：
- 验证 UserID 有效性
- 处理异常并返回友好错误消息
- 封装 DAL 调用

### 3. Controller 层

**HomeController.ContactAdmin()**
```csharp
[HttpGet]
public ActionResult ContactAdmin()
{
    // 检查登录
    // 创建联系请求
    // 返回视图显示确认消息
}
```

**StaffController 新增方法：**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult GetPendingContactRequests()
{
    // 返回所有待处理请求
}

[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult StartContactWithUser(int requestId, int userId)
{
    // 标记请求为已处理
    // 创建会话
    // 发送系统欢迎消息
}
```

### 4. View 层

**ContactAdmin.cshtml 关键改动：**

- 移除历史对话功能
- 移除会话列表
- 默认显示请求确认消息
- 聊天界面初始为隐藏状态
- 显示居中的系统消息"管理在线客服"
- JavaScript 不自动创建会话

**UserContactManagement.cshtml 关键改动：**

- 新增"待联系"标签页作为默认视图
- 显示待处理请求列表
- 每个请求显示用户名、手机号、请求时间
- 点击请求可发起对话
- 集成 CSRF 保护

## 安全性

### CSRF 保护

所有新的 POST 端点都添加了 `[ValidateAntiForgeryToken]` 属性：

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult GetPendingContactRequests() { ... }

[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult StartContactWithUser(...) { ... }
```

视图中添加了 CSRF 令牌：
```html
@Html.AntiForgeryToken()
```

JavaScript 在 POST 请求中包含令牌：
```javascript
var formData = new FormData();
formData.append('__RequestVerificationToken', getAntiForgeryToken());
```

### SQL 注入防护

所有数据库操作使用参数化查询：
```csharp
cmd.Parameters.AddWithValue("@UserID", userId);
```

### 身份验证

所有端点都检查会话状态：
```csharp
if (Session["LoginUser"] == null)
    return RedirectToAction("LoginSelect", "Home");

if (Session["LoginStaff"] == null)
    return Json(new { success = false, message = "请先登录" });
```

## 部署步骤

### 1. 数据库迁移

在 SQL Server 中执行：
```sql
-- 位置: Database/CreateUserContactRequestsTable.sql
```

验证表已创建：
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'UserContactRequests'
```

### 2. 编译项目

在 Visual Studio 中：
1. 打开解决方案
2. 清理解决方案 (Clean Solution)
3. 重新生成解决方案 (Rebuild Solution)
4. 确保没有编译错误

### 3. 部署到服务器

复制以下文件到服务器：
- 所有 .dll 文件
- 更新的 .cshtml 视图文件
- Web.config（如有更改）

### 4. 测试部署

**用户端测试：**
1. 使用测试用户登录
2. 访问 `/Home/Feedback`
3. 点击"联系我们"
4. 验证显示确认消息
5. 检查数据库中是否创建了记录

**管理员端测试：**
1. 使用管理员账户登录
2. 访问 `/Staff/UserContactManagement`
3. 查看"待联系"标签页
4. 验证能看到测试用户的请求
5. 点击用户记录发起对话
6. 发送测试消息
7. 验证用户端能收到消息

## 配置说明

### Web.config 连接字符串

确保正确配置数据库连接：
```xml
<connectionStrings>
  <add name="RecyclingDB" 
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=RecyclingDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Session 设置

默认 Session 超时为 30 分钟：
```xml
<system.web>
  <sessionState timeout="30" />
</system.web>
```

## 故障排除

### 问题1: "表 UserContactRequests 不存在"

**解决方案：**
- 确认已执行 `CreateUserContactRequestsTable.sql`
- 检查数据库连接是否正确
- 验证数据库用户是否有创建表的权限

### 问题2: 管理员看不到待处理请求

**可能原因：**
- 没有用户点击过"联系我们"
- 请求已被标记为已处理
- 数据库查询失败

**检查方法：**
```sql
-- 查询所有请求
SELECT * FROM UserContactRequests ORDER BY RequestTime DESC

-- 查询待处理请求
SELECT * FROM UserContactRequests WHERE RequestStatus = 1
```

### 问题3: CSRF 令牌验证失败

**解决方案：**
- 确认视图中包含 `@Html.AntiForgeryToken()`
- 检查 JavaScript 是否正确获取和发送令牌
- 清除浏览器缓存
- 检查是否有代理或防火墙修改请求

### 问题4: 用户重复点击创建多个请求

**说明：**
这是预期行为。系统设计为只保留一个待处理请求：
```csharp
// DAL 中的检查
SELECT COUNT(*) FROM UserContactRequests 
WHERE UserID = @UserID AND RequestStatus = 1

// 如果已存在，返回 0 而不创建新记录
```

## API 参考

### HomeController

**ContactAdmin()**
- 方法: GET
- 路径: `/Home/ContactAdmin`
- 权限: 需要登录
- 返回: 视图，显示请求确认消息

### StaffController

**GetPendingContactRequests()**
- 方法: POST
- 路径: `/Staff/GetPendingContactRequests`
- 权限: 管理员
- CSRF: 需要令牌
- 返回: JSON `{ success: bool, requests: [] }`

**StartContactWithUser()**
- 方法: POST
- 路径: `/Staff/StartContactWithUser`
- 参数: `requestId` (int), `userId` (int)
- 权限: 管理员
- CSRF: 需要令牌
- 返回: JSON `{ success: bool, message: string, conversationId: int, userId: int }`

## 数据流示例

### 用户创建请求

```
POST /Home/ContactAdmin
→ HomeController.ContactAdmin()
  → UserContactRequestsBLL.CreateContactRequest(userId)
    → UserContactRequestsDAL.CreateContactRequest(userId)
      → 检查是否已有待处理请求
      → 创建新记录或返回 0
    ← 返回 OperationResult
  ← 设置 ViewBag.RequestMessage
← 返回视图显示确认
```

### 管理员发起对话

```
POST /Staff/StartContactWithUser
  requestId, userId, __RequestVerificationToken
→ StaffController.StartContactWithUser(requestId, userId)
  → ValidateAntiForgeryToken 验证
  → 检查管理员登录
  → UserContactRequestsBLL.MarkAsContacted(requestId, adminId)
    → UserContactRequestsDAL.MarkAsContacted(requestId, adminId)
      → UPDATE UserContactRequests SET RequestStatus = 0, ...
  → AdminContactBLL.GetOrCreateConversation(userId, adminId)
    → AdminContactDAL.GetOrCreateConversation(userId, adminId)
      → 创建会话记录
  → AdminContactBLL.SendMessage(userId, adminId, "system", "管理在线客服")
    → AdminContactDAL.SendMessage(...)
      → INSERT INTO AdminContactMessages ...
← 返回 JSON { success: true, conversationId, userId }
```

## 性能考虑

### 数据库索引

已创建以下索引以优化查询性能：
- `IX_UserContactRequests_UserID`
- `IX_UserContactRequests_RequestStatus`
- `IX_UserContactRequests_RequestTime`

### 查询优化

待处理请求查询使用索引：
```sql
SELECT ... FROM UserContactRequests 
WHERE RequestStatus = 1  -- 使用 IX_UserContactRequests_RequestStatus
ORDER BY RequestTime DESC -- 使用 IX_UserContactRequests_RequestTime
```

### 缓存策略

当前实现不使用缓存，每次都从数据库读取。如果请求量很大，可以考虑：
- 缓存待处理请求列表（30秒过期）
- 使用 Redis 或内存缓存
- 实现 SignalR 推送更新

## 扩展建议

### 功能增强

1. **通知系统**
   - 用户提交请求后发送短信/邮件通知管理员
   - 管理员回复后通知用户

2. **优先级排序**
   - 添加优先级字段
   - VIP 用户请求优先处理
   - 紧急请求标记

3. **统计报表**
   - 请求数量趋势
   - 平均响应时间
   - 管理员工作量统计

4. **自动分配**
   - 根据管理员在线状态自动分配
   - 负载均衡
   - 专业领域匹配

### 技术改进

1. **实时通信**
   - 集成 SignalR 实现真正的实时聊天
   - 消除轮询，减少服务器负载

2. **移动端优化**
   - 响应式设计改进
   - 移动应用 API

3. **安全加固**
   - 实现请求频率限制
   - 添加消息内容过滤
   - 敏感信息脱敏

## 维护指南

### 日常维护

1. **监控待处理请求**
   ```sql
   -- 查看超过1小时未处理的请求
   SELECT u.Username, r.RequestTime 
   FROM UserContactRequests r
   JOIN Users u ON r.UserID = u.UserID
   WHERE r.RequestStatus = 1 
     AND DATEDIFF(HOUR, r.RequestTime, GETDATE()) > 1
   ```

2. **清理旧数据**
   ```sql
   -- 删除6个月前已处理的请求
   DELETE FROM UserContactRequests 
   WHERE RequestStatus = 0 
     AND DATEDIFF(MONTH, ContactedTime, GETDATE()) > 6
   ```

3. **性能监控**
   ```sql
   -- 检查索引使用情况
   SELECT * FROM sys.dm_db_index_usage_stats 
   WHERE object_id = OBJECT_ID('UserContactRequests')
   ```

### 备份策略

定期备份 UserContactRequests 表：
```sql
-- 备份到专用表
SELECT * INTO UserContactRequests_Backup_20240101
FROM UserContactRequests
```

## 版本历史

### v1.0 (2024)
- 初始实现
- 基本的请求记录和管理功能
- CSRF 保护
- 用户和管理员界面

## 许可证

本功能是 Comprehensive Recyclable Items Scheduled Recycling System 的一部分。

## 联系方式

如有问题或建议，请联系开发团队。

---

**文档版本:** 1.0  
**最后更新:** 2024年
