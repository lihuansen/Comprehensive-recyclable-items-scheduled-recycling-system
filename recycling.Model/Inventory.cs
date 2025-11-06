namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        public int OrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryKey { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        public decimal Weight { get; set; }

        public decimal? Price { get; set; }

        public int RecyclerID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
