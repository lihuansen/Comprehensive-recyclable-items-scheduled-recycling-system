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
        /// 构造函数
        /// </summary>
        public BaseWarehouseManagementViewModel()
        {
            CompletedTransportOrders = new List<TransportNotificationViewModel>();
            WarehouseReceipts = new List<WarehouseReceiptViewModel>();
        }
    }
}
