namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BaseStaffNotifications
    {
        [Key]
        public int NotificationID { get; set; }

        public int? WorkerID { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int? RelatedTransportOrderID { get; set; }

        public int? RelatedWarehouseReceiptID { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
    }
    // <summary>
    /// ���ع�����Ա֪ͨ����ö��
    /// </summary>
    public static class BaseStaffNotificationTypes
    {
        /// <summary>
        /// ���䵥�Ѵ���������Ա��ϵ������Ա��
        /// </summary>
        public const string TransportOrderCreated = "TransportOrderCreated";

        /// <summary>
        /// ���䵥�����
        /// </summary>
        public const string TransportOrderCompleted = "TransportOrderCompleted";

        /// <summary>
        /// ��ⵥ�Ѵ���
        /// </summary>
        public const string WarehouseReceiptCreated = "WarehouseReceiptCreated";

        /// <summary>
        /// �ֿ�����д��
        /// </summary>
        public const string WarehouseInventoryWritten = "WarehouseInventoryWritten";

        /// <summary>
        /// ��ȡ֪ͨ���͵���ʾ����
        /// </summary>
        public static string GetDisplayName(string type)
        {
            switch (type)
            {
                case TransportOrderCreated:
                    return "���䵥����";
                case TransportOrderCompleted:
                    return "���䵥���";
                case WarehouseReceiptCreated:
                    return "��ⵥ����";
                case WarehouseInventoryWritten:
                    return "�ֿ�д��";
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
                case TransportOrderCreated:
                    return "fa-truck";
                case TransportOrderCompleted:
                    return "fa-check-circle";
                case WarehouseReceiptCreated:
                    return "fa-file-alt";
                case WarehouseInventoryWritten:
                    return "fa-warehouse";
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
                case TransportOrderCreated:
                    return "#17a2b8"; // info blue
                case TransportOrderCompleted:
                    return "#28a745"; // green
                case WarehouseReceiptCreated:
                    return "#007bff"; // primary blue
                case WarehouseInventoryWritten:
                    return "#6f42c1"; // purple
                default:
                    return "#6c757d"; // gray
            }
        }
    }
}
