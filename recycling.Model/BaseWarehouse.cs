namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BaseWarehouse")]
    public partial class BaseWarehouse
    {
        [Key]
        public int WarehouseID { get; set; }

        public int StoragePointID { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public DateTime EntryDate { get; set; }

        [StringLength(255)]
        public string WarehouseLocation { get; set; }

        [StringLength(255)]
        public string Manager { get; set; }
    }
}
