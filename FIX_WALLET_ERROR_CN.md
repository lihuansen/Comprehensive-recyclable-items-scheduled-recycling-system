# 🔧 修复"我的钱包"功能 - UserPaymentAccounts 表缺失

## 问题描述

用户登录后，进入个人中心点击"我的钱包"时，系统显示错误：

```
System.Data.SqlClient.SqlException: "对象名 'UserPaymentAccounts' 无效。"
```

## 问题原因

数据库中缺少钱包系统所需的表：
- `UserPaymentAccounts` - 用户支付账户表
- `WalletTransactions` - 钱包交易记录表
- `Users.money` - 用户余额字段（可能缺失）

## 快速修复方法

### Windows 用户

1. 打开命令提示符（CMD）或 PowerShell
2. 切换到项目的 Database 目录：
   ```batch
   cd Database
   ```
3. 先验证问题：
   ```batch
   VerifyWalletSetup.bat
   ```
4. 如果验证显示表不存在，运行修复脚本：
   ```batch
   SetupWalletTables.bat
   ```

### Linux/macOS 用户

1. 打开终端
2. 切换到项目的 Database 目录：
   ```bash
   cd Database
   ```
3. 给脚本添加执行权限（首次运行需要）：
   ```bash
   chmod +x VerifyWalletSetup.sh SetupWalletTables.sh
   ```
4. 先验证问题：
   ```bash
   ./VerifyWalletSetup.sh
   ```
5. 如果验证显示表不存在，运行修复脚本：
   ```bash
   ./SetupWalletTables.sh
   ```

## 手动修复方法

如果自动脚本无法运行，可以手动执行 SQL 脚本：

1. 打开 **SQL Server Management Studio (SSMS)**
2. 连接到您的数据库服务器
3. 选择 `RecyclingSystemDB` 数据库
4. 打开文件：`Database/CreateWalletTables.sql`
5. 点击"执行"按钮（或按 F5）

## 验证修复成功

修复完成后，重新登录系统并：

1. 进入个人中心
2. 点击"我的钱包"
3. 如果能正常显示钱包页面，说明修复成功

## 创建的数据库对象

此修复将创建以下数据库对象：

### 1. UserPaymentAccounts 表
存储用户的支付账户信息（支付宝、微信、银行卡等）

**字段：**
- AccountID - 账户ID（主键）
- UserID - 用户ID
- AccountType - 账户类型（Alipay/WeChat/BankCard）
- AccountName - 账户名称
- AccountNumber - 账户号码
- BankName - 银行名称（银行卡专用）
- IsDefault - 是否默认账户
- IsVerified - 是否已验证
- CreatedDate - 创建时间
- LastUsedDate - 最后使用时间
- Status - 状态（Active/Suspended/Deleted）

### 2. WalletTransactions 表
存储钱包的所有交易记录

**字段：**
- TransactionID - 交易ID（主键）
- UserID - 用户ID
- TransactionType - 交易类型（Recharge/Withdraw/Payment/Refund/Income）
- Amount - 交易金额
- BalanceBefore - 交易前余额
- BalanceAfter - 交易后余额
- PaymentAccountID - 支付账户ID
- RelatedOrderID - 关联订单ID
- TransactionStatus - 交易状态
- Description - 交易描述
- TransactionNo - 交易流水号
- CreatedDate - 创建时间
- CompletedDate - 完成时间
- Remarks - 备注

### 3. Users.money 字段
为 Users 表添加 money 字段（如果不存在）

- 类型：DECIMAL(18,2)
- 默认值：0.00
- 用途：存储用户的钱包余额

## 常见问题

### Q: 脚本运行时提示"找不到 sqlcmd"
**A:** 需要安装 SQL Server 命令行工具。可以：
- 安装 SQL Server Management Studio (SSMS)
- 或安装 SQL Server Command Line Utilities

### Q: 脚本运行时提示"登录失败"
**A:** 检查以下几点：
- SQL Server 服务是否已启动
- 数据库 RecyclingSystemDB 是否存在
- 当前 Windows 用户是否有数据库访问权限
- 如果使用 SQL Server 身份验证，需要修改脚本中的连接参数

### Q: 数据库名称不是 RecyclingSystemDB
**A:** 如果您的数据库名称不同，需要：
1. 编辑脚本文件中的 DATABASE 变量
2. 或直接在 SSMS 中手动执行 CreateWalletTables.sql

### Q: 表已经存在但仍然报错
**A:** 可能是表结构不完整，可以：
1. 在 SSMS 中检查表结构
2. 如有必要，删除现有表后重新创建
3. 或查看详细错误信息，可能是其他问题

## 需要帮助？

如果以上方法都无法解决问题，请：

1. 查看完整的钱包系统文档：
   - [WALLET_SYSTEM_IMPLEMENTATION.md](WALLET_SYSTEM_IMPLEMENTATION.md)
   - [WALLET_SYSTEM_QUICKSTART.md](WALLET_SYSTEM_QUICKSTART.md)
   - [WALLET_SYSTEM_ARCHITECTURE.md](WALLET_SYSTEM_ARCHITECTURE.md)

2. 检查 Web.config 中的数据库连接字符串

3. 查看 SQL Server 错误日志获取更多信息

## 相关文件

- `Database/CreateWalletTables.sql` - 创建表的 SQL 脚本
- `Database/SetupWalletTables.bat` - Windows 自动安装脚本
- `Database/SetupWalletTables.sh` - Linux/macOS 自动安装脚本
- `Database/VerifyWalletSetup.bat` - Windows 验证脚本
- `Database/VerifyWalletSetup.sh` - Linux/macOS 验证脚本
- `recycling.DAL/PaymentAccountDAL.cs` - 支付账户数据访问层
- `recycling.DAL/WalletTransactionDAL.cs` - 钱包交易数据访问层
- `recycling.BLL/PaymentAccountBLL.cs` - 支付账户业务逻辑层
- `recycling.BLL/WalletTransactionBLL.cs` - 钱包交易业务逻辑层
