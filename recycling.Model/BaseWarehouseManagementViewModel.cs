using System.Collections.Generic;

namespace recycling.Model
{
    /// <summary>
    /// 基地仓库管理页面视图模型
    /// Base Warehouse Management Page View Model
    /// </summary>
    public class BaseWarehouseManagementViewModel
    {
        /// <summary>
        /// 已完成的运输单列表（待入库）
        /// Completed transport orders (ready for warehousing)
        /// </summary>
        public List<TransportNotificationViewModel> CompletedTransportOrders { get; set; }

        /// <summary>
        /// 入库记录列表
        /// Warehouse receipt records
        /// </summary>
        public List<WarehouseReceiptViewModel> WarehouseReceipts { get; set; }

        /// <summary>
        /// 当前库存汇总信息
        /// Current inventory summary information
        /// </summary>
        public List<InventorySummaryViewModel> InventorySummary { get; set; }

        /// <summary>
        /// 库存明细列表
        /// Inventory detail items list
        /// </summary>
        public List<InventoryDetailViewModel> InventoryDetails { get; set; }

        /// <summary>
        /// 库存明细总数
        /// Total count of inventory details
        /// </summary>
        public int InventoryDetailsTotalCount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseWarehouseManagementViewModel()
        {
            CompletedTransportOrders = new List<TransportNotificationViewModel>();
            WarehouseReceipts = new List<WarehouseReceiptViewModel>();
            InventorySummary = new List<InventorySummaryViewModel>();
            InventoryDetails = new List<InventoryDetailViewModel>();
            InventoryDetailsTotalCount = 0;
        }
    }
}
