namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 钱包交易记录实体类
    /// 用于存储所有钱包相关的交易记录
    /// </summary>
    public partial class WalletTransaction
    {
        /// <summary>
        /// 交易ID（主键）
        /// </summary>
        [Key]
        public int TransactionID { get; set; }

        /// <summary>
        /// 用户ID（外键）
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// 交易类型：Recharge(充值), Withdraw(提现), Payment(支付), Refund(退款), Income(收入)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 交易前余额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// 交易后余额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// 支付账户ID（外键，充值/提现时使用）
        /// </summary>
        public int? PaymentAccountID { get; set; }

        /// <summary>
        /// 关联订单ID（支付/退款时使用）
        /// </summary>
        public int? RelatedOrderID { get; set; }

        /// <summary>
        /// 交易状态：Pending(待处理), Processing(处理中), Completed(已完成), Failed(失败), Cancelled(已取消)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TransactionStatus { get; set; }

        /// <summary>
        /// 交易描述
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 交易流水号（唯一）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransactionNo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }

        /// <summary>
        /// 导航属性：关联的支付账户
        /// </summary>
        [ForeignKey("PaymentAccountID")]
        public virtual UserPaymentAccount PaymentAccount { get; set; }

        /// <summary>
        /// 获取交易类型的显示名称
        /// </summary>
        public string GetTransactionTypeDisplayName()
        {
            switch (TransactionType)
            {
                case "Recharge":
                    return "充值";
                case "Withdraw":
                    return "提现";
                case "Payment":
                    return "支付";
                case "Refund":
                    return "退款";
                case "Income":
                    return "收入";
                default:
                    return TransactionType;
            }
        }

        /// <summary>
        /// 获取交易状态的显示名称
        /// </summary>
        public string GetTransactionStatusDisplayName()
        {
            switch (TransactionStatus)
            {
                case "Pending":
                    return "待处理";
                case "Processing":
                    return "处理中";
                case "Completed":
                    return "已完成";
                case "Failed":
                    return "失败";
                case "Cancelled":
                    return "已取消";
                default:
                    return TransactionStatus;
            }
        }

        /// <summary>
        /// 判断是否为收入类交易（充值、退款、收入）
        /// </summary>
        public bool IsIncome()
        {
            return TransactionType == "Recharge" || 
                   TransactionType == "Refund" || 
                   TransactionType == "Income";
        }

        /// <summary>
        /// 判断是否为支出类交易（提现、支付）
        /// </summary>
        public bool IsExpense()
        {
            return TransactionType == "Withdraw" || 
                   TransactionType == "Payment";
        }
    }
}
