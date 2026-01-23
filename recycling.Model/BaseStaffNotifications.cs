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
        /// 运输单已创建，运输人员联系基地人员
        /// </summary>
        public const string TransportOrderCreated = "TransportOrderCreated";

        /// <summary>
        /// 运输单已完成
        /// </summary>
        public const string TransportOrderCompleted = "TransportOrderCompleted";

        /// <summary>
        /// 入库单已创建
        /// </summary>
        public const string WarehouseReceiptCreated = "WarehouseReceiptCreated";

        /// <summary>
        /// 仓库库存已写入
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
                    return "运输单创建";
                case TransportOrderCompleted:
                    return "运输单完成";
                case WarehouseReceiptCreated:
                    return "入库单创建";
                case WarehouseInventoryWritten:
                    return "仓库写入";
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
        /// 获取通知类型的颜色
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
