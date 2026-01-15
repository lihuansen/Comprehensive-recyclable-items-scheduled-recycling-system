# 基地工作人员消息中心实现文档

## 功能概述

本次实现为基地工作人员端添加了完整的消息中心功能，用于接收和管理运输管理和仓库管理相关的通知消息。

## 主要功能

### 1. 消息类型

系统支持以下四种消息类型：

- **运输单创建通知** (`TransportOrderCreated`)
  - 当回收员联系运输人员并创建运输单后自动发送
  - 包含运输单号、回收员姓名、取货地址、预估重量等信息

- **运输单完成通知** (`TransportOrderCompleted`)
  - 当运输人员完成运输单后自动发送
  - 包含运输单号、运输人员姓名、实际重量等信息
  - 提醒基地工作人员及时创建入库单

- **入库单创建通知** (`WarehouseReceiptCreated`)
  - 当基地工作人员创建入库单后自动发送给其他工作人员
  - 包含入库单号、关联运输单号、总重量等信息
  - 提醒其他工作人员关注入库进度

- **仓库库存写入通知** (`WarehouseInventoryWritten`)
  - 当入库单成功创建并将库存写入仓库后自动发送
  - 包含入库单号、物品类别、总重量等信息
  - 确认库存已成功更新

### 2. 已读/未读功能

- 未读消息显示明显标记（蓝色边框、"新"徽章）
- 支持单条消息标记为已读
- 支持一键全部标记为已读
- 导航栏显示未读消息数量徽章
- 已读消息在下次登录时不会再次显示为未读

### 3. 消息管理

- 消息列表按时间倒序显示（最新的在前）
- 支持筛选：全部消息、未读消息、已读消息
- 支持删除单条消息
- 消息卡片显示颜色编码（不同类型不同颜色）
- 显示消息图标、标题、内容、创建时间

### 4. 实时更新

- 页面加载时自动获取未读消息数量
- 每30秒自动刷新未读消息数量
- 导航栏徽章实时更新

## 技术实现

### 数据库层

#### 1. 新增表：BaseStaffNotifications

```sql
CREATE TABLE BaseStaffNotifications (
    NotificationID INT PRIMARY KEY IDENTITY(1,1),
    WorkerID INT NOT NULL,
    NotificationType NVARCHAR(50) NULL,
    Title NVARCHAR(200) NULL,
    Content NVARCHAR(1000) NULL,
    RelatedTransportOrderID INT NULL,
    RelatedWarehouseReceiptID INT NULL,
    CreatedDate DATETIME2 NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadDate DATETIME2 NULL,
    CONSTRAINT FK_BaseStaffNotifications_Workers FOREIGN KEY (WorkerID) 
        REFERENCES SortingCenterWorkers(WorkerID) ON DELETE CASCADE
);
```

#### 2. 索引

- `IX_BaseStaffNotifications_WorkerID` - 按工作人员查询
- `IX_BaseStaffNotifications_CreatedDate` - 按时间排序
- `IX_BaseStaffNotifications_IsRead` - 筛选已读/未读
- `IX_BaseStaffNotifications_NotificationType` - 按类型筛选

### 代码结构

#### 1. Model 层

- **BaseStaffNotifications.cs** - 通知实体类
- **BaseStaffNotificationTypes** - 通知类型枚举和辅助方法

#### 2. DAL 层

- **BaseStaffNotificationDAL.cs** - 数据访问层
  - AddNotification - 添加单条通知
  - AddNotificationsToAllWorkers - 向所有基地工作人员发送通知
  - GetWorkerNotifications - 获取工作人员通知列表（分页）
  - GetUnreadCount - 获取未读消息数量
  - MarkAsRead - 标记单条消息为已读
  - MarkAllAsRead - 标记所有消息为已读
  - DeleteNotification - 删除消息

#### 3. BLL 层

- **BaseStaffNotificationBLL.cs** - 业务逻辑层
  - SendNotification - 发送通用通知
  - SendNotificationToAllWorkers - 向所有工作人员发送通知
  - SendTransportOrderCreatedNotification - 发送运输单创建通知
  - SendTransportOrderCompletedNotification - 发送运输单完成通知
  - SendWarehouseReceiptCreatedNotification - 发送入库单创建通知
  - SendWarehouseInventoryWrittenNotification - 发送仓库写入通知

#### 4. Controller 层

**StaffController.cs** 新增方法：
- `SortingCenterWorkerMessageCenter()` - 消息中心页面
- `GetBaseStaffUnreadNotificationCount()` - 获取未读数量（AJAX）
- `MarkBaseStaffNotificationAsRead()` - 标记为已读（AJAX）
- `MarkAllBaseStaffNotificationsAsRead()` - 全部标记为已读（AJAX）
- `DeleteBaseStaffNotification()` - 删除消息（AJAX）

#### 5. View 层

- **SortingCenterWorkerMessageCenter.cshtml** - 消息中心主页面
- **_SortingCenterWorkerLayout.cshtml** - 导航栏添加未读徽章

#### 6. 集成点

**TransportationOrderBLL.cs**
- `CreateTransportationOrder()` - 创建运输单后发送通知

**TransportationOrderBLL.cs**
- `CompleteTransportation()` - 完成运输后发送通知

**WarehouseReceiptBLL.cs**
- `CreateWarehouseReceipt()` - 创建入库单后发送两种通知
  1. 入库单创建通知
  2. 仓库库存写入通知

## 部署步骤

### 1. 数据库迁移

执行以下SQL脚本创建通知表：

```bash
# 路径: Database/CreateBaseStaffNotificationsTable.sql
```

在SQL Server Management Studio或Azure Data Studio中运行：

```sql
USE [RecyclingDB];  -- 替换为您的数据库名
GO

-- 运行整个 CreateBaseStaffNotificationsTable.sql 文件
```

### 2. 编译项目

```bash
# 在Visual Studio中
1. 打开解决方案
2. 右键点击解决方案 -> 重新生成解决方案
3. 确保所有项目编译成功
```

### 3. 发布到服务器

```bash
# 使用Visual Studio发布功能
1. 右键点击 recycling.Web.UI 项目 -> 发布
2. 选择发布配置
3. 点击发布
```

## 测试指南

### 1. 验证数据库表创建

```sql
-- 检查表是否存在
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'BaseStaffNotifications';

-- 检查索引是否创建
SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID('BaseStaffNotifications');
```

### 2. 测试消息创建

#### 测试运输单创建通知

1. 以回收员身份登录
2. 进入"运输管理"
3. 创建一个新的运输单
4. 以基地工作人员身份登录
5. 检查消息中心是否收到"新运输单待处理"通知
6. 验证通知内容包含：运输单号、回收员姓名、取货地址、预估重量

#### 测试运输单完成通知

1. 以运输人员身份登录
2. 完成一个运输单
3. 以基地工作人员身份登录
4. 检查消息中心是否收到"运输单已完成"通知
5. 验证通知内容包含：运输单号、运输人员姓名、实际重量

#### 测试入库单创建和仓库写入通知

1. 以基地工作人员A登录
2. 进入"仓库管理"
3. 为已完成的运输单创建入库单
4. 以基地工作人员B登录
5. 检查消息中心是否收到两条通知：
   - "入库单已创建"
   - "仓库库存已更新"
6. 验证通知内容正确

### 3. 测试已读/未读功能

1. 登录基地工作人员账号
2. 确保有未读消息
3. 验证未读消息显示蓝色边框和"新"徽章
4. 验证导航栏显示未读数量徽章
5. 点击"标记已读"按钮
6. 验证消息变为灰色，"新"徽章消失
7. 验证导航栏徽章数量减少
8. 退出登录
9. 重新登录
10. 验证已读消息保持已读状态

### 4. 测试筛选功能

1. 确保消息列表中有已读和未读消息
2. 点击"未读"标签，验证只显示未读消息
3. 点击"已读"标签，验证只显示已读消息
4. 点击"全部"标签，验证显示所有消息

### 5. 测试删除功能

1. 点击某条消息的"删除"按钮
2. 确认删除
3. 验证消息从列表中消失
4. 刷新页面，验证消息确实被删除

### 6. 测试一键全部标记已读

1. 确保有多条未读消息
2. 点击"全部标记为已读"按钮
3. 验证所有未读消息变为已读状态
4. 验证导航栏徽章数量变为0

### 7. 测试实时更新

1. 打开消息中心页面
2. 在另一个浏览器/标签页中创建新的运输单或入库单
3. 等待30秒（或刷新页面）
4. 验证导航栏徽章更新
5. 刷新消息中心页面，验证新消息出现

## 故障排除

### 问题1：导航栏徽章不显示

**可能原因：**
- AJAX请求失败
- 数据库连接问题
- 权限问题

**解决方法：**
1. 打开浏览器开发者工具（F12）
2. 查看控制台是否有错误
3. 检查网络请求是否成功
4. 验证数据库连接字符串

### 问题2：消息未创建

**可能原因：**
- 数据库表未创建
- 外键约束问题
- 业务逻辑错误

**解决方法：**
1. 检查数据库表是否存在
2. 检查应用程序日志
3. 在代码中添加断点调试
4. 验证工作人员ID是否有效

### 问题3：已读状态未保存

**可能原因：**
- 数据库更新失败
- Session问题
- 缓存问题

**解决方法：**
1. 检查数据库中的IsRead和ReadDate字段
2. 验证WorkerID是否正确
3. 清除浏览器缓存
4. 检查Session是否有效

## 性能优化建议

### 1. 数据库优化

- 定期清理旧消息（超过6个月）
- 监控索引使用情况
- 考虑分区表（如果消息量很大）

### 2. 缓存策略

- 考虑使用Redis缓存未读数量
- 减少数据库查询频率
- 实现消息推送（SignalR）代替轮询

### 3. 前端优化

- 实现虚拟滚动（如果消息很多）
- 使用懒加载
- 压缩图标资源

## 安全考虑

1. **SQL注入防护** - 所有SQL查询使用参数化
2. **XSS防护** - 视图中使用Html.Raw(HttpUtility.HtmlEncode())
3. **权限验证** - 所有AJAX请求验证Session和角色
4. **级联删除** - 删除工作人员时自动删除相关通知

## 未来增强

1. **消息推送** - 使用SignalR实现实时推送
2. **邮件通知** - 重要消息发送邮件
3. **短信通知** - 紧急消息发送短信
4. **消息搜索** - 添加搜索功能
5. **消息分类** - 更细粒度的分类
6. **消息统计** - 添加统计报表
7. **批量操作** - 批量删除、批量标记

## 技术栈

- ASP.NET MVC 5
- Entity Framework 6
- SQL Server 2019+
- jQuery 3.x
- Bootstrap 3.x
- Font Awesome 4.x

## 文件清单

### 新增文件

- `Database/CreateBaseStaffNotificationsTable.sql`
- `recycling.Model/BaseStaffNotifications.cs`
- `recycling.DAL/BaseStaffNotificationDAL.cs`
- `recycling.BLL/BaseStaffNotificationBLL.cs`

### 修改文件

- `recycling.Web.UI/Controllers/StaffController.cs`
- `recycling.Web.UI/Views/Staff/SortingCenterWorkerMessageCenter.cshtml`
- `recycling.Web.UI/Views/Shared/_SortingCenterWorkerLayout.cshtml`
- `recycling.BLL/TransportationOrderBLL.cs`
- `recycling.BLL/WarehouseReceiptBLL.cs`

## 总结

本次实现完整地满足了需求规格说明中的所有功能点：

✅ 运输单创建时生成消息提示  
✅ 运输单完成后生成消息提示  
✅ 入库单创建时生成消息提示  
✅ 仓库写入时生成消息提示  
✅ 所有消息都设有已读功能  
✅ 下次登录时已读消息不会再次提示  
✅ 导航栏显示未读消息数量  
✅ 消息中心页面美观易用  

消息中心功能已完全实现并可投入使用！
