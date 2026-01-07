# 运输单创建失败修复说明 - ContactPerson NULL 错误

## 问题描述

在选择了基地工作人员后，创建运输单时系统显示错误：

```
localhost:44336 显示
创建失败:创建运输单失败:创建运输单失败:不能将值 NULL插入列
ContactPerson'，表
'RecyclingSystemDB.dbo.TransportationOrders';列不允许有 Null值。INSERT 失败。语句已终止。
```

## 问题原因

### 根本原因分析

1. **数据库结构**：
   - `TransportationOrders` 表的 `ContactPerson` 列设置为 NOT NULL（不允许空值）
   - `Recyclers` 表的 `FullName` 列允许为 NULL（可选字段）

2. **代码问题**：
   - 在 `StaffDAL.cs` 的 `GetRecyclerByUsername` 方法中，SQL 查询**没有包含** `FullName` 字段
   - 当回收员登录时，系统只加载了 `Username`、`PhoneNumber`、`Region` 等字段，但**没有加载** `FullName`
   - 在 `StaffController.cs` 的 `CreateTransportationOrder` 方法中，代码直接使用 `staff.FullName` 作为 `ContactPerson` 的值
   - 由于 `staff.FullName` 为 NULL，插入数据库时就会失败

### 问题流程图

```
回收员登录
    ↓
StaffDAL.GetRecyclerByUsername 查询回收员信息
    ↓
SQL 查询未包含 FullName 字段 ❌
    ↓
staff.FullName = NULL
    ↓
创建运输单
    ↓
ContactPerson = staff.FullName (NULL)
    ↓
INSERT 到数据库失败 ❌
    ↓
错误：不能将值 NULL插入列 ContactPerson
```

## 解决方案

### 修改的文件

1. **recycling.DAL/StaffDAL.cs** - 数据访问层
2. **recycling.Web.UI/Controllers/StaffController.cs** - 控制器层

### 具体修改

#### 1. StaffDAL.cs - 添加 FullName 字段查询

**修改位置**：`GetRecyclerByUsername` 方法（第27行）

**修改前**：
```csharp
string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, Region, LastLoginDate, IsActive, Available 
              FROM Recyclers 
              WHERE Username = @Username";
```

**修改后**：
```csharp
string sql = @"SELECT RecyclerID, Username, PasswordHash, FullName, PhoneNumber, Region, LastLoginDate, IsActive, Available 
              FROM Recyclers 
              WHERE Username = @Username";
```

**修改位置**：对象映射（第44行）

**修改前**：
```csharp
recycler = new Recyclers
{
    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
    Username = reader["Username"].ToString(),
    PasswordHash = reader["PasswordHash"].ToString(),
    PhoneNumber = reader["PhoneNumber"]?.ToString(),
    Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,
    // ... 其他字段
};
```

**修改后**：
```csharp
recycler = new Recyclers
{
    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
    Username = reader["Username"].ToString(),
    PasswordHash = reader["PasswordHash"].ToString(),
    FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : null,  // 新增
    PhoneNumber = reader["PhoneNumber"]?.ToString(),
    Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,
    // ... 其他字段
};
```

#### 2. StaffController.cs - 添加回退逻辑

**修改位置**：`CreateTransportationOrder` 方法（第1762行）

**修改前**：
```csharp
var order = new TransportationOrders
{
    RecyclerID = staff.RecyclerID,
    TransporterID = transporterId,
    PickupAddress = pickupAddress,
    DestinationAddress = "深圳基地",
    ContactPerson = staff.FullName, // 如果 FullName 为 NULL，这里会导致错误
    ContactPhone = staff.PhoneNumber,
    // ... 其他字段
};
```

**修改后**：
```csharp
var order = new TransportationOrders
{
    RecyclerID = staff.RecyclerID,
    TransporterID = transporterId,
    PickupAddress = pickupAddress,
    DestinationAddress = "深圳基地",
    ContactPerson = string.IsNullOrWhiteSpace(staff.FullName) ? staff.Username : staff.FullName, // 如果 FullName 为空，使用 Username 作为回退
    ContactPhone = staff.PhoneNumber,
    // ... 其他字段
};
```

## 修复后的功能流程

```
回收员登录
    ↓
StaffDAL.GetRecyclerByUsername 查询回收员信息
    ↓
SQL 查询包含 FullName 字段 ✓
    ↓
staff.FullName = 数据库中的值（可能为 NULL 或有值）
    ↓
创建运输单
    ↓
判断：FullName 是否为空？
    ├─ 是：ContactPerson = staff.Username ✓
    └─ 否：ContactPerson = staff.FullName ✓
    ↓
INSERT 到数据库成功 ✓
    ↓
运输单创建成功
```

## 测试场景

### 场景 1：回收员有 FullName（真实姓名）

1. 数据库中回收员的 `FullName` 字段有值，例如 "张三"
2. 登录后，`staff.FullName` = "张三"
3. 创建运输单时，`ContactPerson` = "张三"
4. ✓ 运输单创建成功

### 场景 2：回收员没有 FullName（仅有用户名）

1. 数据库中回收员的 `FullName` 字段为 NULL
2. 登录后，`staff.FullName` = NULL
3. 创建运输单时，由于 FullName 为 NULL，使用回退逻辑：`ContactPerson` = `staff.Username`（例如 "user123"）
4. ✓ 运输单创建成功

## 代码审查和安全检查结果

### 代码审查
- ✅ **通过**：代码逻辑正确
- 💡 建议：可以提取回退逻辑为独立方法以提高代码可读性（但不影响功能）

### 安全扫描（CodeQL）
- ✅ **通过**：无安全漏洞
- ✅ SQL 查询使用参数化，防止 SQL 注入
- ✅ 正确处理 NULL 值

### 兼容性检查
- ✅ 不影响现有功能
- ✅ 向后兼容：已有 FullName 的账号继续使用 FullName
- ✅ 新增保护：没有 FullName 的账号使用 Username 作为回退

## 数据库表结构

### Recyclers 表（回收员）
```sql
CREATE TABLE Recyclers (
    RecyclerID INT PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NULL,        -- ← 可以为 NULL
    PhoneNumber NVARCHAR(20) NOT NULL,
    Region NVARCHAR(100) NOT NULL,
    -- ... 其他字段
);
```

### TransportationOrders 表（运输单）
```sql
CREATE TABLE TransportationOrders (
    TransportOrderID INT PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL,
    RecyclerID INT NOT NULL,
    TransporterID INT NOT NULL,
    ContactPerson NVARCHAR(50) NOT NULL,     -- ← 不能为 NULL
    ContactPhone NVARCHAR(20) NOT NULL,
    BaseContactPerson NVARCHAR(50) NULL,     -- ← 基地联系人，可以为 NULL
    BaseContactPhone NVARCHAR(20) NULL,      -- ← 基地联系电话，可以为 NULL
    -- ... 其他字段
);
```

## 相关代码文件

- `/recycling.DAL/StaffDAL.cs` - 数据访问层（回收员查询）
- `/recycling.Web.UI/Controllers/StaffController.cs` - 控制器层（运输单创建）
- `/recycling.Model/Recyclers.cs` - 回收员模型
- `/recycling.Model/TransportationOrders.cs` - 运输单模型
- `/recycling.BLL/TransportationOrderBLL.cs` - 运输单业务逻辑
- `/recycling.DAL/TransportationOrderDAL.cs` - 运输单数据访问

## 注意事项

1. **FullName 字段是可选的**
   - 回收员注册时可能没有填写真实姓名
   - 系统现在可以优雅地处理这种情况

2. **Username 作为回退方案**
   - 当 FullName 为空时，使用 Username 作为联系人姓名
   - Username 是必填字段，保证 ContactPerson 始终有值

3. **基地联系人信息**
   - `BaseContactPerson` 和 `BaseContactPhone` 仍然可以为 NULL
   - 这些字段由用户在创建运输单时手动填写
   - 如果用户不填写，系统允许为空

4. **不影响现有功能**
   - 已有的运输单不受影响
   - 其他使用回收员信息的功能继续正常工作

## 总结

此次修复采用**最小化改动原则**：
- ✅ 只修改了 2 个文件
- ✅ 添加了 2 行代码（FullName 字段查询和赋值）
- ✅ 添加了 1 行回退逻辑（使用 Username 替代空的 FullName）
- ✅ 未破坏任何现有功能
- ✅ 通过了代码审查和安全扫描
- ✅ 兼容所有场景（有/无 FullName）

修复后，无论回收员是否填写了真实姓名（FullName），系统都能成功创建运输单。
