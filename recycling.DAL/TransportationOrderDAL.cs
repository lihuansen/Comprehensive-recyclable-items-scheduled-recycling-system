using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 运输单数据访问层
    /// Transportation Orders Data Access Layer
    /// </summary>
    public class TransportationOrderDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
        
        // Cache for column existence checks to avoid repeated database queries
        // Using a static dictionary to share cache across all instances
        private static readonly Dictionary<string, bool> _columnExistsCache = new Dictionary<string, bool>();
        private static readonly object _cacheLock = new object();

        /// <summary>
        /// 安全地检查列是否存在于DataReader中
        /// Safely check if a column exists in the DataReader
        /// Note: For better performance in high-volume scenarios, consider caching column ordinals
        /// in a HashSet at the beginning of each query method. Current implementation prioritizes
        /// simplicity and backward compatibility.
        /// </summary>
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

        /// <summary>
        /// 安全地从DataReader读取可空字符串列
        /// Safely read nullable string column from DataReader
        /// </summary>
        private string SafeGetString(SqlDataReader reader, string columnName)
        {
            if (!ColumnExists(reader, columnName))
                return null;
            
            return reader[columnName] == DBNull.Value ? null : reader[columnName].ToString();
        }

        /// <summary>
        /// 安全地从DataReader读取可空日期时间列
        /// Safely read nullable DateTime column from DataReader
        /// </summary>
        private DateTime? SafeGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!ColumnExists(reader, columnName))
                return null;
            
            var value = reader[columnName];
            return value == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(value);
        }

        /// <summary>
        /// 检查数据库表中是否存在指定列
        /// Check if a column exists in the database table
        /// Results are cached to improve performance
        /// Supports both transactional and non-transactional contexts
        /// </summary>
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
                
                // SqlCommand constructor accepts null for transaction parameter
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
                // If we can't check, assume column doesn't exist to avoid errors
                return false;
            }
        }

        /// <summary>
        /// 生成运输单号
        /// 格式：TO+YYYYMMDD+4位序号
        /// Note: This implementation has a potential race condition in high-concurrency scenarios.
        /// For production use, consider using database sequences or implementing proper locking.
        /// </summary>
        private string GenerateOrderNumber()
        {
            string datePrefix = "TO" + DateTime.Now.ToString("yyyyMMdd");
            int sequence = 1;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Use a transaction with serializable isolation level to prevent race conditions
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

        /// <summary>
        /// 创建运输单
        /// </summary>
        /// <returns>Tuple containing order ID and order number</returns>
        public (int orderId, string orderNumber) CreateTransportationOrder(TransportationOrders order)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Generate order number
                    order.OrderNumber = GenerateOrderNumber();
                    order.CreatedDate = DateTime.Now;
                    order.Status = "待接单";

                    string sql = @"
                        INSERT INTO TransportationOrders 
                        (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress,
                         ContactPerson, ContactPhone, BaseContactPerson, BaseContactPhone, 
                         EstimatedWeight, ItemTotalValue, ItemCategories, 
                         SpecialInstructions, Status, CreatedDate)
                        VALUES 
                        (@OrderNumber, @RecyclerID, @TransporterID, @PickupAddress, @DestinationAddress,
                         @ContactPerson, @ContactPhone, @BaseContactPerson, @BaseContactPhone,
                         @EstimatedWeight, @ItemTotalValue, @ItemCategories, 
                         @SpecialInstructions, @Status, @CreatedDate);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
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

                        int orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        return (orderId, order.OrderNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTransportationOrder Error: {ex.Message}");
                throw new Exception($"创建运输单失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取回收员的运输单列表
        /// </summary>
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
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString(),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate")
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

        /// <summary>
        /// 获取运输单详情
        /// </summary>
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
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString(),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate")
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

        /// <summary>
        /// 更新运输单状态
        /// </summary>
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

        /// <summary>
        /// 获取运输人员的运输单列表
        /// </summary>
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
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString(),
                                    PickupConfirmedDate = SafeGetDateTime(reader, "PickupConfirmedDate"),
                                    ArrivedAtPickupDate = SafeGetDateTime(reader, "ArrivedAtPickupDate"),
                                    LoadingCompletedDate = SafeGetDateTime(reader, "LoadingCompletedDate"),
                                    DeliveryConfirmedDate = SafeGetDateTime(reader, "DeliveryConfirmedDate"),
                                    ArrivedAtDeliveryDate = SafeGetDateTime(reader, "ArrivedAtDeliveryDate")
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

        /// <summary>
        /// 接单（更新状态为已接单并记录接单时间）
        /// </summary>
        public bool AcceptTransportationOrder(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Build UPDATE SQL
                    // Note: We don't set TransportStage here because '接单' is not in the database constraint
                    // The stage will be set when ConfirmPickupLocation is called
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

        /// <summary>
        /// 开始运输（更新状态为运输中并记录取货时间）
        /// Also moves inventory from StoragePoint to InTransit state
        /// DEPRECATED: Use ConfirmPickupLocation instead to follow new workflow
        /// </summary>
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
                            // 1. Get the RecyclerID from the transport order
                            string getRecyclerSql = @"
                                SELECT RecyclerID 
                                FROM TransportationOrders 
                                WHERE TransportOrderID = @OrderID";
                            
                            int recyclerID;
                            using (SqlCommand cmd = new SqlCommand(getRecyclerSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                var result = cmd.ExecuteScalar();
                                if (result == null)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                                recyclerID = Convert.ToInt32(result);
                            }
                            
                            // 2. Check which columns exist
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasPickupConfirmedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "PickupConfirmedDate");
                            
                            // 3. Build dynamic UPDATE SQL based on available columns
                            string updateOrderSql = "UPDATE TransportationOrders SET Status = '运输中', PickupDate = @PickupDate";
                            
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
                                cmd.Parameters.AddWithValue("@PickupDate", DateTime.Now);
                                rowsAffected = cmd.ExecuteNonQuery();
                            }
                            
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            
                            // 4. Move inventory from StoragePoint to InTransit
                            // This ensures goods are not visible in storage point during transport
                            string moveInventorySql = @"
                                UPDATE Inventory 
                                SET InventoryType = N'InTransit'
                                WHERE RecyclerID = @RecyclerID 
                                  AND InventoryType = N'StoragePoint'";

                            int movedRows;
                            using (SqlCommand cmd = new SqlCommand(moveInventorySql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerID);
                                movedRows = cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Moved {movedRows} inventory items from StoragePoint to InTransit for recycler {recyclerID}");
                            }
                            
                            // Validate that inventory was moved
                            // Note: It's valid to have 0 rows if the storage point is empty (goods already transported)
                            // But we log a warning for tracking purposes
                            if (movedRows == 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: No inventory items moved for recycler {recyclerID}. Storage point may be empty.");
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

        /// <summary>
        /// 确认收货地点
        /// </summary>
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
                            // 1. Get the RecyclerID from the transport order
                            string getRecyclerSql = @"
                                SELECT RecyclerID 
                                FROM TransportationOrders 
                                WHERE TransportOrderID = @OrderID";
                            
                            int recyclerID;
                            using (SqlCommand cmd = new SqlCommand(getRecyclerSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                var result = cmd.ExecuteScalar();
                                if (result == null)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                                recyclerID = Convert.ToInt32(result);
                            }
                            
                            // 2. Check which columns exist
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasPickupConfirmedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "PickupConfirmedDate");
                            
                            // 3. Build dynamic UPDATE SQL based on available columns
                            string updateOrderSql = "UPDATE TransportationOrders SET Status = N'运输中'";
                            
                            if (hasTransportStage)
                            {
                                updateOrderSql += ", TransportStage = N'确认收货地点'";
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

        /// <summary>
        /// 到达收货地点
        /// </summary>
        public bool ArriveAtPickupLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Check which columns exist
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasArrivedAtPickupDate = ColumnExistsInTable(conn, null, "TransportationOrders", "ArrivedAtPickupDate");
                    bool hasStage = ColumnExistsInTable(conn, null, "TransportationOrders", "Stage");
                    
                    // Build dynamic UPDATE SQL based on available columns
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
                    
                    if (hasStage)
                    {
                        // Note: Stage uses "到达收货地点" (arrive at receiving location) while TransportStage uses "到达取货地点" (arrive at pickup location)
                        // This is intentional - Stage represents the perspective of the base receiving goods, while TransportStage represents the transporter's perspective
                        setClauses.Add("Stage = N'到达收货地点'");
                    }
                    
                    // If no columns to update, just return success (backward compatibility)
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ArriveAtPickupLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasTransportStage)
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

        /// <summary>
        /// 装货完毕
        /// </summary>
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
                            // 1. Get the RecyclerID from the transport order
                            string getRecyclerSql = @"
                                SELECT RecyclerID 
                                FROM TransportationOrders 
                                WHERE TransportOrderID = @OrderID";
                            
                            int recyclerID;
                            using (SqlCommand cmd = new SqlCommand(getRecyclerSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                var result = cmd.ExecuteScalar();
                                if (result == null)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                                recyclerID = Convert.ToInt32(result);
                            }
                            
                            // 2. Check which columns exist
                            bool hasTransportStage = ColumnExistsInTable(conn, transaction, "TransportationOrders", "TransportStage");
                            bool hasLoadingCompletedDate = ColumnExistsInTable(conn, transaction, "TransportationOrders", "LoadingCompletedDate");
                            
                            // 3. Build dynamic UPDATE SQL based on available columns
                            string updateOrderSql = "UPDATE TransportationOrders SET ";
                            List<string> setClauses = new List<string>();
                            
                            if (hasTransportStage)
                            {
                                setClauses.Add("TransportStage = N'装货完成'");
                            }
                            
                            if (hasLoadingCompletedDate)
                            {
                                setClauses.Add("LoadingCompletedDate = @CompletedDate");
                            }
                            
                            // If no columns to update, just validate status
                            if (setClauses.Count == 0)
                            {
                                updateOrderSql = "SELECT 1 FROM TransportationOrders";
                            }
                            else
                            {
                                updateOrderSql += string.Join(", ", setClauses);
                            }
                            
                            updateOrderSql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                            
                            if (hasTransportStage)
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
                                // For backward compatibility, if no stage columns exist, just check order exists and is in transit
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
                            
                            // 4. Move inventory from StoragePoint to InTransit
                            // This ensures goods are not visible in storage point during transport
                            string moveInventorySql = @"
                                UPDATE Inventory 
                                SET InventoryType = N'InTransit'
                                WHERE RecyclerID = @RecyclerID 
                                  AND InventoryType = N'StoragePoint'";

                            int movedRows;
                            using (SqlCommand cmd = new SqlCommand(moveInventorySql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerID);
                                movedRows = cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Moved {movedRows} inventory items from StoragePoint to InTransit for recycler {recyclerID}");
                            }
                            
                            // Note: It's valid to have 0 rows if the storage point is empty
                            if (movedRows == 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: No inventory items moved for recycler {recyclerID}. Storage point may be empty.");
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

        /// <summary>
        /// 确认送货地点
        /// </summary>
        public bool ConfirmDeliveryLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Check which columns exist
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasDeliveryConfirmedDate = ColumnExistsInTable(conn, null, "TransportationOrders", "DeliveryConfirmedDate");
                    
                    // Build dynamic UPDATE SQL based on available columns
                    string sql = "UPDATE TransportationOrders SET ";
                    List<string> setClauses = new List<string>();
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = N'确认送货地点'");
                    }
                    
                    if (hasDeliveryConfirmedDate)
                    {
                        setClauses.Add("DeliveryConfirmedDate = @ConfirmedDate");
                    }
                    
                    // If no columns to update, just return success (backward compatibility)
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ConfirmDeliveryLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasTransportStage)
                    {
                        // Accept both "装货完成" (new) and "装货完毕" (old) for backward compatibility
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

        /// <summary>
        /// 到达送货地点
        /// </summary>
        public bool ArriveAtDeliveryLocation(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Check which columns exist
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    bool hasArrivedAtDeliveryDate = ColumnExistsInTable(conn, null, "TransportationOrders", "ArrivedAtDeliveryDate");
                    
                    // Build dynamic UPDATE SQL based on available columns
                    string sql = "UPDATE TransportationOrders SET ";
                    List<string> setClauses = new List<string>();
                    
                    if (hasTransportStage)
                    {
                        setClauses.Add("TransportStage = N'到达送货地点'");
                    }
                    
                    if (hasArrivedAtDeliveryDate)
                    {
                        setClauses.Add("ArrivedAtDeliveryDate = @ArrivedDate");
                    }
                    
                    // If no columns to update, just return success (backward compatibility)
                    if (setClauses.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("ArriveAtDeliveryLocation: No transport stage columns available, skipping update");
                        return true;
                    }
                    
                    sql += string.Join(", ", setClauses);
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasTransportStage)
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

        /// <summary>
        /// 完成运输（更新状态为已完成并记录完成时间）
        /// </summary>
        public bool CompleteTransportation(int orderId, decimal? actualWeight)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Check if TransportStage column exists
                    bool hasTransportStage = ColumnExistsInTable(conn, null, "TransportationOrders", "TransportStage");
                    
                    // Build dynamic UPDATE SQL based on available columns
                    string sql = "UPDATE TransportationOrders SET Status = N'已完成', DeliveryDate = @DeliveryDate, CompletedDate = @CompletedDate";
                    
                    if (actualWeight.HasValue)
                    {
                        sql += ", ActualWeight = @ActualWeight";
                    }
                    
                    if (hasTransportStage)
                    {
                        sql += ", TransportStage = NULL";
                    }
                    
                    sql += " WHERE TransportOrderID = @OrderID AND Status = N'运输中'";
                    
                    if (hasTransportStage)
                    {
                        sql += " AND (TransportStage = N'到达送货地点' OR TransportStage IS NULL)";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@DeliveryDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now);
                        
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
