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

        public int? RelatedWarehouseReceipt { get; set; }

        /// 兼容旧代码中的字段命名（RelatedWarehouseReceiptID）
        [NotMapped]
        public int? RelatedWarehouseReceiptID
        {
            get { return RelatedWarehouseReceipt; }
            set { RelatedWarehouseReceipt = value; }
        }

        public DateTime? CreatedDate { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
    }

    /// 基地工作人员通知类型
    public static class BaseStaffNotificationTypes
    {
        public const string TransportOrderCreated = "TransportOrderCreated";
        public const string TransportOrderInTransit = "TransportOrderInTransit";
        public const string TransportOrderCompleted = "TransportOrderCompleted";
        public const string CreateWarehouseReceiptPrompt = "CreateWarehouseReceiptPrompt";
        public const string WarehouseReceiptReceived = "WarehouseReceiptReceived";
        public const string WarehouseReceiptCreated = "WarehouseReceiptCreated";
        public const string WarehouseInventoryWritten = "WarehouseInventoryWritten";

        public static string GetDisplayName(string type)
        {
            switch (type)
            {
                case TransportOrderCreated:
                    return "运输单开始";
                case TransportOrderInTransit:
                    return "运输中";
                case TransportOrderCompleted:
                    return "已到达";
                case CreateWarehouseReceiptPrompt:
                    return "提示创建入库单";
                case WarehouseReceiptReceived:
                    return "提示细分";
                case WarehouseReceiptCreated:
                    return "提示入库";
                case WarehouseInventoryWritten:
                    return "入库成功";
                default:
                    return "系统通知";
            }
        }

        public static string GetIcon(string type)
        {
            switch (type)
            {
                case TransportOrderCreated:
                    return "fa-truck-loading";
                case TransportOrderInTransit:
                    return "fa-shipping-fast";
                case TransportOrderCompleted:
                    return "fa-map-marker-alt";
                case CreateWarehouseReceiptPrompt:
                    return "fa-file-medical";
                case WarehouseReceiptReceived:
                    return "fa-tasks";
                case WarehouseReceiptCreated:
                    return "fa-warehouse";
                case WarehouseInventoryWritten:
                    return "fa-check-circle";
                default:
                    return "fa-bell";
            }
        }

        public static string GetColor(string type)
        {
            switch (type)
            {
                case TransportOrderCreated:
                    return "#17a2b8";
                case TransportOrderInTransit:
                    return "#007bff";
                case TransportOrderCompleted:
                    return "#28a745";
                case CreateWarehouseReceiptPrompt:
                    return "#fd7e14";
                case WarehouseReceiptReceived:
                    return "#6f42c1";
                case WarehouseReceiptCreated:
                    return "#ffc107";
                case WarehouseInventoryWritten:
                    return "#20c997";
                default:
                    return "#6c757d";
            }
        }
    }
}
