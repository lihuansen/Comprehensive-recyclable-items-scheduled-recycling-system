namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 入库单实体类
    /// Warehouse Receipt Entity Class
    /// </summary>
    public partial class WarehouseReceipts
    {
        /// <summary>
        /// 入库单ID（主键）
        /// </summary>
        [Key]
        public int ReceiptID { get; set; }

        /// <summary>
        /// 入库单号（格式：WR+YYYYMMDD+序号）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// 运输单ID（外键）
        /// </summary>
        [Required]
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 回收员ID（外键）
        /// </summary>
        [Required]
        public int RecyclerID { get; set; }

        /// <summary>
        /// 处理入库的基地人员ID（外键）
        /// </summary>
        [Required]
        public int WorkerID { get; set; }

        /// <summary>
        /// 入库总重量（kg）
        /// </summary>
        [Required]
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 物品类别（JSON格式）
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 状态（已入库、已取消）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }

        /// <summary>
        /// 创建时间（入库时间）
        /// </summary>
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 创建人（基地人员ID）
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }
    }
}
