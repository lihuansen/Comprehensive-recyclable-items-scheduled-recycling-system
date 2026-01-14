using System;

namespace recycling.Model
{
    /// <summary>
    /// 库存汇总视图模型
    /// Inventory Summary View Model
    /// </summary>
    public class InventorySummaryViewModel
    {
        /// <summary>
        /// 品类键（用于前端交互）
        /// Category Key (for frontend interaction)
        /// </summary>
        public string CategoryKey { get; set; }

        /// <summary>
        /// 品类名称
        /// Category Name
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 总重量(kg)
        /// Total Weight (kg)
        /// </summary>
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 总价值(元)
        /// Total Price (CNY)
        /// </summary>
        public decimal TotalPrice { get; set; }
    }
}
