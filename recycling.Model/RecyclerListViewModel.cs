using System;

namespace recycling.Model
{
    /// <summary>
    /// 回收员列表视图模型（包含完成订单数）
    /// </summary>
    public class RecyclerListViewModel
    {
        public int RecyclerID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Region { get; set; }
        public decimal? Rating { get; set; }
        public bool Available { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CompletedOrders { get; set; }
    }
}
