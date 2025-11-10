-- 创建管理员联系消息表
-- 用于存储用户和管理员之间的聊天消息

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminContactMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminContactMessages] (
        [MessageID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] INT NOT NULL,
        [AdminID] INT NULL,  -- 可选，指定管理员ID
        [SenderType] NVARCHAR(20) NOT NULL,  -- 'user', 'admin', 'system'
        [Content] NVARCHAR(2000) NOT NULL,
        [SentTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsRead] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_AdminContactMessages_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT CK_AdminContactMessages_SenderType 
            CHECK ([SenderType] IN ('user', 'admin', 'system'))
    );
    
    -- 创建索引以提高查询性能
    CREATE INDEX IX_AdminContactMessages_UserID ON [dbo].[AdminContactMessages]([UserID]);
    CREATE INDEX IX_AdminContactMessages_AdminID ON [dbo].[AdminContactMessages]([AdminID]);
    CREATE INDEX IX_AdminContactMessages_SentTime ON [dbo].[AdminContactMessages]([SentTime]);
    
    PRINT 'AdminContactMessages 表创建成功';
END
ELSE
BEGIN
    PRINT 'AdminContactMessages 表已存在';
END
GO

-- 创建管理员联系会话表
-- 用于跟踪用户和管理员之间的会话状态

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminContactConversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminContactConversations] (
        [ConversationID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] INT NOT NULL,
        [AdminID] INT NULL,  -- 可选，指定管理员ID
        [StartTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UserEndedTime] DATETIME2 NULL,
        [AdminEndedTime] DATETIME2 NULL,
        [UserEnded] BIT NOT NULL DEFAULT 0,
        [AdminEnded] BIT NOT NULL DEFAULT 0,
        [LastMessageTime] DATETIME2 NULL,
        CONSTRAINT FK_AdminContactConversations_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );
    
    -- 创建索引
    CREATE INDEX IX_AdminContactConversations_UserID ON [dbo].[AdminContactConversations]([UserID]);
    CREATE INDEX IX_AdminContactConversations_AdminID ON [dbo].[AdminContactConversations]([AdminID]);
    
    PRINT 'AdminContactConversations 表创建成功';
END
ELSE
BEGIN
    PRINT 'AdminContactConversations 表已存在';
END
GO
