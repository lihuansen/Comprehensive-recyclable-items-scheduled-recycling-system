namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class WarehouseReceipts
    {
        [Key]
        public int ReceiptID { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; }

        [Required]
        public int TransportOrderID { get; set; }

        [Required]
        public int RecyclerID { get; set; }

        [Required]
        public int WorkerID { get; set; }

        [Required]
        public decimal TotalWeight { get; set; }

        public string ItemCategories { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public decimal? price { get; set; }
    }
}
