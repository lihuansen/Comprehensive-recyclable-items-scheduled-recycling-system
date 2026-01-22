-- ==============================================================================
-- 验证 Transporters 表字段脚本
-- Verify Transporters Table Columns Script
-- 
-- 用途: 验证Transporters表是否包含所有必需的字段
-- Purpose: Verify that Transporters table contains all required columns
--
-- 创建日期: 2026-01-22
-- ==============================================================================

USE RecyclingDB;
GO

PRINT '';
PRINT '==============================================================================';
PRINT '开始验证 Transporters 表字段...';
PRINT 'Starting Transporters Table Columns Verification...';
PRINT '==============================================================================';
PRINT '';

-- ==============================================================================
-- 检查表是否存在
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND type in (N'U'))
BEGIN
    PRINT '✗ 错误: Transporters 表不存在！';
    PRINT '✗ ERROR: Transporters table does not exist!';
    PRINT '';
    PRINT '请先运行 CreateTransportersTable.sql 创建表。';
    PRINT 'Please run CreateTransportersTable.sql to create the table first.';
    PRINT '';
    RETURN;
END
ELSE
BEGIN
    PRINT '✓ Transporters 表存在';
    PRINT '✓ Transporters table exists';
    PRINT '';
END

-- ==============================================================================
-- 检查所有必需字段
-- ==============================================================================
DECLARE @AllColumnsExist BIT = 1;
DECLARE @MissingColumns TABLE (
    ColumnName NVARCHAR(50),
    DataType NVARCHAR(50),
    Description NVARCHAR(200)
);

-- 基础字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'TransporterID')
BEGIN
    INSERT INTO @MissingColumns VALUES ('TransporterID', 'INT', '运输人员ID（主键）');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Username')
BEGIN
    INSERT INTO @MissingColumns VALUES ('Username', 'NVARCHAR(50)', '用户名');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'PasswordHash')
BEGIN
    INSERT INTO @MissingColumns VALUES ('PasswordHash', 'NVARCHAR(255)', '密码哈希');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'FullName')
BEGIN
    INSERT INTO @MissingColumns VALUES ('FullName', 'NVARCHAR(100)', '真实姓名');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'PhoneNumber')
BEGIN
    INSERT INTO @MissingColumns VALUES ('PhoneNumber', 'NVARCHAR(20)', '手机号');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'IDNumber')
BEGIN
    INSERT INTO @MissingColumns VALUES ('IDNumber', 'NVARCHAR(18)', '身份证号');
    SET @AllColumnsExist = 0;
END

-- 新增字段（重点检查）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'LicenseNumber')
BEGIN
    INSERT INTO @MissingColumns VALUES ('LicenseNumber', 'NVARCHAR(50)', '⚠ 驾驶证号（新字段）');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'TotalTrips')
BEGIN
    INSERT INTO @MissingColumns VALUES ('TotalTrips', 'INT', '⚠ 总运输次数（新字段）');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'AvatarURL')
BEGIN
    INSERT INTO @MissingColumns VALUES ('AvatarURL', 'NVARCHAR(255)', '⚠ 头像URL（新字段）');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Notes')
BEGIN
    INSERT INTO @MissingColumns VALUES ('Notes', 'NVARCHAR(500)', '⚠ 备注信息（新字段）');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'money')
BEGIN
    INSERT INTO @MissingColumns VALUES ('money', 'DECIMAL(18,2)', '⚠ 账户余额（新字段）');
    SET @AllColumnsExist = 0;
END

-- 其他必需字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Region')
BEGIN
    INSERT INTO @MissingColumns VALUES ('Region', 'NVARCHAR(100)', '负责区域');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'Available')
BEGIN
    INSERT INTO @MissingColumns VALUES ('Available', 'BIT', '是否可接任务');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'CurrentStatus')
BEGIN
    INSERT INTO @MissingColumns VALUES ('CurrentStatus', 'NVARCHAR(20)', '当前状态');
    SET @AllColumnsExist = 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND name = 'IsActive')
BEGIN
    INSERT INTO @MissingColumns VALUES ('IsActive', 'BIT', '是否激活');
    SET @AllColumnsExist = 0;
END

-- ==============================================================================
-- 显示验证结果
-- ==============================================================================
IF @AllColumnsExist = 1
BEGIN
    PRINT '==============================================================================';
    PRINT '✓✓✓ 验证通过！所有必需字段都已存在！';
    PRINT '✓✓✓ Verification PASSED! All required columns exist!';
    PRINT '==============================================================================';
    PRINT '';
    PRINT '运输人员账号管理功能应该可以正常使用。';
    PRINT 'Transporter account management should work properly.';
    PRINT '';
END
ELSE
BEGIN
    PRINT '==============================================================================';
    PRINT '✗✗✗ 验证失败！发现缺失字段！';
    PRINT '✗✗✗ Verification FAILED! Missing columns detected!';
    PRINT '==============================================================================';
    PRINT '';
    PRINT '缺失的字段列表:';
    PRINT 'Missing columns list:';
    PRINT '';
    
    SELECT 
        ColumnName AS [字段名],
        DataType AS [数据类型],
        Description AS [说明]
    FROM @MissingColumns
    ORDER BY ColumnName;
    
    PRINT '';
    PRINT '解决方案:';
    PRINT 'Solution:';
    PRINT '请运行 UpdateTransportersTableColumns.sql 脚本添加缺失的字段。';
    PRINT 'Please run UpdateTransportersTableColumns.sql script to add missing columns.';
    PRINT '';
    PRINT '或使用快捷脚本:';
    PRINT 'Or use quick script:';
    PRINT '  Windows: UpdateTransportersColumns.bat';
    PRINT '  Linux/Mac: ./UpdateTransportersColumns.sh';
    PRINT '';
END

-- ==============================================================================
-- 显示当前表结构（供参考）
-- ==============================================================================
PRINT '';
PRINT '==============================================================================';
PRINT '当前 Transporters 表结构:';
PRINT 'Current Transporters Table Structure:';
PRINT '==============================================================================';
PRINT '';

SELECT 
    COLUMN_NAME AS [列名],
    DATA_TYPE AS [数据类型],
    CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL 
        THEN DATA_TYPE + '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
        WHEN NUMERIC_PRECISION IS NOT NULL
        THEN DATA_TYPE + '(' + CAST(NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(NUMERIC_SCALE AS VARCHAR) + ')'
        ELSE DATA_TYPE
    END AS [完整类型],
    IS_NULLABLE AS [可空],
    COLUMN_DEFAULT AS [默认值]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Transporters'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '==============================================================================';
PRINT '验证完成';
PRINT 'Verification Complete';
PRINT '==============================================================================';
GO
