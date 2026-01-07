#!/bin/bash

echo "========================================"
echo "钱包表修复工具"
echo "Wallet Tables Fix Tool"
echo "========================================"
echo ""
echo "此脚本将自动修复"我的钱包"功能的数据库问题"
echo "This script will automatically fix the wallet database issue"
echo ""

# 设置数据库名称
DB_NAME="RecyclingSystemDB"
SCRIPT_PATH="$(dirname "$0")/AddWalletTablesToExistingDatabase.sql"

# 询问认证方式
echo "请选择 SQL Server 认证方式："
echo "Please select SQL Server authentication method:"
echo "1. Windows 身份验证 (Windows Authentication) - 仅适用于 Windows 系统"
echo "2. SQL Server 身份验证 (SQL Server Authentication)"
echo ""
read -p "请输入选项 (1 或 2 / Enter option 1 or 2): " auth_choice

if [ "$auth_choice" = "1" ]; then
    # Windows 身份验证
    AUTH_PARAMS="-E"
    echo ""
    echo "使用 Windows 身份验证..."
    echo "Using Windows Authentication..."
elif [ "$auth_choice" = "2" ]; then
    # SQL Server 身份验证
    echo ""
    read -p "请输入 SQL Server 用户名 (Enter SQL Server username): " SQL_USER
    read -sp "请输入密码 (Enter password): " SQL_PASS
    echo ""
    AUTH_PARAMS="-U $SQL_USER -P $SQL_PASS"
    echo "使用 SQL Server 身份验证..."
    echo "Using SQL Server Authentication..."
else
    echo "❌ 无效选项，退出"
    echo "❌ Invalid option, exiting"
    exit 1
fi

echo ""
echo "按 Enter 键开始修复..."
echo "Press Enter to start the fix..."
read

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
sqlcmd -S localhost $AUTH_PARAMS -Q "SELECT @@VERSION" > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 错误：无法连接到 SQL Server"
    echo "❌ Error: Cannot connect to SQL Server"
    echo ""
    echo "可能的原因："
    echo "- SQL Server 服务未启动"
    echo "- 服务器名称不正确（尝试使用 '.' 或具体的服务器名）"
    echo "- 认证信息不正确"
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
DB_EXISTS=$(sqlcmd -S localhost $AUTH_PARAMS -d master -Q "SELECT name FROM sys.databases WHERE name='$DB_NAME'" -h -1 -W 2>/dev/null | grep -c "$DB_NAME")
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
sqlcmd -S localhost $AUTH_PARAMS -d "$DB_NAME" -i "$SCRIPT_PATH"
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
