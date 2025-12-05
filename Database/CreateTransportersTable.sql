-- ==============================================================================
-- 运输人员表（Transporters）建表脚本
-- Transporters Table Creation Script
-- 
-- 用途: 存储运输人员信息，负责将回收物品从回收员处运输到分拣中心
-- Purpose: Store transporter information for transporting recyclable items 
--          from recyclers to sorting centers
--
-- 作者: System
-- 创建日期: 2025-12-05
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- Transporters 表（运输人员表）
-- 实体类: recycling.Model.Transporters
-- 用途: 存储运输人员的基本信息和工作状态
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transporters]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Transporters] (
        [TransporterID] INT PRIMARY KEY IDENTITY(1,1),   -- 运输人员ID（自增主键）
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希（SHA256）
        [FullName] NVARCHAR(100) NULL,                   -- 真实姓名
        [PhoneNumber] NVARCHAR(20) NOT NULL,             -- 手机号
        [IDNumber] NVARCHAR(18) NULL,                    -- 身份证号
        [VehicleType] NVARCHAR(50) NOT NULL,             -- 车辆类型（如：小型货车、中型货车、大型货车）
        [VehiclePlateNumber] NVARCHAR(20) NOT NULL,      -- 车牌号
        [VehicleCapacity] DECIMAL(10, 2) NULL,           -- 车辆载重能力（单位：kg）
        [LicenseNumber] NVARCHAR(50) NULL,               -- 驾驶证号
        [Region] NVARCHAR(100) NOT NULL,                 -- 负责区域
        [Available] BIT NOT NULL DEFAULT 1,              -- 是否可接任务（1=可用，0=忙碌）
        [CurrentStatus] NVARCHAR(20) NOT NULL DEFAULT N'空闲', -- 当前状态（空闲、运输中、休息）
        [TotalTrips] INT NOT NULL DEFAULT 0,             -- 总运输次数
        [TotalWeight] DECIMAL(12, 2) NOT NULL DEFAULT 0, -- 总运输重量（kg）
        [Rating] DECIMAL(3, 2) NULL,                     -- 评分（0-5）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [IsActive] BIT NOT NULL DEFAULT 1,               -- 是否激活（1=激活，0=禁用）
        [AvatarURL] NVARCHAR(255) NULL,                  -- 头像URL
        [Notes] NVARCHAR(500) NULL,                      -- 备注信息
        
        -- 约束
        CONSTRAINT CK_Transporters_Rating CHECK ([Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 5)),
        CONSTRAINT CK_Transporters_CurrentStatus CHECK ([CurrentStatus] IN (N'空闲', N'运输中', N'休息', N'离线'))
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_Transporters_Username ON [dbo].[Transporters]([Username]);
    CREATE INDEX IX_Transporters_PhoneNumber ON [dbo].[Transporters]([PhoneNumber]);
    CREATE INDEX IX_Transporters_Region ON [dbo].[Transporters]([Region]);
    CREATE INDEX IX_Transporters_Available ON [dbo].[Transporters]([Available]);
    CREATE INDEX IX_Transporters_CurrentStatus ON [dbo].[Transporters]([CurrentStatus]);
    CREATE INDEX IX_Transporters_IsActive ON [dbo].[Transporters]([IsActive]);
    CREATE INDEX IX_Transporters_VehiclePlateNumber ON [dbo].[Transporters]([VehiclePlateNumber]);

    PRINT 'Transporters 表创建成功';
END
ELSE
BEGIN
    PRINT 'Transporters 表已存在';
END
GO

-- ==============================================================================
-- 字段说明 / Field Description
-- ==============================================================================
-- TransporterID      : 运输人员唯一标识（主键，自增）
-- Username           : 登录用户名，必须唯一
-- PasswordHash       : SHA256加密的密码哈希值
-- FullName           : 运输人员真实姓名
-- PhoneNumber        : 联系电话
-- IDNumber           : 身份证号码（用于实名认证）
-- VehicleType        : 运输车辆类型
-- VehiclePlateNumber : 车辆牌照号码
-- VehicleCapacity    : 车辆最大载重量（公斤）
-- LicenseNumber      : 驾驶证编号
-- Region             : 负责的运输区域
-- Available          : 是否可接受新任务
-- CurrentStatus      : 当前工作状态
-- TotalTrips         : 累计完成的运输次数
-- TotalWeight        : 累计运输的总重量
-- Rating             : 服务评分（1-5分）
-- CreatedDate        : 账号创建时间
-- LastLoginDate      : 最后一次登录时间
-- IsActive           : 账号是否处于激活状态
-- AvatarURL          : 用户头像图片地址
-- Notes              : 其他备注信息
-- ==============================================================================

-- ==============================================================================
-- 业务规则 / Business Rules
-- ==============================================================================
-- 1. 只有 Available = 1 且 IsActive = 1 的运输人员可以接收新任务
-- 2. CurrentStatus 表示当前实时状态：
--    - 空闲：可以接受新任务
--    - 运输中：正在执行运输任务
--    - 休息：暂时不接单
--    - 离线：不在线
-- 3. Rating 为用户对运输服务的平均评分
-- 4. TotalTrips 和 TotalWeight 用于统计绩效
-- ==============================================================================

-- ==============================================================================
-- 示例数据 / Sample Data
-- ==============================================================================
-- INSERT INTO [dbo].[Transporters] 
--     (Username, PasswordHash, FullName, PhoneNumber, VehicleType, VehiclePlateNumber, 
--      VehicleCapacity, Region, Available, CurrentStatus, IsActive)
-- VALUES 
--     (N'transporter001', 
--      '5e884898da28047d9166910d1887e12ce02eb84a8fc77f9a66d8b5e33917d8d6', -- password
--      N'张三', N'13800138001', N'小型货车', N'粤B12345', 
--      500.00, N'罗湖区', 1, N'空闲', 1);
-- ==============================================================================
