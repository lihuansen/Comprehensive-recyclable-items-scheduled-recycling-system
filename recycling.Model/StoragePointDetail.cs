using System;

namespace recycling.Model
{
    /// 暂存点库存明细模型
    public class StoragePointDetail
    {
        /// 订单ID
        public int OrderID { get; set; }

        /// 类别键
        public string CategoryKey { get; set; }

        /// 类别名称
        public string CategoryName { get; set; }

        /// 重量（公斤）
        public decimal Weight { get; set; }

        /// 价值（元）
        public decimal Price { get; set; }

        /// 完成日期
        public DateTime CreatedDate { get; set; }

        /// 是否为回收员手动录入
        public bool IsManualEntry { get; set; }
    }
}
