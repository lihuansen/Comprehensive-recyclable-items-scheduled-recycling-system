using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 手机号登录视图模型
    /// </summary>
    public class PhoneLoginViewModel
    {
        [Required(ErrorMessage = "手机号不能为空")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号")]
        [Display(Name = "手机号")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "验证码不能为空")]
        [Display(Name = "验证码")]
        public string VerificationCode { get; set; }
    }
}
