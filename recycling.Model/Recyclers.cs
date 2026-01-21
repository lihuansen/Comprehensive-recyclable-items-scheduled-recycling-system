namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Recyclers
    {
        [Key]
        public int RecyclerID { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public bool? Available { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }

        public string Region { get; set; }

        public decimal? Rating { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public string AvatarURL { get; set; }

        public decimal? money { get; set; }
    }
}
