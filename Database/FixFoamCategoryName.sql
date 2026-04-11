-- 修复 AppointmentCategories 表中品类名称英文存储问题
-- 将历史数据中英文品类键名更新为对应的中文显示名称
-- Fix English category key names stored in AppointmentCategories to their Chinese display names

UPDATE AppointmentCategories
SET CategoryName = CASE CategoryName
    WHEN 'foam'      THEN N'泡沫'
    WHEN 'glass'     THEN N'玻璃'
    WHEN 'metal'     THEN N'金属'
    WHEN 'plastic'   THEN N'塑料'
    WHEN 'paper'     THEN N'纸类'
    WHEN 'fabric'    THEN N'纺织品'
    WHEN 'appliance' THEN N'家电'
    ELSE CategoryName
END
WHERE CategoryName IN ('foam', 'glass', 'metal', 'plastic', 'paper', 'fabric', 'appliance');
