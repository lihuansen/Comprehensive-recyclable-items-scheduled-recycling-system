using System;

namespace recycling.Common
{
    /// 系统常量类 - 集中管理系统中使用的常量值
    /// 避免在代码中使用魔法字符串和魔法数字
    public static class Constants
    {
        /// 订单状态常量
        public static class OrderStatus
        {
            public const string Pending = "已预约";
            public const string Confirmed = "进行中";
            public const string Completed = "已完成";
            public const string Cancelled = "已取消";
        }

        /// 用户角色常量
        public static class Roles
        {
            public const string User = "user";
            public const string Recycler = "recycler";
            public const string Admin = "admin";
            public const string SuperAdmin = "superadmin";
        }

        /// Session键名常量
        public static class SessionKeys
        {
            public const string LoginUser = "LoginUser";
            public const string LoginStaff = "LoginStaff";
            public const string StaffRole = "StaffRole";
            public const string AppointmentBasicInfo = "AppointmentBasicInfo";
            public const string CategoryDetailModel = "CategoryDetailModel";
        }

        /// 消息发送者类型常量
        public static class SenderType
        {
            public const string User = "user";
            public const string Recycler = "recycler";
            public const string System = "system";
        }

        /// 验证码相关常量
        public static class Verification
        {
            /// 验证码长度
            public const int CodeLength = 6;

            /// 验证码有效期（分钟）
            public const int ExpirationMinutes = 5;

            /// 验证码字符集（移除易混淆字符）
            public const string CharSet = "0123456789";
        }

        /// 分页相关常量
        public static class Pagination
        {
            /// 默认每页数量
            public const int DefaultPageSize = 20;

            /// 最大每页数量
            public const int MaxPageSize = 100;

            /// 最小页码
            public const int MinPageIndex = 1;
        }

        /// 评分相关常量
        public static class Rating
        {
            /// 最低评分
            public const int MinStars = 1;

            /// 最高评分
            public const int MaxStars = 5;
        }

        /// 反馈状态常量
        public static class FeedbackStatus
        {
            public const string Pending = "待处理";
            public const string InProgress = "处理中";
            public const string Resolved = "已解决";
            public const string Closed = "已关闭";
        }

        /// 文件相关常量
        public static class Files
        {
            // 使用私有只读数组和公共属性，防止外部修改
            private static readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            /// 允许上传的图片文件扩展名（只读副本）
            public static string[] AllowedImageExtensions
            {
                get { return (string[])_allowedImageExtensions.Clone(); }
            }

            /// 最大文件大小（字节） - 5MB
            public const long MaxFileSize = 5 * 1024 * 1024;
        }

        /// 缓存键名常量
        public static class CacheKeys
        {
            public const string RecyclableItems = "RecyclableItems";
            public const string HomepageCarousel = "HomepageCarousel";
            public const string Categories = "Categories";
        }

        /// 时间相关常量
        public static class Time
        {
            /// Session超时时间（分钟）
            public const int SessionTimeoutMinutes = 30;

            /// 默认日期格式
            public const string DateFormat = "yyyy-MM-dd";

            /// 默认日期时间格式
            public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }
}
