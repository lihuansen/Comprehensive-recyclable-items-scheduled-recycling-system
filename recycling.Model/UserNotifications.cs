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

        public int UserID { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        public int? RelatedOrderID { get; set; }

        public int? RelatedFeedbackID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        public bool IsRead { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ReadDate { get; set; }
    }
    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public static class NotificationTypes
    {
        /// <summary>
        /// 订单已创建
        /// </summary>
        public const string OrderCreated = "OrderCreated";

        /// <summary>
        /// 回收员已接单
        /// </summary>
        public const string OrderAccepted = "OrderAccepted";

        /// <summary>
        /// 订单已完成
        /// </summary>
        public const string OrderCompleted = "OrderCompleted";

        /// <summary>
        /// 评价提醒
        /// </summary>
        public const string ReviewReminder = "ReviewReminder";

        /// <summary>
        /// 订单已取消
        /// </summary>
        public const string OrderCancelled = "OrderCancelled";

        /// <summary>
        /// 轮播图更新
        /// </summary>
        public const string CarouselUpdated = "CarouselUpdated";

        /// <summary>
        /// 反馈已回复
        /// </summary>
        public const string FeedbackReplied = "FeedbackReplied";


        /// <summary>
        /// 回收员发送消息
        /// </summary>
        public const string RecyclerMessageReceived = "RecyclerMessageReceived";

        /// <summary>
        /// 回收员回退订单
        /// </summary>
        public const string OrderRolledBack = "OrderRolledBack";

        /// <summary>
        /// 获取通知类型的显示名称
        /// </summary>
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
                default:
                    return "系统通知";
            }
        }

        /// <summary>
        /// 获取通知类型的图标
        /// </summary>
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
                default:
                    return "fa-bell";
            }
        }

        /// <summary>
        /// 获取通知类型的颜色
        /// </summary>
        public static string GetColor(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "#28a745"; // green
                case OrderAccepted:
                    return "#17a2b8"; // info blue
                case OrderCompleted:
                    return "#28a745"; // green
                case ReviewReminder:
                    return "#ffc107"; // warning yellow
                case OrderCancelled:
                    return "#dc3545"; // red
                case CarouselUpdated:
                    return "#6f42c1"; // purple
                case FeedbackReplied:
                    return "#007bff"; // primary blue
                case RecyclerMessageReceived:
                    return "#20c997"; // teal green
                case OrderRolledBack:
                    return "#fd7e14"; // orange
                default:
                    return "#6c757d"; // gray
            }
        }
    }
}
