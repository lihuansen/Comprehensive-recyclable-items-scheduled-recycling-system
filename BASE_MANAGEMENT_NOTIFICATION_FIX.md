# 基地管理通知系统改进文档

## 问题描述

原有基地管理系统存在三个问题：

1. **问题一**：基地管理导航中的右上角出现了数字表示有消息，但是点击基地管理后，我们设计了两个大功能（运输管理和仓库管理），不知道这个数字消息是哪里的消息。

2. **问题二**：当点击进入到有数字消息的管理中后，数字消息应该消失才对，数字消息起到的作用就是提示工作人员有消息。

3. **问题三**：点击进入到运输管理，现在需要点击刷新才会出现数据列表，但需要的是一点击运输管理后进入到运输管理功能页，数据列表就已经显示出来了。

## 解决方案

### 1. 问题一：明确消息来源

**现状**：
- 在 `BaseManagement.cshtml` 页面中，运输管理卡片上已经有一个 `transportCardBadge` 徽章元素
- 页面加载时通过 `checkTransportUpdates()` 函数调用 `GetTransportUpdateCount` API 获取并显示消息数量

**改进**：
- 保持现有设计，运输管理卡片上的徽章会显示运输中订单的数量
- 这样用户可以清楚地知道消息是来自"运输管理"功能

**代码位置**：
- `recycling.Web.UI/Views/Staff/BaseManagement.cshtml` 第 183 行

```html
<span id="transportCardBadge" class="card-notification-badge">0</span>
```

### 2. 问题二：进入页面后清除徽章

**实现机制**：使用 Session 状态追踪用户已查看的订单数量

**核心逻辑**：
1. 在 `BaseManagement` action 中初始化 `LastViewedTransportCount` 为 0
2. 当用户进入 `BaseTransportationManagement` 页面时，将当前运输中订单数量保存到 Session
3. `GetTransportUpdateCount` API 只返回新增的订单数量（当前数量 - 已查看数量）

**代码更改**：

**StaffController.cs - BaseManagement action**（第 4192-4210 行）：
```csharp
public ActionResult BaseManagement()
{
    if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
        return RedirectToAction("Login", "Staff");

    var worker = (SortingCenterWorkers)Session["LoginStaff"];
    ViewBag.StaffName = worker.Username;
    ViewBag.DisplayName = "基地工作人员";
    ViewBag.StaffRole = "sortingcenterworker";

    // 如果是首次访问，初始化已查看数量为0（显示所有订单为新消息）
    if (Session["LastViewedTransportCount"] == null)
    {
        Session["LastViewedTransportCount"] = 0;
    }

    return View();
}
```

**StaffController.cs - BaseTransportationManagement action**（第 4212-4228 行）：
```csharp
public ActionResult BaseTransportationManagement()
{
    if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
        return RedirectToAction("Login", "Staff");

    var worker = (SortingCenterWorkers)Session["LoginStaff"];
    ViewBag.StaffName = worker.Username;
    ViewBag.DisplayName = "基地工作人员";
    ViewBag.StaffRole = "sortingcenterworker";

    // 标记运输通知为已查看（将当前运输中订单数量存储到会话中）
    Session["LastViewedTransportCount"] = _warehouseReceiptBLL.GetInTransitOrders()?.Count() ?? 0;

    return View();
}
```

**StaffController.cs - GetTransportUpdateCount action**（第 4251-4283 行）：
```csharp
[HttpGet]
public ContentResult GetTransportUpdateCount()
{
    try
    {
        if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
        {
            return JsonContent(new { success = false, count = 0 });
        }

        var orders = _warehouseReceiptBLL.GetInTransitOrders();
        var currentCount = orders?.Count() ?? 0;
        
        // 获取上次查看时的数量
        int lastViewedCount = 0;
        if (Session["LastViewedTransportCount"] != null)
        {
            lastViewedCount = (int)Session["LastViewedTransportCount"];
        }
        
        // 只显示新增的订单数量
        var newCount = Math.Max(0, currentCount - lastViewedCount);
        
        return JsonContent(new { success = true, count = newCount });
    }
    catch (Exception ex)
    {
        return JsonContent(new { success = false, count = 0, message = $"获取更新数量失败：{ex.Message}" });
    }
}
```

### 3. 问题三：运输管理页面自动加载数据

**问题原因**：
- 页面中的 JavaScript 代码需要获取 AntiForgeryToken 来发送 POST 请求
- 原来的 `@Html.AntiForgeryToken()` 放在页面底部，可能导致 DOM 加载顺序问题

**解决方案**：
- 将 `@Html.AntiForgeryToken()` 移到页面顶部（在 `<style>` 标签之前）
- 确保 `$(document).ready()` 执行时，AntiForgeryToken 已经存在于 DOM 中
- 添加更详细的错误日志记录以便调试

**代码更改**：

**BaseTransportationManagement.cshtml** 开头部分：
```cshtml
@{
    ViewBag.Title = "运输管理";
    Layout = "~/Views/Shared/_SortingCenterWorkerLayout.cshtml";
}

@Html.AntiForgeryToken()

<style>
    /* ... 样式代码 ... */
</style>

<!-- 将 AntiForgeryToken 移到页面顶部，确保在脚本执行前已加载 -->

<div class="transport-container">
    <!-- ... 页面内容 ... -->
</div>
```

**JavaScript 改进**（增强错误处理）：
```javascript
error: function (xhr, status, error) {
    console.error('获取运输订单失败:', error);
    alert('网络错误，请稍后重试');
}
```

## 用户体验流程

1. **首次访问基地管理页面**：
   - 系统初始化 `LastViewedTransportCount = 0`
   - 如果有运输中订单，徽章显示订单数量
   - 用户可以在"运输管理"卡片上看到具体的消息数量

2. **点击进入运输管理**：
   - 页面自动加载运输中订单数据（无需手动刷新）
   - 系统更新 `LastViewedTransportCount` 为当前订单数量
   - 用户查看运输订单列表

3. **返回基地管理页面**：
   - 如果没有新增运输订单，徽章不显示（数量为 0）
   - 如果有新增运输订单，徽章只显示新增的数量
   - 徽章起到了"新消息提醒"的作用

4. **后续访问**：
   - 导航栏和卡片上的徽章会持续更新（每 30 秒自动刷新）
   - 只显示用户未查看的新订单数量

## 技术要点

1. **Session 状态管理**：
   - 使用 ASP.NET Session 存储用户已查看的订单数量
   - Session 在用户登录期间保持有效（超时时间 30 分钟）
   - 退出登录时 Session 自动清除

2. **实时更新机制**：
   - 页面使用 JavaScript 定时器每 30 秒调用 API 更新徽章
   - 不需要用户手动刷新页面

3. **用户友好的提示**：
   - 徽章数字清晰标识未读消息数量
   - 进入页面后自动标记为已读，避免重复提醒

## 测试要点

1. **首次访问测试**：
   - 以基地工作人员身份登录
   - 访问基地管理页面
   - 确认运输管理卡片上的徽章显示正确的订单数量

2. **标记已读测试**：
   - 点击进入运输管理
   - 确认数据自动加载（无需手动刷新）
   - 返回基地管理页面
   - 确认徽章消失或数量变为 0

3. **新订单提醒测试**：
   - 在运输人员端完成一个订单，使其状态变为"运输中"
   - 在基地管理页面等待自动刷新（或手动刷新）
   - 确认徽章显示新增的订单数量

4. **跨会话测试**：
   - 退出登录
   - 重新登录
   - 确认徽章显示所有运输中订单（因为 Session 已清除）

## 文件清单

修改的文件：
1. `recycling.Web.UI/Controllers/StaffController.cs`
   - `BaseManagement` action（第 4192-4210 行）
   - `BaseTransportationManagement` action（第 4212-4228 行）
   - `GetTransportUpdateCount` action（第 4251-4283 行）

2. `recycling.Web.UI/Views/Staff/BaseTransportationManagement.cshtml`
   - 移动 AntiForgeryToken 到页面顶部
   - 改进 AJAX 错误处理

未修改的文件（已满足需求）：
1. `recycling.Web.UI/Views/Staff/BaseManagement.cshtml`
   - 运输管理卡片上已有徽章显示功能
   - 自动刷新机制已实现

2. `recycling.Web.UI/Views/Shared/_SortingCenterWorkerLayout.cshtml`
   - 导航栏徽章显示功能已实现
   - 自动刷新机制已实现

## 总结

本次改进通过以下方式解决了三个问题：

1. **明确消息来源**：在"运输管理"卡片上直接显示徽章，用户一目了然
2. **智能已读机制**：使用 Session 追踪已查看数量，只提示新增订单
3. **自动加载数据**：确保 AntiForgeryToken 在页面顶部加载，保证 AJAX 请求正常执行

这些改进提升了用户体验，使通知系统更加智能和友好。
