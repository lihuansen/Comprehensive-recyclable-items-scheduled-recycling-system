using System;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 运输人员个人信息编辑视图模型
    /// 根据需求移除了车辆类型和车辆载重字段
    /// </summary>
    public class TransporterProfileViewModel
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

        [Required(ErrorMessage = "车牌号不能为空")]
        [StringLength(20, ErrorMessage = "车牌号长度不能超过20个字符")]
        [Display(Name = "车牌号")]
        public string VehiclePlateNumber { get; set; }

        [StringLength(50, ErrorMessage = "驾驶证号长度不能超过50个字符")]
        [Display(Name = "驾驶证号")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "区域不能为空")]
        [StringLength(100, ErrorMessage = "区域长度不能超过100个字符")]
        [Display(Name = "区域")]
        public string Region { get; set; }
    }
}
