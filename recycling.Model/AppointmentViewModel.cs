using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    /// <summary>
    /// 预约上门视图模型
    /// </summary>
    public class AppointmentViewModel
    {
        [Display(Name = "预约类型")]
        [Required(ErrorMessage = "请选择预约类型")]
        public string AppointmentType { get; set; }

        [Display(Name = "回收品类")]
        [Required(ErrorMessage = "请选择回收品类")]
        public string Category { get; set; }

        [Display(Name = "预估重量")]
        [Required(ErrorMessage = "请输入预估重量")]
        [Range(0.1, 1000, ErrorMessage = "重量必须在0.1-1000公斤之间")]
        public decimal EstimatedWeight { get; set; }

        [Display(Name = "预约日期")]
        [Required(ErrorMessage = "请选择预约日期")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "预约时间段")]
        [Required(ErrorMessage = "请选择时间段")]
        public string TimeSlot { get; set; }

        [Display(Name = "详细地址")]
        [Required(ErrorMessage = "请输入详细地址")]
        [StringLength(200, ErrorMessage = "地址长度不能超过200个字符")]
        public string Address { get; set; }

        [Display(Name = "联系人姓名")]
        [Required(ErrorMessage = "请输入联系人姓名")]
        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string ContactName { get; set; }

        [Display(Name = "联系电话")]
        [Required(ErrorMessage = "请输入联系电话")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号码")]
        public string ContactPhone { get; set; }

        [Display(Name = "特殊要求")]
        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string SpecialRequirements { get; set; }

        [Display(Name = "图片上传")]
        public List<HttpPostedFileBase> Images { get; set; }

        // 可选属性
        [Display(Name = "是否紧急")]
        public bool IsUrgent { get; set; }

        [Display(Name = "期望价格")]
        [Range(0, 100000, ErrorMessage = "价格必须在0-100000元之间")]
        public decimal? ExpectedPrice { get; set; }
    }
    /// <summary>
    /// 预约类型选项
    /// </summary>
    public static class AppointmentTypes
    {
        public const string Household = "household";
        public const string Enterprise = "enterprise";

        public static readonly Dictionary<string, string> AllTypes = new Dictionary<string, string>
        {
            { Household, "家庭回收" },
            { Enterprise, "企业回收" }
        };
    }

    /// <summary>
    /// 回收品类选项
    /// </summary>
    public static class RecyclingCategories
    {
        public const string Paper = "paper";
        public const string Plastic = "plastic";
        public const string Metal = "metal";
        public const string Glass = "glass";
        public const string Fabric = "fabric";

        public static readonly Dictionary<string, string> AllCategories = new Dictionary<string, string>
        {
            { Paper, "纸张类" },
            { Plastic, "塑料类" },
            { Metal, "金属类" },
            { Glass, "玻璃类" },
            { Fabric, "纺织品类" }
        };
    }

    /// <summary>
    /// 时间段选项
    /// </summary>
    public static class TimeSlots
    {
        public static readonly Dictionary<string, string> AllSlots = new Dictionary<string, string>
        {
            { "morning", "上午 9:00-12:00" },
            { "afternoon", "下午 13:00-17:00" },
            { "evening", "晚上 18:00-21:00" },
            { "all_day", "全天 9:00-21:00" }
        };
    }
}
