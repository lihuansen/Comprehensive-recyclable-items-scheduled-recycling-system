# 运输单表结构修改说明
# Transportation Orders Table Structure Modification Guide

## 修改日期
2026-01-07

## 修改目的
根据需求，修改运输单表结构，使回收员联系运输人员创建运输单时，大部分数据自动生成且不可修改，只有基地联系人信息可编辑。

## 主要变更

### 1. 数据库表结构变更

#### 新增字段
- `BaseContactPerson` (NVARCHAR(50), NULL) - 基地人员联系人姓名（可编辑）
- `BaseContactPhone` (NVARCHAR(20), NULL) - 基地人员联系电话（可编辑）
- `ItemTotalValue` (DECIMAL(10,2), NOT NULL, DEFAULT 0) - 物品总金额（自动计算）

#### 修改字段
- `ContactPerson` - 从必填改为可选，存储回收员姓名
- `ContactPhone` - 从必填改为可选，存储回收员电话

#### 数据库迁移
运行以下SQL脚本完成数据库升级：
```
Database/UpdateTransportationOrdersTableStructure.sql
```

### 2. 模型层变更 (Model)

文件：`recycling.Model/TransportationOrders.cs`

- 将 `ContactPerson` 和 `ContactPhone` 的 `[Required]` 特性移除
- 新增 `BaseContactPerson` 属性
- 新增 `BaseContactPhone` 属性
- 新增 `ItemTotalValue` 属性

文件：`recycling.Model/TransportationOrdrers.cs` (DbContext)

- 为 `ItemTotalValue` 配置精度为 `DECIMAL(10,2)`

### 3. 数据访问层变更 (DAL)

文件：`recycling.DAL/TransportationOrderDAL.cs`

- `CreateTransportationOrder` 方法：添加新字段到INSERT语句
- `GetTransportationOrdersByRecycler` 方法：读取新字段并处理NULL值
- `GetTransportationOrderById` 方法：读取新字段并处理NULL值
- `GetTransportationOrdersByTransporter` 方法：读取新字段并处理NULL值

### 4. 业务逻辑层变更 (BLL)

文件：`recycling.BLL/TransportationOrderBLL.cs`

- 移除对 `ContactPerson` 和 `ContactPhone` 的必填验证
- 这两个字段现在由系统自动填充回收员信息

### 5. 控制器层变更 (Controller)

文件：`recycling.Web.UI/Controllers/StaffController.cs`

方法：`CreateTransportationOrder`

**旧参数：**
```csharp
(int transporterId, string pickupAddress, string destinationAddress, 
 string contactPerson, string contactPhone, decimal estimatedWeight, 
 string itemCategories, string specialInstructions)
```

**新参数：**
```csharp
(int transporterId, string pickupAddress, decimal estimatedWeight, 
 decimal itemTotalValue, string itemCategories, 
 string baseContactPerson, string baseContactPhone, string specialInstructions)
```

**自动填充逻辑：**
- `DestinationAddress` = "深圳基地" (固定值)
- `ContactPerson` = `staff.FullName` (从登录session获取)
- `ContactPhone` = `staff.PhoneNumber` (从登录session获取)
- `BaseContactPerson` = 用户输入（可编辑）
- `BaseContactPhone` = 用户输入（可编辑）

### 6. 视图层变更 (View)

文件：`recycling.Web.UI/Views/Staff/StoragePointManagement.cshtml`

#### HTML表单变更
创建运输单模态框中的字段调整：

**只读字段（自动填充）：**
- 取货地址 - 从暂存点数据获取
- 目的地 - 固定为"深圳基地"
- 预估总重量 - 从暂存点汇总数据计算
- 物品总金额 - 从暂存点汇总数据计算
- 物品类别 - 从暂存点汇总数据生成

**可编辑字段：**
- 基地联系人
- 基地联系电话
- 特殊说明（可选）

#### JavaScript变更

1. **新增全局变量**
```javascript
var storagePointData = {
    totalWeight: 0,
    totalValue: 0,
    itemCategories: '',
    pickupAddress: ''
};
```

2. **displaySummary函数**
- 计算并存储总重量、总价值、物品类别列表到 `storagePointData`

3. **selectTransporter函数**
- 自动填充只读字段
- 清空可编辑字段

4. **createTransportationOrder函数**
- 更新AJAX请求参数以匹配新的控制器签名
- 简化验证逻辑（移除对已删除字段的验证）

## 业务流程

### 创建运输单流程

1. **回收员点击"联系运输人员"按钮**
   - 系统加载可用运输人员列表

2. **回收员选择运输人员**
   - 系统打开创建运输单表单
   - 自动填充以下信息：
     * 运输人员信息（从选择结果）
     * 取货地址（从暂存点）
     * 目的地：深圳基地（固定）
     * 预估总重量（从暂存点计算）
     * 物品总金额（从暂存点计算）
     * 物品类别（从暂存点生成）

3. **回收员填写可编辑字段**
   - 基地联系人（可选）
   - 基地联系电话（可选）
   - 特殊说明（可选）

4. **提交创建**
   - 系统在后台自动填充：
     * 回收员ID（从登录session）
     * 回收员姓名（作为ContactPerson）
     * 回收员电话（作为ContactPhone）
     * 运输单号（自动生成）
     * 创建时间
     * 初始状态："待接单"

## 数据关系

### 字段用途说明

| 字段名 | 用途 | 填充方式 | 是否可编辑 |
|-------|------|----------|-----------|
| OrderNumber | 运输单号 | 自动生成（格式：TO+YYYYMMDD+序号） | 否 |
| RecyclerID | 回收员ID | 从登录session获取 | 否 |
| TransporterID | 运输人员ID | 用户选择 | 否 |
| PickupAddress | 取货地址 | 从暂存点数据获取 | 否 |
| DestinationAddress | 目的地 | 固定为"深圳基地" | 否 |
| ContactPerson | 回收员联系人 | 从登录session获取（回收员姓名） | 否 |
| ContactPhone | 回收员电话 | 从登录session获取 | 否 |
| BaseContactPerson | 基地联系人 | 用户输入 | 是 |
| BaseContactPhone | 基地联系电话 | 用户输入 | 是 |
| EstimatedWeight | 预估重量 | 从暂存点汇总计算 | 否 |
| ItemTotalValue | 物品总金额 | 从暂存点汇总计算 | 否 |
| ItemCategories | 物品类别 | 从暂存点汇总生成 | 否 |
| SpecialInstructions | 特殊说明 | 用户输入 | 是 |

## 测试要点

### 1. 数据库测试
- [ ] 验证新字段已正确添加
- [ ] 验证字段类型和约束正确
- [ ] 测试插入包含新字段的数据
- [ ] 测试查询包含新字段的数据

### 2. 功能测试
- [ ] 创建运输单时自动填充字段正确
- [ ] 只读字段不可编辑
- [ ] 基地联系人字段可以编辑
- [ ] 提交时数据正确保存到数据库
- [ ] 运输单列表正确显示新字段

### 3. 兼容性测试
- [ ] 旧的运输单数据（没有新字段）能正常显示
- [ ] 新旧数据可以共存

## 部署步骤

1. **备份数据库**
   ```sql
   -- 创建TransportationOrders表的备份
   SELECT * INTO TransportationOrders_Backup_20260107
   FROM TransportationOrders;
   ```

2. **执行数据库迁移脚本**
   ```
   Database/UpdateTransportationOrdersTableStructure.sql
   ```

3. **部署应用程序代码**
   - 停止Web应用
   - 部署新版本代码
   - 启动Web应用

4. **验证部署**
   - 测试创建运输单功能
   - 检查数据库中新记录是否包含新字段
   - 验证旧数据仍能正常显示

## 回滚方案

如需回滚到旧版本：

1. **回滚数据库**
   ```sql
   -- 删除新增字段
   ALTER TABLE TransportationOrders DROP COLUMN BaseContactPerson;
   ALTER TABLE TransportationOrders DROP COLUMN BaseContactPhone;
   ALTER TABLE TransportationOrders DROP COLUMN ItemTotalValue;
   
   -- 恢复必填约束
   ALTER TABLE TransportationOrders ALTER COLUMN ContactPerson NVARCHAR(50) NOT NULL;
   ALTER TABLE TransportationOrders ALTER COLUMN ContactPhone NVARCHAR(20) NOT NULL;
   ```

2. **回滚代码**
   - 部署旧版本代码

## 注意事项

1. **数据迁移**：现有运输单数据中的 `ContactPerson` 和 `ContactPhone` 字段可能为NULL，需要在查询时注意处理

2. **向后兼容**：新版本代码能够处理旧数据（没有新字段的记录）

3. **暂存点地址**：当前实现中，取货地址使用占位符"暂存点地址"，实际部署时应从回收员的实际暂存点地址获取

4. **基地联系人**：基地联系人和电话是可选字段，如果业务需要设为必填，需要修改前端验证和后端验证逻辑

## 相关文档

- [数据库架构文档](DATABASE_SCHEMA.md)
- [运输单功能实现](TRANSPORTATION_ORDER_IMPLEMENTATION.md)
- [开发指南](DEVELOPMENT_GUIDE.md)
