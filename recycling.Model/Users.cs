namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        [Key]
        public int UserID { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [StringLength(50)]
        public string url { get; set; }

        public decimal? money { get; set; }
    }
}
