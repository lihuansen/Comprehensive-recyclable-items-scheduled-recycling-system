namespace recycling.Model
{
using System;
    using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace recycling.Model
{
    [Table("UserFeedback")]
    public partial class UserFeedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string FeedbackType { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ContactEmail { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "´ı´¦Àí";

        [StringLength(1000)]
        public string AdminReply { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }

        // éæ•°æ®åº“å­—æ®µï¼Œç”¨äºæ˜¾ç¤?
        [NotMapped]
        public string UserName { get; set; }
    }
}
