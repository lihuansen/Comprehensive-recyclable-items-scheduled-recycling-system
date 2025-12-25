@echo off
REM ========================================================================
REM 暂存点管理功能 - 快速验证脚本
REM Storage Point Management - Quick Verification Script
REM ========================================================================

echo ========================================================================
echo 暂存点管理功能 - 验证检查
echo Storage Point Management - Verification Check
echo ========================================================================
echo.

REM 检查SQL Server连接
echo [1/4] 检查SQL Server连接...
sqlcmd -S localhost -Q "SELECT @@VERSION" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [X] 失败 - 无法连接到SQL Server
    echo     请确认SQL Server服务正在运行
    goto :error
)
echo [✓] 成功 - SQL Server连接正常
echo.

REM 检查数据库是否存在
echo [2/4] 检查RecyclingSystemDB数据库...
sqlcmd -S localhost -d RecyclingSystemDB -Q "SELECT DB_NAME()" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [X] 失败 - RecyclingSystemDB数据库不存在
    echo     请先创建数据库
    goto :error
)
echo [✓] 成功 - RecyclingSystemDB数据库存在
echo.

REM 检查Inventory表是否存在
echo [3/4] 检查Inventory表...
sqlcmd -S localhost -d RecyclingSystemDB -Q "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventory') PRINT 'EXISTS' ELSE PRINT 'NOT_EXISTS'" -h -1 > temp_check.txt 2>&1
findstr /C:"EXISTS" temp_check.txt >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [X] 失败 - Inventory表不存在
    echo     这是导致网络错误的原因！
    echo.
    echo     请运行以下脚本创建表：
    echo     FixStoragePointManagement.bat
    del temp_check.txt >nul 2>&1
    goto :error
)
del temp_check.txt >nul 2>&1
echo [✓] 成功 - Inventory表已存在
echo.

REM 检查表结构
echo [4/4] 检查Inventory表结构...
sqlcmd -S localhost -d RecyclingSystemDB -Q "SELECT COUNT(*) AS ColumnCount FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory'" -h -1 -W > temp_columns.txt 2>&1
findstr /R "[0-9]" temp_columns.txt >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [X] 失败 - 无法读取表结构
    del temp_columns.txt >nul 2>&1
    goto :error
)
for /f %%i in (temp_columns.txt) do set COLUMN_COUNT=%%i
del temp_columns.txt >nul 2>&1

if %COLUMN_COUNT% LSS 7 (
    echo [!] 警告 - Inventory表列数不足 ^(当前: %COLUMN_COUNT%, 预期: 8^)
    echo     表可能不完整，建议重新创建
    goto :error
)
echo [✓] 成功 - Inventory表结构正确 ^(%COLUMN_COUNT% 列^)
echo.

REM 显示表信息
echo ========================================================================
echo 表信息详情：
echo ========================================================================
sqlcmd -S localhost -d RecyclingSystemDB -Q "SELECT 'Column: ' + COLUMN_NAME + ' | Type: ' + DATA_TYPE AS Info FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' ORDER BY ORDINAL_POSITION" -h -1 -W
echo.

REM 检查数据量
echo ========================================================================
echo 当前库存数据统计：
echo ========================================================================
sqlcmd -S localhost -d RecyclingSystemDB -Q "SELECT COUNT(*) AS TotalRecords FROM Inventory" -h -1 -W
echo.

echo ========================================================================
echo [✓] 验证完成 - 所有检查通过！
echo ========================================================================
echo.
echo 暂存点管理功能应该可以正常使用。
echo.
echo 如果仍然遇到网络错误，请：
echo 1. 检查浏览器控制台 ^(F12^) 查看详细错误
echo 2. 确认以回收员身份登录
echo 3. 查看 STORAGE_POINT_TROUBLESHOOTING.md 获取更多帮助
echo.
pause
exit /b 0

:error
echo.
echo ========================================================================
echo [X] 验证失败 - 发现问题
echo ========================================================================
echo.
echo 建议操作：
echo 1. 如果Inventory表不存在，运行：FixStoragePointManagement.bat
echo 2. 如果数据库不存在，请先创建RecyclingSystemDB数据库
echo 3. 如果SQL Server无法连接，请检查服务状态
echo.
echo 详细帮助文档：
echo - FIX_STORAGE_POINT_ERROR_CN.md
echo - STORAGE_POINT_TROUBLESHOOTING.md
echo.
pause
exit /b 1
