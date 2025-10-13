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
    /// 预约上门视图模型 - 第一步
    /// </summary>
    public class AppointmentViewModel
    {
        [Display(Name = "预约类型")]
        [Required(ErrorMessage = "请选择预约类型")]
        public string AppointmentType { get; set; }

        [Display(Name = "回收品类")]
        [Required(ErrorMessage = "请至少选择一个回收品类")]
        public List<string> SelectedCategories { get; set; } = new List<string>();

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

        [Display(Name = "是否紧急")]
        public bool IsUrgent { get; set; }
    }

    /// <summary>
    /// 品类详情视图模型 - 第二步
    /// </summary>
    public class CategoryDetailViewModel
    {
        public AppointmentViewModel BasicInfo { get; set; }
        public Dictionary<string, CategoryQuestions> CategoryQuestions { get; set; } = new Dictionary<string, CategoryQuestions>();
        public decimal EstimatedPrice { get; set; }
    }

    /// <summary>
    /// 品类问题
    /// </summary>
    public class CategoryQuestions
    {
        public string CategoryName { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    /// <summary>
    /// 问题定义
    /// </summary>
    public class Question
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Type { get; set; } // "radio", "checkbox"
        public List<Option> Options { get; set; } = new List<Option>();
        public string SelectedValue { get; set; }
        public decimal Weight { get; set; } // 价格权重
    }

    /// <summary>
    /// 选项
    /// </summary>
    public class Option
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public decimal PriceEffect { get; set; } // 价格影响
    }

    // 其他静态类保持不变...
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

    public static class RecyclingCategories
    {
        public const string Glass = "glass";
        public const string Metal = "metal";
        public const string Plastic = "plastic";
        public const string Paper = "paper";
        public const string Fabric = "fabric";

        public static readonly Dictionary<string, string> AllCategories = new Dictionary<string, string>
        {
            { Glass, "玻璃" },
            { Metal, "金属" },
            { Plastic, "塑料" },
            { Paper, "纸类" },
            { Fabric, "纺织品" }
        };

        // 每个品类的问题定义
        public static Dictionary<string, CategoryQuestions> GetCategoryQuestions()
        {
            return new Dictionary<string, CategoryQuestions>
            {
                {
                    Glass, new CategoryQuestions
                    {
                        CategoryName = "玻璃",
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = "glass_condition",
                                Text = "玻璃瓶子是否完整？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "complete", Text = "完整", PriceEffect = 1.0m },
                                    new Option { Value = "broken", Text = "有破损", PriceEffect = 0.6m }
                                }
                            },
                            new Question
                            {
                                Id = "glass_type",
                                Text = "玻璃类型？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "bottle", Text = "瓶子", PriceEffect = 1.0m },
                                    new Option { Value = "window", Text = "窗户玻璃", PriceEffect = 0.8m },
                                    new Option { Value = "other", Text = "其他", PriceEffect = 0.5m }
                                }
                            },
                            new Question
                            {
                                Id = "glass_clean",
                                Text = "是否清洁干净？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "是", PriceEffect = 1.0m },
                                    new Option { Value = "dirty", Text = "否", PriceEffect = 0.7m }
                                }
                            }
                        }
                    }
                },
                {
                    Metal, new CategoryQuestions
                    {
                        CategoryName = "金属",
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = "metal_type",
                                Text = "金属类型？",
                                Type = "radio",
                                Weight = 0.5m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "iron", Text = "铁制品", PriceEffect = 1.0m },
                                    new Option { Value = "aluminum", Text = "铝制品", PriceEffect = 1.2m },
                                    new Option { Value = "copper", Text = "铜制品", PriceEffect = 1.5m },
                                    new Option { Value = "mixed", Text = "混合金属", PriceEffect = 0.8m }
                                }
                            },
                            new Question
                            {
                                Id = "metal_condition",
                                Text = "金属状况？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "good", Text = "状况良好", PriceEffect = 1.0m },
                                    new Option { Value = "rusty", Text = "有锈迹", PriceEffect = 0.7m },
                                    new Option { Value = "broken", Text = "破损严重", PriceEffect = 0.5m }
                                }
                            },
                            new Question
                            {
                                Id = "metal_clean",
                                Text = "是否清洁？",
                                Type = "radio",
                                Weight = 0.2m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "是", PriceEffect = 1.0m },
                                    new Option { Value = "dirty", Text = "否", PriceEffect = 0.8m }
                                }
                            }
                        }
                    }
                },
                {
                    Plastic, new CategoryQuestions
                    {
                        CategoryName = "塑料",
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = "plastic_type",
                                Text = "塑料类型？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "pet", Text = "PET（饮料瓶）", PriceEffect = 1.2m },
                                    new Option { Value = "hdpe", Text = "HDPE（洗发水瓶）", PriceEffect = 1.0m },
                                    new Option { Value = "pvc", Text = "PVC（管道）", PriceEffect = 0.8m },
                                    new Option { Value = "other", Text = "其他塑料", PriceEffect = 0.6m }
                                }
                            },
                            new Question
                            {
                                Id = "plastic_clean",
                                Text = "是否清洁？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "是", PriceEffect = 1.0m },
                                    new Option { Value = "dirty", Text = "否", PriceEffect = 0.7m }
                                }
                            },
                            new Question
                            {
                                Id = "plastic_color",
                                Text = "塑料颜色？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "transparent", Text = "透明", PriceEffect = 1.1m },
                                    new Option { Value = "colored", Text = "有色", PriceEffect = 1.0m },
                                    new Option { Value = "mixed", Text = "混合颜色", PriceEffect = 0.9m }
                                }
                            }
                        }
                    }
                },
                {
                    Paper, new CategoryQuestions
                    {
                        CategoryName = "纸类",
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = "paper_type",
                                Text = "纸张类型？",
                                Type = "radio",
                                Weight = 0.5m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "newspaper", Text = "报纸", PriceEffect = 1.0m },
                                    new Option { Value = "cardboard", Text = "纸板箱", PriceEffect = 1.1m },
                                    new Option { Value = "office", Text = "办公用纸", PriceEffect = 1.2m },
                                    new Option { Value = "mixed", Text = "混合纸张", PriceEffect = 0.8m }
                                }
                            },
                            new Question
                            {
                                Id = "paper_condition",
                                Text = "纸张状况？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "dry", Text = "干燥", PriceEffect = 1.0m },
                                    new Option { Value = "damp", Text = "潮湿", PriceEffect = 0.6m },
                                    new Option { Value = "soiled", Text = "污染", PriceEffect = 0.3m }
                                }
                            },
                            new Question
                            {
                                Id = "paper_clean",
                                Text = "是否干净？",
                                Type = "radio",
                                Weight = 0.2m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "是", PriceEffect = 1.0m },
                                    new Option { Value = "dirty", Text = "否", PriceEffect = 0.7m }
                                }
                            }
                        }
                    }
                },
                {
                    Fabric, new CategoryQuestions
                    {
                        CategoryName = "纺织品",
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = "fabric_type",
                                Text = "纺织品类型？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "cotton", Text = "棉织品", PriceEffect = 1.0m },
                                    new Option { Value = "wool", Text = "毛织品", PriceEffect = 1.1m },
                                    new Option { Value = "synthetic", Text = "化纤", PriceEffect = 0.8m },
                                    new Option { Value = "mixed", Text = "混合面料", PriceEffect = 0.9m }
                                }
                            },
                            new Question
                            {
                                Id = "fabric_condition",
                                Text = "纺织品状况？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "good", Text = "状况良好", PriceEffect = 1.0m },
                                    new Option { Value = "worn", Text = "有磨损", PriceEffect = 0.7m },
                                    new Option { Value = "damaged", Text = "破损", PriceEffect = 0.4m }
                                }
                            },
                            new Question
                            {
                                Id = "fabric_clean",
                                Text = "是否清洁？",
                                Type = "radio",
                                Weight = 0.2m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "是", PriceEffect = 1.0m },
                                    new Option { Value = "dirty", Text = "否", PriceEffect = 0.6m }
                                }
                            }
                        }
                    }
                }
            };
        }
    }

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

    // 基础价格表（元/公斤）
    public static class BasePrices
    {
        public static readonly Dictionary<string, decimal> Prices = new Dictionary<string, decimal>
        {
            { RecyclingCategories.Glass, 0.5m },
            { RecyclingCategories.Metal, 2.0m },
            { RecyclingCategories.Plastic, 1.2m },
            { RecyclingCategories.Paper, 0.8m },
            { RecyclingCategories.Fabric, 0.6m }
        };
    }
}
