# 运输人员表错误快速修复指南
# Quick Fix Guide for Transporters Table Error

> **错误信息 / Error Message:**  
> 系统异常："获取运输人员信息失败：查询运输人员失败：列名 'LicenseNumber' 无效。列名 'TotalTrips' 无效。列名 'AvatarURL' 无效。列名 'Notes' 无效。"

---

## 🚀 快速修复步骤 / Quick Fix Steps

### 步骤 1: 运行数据库更新脚本

**Windows 用户:**
```cmd
cd Database
UpdateTransportersColumns.bat
```

**Linux/Mac 用户:**
```bash
cd Database
./UpdateTransportersColumns.sh
```

### 步骤 2: 验证更新

运行验证脚本确认所有字段已正确添加：

**使用 SQL Server Management Studio (SSMS):**
1. 打开 `Database/VerifyTransportersTableColumns.sql`
2. 连接到数据库
3. 执行脚本
4. 检查输出是否显示"✓✓✓ 验证通过！"

**使用 sqlcmd:**
```cmd
sqlcmd -S localhost -d RecyclingDB -E -i VerifyTransportersTableColumns.sql
```

### 步骤 3: 重启应用程序

重启您的 ASP.NET 应用程序以确保更改生效。

### 步骤 4: 测试功能

1. 登录系统
2. 进入"运输工作人员"模块
3. 点击"账号管理"
4. 确认页面正常加载，没有错误

---

## 📋 技术细节 / Technical Details

### 问题原因 / Root Cause

实体类 `recycling.Model.Transporters` 已更新，包含以下新属性：
- `LicenseNumber` (驾驶证号)
- `TotalTrips` (总运输次数)
- `AvatarURL` (头像URL)
- `Notes` (备注信息)
- `money` (账户余额)

但是数据库表 `Transporters` 还没有这些列。

### 影响的代码文件 / Affected Code Files

**数据访问层 (DAL):**
- `recycling.DAL.StaffDAL.GetTransporterById()` (第332-337行)

该方法尝试查询不存在的列，导致 SQL 异常。

### 修复内容 / What Gets Fixed

数据库更新脚本会：
1. ✅ 添加 `LicenseNumber` 列 (NVARCHAR(50), NULL)
2. ✅ 添加 `TotalTrips` 列 (INT, NULL)
3. ✅ 添加 `AvatarURL` 列 (NVARCHAR(255), NULL)
4. ✅ 添加 `Notes` 列 (NVARCHAR(500), NULL)
5. ✅ 添加 `money` 列 (DECIMAL(18,2), NULL, DEFAULT 0)
6. ✅ 将 `VehicleType` 改为可空
7. ✅ 将 `VehiclePlateNumber` 改为可空

---

## ⚠️ 重要提示 / Important Notes

### 备份数据库
**强烈建议在执行更新前备份数据库！**

```sql
-- 备份命令示例
BACKUP DATABASE RecyclingDB 
TO DISK = 'C:\Backups\RecyclingDB_Backup.bak'
WITH FORMAT;
```

### 安全执行
脚本使用了 `IF NOT EXISTS` 检查，因此：
- ✅ 可以安全地多次运行
- ✅ 不会删除现有数据
- ✅ 只添加缺失的列

---

## 🔍 故障排除 / Troubleshooting

### 问题 1: "找不到数据库 RecyclingDB"

**解决方案:**
检查数据库名称是否正确。如果使用不同的数据库名，请修改脚本中的 `USE` 语句。

### 问题 2: "权限被拒绝"

**解决方案:**
确保数据库用户有 `ALTER TABLE` 权限：
```sql
GRANT ALTER ON SCHEMA::dbo TO [YourUsername];
```

### 问题 3: 脚本运行后错误仍然存在

**检查清单:**
1. ✅ 运行验证脚本确认字段已添加
2. ✅ 重启应用程序
3. ✅ 清除浏览器缓存
4. ✅ 检查连接字符串是否指向正确的数据库

### 问题 4: 手动执行 SQL 失败

如果自动脚本无法运行，可以手动执行以下最简化的 SQL：

```sql
USE RecyclingDB;

-- 添加 LicenseNumber
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transporters') AND name = 'LicenseNumber')
    ALTER TABLE Transporters ADD LicenseNumber NVARCHAR(50) NULL;

-- 添加 TotalTrips
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transporters') AND name = 'TotalTrips')
    ALTER TABLE Transporters ADD TotalTrips INT NULL;

-- 添加 AvatarURL
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transporters') AND name = 'AvatarURL')
    ALTER TABLE Transporters ADD AvatarURL NVARCHAR(255) NULL;

-- 添加 Notes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transporters') AND name = 'Notes')
    ALTER TABLE Transporters ADD Notes NVARCHAR(500) NULL;

-- 添加 money
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transporters') AND name = 'money')
    ALTER TABLE Transporters ADD money DECIMAL(18,2) NULL DEFAULT 0;
```

---

## 📚 相关文档 / Related Documentation

- **详细说明:** `Database/TRANSPORTERS_TABLE_UPDATE_README.md`
- **SQL脚本:** `Database/UpdateTransportersTableColumns.sql`
- **验证脚本:** `Database/VerifyTransportersTableColumns.sql`
- **表创建脚本:** `Database/CreateTransportersTable.sql`

---

## ✅ 验证成功标志 / Success Indicators

更新成功后，您应该看到：

1. **SQL 输出:**
   ```
   ✓ 已添加 LicenseNumber 字段
   ✓ 已添加 TotalTrips 字段
   ✓ 已添加 AvatarURL 字段
   ✓ 已添加 Notes 字段
   ✓ 已添加 money 字段
   ✓✓✓ 验证通过！所有必需字段都已存在！
   ```

2. **应用程序:**
   - 运输工作人员账号管理页面正常加载
   - 可以查看运输人员详细信息
   - 没有列名无效的错误

---

## 🆘 获取帮助 / Get Help

如果问题仍未解决：

1. 查看应用程序日志文件
2. 运行验证脚本并检查输出
3. 检查数据库表结构：
   ```sql
   SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'Transporters'
   ORDER BY ORDINAL_POSITION;
   ```

---

**最后更新 / Last Updated:** 2026-01-22  
**适用版本 / Applies To:** 所有使用 Transporters 表的系统版本
