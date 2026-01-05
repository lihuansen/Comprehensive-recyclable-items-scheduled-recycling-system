# 数据库迁移指南 / Database Migration Guide

## 重要通知 / Important Notice

如果您在基地工作人员登录时遇到以下错误：
```
列名 'LastViewedTransportCount' 无效。
列名 'LastViewedWarehouseCount' 无效。
```

这表示数据库中缺少通知追踪列。请按照以下步骤执行数据库迁移。

---

If you encounter the following error when logging in as a base worker:
```
Invalid column name 'LastViewedTransportCount'.
Invalid column name 'LastViewedWarehouseCount'.
```

This indicates that notification tracking columns are missing from the database. Please follow the steps below to run the database migration.

---

## 解决方案 / Solution

### 方法1：执行迁移脚本 / Method 1: Run Migration Script

1. 打开 SQL Server Management Studio (SSMS) 或您的数据库管理工具
2. 连接到 `RecyclingDB` 数据库
3. 打开并执行以下脚本：

```
Database/AddNotificationTrackingToSortingCenterWorkers.sql
```

该脚本会自动检查列是否存在，如果不存在则添加：
- `LastViewedTransportCount` INT NOT NULL DEFAULT 0
- `LastViewedWarehouseCount` INT NOT NULL DEFAULT 0

---

### 方法2：手动添加列 / Method 2: Add Columns Manually

如果迁移脚本无法执行，您可以手动执行以下SQL命令：

```sql
USE RecyclingDB;
GO

-- 添加运输通知追踪列
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SortingCenterWorkers' 
               AND COLUMN_NAME = 'LastViewedTransportCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    ADD [LastViewedTransportCount] INT NOT NULL DEFAULT 0;
END
GO

-- 添加仓库通知追踪列
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SortingCenterWorkers' 
               AND COLUMN_NAME = 'LastViewedWarehouseCount')
BEGIN
    ALTER TABLE [dbo].[SortingCenterWorkers]
    ADD [LastViewedWarehouseCount] INT NOT NULL DEFAULT 0;
END
GO
```

---

## 验证迁移 / Verify Migration

执行以下查询以验证列是否成功添加：

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SortingCenterWorkers'
AND COLUMN_NAME IN ('LastViewedTransportCount', 'LastViewedWarehouseCount');
```

预期输出 / Expected Output:
```
COLUMN_NAME                    DATA_TYPE  IS_NULLABLE  COLUMN_DEFAULT
LastViewedTransportCount       int        NO           ((0))
LastViewedWarehouseCount       int        NO           ((0))
```

---

## 系统兼容性 / System Compatibility

**重要提示**：即使不执行迁移脚本，系统也能正常运行！

代码已经更新以处理列不存在的情况：
- 登录仍然可以正常工作
- 通知系统会使用默认值（所有通知都显示为新的）
- 更新操作会静默失败（不影响用户体验）

执行迁移后，通知系统将启用持久化功能：
- 用户重新登录后，已读通知不会重复显示
- 通知状态保存在数据库中

---

**Important**: Even without running the migration script, the system will still work!

The code has been updated to handle missing columns gracefully:
- Login will continue to work normally
- Notification system will use default values (all notifications shown as new)
- Update operations will fail silently (no impact on user experience)

After running the migration, notification persistence will be enabled:
- Read notifications won't reappear after re-login
- Notification status is saved in the database

---

## 故障排除 / Troubleshooting

### 问题1：迁移脚本执行失败
**解决方案**：检查数据库连接权限，确保您有ALTER TABLE权限。

### 问题2：列已存在
**解决方案**：这是正常的。脚本会检测列是否存在，如果存在会跳过添加操作。

### 问题3：登录仍然失败
**解决方案**：
1. 确认脚本执行成功（使用验证查询）
2. 重启Web应用程序
3. 清除浏览器缓存和Session
4. 重新登录

---

## 联系支持 / Contact Support

如果您在迁移过程中遇到任何问题，请：
1. 检查SQL Server错误日志
2. 验证数据库连接字符串
3. 确认RecyclingDB数据库存在且可访问

---

## 更新日志 / Changelog

**2026-01-05**
- 添加通知追踪列支持
- 实现向后兼容性（列不存在时也能正常工作）
- 更新StaffDAL.cs以处理缺失的列
