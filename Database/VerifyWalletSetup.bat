@echo off
chcp 65001 >nul
echo ======================================
echo 钱包系统表验证脚本
echo Wallet Tables Verification Script
echo ======================================
echo.

REM 设置数据库连接参数
set SERVER=localhost
set DATABASE=RecyclingSystemDB

echo 正在检查数据库连接和表状态...
echo.

REM 创建临时SQL文件
echo USE [%DATABASE%]; > temp_verify_wallet.sql
echo GO >> temp_verify_wallet.sql
echo. >> temp_verify_wallet.sql
echo PRINT '========================================'; >> temp_verify_wallet.sql
echo PRINT '钱包系统表验证结果'; >> temp_verify_wallet.sql
echo PRINT '========================================'; >> temp_verify_wallet.sql
echo PRINT ''; >> temp_verify_wallet.sql
echo. >> temp_verify_wallet.sql
echo -- 检查 UserPaymentAccounts 表 >> temp_verify_wallet.sql
echo IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPaymentAccounts]') AND type in (N'U')) >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     DECLARE @rowCount1 INT; >> temp_verify_wallet.sql
echo     SELECT @rowCount1 = COUNT(*) FROM [dbo].[UserPaymentAccounts]; >> temp_verify_wallet.sql
echo     PRINT '✓ UserPaymentAccounts 表存在 (记录数: ' + CAST(@rowCount1 AS VARCHAR) + ')'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo ELSE >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     PRINT '✗ UserPaymentAccounts 表不存在 - 需要创建'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo. >> temp_verify_wallet.sql
echo -- 检查 WalletTransactions 表 >> temp_verify_wallet.sql
echo IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U')) >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     DECLARE @rowCount2 INT; >> temp_verify_wallet.sql
echo     SELECT @rowCount2 = COUNT(*) FROM [dbo].[WalletTransactions]; >> temp_verify_wallet.sql
echo     PRINT '✓ WalletTransactions 表存在 (记录数: ' + CAST(@rowCount2 AS VARCHAR) + ')'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo ELSE >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     PRINT '✗ WalletTransactions 表不存在 - 需要创建'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo. >> temp_verify_wallet.sql
echo -- 检查 Users.money 列 >> temp_verify_wallet.sql
echo IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'money') >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     PRINT '✓ Users.money 列存在'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo ELSE >> temp_verify_wallet.sql
echo BEGIN >> temp_verify_wallet.sql
echo     PRINT '✗ Users.money 列不存在 - 需要创建'; >> temp_verify_wallet.sql
echo END >> temp_verify_wallet.sql
echo. >> temp_verify_wallet.sql
echo PRINT ''; >> temp_verify_wallet.sql
echo PRINT '========================================'; >> temp_verify_wallet.sql
echo PRINT '验证完成'; >> temp_verify_wallet.sql
echo PRINT '如果有表不存在，请运行 SetupWalletTables.bat 进行修复'; >> temp_verify_wallet.sql
echo PRINT '========================================'; >> temp_verify_wallet.sql
echo GO >> temp_verify_wallet.sql

REM 执行SQL脚本
sqlcmd -S %SERVER% -d %DATABASE% -i temp_verify_wallet.sql -E

REM 删除临时文件
del temp_verify_wallet.sql

echo.
echo ======================================
echo 验证完成
echo ======================================
echo.
echo 如果表不存在，请运行: SetupWalletTables.bat
echo.
pause
