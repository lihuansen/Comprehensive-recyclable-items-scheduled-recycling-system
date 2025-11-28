-- ==============================================================================
-- 创建管理员操作日志表 (AdminOperationLogs Table)
-- 用途：记录管理员端的所有操作日志，用于日志管理功能
-- Create AdminOperationLogs table for tracking admin actions
-- 
-- 使用方法 (How to use):
-- 1. 在 SQL Server Management Studio (SSMS) 中打开此脚本
-- 2. 连接到您的数据库 (RecyclingDB)
-- 3. 执行此脚本 (按 F5 或点击"执行"按钮)
-- 
-- 或使用命令行:
-- sqlcmd -S localhost -d RecyclingDB -i CreateAdminOperationLogsTable.sql
-- ==============================================================================

USE RecyclingDB;
GO

-- 检查表是否存在，如果不存在则创建
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    PRINT '正在创建 AdminOperationLogs 表...';
    
    CREATE TABLE [dbo].[AdminOperationLogs] (
        -- 日志ID（主键，自增）
        [LogID] INT IDENTITY(1,1) PRIMARY KEY,
        
        -- 操作管理员ID（关联到 Admins 或 SuperAdmins 表）
        [AdminID] INT NOT NULL,
        
        -- 管理员用户名（冗余存储，便于查询）
        [AdminUsername] NVARCHAR(50) NULL,
        
        -- 操作模块：
        -- UserManagement = 用户管理
        -- RecyclerManagement = 回收员管理
        -- FeedbackManagement = 反馈管理
        -- HomepageManagement = 首页页面管理
        -- LogManagement = 日志管理
        [Module] NVARCHAR(50) NOT NULL,
        
        -- 操作类型：
        -- View = 查看
        -- Create = 新增
        -- Update = 更新
        -- Delete = 删除
        -- Export = 导出
        -- Reply = 回复
        -- Search = 搜索
        [OperationType] NVARCHAR(50) NOT NULL,
        
        -- 操作描述（中文描述具体操作内容）
        [Description] NVARCHAR(500) NULL,
        
        -- 目标对象ID（如用户ID、回收员ID等）
        [TargetID] INT NULL,
        
        -- 目标对象名称（如用户名、回收员名等）
        [TargetName] NVARCHAR(100) NULL,
        
        -- 操作者IP地址
        [IPAddress] NVARCHAR(50) NULL,
        
        -- 操作时间（默认为当前时间）
        [OperationTime] DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- 操作结果：Success = 成功，Failed = 失败
        [Result] NVARCHAR(20) NULL,
        
        -- 附加详情（JSON格式，可存储更多操作细节）
        [Details] NVARCHAR(MAX) NULL
    );

    -- 创建索引以提高查询性能
    CREATE INDEX [IX_AdminOperationLogs_AdminID] ON [dbo].[AdminOperationLogs] ([AdminID]);
    CREATE INDEX [IX_AdminOperationLogs_Module] ON [dbo].[AdminOperationLogs] ([Module]);
    CREATE INDEX [IX_AdminOperationLogs_OperationType] ON [dbo].[AdminOperationLogs] ([OperationType]);
    CREATE INDEX [IX_AdminOperationLogs_OperationTime] ON [dbo].[AdminOperationLogs] ([OperationTime] DESC);
    CREATE INDEX [IX_AdminOperationLogs_Result] ON [dbo].[AdminOperationLogs] ([Result]);

    PRINT 'AdminOperationLogs 表创建成功！';
    PRINT '已创建以下索引：';
    PRINT '  - IX_AdminOperationLogs_AdminID';
    PRINT '  - IX_AdminOperationLogs_Module';
    PRINT '  - IX_AdminOperationLogs_OperationType';
    PRINT '  - IX_AdminOperationLogs_OperationTime';
    PRINT '  - IX_AdminOperationLogs_Result';
END
ELSE
BEGIN
    PRINT 'AdminOperationLogs 表已存在，跳过创建。';
END
GO

-- 添加表和列的扩展属性（注释）- 仅当表存在时执行
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'dbo', N'TABLE', N'AdminOperationLogs', NULL, NULL))
    BEGIN
        EXEC sp_addextendedproperty 
            @name = N'MS_Description', 
            @value = N'管理员操作日志表，记录所有管理操作', 
            @level0type = N'SCHEMA', @level0name = N'dbo', 
            @level1type = N'TABLE', @level1name = N'AdminOperationLogs';
        PRINT '已添加表注释。';
    END
END
GO

-- ==============================================================================
-- 验证表创建结果（仅当表存在时执行）
-- ==============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    PRINT '';
    PRINT '============================================';
    PRINT '表结构验证:';
    PRINT '============================================';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    SELECT 
        COLUMN_NAME AS '列名',
        DATA_TYPE AS '数据类型',
        CHARACTER_MAXIMUM_LENGTH AS '最大长度',
        IS_NULLABLE AS '可为空',
        COLUMN_DEFAULT AS '默认值'
    FROM 
        INFORMATION_SCHEMA.COLUMNS 
    WHERE 
        TABLE_NAME = 'AdminOperationLogs'
    ORDER BY 
        ORDINAL_POSITION;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    PRINT '';
    PRINT '============================================';
    PRINT '索引列表:';
    PRINT '============================================';

    SELECT 
        i.name AS '索引名称',
        c.name AS '列名',
        i.type_desc AS '索引类型'
    FROM 
        sys.indexes i
        INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE 
        i.object_id = OBJECT_ID('AdminOperationLogs')
        AND i.name IS NOT NULL
    ORDER BY 
        i.name;

    PRINT '';
    PRINT '============================================';
    PRINT 'AdminOperationLogs 表设置完成！';
    PRINT '现在可以在管理员端访问日志管理功能了。';
    PRINT '============================================';
END
ELSE
BEGIN
    PRINT '';
    PRINT '============================================';
    PRINT '警告: AdminOperationLogs 表不存在或创建失败！';
    PRINT '请检查数据库连接和权限后重新执行脚本。';
    PRINT '============================================';
END
GO
