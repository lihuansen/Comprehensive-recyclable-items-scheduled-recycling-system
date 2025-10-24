using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class AcceptOrderRequest
    {
        public int AppointmentID { get; set; }
        public int RecyclerID { get; set; }
    }
}
