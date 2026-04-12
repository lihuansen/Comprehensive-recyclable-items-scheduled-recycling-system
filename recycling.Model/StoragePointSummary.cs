using System;

namespace recycling.Model
{
    // 中文注释
    /// 暂存点库存汇总模型
    // 中文注释
    public class StoragePointSummary
    {
        // 中文注释
        /// 类别键
        // 中文注释
        public string CategoryKey { get; set; }

        // 中文注释
        /// 类别名称
        // 中文注释
        public string CategoryName { get; set; }

        // 中文注释
        /// 总重量（公斤）
        // 中文注释
        public decimal TotalWeight { get; set; }

        // 中文注释
        /// 总价值（元）
        // 中文注释
        public decimal TotalPrice { get; set; }
    }
}
