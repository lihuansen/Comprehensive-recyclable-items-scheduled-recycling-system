using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 注册视图模型，用于前端表单提交和验证
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "请再次输入密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "请输入手机号")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入正确的手机号格式")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "请输入邮箱")]
        [EmailAddress(ErrorMessage = "请输入正确的邮箱格式")]
        public string Email { get; set; }
    }
}
