using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Status { get; set; } = "待处理";

        [StringLength(1000)]
        public string AdminReply { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }

        // 非数据库字段，用于显示
        [NotMapped]
        public string UserName { get; set; }
    }
}
