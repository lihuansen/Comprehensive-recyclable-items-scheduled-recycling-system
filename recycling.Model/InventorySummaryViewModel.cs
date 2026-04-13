using System;

namespace recycling.Model
{
    /// 库存汇总视图模型
    /// 库存汇总视图模型。
    public class InventorySummaryViewModel
    {
        /// 品类键（用于前端交互）
        /// 表示分类编码。
        public string CategoryKey { get; set; }

        /// 品类名称
        /// 表示分类名称。
        public string CategoryName { get; set; }

        /// 总重量(kg)
        /// 表示总重量。
        public decimal TotalWeight { get; set; }

        /// 总价值(元)
        /// 表示总金额。
        public decimal TotalPrice { get; set; }
    }
}
