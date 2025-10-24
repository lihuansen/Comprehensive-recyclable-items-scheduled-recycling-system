using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    public class OrderDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 根据用户ID和状态获取订单列表
        /// </summary>
        public List<AppointmentOrder> GetOrdersByUserAndStatus(int userId, string status = "all")
        {
            var orders = new List<AppointmentOrder>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
SELECT 
    a.AppointmentID,
    a.AppointmentType,
    a.AppointmentDate,
    a.TimeSlot,
    a.EstimatedWeight,
    a.IsUrgent,
    a.Address,
    a.ContactName,
    a.ContactPhone,
    a.SpecialInstructions,
    a.EstimatedPrice,
    a.Status,
    a.CreatedDate,
    a.UpdatedDate,
    STUFF((
        SELECT DISTINCT ', ' + ac.CategoryName
        FROM AppointmentCategories ac
        WHERE ac.AppointmentID = a.AppointmentID
        FOR XML PATH('')
    ), 1, 2, '') AS CategoryNames
FROM Appointments a
WHERE a.UserID = @UserID";

                // 根据状态筛选
                if (status != "all")
                {
                    sql += " AND a.Status = @Status";
                }

                sql += " ORDER BY a.CreatedDate DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                if (status != "all")
                {
                    // 映射状态名称：前端"已预约"对应数据库"待确认"
                    string dbStatus = status == "pending" ? "待确认" :
                                     status == "confirmed" ? "进行中" :
                                     status == "completed" ? "已完成" : "已取消";
                    cmd.Parameters.AddWithValue("@Status", dbStatus);
                }

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new AppointmentOrder
                        {
                            AppointmentID = Convert.ToInt32(reader["AppointmentID"]),
                            AppointmentType = reader["AppointmentType"].ToString(),
                            AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                            TimeSlot = reader["TimeSlot"].ToString(),
                            EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                            IsUrgent = Convert.ToBoolean(reader["IsUrgent"]),
                            Address = reader["Address"].ToString(),
                            ContactName = reader["ContactName"].ToString(),
                            ContactPhone = reader["ContactPhone"].ToString(),
                            SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? null : reader["SpecialInstructions"].ToString(),
                            EstimatedPrice = reader["EstimatedPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EstimatedPrice"]),
                            Status = reader["Status"].ToString(),
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"]),
                            CategoryNames = reader["CategoryNames"] == DBNull.Value ? "" : reader["CategoryNames"].ToString()
                        };
                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// 根据订单ID获取订单详情（包含品类详情）
        /// </summary>
        public OrderDetail GetOrderDetail(int appointmentId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
SELECT 
    a.*,
    ac.CategoryID,
    ac.CategoryName,
    ac.CategoryKey,
    ac.QuestionsAnswers,
    ac.CreatedDate as CategoryCreatedDate
FROM Appointments a
LEFT JOIN AppointmentCategories ac ON a.AppointmentID = ac.AppointmentID
WHERE a.AppointmentID = @AppointmentID AND a.UserID = @UserID
ORDER BY ac.CategoryID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    OrderDetail orderDetail = null;
                    var categories = new List<AppointmentCategories>();

                    while (reader.Read())
                    {
                        if (orderDetail == null)
                        {
                            orderDetail = new OrderDetail
                            {
                                Appointment = new Appointments
                                {
                                    AppointmentID = Convert.ToInt32(reader["AppointmentID"]),
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    AppointmentType = reader["AppointmentType"].ToString(),
                                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                    TimeSlot = reader["TimeSlot"].ToString(),
                                    EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                    IsUrgent = Convert.ToBoolean(reader["IsUrgent"]),
                                    Address = reader["Address"].ToString(),
                                    ContactName = reader["ContactName"].ToString(),
                                    ContactPhone = reader["ContactPhone"].ToString(),
                                    SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? null : reader["SpecialInstructions"].ToString(),
                                    EstimatedPrice = reader["EstimatedPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EstimatedPrice"]),
                                    Status = reader["Status"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"])
                                },
                                Categories = categories
                            };
                        }

                        // 添加品类信息（可能有多条记录）
                        if (reader["CategoryID"] != DBNull.Value)
                        {
                            var category = new AppointmentCategories
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                AppointmentID = Convert.ToInt32(reader["AppointmentID"]),
                                CategoryName = reader["CategoryName"].ToString(),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                QuestionsAnswers = reader["QuestionsAnswers"] == DBNull.Value ? null : reader["QuestionsAnswers"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CategoryCreatedDate"])
                            };
                            categories.Add(category);
                        }
                    }

                    return orderDetail;
                }
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        public bool CancelOrder(int appointmentId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
UPDATE Appointments 
SET Status = '已取消', 
    UpdatedDate = @UpdatedDate
WHERE AppointmentID = @AppointmentID 
  AND UserID = @UserID 
  AND Status = '待确认'"; // 只能取消待确认的订单

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}
