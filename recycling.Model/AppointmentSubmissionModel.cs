using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class AppointmentSubmissionModel
    {
        public AppointmentViewModel BasicInfo { get; set; }
        public Dictionary<string, Dictionary<string, string>> CategoryAnswers { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
