# Order Rollback Feature Test Guide

## Feature Overview
This guide provides test scenarios for the new Order Rollback feature that allows recyclers to rollback orders when items don't meet recycling requirements.

## Prerequisites
- System must be running
- Test database must be populated with test data
- At least one user account with an active order
- At least one recycler account

## Test Scenarios

### Scenario 1: Successful Order Rollback
**Objective:** Verify that a recycler can successfully rollback an order in "进行中" status

**Test Steps:**
1. Login as a recycler account
2. Navigate to "订单管理" (Order Management)
3. Locate an order with status "进行中" (In Progress)
4. Click the "回退订单" (Rollback Order) button
5. Enter a reason in the prompt dialog: "物品不符合回收要求"
6. Confirm the rollback action
7. Wait for success message

**Expected Results:**
- Success message displayed: "订单已成功回退，用户已收到通知"
- Order status changes to "已取消-回收员回退"
- Order list refreshes automatically
- User receives notification about the rollback

### Scenario 2: User Receives Notification
**Objective:** Verify that user receives proper notification when order is rolled back

**Test Steps:**
1. Complete Scenario 1 (rollback an order)
2. Logout from recycler account
3. Login as the user who owns the rolled-back order
4. Navigate to "我的消息" (My Messages)
5. Check for new notification

**Expected Results:**
- New notification appears with title "订单已回退"
- Notification shows recycler name and reason
- Notification content mentions ability to contact recycler
- Notification icon is "fa-undo"
- Notification color is orange (#fd7e14)

### Scenario 3: User Can Contact Recycler After Rollback
**Objective:** Verify that communication remains available after order rollback

**Test Steps:**
1. Complete Scenario 1 (rollback an order)
2. Logout from recycler account
3. Login as the user who owns the rolled-back order
4. Navigate to "我的订单" (My Orders)
5. Switch to "已取消" (Cancelled) tab or view all orders
6. Locate the rolled-back order (status: "已取消-回收员回退")
7. Click "联系回收员" (Contact Recycler) button

**Expected Results:**
- "联系回收员" button is visible for rolled-back orders
- Clicking button navigates to conversation page
- User can send and receive messages with recycler
- System message about rollback is visible in conversation

### Scenario 4: Order Status Validation
**Objective:** Verify that only orders with "进行中" status can be rolled back

**Test Steps:**
1. Login as a recycler account
2. Navigate to "订单管理" (Order Management)
3. Verify "回退订单" button is NOT present for:
   - "已预约" (Pending) orders
   - "已完成" (Completed) orders
   - "已取消" (Cancelled) orders
4. Verify "回退订单" button IS present for:
   - "进行中" (In Progress) orders

**Expected Results:**
- Only "进行中" orders show the "回退订单" button
- Other statuses do not have this button

### Scenario 5: Empty Reason Validation
**Objective:** Verify that rollback requires a reason

**Test Steps:**
1. Login as a recycler account
2. Navigate to an order with status "进行中"
3. Click "回退订单" button
4. Clear the default reason text
5. Click OK without entering a reason

**Expected Results:**
- Alert message displayed: "请输入回退原因"
- Order is NOT rolled back
- Order remains in "进行中" status

### Scenario 6: Cancel Rollback Action
**Objective:** Verify that user can cancel rollback action

**Test Steps:**
1. Login as a recycler account
2. Navigate to an order with status "进行中"
3. Click "回退订单" button
4. Enter a reason
5. Click "Cancel" on the confirmation dialog

**Expected Results:**
- Rollback action is cancelled
- Order remains in "进行中" status
- No notification sent to user
- Order list not refreshed

### Scenario 7: UI Button States
**Objective:** Verify button states during rollback operation

**Test Steps:**
1. Login as a recycler account
2. Navigate to an order with status "进行中"
3. Click "回退订单" button
4. Enter reason and confirm
5. Observe button state during processing

**Expected Results:**
- Button becomes disabled during processing
- Button text changes to "处理中..." with spinner icon
- After completion, button restores to original state
- If error occurs, button is re-enabled

### Scenario 8: Multiple Orders with Rollback
**Objective:** Verify handling of multiple orders in different states

**Test Steps:**
1. Login as a recycler account
2. Accept multiple orders (at least 3)
3. Rollback one order
4. Complete another order normally
5. Leave one order in progress
6. Review order list and filter by different statuses

**Expected Results:**
- Each order shows in correct status
- Statistics at top reflect correct counts
- Filters work correctly for all statuses
- Rolled-back orders show in cancelled section with special badge

### Scenario 9: Visual Styling Verification
**Objective:** Verify visual elements are correctly styled

**Test Steps:**
1. Login as user who has a rolled-back order
2. Navigate to orders page
3. Locate rolled-back order

**Expected Results:**
- Rolled-back order has yellow/warning badge (status-rolledback-badge)
- Badge background: #fff3cd
- Badge text color: #856404
- Badge border: #ffeeba
- "联系回收员" button has warning style (yellow)

### Scenario 10: System Message in Conversation
**Objective:** Verify system message is sent to conversation

**Test Steps:**
1. Complete Scenario 1 (rollback an order)
2. Login as user
3. Navigate to conversation for rolled-back order
4. Check message history

**Expected Results:**
- System message exists in conversation
- Message contains: recycler name, reason, and guidance to contact recycler
- Message is marked as from "system"
- Message timestamp is correct

## Edge Cases and Error Scenarios

### Edge Case 1: Concurrent Rollback Attempts
**Test Steps:**
1. Open two browser windows with same recycler account
2. Navigate to same order in both windows
3. Attempt to rollback from both windows simultaneously

**Expected Results:**
- Only one rollback succeeds
- Second attempt shows error message
- Order status updated only once

### Edge Case 2: Network Interruption
**Test Steps:**
1. Start rollback process
2. Disable network connection during processing
3. Re-enable network

**Expected Results:**
- Proper error message displayed
- Button restored to enabled state
- Can retry rollback

### Edge Case 3: Special Characters in Reason
**Test Steps:**
1. Attempt rollback with reason containing special characters: `<script>alert('test')</script>`
2. Attempt rollback with very long reason (>500 characters)

**Expected Results:**
- Special characters are properly escaped
- Long reasons are handled appropriately
- No XSS vulnerabilities
- No database errors

## Performance Testing

### Performance Test 1: Button Response Time
**Objective:** Verify rollback completes in reasonable time

**Test Steps:**
1. Measure time from clicking "回退订单" to success message

**Expected Results:**
- Total time < 2 seconds under normal load
- UI remains responsive during operation

## Regression Testing

### Regression 1: Normal Order Completion Still Works
**Test Steps:**
1. Complete an order normally (without rollback)
2. Verify all existing functionality works

**Expected Results:**
- Order completion works as before
- Inventory updated correctly
- Notifications sent correctly

### Regression 2: User Order Cancellation Still Works
**Test Steps:**
1. As user, cancel an order normally

**Expected Results:**
- User cancellation works as before
- Status is "已取消" (not "已取消-回收员回退")
- No "联系回收员" button for user-cancelled orders

## Test Data Requirements

### User Account
- Username: test_user_001
- At least 3 active orders in different statuses

### Recycler Account
- Username: test_recycler_001
- Has permission to accept and manage orders
- Region matches user orders

### Orders
- Order 1: Status "已预约"
- Order 2: Status "进行中" (for rollback testing)
- Order 3: Status "已完成"

## Test Environment Checklist

- [ ] Database connection working
- [ ] Web application running
- [ ] Test accounts created
- [ ] Test orders created
- [ ] Browser console open (for debugging)
- [ ] Network tab open (to monitor AJAX requests)

## Known Issues / Limitations

None identified at this time.

## Sign-off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Developer | | | |
| Tester | | | |
| Product Owner | | | |

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-08 | GitHub Copilot | Initial test guide |
