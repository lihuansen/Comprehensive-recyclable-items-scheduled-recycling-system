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

        [Display(Name = "选择地址")]
        public int? SelectedAddressID { get; set; }

        [Display(Name = "街道")]
        public string Street { get; set; }

        [Display(Name = "详细地址")]
        [StringLength(200, ErrorMessage = "地址长度不能超过200个字符")]
        public string Address { get; set; }

        [Display(Name = "联系人姓名")]
        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string ContactName { get; set; }

        [Display(Name = "联系电话")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的手机号码")]
        public string ContactPhone { get; set; }

        [Display(Name = "特殊要求")]
        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string SpecialRequirements { get; set; }

        [Display(Name = "图片上传")]
        public List<HttpPostedFileBase> Images { get; set; }

        [Display(Name = "是否紧急")]
        public bool IsUrgent { get; set; }

        [Display(Name = "类别重量")]
        public Dictionary<string, decimal> CategoryWeights { get; set; } = new Dictionary<string, decimal>();
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

        // 每个品类的问题定义 - 针对大类的通用问题，不涉及具体小类
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
                                Text = "玻璃物品整体状况如何？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "complete", Text = "完整无损", PriceEffect = 1.0m },
                                    new Option { Value = "minor_damage", Text = "轻微破损", PriceEffect = 0.8m },
                                    new Option { Value = "broken", Text = "严重破损", PriceEffect = 0.5m }
                                }
                            },
                            new Question
                            {
                                Id = "glass_clean",
                                Text = "玻璃物品是否已清洁处理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "已清洁", PriceEffect = 1.0m },
                                    new Option { Value = "partial", Text = "部分清洁", PriceEffect = 0.85m },
                                    new Option { Value = "dirty", Text = "未清洁", PriceEffect = 0.7m }
                                }
                            },
                            new Question
                            {
                                Id = "glass_quantity",
                                Text = "玻璃物品的数量规模？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "small", Text = "少量（10件以下）", PriceEffect = 0.9m },
                                    new Option { Value = "medium", Text = "中等（10-50件）", PriceEffect = 1.0m },
                                    new Option { Value = "large", Text = "大量（50件以上）", PriceEffect = 1.1m }
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
                                Id = "metal_condition",
                                Text = "金属物品整体状况如何？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "good", Text = "状况良好", PriceEffect = 1.0m },
                                    new Option { Value = "rusty", Text = "有锈迹/氧化", PriceEffect = 0.7m },
                                    new Option { Value = "damaged", Text = "损坏/变形", PriceEffect = 0.5m }
                                }
                            },
                            new Question
                            {
                                Id = "metal_clean",
                                Text = "金属物品是否已清洁处理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "已清洁", PriceEffect = 1.0m },
                                    new Option { Value = "partial", Text = "部分清洁", PriceEffect = 0.9m },
                                    new Option { Value = "dirty", Text = "未清洁", PriceEffect = 0.8m }
                                }
                            },
                            new Question
                            {
                                Id = "metal_purity",
                                Text = "金属物品的纯度情况？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "pure", Text = "单一金属", PriceEffect = 1.2m },
                                    new Option { Value = "mostly_pure", Text = "以某种金属为主", PriceEffect = 1.0m },
                                    new Option { Value = "mixed", Text = "混合金属", PriceEffect = 0.8m }
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
                                Id = "plastic_condition",
                                Text = "塑料物品整体状况如何？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "good", Text = "完好无损", PriceEffect = 1.0m },
                                    new Option { Value = "worn", Text = "有使用痕迹", PriceEffect = 0.85m },
                                    new Option { Value = "damaged", Text = "破损/老化", PriceEffect = 0.6m }
                                }
                            },
                            new Question
                            {
                                Id = "plastic_clean",
                                Text = "塑料物品是否已清洁处理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "已清洁", PriceEffect = 1.0m },
                                    new Option { Value = "partial", Text = "部分清洁", PriceEffect = 0.85m },
                                    new Option { Value = "dirty", Text = "未清洁", PriceEffect = 0.7m }
                                }
                            },
                            new Question
                            {
                                Id = "plastic_sorted",
                                Text = "塑料物品是否已分类整理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "sorted", Text = "已分类", PriceEffect = 1.1m },
                                    new Option { Value = "partial", Text = "部分分类", PriceEffect = 1.0m },
                                    new Option { Value = "unsorted", Text = "未分类", PriceEffect = 0.9m }
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
                                Id = "paper_condition",
                                Text = "纸类物品整体状况如何？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "dry", Text = "干燥完好", PriceEffect = 1.0m },
                                    new Option { Value = "damp", Text = "有潮湿", PriceEffect = 0.6m },
                                    new Option { Value = "soiled", Text = "有污染/破损", PriceEffect = 0.3m }
                                }
                            },
                            new Question
                            {
                                Id = "paper_clean",
                                Text = "纸类物品是否干净无油污？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "干净无污", PriceEffect = 1.0m },
                                    new Option { Value = "minor", Text = "轻微污渍", PriceEffect = 0.8m },
                                    new Option { Value = "dirty", Text = "有明显污渍", PriceEffect = 0.5m }
                                }
                            },
                            new Question
                            {
                                Id = "paper_sorted",
                                Text = "纸类物品是否已分类整理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "sorted", Text = "已分类整理", PriceEffect = 1.1m },
                                    new Option { Value = "partial", Text = "部分整理", PriceEffect = 1.0m },
                                    new Option { Value = "unsorted", Text = "未整理", PriceEffect = 0.9m }
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
                                Id = "fabric_condition",
                                Text = "纺织品整体状况如何？",
                                Type = "radio",
                                Weight = 0.4m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "good", Text = "状况良好", PriceEffect = 1.0m },
                                    new Option { Value = "worn", Text = "有磨损/褪色", PriceEffect = 0.7m },
                                    new Option { Value = "damaged", Text = "破损严重", PriceEffect = 0.4m }
                                }
                            },
                            new Question
                            {
                                Id = "fabric_clean",
                                Text = "纺织品是否已清洗？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "clean", Text = "已清洗", PriceEffect = 1.0m },
                                    new Option { Value = "partial", Text = "部分清洗", PriceEffect = 0.8m },
                                    new Option { Value = "dirty", Text = "未清洗", PriceEffect = 0.6m }
                                }
                            },
                            new Question
                            {
                                Id = "fabric_sorted",
                                Text = "纺织品是否已分类整理？",
                                Type = "radio",
                                Weight = 0.3m,
                                Options = new List<Option>
                                {
                                    new Option { Value = "sorted", Text = "已分类整理", PriceEffect = 1.1m },
                                    new Option { Value = "partial", Text = "部分整理", PriceEffect = 1.0m },
                                    new Option { Value = "unsorted", Text = "未整理", PriceEffect = 0.9m }
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

    public static class Streets
    {
        public static readonly Dictionary<string, string> LuohuStreets = new Dictionary<string, string>
        {
            { "guiyuan", "桂园街道" },
            { "huangbei", "黄贝街道" },
            { "dongmen", "东门街道" },
            { "cuizhu", "翠竹街道" },
            { "dongxiao", "东晓街道" },
            { "nanhu", "南湖街道" },
            { "sungang", "笋岗街道" },
            { "donghu", "东湖街道" },
            { "liantang", "莲塘街道" },
            { "qingshuihe", "清水河街道" }
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
