namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用户地址实体类
    /// </summary>
    public partial class UserAddresses
    {
        [Key]
        public int AddressID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Province { get; set; } = "广东省";

        [Required]
        [StringLength(50)]
        public string City { get; set; } = "深圳市";

        [Required]
        [StringLength(50)]
        public string District { get; set; } = "罗湖区";

        [Required]
        [StringLength(50)]
        public string Street { get; set; }

        [Required]
        [StringLength(200)]
        public string DetailAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactName { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactPhone { get; set; }

        [Required]
        public bool IsDefault { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 获取完整地址字符串
        /// </summary>
        [NotMapped]
        public string FullAddress
        {
            get
            {
                return $"{Province}{City}{District}{Street}{DetailAddress}";
            }
        }
    }
}
