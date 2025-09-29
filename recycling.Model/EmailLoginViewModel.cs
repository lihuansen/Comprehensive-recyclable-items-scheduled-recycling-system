using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 邮箱登录视图模型
    /// </summary>
    public class EmailLoginViewModel
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[^\s]+@[^\s]+\.[^\s]+$", ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Required(ErrorMessage = "验证码不能为空")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "验证码必须为6位")]
        [Display(Name = "验证码")]
        public string VerificationCode { get; set; }
    }
}
