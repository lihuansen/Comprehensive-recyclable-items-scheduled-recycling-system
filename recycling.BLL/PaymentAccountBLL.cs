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
        /// </summary>
        public OperationResult AddPaymentAccount(AddPaymentAccountViewModel model, int userId)
        {
            try
            {
                // 验证账户类型
                if (model.AccountType != "Alipay" && model.AccountType != "WeChat" && model.AccountType != "BankCard")
                {
                    return new OperationResult { Success = false, Message = "无效的账户类型" };
                }

                // 如果是银行卡，需要提供银行名称
                if (model.AccountType == "BankCard" && string.IsNullOrWhiteSpace(model.BankName))
                {
                    return new OperationResult { Success = false, Message = "请提供银行名称" };
                }

                // 如果设为默认账户，先取消其他默认账户
                if (model.IsDefault)
                {
                    var existingAccounts = _dal.GetPaymentAccountsByUserId(userId);
                    foreach (var account in existingAccounts)
                    {
                        if (account.IsDefault)
                        {
                            account.IsDefault = false;
                            _dal.UpdatePaymentAccount(account);
                        }
                    }
                }

                // 创建新账户
                var newAccount = new UserPaymentAccount
                {
                    UserID = userId,
                    AccountType = model.AccountType,
                    AccountName = model.AccountName,
                    AccountNumber = model.AccountNumber, // 实际应用中应该加密存储
                    BankName = model.BankName,
                    IsDefault = model.IsDefault,
                    IsVerified = false, // 新添加的账户默认未验证
                    CreatedDate = DateTime.Now,
                    Status = "Active"
                };

                int accountId = _dal.AddPaymentAccount(newAccount);
                
                if (accountId > 0)
                {
                    return new OperationResult { Success = true, Message = "支付账户添加成功", Data = accountId };
                }
                else
                {
                    return new OperationResult { Success = false, Message = "添加支付账户失败" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }

        /// <summary>
        /// 获取用户的支付账户列表
        /// </summary>
        public List<UserPaymentAccount> GetPaymentAccountsByUserId(int userId)
        {
            return _dal.GetPaymentAccountsByUserId(userId);
        }

        /// <summary>
        /// 获取支付账户详情
        /// </summary>
        public UserPaymentAccount GetPaymentAccountById(int accountId)
        {
            return _dal.GetPaymentAccountById(accountId);
        }

        /// <summary>
        /// 删除支付账户
        /// </summary>
        public OperationResult DeletePaymentAccount(int accountId, int userId)
        {
            try
            {
                // 验证账户是否属于该用户
                var account = _dal.GetPaymentAccountById(accountId);
                if (account == null)
                {
                    return new OperationResult { Success = false, Message = "支付账户不存在" };
                }

                if (account.UserID != userId)
                {
                    return new OperationResult { Success = false, Message = "无权删除此账户" };
                }

                bool success = _dal.DeletePaymentAccount(accountId);
                return new OperationResult 
                { 
                    Success = success, 
                    Message = success ? "支付账户删除成功" : "删除支付账户失败" 
                };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }

        /// <summary>
        /// 设置默认支付账户
        /// </summary>
        public OperationResult SetDefaultAccount(int accountId, int userId)
        {
            try
            {
                // 验证账户是否属于该用户
                var account = _dal.GetPaymentAccountById(accountId);
                if (account == null)
                {
                    return new OperationResult { Success = false, Message = "支付账户不存在" };
                }

                if (account.UserID != userId)
                {
                    return new OperationResult { Success = false, Message = "无权设置此账户" };
                }

                bool success = _dal.SetDefaultAccount(userId, accountId);
                return new OperationResult 
                { 
                    Success = success, 
                    Message = success ? "默认账户设置成功" : "设置默认账户失败" 
                };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }

        /// <summary>
        /// 验证支付账户
        /// </summary>
        public OperationResult VerifyAccount(int accountId, int userId)
        {
            try
            {
                var account = _dal.GetPaymentAccountById(accountId);
                if (account == null || account.UserID != userId)
                {
                    return new OperationResult { Success = false, Message = "支付账户不存在或无权访问" };
                }

                account.IsVerified = true;
                bool success = _dal.UpdatePaymentAccount(account);
                return new OperationResult 
                { 
                    Success = success, 
                    Message = success ? "账户验证成功" : "账户验证失败" 
                };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "操作失败：" + ex.Message };
            }
        }
    }
}
