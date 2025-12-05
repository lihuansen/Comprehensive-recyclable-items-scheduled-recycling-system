namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// 运输人员实体类
    /// 负责将回收物品从回收员处运输到分拣中心
    /// </summary>
    public partial class Transporters
    {
        /// <summary>
        /// 运输人员ID（主键，自增）
        /// </summary>
        [Key]
        public int TransporterID { get; set; }

        /// <summary>
        /// 用户名（唯一）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// 密码哈希（SHA256）
        /// </summary>
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [StringLength(18)]
        public string IDNumber { get; set; }

        /// <summary>
        /// 车辆类型（如：小型货车、中型货车、大型货车）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; }

        /// <summary>
        /// 车牌号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string VehiclePlateNumber { get; set; }

        /// <summary>
        /// 车辆载重能力（单位：kg）
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "车辆载重能力必须大于0")]
        public decimal? VehicleCapacity { get; set; }

        /// <summary>
        /// 驾驶证号
        /// </summary>
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        /// <summary>
        /// 负责区域
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Region { get; set; }

        /// <summary>
        /// 是否可接任务（1=可用，0=忙碌）
        /// </summary>
        public bool Available { get; set; }

        /// <summary>
        /// 当前状态（空闲、运输中、休息、离线）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string CurrentStatus { get; set; }

        /// <summary>
        /// 总运输次数
        /// </summary>
        public int TotalTrips { get; set; }

        /// <summary>
        /// 总运输重量（kg）
        /// </summary>
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 评分（0-5）
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// 是否激活（1=激活，0=禁用）
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        [StringLength(255)]
        public string AvatarURL { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }
}
