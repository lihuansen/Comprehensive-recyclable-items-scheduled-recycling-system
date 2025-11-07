using System;

namespace recycling.Common
{
    /// <summary>
    /// 系统常量类 - 集中管理系统中使用的常量值
    /// 避免在代码中使用魔法字符串和魔法数字
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 订单状态常量
        /// </summary>
        public static class OrderStatus
        {
            public const string Pending = "已预约";
            public const string Confirmed = "进行中";
            public const string Completed = "已完成";
            public const string Cancelled = "已取消";
        }

        /// <summary>
        /// 用户角色常量
        /// </summary>
        public static class Roles
        {
            public const string User = "user";
            public const string Recycler = "recycler";
            public const string Admin = "admin";
            public const string SuperAdmin = "superadmin";
        }

        /// <summary>
        /// Session键名常量
        /// </summary>
        public static class SessionKeys
        {
            public const string LoginUser = "LoginUser";
            public const string LoginStaff = "LoginStaff";
            public const string StaffRole = "StaffRole";
            public const string AppointmentBasicInfo = "AppointmentBasicInfo";
            public const string CategoryDetailModel = "CategoryDetailModel";
        }

        /// <summary>
        /// 消息发送者类型常量
        /// </summary>
        public static class SenderType
        {
            public const string User = "user";
            public const string Recycler = "recycler";
            public const string System = "system";
        }

        /// <summary>
        /// 验证码相关常量
        /// </summary>
        public static class Verification
        {
            /// <summary>
            /// 验证码长度
            /// </summary>
            public const int CodeLength = 6;

            /// <summary>
            /// 验证码有效期（分钟）
            /// </summary>
            public const int ExpirationMinutes = 5;

            /// <summary>
            /// 验证码字符集（移除易混淆字符）
            /// </summary>
            public const string CharSet = "0123456789";
        }

        /// <summary>
        /// 分页相关常量
        /// </summary>
        public static class Pagination
        {
            /// <summary>
            /// 默认每页数量
            /// </summary>
            public const int DefaultPageSize = 20;

            /// <summary>
            /// 最大每页数量
            /// </summary>
            public const int MaxPageSize = 100;

            /// <summary>
            /// 最小页码
            /// </summary>
            public const int MinPageIndex = 1;
        }

        /// <summary>
        /// 评分相关常量
        /// </summary>
        public static class Rating
        {
            /// <summary>
            /// 最低评分
            /// </summary>
            public const int MinStars = 1;

            /// <summary>
            /// 最高评分
            /// </summary>
            public const int MaxStars = 5;
        }

        /// <summary>
        /// 反馈状态常量
        /// </summary>
        public static class FeedbackStatus
        {
            public const string Pending = "待处理";
            public const string InProgress = "处理中";
            public const string Resolved = "已解决";
            public const string Closed = "已关闭";
        }

        /// <summary>
        /// 文件相关常量
        /// </summary>
        public static class Files
        {
            /// <summary>
            /// 允许上传的图片文件扩展名
            /// </summary>
            public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            /// <summary>
            /// 最大文件大小（字节） - 5MB
            /// </summary>
            public const long MaxFileSize = 5 * 1024 * 1024;
        }

        /// <summary>
        /// 缓存键名常量
        /// </summary>
        public static class CacheKeys
        {
            public const string RecyclableItems = "RecyclableItems";
            public const string HomepageCarousel = "HomepageCarousel";
            public const string Categories = "Categories";
        }

        /// <summary>
        /// 时间相关常量
        /// </summary>
        public static class Time
        {
            /// <summary>
            /// Session超时时间（分钟）
            /// </summary>
            public const int SessionTimeoutMinutes = 30;

            /// <summary>
            /// 默认日期格式
            /// </summary>
            public const string DateFormat = "yyyy-MM-dd";

            /// <summary>
            /// 默认日期时间格式
            /// </summary>
            public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }
}
