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

        [StringLength(50)]
        public string ReceiptNumber { get; set; }

        public int? TransportOrderID { get; set; }

        public int? RecyclerID { get; set; }

        public int? WorkerID { get; set; }

        public decimal? TotalWeight { get; set; }

        public string ItemCategories { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public decimal? Price { get; set; }
    }
}
