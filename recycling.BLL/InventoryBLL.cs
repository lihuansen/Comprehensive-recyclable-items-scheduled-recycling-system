using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="recyclerId">回收员ID（可选）</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为StoragePoint</param>
        public List<Inventory> GetInventoryList(int? recyclerId = null, int pageIndex = 1, int pageSize = 50, string inventoryType = "StoragePoint")
        {
            return _inventoryDAL.GetInventoryList(recyclerId, pageIndex, pageSize, inventoryType);
        }

        /// <summary>
        /// 获取库存汇总（按类别分组）
        /// </summary>
        /// <param name="recyclerId">回收员ID（可选）</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为Warehouse</param>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetInventorySummary(int? recyclerId = null, string inventoryType = "Warehouse")
        {
            return _inventoryDAL.GetInventorySummary(recyclerId, inventoryType);
        }

        /// <summary>
        /// 获取库存明细（包含回收员信息）- 管理员端使用
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="categoryKey">类别键（可选）</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为Warehouse</param>
        public PagedResult<InventoryDetailViewModel> GetInventoryDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null, string inventoryType = "Warehouse")
        {
            return _inventoryDAL.GetInventoryDetailWithRecycler(pageIndex, pageSize, categoryKey, inventoryType);
        }
    }
}
