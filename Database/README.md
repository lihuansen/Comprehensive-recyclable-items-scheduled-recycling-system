# 数据库脚本说明

本目录包含数据库相关的SQL脚本文件。

## 脚本文件列表

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

### AddInventoryPriceColumn.sql
**用途：** 为库存表（Inventory）添加价格列

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

## 注意事项

1. 执行脚本前请确保已连接到正确的数据库
2. 脚本会自动检查表是否存在，可以安全地重复执行
3. 所有文本字段使用 NVARCHAR 类型以支持中文字符
4. 外键约束确保数据完整性
