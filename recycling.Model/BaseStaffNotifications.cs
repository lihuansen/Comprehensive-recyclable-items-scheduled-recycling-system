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

        // Alias property for DAL compatibility
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
    /// <summary>
    /// 基地工作人员通知类型枚举
    /// </summary>
    public static class BaseStaffNotificationTypes
    {
        /// <summary>
        /// 运输单已创建，运输已开始
        /// </summary>
        public const string TransportOrderCreated = "TransportOrderCreated";

        /// <summary>
        /// 运输单处于运输中状态
        /// </summary>
        public const string TransportOrderInTransit = "TransportOrderInTransit";

        /// <summary>
        /// 运输单已到达目的地
        /// </summary>
        public const string TransportOrderCompleted = "TransportOrderCompleted";

        /// <summary>
        /// 提示创建入库单
        /// </summary>
        public const string CreateWarehouseReceiptPrompt = "CreateWarehouseReceiptPrompt";

        /// <summary>
        /// 入库单已创建，提示细分
        /// </summary>
        public const string WarehouseReceiptReceived = "WarehouseReceiptReceived";

        /// <summary>
        /// 细分完成，提示入库
        /// </summary>
        public const string WarehouseReceiptCreated = "WarehouseReceiptCreated";

        /// <summary>
        /// 入库成功
        /// </summary>
        public const string WarehouseInventoryWritten = "WarehouseInventoryWritten";

        /// <summary>
        /// 获取通知类型的显示名称
        /// </summary>
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

        /// <summary>
        /// 获取通知类型的图标
        /// </summary>
        public static string GetIcon(string type)
        {
            switch (type)
            {
                case TransportOrderCreated:
                    return "fa-truck";
                case TransportOrderInTransit:
                    return "fa-route";
                case TransportOrderCompleted:
                    return "fa-map-marker-alt";
                case CreateWarehouseReceiptPrompt:
                    return "fa-clipboard-list";
                case WarehouseReceiptReceived:
                    return "fa-inbox";
                case WarehouseReceiptCreated:
                    return "fa-tasks";
                case WarehouseInventoryWritten:
                    return "fa-warehouse";
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
                case TransportOrderCreated:
                    return "#17a2b8"; // info blue
                case TransportOrderInTransit:
                    return "#007bff"; // primary blue
                case TransportOrderCompleted:
                    return "#28a745"; // green
                case CreateWarehouseReceiptPrompt:
                    return "#20c997"; // teal
                case WarehouseReceiptReceived:
                    return "#fd7e14"; // orange
                case WarehouseReceiptCreated:
                    return "#e83e8c"; // pink
                case WarehouseInventoryWritten:
                    return "#6f42c1"; // purple
                default:
                    return "#6c757d"; // gray
            }
        }
    }
}
