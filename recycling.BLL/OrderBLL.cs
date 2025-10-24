using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    public class OrderBLL
    {
        private OrderDAL _orderDAL = new OrderDAL();

        /// <summary>
        /// 获取用户订单列表
        /// </summary>
        public List<AppointmentOrder> GetUserOrders(int userId, string status = "all")
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID无效");
            }

            return _orderDAL.GetOrdersByUserAndStatus(userId, status);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        public OrderDetail GetOrderDetail(int appointmentId, int userId)
        {
            if (appointmentId <= 0 || userId <= 0)
            {
                throw new ArgumentException("参数无效");
            }

            var orderDetail = _orderDAL.GetOrderDetail(appointmentId, userId);

            if (orderDetail == null)
            {
                throw new Exception("订单不存在或无权访问");
            }

            return orderDetail;
        }

        /// <summary>
        /// 获取各状态订单数量统计
        /// </summary>
        public OrderStatistics GetOrderStatistics(int userId)
        {
            var statistics = new OrderStatistics
            {
                Total = GetUserOrders(userId, "all").Count,
                Pending = GetUserOrders(userId, "pending").Count,
                Confirmed = GetUserOrders(userId, "confirmed").Count,
                Completed = GetUserOrders(userId, "completed").Count,
                Cancelled = GetUserOrders(userId, "cancelled").Count
            };

            return statistics;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        public (bool Success, string Message) CancelOrder(int appointmentId, int userId)
        {
            if (appointmentId <= 0 || userId <= 0)
            {
                return (false, "参数无效");
            }

            try
            {
                bool result = _orderDAL.CancelOrder(appointmentId, userId);

                if (result)
                {
                    return (true, "订单取消成功");
                }
                else
                {
                    return (false, "订单取消失败：订单不存在或状态不允许取消");
                }
            }
            catch (Exception ex)
            {
                return (false, $"订单取消失败：{ex.Message}");
            }
        }
    }
}
