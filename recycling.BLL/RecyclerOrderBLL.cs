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
        private readonly StaffDAL _staffDAL = new StaffDAL();

        /// <summary>
        /// 获取回收员订单列表
        /// 说明：在获取 DAL 返回的分页结果后，遍历每个订单计算 CanComplete（是否显示“完成订单”按钮）。
        /// 计算规则（保守实现）：订单状态为“进行中”且存在最近一次结束会话（Conversations）且在该 EndedTime 之后没有新的消息，则 CanComplete = true。
        /// 为兼容你当前仓库，我使用了 ConversationBLL.GetLatestConversation 和 MessageBLL.GetOrderMessages（这两个在之前代码中已有或我建议添加）。
        /// </summary>
        public PagedResult<RecyclerOrderViewModel> GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            // 从 DAL 获取分页结果
            var pagedResult = _recyclerOrderDAL.GetRecyclerOrders(filter, recyclerId);

            // 若无数据直接返回
            if (pagedResult == null || pagedResult.Items == null || !pagedResult.Items.Any())
                return pagedResult;

            // 计算 CanComplete 标识
            var conversationBll = new ConversationBLL();
            var messageBll = new MessageBLL();

            foreach (var orderVm in pagedResult.Items)
            {
                orderVm.CanComplete = false; // 默认 false

                try
                {
                    // 仅对“进行中”订单判断
                    if (string.Equals(orderVm.Status, "进行中", StringComparison.OrdinalIgnoreCase))
                    {
                        // 获取最近一次结束会话（若存在）
                        var latestConv = conversationBll.GetLatestConversation(orderVm.AppointmentID);
                        if (latestConv != null && latestConv.EndedTime.HasValue)
                        {
                            // 获取该订单全部消息（简单实现）
                            var allMessages = messageBll.GetOrderMessages(orderVm.AppointmentID);

                            // 如果所有消息的 SentTime 均 <= latestConv.EndedTime，则说明没有新消息
                            bool hasAfter = allMessages != null && allMessages.Any(m =>
                                (m.SentTime.HasValue ? m.SentTime.Value : DateTime.MinValue) > latestConv.EndedTime.Value);

                            if (!hasAfter)
                            {
                                orderVm.CanComplete = true;
                            }
                        }
                    }
                }
                catch
                {
                    // 发生异常时保持 CanComplete = false，避免影响主流程
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
                // 检查回收员的Available状态
                var recycler = _staffDAL.GetRecyclerById(recyclerId);
                if (recycler == null)
                {
                    return (false, "回收员不存在");
                }
                
                if (recycler.Available != true)
                {
                    return (false, "当前状态不可接单");
                }

                bool result = _recyclerOrderDAL.AcceptOrder(appointmentId, recyclerId);
                return result ? (true, "订单接收成功") : (false, "订单接收失败：订单不存在或状态不允许接收");
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
            if (recyclerId <= 0) return new List<RecyclerMessageViewModel>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            return _recyclerOrderDAL.GetRecyclerMessages(recyclerId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取订单对话
        /// </summary>
        public List<RecyclerMessageViewModel> GetOrderConversation(int orderId)
        {
            if (orderId <= 0) return new List<RecyclerMessageViewModel>();
            return _recyclerOrderDAL.GetOrderConversation(orderId);
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public (bool Success, string Message) MarkMessageAsRead(int messageId, int recyclerId)
        {
            if (messageId <= 0 || recyclerId <= 0) return (false, "参数无效");

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
            if (appointmentId <= 0 || recyclerId <= 0) return (null, "参数无效");

            try
            {
                var detail = _recyclerOrderDAL.GetOrderDetail(appointmentId, recyclerId);
                if (detail == null || string.IsNullOrEmpty(detail.OrderNumber))
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

        /// <summary>
        /// 回收员回退订单（当线下发现物品不符合要求时使用）
        /// </summary>
        public (bool Success, string Message) RollbackOrder(int appointmentId, int recyclerId, string reason = null)
        {
            if (appointmentId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            try
            {
                // 验证回收员
                var recycler = _staffDAL.GetRecyclerById(recyclerId);
                if (recycler == null)
                {
                    return (false, "回收员不存在");
                }

                // 获取订单详情，验证回收员权限
                var orderDetail = _recyclerOrderDAL.GetOrderDetail(appointmentId, recyclerId);
                if (orderDetail == null || string.IsNullOrEmpty(orderDetail.OrderNumber))
                {
                    return (false, "订单不存在或无权操作");
                }

                // 验证订单状态必须是"进行中"
                if (orderDetail.Status != "进行中")
                {
                    return (false, $"订单状态不正确，当前状态为：{orderDetail.Status}，只有进行中的订单才能回退");
                }

                // 更新订单状态为"已取消-回收员回退"
                var orderDAL = new OrderDAL();
                bool updateResult = orderDAL.UpdateOrderStatus(appointmentId, "已取消-回收员回退");
                
                if (!updateResult)
                {
                    return (false, "更新订单状态失败");
                }

                return (true, "订单已成功回退");
            }
            catch (Exception ex)
            {
                return (false, $"回退订单失败：{ex.Message}");
            }
        }
    }
}
