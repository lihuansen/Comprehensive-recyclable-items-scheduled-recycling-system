# 运输单功能实现文档
# Transportation Order Feature Implementation Documentation

## 概述 (Overview)

本文档描述了在"暂存点管理"模块中新增的运输单功能。该功能允许回收员在暂存点积累物品后，选择可用的运输人员，并创建运输单将物品运输到基地。

This document describes the newly added transportation order feature in the "Storage Point Management" module. This feature allows recyclers to select available transporters and create transportation orders to transport items from storage points to the base.

## 功能描述 (Feature Description)

### 业务流程 (Business Flow)

1. **查看暂存点库存**: 回收员登录后进入暂存点管理页面，查看当前库存的物品类别、重量和价值统计。
2. **联系运输人员**: 点击"联系运输人员"按钮，系统会显示当前区域内所有可用的运输人员列表。
3. **选择运输人员**: 浏览运输人员列表，可以查看每个运输人员的基本信息（姓名、电话、车辆信息、评分等），选择合适的运输人员。
4. **填写运输单**: 选择运输人员后，填写运输单信息：
   - 取货地址（暂存点地址）
   - 目的地地址（基地地址）
   - 联系人姓名
   - 联系电话
   - 预估总重量
   - 物品类别信息
   - 特殊说明
5. **提交运输单**: 提交后生成运输单，状态为"待接单"。

### 运输人员筛选条件 (Transporter Filter Criteria)

系统自动筛选符合以下条件的运输人员：
- 与回收员在同一区域 (Same region as recycler)
- 账号处于激活状态 (`IsActive = 1`)
- 当前状态为可接单 (`Available = 1`, `CurrentStatus = '空闲'`)

### 运输单状态流转 (Transportation Order Status Flow)

运输单的状态流转如下：

```
待接单 → 已接单 → 运输中 → 已完成
         ↓
      已取消（可在任何状态下取消）
```

- **待接单**: 运输单创建后的初始状态，等待运输人员接单
- **已接单**: 运输人员确认接单
- **运输中**: 运输人员开始运输
- **已完成**: 运输完成，物品送达基地
- **已取消**: 运输单被取消（需填写取消原因）

## 数据库表结构 (Database Schema)

### TransportationOrders 表

运输单表包含以下字段：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| TransportOrderID | INT | 主键，自增 |
| OrderNumber | NVARCHAR(50) | 运输单号，格式：TO+YYYYMMDD+序号 |
| RecyclerID | INT | 回收员ID（外键） |
| TransporterID | INT | 运输人员ID（外键） |
| PickupAddress | NVARCHAR(200) | 取货地址 |
| DestinationAddress | NVARCHAR(200) | 目的地地址 |
| ContactPerson | NVARCHAR(50) | 联系人 |
| ContactPhone | NVARCHAR(20) | 联系电话 |
| EstimatedWeight | DECIMAL(10,2) | 预估总重量（kg） |
| ActualWeight | DECIMAL(10,2) | 实际重量（kg） |
| ItemCategories | NVARCHAR(MAX) | 物品类别（JSON格式） |
| SpecialInstructions | NVARCHAR(500) | 特殊说明 |
| Status | NVARCHAR(20) | 状态 |
| CreatedDate | DATETIME2 | 创建时间 |
| AcceptedDate | DATETIME2 | 接单时间 |
| PickupDate | DATETIME2 | 取货时间 |
| DeliveryDate | DATETIME2 | 送达时间 |
| CompletedDate | DATETIME2 | 完成时间 |
| CancelledDate | DATETIME2 | 取消时间 |
| CancelReason | NVARCHAR(200) | 取消原因 |
| TransporterNotes | NVARCHAR(500) | 运输人员备注 |
| RecyclerRating | INT | 回收员评分（1-5） |
| RecyclerReview | NVARCHAR(500) | 回收员评价 |

### 创建表的SQL脚本位置

SQL脚本文件位于：`Database/CreateTransportationOrdersTable.sql`

## 代码结构 (Code Structure)

### 1. Model层 (Model Layer)

**文件**: `recycling.Model/TransportationOrders.cs`

定义了运输单的实体类，包含所有字段的属性定义和数据注解。

### 2. DAL层 (Data Access Layer)

**文件**: `recycling.DAL/TransportationOrderDAL.cs`

实现了运输单的数据访问方法：
- `CreateTransportationOrder()`: 创建运输单
- `GetTransportationOrdersByRecycler()`: 获取回收员的运输单列表
- `GetTransportationOrderById()`: 获取运输单详情
- `UpdateTransportationOrderStatus()`: 更新运输单状态
- `GenerateOrderNumber()`: 生成运输单号

### 3. BLL层 (Business Logic Layer)

**文件**: `recycling.BLL/TransportationOrderBLL.cs`

实现了运输单的业务逻辑，包括：
- 数据验证
- 调用DAL层方法
- 异常处理

### 4. Controller层 (Controller Layer)

**文件**: `recycling.Web.UI/Controllers/StaffController.cs`

新增了以下API端点：
- `GetAvailableTransporters()`: 获取可用的运输人员列表（已存在）
- `CreateTransportationOrder()`: 创建运输单

### 5. View层 (View Layer)

**文件**: `recycling.Web.UI/Views/Staff/StoragePointManagement.cshtml`

更新了暂存点管理页面，新增：
- 运输人员选择模态框
- 运输单创建表单模态框
- 相关JavaScript函数

## API接口说明 (API Documentation)

### 1. 获取可用运输人员

**URL**: `/Staff/GetAvailableTransporters`

**Method**: `POST`

**Headers**: 
- `__RequestVerificationToken`: Anti-forgery token

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "transporterId": 1,
      "username": "transporter001",
      "fullName": "张三",
      "phoneNumber": "13800138001",
      "region": "罗湖区",
      "vehicleType": "小型货车",
      "vehiclePlateNumber": "粤B12345",
      "vehicleCapacity": 500.00,
      "currentStatus": "空闲",
      "available": true,
      "rating": 4.5
    }
  ]
}
```

### 2. 创建运输单

**URL**: `/Staff/CreateTransportationOrder`

**Method**: `POST`

**Headers**: 
- `__RequestVerificationToken`: Anti-forgery token

**Parameters**:
- `transporterId` (int): 运输人员ID
- `pickupAddress` (string): 取货地址
- `destinationAddress` (string): 目的地地址
- `contactPerson` (string): 联系人
- `contactPhone` (string): 联系电话
- `estimatedWeight` (decimal): 预估总重量
- `itemCategories` (string): 物品类别信息
- `specialInstructions` (string): 特殊说明

**Response**:
```json
{
  "success": true,
  "message": "运输单创建成功",
  "orderId": 1
}
```

## 使用指南 (User Guide)

### 前置条件 (Prerequisites)

1. 确保数据库已创建 `TransportationOrders` 表
   ```sql
   -- 执行脚本
   USE RecyclingDB;
   GO
   -- 运行 Database/CreateTransportationOrdersTable.sql 中的SQL脚本
   ```

2. 确保系统中已有运输人员数据
   ```sql
   -- 可以使用 Database/CreateTransportationOrdersTable.sql 中的示例数据
   ```

### 操作步骤 (Operation Steps)

1. **登录系统**: 使用回收员账号登录系统

2. **进入暂存点管理**: 在导航菜单中选择"暂存点管理"

3. **查看库存**: 页面会显示当前暂存点的库存汇总信息

4. **联系运输人员**: 
   - 点击"联系运输人员"按钮
   - 系统弹出运输人员列表对话框
   - 查看可用的运输人员信息

5. **选择运输人员**:
   - 点击某个运输人员卡片上的"选择此人"按钮
   - 系统关闭运输人员列表，打开运输单创建表单

6. **填写运输单信息**:
   - 取货地址：填写暂存点的详细地址
   - 目的地地址：填写基地或分拣中心的地址
   - 联系人：填写回收员或联系人姓名
   - 联系电话：填写联系电话
   - 预估总重量：输入本次运输的总重量（单位：公斤）
   - 物品类别：填写物品类别和重量信息（例如：纸类 20kg, 塑料 15kg）
   - 特殊说明：填写任何特殊要求或备注

7. **提交运输单**:
   - 检查信息是否正确
   - 点击"创建运输单"按钮
   - 系统提示创建成功

## 测试说明 (Testing Notes)

### 手动测试步骤

1. **准备测试数据**:
   ```sql
   -- 确保有测试用的回收员
   SELECT * FROM Recyclers WHERE IsActive = 1;
   
   -- 确保有测试用的运输人员
   SELECT * FROM Transporters WHERE IsActive = 1 AND Available = 1;
   ```

2. **测试创建运输单**:
   - 登录回收员账号
   - 进入暂存点管理页面
   - 点击"联系运输人员"
   - 验证运输人员列表正确显示
   - 选择一个运输人员
   - 填写完整的运输单信息
   - 提交并验证运输单创建成功

3. **验证数据库**:
   ```sql
   -- 查看创建的运输单
   SELECT * FROM TransportationOrders 
   ORDER BY CreatedDate DESC;
   ```

### 预期结果

- 运输人员列表只显示与回收员同区域且状态为空闲的运输人员
- 运输单创建成功后，数据库中新增一条记录
- 运输单号格式正确（TO+日期+序号）
- 运输单状态为"待接单"

## 注意事项 (Important Notes)

1. **权限控制**: 只有登录的回收员可以访问此功能
2. **区域限制**: 只能选择同区域的运输人员
3. **表单验证**: 必填字段不能为空，重量必须大于0
4. **数据一致性**: 使用事务确保数据完整性
5. **错误处理**: 所有异常都会记录到日志并返回友好的错误消息

## 未来扩展 (Future Enhancements)

1. **运输单追踪**: 添加实时追踪功能，显示运输状态
2. **推送通知**: 运输单状态变化时推送通知给回收员
3. **运输历史**: 添加运输历史记录查询功能
4. **数据统计**: 添加运输统计报表
5. **评价系统**: 完善评价功能，允许回收员对运输服务评分

## 故障排查 (Troubleshooting)

### 问题：运输人员列表为空

**原因**: 
- 当前区域没有可用的运输人员
- 运输人员账号未激活
- 运输人员当前状态不是"空闲"

**解决方法**:
```sql
-- 检查运输人员状态
SELECT TransporterID, Username, FullName, Region, Available, CurrentStatus, IsActive
FROM Transporters
WHERE Region = '你的区域名称';

-- 更新运输人员状态为可用
UPDATE Transporters 
SET Available = 1, CurrentStatus = N'空闲', IsActive = 1
WHERE TransporterID = 你的运输人员ID;
```

### 问题：创建运输单失败

**原因**: 
- 必填字段缺失
- 数据类型不匹配
- 外键约束失败

**解决方法**:
1. 检查浏览器控制台的错误信息
2. 检查服务器日志
3. 验证回收员ID和运输人员ID是否存在

## 联系方式 (Contact)

如有问题或建议，请联系开发团队。

---

**版本**: 1.0  
**创建日期**: 2026-01-03  
**最后更新**: 2026-01-03
