namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserNotifications
    {
        [Key]
        public int NotificationID { get; set; }

        public int? UserID { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int? RelatedOrderID { get; set; }

        public int? RelatedFeedbackID { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
    }
    /// 通知类型枚举
    public static class NotificationTypes
    {
        /// 订单已创建
        public const string OrderCreated = "OrderCreated";

        /// 回收员已接单
        public const string OrderAccepted = "OrderAccepted";

        /// 订单已完成
        public const string OrderCompleted = "OrderCompleted";

        /// 评价提醒
        public const string ReviewReminder = "ReviewReminder";

        /// 订单已取消
        public const string OrderCancelled = "OrderCancelled";

        /// 轮播图更新
        public const string CarouselUpdated = "CarouselUpdated";

        /// 反馈已回复
        public const string FeedbackReplied = "FeedbackReplied";


        /// 回收员发来消息
        public const string RecyclerMessageReceived = "RecyclerMessageReceived";

        /// 回收员退回订单
        public const string OrderRolledBack = "OrderRolledBack";

        /// 订单超时自动回退
        public const string OrderExpiredAutoRollback = "OrderExpiredAutoRollback";

        /// 获取通知类型的显示名称
        public static string GetDisplayName(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "下单通知";
                case OrderAccepted:
                    return "接单通知";
                case OrderCompleted:
                    return "完成通知";
                case ReviewReminder:
                    return "评价提醒";
                case OrderCancelled:
                    return "取消通知";
                case CarouselUpdated:
                    return "系统公告";
                case FeedbackReplied:
                    return "反馈回复";
                case RecyclerMessageReceived:
                    return "回收员消息";
                case OrderRolledBack:
                    return "订单回退";
                case OrderExpiredAutoRollback:
                    return "订单超时回退";
                default:
                    return "系统通知";
            }
        }

        /// 获取通知类型的图标
        public static string GetIcon(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "fa-file-alt";
                case OrderAccepted:
                    return "fa-user-check";
                case OrderCompleted:
                    return "fa-check-circle";
                case ReviewReminder:
                    return "fa-star";
                case OrderCancelled:
                    return "fa-times-circle";
                case CarouselUpdated:
                    return "fa-bullhorn";
                case FeedbackReplied:
                    return "fa-comment-dots";
                case RecyclerMessageReceived:
                    return "fa-envelope";
                case OrderRolledBack:
                    return "fa-undo";
                case OrderExpiredAutoRollback:
                    return "fa-clock";
                default:
                    return "fa-bell";
            }
        }

        /// 获取通知类型的颜色
        public static string GetColor(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "#28a745"; // 中文说明
                case OrderAccepted:
                    return "#17a2b8"; // 中文说明
                case OrderCompleted:
                    return "#28a745"; // 中文说明
                case ReviewReminder:
                    return "#ffc107"; // 中文说明
                case OrderCancelled:
                    return "#dc3545"; // 中文说明
                case CarouselUpdated:
                    return "#6f42c1"; // 中文说明
                case FeedbackReplied:
                    return "#007bff"; // 中文说明
                case RecyclerMessageReceived:
                    return "#20c997"; // 中文说明
                case OrderRolledBack:
                    return "#fd7e14"; // 中文说明
                case OrderExpiredAutoRollback:
                    return "#e83e8c"; // 中文说明
                default:
                    return "#6c757d"; // 中文说明
            }
        }
    }
}
