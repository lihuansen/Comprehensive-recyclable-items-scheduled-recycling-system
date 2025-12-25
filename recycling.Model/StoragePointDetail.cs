using System;

namespace recycling.Model
{
    /// <summary>
    /// 暂存点库存明细模型
    /// </summary>
    public class StoragePointDetail
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// 类别键
        /// </summary>
        public string CategoryKey { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 重量（公斤）
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// 价值（元）
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 完成日期
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
