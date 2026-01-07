using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
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
        /// 获取已完成的运输单列表（用于入库）
        /// </summary>
        public List<TransportNotificationViewModel> GetCompletedTransportOrders()
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
                        WHERE t.Status = N'已完成'
                        ORDER BY t.CompletedDate DESC";

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
                System.Diagnostics.Debug.WriteLine($"GetCompletedTransportOrders Error: {ex.Message}");
                throw new Exception($"获取已完成运输单失败: {ex.Message}", ex);
            }

            return orders;
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

        /// <summary>
        /// 获取仓库库存汇总（按类别分组） - 从入库单数据中统计
        /// Get warehouse inventory summary grouped by category - from warehouse receipts
        /// </summary>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetWarehouseSummary()
        {
            var summary = new List<(string, string, decimal, decimal)>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 查询所有已入库的入库单，解析ItemCategories JSON并聚合
                    string sql = @"
                        SELECT 
                            wr.ItemCategories,
                            wr.ReceiptID
                        FROM WarehouseReceipts wr
                        WHERE wr.Status = N'已入库'";

                    var categoryData = new Dictionary<string, (string CategoryName, decimal TotalWeight)>();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string itemCategoriesJson = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString();
                                
                                if (!string.IsNullOrEmpty(itemCategoriesJson))
                                {
                                    try
                                    {
                                        // 解析JSON数组
                                        var categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemCategoriesJson);
                                        
                                        foreach (var category in categories)
                                        {
                                            string categoryKey = category.ContainsKey("categoryKey") ? category["categoryKey"].ToString() : "";
                                            string categoryName = category.ContainsKey("categoryName") ? category["categoryName"].ToString() : "";
                                            decimal weight = category.ContainsKey("weight") ? Convert.ToDecimal(category["weight"]) : 0;

                                            if (!string.IsNullOrEmpty(categoryKey) && weight > 0)
                                            {
                                                if (categoryData.ContainsKey(categoryKey))
                                                {
                                                    var existing = categoryData[categoryKey];
                                                    categoryData[categoryKey] = (existing.CategoryName, existing.TotalWeight + weight);
                                                }
                                                else
                                                {
                                                    categoryData[categoryKey] = (categoryName, weight);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"解析ItemCategories JSON失败: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // 获取每个类别的价格并计算总价
                    foreach (var kvp in categoryData)
                    {
                        string categoryKey = kvp.Key;
                        string categoryName = kvp.Value.CategoryName;
                        decimal totalWeight = kvp.Value.TotalWeight;
                        decimal totalPrice = 0;

                        // 从RecyclableItems表获取该类别的价格
                        string priceSql = @"
                            SELECT TOP 1 PricePerKg 
                            FROM RecyclableItems 
                            WHERE Category = @Category AND IsActive = 1
                            ORDER BY SortOrder";

                        using (SqlCommand priceCmd = new SqlCommand(priceSql, conn))
                        {
                            priceCmd.Parameters.AddWithValue("@Category", categoryKey);
                            var priceResult = priceCmd.ExecuteScalar();
                            
                            if (priceResult != null && priceResult != DBNull.Value)
                            {
                                decimal pricePerKg = Convert.ToDecimal(priceResult);
                                totalPrice = totalWeight * pricePerKg;
                            }
                        }

                        summary.Add((categoryKey, categoryName, totalWeight, totalPrice));
                    }

                    // 按类别名称排序
                    summary = summary.OrderBy(s => s.Item2).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseSummary Error: {ex.Message}");
                throw new Exception($"获取仓库汇总失败: {ex.Message}", ex);
            }

            return summary;
        }

        /// <summary>
        /// 获取仓库库存明细（包含回收员信息）- 从入库单数据中提取
        /// Get warehouse inventory detail with recycler info - from warehouse receipts
        /// </summary>
        public PagedResult<InventoryDetailViewModel> GetWarehouseDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null)
        {
            var result = new PagedResult<InventoryDetailViewModel>
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 首先获取所有入库单并解析为明细记录
                    string sql = @"
                        SELECT 
                            wr.ReceiptID,
                            wr.ReceiptNumber,
                            wr.TransportOrderID,
                            wr.RecyclerID,
                            wr.ItemCategories,
                            wr.CreatedDate,
                            r.Username AS RecyclerName,
                            t.OrderNumber AS TransportOrderNumber
                        FROM WarehouseReceipts wr
                        LEFT JOIN Recyclers r ON wr.RecyclerID = r.RecyclerID
                        LEFT JOIN TransportationOrders t ON wr.TransportOrderID = t.TransportOrderID
                        WHERE wr.Status = N'已入库'
                        ORDER BY wr.CreatedDate DESC";

                    var allDetails = new List<InventoryDetailViewModel>();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int receiptID = Convert.ToInt32(reader["ReceiptID"]);
                                string receiptNumber = reader["ReceiptNumber"].ToString();
                                int transportOrderID = Convert.ToInt32(reader["TransportOrderID"]);
                                int recyclerID = Convert.ToInt32(reader["RecyclerID"]);
                                string recyclerName = reader["RecyclerName"] == DBNull.Value ? "未知回收员" : reader["RecyclerName"].ToString();
                                DateTime createdDate = Convert.ToDateTime(reader["CreatedDate"]);
                                string itemCategoriesJson = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString();

                                if (!string.IsNullOrEmpty(itemCategoriesJson))
                                {
                                    try
                                    {
                                        // 解析JSON数组
                                        var categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemCategoriesJson);

                                        foreach (var category in categories)
                                        {
                                            string catKey = category.ContainsKey("categoryKey") ? category["categoryKey"].ToString() : "";
                                            string catName = category.ContainsKey("categoryName") ? category["categoryName"].ToString() : "";
                                            decimal weight = category.ContainsKey("weight") ? Convert.ToDecimal(category["weight"]) : 0;

                                            // 如果指定了类别筛选，则只包含匹配的类别
                                            if (!string.IsNullOrEmpty(categoryKey) && catKey != categoryKey)
                                                continue;

                                            if (!string.IsNullOrEmpty(catKey) && weight > 0)
                                            {
                                                allDetails.Add(new InventoryDetailViewModel
                                                {
                                                    InventoryID = receiptID, // 使用ReceiptID作为标识
                                                    OrderID = transportOrderID,
                                                    OrderNumber = receiptNumber, // 使用入库单号
                                                    CategoryKey = catKey,
                                                    CategoryName = catName,
                                                    Weight = weight,
                                                    Price = null, // 稍后计算
                                                    RecyclerID = recyclerID,
                                                    RecyclerName = recyclerName,
                                                    CreatedDate = createdDate
                                                });
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"解析ItemCategories JSON失败: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // 为每条明细计算价格
                    foreach (var detail in allDetails)
                    {
                        string priceSql = @"
                            SELECT TOP 1 PricePerKg 
                            FROM RecyclableItems 
                            WHERE Category = @Category AND IsActive = 1
                            ORDER BY SortOrder";

                        using (SqlCommand priceCmd = new SqlCommand(priceSql, conn))
                        {
                            priceCmd.Parameters.AddWithValue("@Category", detail.CategoryKey);
                            var priceResult = priceCmd.ExecuteScalar();

                            if (priceResult != null && priceResult != DBNull.Value)
                            {
                                decimal pricePerKg = Convert.ToDecimal(priceResult);
                                detail.Price = detail.Weight * pricePerKg;
                            }
                        }
                    }

                    // 分页
                    result.TotalCount = allDetails.Count;
                    result.Items = allDetails
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseDetailWithRecycler Error: {ex.Message}");
                throw new Exception($"获取仓库明细失败: {ex.Message}", ex);
            }

            return result;
        }
    }
}
