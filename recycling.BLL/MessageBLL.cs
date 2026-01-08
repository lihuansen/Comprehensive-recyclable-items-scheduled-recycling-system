using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class MessageBLL
    {
        private readonly MessageDAL _messageDAL = new MessageDAL();
        private readonly ConversationBLL _conversationBLL = new ConversationBLL();
        private readonly UserNotificationBLL _userNotificationBLL = new UserNotificationBLL();

        /// <summary>
        /// 发送消息
        /// </summary>
        public (bool Success, string Message) SendMessage(SendMessageRequest request)
        {
            if (request.OrderID <= 0 || string.IsNullOrEmpty(request.Content))
            {
                return (false, "消息内容不能为空");
            }

            try
            {
                // 在发送消息前，确保有活跃的对话会话
                // 如果之前的对话双方都已结束，则创建新对话
                _conversationBLL.EnsureActiveConversation(request.OrderID);

                var message = new Messages
                {
                    OrderID = request.OrderID,
                    SenderType = request.SenderType,
                    SenderID = request.SenderID,
                    Content = request.Content,
                    SentTime = DateTime.Now,
                    IsRead = false
                };

                bool result = _messageDAL.SendMessage(message);
                if (result)
                {
                    // 如果是回收员发送的消息，创建用户通知
                    if (request.SenderType == "recycler" && request.SenderID > 0)
                    {
                        try
                        {
                            // 获取回收员名称
                            var staffDAL = new StaffDAL();
                            var recycler = staffDAL.GetRecyclerById(request.SenderID);
                            string recyclerName = recycler?.Username ?? "回收员";
                            
                            // 发送用户通知
                            _userNotificationBLL.SendRecyclerMessageNotification(
                                request.OrderID, 
                                recyclerName, 
                                request.Content
                            );
                        }
                        catch (Exception ex)
                        {
                            // 通知发送失败不应影响消息发送，只记录错误
                            System.Diagnostics.Debug.WriteLine($"创建用户通知失败：{ex.Message}");
                        }
                    }
                    
                    return (true, "消息发送成功");
                }
                else
                {
                    return (false, "消息发送失败");
                }
            }
            catch (Exception ex)
            {
                return (false, $"消息发送失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取订单聊天记录
        /// </summary>
        public List<Messages> GetOrderMessages(int orderId)
        {
            if (orderId <= 0)
            {
                return new List<Messages>();
            }

            return _messageDAL.GetOrderMessages(orderId);
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public bool MarkMessagesAsRead(int orderId, string readerType, int readerId)
        {
            if (orderId <= 0 || string.IsNullOrEmpty(readerType))
            {
                return false;
            }

            return _messageDAL.MarkMessagesAsRead(orderId, readerType, readerId);
        }

        /// <summary>
        /// 获取用户的消息列表
        /// </summary>
        public List<RecyclerMessageViewModel> GetUserMessages(int userId, int pageIndex = 1, int pageSize = 20)
        {
            if (userId <= 0)
            {
                return new List<RecyclerMessageViewModel>();
            }
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            return _messageDAL.GetUserMessages(userId, pageIndex, pageSize);
        }

        /// <summary>
        /// 用户发送消息给回收员
        /// </summary>
        // 在MessageBLL中补充用户发送消息方法（修正版）
        public (bool Success, string Message) UserSendMessage(SendMessageRequest request)
        {
            if (request.OrderID <= 0 || string.IsNullOrEmpty(request.Content))
            {
                return (false, "订单ID和消息内容不能为空");
            }

            try
            {
                // 校验订单是否存在且状态为"进行中"（复用OrderBLL的GetOrderDetail）
                var orderBLL = new OrderBLL();
                OrderDetail orderDetail;
                try
                {
                    // 通过用户ID和订单ID获取详情（确保订单属于该用户）
                    orderDetail = orderBLL.GetOrderDetail(request.OrderID, request.SenderID);
                }
                catch (Exception)
                {
                    return (false, "订单不存在或无权访问");
                }

                // 检查订单状态是否为"进行中"（数据库中状态字段值为"进行中"）
                if (orderDetail.Appointment.Status != "进行中")
                {
                    return (false, "仅可对进行中的订单发送消息");
                }

                // 在发送消息前，确保有活跃的对话会话
                // 如果之前的对话双方都已结束，则创建新对话
                _conversationBLL.EnsureActiveConversation(request.OrderID);

                // 发送消息（指定发送者类型为user）
                request.SenderType = "user";
                var message = new Messages
                {
                    OrderID = request.OrderID,
                    SenderType = request.SenderType,
                    SenderID = request.SenderID,
                    Content = request.Content,
                    SentTime = DateTime.Now,
                    IsRead = false
                };

                bool result = _messageDAL.SendMessage(message);
                return result ? (true, "消息发送成功") : (false, "消息发送失败");
            }
            catch (Exception ex)
            {
                return (false, $"发送失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取回收员的未读消息数量
        /// </summary>
        public int GetRecyclerUnreadCount(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return 0;
            }

            return _messageDAL.GetRecyclerUnreadCount(recyclerId);
        }
    }
}
