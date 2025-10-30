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

        // 结束会话（endedByType: "user"|"recycler", endedById 对应 ID）
        public bool EndConversationBy(int orderId, string endedByType, int endedById)
        {
            if (orderId <= 0) return false;
            return _conversationDAL.EndConversation(orderId, endedByType, endedById);
        }

        // 获取最近一次结束会话（若无则返回 null）
        public ConversationViewModel GetLatestConversation(int orderId)
        {
            if (orderId <= 0) return null;
            return _conversationDAL.GetLatestConversation(orderId);
        }

        public List<Messages> GetConversationMessagesBeforeEnd(int orderId, DateTime endedTime)
        {
            if (orderId <= 0 || endedTime == default(DateTime)) return new List<Messages>();
            return _conversationDAL.GetConversationMessagesBeforeEnd(orderId, endedTime);
        }

        public List<ConversationViewModel> GetUserConversations(int userId, int pageIndex = 1, int pageSize = 50)
        {
            return _conversationDAL.GetUserConversations(userId, pageIndex, pageSize);
        }

        public List<ConversationViewModel> GetRecyclerConversations(int recyclerId, int pageIndex = 1, int pageSize = 50)
        {
            return _conversationDAL.GetRecyclerConversations(recyclerId, pageIndex, pageSize);
        }
    }
}
