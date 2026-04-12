using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// 运输单品类明细业务逻辑层
    /// 中文说明
    public class TransportationOrderCategoriesBLL
    {
        private readonly TransportationOrderCategoriesDAL _dal = new TransportationOrderCategoriesDAL();

        /// 批量插入运输单品类明细
        /// 中文说明
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="transportOrderId">运输单ID</param>
        /// <param name="categories">品类明细列表</param>
        public void BatchInsertCategories(SqlConnection conn, SqlTransaction transaction, 
            int transportOrderId, List<TransportationOrderCategories> categories)
        {
            if (conn == null)
            {
                throw new ArgumentNullException(nameof(conn), "数据库连接不能为空");
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "事务不能为空");
            }

            if (transportOrderId <= 0)
            {
                throw new ArgumentException("运输单ID无效", nameof(transportOrderId));
            }

            _dal.BatchInsertCategories(conn, transaction, transportOrderId, categories);
        }

        /// 获取运输单的所有品类明细
        /// 中文说明
        /// <param name="transportOrderId">运输单ID</param>
        /// <returns>品类明细列表</returns>
        public List<TransportationOrderCategories> GetCategoriesByTransportOrderId(int transportOrderId)
        {
            if (transportOrderId <= 0)
            {
                return new List<TransportationOrderCategories>();
            }

            return _dal.GetCategoriesByTransportOrderId(transportOrderId);
        }

        /// 删除运输单的所有品类明细
        /// 中文说明
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="transportOrderId">运输单ID</param>
        public void DeleteCategoriesByTransportOrderId(SqlConnection conn, SqlTransaction transaction, int transportOrderId)
        {
            if (conn == null)
            {
                throw new ArgumentNullException(nameof(conn), "数据库连接不能为空");
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "事务不能为空");
            }

            if (transportOrderId <= 0)
            {
                throw new ArgumentException("运输单ID无效", nameof(transportOrderId));
            }

            _dal.DeleteCategoriesByTransportOrderId(conn, transaction, transportOrderId);
        }

        /// 检查 TransportationOrderCategories 表是否存在
        /// 中文说明
        /// <returns>表是否存在</returns>
        public bool TableExists()
        {
            return _dal.TableExists();
        }
    }
}
