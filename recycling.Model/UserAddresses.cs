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

        // 完整地址（包含省、市、区、街道、详细地址）
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
