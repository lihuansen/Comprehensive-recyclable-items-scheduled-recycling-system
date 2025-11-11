-- 更新用户反馈表状态约束
-- 将状态从4个简化为2个：反馈中、已完成

-- 首先检查表是否存在
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFeedback]') AND type in (N'U'))
BEGIN
    -- 更新现有数据：将旧状态映射到新状态
    UPDATE [dbo].[UserFeedback]
    SET [Status] = CASE 
        WHEN [Status] IN (N'待处理', N'处理中') THEN N'反馈中'
        WHEN [Status] IN (N'已完成', N'已关闭') THEN N'已完成'
        ELSE N'反馈中'
    END
    WHERE [Status] NOT IN (N'反馈中', N'已完成');
    
    PRINT '状态数据更新完成';
    
    -- 删除旧的CHECK约束
    IF EXISTS (SELECT * FROM sys.check_constraints 
               WHERE name = 'CK_UserFeedback_Status' AND parent_object_id = OBJECT_ID(N'[dbo].[UserFeedback]'))
    BEGIN
        ALTER TABLE [dbo].[UserFeedback] DROP CONSTRAINT CK_UserFeedback_Status;
        PRINT '旧的状态约束已删除';
    END
    
    -- 添加新的CHECK约束
    ALTER TABLE [dbo].[UserFeedback]
    ADD CONSTRAINT CK_UserFeedback_Status 
        CHECK ([Status] IN (N'反馈中', N'已完成'));
    
    PRINT '新的状态约束已添加';
    
    -- 更新默认值约束
    IF EXISTS (SELECT * FROM sys.default_constraints 
               WHERE name = 'DF_UserFeedback_Status' AND parent_object_id = OBJECT_ID(N'[dbo].[UserFeedback]'))
    BEGIN
        ALTER TABLE [dbo].[UserFeedback] DROP CONSTRAINT DF_UserFeedback_Status;
    END
    
    ALTER TABLE [dbo].[UserFeedback]
    ADD CONSTRAINT DF_UserFeedback_Status DEFAULT N'反馈中' FOR [Status];
    
    PRINT '默认状态约束已更新为"反馈中"';
    
    PRINT '用户反馈表状态约束更新成功';
END
ELSE
BEGIN
    PRINT 'UserFeedback 表不存在，请先创建该表';
END
GO
