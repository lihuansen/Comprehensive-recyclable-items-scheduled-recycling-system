-- ==============================================================================
-- 更新运输人员表（Transporters）字段脚本
-- Update Transporters Table Columns Script
-- 
-- 用途: 确保Transporters表包含所有必需的字段
-- Purpose: Ensure Transporters table contains all required columns
--
-- 问题: GetTransporterById方法查询失败，提示列名无效
-- Issue: GetTransporterById method fails with invalid column names
--
-- 修复字段: LicenseNumber, TotalTrips, AvatarURL, Notes, money
-- Fixed Columns: LicenseNumber, TotalTrips, AvatarURL, Notes, money
--
-- 创建日期: 2026-01-22
-- ==============================================================================

USE RecyclingDB;
GO

PRINT '开始更新 Transporters 表字段...';
GO

-- ==============================================================================
-- 添加 LicenseNumber 字段（驾驶证号）
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'LicenseNumber'
)
BEGIN
    ALTER TABLE [dbo].[Transporters]
    ADD [LicenseNumber] NVARCHAR(50) NULL;
    PRINT '✓ 已添加 LicenseNumber 字段';
END
ELSE
BEGIN
    PRINT '✓ LicenseNumber 字段已存在';
END
GO

-- ==============================================================================
-- 添加 TotalTrips 字段（总运输次数）
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'TotalTrips'
)
BEGIN
    ALTER TABLE [dbo].[Transporters]
    ADD [TotalTrips] INT NOT NULL DEFAULT 0;
    PRINT '✓ 已添加 TotalTrips 字段';
END
ELSE
BEGIN
    PRINT '✓ TotalTrips 字段已存在';
END
GO

-- ==============================================================================
-- 添加 AvatarURL 字段（头像URL）
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'AvatarURL'
)
BEGIN
    ALTER TABLE [dbo].[Transporters]
    ADD [AvatarURL] NVARCHAR(255) NULL;
    PRINT '✓ 已添加 AvatarURL 字段';
END
ELSE
BEGIN
    PRINT '✓ AvatarURL 字段已存在';
END
GO

-- ==============================================================================
-- 添加 Notes 字段（备注信息）
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'Notes'
)
BEGIN
    ALTER TABLE [dbo].[Transporters]
    ADD [Notes] NVARCHAR(500) NULL;
    PRINT '✓ 已添加 Notes 字段';
END
ELSE
BEGIN
    PRINT '✓ Notes 字段已存在';
END
GO

-- ==============================================================================
-- 添加 money 字段（运输人员账户余额）
-- ==============================================================================
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'money'
)
BEGIN
    ALTER TABLE [dbo].[Transporters]
    ADD [money] DECIMAL(18, 2) NULL DEFAULT 0;
    PRINT '✓ 已添加 money 字段';
END
ELSE
BEGIN
    PRINT '✓ money 字段已存在';
END
GO

-- ==============================================================================
-- 确保 VehicleType 字段可为空（根据实体类定义）
-- ==============================================================================
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'VehicleType'
    AND is_nullable = 0
)
BEGIN
    -- 修改 VehicleType 为可空字段
    ALTER TABLE [dbo].[Transporters]
    ALTER COLUMN [VehicleType] NVARCHAR(50) NULL;
    PRINT '✓ 已将 VehicleType 字段改为可空';
END
ELSE
BEGIN
    PRINT '✓ VehicleType 字段已是可空或不存在';
END
GO

-- ==============================================================================
-- 确保 VehiclePlateNumber 字段可为空（根据实体类定义）
-- ==============================================================================
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'VehiclePlateNumber'
    AND is_nullable = 0
)
BEGIN
    -- 修改 VehiclePlateNumber 为可空字段
    ALTER TABLE [dbo].[Transporters]
    ALTER COLUMN [VehiclePlateNumber] NVARCHAR(50) NULL;
    PRINT '✓ 已将 VehiclePlateNumber 字段改为可空';
END
ELSE
BEGIN
    PRINT '✓ VehiclePlateNumber 字段已是可空或不存在';
END
GO

-- ==============================================================================
-- 确保 TotalTrips 字段可为空（根据实体类定义）
-- ==============================================================================
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') 
    AND name = 'TotalTrips'
    AND is_nullable = 0
)
BEGIN
    -- 修改 TotalTrips 为可空字段
    ALTER TABLE [dbo].[Transporters]
    ALTER COLUMN [TotalTrips] INT NULL;
    PRINT '✓ 已将 TotalTrips 字段改为可空';
END
ELSE
BEGIN
    PRINT '✓ TotalTrips 字段已是可空或不存在';
END
GO

PRINT '';
PRINT '==============================================================================';
PRINT 'Transporters 表字段更新完成！';
PRINT 'Transporters Table Columns Update Complete!';
PRINT '==============================================================================';
PRINT '';
PRINT '字段列表 / Column List:';
PRINT '- TransporterID (主键)';
PRINT '- Username';
PRINT '- PasswordHash';
PRINT '- FullName';
PRINT '- PhoneNumber';
PRINT '- IDNumber';
PRINT '- VehicleType (可空)';
PRINT '- VehiclePlateNumber (可空)';
PRINT '- VehicleCapacity';
PRINT '- LicenseNumber (可空) ✓';
PRINT '- Region';
PRINT '- Available';
PRINT '- CurrentStatus';
PRINT '- TotalTrips (可空) ✓';
PRINT '- TotalWeight';
PRINT '- Rating';
PRINT '- CreatedDate';
PRINT '- LastLoginDate';
PRINT '- IsActive';
PRINT '- AvatarURL (可空) ✓';
PRINT '- Notes (可空) ✓';
PRINT '- money (可空) ✓';
PRINT '';
GO

-- ==============================================================================
-- 验证所有字段是否存在
-- ==============================================================================
PRINT '验证字段是否存在...';
PRINT '';

DECLARE @MissingColumns TABLE (ColumnName NVARCHAR(50));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'LicenseNumber')
    INSERT INTO @MissingColumns VALUES ('LicenseNumber');

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'TotalTrips')
    INSERT INTO @MissingColumns VALUES ('TotalTrips');

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'AvatarURL')
    INSERT INTO @MissingColumns VALUES ('AvatarURL');

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Notes')
    INSERT INTO @MissingColumns VALUES ('Notes');

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'money')
    INSERT INTO @MissingColumns VALUES ('money');

IF EXISTS (SELECT * FROM @MissingColumns)
BEGIN
    PRINT '⚠ 警告: 以下字段仍然缺失:';
    SELECT ColumnName FROM @MissingColumns;
END
ELSE
BEGIN
    PRINT '✓ 所有必需字段都已存在！';
END

PRINT '';
PRINT '==============================================================================';
PRINT '脚本执行完成 / Script Execution Complete';
PRINT '==============================================================================';
GO
