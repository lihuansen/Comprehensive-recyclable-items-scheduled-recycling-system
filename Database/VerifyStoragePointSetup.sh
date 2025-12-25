#!/bin/bash
################################################################################
# 暂存点管理功能 - 快速验证脚本
# Storage Point Management - Quick Verification Script
################################################################################

# 配置变量
SERVER="localhost"
DATABASE="RecyclingSystemDB"

# 颜色定义
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================================================"
echo "暂存点管理功能 - 验证检查"
echo "Storage Point Management - Verification Check"
echo "========================================================================"
echo ""

# 检查sqlcmd是否可用
if ! command -v sqlcmd &> /dev/null; then
    echo -e "${RED}[X] 失败 - sqlcmd命令不可用${NC}"
    echo "    请安装SQL Server命令行工具"
    echo "    - Ubuntu/Debian: sudo apt-get install mssql-tools"
    echo "    - macOS: brew install microsoft/mssql-release/mssql-tools"
    echo ""
    exit 1
fi

# 检查SQL Server连接
echo "[1/4] 检查SQL Server连接..."
if ! sqlcmd -S "$SERVER" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
    echo -e "${RED}[X] 失败 - 无法连接到SQL Server${NC}"
    echo "    请确认SQL Server服务正在运行"
    echo ""
    exit 1
fi
echo -e "${GREEN}[✓] 成功 - SQL Server连接正常${NC}"
echo ""

# 检查数据库是否存在
echo "[2/4] 检查$DATABASE数据库..."
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT DB_NAME()" > /dev/null 2>&1; then
    echo -e "${RED}[X] 失败 - $DATABASE数据库不存在${NC}"
    echo "    请先创建数据库"
    echo ""
    exit 1
fi
echo -e "${GREEN}[✓] 成功 - $DATABASE数据库存在${NC}"
echo ""

# 检查Inventory表是否存在
echo "[3/4] 检查Inventory表..."
TABLE_EXISTS=$(sqlcmd -S "$SERVER" -d "$DATABASE" -Q "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventory') PRINT 'EXISTS' ELSE PRINT 'NOT_EXISTS'" -h -1 -W 2>&1 | tr -d '\r\n' | xargs)

if [[ "$TABLE_EXISTS" != *"EXISTS"* ]]; then
    echo -e "${RED}[X] 失败 - Inventory表不存在${NC}"
    echo "    这是导致网络错误的原因！"
    echo ""
    echo "    请运行以下脚本创建表："
    echo "    ./FixStoragePointManagement.sh"
    echo ""
    exit 1
fi
echo -e "${GREEN}[✓] 成功 - Inventory表已存在${NC}"
echo ""

# 检查表结构
echo "[4/4] 检查Inventory表结构..."
COLUMN_COUNT=$(sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory'" -h -1 -W 2>&1 | grep -o '[0-9]\+' | head -1)

if [ -z "$COLUMN_COUNT" ]; then
    echo -e "${RED}[X] 失败 - 无法读取表结构${NC}"
    exit 1
fi

if [ "$COLUMN_COUNT" -lt 7 ]; then
    echo -e "${YELLOW}[!] 警告 - Inventory表列数不足 (当前: $COLUMN_COUNT, 预期: 8)${NC}"
    echo "    表可能不完整，建议重新创建"
    exit 1
fi
echo -e "${GREEN}[✓] 成功 - Inventory表结构正确 ($COLUMN_COUNT 列)${NC}"
echo ""

# 显示表信息
echo "========================================================================"
echo "表信息详情："
echo "========================================================================"
sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT COLUMN_NAME + ' | ' + DATA_TYPE AS Info FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' ORDER BY ORDINAL_POSITION" -h -1 -W 2>&1 | grep -v "^$"
echo ""

# 检查数据量
echo "========================================================================"
echo "当前库存数据统计："
echo "========================================================================"
RECORD_COUNT=$(sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT COUNT(*) FROM Inventory" -h -1 -W 2>&1 | grep -o '[0-9]\+' | head -1)
echo "总记录数: $RECORD_COUNT"
echo ""

# 成功完成
echo "========================================================================"
echo -e "${GREEN}[✓] 验证完成 - 所有检查通过！${NC}"
echo "========================================================================"
echo ""
echo "暂存点管理功能应该可以正常使用。"
echo ""
echo "如果仍然遇到网络错误，请："
echo "1. 检查浏览器控制台 (F12) 查看详细错误"
echo "2. 确认以回收员身份登录"
echo "3. 查看 STORAGE_POINT_TROUBLESHOOTING.md 获取更多帮助"
echo ""

exit 0
