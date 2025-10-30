using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class ConversationViewModel
    {
        public int ConversationID { get; set; }
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public int RecyclerID { get; set; }
        public string RecyclerName { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? EndedTime { get; set; }
        public string Status { get; set; }
    }
}
