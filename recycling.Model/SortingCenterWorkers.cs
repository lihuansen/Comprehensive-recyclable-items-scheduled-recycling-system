namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// 分拣中心工作人员实体类
    /// 负责对运输到分拣中心的回收物品进行分类、质检和处理
    /// </summary>
    public partial class SortingCenterWorkers
    {
        /// <summary>
        /// 工作人员ID（主键，自增）
        /// </summary>
        [Key]
        public int WorkerID { get; set; }

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
        /// 所属分拣中心ID
        /// </summary>
        public int SortingCenterID { get; set; }

        /// <summary>
        /// 分拣中心名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SortingCenterName { get; set; }

        /// <summary>
        /// 职位（分拣员、质检员、组长、主管）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        /// <summary>
        /// 工位编号
        /// </summary>
        [StringLength(50)]
        public string WorkStation { get; set; }

        /// <summary>
        /// 专长品类（如：塑料、金属、纸类等）
        /// </summary>
        [StringLength(100)]
        public string Specialization { get; set; }

        /// <summary>
        /// 班次类型（白班、夜班、轮班）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ShiftType { get; set; }

        /// <summary>
        /// 是否可工作（1=可用，0=忙碌）
        /// </summary>
        public bool Available { get; set; }

        /// <summary>
        /// 当前状态（待命、分拣中、休息、离岗、离线）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string CurrentStatus { get; set; }

        /// <summary>
        /// 总处理物品数量
        /// </summary>
        public int TotalItemsProcessed { get; set; }

        /// <summary>
        /// 总处理重量（kg）
        /// </summary>
        public decimal TotalWeightProcessed { get; set; }

        /// <summary>
        /// 分拣准确率（百分比）
        /// </summary>
        public decimal? AccuracyRate { get; set; }

        /// <summary>
        /// 评分（0-5）
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// 入职日期
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime? HireDate { get; set; }

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
