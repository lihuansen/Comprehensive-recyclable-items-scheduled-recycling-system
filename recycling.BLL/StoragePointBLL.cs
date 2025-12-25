using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 暂存点管理业务逻辑层 - 简化实现
    /// 直接从已完成的订单中获取数据，无需单独的Inventory表
    /// </summary>
    public class StoragePointBLL
    {
        private readonly StoragePointDAL _storagePointDAL = new StoragePointDAL();

        /// <summary>
        /// 获取回收员的暂存点库存汇总（按类别分组）
        /// </summary>
        /// <param name="recyclerId">回收员ID</param>
        /// <returns>按类别汇总的库存数据</returns>
        public List<StoragePointSummary> GetStoragePointSummary(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return new List<StoragePointSummary>();
            }

            return _storagePointDAL.GetStoragePointSummary(recyclerId);
        }

        /// <summary>
        /// 获取回收员的暂存点库存明细
        /// </summary>
        /// <param name="recyclerId">回收员ID</param>
        /// <param name="categoryKey">类别键（可选，用于筛选特定类别）</param>
        /// <returns>库存明细列表</returns>
        public List<StoragePointDetail> GetStoragePointDetail(int recyclerId, string categoryKey = null)
        {
            if (recyclerId <= 0)
            {
                return new List<StoragePointDetail>();
            }

            return _storagePointDAL.GetStoragePointDetail(recyclerId, categoryKey);
        }
    }
}
