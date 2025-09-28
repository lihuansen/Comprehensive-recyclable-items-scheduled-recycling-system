using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 忘记密码视图模型
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [Display(Name = "手机号")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Required]
        [Display(Name = "验证码")]
        public string VerificationCode { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        [Required]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        /// <summary>
        /// 确认新密码
        /// </summary>
        [Required]
        [Display(Name = "确认新密码")]
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>
    /// 验证码存储模型（用于服务器端临时存储验证码）
    /// </summary>
    public class VerificationCodeModel
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}
