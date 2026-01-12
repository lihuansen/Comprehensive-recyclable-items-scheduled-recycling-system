# 运输阶段属性修复文档 / Transport Stage Property Fix Documentation

## 问题描述 / Problem Description

用户添加了一个新的属性 `Stage` (阶段) 到 `TransportationOrders` 模型中，但系统出现了编译错误，提示以下属性未定义：
- `TransportStage`
- `PickupConfirmedDate`
- `ArrivedAtPickupDate`
- `LoadingCompletedDate`
- `DeliveryConfirmedDate`
- `ArrivedAtDeliveryDate`

The user added a new property `Stage` to the `TransportationOrders` model, but the system encountered compilation errors indicating that the following properties were not defined:
- `TransportStage`
- `PickupConfirmedDate`
- `ArrivedAtPickupDate`
- `LoadingCompletedDate`
- `DeliveryConfirmedDate`
- `ArrivedAtDeliveryDate`

## 根本原因 / Root Cause

DAL (数据访问层) 代码尝试访问模型中不存在的属性。数据库表结构和 DAL 代码使用的是 `TransportStage` 而不是 `Stage`，并且需要额外的日期时间属性来跟踪运输的各个阶段。

The DAL (Data Access Layer) code was attempting to access properties that didn't exist in the model. The database table structure and DAL code use `TransportStage` instead of `Stage`, and require additional datetime properties to track various stages of transportation.

## 解决方案 / Solution

### 1. 模型修复 / Model Fix

在 `recycling.Model/TransportationOrders.cs` 中：
- 将 `Stage` 属性替换为 `TransportStage` (匹配数据库列名)
- 添加了缺失的日期时间属性

In `recycling.Model/TransportationOrders.cs`:
- Replaced `Stage` property with `TransportStage` (to match database column name)
- Added missing datetime properties

```csharp
[StringLength(50)]
public string TransportStage { get; set; }

[Column(TypeName = "datetime2")]
public DateTime? PickupConfirmedDate { get; set; }

[Column(TypeName = "datetime2")]
public DateTime? ArrivedAtPickupDate { get; set; }

[Column(TypeName = "datetime2")]
public DateTime? LoadingCompletedDate { get; set; }

[Column(TypeName = "datetime2")]
public DateTime? DeliveryConfirmedDate { get; set; }

[Column(TypeName = "datetime2")]
public DateTime? ArrivedAtDeliveryDate { get; set; }
```

## 运输工作流程 / Transportation Workflow

### 状态转换 / Status Transitions

系统实现了以下状态转换逻辑：

1. **待接单 (Pending)**
   - 初始状态，当运输订单被创建时
   - 运输人员可以看到此订单并选择接单
   - Initial state when transport order is created
   - Transporter can see and accept the order

2. **已接单 (Accepted)**
   - 运输人员点击"接单"按钮后
   - 状态从"待接单"变为"已接单"
   - After transporter clicks "Accept Order" button
   - Status changes from "Pending" to "Accepted"

3. **运输中 (In Transit)**
   - 运输人员点击"确认收货地点"后进入此状态
   - 在此状态下，通过 `TransportStage` 属性跟踪详细的运输阶段
   - After transporter clicks "Confirm Pickup Location"
   - Detailed transport stages are tracked via `TransportStage` property

4. **已完成 (Completed)**
   - 运输人员点击"运输完成"后
   - 标志着整个运输流程的结束
   - After transporter clicks "Complete Transportation"
   - Marks the end of the entire transportation process

### 运输阶段详细流程 / Detailed Transport Stages

在"运输中"状态下，系统通过以下阶段按钮顺序跟踪进度：

During "In Transit" status, the system tracks progress through the following stage buttons in order:

#### 1. 确认收货地点 / Confirm Pickup Location
- **触发条件**: 订单状态为"已接单"
- **操作**: 点击"确认收货地点"按钮
- **结果**: 
  - 状态变为"运输中"
  - `TransportStage` 设置为 "确认取货地点"
  - `PickupConfirmedDate` 记录当前时间
- **Trigger**: Order status is "Accepted"
- **Action**: Click "Confirm Pickup Location" button
- **Result**: 
  - Status changes to "In Transit"
  - `TransportStage` set to "确认取货地点"
  - `PickupConfirmedDate` records current time

#### 2. 到达收货地点 / Arrive at Pickup Location
- **触发条件**: `TransportStage` 为 "确认取货地点"
- **操作**: 点击"到达收货地点"按钮
- **结果**: 
  - `TransportStage` 更新为 "到达取货地点"
  - `ArrivedAtPickupDate` 记录当前时间
- **Trigger**: `TransportStage` is "确认取货地点"
- **Action**: Click "Arrive at Pickup" button
- **Result**: 
  - `TransportStage` updates to "到达取货地点"
  - `ArrivedAtPickupDate` records current time

#### 3. 装收货物完毕 / Loading Completed
- **触发条件**: `TransportStage` 为 "到达取货地点"
- **操作**: 点击"装货完毕"按钮
- **结果**: 
  - `TransportStage` 更新为 "装货完毕"
  - `LoadingCompletedDate` 记录当前时间
  - 系统将库存从暂存点转移到运输中状态
- **Trigger**: `TransportStage` is "到达取货地点"
- **Action**: Click "Loading Completed" button
- **Result**: 
  - `TransportStage` updates to "装货完毕"
  - `LoadingCompletedDate` records current time
  - System moves inventory from storage point to in-transit state

#### 4. 确认送货地点 / Confirm Delivery Location
- **触发条件**: `TransportStage` 为 "装货完毕"
- **操作**: 点击"确认送货地点"按钮
- **结果**: 
  - `TransportStage` 更新为 "确认送货地点"
  - `DeliveryConfirmedDate` 记录当前时间
- **Trigger**: `TransportStage` is "装货完毕"
- **Action**: Click "Confirm Delivery Location" button
- **Result**: 
  - `TransportStage` updates to "确认送货地点"
  - `DeliveryConfirmedDate` records current time

#### 5. 到达送货地点 / Arrive at Delivery Location
- **触发条件**: `TransportStage` 为 "确认送货地点"
- **操作**: 点击"到达送货地点"按钮
- **结果**: 
  - `TransportStage` 更新为 "到达送货地点"
  - `ArrivedAtDeliveryDate` 记录当前时间
- **Trigger**: `TransportStage` is "确认送货地点"
- **Action**: Click "Arrive at Delivery" button
- **Result**: 
  - `TransportStage` updates to "到达送货地点"
  - `ArrivedAtDeliveryDate` records current time

#### 6. 运输完成 / Complete Transportation
- **触发条件**: `TransportStage` 为 "到达送货地点"
- **操作**: 点击"运输完成"按钮
- **结果**: 
  - 状态变为"已完成"
  - `TransportStage` 清空 (设置为 NULL)
  - `DeliveryDate` 和 `CompletedDate` 记录当前时间
  - 可选择填写实际重量
- **Trigger**: `TransportStage` is "到达送货地点"
- **Action**: Click "Complete Transportation" button
- **Result**: 
  - Status changes to "Completed"
  - `TransportStage` cleared (set to NULL)
  - `DeliveryDate` and `CompletedDate` record current time
  - Optional actual weight can be entered

## 工作流程图 / Workflow Diagram

```
待接单 (Pending)
    ↓ [接单 / Accept Order]
已接单 (Accepted)
    ↓ [确认收货地点 / Confirm Pickup Location]
运输中 (In Transit)
    ├─ Stage: 确认取货地点 (Confirm Pickup Location)
    ↓  [到达收货地点 / Arrive at Pickup]
    ├─ Stage: 到达取货地点 (Arrive at Pickup)
    ↓  [装货完毕 / Loading Completed]
    ├─ Stage: 装货完毕 (Loading Completed)
    ↓  [确认送货地点 / Confirm Delivery Location]
    ├─ Stage: 确认送货地点 (Confirm Delivery Location)
    ↓  [到达送货地点 / Arrive at Delivery]
    ├─ Stage: 到达送货地点 (Arrive at Delivery)
    ↓  [运输完成 / Complete Transportation]
已完成 (Completed)
```

## 业务规则 / Business Rules

1. **顺序执行**: 运输阶段必须按照既定顺序执行，不能跳过任何阶段
   - Sequential execution: Transport stages must be executed in the predefined order, cannot skip stages

2. **状态验证**: 每个操作都会验证当前订单状态是否符合要求
   - Status validation: Each operation validates if current order status meets requirements

3. **权限控制**: 只有分配给该运输单的运输人员才能操作
   - Permission control: Only the assigned transporter can operate on the order

4. **时间记录**: 每个阶段完成时都会自动记录时间戳
   - Time recording: Timestamp is automatically recorded when each stage completes

5. **库存管理**: 装货完毕时，系统自动将库存从暂存点转移到运输中状态
   - Inventory management: When loading completes, system automatically moves inventory from storage point to in-transit status

## 数据库架构 / Database Schema

确保数据库包含以下列（可运行 `Database/EnsureTransportStageColumns.sql` 脚本）：

Ensure database contains the following columns (run `Database/EnsureTransportStageColumns.sql` script):

- `TransportStage` NVARCHAR(50) NULL
- `PickupConfirmedDate` DATETIME2 NULL
- `ArrivedAtPickupDate` DATETIME2 NULL
- `LoadingCompletedDate` DATETIME2 NULL
- `DeliveryConfirmedDate` DATETIME2 NULL
- `ArrivedAtDeliveryDate` DATETIME2 NULL

## 相关文件 / Related Files

### 模型层 / Model Layer
- `recycling.Model/TransportationOrders.cs` - 数据模型 / Data model

### 数据访问层 / Data Access Layer
- `recycling.DAL/TransportationOrderDAL.cs` - 数据访问方法 / Data access methods

### 业务逻辑层 / Business Logic Layer
- `recycling.BLL/TransportationOrderBLL.cs` - 业务逻辑方法 / Business logic methods

### 控制器 / Controller
- `recycling.Web.UI/Controllers/StaffController.cs` - 运输管理控制器 / Transport management controller

### 视图 / View
- `recycling.Web.UI/Views/Staff/TransportationManagement.cshtml` - 运输管理界面 / Transport management UI

### 数据库脚本 / Database Scripts
- `Database/EnsureTransportStageColumns.sql` - 确保数据库列存在 / Ensure database columns exist
- `Database/AddTransportStageColumn.sql` - 添加运输阶段列 / Add transport stage columns

## 测试建议 / Testing Recommendations

### 功能测试 / Functional Testing
1. 以运输人员身份登录系统
2. 查看运输管理页面，确认能看到待接单的订单
3. 按顺序点击每个阶段按钮，验证：
   - 按钮显示正确
   - 状态转换正确
   - 时间戳正确记录
   - UI 实时更新

### 边界测试 / Boundary Testing
1. 尝试跳过阶段（应该失败）
2. 尝试重复点击同一阶段（应该失败）
3. 尝试操作其他运输人员的订单（应该失败）

### 性能测试 / Performance Testing
1. 创建多个运输订单
2. 同时操作多个订单
3. 验证系统响应时间

## 安全考虑 / Security Considerations

- ✅ 所有操作都需要身份验证
- ✅ 运输人员只能操作分配给自己的订单
- ✅ 状态转换有严格的前置条件验证
- ✅ 使用参数化查询防止 SQL 注入
- ✅ 使用防伪令牌防止 CSRF 攻击

- ✅ All operations require authentication
- ✅ Transporters can only operate on their assigned orders
- ✅ Status transitions have strict precondition validation
- ✅ Parameterized queries prevent SQL injection
- ✅ Anti-forgery tokens prevent CSRF attacks

## 维护说明 / Maintenance Notes

### 添加新阶段 / Adding New Stages
如果需要添加新的运输阶段：
1. 在数据库中添加对应的日期时间列
2. 更新 `TransportationOrders` 模型添加新属性
3. 在 DAL 中添加对应的方法
4. 在 BLL 中添加业务逻辑验证
5. 在控制器中添加新的 Action 方法
6. 在视图中添加新的按钮和处理逻辑
7. 更新数据库约束检查

If you need to add new transport stages:
1. Add corresponding datetime column in database
2. Update `TransportationOrders` model to add new property
3. Add corresponding method in DAL
4. Add business logic validation in BLL
5. Add new Action method in controller
6. Add new button and handling logic in view
7. Update database constraint checks

### 版本兼容性 / Version Compatibility
代码实现了向后兼容性：
- 检查列是否存在再进行操作
- 对于没有 `TransportStage` 列的旧数据库，系统仍能正常工作
- 使用动态 SQL 构建查询

The code implements backward compatibility:
- Checks if columns exist before operations
- For old databases without `TransportStage` column, system still works
- Uses dynamic SQL to build queries

## 总结 / Summary

此修复解决了模型属性与数据库架构不匹配的问题，确保了：
1. 编译错误已解决
2. 运输工作流程完整且按正确顺序执行
3. 状态和阶段转换符合业务需求
4. 代码与数据库架构保持一致

This fix resolves the mismatch between model properties and database schema, ensuring:
1. Compilation errors are resolved
2. Transport workflow is complete and executes in correct order
3. Status and stage transitions meet business requirements
4. Code is consistent with database schema

---

**修复日期 / Fix Date**: 2026-01-12  
**修复人员 / Fixed By**: GitHub Copilot Agent
