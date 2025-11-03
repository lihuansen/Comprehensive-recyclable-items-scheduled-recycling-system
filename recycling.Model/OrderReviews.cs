namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class OrderReviews
    {
        [Key]
        public int ReviewID { get; set; }

        public int OrderID { get; set; }

        public int UserID { get; set; }

        public int RecyclerID { get; set; }

        [Range(1, 5)]
        public int StarRating { get; set; }

        [StringLength(500)]
        public string ReviewText { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
