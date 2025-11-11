-- 创建用户反馈表
-- 用于存储用户提交的问题反馈、功能建议、投诉举报等信息

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFeedback]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserFeedback] (
        [FeedbackID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] INT NOT NULL,
        [FeedbackType] NVARCHAR(50) NOT NULL,  -- '问题反馈', '功能建议', '投诉举报', '其他'
        [Subject] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL,
        [ContactEmail] NVARCHAR(100) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT N'反馈中',  -- '反馈中', '已完成'
        [AdminReply] NVARCHAR(1000) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT FK_UserFeedback_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT CK_UserFeedback_FeedbackType 
            CHECK ([FeedbackType] IN (N'问题反馈', N'功能建议', N'投诉举报', N'其他')),
        CONSTRAINT CK_UserFeedback_Status 
            CHECK ([Status] IN (N'反馈中', N'已完成'))
    );
    
    -- 创建索引以提高查询性能
    CREATE INDEX IX_UserFeedback_UserID ON [dbo].[UserFeedback]([UserID]);
    CREATE INDEX IX_UserFeedback_Status ON [dbo].[UserFeedback]([Status]);
    CREATE INDEX IX_UserFeedback_FeedbackType ON [dbo].[UserFeedback]([FeedbackType]);
    CREATE INDEX IX_UserFeedback_CreatedDate ON [dbo].[UserFeedback]([CreatedDate] DESC);
    
    PRINT 'UserFeedback 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserFeedback 表已存在';
END
GO
