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

        public int? StoragePointID { get; set; }

        public string ProductName { get; set; }

        public int? Quantity { get; set; }

        public DateTime? EntryDate { get; set; }

        public string WarehouseLocation { get; set; }

        public string Manager { get; set; }
    }
}
