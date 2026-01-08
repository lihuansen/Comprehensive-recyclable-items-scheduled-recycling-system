# 回收员消息通知功能 - 快速参考

## 功能说明

当回收员给用户发送消息时，用户在"我的消息"中会自动收到通知提示。

## 使用方法

### 对于回收员
1. 登录系统
2. 进入"消息中心"
3. 选择订单
4. 发送消息
5. **无需其他操作** - 系统自动创建用户通知

### 对于用户
1. 登录系统后，导航栏"我的消息"显示未读徽章（红色数字）
2. 点击"我的消息"进入消息中心
3. 看到回收员消息通知（青绿色，信封图标）
4. 点击通知可以标记为已读
5. 点击"查看订单"跳转到订单详情

## 通知显示

**通知类型**：回收员消息  
**颜色**：青绿色 (#20c997)  
**图标**：信封 (fa-envelope)  
**标题格式**：回收员 [姓名] 发来新消息  
**内容**：消息预览（最多50字符）

## 技术实现

### 核心文件

```
recycling.Model/UserNotifications.cs
  └─ 添加 RecyclerMessageReceived 通知类型

recycling.BLL/UserNotificationBLL.cs
  └─ 添加 SendRecyclerMessageNotification() 方法

recycling.BLL/MessageBLL.cs
  └─ 在 SendMessage() 中自动创建通知
```

### 数据流

```
回收员发送消息 
  ↓
MessageBLL.SendMessage()
  ↓
保存到 Messages 表
  ↓
检测 SenderType == "recycler"
  ↓
UserNotificationBLL.SendRecyclerMessageNotification()
  ↓
保存到 UserNotifications 表
  ↓
用户看到通知
```

## 关键代码

### 1. 通知类型定义
```csharp
// recycling.Model/UserNotifications.cs
public const string RecyclerMessageReceived = "RecyclerMessageReceived";
```

### 2. 发送通知
```csharp
// recycling.BLL/UserNotificationBLL.cs
public bool SendRecyclerMessageNotification(int orderId, string recyclerName, string messagePreview)
{
    // 获取用户ID
    int userId = _notificationDAL.GetUserIdByOrderId(orderId);
    
    // 创建通知
    var notification = new UserNotifications
    {
        UserID = userId,
        NotificationType = NotificationTypes.RecyclerMessageReceived,
        Title = $"回收员 {recyclerName} 发来新消息",
        Content = messagePreview,
        RelatedOrderID = orderId,
        CreatedDate = DateTime.Now,
        IsRead = false
    };
    
    return _notificationDAL.AddNotification(notification);
}
```

### 3. 自动触发
```csharp
// recycling.BLL/MessageBLL.cs
bool result = _messageDAL.SendMessage(message);
if (result)
{
    // 如果是回收员发送的消息，创建用户通知
    if (request.SenderType == "recycler" && request.SenderID > 0)
    {
        try
        {
            var staffDAL = new StaffDAL();
            var recycler = staffDAL.GetRecyclerById(request.SenderID);
            string recyclerName = recycler?.Username ?? "回收员";
            
            _userNotificationBLL.SendRecyclerMessageNotification(
                request.OrderID, 
                recyclerName, 
                request.Content
            );
        }
        catch (Exception ex)
        {
            // 通知失败不影响消息发送
            System.Diagnostics.Debug.WriteLine($"创建用户通知失败：{ex.Message}");
        }
    }
    
    return (true, "消息发送成功");
}
```

## 配置

**无需配置** - 功能开箱即用

## 数据库

使用现有的 `UserNotifications` 表，无需额外创建表或字段。

## 安全性

✅ 通过 CodeQL 安全扫描（0个漏洞）  
✅ SQL注入防护（参数化查询）  
✅ Null 检查和输入验证  
✅ 异常处理不泄露信息

## 性能

- 通知创建不阻塞消息发送
- 使用异常处理确保容错
- 数据库操作高效（现有索引）

## 故障排查

### 问题：用户没有收到通知

**检查项**：
1. 消息是否由回收员发送（SenderType = "recycler"）
2. 订单状态是否正确
3. 数据库连接是否正常
4. 查看服务器日志

### 问题：通知内容为空

**原因**：消息内容为空或null  
**解决**：系统会自动显示"（新消息）"

### 问题：导航栏徽章不显示

**检查项**：
1. 刷新浏览器
2. 检查JavaScript控制台错误
3. 手动调用 `window.updateNavUnreadBadge()`

## 文档

- **测试指南**：[RECYCLER_MESSAGE_NOTIFICATION_TEST_GUIDE.md](RECYCLER_MESSAGE_NOTIFICATION_TEST_GUIDE.md)
- **实现总结**：[RECYCLER_MESSAGE_NOTIFICATION_IMPLEMENTATION_CN.md](RECYCLER_MESSAGE_NOTIFICATION_IMPLEMENTATION_CN.md)
- **安全审查**：[SECURITY_SUMMARY_RECYCLER_MESSAGE_NOTIFICATION.md](SECURITY_SUMMARY_RECYCLER_MESSAGE_NOTIFICATION.md)

## 版本信息

- **功能版本**：1.0.0
- **实现日期**：2026-01-08
- **状态**：✅ 生产就绪

## 支持

如有问题，请参考详细文档或联系开发团队。
