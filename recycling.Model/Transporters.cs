namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Transporters
    {
        [Key]
        public int TransporterID { get; set; }

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

        [StringLength(50)]
        public string VehicleType { get; set; }

        [Required]
        [StringLength(20)]
        public string VehiclePlateNumber { get; set; }

        public decimal? VehicleCapacity { get; set; }

        [StringLength(50)]
        public string LicenseNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Region { get; set; }

        public bool Available { get; set; }

        [Required]
        [StringLength(20)]
        public string CurrentStatus { get; set; }

        public int TotalTrips { get; set; }

        public decimal TotalWeight { get; set; }

        public decimal? Rating { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(255)]
        public string AvatarURL { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public decimal? money { get; set; }
    }
}
