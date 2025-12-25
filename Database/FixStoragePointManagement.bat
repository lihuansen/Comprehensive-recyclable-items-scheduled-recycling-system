@echo off
REM ========================================================================
REM 快速修复暂存点管理功能 - 创建Inventory表
REM Quick Fix for Storage Point Management - Create Inventory Table
REM ========================================================================

echo ========================================================================
echo 暂存点管理功能 - 数据库表创建脚本
echo Storage Point Management - Database Table Setup
echo ========================================================================
echo.

REM 检查SQL Server连接
echo 正在检查SQL Server连接...
sqlcmd -S localhost -Q "SELECT @@VERSION" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [错误] 无法连接到SQL Server
    echo 请确认：
    echo 1. SQL Server服务正在运行
    echo 2. 使用正确的服务器名称（默认：localhost）
    echo.
    echo 如果您的SQL Server实例不是localhost，请修改此脚本中的服务器名称
    echo.
    pause
    exit /b 1
)

echo [成功] SQL Server连接正常
echo.

REM 检查数据库是否存在
echo 正在检查RecyclingSystemDB数据库...
sqlcmd -S localhost -d RecyclingSystemDB -Q "SELECT DB_NAME()" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [错误] RecyclingSystemDB数据库不存在
    echo 请先创建数据库或修改此脚本中的数据库名称
    echo.
    pause
    exit /b 1
)

echo [成功] RecyclingSystemDB数据库存在
echo.

REM 创建Inventory表
echo 正在创建Inventory表...
echo.
sqlcmd -S localhost -d RecyclingSystemDB -i CreateInventoryTable.sql
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [错误] Inventory表创建失败
    echo 请检查CreateInventoryTable.sql文件是否存在
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================================================
echo [完成] Inventory表创建成功！
echo ========================================================================
echo.
echo 暂存点管理功能现在应该可以正常使用了。
echo.
echo 下一步：
echo 1. 以回收员身份登录系统
echo 2. 完成一个订单（确保订单包含类别和重量信息）
echo 3. 点击"暂存点管理"查看库存数据
echo.
echo 如果仍然遇到问题，请参考：
echo - STORAGE_POINT_TROUBLESHOOTING.md（故障排查指南）
echo - DATABASE_SETUP_INSTRUCTIONS.md（数据库设置说明）
echo.
pause
