@echo off
REM ==============================================================================
REM 运行 UpdateTransportersTableColumns.sql 脚本
REM Run UpdateTransportersTableColumns.sql Script
REM 
REM 用途: 更新Transporters表结构，添加缺失的字段
REM Purpose: Update Transporters table structure, add missing columns
REM ==============================================================================

echo.
echo ========================================================================
echo 运输人员表字段更新工具
echo Transporters Table Columns Update Tool
echo ========================================================================
echo.

REM 获取当前脚本所在目录
set SCRIPT_DIR=%~dp0

REM 设置SQL文件路径
set SQL_FILE=%SCRIPT_DIR%UpdateTransportersTableColumns.sql

REM 检查SQL文件是否存在
if not exist "%SQL_FILE%" (
    echo [错误] 找不到SQL文件: %SQL_FILE%
    echo [ERROR] SQL file not found: %SQL_FILE%
    pause
    exit /b 1
)

echo [信息] 找到SQL文件: %SQL_FILE%
echo [INFO] Found SQL file: %SQL_FILE%
echo.

REM 提示用户输入数据库连接信息
echo 请确认数据库连接信息:
echo Please confirm database connection information:
echo.
echo 默认服务器: localhost
echo Default Server: localhost
echo.
echo 默认数据库: RecyclingDB
echo Default Database: RecyclingDB
echo.

set /p SERVER_NAME="请输入SQL Server服务器名称 (直接回车使用 localhost): "
if "%SERVER_NAME%"=="" set SERVER_NAME=localhost

set /p DB_NAME="请输入数据库名称 (直接回车使用 RecyclingDB): "
if "%DB_NAME%"=="" set DB_NAME=RecyclingDB

echo.
echo 请选择身份验证方式:
echo Please select authentication method:
echo   1. Windows 身份验证 (Windows Authentication)
echo   2. SQL Server 身份验证 (SQL Server Authentication)
echo.
set /p AUTH_TYPE="请输入 1 或 2 (直接回车使用 Windows 身份验证): "
if "%AUTH_TYPE%"=="" set AUTH_TYPE=1

if "%AUTH_TYPE%"=="2" (
    set /p SQL_USER="请输入SQL Server用户名: "
    set /p SQL_PASS="请输入SQL Server密码: "
)

echo.
echo ========================================================================
echo 开始执行更新脚本...
echo Starting script execution...
echo ========================================================================
echo.

REM 执行SQL脚本
if "%AUTH_TYPE%"=="2" (
    REM 使用SQL Server身份验证
    sqlcmd -S %SERVER_NAME% -d %DB_NAME% -U %SQL_USER% -P %SQL_PASS% -i "%SQL_FILE%"
) else (
    REM 使用Windows身份验证
    sqlcmd -S %SERVER_NAME% -d %DB_NAME% -E -i "%SQL_FILE%"
)

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================================
    echo [成功] 脚本执行成功！
    echo [SUCCESS] Script executed successfully!
    echo ========================================================================
    echo.
    echo Transporters表已更新，现在包含以下新字段:
    echo Transporters table has been updated with the following new columns:
    echo.
    echo   - LicenseNumber    ^(驾驶证号^)
    echo   - TotalTrips       ^(总运输次数^)
    echo   - AvatarURL        ^(头像URL^)
    echo   - Notes            ^(备注信息^)
    echo   - money            ^(账户余额^)
    echo.
) else (
    echo.
    echo ========================================================================
    echo [错误] 脚本执行失败！错误代码: %ERRORLEVEL%
    echo [ERROR] Script execution failed! Error code: %ERRORLEVEL%
    echo ========================================================================
    echo.
    echo 可能的原因:
    echo Possible reasons:
    echo   1. 数据库服务器连接失败
    echo   2. 数据库不存在
    echo   3. 权限不足
    echo   4. SQL语法错误
    echo.
)

echo.
pause
