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

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? EndedTime { get; set; }

        public bool? UserEnded { get; set; }

        public bool? RecyclerEnded { get; set; }

        public DateTime? UserEndedTime { get; set; }

        public DateTime? RecyclerEndedTime { get; set; }
    }
}
