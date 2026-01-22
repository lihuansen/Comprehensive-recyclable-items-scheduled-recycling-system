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

        [StringLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        public string IDNumber { get; set; }

        public string Region { get; set; }

        public bool? Available { get; set; }

        [StringLength(50)]
        public string CurrentStatus { get; set; }

        public decimal? TotalWeight { get; set; }

        public decimal? Rating { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public decimal? money { get; set; }

        [StringLength(50)]
        public string VehicleType { get; set; }

        [StringLength(50)]
        public string VehiclePlateNumber { get; set; }

        public decimal? VehicleCapacity { get; set; }
    }
}
