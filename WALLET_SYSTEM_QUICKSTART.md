# 钱包系统快速开始指南
# Wallet System Quick Start Guide

## 数据库设置 (Database Setup)

### 步骤 1: 执行数据库脚本

1. 打开 SQL Server Management Studio (SSMS)
2. 连接到您的数据库服务器
3. 打开文件：`Database/CreateWalletTables.sql`
4. 执行脚本（按 F5 或点击"执行"按钮）

脚本将创建以下对象：
- `UserPaymentAccounts` 表 - 用户支付账户表
- `WalletTransactions` 表 - 钱包交易记录表
- `Users.money` 字段 - 如果不存在则添加

### 步骤 2: 验证安装

执行以下 SQL 查询验证表是否正确创建：

```sql
USE RecyclingDB;
GO

-- 检查表是否存在
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('UserPaymentAccounts', 'WalletTransactions');

-- 检查 Users 表的 money 字段
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'money';
```

预期结果：应该看到两个表名和 money 字段信息。

## 项目编译 (Project Compilation)

### 在 Visual Studio 中编译

1. 打开 Visual Studio 2019 或更高版本
2. 打开解决方案文件：`全品类可回收物预约回收系统（解决方案）.sln`
3. 右键点击解决方案 -> "重新生成解决方案"
4. 确保所有项目成功编译，无错误

### 检查新增文件

确认以下文件已添加到项目中：

**Model 项目 (recycling.Model):**
- `UserPaymentAccount.cs`
- `WalletTransaction.cs`
- `WalletViewModel.cs`

**DAL 项目 (recycling.DAL):**
- `PaymentAccountDAL.cs`
- `WalletTransactionDAL.cs`

**BLL 项目 (recycling.BLL):**
- `PaymentAccountBLL.cs`
- `WalletTransactionBLL.cs`

## 测试钱包功能 (Testing Wallet Features)

### 1. 启动应用程序

1. 在 Visual Studio 中按 F5 启动应用程序
2. 应用程序将在浏览器中打开

### 2. 登录系统

1. 使用现有用户账号登录，或注册新账号
2. 登录成功后，进入个人中心

### 3. 访问我的钱包

1. 在个人中心页面，点击"我的钱包"链接
2. 或直接访问：`http://localhost:xxxx/Home/MyWallet`

### 4. 查看钱包页面

在钱包页面，您应该能看到：

✓ **钱包头部**
- 当前余额显示
- 资金安全提示
- 快捷操作按钮（充值、提现、转账）

✓ **资金概览**
- 累计收入（实时统计）
- 累计支出（实时统计）
- 本月交易次数

✓ **支付账户管理**（新功能）
- 已绑定的支付账户列表
- 账户类型、名称、脱敏账户号
- 默认账户标识
- 添加新账户按钮

✓ **钱包功能区**
- 余额充值
- 余额提现
- 转账支付
- 账单明细
- 银行卡管理
- 支付密码

✓ **交易记录**（新功能）
- 最近的交易记录列表
- 交易类型、金额、时间
- 收入/支出区分
- 交易流水号

✓ **安全提示**
- 安全使用建议

## 测试数据示例 (Test Data Examples)

### 添加测试支付账户

可以通过 SQL 手动添加测试数据：

```sql
USE RecyclingDB;
GO

-- 假设用户ID为1，添加一个支付宝账户
INSERT INTO UserPaymentAccounts 
(UserID, AccountType, AccountName, AccountNumber, BankName, IsDefault, IsVerified, CreatedDate, Status)
VALUES 
(1, 'Alipay', '张三', '13800138000', NULL, 1, 1, GETDATE(), 'Active');

-- 添加一个微信账户
INSERT INTO UserPaymentAccounts 
(UserID, AccountType, AccountName, AccountNumber, BankName, IsDefault, IsVerified, CreatedDate, Status)
VALUES 
(1, 'WeChat', '张三', 'wxid_abc123456', NULL, 0, 1, GETDATE(), 'Active');

-- 添加一个银行卡账户
INSERT INTO UserPaymentAccounts 
(UserID, AccountType, AccountName, AccountNumber, BankName, IsDefault, IsVerified, CreatedDate, Status)
VALUES 
(1, 'BankCard', '张三', '6222021234567890123', '中国工商银行', 0, 1, GETDATE(), 'Active');
```

### 添加测试交易记录

```sql
USE RecyclingDB;
GO

-- 假设用户ID为1，添加一条充值记录
DECLARE @CurrentBalance DECIMAL(18,2);
SELECT @CurrentBalance = ISNULL(money, 0) FROM Users WHERE UserID = 1;

INSERT INTO WalletTransactions 
(UserID, TransactionType, Amount, BalanceBefore, BalanceAfter, PaymentAccountID, 
 TransactionStatus, Description, TransactionNo, CreatedDate, CompletedDate)
VALUES 
(1, 'Recharge', 100.00, @CurrentBalance, @CurrentBalance + 100.00, 1, 
 'Completed', '钱包充值', 'TXN' + CONVERT(VARCHAR(20), GETDATE(), 112) + '001', GETDATE(), GETDATE());

-- 更新用户余额
UPDATE Users SET money = @CurrentBalance + 100.00 WHERE UserID = 1;

-- 添加一条支出记录
SET @CurrentBalance = @CurrentBalance + 100.00;
INSERT INTO WalletTransactions 
(UserID, TransactionType, Amount, BalanceBefore, BalanceAfter, 
 TransactionStatus, Description, TransactionNo, CreatedDate, CompletedDate)
VALUES 
(1, 'Payment', 20.00, @CurrentBalance, @CurrentBalance - 20.00, 
 'Completed', '支付订单费用', 'TXN' + CONVERT(VARCHAR(20), GETDATE(), 112) + '002', GETDATE(), GETDATE());

-- 更新用户余额
UPDATE Users SET money = @CurrentBalance - 20.00 WHERE UserID = 1;
```

## 功能验证清单 (Verification Checklist)

### 页面显示
- [ ] 钱包余额正确显示
- [ ] 累计收入/支出正确计算
- [ ] 本月交易次数正确统计
- [ ] 支付账户列表正确显示
- [ ] 账户号正确脱敏（例如：1380****8000）
- [ ] 默认账户有"默认"标识
- [ ] 已验证账户有验证图标
- [ ] 交易记录正确显示
- [ ] 收入显示绿色加号，支出显示红色减号
- [ ] 交易后余额正确显示

### 数据准确性
- [ ] 余额与数据库中的 Users.money 一致
- [ ] 统计数据与交易记录匹配
- [ ] 交易记录按时间倒序排列

### 用户体验
- [ ] 页面加载速度合理
- [ ] 样式美观，与系统其他页面一致
- [ ] 按钮和链接清晰可见
- [ ] 提示信息清晰明了

## 常见问题 (Troubleshooting)

### 问题 1: 编译错误 - 找不到类型

**症状:** 编译时提示找不到 `UserPaymentAccount` 或 `WalletTransaction` 等类型。

**解决方案:**
1. 确认新文件已添加到项目中（在 Solution Explorer 中可见）
2. 检查 .csproj 文件是否包含新文件的编译条目
3. 清理解决方案（生成 -> 清理解决方案）
4. 重新生成解决方案（生成 -> 重新生成解决方案）

### 问题 2: 数据库错误 - 表不存在

**症状:** 运行时提示 "Invalid object name 'UserPaymentAccounts'" 等错误。

**解决方案:**
1. 确认已执行 `Database/CreateWalletTables.sql` 脚本
2. 检查数据库连接字符串是否正确
3. 确认连接到正确的数据库（RecyclingDB）
4. 使用 SSMS 手动验证表是否存在

### 问题 3: 钱包页面显示空白或错误

**症状:** 访问 /Home/MyWallet 页面时显示错误或空白。

**解决方案:**
1. 检查是否已登录（必须先登录才能访问钱包页面）
2. 查看应用程序日志或错误详情
3. 确认数据库连接正常
4. 检查用户ID是否存在于 Users 表中

### 问题 4: 统计数据显示为 0

**症状:** 累计收入、累计支出、本月交易都显示为 0。

**这是正常现象：** 如果用户还没有任何交易记录，统计数据会显示为 0。

**解决方案:** 使用上面提供的 SQL 脚本添加测试交易记录。

## 下一步开发 (Next Steps)

当前实现提供了完整的数据结构和基础功能，但以下功能仍需进一步开发：

1. **添加支付账户功能**
   - 创建添加支付账户的表单页面
   - 实现表单提交和验证
   - 添加账户验证机制

2. **充值和提现功能**
   - 集成第三方支付网关
   - 实现支付状态回调处理
   - 添加支付密码验证

3. **交易记录分页**
   - 实现交易记录的完整列表页面
   - 添加分页功能
   - 添加筛选和搜索功能

4. **账户编辑和删除**
   - 实现编辑支付账户功能
   - 添加删除确认对话框
   - 处理账户删除的级联操作

5. **安全增强**
   - 实现账户号码加密存储
   - 添加支付密码设置功能
   - 实现交易短信通知

## 技术支持 (Technical Support)

如有问题，请参考：
- 完整实现文档：`WALLET_SYSTEM_IMPLEMENTATION.md`
- 数据库建表脚本：`Database/CreateWalletTables.sql`
- 源代码注释：所有新增类都包含详细的中文注释

## 版本信息 (Version Info)

- 钱包系统版本: 1.0
- 创建日期: 2026-01-07
- 适用于: 全品类可回收物预约回收系统
