using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 工作人员登录视图模型（绑定登录表单数据）
    /// </summary>
    public class StaffLoginViewModel
    {
        /// <summary>
        /// 登录用户名（与数据库中各角色表的Username对应）
        /// </summary>
        [Required(ErrorMessage = "请输入工作人员账号")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        /// <summary>
        /// 登录密码（明文，提交后在服务端进行哈希验证）
        /// </summary>
        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        /// <summary>
        /// 用户输入的验证码
        /// </summary>
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        [MaxLength(4, ErrorMessage = "验证码长度为4位")]
        public string Captcha { get; set; }

        /// <summary>
        /// 服务器生成的验证码（用于与用户输入的Captcha比对）
        /// </summary>
        [Required]
        public string GeneratedCaptcha { get; set; }

        /// <summary>
        /// 工作人员角色（回收员/管理员/超级管理员）
        /// </summary>
        [Required(ErrorMessage = "请选择角色")]
        [Display(Name = "工作人员角色")]
        public string StaffRole { get; set; } // 值对应：collector/admin/superadmin
    }
}
