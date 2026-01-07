#!/bin/bash

echo "========================================"
echo "钱包表修复工具"
echo "Wallet Tables Fix Tool"
echo "========================================"
echo ""
echo "此脚本将自动修复"我的钱包"功能的数据库问题"
echo "This script will automatically fix the wallet database issue"
echo ""
echo "按 Enter 键开始修复..."
echo "Press Enter to start the fix..."
read

# 设置数据库名称
DB_NAME="RecyclingSystemDB"
SCRIPT_PATH="$(dirname "$0")/AddWalletTablesToExistingDatabase.sql"

echo "[步骤 1/3] 检查 SQL Server 连接..."
echo "[Step 1/3] Checking SQL Server connection..."

# 检查 sqlcmd 是否可用
if ! command -v sqlcmd &> /dev/null; then
    echo ""
    echo "❌ 错误：sqlcmd 未安装"
    echo "❌ Error: sqlcmd is not installed"
    echo ""
    echo "请安装 SQL Server 命令行工具："
    echo "Please install SQL Server command-line tools:"
    echo "https://docs.microsoft.com/sql/linux/sql-server-linux-setup-tools"
    echo ""
    echo "或手动执行修复脚本："
    echo "1. 打开 SQL Server Management Studio"
    echo "2. 执行脚本：AddWalletTablesToExistingDatabase.sql"
    echo ""
    read -p "按 Enter 键退出..."
    exit 1
fi

# 尝试连接到 SQL Server
sqlcmd -S localhost -E -Q "SELECT @@VERSION" > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 错误：无法连接到 SQL Server"
    echo "❌ Error: Cannot connect to SQL Server"
    echo ""
    echo "可能的原因："
    echo "- SQL Server 服务未启动"
    echo "- 连接参数不正确"
    echo "- 需要使用 SQL 身份验证"
    echo ""
    echo "请手动执行修复脚本："
    echo "1. 打开 SQL Server Management Studio"
    echo "2. 执行脚本：AddWalletTablesToExistingDatabase.sql"
    echo ""
    read -p "按 Enter 键退出..."
    exit 1
fi
echo "✅ SQL Server 连接成功"
echo ""

echo "[步骤 2/3] 检查数据库是否存在..."
echo "[Step 2/3] Checking if database exists..."

# 检查数据库是否存在
DB_EXISTS=$(sqlcmd -S localhost -E -d master -Q "SELECT name FROM sys.databases WHERE name='$DB_NAME'" -h -1 -W 2>/dev/null | grep -c "$DB_NAME")
if [ "$DB_EXISTS" -eq 0 ]; then
    echo ""
    echo "❌ 错误：数据库 $DB_NAME 不存在"
    echo "❌ Error: Database $DB_NAME does not exist"
    echo ""
    echo "请检查："
    echo "1. 数据库名称是否正确"
    echo "2. 是否需要先创建数据库"
    echo ""
    echo "查看 Web.config 中的连接字符串确认数据库名称"
    echo ""
    read -p "按 Enter 键退出..."
    exit 1
fi
echo "✅ 数据库 $DB_NAME 存在"
echo ""

echo "[步骤 3/3] 执行修复脚本..."
echo "[Step 3/3] Executing fix script..."
echo ""

# 执行修复脚本
sqlcmd -S localhost -E -d "$DB_NAME" -i "$SCRIPT_PATH"
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 脚本执行失败"
    echo "❌ Script execution failed"
    echo ""
    echo "请手动在 SSMS 中执行脚本"
    echo ""
    read -p "按 Enter 键退出..."
    exit 1
fi

echo ""
echo "========================================"
echo "✅✅✅ 修复完成！"
echo "✅✅✅ Fix completed!"
echo "========================================"
echo ""
echo "下一步："
echo "1. 重启应用程序"
echo "2. 登录系统"
echo "3. 访问"我的钱包"页面测试"
echo ""
echo "Next steps:"
echo "1. Restart your application"
echo "2. Login to the system"
echo "3. Visit 'My Wallet' page to test"
echo ""
read -p "按 Enter 键退出..."
