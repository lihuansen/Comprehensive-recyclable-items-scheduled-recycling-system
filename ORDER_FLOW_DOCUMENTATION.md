# 预约回收订单完整流程文档
## Order Flow Complete Documentation

本文档详细描述了预约回收系统从用户预约到最终入库的完整业务流程，以及各个阶段的状态变更。

---

## 1. 完整业务流程概览

### 流程图
```
用户预约 → 生成预约单 → 回收员接单 → 回收员上门收货 → 完成订单 
   ↓           ↓            ↓             ↓              ↓
 填写信息   状态:已预约   状态:进行中    收货检查    状态:已完成 + 暂存点更新
                ↓
           可取消(状态:已取消)

回收员完成订单后 ↓

创建运输单 → 运输员接单 → 开始运输 → 运输员送达基地 → 完成运输
    ↓            ↓          ↓              ↓               ↓
状态:待接单   状态:已接单  状态:运输中    送货到基地    状态:已完成
                           + 暂存点清空

运输完成后 ↓

基地人员创建入库单 → 生成入库记录 → 仓库管理更新
        ↓                  ↓              ↓
    检查验收        入库单状态:已入库   仓库总量更新
```

---

## 2. 详细流程说明

### 2.1 预约订单流程（Appointments表）

#### 状态定义
- **已预约** - 用户提交预约后的初始状态，等待回收员接单
- **进行中** - 回收员接单后，正在进行上门回收
- **已取消** - 用户在回收员接单前取消预约
- **已完成** - 回收员完成上门回收，货物已收集到暂存点

**关键点：预约订单的状态只有以上4种，不存在"已入库"状态！**

#### 2.1.1 用户预约 (Status: 已预约)
**触发条件：** 用户在前端填写预约信息并提交

**数据操作：**
- 在 `Appointments` 表创建记录
- 在 `AppointmentCategories` 表创建品类详情记录
- Status = "已预约"
- RecyclerID = NULL (未分配回收员)

**涉及文件：**
- `AppointmentDAL.cs` - `InsertCompleteAppointment()`

#### 2.1.2 回收员接单 (Status: 进行中)
**触发条件：** 回收员在其管理界面点击接单

**数据操作：**
- 更新 `Appointments.Status` = "进行中"
- 更新 `Appointments.RecyclerID` = 回收员ID
- 更新 `UpdatedDate` 为当前时间

**涉及文件：**
- `RecyclerOrderDAL.cs` - `AcceptOrder()`

#### 2.1.3 用户取消订单 (Status: 已取消)
**触发条件：** 用户在回收员接单前取消预约

**前置条件：**
- Status = "已预约"
- RecyclerID = NULL

**数据操作：**
- 更新 `Appointments.Status` = "已取消"
- 记录取消时间

**涉及文件：**
- `OrderDAL.cs` - `CancelOrder()`

#### 2.1.4 回收员完成订单 (Status: 已完成)
**触发条件：** 回收员上门收货完成，录入实际信息

**数据操作：**
1. 更新 `Appointments.Status` = "已完成"
2. 更新 `UpdatedDate` 为当前时间
3. 写入暂存点数据（通过查询 Appointments 和 AppointmentCategories 计算）

**暂存点管理：**
- 暂存点数据来源于状态为"已完成"的 Appointments 记录
- 通过汇总 `AppointmentCategories` 表得到各类别的重量和价值

**涉及文件：**
- `OrderBLL.cs` - `UpdateOrderStatus()`
- `StoragePointDAL.cs` - `GetStoragePointSummary()`, `GetStoragePointDetail()`

**状态终点：预约订单到此状态后不再变更，保持"已完成"**

---

### 2.2 运输订单流程（TransportationOrders表）

#### 状态定义
- **待接单** - 回收员创建运输单后的初始状态
- **已接单** - 运输员接受运输任务
- **运输中** - 运输员装货完成，开始运输
- **已完成** - 运输员将货物送达基地
- **已取消** - 运输单被取消（特殊情况）

#### 2.2.1 创建运输单 (Status: 待接单)
**触发条件：** 回收员在暂存点管理界面联系运输人员

**前置条件：**
- 必须有"已完成"状态的预约订单
- 暂存点有货物需要运输

**数据操作：**
- 在 `TransportationOrders` 表创建记录
- 生成运输单号（格式：TO+YYYYMMDD+序号）
- Status = "待接单"
- 记录回收员ID、运输员ID
- 记录预估重量（从暂存点汇总）
- 记录物品类别（从暂存点获取，JSON格式）

**涉及文件：**
- `TransportationOrderDAL.cs` - `CreateTransportationOrder()`

#### 2.2.2 运输员接单 (Status: 已接单)
**触发条件：** 运输员在运输管理界面点击接单

**数据操作：**
- 更新 `TransportationOrders.Status` = "已接单"
- 记录 `AcceptedDate` 为当前时间

**涉及文件：**
- `TransportationOrderDAL.cs` - `AcceptTransportationOrder()`

#### 2.2.3 开始运输 (Status: 运输中)
**触发条件：** 运输员到达现场装货完成，点击开始运输

**数据操作：**
1. 更新 `TransportationOrders.Status` = "运输中"
2. 记录 `PickupDate` 为当前时间
3. **清空暂存点**：删除 `Inventory` 表中该回收员的记录
4. 发送通知给基地人员

**关键修复点：**
- ✅ **正确行为**：清空 Inventory 表记录
- ❌ **错误行为（已修复）**：不应该更新 Appointments.Status 为"已入库"

**涉及文件：**
- `TransportationOrderDAL.cs` - `StartTransportation()`
- `TransportationOrderBLL.cs` - `StartTransportation()`
- `StoragePointDAL.cs` - `ClearStoragePointForRecycler()`（已修复）

#### 2.2.4 完成运输 (Status: 已完成)
**触发条件：** 运输员将货物送达基地，基地确认无误

**数据操作：**
- 更新 `TransportationOrders.Status` = "已完成"
- 记录 `DeliveryDate` 和 `CompletedDate` 为当前时间
- 可选：更新 `ActualWeight`（实际运输重量）

**涉及文件：**
- `TransportationOrderDAL.cs` - `CompleteTransportation()`

---

### 2.3 入库单流程（WarehouseReceipts表）

#### 状态定义
- **已入库** - 入库单创建后的唯一状态

**注意：这里的"已入库"是 WarehouseReceipts 表的状态，不是 Appointments 表的状态！**

#### 2.3.1 创建入库单 (Status: 已入库)
**触发条件：** 基地人员检查已完成的运输单，创建入库单

**前置条件：**
- 运输单状态为"已完成"
- 该运输单尚未创建入库单

**数据操作：**
1. 创建 `WarehouseReceipts` 记录
   - 生成入库单号（格式：WR+YYYYMMDD+序号）
   - 关联 TransportOrderID
   - 记录回收员ID（从运输单获取）
   - 记录基地人员ID（当前登录用户）
   - 记录入库重量
   - 记录物品类别（从运输单获取，可修改）
   - Status = "已入库"
2. 不修改 Appointments 表的任何记录
3. 不修改 TransportationOrders 表的状态
4. 发送通知给回收员

**仓库管理更新：**
- 仓库数据来源于 WarehouseReceipts 表
- 通过解析 ItemCategories JSON 字段统计各类别总量
- 计算总重量和总价值显示在数据大屏

**涉及文件：**
- `WarehouseReceiptDAL.cs` - `CreateWarehouseReceipt()`
- `WarehouseReceiptBLL.cs` - `CreateWarehouseReceipt()`
- `StaffController.cs` - `CreateWarehouseReceipt()` (Action)
- `BaseWarehouseManagement.cshtml` - 入库单创建界面

---

## 3. 关键数据表及其状态字段

### 3.1 Appointments（预约订单表）
```sql
AppointmentID    -- 预约单ID
UserID           -- 用户ID
RecyclerID       -- 回收员ID（接单后赋值）
Status           -- 状态：已预约、进行中、已取消、已完成
AppointmentDate  -- 预约日期
CreatedDate      -- 创建时间
UpdatedDate      -- 更新时间
... 其他字段
```

**状态流转：**
- 已预约 → 进行中 → 已完成 ✅
- 已预约 → 已取消 ✅
- ❌ 不存在：已完成 → 已入库（这是错误的，已修复）

### 3.2 AppointmentCategories（预约品类表）
```sql
CategoryID       -- 品类ID
AppointmentID    -- 关联预约单ID
CategoryKey      -- 类别键（如 "paper"）
CategoryName     -- 类别名称（如 "纸类"）
Weight           -- 重量（kg）
CreatedDate      -- 创建时间
```

### 3.3 TransportationOrders（运输订单表）
```sql
TransportOrderID  -- 运输单ID
OrderNumber       -- 运输单号
RecyclerID        -- 回收员ID
TransporterID     -- 运输员ID
Status            -- 状态：待接单、已接单、运输中、已完成
EstimatedWeight   -- 预估重量
ActualWeight      -- 实际重量
ItemCategories    -- 物品类别（JSON格式）
CreatedDate       -- 创建时间
AcceptedDate      -- 接单时间
PickupDate        -- 取货时间
DeliveryDate      -- 送达时间
CompletedDate     -- 完成时间
... 其他字段
```

### 3.4 WarehouseReceipts（入库单表）
```sql
ReceiptID         -- 入库单ID
ReceiptNumber     -- 入库单号
TransportOrderID  -- 关联运输单ID
RecyclerID        -- 回收员ID
WorkerID          -- 基地人员ID
TotalWeight       -- 入库总重量
ItemCategories    -- 物品类别（JSON格式）
Status            -- 状态：已入库（唯一状态）
Notes             -- 备注
CreatedDate       -- 创建时间
CreatedBy         -- 创建人ID
```

### 3.5 Inventory（库存表）
```sql
InventoryID       -- 库存ID
RecyclerID        -- 回收员ID
CategoryKey       -- 类别键
CategoryName      -- 类别名称
Weight            -- 重量
Price             -- 价值
UpdatedDate       -- 更新时间
```

**注意：**
- 这个表用于存储回收员暂存点的实时库存
- 当运输开始时（Status变为"运输中"），清空该回收员的记录
- 不影响 Appointments 表的状态

---

## 4. 重要修复说明

### 4.1 错误行为（已修复）
**文件：** `recycling.DAL/StoragePointDAL.cs`
**方法：** `ClearStoragePointForRecycler()`

**修复前的错误代码：**
```csharp
// ❌ 错误：将预约订单状态改为"已入库"
string sql = @"
    UPDATE Appointments 
    SET Status = N'已入库',
        UpdatedDate = GETDATE()
    WHERE RecyclerID = @RecyclerID 
        AND Status = N'已完成'";
```

**问题：**
1. 预约订单的状态不应该有"已入库"这个状态
2. 预约订单完成后应该保持"已完成"状态
3. "已入库"状态应该只属于 WarehouseReceipts 表

**修复后的正确代码：**
```csharp
// ✅ 正确：只清空库存记录，不改变预约订单状态
string sql = @"
    DELETE FROM Inventory 
    WHERE RecyclerID = @RecyclerID";
```

### 4.2 修复影响

修复前的错误流程：
```
预约完成(已完成) → 创建运输单 → 开始运输 → 预约状态错误变为"已入库" ❌
```

修复后的正确流程：
```
预约完成(已完成) → 创建运输单 → 开始运输 → 预约状态保持"已完成" ✅
                                        → 暂存点库存清空 ✅
```

---

## 5. 数据来源说明

### 5.1 暂存点管理数据来源
- **数据表：** Appointments + AppointmentCategories
- **筛选条件：** Status = '已完成' AND RecyclerID = 当前回收员
- **显示内容：**
  - 各类别总重量和总价值
  - 订单明细列表
  - 总重量、总金额（大屏显示）

### 5.2 仓库管理数据来源
- **数据表：** WarehouseReceipts
- **筛选条件：** Status = '已入库'
- **显示内容：**
  - 各类别总重量和总价值（解析ItemCategories JSON）
  - 入库记录列表（包含回收员信息）
  - 总重量、总金额（大屏显示）

### 5.3 运输中订单数据来源
- **数据表：** TransportationOrders
- **筛选条件：** Status = '运输中'
- **显示内容：**
  - 运输单列表
  - 回收员、运输员信息
  - 预估重量、物品类别

---

## 6. 重要业务规则

### 6.1 状态变更规则
1. **预约订单（Appointments）**
   - 只能从"已预约"变为"进行中"或"已取消"
   - 只能从"进行中"变为"已完成"
   - "已完成"是最终状态，不再变更 ✅
   - 不存在"已入库"状态 ✅

2. **运输订单（TransportationOrders）**
   - 按顺序：待接单 → 已接单 → 运输中 → 已完成
   - 只能顺序流转，不能跳过或回退

3. **入库单（WarehouseReceipts）**
   - 创建后状态固定为"已入库"
   - 不再变更状态

### 6.2 数据一致性规则
1. 一个运输单只能创建一个入库单
2. 入库单创建不影响预约订单状态
3. 入库单创建不影响运输单状态
4. 暂存点清空不影响预约订单状态

### 6.3 权限规则
- **用户：** 可创建预约、可取消未接单的预约、可查看自己的订单
- **回收员：** 可接单、可完成订单、可创建运输单、可管理暂存点
- **运输员：** 可接运输单、可开始运输、可完成运输
- **基地人员：** 可查看运输中订单、可创建入库单、可管理仓库

---

## 7. 前端界面优化

### 7.1 入库单创建表单增强
**文件：** `recycling.Web.UI/Views/Staff/BaseWarehouseManagement.cshtml`

**优化内容：**
1. **自动填充字段（只读）：**
   - 运输单号
   - 回收员姓名
   - 运输人员姓名
   - 取货时间
   - 预估重量

2. **可编辑字段：**
   - 实际入库重量（默认使用预估重量，可修改）
   - 物品类别明细（JSON格式，自动填充，可调整）
   - 入库备注

3. **类别预览区域：**
   - 友好显示物品类别信息
   - 自动解析JSON数据
   - 显示各类别重量和价值
   - 显示总重量

---

## 8. 开发注意事项

### 8.1 添加新功能时
1. 不要修改 Appointments.Status 的可能值
2. 不要在预约订单完成后继续修改其状态
3. 暂存点数据应该从"已完成"的预约订单实时计算
4. 仓库数据应该从"已入库"的入库单实时计算

### 8.2 调试技巧
1. 检查预约订单状态：`SELECT Status, COUNT(*) FROM Appointments GROUP BY Status`
2. 检查暂存点数据：查询 Status='已完成' 的 Appointments
3. 检查仓库数据：查询 Status='已入库' 的 WarehouseReceipts
4. 检查运输流程：查询 TransportationOrders 的状态流转

### 8.3 常见错误
1. ❌ 将预约订单状态设置为"已入库"
2. ❌ 在入库单创建时修改预约订单状态
3. ❌ 在运输开始时修改预约订单状态
4. ❌ 混淆 WarehouseReceipts.Status 和 Appointments.Status

---

## 9. 测试检查清单

### 9.1 预约订单流程测试
- [ ] 用户创建预约，状态为"已预约" ✅
- [ ] 回收员接单，状态变为"进行中" ✅
- [ ] 用户取消未接单的预约，状态变为"已取消" ✅
- [ ] 回收员完成订单，状态变为"已完成" ✅
- [ ] 完成的订单出现在暂存点管理中 ✅
- [ ] **预约订单状态保持"已完成"，不变为"已入库"** ✅

### 9.2 运输流程测试
- [ ] 回收员创建运输单，状态为"待接单" ✅
- [ ] 运输员接单，状态变为"已接单" ✅
- [ ] 运输员开始运输，状态变为"运输中" ✅
- [ ] **暂存点库存被清空（Inventory表）** ✅
- [ ] **预约订单状态仍为"已完成"** ✅
- [ ] 基地收到运输中通知 ✅
- [ ] 运输员完成运输，状态变为"已完成" ✅

### 9.3 入库流程测试
- [ ] 基地人员看到已完成的运输单 ✅
- [ ] 选择运输单，表单自动填充数据 ✅
- [ ] 创建入库单成功，生成入库单号 ✅
- [ ] 入库单状态为"已入库" ✅
- [ ] **预约订单状态仍为"已完成"** ✅
- [ ] 仓库管理数据正确更新 ✅
- [ ] 回收员收到入库完成通知 ✅

### 9.4 数据一致性测试
- [ ] 检查数据库中没有状态为"已入库"的 Appointments 记录 ✅
- [ ] 暂存点数据 = 状态为"已完成"的预约订单汇总 ✅
- [ ] 仓库数据 = 状态为"已入库"的入库单汇总 ✅
- [ ] 一个运输单只能创建一个入库单 ✅

---

## 10. 总结

### 核心要点
1. **预约订单状态终点是"已完成"** - 不会也不应该变为"已入库"
2. **暂存点清空不影响预约状态** - 只删除 Inventory 表记录
3. **入库单有独立的状态字段** - WarehouseReceipts.Status = "已入库"
4. **数据来源清晰分离** - 暂存点从 Appointments 查询，仓库从 WarehouseReceipts 查询

### 关键修复
修复了 `StoragePointDAL.ClearStoragePointForRecycler()` 方法，使其不再错误地更新预约订单状态为"已入库"，确保了业务流程的正确性和数据一致性。

### 优化改进
增强了入库单创建界面，提供更详细的信息展示和更友好的操作体验，所有可自动填充的数据都从运输单自动获取，减少手动输入错误。

---

**文档版本：** 1.0
**最后更新：** 2026-01-07
**维护人员：** 开发团队
