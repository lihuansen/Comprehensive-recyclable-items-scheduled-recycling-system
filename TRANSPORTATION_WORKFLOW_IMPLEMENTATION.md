# 运输管理工作流实现文档
# Transportation Management Workflow Implementation

## 概述 / Overview

本文档描述了运输管理系统的详细工作流程实现。系统现在支持在运输过程中跟踪多个详细阶段。

This document describes the implementation of the detailed transportation management workflow. The system now supports tracking multiple detailed stages during the transportation process.

## 工作流程 / Workflow

### 状态流转 / Status Flow

```
待接单 (Pending) 
  ↓ [运输人员点击"接单"]
已接单 (Accepted)
  ↓ [运输人员点击"确认取货地点"]
运输中 (In Transit) - 阶段: 确认取货地点
  ↓ [运输人员点击"到达取货地点"]
运输中 (In Transit) - 阶段: 到达取货地点
  ↓ [运输人员点击"装货完毕"]
运输中 (In Transit) - 阶段: 装货完毕
  ↓ [运输人员点击"确认送货地点"]
运输中 (In Transit) - 阶段: 确认送货地点
  ↓ [运输人员点击"到达送货地点"]
运输中 (In Transit) - 阶段: 到达送货地点
  ↓ [运输人员点击"运输完成"]
已完成 (Completed)
```

### 运输阶段详细说明 / Stage Details

1. **待接单 (Pending Acceptance)**
   - 初始状态，由回收员创建运输单后自动设置
   - Initial status, automatically set after recycler creates transport order
   - 运输人员可见此订单并可选择接单
   - Transporter can see this order and choose to accept

2. **已接单 (Accepted)**
   - 运输人员接单后的状态
   - Status after transporter accepts the order
   - 系统记录接单时间 (AcceptedDate)
   - System records acceptance time
   - 运输人员可以开始确认取货地点
   - Transporter can start confirming pickup location

3. **运输中 - 确认取货地点 (In Transit - Confirm Pickup Location)**
   - 运输人员确认取货地点
   - Transporter confirms pickup location
   - 状态变为"运输中"，阶段为"确认取货地点"
   - Status changes to "In Transit", stage is "Confirm Pickup Location"
   - 系统记录确认时间 (PickupConfirmedDate)
   - System records confirmation time

4. **运输中 - 到达取货地点 (In Transit - Arrive at Pickup)**
   - 运输人员到达取货地点
   - Transporter arrives at pickup location
   - 系统记录到达时间 (ArrivedAtPickupDate)
   - System records arrival time

5. **运输中 - 装货完毕 (In Transit - Loading Complete)**
   - 运输人员完成装货
   - Transporter completes loading
   - 系统记录装货完成时间 (LoadingCompletedDate)
   - System records loading completion time
   - **重要**: 此时系统会清空回收员的暂存点物品
   - **Important**: At this point, the system clears recycler's storage point items
   - 系统将库存从 StoragePoint 移动到 InTransit 状态
   - System moves inventory from StoragePoint to InTransit status

6. **运输中 - 确认送货地点 (In Transit - Confirm Delivery Location)**
   - 运输人员确认送货地点
   - Transporter confirms delivery location
   - 系统记录确认时间 (DeliveryConfirmedDate)
   - System records confirmation time

7. **运输中 - 到达送货地点 (In Transit - Arrive at Delivery)**
   - 运输人员到达送货地点
   - Transporter arrives at delivery location
   - 系统记录到达时间 (ArrivedAtDeliveryDate)
   - System records arrival time

8. **已完成 (Completed)**
   - 运输人员完成整个运输流程
   - Transporter completes entire transportation process
   - 系统记录完成时间 (CompletedDate, DeliveryDate)
   - System records completion time
   - 运输人员可选择填写实际重量 (ActualWeight)
   - Transporter can optionally enter actual weight
   - TransportStage 字段被清空
   - TransportStage field is cleared

## 数据库修改 / Database Changes

### 新增字段 / New Fields

在 `TransportationOrders` 表中添加以下字段:
The following fields are added to the `TransportationOrders` table:

1. **TransportStage** (NVARCHAR(50), NULL)
   - 运输阶段标识
   - Transport stage identifier
   - 可能的值: 确认取货地点, 到达取货地点, 装货完毕, 确认送货地点, 到达送货地点
   - Possible values: Confirm Pickup Location, Arrive at Pickup, Loading Complete, Confirm Delivery Location, Arrive at Delivery

2. **PickupConfirmedDate** (DATETIME2, NULL)
   - 确认取货地点的时间
   - Timestamp when pickup location was confirmed

3. **ArrivedAtPickupDate** (DATETIME2, NULL)
   - 到达取货地点的时间
   - Timestamp when transporter arrived at pickup location

4. **LoadingCompletedDate** (DATETIME2, NULL)
   - 装货完毕的时间
   - Timestamp when loading was completed

5. **DeliveryConfirmedDate** (DATETIME2, NULL)
   - 确认送货地点的时间
   - Timestamp when delivery location was confirmed

6. **ArrivedAtDeliveryDate** (DATETIME2, NULL)
   - 到达送货地点的时间
   - Timestamp when transporter arrived at delivery location

### 数据库迁移 / Database Migration

运行以下脚本来更新数据库结构:
Run the following script to update database structure:

```sql
Database/AddTransportStageColumn.sql
```

## API 端点 / API Endpoints

### 运输人员端点 / Transporter Endpoints

所有端点都需要运输人员身份认证和 CSRF 防护。
All endpoints require transporter authentication and CSRF protection.

1. **POST /Staff/AcceptTransportOrder**
   - 接单
   - Accept transport order
   - 参数: orderId
   - 前置条件: 订单状态为"待接单"
   - Precondition: Order status is "Pending"

2. **POST /Staff/ConfirmPickupLocation**
   - 确认取货地点
   - Confirm pickup location
   - 参数: orderId
   - 前置条件: 订单状态为"已接单"
   - Precondition: Order status is "Accepted"

3. **POST /Staff/ArriveAtPickupLocation**
   - 到达取货地点
   - Arrive at pickup location
   - 参数: orderId
   - 前置条件: 订单状态为"运输中"，阶段为"确认取货地点"
   - Precondition: Order status is "In Transit", stage is "Confirm Pickup Location"

4. **POST /Staff/CompleteLoading**
   - 装货完毕
   - Complete loading
   - 参数: orderId
   - 前置条件: 订单状态为"运输中"，阶段为"到达取货地点"
   - Precondition: Order status is "In Transit", stage is "Arrive at Pickup"

5. **POST /Staff/ConfirmDeliveryLocation**
   - 确认送货地点
   - Confirm delivery location
   - 参数: orderId
   - 前置条件: 订单状态为"运输中"，阶段为"装货完毕"
   - Precondition: Order status is "In Transit", stage is "Loading Complete"

6. **POST /Staff/ArriveAtDeliveryLocation**
   - 到达送货地点
   - Arrive at delivery location
   - 参数: orderId
   - 前置条件: 订单状态为"运输中"，阶段为"确认送货地点"
   - Precondition: Order status is "In Transit", stage is "Confirm Delivery Location"

7. **POST /Staff/CompleteTransport**
   - 完成运输
   - Complete transport
   - 参数: orderId, actualWeight (可选)
   - 前置条件: 订单状态为"运输中"，阶段为"到达送货地点"
   - Precondition: Order status is "In Transit", stage is "Arrive at Delivery"

## 界面说明 / UI Description

### 运输管理页面 / Transportation Management Page

路径: `/Staff/TransportationManagement`

#### 功能特性 / Features

1. **订单卡片显示 / Order Card Display**
   - 每个订单显示为一个卡片
   - Each order is displayed as a card
   - 显示订单基本信息：单号、地址、联系人、重量等
   - Shows basic order information: number, addresses, contact, weight, etc.
   - 显示当前状态和运输阶段
   - Shows current status and transport stage

2. **阶段提示 / Stage Indicator**
   - 运输中的订单会显示当前所在的运输阶段
   - Orders in transit show current transport stage
   - 使用蓝色背景突出显示
   - Highlighted with blue background

3. **动态按钮 / Dynamic Buttons**
   - 根据订单状态和阶段显示相应的操作按钮
   - Appropriate action buttons shown based on order status and stage
   - 按钮按照工作流顺序依次出现
   - Buttons appear in workflow sequence
   - 每个按钮都有相应的图标和文字说明
   - Each button has corresponding icon and text description

4. **状态筛选 / Status Filter**
   - 可按状态筛选订单：全部、待接单、已接单、运输中、已完成
   - Filter orders by status: All, Pending, Accepted, In Transit, Completed

5. **统计信息 / Statistics**
   - 页面顶部显示各状态订单的数量统计
   - Top of page shows count statistics for each status
   - 待接单、运输中、已完成、总计
   - Pending, In Transit, Completed, Total

## 测试指南 / Testing Guide

### 前提条件 / Prerequisites

1. 运行数据库迁移脚本
   Run database migration script
   ```sql
   Database/AddTransportStageColumn.sql
   ```

2. 确保有测试数据
   Ensure test data exists
   - 至少一个回收员账户
   - At least one recycler account
   - 至少一个运输人员账户
   - At least one transporter account
   - 至少一个运输单
   - At least one transport order

### 测试步骤 / Test Steps

1. **登录运输人员账户**
   Login as transporter

2. **访问运输管理页面**
   Navigate to Transportation Management page

3. **接单测试**
   Test order acceptance
   - 找到"待接单"状态的订单
   - Find order with "Pending" status
   - 点击"接单"按钮
   - Click "Accept" button
   - 验证状态变为"已接单"
   - Verify status changes to "Accepted"

4. **确认取货地点测试**
   Test confirm pickup location
   - 在"已接单"状态的订单上点击"确认取货地点"
   - Click "Confirm Pickup Location" on accepted order
   - 验证状态变为"运输中"，阶段为"确认取货地点"
   - Verify status is "In Transit", stage is "Confirm Pickup Location"

5. **到达取货地点测试**
   Test arrive at pickup
   - 点击"到达取货地点"按钮
   - Click "Arrive at Pickup Location" button
   - 验证阶段变为"到达取货地点"
   - Verify stage changes to "Arrive at Pickup"

6. **装货完毕测试**
   Test complete loading
   - 点击"装货完毕"按钮
   - Click "Loading Complete" button
   - 验证阶段变为"装货完毕"
   - Verify stage changes to "Loading Complete"
   - **验证回收员的暂存点物品已清空**
   - **Verify recycler's storage point items are cleared**

7. **确认送货地点测试**
   Test confirm delivery location
   - 点击"确认送货地点"按钮
   - Click "Confirm Delivery Location" button
   - 验证阶段变为"确认送货地点"
   - Verify stage changes to "Confirm Delivery Location"

8. **到达送货地点测试**
   Test arrive at delivery
   - 点击"到达送货地点"按钮
   - Click "Arrive at Delivery Location" button
   - 验证阶段变为"到达送货地点"
   - Verify stage changes to "Arrive at Delivery"

9. **完成运输测试**
   Test complete transport
   - 点击"运输完成"按钮
   - Click "Complete Transport" button
   - 可选择填写实际重量
   - Optionally enter actual weight
   - 验证状态变为"已完成"
   - Verify status changes to "Completed"
   - 验证 TransportStage 字段被清空
   - Verify TransportStage field is cleared

### 验证项 / Verification Items

- [ ] 所有状态转换正常工作
- [ ] All status transitions work correctly
- [ ] 所有时间戳正确记录
- [ ] All timestamps are recorded correctly
- [ ] UI 按钮根据状态正确显示/隐藏
- [ ] UI buttons show/hide correctly based on status
- [ ] 阶段提示正确显示
- [ ] Stage indicator displays correctly
- [ ] 装货完毕时暂存点物品被清空
- [ ] Storage point items are cleared when loading completes
- [ ] 完成运输时可以填写实际重量
- [ ] Actual weight can be entered when completing transport
- [ ] 状态筛选功能正常
- [ ] Status filter works correctly
- [ ] 统计数据准确
- [ ] Statistics are accurate

## 技术实现细节 / Technical Implementation Details

### 事务处理 / Transaction Handling

在某些关键操作中使用了数据库事务以确保数据一致性:
Database transactions are used in critical operations to ensure data consistency:

1. **装货完毕 (CompleteLoading)**
   - 事务包含：更新运输阶段、移动库存状态
   - Transaction includes: update transport stage, move inventory status
   - 如果库存移动失败，整个操作回滚
   - If inventory move fails, entire operation rolls back

### 权限验证 / Permission Validation

所有 API 端点都进行了以下验证:
All API endpoints perform the following validations:

1. 用户已登录且为运输人员角色
   User is logged in and has transporter role

2. 运输单属于当前登录的运输人员
   Transport order belongs to current logged-in transporter

3. 运输单状态符合操作要求
   Transport order status meets operation requirements

4. 运输阶段符合操作要求（如适用）
   Transport stage meets operation requirements (if applicable)

### 安全考虑 / Security Considerations

1. **CSRF 防护**
   - 所有 POST 请求都需要防伪令牌
   - All POST requests require anti-forgery token

2. **权限检查**
   - 严格验证用户权限
   - Strict user permission validation
   - 验证运输单所有权
   - Validate transport order ownership

3. **状态验证**
   - 严格的状态转换验证
   - Strict status transition validation
   - 防止跳过中间步骤
   - Prevent skipping intermediate steps

4. **输入验证**
   - 验证所有输入参数
   - Validate all input parameters
   - 防止 SQL 注入
   - Prevent SQL injection

## 向后兼容性 / Backward Compatibility

### 已保留方法 / Preserved Methods

`StartTransportation` 方法在 BLL 和 DAL 层中被标记为 DEPRECATED 但仍然保留，以确保向后兼容性。
The `StartTransportation` method is marked as DEPRECATED in BLL and DAL layers but remains for backward compatibility.

新实现使用 `ConfirmPickupLocation` 方法代替。
New implementation uses `ConfirmPickupLocation` method instead.

### 数据迁移 / Data Migration

- 新增字段都设置为 NULL 可选
- All new fields are set as NULL (optional)
- 现有数据不受影响
- Existing data is not affected
- 系统可以处理没有 TransportStage 的历史订单
- System can handle historical orders without TransportStage

## 故障排除 / Troubleshooting

### 常见问题 / Common Issues

1. **按钮不显示**
   Buttons not showing
   - 检查订单状态和阶段
   - Check order status and stage
   - 刷新页面
   - Refresh page
   - 检查浏览器控制台错误
   - Check browser console for errors

2. **状态转换失败**
   Status transition fails
   - 验证当前状态和阶段是否正确
   - Verify current status and stage are correct
   - 检查后端日志
   - Check backend logs
   - 确认数据库字段已添加
   - Confirm database fields are added

3. **暂存点未清空**
   Storage point not cleared
   - 检查装货完毕操作是否成功
   - Check if loading completion was successful
   - 检查库存表的 InventoryType 字段
   - Check InventoryType field in Inventory table
   - 查看后端日志了解详情
   - Check backend logs for details

## 文件清单 / File List

### 修改的文件 / Modified Files

1. `recycling.Model/TransportationOrders.cs`
   - 添加新属性
   - Added new properties

2. `recycling.DAL/TransportationOrderDAL.cs`
   - 添加新方法处理各个阶段
   - Added new methods for each stage
   - 更新查询以包含新字段
   - Updated queries to include new fields

3. `recycling.BLL/TransportationOrderBLL.cs`
   - 添加新业务逻辑方法
   - Added new business logic methods

4. `recycling.Web.UI/Controllers/StaffController.cs`
   - 添加新 API 端点
   - Added new API endpoints
   - 更新响应数据包含 TransportStage
   - Updated response data to include TransportStage

5. `recycling.Web.UI/Views/Staff/TransportationManagement.cshtml`
   - 更新 UI 以显示阶段和动态按钮
   - Updated UI to show stage and dynamic buttons
   - 添加新的 JavaScript 函数
   - Added new JavaScript functions

### 新增的文件 / New Files

1. `Database/AddTransportStageColumn.sql`
   - 数据库迁移脚本
   - Database migration script

2. `TRANSPORTATION_WORKFLOW_IMPLEMENTATION.md`
   - 本文档
   - This document

## 总结 / Summary

本次实现完成了运输管理系统的详细工作流程，将原来简单的"开始运输-完成运输"两步流程扩展为包含多个中间步骤的完整流程。这使得系统能够更准确地跟踪运输过程，提供更好的可见性和控制。

This implementation completes a detailed transportation management workflow, expanding the original simple "start-complete" two-step process into a comprehensive workflow with multiple intermediate steps. This enables the system to track the transportation process more accurately, providing better visibility and control.

主要改进包括:
Main improvements include:

- 详细的运输阶段跟踪
- Detailed transport stage tracking
- 每个阶段的时间戳记录
- Timestamp recording for each stage
- 灵活的 UI 根据当前阶段显示相应操作
- Flexible UI showing appropriate actions based on current stage
- 与库存系统的集成（装货完毕时清空暂存点）
- Integration with inventory system (clear storage point when loading completes)
- 完整的权限和状态验证
- Complete permission and status validation
