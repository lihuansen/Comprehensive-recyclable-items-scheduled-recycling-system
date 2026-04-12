using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    // 中文注释
    /// 暂存点管理数据访问层 - 简化实现
    /// 直接从Appointments和AppointmentCategories表查询数据
    // 中文注释
    public class StoragePointDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
        
        // 中文注释
        private const int SQL_ERROR_INVALID_OBJECT = 208; // 中文注释

        // 中文注释
        /// 获取回收员的暂存点库存汇总（按类别分组）
        /// 从Inventory表查询InventoryType = 'StoragePoint'的数据
        // 中文注释
        public List<StoragePointSummary> GetStoragePointSummary(int recyclerId)
        {
            var summary = new List<StoragePointSummary>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                // 中文注释
                // 中文注释
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
                                // 中文注释
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

        // 中文注释
        /// 中文注释
        /// 中文注释
        // 中文注释
        public List<StoragePointDetail> GetStoragePointDetail(int recyclerId, string categoryKey = null)
        {
            var details = new List<StoragePointDetail>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                // 中文注释
                // 中文注释
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
                                    OrderID = reader["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OrderID"]),
                                    CategoryKey = reader["CategoryKey"]?.ToString() ?? "",
                                    CategoryName = reader["CategoryName"]?.ToString() ?? "未知类别",
                                    Weight = reader["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Weight"]),
                                    Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                    CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedDate"]),
                                    IsManualEntry = reader["OrderID"] == DBNull.Value || Convert.ToInt32(reader["OrderID"]) <= 0
                                });
                            }
                            catch (Exception ex)
                            {
                                // 中文注释
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

        // 中文注释
        /// 回收员手动添加暂存点物品
        // 中文注释
        public bool AddManualStoragePointItem(int recyclerId, string categoryKey, string categoryName, decimal weight, decimal price)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    const string insertSql = @"
                        INSERT INTO Inventory (OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate, InventoryType)
                        VALUES (@OrderID, @CategoryKey, @CategoryName, @Weight, @Price, @RecyclerID, @CreatedDate, N'StoragePoint')";

                    object orderIdValue = DBNull.Value;
                    if (!IsInventoryOrderIdNullable(conn))
                    {
                        // Inventory.OrderID 设计上关联 Appointments.AppointmentID
                        // 如果数据库仍要求非空，则复用该回收员最近的预约单ID作为兼容方案
                        string fallbackOrderSql = @"
                            SELECT TOP 1 AppointmentID
                            FROM Appointments
                            WHERE RecyclerID = @RecyclerID
                            ORDER BY UpdatedDate DESC, CreatedDate DESC, AppointmentID DESC";

                        object fallbackOrderId;
                        using (SqlCommand fallbackCmd = new SqlCommand(fallbackOrderSql, conn))
                        {
                            fallbackCmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                            fallbackOrderId = fallbackCmd.ExecuteScalar();
                        }

                        if (fallbackOrderId == null || fallbackOrderId == DBNull.Value)
                        {
                            throw new Exception("无法添加物品：需要先完成至少一笔预约单才能手动添加物品");
                        }

                        orderIdValue = Convert.ToInt32(fallbackOrderId);
                    }

                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderIdValue;
                        cmd.Parameters.AddWithValue("@CategoryKey", categoryKey);
                        cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                        cmd.Parameters.AddWithValue("@Weight", weight);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in AddManualStoragePointItem: {sqlEx.Message}");
                throw new Exception($"数据库更新错误 (代码: {sqlEx.Number}): {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddManualStoragePointItem: {ex.Message}");
                throw;
            }
        }

        private bool IsInventoryOrderIdNullable(SqlConnection conn)
        {
            const string sql = @"
                SELECT IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Inventory'
                  AND COLUMN_NAME = 'OrderID'";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                var result = cmd.ExecuteScalar()?.ToString();
                return string.Equals(result, "YES", StringComparison.OrdinalIgnoreCase);
            }
        }

        // 中文注释
        /// 清空回收员的暂存点库存记录（删除库存数据，不改变预约订单状态）
        /// 中文注释
        /// Note: This method does NOT change appointment status. Appointments remain "已完成".
        /// 中文注释
        // 中文注释
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
                        return true; // 中文注释
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // 中文注释
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
