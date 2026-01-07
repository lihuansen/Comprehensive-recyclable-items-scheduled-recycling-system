using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 用户支付账户业务逻辑层
    /// 注意：当前为存根实现，待数据库表就绪后恢复完整功能
    /// </summary>
    public class PaymentAccountBLL
    {
        // DAL 对象保留以便将来恢复完整功能时使用
        private PaymentAccountDAL _dal = new PaymentAccountDAL();

        /// <summary>
        /// 添加支付账户
        /// 注意：支付账户功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult AddPaymentAccount(AddPaymentAccountViewModel model, int userId)
        {
            return new OperationResult { Success = false, Message = "添加支付账户功能即将上线，敬请期待！" };
        }

        /// <summary>
        /// 获取用户的支付账户列表
        /// 注意：支付账户功能暂时存根实现，返回空列表
        /// </summary>
        public List<UserPaymentAccount> GetPaymentAccountsByUserId(int userId)
        {
            return new List<UserPaymentAccount>();
        }

        /// <summary>
        /// 获取支付账户详情
        /// 注意：支付账户功能暂时存根实现，返回null
        /// </summary>
        public UserPaymentAccount GetPaymentAccountById(int accountId)
        {
            return null;
        }

        /// <summary>
        /// 删除支付账户
        /// 注意：支付账户功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult DeletePaymentAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "删除支付账户功能即将上线，敬请期待！" };
        }

        /// <summary>
        /// 设置默认支付账户
        /// 注意：支付账户功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult SetDefaultAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "设置默认账户功能即将上线，敬请期待！" };
        }

        /// <summary>
        /// 验证支付账户
        /// 注意：支付账户功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult VerifyAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "验证账户功能即将上线，敬请期待！" };
        }
    }
}
