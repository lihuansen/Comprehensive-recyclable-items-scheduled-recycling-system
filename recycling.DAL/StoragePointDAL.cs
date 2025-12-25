using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 暂存点管理数据访问层 - 简化实现
    /// 直接从Appointments和AppointmentCategories表查询数据
    /// </summary>
    public class StoragePointDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 获取回收员的暂存点库存汇总（按类别分组）
        /// 从已完成的订单中汇总数据
        /// </summary>
        public List<StoragePointSummary> GetStoragePointSummary(int recyclerId)
        {
            var summary = new List<StoragePointSummary>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Query completed orders summary by category for this recycler
                string sql = @"
                    SELECT 
                        ac.CategoryKey, 
                        ac.CategoryName, 
                        SUM(ac.Weight) AS TotalWeight,
                        SUM(CASE 
                            WHEN a.EstimatedWeight > 0 THEN ISNULL(a.EstimatedPrice, 0) * ac.Weight / a.EstimatedWeight
                            ELSE 0
                        END) AS TotalPrice
                    FROM Appointments a
                    INNER JOIN AppointmentCategories ac ON a.AppointmentID = ac.AppointmentID
                    WHERE a.RecyclerID = @RecyclerID 
                        AND a.Status = '已完成'
                        AND ac.Weight > 0
                    GROUP BY ac.CategoryKey, ac.CategoryName
                    ORDER BY ac.CategoryName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summary.Add(new StoragePointSummary
                            {
                                CategoryKey = reader["CategoryKey"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                TotalWeight = Convert.ToDecimal(reader["TotalWeight"]),
                                TotalPrice = reader["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalPrice"])
                            });
                        }
                    }
                }
            }

            return summary;
        }

        /// <summary>
        /// Get storage point inventory details for a recycler
        /// Query from completed orders
        /// </summary>
        public List<StoragePointDetail> GetStoragePointDetail(int recyclerId, string categoryKey = null)
        {
            var details = new List<StoragePointDetail>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Query detailed records of completed orders for this recycler
                string sql = @"
                    SELECT 
                        a.AppointmentID AS OrderID,
                        ac.CategoryKey, 
                        ac.CategoryName, 
                        ac.Weight,
                        CASE 
                            WHEN a.EstimatedWeight > 0 THEN ISNULL(a.EstimatedPrice, 0) * ac.Weight / a.EstimatedWeight
                            ELSE 0
                        END AS Price,
                        a.UpdatedDate AS CompletedDate
                    FROM Appointments a
                    INNER JOIN AppointmentCategories ac ON a.AppointmentID = ac.AppointmentID
                    WHERE a.RecyclerID = @RecyclerID 
                        AND a.Status = '已完成'
                        AND ac.Weight > 0
                        AND (@CategoryKey IS NULL OR @CategoryKey = '' OR ac.CategoryKey = @CategoryKey)
                    ORDER BY a.UpdatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            details.Add(new StoragePointDetail
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                CreatedDate = Convert.ToDateTime(reader["CompletedDate"])
                            });
                        }
                    }
                }
            }

            return details;
        }
    }
}
