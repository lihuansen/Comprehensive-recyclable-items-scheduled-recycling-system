namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用户通知消息实体类
    /// </summary>
    public class UserNotifications
    {
        [Key]
        public int NotificationID { get; set; }

        /// <summary>
        /// 用户ID (外键)
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        [StringLength(50)]
        public string NotificationType { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// 通知内容
        /// </summary>
        [StringLength(1000)]
        public string Content { get; set; }

        /// <summary>
        /// 关联的订单ID (可为空)
        /// </summary>
        public int? RelatedOrderID { get; set; }

        /// <summary>
        /// 关联的反馈ID (可为空)
        /// </summary>
        public int? RelatedFeedbackID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 已读时间
        /// </summary>
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
        /// 获取通知类型的中文名称
        /// </summary>
        public static string GetDisplayName(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "订单通知";
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
                default:
                    return "#6c757d"; // gray
            }
        }
    }
}
