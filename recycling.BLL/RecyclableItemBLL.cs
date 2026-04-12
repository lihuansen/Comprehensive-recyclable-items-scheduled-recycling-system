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
        private static readonly Dictionary<string, string> CategoryNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "glass", "玻璃" },
            { "metal", "金属" },
            { "plastic", "塑料" },
            { "paper", "纸类" },
            { "fabric", "纺织品" },
            { "appliance", "家电" },
            { "foam", "泡沫" }
        };

        // 中文注释
        /// 分页查询可回收物（处理参数有效性，调用DAL层）
        // 中文注释
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

        // 中文注释
        /// 分页查询所有可回收物（管理端使用，不过滤IsActive状态）
        // 中文注释
        public PagedResult<RecyclableItems> GetPagedItemsForAdmin(RecyclableQueryModel query)
        {
            if (query == null)
                throw new ArgumentNullException("查询参数不能为空");

            if (query.PageIndex < 1)
                query.PageIndex = 1;

            return _recyclableItemDAL.GetPagedItemsForAdmin(query);
        }

        // 中文注释
        /// 获取所有可回收物品类（供筛选下拉框使用）
        // 中文注释
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

        // 中文注释
        /// 确保数据库中存在可回收物数据（首次访问时检查）
        // 中文注释
        public void EnsureDataExists()
        {
            try
            {
                if (!_recyclableItemDAL.HasData())
                {
                    // 数据已通过SQL脚本初始化，此处仅提示（实际项目可记录日志）
                    throw new Exception("可回收物数据未初始化，请先执行数据库脚本");
                }
                _recyclableItemDAL.EnsureApplianceCategoryExists();
                _recyclableItemDAL.EnsureFoamCategoryExists();
            }
            catch (Exception ex)
            {
                throw new Exception("检查可回收物数据失败：" + ex.Message);
            }
        }
        // 中文注释
        /// 获取所有有效数据（用于调试）
        // 中文注释
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

        // 中文注释
        /// 中文注释
        // 中文注释
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

        // 中文注释
        /// 中文注释
        // 中文注释
        public (bool Success, string Message) Add(RecyclableItems item)
        {
            if (item == null)
            {
                return (false, "请求数据无效");
            }

            NormalizeItemFields(item);

            // 中文注释
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

            if (!item.PricePerKg.HasValue)
            {
                return (false, "价格不能为空");
            }

            if (item.PricePerKg.Value < 0)
            {
                return (false, "价格不能为负数");
            }

            if (!item.SortOrder.HasValue || item.SortOrder.Value < 0)
            {
                item.SortOrder = 0;
            }

            if (!item.IsActive.HasValue)
            {
                item.IsActive = true;
            }

            try
            {
                if (_recyclableItemDAL.ExistsByNameAndCategory(item.Name, item.Category))
                {
                    return (false, $"该品类下已存在名为“{item.Name}”的物品，请勿重复添加");
                }

                bool result = _recyclableItemDAL.Add(item);
                return result ? (true, "添加可回收物品成功") : (false, "添加可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"添加可回收物品失败：{ex.Message}");
            }
        }

        // 中文注释
        /// 中文注释
        // 中文注释
        public (bool Success, string Message) Update(RecyclableItems item)
        {
            if (item == null)
            {
                return (false, "请求数据无效");
            }

            NormalizeItemFields(item);

            // 中文注释
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

            if (!item.PricePerKg.HasValue)
            {
                return (false, "价格不能为空");
            }

            if (item.PricePerKg.Value < 0)
            {
                return (false, "价格不能为负数");
            }

            if (!item.SortOrder.HasValue || item.SortOrder.Value < 0)
            {
                item.SortOrder = 0;
            }

            if (!item.IsActive.HasValue)
            {
                item.IsActive = true;
            }

            try
            {
                if (_recyclableItemDAL.ExistsByNameAndCategory(item.Name, item.Category, item.ItemId))
                {
                    return (false, $"该品类下已存在名为“{item.Name}”的物品，请调整名称或品类后重试");
                }

                bool result = _recyclableItemDAL.Update(item);
                return result ? (true, "更新可回收物品成功") : (false, "更新可回收物品失败");
            }
            catch (Exception ex)
            {
                return (false, $"更新可回收物品失败：{ex.Message}");
            }
        }

        // 中文注释
        /// 中文注释
        // 中文注释
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

        // 中文注释
        /// 中文注释
        // 中文注释
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

        // 中文注释
        /// 中文注释
        // 中文注释
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

        // 中文注释
        /// 中文注释
        // 中文注释
        private static void NormalizeItemFields(RecyclableItems item)
        {
            if (item == null)
            {
                return;
            }

            item.Name = item.Name?.Trim();
            item.Category = item.Category?.Trim();
            item.CategoryName = item.CategoryName?.Trim();

            if (!string.IsNullOrEmpty(item.Category) && string.IsNullOrEmpty(item.CategoryName))
            {
                if (CategoryNameMap.TryGetValue(item.Category, out string categoryName))
                {
                    item.CategoryName = categoryName;
                }
                else
                {
                    item.CategoryName = item.Category;
                }
            }
        }
    }
}
