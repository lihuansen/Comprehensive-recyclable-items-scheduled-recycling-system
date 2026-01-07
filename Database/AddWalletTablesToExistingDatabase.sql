-- ==============================================================================
-- 钱包系统表添加脚本（用于现有数据库）
-- Add Wallet System Tables to Existing Database
-- 
-- 使用说明:
-- 1. 在 SQL Server Management Studio (SSMS) 中打开此脚本
-- 2. 确保已连接到数据库 RecyclingSystemDB
-- 3. 执行此脚本 (按 F5 或点击"执行"按钮)
-- 
-- 此脚本用于修复"对象名 'UserPaymentAccounts' 无效"错误
-- ==============================================================================

USE RecyclingSystemDB;
GO

PRINT '========================================';
PRINT '开始添加钱包系统表...';
PRINT '========================================';
PRINT '';

-- ==============================================================================
-- 1. 为 Users 表添加 money 列（如果不存在）
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Users]') 
               AND name = 'money')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [money] DECIMAL(18,2) NULL DEFAULT 0.00;
    
    PRINT '✓ Users 表添加 money 列成功';
    
    -- 更新现有用户的 money 字段默认值（如果为 NULL）
    UPDATE [dbo].[Users]
    SET [money] = 0.00
    WHERE [money] IS NULL;
    
    PRINT '✓ 已初始化现有用户的钱包余额为 0.00';
END
ELSE
BEGIN
    PRINT '✓ Users 表的 money 列已存在';
END
GO

-- ==============================================================================
-- 2. UserPaymentAccounts 表（用户支付账户表）
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPaymentAccounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserPaymentAccounts] (
        [AccountID] INT PRIMARY KEY IDENTITY(1,1),              -- 账户ID（自增主键）
        [UserID] INT NOT NULL,                                   -- 用户ID（外键关联Users表）
        [AccountType] NVARCHAR(20) NOT NULL,                     -- 账户类型：Alipay(支付宝), WeChat(微信), BankCard(银行卡)
        [AccountName] NVARCHAR(100) NOT NULL,                    -- 账户名称/持卡人姓名
        [AccountNumber] NVARCHAR(100) NOT NULL,                  -- 账户号/卡号（加密存储）
        [BankName] NVARCHAR(100) NULL,                           -- 银行名称（仅银行卡需要）
        [IsDefault] BIT NOT NULL DEFAULT 0,                      -- 是否默认账户
        [IsVerified] BIT NOT NULL DEFAULT 0,                     -- 是否已验证
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),      -- 创建时间
        [LastUsedDate] DATETIME2 NULL,                           -- 最后使用时间
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Active',         -- 状态：Active(激活), Suspended(暂停), Deleted(已删除)
        
        -- 外键约束
        CONSTRAINT FK_UserPaymentAccounts_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        
        -- 检查约束
        CONSTRAINT CHK_AccountType CHECK ([AccountType] IN ('Alipay', 'WeChat', 'BankCard')),
        CONSTRAINT CHK_PaymentAccountStatus CHECK ([Status] IN ('Active', 'Suspended', 'Deleted'))
    );

    -- 创建索引
    CREATE INDEX IX_UserPaymentAccounts_UserID ON [dbo].[UserPaymentAccounts]([UserID]);
    CREATE INDEX IX_UserPaymentAccounts_AccountType ON [dbo].[UserPaymentAccounts]([AccountType]);
    CREATE INDEX IX_UserPaymentAccounts_IsDefault ON [dbo].[UserPaymentAccounts]([IsDefault]);
    CREATE INDEX IX_UserPaymentAccounts_Status ON [dbo].[UserPaymentAccounts]([Status]);

    PRINT '✓ UserPaymentAccounts 表创建成功';
END
ELSE
BEGIN
    PRINT '✓ UserPaymentAccounts 表已存在';
END
GO

-- ==============================================================================
-- 3. WalletTransactions 表（钱包交易记录表）
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WalletTransactions] (
        [TransactionID] INT PRIMARY KEY IDENTITY(1,1),          -- 交易ID（自增主键）
        [UserID] INT NOT NULL,                                   -- 用户ID（外键关联Users表）
        [TransactionType] NVARCHAR(20) NOT NULL,                 -- 交易类型：Recharge(充值), Withdraw(提现), Payment(支付), Refund(退款), Income(收入)
        [Amount] DECIMAL(18,2) NOT NULL,                         -- 交易金额
        [BalanceBefore] DECIMAL(18,2) NOT NULL,                  -- 交易前余额
        [BalanceAfter] DECIMAL(18,2) NOT NULL,                   -- 交易后余额
        [PaymentAccountID] INT NULL,                             -- 支付账户ID（外键，充值/提现时使用）
        [RelatedOrderID] INT NULL,                               -- 关联订单ID（支付/退款时使用）
        [TransactionStatus] NVARCHAR(20) NOT NULL DEFAULT 'Completed', -- 交易状态：Pending(待处理), Processing(处理中), Completed(已完成), Failed(失败), Cancelled(已取消)
        [Description] NVARCHAR(500) NULL,                        -- 交易描述
        [TransactionNo] NVARCHAR(50) NOT NULL UNIQUE,            -- 交易流水号（唯一）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),      -- 创建时间
        [CompletedDate] DATETIME2 NULL,                          -- 完成时间
        [Remarks] NVARCHAR(500) NULL,                            -- 备注
        
        -- 外键约束
        CONSTRAINT FK_WalletTransactions_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT FK_WalletTransactions_PaymentAccounts FOREIGN KEY ([PaymentAccountID]) 
            REFERENCES [dbo].[UserPaymentAccounts]([AccountID]),
        
        -- 检查约束
        CONSTRAINT CHK_TransactionType CHECK ([TransactionType] IN ('Recharge', 'Withdraw', 'Payment', 'Refund', 'Income')),
        CONSTRAINT CHK_WalletTransactionStatus CHECK ([TransactionStatus] IN ('Pending', 'Processing', 'Completed', 'Failed', 'Cancelled')),
        CONSTRAINT CHK_Amount CHECK ([Amount] >= 0)
    );

    -- 创建索引
    CREATE INDEX IX_WalletTransactions_UserID ON [dbo].[WalletTransactions]([UserID]);
    CREATE INDEX IX_WalletTransactions_TransactionType ON [dbo].[WalletTransactions]([TransactionType]);
    CREATE INDEX IX_WalletTransactions_TransactionStatus ON [dbo].[WalletTransactions]([TransactionStatus]);
    CREATE INDEX IX_WalletTransactions_CreatedDate ON [dbo].[WalletTransactions]([CreatedDate] DESC);
    CREATE UNIQUE INDEX IX_WalletTransactions_TransactionNo ON [dbo].[WalletTransactions]([TransactionNo]);

    PRINT '✓ WalletTransactions 表创建成功';
END
ELSE
BEGIN
    PRINT '✓ WalletTransactions 表已存在';
END
GO

PRINT '';
PRINT '========================================';
PRINT '钱包系统表添加完成！';
PRINT '========================================';
PRINT '';
PRINT '已创建/检查的表：';
PRINT '  1. Users.money - 用户余额字段';
PRINT '  2. UserPaymentAccounts - 用户支付账户表';
PRINT '  3. WalletTransactions - 钱包交易记录表';
PRINT '';
PRINT '现在您可以使用钱包功能了！';
PRINT '';
GO
