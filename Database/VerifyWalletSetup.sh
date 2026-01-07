#!/bin/bash

echo "======================================"
echo "钱包系统表验证脚本"
echo "Wallet Tables Verification Script"
echo "======================================"
echo ""

# 设置数据库连接参数
SERVER="."
DATABASE="RecyclingSystemDB"

echo "正在检查数据库连接和表状态..."
echo ""

# 创建临时SQL文件
cat > temp_verify_wallet.sql << 'EOF'
USE [RecyclingSystemDB];
GO

PRINT '========================================';
PRINT '钱包系统表验证结果';
PRINT '========================================';
PRINT '';

-- 检查 UserPaymentAccounts 表
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPaymentAccounts]') AND type in (N'U'))
BEGIN
    DECLARE @rowCount1 INT;
    SELECT @rowCount1 = COUNT(*) FROM [dbo].[UserPaymentAccounts];
    PRINT '✓ UserPaymentAccounts 表存在 (记录数: ' + CAST(@rowCount1 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '✗ UserPaymentAccounts 表不存在 - 需要创建';
END

-- 检查 WalletTransactions 表
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U'))
BEGIN
    DECLARE @rowCount2 INT;
    SELECT @rowCount2 = COUNT(*) FROM [dbo].[WalletTransactions];
    PRINT '✓ WalletTransactions 表存在 (记录数: ' + CAST(@rowCount2 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '✗ WalletTransactions 表不存在 - 需要创建';
END

-- 检查 Users.money 列
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'money')
BEGIN
    PRINT '✓ Users.money 列存在';
END
ELSE
BEGIN
    PRINT '✗ Users.money 列不存在 - 需要创建';
END

PRINT '';
PRINT '========================================';
PRINT '验证完成';
PRINT '如果有表不存在，请运行 SetupWalletTables.sh 进行修复';
PRINT '========================================';
GO
EOF

# 执行SQL脚本
sqlcmd -S "$SERVER" -d "$DATABASE" -i temp_verify_wallet.sql -E

# 删除临时文件
rm -f temp_verify_wallet.sql

echo ""
echo "======================================"
echo "验证完成"
echo "======================================"
echo ""
echo "如果表不存在，请运行: ./SetupWalletTables.sh"
echo ""
