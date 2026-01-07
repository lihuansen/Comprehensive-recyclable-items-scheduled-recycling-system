-- ============================================================================
-- 诊断和修复 UserPaymentAccounts 表缺失问题
-- Diagnose and Fix UserPaymentAccounts Table Missing Issue
-- ============================================================================
-- 
-- 问题描述: System.Data.SqlClient.SqlException:"对象名 'UserPaymentAccounts' 无效。"
-- Problem: Invalid object name 'UserPaymentAccounts'
--
-- 此脚本将:
-- 1. 诊断数据库中是否存在 UserPaymentAccounts 和 WalletTransactions 表
-- 2. 检查 Users 表是否有 money 列
-- 3. 如果不存在，自动创建这些表和列
-- 4. 提供详细的诊断信息
-- ============================================================================

USE RecyclingSystemDB;
GO

PRINT '========================================';
PRINT '开始诊断 UserPaymentAccounts 问题';
PRINT 'Starting UserPaymentAccounts Diagnosis';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 第1步: 检查数据库连接
-- ============================================================================
PRINT '【步骤 1】检查数据库连接...';
PRINT 'Current Database: ' + DB_NAME();
PRINT '';

-- ============================================================================
-- 第2步: 检查 Users 表的 money 列
-- ============================================================================
PRINT '【步骤 2】检查 Users 表的 money 列...';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'money')
BEGIN
    PRINT '✓ Users.money 列存在';
END
ELSE
BEGIN
    PRINT '✗ Users.money 列不存在 - 正在添加...';
    
    ALTER TABLE [dbo].[Users] 
    ADD [money] DECIMAL(18,2) NULL DEFAULT 0.00;
    
    -- 初始化现有用户的钱包余额
    UPDATE [dbo].[Users] 
    SET [money] = 0.00 
    WHERE [money] IS NULL;
    
    PRINT '✓ Users.money 列已添加并初始化';
END
PRINT '';

-- ============================================================================
-- 第3步: 检查 UserPaymentAccounts 表
-- ============================================================================
PRINT '【步骤 3】检查 UserPaymentAccounts 表...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPaymentAccounts]') AND type in (N'U'))
BEGIN
    DECLARE @rowCount1 INT;
    SELECT @rowCount1 = COUNT(*) FROM [dbo].[UserPaymentAccounts];
    PRINT '✓ UserPaymentAccounts 表存在 (记录数: ' + CAST(@rowCount1 AS VARCHAR) + ')';
    
    -- 显示表结构
    PRINT '  表结构:';
    SELECT 
        COLUMN_NAME as '列名',
        DATA_TYPE as '数据类型',
        CHARACTER_MAXIMUM_LENGTH as '最大长度',
        IS_NULLABLE as '可为空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserPaymentAccounts'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '✗ UserPaymentAccounts 表不存在 - 正在创建...';
    
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

    PRINT '✓ UserPaymentAccounts 表已创建';
END
PRINT '';

-- ============================================================================
-- 第4步: 检查 WalletTransactions 表
-- ============================================================================
PRINT '【步骤 4】检查 WalletTransactions 表...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U'))
BEGIN
    DECLARE @rowCount2 INT;
    SELECT @rowCount2 = COUNT(*) FROM [dbo].[WalletTransactions];
    PRINT '✓ WalletTransactions 表存在 (记录数: ' + CAST(@rowCount2 AS VARCHAR) + ')';
    
    -- 显示表结构
    PRINT '  表结构:';
    SELECT 
        COLUMN_NAME as '列名',
        DATA_TYPE as '数据类型',
        CHARACTER_MAXIMUM_LENGTH as '最大长度',
        IS_NULLABLE as '可为空'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'WalletTransactions'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '✗ WalletTransactions 表不存在 - 正在创建...';
    
    CREATE TABLE [dbo].[WalletTransactions] (
        [TransactionID] INT PRIMARY KEY IDENTITY(1,1),          -- 交易ID（自增主键）
        [UserID] INT NOT NULL,                                   -- 用户ID（外键关联Users表）
        [TransactionType] NVARCHAR(20) NOT NULL,                 -- 交易类型：Recharge(充值), Withdraw(提现), Payment(支付), Refund(退款), Income(收入)
        [Amount] DECIMAL(18,2) NOT NULL,                         -- 交易金额
        [BalanceBefore] DECIMAL(18,2) NOT NULL,                  -- 交易前余额
        [BalanceAfter] DECIMAL(18,2) NOT NULL,                   -- 交易后余额
        [PaymentAccountID] INT NULL,                             -- 支付账户ID（外键，充值/提现时使用）
        [RelatedOrderID] INT NULL,                               -- 关联订单ID（支付/退款时使用）
        [TransactionStatus] NVARCHAR(20) NOT NULL DEFAULT 'Completed',  -- 交易状态：Pending(待处理), Processing(处理中), Completed(已完成), Failed(失败), Cancelled(已取消)
        [Description] NVARCHAR(500) NULL,                        -- 交易描述
        [TransactionNo] NVARCHAR(50) NOT NULL UNIQUE,            -- 交易流水号（唯一）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),      -- 创建时间
        [CompletedDate] DATETIME2 NULL,                          -- 完成时间
        [Remarks] NVARCHAR(500) NULL,                            -- 备注
        
        -- 外键约束
        CONSTRAINT FK_WalletTransactions_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT FK_WalletTransactions_PaymentAccount FOREIGN KEY ([PaymentAccountID]) 
            REFERENCES [dbo].[UserPaymentAccounts]([AccountID]),
        
        -- 检查约束
        CONSTRAINT CHK_TransactionType CHECK ([TransactionType] IN ('Recharge', 'Withdraw', 'Payment', 'Refund', 'Income')),
        CONSTRAINT CHK_TransactionStatus CHECK ([TransactionStatus] IN ('Pending', 'Processing', 'Completed', 'Failed', 'Cancelled')),
        CONSTRAINT CHK_Amount CHECK ([Amount] > 0)
    );

    -- 创建索引
    CREATE INDEX IX_WalletTransactions_UserID ON [dbo].[WalletTransactions]([UserID]);
    CREATE INDEX IX_WalletTransactions_TransactionType ON [dbo].[WalletTransactions]([TransactionType]);
    CREATE INDEX IX_WalletTransactions_TransactionStatus ON [dbo].[WalletTransactions]([TransactionStatus]);
    CREATE INDEX IX_WalletTransactions_CreatedDate ON [dbo].[WalletTransactions]([CreatedDate]);
    CREATE UNIQUE INDEX IX_WalletTransactions_TransactionNo ON [dbo].[WalletTransactions]([TransactionNo]);

    PRINT '✓ WalletTransactions 表已创建';
END
PRINT '';

-- ============================================================================
-- 第5步: 验证外键关系
-- ============================================================================
PRINT '【步骤 5】验证外键关系...';

SELECT 
    fk.name AS '外键名称',
    OBJECT_NAME(fk.parent_object_id) AS '从表',
    OBJECT_NAME(fk.referenced_object_id) AS '到表'
FROM sys.foreign_keys AS fk
WHERE OBJECT_NAME(fk.parent_object_id) IN ('UserPaymentAccounts', 'WalletTransactions')
   OR OBJECT_NAME(fk.referenced_object_id) IN ('UserPaymentAccounts', 'WalletTransactions');

PRINT '';

-- ============================================================================
-- 第6步: 验证索引
-- ============================================================================
PRINT '【步骤 6】验证索引...';

SELECT 
    t.name AS '表名',
    i.name AS '索引名称',
    i.type_desc AS '索引类型'
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('UserPaymentAccounts', 'WalletTransactions')
ORDER BY t.name, i.name;

PRINT '';

-- ============================================================================
-- 第7步: 测试查询
-- ============================================================================
PRINT '【步骤 7】执行测试查询...';

BEGIN TRY
    -- 测试查询 UserPaymentAccounts 表
    DECLARE @testCount1 INT;
    SELECT @testCount1 = COUNT(*) FROM [dbo].[UserPaymentAccounts];
    PRINT '✓ UserPaymentAccounts 表查询成功 (记录数: ' + CAST(@testCount1 AS VARCHAR) + ')';
    
    -- 测试查询 WalletTransactions 表
    DECLARE @testCount2 INT;
    SELECT @testCount2 = COUNT(*) FROM [dbo].[WalletTransactions];
    PRINT '✓ WalletTransactions 表查询成功 (记录数: ' + CAST(@testCount2 AS VARCHAR) + ')';
    
    -- 测试查询 Users.money 列
    DECLARE @testCount3 INT;
    SELECT @testCount3 = COUNT(*) FROM [dbo].[Users] WHERE money IS NOT NULL;
    PRINT '✓ Users.money 列查询成功 (有余额的用户数: ' + CAST(@testCount3 AS VARCHAR) + ')';
    
    PRINT '';
    PRINT '========================================';
    PRINT '诊断完成！所有表和列都已就绪。';
    PRINT 'Diagnosis Complete! All tables and columns are ready.';
    PRINT '========================================';
    
END TRY
BEGIN CATCH
    PRINT '';
    PRINT '✗ 测试查询失败:';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
END CATCH

PRINT '';

-- ============================================================================
-- 第8步: 提供下一步建议
-- ============================================================================
PRINT '【下一步建议】';
PRINT '1. 如果所有检查都通过，请重启应用程序';
PRINT '2. 访问"我的钱包"页面，检查是否还有错误';
PRINT '3. 如果仍有错误，请检查 Web.config 中的连接字符串是否指向正确的数据库';
PRINT '4. 确认连接字符串中的数据库名称为: RecyclingSystemDB';
PRINT '';
PRINT '【Next Steps】';
PRINT '1. If all checks pass, restart your application';
PRINT '2. Visit the "My Wallet" page and check for errors';
PRINT '3. If errors persist, verify Web.config connection string points to the correct database';
PRINT '4. Ensure the database name in connection string is: RecyclingSystemDB';
PRINT '';

GO
