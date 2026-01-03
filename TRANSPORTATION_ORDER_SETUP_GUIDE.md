# 运输单功能设置指南
# Transportation Order Feature Setup Guide

## 问题说明 / Problem Description

原问题：点击回收员端中的暂存点管理中的"联系运输人员"按钮后，系统显示错误：
```
System.NullReferenceException: "未将对象引用设置到对象的实例。"
System.Configuration.ConnectionStringSettingsCollection.this[string].get 返回 null。
```

**根本原因**：`StaffController.GetAvailableTransporters` 方法中使用了错误的连接字符串名称 `"RecyclingDBConnectionString"`，而实际配置文件中的连接字符串名称是 `"RecyclingDB"`。

## 已完成的修复 / Completed Fixes

### 1. 代码修复
- ✅ 修复了 `GetAvailableTransporters` 方法中的连接字符串名称
- ✅ 确认了 `CreateTransportationOrder` 方法的实现
- ✅ 验证了完整的 Model-DAL-BLL 架构

### 2. 已有的数据库表结构
运输单表（TransportationOrders）已定义，包含以下字段：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| TransportOrderID | INT | 运输单ID（主键，自增） |
| OrderNumber | NVARCHAR(50) | 运输单号（格式：TO+日期+序号） |
| RecyclerID | INT | 回收员ID（外键） |
| TransporterID | INT | 运输人员ID（外键） |
| PickupAddress | NVARCHAR(200) | 取货地址 |
| DestinationAddress | NVARCHAR(200) | 目的地地址 |
| ContactPerson | NVARCHAR(50) | 联系人 |
| ContactPhone | NVARCHAR(20) | 联系电话 |
| EstimatedWeight | DECIMAL(10,2) | 预估重量(kg) |
| ActualWeight | DECIMAL(10,2) | 实际重量(kg) |
| ItemCategories | NVARCHAR(MAX) | 物品类别（JSON格式） |
| SpecialInstructions | NVARCHAR(500) | 特殊说明 |
| Status | NVARCHAR(20) | 状态（待接单/已接单/运输中/已完成/已取消） |
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

## 数据库设置步骤 / Database Setup Steps

### 步骤 1：执行建表脚本

运行以下 SQL 脚本创建 TransportationOrders 表：

```bash
# 使用 SQL Server Management Studio (SSMS)
# 打开文件：Database/CreateTransportationOrdersTable.sql
# 连接到你的数据库
# 执行脚本
```

或者使用命令行：

```bash
sqlcmd -S 服务器名称 -d RecyclingSystemDB -i Database/CreateTransportationOrdersTable.sql
```

### 步骤 2：验证表创建

执行以下 SQL 验证表是否创建成功：

```sql
-- 检查表是否存在
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'TransportationOrders';

-- 检查表结构
EXEC sp_help 'TransportationOrders';

-- 检查索引
EXEC sp_helpindex 'TransportationOrders';
```

### 步骤 3：验证外键关系

确保 Recyclers 和 Transporters 表已存在：

```sql
-- 检查 Recyclers 表
SELECT COUNT(*) as RecyclerCount FROM Recyclers;

-- 检查 Transporters 表
SELECT COUNT(*) as TransporterCount FROM Transporters;
```

## 功能使用说明 / Feature Usage Guide

### 工作流程

1. **回收员登录系统**
   - 进入"暂存点管理"页面

2. **查看库存**
   - 系统自动显示已完成订单的物品库存汇总
   - 按物品类别显示总重量和总价值

3. **联系运输人员**
   - 点击"联系运输人员"按钮
   - 系统显示同一区域的可用运输人员列表
   - 运输人员信息包括：
     - 姓名、电话
     - 车辆类型、车牌号
     - 载重能力、评分

4. **选择运输人员**
   - 点击"选择此人"按钮
   - 系统显示创建运输单表单

5. **填写运输单**
   表单字段说明：
   - **运输人员**：自动填充（已选择的运输人员）
   - **取货地址**：手动填写（回收员暂存点地址）
   - **目的地地址**：手动填写（基地或分拣中心地址）
   - **联系人**：手动填写（回收员姓名）
   - **联系电话**：手动填写（回收员电话）
   - **预估总重量**：手动填写（单位：kg）
   - **物品类别**：选填（例如：纸类 20kg, 塑料 15kg）
   - **特殊说明**：选填（备注信息）

6. **提交运输单**
   - 点击"创建运输单"按钮
   - 系统自动生成运输单号（格式：TO+年月日+序号）
   - 运输单状态默认为"待接单"
   - 显示创建成功消息

### 运输单状态流转

```
待接单 → 已接单 → 运输中 → 已完成
   ↓         ↓        ↓
       已取消（任何状态都可取消）
```

### 示例数据

```json
{
  "orderNumber": "TO202601030001",
  "recyclerId": 1,
  "transporterId": 3,
  "pickupAddress": "深圳市罗湖区XX街道XX号",
  "destinationAddress": "深圳市罗湖区分拣中心",
  "contactPerson": "张三",
  "contactPhone": "13800138000",
  "estimatedWeight": 45.5,
  "itemCategories": "纸类 20kg, 塑料 15kg, 金属 10.5kg",
  "specialInstructions": "请在下午3点后取货",
  "status": "待接单"
}
```

## 测试验证 / Testing & Verification

### 1. 测试获取可用运输人员

访问暂存点管理页面，点击"联系运输人员"按钮，应该：
- ✓ 不再显示 NullReferenceException 错误
- ✓ 显示可用运输人员列表（如果有的话）
- ✓ 如果没有可用运输人员，显示友好提示

### 2. 测试创建运输单

选择一个运输人员，填写运输单表单，提交后应该：
- ✓ 显示"运输单创建成功"消息
- ✓ 显示生成的运输单号
- ✓ 数据正确保存到数据库

验证 SQL：
```sql
-- 查看最新创建的运输单
SELECT TOP 10 * FROM TransportationOrders 
ORDER BY CreatedDate DESC;

-- 查看特定回收员的运输单
SELECT * FROM TransportationOrders 
WHERE RecyclerID = 你的回收员ID
ORDER BY CreatedDate DESC;
```

### 3. 测试数据完整性

```sql
-- 检查运输单与回收员的关联
SELECT t.*, r.FullName as RecyclerName
FROM TransportationOrders t
JOIN Recyclers r ON t.RecyclerID = r.RecyclerID;

-- 检查运输单与运输人员的关联
SELECT t.*, tp.FullName as TransporterName
FROM TransportationOrders t
JOIN Transporters tp ON t.TransporterID = tp.TransporterID;
```

## 常见问题 / Troubleshooting

### Q1: 点击"联系运输人员"仍然报错？
**A**: 检查以下几点：
1. 确认已执行本次代码修复（连接字符串名称改为 "RecyclingDB"）
2. 确认 Web.config 中连接字符串配置正确
3. 确认数据库服务器正在运行
4. 查看浏览器控制台和服务器日志获取详细错误信息

### Q2: 没有显示可用运输人员？
**A**: 需要确保：
1. Transporters 表中有数据
2. 运输人员的 `IsActive` = 1
3. 运输人员的 `Available` = 1
4. 运输人员的 `Region` 与回收员的 `Region` 相同

添加测试运输人员：
```sql
INSERT INTO Transporters 
(Username, PasswordHash, FullName, PhoneNumber, Region, 
 VehiclePlateNumber, VehicleType, VehicleCapacity, 
 CurrentStatus, Available, IsActive, CreatedDate)
VALUES 
('transporter1', 'hashed_password', '李运输', '13900139000', '罗湖区',
 '粤B12345', '货车', 500.00,
 '空闲', 1, 1, GETDATE());
```

### Q3: 创建运输单失败？
**A**: 检查：
1. TransportationOrders 表是否已创建
2. 外键约束是否正确（Recyclers 和 Transporters 表）
3. 必填字段是否都已填写
4. 预估重量是否大于 0
5. 查看服务器日志获取详细错误信息

### Q4: 如何查看已创建的运输单？
**A**: 当前功能只包含创建运输单，查看功能需要后续开发。临时可以通过 SQL 查询：
```sql
SELECT 
    t.OrderNumber,
    r.FullName as RecyclerName,
    tp.FullName as TransporterName,
    t.Status,
    t.EstimatedWeight,
    t.CreatedDate
FROM TransportationOrders t
LEFT JOIN Recyclers r ON t.RecyclerID = r.RecyclerID
LEFT JOIN Transporters tp ON t.TransporterID = tp.TransporterID
ORDER BY t.CreatedDate DESC;
```

## 后续开发建议 / Future Enhancements

1. **运输单管理页面**
   - 回收员查看自己的运输单列表
   - 运输单详情查看
   - 取消运输单功能

2. **运输人员端功能**
   - 查看待接单列表
   - 接单/拒单功能
   - 更新运输状态
   - 填写实际重量

3. **通知功能**
   - 运输单创建通知运输人员
   - 状态变更通知回收员
   - 短信/站内消息提醒

4. **统计报表**
   - 运输单统计
   - 运输人员工作量统计
   - 运输效率分析

## 技术架构 / Technical Architecture

```
前端 (View)
  └─ StoragePointManagement.cshtml
      ├─ 显示库存汇总
      ├─ 联系运输人员按钮
      └─ 创建运输单表单

控制器 (Controller)
  └─ StaffController.cs
      ├─ GetAvailableTransporters()    # 获取可用运输人员
      └─ CreateTransportationOrder()   # 创建运输单

业务逻辑层 (BLL)
  └─ TransportationOrderBLL.cs
      ├─ CreateTransportationOrder()
      ├─ GetTransportationOrdersByRecycler()
      └─ UpdateTransportationOrderStatus()

数据访问层 (DAL)
  └─ TransportationOrderDAL.cs
      ├─ CreateTransportationOrder()
      ├─ GetTransportationOrdersByRecycler()
      └─ UpdateTransportationOrderStatus()

数据模型 (Model)
  └─ TransportationOrders.cs

数据库 (Database)
  └─ TransportationOrders 表
```

## 联系支持 / Support

如果遇到问题，请提供以下信息：
1. 错误消息的完整文本
2. 浏览器控制台的错误日志
3. 服务器日志（Visual Studio Output 窗口）
4. 数据库表结构验证结果
5. 操作步骤重现

---

**修复日期**: 2025-01-03  
**修复内容**: NullReferenceException 错误 - 连接字符串名称错误  
**影响范围**: StaffController.GetAvailableTransporters 方法  
**测试状态**: 代码修复完成，等待用户验证
