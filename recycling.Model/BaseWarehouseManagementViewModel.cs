using System.Collections.Generic;

namespace recycling.Model
{
    // 中文注释
    /// 基地仓库管理页面视图模型
    /// 中文注释
    // 中文注释
    public class BaseWarehouseManagementViewModel
    {
        // 中文注释
        /// 已完成的运输单列表（待入库）
        /// 中文注释
        // 中文注释
        public List<TransportNotificationViewModel> CompletedTransportOrders { get; set; }

        // 中文注释
        /// 入库记录列表
        /// 中文注释
        // 中文注释
        public List<WarehouseReceiptViewModel> WarehouseReceipts { get; set; }

        // 中文注释
        /// 当前库存汇总信息
        /// 中文注释
        // 中文注释
        public List<InventorySummaryViewModel> InventorySummary { get; set; }

        // 中文注释
        /// 库存明细列表
        /// 中文注释
        // 中文注释
        public List<InventoryDetailViewModel> InventoryDetails { get; set; }

        // 中文注释
        /// 库存明细总数
        /// 中文注释
        // 中文注释
        public int InventoryDetailsTotalCount { get; set; }

        // 中文注释
        /// 构造函数
        // 中文注释
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
