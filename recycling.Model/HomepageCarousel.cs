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

        [Required]
        [StringLength(20)]
        public string MediaType { get; set; }

        [Required]
        [StringLength(500)]
        public string MediaUrl { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }
    }
}
