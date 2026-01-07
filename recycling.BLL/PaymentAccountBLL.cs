using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 用户支付账户业务逻辑层
    /// </summary>
    public class PaymentAccountBLL
    {
        private PaymentAccountDAL _dal = new PaymentAccountDAL();

        /// <summary>
        /// 添加支付账户
        /// 注意：支付账户功能暂未开发
        /// </summary>
        public OperationResult AddPaymentAccount(AddPaymentAccountViewModel model, int userId)
        {
            return new OperationResult { Success = false, Message = "添加支付账户功能开发中" };
        }

        /// <summary>
        /// 获取用户的支付账户列表
        /// 注意：支付账户功能暂未开发，返回空列表
        /// </summary>
        public List<UserPaymentAccount> GetPaymentAccountsByUserId(int userId)
        {
            return new List<UserPaymentAccount>();
        }

        /// <summary>
        /// 获取支付账户详情
        /// 注意：支付账户功能暂未开发，返回null
        /// </summary>
        public UserPaymentAccount GetPaymentAccountById(int accountId)
        {
            return null;
        }

        /// <summary>
        /// 删除支付账户
        /// 注意：支付账户功能暂未开发
        /// </summary>
        public OperationResult DeletePaymentAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "删除功能开发中" };
        }

        /// <summary>
        /// 设置默认支付账户
        /// 注意：支付账户功能暂未开发
        /// </summary>
        public OperationResult SetDefaultAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "设置默认账户功能开发中" };
        }

        /// <summary>
        /// 验证支付账户
        /// 注意：支付账户功能暂未开发
        /// </summary>
        public OperationResult VerifyAccount(int accountId, int userId)
        {
            return new OperationResult { Success = false, Message = "验证账户功能开发中" };
        }
    }
}
