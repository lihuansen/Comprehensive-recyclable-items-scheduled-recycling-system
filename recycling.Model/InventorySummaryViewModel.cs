using System;

namespace recycling.Model
{
    /// 库存汇总视图模型
    /// 中文说明
    public class InventorySummaryViewModel
    {
        /// 品类键（用于前端交互）
        /// 中文说明
        public string CategoryKey { get; set; }

        /// 品类名称
        /// 中文说明
        public string CategoryName { get; set; }

        /// 总重量(kg)
        /// 中文说明
        public decimal TotalWeight { get; set; }

        /// 总价值(元)
        /// 中文说明
        public decimal TotalPrice { get; set; }
    }
}
