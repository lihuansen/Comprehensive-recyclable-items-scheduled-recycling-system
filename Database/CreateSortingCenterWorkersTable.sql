-- ==============================================================================
-- 基地工作人员表（SortingCenterWorkers）建表脚本
-- SortingCenterWorkers Table Creation Script
-- 
-- 用途: 存储基地工作人员信息，负责对运输到基地的回收物品进行分类、
--       质检和处理
-- Purpose: Store base worker information for classifying, 
--          quality checking and processing recyclable items
--
-- 作者: System
-- 创建日期: 2025-12-05
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- SortingCenterWorkers 表（基地工作人员表）
-- 实体类: recycling.Model.SortingCenterWorkers
-- 用途: 存储基地工作人员的基本信息和工作状态
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SortingCenterWorkers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SortingCenterWorkers] (
        [WorkerID] INT PRIMARY KEY IDENTITY(1,1),        -- 工作人员ID（自增主键）
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希（SHA256）
        [FullName] NVARCHAR(100) NULL,                   -- 真实姓名
        [PhoneNumber] NVARCHAR(20) NOT NULL,             -- 手机号
        [IDNumber] NVARCHAR(18) NULL,                    -- 身份证号
        [SortingCenterID] INT NOT NULL,                  -- 所属基地ID
        [SortingCenterName] NVARCHAR(100) NOT NULL,      -- 基地名称
        [Position] NVARCHAR(50) NOT NULL DEFAULT N'分拣员', -- 职位（分拣员、质检员、组长、主管）
        [WorkStation] NVARCHAR(50) NULL,                 -- 工位编号
        [Specialization] NVARCHAR(100) NULL,             -- 专长品类（如：塑料、金属、纸类等）
        [ShiftType] NVARCHAR(20) NOT NULL DEFAULT N'白班', -- 班次类型（白班、夜班、轮班）
        [Available] BIT NOT NULL DEFAULT 1,              -- 是否可工作（1=可用，0=忙碌）
        [CurrentStatus] NVARCHAR(20) NOT NULL DEFAULT N'待命', -- 当前状态（待命、分拣中、休息、离岗）
        [TotalItemsProcessed] INT NOT NULL DEFAULT 0,    -- 总处理物品数量
        [TotalWeightProcessed] DECIMAL(12, 2) NOT NULL DEFAULT 0, -- 总处理重量（kg）
        [AccuracyRate] DECIMAL(5, 2) NULL,               -- 分拣准确率（百分比）
        [Rating] DECIMAL(3, 2) NULL,                     -- 评分（0-5）
        [HireDate] DATE NULL,                            -- 入职日期
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [IsActive] BIT NOT NULL DEFAULT 1,               -- 是否激活（1=激活，0=禁用）
        [AvatarURL] NVARCHAR(255) NULL,                  -- 头像URL
        [Notes] NVARCHAR(500) NULL,                      -- 备注信息
        
        -- 约束
        CONSTRAINT CK_SortingCenterWorkers_Rating CHECK ([Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 5)),
        CONSTRAINT CK_SortingCenterWorkers_AccuracyRate CHECK ([AccuracyRate] IS NULL OR ([AccuracyRate] >= 0 AND [AccuracyRate] <= 100)),
        CONSTRAINT CK_SortingCenterWorkers_CurrentStatus CHECK ([CurrentStatus] IN (N'待命', N'分拣中', N'休息', N'离岗', N'离线')),
        CONSTRAINT CK_SortingCenterWorkers_Position CHECK ([Position] IN (N'分拣员', N'质检员', N'组长', N'主管', N'其他')),
        CONSTRAINT CK_SortingCenterWorkers_ShiftType CHECK ([ShiftType] IN (N'白班', N'夜班', N'轮班'))
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_SortingCenterWorkers_Username ON [dbo].[SortingCenterWorkers]([Username]);
    CREATE INDEX IX_SortingCenterWorkers_PhoneNumber ON [dbo].[SortingCenterWorkers]([PhoneNumber]);
    CREATE INDEX IX_SortingCenterWorkers_SortingCenterID ON [dbo].[SortingCenterWorkers]([SortingCenterID]);
    CREATE INDEX IX_SortingCenterWorkers_Position ON [dbo].[SortingCenterWorkers]([Position]);
    CREATE INDEX IX_SortingCenterWorkers_Available ON [dbo].[SortingCenterWorkers]([Available]);
    CREATE INDEX IX_SortingCenterWorkers_CurrentStatus ON [dbo].[SortingCenterWorkers]([CurrentStatus]);
    CREATE INDEX IX_SortingCenterWorkers_IsActive ON [dbo].[SortingCenterWorkers]([IsActive]);
    CREATE INDEX IX_SortingCenterWorkers_ShiftType ON [dbo].[SortingCenterWorkers]([ShiftType]);

    PRINT 'SortingCenterWorkers 表创建成功';
END
ELSE
BEGIN
    PRINT 'SortingCenterWorkers 表已存在';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- WorkerID              : 工作人员唯一标识（主键，自增）
-- Username              : 登录用户名，必须唯一
-- PasswordHash          : SHA256加密的密码哈希值
-- FullName              : 工作人员真实姓名
-- PhoneNumber           : 联系电话
-- IDNumber              : 身份证号码（用于实名认证）
-- SortingCenterID       : 所属基地的ID
-- SortingCenterName     : 所属基地的名称
-- Position              : 工作职位
-- WorkStation           : 工作站/工位编号
-- Specialization        : 专长的分拣品类
-- ShiftType             : 工作班次
-- Available             : 是否可接受新任务
-- CurrentStatus         : 当前工作状态
-- TotalItemsProcessed   : 累计处理的物品数量
-- TotalWeightProcessed  : 累计处理的总重量
-- AccuracyRate          : 分拣准确率
-- Rating                : 工作评分（1-5分）
-- HireDate              : 入职日期
-- CreatedDate           : 账号创建时间
-- LastLoginDate         : 最后一次登录时间
-- IsActive              : 账号是否处于激活状态
-- AvatarURL             : 用户头像图片地址
-- Notes                 : 其他备注信息
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. 只有 Available = 1 且 IsActive = 1 的工作人员可以接收新分拣任务
-- 2. CurrentStatus 表示当前实时状态：
--    - 待命：准备就绪，可以接受任务
--    - 分拣中：正在执行分拣任务
--    - 休息：休息时间
--    - 离岗：暂时离开工位
--    - 离线：不在线
-- 3. Position 职位层级：
--    - 分拣员：基础分拣工作
--    - 质检员：负责质量检查
--    - 组长：负责小组管理
--    - 主管：负责整体管理
-- 4. AccuracyRate 用于评估分拣质量
-- 5. ShiftType 用于排班管理
-- ==============================================================================

-- ==============================================================================
-- 示例数据 / Sample Data
-- ==============================================================================
-- INSERT INTO [dbo].[SortingCenterWorkers] 
--     (Username, PasswordHash, FullName, PhoneNumber, SortingCenterID, SortingCenterName,
--      Position, WorkStation, Specialization, ShiftType, Available, CurrentStatus, IsActive)
-- VALUES 
--     (N'worker001', 
--      '5e884898da28047d9166910d1887e12ce02eb84a8fc77f9a66d8b5e33917d8d6', -- password
--      N'李四', N'13800138002', 1, N'罗湖基地',
--      N'分拣员', N'A-01', N'塑料,金属', N'白班', 1, N'待命', 1);
-- ==============================================================================
