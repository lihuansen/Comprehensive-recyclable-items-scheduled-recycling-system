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

        [StringLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        public string IDNumber { get; set; }

        public int? SortingCenterID { get; set; }

        public string SortingCenterName { get; set; }

        public bool? Available { get; set; }

        [StringLength(50)]
        public string CurrentStatus { get; set; }

        public int? TotalItemsProcessed { get; set; }

        public decimal? TotalWeightProcessed { get; set; }

        public decimal? Rating { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public int? LastViewedTransportCount { get; set; }

        public int? LastViewedWarehouseCount { get; set; }

        public decimal? money { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(100)]
        public string WorkStation { get; set; }

        [StringLength(50)]
        public string ShiftType { get; set; }

        public string Specialization { get; set; }

        public DateTime? HireDate { get; set; }

        public decimal? AccuracyRate { get; set; }

        public string Notes { get; set; }

        public string AvatarURL { get; set; }
    }
}
