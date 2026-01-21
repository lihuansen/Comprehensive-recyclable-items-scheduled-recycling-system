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

        public int? OrderID { get; set; }

        public int? UserID { get; set; }

        public int? RecyclerID { get; set; }

        public int? StarRating { get; set; }

        public string ReviewText { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }
}
