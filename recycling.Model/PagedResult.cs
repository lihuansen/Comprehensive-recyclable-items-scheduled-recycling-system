using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    /// <summary>
    /// 通用分页结果模型
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>(); // 当前页数据
        public int TotalCount { get; set; } // 总记录数
        public int PageIndex { get; set; } // 当前页码
        public int PageSize { get; set; } // 每页条数
        public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount * 1.0 / PageSize); // 总页数
    }
}
