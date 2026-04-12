using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    // 中文注释
    /// 登录视图模型
    // 中文注释
    public class LoginViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入验证码")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "验证码必须为4位")]
        [Display(Name = "验证码")]
        public string Captcha { get; set; }

        // 中文注释
        /// 前端生成的验证码（用于后端验证）
        // 中文注释
        public string GeneratedCaptcha { get; set; }
    }
}
