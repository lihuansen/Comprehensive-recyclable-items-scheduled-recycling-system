using System;

namespace recycling.Model
{
    // 中文注释
    /// 暂存点库存明细模型
    // 中文注释
    public class StoragePointDetail
    {
        // 中文注释
        /// 订单ID
        // 中文注释
        public int OrderID { get; set; }

        // 中文注释
        /// 类别键
        // 中文注释
        public string CategoryKey { get; set; }

        // 中文注释
        /// 类别名称
        // 中文注释
        public string CategoryName { get; set; }

        // 中文注释
        /// 重量（公斤）
        // 中文注释
        public decimal Weight { get; set; }

        // 中文注释
        /// 价值（元）
        // 中文注释
        public decimal Price { get; set; }

        // 中文注释
        /// 完成日期
        // 中文注释
        public DateTime CreatedDate { get; set; }

        // 中文注释
        /// 是否为回收员手动录入
        // 中文注释
        public bool IsManualEntry { get; set; }
    }
}
