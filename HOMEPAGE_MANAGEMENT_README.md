# 首页管理功能说明

## 功能概述

本次实现了管理员对用户首页的完整管理功能，包括：

1. **轮播内容管理** - 管理首页顶部的图片/视频轮播
2. **可回收物管理** - 管理首页展示的可回收物品信息

## 功能详情

### 1. 轮播内容管理

**访问路径**: 管理员登录后 → 首页页面管理 → 轮播管理

**功能特性**:
- ✅ 支持图片和视频混合轮播
- ✅ 新增、修改、删除轮播内容
- ✅ 自定义轮播顺序（DisplayOrder字段）
- ✅ 启用/禁用轮播项
- ✅ 超过2个内容时自动8秒轮播
- ✅ 视频支持自动播放和循环，默认8秒切换

**字段说明**:
- **媒体类型**: Image（图片）或 Video（视频）
- **媒体URL**: 图片或视频的完整URL地址
- **标题**: 可选，显示在轮播下方
- **描述**: 可选，标题下方的详细描述
- **排序顺序**: 数字越小越靠前
- **状态**: 启用或禁用

**技术实现**:
- 使用Bootstrap 3的Carousel组件
- 8秒自动切换间隔（data-interval="8000"）
- 视频标签设置为自动播放、循环、静音

### 2. 可回收物管理

**访问路径**: 管理员登录后 → 首页页面管理 → 可回收物管理

**功能特性**:
- ✅ 新增、修改、删除可回收物品
- ✅ 设置物品价格（元/kg）
- ✅ 按品类分类（玻璃、金属、塑料、纸类、纺织品）
- ✅ 自定义排序和显示状态
- ✅ 支持模糊搜索和筛选

**字段说明**:
- **物品名称**: 如"矿泉水瓶"、"废纸箱"等
- **品类**: glass（玻璃）、metal（金属）、plastic（塑料）、paper（纸类）、fabric（纺织品）
- **价格**: 每公斤回收价格，精确到小数点后两位
- **描述**: 物品详细描述（可选）
- **排序顺序**: 控制首页显示顺序
- **状态**: 启用的物品才会在用户首页显示

## 数据库表结构

### HomepageCarousel 表

```sql
CREATE TABLE [dbo].[HomepageCarousel] (
    [CarouselID] INT PRIMARY KEY IDENTITY(1,1),
    [MediaType] NVARCHAR(20) NOT NULL,           -- 'Image' or 'Video'
    [MediaUrl] NVARCHAR(500) NOT NULL,           -- URL to the media file
    [Title] NVARCHAR(200) NULL,                  -- Optional title
    [Description] NVARCHAR(500) NULL,            -- Optional description
    [DisplayOrder] INT NOT NULL DEFAULT 0,       -- Order of display
    [IsActive] BIT NOT NULL DEFAULT 1,           -- Is active?
    [CreatedDate] DATETIME2 NOT NULL,            -- Creation timestamp
    [CreatedBy] INT NOT NULL,                    -- Admin ID
    [UpdatedDate] DATETIME2 NULL                 -- Last update timestamp
);
```

**创建表的SQL脚本**: `Database/CreateHomepageCarouselTable.sql`

## 代码架构

### 新增文件

**DAL层**:
- `recycling.DAL/HomepageCarouselDAL.cs` - 轮播数据访问层
- 扩展了 `recycling.DAL/RecyclableItemDAL.cs` - 增加CRUD方法

**BLL层**:
- `recycling.BLL/HomepageCarouselBLL.cs` - 轮播业务逻辑层
- 扩展了 `recycling.BLL/RecyclableItemBLL.cs` - 增加CRUD方法

**Controller**:
- 扩展了 `recycling.Web.UI/Controllers/StaffController.cs` - 增加管理员端的轮播和可回收物管理方法
- 扩展了 `recycling.Web.UI/Controllers/HomeController.cs` - 增加轮播数据获取

**Views**:
- `recycling.Web.UI/Views/Staff/HomepageManagement.cshtml` - 首页管理入口
- `recycling.Web.UI/Views/Staff/HomepageCarouselManagement.cshtml` - 轮播管理页面
- `recycling.Web.UI/Views/Staff/RecyclableItemsManagement.cshtml` - 可回收物管理页面
- 修改了 `recycling.Web.UI/Views/Home/Index.cshtml` - 用户首页增加轮播显示

## 使用说明

### 1. 数据库初始化

执行SQL脚本创建轮播表：
```bash
sqlcmd -S <server> -d <database> -i Database/CreateHomepageCarouselTable.sql
```

### 2. 管理轮播内容

1. 以管理员身份登录系统
2. 点击导航栏的"首页页面管理"
3. 选择"轮播管理"
4. 点击"新增轮播"按钮
5. 填写表单：
   - 选择媒体类型（图片或视频）
   - 输入媒体URL
   - 可选：填写标题和描述
   - 设置排序顺序（数字越小越靠前）
   - 选择状态（启用/禁用）
6. 点击"保存"

**图片建议**:
- 尺寸：1920x600像素
- 格式：JPG、PNG
- 大小：不超过2MB

**视频建议**:
- 时长：建议8秒左右
- 格式：MP4、WebM
- 编码：H.264
- 大小：建议不超过10MB

### 3. 管理可回收物

1. 以管理员身份登录系统
2. 点击导航栏的"首页页面管理"
3. 选择"可回收物管理"
4. 点击"新增物品"按钮
5. 填写表单：
   - 输入物品名称
   - 选择品类
   - 输入价格（元/kg）
   - 可选：填写描述
   - 设置排序顺序
   - 选择状态（启用/禁用）
6. 点击"保存"

### 4. 查看用户首页效果

1. 退出管理员账号
2. 访问系统首页（或以普通用户身份登录）
3. 可以看到：
   - 顶部轮播区域显示已启用的图片/视频
   - 自动8秒切换（超过2个内容时）
   - 下方展示可回收物列表

## 权限控制

- **轮播管理**: 仅管理员（admin）和超级管理员（superadmin）可访问
- **可回收物管理**: 仅管理员（admin）和超级管理员（superadmin）可访问
- **用户首页**: 所有用户可查看，但只显示启用状态的内容

## API接口

### 轮播管理接口

```csharp
// 获取轮播列表（分页）
POST /Staff/GetCarouselList
参数: page, pageSize

// 获取单个轮播
POST /Staff/GetCarousel
参数: id

// 添加轮播
POST /Staff/AddCarousel
参数: HomepageCarousel对象

// 更新轮播
POST /Staff/UpdateCarousel
参数: HomepageCarousel对象

// 删除轮播（软删除）
POST /Staff/DeleteCarousel
参数: id
```

### 可回收物管理接口

```csharp
// 获取可回收物列表（分页）
POST /Staff/GetRecyclableItemsList
参数: page, pageSize

// 获取单个可回收物
POST /Staff/GetRecyclableItem
参数: id

// 添加可回收物
POST /Staff/AddRecyclableItem
参数: RecyclableItems对象

// 更新可回收物
POST /Staff/UpdateRecyclableItem
参数: RecyclableItems对象

// 删除可回收物（软删除）
POST /Staff/DeleteRecyclableItem
参数: id
```

## 注意事项

1. **媒体文件存储**: 当前实现使用URL方式，需要确保媒体文件可通过HTTP访问
2. **视频格式**: 建议使用MP4格式，确保浏览器兼容性
3. **轮播性能**: 建议轮播内容不超过10个，避免页面加载过慢
4. **软删除策略**: 删除操作只是将IsActive设为false，数据仍保留在数据库中
5. **排序控制**: DisplayOrder和SortOrder值相同时，按ID升序排序

## 测试建议

1. **功能测试**:
   - 测试新增、编辑、删除操作
   - 测试不同媒体类型的轮播效果
   - 测试排序功能
   - 测试启用/禁用状态切换

2. **边界测试**:
   - 测试空列表情况
   - 测试单个轮播项（不应显示左右箭头）
   - 测试大量数据的分页功能

3. **兼容性测试**:
   - 测试不同浏览器的视频播放支持
   - 测试移动端响应式布局
   - 测试图片加载失败的情况

## 未来改进建议

1. 增加图片/视频上传功能，而不仅仅是URL输入
2. 支持批量导入/导出
3. 增加轮播效果配置（淡入淡出、滑动等）
4. 添加轮播内容点击跳转链接
5. 增加访问统计功能
6. 支持定时发布/下架功能

## 技术栈

- **后端**: ASP.NET MVC 5 (.NET Framework 4.8)
- **前端**: Bootstrap 3, jQuery, Font Awesome 5
- **数据库**: SQL Server
- **架构**: 三层架构（DAL-BLL-UI）

## 联系方式

如有问题或建议，请联系开发团队。
