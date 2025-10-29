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

        public List<Messages> GetOrderMessages(int orderId)
        {
            return _messageDAL.GetMessagesByOrderId(orderId);
        }

        public Result SendMessage(SendMessageRequest request)
        {
            try
            {
                var message = new Messages
                {
                    OrderID = request.OrderID,
                    SenderID = request.SenderID,
                    SenderType = request.SenderType,
                    Content = request.Content,
                    SentTime = DateTime.Now,
                    IsRead = false
                };

                _messageDAL.SaveMessage(message);
                return new Result { Success = true, Message = "发送成功" };
            }
            catch (Exception ex)
            {
                return new Result { Success = false, Message = ex.Message };
            }
        }

        public bool MarkMessagesAsRead(int orderId, string receiverType, int receiverId)
        {
            return _messageDAL.MarkMessagesAsRead(orderId, receiverType, receiverId);
        }
    }
}
