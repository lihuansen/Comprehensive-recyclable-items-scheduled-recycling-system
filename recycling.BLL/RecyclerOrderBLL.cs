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
        /// 修改说明：在获取 DAL 返回的分页结果后，遍历每个订单计算 CanComplete（是否显示“完成订单”按钮）
        /// 计算规则：订单状态为“进行中”且存在最近一次结束会话（Conversations）且在该 EndedTime 之后没有新的消息，则 CanComplete = true
        /// </summary>
        public PagedResult<RecyclerOrderViewModel> GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
        {
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            // 1) 从 DAL 获取分页结果
            var pagedResult = _recyclerOrderDAL.GetRecyclerOrders(filter, recyclerId);

            // 2) 若分页结果或项目为空，直接返回
            if (pagedResult == null || pagedResult.Items == null || !pagedResult.Items.Any())
            {
                return pagedResult;
            }

            // 3) 计算每个订单是否可完成（CanComplete）
            var conversationBll = new ConversationBLL();
            var messageBll = new MessageBLL();

            foreach (var orderVm in pagedResult.Items)
            {
                orderVm.CanComplete = false; // 默认为 false

                try
                {
                    // 只有“进行中”的订单才考虑完成按钮
                    if (string.Equals(orderVm.Status, "进行中", StringComparison.OrdinalIgnoreCase))
                    {
                        // 获取该订单最近的一次结束会话（如果有）
                        var latestConv = conversationBll.GetLatestConversation(orderVm.AppointmentID);
                        if (latestConv != null && latestConv.EndedTime.HasValue)
                        {
                            // 获取此订单的全部消息（或可优化为 DAL 层直接判断是否存在 SentTime > EndedTime 的消息）
                            var allMessages = messageBll.GetOrderMessages(orderVm.AppointmentID);

                            // 如果没有任何消息的 SentTime 在 EndedTime 之后，则允许完成订单
                            bool hasAfter = allMessages != null && allMessages.Any(m => m.SentTime > latestConv.EndedTime.Value);
                            if (!hasAfter)
                            {
                                orderVm.CanComplete = true;
                            }
                        }
                    }
                }
                catch
                {
                    // 如果计算出现异常，不影响主流程（保持 CanComplete=false）
                    orderVm.CanComplete = false;
                }
            }

            return pagedResult;
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
