#!/bin/bash
# ==============================================================================
# 运行 UpdateTransportersTableColumns.sql 脚本
# Run UpdateTransportersTableColumns.sql Script
# 
# 用途: 更新Transporters表结构，添加缺失的字段
# Purpose: Update Transporters table structure, add missing columns
# ==============================================================================

echo ""
echo "========================================================================"
echo "运输人员表字段更新工具"
echo "Transporters Table Columns Update Tool"
echo "========================================================================"
echo ""

# 获取当前脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# 设置SQL文件路径
SQL_FILE="$SCRIPT_DIR/UpdateTransportersTableColumns.sql"

# 检查SQL文件是否存在
if [ ! -f "$SQL_FILE" ]; then
    echo "[错误] 找不到SQL文件: $SQL_FILE"
    echo "[ERROR] SQL file not found: $SQL_FILE"
    exit 1
fi

echo "[信息] 找到SQL文件: $SQL_FILE"
echo "[INFO] Found SQL file: $SQL_FILE"
echo ""

# 提示用户输入数据库连接信息
echo "请确认数据库连接信息:"
echo "Please confirm database connection information:"
echo ""
echo "默认服务器: localhost"
echo "Default Server: localhost"
echo ""
echo "默认数据库: RecyclingDB"
echo "Default Database: RecyclingDB"
echo ""

read -p "请输入SQL Server服务器名称 (直接回车使用 localhost): " SERVER_NAME
SERVER_NAME=${SERVER_NAME:-localhost}

read -p "请输入数据库名称 (直接回车使用 RecyclingDB): " DB_NAME
DB_NAME=${DB_NAME:-RecyclingDB}

read -p "请输入用户名: " DB_USER
read -sp "请输入密码: " DB_PASSWORD
echo ""

echo ""
echo "========================================================================"
echo "验证数据库连接..."
echo "Verifying database connection..."
echo "========================================================================"
echo ""

# 测试连接
if command -v sqlcmd &> /dev/null; then
    # 尝试连接数据库
    sqlcmd -S "$SERVER_NAME" -d "$DB_NAME" -U "$DB_USER" -P "$DB_PASSWORD" -Q "SELECT 1" &> /dev/null
    if [ $? -ne 0 ]; then
        echo "[错误] 无法连接到数据库。请检查连接信息。"
        echo "[ERROR] Cannot connect to database. Please check connection information."
        exit 1
    fi
    echo "[成功] 数据库连接验证通过"
    echo "[SUCCESS] Database connection verified"
else
    echo "[错误] 未找到 sqlcmd 命令。请安装 SQL Server 命令行工具。"
    echo "[ERROR] sqlcmd command not found. Please install SQL Server command line tools."
    exit 1
fi

echo ""
echo "========================================================================"
echo "开始执行更新脚本..."
echo "Starting script execution..."
echo "========================================================================"
echo ""

# 执行SQL脚本
sqlcmd -S "$SERVER_NAME" -d "$DB_NAME" -U "$DB_USER" -P "$DB_PASSWORD" -i "$SQL_FILE"
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo ""
    echo "========================================================================"
    echo "[成功] 脚本执行成功！"
    echo "[SUCCESS] Script executed successfully!"
    echo "========================================================================"
    echo ""
    echo "Transporters表已更新，现在包含以下新字段:"
    echo "Transporters table has been updated with the following new columns:"
    echo ""
    echo "  - LicenseNumber    (驾驶证号)"
    echo "  - TotalTrips       (总运输次数)"
    echo "  - AvatarURL        (头像URL)"
    echo "  - Notes            (备注信息)"
    echo "  - money            (账户余额)"
    echo ""
else
    echo ""
    echo "========================================================================"
    echo "[错误] 脚本执行失败！错误代码: $EXIT_CODE"
    echo "[ERROR] Script execution failed! Error code: $EXIT_CODE"
    echo "========================================================================"
    echo ""
    echo "可能的原因:"
    echo "Possible reasons:"
    echo "  1. 数据库服务器连接失败"
    echo "  2. 数据库不存在"
    echo "  3. 权限不足"
    echo "  4. SQL语法错误"
    echo ""
    exit $EXIT_CODE
fi
