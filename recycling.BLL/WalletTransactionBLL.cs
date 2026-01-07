using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 钱包交易业务逻辑层
    /// </summary>
    public class WalletTransactionBLL
    {
        private WalletTransactionDAL _transactionDAL = new WalletTransactionDAL();
        private PaymentAccountDAL _accountDAL = new PaymentAccountDAL();
        private UserDAL _userDAL = new UserDAL();

        /// <summary>
        /// 充值
        /// </summary>
        public OperationResult Recharge(RechargeViewModel model, int userId)
        {
            try
            {
                // 验证充值金额
                if (model.Amount <= 0)
                {
                    return new OperationResult { Success = false, Message = "充值金额必须大于0" };
                }

                // 验证支付账户
                var account = _accountDAL.GetPaymentAccountById(model.PaymentAccountID);
                if (account == null || account.UserID != userId || account.Status != "Active")
                {
                    return new OperationResult { Success = false, Message = "支付账户无效" };
                }

                // 获取用户当前余额
                var user = _userDAL.GetUserById(userId);
                if (user == null)
                {
                    return new OperationResult { Success = false, Message = "用户不存在" };
                }

                decimal currentBalance = user.money ?? 0;
                decimal newBalance = currentBalance + model.Amount;

                // 创建交易记录
                var transaction = new WalletTransaction
                {
                    UserID = userId,
                    TransactionType = "Recharge",
                    Amount = model.Amount,
                    BalanceBefore = currentBalance,
                    BalanceAfter = newBalance,
                    PaymentAccountID = model.PaymentAccountID,
                    TransactionStatus = "Completed", // 实际应用中，应该先创建为Pending，等支付成功后再更新为Completed
                    Description = "钱包充值",
                    TransactionNo = _transactionDAL.GenerateTransactionNo(),
                    CreatedDate = DateTime.Now,
                    CompletedDate = DateTime.Now,
                    Remarks = model.Remarks
                };

                // 保存交易记录
                int transactionId = _transactionDAL.AddTransaction(transaction);
                if (transactionId <= 0)
                {
                    return new OperationResult { Success = false, Message = "创建交易记录失败" };
                }

                // 更新用户余额
                user.money = newBalance;
                bool updateSuccess = _userDAL.UpdateUser(user);
                if (!updateSuccess)
                {
                    return new OperationResult { Success = false, Message = "更新用户余额失败" };
                }

                // 更新支付账户最后使用时间
                _accountDAL.UpdateLastUsedDate(model.PaymentAccountID);

                return new OperationResult 
                { 
                    Success = true, 
                    Message = "充值成功", 
                    Data = transactionId 
                };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }

        /// <summary>
        /// 提现
        /// </summary>
        public OperationResult Withdraw(WithdrawViewModel model, int userId)
        {
            try
            {
                // 验证提现金额
                if (model.Amount <= 0)
                {
                    return new OperationResult { Success = false, Message = "提现金额必须大于0" };
                }

                // 验证支付账户
                var account = _accountDAL.GetPaymentAccountById(model.PaymentAccountID);
                if (account == null || account.UserID != userId || account.Status != "Active")
                {
                    return new OperationResult { Success = false, Message = "支付账户无效" };
                }

                // 获取用户当前余额
                var user = _userDAL.GetUserById(userId);
                if (user == null)
                {
                    return new OperationResult { Success = false, Message = "用户不存在" };
                }

                decimal currentBalance = user.money ?? 0;
                
                // 验证余额是否足够
                if (currentBalance < model.Amount)
                {
                    return new OperationResult { Success = false, Message = "余额不足" };
                }

                decimal newBalance = currentBalance - model.Amount;

                // 创建交易记录
                var transaction = new WalletTransaction
                {
                    UserID = userId,
                    TransactionType = "Withdraw",
                    Amount = model.Amount,
                    BalanceBefore = currentBalance,
                    BalanceAfter = newBalance,
                    PaymentAccountID = model.PaymentAccountID,
                    TransactionStatus = "Completed", // 实际应用中，应该先创建为Processing，等提现成功后再更新为Completed
                    Description = "钱包提现",
                    TransactionNo = _transactionDAL.GenerateTransactionNo(),
                    CreatedDate = DateTime.Now,
                    CompletedDate = DateTime.Now,
                    Remarks = model.Remarks
                };

                // 保存交易记录
                int transactionId = _transactionDAL.AddTransaction(transaction);
                if (transactionId <= 0)
                {
                    return new OperationResult { Success = false, Message = "创建交易记录失败" };
                }

                // 更新用户余额
                user.money = newBalance;
                bool updateSuccess = _userDAL.UpdateUser(user);
                if (!updateSuccess)
                {
                    return new OperationResult { Success = false, Message = "更新用户余额失败" };
                }

                // 更新支付账户最后使用时间
                _accountDAL.UpdateLastUsedDate(model.PaymentAccountID);

                return new OperationResult 
                { 
                    Success = true, 
                    Message = "提现申请已提交", 
                    Data = transactionId 
                };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }

        /// <summary>
        /// 获取用户交易记录
        /// </summary>
        public List<WalletTransaction> GetTransactionsByUserId(int userId, int pageIndex = 1, int pageSize = 20)
        {
            return _transactionDAL.GetTransactionsByUserId(userId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取交易详情
        /// </summary>
        public WalletTransaction GetTransactionById(int transactionId)
        {
            return _transactionDAL.GetTransactionById(transactionId);
        }

        /// <summary>
        /// 获取用户交易统计
        /// </summary>
        public Dictionary<string, decimal> GetUserTransactionStatistics(int userId)
        {
            return _transactionDAL.GetUserTransactionStatistics(userId);
        }

        /// <summary>
        /// 获取钱包视图模型
        /// </summary>
        public WalletViewModel GetWalletViewModel(int userId)
        {
            var viewModel = new WalletViewModel();

            // 获取用户信息
            viewModel.User = _userDAL.GetUserById(userId);

            // 获取支付账户列表
            viewModel.PaymentAccounts = _accountDAL.GetPaymentAccountsByUserId(userId);

            // 获取最近的交易记录（最多10条）
            viewModel.RecentTransactions = _transactionDAL.GetTransactionsByUserId(userId, 1, 10);

            // 获取统计信息
            var stats = _transactionDAL.GetUserTransactionStatistics(userId);
            viewModel.TotalIncome = stats["TotalIncome"];
            viewModel.TotalExpense = stats["TotalExpense"];
            viewModel.MonthlyTransactionCount = Convert.ToInt32(stats["MonthlyCount"]);

            return viewModel;
        }
    }
}
