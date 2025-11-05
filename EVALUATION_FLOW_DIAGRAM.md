# 订单评价功能流程图

## 修复前的问题流程

```
用户点击"评价订单"
    ↓
ReviewOrder 视图加载
    ↓
JavaScript 尝试访问 order.RecyclerID  ← ❌ 错误！属性不存在
    ↓
JavaScript 报错（undefined）
    ↓
无法提交评价
```

## 修复后的正确流程

```
用户点击"评价订单"
    ↓
HomeController.ReviewOrder() 检查：
  - 用户是否登录？
  - 订单状态是否为"已完成"？
  - 订单是否分配了回收员？ ← ✓ 新增验证
  - 订单是否已评价？
    ↓
所有检查通过，加载 ReviewOrder 视图
    ↓
ViewBag.Order = OrderDetail 对象
  {
    Appointment: {
      AppointmentID: 123,
      RecyclerID: 5,  ← ✓ 正确的数据结构
      Status: "已完成",
      ...
    },
    Categories: [...]
  }
    ↓
JavaScript 正确访问：
  - orderId = orderDetail.Appointment.AppointmentID
  - recyclerId = orderDetail.Appointment.RecyclerID
    ↓
用户选择星级和输入评价内容
    ↓
点击"提交评价"
    ↓
AJAX POST 到 HomeController.SubmitReview()
  参数：orderId, recyclerId, starRating, reviewText
    ↓
验证：
  - recyclerId > 0？ ← ✓ 新增验证
  - starRating 在 1-5 之间？
    ↓
调用 OrderReviewBLL.AddReview()
    ↓
OrderReviewDAL.AddReview()
  使用 SqlDbType.NVarChar ← ✓ 修复中文乱码
  写入 OrderReviews 表
    ↓
返回成功响应
    ↓
显示"评价成功"提示
    ↓
跳转回订单列表
```

## 数据库表关系

```
┌─────────────────┐
│   Appointments  │
│─────────────────│
│ AppointmentID   │←─────┐
│ UserID          │      │
│ RecyclerID      │      │ FK (外键)
│ Status          │      │
│ ...             │      │
└─────────────────┘      │
                         │
                         │
┌─────────────────┐      │
│  OrderReviews   │      │
│─────────────────│      │
│ ReviewID (PK)   │      │
│ OrderID         │──────┘
│ UserID          │──────┐
│ RecyclerID      │──────┼───┐
│ StarRating      │      │   │
│ ReviewText      │      │   │
│ CreatedDate     │      │   │
└─────────────────┘      │   │
                         │   │
        ┌────────────────┘   │
        │                    │
        ↓                    ↓
┌─────────────┐    ┌─────────────┐
│    Users    │    │  Recyclers  │
│─────────────│    │─────────────│
│ UserID (PK) │    │RecyclerID(PK)│
│ Username    │    │ Username    │
│ ...         │    │ Rating      │
└─────────────┘    └─────────────┘
```

## 中文编码处理对比

### 错误方式（可能导致乱码）:
```csharp
cmd.Parameters.AddWithValue("@ReviewText", "服务很好！");

// SQL Server 可能错误地识别为 VARCHAR
// 存储结果: "?????" (乱码)
```

### 正确方式（显式声明 Unicode）:
```csharp
cmd.Parameters.Add("@ReviewText", SqlDbType.NVarChar, 500)
    .Value = "服务很好！";

// SQL Server 正确识别为 NVARCHAR
// 存储结果: "服务很好！" (正常显示)
```

## 关键验证点

### 前端验证 (ReviewOrder.cshtml)
```javascript
if (selectedRating === 0) {
    alert('请选择星级评分');
    return;
}

if (reviewText.length > 500) {
    alert('评价内容不能超过500字');
    return;
}
```

### 后端验证 (HomeController.cs)
```csharp
// 检查登录状态
if (Session["LoginUser"] == null)
    return Json(new { success = false, message = "请先登录" });

// 检查回收员ID
if (recyclerId <= 0)
    return Json(new { success = false, message = "订单未分配回收员，无法评价" });

// 检查订单状态
if (order.Appointment.Status != "已完成")
{
    TempData["ErrorMsg"] = "只能评价已完成的订单";
    return RedirectToAction("Order", "Home");
}
```

### 业务逻辑验证 (OrderReviewBLL.cs)
```csharp
// 检查参数有效性
if (orderId <= 0 || userId <= 0 || recyclerId <= 0)
{
    return (false, "参数无效");
}

// 检查评分范围
if (starRating < 1 || starRating > 5)
{
    return (false, "评分必须在1-5星之间");
}

// 检查是否已评价
if (_reviewDAL.HasReviewed(orderId, userId))
{
    return (false, "该订单已经评价过了");
}
```

## 测试场景

### ✓ 正常场景
1. 用户已登录
2. 订单状态为"已完成"
3. 订单已分配回收员
4. 订单未被评价
5. 选择星级 (1-5)
6. 输入评价内容（可选，支持中文）
→ **结果：评价成功**

### ✗ 异常场景及处理

| 场景 | 验证点 | 错误提示 |
|------|--------|----------|
| 用户未登录 | Controller | "请先登录" |
| 订单未完成 | ReviewOrder | "只能评价已完成的订单" |
| 未分配回收员 | ReviewOrder | "该订单未分配回收员，无法评价" |
| 已经评价过 | ReviewOrder | "该订单已经评价过了" |
| 未选择星级 | JavaScript | "请选择星级评分" |
| 内容超长 | JavaScript | "评价内容不能超过500字" |

---

**创建日期：** 2025-11-05  
**文档版本：** 1.0
