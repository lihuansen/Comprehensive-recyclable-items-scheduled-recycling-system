-- 创建用户联系请求表
-- 用于记录用户点击"联系我们"的请求，管理员可以看到哪些用户需要联系

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserContactRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserContactRequests] (
        [RequestID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] INT NOT NULL,
        [RequestStatus] BIT NOT NULL DEFAULT 1,  -- 1=待联系, 0=已处理
        [RequestTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [ContactedTime] DATETIME2 NULL,  -- 管理员处理时间
        [AdminID] INT NULL,  -- 处理的管理员ID
        CONSTRAINT FK_UserContactRequests_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );
    
    -- 创建索引以提高查询性能
    CREATE INDEX IX_UserContactRequests_UserID ON [dbo].[UserContactRequests]([UserID]);
    CREATE INDEX IX_UserContactRequests_RequestStatus ON [dbo].[UserContactRequests]([RequestStatus]);
    CREATE INDEX IX_UserContactRequests_RequestTime ON [dbo].[UserContactRequests]([RequestTime]);
    
    PRINT 'UserContactRequests 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserContactRequests 表已存在';
END
GO
