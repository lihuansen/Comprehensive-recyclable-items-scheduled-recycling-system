# 基地管理功能实现指南
# Base Management Feature Implementation Guide

## 概述 | Overview

本文档介绍了为基地人员（SortingCenterWorkers）实现的基地管理功能，包括运输管理和仓库管理两大核心模块。

This document describes the base management functionality implemented for base personnel (SortingCenterWorkers), including two core modules: Transportation Management and Warehouse Management.

## 功能特性 | Features

### 1. 运输管理 (Transportation Management)

**功能描述：**
- 实时查看运输中（运输中状态）的订单
- 显示订单详细信息，包括回收员、运输人员、预估重量等
- 自动刷新功能，每30秒更新一次数据
- 统计显示运输中订单数量

**访问路径：**
```
基地工作台 → 基地管理 → 运输管理
或直接访问：/Staff/BaseTransportationManagement
```

**主要功能：**
- ✅ 查看运输中订单列表
- ✅ 了解预估重量和物品类别
- ✅ 查看运输人员和回收员信息
- ✅ 自动刷新（30秒间隔）

### 2. 仓库管理 (Warehouse Management)

**功能描述：**
- 处理可回收物入库操作
- 创建入库单（WarehouseReceipts）
- 自动清零对应暂存点的重量
- 查看入库记录

**访问路径：**
```
基地工作台 → 基地管理 → 仓库管理
或直接访问：/Staff/BaseWarehouseManagement
```

**主要功能：**
- ✅ 创建入库单
- ✅ 自动清零暂存点重量
- ✅ 查看入库记录
- ✅ 防止重复入库
- ✅ 发送通知给回收员

## 数据库设计 | Database Design

### WarehouseReceipts 表 (入库单表)

```sql
CREATE TABLE [dbo].[WarehouseReceipts] (
    [ReceiptID] INT PRIMARY KEY IDENTITY(1,1),            -- 入库单ID
    [ReceiptNumber] NVARCHAR(50) NOT NULL UNIQUE,         -- 入库单号 (WR+YYYYMMDD+序号)
    [TransportOrderID] INT NOT NULL,                      -- 运输单ID
    [RecyclerID] INT NOT NULL,                            -- 回收员ID
    [WorkerID] INT NOT NULL,                              -- 基地人员ID
    [TotalWeight] DECIMAL(10, 2) NOT NULL,                -- 入库总重量（kg）
    [ItemCategories] NVARCHAR(MAX) NULL,                  -- 物品类别（JSON）
    [Status] NVARCHAR(20) NOT NULL DEFAULT N'已入库',     -- 状态
    [Notes] NVARCHAR(500) NULL,                           -- 备注信息
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),   -- 入库时间
    [CreatedBy] INT NOT NULL,                             -- 创建人
    
    -- 外键约束
    CONSTRAINT FK_WarehouseReceipts_TransportationOrders 
        FOREIGN KEY ([TransportOrderID]) REFERENCES [dbo].[TransportationOrders]([TransportOrderID]),
    CONSTRAINT FK_WarehouseReceipts_Recyclers 
        FOREIGN KEY ([RecyclerID]) REFERENCES [dbo].[Recyclers]([RecyclerID]),
    CONSTRAINT FK_WarehouseReceipts_Workers 
        FOREIGN KEY ([WorkerID]) REFERENCES [dbo].[SortingCenterWorkers]([WorkerID])
);
```

**建表脚本位置：**
- `/Database/CreateWarehouseReceiptsTable.sql`

## 业务流程 | Business Flow

### 入库流程

```
1. 运输人员完成运输，运输单状态变为"已完成"
   ↓
2. 基地人员在运输管理中看到运输中订单
   ↓
3. 基地人员进入仓库管理，选择已完成的运输单
   ↓
4. 填写入库信息（重量、物品类别、备注）
   ↓
5. 系统创建入库单：
   - 生成唯一入库单号 (WR+YYYYMMDD+序号)
   - 记录入库信息
   - 清零对应回收员的暂存点重量（删除Inventory记录）
   - 发送通知给回收员
   ↓
6. 入库完成，记录可查询
```

### 通知流程

**运输中通知：**
```
运输单状态变为"运输中" → 基地人员可在运输管理中查看
```

**入库完成通知：**
```
创建入库单成功 → 系统发送通知给回收员
```

## 技术实现 | Technical Implementation

### 架构层次

```
View Layer (Views/Staff/)
    ├── BaseManagement.cshtml                    # 基地管理主页
    ├── BaseTransportationManagement.cshtml      # 运输管理
    └── BaseWarehouseManagement.cshtml           # 仓库管理
    
Controller Layer (Controllers/StaffController.cs)
    ├── BaseManagement()                         # 主页
    ├── BaseTransportationManagement()           # 运输管理页面
    ├── GetInTransitOrders()                     # 获取运输中订单
    ├── BaseWarehouseManagement()                # 仓库管理页面
    ├── CreateWarehouseReceipt()                 # 创建入库单
    ├── GetWarehouseReceipts()                   # 获取入库记录
    └── CheckWarehouseReceipt()                  # 检查入库单

Business Logic Layer (BLL/)
    └── WarehouseReceiptBLL.cs
        ├── CreateWarehouseReceipt()             # 创建入库单
        ├── GetWarehouseReceipts()               # 获取入库记录
        ├── GetInTransitOrders()                 # 获取运输中订单
        └── HasWarehouseReceipt()                # 检查是否已入库

Data Access Layer (DAL/)
    └── WarehouseReceiptDAL.cs
        ├── CreateWarehouseReceipt()             # 数据库操作：创建入库单
        ├── GetWarehouseReceipts()               # 数据库操作：查询入库记录
        ├── GetInTransitOrders()                 # 数据库操作：查询运输中订单
        └── GetWarehouseReceiptByTransportOrderId() # 数据库操作：根据运输单查询入库单

Model Layer (Model/)
    ├── WarehouseReceipts.cs                     # 入库单实体类
    ├── WarehouseReceiptViewModel.cs             # 入库单视图模型
    └── TransportNotificationViewModel.cs        # 运输通知视图模型
```

### 核心功能代码

#### 1. 创建入库单并清零暂存点

```csharp
// DAL层：使用事务确保数据一致性
using (SqlTransaction transaction = conn.BeginTransaction())
{
    try
    {
        // 1. 插入入库单
        // INSERT INTO WarehouseReceipts ...
        
        // 2. 清零暂存点重量（删除Inventory记录）
        // DELETE FROM Inventory WHERE RecyclerID = @RecyclerID
        
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

#### 2. 生成唯一入库单号

```csharp
// 格式：WR + YYYYMMDD + 4位序号
// 例如：WR202601040001
private string GenerateReceiptNumber()
{
    string datePrefix = "WR" + DateTime.Now.ToString("yyyyMMdd");
    int sequence = 1;
    
    using (SqlTransaction transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
    {
        // 使用表锁防止并发问题
        string sql = "SELECT COUNT(*) FROM WarehouseReceipts WITH (TABLOCKX) WHERE ReceiptNumber LIKE @DatePrefix + '%'";
        // ...
        sequence = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
    }
    
    return datePrefix + sequence.ToString("D4");
}
```

#### 3. 验证和通知

```csharp
// BLL层：业务逻辑验证
public (bool success, string message, int receiptId, string receiptNumber) CreateWarehouseReceipt(...)
{
    // 1. 验证运输单状态
    if (transportOrder.Status != "已完成")
        return (false, "只能为已完成的运输单创建入库单", 0, null);
    
    // 2. 检查是否已创建过入库单
    if (_dal.GetWarehouseReceiptByTransportOrderId(transportOrderId) != null)
        return (false, "该运输单已创建入库单", 0, null);
    
    // 3. 创建入库单
    var (receiptId, receiptNumber) = _dal.CreateWarehouseReceipt(receipt);
    
    // 4. 发送通知给回收员
    _notificationBLL.SendNotification(
        transportOrder.RecyclerID,
        "入库完成",
        $"您的运输单 {transportOrder.OrderNumber} 已成功入库...",
        "WarehouseReceipt",
        receiptId);
    
    return (true, "入库成功", receiptId, receiptNumber);
}
```

## API接口说明 | API Documentation

### 1. 获取运输中订单

**请求：**
```
POST /Staff/GetInTransitOrders
Content-Type: application/x-www-form-urlencoded

__RequestVerificationToken: [token]
```

**响应：**
```json
{
  "success": true,
  "data": [
    {
      "TransportOrderID": 1,
      "OrderNumber": "TO202601040001",
      "RecyclerName": "张回收员",
      "TransporterName": "李运输员",
      "EstimatedWeight": 45.50,
      "ItemCategories": "[{...}]",
      "PickupDate": "2026-01-04T10:30:00",
      "Status": "运输中"
    }
  ]
}
```

### 2. 创建入库单

**请求：**
```
POST /Staff/CreateWarehouseReceipt
Content-Type: application/x-www-form-urlencoded

transportOrderId: 1
totalWeight: 45.50
itemCategories: [{"categoryKey":"paper","categoryName":"纸类","weight":20.5}]
notes: 入库备注
__RequestVerificationToken: [token]
```

**响应：**
```json
{
  "success": true,
  "message": "入库成功",
  "receiptId": 1,
  "receiptNumber": "WR202601040001"
}
```

### 3. 获取入库记录

**请求：**
```
POST /Staff/GetWarehouseReceipts
Content-Type: application/x-www-form-urlencoded

page: 1
pageSize: 20
status: 已入库
__RequestVerificationToken: [token]
```

**响应：**
```json
{
  "success": true,
  "data": [
    {
      "ReceiptID": 1,
      "ReceiptNumber": "WR202601040001",
      "TransportOrderID": 1,
      "TransportOrderNumber": "TO202601040001",
      "RecyclerName": "张回收员",
      "WorkerName": "王基地人员",
      "TotalWeight": 45.50,
      "ItemCategories": "[{...}]",
      "Status": "已入库",
      "Notes": "入库备注",
      "CreatedDate": "2026-01-04T15:00:00"
    }
  ]
}
```

### 4. 检查入库单

**请求：**
```
POST /Staff/CheckWarehouseReceipt
Content-Type: application/x-www-form-urlencoded

transportOrderId: 1
__RequestVerificationToken: [token]
```

**响应：**
```json
{
  "success": true,
  "hasReceipt": true
}
```

## 使用指南 | User Guide

### 基地人员操作步骤

#### 查看运输中订单

1. 登录基地人员账号
2. 点击顶部导航"基地管理"
3. 点击"运输管理"卡片
4. 查看运输中订单列表
5. 系统每30秒自动刷新数据

#### 创建入库单

1. 登录基地人员账号
2. 点击顶部导航"基地管理"
3. 点击"仓库管理"卡片
4. 左侧显示可入库的运输单列表
5. 点击选择一个运输单
6. 填写入库信息：
   - 入库重量（必填，系统自动填充预估重量）
   - 物品类别（可选，JSON格式）
   - 备注（可选）
7. 点击"创建入库单"按钮
8. 系统自动：
   - 生成入库单号
   - 清零暂存点重量
   - 发送通知给回收员
9. 右侧入库记录列表自动更新

## 安全性 | Security

### 权限控制

所有基地管理功能都需要：
- 登录状态验证：`Session["LoginStaff"] != null`
- 角色验证：`Session["StaffRole"] == "sortingcenterworker"`

### 数据验证

1. **入库单创建验证：**
   - 运输单必须存在
   - 运输单状态必须为"已完成"
   - 不能重复创建入库单
   - 入库重量必须大于0

2. **CSRF防护：**
   - 所有POST请求都使用`@Html.AntiForgeryToken()`
   - 服务器端验证`[ValidateAntiForgeryToken]`

3. **SQL注入防护：**
   - 使用参数化查询
   - 所有用户输入都经过验证

## 错误处理 | Error Handling

### 常见错误及解决方案

| 错误信息 | 原因 | 解决方案 |
|---------|------|---------|
| "运输单不存在" | 运输单ID无效 | 检查运输单ID |
| "只能为已完成的运输单创建入库单" | 运输单状态不正确 | 等待运输完成 |
| "该运输单已创建入库单" | 重复入库 | 查看入库记录 |
| "入库重量必须大于0" | 重量验证失败 | 输入有效重量 |
| "请先登录" | 会话过期 | 重新登录 |

## 测试指南 | Testing Guide

### 功能测试清单

- [ ] 基地人员登录
- [ ] 访问基地管理主页
- [ ] 查看运输管理页面
- [ ] 验证运输中订单显示
- [ ] 测试自动刷新功能
- [ ] 访问仓库管理页面
- [ ] 选择运输单
- [ ] 创建入库单
- [ ] 验证暂存点重量清零
- [ ] 验证通知发送
- [ ] 查看入库记录
- [ ] 测试重复入库防护

### 测试数据

可以使用以下SQL脚本创建测试数据：

```sql
-- 1. 创建测试运输单（已完成状态）
INSERT INTO TransportationOrders 
    (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress, 
     ContactPerson, ContactPhone, EstimatedWeight, Status, CreatedDate, CompletedDate)
VALUES 
    ('TO202601040001', 1, 1, '测试取货地址', '测试基地地址',
     '测试联系人', '13800138000', 50.00, '已完成', GETDATE(), GETDATE());

-- 2. 创建测试运输单（运输中状态）
INSERT INTO TransportationOrders 
    (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress, 
     ContactPerson, ContactPhone, EstimatedWeight, Status, CreatedDate, PickupDate)
VALUES 
    ('TO202601040002', 1, 1, '测试取货地址2', '测试基地地址',
     '测试联系人', '13800138000', 30.00, '运输中', GETDATE(), GETDATE());
```

## 性能优化 | Performance Optimization

### 已实现的优化

1. **数据库索引：**
   - 入库单号唯一索引
   - 运输单ID索引
   - 回收员ID索引
   - 基地人员ID索引
   - 状态索引
   - 创建时间降序索引

2. **分页查询：**
   - 入库记录支持分页（默认20条/页）
   - 使用OFFSET/FETCH NEXT进行高效分页

3. **事务处理：**
   - 使用数据库事务确保数据一致性
   - 减少数据库往返次数

### 建议的优化

1. **缓存策略：**
   - 可以缓存基地人员信息
   - 可以缓存运输中订单列表（短期缓存）

2. **批量处理：**
   - 支持批量创建入库单
   - 支持批量清零暂存点

## 扩展功能 | Future Enhancements

### 计划中的功能

1. **高级统计：**
   - 入库量统计（日/周/月）
   - 回收员入库排行
   - 物品类别统计

2. **导出功能：**
   - 入库记录导出（CSV/Excel）
   - 统计报表导出

3. **通知增强：**
   - 实时推送运输中订单
   - 邮件通知
   - 短信通知

4. **权限细化：**
   - 基地主管权限
   - 普通基地人员权限
   - 操作审核功能

## 常见问题 | FAQ

**Q1: 为什么看不到运输中订单？**
A: 需要运输人员将运输单状态更新为"运输中"后才能看到。

**Q2: 如何撤销入库单？**
A: 目前不支持撤销，请在创建前仔细核对信息。

**Q3: 暂存点重量清零后能恢复吗？**
A: 不能自动恢复，需要手动重新录入。

**Q4: 物品类别JSON格式是什么？**
A: 格式示例：`[{"categoryKey":"paper","categoryName":"纸类","weight":20.5}]`

**Q5: 入库单号有什么规则？**
A: 格式为WR+年月日+4位序号，例如：WR202601040001

## 联系支持 | Support

如有问题或建议，请联系开发团队。

---

**文档版本：** 1.0  
**最后更新：** 2026-01-04  
**作者：** System Development Team
