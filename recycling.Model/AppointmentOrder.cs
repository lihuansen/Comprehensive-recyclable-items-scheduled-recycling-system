using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class AppointmentOrder
    {
        public int AppointmentID { get; set; }
        public string AppointmentType { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public decimal EstimatedWeight { get; set; }
        public bool IsUrgent { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CategoryNames { get; set; } // 品类名称列表，逗号分隔
    }

    public class OrderDetail
    {
        public Appointments Appointment { get; set; }
        public List<AppointmentCategories> Categories { get; set; }
        public string RecyclerName { get; set; } // 回收员姓名
    }

    public class OrderStatistics
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
