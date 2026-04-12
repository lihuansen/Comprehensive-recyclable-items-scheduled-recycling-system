using System;

namespace recycling.Model
{
    /// 超时订单信息模型（用于超时自动回退检查）
    public class ExpiredOrderInfo
    {
        /// 预约订单ID
        public int AppointmentID { get; set; }

        /// 用户ID
        public int UserID { get; set; }

        /// 预约日期
        public DateTime AppointmentDate { get; set; }

        /// 时间段
        public string TimeSlot { get; set; }

        /// 订单状态
        public string Status { get; set; }

        /// 回收员ID
        public int? RecyclerID { get; set; }

        /// 联系人姓名
        public string ContactName { get; set; }

        /// 截止时间（根据预约日期和时间段计算）
        public DateTime DeadlineTime { get; set; }

        /// 格式化的订单编号
        public string OrderNumber => $"#AP{AppointmentID:D6}";
    }
}
