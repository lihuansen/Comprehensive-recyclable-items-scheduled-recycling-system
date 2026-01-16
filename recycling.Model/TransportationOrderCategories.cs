namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 运输单品类明细模型
    /// Transportation Order Category Detail Model
    /// 用途：存储运输单的品类详细信息，确保品类、重量、价格、金额对齐
    /// </summary>
    public partial class TransportationOrderCategories
    {
        [Key]
        public int CategoryID { get; set; }

        public int TransportOrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryKey { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        public decimal Weight { get; set; }

        public decimal PricePerKg { get; set; }

        public decimal TotalAmount { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }
    }
}
