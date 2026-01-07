-- ==============================================================================
-- 快速验证钱包表是否存在
-- Quick Verification for Wallet Tables
-- 
-- 使用说明：
-- 1. 在 SSMS 中连接到您的数据库
-- 2. 修改第 10 行的数据库名称（如果需要）
-- 3. 执行此脚本 (按 F5)
-- 4. 查看结果，了解哪些表缺失
-- ==============================================================================

USE RecyclingSystemDB;  -- 修改为您的数据库名称
GO

SET NOCOUNT ON;

PRINT '========================================';
PRINT '钱包表验证脚本';
PRINT 'Wallet Tables Verification';
PRINT '========================================';
PRINT '';
PRINT '当前数据库: ' + DB_NAME();
PRINT '';

-- 检查 Users.money 列
PRINT '【检查 1】Users 表的 money 列';
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'money')
BEGIN
    PRINT '  ✅ Users.money 列存在';
END
ELSE
BEGIN
    PRINT '  ❌ Users.money 列不存在 - 需要创建';
END
PRINT '';

-- 检查 UserPaymentAccounts 表
PRINT '【检查 2】UserPaymentAccounts 表';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'UserPaymentAccounts') AND type = 'U')
BEGIN
    DECLARE @count1 INT;
    SELECT @count1 = COUNT(*) FROM UserPaymentAccounts;
    PRINT '  ✅ UserPaymentAccounts 表存在 (记录数: ' + CAST(@count1 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '  ❌ UserPaymentAccounts 表不存在 - 需要创建';
END
PRINT '';

-- 检查 WalletTransactions 表
PRINT '【检查 3】WalletTransactions 表';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WalletTransactions') AND type = 'U')
BEGIN
    DECLARE @count2 INT;
    SELECT @count2 = COUNT(*) FROM WalletTransactions;
    PRINT '  ✅ WalletTransactions 表存在 (记录数: ' + CAST(@count2 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '  ❌ WalletTransactions 表不存在 - 需要创建';
END
PRINT '';

-- 总结
PRINT '========================================';
PRINT '验证结果总结';
PRINT '========================================';

DECLARE @allOk BIT = 1;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'money')
    SET @allOk = 0;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'UserPaymentAccounts') AND type = 'U')
    SET @allOk = 0;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WalletTransactions') AND type = 'U')
    SET @allOk = 0;

IF @allOk = 1
BEGIN
    PRINT '';
    PRINT '✅✅✅ 所有钱包表都已存在！';
    PRINT '✅✅✅ All wallet tables exist!';
    PRINT '';
    PRINT '钱包功能应该可以正常使用。';
    PRINT 'Wallet functionality should work properly.';
    PRINT '';
    PRINT '如果仍然有错误，请：';
    PRINT '1. 重启应用程序';
    PRINT '2. 检查 Web.config 连接字符串';
    PRINT '3. 清除浏览器缓存';
END
ELSE
BEGIN
    PRINT '';
    PRINT '❌❌❌ 检测到缺失的表或列！';
    PRINT '❌❌❌ Missing tables or columns detected!';
    PRINT '';
    PRINT '请执行修复脚本：';
    PRINT 'Please run the fix script:';
    PRINT '  Database/AddWalletTablesToExistingDatabase.sql';
    PRINT '';
    PRINT '或查看详细修复指南：';
    PRINT 'Or see detailed fix guide:';
    PRINT '  FIX_WALLET_ERROR_IMMEDIATE.md';
END

PRINT '';
PRINT '========================================';

GO
