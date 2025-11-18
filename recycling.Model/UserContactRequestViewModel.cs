using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    /// <summary>
    /// 用户联系请求视图模型
    /// 包含用户信息和请求信息
    /// </summary>
    public class UserContactRequestViewModel
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool RequestStatus { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime? ContactedTime { get; set; }
        public int? AdminID { get; set; }
        public string AdminName { get; set; }
    }
}
