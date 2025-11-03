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
                        // 获取订单的类别和重量信息
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

                        // 插入库存记录
                        string insertSql = @"
                            INSERT INTO Inventory (OrderID, CategoryKey, CategoryName, Weight, RecyclerID, CreatedDate)
                            VALUES (@OrderID, @CategoryKey, @CategoryName, @Weight, @RecyclerID, @CreatedDate)";

                        foreach (var category in categories)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CategoryKey", category.key);
                                cmd.Parameters.AddWithValue("@CategoryName", category.name);
                                cmd.Parameters.AddWithValue("@Weight", category.weight);
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
                    SELECT InventoryID, OrderID, CategoryKey, CategoryName, Weight, RecyclerID, CreatedDate
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
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
