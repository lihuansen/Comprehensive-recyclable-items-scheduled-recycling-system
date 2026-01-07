namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用户支付账户实体类
    /// 用于存储用户绑定的支付账户（支付宝、微信、银行卡等）
    /// </summary>
    public partial class UserPaymentAccount
    {
        /// <summary>
        /// 账户ID（主键）
        /// </summary>
        [Key]
        public int AccountID { get; set; }

        /// <summary>
        /// 用户ID（外键）
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// 账户类型：Alipay(支付宝), WeChat(微信), BankCard(银行卡)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string AccountType { get; set; }

        /// <summary>
        /// 账户名称/持卡人姓名
        /// </summary>
        [Required]
        [StringLength(100)]
        public string AccountName { get; set; }

        /// <summary>
        /// 账户号/卡号（加密存储）
        /// </summary>
        [Required]
        [StringLength(100)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行名称（仅银行卡需要）
        /// </summary>
        [StringLength(100)]
        public string BankName { get; set; }

        /// <summary>
        /// 是否默认账户
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 是否已验证
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? LastUsedDate { get; set; }

        /// <summary>
        /// 状态：Active(激活), Suspended(暂停), Deleted(已删除)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }

        /// <summary>
        /// 获取账户类型的显示名称
        /// </summary>
        public string GetAccountTypeDisplayName()
        {
            switch (AccountType)
            {
                case "Alipay":
                    return "支付宝";
                case "WeChat":
                    return "微信支付";
                case "BankCard":
                    return "银行卡";
                default:
                    return AccountType;
            }
        }

        /// <summary>
        /// 获取脱敏后的账户号码（用于显示）
        /// </summary>
        public string GetMaskedAccountNumber()
        {
            if (string.IsNullOrEmpty(AccountNumber))
                return "";

            if (AccountNumber.Length <= 4)
                return AccountNumber;

            // 显示前4位和后4位，中间用星号代替
            int visibleChars = 4;
            if (AccountNumber.Length <= 8)
                visibleChars = 2;

            string prefix = AccountNumber.Substring(0, visibleChars);
            string suffix = AccountNumber.Substring(AccountNumber.Length - visibleChars);
            int maskLength = AccountNumber.Length - (visibleChars * 2);
            string mask = new string('*', maskLength);

            return prefix + mask + suffix;
        }
    }
}
