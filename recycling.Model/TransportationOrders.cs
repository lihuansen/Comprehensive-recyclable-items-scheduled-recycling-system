namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 运输单实体类
    /// Transportation Orders Entity
    /// 用于存储从回收员暂存点到基地的运输单信息
    /// </summary>
    public partial class TransportationOrders
    {
        /// <summary>
        /// 运输单ID（主键）
        /// </summary>
        [Key]
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 运输单号（唯一，格式：TO+年月日+序号）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        /// <summary>
        /// 回收员ID（外键）
        /// </summary>
        [Required]
        public int RecyclerID { get; set; }

        /// <summary>
        /// 运输人员ID（外键）
        /// </summary>
        [Required]
        public int TransporterID { get; set; }

        /// <summary>
        /// 取货地址（回收员暂存点地址）
        /// </summary>
        [Required]
        [StringLength(200)]
        public string PickupAddress { get; set; }

        /// <summary>
        /// 目的地地址（基地地址）
        /// </summary>
        [Required]
        [StringLength(200)]
        public string DestinationAddress { get; set; }

        /// <summary>
        /// 联系人（回收员姓名）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ContactPerson { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 预估总重量（kg）
        /// </summary>
        [Required]
        public decimal EstimatedWeight { get; set; }

        /// <summary>
        /// 实际重量（kg）
        /// </summary>
        public decimal? ActualWeight { get; set; }

        /// <summary>
        /// 物品类别（JSON格式存储）
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 特殊说明
        /// </summary>
        [StringLength(500)]
        public string SpecialInstructions { get; set; }

        /// <summary>
        /// 状态（待接单、已接单、运输中、已完成、已取消）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 接单时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? AcceptedDate { get; set; }

        /// <summary>
        /// 取货时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? PickupDate { get; set; }

        /// <summary>
        /// 送达时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// 取消时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// 取消原因
        /// </summary>
        [StringLength(200)]
        public string CancelReason { get; set; }

        /// <summary>
        /// 运输人员备注
        /// </summary>
        [StringLength(500)]
        public string TransporterNotes { get; set; }

        /// <summary>
        /// 回收员评分（1-5）
        /// </summary>
        public int? RecyclerRating { get; set; }

        /// <summary>
        /// 回收员评价
        /// </summary>
        [StringLength(500)]
        public string RecyclerReview { get; set; }

        /// <summary>
        /// 导航属性 - 回收员
        /// </summary>
        [ForeignKey("RecyclerID")]
        public virtual Recyclers Recycler { get; set; }

        /// <summary>
        /// 导航属性 - 运输人员
        /// </summary>
        [ForeignKey("TransporterID")]
        public virtual Transporters Transporter { get; set; }
    }
}
