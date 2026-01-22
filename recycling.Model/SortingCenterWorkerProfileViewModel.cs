using System;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 基地工作人员个人信息编辑视图模型
    /// </summary>
    public class SortingCenterWorkerProfileViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(100, ErrorMessage = "姓名长度不能超过100个字符")]
        [Display(Name = "姓名")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "手机号不能为空")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号")]
        [Display(Name = "手机号")]
        public string PhoneNumber { get; set; }

        [StringLength(18, ErrorMessage = "身份证号长度不能超过18个字符")]
        [Display(Name = "身份证号")]
        public string IDNumber { get; set; }

        [Required(ErrorMessage = "班次类型不能为空")]
        [Display(Name = "班次类型")]
        public string ShiftType { get; set; }
    }
}
