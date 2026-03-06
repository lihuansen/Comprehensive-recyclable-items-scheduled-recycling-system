-- ==============================================================================
-- 添加图片URL字段到 OrderReviews 表
-- 用途: 存储用户评价时上传的图片URL（最多6张，逗号分隔）
-- ==============================================================================

-- 检查并添加 PictureUrl 列
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND name = 'PictureUrl')
BEGIN
    ALTER TABLE [dbo].[OrderReviews] ADD [PictureUrl] NVARCHAR(2000) NULL;
    PRINT 'PictureUrl 列已添加到 OrderReviews 表';
END
ELSE
BEGIN
    PRINT 'PictureUrl 列已存在于 OrderReviews 表';
END
GO

-- 验证列添加成功
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[OrderReviews]')
  AND c.name = 'PictureUrl';

PRINT '验证完成：PictureUrl 列已成功添加';
GO
