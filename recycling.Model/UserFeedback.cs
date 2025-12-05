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
        public string FeedbackType { get; set; }  // 问题反馈、功能建议、投诉举报、其他
        
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
        public string Status { get; set; }  // 反馈中、已完成
        
        [StringLength(1000)]
        public string AdminReply { get; set; }
        
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }
        
        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }
    }
}
