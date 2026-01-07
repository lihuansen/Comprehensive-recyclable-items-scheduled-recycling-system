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
echo "服务器: . (本地SQL Server)"
echo ""

# 设置数据库连接参数
SERVER="."
DATABASE="RecyclingSystemDB"
SQL_SCRIPT="CreateWalletTables.sql"

# 检查SQL脚本文件是否存在
if [ ! -f "$SQL_SCRIPT" ]; then
    echo "错误: 找不到 $SQL_SCRIPT 文件"
    echo "请确保您在 Database 目录下运行此脚本"
    exit 1
fi

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
