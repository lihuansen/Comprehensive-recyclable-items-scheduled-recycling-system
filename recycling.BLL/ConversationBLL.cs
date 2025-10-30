using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class ConversationBLL
    {
        private readonly ConversationDAL _conversationDAL = new ConversationDAL();

        public bool EndConversation(int orderId, int userId)
        {
            if (orderId <= 0 || userId <= 0) return false;
            return _conversationDAL.EndConversation(orderId, userId);
        }

        public List<ConversationViewModel> GetUserConversations(int userId, int pageIndex = 1, int pageSize = 50)
        {
            if (userId <= 0) return new List<ConversationViewModel>();
            return _conversationDAL.GetUserConversations(userId, pageIndex, pageSize);
        }

        public List<ConversationViewModel> GetRecyclerConversations(int recyclerId, int pageIndex = 1, int pageSize = 50)
        {
            if (recyclerId <= 0) return new List<ConversationViewModel>();
            return _conversationDAL.GetRecyclerConversations(recyclerId, pageIndex, pageSize);
        }

        public List<Messages> GetConversationMessagesBeforeEnd(int orderId, DateTime endedTime)
        {
            if (orderId <= 0 || endedTime == default(DateTime)) return new List<Messages>();
            return _conversationDAL.GetConversationMessagesBeforeEnd(orderId, endedTime);
        }
    }
}
