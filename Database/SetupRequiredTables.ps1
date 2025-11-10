# 数据库表设置脚本 (PowerShell版本)
# Database Tables Setup Script (PowerShell Version)

param(
    [string]$Server = "localhost",
    [string]$Database = "RecyclingDB"
)

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "数据库表设置脚本" -ForegroundColor Cyan
Write-Host "Database Tables Setup Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "正在连接到服务器: $Server" -ForegroundColor Yellow
Write-Host "数据库: $Database" -ForegroundColor Yellow
Write-Host ""

# 测试 sqlcmd 是否可用
try {
    $null = Get-Command sqlcmd -ErrorAction Stop
} catch {
    Write-Host "错误: 未找到 sqlcmd 命令！" -ForegroundColor Red
    Write-Host "请确保已安装 SQL Server 命令行工具。" -ForegroundColor Red
    Write-Host "下载地址: https://docs.microsoft.com/sql/tools/sqlcmd-utility" -ForegroundColor Red
    exit 1
}

# 创建用户反馈表
Write-Host "[1/2] 创建用户反馈表 (UserFeedback)..." -ForegroundColor Yellow
$result = & sqlcmd -S $Server -d $Database -E -i "CreateUserFeedbackTable.sql" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 用户反馈表创建失败！" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Write-Host "请检查数据库连接和权限。" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 用户反馈表创建成功" -ForegroundColor Green
Write-Host ""

# 创建管理员联系表
Write-Host "[2/2] 创建管理员联系表 (AdminContactConversations, AdminContactMessages)..." -ForegroundColor Yellow
$result = & sqlcmd -S $Server -d $Database -E -i "CreateAdminContactMessagesTable.sql" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 管理员联系表创建失败！" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Write-Host "请检查数据库连接和权限。" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 管理员联系表创建成功" -ForegroundColor Green
Write-Host ""

# 验证表是否创建成功
Write-Host "正在验证数据库表..." -ForegroundColor Yellow
$verifyQuery = @"
SELECT 
    TABLE_NAME,
    'Created' AS Status
FROM 
    INFORMATION_SCHEMA.TABLES 
WHERE 
    TABLE_TYPE = 'BASE TABLE' 
    AND TABLE_NAME IN ('UserFeedback', 'AdminContactConversations', 'AdminContactMessages')
ORDER BY 
    TABLE_NAME;
"@

$verifyResult = & sqlcmd -S $Server -d $Database -E -Q $verifyQuery -h -1 2>&1
Write-Host $verifyResult
Write-Host ""

Write-Host "====================================" -ForegroundColor Green
Write-Host "所有必需的数据库表已成功创建！" -ForegroundColor Green
Write-Host "All required database tables have been created successfully!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "您现在可以使用以下功能：" -ForegroundColor Cyan
Write-Host "1. 问题反馈功能" -ForegroundColor White
Write-Host "2. 联系管理员功能" -ForegroundColor White
Write-Host ""
