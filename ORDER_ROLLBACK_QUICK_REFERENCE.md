# Order Rollback Feature - Quick Reference

## Quick Start

This feature allows recyclers to rollback orders when items don't meet recycling requirements offline.

## User Story

**As a** recycler
**I want to** rollback an order when I discover items are unsuitable
**So that** I can properly communicate the issue to the user while maintaining contact

## Key Files Modified

```
Backend:
├── recycling.Model/UserNotifications.cs (Added OrderRolledBack type)
├── recycling.BLL/UserNotificationBLL.cs (Added SendOrderRolledBackNotification)
├── recycling.BLL/RecyclerOrderBLL.cs (Added RollbackOrder method)
└── recycling.Web.UI/Controllers/
    ├── StaffController.cs (Added RollbackOrder endpoint)
    └── HomeController.cs (Updated GetStatusBadgeClass)

Frontend:
├── recycling.Web.UI/Views/Staff/Recycler_OrderManagement.cshtml
│   ├── Added rollback button
│   └── Added rollbackOrder() JavaScript function
└── recycling.Web.UI/Views/Home/Order.cshtml
    ├── Added handling for "已取消-回收员回退" status
    ├── Added "联系回收员" button for rollback orders
    └── Added CSS for rollback badge

Documentation:
├── ORDER_ROLLBACK_TEST_GUIDE.md
└── ORDER_ROLLBACK_IMPLEMENTATION_SUMMARY_CN.md
```

## API Endpoint

**URL:** `/Staff/RollbackOrder`
**Method:** POST
**Authentication:** Required (Recycler role)
**CSRF Protection:** Yes

**Parameters:**
- `appointmentId` (int) - Order ID to rollback
- `reason` (string) - Reason for rollback

**Response:**
```json
{
  "success": true,
  "message": "订单已成功回退"
}
```

## Business Logic Flow

```
1. Validate recycler session
2. Validate order exists and belongs to recycler
3. Validate order status is "进行中"
4. Update order status to "已取消-回收员回退"
5. Send system message to conversation
6. Send notification to user
7. Return success response
```

## Status Transition

```
已预约 → [Accept Order] → 进行中 → [Rollback Order] → 已取消-回收员回退
                          ↓
                    [Complete Order] → 已完成
```

## New Order Status

**Status:** 已取消-回收员回退
**Badge Class:** status-rolledback-badge
**Badge Color:** Orange/Yellow (#fff3cd)
**User Actions:** View Details, Contact Recycler, Re-book

## Notification Details

**Type:** OrderRolledBack
**Icon:** fa-undo
**Color:** #fd7e14 (Orange)
**Title:** "订单已回退"
**Content Template:**
```
您的订单 {OrderNumber} 已被回收员 {RecyclerName} 回退。
原因：{Reason}。
您可以继续与回收员沟通了解详情。
```

## UI Components

### Recycler Side (Order Management)

**Button:** 回退订单
- **Visibility:** Only for "进行中" orders
- **Style:** `btn-action warning`
- **Icon:** fa-undo
- **Action:** Prompts for reason, confirms, then submits

### User Side (My Orders)

**Status Badge:** 已取消-回收员回退
- **Background:** #fff3cd
- **Text Color:** #856404
- **Border:** #ffeeba

**Action Buttons:**
1. 查看详情 (View Details)
2. 联系回收员 (Contact Recycler) - **New for rollback orders**
3. 再预约一单 (Re-book)

## Security Measures

1. ✅ CSRF token validation
2. ✅ Session-based authentication
3. ✅ Order ownership verification
4. ✅ Order status validation
5. ✅ Input validation (reason required)

## Testing Checklist

- [ ] Recycler can rollback "进行中" order
- [ ] User receives notification
- [ ] User can contact recycler after rollback
- [ ] Only "进行中" orders show rollback button
- [ ] Reason input is required
- [ ] Button states work correctly
- [ ] Visual styling is correct
- [ ] System message sent to conversation

## Common Issues & Solutions

**Issue:** Button not showing
**Solution:** Check order status is exactly "进行中"

**Issue:** Rollback fails
**Solution:** Verify order belongs to current recycler

**Issue:** Notification not received
**Solution:** Check UserNotificationBLL.SendOrderRolledBackNotification

**Issue:** Can't contact recycler
**Solution:** Verify "已取消-回收员回退" status handling in Order.cshtml

## Code Examples

### Backend - Rollback Order
```csharp
var result = _recyclerOrderBLL.RollbackOrder(appointmentId, recyclerId, reason);
if (result.Success) {
    _messageBLL.SendMessage(systemMessage);
    _notificationBLL.SendOrderRolledBackNotification(appointmentId, recyclerName, reason);
}
```

### Frontend - Rollback Action
```javascript
function rollbackOrder(appointmentId) {
    const reason = prompt('请输入回退订单的原因：', '物品不符合回收要求');
    if (reason && confirm('确认要回退此订单吗？')) {
        // Submit rollback request
    }
}
```

## Performance Considerations

- Rollback operation: < 2 seconds
- Button response: Immediate with loading state
- Notification delivery: Asynchronous

## Future Enhancements

1. Rollback statistics dashboard
2. Predefined reason templates
3. Rollback history tracking
4. Auto-rematch to other recyclers
5. User feedback on rollback experience

## Support

For issues or questions:
- See: ORDER_ROLLBACK_TEST_GUIDE.md
- See: ORDER_ROLLBACK_IMPLEMENTATION_SUMMARY_CN.md
- Contact: Development team

## Version

**Version:** 1.0
**Date:** 2026-01-08
**Status:** ✅ Implemented, Pending Testing
