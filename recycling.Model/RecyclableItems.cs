namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RecyclableItems
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Category { get; set; }

        [Required]
        [StringLength(20)]
        public string CategoryName { get; set; }

        public decimal PricePerKg { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
