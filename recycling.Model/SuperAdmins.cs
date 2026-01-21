namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SuperAdmins
    {
        [Key]
        public int SuperAdminID { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
