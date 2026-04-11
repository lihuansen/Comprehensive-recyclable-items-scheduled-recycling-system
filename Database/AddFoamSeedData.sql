-- 添加泡沫品类回收物品初始数据
-- 用途：确保首页回收品类搜索下拉框中包含"泡沫"选项
-- 执行时机：在 CreateAllTables.sql 执行之后运行

DECLARE @MaxSortOrder INT;
SELECT @MaxSortOrder = ISNULL(MAX(SortOrder), 0) FROM RecyclableItems;

IF NOT EXISTS (SELECT 1 FROM RecyclableItems WHERE Category = 'foam' AND IsActive = 1)
BEGIN
    INSERT INTO RecyclableItems (Name, Category, CategoryName, PricePerKg, Description, SortOrder, IsActive)
    VALUES
        (N'EPS泡沫',    'foam', N'泡沫', 0.30, N'白色发泡泡沫，需压缩处理，整洁无污染', @MaxSortOrder + 1, 1),
        (N'XPS挤塑板',  'foam', N'泡沫', 0.25, N'挤塑聚苯乙烯泡沫板，常见于建筑保温材料', @MaxSortOrder + 2, 1),
        (N'聚氨酯泡沫', 'foam', N'泡沫', 0.20, N'聚氨酯发泡材料，需清洁无杂质',          @MaxSortOrder + 3, 1);

    PRINT '泡沫品类初始数据插入成功（共3条）';
END
ELSE
BEGIN
    PRINT '泡沫品类数据已存在，跳过插入';
END
