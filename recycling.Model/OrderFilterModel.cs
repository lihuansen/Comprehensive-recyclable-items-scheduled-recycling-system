using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class OrderFilterModel
    {
        public string OrderNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public bool? IsUrgent { get; set; }
        public string Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
