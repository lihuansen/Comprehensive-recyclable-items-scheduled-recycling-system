# 用户与管理员实时聊天功能实现文档

## 概述

本文档描述了用户端与管理员端之间实时聊天功能的实现细节。该功能允许用户通过"联系我们"与管理员进行实时对话，只有管理员有权结束对话。

## 核心特性

### 1. 实时消息更新
- **用户端**: 每3秒自动轮询检查新消息
- **管理员端**: 每3秒自动轮询检查新消息
- **会话列表**: 每30秒自动刷新会话列表

### 2. 会话控制机制
- **只有管理员可以结束对话**: 用户端已移除"结束对话"按钮
- **会话持久性**: 用户多次点击"联系我们"会自动重用未被管理员结束的会话
- **状态指示**: 实时显示会话状态（进行中/已结束）

### 3. 智能消息显示
- **自动滚动**: 新消息到达时自动滚动到底部
- **位置保持**: 用户查看历史消息时，轮询更新不会影响当前滚动位置
- **静默更新**: 轮询时只在消息数量变化时才更新界面

## 技术实现

### 前端实现

#### 用户端 (ContactAdmin.cshtml)

**关键变量:**
```javascript
let currentConversationId = null;      // 当前会话ID
let currentUserId = null;              // 当前用户ID
let isHistoryMode = false;             // 是否在查看历史
let pollingInterval = null;            // 轮询定时器
let isConversationEnded = false;       // 会话是否已结束
let lastMessageCount = 0;              // 上次消息数量
```

**核心函数:**

1. **startPolling()**: 启动轮询机制
   - 每3秒执行一次
   - 调用 `loadMessages(true)` 静默加载消息
   - 调用 `checkConversationStatus()` 检查会话状态

2. **checkConversationStatus()**: 检查会话状态
   - 获取最新会话信息
   - 根据 `adminEnded` 状态更新界面
   - 如果管理员已结束，停止轮询并禁用输入

3. **loadMessages(silent)**: 加载消息
   - silent=true: 静默加载，只在消息数变化时更新
   - silent=false: 强制更新并滚动到底部

4. **updateStatusDisplay(status, color)**: 更新状态显示
   - 在输入框下方显示当前会话状态

#### 管理员端 (UserContactManagement.cshtml)

**关键变量:**
```javascript
let currentUserId = null;              // 当前选中的用户ID
let currentConversationId = null;      // 当前会话ID
let allConversations = [];             // 所有会话列表
let currentFilter = 'all';             // 当前筛选条件
let pollingInterval = null;            // 轮询定时器
let lastMessageCount = 0;              // 上次消息数量
```

**核心函数:**

1. **selectConversation(userId, conversationId)**: 选择会话
   - 加载用户信息
   - 加载消息记录
   - 检查会话状态
   - 启动轮询

2. **startPolling()**: 启动轮询
   - 每3秒调用 `loadMessages(userId, true)` 静默更新

3. **filterConversations(filter)**: 筛选会话
   - 'all': 显示所有会话
   - 'active': 只显示 adminEnded=false 的会话
   - 'ended': 只显示 adminEnded=true 的会话

4. **endConversation()**: 结束会话
   - 管理员专用功能
   - 结束后停止轮询
   - 刷新会话列表

### 后端实现

#### 数据访问层 (AdminContactDAL.cs)

**GetOrCreateConversation 改进:**
```csharp
// 只检查 AdminEnded 状态
WHERE UserID = @UserID AND AdminEnded = 0
```
- 确保只要管理员未结束，用户就能重用同一会话

**SendMessage 改进:**
```csharp
// 检查会话是否被管理员结束
SELECT AdminEnded FROM AdminContactConversations 
WHERE UserID = @UserID AND AdminEnded = 0
```
- 如果管理员已结束会话，拒绝发送消息

**IsBothEnded 改进:**
```csharp
// 改名为更准确的语义，只检查 AdminEnded
SELECT TOP 1 AdminEnded FROM AdminContactConversations
WHERE UserID = @UserID
```

#### 业务逻辑层 (AdminContactBLL.cs)

**SendMessage 改进:**
- 当会话已结束时返回: "对话已结束，无法发送消息"
- 提供更友好的错误提示

## 数据库表结构

### AdminContactConversations (会话表)
```sql
ConversationID INT PRIMARY KEY       -- 会话ID
UserID INT NOT NULL                  -- 用户ID
AdminID INT NULL                     -- 管理员ID
StartTime DATETIME2                  -- 开始时间
UserEndedTime DATETIME2 NULL         -- 用户结束时间（已废弃）
AdminEndedTime DATETIME2 NULL        -- 管理员结束时间
UserEnded BIT NOT NULL               -- 用户是否结束（已废弃）
AdminEnded BIT NOT NULL              -- 管理员是否结束（决定性状态）
LastMessageTime DATETIME2 NULL       -- 最后消息时间
```

**关键字段说明:**
- `AdminEnded`: **唯一的决定性状态**，只有此字段为0时会话才是活跃的
- `UserEnded`: 保留字段，不再用于判断会话状态
- `AdminEndedTime`: 管理员结束对话的时间

### AdminContactMessages (消息表)
```sql
MessageID INT PRIMARY KEY            -- 消息ID
UserID INT NOT NULL                  -- 用户ID
AdminID INT NULL                     -- 管理员ID
SenderType NVARCHAR(20)              -- 'user', 'admin', 'system'
Content NVARCHAR(2000)               -- 消息内容
SentTime DATETIME2                   -- 发送时间
IsRead BIT                           -- 是否已读
```

## 工作流程

### 用户发起对话流程

```
1. 用户点击"联系我们"
   ↓
2. 前端调用 StartAdminContact API
   ↓
3. 后端调用 GetOrCreateConversation
   ├─ 检查是否有 AdminEnded=0 的会话
   ├─ 有 → 返回现有会话ID
   └─ 无 → 创建新会话
   ↓
4. 如果是新会话，发送系统欢迎消息
   ↓
5. 启动轮询机制（每3秒）
   ↓
6. 实时显示消息和状态
```

### 管理员管理对话流程

```
1. 管理员打开"用户联系管理"
   ↓
2. 加载所有会话列表（可筛选）
   ↓
3. 选择一个会话
   ├─ 加载用户信息
   ├─ 加载消息记录
   └─ 启动轮询（每3秒）
   ↓
4. 管理员可以：
   ├─ 发送消息
   └─ 结束对话（点击"结束对话"按钮）
   ↓
5. 结束对话后：
   ├─ 设置 AdminEnded=1
   ├─ 停止轮询
   ├─ 禁用输入框
   └─ 发送系统消息"管理员已结束对话"
```

### 消息发送流程

```
用户/管理员输入消息并点击发送
   ↓
前端调用 SendAdminContactMessage API
   ↓
后端验证：
   ├─ 检查会话是否存在
   ├─ 检查 AdminEnded 是否为 0
   └─ 检查内容是否有效
   ↓
验证通过：
   ├─ 插入消息到 AdminContactMessages
   ├─ 更新 LastMessageTime
   └─ 返回成功
   ↓
验证失败：
   └─ 返回错误"对话已结束，无法发送消息"
   ↓
前端接收响应：
   ├─ 成功 → 清空输入框，刷新消息列表
   └─ 失败 → 显示错误提示
```

## API 接口说明

### 用户端 API (HomeController)

1. **POST /Home/StartAdminContact**
   - 功能: 开始或恢复对话
   - 返回: `{ success, conversationId, userId }`

2. **POST /Home/GetAdminContactMessages**
   - 参数: `{ userId, beforeTime? }`
   - 返回: `{ success, messages }`

3. **POST /Home/SendAdminContactMessage**
   - 参数: `{ userId, content }`
   - 返回: `{ success, message }`

4. **POST /Home/GetUserAdminConversations**
   - 返回: `{ success, conversations }`

### 管理员端 API (StaffController)

1. **POST /Staff/GetAllAdminContacts**
   - 返回: `{ success, conversations }`

2. **POST /Staff/GetUserInfo**
   - 参数: `{ userId }`
   - 返回: `{ success, user }`

3. **POST /Staff/GetAdminContactMessagesForAdmin**
   - 参数: `{ userId, beforeTime? }`
   - 返回: `{ success, messages }`

4. **POST /Staff/SendAdminContactMessageAsAdmin**
   - 参数: `{ userId, content }`
   - 返回: `{ success, message }`

5. **POST /Staff/EndAdminContactAsAdmin**
   - 参数: `{ userId }`
   - 返回: `{ success, message }`

## 测试场景

### 场景1: 用户首次联系管理员
1. 用户登录系统
2. 进入"问题反馈"页面
3. 点击"联系我们"链接
4. 应该看到欢迎消息："您好，这里是在线客服"
5. 状态显示为"进行中"（绿色）
6. 可以正常输入和发送消息

### 场景2: 用户多次打开联系我们
1. 用户在会话未结束时关闭页面
2. 再次点击"联系我们"
3. 应该看到之前的聊天记录
4. 可以继续对话
5. 不会创建新会话

### 场景3: 管理员回复用户
1. 管理员登录系统
2. 进入"用户联系管理"
3. 在列表中看到用户的会话（标记为"进行中"）
4. 点击该会话
5. 看到完整的对话历史
6. 可以发送回复消息
7. 用户端自动（3秒内）收到管理员回复

### 场景4: 实时消息更新
1. 用户和管理员同时打开对话界面
2. 一方发送消息
3. 另一方在3秒内自动看到新消息
4. 无需手动刷新页面

### 场景5: 管理员结束对话
1. 管理员在对话界面点击"结束对话"
2. 确认结束
3. 管理员端界面：
   - 输入框被禁用
   - 看到系统消息"管理员已结束对话"
   - 会话列表中该会话标记为"已结束"
4. 用户端界面（3秒内）：
   - 自动显示状态为"已结束"（红色）
   - 输入框被禁用
   - 看到系统消息"管理员已结束对话"
   - 无法继续发送消息

### 场景6: 对话结束后用户再次联系
1. 管理员结束对话后
2. 用户关闭页面
3. 用户再次点击"联系我们"
4. 系统创建新的会话
5. 发送新的欢迎消息
6. 可以开始新的对话

### 场景7: 会话筛选
1. 管理员在"用户联系管理"页面
2. 点击"进行中"筛选按钮
3. 只显示 AdminEnded=false 的会话
4. 点击"已结束"筛选按钮
5. 只显示 AdminEnded=true 的会话
6. 点击"全部"显示所有会话

### 场景8: 错误处理
1. 用户尝试在对话结束后发送消息
2. 看到提示："对话已结束，无法发送消息"
3. 管理员尝试对已结束的对话再次结束
4. 看到提示："没有活动的会话需要结束"

## 性能优化

### 轮询优化
- **智能更新**: 只在消息数量变化时才更新DOM
- **位置保持**: 用户浏览历史消息时不会被自动滚动打扰
- **间隔设置**: 3秒是平衡实时性和服务器负载的合理值

### 网络优化
- **最小数据传输**: 只传输必要的字段
- **条件更新**: 使用消息计数快速判断是否需要更新
- **批量处理**: 一次请求获取所有消息

### 用户体验优化
- **即时反馈**: 发送消息后立即在界面显示
- **状态提示**: 清晰的视觉反馈（颜色、图标）
- **平滑滚动**: 新消息到达时自然滚动

## 安全性

### 已实施的安全措施

1. **身份验证**
   - 所有API都检查Session登录状态
   - 用户只能访问自己的会话
   - 管理员需要admin或superadmin角色

2. **权限控制**
   - 用户只能发送属于自己的消息
   - 用户只能查看自己的对话
   - 只有管理员可以结束对话

3. **输入验证**
   - 消息内容最大2000字符
   - 不允许空消息
   - HTML转义防止XSS攻击

4. **SQL注入防护**
   - 使用参数化查询
   - 所有数据库操作都使用SqlParameter

5. **事务保护**
   - 消息发送使用事务确保数据一致性
   - 失败时自动回滚

## 维护说明

### 调整轮询间隔

如需修改轮询间隔，在相应的JavaScript代码中修改：

**用户端 (ContactAdmin.cshtml):**
```javascript
// 修改此处的3000（毫秒）
pollingInterval = setInterval(function() {
    // ...
}, 3000);
```

**管理员端 (UserContactManagement.cshtml):**
```javascript
// 修改此处的3000（毫秒）
pollingInterval = setInterval(function() {
    // ...
}, 3000);

// 会话列表刷新间隔
setInterval(function() {
    loadConversations();
}, 30000);  // 修改此处的30000（毫秒）
```

### 添加新的发送者类型

如需添加新的发送者类型（当前支持：user, admin, system）:

1. 更新数据库约束：
```sql
ALTER TABLE AdminContactMessages DROP CONSTRAINT CK_AdminContactMessages_SenderType;
ALTER TABLE AdminContactMessages ADD CONSTRAINT CK_AdminContactMessages_SenderType 
    CHECK ([SenderType] IN ('user', 'admin', 'system', 'new_type'));
```

2. 更新 BLL 验证：
```csharp
// AdminContactBLL.cs
if (!new[] { "user", "admin", "system", "new_type" }.Contains(senderType.ToLower()))
```

3. 更新前端显示逻辑

### 数据库维护

**清理旧会话:**
```sql
-- 删除6个月前已结束的会话及其消息
DELETE FROM AdminContactMessages 
WHERE UserID IN (
    SELECT UserID FROM AdminContactConversations 
    WHERE AdminEnded = 1 AND AdminEndedTime < DATEADD(MONTH, -6, GETDATE())
);

DELETE FROM AdminContactConversations 
WHERE AdminEnded = 1 AND AdminEndedTime < DATEADD(MONTH, -6, GETDATE());
```

**统计查询:**
```sql
-- 活跃会话数量
SELECT COUNT(*) as ActiveConversations 
FROM AdminContactConversations 
WHERE AdminEnded = 0;

-- 今日新会话
SELECT COUNT(*) as TodayConversations 
FROM AdminContactConversations 
WHERE CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE);

-- 平均响应时间
SELECT AVG(DATEDIFF(MINUTE, c.StartTime, m.SentTime)) as AvgResponseMinutes
FROM AdminContactConversations c
INNER JOIN AdminContactMessages m ON c.UserID = m.UserID
WHERE m.SenderType = 'admin' AND m.SentTime > c.StartTime;
```

## 已知限制

1. **不支持多标签页同步**: 同一用户在多个浏览器标签页打开对话，各自独立轮询
2. **轮询延迟**: 最多3秒的消息接收延迟
3. **历史消息加载**: 一次性加载所有历史消息，大量消息可能影响性能
4. **离线消息**: 用户离线时发送的消息，需要再次打开页面才能看到

## 未来改进建议

1. **WebSocket支持**: 替代轮询机制，实现真正的实时推送
2. **消息分页**: 支持大量历史消息的分页加载
3. **已读状态**: 实现消息的已读/未读状态跟踪
4. **文件传输**: 支持发送图片和文件
5. **打字指示器**: 显示对方正在输入的状态
6. **通知提醒**: 新消息到达时的浏览器通知
7. **会话转接**: 支持将会话转给其他管理员
8. **满意度评价**: 对话结束后的用户评价功能

## 总结

本实现方案通过JavaScript轮询机制实现了准实时的用户-管理员聊天功能。虽然不是真正的WebSocket实时推送，但对于中小规模应用来说，3秒的轮询间隔提供了良好的用户体验，同时保持了技术实现的简单性。系统通过只允许管理员结束对话的机制，确保了对话的连续性和可控性。
