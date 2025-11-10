using System;
using System.Collections.Generic;
using System.Linq;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class AdminContactBLL
    {
        private readonly AdminContactDAL _adminContactDAL = new AdminContactDAL();

        /// <summary>
        /// 获取或创建用户与管理员的会话
        /// </summary>
        public int GetOrCreateConversation(int userId, int? adminId = null)
        {
            if (userId <= 0)
                throw new ArgumentException("无效的用户ID");

            return _adminContactDAL.GetOrCreateConversation(userId, adminId);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public OperationResult SendMessage(int userId, int? adminId, string senderType, string content)
        {
            // 验证输入
            if (userId <= 0)
                return new OperationResult { Success = false, Message = "无效的用户ID" };

            if (string.IsNullOrWhiteSpace(content))
                return new OperationResult { Success = false, Message = "消息内容不能为空" };

            if (content.Length > 2000)
                return new OperationResult { Success = false, Message = "消息内容不能超过2000字符" };

            if (!new[] { "user", "admin", "system" }.Contains(senderType.ToLower()))
                return new OperationResult { Success = false, Message = "无效的发送者类型" };

            try
            {
                bool success = _adminContactDAL.SendMessage(userId, adminId, senderType.ToLower(), content);
                
                if (success)
                {
                    return new OperationResult { Success = true, Message = "消息发送成功" };
                }
                else
                {
                    return new OperationResult { Success = false, Message = "消息发送失败" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "发送消息时发生错误：" + ex.Message };
            }
        }

        /// <summary>
        /// 获取用户的会话列表
        /// </summary>
        public List<AdminContactConversations> GetUserConversations(int userId)
        {
            if (userId <= 0)
                return new List<AdminContactConversations>();

            return _adminContactDAL.GetUserConversations(userId);
        }

        /// <summary>
        /// 获取所有会话列表（管理员使用）
        /// </summary>
        public List<AdminContactConversations> GetAllConversations()
        {
            return _adminContactDAL.GetAllConversations();
        }

        /// <summary>
        /// 获取会话的消息记录
        /// </summary>
        public List<AdminContactMessages> GetConversationMessages(int userId, DateTime? beforeTime = null)
        {
            if (userId <= 0)
                return new List<AdminContactMessages>();

            return _adminContactDAL.GetConversationMessages(userId, beforeTime);
        }

        /// <summary>
        /// 结束会话（用户端）
        /// </summary>
        public OperationResult EndConversationByUser(int userId)
        {
            if (userId <= 0)
                return new OperationResult { Success = false, Message = "无效的用户ID" };

            try
            {
                bool success = _adminContactDAL.EndConversationByUser(userId);
                
                if (success)
                {
                    // 发送系统消息
                    _adminContactDAL.SendMessage(userId, null, "system", "用户已结束对话");
                    return new OperationResult { Success = true, Message = "对话已结束" };
                }
                else
                {
                    return new OperationResult { Success = false, Message = "没有活动的会话需要结束" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "结束对话时发生错误：" + ex.Message };
            }
        }

        /// <summary>
        /// 结束会话（管理员端）
        /// </summary>
        public OperationResult EndConversationByAdmin(int userId, int adminId)
        {
            if (userId <= 0)
                return new OperationResult { Success = false, Message = "无效的用户ID" };

            if (adminId <= 0)
                return new OperationResult { Success = false, Message = "无效的管理员ID" };

            try
            {
                bool success = _adminContactDAL.EndConversationByAdmin(userId, adminId);
                
                if (success)
                {
                    // 发送系统消息
                    _adminContactDAL.SendMessage(userId, adminId, "system", "管理员已结束对话");
                    return new OperationResult { Success = true, Message = "对话已结束" };
                }
                else
                {
                    return new OperationResult { Success = false, Message = "没有活动的会话需要结束" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "结束对话时发生错误：" + ex.Message };
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public Users GetUserById(int userId)
        {
            if (userId <= 0)
                return null;

            return _adminContactDAL.GetUserById(userId);
        }

        /// <summary>
        /// 检查会话是否已完全结束
        /// </summary>
        public bool IsBothEnded(int userId)
        {
            if (userId <= 0)
                return false;

            return _adminContactDAL.IsBothEnded(userId);
        }
    }
}
