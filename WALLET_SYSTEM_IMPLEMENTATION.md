# 钱包系统实现文档
# Wallet System Implementation Documentation

## 概述 (Overview)

本文档描述了全品类可回收物预约回收系统中钱包功能的完整实现。该实现解决了原有单一 `money` 字段无法支持多种支付方式（支付宝、微信、银行卡）的问题，提供了一个接近现实应用场景的钱包管理系统。

This document describes the complete implementation of the wallet functionality in the Comprehensive Recyclable Items Scheduled Recycling System. This implementation addresses the limitation of the original single `money` field, which could not support multiple payment methods (Alipay, WeChat, Bank Cards), and provides a wallet management system close to real-world application scenarios.

## 系统架构 (System Architecture)

### 数据库层 (Database Layer)

#### 1. UserPaymentAccounts 表（用户支付账户表）

存储用户绑定的支付账户信息。

**字段说明：**
- `AccountID`: 账户ID（主键，自增）
- `UserID`: 用户ID（外键，关联 Users 表）
- `AccountType`: 账户类型（Alipay/支付宝、WeChat/微信、BankCard/银行卡）
- `AccountName`: 账户名称/持卡人姓名
- `AccountNumber`: 账户号/卡号（建议加密存储）
- `BankName`: 银行名称（仅银行卡需要）
- `IsDefault`: 是否默认账户
- `IsVerified`: 是否已验证
- `CreatedDate`: 创建时间
- `LastUsedDate`: 最后使用时间
- `Status`: 状态（Active/激活、Suspended/暂停、Deleted/已删除）

**索引：**
- `IX_UserPaymentAccounts_UserID`: 用户ID索引
- `IX_UserPaymentAccounts_AccountType`: 账户类型索引
- `IX_UserPaymentAccounts_IsDefault`: 默认账户索引
- `IX_UserPaymentAccounts_Status`: 状态索引

#### 2. WalletTransactions 表（钱包交易记录表）

存储所有钱包相关的交易记录。

**字段说明：**
- `TransactionID`: 交易ID（主键，自增）
- `UserID`: 用户ID（外键，关联 Users 表）
- `TransactionType`: 交易类型（Recharge/充值、Withdraw/提现、Payment/支付、Refund/退款、Income/收入）
- `Amount`: 交易金额
- `BalanceBefore`: 交易前余额
- `BalanceAfter`: 交易后余额
- `PaymentAccountID`: 支付账户ID（外键，充值/提现时使用）
- `RelatedOrderID`: 关联订单ID（支付/退款时使用）
- `TransactionStatus`: 交易状态（Pending/待处理、Processing/处理中、Completed/已完成、Failed/失败、Cancelled/已取消）
- `Description`: 交易描述
- `TransactionNo`: 交易流水号（唯一）
- `CreatedDate`: 创建时间
- `CompletedDate`: 完成时间
- `Remarks`: 备注

**索引：**
- `IX_WalletTransactions_UserID`: 用户ID索引
- `IX_WalletTransactions_TransactionType`: 交易类型索引
- `IX_WalletTransactions_TransactionStatus`: 交易状态索引
- `IX_WalletTransactions_CreatedDate`: 创建时间索引（降序）
- `IX_WalletTransactions_TransactionNo`: 交易流水号唯一索引

#### 3. Users 表更新

确保 Users 表包含 `money` 字段（DECIMAL(18,2)），用于存储用户当前余额。

### 模型层 (Model Layer)

#### 1. UserPaymentAccount.cs
用户支付账户实体类，包含：
- 基本属性映射数据库字段
- `GetAccountTypeDisplayName()`: 获取账户类型的中文显示名称
- `GetMaskedAccountNumber()`: 获取脱敏后的账户号码（前4位+星号+后4位）

#### 2. WalletTransaction.cs
钱包交易记录实体类，包含：
- 基本属性映射数据库字段
- `GetTransactionTypeDisplayName()`: 获取交易类型的中文显示名称
- `GetTransactionStatusDisplayName()`: 获取交易状态的中文显示名称
- `IsIncome()`: 判断是否为收入类交易
- `IsExpense()`: 判断是否为支出类交易

#### 3. WalletViewModel.cs
钱包视图模型，用于钱包页面的数据展示，包含：
- `User`: 用户信息
- `PaymentAccounts`: 用户的支付账户列表
- `RecentTransactions`: 最近的交易记录列表
- `CurrentBalance`: 当前余额
- `TotalIncome`: 累计收入
- `TotalExpense`: 累计支出
- `MonthlyTransactionCount`: 本月交易次数
- `HasDefaultPaymentAccount`: 是否有默认支付账户

#### 4. AddPaymentAccountViewModel.cs
添加支付账户的视图模型，包含表单验证。

#### 5. RechargeViewModel.cs
充值视图模型，包含表单验证。

#### 6. WithdrawViewModel.cs
提现视图模型，包含表单验证。

### 数据访问层 (DAL - Data Access Layer)

#### 1. PaymentAccountDAL.cs
支付账户数据访问类，提供：
- `AddPaymentAccount()`: 添加支付账户
- `GetPaymentAccountsByUserId()`: 根据用户ID获取支付账户列表
- `GetPaymentAccountById()`: 根据账户ID获取支付账户
- `UpdatePaymentAccount()`: 更新支付账户
- `DeletePaymentAccount()`: 删除支付账户（软删除）
- `SetDefaultAccount()`: 设置默认支付账户
- `UpdateLastUsedDate()`: 更新账户最后使用时间

#### 2. WalletTransactionDAL.cs
钱包交易数据访问类，提供：
- `AddTransaction()`: 添加交易记录
- `GetTransactionsByUserId()`: 根据用户ID获取交易记录列表（分页）
- `GetTransactionById()`: 根据交易ID获取交易记录
- `GetTransactionByNo()`: 根据交易流水号获取交易记录
- `UpdateTransactionStatus()`: 更新交易状态
- `GetUserTransactionStatistics()`: 获取用户交易统计信息
- `GetTransactionCountByUserId()`: 获取用户交易记录总数
- `GenerateTransactionNo()`: 生成唯一的交易流水号

### 业务逻辑层 (BLL - Business Logic Layer)

#### 1. PaymentAccountBLL.cs
支付账户业务逻辑类，提供：
- `AddPaymentAccount()`: 添加支付账户（包含业务验证）
- `GetPaymentAccountsByUserId()`: 获取用户的支付账户列表
- `GetPaymentAccountById()`: 获取支付账户详情
- `DeletePaymentAccount()`: 删除支付账户（验证权限）
- `SetDefaultAccount()`: 设置默认支付账户（验证权限）
- `VerifyAccount()`: 验证支付账户

#### 2. WalletTransactionBLL.cs
钱包交易业务逻辑类，提供：
- `Recharge()`: 充值功能（验证金额、账户，更新余额，记录交易）
- `Withdraw()`: 提现功能（验证金额、账户、余额，更新余额，记录交易）
- `GetTransactionsByUserId()`: 获取用户交易记录
- `GetTransactionById()`: 获取交易详情
- `GetUserTransactionStatistics()`: 获取用户交易统计
- `GetWalletViewModel()`: 获取钱包视图模型（整合所有数据）

### 控制器层 (Controller Layer)

#### HomeController.cs
更新了 `MyWallet` 方法：
- 检查用户登录状态
- 调用 `WalletTransactionBLL.GetWalletViewModel()` 获取完整的钱包数据
- 返回视图模型到视图

### 视图层 (View Layer)

#### MyWallet.cshtml
更新了钱包页面：
- 显示实时余额（从 WalletViewModel 获取）
- 显示资金概览统计（累计收入、累计支出、本月交易）
- **新增：支付账户管理区**
  - 显示已绑定的支付账户列表
  - 显示账户类型、账户名称、脱敏账户号
  - 标识默认账户和已验证账户
  - 支持删除账户操作
  - 支持添加新账户
- 保留原有钱包功能区（充值、提现、转账等）
- **更新：交易记录区**
  - 显示最近的交易记录
  - 显示交易类型、金额、时间、流水号
  - 区分收入和支出（不同颜色）
  - 显示交易后余额
  - 提供查看完整交易记录的链接
- 保留安全提示区

## 数据库部署 (Database Deployment)

### 步骤 1: 执行建表脚本

在 SQL Server Management Studio (SSMS) 中执行以下脚本：

```
Database/CreateWalletTables.sql
```

此脚本将：
1. 创建 `UserPaymentAccounts` 表
2. 创建 `WalletTransactions` 表
3. 确保 `Users` 表包含 `money` 字段（如果不存在则添加）
4. 更新现有用户的 `money` 字段默认值为 0.00

### 步骤 2: 验证表创建

执行以下查询验证表是否创建成功：

```sql
-- 检查 UserPaymentAccounts 表
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserPaymentAccounts';

-- 检查 WalletTransactions 表
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WalletTransactions';

-- 检查 Users 表的 money 字段
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'money';
```

## 功能特点 (Features)

### 1. 多支付方式支持
- 支持支付宝（Alipay）
- 支持微信支付（WeChat）
- 支持银行卡（BankCard）
- 每个用户可以绑定多个支付账户
- 可设置默认支付账户

### 2. 完整的交易记录
- 记录所有充值、提现、支付、退款、收入交易
- 记录交易前后余额变化
- 生成唯一的交易流水号
- 支持交易状态跟踪

### 3. 数据统计
- 累计收入统计
- 累计支出统计
- 本月交易次数统计

### 4. 安全性
- 账户号码脱敏显示
- 软删除支付账户（保留历史记录）
- 交易状态管理
- 余额验证（提现时）

### 5. 用户体验
- 清晰的视觉设计
- 支付账户管理界面
- 交易记录展示
- 实时余额更新

## 扩展功能建议 (Future Enhancements)

以下功能已在代码中预留接口，但需要进一步实现：

### 1. 充值和提现功能
- 集成第三方支付网关（支付宝、微信支付）
- 实现银行卡充值和提现
- 实现异步支付状态更新
- 添加支付密码验证

### 2. 账户管理功能
- 实现添加支付账户的完整流程
- 实现账户验证机制（银行卡四要素验证）
- 实现账户编辑功能
- 实现账户删除确认流程

### 3. 交易管理功能
- 实现交易详情页面
- 实现交易记录分页浏览
- 实现交易记录筛选（按类型、时间）
- 实现交易记录导出功能

### 4. 转账功能
- 实现用户间转账
- 实现转账手续费计算
- 实现转账限额管理

### 5. 安全增强
- 实现支付密码设置和验证
- 实现账户号码加密存储
- 实现交易通知（短信、邮件）
- 实现异常交易检测和预警

### 6. 数据报表
- 实现收支趋势图表
- 实现交易类型分析
- 实现月度对账单生成

## 测试建议 (Testing Recommendations)

### 单元测试
- 测试 DAL 层的数据库操作
- 测试 BLL 层的业务逻辑验证
- 测试金额计算的精度

### 集成测试
- 测试完整的充值流程
- 测试完整的提现流程
- 测试账户管理流程
- 测试交易记录查询

### 性能测试
- 测试大量交易记录的查询性能
- 测试并发充值/提现的性能
- 测试数据库索引效果

## 注意事项 (Important Notes)

1. **安全性**：
   - 账户号码应该加密存储（当前实现未加密）
   - 建议实施支付密码验证机制
   - 建议添加交易限额和频率限制

2. **事务处理**：
   - 充值和提现操作应该在数据库事务中执行
   - 确保余额更新和交易记录创建的原子性

3. **第三方集成**：
   - 当前实现未集成真实的支付网关
   - 需要根据实际需求集成支付宝、微信支付等API

4. **合规性**：
   - 如果涉及真实资金交易，需要获得相关金融牌照
   - 需要遵守反洗钱等相关法律法规

5. **数据备份**：
   - 交易记录是重要的财务数据，需要定期备份
   - 建议实施数据审计日志

## 技术栈 (Technology Stack)

- **后端框架**: ASP.NET MVC 4.8
- **数据库**: SQL Server
- **ORM**: ADO.NET (SqlHelper)
- **前端**: HTML, CSS, JavaScript, Bootstrap, Font Awesome

## 总结 (Summary)

本钱包系统实现提供了一个完整的、接近现实应用场景的钱包管理解决方案。它解决了原有单一 `money` 字段的限制，支持多种支付方式，提供完整的交易记录和统计功能。系统采用分层架构设计，易于维护和扩展。

在实际部署时，建议根据具体业务需求进一步完善安全机制、集成第三方支付网关，并实施充分的测试。
