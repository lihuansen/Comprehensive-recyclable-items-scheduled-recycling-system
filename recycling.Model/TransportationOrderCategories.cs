namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TransportationOrderCategories
    {
        [Key]
        public int CategoryID { get; set; }

        public int? TransportOrderID { get; set; }

        [StringLength(50)]
        public string CategoryKey { get; set; }

        [StringLength(50)]
        public string CategoryName { get; set; }

        public decimal? Weight { get; set; }

        public decimal? PricePerKg { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
