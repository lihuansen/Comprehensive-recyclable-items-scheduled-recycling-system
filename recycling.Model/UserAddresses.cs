namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserAddresses
    {
        [Key]
        public int AddressID { get; set; }

        public int? UserID { get; set; }

        [StringLength(50)]
        public string Province { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string District { get; set; }

        [StringLength(50)]
        public string Street { get; set; }

        public string DetailAddress { get; set; }

        [StringLength(50)]
        public string ContactName { get; set; }

        [StringLength(50)]
        public string ContactPhone { get; set; }

        public bool? IsDefault { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 完整地址 (组合省份、城市、区域、街道和详细地址)
        /// </summary>
        [NotMapped]
        public string FullAddress
        {
            get
            {
                return $"{Province ?? ""}{City ?? ""}{District ?? ""}{Street ?? ""}{DetailAddress ?? ""}";
            }
        }
    }
}
