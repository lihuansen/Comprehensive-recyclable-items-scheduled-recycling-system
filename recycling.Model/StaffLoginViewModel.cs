using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 工作人员登录视图模型（与用户登录模型保持一致，仅增加角色选择）
    /// </summary>
    public class StaffLoginViewModel
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

        /// <summary>
        /// 服务器生成的验证码（用于与用户输入的Captcha比对）
        /// </summary>
        public string GeneratedCaptcha { get; set; }

        /// <summary>
        /// 工作人员角色（回收员/管理员/超级管理员）- 仅此处与用户登录不同
        /// </summary>
        [Required(ErrorMessage = "请选择角色")]
        [Display(Name = "工作人员角色")]
        public string StaffRole { get; set; } // 值对应：recycler/admin/superadmin
    }
}
