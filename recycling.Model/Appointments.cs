namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Appointments
    {
        [Key]
        public int AppointmentID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string AppointmentType { get; set; }

        [Column(TypeName = "date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string TimeSlot { get; set; }

        public decimal EstimatedWeight { get; set; }

        public bool IsUrgent { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactName { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^1[3-9]\d{9}$")]
        public string ContactPhone { get; set; }

        [StringLength(500)]
        public string SpecialInstructions { get; set; }

        public decimal? EstimatedPrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "´ýÈ·ÈÏ";

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
