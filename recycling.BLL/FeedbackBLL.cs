using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class FeedbackBLL
    {
        private readonly FeedbackDAL _feedbackDAL;

        public FeedbackBLL()
        {
            _feedbackDAL = new FeedbackDAL();
        }

        /// <summary>
        /// 添加用户反馈
        /// </summary>
        public (bool Success, string Message) AddFeedback(UserFeedback feedback)
        {
            // 验证输入
            if (feedback == null)
                return (false, "反馈对象不能为空");

            if (feedback.UserID <= 0)
                return (false, "用户ID无效");

            if (string.IsNullOrWhiteSpace(feedback.FeedbackType))
                return (false, "请选择反馈类型");

            if (!IsValidFeedbackType(feedback.FeedbackType))
                return (false, "反馈类型无效");

            if (string.IsNullOrWhiteSpace(feedback.Subject))
                return (false, "请输入反馈主题");

            if (feedback.Subject.Length > 100)
                return (false, "反馈主题不能超过100字");

            if (string.IsNullOrWhiteSpace(feedback.Description))
                return (false, "请输入详细描述");

            if (feedback.Description.Length < 10)
                return (false, "详细描述至少需要10个字符");

            if (feedback.Description.Length > 1000)
                return (false, "详细描述不能超过1000字");

            // 验证邮箱格式（如果提供）
            if (!string.IsNullOrWhiteSpace(feedback.ContactEmail))
            {
                if (!IsValidEmail(feedback.ContactEmail))
                    return (false, "邮箱格式不正确");
            }

            // 调用DAL层
            return _feedbackDAL.AddFeedback(feedback);
        }

        /// <summary>
        /// 获取所有反馈（支持分页、筛选和搜索）
        /// </summary>
        public (List<UserFeedback> Feedbacks, int TotalCount, int TotalPages) GetAllFeedbacks(
            string feedbackType = null,
            string status = null,
            string searchKeyword = null,
            int page = 1,
            int pageSize = 20)
        {
            // 验证分页参数
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // 调用DAL层
            var (feedbacks, totalCount) = _feedbackDAL.GetAllFeedbacks(feedbackType, status, searchKeyword, page, pageSize);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return (feedbacks, totalCount, totalPages);
        }

        /// <summary>
        /// 根据ID获取反馈详情
        /// </summary>
        public UserFeedback GetFeedbackById(int feedbackId)
        {
            if (feedbackId <= 0)
                return null;

            return _feedbackDAL.GetFeedbackById(feedbackId);
        }

        /// <summary>
        /// 更新反馈状态和回复
        /// </summary>
        public (bool Success, string Message) UpdateFeedbackStatus(int feedbackId, string status, string adminReply)
        {
            // 验证输入
            if (feedbackId <= 0)
                return (false, "反馈ID无效");

            if (string.IsNullOrWhiteSpace(status))
                return (false, "请选择状态");

            if (!IsValidStatus(status))
                return (false, "状态无效");

            if (!string.IsNullOrWhiteSpace(adminReply) && adminReply.Length > 1000)
                return (false, "回复内容不能超过1000字");

            // 调用DAL层
            return _feedbackDAL.UpdateFeedbackStatus(feedbackId, status, adminReply);
        }

        /// <summary>
        /// 获取反馈统计数据
        /// </summary>
        public Dictionary<string, int> GetFeedbackStatistics()
        {
            return _feedbackDAL.GetFeedbackStatistics();
        }

        /// <summary>
        /// 验证反馈类型是否有效
        /// </summary>
        private bool IsValidFeedbackType(string feedbackType)
        {
            var validTypes = new List<string> { "问题反馈", "功能建议", "投诉举报", "其他" };
            return validTypes.Contains(feedbackType);
        }

        /// <summary>
        /// 验证状态是否有效
        /// </summary>
        private bool IsValidStatus(string status)
        {
            var validStatuses = new List<string> { "待处理", "处理中", "已完成", "已关闭" };
            return validStatuses.Contains(status);
        }

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
