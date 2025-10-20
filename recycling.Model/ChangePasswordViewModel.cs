using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "请输入当前密码")]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "请输入新密码")]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "请确认新密码")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
        [Display(Name = "确认新密码")]
        public string ConfirmNewPassword { get; set; }
    }
}
