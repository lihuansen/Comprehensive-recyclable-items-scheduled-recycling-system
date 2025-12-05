namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SortingCenterWorkers
    {
        [Key]
        public int WorkerID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(18)]
        public string IDNumber { get; set; }

        public int SortingCenterID { get; set; }

        [Required]
        [StringLength(100)]
        public string SortingCenterName { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        [StringLength(50)]
        public string WorkStation { get; set; }

        [StringLength(100)]
        public string Specialization { get; set; }

        [Required]
        [StringLength(20)]
        public string ShiftType { get; set; }

        public bool Available { get; set; }

        [Required]
        [StringLength(20)]
        public string CurrentStatus { get; set; }

        public int TotalItemsProcessed { get; set; }

        public decimal TotalWeightProcessed { get; set; }

        public decimal? AccuracyRate { get; set; }

        public decimal? Rating { get; set; }

        [Column(TypeName = "date")]
        public DateTime? HireDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(255)]
        public string AvatarURL { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
