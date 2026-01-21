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

        public string TransporterNotes { get; set; }

        [StringLength(50)]
        public string BaseContactPerson { get; set; }

        [StringLength(50)]
        public string BaseContactPhone { get; set; }

        public decimal? ItemTotalValue { get; set; }

        [StringLength(50)]
        public string Stage { get; set; }
    }
}
