using System;

namespace recycling.Model
{
    /// <summary>
    /// 暂存点库存汇总模型
    /// </summary>
    public class StoragePointSummary
    {
        /// <summary>
        /// 类别键
        /// </summary>
        public string CategoryKey { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 总重量（公斤）
        /// </summary>
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 总价值（元）
        /// </summary>
        public decimal TotalPrice { get; set; }
    }
}
