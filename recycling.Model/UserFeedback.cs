namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserFeedback")]
    public partial class UserFeedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public int? UserID { get; set; }

        [StringLength(50)]
        public string FeedbackType { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string ContactEmail { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public string AdminReply { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
