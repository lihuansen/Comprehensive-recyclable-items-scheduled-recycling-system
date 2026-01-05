# 基地管理通知持久化修复文档

## 问题描述

根据用户反馈，基地管理系统存在以下两个问题：

1. **无法区分消息来源**：进入基地管理页面后，导航栏显示数字徽章，但用户不知道是运输管理还是仓库管理有新消息。
2. **已读状态不持久**：每次重新登录后，明明已经查看过的消息，但还是出现消息提示。这是因为之前使用Session存储"已查看数量"，退出登录后Session被清除。

## 解决方案概述

### 1. 区分消息来源

**实现方式**：
- 在"基地管理"页面的两个功能卡片（运输管理和仓库管理）上分别显示独立的数字徽章
- 运输管理卡片显示运输中订单的新增数量
- 仓库管理卡片显示已完成运输单的新增数量
- 导航栏的"基地管理"徽章显示两者的总和

### 2. 持久化已读状态

**实现方式**：
- 在数据库`SortingCenterWorkers`表中添加两个字段：
  - `LastViewedTransportCount`：记录工作人员最后一次查看运输管理时的订单数量
  - `LastViewedWarehouseCount`：记录工作人员最后一次查看仓库管理时的订单数量
- 用户登录时，从数据库加载这些值到Session
- 用户访问运输管理或仓库管理页面时，更新数据库中的对应字段
- 徽章只显示新增数量（当前总数 - 已查看数量）

## 技术实现

### 1. 数据库层更改

**新增SQL脚本**：`Database/AddNotificationTrackingToSortingCenterWorkers.sql`

```sql
ALTER TABLE [dbo].[SortingCenterWorkers]
ADD [LastViewedTransportCount] INT NOT NULL DEFAULT 0;

ALTER TABLE [dbo].[SortingCenterWorkers]
ADD [LastViewedWarehouseCount] INT NOT NULL DEFAULT 0;
```

### 2. Model层更改

**文件**：`recycling.Model/SortingCenterWorkers.cs`

新增属性：
```csharp
public int LastViewedTransportCount { get; set; }
public int LastViewedWarehouseCount { get; set; }
```

### 3. DAL层更改

**文件**：`recycling.DAL/StaffDAL.cs`

1. 更新`GetSortingCenterWorkerByUsername`方法，查询时包含新字段
2. 新增方法：
   - `UpdateSortingCenterWorkerTransportViewCount(int workerId, int count)`
   - `UpdateSortingCenterWorkerWarehouseViewCount(int workerId, int count)`

### 4. BLL层更改

**文件**：`recycling.BLL/StaffBLL.cs`

新增方法：
- `UpdateSortingCenterWorkerTransportViewCount(int workerId, int count)`
- `UpdateSortingCenterWorkerWarehouseViewCount(int workerId, int count)`

### 5. Controller层更改

**文件**：`recycling.Web.UI/Controllers/StaffController.cs`

1. **BaseManagement** action：
   - 从worker对象加载已查看数量到Session（如果Session为空）

2. **BaseTransportationManagement** action：
   - 获取当前运输中订单数量
   - 调用BLL方法更新到数据库
   - 同时更新Session

3. **BaseWarehouseManagement** action：
   - 获取当前已完成运输单数量
   - 调用BLL方法更新到数据库
   - 同时更新Session

4. **新增API**：`GetWarehouseUpdateCount`
   - 获取仓库管理的新增数量
   - 计算方式：当前总数 - 已查看数量

### 6. View层更改

**文件1**：`recycling.Web.UI/Views/Staff/BaseManagement.cshtml`

- 为仓库管理卡片添加徽章：`<span id="warehouseCardBadge" class="card-notification-badge">0</span>`
- 添加JavaScript函数`checkWarehouseUpdates()`
- 页面加载时同时检查运输和仓库更新

**文件2**：`recycling.Web.UI/Views/Shared/_SortingCenterWorkerLayout.cshtml`

- 更新JavaScript逻辑，同时获取运输和仓库的更新数量
- 导航栏徽章显示两者总和

## 用户体验流程

### 场景1：首次登录

1. 用户以基地工作人员身份登录
2. 系统从数据库加载`LastViewedTransportCount`和`LastViewedWarehouseCount`（默认为0）
3. 导航栏"基地管理"显示运输+仓库的总未读数
4. 进入基地管理页面，看到两个卡片分别显示未读数：
   - 运输管理卡片：显示运输中订单总数
   - 仓库管理卡片：显示已完成运输单总数

### 场景2：查看运输管理

1. 用户点击"运输管理"卡片
2. 进入运输管理页面，系统自动：
   - 获取当前运输中订单数量（例如：5个）
   - 更新数据库`LastViewedTransportCount = 5`
   - 更新Session
3. 返回基地管理页面，运输管理卡片徽章消失（5 - 5 = 0）
4. 导航栏徽章只显示仓库管理的未读数

### 场景3：有新订单到达

1. 用户停留在基地管理页面
2. 有新的运输订单状态变为"运输中"（总数变为6个）
3. 30秒后自动刷新，运输管理卡片徽章显示"1"（6 - 5 = 1）
4. 导航栏徽章相应更新

### 场景4：退出重新登录（关键场景）

1. 用户已经查看过运输管理（数据库中`LastViewedTransportCount = 5`）
2. 用户退出登录
3. 用户重新登录
4. **关键验证点**：
   - 系统从数据库加载`LastViewedTransportCount = 5`
   - 如果运输中订单仍为5个，徽章不显示（5 - 5 = 0）
   - 如果运输中订单增加到6个，徽章显示"1"（6 - 5 = 1）
5. **不会出现之前的问题**：已读消息不会重复显示为未读

## 部署步骤

### 1. 运行数据库迁移

在SQL Server Management Studio或命令行中执行：

```bash
sqlcmd -S [服务器名] -d RecyclingDB -i Database/AddNotificationTrackingToSortingCenterWorkers.sql
```

### 2. 编译和部署代码

在Visual Studio中：
1. 清理解决方案（Clean Solution）
2. 重新生成解决方案（Rebuild Solution）
3. 部署到IIS或测试环境

### 3. 验证部署

检查数据库表结构：
```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SortingCenterWorkers'
AND COLUMN_NAME IN ('LastViewedTransportCount', 'LastViewedWarehouseCount');
```

预期结果：
```
COLUMN_NAME                    DATA_TYPE  IS_NULLABLE  COLUMN_DEFAULT
LastViewedTransportCount       int        NO           ((0))
LastViewedWarehouseCount       int        NO           ((0))
```

## 测试场景

### 测试1：区分消息来源

**步骤**：
1. 确保有运输中订单（在运输人员端完成订单）
2. 确保有已完成运输单（在运输人员端标记到达基地）
3. 以基地工作人员身份登录
4. 访问基地管理页面

**预期结果**：
- 运输管理卡片右上角显示运输中订单数量
- 仓库管理卡片右上角显示已完成运输单数量
- 导航栏"基地管理"显示两者总和

### 测试2：标记已读

**步骤**：
1. 从测试1继续
2. 点击"运输管理"卡片
3. 等待页面加载完成
4. 返回基地管理页面

**预期结果**：
- 运输管理卡片徽章消失
- 导航栏徽章只显示仓库管理的数量

### 测试3：持久化验证（关键测试）

**步骤**：
1. 从测试2继续（运输管理已标记为已读）
2. 点击"退出登录"
3. 重新以相同的基地工作人员账号登录
4. 访问基地管理页面

**预期结果**：
- 运输管理卡片徽章**不显示**（因为没有新增订单）
- 只显示仓库管理的未读数
- **这是与之前版本的关键区别**：之前会重新显示所有运输订单

### 测试4：新订单提醒

**步骤**：
1. 从测试3继续（保持登录状态）
2. 用运输人员账号完成一个新订单，使其状态变为"运输中"
3. 在基地管理页面等待30秒（自动刷新）

**预期结果**：
- 运输管理卡片徽章重新出现，显示"1"
- 导航栏徽章相应更新

### 测试5：同时查看两个管理

**步骤**：
1. 以基地工作人员身份登录（有运输和仓库未读数）
2. 先访问运输管理
3. 返回基地管理
4. 再访问仓库管理
5. 返回基地管理

**预期结果**：
- 第3步：运输管理徽章消失，仓库管理徽章保留
- 第5步：两个徽章都消失
- 导航栏徽章为0（不显示）

### 测试6：跨会话持久化

**步骤**：
1. 登录并查看运输和仓库管理（都标记为已读）
2. 退出登录
3. 等待1小时（确保Session过期）
4. 重新登录

**预期结果**：
- 如果没有新订单，所有徽章都不显示
- 已读状态正确保持（从数据库加载）

## 技术要点

### 1. Session与数据库的协同

- **数据库**：持久化存储，跨登录会话
- **Session**：临时缓存，避免频繁查询数据库
- **同步机制**：
  - 登录时：数据库 → Session
  - 查看页面时：更新到数据库和Session
  - 获取徽章数时：从Session读取

### 2. 计数逻辑

```
未读数 = Max(0, 当前总数 - 已查看数量)
```

使用`Math.Max(0, ...)`确保结果不为负数（处理订单被取消或完成的情况）

### 3. 错误处理

- 数据库更新失败不中断页面加载
- AJAX请求失败静默处理（不影响用户体验）
- 使用Debug.WriteLine记录异常情况

### 4. 性能考虑

- 使用Session缓存减少数据库查询
- AJAX请求间隔30秒（平衡实时性和性能）
- 数据库更新只在访问页面时触发（不是每次刷新徽章）

## 兼容性说明

### 现有数据兼容

- 新字段有默认值0，不影响现有记录
- 已登录的用户会话不受影响（使用Session值）
- 下次登录时自动应用新逻辑

### 向后兼容

- 如果数据库字段不存在，代码会优雅降级（使用Session值0）
- 不影响其他工作人员类型（只针对sortingcenterworker角色）

## 注意事项

1. **数据库迁移必须先执行**：在部署代码前运行SQL脚本
2. **Session超时设置**：确保Web.config中Session超时时间合理（建议30分钟）
3. **数据库备份**：执行ALTER TABLE前备份数据库
4. **IIS应用程序池**：部署后重启应用程序池以清除旧Session

## 故障排查

### 问题1：徽章始终显示0

**可能原因**：
- 数据库字段未添加成功
- GetInTransitOrders或GetCompletedTransportOrders返回空

**解决方法**：
```sql
-- 检查字段是否存在
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SortingCenterWorkers' 
AND COLUMN_NAME LIKE 'LastViewed%';

-- 检查订单数据
SELECT COUNT(*) FROM TransportationOrders WHERE Status = N'运输中';
SELECT COUNT(*) FROM TransportationOrders WHERE Status = N'已完成';
```

### 问题2：退出重新登录后仍显示未读

**可能原因**：
- 数据库更新失败
- Session未从数据库加载

**解决方法**：
```sql
-- 检查特定工作人员的记录
SELECT WorkerID, Username, LastViewedTransportCount, LastViewedWarehouseCount
FROM SortingCenterWorkers
WHERE Username = 'worker001';
```

### 问题3：徽章不更新

**可能原因**：
- JavaScript错误
- API返回失败

**解决方法**：
- 打开浏览器开发者工具（F12）
- 检查Console是否有JavaScript错误
- 检查Network标签，查看AJAX请求状态

## 总结

本次修复通过以下方式彻底解决了问题：

1. **明确消息来源**：在卡片上直接显示独立徽章，用户一眼就能看出哪个模块有新消息
2. **持久化已读状态**：使用数据库存储，确保跨登录会话的状态保持
3. **改善用户体验**：
   - 不再重复提醒已读消息
   - 实时更新（30秒自动刷新）
   - 准确的未读数显示

这个方案已经在代码层面完整实现，待数据库迁移和测试验证后即可上线使用。
