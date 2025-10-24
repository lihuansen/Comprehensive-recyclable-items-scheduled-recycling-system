using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class RecyclerOrderBLL
    {
        private readonly RecyclerOrderDAL _recyclerOrderDAL = new RecyclerOrderDAL();

        /// <summary>
        /// 获取回收员订单列表
        /// </summary>
        public PagedResult<RecyclerOrderViewModel> GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
        {
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            return _recyclerOrderDAL.GetRecyclerOrders(filter, recyclerId);
        }

        /// <summary>
        /// 获取订单统计信息
        /// </summary>
        public OrderStatistics GetOrderStatistics()
        {
            return _recyclerOrderDAL.GetOrderStatistics();
        }

        /// <summary>
        /// 回收员接收订单
        /// </summary>
        public (bool Success, string Message) AcceptOrder(int appointmentId, int recyclerId)
        {
            if (appointmentId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            try
            {
                bool result = _recyclerOrderDAL.AcceptOrder(appointmentId, recyclerId);
                if (result)
                {
                    return (true, "订单接收成功");
                }
                else
                {
                    return (false, "订单接收失败：订单不存在或状态不允许接收");
                }
            }
            catch (Exception ex)
            {
                return (false, $"订单接收失败：{ex.Message}");
            }
        }
    }
}
