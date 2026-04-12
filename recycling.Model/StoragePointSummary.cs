using System;

namespace recycling.Model
{
    /// 暂存点库存汇总模型
    public class StoragePointSummary
    {
        /// 类别键
        public string CategoryKey { get; set; }

        /// 类别名称
        public string CategoryName { get; set; }

        /// 总重量（公斤）
        public decimal TotalWeight { get; set; }

        /// 总价值（元）
        public decimal TotalPrice { get; set; }
    }
}
