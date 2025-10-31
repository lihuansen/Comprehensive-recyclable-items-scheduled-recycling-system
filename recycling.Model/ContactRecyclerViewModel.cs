using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace recycling.Model
{
    public class ContactRecyclerViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string RecyclerName { get; set; }
        public string RecyclerPhone { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }

        [Required(ErrorMessage = "请输入消息内容")]
        [StringLength(500, ErrorMessage = "消息内容不能超过500个字符")]
        public string MessageContent { get; set; }
    }
}
