namespace recycling.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 钱包视图模型
    /// 用于钱包页面的数据展示
    /// </summary>
    public class WalletViewModel
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public Users User { get; set; }

        /// <summary>
        /// 用户的支付账户列表
        /// </summary>
        public List<UserPaymentAccount> PaymentAccounts { get; set; }

        /// <summary>
        /// 最近的交易记录列表
        /// </summary>
        public List<WalletTransaction> RecentTransactions { get; set; }

        /// <summary>
        /// 当前余额
        /// </summary>
        public decimal CurrentBalance
        {
            get { return User?.money ?? 0; }
        }

        /// <summary>
        /// 累计收入（统计数据）
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// 累计支出（统计数据）
        /// </summary>
        public decimal TotalExpense { get; set; }

        /// <summary>
        /// 本月交易次数（统计数据）
        /// </summary>
        public int MonthlyTransactionCount { get; set; }

        /// <summary>
        /// 是否有默认支付账户
        /// </summary>
        public bool HasDefaultPaymentAccount
        {
            get
            {
                return PaymentAccounts != null && 
                       PaymentAccounts.Exists(a => a.IsDefault && a.Status == "Active");
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WalletViewModel()
        {
            PaymentAccounts = new List<UserPaymentAccount>();
            RecentTransactions = new List<WalletTransaction>();
            TotalIncome = 0;
            TotalExpense = 0;
            MonthlyTransactionCount = 0;
        }
    }

    /// <summary>
    /// 添加支付账户的视图模型
    /// </summary>
    public class AddPaymentAccountViewModel
    {
        /// <summary>
        /// 账户类型：Alipay, WeChat, BankCard
        /// </summary>
        [Required(ErrorMessage = "请选择账户类型")]
        public string AccountType { get; set; }

        /// <summary>
        /// 账户名称/持卡人姓名
        /// </summary>
        [Required(ErrorMessage = "请输入账户名称")]
        [StringLength(100, ErrorMessage = "账户名称不能超过100个字符")]
        public string AccountName { get; set; }

        /// <summary>
        /// 账户号/卡号
        /// </summary>
        [Required(ErrorMessage = "请输入账户号码")]
        [StringLength(100, ErrorMessage = "账户号码不能超过100个字符")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行名称（仅银行卡需要）
        /// </summary>
        [StringLength(100, ErrorMessage = "银行名称不能超过100个字符")]
        public string BankName { get; set; }

        /// <summary>
        /// 是否设为默认账户
        /// </summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// 充值视图模型
    /// </summary>
    public class RechargeViewModel
    {
        /// <summary>
        /// 充值金额
        /// </summary>
        [Required(ErrorMessage = "请输入充值金额")]
        [Range(0.01, 100000, ErrorMessage = "充值金额必须在0.01到100000之间")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付账户ID
        /// </summary>
        [Required(ErrorMessage = "请选择支付账户")]
        public int PaymentAccountID { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 提现视图模型
    /// </summary>
    public class WithdrawViewModel
    {
        /// <summary>
        /// 提现金额
        /// </summary>
        [Required(ErrorMessage = "请输入提现金额")]
        [Range(0.01, 100000, ErrorMessage = "提现金额必须在0.01到100000之间")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 提现到的支付账户ID
        /// </summary>
        [Required(ErrorMessage = "请选择提现账户")]
        public int PaymentAccountID { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks { get; set; }
    }
}
