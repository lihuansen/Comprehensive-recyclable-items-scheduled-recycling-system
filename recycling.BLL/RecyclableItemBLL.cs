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
        public PagedResult<RecyclableItems> SearchItems(string keyword, string category, int pageIndex, int pageSize)
        {
            // 处理空参数
            keyword = string.IsNullOrEmpty(keyword) ? "" : keyword.Trim();
            category = string.IsNullOrEmpty(category) || category == "All" ? "" : category;

            try
            {
                return _recyclableItemDAL.SearchItems(keyword, category, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("查询物品失败：" + ex.Message);
            }
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
    }
}
