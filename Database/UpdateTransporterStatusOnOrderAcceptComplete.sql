-- ==============================================================================
-- 运输人员状态自动更新说明
-- Transporter Status Auto-Update on Order Accept/Complete
-- ==============================================================================
-- 此脚本说明运输人员接单和完成运输时的状态自动更新逻辑。
-- 无需数据库结构变更，Transporters.CurrentStatus 字段已存在。
--
-- 业务逻辑变更（在 BLL 层实现）:
-- 1. 运输人员接单时（AcceptTransportationOrder）:
--    - 运输人员 CurrentStatus 从 '空闲' 更新为 '运输中'
-- 2. 运输订单完成时（CompleteTransportation）:
--    - 检查该运输人员是否还有其他未完成的运输单
--    - 如果没有未完成的运输单，CurrentStatus 从 '运输中' 更新为 '空闲'
--
-- DAL 层新增方法:
-- 1. UpdateTransporterStatus(transporterId, status) - 更新运输人员状态
-- 2. HasActiveTransportOrdersForTransporter(transporterId) - 检查是否有未完成运输单
-- ==============================================================================

-- 验证 Transporters 表的 CurrentStatus 字段和约束
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable,
    dc.definition AS DefaultValue
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('Transporters')
AND c.name = 'CurrentStatus';

-- 验证 CurrentStatus 的检查约束
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('Transporters')
AND cc.definition LIKE '%CurrentStatus%';

-- 查看当前运输人员状态分布
SELECT CurrentStatus, COUNT(*) AS TransporterCount
FROM Transporters
GROUP BY CurrentStatus;

PRINT '运输人员状态自动更新逻辑已在 BLL/DAL 层实现，无需数据库结构变更。';
