using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    public class FeedbackBLL
    {
        private FeedbackDAL _feedbackDAL = new FeedbackDAL();

        /// <summary>
        /// 添加用户反馈（带业务逻辑验证）
        /// </summary>
        public (bool Success, string Message) AddFeedback(UserFeedback feedback)
        {
            // 1. 检查反馈对象不为空
            if (feedback == null)
            {
                return (false, "反馈对象不能为空");
            }

            // 2. 验证 UserID 有效性
            if (feedback.UserID <= 0)
            {
                return (false, "用户ID无效");
            }

            // 3. 验证反馈类型
            string[] validTypes = { "问题反馈", "功能建议", "投诉举报", "其他" };
            if (string.IsNullOrEmpty(feedback.FeedbackType) || 
                Array.IndexOf(validTypes, feedback.FeedbackType) == -1)
            {
                return (false, "请选择有效的反馈类型");
            }

            // 4. 验证主题
            if (string.IsNullOrWhiteSpace(feedback.Subject))
            {
                return (false, "反馈主题不能为空");
            }
            if (feedback.Subject.Length > 100)
            {
                return (false, "反馈主题不能超过100字");
            }

            // 5. 验证描述
            if (string.IsNullOrWhiteSpace(feedback.Description))
            {
                return (false, "详细描述不能为空");
            }
            if (feedback.Description.Length < 10)
            {
                return (false, "详细描述至少需要10个字符");
            }
            if (feedback.Description.Length > 1000)
            {
                return (false, "详细描述不能超过1000字");
            }

            // 6. 验证邮箱格式（如果提供）
            if (!string.IsNullOrEmpty(feedback.ContactEmail))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(feedback.ContactEmail, emailPattern))
                {
                    return (false, "邮箱格式不正确");
                }
            }

            // 7. 设置默认值
            feedback.Status = "反馈中";
            feedback.CreatedDate = DateTime.Now;

            // 8. 调用 DAL 层执行数据库操作
            return _feedbackDAL.AddFeedback(feedback);
        }

        /// <summary>
        /// 获取所有反馈（管理员用）
        /// </summary>
        public List<UserFeedback> GetAllFeedbacks(string status = null, string feedbackType = null)
        {
            return _feedbackDAL.GetAllFeedbacks(status, feedbackType);
        }

        /// <summary>
        /// 更新反馈状态和管理员回复
        /// </summary>
        public (bool Success, string Message) UpdateFeedbackStatus(int feedbackId, string status, string adminReply)
        {
            // 验证反馈ID
            if (feedbackId <= 0)
            {
                return (false, "反馈ID无效");
            }

            // 验证状态（如果提供）
            if (!string.IsNullOrEmpty(status))
            {
                string[] validStatuses = { "反馈中", "已完成" };
                if (Array.IndexOf(validStatuses, status) == -1)
                {
                    return (false, "请选择有效的状态");
                }
            }

            // 验证管理员回复长度（如果提供）
            if (adminReply != null && adminReply.Length > 1000)
            {
                return (false, "管理员回复不能超过1000字");
            }

            return _feedbackDAL.UpdateFeedbackStatus(feedbackId, status, adminReply);
        }
    }
}
