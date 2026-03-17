namespace recycling.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TransportationOrders
    {
        [Key]
        public int TransportOrderID { get; set; }

        [StringLength(50)]
        public string OrderNumber { get; set; }

        public int? RecyclerID { get; set; }

        public int? TransporterID { get; set; }

        public string PickupAddress { get; set; }

        public string DestinationAddress { get; set; }

        [StringLength(50)]
        public string ContactPerson { get; set; }

        [StringLength(50)]
        public string ContactPhone { get; set; }

        public decimal? EstimatedWeight { get; set; }

        public decimal? ActualWeight { get; set; }

        public string ItemCategories { get; set; }

        public string SpecialInstructions { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? AcceptedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? PickupDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DeliveryDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? CompletedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? CancelledDate { get; set; }

        [StringLength(200)]
        public string CancelReason { get; set; }

        public int? RecyclerRating { get; set; }

        [StringLength(500)]
        public string RecyclerReview { get; set; }

        [StringLength(50)]
        public string BaseContactPerson { get; set; }

        [StringLength(50)]
        public string BaseContactPhone { get; set; }

        public decimal? ItemTotalValue { get; set; }

        [StringLength(50)]
        public string Stage { get; set; }

        [StringLength(50)]
        public string TransportStage { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? PickupConfirmedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ArrivedAtPickupDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LoadingCompletedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DeliveryConfirmedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ArrivedAtDeliveryDate { get; set; }

        public int? AssignedWorkerID { get; set; }
    }
}
