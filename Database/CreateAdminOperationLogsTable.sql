-- 创建管理员操作日志表
-- Create AdminOperationLogs table for tracking admin actions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminOperationLogs] (
        [LogID] INT IDENTITY(1,1) PRIMARY KEY,
        [AdminID] INT NOT NULL,
        [AdminUsername] NVARCHAR(50) NULL,
        [Module] NVARCHAR(50) NOT NULL,
        [OperationType] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [TargetID] INT NULL,
        [TargetName] NVARCHAR(100) NULL,
        [IPAddress] NVARCHAR(50) NULL,
        [OperationTime] DATETIME NOT NULL DEFAULT GETDATE(),
        [Result] NVARCHAR(20) NULL,
        [Details] NVARCHAR(MAX) NULL
    );

    -- Create indexes for better query performance
    CREATE INDEX [IX_AdminOperationLogs_AdminID] ON [dbo].[AdminOperationLogs] ([AdminID]);
    CREATE INDEX [IX_AdminOperationLogs_Module] ON [dbo].[AdminOperationLogs] ([Module]);
    CREATE INDEX [IX_AdminOperationLogs_OperationType] ON [dbo].[AdminOperationLogs] ([OperationType]);
    CREATE INDEX [IX_AdminOperationLogs_OperationTime] ON [dbo].[AdminOperationLogs] ([OperationTime] DESC);

    PRINT 'AdminOperationLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'AdminOperationLogs table already exists.';
END
GO

-- Add comments for the table and columns
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'管理员操作日志表，记录所有管理操作', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'AdminOperationLogs';
GO
