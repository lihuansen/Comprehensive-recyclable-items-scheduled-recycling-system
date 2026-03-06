using System;

namespace recycling.Model
{
    /// <summary>
    /// 超时订单信息模型（用于超时自动回退检查）
    /// </summary>
    public class ExpiredOrderInfo
    {
        /// <summary>
        /// 预约订单ID
        /// </summary>
        public int AppointmentID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 预约日期
        /// </summary>
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// 时间段
        /// </summary>
        public string TimeSlot { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 回收员ID
        /// </summary>
        public int? RecyclerID { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// 截止时间（根据预约日期和时间段计算）
        /// </summary>
        public DateTime DeadlineTime { get; set; }

        /// <summary>
        /// 格式化的订单编号
        /// </summary>
        public string OrderNumber => $"#AP{AppointmentID:D6}";
    }
}
