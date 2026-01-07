using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace recycling.Model
{
    public partial class TransportationOrdrers : DbContext
    {
        public TransportationOrdrers()
            : base("name=TransportationOrdrers")
        {
        }

        public virtual DbSet<TransportationOrders> TransportationOrders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransportationOrders>()
                .Property(e => e.EstimatedWeight)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TransportationOrders>()
                .Property(e => e.ActualWeight)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TransportationOrders>()
                .Property(e => e.ItemTotalValue)
                .HasPrecision(10, 2);
        }
    }
}
