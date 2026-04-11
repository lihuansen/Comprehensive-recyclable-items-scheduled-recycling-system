-- 添加家电品类回收物品初始数据
-- 用途：确保首页回收品类搜索下拉框中包含"家电"选项
-- 执行时机：在 CreateAllTables.sql 执行之后运行

DECLARE @MaxSortOrder INT;
SELECT @MaxSortOrder = ISNULL(MAX(SortOrder), 0) FROM RecyclableItems;

IF NOT EXISTS (SELECT 1 FROM RecyclableItems WHERE Category = 'appliance' AND IsActive = 1)
BEGIN
    INSERT INTO RecyclableItems (Name, Category, CategoryName, PricePerKg, Description, SortOrder, IsActive)
    VALUES
        (N'冰箱',   'appliance', N'家电', 1.50, N'各类型废旧冰箱，需提前断电并清空内部物品', @MaxSortOrder + 1, 1),
        (N'洗衣机', 'appliance', N'家电', 1.20, N'滚筒式或波轮式废旧洗衣机，需断电处理',     @MaxSortOrder + 2, 1),
        (N'空调',   'appliance', N'家电', 2.00, N'废旧空调主机及室内机，需提前抽空冷媒',     @MaxSortOrder + 3, 1),
        (N'电视机', 'appliance', N'家电', 0.80, N'液晶电视或传统CRT电视，整机回收',           @MaxSortOrder + 4, 1),
        (N'电脑',   'appliance', N'家电', 3.00, N'台式电脑整机或笔记本电脑，需清除个人数据',  @MaxSortOrder + 5, 1),
        (N'微波炉', 'appliance', N'家电', 1.00, N'废旧微波炉，整机回收',                       @MaxSortOrder + 6, 1);

    PRINT '家电品类初始数据插入成功（共6条）';
END
ELSE
BEGIN
    PRINT '家电品类数据已存在，跳过插入';
END
