# 运输人员表字段更新说明
# Transporters Table Columns Update Guide

## 问题描述 / Problem Description

当点击运输工作人员中的账号管理时，系统显示以下错误：

```
System.Exception: "获取运输人员信息失败：查询运输人员失败：
列名 'LicenseNumber' 无效。
列名 'TotalTrips' 无效。
列名 'AvatarURL' 无效。
列名 'Notes' 无效。"
```

**原因 / Cause:**
实体类 `Transporters.cs` 已更新以包含新的属性字段，但数据库表 `Transporters` 还没有这些列。

**影响范围 / Impact:**
- 运输人员账号管理功能无法使用
- 查看运输人员详细信息失败
- 运输人员个人资料编辑功能受影响

---

## 解决方案 / Solution

### 方案一：自动更新（推荐） / Option 1: Automatic Update (Recommended)

#### Windows 用户:
1. 打开命令提示符（以管理员身份运行）
2. 导航到 `Database` 目录
3. 运行以下命令：
   ```cmd
   UpdateTransportersColumns.bat
   ```
4. 按照提示输入数据库连接信息
5. 等待脚本执行完成

#### Linux/Mac 用户:
1. 打开终端
2. 导航到 `Database` 目录
3. 运行以下命令：
   ```bash
   ./UpdateTransportersColumns.sh
   ```
4. 按照提示输入数据库连接信息
5. 等待脚本执行完成

---

### 方案二：手动更新 / Option 2: Manual Update

如果自动脚本无法执行，可以手动在数据库中运行以下SQL命令：

```sql
USE RecyclingDB;
GO

-- 添加 LicenseNumber 字段（驾驶证号）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'LicenseNumber')
BEGIN
    ALTER TABLE [dbo].[Transporters] ADD [LicenseNumber] NVARCHAR(50) NULL;
END
GO

-- 添加 TotalTrips 字段（总运输次数）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'TotalTrips')
BEGIN
    ALTER TABLE [dbo].[Transporters] ADD [TotalTrips] INT NULL;
END
GO

-- 添加 AvatarURL 字段（头像URL）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'AvatarURL')
BEGIN
    ALTER TABLE [dbo].[Transporters] ADD [AvatarURL] NVARCHAR(255) NULL;
END
GO

-- 添加 Notes 字段（备注信息）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Notes')
BEGIN
    ALTER TABLE [dbo].[Transporters] ADD [Notes] NVARCHAR(500) NULL;
END
GO

-- 添加 money 字段（账户余额）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'money')
BEGIN
    ALTER TABLE [dbo].[Transporters] ADD [money] DECIMAL(18, 2) NULL DEFAULT 0;
END
GO

-- 确保 VehicleType 字段可为空
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'VehicleType' AND is_nullable = 0)
BEGIN
    ALTER TABLE [dbo].[Transporters] ALTER COLUMN [VehicleType] NVARCHAR(50) NULL;
END
GO

-- 确保 VehiclePlateNumber 字段可为空
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'VehiclePlateNumber' AND is_nullable = 0)
BEGIN
    ALTER TABLE [dbo].[Transporters] ALTER COLUMN [VehiclePlateNumber] NVARCHAR(50) NULL;
END
GO

PRINT '✓ Transporters 表更新完成！';
GO
```

---

## 更新的字段列表 / Updated Columns

| 字段名 | 数据类型 | 说明 | 是否可空 |
|-------|---------|------|---------|
| LicenseNumber | NVARCHAR(50) | 驾驶证号 | 是 |
| TotalTrips | INT | 总运输次数 | 是 |
| AvatarURL | NVARCHAR(255) | 头像URL | 是 |
| Notes | NVARCHAR(500) | 备注信息 | 是 |
| money | DECIMAL(18,2) | 账户余额 | 是 |

---

## 验证更新是否成功 / Verify Update

执行以下SQL查询来验证字段是否已成功添加：

```sql
-- 查询 Transporters 表的所有列
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Transporters'
ORDER BY ORDINAL_POSITION;
```

确保以下列存在：
- LicenseNumber
- TotalTrips
- AvatarURL
- Notes
- money

---

## 相关文件 / Related Files

### 数据库脚本 / Database Scripts
- `UpdateTransportersTableColumns.sql` - 主SQL更新脚本
- `UpdateTransportersColumns.bat` - Windows批处理脚本
- `UpdateTransportersColumns.sh` - Linux/Mac shell脚本
- `CreateTransportersTable.sql` - 原始表创建脚本（参考）

### 代码文件 / Code Files
- `recycling.Model/Transporters.cs` - 运输人员实体类
- `recycling.DAL/StaffDAL.cs` - 数据访问层（包含 GetTransporterById 方法）
- `recycling.BLL/StaffBLL.cs` - 业务逻辑层

---

## 注意事项 / Important Notes

1. **备份数据库**: 在执行任何数据库更新之前，请务必备份您的数据库
2. **权限要求**: 执行脚本需要数据库的 ALTER TABLE 权限
3. **停机时间**: 建议在系统维护时间执行更新，避免影响用户使用
4. **测试环境**: 建议先在测试环境中验证更新脚本，确保无误后再在生产环境执行

---

## 常见问题 / FAQ

**Q1: 脚本执行失败怎么办？**
A: 检查以下几点：
- 数据库连接字符串是否正确
- 是否有足够的权限执行 ALTER TABLE
- 数据库服务是否正在运行
- SQL Server 版本是否兼容

**Q2: 更新后数据会丢失吗？**
A: 不会。这个脚本只添加新列，不会删除或修改现有数据。

**Q3: 可以重复执行脚本吗？**
A: 可以。脚本使用了 `IF NOT EXISTS` 检查，不会重复添加已存在的列。

**Q4: 为什么这些列之前不存在？**
A: 实体类被更新以支持新功能，但数据库迁移脚本没有同步执行。

---

## 更新历史 / Update History

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-01-22 | 1.0 | 初始版本，添加 LicenseNumber、TotalTrips、AvatarURL、Notes、money 字段 |

---

## 技术支持 / Technical Support

如果在执行更新过程中遇到问题，请：
1. 检查错误日志
2. 查看本文档的常见问题部分
3. 联系系统管理员

---

**最后更新 / Last Updated:** 2026-01-22
**作者 / Author:** System
