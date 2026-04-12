-- ==============================================================================
-- 修复 WarehouseReceipts 表的 Status CHECK 约束
-- Fix WarehouseReceipts Status CHECK Constraint
--
-- 问题描述:
--   原建表脚本中 CK_WarehouseReceipts_Status 约束只允许 '已入库' 和 '已取消'，
--   但业务逻辑使用 '待入库' 作为入库单创建后的初始状态（等待细分和入库处理）。
--   该约束导致入库单无法创建，进而导致基地人员消息中心无法收到消息。
--
-- 修复内容:
--   1. 删除原有的 CK_WarehouseReceipts_Status 约束
--   2. 重新创建约束，将 '待入库' 加入允许的状态值
--   3. 修改 Status 列的默认值为 '待入库'
-- ==============================================================================

USE RecyclingDB;
GO

-- 步骤 1: 删除原有 CHECK 约束（如果存在）
IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_WarehouseReceipts_Status'
      AND parent_object_id = OBJECT_ID('dbo.WarehouseReceipts')
)
BEGIN
    ALTER TABLE [dbo].[WarehouseReceipts] DROP CONSTRAINT [CK_WarehouseReceipts_Status];
    PRINT '已删除原有 CK_WarehouseReceipts_Status 约束';
END
ELSE
BEGIN
    PRINT 'CK_WarehouseReceipts_Status 约束不存在，跳过删除';
END
GO

-- 步骤 2: 重新创建 CHECK 约束，包含 '待入库' 状态
ALTER TABLE [dbo].[WarehouseReceipts]
ADD CONSTRAINT [CK_WarehouseReceipts_Status]
    CHECK ([Status] IN (N'待入库', N'已入库', N'已取消'));
PRINT '已重新创建 CK_WarehouseReceipts_Status 约束（包含 待入库、已入库、已取消）';
GO

-- 步骤 3: 删除并重建 Status 列的默认值约束（改为 '待入库'）
-- 先查找默认约束名称
DECLARE @defaultConstraintName NVARCHAR(256);
SELECT @defaultConstraintName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns col ON dc.parent_object_id = col.object_id AND dc.parent_column_id = col.column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.WarehouseReceipts')
  AND col.name = 'Status';

IF @defaultConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[WarehouseReceipts] DROP CONSTRAINT [' + @defaultConstraintName + ']');
    PRINT '已删除 Status 列的原有默认值约束: ' + @defaultConstraintName;
END

ALTER TABLE [dbo].[WarehouseReceipts]
ADD CONSTRAINT [DF_WarehouseReceipts_Status] DEFAULT N'待入库' FOR [Status];
PRINT '已将 Status 列的默认值设置为 ''待入库''';
GO

PRINT '============================================================';
PRINT 'WarehouseReceipts Status 约束修复完成';
PRINT '现支持状态值: 待入库（初始）、已入库（入库完成）、已取消';
PRINT '============================================================';
GO
