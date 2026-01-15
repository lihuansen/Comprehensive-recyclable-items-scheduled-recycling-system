# 基地工作人员消息中心 - 快速开始指南

## 立即开始

### 第一步：运行数据库脚本

```sql
-- 在SQL Server中执行
USE [你的数据库名];
GO

-- 运行脚本
-- 文件位置: Database/CreateBaseStaffNotificationsTable.sql
```

### 第二步：重新编译项目

在Visual Studio中：
1. 解决方案资源管理器 -> 右键点击解决方案
2. 选择"重新生成解决方案"
3. 等待编译完成

### 第三步：发布/运行

- **开发环境**: 直接按F5运行
- **生产环境**: 使用Visual Studio发布功能

### 第四步：验证功能

1. 以基地工作人员身份登录
2. 点击导航栏"消息中心"
3. 应该看到消息列表页面

## 快速测试

### 触发运输单创建通知

```
回收员登录 -> 创建运输单 -> 基地工作人员查看消息中心
```

### 触发运输单完成通知

```
运输人员登录 -> 完成运输单 -> 基地工作人员查看消息中心
```

### 触发入库通知

```
基地工作人员A登录 -> 创建入库单 -> 基地工作人员B查看消息中心
```

## 功能清单

✅ 显示运输管理消息  
✅ 显示仓库管理消息  
✅ 未读消息高亮显示  
✅ 已读功能  
✅ 导航栏显示未读数量徽章  
✅ 下次登录已读消息不会再提示  

## 消息类型说明

| 类型 | 触发时机 | 图标 | 颜色 |
|------|---------|------|------|
| 运输单创建 | 回收员创建运输单 | 🚚 | 蓝色 |
| 运输单完成 | 运输人员完成运输 | ✅ | 绿色 |
| 入库单创建 | 工作人员创建入库单 | 📄 | 蓝色 |
| 仓库写入 | 库存写入仓库 | 🏭 | 紫色 |

## 常用操作

### 标记单条消息为已读
点击消息卡片上的"标记已读"按钮

### 全部标记为已读
点击页面右上角的"全部标记为已读"按钮

### 删除消息
点击消息卡片上的"删除"按钮

### 筛选消息
点击页面顶部的标签：全部/未读/已读

## 故障排查

### 导航栏徽章不显示？
1. 检查是否有未读消息
2. 按F12打开开发者工具查看Console错误
3. 刷新页面

### 消息未创建？
1. 检查数据库表是否创建
2. 检查应用程序日志
3. 验证触发操作是否正确

### 已读状态未保存？
1. 检查Session是否有效
2. 清除浏览器缓存
3. 重新登录

## 技术支持

如有问题，请检查：
- `BASE_STAFF_MESSAGE_CENTER_IMPLEMENTATION.md` - 完整实现文档
- 应用程序日志
- SQL Server日志

## 数据库查询

### 查看所有通知
```sql
SELECT * FROM BaseStaffNotifications 
ORDER BY CreatedDate DESC;
```

### 查看特定工作人员的未读通知
```sql
SELECT * FROM BaseStaffNotifications 
WHERE WorkerID = [工作人员ID] AND IsRead = 0
ORDER BY CreatedDate DESC;
```

### 统计各类型通知数量
```sql
SELECT NotificationType, COUNT(*) as Count
FROM BaseStaffNotifications
GROUP BY NotificationType;
```

## 配置说明

### 自动刷新频率
默认30秒，可在 `_SortingCenterWorkerLayout.cshtml` 中修改：
```javascript
setInterval(function () {
    // ...
}, 30000);  // 修改这个值（毫秒）
```

### 消息分页数量
默认50条，可在 `StaffController.cs` 中修改：
```csharp
var notifications = notificationBLL.GetWorkerNotifications(worker.WorkerID, 1, 50);
                                                                            // ^^修改这里
```

## 完成！

消息中心已就绪，可以开始使用了！🎉
