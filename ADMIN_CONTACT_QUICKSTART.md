# 管理员联系功能 - 快速开始指南

## 🚀 快速部署

### 1. 数据库部署（必需）

在SQL Server中执行以下脚本创建必要的表：

```sql
-- 运行此脚本文件
Database/CreateAdminContactMessagesTable.sql
```

该脚本会创建：
- `AdminContactMessages` - 存储聊天消息
- `AdminContactConversations` - 存储会话状态

### 2. 构建项目

在Visual Studio中：
1. 打开解决方案 `全品类可回收物预约回收系统（解决方案）.sln`
2. 构建解决方案 (Ctrl+Shift+B)
3. 确保没有编译错误

### 3. 运行项目

1. 设置 `recycling.Web.UI` 为启动项目
2. 按 F5 运行项目
3. 浏览器会自动打开首页

---

## 👥 用户端使用

### 访问路径
1. 用户登录系统
2. 导航到 **问题反馈** 页面 (`/Home/Feedback`)
3. 点击页面底部的 **"联系我们"** 链接
4. 进入 **联系管理员** 界面 (`/Home/ContactAdmin`)

### 功能操作

#### 开始新对话
```
点击左侧 "开始新对话" 按钮 → 自动创建会话 → 开始聊天
```

#### 发送消息
```
在底部输入框输入消息 → 点击"发送"按钮 或 按Enter键
```

#### 查看历史对话
```
点击左侧 "查看历史对话" 按钮 → 显示所有历史会话列表 → 点击某个会话查看详情
```

#### 结束对话
```
点击底部 "结束对话" 按钮 → 确认 → 对话结束
```

---

## 👨‍💼 管理员端使用

### 访问路径
1. 管理员登录系统
2. 从顶部导航栏选择 **"反馈管理"** (`/Staff/FeedbackManagement`)

### 功能操作

#### 查看用户对话
```
左侧显示所有用户对话列表 → 点击某个对话 → 右侧显示聊天内容
```

#### 筛选对话
```
使用顶部筛选按钮：
- 全部：显示所有对话
- 进行中：只显示未结束的对话
- 已结束：只显示已完成的对话
```

#### 回复用户
```
选择一个对话 → 在底部输入框输入回复 → 点击"发送"
```

#### 查看用户信息
```
选择对话后，顶部会显示：
- 用户名
- 手机号
- 邮箱地址
```

#### 结束对话
```
点击底部 "结束对话" 按钮 → 确认 → 标记为已完成
```

---

## 🎨 界面预览

### 用户端 - 联系管理员界面
- 左侧：会话列表和历史记录按钮
- 右侧：聊天消息区域
- 底部：消息输入框和操作按钮

### 管理员端 - 反馈管理界面
- 顶部：筛选按钮（全部/进行中/已结束）
- 左侧：用户对话列表
- 中间：用户信息栏
- 右侧：聊天消息区域
- 底部：消息输入框和操作按钮

---

## 🔧 配置说明

### 数据库连接

确保 `Web.config` 中的连接字符串正确：

```xml
<connectionStrings>
  <add name="RecyclingDB" 
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=RecyclingDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Session 超时

默认Session超时时间为30分钟，可在 `Web.config` 中修改：

```xml
<system.web>
  <sessionState timeout="30" />
</system.web>
```

---

## 🐛 常见问题

### Q1: 看不到"联系我们"链接？
**A:** 确保您已登录用户账户，并且访问的是 `/Home/Feedback` 页面。

### Q2: 无法发送消息？
**A:** 检查以下几点：
- 是否已登录
- 是否已创建会话（点击"开始新对话"）
- 消息内容不为空且不超过2000字符
- 网络连接是否正常

### Q3: 管理员看不到用户对话？
**A:** 检查：
- 是否以管理员身份登录（admin 或 superadmin）
- 是否有用户创建了对话
- 数据库中是否有记录

### Q4: 数据库表创建失败？
**A:** 
- 确保有足够的数据库权限
- 检查SQL Server版本（需要支持DATETIME2）
- 查看SQL错误消息

---

## 📊 数据流程图

```
用户端流程:
用户登录 → 访问反馈页面 → 点击"联系我们" → 
进入聊天界面 → 开始新对话 → 发送消息 → 
等待管理员回复 → 结束对话

管理员端流程:
管理员登录 → 访问"反馈管理" → 查看对话列表 → 
选择某个对话 → 查看用户信息和消息历史 → 
回复用户 → 结束对话
```

---

## 🔐 权限说明

### 用户权限
- ✅ 创建自己的对话
- ✅ 发送消息
- ✅ 查看自己的历史对话
- ✅ 结束自己的对话
- ❌ 查看其他用户的对话
- ❌ 访问管理员后台

### 管理员权限
- ✅ 查看所有用户对话
- ✅ 回复任何用户
- ✅ 查看用户信息
- ✅ 结束任何对话
- ✅ 筛选和管理对话

### 回收员权限
- ❌ 无法访问反馈管理功能
- ℹ️ 如需访问，可升级为管理员角色

---

## 📝 技术架构

```
表示层 (Views)
    ↓
控制器层 (Controllers)
    ↓
业务逻辑层 (BLL)
    ↓
数据访问层 (DAL)
    ↓
数据库 (SQL Server)
```

### 主要文件
- **Views**: `ContactAdmin.cshtml`, `FeedbackManagement.cshtml`
- **Controllers**: `HomeController.cs`, `StaffController.cs`
- **BLL**: `AdminContactBLL.cs`
- **DAL**: `AdminContactDAL.cs`
- **Models**: `AdminContactMessages.cs`, `AdminContactConversations.cs`

---

## 📚 相关文档

- [完整实现文档](./ADMIN_CONTACT_IMPLEMENTATION.md)
- [安全分析报告](./SECURITY_SUMMARY_ADMIN_CONTACT.md)
- [主项目README](./README.md)
- [开发指南](./DEVELOPMENT_GUIDE.md)

---

## ⚡ 快速测试

### 测试用户端
1. 注册/登录一个用户账户
2. 访问 `/Home/Feedback`
3. 点击"联系我们"
4. 发送测试消息："您好，我需要帮助"

### 测试管理员端
1. 使用管理员账户登录
2. 访问 `/Staff/FeedbackManagement`
3. 查看刚才的测试消息
4. 回复："您好，我能帮您什么？"

---

## 🆘 获取帮助

如果遇到问题：
1. 查看 [常见问题](#-常见问题) 部分
2. 检查 [故障排除](./ADMIN_CONTACT_IMPLEMENTATION.md#故障排除) 章节
3. 查看浏览器控制台错误信息
4. 检查数据库连接和表结构

---

## ✨ 特色功能

- ✅ 实时聊天体验
- ✅ 历史对话记录
- ✅ 会话状态管理
- ✅ 用户信息展示
- ✅ 对话筛选功能
- ✅ 美观的UI设计
- ✅ 响应式布局
- ✅ 键盘快捷键支持（Enter发送）

---

**祝使用愉快！** 🎉
