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

        public (bool BothEnded, DateTime? LatestEndedTime) HasBothEnded(int orderId)
        {
            return _conversationDAL.HasBothEnded(orderId);
        }

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

        /// <summary>
        /// 获取用户的历史消息（超过1个月的消息）
        /// </summary>
        public List<Messages> GetUserHistoricalMessages(int orderId, int userId)
        {
            if (orderId <= 0 || userId <= 0) return new List<Messages>();
            return _conversationDAL.GetUserHistoricalMessages(orderId, userId);
        }

        /// <summary>
        /// 确保有一个活跃的对话会话
        /// 如果双方都已结束之前的对话，则创建新的对话
        /// </summary>
        public bool EnsureActiveConversation(int orderId)
        {
            if (orderId <= 0) return false;
            return _conversationDAL.EnsureActiveConversation(orderId);
        }

        /// <summary>
        /// 获取对话结束状态信息
        /// 返回：谁已结束对话的字符串("user", "recycler", "both", 或空字符串)
        /// </summary>
        public string GetConversationEndedByStatus(int orderId)
        {
            var latestConv = GetLatestConversation(orderId);
            if (latestConv == null) return "";

            if (latestConv.UserEnded && latestConv.RecyclerEnded)
            {
                return "both";
            }
            else if (latestConv.UserEnded)
            {
                return "user";
            }
            else if (latestConv.RecyclerEnded)
            {
                return "recycler";
            }
            return "";
        }
    }
}
