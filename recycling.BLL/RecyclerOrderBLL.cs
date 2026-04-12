using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// 获取回收员订单列表
        /// 说明：在获取 DAL 返回的分页结果后，遍历每个订单计算 CanComplete（是否显示“完成订单”按钮）。
        /// 计算规则（当前实现）：订单状态为“进行中”则允许完成。
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

            foreach (var orderVm in pagedResult.Items)
            {
                // 仅在进行中订单允许完成
                orderVm.CanComplete = string.Equals(orderVm.Status, "进行中", StringComparison.OrdinalIgnoreCase);
            }

            return pagedResult;
        }

        /// 获取订单统计信息
        public OrderStatistics GetOrderStatistics()
        {
            return _recyclerOrderDAL.GetOrderStatistics();
        }

        /// 回收员接收订单
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

        /// 获取回收员订单统计
        public RecyclerOrderStatistics GetRecyclerOrderStatistics(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return new RecyclerOrderStatistics();
            }

            return _recyclerOrderDAL.GetRecyclerOrderStatistics(recyclerId);
        }

        /// 获取回收员消息列表
        public List<RecyclerMessageViewModel> GetRecyclerMessages(int recyclerId, int pageIndex = 1, int pageSize = 20)
        {
            if (recyclerId <= 0) return new List<RecyclerMessageViewModel>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            return _recyclerOrderDAL.GetRecyclerMessages(recyclerId, pageIndex, pageSize);
        }

        /// 获取订单对话
        public List<RecyclerMessageViewModel> GetOrderConversation(int orderId)
        {
            if (orderId <= 0) return new List<RecyclerMessageViewModel>();
            return _recyclerOrderDAL.GetOrderConversation(orderId);
        }

        /// 标记消息为已读
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

        /// 获取订单详情
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

        /// 回收员回退订单（当线下发现物品不符合要求时使用）
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

                // 如果没有提供原因，使用默认原因
                var rollbackReason = string.IsNullOrWhiteSpace(reason) ? "物品不符合回收要求" : reason;

                // 更新订单状态为"已取消-回收员回退"并保存回退原因
                var orderDAL = new OrderDAL();
                bool updateResult = orderDAL.UpdateOrderStatusWithReason(appointmentId, "已取消-回收员回退", rollbackReason);
                
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

        /// 检查并处理超时订单（自动回退超过预约时间段的订单）
        /// 根据北京时间判断，如果订单的预约日期+时间段最晚时间已过，则自动回退订单并通知用户
        /// <param name="recyclerId">回收员ID（0表示检查所有订单）</param>
        /// <returns>处理结果，包含超时订单数量和消息</returns>
        public (int ExpiredCount, List<string> Messages) CheckAndHandleExpiredOrders(int recyclerId = 0)
        {
            var messages = new List<string>();
            int expiredCount = 0;

            try
            {
                // 获取已超时的订单
                var expiredOrders = _recyclerOrderDAL.GetExpiredOrders(recyclerId);

                if (expiredOrders == null || expiredOrders.Count == 0)
                {
                    return (0, messages);
                }

                var orderDAL = new OrderDAL();
                var notificationBLL = new UserNotificationBLL();
                var messageBLL = new MessageBLL();

                foreach (var order in expiredOrders)
                {
                    try
                    {
                        // 更新订单状态为"已取消-系统超时回退"
                        string rollbackReason = "订单已超过预约时间段，系统自动回退";
                        bool updateResult = orderDAL.UpdateOrderStatusWithReason(
                            order.AppointmentID, 
                            "已取消-系统超时回退", 
                            rollbackReason);

                        if (updateResult)
                        {
                            expiredCount++;

                            // 发送系统消息到聊天记录（如果订单有关联的回收员）
                            if (order.RecyclerID.HasValue)
                            {
                                var systemMessage = new SendMessageRequest
                                {
                                    OrderID = order.AppointmentID,
                                    SenderType = "system",
                                    SenderID = 0,
                                    Content = $"系统提示：订单 {order.OrderNumber} 已超过预约时间段（{order.DeadlineTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}），系统已自动回退此订单。"
                                };
                                messageBLL.SendMessage(systemMessage);
                            }

                            // 发送用户通知消息
                            notificationBLL.SendOrderExpiredNotification(order.AppointmentID, order.UserID);

                            messages.Add($"订单 {order.OrderNumber} 已超时自动回退");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"处理超时订单 {order.AppointmentID} 失败: {ex.Message}");
                        messages.Add($"订单 {order.OrderNumber} 处理失败: {ex.Message}");
                    }
                }

                return (expiredCount, messages);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查超时订单失败: {ex.Message}");
                messages.Add($"检查超时订单失败: {ex.Message}");
                return (0, messages);
            }
        }
    }
}
