using System;

namespace recycling.Model
{
    // 中文注释
    /// 超时订单信息模型（用于超时自动回退检查）
    // 中文注释
    public class ExpiredOrderInfo
    {
        // 中文注释
        /// 预约订单ID
        // 中文注释
        public int AppointmentID { get; set; }

        // 中文注释
        /// 用户ID
        // 中文注释
        public int UserID { get; set; }

        // 中文注释
        /// 预约日期
        // 中文注释
        public DateTime AppointmentDate { get; set; }

        // 中文注释
        /// 时间段
        // 中文注释
        public string TimeSlot { get; set; }

        // 中文注释
        /// 订单状态
        // 中文注释
        public string Status { get; set; }

        // 中文注释
        /// 回收员ID
        // 中文注释
        public int? RecyclerID { get; set; }

        // 中文注释
        /// 联系人姓名
        // 中文注释
        public string ContactName { get; set; }

        // 中文注释
        /// 截止时间（根据预约日期和时间段计算）
        // 中文注释
        public DateTime DeadlineTime { get; set; }

        // 中文注释
        /// 格式化的订单编号
        // 中文注释
        public string OrderNumber => $"#AP{AppointmentID:D6}";
    }
}
