namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AppointmentCategories
    {
        [Key]
        public int CategoryID { get; set; }

        public int AppointmentID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryKey { get; set; }

        public string QuestionsAnswers { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }= DateTime.Now;
    }
}
