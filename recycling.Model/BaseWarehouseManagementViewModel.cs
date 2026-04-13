using System.Collections.Generic;

namespace recycling.Model
{
    /// 基地仓库管理页面视图模型
    /// 基地仓库管理页面视图模型。
    public class BaseWarehouseManagementViewModel
    {
        /// 已完成的运输单列表（待入库）
        /// 表示已完成运输订单列表。
        public List<TransportNotificationViewModel> CompletedTransportOrders { get; set; }

        /// 入库记录列表
        /// 表示入库单列表。
        public List<WarehouseReceiptViewModel> WarehouseReceipts { get; set; }

        /// 当前库存汇总信息
        /// 表示库存汇总数据。
        public List<InventorySummaryViewModel> InventorySummary { get; set; }

        /// 库存明细列表
        /// 表示库存明细列表。
        public List<InventoryDetailViewModel> InventoryDetails { get; set; }

        /// 库存明细总数
        /// 表示库存明细总数量。
        public int InventoryDetailsTotalCount { get; set; }

        /// 构造函数
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
