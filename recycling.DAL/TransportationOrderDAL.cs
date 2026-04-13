using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// 运输单数据访问层
    /// 运输订单数据访问处理类。
    public class TransportationOrderDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
        
        private static readonly Dictionary<string, bool> _columnExistsCache = new Dictionary<string, bool>();
        private static readonly object _cacheLock = new object();

        /// 安全地检查列是否存在于DataReader中
        /// 判断读取结果中是否包含指定列。
        private bool ColumnExists(SqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        /// 安全地从DataReader读取可空字符串列
        /// 安全读取字符串字段，避免空值异常。
        private string SafeGetString(SqlDataReader reader, string columnName)
        {
            if (!ColumnExists(reader, columnName))
                return null;
            
            return reader[columnName] == DBNull.Value ? null : reader[columnName].ToString();
        }

        /// 安全地从DataReader读取可空日期时间列
        /// 安全读取日期字段，避免空值异常。
        private DateTime? SafeGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!ColumnExists(reader, columnName))
                return null;
            
            var value = reader[columnName];
            return value == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(value);
        }

        /// 安全地从DataReader读取可空整数列
        /// 安全读取整数字段，避免空值异常。
        private int? SafeGetInt(SqlDataReader reader, string columnName)
        {
            if (!ColumnExists(reader, columnName))
                return null;
            
            var value = reader[columnName];
            return value == DBNull.Value ? null : (int?)Convert.ToInt32(value);
        }

        /// 检查数据库表中是否存在指定列
        /// 判断数据表中是否存在指定字段。
        private bool ColumnExistsInTable(SqlConnection conn, SqlTransaction transaction, string tableName, string columnName)
        {
            string cacheKey = $"{tableName}.{columnName}";
            
            lock (_cacheLock)
            {
                if (_columnExistsCache.ContainsKey(cacheKey))
                {
                    return _columnExistsCache[cacheKey];
                }
            }
            
            try
            {
                string sql = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TableName 
                    AND COLUMN_NAME = @ColumnName";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    cmd.Parameters.AddWithValue("@ColumnName", columnName);
                    
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    bool exists = count > 0;
                    
                    lock (_cacheLock)
                    {
                        _columnExistsCache[cacheKey] = exists;
                    }
                    
                    return exists;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ColumnExistsInTable Error: {ex.Message}");
                return false;
            }
        }

        /// 生成运输单号
        /// 格式：TO+YYYYMMDD+4位序号
        /// 生成唯一运输订单编号。
        private string GenerateOrderNumber()
        {
            string datePrefix = "TO" + DateTime.Now.ToString("yyyyMMdd");
            int sequence = 1;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                using (SqlTransaction transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        string sql = "SELECT COUNT(*) FROM TransportationOrders WITH (TABLOCKX) WHERE OrderNumber LIKE @DatePrefix + '%'";
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

        /// 创建运输单
        /// <param name="order">运输单信息</param>
        /// <param name="categories">品类明细列表（可选）</param>
        /// <returns>中文说明</returns>
        public (int orderId, string orderNumber) CreateTransportationOrder(TransportationOrders order, List<TransportationOrderCategories> categories = null)
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
                            order.OrderNumber = GenerateOrderNumber();
                            order.CreatedDate = DateTime.Now;
                            order.Status = "待接单";

                            string sql = @"
                                INSERT INTO TransportationOrders 
                                (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress,
                                 ContactPerson, ContactPhone, BaseContactPerson, BaseContactPhone, 
                                 EstimatedWeight, ItemTotalValue, ItemCategories, 
                                 SpecialInstructions, Status, CreatedDate, AssignedWorkerID)
                                VALUES 
                                (@OrderNumber, @RecyclerID, @TransporterID, @PickupAddress, @DestinationAddress,
                                 @ContactPerson, @ContactPhone, @BaseContactPerson, @BaseContactPhone,
                                 @EstimatedWeight, @ItemTotalValue, @ItemCategories, 
                                 @SpecialInstructions, @Status, @CreatedDate, @AssignedWorkerID);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            int orderId;
                            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                cmd.Parameters.AddWithValue("@RecyclerID", order.RecyclerID);
                                cmd.Parameters.AddWithValue("@TransporterID", order.TransporterID);
                                cmd.Parameters.AddWithValue("@PickupAddress", order.PickupAddress);
                                cmd.Parameters.AddWithValue("@DestinationAddress", order.DestinationAddress);
                                cmd.Parameters.AddWithValue("@ContactPerson", (object)order.ContactPerson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ContactPhone", (object)order.ContactPhone ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@BaseContactPerson", (object)order.BaseContactPerson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@BaseContactPhone", (object)order.BaseContactPhone ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@EstimatedWeight", order.EstimatedWeight);
                                cmd.Parameters.AddWithValue("@ItemTotalValue", order.ItemTotalValue);
                                cmd.Parameters.AddWithValue("@ItemCategories", (object)order.ItemCategories ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@SpecialInstructions", (object)order.SpecialInstructions ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", order.Status);
                                cmd.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
                                cmd.Parameters.AddWithValue("@AssignedWorkerID", (object)order.AssignedWorkerID ?? DBNull.Value);

                                orderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // 如果提供了品类明细列表，则保存到 TransportationOrderCategories 表
                            if (categories != null && categories.Count > 0)
                            {
                                var categoriesDAL = new TransportationOrderCategoriesDAL();
                                // 检查表是否存在
                                if (categoriesDAL.TableExists())
                                {
                                    categoriesDAL.BatchInsertCategories(conn, transaction, orderId, categories);
                                    System.Diagnostics.Debug.WriteLine($"保存了 {categories.Count} 条品类明细记录到 TransportationOrderCategories 表");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("TransportationOrderCategories 表不存在，跳过品类明细保存");
                                }
                            }

                            // 创建运输单时立即将当前暂存点物品转为运输中，确保后续新增物品可独立创建新运输单
                            string moveInventorySql = @"
                                UPDATE Inventory
                                SET InventoryType = N'InTransit'
                                WHERE RecyclerID = @RecyclerID
                                  AND InventoryType = N'StoragePoint'";
                            using (SqlCommand moveCmd = new SqlCommand(moveInventorySql, conn, transaction))
                            {
                                moveCmd.Parameters.AddWithValue("@RecyclerID", order.RecyclerID);
                                int movedRows = moveCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"CreateTransportationOrder moved {movedRows} storage items to InTransit for recycler {order.RecyclerID}");
                            }

                            transaction.Commit();
                            return (orderId, order.OrderNumber);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rollback in CreateTransportationOrder: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTransportationOrder Error: {ex.Message}");
                throw new Exception($"创建运输单失败: {ex.Message}", ex);
            }
        }

        /// 更新基地工作人员的当前状态
        /// 更新分拣中心工作人员状态。
        /// <param name="workerId">基地工作人员ID</param>
        /// <param name="status">新状态（空闲/工作中）</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateSortingCenterWorkerStatus(int workerId, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE SortingCenterWorkers SET CurrentStatus = @Status WHERE WorkerID = @WorkerID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSortingCenterWorkerStatus Error: {ex.Message}");
                throw new Exception($"更新基地工作人员状态失败: {ex.Message}", ex);
            }
        }

        /// 检查基地工作人员是否有未完成入库的运输单
        /// 检查分拣员是否存在待处理任务。
        /// <param name="workerId">基地工作人员ID</param>
        /// <returns>是否有未完成的任务</returns>
        public bool HasPendingTasksForWorker(int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 检查是否有指定给该工作人员的、尚未入库的运输单
                    // （状态不是已取消，且没有已入库的入库单）
                    string sql = @"
                        SELECT COUNT(*) 
                        FROM TransportationOrders t
                        LEFT JOIN WarehouseReceipts w ON t.TransportOrderID = w.TransportOrderID AND w.Status = N'已入库'
                        WHERE t.AssignedWorkerID = @WorkerID 
                        AND t.Status != N'已取消'
                        AND w.ReceiptID IS NULL";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasPendingTasksForWorker Error: {ex.Message}");
                return false;
            }
        }

        /// 获取运输单的指定基地工作人员ID
        /// 获取运输订单分配的分拣员编号。
        public int? GetAssignedWorkerIdByTransportOrderId(int transportOrderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT AssignedWorkerID FROM TransportationOrders WHERE TransportOrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", transportOrderId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return null;
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAssignedWorkerIdByTransportOrderId Error: {ex.Message}");
                return null;
            }
        }

        /// 更新运输人员的当前状态
        /// 更新运输员状态。
        /// <param name="transporterId">运输人员ID</param>
        /// <param name="status">新状态（空闲/运输中）</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateTransporterStatus(int transporterId, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE Transporters SET CurrentStatus = @Status WHERE TransporterID = @TransporterID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@TransporterID", transporterId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTransporterStatus Error: {ex.Message}");
                throw new Exception($"更新运输人员状态失败: {ex.Message}", ex);
            }
        }

        /// 累加运输人员的总运输重量
        /// 更新运输员累计承运重量。
        /// <param name="transporterId">运输人员ID</param>
        /// <param name="weight">本次运输重量（公斤）</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateTransporterTotalWeight(int transporterId, decimal weight)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE Transporters SET TotalWeight = ISNULL(TotalWeight, 0) + @Weight WHERE TransporterID = @TransporterID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Weight", weight);
                        cmd.Parameters.AddWithValue("@TransporterID", transporterId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTransporterTotalWeight Error: {ex.Message}");
                throw new Exception($"更新运输人员总重量失败: {ex.Message}", ex);
            }
        }

        /// 检查运输人员是否有未完成的运输单
        /// 检查运输员是否存在进行中的运输订单。
        /// <param name="transporterId">运输人员ID</param>
        /// <returns>是否有未完成的运输单</returns>
        public bool HasActiveTransportOrdersForTransporter(int transporterId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT COUNT(*) 
                        FROM TransportationOrders 
                        WHERE TransporterID = @TransporterID 
                        AND Status NOT IN (N'已完成', N'已取消')";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransporterID", transporterId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasActiveTransportOrdersForTransporter Error: {ex.Message}");
                throw new Exception($"检查运输人员未完成运输单失败: {ex.Message}", ex);
            }
        }

        /// 检查回收员是否有未完成的运输单（即暂存点物品已创建了运输单但尚未完成）
        /// 检查回收员是否存在进行中的运输订单。
        /// <param name="recyclerId">回收员ID</param>
        /// <returns>是否有未完成的运输单</returns>
        public bool HasActiveTransportOrdersForRecycler(int recyclerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT COUNT(*) 
                        FROM TransportationOrders 
                        WHERE RecyclerID = @RecyclerID 
                        AND Status NOT IN (N'已完成', N'已取消')";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasActiveTransportOrdersForRecycler Error: {ex.Message}");
                throw new Exception($"检查回收员未完成运输单失败: {ex.Message}", ex);
            }
        }

        /// 获取回收员的运输单列表
        public List<TransportationOrders> GetTransportationOrdersByRecycler(int recyclerId)
        {
            var orders = new List<TransportationOrders>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.*, 
                               r.FullName AS RecyclerName,
                               tr.FullName AS TransporterName, tr.PhoneNumber AS TransporterPhone
                        FROM TransportationOrders t
                        LEFT JOIN Recyclers r ON t.RecyclerID = r.RecyclerID
                        LEFT JOIN Transporters tr ON t.TransporterID = tr.TransporterID
                        WHERE t.RecyclerID = @RecyclerID
                        ORDER BY t.CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new TransportationOrders
                                {
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    OrderNumber = reader["OrderNumber"].ToString(),
                                    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                    TransporterID = Convert.ToInt32(reader["TransporterID"]),
                                    PickupAddress = reader["PickupAddress"].ToString(),
                                    DestinationAddress = reader["DestinationAddress"].ToString(),
                                    ContactPerson = reader["ContactPerson"] == DBNull.Value ? null : reader["ContactPerson"].ToString(),
                                    ContactPhone = reader["ContactPhone"] == DBNull.Value ? null : reader["ContactPhone"].ToString(),
                                    BaseContactPerson = reader["BaseContactPerson"] == DBNull.Value ? null : reader["BaseContactPerson"].ToString(),
                                    BaseContactPhone = reader["BaseContactPhone"] == DBNull.Value ? null : reader["BaseContactPhone"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ActualWeight = reader["ActualWeight"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(reader["ActualWeight"]),
                                    ItemTotalValue = reader["ItemTotalValue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ItemTotalValue"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? null : reader["SpecialInstructions"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    TransportStage = SafeGetString(reader, "TransportStage"),
                                    Stage = SafeGetString(reader, "Stage"),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = SafeGetDateTime(reader, "AcceptedDate"),
                                    PickupDate = SafeGetDateTime(reader, "PickupDate"),
                                    DeliveryDate = SafeGetDateTime(reader, "DeliveryDate"),
                                    CompletedDate = SafeGetDateTime(reader, "CompletedDate"),
                                    CancelledDate = SafeGetDateTime(reader, "CancelledDate"),
                                    CancelReason = SafeGetString(reader, "CancelReason"),
                                    RecyclerRating = SafeGetInt(reader, "RecyclerRating"),
                                    RecyclerReview = SafeGetString(reader, "RecyclerReview"),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate"),
                                    AssignedWorkerID = SafeGetInt(reader, "AssignedWorkerID")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrdersByRecycler Error: {ex.Message}");
                throw new Exception($"获取运输单列表失败: {ex.Message}", ex);
            }

            return orders;
        }

        /// 获取运输单详情
        public TransportationOrders GetTransportationOrderById(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT * FROM TransportationOrders 
                        WHERE TransportOrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new TransportationOrders
                                {
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    OrderNumber = reader["OrderNumber"].ToString(),
                                    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                    TransporterID = Convert.ToInt32(reader["TransporterID"]),
                                    PickupAddress = reader["PickupAddress"].ToString(),
                                    DestinationAddress = reader["DestinationAddress"].ToString(),
                                    ContactPerson = reader["ContactPerson"] == DBNull.Value ? null : reader["ContactPerson"].ToString(),
                                    ContactPhone = reader["ContactPhone"] == DBNull.Value ? null : reader["ContactPhone"].ToString(),
                                    BaseContactPerson = reader["BaseContactPerson"] == DBNull.Value ? null : reader["BaseContactPerson"].ToString(),
                                    BaseContactPhone = reader["BaseContactPhone"] == DBNull.Value ? null : reader["BaseContactPhone"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ActualWeight = reader["ActualWeight"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(reader["ActualWeight"]),
                                    ItemTotalValue = reader["ItemTotalValue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ItemTotalValue"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? null : reader["SpecialInstructions"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    TransportStage = SafeGetString(reader, "TransportStage"),
                                    Stage = SafeGetString(reader, "Stage"),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = SafeGetDateTime(reader, "AcceptedDate"),
                                    PickupDate = SafeGetDateTime(reader, "PickupDate"),
                                    DeliveryDate = SafeGetDateTime(reader, "DeliveryDate"),
                                    CompletedDate = SafeGetDateTime(reader, "CompletedDate"),
                                    CancelledDate = SafeGetDateTime(reader, "CancelledDate"),
                                    CancelReason = SafeGetString(reader, "CancelReason"),
                                    RecyclerRating = SafeGetInt(reader, "RecyclerRating"),
                                    RecyclerReview = SafeGetString(reader, "RecyclerReview"),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate"),
                                    AssignedWorkerID = SafeGetInt(reader, "AssignedWorkerID")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrderById Error: {ex.Message}");
                throw new Exception($"获取运输单详情失败: {ex.Message}", ex);
            }

            return null;
        }

        /// 更新运输单状态
        public bool UpdateTransportationOrderStatus(int orderId, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE TransportationOrders 
                        SET Status = @Status
                        WHERE TransportOrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@Status", status);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTransportationOrderStatus Error: {ex.Message}");
                throw new Exception($"更新运输单状态失败: {ex.Message}", ex);
            }
        }

        /// 获取运输人员的运输单列表
        /// <param name="transporterId">运输人员ID</param>
        /// <param name="status">可选的状态筛选</param>
        public List<TransportationOrders> GetTransportationOrdersByTransporter(int transporterId, string status = null)
        {
            var orders = new List<TransportationOrders>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT t.*, 
                               r.FullName AS RecyclerName, r.PhoneNumber AS RecyclerPhone,
                               tr.FullName AS TransporterName, tr.PhoneNumber AS TransporterPhone
                        FROM TransportationOrders t
                        LEFT JOIN Recyclers r ON t.RecyclerID = r.RecyclerID
                        LEFT JOIN Transporters tr ON t.TransporterID = tr.TransporterID
                        WHERE t.TransporterID = @TransporterID";
                    
                    // 如果提供了状态参数，添加状态筛选
                    if (!string.IsNullOrEmpty(status) && status != "all")
                    {
                        sql += " AND t.Status = @Status";
                    }
                    
                    sql += " ORDER BY t.CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransporterID", transporterId);
                        
                        if (!string.IsNullOrEmpty(status) && status != "all")
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new TransportationOrders
                                {
                                    TransportOrderID = Convert.ToInt32(reader["TransportOrderID"]),
                                    OrderNumber = reader["OrderNumber"].ToString(),
                                    RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                    TransporterID = Convert.ToInt32(reader["TransporterID"]),
                                    PickupAddress = reader["PickupAddress"].ToString(),
                                    DestinationAddress = reader["DestinationAddress"].ToString(),
                                    ContactPerson = reader["ContactPerson"] == DBNull.Value ? null : reader["ContactPerson"].ToString(),
                                    ContactPhone = reader["ContactPhone"] == DBNull.Value ? null : reader["ContactPhone"].ToString(),
                                    BaseContactPerson = reader["BaseContactPerson"] == DBNull.Value ? null : reader["BaseContactPerson"].ToString(),
                                    BaseContactPhone = reader["BaseContactPhone"] == DBNull.Value ? null : reader["BaseContactPhone"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ActualWeight = reader["ActualWeight"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(reader["ActualWeight"]),
                                    ItemTotalValue = reader["ItemTotalValue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ItemTotalValue"]),
                                    ItemCategories = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString(),
                                    SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? null : reader["SpecialInstructions"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    TransportStage = SafeGetString(reader, "TransportStage"),
                                    Stage = SafeGetString(reader, "Stage"),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = SafeGetDateTime(reader, "AcceptedDate"),
                                    PickupDate = SafeGetDateTime(reader, "PickupDate"),
                                    DeliveryDate = SafeGetDateTime(reader, "DeliveryDate"),
                                    CompletedDate = SafeGetDateTime(reader, "CompletedDate"),
                                    CancelledDate = SafeGetDateTime(reader, "CancelledDate"),
                                    CancelReason = SafeGetString(reader, "CancelReason"),
                                    RecyclerRating = SafeGetInt(reader, "RecyclerRating"),
                                    RecyclerReview = SafeGetString(reader, "RecyclerReview"),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate"),
                                    AssignedWorkerID = SafeGetInt(reader, "AssignedWorkerID")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrdersByTransporter Error: {ex.Message}");
                throw new Exception($"获取运输单列表失败: {ex.Message}", ex);
            }

            return orders;
        }

        /// 接单（更新状态为已接单并记录接单时间）
        public bool AcceptTransportationOrder(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Note: We don't set TransportStage here because '接单' is not in the database constraint
                    string sql = @"
                        UPDATE TransportationOrders 
                        SET Status = N'已接单',
                            AcceptedDate = @AcceptedDate
                        WHERE TransportOrderID = @OrderID
                        AND Status = N'待接单'";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@AcceptedDate", DateTime.Now);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AcceptTransportationOrder Error: {ex.Message}");
                throw new Exception($"接单失败: {ex.Message}", ex);
            }
        }

        /// 开始运输（更新状态为运输中并记录取货时间）
        /// 启动运输流程。
        public bool StartTransportation(int orderId)
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
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasPickupConfirmedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "PickupConfirmedDate");
                            bool hasPickupDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "PickupDate");
                            
                            string updateOrderSql = "UPDATE TransportationOrders SET Status = '运输中'";
                            
                            if (hasPickupDate)
                            {
                                updateOrderSql += ", PickupDate = @PickupDate";
                            }
                            
                            if (hasTransportStage)
                            {
                                updateOrderSql += ", TransportStage = N'确认收货地点'";
                            }
                            
                            if (hasPickupConfirmedDate)
                            {
                                updateOrderSql += ", PickupConfirmedDate = @PickupDate";
                            }
                            
                            updateOrderSql += " WHERE TransportOrderID = @OrderID AND Status = '已接单'";

                            int rowsAffected;
                            using (SqlCommand cmd = new SqlCommand(updateOrderSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                if (hasPickupDate || hasPickupConfirmedDate)
                                {
                                    cmd.Parameters.AddWithValue("@PickupDate", DateTime.Now);
                                }
                                rowsAffected = cmd.ExecuteNonQuery();
                            }
                            
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rollback in StartTransportation: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartTransportation Error: {ex.Message}");
                throw new Exception($"开始运输失败: {ex.Message}", ex);
            }
        }

        /// 确认收货地点
        public bool ConfirmPickupLocation(int orderId)
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
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasPickupConfirmedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "PickupConfirmedDate");
                            bool hasStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "Stage");
                            
                            string updateOrderSql = "UPDATE TransportationOrders SET Status = N'运输中'";
                            
                            if (hasTransportStage)
                            {
                                updateOrderSql += ", TransportStage = N'确认收货地点'";
                            }
                            
                            if (hasStage)
                            {
                                updateOrderSql += ", Stage = N'确认收货地点'";
                            }
                            
                            if (hasPickupConfirmedDate)
                            {
                                updateOrderSql += ", PickupConfirmedDate = @ConfirmedDate";
                            }
                            
                            updateOrderSql += " WHERE TransportOrderID = @OrderID AND Status = N'已接单'";

                            int rowsAffected;
                            using (SqlCommand cmd = new SqlCommand(updateOrderSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                
                                if (hasPickupConfirmedDate)
                                {
                                    cmd.Parameters.AddWithValue("@ConfirmedDate", DateTime.Now);
                                }
                                
                                rowsAffected = cmd.ExecuteNonQuery();
                            }
                            
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rollback in ConfirmPickupLocation: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfirmPickupLocation Error: {ex.Message}");
                throw new Exception($"确认收货地点失败: {ex.Message}", ex);
            }
        }

        /// 到达收货地点
        public bool ArriveAtPickupLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasArrivedAtPickupDate = ColumnExistsInTable(conn, null, "TransportationOrders", "ArrivedAtPickupDate");
                    bool hasStage = ColumnExistsInTable(conn, null, "TransportationOrders", "Stage");
                    
                    string sql = "UPDATE TransportationOrders SET ";
                    List<string> setClauses = new List<string>();
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = N'到达收货地点'");
                    }
                    
                    if (hasArrivedAtPickupDate)
                    {
                        setClauses.Add("ArrivedAtPickupDate = @ArrivedDate");
                    }
                    
                    // Uses '到达收货地点' (arrive at receiving location) which is the standardized terminology
                    if (hasStage)
                    {
                        setClauses.Add("Stage = N'到达收货地点'");
                    }
                    
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ArriveAtPickupLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    // Accept both '确认收货地点' (correct) and '确认取货地点' (legacy) for backward compatibility
                    if (hasStage)
                    {
                        sql += " AND (Stage = N'确认收货地点' OR Stage = N'确认取货地点')";
                    }
                    else if (hasTransportStage)
                    {
                        sql += " AND (TransportStage = N'确认收货地点' OR TransportStage = N'确认取货地点')";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        
                        if (hasArrivedAtPickupDate)
                        {
                            cmd.Parameters.AddWithValue("@ArrivedDate", DateTime.Now);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArriveAtPickupLocation Error: {ex.Message}");
                throw new Exception($"到达收货地点失败: {ex.Message}", ex);
            }
        }

        /// 装货完毕
        public bool CompleteLoading(int orderId)
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
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasLoadingCompletedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "LoadingCompletedDate");
                            bool hasStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "Stage");
                            
                            string updateOrderSql = "UPDATE TransportationOrders SET ";
                            List<string> setClauses = new List<string>();
                            
                            if (hasTransportStage)
                            {
                                setClauses.Add("TransportStage = N'装货完成'");
                            }
                            
                            if (hasStage)
                            {
                                setClauses.Add("Stage = N'装货完成'");
                            }
                            
                            if (hasLoadingCompletedDate)
                            {
                                setClauses.Add("LoadingCompletedDate = @CompletedDate");
                            }
                            
                            if (setClauses.Count == 0)
                            {
                                updateOrderSql = "SELECT 1 FROM TransportationOrders";
                            }
                            else
                            {
                                updateOrderSql += string.Join(", ", setClauses);
                            }
                            
                            updateOrderSql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                            
                            // Accept both '到达收货地点' (correct) and '到达取货地点' (legacy) for backward compatibility
                            if (hasStage)
                            {
                                updateOrderSql += " AND (Stage = N'到达收货地点' OR Stage = N'到达取货地点')";
                            }
                            else if (hasTransportStage)
                            {
                                updateOrderSql += " AND (TransportStage = N'到达收货地点' OR TransportStage = N'到达取货地点')";
                            }

                            int rowsAffected = 0;
                            if (setClauses.Count > 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(updateOrderSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                                    
                                    if (hasLoadingCompletedDate)
                                    {
                                        cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now);
                                    }
                                    
                                    rowsAffected = cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand(updateOrderSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                                    var result = cmd.ExecuteScalar();
                                    rowsAffected = result != null ? 1 : 0;
                                }
                            }
                            
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rollback in CompleteLoading: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteLoading Error: {ex.Message}");
                throw new Exception($"装货完毕失败: {ex.Message}", ex);
            }
        }

        /// 确认送货地点
        public bool ConfirmDeliveryLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasDeliveryConfirmedDate = ColumnExistsInTable(conn, null, "TransportationOrders", "DeliveryConfirmedDate");
                    bool hasStage = ColumnExistsInTable(conn, null, "TransportationOrders", "Stage");
                    
                    string sql = "UPDATE TransportationOrders SET ";
                    List<string> setClauses = new List<string>();
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = N'确认送货地点'");
                    }
                    
                    if (hasStage)
                    {
                        setClauses.Add("Stage = N'确认送货地点'");
                    }
                    
                    if (hasDeliveryConfirmedDate)
                    {
                        setClauses.Add("DeliveryConfirmedDate = @ConfirmedDate");
                    }
                    
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ConfirmDeliveryLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    // Accept both "装货完成" (new) and "装货完毕" (old) for backward compatibility
                    if (hasStage)
                    {
                        sql += " AND (Stage = N'装货完成' OR Stage = N'装货完毕')";
                    }
                    else if (hasTransportStage)
                    {
                        sql += " AND (TransportStage = N'装货完成' OR TransportStage = N'装货完毕')";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        
                        if (hasDeliveryConfirmedDate)
                        {
                            cmd.Parameters.AddWithValue("@ConfirmedDate", DateTime.Now);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfirmDeliveryLocation Error: {ex.Message}");
                throw new Exception($"确认送货地点失败: {ex.Message}", ex);
            }
        }

        /// 到达送货地点
        public bool ArriveAtDeliveryLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasArrivedAtDeliveryDate = ColumnExistsInTable(conn, null, "TransportationOrders", "ArrivedAtDeliveryDate");
                    bool hasStage = ColumnExistsInTable(conn, null, "TransportationOrders", "Stage");
                    
                    string sql = "UPDATE TransportationOrders SET ";
                    List<string> setClauses = new List<string>();
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = N'到达送货地点'");
                    }
                    
                    if (hasStage)
                    {
                        setClauses.Add("Stage = N'到达送货地点'");
                    }
                    
                    if (hasArrivedAtDeliveryDate)
                    {
                        setClauses.Add("ArrivedAtDeliveryDate = @ArrivedDate");
                    }
                    
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ArriveAtDeliveryLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasStage)
                    {
                        sql += " AND Stage = N'确认送货地点'";
                    }
                    else if (hasTransportStage)
                    {
                        sql += " AND TransportStage = N'确认送货地点'";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        
                        if (hasArrivedAtDeliveryDate)
                        {
                            cmd.Parameters.AddWithValue("@ArrivedDate", DateTime.Now);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArriveAtDeliveryLocation Error: {ex.Message}");
                throw new Exception($"到达送货地点失败: {ex.Message}", ex);
            }
        }

        /// 完成运输（更新状态为已完成并记录完成时间）
        public bool CompleteTransportation(int orderId, decimal? actualWeight)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasStage = ColumnExistsInTable(conn, null, "TransportationOrders", "Stage");
                    bool hasDeliveryDate = ColumnExistsInTable(conn, null, "TransportationOrders", "DeliveryDate");
                    bool hasCompletedDate = ColumnExistsInTable(conn, null, "TransportationOrders", "CompletedDate");
                    
                    List<string> setClauses = new List<string>();
                    setClauses.Add("Status = N'已完成'");
                    
                    if (hasDeliveryDate)
                    {
                        setClauses.Add("DeliveryDate = @DeliveryDate");
                    }
                    
                    if (hasCompletedDate)
                    {
                        setClauses.Add("CompletedDate = @CompletedDate");
                    }
                    
                    if (actualWeight.HasValue)
                    {
                        setClauses.Add("ActualWeight = @ActualWeight");
                    }
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = NULL");
                    }
                    
                    if (hasStage)
                    {
                        setClauses.Add("Stage = NULL");
                    }
                    
                    string sql = "UPDATE TransportationOrders SET " + string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasStage)
                    {
                        sql += " AND (Stage = N'到达送货地点' OR Stage IS NULL)";
                    }
                    else if (hasTransportStage)
                    {
                        sql += " AND (TransportStage = N'到达送货地点' OR TransportStage IS NULL)";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        
                        if (hasDeliveryDate)
                        {
                            cmd.Parameters.AddWithValue("@DeliveryDate", DateTime.Now);
                        }
                        
                        if (hasCompletedDate)
                        {
                            cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now);
                        }
                        
                        if (actualWeight.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ActualWeight", actualWeight.Value);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteTransportation Error: {ex.Message}");
                throw new Exception($"完成运输失败: {ex.Message}", ex);
            }
        }
    }
}
