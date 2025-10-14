using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycling.Model
{
    public class RecyclableQueryModel
    {
        public string Keyword { get; set; } // 模糊查询关键词（名称/描述）
        public string Category { get; set; } // 品类筛选（空则不筛选）
        public decimal? MinPrice { get; set; } // 最低价格
        public decimal? MaxPrice { get; set; } // 最高价格
        public int PageIndex { get; set; } = 1; // 当前页码（默认第1页）
        public int PageSize { get; set; } = 6; // 每页条数（固定6条）
    }
}
