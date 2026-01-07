-- ==============================================================================
-- 全品类可回收物预约回收系统 - 完整数据库建表脚本
-- Comprehensive Recyclable Items Scheduled Recycling System - Complete Database Schema
-- 
-- 使用说明:
-- 1. 在 SQL Server Management Studio (SSMS) 中打开此脚本
-- 2. 首先创建数据库（如果不存在）
-- 3. 连接到您的数据库 (RecyclingDB)
-- 4. 执行此脚本 (按 F5 或点击"执行"按钮)
-- 
-- 注意: 请按顺序执行，因为表之间存在外键依赖关系
-- ==============================================================================

-- 创建数据库（如果不存在）
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RecyclingSystemDB')
BEGIN
    CREATE DATABASE RecyclingSystemDB;
    PRINT '数据库 RecyclingSystemDB 创建成功';
END
ELSE
BEGIN
    PRINT '数据库 RecyclingSystemDB 已存在';
END
GO

USE RecyclingSystemDB;
GO

-- ==============================================================================
-- 1. Users 表（用户表）
-- 实体类: recycling.Model.Users
-- 用途: 存储普通用户信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [UserID] INT PRIMARY KEY IDENTITY(1,1),          -- 用户ID（自增主键）
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希（SHA256）
        [PhoneNumber] NVARCHAR(20) NOT NULL UNIQUE,      -- 手机号（唯一）
        [Email] NVARCHAR(100) NOT NULL UNIQUE,           -- 邮箱（唯一）
        [RegistrationDate] DATETIME2 NULL,               -- 注册时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [url] NVARCHAR(50) NULL,                         -- 头像URL
        [money] DECIMAL(18,2) NULL DEFAULT 0.00          -- 钱包余额
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_Users_Username ON [dbo].[Users]([Username]);
    CREATE UNIQUE INDEX IX_Users_PhoneNumber ON [dbo].[Users]([PhoneNumber]);
    CREATE UNIQUE INDEX IX_Users_Email ON [dbo].[Users]([Email]);

    PRINT 'Users 表创建成功';
END
ELSE
BEGIN
    PRINT 'Users 表已存在';
END
GO

-- ==============================================================================
-- 2. Recyclers 表（回收员表）
-- 实体类: recycling.Model.Recyclers
-- 用途: 存储回收员信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Recyclers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Recyclers] (
        [RecyclerID] INT PRIMARY KEY IDENTITY(1,1),      -- 回收员ID（自增主键）
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希
        [Available] BIT NOT NULL DEFAULT 1,              -- 是否可接单
        [PhoneNumber] NVARCHAR(20) NOT NULL,             -- 手机号
        [FullName] NVARCHAR(100) NULL,                   -- 真实姓名
        [Region] NVARCHAR(100) NOT NULL,                 -- 负责区域
        [Rating] DECIMAL(3, 2) NULL,                     -- 评分（0-5）
        [CreatedDate] DATETIME2 NULL,                    -- 创建时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [IsActive] BIT NOT NULL DEFAULT 1,               -- 是否激活
        [AvatarURL] NVARCHAR(255) NULL                   -- 头像 URL
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_Recyclers_Username ON [dbo].[Recyclers]([Username]);
    CREATE INDEX IX_Recyclers_Region ON [dbo].[Recyclers]([Region]);
    CREATE INDEX IX_Recyclers_Available ON [dbo].[Recyclers]([Available]);
    CREATE INDEX IX_Recyclers_IsActive ON [dbo].[Recyclers]([IsActive]);

    PRINT 'Recyclers 表创建成功';
END
ELSE
BEGIN
    PRINT 'Recyclers 表已存在';
END
GO

-- ==============================================================================
-- 3. Admins 表（管理员表）
-- 实体类: recycling.Model.Admins
-- 用途: 存储管理员信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Admins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Admins] (
        [AdminID] INT PRIMARY KEY IDENTITY(1,1),         -- 管理员ID
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希
        [FullName] NVARCHAR(100) NOT NULL,               -- 真实姓名
        [CreatedDate] DATETIME2 NULL,                    -- 创建时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [IsActive] BIT NULL DEFAULT 1,                   -- 是否激活
        [Character] NVARCHAR(50) NULL                    -- 权限角色
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_Admins_Username ON [dbo].[Admins]([Username]);

    PRINT 'Admins 表创建成功';
END
ELSE
BEGIN
    PRINT 'Admins 表已存在';
END
GO

-- ==============================================================================
-- 4. SuperAdmins 表（超级管理员表）
-- 实体类: recycling.Model.SuperAdmins
-- 用途: 存储超级管理员信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuperAdmins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SuperAdmins] (
        [SuperAdminID] INT PRIMARY KEY IDENTITY(1,1),    -- 超级管理员ID
        [Username] NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名
        [PasswordHash] NVARCHAR(255) NOT NULL,           -- 密码哈希
        [FullName] NVARCHAR(100) NOT NULL,               -- 真实姓名
        [CreatedDate] DATETIME2 NULL,                    -- 创建时间
        [LastLoginDate] DATETIME2 NULL,                  -- 最后登录时间
        [IsActive] BIT NOT NULL DEFAULT 1                -- 是否激活
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_SuperAdmins_Username ON [dbo].[SuperAdmins]([Username]);

    PRINT 'SuperAdmins 表创建成功';
END
ELSE
BEGIN
    PRINT 'SuperAdmins 表已存在';
END
GO

-- ==============================================================================
-- 5. RecyclableItems 表（可回收物品表）
-- 实体类: recycling.Model.RecyclableItems
-- 用途: 存储可回收物品类型和价格信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecyclableItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RecyclableItems] (
        [ItemId] INT PRIMARY KEY IDENTITY(1,1),          -- 物品ID（自增主键）
        [Name] NVARCHAR(50) NOT NULL,                    -- 物品名称
        [Category] NVARCHAR(20) NOT NULL,                -- 品类代码
        [CategoryName] NVARCHAR(20) NOT NULL,            -- 品类名称
        [PricePerKg] DECIMAL(10, 2) NOT NULL,            -- 每公斤价格
        [Description] NVARCHAR(200) NULL,                -- 描述
        [SortOrder] INT NOT NULL DEFAULT 0,              -- 排序顺序
        [IsActive] BIT NOT NULL DEFAULT 1                -- 是否有效
    );

    -- 创建索引
    CREATE INDEX IX_RecyclableItems_Category ON [dbo].[RecyclableItems]([Category]);
    CREATE INDEX IX_RecyclableItems_IsActive ON [dbo].[RecyclableItems]([IsActive]);
    CREATE INDEX IX_RecyclableItems_SortOrder ON [dbo].[RecyclableItems]([SortOrder]);

    PRINT 'RecyclableItems 表创建成功';
END
ELSE
BEGIN
    PRINT 'RecyclableItems 表已存在';
END
GO

-- ==============================================================================
-- 6. Appointments 表（预约订单表）
-- 实体类: recycling.Model.Appointments
-- 用途: 存储用户的回收预约信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Appointments] (
        [AppointmentID] INT PRIMARY KEY IDENTITY(1,1),   -- 预约ID（自增主键）
        [UserID] INT NOT NULL,                           -- 用户ID（外键）
        [AppointmentType] NVARCHAR(50) NOT NULL,         -- 预约类型
        [AppointmentDate] DATE NOT NULL,                 -- 预约日期
        [TimeSlot] NVARCHAR(50) NOT NULL,                -- 时间段
        [EstimatedWeight] DECIMAL(10, 2) NOT NULL,       -- 预估重量（kg）
        [IsUrgent] BIT NOT NULL DEFAULT 0,               -- 是否紧急
        [Address] NVARCHAR(200) NOT NULL,                -- 回收地址
        [ContactName] NVARCHAR(50) NOT NULL,             -- 联系人
        [ContactPhone] NVARCHAR(20) NOT NULL,            -- 联系电话
        [SpecialInstructions] NVARCHAR(500) NULL,        -- 特殊说明
        [EstimatedPrice] DECIMAL(10, 2) NULL,            -- 预估价格
        [Status] NVARCHAR(20) NOT NULL,                  -- 订单状态
        [CreatedDate] DATETIME2 NOT NULL,                -- 创建时间
        [UpdatedDate] DATETIME2 NOT NULL,                -- 更新时间
        [RecyclerID] INT NULL,                           -- 回收员ID（外键）
        
        CONSTRAINT FK_Appointments_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]),
        CONSTRAINT FK_Appointments_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID])
    );

    -- 创建索引
    CREATE INDEX IX_Appointments_UserID ON [dbo].[Appointments]([UserID]);
    CREATE INDEX IX_Appointments_RecyclerID ON [dbo].[Appointments]([RecyclerID]);
    CREATE INDEX IX_Appointments_Status ON [dbo].[Appointments]([Status]);
    CREATE INDEX IX_Appointments_AppointmentDate ON [dbo].[Appointments]([AppointmentDate]);
    CREATE INDEX IX_Appointments_CreatedDate ON [dbo].[Appointments]([CreatedDate]);

    PRINT 'Appointments 表创建成功';
END
ELSE
BEGIN
    PRINT 'Appointments 表已存在';
END
GO

-- ==============================================================================
-- 7. AppointmentCategories 表（预约品类表）
-- 实体类: recycling.Model.AppointmentCategories
-- 用途: 存储每个预约订单的品类详细信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AppointmentCategories] (
        [CategoryID] INT PRIMARY KEY IDENTITY(1,1),      -- 品类ID（自增主键）
        [AppointmentID] INT NOT NULL,                    -- 预约ID（外键）
        [CategoryName] NVARCHAR(50) NOT NULL,            -- 品类名称
        [CategoryKey] NVARCHAR(50) NOT NULL,             -- 品类键名
        [QuestionsAnswers] NVARCHAR(MAX) NULL,           -- JSON格式的问答数据
        [Weight] DECIMAL(10, 2) NOT NULL DEFAULT 0,      -- 品类重量
        [CreatedDate] DATETIME2 NOT NULL,                -- 创建时间
        
        CONSTRAINT FK_AppointmentCategories_Appointments FOREIGN KEY ([AppointmentID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_AppointmentCategories_AppointmentID ON [dbo].[AppointmentCategories]([AppointmentID]);

    PRINT 'AppointmentCategories 表创建成功';
END
ELSE
BEGIN
    PRINT 'AppointmentCategories 表已存在';
END
GO

-- ==============================================================================
-- 8. Messages 表（消息表）
-- 实体类: recycling.Model.Messages
-- 用途: 存储用户和回收员之间的消息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Messages] (
        [MessageID] INT PRIMARY KEY IDENTITY(1,1),       -- 消息ID（自增主键）
        [OrderID] INT NULL,                              -- 关联的订单ID
        [SenderType] NVARCHAR(20) NULL,                  -- 发送者类型
        [SenderID] INT NULL,                             -- 发送者ID
        [Content] NVARCHAR(1000) NULL,                   -- 消息内容
        [SentTime] DATETIME2 NULL,                       -- 发送时间
        [IsRead] BIT NULL DEFAULT 0,                     -- 是否已读
        
        CONSTRAINT FK_Messages_Appointments FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID])
    );

    -- 创建索引
    CREATE INDEX IX_Messages_OrderID ON [dbo].[Messages]([OrderID]);
    CREATE INDEX IX_Messages_SenderType_SenderID ON [dbo].[Messages]([SenderType], [SenderID]);
    CREATE INDEX IX_Messages_SentTime ON [dbo].[Messages]([SentTime]);
    CREATE INDEX IX_Messages_IsRead ON [dbo].[Messages]([IsRead]);

    PRINT 'Messages 表创建成功';
END
ELSE
BEGIN
    PRINT 'Messages 表已存在';
END
GO

-- ==============================================================================
-- 9. Conversations 表（会话表）
-- 实体类: recycling.Model.Conversations
-- 用途: 存储订单的会话状态
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Conversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Conversations] (
        [ConversationID] INT PRIMARY KEY IDENTITY(1,1),  -- 会话ID（自增主键）
        [OrderID] INT NULL,                              -- 关联的订单ID
        [UserID] INT NULL,                               -- 用户ID
        [RecyclerID] INT NULL,                           -- 回收员ID
        [Status] NVARCHAR(20) NULL,                      -- 会话状态
        [CreatedTime] DATETIME2 NULL,                    -- 创建时间
        [EndedTime] DATETIME2 NULL,                      -- 结束时间
        [UserEnded] BIT NULL,                            -- 用户是否结束
        [RecyclerEnded] BIT NULL,                        -- 回收员是否结束
        [UserEndedTime] DATETIME2 NULL,                  -- 用户结束时间
        [RecyclerEndedTime] DATETIME2 NULL,              -- 回收员结束时间
        
        CONSTRAINT FK_Conversations_Appointments FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT FK_Conversations_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]),
        CONSTRAINT FK_Conversations_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID])
    );

    -- 创建索引
    CREATE INDEX IX_Conversations_OrderID ON [dbo].[Conversations]([OrderID]);
    CREATE INDEX IX_Conversations_UserID ON [dbo].[Conversations]([UserID]);
    CREATE INDEX IX_Conversations_RecyclerID ON [dbo].[Conversations]([RecyclerID]);

    PRINT 'Conversations 表创建成功';
END
ELSE
BEGIN
    PRINT 'Conversations 表已存在';
END
GO

-- ==============================================================================
-- 10. HomepageCarousel 表（首页轮播图表）
-- 实体类: recycling.Model.HomepageCarousel
-- 用途: 存储首页轮播图和视频内容
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HomepageCarousel]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HomepageCarousel] (
        [CarouselID] INT PRIMARY KEY IDENTITY(1,1),      -- 轮播ID
        [MediaType] NVARCHAR(20) NOT NULL,               -- 媒体类型 ('Image' 或 'Video')
        [MediaUrl] NVARCHAR(500) NOT NULL,               -- 媒体文件URL
        [Title] NVARCHAR(200) NULL,                      -- 标题
        [Description] NVARCHAR(500) NULL,                -- 描述
        [DisplayOrder] INT NOT NULL DEFAULT 0,           -- 显示顺序
        [IsActive] BIT NOT NULL DEFAULT 1,               -- 是否激活
        [CreatedDate] DATETIME2 NOT NULL,                -- 创建时间
        [CreatedBy] INT NOT NULL,                        -- 创建者ID
        [UpdatedDate] DATETIME2 NULL                     -- 更新时间
    );

    -- 创建索引
    CREATE INDEX IX_HomepageCarousel_IsActive_DisplayOrder ON [dbo].[HomepageCarousel]([IsActive], [DisplayOrder]);

    PRINT 'HomepageCarousel 表创建成功';
END
ELSE
BEGIN
    PRINT 'HomepageCarousel 表已存在';
END
GO

-- ==============================================================================
-- 11. Inventory 表（库存表）
-- 实体类: recycling.Model.Inventory
-- 用途: 存储回收员的库存管理信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Inventory] (
        [InventoryID] INT PRIMARY KEY IDENTITY(1,1),     -- 库存ID（自增主键）
        [OrderID] INT NOT NULL,                          -- 订单ID（外键）
        [CategoryKey] NVARCHAR(50) NOT NULL,             -- 品类键名
        [CategoryName] NVARCHAR(50) NOT NULL,            -- 品类名称
        [Weight] DECIMAL(10, 2) NOT NULL,                -- 重量（kg）
        [Price] DECIMAL(10, 2) NULL,                     -- 价格
        [RecyclerID] INT NOT NULL,                       -- 回收员ID（外键）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        
        CONSTRAINT FK_Inventory_Appointments FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT FK_Inventory_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT CK_Inventory_Weight CHECK ([Weight] > 0),
        CONSTRAINT CK_Inventory_Price CHECK ([Price] IS NULL OR [Price] >= 0)
    );

    -- 创建索引
    CREATE INDEX IX_Inventory_OrderID ON [dbo].[Inventory]([OrderID]);
    CREATE INDEX IX_Inventory_RecyclerID ON [dbo].[Inventory]([RecyclerID]);
    CREATE INDEX IX_Inventory_CategoryKey ON [dbo].[Inventory]([CategoryKey]);
    CREATE INDEX IX_Inventory_CreatedDate ON [dbo].[Inventory]([CreatedDate]);

    PRINT 'Inventory 表创建成功';
END
ELSE
BEGIN
    PRINT 'Inventory 表已存在';
END
GO

-- ==============================================================================
-- 12. OrderReviews 表（订单评价表）
-- 实体类: recycling.Model.OrderReviews
-- 用途: 存储用户对回收员的评价信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderReviews] (
        [ReviewID] INT PRIMARY KEY IDENTITY(1,1),        -- 评价ID（自增主键）
        [OrderID] INT NOT NULL,                          -- 订单ID（外键）
        [UserID] INT NOT NULL,                           -- 用户ID（外键）
        [RecyclerID] INT NOT NULL,                       -- 回收员ID（外键）
        [StarRating] INT NOT NULL,                       -- 星级评分（1-5）
        [ReviewText] NVARCHAR(500) NULL,                 -- 评价文字
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        
        CONSTRAINT FK_OrderReviews_Appointments FOREIGN KEY ([OrderID]) 
            REFERENCES [dbo].[Appointments]([AppointmentID]),
        CONSTRAINT FK_OrderReviews_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]),
        CONSTRAINT FK_OrderReviews_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT CK_OrderReviews_StarRating CHECK ([StarRating] >= 1 AND [StarRating] <= 5)
    );

    -- 创建索引
    CREATE INDEX IX_OrderReviews_OrderID ON [dbo].[OrderReviews]([OrderID]);
    CREATE INDEX IX_OrderReviews_UserID ON [dbo].[OrderReviews]([UserID]);
    CREATE INDEX IX_OrderReviews_RecyclerID ON [dbo].[OrderReviews]([RecyclerID]);
    CREATE INDEX IX_OrderReviews_CreatedDate ON [dbo].[OrderReviews]([CreatedDate]);

    PRINT 'OrderReviews 表创建成功';
END
ELSE
BEGIN
    PRINT 'OrderReviews 表已存在';
END
GO

-- ==============================================================================
-- 13. UserFeedback 表（用户反馈表）
-- 实体类: recycling.Model.UserFeedback
-- 用途: 存储用户提交的问题反馈、功能建议等
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFeedback]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserFeedback] (
        [FeedbackID] INT PRIMARY KEY IDENTITY(1,1),      -- 反馈ID
        [UserID] INT NOT NULL,                           -- 用户ID（外键）
        [FeedbackType] NVARCHAR(50) NOT NULL,            -- 反馈类型
        [Subject] NVARCHAR(200) NOT NULL,                -- 主题
        [Description] NVARCHAR(2000) NOT NULL,           -- 描述
        [ContactEmail] NVARCHAR(100) NULL,               -- 联系邮箱
        [Status] NVARCHAR(50) NOT NULL DEFAULT N'反馈中', -- 状态
        [AdminReply] NVARCHAR(1000) NULL,                -- 管理员回复
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        [UpdatedDate] DATETIME2 NULL,                    -- 更新时间
        
        CONSTRAINT FK_UserFeedback_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT CK_UserFeedback_FeedbackType 
            CHECK ([FeedbackType] IN (N'问题反馈', N'功能建议', N'投诉举报', N'其他')),
        CONSTRAINT CK_UserFeedback_Status 
            CHECK ([Status] IN (N'反馈中', N'已完成'))
    );

    -- 创建索引
    CREATE INDEX IX_UserFeedback_UserID ON [dbo].[UserFeedback]([UserID]);
    CREATE INDEX IX_UserFeedback_Status ON [dbo].[UserFeedback]([Status]);
    CREATE INDEX IX_UserFeedback_FeedbackType ON [dbo].[UserFeedback]([FeedbackType]);
    CREATE INDEX IX_UserFeedback_CreatedDate ON [dbo].[UserFeedback]([CreatedDate] DESC);

    PRINT 'UserFeedback 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserFeedback 表已存在';
END
GO

-- ==============================================================================
-- 14. UserNotifications 表（用户通知表）
-- 实体类: recycling.Model.UserNotifications
-- 用途: 存储用户通知消息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserNotifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserNotifications] (
        [NotificationID] INT PRIMARY KEY IDENTITY(1,1),  -- 通知ID（自增主键）
        [UserID] INT NOT NULL,                           -- 用户ID（外键）
        [NotificationType] NVARCHAR(50) NULL,            -- 通知类型
        [Title] NVARCHAR(200) NULL,                      -- 通知标题
        [Content] NVARCHAR(1000) NULL,                   -- 通知内容
        [RelatedOrderID] INT NULL,                       -- 关联订单ID
        [RelatedFeedbackID] INT NULL,                    -- 关联反馈ID
        [CreatedDate] DATETIME2 NOT NULL,                -- 创建时间
        [IsRead] BIT NOT NULL DEFAULT 0,                 -- 是否已读
        [ReadDate] DATETIME2 NULL,                       -- 已读时间
        
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_UserNotifications_UserID ON [dbo].[UserNotifications]([UserID]);
    CREATE INDEX IX_UserNotifications_CreatedDate ON [dbo].[UserNotifications]([CreatedDate]);
    CREATE INDEX IX_UserNotifications_IsRead ON [dbo].[UserNotifications]([IsRead]);
    CREATE INDEX IX_UserNotifications_NotificationType ON [dbo].[UserNotifications]([NotificationType]);

    PRINT 'UserNotifications 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserNotifications 表已存在';
END
GO

-- ==============================================================================
-- 15. UserAddresses 表（用户地址表）
-- 实体类: recycling.Model.UserAddresses
-- 用途: 存储用户的收货地址信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserAddresses] (
        [AddressID] INT PRIMARY KEY IDENTITY(1,1),       -- 地址ID（主键）
        [UserID] INT NOT NULL,                           -- 用户ID（外键关联Users表）
        [Province] NVARCHAR(50) NOT NULL DEFAULT N'广东省', -- 省份（默认：广东省）
        [City] NVARCHAR(50) NOT NULL DEFAULT N'深圳市',    -- 城市（默认：深圳市）
        [District] NVARCHAR(50) NOT NULL DEFAULT N'罗湖区', -- 区域（默认：罗湖区）
        [Street] NVARCHAR(50) NOT NULL,                  -- 街道（罗湖区10个街道之一）
        [DetailAddress] NVARCHAR(200) NOT NULL,          -- 详细地址（用户填写的具体地址）
        [ContactName] NVARCHAR(50) NOT NULL,             -- 联系人姓名
        [ContactPhone] NVARCHAR(20) NOT NULL,            -- 联系电话
        [IsDefault] BIT NOT NULL DEFAULT 0,              -- 是否为默认地址（1=是，0=否）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
        [UpdatedDate] DATETIME2 NULL,                    -- 更新时间
        
        CONSTRAINT FK_UserAddresses_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );

    -- 创建索引以提高查询性能
    CREATE INDEX IX_UserAddresses_UserID ON [dbo].[UserAddresses]([UserID]);
    CREATE INDEX IX_UserAddresses_IsDefault ON [dbo].[UserAddresses]([IsDefault]);

    PRINT 'UserAddresses 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserAddresses 表已存在';
END
GO

-- ==============================================================================
-- 16. AdminOperationLogs 表（管理员操作日志表）
-- 实体类: recycling.Model.AdminOperationLogs
-- 用途: 记录管理员端的所有操作日志
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminOperationLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminOperationLogs] (
        [LogID] INT PRIMARY KEY IDENTITY(1,1),           -- 日志ID（主键）
        [AdminID] INT NOT NULL,                          -- 管理员ID
        [AdminUsername] NVARCHAR(50) NULL,               -- 管理员用户名
        [Module] NVARCHAR(50) NOT NULL,                  -- 操作模块
        [OperationType] NVARCHAR(50) NOT NULL,           -- 操作类型
        [Description] NVARCHAR(500) NULL,                -- 操作描述
        [TargetID] INT NULL,                             -- 目标对象ID
        [TargetName] NVARCHAR(100) NULL,                 -- 目标对象名称
        [IPAddress] NVARCHAR(50) NULL,                   -- IP地址
        [OperationTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 操作时间
        [Result] NVARCHAR(20) NULL,                      -- 操作结果
        [Details] NVARCHAR(MAX) NULL                     -- 附加详情（JSON）
    );

    -- 创建索引
    CREATE INDEX IX_AdminOperationLogs_AdminID ON [dbo].[AdminOperationLogs]([AdminID]);
    CREATE INDEX IX_AdminOperationLogs_Module ON [dbo].[AdminOperationLogs]([Module]);
    CREATE INDEX IX_AdminOperationLogs_OperationType ON [dbo].[AdminOperationLogs]([OperationType]);
    CREATE INDEX IX_AdminOperationLogs_OperationTime ON [dbo].[AdminOperationLogs]([OperationTime] DESC);
    CREATE INDEX IX_AdminOperationLogs_Result ON [dbo].[AdminOperationLogs]([Result]);

    PRINT 'AdminOperationLogs 表创建成功';
END
ELSE
BEGIN
    PRINT 'AdminOperationLogs 表已存在';
END
GO

-- ==============================================================================
-- 17. UserContactRequests 表（用户联系请求表）
-- 用途: 记录用户点击"联系我们"的请求
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserContactRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserContactRequests] (
        [RequestID] INT PRIMARY KEY IDENTITY(1,1),       -- 请求ID
        [UserID] INT NOT NULL,                           -- 用户ID
        [RequestStatus] BIT NOT NULL DEFAULT 1,          -- 请求状态（1=待联系, 0=已处理）
        [RequestTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 请求时间
        [ContactedTime] DATETIME2 NULL,                  -- 处理时间
        [AdminID] INT NULL,                              -- 处理的管理员ID
        
        CONSTRAINT FK_UserContactRequests_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_UserContactRequests_UserID ON [dbo].[UserContactRequests]([UserID]);
    CREATE INDEX IX_UserContactRequests_RequestStatus ON [dbo].[UserContactRequests]([RequestStatus]);
    CREATE INDEX IX_UserContactRequests_RequestTime ON [dbo].[UserContactRequests]([RequestTime]);

    PRINT 'UserContactRequests 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserContactRequests 表已存在';
END
GO

-- ==============================================================================
-- 18. AdminContactMessages 表（管理员联系消息表）
-- 用途: 存储用户和管理员之间的聊天消息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminContactMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminContactMessages] (
        [MessageID] INT PRIMARY KEY IDENTITY(1,1),       -- 消息ID
        [UserID] INT NOT NULL,                           -- 用户ID
        [AdminID] INT NULL,                              -- 管理员ID
        [SenderType] NVARCHAR(20) NOT NULL,              -- 发送者类型
        [Content] NVARCHAR(2000) NOT NULL,               -- 消息内容
        [SentTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 发送时间
        [IsRead] BIT NOT NULL DEFAULT 0,                 -- 是否已读
        
        CONSTRAINT FK_AdminContactMessages_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT CK_AdminContactMessages_SenderType 
            CHECK ([SenderType] IN (N'user', N'admin', N'system'))
    );

    -- 创建索引
    CREATE INDEX IX_AdminContactMessages_UserID ON [dbo].[AdminContactMessages]([UserID]);
    CREATE INDEX IX_AdminContactMessages_AdminID ON [dbo].[AdminContactMessages]([AdminID]);
    CREATE INDEX IX_AdminContactMessages_SentTime ON [dbo].[AdminContactMessages]([SentTime]);

    PRINT 'AdminContactMessages 表创建成功';
END
ELSE
BEGIN
    PRINT 'AdminContactMessages 表已存在';
END
GO

-- ==============================================================================
-- 19. AdminContactConversations 表（管理员联系会话表）
-- 用途: 跟踪用户和管理员之间的会话状态
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminContactConversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminContactConversations] (
        [ConversationID] INT PRIMARY KEY IDENTITY(1,1),  -- 会话ID
        [UserID] INT NOT NULL,                           -- 用户ID
        [AdminID] INT NULL,                              -- 管理员ID
        [StartTime] DATETIME2 NOT NULL DEFAULT GETDATE(), -- 开始时间
        [UserEndedTime] DATETIME2 NULL,                  -- 用户结束时间
        [AdminEndedTime] DATETIME2 NULL,                 -- 管理员结束时间
        [UserEnded] BIT NOT NULL DEFAULT 0,              -- 用户是否结束
        [AdminEnded] BIT NOT NULL DEFAULT 0,             -- 管理员是否结束
        [LastMessageTime] DATETIME2 NULL,                -- 最后消息时间
        
        CONSTRAINT FK_AdminContactConversations_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_AdminContactConversations_UserID ON [dbo].[AdminContactConversations]([UserID]);
    CREATE INDEX IX_AdminContactConversations_AdminID ON [dbo].[AdminContactConversations]([AdminID]);

    PRINT 'AdminContactConversations 表创建成功';
END
ELSE
BEGIN
    PRINT 'AdminContactConversations 表已存在';
END
GO

-- ==============================================================================
-- 20. UserPaymentAccounts 表（用户支付账户表）
-- 实体类: recycling.Model.UserPaymentAccount
-- 用途: 存储用户绑定的支付账户（支付宝、微信、银行卡等）
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPaymentAccounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserPaymentAccounts] (
        [AccountID] INT PRIMARY KEY IDENTITY(1,1),              -- 账户ID（自增主键）
        [UserID] INT NOT NULL,                                   -- 用户ID（外键关联Users表）
        [AccountType] NVARCHAR(20) NOT NULL,                     -- 账户类型：Alipay(支付宝), WeChat(微信), BankCard(银行卡)
        [AccountName] NVARCHAR(100) NOT NULL,                    -- 账户名称/持卡人姓名
        [AccountNumber] NVARCHAR(100) NOT NULL,                  -- 账户号/卡号（加密存储）
        [BankName] NVARCHAR(100) NULL,                           -- 银行名称（仅银行卡需要）
        [IsDefault] BIT NOT NULL DEFAULT 0,                      -- 是否默认账户
        [IsVerified] BIT NOT NULL DEFAULT 0,                     -- 是否已验证
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),      -- 创建时间
        [LastUsedDate] DATETIME2 NULL,                           -- 最后使用时间
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Active',         -- 状态：Active(激活), Suspended(暂停), Deleted(已删除)
        
        -- 外键约束
        CONSTRAINT FK_UserPaymentAccounts_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        
        -- 检查约束
        CONSTRAINT CHK_AccountType CHECK ([AccountType] IN ('Alipay', 'WeChat', 'BankCard')),
        CONSTRAINT CHK_PaymentAccountStatus CHECK ([Status] IN ('Active', 'Suspended', 'Deleted'))
    );

    -- 创建索引
    CREATE INDEX IX_UserPaymentAccounts_UserID ON [dbo].[UserPaymentAccounts]([UserID]);
    CREATE INDEX IX_UserPaymentAccounts_AccountType ON [dbo].[UserPaymentAccounts]([AccountType]);
    CREATE INDEX IX_UserPaymentAccounts_IsDefault ON [dbo].[UserPaymentAccounts]([IsDefault]);
    CREATE INDEX IX_UserPaymentAccounts_Status ON [dbo].[UserPaymentAccounts]([Status]);

    PRINT 'UserPaymentAccounts 表创建成功';
END
ELSE
BEGIN
    PRINT 'UserPaymentAccounts 表已存在';
END
GO

-- ==============================================================================
-- 21. WalletTransactions 表（钱包交易记录表）
-- 用途: 存储所有钱包相关的交易记录
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WalletTransactions] (
        [TransactionID] INT PRIMARY KEY IDENTITY(1,1),          -- 交易ID（自增主键）
        [UserID] INT NOT NULL,                                   -- 用户ID（外键关联Users表）
        [TransactionType] NVARCHAR(20) NOT NULL,                 -- 交易类型：Recharge(充值), Withdraw(提现), Payment(支付), Refund(退款), Income(收入)
        [Amount] DECIMAL(18,2) NOT NULL,                         -- 交易金额
        [BalanceBefore] DECIMAL(18,2) NOT NULL,                  -- 交易前余额
        [BalanceAfter] DECIMAL(18,2) NOT NULL,                   -- 交易后余额
        [PaymentAccountID] INT NULL,                             -- 支付账户ID（外键，充值/提现时使用）
        [RelatedOrderID] INT NULL,                               -- 关联订单ID（支付/退款时使用）
        [TransactionStatus] NVARCHAR(20) NOT NULL DEFAULT 'Completed', -- 交易状态：Pending(待处理), Processing(处理中), Completed(已完成), Failed(失败), Cancelled(已取消)
        [Description] NVARCHAR(500) NULL,                        -- 交易描述
        [TransactionNo] NVARCHAR(50) NOT NULL UNIQUE,            -- 交易流水号（唯一）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),      -- 创建时间
        [CompletedDate] DATETIME2 NULL,                          -- 完成时间
        [Remarks] NVARCHAR(500) NULL,                            -- 备注
        
        -- 外键约束
        CONSTRAINT FK_WalletTransactions_Users FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users]([UserID]) ON DELETE CASCADE,
        CONSTRAINT FK_WalletTransactions_PaymentAccounts FOREIGN KEY ([PaymentAccountID]) 
            REFERENCES [dbo].[UserPaymentAccounts]([AccountID]),
        
        -- 检查约束
        CONSTRAINT CHK_TransactionType CHECK ([TransactionType] IN ('Recharge', 'Withdraw', 'Payment', 'Refund', 'Income')),
        CONSTRAINT CHK_WalletTransactionStatus CHECK ([TransactionStatus] IN ('Pending', 'Processing', 'Completed', 'Failed', 'Cancelled')),
        CONSTRAINT CHK_Amount CHECK ([Amount] >= 0)
    );

    -- 创建索引
    CREATE INDEX IX_WalletTransactions_UserID ON [dbo].[WalletTransactions]([UserID]);
    CREATE INDEX IX_WalletTransactions_TransactionType ON [dbo].[WalletTransactions]([TransactionType]);
    CREATE INDEX IX_WalletTransactions_TransactionStatus ON [dbo].[WalletTransactions]([TransactionStatus]);
    CREATE INDEX IX_WalletTransactions_CreatedDate ON [dbo].[WalletTransactions]([CreatedDate] DESC);
    CREATE UNIQUE INDEX IX_WalletTransactions_TransactionNo ON [dbo].[WalletTransactions]([TransactionNo]);

    PRINT 'WalletTransactions 表创建成功';
END
ELSE
BEGIN
    PRINT 'WalletTransactions 表已存在';
END
GO

-- ==============================================================================
-- 完成信息
-- ==============================================================================
PRINT '';
PRINT '============================================';
PRINT '所有数据库表创建脚本执行完成！';
PRINT '============================================';
PRINT '';
PRINT '表列表:';
PRINT '  1. Users - 用户表';
PRINT '  2. Recyclers - 回收员表';
PRINT '  3. Admins - 管理员表';
PRINT '  4. SuperAdmins - 超级管理员表';
PRINT '  5. RecyclableItems - 可回收物品表';
PRINT '  6. Appointments - 预约订单表';
PRINT '  7. AppointmentCategories - 预约品类表';
PRINT '  8. Messages - 消息表';
PRINT '  9. Conversations - 会话表';
PRINT ' 10. HomepageCarousel - 首页轮播图表';
PRINT ' 11. Inventory - 库存表';
PRINT ' 12. OrderReviews - 订单评价表';
PRINT ' 13. UserFeedback - 用户反馈表';
PRINT ' 14. UserNotifications - 用户通知表';
PRINT ' 15. UserAddresses - 用户地址表';
PRINT ' 16. AdminOperationLogs - 管理员操作日志表';
PRINT ' 17. UserContactRequests - 用户联系请求表';
PRINT ' 18. AdminContactMessages - 管理员联系消息表';
PRINT ' 19. AdminContactConversations - 管理员联系会话表';
PRINT ' 20. UserPaymentAccounts - 用户支付账户表';
PRINT ' 21. WalletTransactions - 钱包交易记录表';
PRINT '';
PRINT '对应的实体类位于: recycling.Model 项目';
PRINT '';
GO
