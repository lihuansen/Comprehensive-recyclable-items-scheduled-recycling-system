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
    /// <summary>
    /// ֪ͨ����ö��
    /// </summary>
    public static class NotificationTypes
    {
        /// <summary>
        /// �����Ѵ���
        /// </summary>
        public const string OrderCreated = "OrderCreated";

        /// <summary>
        /// ����Ա�ѽӵ�
        /// </summary>
        public const string OrderAccepted = "OrderAccepted";

        /// <summary>
        /// ���������
        /// </summary>
        public const string OrderCompleted = "OrderCompleted";

        /// <summary>
        /// ��������
        /// </summary>
        public const string ReviewReminder = "ReviewReminder";

        /// <summary>
        /// ������ȡ��
        /// </summary>
        public const string OrderCancelled = "OrderCancelled";

        /// <summary>
        /// �ֲ�ͼ����
        /// </summary>
        public const string CarouselUpdated = "CarouselUpdated";

        /// <summary>
        /// �����ѻظ�
        /// </summary>
        public const string FeedbackReplied = "FeedbackReplied";


        /// <summary>
        /// ����Ա������Ϣ
        /// </summary>
        public const string RecyclerMessageReceived = "RecyclerMessageReceived";

        /// <summary>
        /// ����Ա���˶���
        /// </summary>
        public const string OrderRolledBack = "OrderRolledBack";

        /// <summary>
        /// 订单超时自动回退
        /// </summary>
        public const string OrderExpiredAutoRollback = "OrderExpiredAutoRollback";

        /// <summary>
        /// ��ȡ֪ͨ���͵���ʾ����
        /// </summary>
        public static string GetDisplayName(string type)
        {
            switch (type)
            {
                case OrderCreated:
                    return "�µ�֪ͨ";
                case OrderAccepted:
                    return "�ӵ�֪ͨ";
                case OrderCompleted:
                    return "���֪ͨ";
                case ReviewReminder:
                    return "��������";
                case OrderCancelled:
                    return "ȡ��֪ͨ";
                case CarouselUpdated:
                    return "ϵͳ����";
                case FeedbackReplied:
                    return "�����ظ�";
                case RecyclerMessageReceived:
                    return "����Ա��Ϣ";
                case OrderRolledBack:
                    return "��������";
                case OrderExpiredAutoRollback:
                    return "订单超时回退";
                default:
                    return "ϵͳ֪ͨ";
            }
        }

        /// <summary>
        /// ��ȡ֪ͨ���͵�ͼ��
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
                case OrderExpiredAutoRollback:
                    return "fa-clock";
                default:
                    return "fa-bell";
            }
        }

        /// <summary>
        /// ��ȡ֪ͨ���͵���ɫ
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
                case OrderExpiredAutoRollback:
                    return "#e83e8c"; // pink
                default:
                    return "#6c757d"; // gray
            }
        }
    }
}
