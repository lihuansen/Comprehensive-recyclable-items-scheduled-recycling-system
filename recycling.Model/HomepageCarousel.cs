namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HomepageCarousel")]
    public partial class HomepageCarousel
    {
        [Key]
        public int CarouselID { get; set; }

        [StringLength(50)]
        public string MediaType { get; set; }

        public string MediaUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
