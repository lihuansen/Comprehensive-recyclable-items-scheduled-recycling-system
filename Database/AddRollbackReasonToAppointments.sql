-- ==============================================================================
-- 添加回退原因字段到 Appointments 表
-- 用途: 存储回收员回退订单时填写的原因
-- ==============================================================================

-- 检查并添加 RollbackReason 列
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'RollbackReason')
BEGIN
    ALTER TABLE [dbo].[Appointments] ADD [RollbackReason] NVARCHAR(500) NULL;
    PRINT 'RollbackReason 列已添加到 Appointments 表';
END
ELSE
BEGIN
    PRINT 'RollbackReason 列已存在于 Appointments 表';
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
WHERE c.object_id = OBJECT_ID(N'[dbo].[Appointments]')
  AND c.name = 'RollbackReason';

PRINT '验证完成：RollbackReason 列已成功添加';
GO
