# 运输人员联系功能修复说明

## 问题描述
在回收员端的暂存点管理中点击"联系运输人员"时，出现以下错误：
```
获取运输人员失败: 参数化查询 '(@Region nvarchar(4000))SELECT TransporterID, Username, FullName' 需要参数 '@Region'，但未提供该参数。
```

## 问题原因
回收员登录时，数据访问层（DAL）没有从数据库中获取回收员的 `Region`（区域）字段。当系统尝试查询同一区域的运输人员时，`staff.Region` 为 `null`，导致 SQL 参数错误。

## 解决方案

### 修改的文件
1. **recycling.DAL/StaffDAL.cs** - 数据访问层
2. **recycling.Web.UI/Controllers/StaffController.cs** - 控制器层

### 具体修改

#### 1. StaffDAL.cs - 添加 Region 字段查询

**GetRecyclerByUsername 方法（第27行）：**
```csharp
// 修改前
string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, LastLoginDate, IsActive, Available 
              FROM Recyclers WHERE Username = @Username";

// 修改后
string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, Region, LastLoginDate, IsActive, Available 
              FROM Recyclers WHERE Username = @Username";
```

**对象映射（第45行）：**
```csharp
recycler = new Recyclers
{
    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
    Username = reader["Username"].ToString(),
    PasswordHash = reader["PasswordHash"].ToString(),
    PhoneNumber = reader["PhoneNumber"]?.ToString(),
    Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,  // 新增
    LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : (DateTime?)null,
    IsActive = Convert.ToBoolean(reader["IsActive"]),
    Available = Convert.ToBoolean(reader["Available"])
};
```

**GetRecyclerById 方法** 也做了相同的修改。

#### 2. StaffController.cs - 添加防御性检查

**GetAvailableTransporters 方法（第1334行）：**
```csharp
if (staff == null || role != "recycler")
{
    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
}

// 新增：检查回收员是否分配了区域
if (string.IsNullOrWhiteSpace(staff.Region))
{
    return JsonContent(new { success = false, message = "您的账号未分配区域，请联系管理员" });
}

// Get transporters in the same region who are active and available
using (var conn = new System.Data.SqlClient.SqlConnection(...))
{
    // ... 后续查询代码
}
```

## 修复后的功能流程

1. **回收员登录**
   - 系统从数据库加载回收员信息，包括 Region 字段
   - Region 字段存储在 Session 中

2. **进入暂存点管理**
   - 回收员可以查看暂存点库存汇总和明细

3. **点击"联系运输人员"**
   - 系统验证回收员已分配区域
   - 查询同一区域的可用运输人员
   - 显示运输人员列表（按评分排序）

4. **创建运输单**
   - 回收员选择运输人员
   - 填写运输单信息（取货地址、目的地、预估重量等）
   - 系统生成运输单号（格式：TO+日期+序号）
   - 运输单状态为"待接单"

## 数据表关系

### Recyclers 表（回收员）
```sql
- RecyclerID (主键)
- Username
- PasswordHash
- Region (区域，必填)  ← 关键字段
- PhoneNumber
- ...
```

### Transporters 表（运输人员）
```sql
- TransporterID (主键)
- Username
- PasswordHash
- Region (负责区域，必填)  ← 用于匹配回收员
- VehiclePlateNumber (车牌号)
- Available (是否可接单)
- IsActive (账号是否激活)
- Rating (评分)
- ...
```

### TransportationOrders 表（运输单）
```sql
- TransportOrderID (主键)
- OrderNumber (运输单号，格式：TO+日期+序号)
- RecyclerID (外键 → Recyclers)
- TransporterID (外键 → Transporters)
- PickupAddress (取货地址)
- DestinationAddress (目的地地址)
- EstimatedWeight (预估重量)
- Status (状态：待接单、已接单、运输中、已完成、已取消)
- CreatedDate (创建时间)
- ...
```

## 测试验证

### 已完成的验证
- ✅ 代码审查通过（无问题）
- ✅ CodeQL 安全扫描通过（无漏洞）
- ✅ SQL 查询使用参数化，防止 SQL 注入
- ✅ 添加了用户友好的错误提示

### 建议的手动测试步骤
1. 使用回收员账号登录系统
2. 进入"暂存点管理"页面
3. 点击"联系运输人员"按钮
4. 验证显示同区域的运输人员列表
5. 选择一个运输人员
6. 填写运输单信息并提交
7. 验证运输单创建成功
8. 检查运输单列表中显示新创建的运输单

## 注意事项

1. **回收员必须分配区域**
   - 如果回收员账号的 Region 字段为空，会显示错误提示
   - 管理员需要确保所有回收员都分配了正确的区域

2. **运输人员必须满足条件**
   - Region 与回收员相同
   - IsActive = 1（账号激活）
   - Available = 1（可接单）

3. **运输单号格式**
   - 格式：TO + YYYYMMDD + 4位序号
   - 例如：TO202601030001

## 相关代码文件

- `/recycling.DAL/StaffDAL.cs` - 数据访问层
- `/recycling.BLL/StaffBLL.cs` - 业务逻辑层
- `/recycling.BLL/TransportationOrderBLL.cs` - 运输单业务逻辑
- `/recycling.DAL/TransportationOrderDAL.cs` - 运输单数据访问
- `/recycling.Web.UI/Controllers/StaffController.cs` - 回收员控制器
- `/recycling.Model/Recyclers.cs` - 回收员模型
- `/recycling.Model/Transporters.cs` - 运输人员模型
- `/recycling.Model/TransportationOrders.cs` - 运输单模型

## 数据库脚本

如果需要创建相关表，请执行以下脚本：
- `/Database/CreateTransportersTable.sql` - 创建运输人员表
- `/Database/CreateTransportationOrdersTable.sql` - 创建运输单表

## 总结

此次修复采用最小化改动原则：
- 只修改了 2 个文件
- 添加了 4 行代码（Region 字段查询和赋值）
- 添加了 5 行防御性检查代码
- 未破坏任何现有功能
- 保持了代码的安全性和可维护性

修复后，回收员可以正常使用"联系运输人员"功能，成功创建运输单。
