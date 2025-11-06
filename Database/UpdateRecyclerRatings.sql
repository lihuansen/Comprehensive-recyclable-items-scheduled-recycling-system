-- 更新所有回收员的评分
-- 此脚本用于初始化或重新计算所有回收员的平均评分

-- 更新所有回收员的评分为其平均星级
UPDATE Recyclers 
SET Rating = (
    SELECT ISNULL(AVG(CAST(StarRating AS DECIMAL(10,2))), 0)
    FROM OrderReviews
    WHERE OrderReviews.RecyclerID = Recyclers.RecyclerID
)

-- 验证更新结果
SELECT 
    r.RecyclerID,
    r.Username,
    r.Rating AS CurrentRating,
    (SELECT COUNT(*) FROM OrderReviews WHERE RecyclerID = r.RecyclerID) AS ReviewCount,
    (SELECT AVG(CAST(StarRating AS DECIMAL(10,2))) FROM OrderReviews WHERE RecyclerID = r.RecyclerID) AS CalculatedAvgRating
FROM Recyclers r
ORDER BY r.RecyclerID

PRINT '所有回收员的评分已更新完成'
GO
