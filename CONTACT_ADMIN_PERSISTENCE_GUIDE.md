# 用户与管理员联系功能 - 持久化聊天记录实现指南

## 功能概述

本系统实现了用户与管理员之间的实时聊天功能，支持**消息持久化**，确保用户重新登录后仍能查看所有历史消息记录。

## 核心特性

### ✅ 已实现的功能

1. **消息持久化**
   - 所有消息（用户消息、管理员消息、系统消息）都保存在数据库中
   - 用户重新登录后可以看到完整的历史对话
   - 系统消息（如欢迎语）也会被保存和显示

2. **会话管理**
   - 智能会话重用：如果管理员未结束会话，用户可以继续之前的对话
   - 会话状态跟踪：记录用户和管理员是否结束对话
   - 最后消息时间跟踪

3. **实时更新**
   - 每3秒自动轮询新消息
   - 管理员回复后用户端自动显示
   - 会话状态实时更新

## 数据库设计

### AdminContactMessages 表

存储所有用户与管理员之间的消息。

```sql
CREATE TABLE [dbo].[AdminContactMessages] (
    [MessageID] INT IDENTITY(1,1) PRIMARY KEY,
    [UserID] INT NOT NULL,                        -- 用户ID
    [AdminID] INT NULL,                           -- 管理员ID（可选）
    [SenderType] NVARCHAR(20) NOT NULL,          -- 'user', 'admin', 'system'
    [Content] NVARCHAR(2000) NOT NULL,           -- 消息内容
    [SentTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 发送时间
    [IsRead] BIT NOT NULL DEFAULT 0,             -- 是否已读
    CONSTRAINT FK_AdminContactMessages_Users FOREIGN KEY ([UserID]) 
        REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
    CONSTRAINT CK_AdminContactMessages_SenderType 
        CHECK ([SenderType] IN ('user', 'admin', 'system'))
);
```

**索引设计**：
- `IX_AdminContactMessages_UserID` - 按用户ID查询
- `IX_AdminContactMessages_AdminID` - 按管理员ID查询
- `IX_AdminContactMessages_SentTime` - 按时间排序

### AdminContactConversations 表

跟踪用户与管理员之间的会话状态。

```sql
CREATE TABLE [dbo].[AdminContactConversations] (
    [ConversationID] INT IDENTITY(1,1) PRIMARY KEY,
    [UserID] INT NOT NULL,                       -- 用户ID
    [AdminID] INT NULL,                          -- 管理员ID（可选）
    [StartTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 会话开始时间
    [UserEndedTime] DATETIME2 NULL,              -- 用户结束时间
    [AdminEndedTime] DATETIME2 NULL,             -- 管理员结束时间
    [UserEnded] BIT NOT NULL DEFAULT 0,          -- 用户是否结束
    [AdminEnded] BIT NOT NULL DEFAULT 0,         -- 管理员是否结束
    [LastMessageTime] DATETIME2 NULL,            -- 最后消息时间
    CONSTRAINT FK_AdminContactConversations_Users FOREIGN KEY ([UserID]) 
        REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
);
```

**索引设计**：
- `IX_AdminContactConversations_UserID` - 按用户ID查询
- `IX_AdminContactConversations_AdminID` - 按管理员ID查询

## 技术实现

### 消息持久化机制

#### 1. 会话获取或创建（DAL层）

```csharp
// AdminContactDAL.cs - GetOrCreateConversation()
// 关键逻辑：检查是否有未结束的会话
string checkSql = @"
    SELECT TOP 1 ConversationID 
    FROM AdminContactConversations 
    WHERE UserID = @UserID 
      AND AdminEnded = 0    -- 只要管理员未结束，就重用会话
    ORDER BY StartTime DESC";
```

**设计理念**：
- 只检查 `AdminEnded = 0`，不检查 `UserEnded`
- 这确保了只要管理员未结束，用户可以继续之前的对话
- 用户重新登录后可以看到所有历史消息

#### 2. 消息加载（DAL层）

```csharp
// AdminContactDAL.cs - GetConversationMessages()
// 关键逻辑：加载用户的所有消息，不按会话过滤
string sql = @"
    SELECT MessageID, UserID, AdminID, SenderType, Content, SentTime, IsRead
    FROM AdminContactMessages
    WHERE UserID = @UserID    -- 只按用户ID过滤
    ORDER BY SentTime ASC";   -- 按时间排序
```

**设计理念**：
- **不按 ConversationID 过滤**，而是按 UserID 过滤
- 这确保了跨会话的消息历史都会显示
- 包含所有类型的消息（user, admin, system）

#### 3. 前端加载流程

```javascript
// ContactAdmin.cshtml - startNewConversation()
// 页面加载时自动执行
function startNewConversation() {
    // 1. 获取或创建会话
    fetch('/Home/StartAdminContact', { method: 'POST' })
    .then(r => r.json())
    .then(data => {
        currentUserId = data.userId;
        // 2. 加载所有历史消息
        loadMessages();
        // 3. 开始轮询新消息
        startPolling();
    });
}
```

### 系统消息处理

系统消息（如欢迎语、结束通知）与普通消息一样被保存在数据库中：

```csharp
// 发送系统消息示例
_adminContactBLL.SendMessage(user.UserID, null, "system", "您好，这里是在线客服");
```

前端显示时居中对齐：

```javascript
// displayMessages() 函数
if (senderType === 'system') {
    messageDiv.innerHTML = '<i class="fas fa-info-circle"></i> ' + 
                          escapeHtml(msg.content);
}
```

CSS样式：

```css
.message-wrapper.system {
    justify-content: center;  /* 居中显示 */
}

.message.system {
    background: #f0f0f0;
    color: #666;
    font-size: 13px;
    padding: 6px 12px;
    border-radius: 15px;
    text-align: center;
}
```

## 使用流程

### 用户端使用

1. **访问入口**：
   - 方式1：从"问题反馈"页面底部点击"联系我们"
   - 方式2：直接访问 `/Home/ContactAdmin`

2. **首次对话**：
   - 页面自动创建新会话
   - 显示系统欢迎消息："您好，这里是在线客服"
   - 用户可以输入消息并发送

3. **重新登录**：
   - 用户再次访问时，如果管理员未结束会话：
     - 自动加载之前的所有消息（包括系统消息）
     - 可以继续之前的对话
   - 如果管理员已结束会话：
     - 创建新会话
     - 之前的消息仍然可以通过"查看历史对话"查看

4. **历史对话**：
   - 点击"查看历史对话"按钮
   - 显示所有会话列表
   - 点击任意会话查看完整消息记录

### 管理员端使用

管理员可以通过 `/Staff/FeedbackManagement` 访问：

1. 查看所有用户的对话列表
2. 筛选对话（全部/进行中/已结束）
3. 回复用户消息
4. 结束对话

## 安全特性

### 已实现的安全措施

1. **身份验证**：
   - 所有控制器方法检查 `Session["LoginUser"]`
   - 未登录用户无法访问

2. **授权检查**：
   ```csharp
   // 确保用户只能查看自己的消息
   if (user.UserID != userId)
       return Json(new { success = false, message = "无权查看该对话" });
   ```

3. **输入验证**：
   - 消息内容长度限制：最大2000字符
   - SenderType 限制：只能是 'user', 'admin', 'system'
   - 参数验证（UserID > 0, Content 非空）

4. **SQL注入防护**：
   - 使用参数化查询：`cmd.Parameters.AddWithValue("@UserID", userId)`

5. **XSS防护**：
   ```javascript
   // 前端 HTML 转义
   function escapeHtml(str) {
       return String(str).replace(/&/g, '&amp;')
                         .replace(/</g, '&lt;')
                         .replace(/>/g, '&gt;');
   }
   ```

## 数据库部署

运行以下SQL脚本创建数据库表：

```bash
sqlcmd -S your_server -d RecyclingDB -i Database/CreateAdminContactMessagesTable.sql
```

或者在SQL Server Management Studio中执行：
`Database/CreateAdminContactMessagesTable.sql`

## 测试场景

### 测试1：消息持久化

1. 用户A登录系统
2. 访问"问题反馈" → 点击"联系我们"
3. 发送消息："你好，我有个问题"
4. **退出登录**
5. 重新登录（用户A）
6. 再次访问"联系我们"
7. **预期结果**：之前的消息"你好，我有个问题"仍然显示

### 测试2：系统消息持久化

1. 用户B登录系统
2. 访问"联系我们"（首次）
3. 看到系统消息："您好，这里是在线客服"
4. 退出登录
5. 重新登录（用户B）
6. 再次访问"联系我们"
7. **预期结果**：系统消息仍然显示在聊天记录中

### 测试3：管理员回复持久化

1. 用户C发送消息："需要帮助"
2. 管理员登录，回复："请问有什么可以帮您的？"
3. 用户C退出登录
4. 重新登录（用户C）
5. 访问"联系我们"
6. **预期结果**：用户和管理员的对话都完整显示

### 测试4：跨会话消息保留

1. 用户D发送消息："第一个问题"
2. 管理员回复后结束会话
3. 用户D再次访问，发送："第二个问题"（新会话）
4. 点击"查看历史对话"
5. **预期结果**：可以看到两个会话及其完整消息

## 故障排查

### 问题1：消息不持久化

**症状**：用户重新登录后看不到历史消息

**可能原因**：
1. 数据库表未创建
2. 数据库连接字符串错误

**解决方案**：
```sql
-- 检查表是否存在
SELECT * FROM sys.tables WHERE name = 'AdminContactMessages';
SELECT * FROM sys.tables WHERE name = 'AdminContactConversations';

-- 检查是否有数据
SELECT COUNT(*) FROM AdminContactMessages;
```

### 问题2：系统消息不显示

**症状**：系统消息没有居中显示或根本不显示

**可能原因**：
1. SenderType 大小写问题
2. CSS未加载

**解决方案**：
```javascript
// 确保 SenderType 转换为小写
const senderType = (msg.senderType || msg.SenderType || '').toLowerCase();
```

### 问题3：会话未重用

**症状**：用户每次访问都创建新会话

**可能原因**：
- 管理员自动结束了所有会话

**检查方法**：
```sql
-- 查看用户的会话状态
SELECT * FROM AdminContactConversations 
WHERE UserID = 123 
ORDER BY StartTime DESC;
```

## 性能优化建议

1. **索引优化**：
   - 确保 UserID 和 SentTime 都有索引
   - 定期重建索引以保持性能

2. **消息分页**（未来改进）：
   - 当消息数量很大时，考虑实现分页加载
   - 例如：一次只加载最近100条消息

3. **轮询优化**：
   - 当前3秒轮询频率适中
   - 可以考虑使用 SignalR 实现真正的实时推送

4. **缓存策略**：
   - 可以考虑缓存最近的会话信息
   - 减少数据库查询次数

## 总结

本系统的用户-管理员联系功能已经完整实现了消息持久化：

✅ 用户重新登录后可以看到所有历史消息  
✅ 系统消息被保存并正确显示  
✅ 使用独立的数据库表（避免与用户-回收员聊天混淆）  
✅ 实时更新和轮询机制  
✅ 完善的安全措施  

唯一需要的修改是将"问题反馈"页面的"联系我们"链接指向正确的功能页面，这已经完成。
