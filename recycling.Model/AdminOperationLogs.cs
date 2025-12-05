namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AdminOperationLogs
    {
        [Key]
        public int LogID { get; set; }

        public int AdminID { get; set; }

        [StringLength(50)]
        public string AdminUsername { get; set; }

        [Required]
        [StringLength(50)]
        public string Module { get; set; }

        [Required]
        [StringLength(50)]
        public string OperationType { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? TargetID { get; set; }

        [StringLength(100)]
        public string TargetName { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        public DateTime OperationTime { get; set; }

        [StringLength(20)]
        public string Result { get; set; }

        public string Details { get; set; }
    }
}
