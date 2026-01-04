using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 入库单数据访问层
    /// Warehouse Receipt Data Access Layer
    /// </summary>
    public class WarehouseReceiptDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 生成入库单号
        /// 格式：WR+YYYYMMDD+4位序号
        /// </summary>
        private string GenerateReceiptNumber()
        {
            string datePrefix = "WR" + DateTime.Now.ToString("yyyyMMdd");
            int sequence = 1;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                using (SqlTransaction transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        string sql = "SELECT COUNT(*) FROM WarehouseReceipts WITH (TABLOCKX) WHERE ReceiptNumber LIKE @DatePrefix + '%'";
                        using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DatePrefix", datePrefix);
                            sequence = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                        }
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return datePrefix + sequence.ToString("D4");
        }

        /// <summary>
        /// 创建入库单并清零暂存点重量
        /// </summary>
        public (int receiptId, string receiptNumber) CreateWarehouseReceipt(WarehouseReceipts receipt)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. 生成入库单号
                            receipt.ReceiptNumber = GenerateReceiptNumber();
                            receipt.CreatedDate = DateTime.Now;
                            receipt.Status = "已入库";

                            // 2. 插入入库单记录
                            string insertSql = @"
                                INSERT INTO WarehouseReceipts 
                                (ReceiptNumber, TransportOrderID, RecyclerID, WorkerID, TotalWeight, 
                                 ItemCategories, Status, Notes, CreatedDate, CreatedBy)
                                VALUES 
                                (@ReceiptNumber, @TransportOrderID, @RecyclerID, @WorkerID, @TotalWeight, 
                                 @ItemCategories, @Status, @Notes, @CreatedDate, @CreatedBy);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            int receiptId;
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ReceiptNumber", receipt.ReceiptNumber);
                                cmd.Parameters.AddWithValue("@TransportOrderID", receipt.TransportOrderID);
                                cmd.Parameters.AddWithValue("@RecyclerID", receipt.RecyclerID);
                                cmd.Parameters.AddWithValue("@WorkerID", receipt.WorkerID);
                                cmd.Parameters.AddWithValue("@TotalWeight", receipt.TotalWeight);
                                cmd.Parameters.AddWithValue("@ItemCategories", (object)receipt.ItemCategories ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", receipt.Status);
                                cmd.Parameters.AddWithValue("@Notes", (object)receipt.Notes ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CreatedDate", receipt.CreatedDate);
                                cmd.Parameters.AddWithValue("@CreatedBy", receipt.CreatedBy);

                                receiptId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // 3. 清零对应暂存点的重量（通过删除Inventory记录）
                            string clearInventorySql = @"
                                DELETE FROM Inventory 
                                WHERE RecyclerID = @RecyclerID";

                            using (SqlCommand cmd = new SqlCommand(clearInventorySql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RecyclerID", receipt.RecyclerID);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return (receiptId, receipt.ReceiptNumber);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWarehouseReceipt Error: {ex.Message}");
                throw new Exception($"创建入库单失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取入库单列表（带分页）
        /// </summary>
        public List<WarehouseReceiptViewModel> GetWarehouseReceipts(int page = 1, int pageSize = 20, string status = null, int? workerId = null)
        {
            var receipts = new List<WarehouseReceiptViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT 
                            wr.ReceiptID, wr.ReceiptNumber, wr.TransportOrderID, wr.TotalWeight,
                            wr.ItemCategories, wr.Status, wr.Notes, wr.CreatedDate,
                            t.OrderNumber AS TransportOrderNumber,
                            r.FullName AS RecyclerName,
                            w.FullName AS WorkerName
                        FROM WarehouseReceipts wr
                        LEFT JOIN TransportationOrders t ON wr.TransportOrderID = t.TransportOrderID
                        LEFT JOIN Recyclers r ON wr.RecyclerID = r.RecyclerID
                        LEFT JOIN SortingCenterWorkers w ON wr.WorkerID = w.WorkerID
                        WHERE 1=1";
                    
                    if (!string.IsNullOrEmpty(status) && status != "all")
                    {
                        sql += " AND wr.Status = @Status";
                    }
                    
                    if (workerId.HasValue)
                    {
                        sql += " AND wr.WorkerID = @WorkerID";
                    }
                    
                    sql += @"
                        ORDER BY wr.CreatedDate DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(status) && status != "all")
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }
                        
                        if (workerId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@WorkerID", workerId.Value);
                        }
                        
                        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                receipts.Add(new WarehouseReceiptViewModel
                                {
                                    ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                                    ReceiptNumber = reader["ReceiptNumber"].ToString(),
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    TransportOrderNumber = reader["TransportOrderNumber"] == DBNull.Value ? null : reader["TransportOrderNumber"].ToString(),
                                    TotalWeight = Convert.ToDecimal(reader["TotalWeight"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    RecyclerName = reader["RecyclerName"] == DBNull.Value ? null : reader["RecyclerName"].ToString(),
                                    WorkerName = reader["WorkerName"] == DBNull.Value ? null : reader["WorkerName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseReceipts Error: {ex.Message}");
                throw new Exception($"获取入库单列表失败: {ex.Message}", ex);
            }

            return receipts;
        }

        /// <summary>
        /// 获取运输中的订单列表（用于基地管理）
        /// </summary>
        public List<TransportNotificationViewModel> GetInTransitOrders()
        {
            var orders = new List<TransportNotificationViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT 
                            t.TransportOrderID, t.OrderNumber, t.EstimatedWeight, 
                            t.ItemCategories, t.PickupDate, t.Status,
                            r.FullName AS RecyclerName,
                            tr.FullName AS TransporterName
                        FROM TransportationOrders t
                        LEFT JOIN Recyclers r ON t.RecyclerID = r.RecyclerID
                        LEFT JOIN Transporters tr ON t.TransporterID = tr.TransporterID
                        WHERE t.Status = N'运输中'
                        ORDER BY t.PickupDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new TransportNotificationViewModel
                                {
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    OrderNumber = reader["OrderNumber"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    Status = reader["Status"].ToString(),
                                    RecyclerName = reader["RecyclerName"] == DBNull.Value ? null : reader["RecyclerName"].ToString(),
                                    TransporterName = reader["TransporterName"] == DBNull.Value ? null : reader["TransporterName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInTransitOrders Error: {ex.Message}");
                throw new Exception($"获取运输中订单失败: {ex.Message}", ex);
            }

            return orders;
        }

        /// <summary>
        /// 根据运输单ID获取入库单
        /// </summary>
        public WarehouseReceipts GetWarehouseReceiptByTransportOrderId(int transportOrderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT * FROM WarehouseReceipts 
                        WHERE TransportOrderID = @TransportOrderID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransportOrderID", transportOrderId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new WarehouseReceipts
                                {
                                    ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                                    ReceiptNumber = reader["ReceiptNumber"].ToString(),
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                    WorkerID = Convert.ToInt32(reader["WorkerID"]),
                                    TotalWeight = Convert.ToDecimal(reader["TotalWeight"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    CreatedBy = Convert.ToInt32(reader["CreatedBy"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseReceiptByTransportOrderId Error: {ex.Message}");
                throw new Exception($"获取入库单失败: {ex.Message}", ex);
            }

            return null;
        }
    }
}
