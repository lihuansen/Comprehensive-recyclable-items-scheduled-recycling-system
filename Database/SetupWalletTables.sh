#!/bin/bash

echo "======================================"
echo "钱包系统表创建脚本"
echo "Wallet Tables Setup Script"
echo "======================================"
echo ""
echo "此脚本将为您创建以下数据库表："
echo "1. UserPaymentAccounts - 用户支付账户表"
echo "2. WalletTransactions - 钱包交易记录表"
echo "3. Users.money - 用户余额字段（如不存在）"
echo ""
echo "数据库: RecyclingSystemDB"
echo "服务器: localhost"
echo ""

# 设置数据库连接参数
SERVER="localhost"
DATABASE="RecyclingSystemDB"
SQL_SCRIPT="CreateWalletTables.sql"

# 检查SQL脚本文件是否存在
if [ ! -f "$SQL_SCRIPT" ]; then
    echo "错误: 找不到 $SQL_SCRIPT 文件"
    echo "请确保您在 Database 目录下运行此脚本"
    exit 1
fi

# 检查SQL Server连接
echo "正在检查SQL Server连接..."
sqlcmd -S "$SERVER" -Q "SELECT @@VERSION" > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo ""
    echo "错误: 无法连接到SQL Server"
    echo "请确认："
    echo "1. SQL Server服务正在运行"
    echo "2. 使用正确的服务器名称（默认：localhost）"
    echo ""
    exit 1
fi

echo "SQL Server连接正常"
echo ""

# 检查数据库是否存在
echo "正在检查数据库 $DATABASE ..."
sqlcmd -S "$SERVER" -Q "IF DB_ID('$DATABASE') IS NOT NULL SELECT 1" > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo ""
    echo "错误: 数据库 $DATABASE 不存在"
    echo "请先创建数据库或修改此脚本中的数据库名称"
    echo ""
    exit 1
fi

echo "数据库存在"
echo ""

echo "正在执行 SQL 脚本..."
echo ""

# 执行SQL脚本
sqlcmd -S "$SERVER" -d "$DATABASE" -i "$SQL_SCRIPT" -E

if [ $? -eq 0 ]; then
    echo ""
    echo "======================================"
    echo "✓ 钱包系统表创建成功！"
    echo "======================================"
    echo ""
    echo "已成功创建以下表："
    echo "- UserPaymentAccounts 用户支付账户表"
    echo "- WalletTransactions 钱包交易记录表"
    echo "- Users.money 用户余额字段"
    echo ""
    echo "现在您可以正常使用"我的钱包"功能了！"
    echo ""
else
    echo ""
    echo "======================================"
    echo "✗ 创建失败"
    echo "======================================"
    echo ""
    echo "可能的原因："
    echo "1. SQL Server 服务未启动"
    echo "2. 数据库 RecyclingSystemDB 不存在"
    echo "3. 没有足够的权限"
    echo ""
    echo "请检查上面的错误信息并修复问题后重试"
    echo ""
fi
