@echo off
chcp 65001 >nul
echo ====================================
echo 数据库表设置脚本
echo Database Tables Setup Script
echo ====================================
echo.

REM 设置数据库连接参数
set SERVER=localhost
set DATABASE=RecyclingDB

echo 正在连接到服务器: %SERVER%
echo 数据库: %DATABASE%
echo.

echo [1/2] 创建用户反馈表 (UserFeedback)...
sqlcmd -S %SERVER% -d %DATABASE% -E -i CreateUserFeedbackTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 用户反馈表创建失败！
    echo 请检查数据库连接和权限。
    pause
    exit /b 1
)
echo ✓ 用户反馈表创建成功
echo.

echo [2/2] 创建管理员联系表 (AdminContactConversations, AdminContactMessages)...
sqlcmd -S %SERVER% -d %DATABASE% -E -i CreateAdminContactMessagesTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 管理员联系表创建失败！
    echo 请检查数据库连接和权限。
    pause
    exit /b 1
)
echo ✓ 管理员联系表创建成功
echo.

echo ====================================
echo 所有必需的数据库表已成功创建！
echo All required database tables have been created successfully!
echo ====================================
echo.
echo 您现在可以使用以下功能：
echo 1. 问题反馈功能
echo 2. 联系管理员功能
echo.
pause
