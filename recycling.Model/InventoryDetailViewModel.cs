using System;

namespace recycling.Model
{
    /// <summary>
    /// 库存明细视图模型（包含回收员信息）- 管理员端使用
    /// </summary>
    public class InventoryDetailViewModel
    {
        public int InventoryID { get; set; }
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string CategoryKey { get; set; }
        public string CategoryName { get; set; }
        public decimal Weight { get; set; }
        public decimal? Price { get; set; }
        public int RecyclerID { get; set; }
        public string RecyclerName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
