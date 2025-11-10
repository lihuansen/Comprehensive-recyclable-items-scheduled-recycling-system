# 管理员联系功能实现文档

## 功能概述

本系统实现了用户与管理员之间的实时聊天功能，用于处理用户反馈和咨询。

## 主要功能

### 用户端功能
1. 从"问题反馈"页面通过"联系我们"链接进入聊天界面
2. 开始新的对话与管理员沟通
3. 查看历史对话记录
4. 发送消息给管理员
5. 结束对话

### 管理员端功能
1. 通过"反馈管理"菜单进入管理界面
2. 查看所有用户的咨询对话列表
3. 筛选对话（全部/进行中/已结束）
4. 查看用户信息和对话历史
5. 回复用户消息
6. 结束对话

## 数据库表结构

### AdminContactMessages 表
存储用户与管理员之间的聊天消息

| 字段名 | 类型 | 说明 |
|--------|------|------|
| MessageID | INT | 消息ID（主键，自增）|
| UserID | INT | 用户ID |
| AdminID | INT | 管理员ID（可选）|
| SenderType | NVARCHAR(20) | 发送者类型：'user', 'admin', 'system' |
| Content | NVARCHAR(2000) | 消息内容 |
| SentTime | DATETIME2 | 发送时间 |
| IsRead | BIT | 是否已读 |

### AdminContactConversations 表
存储用户与管理员之间的会话状态

| 字段名 | 类型 | 说明 |
|--------|------|------|
| ConversationID | INT | 会话ID（主键，自增）|
| UserID | INT | 用户ID |
| AdminID | INT | 管理员ID（可选）|
| StartTime | DATETIME2 | 会话开始时间 |
| UserEndedTime | DATETIME2 | 用户结束时间 |
| AdminEndedTime | DATETIME2 | 管理员结束时间 |
| UserEnded | BIT | 用户是否结束 |
| AdminEnded | BIT | 管理员是否结束 |
| LastMessageTime | DATETIME2 | 最后消息时间 |

## 文件结构

### 数据库脚本
- `Database/CreateAdminContactMessagesTable.sql` - 创建数据库表的SQL脚本

### 模型层 (recycling.Model)
- `AdminContactMessages.cs` - 消息实体模型
- `AdminContactConversations.cs` - 会话实体模型

### 数据访问层 (recycling.DAL)
- `AdminContactDAL.cs` - 数据库操作类
  - 获取/创建会话
  - 发送消息
  - 获取会话列表
  - 获取消息记录
  - 结束会话

### 业务逻辑层 (recycling.BLL)
- `AdminContactBLL.cs` - 业务逻辑类
  - 数据验证
  - 业务规则处理
  - 异常处理

### 控制器层 (recycling.Web.UI/Controllers)

#### HomeController.cs
用户端控制器方法：
- `ContactAdmin()` - 联系管理员页面
- `StartAdminContact()` - 开始会话
- `GetUserAdminConversations()` - 获取用户会话列表
- `GetAdminContactMessages()` - 获取消息记录
- `SendAdminContactMessage()` - 发送消息
- `EndAdminContact()` - 结束会话

#### StaffController.cs
管理员端控制器方法：
- `FeedbackManagement()` - 反馈管理页面
- `GetAllAdminContacts()` - 获取所有会话
- `GetUserInfo()` - 获取用户信息
- `GetAdminContactMessagesForAdmin()` - 获取消息记录
- `SendAdminContactMessageAsAdmin()` - 发送消息
- `EndAdminContactAsAdmin()` - 结束会话

### 视图层 (recycling.Web.UI/Views)
- `Home/Feedback.cshtml` - 更新了"联系我们"链接
- `Home/ContactAdmin.cshtml` - 用户联系管理员界面
- `Staff/FeedbackManagement.cshtml` - 管理员反馈管理界面

## 使用流程

### 用户使用流程
1. 用户登录系统
2. 访问"问题反馈"页面
3. 点击底部"联系我们"链接
4. 进入聊天界面，可以：
   - 点击"开始新对话"按钮创建新会话
   - 点击"查看历史对话"查看过往对话
   - 在聊天框中输入消息并发送
   - 点击"结束对话"按钮结束当前对话

### 管理员使用流程
1. 管理员登录系统
2. 从导航栏选择"反馈管理"
3. 查看左侧用户对话列表
4. 可以使用筛选按钮（全部/进行中/已结束）
5. 点击某个对话查看详情和消息历史
6. 在聊天框中输入回复并发送
7. 完成后点击"结束对话"按钮

## 安全特性

### 已实现的安全措施
1. **身份验证**：所有方法都检查用户登录状态
2. **授权检查**：
   - 用户只能访问自己的对话
   - 管理员需要admin或superadmin角色才能访问反馈管理
3. **输入验证**：
   - 消息内容长度限制（最大2000字符）
   - 参数验证（用户ID、消息内容等）
4. **SQL注入防护**：使用参数化查询
5. **XSS防护**：前端使用escapeHtml函数转义HTML

### 已知限制
1. **CSRF防护**：当前AJAX JSON请求未实现CSRF令牌验证，这与现有代码库的模式保持一致。建议在未来版本中为所有AJAX请求添加CSRF防护。

## 数据库部署

运行以下SQL脚本创建必要的数据库表：

```sql
-- 执行 Database/CreateAdminContactMessagesTable.sql
```

该脚本会创建两个表：
- AdminContactMessages
- AdminContactConversations

## 维护和扩展

### 可能的扩展功能
1. 实时消息推送（使用SignalR）
2. 消息已读状态跟踪
3. 文件/图片发送功能
4. 消息搜索功能
5. 自动回复和智能客服
6. 对话评分和反馈
7. 消息通知（邮件/短信）

### 性能优化建议
1. 对高频访问的查询添加缓存
2. 对大量历史消息进行分页加载
3. 定期归档旧的对话记录
4. 添加消息队列处理大量并发消息

## 测试建议

### 功能测试
1. 测试用户创建新对话
2. 测试发送和接收消息
3. 测试会话结束功能
4. 测试历史对话查看
5. 测试管理员回复功能
6. 测试筛选和搜索功能

### 安全测试
1. 测试未登录用户访问限制
2. 测试用户只能访问自己的对话
3. 测试管理员权限验证
4. 测试SQL注入攻击防护
5. 测试XSS攻击防护

### 性能测试
1. 测试大量消息加载性能
2. 测试并发用户聊天性能
3. 测试数据库查询优化效果

## 故障排除

### 常见问题

**问题1：无法发送消息**
- 检查用户是否已登录
- 检查会话是否已结束
- 检查消息内容是否符合长度限制
- 检查数据库连接

**问题2：看不到历史对话**
- 检查用户是否有对话记录
- 检查数据库查询是否正确
- 检查前端JavaScript是否有错误

**问题3：管理员看不到用户对话**
- 检查管理员角色权限
- 检查是否有用户创建了对话
- 检查数据库中是否有记录

## 技术栈

- **后端**：ASP.NET MVC, C#
- **数据库**：SQL Server
- **前端**：jQuery, Bootstrap, HTML5/CSS3
- **架构模式**：三层架构（Model-BLL-DAL）

## 版本历史

- **v1.0** (2025-11-10)
  - 初始版本
  - 实现基本的用户-管理员聊天功能
  - 实现会话管理和消息存储
  - 实现历史对话查看
