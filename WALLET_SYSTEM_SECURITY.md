# 钱包系统安全说明
# Wallet System Security Notes

## ⚠️ 重要安全提示 (Important Security Notes)

### 当前实现状态 (Current Implementation Status)

本钱包系统的当前实现提供了**完整的数据结构和基础框架**，但以下安全功能需要在生产环境部署前实施：

This wallet system implementation provides a **complete data structure and basic framework**, but the following security features must be implemented before production deployment:

---

## 1. 账户号码加密 (Account Number Encryption)

### 当前状态 (Current Status)
❌ 账户号码以**明文**存储在 `UserPaymentAccounts.AccountNumber` 字段中

❌ Account numbers are stored in **plain text** in the `UserPaymentAccounts.AccountNumber` field

### 生产要求 (Production Requirements)
✅ **必须实施加密存储**

建议实施方案：
```csharp
// 添加加密帮助类
public class CryptoHelper
{
    private static readonly string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
    
    // 使用 AES 加密
    public static string Encrypt(string plainText)
    {
        // AES encryption implementation
    }
    
    // 使用 AES 解密
    public static string Decrypt(string cipherText)
    {
        // AES decryption implementation
    }
}

// 在 PaymentAccountDAL 中使用
public int AddPaymentAccount(UserPaymentAccount account)
{
    // 加密账户号码
    string encryptedNumber = CryptoHelper.Encrypt(account.AccountNumber);
    
    // 存储加密后的号码
    // ...
}
```

**代码位置：**
- `recycling.BLL/PaymentAccountBLL.cs` 第 54 行有相关注释
- `recycling.DAL/PaymentAccountDAL.cs` 需要添加加密/解密逻辑

---

## 2. 交易状态管理 (Transaction Status Management)

### 当前状态 (Current Status)
⚠️ 充值和提现交易立即标记为 `Completed`（已完成）

⚠️ Recharge and withdraw transactions are immediately marked as `Completed`

### 生产要求 (Production Requirements)
✅ **必须实施异步支付流程**

建议实施方案：
```csharp
// 充值流程应该是：
// 1. 创建交易记录，状态为 Pending
var transaction = new WalletTransaction
{
    TransactionStatus = "Pending",  // 而不是 "Completed"
    // ...
};

// 2. 调用第三方支付接口
var paymentResult = AlipaySDK.CreateOrder(transaction);

// 3. 等待支付回调
// 在支付回调处理中：
public void PaymentCallback(string transactionNo, string status)
{
    if (status == "SUCCESS")
    {
        // 更新交易状态为 Completed
        _walletTransactionDAL.UpdateTransactionStatus(transactionId, "Completed", DateTime.Now);
        
        // 更新用户余额
        _userDAL.UpdateUserBalance(userId, newBalance);
    }
    else
    {
        // 更新交易状态为 Failed
        _walletTransactionDAL.UpdateTransactionStatus(transactionId, "Failed", DateTime.Now);
    }
}
```

**代码位置：**
- `recycling.BLL/WalletTransactionBLL.cs` 第 56 行和第 120 行有相关注释

---

## 3. 支付密码 (Payment Password)

### 当前状态 (Current Status)
❌ 没有支付密码验证机制

❌ No payment password verification mechanism

### 生产要求 (Production Requirements)
✅ **必须添加支付密码功能**

建议实施方案：
```sql
-- 在 Users 表中添加支付密码字段
ALTER TABLE Users ADD PaymentPasswordHash NVARCHAR(255) NULL;
ALTER TABLE Users ADD PaymentPasswordSalt NVARCHAR(255) NULL;

-- 在 UserPaymentAccounts 中记录密码错误次数
ALTER TABLE UserPaymentAccounts ADD PasswordFailCount INT DEFAULT 0;
ALTER TABLE UserPaymentAccounts ADD LockedUntil DATETIME2 NULL;
```

```csharp
// 在充值/提现前验证支付密码
public OperationResult Withdraw(WithdrawViewModel model, int userId, string paymentPassword)
{
    // 验证支付密码
    if (!_userBLL.VerifyPaymentPassword(userId, paymentPassword))
    {
        return new OperationResult { Success = false, Message = "支付密码错误" };
    }
    
    // 继续提现流程
    // ...
}
```

---

## 4. 交易限额 (Transaction Limits)

### 当前状态 (Current Status)
⚠️ 没有交易限额限制

⚠️ No transaction limit restrictions

### 生产要求 (Production Requirements)
✅ **建议添加交易限额**

建议实施方案：
```csharp
public class TransactionLimitConfig
{
    // 单笔充值限额
    public decimal MaxRechargeAmount { get; set; } = 50000;
    public decimal MinRechargeAmount { get; set; } = 0.01;
    
    // 单笔提现限额
    public decimal MaxWithdrawAmount { get; set; } = 50000;
    public decimal MinWithdrawAmount { get; set; } = 0.01;
    
    // 每日累计限额
    public decimal DailyRechargeLimit { get; set; } = 100000;
    public decimal DailyWithdrawLimit { get; set; } = 100000;
}

// 在充值/提现前检查限额
public OperationResult Recharge(RechargeViewModel model, int userId)
{
    // 检查单笔限额
    if (model.Amount > _config.MaxRechargeAmount || model.Amount < _config.MinRechargeAmount)
    {
        return new OperationResult { Success = false, Message = "充值金额超出限制" };
    }
    
    // 检查每日累计限额
    var todayRechargeTotal = GetTodayRechargeTotal(userId);
    if (todayRechargeTotal + model.Amount > _config.DailyRechargeLimit)
    {
        return new OperationResult { Success = false, Message = "超出每日充值限额" };
    }
    
    // 继续充值流程
    // ...
}
```

---

## 5. 交易通知 (Transaction Notifications)

### 当前状态 (Current Status)
❌ 没有交易通知功能

❌ No transaction notification functionality

### 生产要求 (Production Requirements)
✅ **建议添加交易通知**

建议实施方案：
```csharp
public interface INotificationService
{
    void SendSMS(string phoneNumber, string message);
    void SendEmail(string email, string subject, string body);
}

// 在交易完成后发送通知
private void SendTransactionNotification(WalletTransaction transaction, Users user)
{
    string message = $"您的钱包{transaction.GetTransactionTypeDisplayName()}已完成，金额：¥{transaction.Amount}，余额：¥{transaction.BalanceAfter}。【回收系统】";
    
    _notificationService.SendSMS(user.PhoneNumber, message);
}
```

---

## 6. 数据库事务 (Database Transactions)

### 当前状态 (Current Status)
⚠️ 充值和提现操作未在数据库事务中执行

⚠️ Recharge and withdraw operations are not executed within database transactions

### 生产要求 (Production Requirements)
✅ **必须使用数据库事务**

建议实施方案：
```csharp
public OperationResult Recharge(RechargeViewModel model, int userId)
{
    using (var connection = new SqlConnection(SqlHelper.ConnectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. 创建交易记录
                var transactionId = _transactionDAL.AddTransaction(walletTransaction, transaction);
                
                // 2. 更新用户余额
                _userDAL.UpdateUserBalance(userId, newBalance, transaction);
                
                // 3. 更新支付账户
                _accountDAL.UpdateLastUsedDate(accountId, transaction);
                
                // 提交事务
                transaction.Commit();
                
                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                // 回滚事务
                transaction.Rollback();
                return new OperationResult { Success = false, Message = ex.Message };
            }
        }
    }
}
```

---

## 7. 反洗钱合规 (AML Compliance)

### 当前状态 (Current Status)
❌ 没有反洗钱检测机制

❌ No anti-money laundering detection mechanism

### 生产要求 (Production Requirements)
✅ **根据当地法律要求实施反洗钱措施**

可能需要的功能：
1. **交易监控**：检测异常交易模式
2. **身份验证**：实名认证、身份证验证
3. **交易记录保存**：保存完整的交易记录至少5年
4. **大额交易报告**：自动报告超过阈值的交易
5. **可疑交易报告**：报告可疑的交易活动

---

## 8. 日志和审计 (Logging and Auditing)

### 当前状态 (Current Status)
⚠️ 没有专门的交易审计日志

⚠️ No dedicated transaction audit logs

### 生产要求 (Production Requirements)
✅ **建议添加审计日志**

建议实施方案：
```sql
CREATE TABLE WalletAuditLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,  -- AddAccount, DeleteAccount, Recharge, Withdraw
    EntityType NVARCHAR(50) NOT NULL,
    EntityID INT NOT NULL,
    BeforeValue NVARCHAR(MAX) NULL,
    AfterValue NVARCHAR(MAX) NULL,
    IPAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

---

## 9. 第三方支付集成 (Third-party Payment Integration)

### 当前状态 (Current Status)
❌ 没有集成真实的支付网关

❌ No real payment gateway integration

### 生产要求 (Production Requirements)
✅ **必须集成第三方支付服务**

需要集成的服务：
1. **支付宝**：
   - 注册支付宝开发者账号
   - 获取 App ID 和密钥
   - 集成支付宝 SDK
   - 实现支付回调处理

2. **微信支付**：
   - 注册微信支付商户
   - 获取商户号和 API 密钥
   - 集成微信支付 SDK
   - 实现支付回调处理

3. **银行支付**：
   - 对接银行支付网关
   - 实现银行卡四要素验证
   - 实现快捷支付或网银支付

---

## 10. 测试和验证 (Testing and Validation)

### 生产部署前检查清单

- [ ] 账户号码已加密存储
- [ ] 支付密码功能已实施
- [ ] 交易状态异步更新流程已实施
- [ ] 交易限额已配置
- [ ] 数据库事务已正确使用
- [ ] 交易通知已实施
- [ ] 审计日志已实施
- [ ] 第三方支付已集成并测试
- [ ] 错误处理机制已完善
- [ ] 安全测试已通过
- [ ] 渗透测试已通过
- [ ] 性能测试已通过
- [ ] 负载测试已通过
- [ ] 灾备方案已制定
- [ ] 数据备份策略已实施

---

## 总结 (Summary)

本钱包系统实现提供了一个**坚实的基础架构**和**完整的数据模型**，但在生产环境部署前，**必须实施上述安全措施**。

特别重要的安全措施：
1. ✅ **必须**：账户号码加密
2. ✅ **必须**：支付密码验证
3. ✅ **必须**：交易状态异步更新
4. ✅ **必须**：数据库事务
5. ✅ **必须**：第三方支付集成
6. ⚠️ **建议**：交易限额
7. ⚠️ **建议**：交易通知
8. ⚠️ **建议**：审计日志
9. ⚠️ **根据需要**：反洗钱合规

请务必在部署到生产环境前，咨询安全专家和法律顾问，确保系统符合所有安全标准和法律法规要求。

---

**文档版本**: 1.0  
**更新日期**: 2026-01-07  
**适用系统**: 全品类可回收物预约回收系统钱包模块
