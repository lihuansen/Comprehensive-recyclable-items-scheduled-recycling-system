using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 反馈提交视图模型
    /// </summary>
    public class FeedbackSubmissionViewModel
    {
        [Required(ErrorMessage = "请选择反馈类型")]
        public string FeedbackType { get; set; }

        [Required(ErrorMessage = "请输入反馈主题")]
        [StringLength(100, ErrorMessage = "反馈主题不能超过100字")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "请输入详细描述")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "详细描述长度应在10-1000字之间")]
        public string Description { get; set; }

        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [StringLength(100, ErrorMessage = "邮箱不能超过100字")]
        public string ContactEmail { get; set; }
    }
}
