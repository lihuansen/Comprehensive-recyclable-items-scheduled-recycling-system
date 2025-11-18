using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 用户联系请求业务逻辑层
    /// </summary>
    public class UserContactRequestsBLL
    {
        private readonly UserContactRequestsDAL _dal = new UserContactRequestsDAL();

        /// <summary>
        /// 创建用户联系请求
        /// </summary>
        public OperationResult CreateContactRequest(int userId)
        {
            if (userId <= 0)
                return new OperationResult { Success = false, Message = "无效的用户ID" };

            try
            {
                int requestId = _dal.CreateContactRequest(userId);

                if (requestId == 0)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Message = "您已有待处理的联系请求，请等待管理员回复"
                    };
                }
                else if (requestId > 0)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Message = "联系请求已提交，管理员会尽快与您联系"
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "提交请求失败，请重试"
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "提交请求时发生错误：" + ex.Message
                };
            }
        }

        /// <summary>
        /// 获取所有待处理的联系请求（管理员使用）
        /// </summary>
        public List<UserContactRequestViewModel> GetPendingRequests()
        {
            try
            {
                return _dal.GetPendingRequests();
            }
            catch (Exception)
            {
                return new List<UserContactRequestViewModel>();
            }
        }

        /// <summary>
        /// 获取所有联系请求（包括已处理的）
        /// </summary>
        public List<UserContactRequestViewModel> GetAllRequests()
        {
            try
            {
                return _dal.GetAllRequests();
            }
            catch (Exception)
            {
                return new List<UserContactRequestViewModel>();
            }
        }

        /// <summary>
        /// 标记请求为已处理（管理员开始处理时调用）
        /// </summary>
        public OperationResult MarkAsContacted(int requestId, int adminId)
        {
            if (requestId <= 0)
                return new OperationResult { Success = false, Message = "无效的请求ID" };

            if (adminId <= 0)
                return new OperationResult { Success = false, Message = "无效的管理员ID" };

            try
            {
                bool success = _dal.MarkAsContacted(requestId, adminId);

                if (success)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Message = "已标记为已处理"
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "标记失败"
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "标记请求时发生错误：" + ex.Message
                };
            }
        }

        /// <summary>
        /// 检查用户是否有待处理的请求
        /// </summary>
        public bool HasPendingRequest(int userId)
        {
            if (userId <= 0)
                return false;

            try
            {
                return _dal.HasPendingRequest(userId);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
