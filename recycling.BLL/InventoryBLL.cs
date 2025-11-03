using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class InventoryBLL
    {
        private readonly InventoryDAL _inventoryDAL = new InventoryDAL();

        /// <summary>
        /// 从订单添加库存记录
        /// </summary>
        public bool AddInventoryFromOrder(int orderId, int recyclerId)
        {
            if (orderId <= 0 || recyclerId <= 0) return false;
            return _inventoryDAL.AddInventoryFromOrder(orderId, recyclerId);
        }

        /// <summary>
        /// 获取库存列表
        /// </summary>
        public List<Inventory> GetInventoryList(int? recyclerId = null, int pageIndex = 1, int pageSize = 50)
        {
            return _inventoryDAL.GetInventoryList(recyclerId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取库存汇总（按类别分组）
        /// </summary>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight)> GetInventorySummary(int? recyclerId = null)
        {
            return _inventoryDAL.GetInventorySummary(recyclerId);
        }
    }
}
