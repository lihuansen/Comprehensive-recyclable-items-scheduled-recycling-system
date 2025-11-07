using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class RecyclableItemBLL
    {
        // 依赖DAL层，与UserBLL中依赖UserDAL的方式一致
        private readonly RecyclableItemDAL _recyclableItemDAL = new RecyclableItemDAL();

        /// <summary>
        /// 分页查询可回收物（处理参数有效性，调用DAL层）
        /// </summary>
        public PagedResult<RecyclableItems> GetPagedItems(RecyclableQueryModel query)
        {
            // 业务参数校验（模仿UserBLL中的参数检查逻辑）
            if (query == null)
                throw new ArgumentNullException("查询参数不能为空");

            // 确保页码有效（最小为1）
            if (query.PageIndex < 1)
                query.PageIndex = 1;

            // 调用DAL层获取数据（与UserBLL调用UserDAL的方式一致）
            return _recyclableItemDAL.GetPagedItems(query);
        }

        /// <summary>
        /// 获取所有可回收物品类（供筛选下拉框使用）
        /// </summary>
        public Dictionary<string, string> GetAllCategories()
        {
            try
            {
                // 直接调用DAL层方法，无额外业务逻辑
                return _recyclableItemDAL.GetAllCategories();
            }
            catch (Exception ex)
            {
                // 异常处理风格与UserBLL一致，包装异常信息
                throw new Exception("获取品类列表失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 确保数据库中存在可回收物数据（首次访问时检查）
        /// </summary>
        public void EnsureDataExists()
        {
            try
            {
                if (!_recyclableItemDAL.HasData())
                {
                    // 数据已通过SQL脚本初始化，此处仅提示（实际项目可记录日志）
                    throw new Exception("可回收物数据未初始化，请先执行数据库脚本");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("检查可回收物数据失败：" + ex.Message);
            }
        }
        /// <summary>
        /// 获取所有有效数据（用于调试）
        /// </summary>
        public List<RecyclableItems> GetAllActiveItems()
        {
            try
            {
                return _recyclableItemDAL.GetAllActiveItems();
            }
            catch (Exception ex)
            {
                throw new Exception("获取可回收物数据失败：" + ex.Message);
            }
        }

        /// <summary>
        /// Get recyclable item by ID (for admin management)
        /// </summary>
        public RecyclableItems GetById(int itemId)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("无效的物品ID");
            }

            try
            {
                return _recyclableItemDAL.GetById(itemId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取可回收物品失败：" + ex.Message);
            }
        }

        /// <summary>
        /// Add new recyclable item (for admin management)
        /// </summary>
        public (bool Success, string Message) Add(RecyclableItems item)
        {
            // Validation
            if (string.IsNullOrEmpty(item.Name))
            {
                return (false, "物品名称不能为空");
            }

            if (string.IsNullOrEmpty(item.Category))
            {
                return (false, "品类代码不能为空");
            }

            if (string.IsNullOrEmpty(item.CategoryName))
            {
                return (false, "品类名称不能为空");
            }

            if (item.PricePerKg < 0)
            {
                return (false, "价格不能为负数");
            }

            if (item.SortOrder < 0)
            {
                item.SortOrder = 0;
            }

            item.IsActive = true;

            try
            {
                bool result = _recyclableItemDAL.Add(item);
                return result ? (true, "添加可回收物品成功") : (false, "添加可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"添加可回收物品失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Update recyclable item (for admin management)
        /// </summary>
        public (bool Success, string Message) Update(RecyclableItems item)
        {
            // Validation
            if (item.ItemId <= 0)
            {
                return (false, "无效的物品ID");
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                return (false, "物品名称不能为空");
            }

            if (string.IsNullOrEmpty(item.Category))
            {
                return (false, "品类代码不能为空");
            }

            if (string.IsNullOrEmpty(item.CategoryName))
            {
                return (false, "品类名称不能为空");
            }

            if (item.PricePerKg < 0)
            {
                return (false, "价格不能为负数");
            }

            if (item.SortOrder < 0)
            {
                item.SortOrder = 0;
            }

            try
            {
                bool result = _recyclableItemDAL.Update(item);
                return result ? (true, "更新可回收物品成功") : (false, "更新可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"更新可回收物品失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Delete recyclable item (soft delete)
        /// </summary>
        public (bool Success, string Message) Delete(int itemId)
        {
            if (itemId <= 0)
            {
                return (false, "无效的物品ID");
            }

            try
            {
                bool result = _recyclableItemDAL.Delete(itemId);
                return result ? (true, "删除可回收物品成功") : (false, "删除可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"删除可回收物品失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Hard delete recyclable item
        /// </summary>
        public (bool Success, string Message) HardDelete(int itemId)
        {
            if (itemId <= 0)
            {
                return (false, "无效的物品ID");
            }

            try
            {
                bool result = _recyclableItemDAL.HardDelete(itemId);
                return result ? (true, "永久删除可回收物品成功") : (false, "永久删除可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"永久删除可回收物品失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get the maximum SortOrder value
        /// </summary>
        public int GetMaxSortOrder()
        {
            try
            {
                return _recyclableItemDAL.GetMaxSortOrder();
            }
            catch (Exception ex)
            {
                throw new Exception("获取最大排序值失败：" + ex.Message);
            }
        }
    }
}
