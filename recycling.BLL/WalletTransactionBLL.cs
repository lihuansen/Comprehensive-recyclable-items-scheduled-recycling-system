using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 钱包交易业务逻辑层
    /// 注意：当前为存根实现，待数据库表就绪后恢复完整功能
    /// </summary>
    public class WalletTransactionBLL
    {
        // DAL 对象保留用于 GetWalletViewModel() 中获取用户信息
        // 其他 DAL 对象保留以便将来恢复完整功能时使用
        private WalletTransactionDAL _transactionDAL = new WalletTransactionDAL();
        private PaymentAccountDAL _accountDAL = new PaymentAccountDAL();
        private UserDAL _userDAL = new UserDAL();

        /// <summary>
        /// 充值
        /// 注意：钱包充值功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult Recharge(RechargeViewModel model, int userId)
        {
            return new OperationResult { Success = false, Message = "充值功能即将上线，敬请期待！" };
        }

        /// <summary>
        /// 提现
        /// 注意：钱包提现功能暂时存根实现，待数据库表就绪后恢复
        /// </summary>
        public OperationResult Withdraw(WithdrawViewModel model, int userId)
        {
            return new OperationResult { Success = false, Message = "提现功能即将上线，敬请期待！" };
        }

        /// <summary>
        /// 获取用户交易记录
        /// 注意：钱包功能暂时存根实现，返回空列表
        /// </summary>
        public List<WalletTransaction> GetTransactionsByUserId(int userId, int pageIndex = 1, int pageSize = 20)
        {
            return new List<WalletTransaction>();
        }

        /// <summary>
        /// 获取交易详情
        /// 注意：钱包功能暂时存根实现，返回null
        /// </summary>
        public WalletTransaction GetTransactionById(int transactionId)
        {
            return null;
        }

        /// <summary>
        /// 获取用户交易统计
        /// 注意：钱包功能暂时存根实现，返回空统计数据
        /// </summary>
        public Dictionary<string, decimal> GetUserTransactionStatistics(int userId)
        {
            return new Dictionary<string, decimal>
            {
                { "TotalIncome", 0 },
                { "TotalExpense", 0 },
                { "MonthlyCount", 0 }
            };
        }

        /// <summary>
        /// 获取钱包视图模型
        /// 注意：钱包功能暂时存根实现，返回空数据以保留UI显示
        /// </summary>
        public WalletViewModel GetWalletViewModel(int userId)
        {
            var viewModel = new WalletViewModel();

            // 获取用户基本信息
            viewModel.User = _userDAL.GetUserById(userId);

            // 钱包功能暂时存根实现，返回空列表
            viewModel.PaymentAccounts = new List<UserPaymentAccount>();
            viewModel.RecentTransactions = new List<WalletTransaction>();

            // 返回默认统计数据
            viewModel.TotalIncome = 0;
            viewModel.TotalExpense = 0;
            viewModel.MonthlyTransactionCount = 0;

            return viewModel;
        }
    }
}
