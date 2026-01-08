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
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString()
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
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString()
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
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    AcceptedDate = reader["AcceptedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["AcceptedDate"]),
                                    PickupDate = reader["PickupDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["PickupDate"]),
                                    DeliveryDate = reader["DeliveryDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["DeliveryDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CompletedDate"]),
                                    CancelledDate = reader["CancelledDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["CancelledDate"]),
                                    CancelReason = reader["CancelReason"] == DBNull.Value ? null : reader["CancelReason"].ToString(),
                                    TransporterNotes = reader["TransporterNotes"] == DBNull.Value ? null : reader["TransporterNotes"].ToString(),
                                    RecyclerRating = reader["RecyclerRating"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["RecyclerRating"]),
                                    RecyclerReview = reader["RecyclerReview"] == DBNull.Value ? null : reader["RecyclerReview"].ToString()
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
                    string sql = @"
                        UPDATE TransportationOrders 
                        SET Status = '已接单',
                            AcceptedDate = @AcceptedDate
                        WHERE TransportOrderID = @OrderID
                        AND Status = '待接单'";

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
                            
                            // 2. Update transport order status to "运输中"
                            string updateOrderSql = @"
                                UPDATE TransportationOrders 
                                SET Status = '运输中',
                                    PickupDate = @PickupDate
                                WHERE TransportOrderID = @OrderID
                                AND Status = '已接单'";

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
                            
                            // 3. Move inventory from StoragePoint to InTransit
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
        /// 完成运输（更新状态为已完成并记录完成时间）
        /// </summary>
        public bool CompleteTransportation(int orderId, decimal? actualWeight)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // 根据是否提供了实际重量来决定SQL语句
                    string sql;
                    if (actualWeight.HasValue)
                    {
                        sql = @"
                            UPDATE TransportationOrders 
                            SET Status = '已完成',
                                DeliveryDate = @DeliveryDate,
                                CompletedDate = @CompletedDate,
                                ActualWeight = @ActualWeight
                            WHERE TransportOrderID = @OrderID
                            AND Status = '运输中'";
                    }
                    else
                    {
                        sql = @"
                            UPDATE TransportationOrders 
                            SET Status = '已完成',
                                DeliveryDate = @DeliveryDate,
                                CompletedDate = @CompletedDate
                            WHERE TransportOrderID = @OrderID
                            AND Status = '运输中'";
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
