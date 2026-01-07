# 钱包系统架构图
# Wallet System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            用户界面层 (Presentation Layer)                     │
│                         /Home/MyWallet - 我的钱包页面                         │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              控制器层 (Controller Layer)                       │
│                              HomeController.cs                               │
│  ┌───────────────────────────────────────────────────────────────────┐     │
│  │  public ActionResult MyWallet()                                    │     │
│  │  {                                                                 │     │
│  │      var walletViewModel = _walletTransactionBLL                  │     │
│  │                              .GetWalletViewModel(userId);          │     │
│  │      return View(walletViewModel);                                │     │
│  │  }                                                                 │     │
│  └───────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         业务逻辑层 (Business Logic Layer)                     │
│  ┌─────────────────────────────┐      ┌──────────────────────────────┐     │
│  │  PaymentAccountBLL.cs       │      │  WalletTransactionBLL.cs     │     │
│  ├─────────────────────────────┤      ├──────────────────────────────┤     │
│  │  • AddPaymentAccount()      │      │  • Recharge()                │     │
│  │  • GetPaymentAccounts()     │      │  • Withdraw()                │     │
│  │  • DeletePaymentAccount()   │      │  • GetTransactions()         │     │
│  │  • SetDefaultAccount()      │      │  • GetStatistics()           │     │
│  │  • VerifyAccount()          │      │  • GetWalletViewModel()      │     │
│  └─────────────────────────────┘      └──────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          数据访问层 (Data Access Layer)                       │
│  ┌─────────────────────────────┐      ┌──────────────────────────────┐     │
│  │  PaymentAccountDAL.cs       │      │  WalletTransactionDAL.cs     │     │
│  ├─────────────────────────────┤      ├──────────────────────────────┤     │
│  │  • AddPaymentAccount()      │      │  • AddTransaction()          │     │
│  │  • GetPaymentAccountById()  │      │  • GetTransactionById()      │     │
│  │  • UpdatePaymentAccount()   │      │  • UpdateTransactionStatus() │     │
│  │  • DeletePaymentAccount()   │      │  • GetUserStatistics()       │     │
│  │  • SetDefaultAccount()      │      │  • GenerateTransactionNo()   │     │
│  └─────────────────────────────┘      └──────────────────────────────┘     │
│                                │                    │                        │
│                                └────────┬───────────┘                        │
│                                         ▼                                    │
│                              ┌──────────────────┐                           │
│                              │   SqlHelper      │                           │
│                              └──────────────────┘                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              数据库层 (Database Layer)                        │
│                                  SQL Server                                  │
│  ┌──────────────────────────┐  ┌──────────────────────┐  ┌─────────────┐  │
│  │  Users                   │  │ UserPaymentAccounts  │  │ WalletTrans │  │
│  ├──────────────────────────┤  ├──────────────────────┤  │  actions    │  │
│  │ • UserID (PK)            │  │ • AccountID (PK)     │  │ • TransID   │  │
│  │ • Username               │  │ • UserID (FK)        │  │   (PK)      │  │
│  │ • Email                  │  │ • AccountType        │  │ • UserID    │  │
│  │ • PhoneNumber            │  │   - Alipay           │  │   (FK)      │  │
│  │ • money (余额)           │◄─┤   - WeChat           │  │ • Type      │  │
│  └──────────────────────────┘  │   - BankCard         │  │ • Amount    │  │
│                                 │ • AccountName        │  │ • BalBefore │  │
│                                 │ • AccountNumber      │  │ • BalAfter  │  │
│                                 │ • BankName           │  │ • PayAccID  │  │
│                                 │ • IsDefault          │◄─┤   (FK)      │  │
│                                 │ • IsVerified         │  │ • Status    │  │
│                                 │ • Status             │  │ • TxnNo     │  │
│                                 └──────────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘

数据流示例 (Data Flow Example):
═══════════════════════════════════════════════════════════════════════════════

1. 用户访问钱包页面 (User visits wallet page):
   Browser → HomeController.MyWallet() → WalletTransactionBLL.GetWalletViewModel()
   → PaymentAccountDAL.GetPaymentAccounts() + WalletTransactionDAL.GetTransactions()
   → Database Query → Return WalletViewModel → Render MyWallet.cshtml

2. 充值流程 (Recharge flow):
   User clicks Recharge → Submit RechargeViewModel 
   → WalletTransactionBLL.Recharge(model, userId)
   ├─ Validate amount and payment account
   ├─ Get current balance from Users table
   ├─ Create transaction record: WalletTransactionDAL.AddTransaction()
   ├─ Update user balance: UserDAL.UpdateUser()
   └─ Update account last used: PaymentAccountDAL.UpdateLastUsedDate()

3. 添加支付账户 (Add payment account):
   User clicks Add Account → Submit AddPaymentAccountViewModel
   → PaymentAccountBLL.AddPaymentAccount(model, userId)
   ├─ Validate account type and data
   ├─ If IsDefault, clear other default accounts
   └─ PaymentAccountDAL.AddPaymentAccount() → Insert into database

视图模型结构 (ViewModel Structure):
═══════════════════════════════════════════════════════════════════════════════

WalletViewModel
├─ User (用户基本信息)
├─ PaymentAccounts (支付账户列表)
│  ├─ Account 1
│  │  ├─ AccountType: "Alipay" → Display: "支付宝"
│  │  ├─ AccountNumber: "13800138000" → Masked: "1380****8000"
│  │  ├─ IsDefault: true → Show badge "默认"
│  │  └─ IsVerified: true → Show icon ✓
│  ├─ Account 2 (WeChat)
│  └─ Account 3 (BankCard)
├─ RecentTransactions (最近交易记录)
│  ├─ Transaction 1
│  │  ├─ Type: "Recharge" → Display: "充值" (Green +)
│  │  ├─ Amount: 100.00
│  │  ├─ BalanceAfter: 100.00
│  │  └─ TransactionNo: "TXN20260107123456001"
│  └─ Transaction 2
│     ├─ Type: "Payment" → Display: "支付" (Red -)
│     ├─ Amount: 20.00
│     └─ BalanceAfter: 80.00
└─ Statistics (统计数据)
   ├─ CurrentBalance: 80.00 (当前余额)
   ├─ TotalIncome: 100.00 (累计收入)
   ├─ TotalExpense: 20.00 (累计支出)
   └─ MonthlyTransactionCount: 2 (本月交易)

关键特性 (Key Features):
═══════════════════════════════════════════════════════════════════════════════

✓ 多支付方式        - 支持支付宝、微信、银行卡
✓ 账户脱敏          - 显示脱敏账户号（1380****8000）
✓ 默认账户          - 支持设置和管理默认支付账户
✓ 交易追踪          - 完整记录每笔交易的前后余额
✓ 实时统计          - 自动计算收入、支出、交易次数
✓ 交易流水号        - 每笔交易生成唯一流水号
✓ 状态管理          - 交易状态、账户状态的完整管理
✓ 软删除            - 账户删除保留历史记录
✓ 安全性            - 余额验证、权限检查、数据验证

扩展点 (Extension Points):
═══════════════════════════════════════════════════════════════════════════════

→ 第三方支付集成     - 支付宝SDK、微信支付SDK
→ 支付密码          - 添加支付密码验证层
→ 账户验证          - 实名认证、银行卡四要素验证
→ 交易限额          - 单笔/每日限额控制
→ 通知系统          - 交易成功短信/邮件通知
→ 对账功能          - 月度账单、交易报表
→ 风控系统          - 异常交易检测、风险预警
```
