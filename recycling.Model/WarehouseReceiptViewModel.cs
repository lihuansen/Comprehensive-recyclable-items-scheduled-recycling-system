using System;
using System.Collections.Generic;

namespace recycling.Model
{
    /// <summary>
    /// 入库单视图模型
    /// Warehouse Receipt View Model
    /// </summary>
    public class WarehouseReceiptViewModel
    {
        /// <summary>
        /// 入库单ID
        /// </summary>
        public int ReceiptID { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// 运输单ID
        /// </summary>
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 运输单号
        /// </summary>
        public string TransportOrderNumber { get; set; }

        /// <summary>
        /// 回收员ID
        /// </summary>
        public int RecyclerID { get; set; }

        /// <summary>
        /// 回收员姓名
        /// </summary>
        public string RecyclerName { get; set; }

        /// <summary>
        /// 基地人员ID
        /// </summary>
        public int WorkerID { get; set; }

        /// <summary>
        /// 基地人员姓名
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// 入库总重量（kg）
        /// </summary>
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 物品类别（JSON格式）
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// 运输中通知视图模型
    /// In-Transit Notification View Model
    /// </summary>
    public class TransportNotificationViewModel
    {
        /// <summary>
        /// 运输单ID
        /// </summary>
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 运输单号
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// 回收员姓名
        /// </summary>
        public string RecyclerName { get; set; }

        /// <summary>
        /// 运输人员姓名
        /// </summary>
        public string TransporterName { get; set; }

        /// <summary>
        /// 预估重量
        /// </summary>
        public decimal EstimatedWeight { get; set; }

        /// <summary>
        /// 物品类别
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 取货时间
        /// Pickup Date
        /// </summary>
        public DateTime? PickupDate { get; set; }
    }
}
