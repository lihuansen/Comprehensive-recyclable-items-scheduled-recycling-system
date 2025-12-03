-- 创建用户通知表
-- Create UserNotifications table for storing user notification messages

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserNotifications' AND xtype='U')
BEGIN
    CREATE TABLE UserNotifications (
        NotificationID INT PRIMARY KEY IDENTITY(1,1),   -- 通知ID（自增主键）
        UserID INT NOT NULL,                             -- 用户ID（外键）
        NotificationType NVARCHAR(50) NULL,              -- 通知类型
        Title NVARCHAR(200) NULL,                        -- 通知标题
        Content NVARCHAR(1000) NULL,                     -- 通知内容
        RelatedOrderID INT NULL,                         -- 关联订单ID
        RelatedFeedbackID INT NULL,                      -- 关联反馈ID
        CreatedDate DATETIME2 NOT NULL,                  -- 创建时间
        IsRead BIT NOT NULL DEFAULT 0,                   -- 是否已读
        ReadDate DATETIME2 NULL,                         -- 已读时间
        
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY (UserID) 
            REFERENCES Users(UserID) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_UserNotifications_UserID ON UserNotifications(UserID);
    CREATE INDEX IX_UserNotifications_CreatedDate ON UserNotifications(CreatedDate);
    CREATE INDEX IX_UserNotifications_IsRead ON UserNotifications(IsRead);
    CREATE INDEX IX_UserNotifications_NotificationType ON UserNotifications(NotificationType);

    PRINT 'UserNotifications table created successfully.';
END
ELSE
BEGIN
    PRINT 'UserNotifications table already exists.';
END
GO

-- 通知类型说明：
-- OrderCreated     - 预约订单已生成
-- OrderAccepted    - 回收员已接单
-- OrderCompleted   - 订单已完成
-- ReviewReminder   - 评价提醒
-- OrderCancelled   - 订单已取消
-- CarouselUpdated  - 首页内容更新
-- FeedbackReplied  - 反馈已回复
