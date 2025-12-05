namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserNotifications
    {
        [Key]
        public int NotificationID { get; set; }

        public int UserID { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        public int? RelatedOrderID { get; set; }

        public int? RelatedFeedbackID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        public bool IsRead { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ReadDate { get; set; }
    }
}
