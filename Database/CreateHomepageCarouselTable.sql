-- Create HomepageCarousel table for managing homepage carousel content
-- This table stores both images and videos for the homepage carousel

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HomepageCarousel]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HomepageCarousel] (
        [CarouselID] INT PRIMARY KEY IDENTITY(1,1),
        [MediaType] NVARCHAR(20) NOT NULL,           -- 'Image' or 'Video'
        [MediaUrl] NVARCHAR(500) NOT NULL,           -- URL to the media file
        [Title] NVARCHAR(200) NULL,                  -- Optional title
        [Description] NVARCHAR(500) NULL,            -- Optional description
        [DisplayOrder] INT NOT NULL DEFAULT 0,       -- Order of display (lower numbers first)
        [IsActive] BIT NOT NULL DEFAULT 1,           -- Is this carousel item active?
        [CreatedDate] DATETIME2 NOT NULL,            -- When was this created
        [CreatedBy] INT NOT NULL,                    -- Which admin created this (AdminID or SuperAdminID)
        [UpdatedDate] DATETIME2 NULL                 -- Last update timestamp
    );

    -- Create index for faster retrieval of active items
    CREATE INDEX IX_HomepageCarousel_IsActive_DisplayOrder 
        ON HomepageCarousel(IsActive, DisplayOrder);

    PRINT 'HomepageCarousel table created successfully.';
END
ELSE
BEGIN
    PRINT 'HomepageCarousel table already exists.';
END
GO

-- Insert sample data for testing (optional)
-- You can comment this out if you don't want sample data

IF NOT EXISTS (SELECT * FROM HomepageCarousel)
BEGIN
    INSERT INTO HomepageCarousel (MediaType, MediaUrl, Title, Description, DisplayOrder, IsActive, CreatedDate, CreatedBy)
    VALUES 
        ('Image', '/Content/images/carousel/sample1.jpg', '环保回收，从我做起', '让我们一起为地球的未来努力', 1, 1, GETDATE(), 1),
        ('Image', '/Content/images/carousel/sample2.jpg', '分类回收，变废为宝', '正确的分类可以让回收更高效', 2, 1, GETDATE(), 1),
        ('Image', '/Content/images/carousel/sample3.jpg', '便捷预约，上门服务', '一键预约，专业回收员上门服务', 3, 1, GETDATE(), 1);

    PRINT 'Sample carousel data inserted.';
END
GO
