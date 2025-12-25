#!/bin/bash
################################################################################
# 快速修复暂存点管理功能 - 创建Inventory表
# Quick Fix for Storage Point Management - Create Inventory Table
################################################################################

echo "========================================================================"
echo "暂存点管理功能 - 数据库表创建脚本"
echo "Storage Point Management - Database Table Setup"
echo "========================================================================"
echo ""

# 配置变量（根据需要修改）
SERVER="localhost"
DATABASE="RecyclingSystemDB"
SQL_FILE="CreateInventoryTable.sql"

# 检查sqlcmd是否可用
if ! command -v sqlcmd &> /dev/null; then
    echo ""
    echo "[错误] sqlcmd 命令不可用"
    echo "请安装 SQL Server 命令行工具"
    echo "- Ubuntu/Debian: sudo apt-get install mssql-tools"
    echo "- macOS: brew install microsoft/mssql-release/mssql-tools"
    echo ""
    exit 1
fi

# 检查SQL文件是否存在
if [ ! -f "$SQL_FILE" ]; then
    echo ""
    echo "[错误] 找不到文件: $SQL_FILE"
    echo "请确保在 Database 目录中运行此脚本"
    echo ""
    exit 1
fi

# 检查SQL Server连接
echo "正在检查SQL Server连接..."
if ! sqlcmd -S "$SERVER" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
    echo ""
    echo "[错误] 无法连接到SQL Server"
    echo "请确认："
    echo "1. SQL Server服务正在运行"
    echo "2. 使用正确的服务器名称（当前：$SERVER）"
    echo ""
    echo "如果需要修改服务器名称，请编辑此脚本"
    echo ""
    exit 1
fi

echo "[成功] SQL Server连接正常"
echo ""

# 检查数据库是否存在
echo "正在检查 $DATABASE 数据库..."
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT DB_NAME()" > /dev/null 2>&1; then
    echo ""
    echo "[错误] $DATABASE 数据库不存在"
    echo "请先创建数据库或修改此脚本中的数据库名称"
    echo ""
    exit 1
fi

echo "[成功] $DATABASE 数据库存在"
echo ""

# 创建Inventory表
echo "正在创建Inventory表..."
echo ""
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -i "$SQL_FILE"; then
    echo ""
    echo "[错误] Inventory表创建失败"
    echo "请检查SQL文件和数据库权限"
    echo ""
    exit 1
fi

echo ""
echo "========================================================================"
echo "[完成] Inventory表创建成功！"
echo "========================================================================"
echo ""
echo "暂存点管理功能现在应该可以正常使用了。"
echo ""
echo "下一步："
echo "1. 以回收员身份登录系统"
echo "2. 完成一个订单（确保订单包含类别和重量信息）"
echo "3. 点击'暂存点管理'查看库存数据"
echo ""
echo "如果仍然遇到问题，请参考："
echo "- STORAGE_POINT_TROUBLESHOOTING.md（故障排查指南）"
echo "- DATABASE_SETUP_INSTRUCTIONS.md（数据库设置说明）"
echo ""
