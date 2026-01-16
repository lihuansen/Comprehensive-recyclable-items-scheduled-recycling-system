using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 运输单品类明细数据访问层
    /// Transportation Order Categories Data Access Layer
    /// </summary>
    public class TransportationOrderCategoriesDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 批量插入运输单品类明细
        /// Batch insert transportation order category details
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="transportOrderId">运输单ID</param>
        /// <param name="categories">品类明细列表</param>
        public void BatchInsertCategories(SqlConnection conn, SqlTransaction transaction, 
            int transportOrderId, List<TransportationOrderCategories> categories)
        {
            if (categories == null || categories.Count == 0)
            {
                return;
            }

            string sql = @"
                INSERT INTO TransportationOrderCategories 
                (TransportOrderID, CategoryKey, CategoryName, Weight, PricePerKg, TotalAmount, CreatedDate)
                VALUES 
                (@TransportOrderID, @CategoryKey, @CategoryName, @Weight, @PricePerKg, @TotalAmount, @CreatedDate)";

            foreach (var category in categories)
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@TransportOrderID", transportOrderId);
                    cmd.Parameters.AddWithValue("@CategoryKey", category.CategoryKey);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Weight", category.Weight);
                    cmd.Parameters.AddWithValue("@PricePerKg", category.PricePerKg);
                    cmd.Parameters.AddWithValue("@TotalAmount", category.TotalAmount);
                    cmd.Parameters.AddWithValue("@CreatedDate", category.CreatedDate);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 获取运输单的所有品类明细
        /// Get all category details for a transportation order
        /// </summary>
        /// <param name="transportOrderId">运输单ID</param>
        /// <returns>品类明细列表</returns>
        public List<TransportationOrderCategories> GetCategoriesByTransportOrderId(int transportOrderId)
        {
            var categories = new List<TransportationOrderCategories>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT CategoryID, TransportOrderID, CategoryKey, CategoryName, 
                               Weight, PricePerKg, TotalAmount, CreatedDate
                        FROM TransportationOrderCategories
                        WHERE TransportOrderID = @TransportOrderID
                        ORDER BY CategoryID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransportOrderID", transportOrderId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new TransportationOrderCategories
                                {
                                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    CategoryKey = reader["CategoryKey"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    Weight = Convert.ToDecimal(reader["Weight"]),
                                    PricePerKg = Convert.ToDecimal(reader["PricePerKg"]),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCategoriesByTransportOrderId Error: {ex.Message}");
                throw new Exception($"获取运输单品类明细失败: {ex.Message}", ex);
            }

            return categories;
        }

        /// <summary>
        /// 删除运输单的所有品类明细
        /// Delete all category details for a transportation order
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="transportOrderId">运输单ID</param>
        public void DeleteCategoriesByTransportOrderId(SqlConnection conn, SqlTransaction transaction, int transportOrderId)
        {
            string sql = @"
                DELETE FROM TransportationOrderCategories 
                WHERE TransportOrderID = @TransportOrderID";

            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@TransportOrderID", transportOrderId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 检查 TransportationOrderCategories 表是否存在
        /// Check if TransportationOrderCategories table exists
        /// </summary>
        public bool TableExists()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'TransportationOrderCategories'";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TableExists Error: {ex.Message}");
                return false;
            }
        }
    }
}
