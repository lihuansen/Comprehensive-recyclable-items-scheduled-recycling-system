using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class RecyclerOrderViewModel
    {
        public int AppointmentID { get; set; }
        public string OrderNumber { get; set; }
        public string AppointmentType { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public decimal EstimatedWeight { get; set; }
        public bool IsUrgent { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CategoryNames { get; set; }
        public int? RecyclerID { get; set; }
        public string RecyclerName { get; set; }
    }
}
