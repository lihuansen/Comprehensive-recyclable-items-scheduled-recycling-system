namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Conversations
    {
        [Key]
        public int ConversationID { get; set; }

        public int? OrderID { get; set; }

        public int? UserID { get; set; }

        public int? RecyclerID { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? CreatedTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? EndedTime { get; set; }
    }
}
