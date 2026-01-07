# UserPaymentAccounts 表缺失问题修复总结

## 问题描述

在系统中的 `PaymentAccountDAL` 类的第65行存在以下错误：

```
System.Data.SqlClient.SqlException: "对象名 'UserPaymentAccounts' 无效。"
```

## 根本原因

1. **数据库表缺失**：`UserPaymentAccounts` 表和 `WalletTransactions` 表未在数据库中创建
2. **脚本不完整**：主数据库脚本 `CreateAllTables.sql` 中没有包含钱包系统相关的表
3. **数据库名称不一致**：`CreateAllTables.sql` 使用 `RecyclingDB`，但 `Web.config` 连接字符串指向 `RecyclingSystemDB`

## 解决方案

### 1. 更新 CreateAllTables.sql

修改了主数据库创建脚本，包含以下改动：

#### a. 修正数据库名称
```sql
-- 从
USE RecyclingDB;
-- 改为
USE RecyclingSystemDB;
```

#### b. 为 Users 表添加钱包余额字段
```sql
CREATE TABLE [dbo].[Users] (
    ...
    [url] NVARCHAR(50) NULL,                         -- 头像URL
    [money] DECIMAL(18,2) NULL DEFAULT 0.00          -- 钱包余额（新增）
);
```

#### c. 添加 UserPaymentAccounts 表（表20）
```sql
CREATE TABLE [dbo].[UserPaymentAccounts] (
    [AccountID] INT PRIMARY KEY IDENTITY(1,1),
    [UserID] INT NOT NULL,
    [AccountType] NVARCHAR(20) NOT NULL,
    [AccountName] NVARCHAR(100) NOT NULL,
    [AccountNumber] NVARCHAR(100) NOT NULL,
    [BankName] NVARCHAR(100) NULL,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [LastUsedDate] DATETIME2 NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Active',
    ...
);
```

#### d. 添加 WalletTransactions 表（表21）
```sql
CREATE TABLE [dbo].[WalletTransactions] (
    [TransactionID] INT PRIMARY KEY IDENTITY(1,1),
    [UserID] INT NOT NULL,
    [TransactionType] NVARCHAR(20) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [BalanceBefore] DECIMAL(18,2) NOT NULL,
    [BalanceAfter] DECIMAL(18,2) NOT NULL,
    [PaymentAccountID] INT NULL,
    [RelatedOrderID] INT NULL,
    [TransactionStatus] NVARCHAR(20) NOT NULL DEFAULT 'Completed',
    [Description] NVARCHAR(500) NULL,
    [TransactionNo] NVARCHAR(50) NOT NULL UNIQUE,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CompletedDate] DATETIME2 NULL,
    [Remarks] NVARCHAR(500) NULL,
    ...
);
```

### 2. 创建迁移脚本 AddWalletTablesToExistingDatabase.sql

为现有数据库创建了单独的迁移脚本，用于：
- 智能检测并添加缺失的表
- 为 Users 表添加 money 列（如不存在）
- 初始化现有用户的钱包余额为 0.00
- 提供友好的进度提示

### 3. 更新文档 Database/README.md

添加了详细的快速修复指南，包括：
- SQL脚本直接修复方法（最简单）
- 批处理脚本方法
- 问题原因说明
- 会创建的表列表
- 更新了完整表格，显示21个表（包含钱包系统表）

## 使用方法

### 方法一：新建数据库（推荐）

如果您是首次设置数据库，直接运行更新后的 `CreateAllTables.sql`：

```sql
-- 在 SQL Server Management Studio (SSMS) 中
-- 1. 打开 CreateAllTables.sql
-- 2. 执行脚本（F5）
-- 脚本会自动创建 RecyclingSystemDB 数据库和所有21个表
```

### 方法二：现有数据库迁移（推荐）

如果数据库已存在但缺少钱包表，运行迁移脚本：

```sql
-- 在 SQL Server Management Studio (SSMS) 中
-- 1. 连接到 RecyclingSystemDB 数据库
-- 2. 打开 AddWalletTablesToExistingDatabase.sql
-- 3. 执行脚本（F5）
```

### 方法三：使用批处理脚本

Windows 用户：
```batch
cd Database
SetupWalletTables.bat
```

Linux/macOS 用户：
```bash
cd Database
chmod +x SetupWalletTables.sh
./SetupWalletTables.sh
```

## 验证修复

修复完成后，可以通过以下方式验证：

### 1. 检查表是否存在

```sql
USE RecyclingSystemDB;

-- 检查 UserPaymentAccounts 表
SELECT * FROM sys.tables WHERE name = 'UserPaymentAccounts';

-- 检查 WalletTransactions 表
SELECT * FROM sys.tables WHERE name = 'WalletTransactions';

-- 检查 Users 表的 money 列
SELECT * FROM sys.columns 
WHERE object_id = OBJECT_ID('Users') AND name = 'money';
```

### 2. 测试应用程序

1. 启动应用程序
2. 登录用户端
3. 访问"我的钱包"功能
4. 确认不再出现 "对象名 'UserPaymentAccounts' 无效" 错误

## 技术细节

### 表结构

#### UserPaymentAccounts（用户支付账户表）
- 存储用户的支付账户信息（支付宝、微信、银行卡）
- 支持设置默认账户
- 支持账户验证状态
- 软删除机制（Status字段）

#### WalletTransactions（钱包交易记录表）
- 记录所有钱包交易（充值、提现、支付、退款、收入）
- 保存交易前后余额，确保数据可追溯
- 唯一交易流水号
- 支持多种交易状态

#### Users.money（用户钱包余额）
- DECIMAL(18,2) 类型，精确到分
- 默认值为 0.00
- 支持NULL值以兼容旧数据

### 外键关系

```
Users (UserID)
  ├─→ UserPaymentAccounts (UserID)
  └─→ WalletTransactions (UserID)
  
UserPaymentAccounts (AccountID)
  └─→ WalletTransactions (PaymentAccountID)
```

### 索引优化

- UserPaymentAccounts: 在 UserID, AccountType, IsDefault, Status 上创建索引
- WalletTransactions: 在 UserID, TransactionType, TransactionStatus, CreatedDate, TransactionNo 上创建索引

## 相关文件

### 修改的文件
- `Database/CreateAllTables.sql` - 添加钱包系统表，修正数据库名称
- `Database/README.md` - 更新文档，添加修复指南

### 新增的文件
- `Database/AddWalletTablesToExistingDatabase.sql` - 现有数据库迁移脚本

### 相关实体类
- `recycling.Model.UserPaymentAccount` - 用户支付账户实体类
- `recycling.DAL.PaymentAccountDAL` - 数据访问层
- `recycling.BLL.PaymentAccountBLL` - 业务逻辑层

## 注意事项

1. **数据库名称**：确保使用 `RecyclingSystemDB` 而非 `RecyclingDB`
2. **备份数据**：在执行任何数据库脚本前，建议备份现有数据
3. **权限要求**：执行脚本需要数据库创建表的权限
4. **兼容性**：脚本使用 SQL Server T-SQL 语法，需要 SQL Server 数据库

## 总结

此修复通过以下三个关键更改彻底解决了 UserPaymentAccounts 表缺失问题：

1. ✅ 更新主数据库脚本，包含所有21个表（包括钱包系统表）
2. ✅ 创建专用迁移脚本，方便现有数据库升级
3. ✅ 完善文档，提供多种修复方法

用户现在可以正常使用"我的钱包"功能，无需再担心表缺失错误。
