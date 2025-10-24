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
    }
}
