using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class SendMessageRequest
    {
        public int OrderID { get; set; }
        public string SenderType { get; set; }
        public int SenderID { get; set; }
        public string Content { get; set; }
    }
}
