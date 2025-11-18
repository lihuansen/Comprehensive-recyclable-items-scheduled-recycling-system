namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用户联系请求实体类
    /// 用于记录用户点击"联系我们"的请求
    /// </summary>
    [Table("UserContactRequests")]
    public partial class UserContactRequests
    {
        /// <summary>
        /// 请求ID
        /// </summary>
        [Key]
        public int RequestID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 请求状态：true=待联系，false=已处理
        /// </summary>
        public bool RequestStatus { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// 管理员联系处理时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? ContactedTime { get; set; }

        /// <summary>
        /// 处理的管理员ID
        /// </summary>
        public int? AdminID { get; set; }
    }
}
