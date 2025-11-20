-- =============================================
-- 管理员权限分配脚本
-- 用于为现有管理员分配权限或更新权限
-- =============================================

-- 查看当前所有管理员及其权限
SELECT AdminID, Username, FullName, Character as Permission, IsActive
FROM Admins
ORDER BY AdminID;

-- =============================================
-- 更新管理员权限的示例
-- =============================================

-- 示例1: 设置管理员为"用户管理"权限
-- UPDATE Admins SET Character = 'user_management' WHERE AdminID = 1;

-- 示例2: 设置管理员为"回收员管理"权限
-- UPDATE Admins SET Character = 'recycler_management' WHERE AdminID = 2;

-- 示例3: 设置管理员为"反馈管理"权限
-- UPDATE Admins SET Character = 'feedback_management' WHERE AdminID = 3;

-- 示例4: 设置管理员为"首页页面管理"权限
-- UPDATE Admins SET Character = 'homepage_management' WHERE AdminID = 4;

-- 示例5: 设置管理员为"全部权限"
-- UPDATE Admins SET Character = 'full_access' WHERE AdminID = 5;

-- =============================================
-- 权限值说明
-- =============================================
-- user_management       - 用户管理权限
-- recycler_management   - 回收员管理权限
-- feedback_management   - 反馈管理权限
-- homepage_management   - 首页页面管理权限
-- full_access          - 全部权限

-- =============================================
-- 批量更新示例
-- =============================================

-- 如果需要为所有现有管理员设置默认权限（全部权限）
-- UPDATE Admins SET Character = 'full_access' WHERE Character IS NULL;

-- =============================================
-- 验证权限设置
-- =============================================
SELECT 
    AdminID, 
    Username, 
    FullName,
    CASE Character
        WHEN 'user_management' THEN '用户管理'
        WHEN 'recycler_management' THEN '回收员管理'
        WHEN 'feedback_management' THEN '反馈管理'
        WHEN 'homepage_management' THEN '首页页面管理'
        WHEN 'full_access' THEN '全部权限'
        ELSE '未设置权限'
    END AS PermissionName,
    Character as PermissionCode,
    IsActive
FROM Admins
ORDER BY AdminID;
