-- 为回收员表添加URL（头像）列
-- 其他工作人员表（Admins, SuperAdmins, Transporters, SortingCenterWorkers）已有URL列
-- Add URL (avatar) column to Recyclers table
-- Other staff tables (Admins, SuperAdmins, Transporters, SortingCenterWorkers) already have URL column

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Recyclers' AND COLUMN_NAME = 'URL'
)
BEGIN
    ALTER TABLE Recyclers ADD URL NVARCHAR(500) NULL;
    PRINT 'URL column added to Recyclers table successfully.';
END
ELSE
BEGIN
    PRINT 'URL column already exists in Recyclers table.';
END
