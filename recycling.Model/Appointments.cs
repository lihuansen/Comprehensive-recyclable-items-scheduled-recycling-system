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

        public int? UserID { get; set; }

        [StringLength(50)]
        public string AppointmentType { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [StringLength(50)]
        public string TimeSlot { get; set; }

        public decimal? EstimatedWeight { get; set; }

        public bool? IsUrgent { get; set; }

        public string Address { get; set; }

        [StringLength(50)]
        public string ContactName { get; set; }

        [StringLength(50)]
        public string ContactPhone { get; set; }

        public string SpecialInstructions { get; set; }

        public decimal? EstimatedPrice { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? RecyclerID { get; set; }

        /// <summary>
        /// 回收员回退订单的原因（当状态为"已取消-回收员回退"时有值）
        /// </summary>
        [StringLength(500)]
        public string RollbackReason { get; set; }
    }
}
