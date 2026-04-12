using System;

namespace recycling.Model
{
    // 中文注释
    /// 库存汇总视图模型
    /// 中文注释
    // 中文注释
    public class InventorySummaryViewModel
    {
        // 中文注释
        /// 品类键（用于前端交互）
        /// 中文注释
        // 中文注释
        public string CategoryKey { get; set; }

        // 中文注释
        /// 品类名称
        /// 中文注释
        // 中文注释
        public string CategoryName { get; set; }

        // 中文注释
        /// 总重量(kg)
        /// 中文注释
        // 中文注释
        public decimal TotalWeight { get; set; }

        // 中文注释
        /// 总价值(元)
        /// 中文注释
        // 中文注释
        public decimal TotalPrice { get; set; }
    }
}
