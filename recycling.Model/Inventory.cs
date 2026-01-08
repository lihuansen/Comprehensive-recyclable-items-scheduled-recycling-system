namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Inventory")]
    public partial class Inventory
    {
        public int InventoryID { get; set; }

        public int OrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryKey { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        public decimal Weight { get; set; }

        public int RecyclerID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public decimal? Price { get; set; }

        [Required]
        [StringLength(20)]
        public string InventoryType { get; set; } = "StoragePoint";
    }
}
