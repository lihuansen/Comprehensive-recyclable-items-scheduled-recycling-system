namespace recycling.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AdminContactConversations
    {
        [Key]
        public int ConversationID { get; set; }

        public int UserID { get; set; }

        public int? AdminID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime StartTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UserEndedTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? AdminEndedTime { get; set; }

        public bool UserEnded { get; set; }

        public bool AdminEnded { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LastMessageTime { get; set; }
    }
}
