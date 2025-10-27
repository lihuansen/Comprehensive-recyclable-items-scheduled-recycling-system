using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class RecyclerOrderBLL
    {
        private readonly RecyclerOrderDAL _recyclerOrderDAL = new RecyclerOrderDAL();

        /// <summary>
        /// 获取回收员订单列表
        /// </summary>
        public PagedResult<RecyclerOrderViewModel> GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
        {
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            return _recyclerOrderDAL.GetRecyclerOrders(filter, recyclerId);
        }

        /// <summary>
        /// 获取订单统计信息
        /// </summary>
        public OrderStatistics GetOrderStatistics()
        {
            return _recyclerOrderDAL.GetOrderStatistics();
        }

        /// <summary>
        /// 回收员接收订单
        /// </summary>
        public (bool Success, string Message) AcceptOrder(int appointmentId, int recyclerId)
        {
            if (appointmentId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            try
            {
                bool result = _recyclerOrderDAL.AcceptOrder(appointmentId, recyclerId);
                if (result)
                {
                    return (true, "订单接收成功");
                }
                else
                {
                    return (false, "订单接收失败：订单不存在或状态不允许接收");
                }
            }
            catch (Exception ex)
            {
                return (false, $"订单接收失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取回收员订单统计
        /// </summary>
        public RecyclerOrderStatistics GetRecyclerOrderStatistics(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return new RecyclerOrderStatistics();
            }

            return _recyclerOrderDAL.GetRecyclerOrderStatistics(recyclerId);
        }

        /// <summary>
        /// 获取回收员消息列表
        /// </summary>
        public List<RecyclerMessageViewModel> GetRecyclerMessages(int recyclerId, int pageIndex = 1, int pageSize = 20)
        {
            if (recyclerId <= 0)
            {
                return new List<RecyclerMessageViewModel>();
            }

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            return _recyclerOrderDAL.GetRecyclerMessages(recyclerId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取订单对话
        /// </summary>
        public List<RecyclerMessageViewModel> GetOrderConversation(int orderId)
        {
            if (orderId <= 0)
            {
                return new List<RecyclerMessageViewModel>();
            }

            return _recyclerOrderDAL.GetOrderConversation(orderId);
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public (bool Success, string Message) MarkMessageAsRead(int messageId, int recyclerId)
        {
            if (messageId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            try
            {
                bool result = _recyclerOrderDAL.MarkRecyclerMessagesAsRead(messageId, recyclerId);
                return (result, result ? "标记成功" : "标记失败");
            }
            catch (Exception ex)
            {
                return (false, $"标记失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        public (OrderDetailModel Detail, string Message) GetOrderDetail(int appointmentId, int recyclerId)
        {
            if (appointmentId <= 0 || recyclerId <= 0)
            {
                return (null, "参数无效");
            }

            try
            {
                var detail = _recyclerOrderDAL.GetOrderDetail(appointmentId, recyclerId);

                // 检查是否获取到数据
                if (string.IsNullOrEmpty(detail.OrderNumber))
                {
                    return (null, "订单不存在或无权查看");
                }

                return (detail, "获取成功");
            }
            catch (Exception ex)
            {
                return (null, $"获取订单详情失败：{ex.Message}");
            }
        }
    }
}
