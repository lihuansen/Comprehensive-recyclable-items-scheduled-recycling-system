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
        
        // SQL Server error codes
        private const int SQL_ERROR_INVALID_OBJECT = 208; // Invalid object name (table doesn't exist)

        /// <summary>
        /// 获取回收员的暂存点库存汇总（按类别分组）
        /// 从Inventory表查询InventoryType = 'StoragePoint'的数据
        /// </summary>
        public List<StoragePointSummary> GetStoragePointSummary(int recyclerId)
        {
            var summary = new List<StoragePointSummary>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                // Query from Inventory table where InventoryType = 'StoragePoint'
                // This ensures only items in storage point are shown (not in transit or warehouse)
                string sql = @"
                    SELECT 
                        CategoryKey, 
                        CategoryName, 
                        SUM(ISNULL(Weight, 0)) AS TotalWeight,
                        SUM(ISNULL(Price, 0)) AS TotalPrice
                    FROM Inventory WITH (NOLOCK)
                    WHERE RecyclerID = @RecyclerID 
                        AND InventoryType = N'StoragePoint'
                        AND ISNULL(Weight, 0) > 0
                    GROUP BY CategoryKey, CategoryName
                    ORDER BY CategoryName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                summary.Add(new StoragePointSummary
                                {
                                    CategoryKey = reader["CategoryKey"]?.ToString() ?? "",
                                    CategoryName = reader["CategoryName"]?.ToString() ?? "未知类别",
                                    TotalWeight = reader["TotalWeight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalWeight"]),
                                    TotalPrice = reader["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalPrice"])
                                });
                            }
                            catch (Exception ex)
                            {
                                // Log row-level error but continue processing
                                System.Diagnostics.Debug.WriteLine($"Error processing summary row: {ex.Message}");
                            }
                        }
                    }
                }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in GetStoragePointSummary: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL State: {sqlEx.State}, Number: {sqlEx.Number}");
                throw new Exception($"数据库查询错误 (代码: {sqlEx.Number}): {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetStoragePointSummary: {ex.Message}");
                throw new Exception($"获取库存汇总失败: {ex.Message}", ex);
            }

            return summary;
        }

        /// <summary>
        /// Get storage point inventory details for a recycler
        /// Query from Inventory table where InventoryType = 'StoragePoint'
        /// </summary>
        public List<StoragePointDetail> GetStoragePointDetail(int recyclerId, string categoryKey = null)
        {
            var details = new List<StoragePointDetail>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                // Query from Inventory table where InventoryType = 'StoragePoint'
                // This ensures only items in storage point are shown (not in transit or warehouse)
                string sql = @"
                    SELECT 
                        OrderID,
                        CategoryKey, 
                        CategoryName, 
                        ISNULL(Weight, 0) AS Weight,
                        ISNULL(Price, 0) AS Price,
                        CreatedDate
                    FROM Inventory WITH (NOLOCK)
                    WHERE RecyclerID = @RecyclerID 
                        AND InventoryType = N'StoragePoint'
                        AND ISNULL(Weight, 0) > 0
                        AND (@CategoryKey IS NULL OR @CategoryKey = '' OR CategoryKey = @CategoryKey)
                    ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                details.Add(new StoragePointDetail
                                {
                                    OrderID = Convert.ToInt32(reader["OrderID"]),
                                    CategoryKey = reader["CategoryKey"]?.ToString() ?? "",
                                    CategoryName = reader["CategoryName"]?.ToString() ?? "未知类别",
                                    Weight = reader["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Weight"]),
                                    Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                    CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedDate"])
                                });
                            }
                            catch (Exception ex)
                            {
                                // Log row-level error but continue processing
                                System.Diagnostics.Debug.WriteLine($"Error processing detail row: {ex.Message}");
                            }
                        }
                    }
                }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in GetStoragePointDetail: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL State: {sqlEx.State}, Number: {sqlEx.Number}");
                throw new Exception($"数据库查询错误 (代码: {sqlEx.Number}): {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetStoragePointDetail: {ex.Message}");
                throw new Exception($"获取库存详情失败: {ex.Message}", ex);
            }

            return details;
        }

        /// <summary>
        /// 清空回收员的暂存点库存记录（删除库存数据，不改变预约订单状态）
        /// Clear storage point items for a recycler by deleting inventory records
        /// Note: This method does NOT change appointment status. Appointments remain "已完成".
        /// The inventory is cleared when transport starts, representing items being moved to the base.
        /// </summary>
        /// <param name="recyclerId">回收员ID</param>
        /// <returns>是否成功</returns>
        public bool ClearStoragePointForRecycler(int recyclerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // 清空该回收员的暂存点库存记录（如果存在Inventory表）
                    // 注意：不改变预约订单的状态，预约订单应该保持"已完成"状态
                    // 暂存点的清空仅表示物品已被运输走，不应该改变预约订单的状态
                    // 更新：只删除StoragePoint类型的库存，不删除Warehouse类型的
                    string sql = @"
                        DELETE FROM Inventory 
                        WHERE RecyclerID = @RecyclerID 
                          AND InventoryType = N'StoragePoint'";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Cleared {rowsAffected} inventory items for recycler {recyclerId}. Appointment status remains unchanged.");
                        return true; // Return true even if no rows affected (no items to clear)
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // If Inventory table doesn't exist, that's okay - just log and return true
                if (sqlEx.Number == SQL_ERROR_INVALID_OBJECT)
                {
                    System.Diagnostics.Debug.WriteLine($"Inventory table not found for recycler {recyclerId}, this is expected if inventory is tracked via appointments only.");
                    return true;
                }
                
                System.Diagnostics.Debug.WriteLine($"SQL Error in ClearStoragePointForRecycler: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL State: {sqlEx.State}, Number: {sqlEx.Number}");
                throw new Exception($"数据库更新错误 (代码: {sqlEx.Number}): {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearStoragePointForRecycler: {ex.Message}");
                throw new Exception($"清空暂存点失败: {ex.Message}", ex);
            }
        }
    }
}
