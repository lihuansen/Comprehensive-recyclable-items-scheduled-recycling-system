# 回收员管理改进 - 测试指南

## 概述
本文档描述了对回收员管理功能所做的五项改进及其测试方法。

## 改进内容

### 1. 评分系统 - 基于真实用户评价的平均星级

**实现内容：**
- 当用户提交订单评价后，系统自动计算该回收员的平均星级并更新到 `Recyclers.Rating` 字段
- 如果回收员还没有接单或没有评价，评分显示为 0
- 评分计算公式：`AVG(StarRating)` from `OrderReviews` table

**相关文件：**
- `recycling.DAL/OrderReviewDAL.cs` - 添加了 `UpdateRecyclerRating()` 方法
- `Database/UpdateRecyclerRatings.sql` - 用于初始化所有回收员评分的SQL脚本

**测试步骤：**
1. 登录用户账户
2. 完成一个订单
3. 对回收员进行评价（1-5星）
4. 在管理员界面查看回收员管理页面
5. 验证该回收员的评分已更新
6. 添加多个评价，验证评分为平均值

**SQL验证：**
```sql
-- 查看回收员的评分和评价统计
SELECT 
    r.RecyclerID,
    r.Username,
    r.Rating,
    COUNT(or.ReviewID) AS ReviewCount,
    AVG(CAST(or.StarRating AS DECIMAL(10,2))) AS CalculatedAvg
FROM Recyclers r
LEFT JOIN OrderReviews or ON r.RecyclerID = or.RecyclerID
GROUP BY r.RecyclerID, r.Username, r.Rating
ORDER BY r.RecyclerID
```

---

### 2. 列表排序 - 按照 RecyclerID 排序

**实现内容：**
- 回收员列表现在按照 RecyclerID 升序排列，而不是创建时间

**相关文件：**
- `recycling.DAL/AdminDAL.cs` - 修改了 `GetAllRecyclers()` 的 ORDER BY 子句

**测试步骤：**
1. 登录管理员账户
2. 进入回收员管理页面
3. 验证列表按 RecyclerID 从小到大排序
4. 翻页验证排序在所有页面保持一致

---

### 3. 操作按钮显示文字

**实现内容：**
- 编辑按钮现在显示 "📝 编辑"
- 删除按钮现在显示 "🗑️ 删除"
- 提升了用户界面的可读性

**相关文件：**
- `recycling.Web.UI/Views/Staff/RecyclerManagement.cshtml` - 修改了按钮的HTML

**测试步骤：**
1. 登录管理员账户
2. 进入回收员管理页面
3. 验证每个回收员行的操作列显示有文字的按钮
4. 验证按钮功能仍然正常工作

---

### 4. 删除操作 - 直接从数据库删除

**实现内容：**
- 删除回收员时，现在从数据库中物理删除记录（硬删除）
- 之前是软删除（设置 IsActive = 0）

**相关文件：**
- `recycling.DAL/AdminDAL.cs` - 修改了 `DeleteRecycler()` 方法

**测试步骤：**
1. 登录管理员账户
2. 进入回收员管理页面
3. 创建一个测试回收员
4. 删除该回收员
5. 验证该回收员从列表中消失
6. 在数据库中验证记录已被物理删除

**SQL验证：**
```sql
-- 验证回收员是否已从数据库删除
SELECT * FROM Recyclers WHERE RecyclerID = [被删除的ID]
-- 应该返回 0 条记录
```

**⚠️ 注意事项：**
- 硬删除会导致外键约束问题，如果该回收员有关联的订单记录
- 建议在删除前检查是否有关联数据
- 或者考虑在数据库中添加 ON DELETE CASCADE 外键约束

---

### 5. 系统逻辑修改 - 基于状态的登录和接单限制

**实现内容：**

#### 5.1 账号禁用时无法登录
- 当回收员的 `IsActive = 0`（禁用状态）时，无法登录
- 登录时返回错误消息："账号已被禁用，无法登录"

**相关文件：**
- `recycling.DAL/StaffDAL.cs` - 加载 IsActive 和 Available 字段
- `recycling.BLL/StaffBLL.cs` - 添加 IsActive 检查

**测试步骤：**
1. 登录管理员账户
2. 编辑一个回收员，取消勾选"激活状态"
3. 尝试使用该回收员账号登录
4. 验证登录失败，显示"账号已被禁用，无法登录"

#### 5.2 不可接单状态可以登录但不能接单
- 当回收员的 `Available = 0`（不可接单）但 `IsActive = 1` 时，可以登录
- 登录后可以查看订单，但无法接受新订单
- 接单时返回错误消息："当前状态不可接单"

**相关文件：**
- `recycling.BLL/RecyclerOrderBLL.cs` - 在 `AcceptOrder()` 中添加 Available 检查

**测试步骤：**
1. 登录管理员账户
2. 编辑一个回收员，取消勾选"可接单"，保持"激活状态"勾选
3. 使用该回收员账号登录
4. 验证可以成功登录
5. 进入订单管理页面
6. 尝试接受一个待接单的订单
7. 验证接单失败，显示"当前状态不可接单"

**SQL验证：**
```sql
-- 查看回收员状态
SELECT RecyclerID, Username, IsActive, Available 
FROM Recyclers 
ORDER BY RecyclerID

-- IsActive = 1, Available = 1: 可以登录和接单
-- IsActive = 1, Available = 0: 可以登录但不能接单
-- IsActive = 0, Available = 任意: 不能登录
```

---

## 完整测试场景

### 场景 1: 新回收员从创建到接单到评价
1. 管理员创建新回收员（初始 Rating = 0）
2. 回收员登录成功
3. 回收员接受订单
4. 完成订单
5. 用户评价订单（例如 5 星）
6. 验证回收员的 Rating 更新为 5.0

### 场景 2: 回收员被禁用
1. 管理员将回收员设置为禁用状态（IsActive = 0）
2. 回收员尝试登录失败
3. 管理员重新启用回收员
4. 回收员可以正常登录

### 场景 3: 回收员设置为不可接单
1. 管理员将回收员设置为不可接单（Available = 0, IsActive = 1）
2. 回收员可以正常登录
3. 回收员尝试接单失败
4. 管理员将回收员设置回可接单
5. 回收员可以成功接单

### 场景 4: 删除回收员
1. 管理员创建测试回收员
2. 验证回收员在列表中（按 RecyclerID 排序）
3. 管理员删除回收员（点击"删除"按钮）
4. 验证回收员从列表中消失
5. 数据库中验证记录已删除

---

## 数据库初始化

如果需要初始化所有回收员的评分，可以运行以下SQL脚本：

```bash
sqlcmd -S [服务器名] -d RecyclingDB -i Database/UpdateRecyclerRatings.sql
```

或在 SQL Server Management Studio 中执行 `Database/UpdateRecyclerRatings.sql`

---

## 回滚说明

如果需要回滚这些更改：

1. **评分系统**: 可以继续使用，不影响现有功能
2. **列表排序**: 修改 `AdminDAL.cs` 的 ORDER BY 回到 `CreatedDate DESC`
3. **按钮文字**: 移除按钮中的文字部分，只保留图标
4. **硬删除**: 修改为 `UPDATE Recyclers SET IsActive = 0`
5. **登录限制**: 移除 StaffBLL 中的 IsActive 检查
6. **接单限制**: 移除 RecyclerOrderBLL 中的 Available 检查

---

## 技术说明

### 数据库表结构
```sql
-- Recyclers 表相关字段
RecyclerID INT PRIMARY KEY IDENTITY(1,1)
Username NVARCHAR(50)
IsActive BIT NOT NULL DEFAULT 1     -- 是否激活（0=禁用，1=启用）
Available BIT NOT NULL DEFAULT 1    -- 是否可接单（0=不可接单，1=可接单）
Rating DECIMAL(3, 2) NULL           -- 评分（0-5）

-- OrderReviews 表
ReviewID INT PRIMARY KEY IDENTITY(1,1)
RecyclerID INT NOT NULL
StarRating INT NOT NULL             -- 星级（1-5）
```

### 业务规则
- `Rating` 字段自动计算，不应手动修改
- `IsActive = 0` 时不能登录
- `Available = 0` 时可以登录但不能接单
- 只有 `IsActive = 1 AND Available = 1` 才能正常接单
- 删除回收员前应确保没有关联的进行中订单

---

**更新日期**: 2025-11-06  
**版本**: 1.0
