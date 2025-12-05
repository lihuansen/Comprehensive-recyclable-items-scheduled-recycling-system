namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AdminContactMessages
    {
        [Key]
        public int MessageID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        public string SenderType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime SentTime { get; set; }

        public bool IsRead { get; set; }
    }
}
