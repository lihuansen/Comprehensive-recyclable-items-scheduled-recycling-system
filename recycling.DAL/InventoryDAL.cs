using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    public class InventoryDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加库存记录（从订单的类别重量写入）
        /// </summary>
        public bool AddInventoryFromOrder(int orderId, int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 获取订单的类别和重量信息，以及订单的估算价格
                        string getCategoriesSql = @"
                            SELECT CategoryKey, CategoryName, Weight
                            FROM AppointmentCategories
                            WHERE AppointmentID = @OrderID AND Weight > 0";

                        List<(string key, string name, decimal weight)> categories = new List<(string, string, decimal)>();

                        using (SqlCommand cmd = new SqlCommand(getCategoriesSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    categories.Add((
                                        reader["CategoryKey"].ToString(),
                                        reader["CategoryName"].ToString(),
                                        Convert.ToDecimal(reader["Weight"])
                                    ));
                                }
                            }
                        }

                        if (categories.Count == 0)
                        {
                            trans.Rollback();
                            return false; // 没有类别数据
                        }

                        // 获取订单的估算价格
                        string getPriceSql = "SELECT EstimatedPrice FROM Appointments WHERE AppointmentID = @OrderID";
                        decimal? orderPrice = null;
                        using (SqlCommand cmd = new SqlCommand(getPriceSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            var result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                orderPrice = Convert.ToDecimal(result);
                            }
                        }

                        // 计算总重量用于按比例分配价格
                        decimal totalWeight = 0;
                        foreach (var category in categories)
                        {
                            totalWeight += category.weight;
                        }

                        // 插入库存记录
                        string insertSql = @"
                            INSERT INTO Inventory (OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate)
                            VALUES (@OrderID, @CategoryKey, @CategoryName, @Weight, @Price, @RecyclerID, @CreatedDate)";

                        foreach (var category in categories)
                        {
                            // 按照重量比例分配价格
                            decimal? categoryPrice = null;
                            if (orderPrice.HasValue && totalWeight > 0)
                            {
                                categoryPrice = orderPrice.Value * (category.weight / totalWeight);
                            }

                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CategoryKey", category.key);
                                cmd.Parameters.AddWithValue("@CategoryName", category.name);
                                cmd.Parameters.AddWithValue("@Weight", category.weight);
                                cmd.Parameters.AddWithValue("@Price", categoryPrice.HasValue ? (object)categoryPrice.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 获取库存列表
        /// </summary>
        public List<Inventory> GetInventoryList(int? recyclerId = null, int pageIndex = 1, int pageSize = 50)
        {
            List<Inventory> list = new List<Inventory>();
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT InventoryID, OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate
                    FROM Inventory
                    WHERE (@RecyclerID IS NULL OR RecyclerID = @RecyclerID)
                    ORDER BY CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId.HasValue ? (object)recyclerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Inventory
                            {
                                InventoryID = Convert.ToInt32(reader["InventoryID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : Convert.ToDecimal(reader["Price"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取库存汇总（按类别分组）
        /// </summary>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetInventorySummary(int? recyclerId = null)
        {
            var summary = new List<(string, string, decimal, decimal)>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        CategoryKey, 
                        CategoryName, 
                        SUM(Weight) AS TotalWeight,
                        SUM(ISNULL(Price, 0)) AS TotalPrice
                    FROM Inventory
                    WHERE (@RecyclerID IS NULL OR RecyclerID = @RecyclerID)
                    GROUP BY CategoryKey, CategoryName
                    ORDER BY CategoryName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId.HasValue ? (object)recyclerId.Value : DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summary.Add((
                                reader["CategoryKey"].ToString(),
                                reader["CategoryName"].ToString(),
                                Convert.ToDecimal(reader["TotalWeight"]),
                                Convert.ToDecimal(reader["TotalPrice"])
                            ));
                        }
                    }
                }
            }

            return summary;
        }

        /// <summary>
        /// 获取库存明细（包含回收员信息）- 管理员端使用
        /// </summary>
        public PagedResult<InventoryDetailViewModel> GetInventoryDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null)
        {
            var result = new PagedResult<InventoryDetailViewModel>
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 获取总数
                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Inventory i
                    WHERE (@CategoryKey IS NULL OR @CategoryKey = '' OR i.CategoryKey = @CategoryKey)";

                conn.Open();

                using (SqlCommand cmd = new SqlCommand(countSql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);
                    result.TotalCount = (int)cmd.ExecuteScalar();
                }

                // 获取数据
                string sql = @"
                    SELECT 
                        i.InventoryID, 
                        i.OrderID, 
                        'AP' + RIGHT('000000' + CAST(i.OrderID AS VARCHAR(6)), 6) AS OrderNumber,
                        i.CategoryKey, 
                        i.CategoryName, 
                        i.Weight, 
                        i.Price, 
                        i.RecyclerID, 
                        ISNULL(r.Username, '未知回收员') AS RecyclerName,
                        i.CreatedDate
                    FROM Inventory i
                    LEFT JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
                    WHERE (@CategoryKey IS NULL OR @CategoryKey = '' OR i.CategoryKey = @CategoryKey)
                    ORDER BY i.CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Items.Add(new InventoryDetailViewModel
                            {
                                InventoryID = Convert.ToInt32(reader["InventoryID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = reader["OrderNumber"].ToString(),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : Convert.ToDecimal(reader["Price"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                RecyclerName = reader["RecyclerName"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
