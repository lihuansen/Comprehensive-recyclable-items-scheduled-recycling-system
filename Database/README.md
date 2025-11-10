# 数据库脚本说明

本目录包含数据库相关的SQL脚本文件。

## 快速开始（必需表）

如果您遇到以下错误：
- "对象名 'UserFeedback' 无效"
- "对象名 'AdminContactConversations' 无效"

请执行快速设置脚本来创建必需的表：

**Windows 批处理方式：**
```batch
SetupRequiredTables.bat
```

**PowerShell 方式：**
```powershell
.\SetupRequiredTables.ps1
```

或参考 [DATABASE_SETUP_INSTRUCTIONS.md](DATABASE_SETUP_INSTRUCTIONS.md) 获取详细的数据库设置说明。

## 脚本文件列表

### CreateUserFeedbackTable.sql
**用途：** 创建用户反馈表（UserFeedback）

**功能说明：**
- 创建 UserFeedback 表用于存储用户提交的问题反馈、功能建议、投诉举报等信息
- 支持反馈类型分类（问题反馈、功能建议、投诉举报、其他）
- 支持状态跟踪（待处理、处理中、已完成、已关闭）
- 支持管理员回复
- 自动创建必要的外键约束和索引
- 包含智能检测：如果表已存在则跳过创建

**使用方法：**
```sql
sqlcmd -S localhost -d RecyclingDB -i CreateUserFeedbackTable.sql
```

**表结构：**
- `FeedbackID`: 反馈ID（主键，自增）
- `UserID`: 用户ID（外键，关联到 Users.UserID）
- `FeedbackType`: 反馈类型（NVARCHAR(50)，'问题反馈'/'功能建议'/'投诉举报'/'其他'）
- `Subject`: 反馈主题（NVARCHAR(200)，必填）
- `Description`: 详细描述（NVARCHAR(2000)，必填）
- `ContactEmail`: 联系邮箱（NVARCHAR(100)，可选）
- `Status`: 处理状态（NVARCHAR(50)，默认'待处理'）
- `AdminReply`: 管理员回复（NVARCHAR(1000)，可选）
- `CreatedDate`: 创建时间（DATETIME2，默认当前时间）
- `UpdatedDate`: 更新时间（DATETIME2，可空）

### CreateAdminContactMessagesTable.sql
**用途：** 创建管理员联系功能相关表（AdminContactConversations 和 AdminContactMessages）

**功能说明：**
- 创建 AdminContactConversations 表用于跟踪用户与管理员之间的会话状态
- 创建 AdminContactMessages 表用于存储用户和管理员之间的聊天消息
- 支持会话状态管理（进行中、已结束）
- 支持消息类型区分（用户消息、管理员消息、系统消息）
- 自动创建必要的外键约束和索引

**使用方法：**
```sql
sqlcmd -S localhost -d RecyclingDB -i CreateAdminContactMessagesTable.sql
```

**表结构：**

**AdminContactConversations:**
- `ConversationID`: 会话ID（主键，自增）
- `UserID`: 用户ID（外键）
- `AdminID`: 管理员ID（外键，可空）
- `StartTime`: 开始时间（DATETIME2）
- `UserEndedTime`: 用户结束时间（DATETIME2，可空）
- `AdminEndedTime`: 管理员结束时间（DATETIME2，可空）
- `UserEnded`: 用户是否结束（BIT）
- `AdminEnded`: 管理员是否结束（BIT）
- `LastMessageTime`: 最后消息时间（DATETIME2，可空）

**AdminContactMessages:**
- `MessageID`: 消息ID（主键，自增）
- `UserID`: 用户ID（外键）
- `AdminID`: 管理员ID（外键，可空）
- `SenderType`: 发送者类型（'user'/'admin'/'system'）
- `Content`: 消息内容（NVARCHAR(2000)）
- `SentTime`: 发送时间（DATETIME2）
- `IsRead`: 是否已读（BIT）

### CreateOrderReviewsTable.sql
**用途：** 创建订单评价表（OrderReviews）

**功能说明：**
- 创建 OrderReviews 表用于存储用户对回收员的评价
- 支持1-5星评分
- 支持中文评价内容（使用 NVARCHAR）
- 自动创建必要的外键约束和索引
- 包含智能检测：如果表已存在则跳过创建

**使用方法：**
1. 打开 SQL Server Management Studio (SSMS)
2. 连接到数据库 `RecyclingSystemDB`
3. 打开此脚本文件
4. 执行脚本（F5 或点击"执行"按钮）

**表结构：**
- `ReviewID`: 评价ID（主键，自增）
- `OrderID`: 订单ID（外键，关联到 Appointments.AppointmentID）
- `UserID`: 用户ID（外键，关联到 Users.UserID）
- `RecyclerID`: 回收员ID（外键，关联到 Recyclers.RecyclerID）
- `StarRating`: 星级评分（1-5，有检查约束）
- `ReviewText`: 评价内容（NVARCHAR(500)，可为空，支持中文）
- `CreatedDate`: 创建时间（DATETIME2，默认当前时间）

**索引：**
- `IX_OrderReviews_OrderID`: 按订单ID查询
- `IX_OrderReviews_UserID`: 按用户ID查询
- `IX_OrderReviews_RecyclerID`: 按回收员ID查询
- `IX_OrderReviews_CreatedDate`: 按创建时间查询

### CreateInventoryTable.sql
**用途：** 创建库存表（Inventory）

**功能说明：**
- 创建 Inventory 表用于存储回收员的库存管理信息
- 包含品类、重量、价格等关键信息
- 自动创建必要的外键约束和索引
- 包含智能检测：如果表已存在则跳过创建

**使用方法：**
1. 打开 SQL Server Management Studio (SSMS)
2. 连接到数据库 `RecyclingSystemDB`
3. 打开此脚本文件
4. 执行脚本（F5 或点击"执行"按钮）

**表结构：**
- `InventoryID`: 库存ID（主键，自增）
- `OrderID`: 订单ID（外键，关联到 Appointments.AppointmentID）
- `CategoryKey`: 品类键名（NVARCHAR(50)，如 "glass", "metal"）
- `CategoryName`: 品类名称（NVARCHAR(50)，如 "玻璃", "金属"）
- `Weight`: 重量（DECIMAL(10,2)，单位：kg，必须大于0）
- `Price`: 回收价格（DECIMAL(10,2)，可为空，必须非负）
- `RecyclerID`: 回收员ID（外键，关联到 Recyclers.RecyclerID）
- `CreatedDate`: 创建时间（DATETIME2，默认当前时间）

**索引：**
- `IX_Inventory_OrderID`: 按订单ID查询
- `IX_Inventory_RecyclerID`: 按回收员ID查询
- `IX_Inventory_CategoryKey`: 按品类键名查询
- `IX_Inventory_CreatedDate`: 按创建时间查询

**约束：**
- Weight 必须大于0
- Price 如果不为 NULL，必须大于等于0

### AddInventoryPriceColumn.sql
**用途：** 为库存表（Inventory）添加价格列（如果表已创建但缺少Price列）

**功能说明：**
- 为 Inventory 表添加 Price 列用于存储回收物品的价格
- Price 列为可空的 DECIMAL(10,2) 类型
- 包含智能检测：如果列已存在则跳过添加
- 显示更新后的表结构

**使用方法：**
1. 打开 SQL Server Management Studio (SSMS)
2. 连接到数据库 `RecyclingSystemDB`
3. 打开此脚本文件
4. 执行脚本（F5 或点击"执行"按钮）

**列说明：**
- `Price`: 回收价格（DECIMAL(10,2)，可为空）
  - 用于记录回收员回收该物品的价格
  - 从订单的 EstimatedPrice 按重量比例分配

**注意：** 如果使用 CreateInventoryTable.sql 创建表，则无需执行此脚本，因为表已包含 Price 列

## 注意事项

1. 执行脚本前请确保已连接到正确的数据库
2. 脚本会自动检查表是否存在，可以安全地重复执行
3. 所有文本字段使用 NVARCHAR 类型以支持中文字符
4. 外键约束确保数据完整性

## 执行顺序建议

如果是全新安装，建议按以下顺序执行脚本：
1. `CreateInventoryTable.sql` - 创建库存表（包含 Price 列）
2. `CreateOrderReviewsTable.sql` - 创建订单评价表

如果表已存在但缺少某些列：
- 执行 `AddInventoryPriceColumn.sql` - 为已存在的 Inventory 表添加 Price 列

## Entity Framework 集成

本系统使用 Entity Framework 进行数据访问：
- 模型定义位于：`recycling.Model/Inventory.cs`
- 数据访问层位于：`recycling.DAL/InventoryDAL.cs`
- 业务逻辑层位于：`recycling.BLL/InventoryBLL.cs`

数据库表结构必须与 Entity Framework 模型保持一致。
