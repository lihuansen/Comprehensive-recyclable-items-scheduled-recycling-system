namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Messages
    {
        [Key]
        public int MessageID { get; set; }

        public int? OrderID { get; set; }

        [StringLength(20)]
        public string SenderType { get; set; }

        public int? SenderID { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? SentTime { get; set; }

        public bool? IsRead { get; set; }
    }
}
