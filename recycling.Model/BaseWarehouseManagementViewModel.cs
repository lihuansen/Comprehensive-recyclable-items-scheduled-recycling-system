using System.Collections.Generic;

namespace recycling.Model
{
    /// 基地仓库管理页面视图模型
    /// 中文说明
    public class BaseWarehouseManagementViewModel
    {
        /// 已完成的运输单列表（待入库）
        /// 中文说明
        public List<TransportNotificationViewModel> CompletedTransportOrders { get; set; }

        /// 入库记录列表
        /// 中文说明
        public List<WarehouseReceiptViewModel> WarehouseReceipts { get; set; }

        /// 当前库存汇总信息
        /// 中文说明
        public List<InventorySummaryViewModel> InventorySummary { get; set; }

        /// 库存明细列表
        /// 中文说明
        public List<InventoryDetailViewModel> InventoryDetails { get; set; }

        /// 库存明细总数
        /// 中文说明
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
