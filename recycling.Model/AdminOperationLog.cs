namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 管理员操作日志模型
    /// </summary>
    public partial class AdminOperationLog
    {
        [Key]
        public int LogID { get; set; }

        /// <summary>
        /// 操作管理员ID
        /// </summary>
        public int AdminID { get; set; }

        /// <summary>
        /// 管理员用户名
        /// </summary>
        [StringLength(50)]
        public string AdminUsername { get; set; }

        /// <summary>
        /// 操作模块：UserManagement, RecyclerManagement, FeedbackManagement, HomepageManagement
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Module { get; set; }

        /// <summary>
        /// 操作类型：Create, Update, Delete, View, Export, Reply
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OperationType { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 目标对象ID（如用户ID、回收员ID等）
        /// </summary>
        public int? TargetID { get; set; }

        /// <summary>
        /// 目标对象名称（如用户名、回收员名等）
        /// </summary>
        [StringLength(100)]
        public string TargetName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [StringLength(50)]
        public string IPAddress { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// 操作结果：Success, Failed
        /// </summary>
        [StringLength(20)]
        public string Result { get; set; }

        /// <summary>
        /// 附加信息（JSON格式，存储更多细节）
        /// </summary>
        public string Details { get; set; }
    }
}
