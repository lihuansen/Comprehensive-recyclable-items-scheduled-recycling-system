-- 创建用户地址表
-- Create UserAddresses Table for Address Management Feature

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserAddresses](
        [AddressID] INT IDENTITY(1,1) NOT NULL,
        [UserID] INT NOT NULL,
        [Province] NVARCHAR(50) NOT NULL DEFAULT N'广东省',
        [City] NVARCHAR(50) NOT NULL DEFAULT N'深圳市',
        [District] NVARCHAR(50) NOT NULL DEFAULT N'罗湖区',
        [Street] NVARCHAR(50) NOT NULL,
        [DetailAddress] NVARCHAR(200) NOT NULL,
        [ContactName] NVARCHAR(50) NOT NULL,
        [ContactPhone] NVARCHAR(20) NOT NULL,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME2 NULL,
        
        CONSTRAINT [PK_UserAddresses] PRIMARY KEY CLUSTERED ([AddressID] ASC),
        CONSTRAINT [FK_UserAddresses_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users] ([UserID]) ON DELETE CASCADE
    );

    -- 创建索引以提高查询性能
    CREATE INDEX [IX_UserAddresses_UserID] ON [dbo].[UserAddresses]([UserID]);
    CREATE INDEX [IX_UserAddresses_IsDefault] ON [dbo].[UserAddresses]([IsDefault]);

    PRINT 'UserAddresses table created successfully.';
END
ELSE
BEGIN
    PRINT 'UserAddresses table already exists.';
END
GO

-- 添加注释
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'用户地址管理表 - 存储用户的收货地址信息', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'地址ID（主键）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'AddressID';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'用户ID（外键关联Users表）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'UserID';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'省份（默认：广东省）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'Province';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'城市（默认：深圳市）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'City';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'区域（默认：罗湖区）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'District';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'街道（罗湖区10个街道之一）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'Street';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'详细地址（用户填写的具体地址）', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'DetailAddress';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'联系人姓名', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'ContactName';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'联系电话', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'ContactPhone';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'是否为默认地址（1=是，0=否），每个用户只能有一个默认地址', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'IsDefault';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'创建时间', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'CreatedDate';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'更新时间', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'UserAddresses',
    @level2type = N'COLUMN', @level2name = N'UpdatedDate';
GO
