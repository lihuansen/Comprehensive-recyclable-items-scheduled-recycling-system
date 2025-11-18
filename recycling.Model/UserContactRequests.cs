namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserContactRequests
    {
        [Key]
        public int RequestID { get; set; }

        public int? UserID { get; set; }

        [StringLength(50)]
        public string RequestStatus { get; set; }

        public DateTime? RequestTime { get; set; }

        public DateTime? ContactedTime { get; set; }

        public int? AdminID { get; set; }
    }
}
