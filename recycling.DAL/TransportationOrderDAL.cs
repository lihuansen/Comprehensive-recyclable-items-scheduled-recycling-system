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
                         ContactPerson, ContactPhone, EstimatedWeight, ItemCategories, 
                         SpecialInstructions, Status, CreatedDate)
                        VALUES 
                        (@OrderNumber, @RecyclerID, @TransporterID, @PickupAddress, @DestinationAddress,
                         @ContactPerson, @ContactPhone, @EstimatedWeight, @ItemCategories, 
                         @SpecialInstructions, @Status, @CreatedDate);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                        cmd.Parameters.AddWithValue("@RecyclerID", order.RecyclerID);
                        cmd.Parameters.AddWithValue("@TransporterID", order.TransporterID);
                        cmd.Parameters.AddWithValue("@PickupAddress", order.PickupAddress);
                        cmd.Parameters.AddWithValue("@DestinationAddress", order.DestinationAddress);
                        cmd.Parameters.AddWithValue("@ContactPerson", order.ContactPerson);
                        cmd.Parameters.AddWithValue("@ContactPhone", order.ContactPhone);
                        cmd.Parameters.AddWithValue("@EstimatedWeight", order.EstimatedWeight);
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
                                    ContactPerson = reader["ContactPerson"].ToString(),
                                    ContactPhone = reader["ContactPhone"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ActualWeight = reader["ActualWeight"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(reader["ActualWeight"]),
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
                                    ContactPerson = reader["ContactPerson"].ToString(),
                                    ContactPhone = reader["ContactPhone"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    ActualWeight = reader["ActualWeight"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(reader["ActualWeight"]),
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
    }
}
