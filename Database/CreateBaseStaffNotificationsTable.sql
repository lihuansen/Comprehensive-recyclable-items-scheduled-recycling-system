-- 创建基地工作人员通知表
-- Create BaseStaffNotifications table for storing base staff notification messages

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BaseStaffNotifications' AND xtype='U')
BEGIN
    CREATE TABLE BaseStaffNotifications (
        NotificationID INT PRIMARY KEY IDENTITY(1,1),   -- 通知ID（自增主键）
        WorkerID INT NOT NULL,                           -- 工作人员ID（外键）
        NotificationType NVARCHAR(50) NULL,              -- 通知类型
        Title NVARCHAR(200) NULL,                        -- 通知标题
        Content NVARCHAR(1000) NULL,                     -- 通知内容
        RelatedTransportOrderID INT NULL,                -- 关联运输单ID
        RelatedWarehouseReceiptID INT NULL,              -- 关联入库单ID
        CreatedDate DATETIME2 NOT NULL,                  -- 创建时间
        IsRead BIT NOT NULL DEFAULT 0,                   -- 是否已读
        ReadDate DATETIME2 NULL,                         -- 已读时间
        
        CONSTRAINT FK_BaseStaffNotifications_Workers FOREIGN KEY (WorkerID) 
            REFERENCES SortingCenterWorkers(WorkerID) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX IX_BaseStaffNotifications_WorkerID ON BaseStaffNotifications(WorkerID);
    CREATE INDEX IX_BaseStaffNotifications_CreatedDate ON BaseStaffNotifications(CreatedDate);
    CREATE INDEX IX_BaseStaffNotifications_IsRead ON BaseStaffNotifications(IsRead);
    CREATE INDEX IX_BaseStaffNotifications_NotificationType ON BaseStaffNotifications(NotificationType);

    PRINT 'BaseStaffNotifications table created successfully.';
END
ELSE
BEGIN
    PRINT 'BaseStaffNotifications table already exists.';
END
GO

-- 通知类型说明：
-- TransportOrderCreated      - 运输单已创建（回收员联系运输人员后）
-- TransportOrderCompleted    - 运输单已完成
-- WarehouseReceiptCreated    - 入库单已创建
-- WarehouseInventoryWritten  - 仓库库存已写入
