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
    public class RecyclerOrderDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 获取回收员订单列表（带分页和筛选）
        /// </summary>
        public PagedResult<RecyclerOrderViewModel> GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
        {
            var result = new PagedResult<RecyclerOrderViewModel>
            {
                Items = new List<RecyclerOrderViewModel>(),
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 构建查询条件
                var conditions = new List<string>();
                var parameters = new List<SqlParameter>();

                // 订单编号筛选
                if (!string.IsNullOrEmpty(filter.OrderNumber))
                {
                    conditions.Add("a.AppointmentID = @OrderNumber");
                    parameters.Add(new SqlParameter("@OrderNumber", filter.OrderNumber.Replace("AP", "")));
                }

                // 预约日期筛选
                if (filter.AppointmentDate.HasValue)
                {
                    conditions.Add("CONVERT(DATE, a.AppointmentDate) = CONVERT(DATE, @AppointmentDate)");
                    parameters.Add(new SqlParameter("@AppointmentDate", filter.AppointmentDate.Value));
                }

                // 加急订单筛选
                if (filter.IsUrgent.HasValue)
                {
                    conditions.Add("a.IsUrgent = @IsUrgent");
                    parameters.Add(new SqlParameter("@IsUrgent", filter.IsUrgent.Value));
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    conditions.Add("a.Status = @Status");
                    parameters.Add(new SqlParameter("@Status", filter.Status));
                }

                // 回收员关联筛选（如果指定了回收员ID）
                if (recyclerId > 0)
                {
                    conditions.Add("(a.RecyclerID = @RecyclerID OR a.RecyclerID IS NULL)");
                    parameters.Add(new SqlParameter("@RecyclerID", recyclerId));
                }

                string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

                // 获取总数
                string countSql = $@"
                    SELECT COUNT(*) 
                    FROM Appointments a
                    {whereClause}";

                using (SqlCommand countCmd = new SqlCommand(countSql, conn))
                {
                    // 为计数命令创建新的参数实例
                    foreach (var param in parameters)
                    {
                        countCmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }

                    conn.Open();
                    result.TotalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // 获取分页数据
                string dataSql = $@"
                    SELECT 
                        a.AppointmentID,
                        a.AppointmentType,
                        a.AppointmentDate,
                        a.TimeSlot,
                        a.EstimatedWeight,
                        a.EstimatedPrice,
                        a.IsUrgent,
                        a.Address,
                        a.ContactName,
                        a.ContactPhone,
                        a.Status,
                        a.CreatedDate,
                        a.RecyclerID,
                        r.Username as RecyclerName,
                        STUFF((
                            SELECT DISTINCT ', ' + ac.CategoryName
                            FROM AppointmentCategories ac
                            WHERE ac.AppointmentID = a.AppointmentID
                            FOR XML PATH('')
                        ), 1, 2, '') AS CategoryNames
                    FROM Appointments a
                    LEFT JOIN Recyclers r ON a.RecyclerID = r.RecyclerID
                    {whereClause}
                    ORDER BY a.CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand dataCmd = new SqlCommand(dataSql, conn))
                {
                    // 为数据命令创建新的参数实例
                    foreach (var param in parameters)
                    {
                        dataCmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }

                    // 添加分页参数
                    dataCmd.Parameters.Add(new SqlParameter("@Offset", (filter.PageIndex - 1) * filter.PageSize));
                    dataCmd.Parameters.Add(new SqlParameter("@PageSize", filter.PageSize));

                    using (SqlDataReader reader = dataCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new RecyclerOrderViewModel
                            {
                                AppointmentID = Convert.ToInt32(reader["AppointmentID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["AppointmentID"]):D6}",
                                AppointmentType = GetAppointmentTypeChinese(reader["AppointmentType"].ToString()),
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                TimeSlot = GetTimeSlotChinese(reader["TimeSlot"].ToString()),
                                EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]),
                                EstimatedPrice = reader["EstimatedPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EstimatedPrice"]),
                                IsUrgent = Convert.ToBoolean(reader["IsUrgent"]),
                                Address = reader["Address"].ToString(),
                                ContactName = reader["ContactName"].ToString(),
                                ContactPhone = reader["ContactPhone"].ToString(),
                                Status = reader["Status"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                CategoryNames = reader["CategoryNames"] == DBNull.Value ? "" : reader["CategoryNames"].ToString(),
                                RecyclerID = reader["RecyclerID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RecyclerID"]),
                                RecyclerName = reader["RecyclerName"] == DBNull.Value ? null : reader["RecyclerName"].ToString()
                            };
                            result.Items.Add(order);
                        }
                    }
                }
            }

            return result;
        }

        // 在 RecyclerOrderDAL 类中添加这两个辅助方法
        /// <summary>
        /// 获取预约类型的中文显示
        /// </summary>
        private string GetAppointmentTypeChinese(string appointmentType)
        {
            switch (appointmentType?.ToLower())
            {
                case "household":
                    return "家庭回收";
                case "enterprise":
                    return "企业回收";
                default:
                    return appointmentType;
            }
        }

        /// <summary>
        /// 获取时间段的中文显示
        /// </summary>
        private string GetTimeSlotChinese(string timeSlot)
        {
            switch (timeSlot?.ToLower())
            {
                case "morning":
                    return "上午 (9:00-12:00)";
                case "afternoon":
                    return "下午 (13:00-17:00)";
                case "evening":
                    return "晚上 (18:00-21:00)";
                case "all day":
                    return "全天 (9:00-21:00)";
                default:
                    return timeSlot;
            }
        }

        /// <summary>
        /// 获取订单总数统计
        /// </summary>
        public OrderStatistics GetOrderStatistics()
        {
            var statistics = new OrderStatistics();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        COUNT(*) as Total,
                        COUNT(CASE WHEN Status = '待确认' THEN 1 END) as Pending,
                        COUNT(CASE WHEN Status = '进行中' THEN 1 END) as Confirmed,
                        COUNT(CASE WHEN Status = '已完成' THEN 1 END) as Completed,
                        COUNT(CASE WHEN Status = '已取消' THEN 1 END) as Cancelled
                    FROM Appointments";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            statistics.Total = Convert.ToInt32(reader["Total"]);
                            statistics.Pending = Convert.ToInt32(reader["Pending"]);
                            statistics.Confirmed = Convert.ToInt32(reader["Confirmed"]);
                            statistics.Completed = Convert.ToInt32(reader["Completed"]);
                            statistics.Cancelled = Convert.ToInt32(reader["Cancelled"]);
                        }
                    }
                }
            }
            return statistics;
        }

        /// <summary>
        /// 回收员接收订单
        /// </summary>
        public bool AcceptOrder(int appointmentId, int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Appointments 
                    SET Status = '进行中', 
                        RecyclerID = @RecyclerID,
                        UpdatedDate = GETDATE()
                    WHERE AppointmentID = @AppointmentID 
                      AND Status = '待确认'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// 获取回收员订单统计信息
        /// </summary>
        public RecyclerOrderStatistics GetRecyclerOrderStatistics(int recyclerId)
        {
            var statistics = new RecyclerOrderStatistics();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        COUNT(*) as TotalOrders,
                        COUNT(CASE WHEN Status = '待确认' THEN 1 END) as PendingOrders,
                        COUNT(CASE WHEN Status = '进行中' AND RecyclerID = @RecyclerID THEN 1 END) as ConfirmedOrders,
                        COUNT(CASE WHEN Status = '已完成' AND RecyclerID = @RecyclerID THEN 1 END) as CompletedOrders,
                        COUNT(CASE WHEN Status = '已取消' THEN 1 END) as CancelledOrders
                    FROM Appointments";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            statistics.TotalOrders = Convert.ToInt32(reader["TotalOrders"]);
                            statistics.PendingOrders = Convert.ToInt32(reader["PendingOrders"]);
                            statistics.ConfirmedOrders = Convert.ToInt32(reader["ConfirmedOrders"]);
                            statistics.CompletedOrders = Convert.ToInt32(reader["CompletedOrders"]);
                            statistics.CancelledOrders = Convert.ToInt32(reader["CancelledOrders"]);
                        }
                    }
                }
            }
            return statistics;
        }

        /// <summary>
        /// 获取回收员的消息列表
        /// </summary>
        public List<RecyclerMessageViewModel> GetRecyclerMessages(int recyclerId, int pageIndex = 1, int pageSize = 20)
        {
            var messages = new List<RecyclerMessageViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        m.MessageID,
                        m.OrderID,
                        a.AppointmentID as OrderNumber,
                        m.SenderType,
                        m.SenderID,
                        CASE 
                            WHEN m.SenderType = 'user' THEN u.Username
                            WHEN m.SenderType = 'recycler' THEN r.Username
                            ELSE '系统'
                        END as SenderName,
                        m.Content,
                        m.SentTime,
                        m.IsRead
                    FROM Messages m
                    INNER JOIN Appointments a ON m.OrderID = a.AppointmentID
                    LEFT JOIN Users u ON m.SenderType = 'user' AND m.SenderID = u.UserID
                    LEFT JOIN Recyclers r ON m.SenderType = 'recycler' AND m.SenderID = r.RecyclerID
                    WHERE a.RecyclerID = @RecyclerID
                    ORDER BY m.SentTime DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new RecyclerMessageViewModel
                            {
                                MessageID = Convert.ToInt32(reader["MessageID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["OrderNumber"]):D6}",
                                SenderType = reader["SenderType"].ToString(),
                                SenderID = Convert.ToInt32(reader["SenderID"]),
                                SenderName = reader["SenderName"].ToString(),
                                Content = reader["Content"].ToString(),
                                SentTime = Convert.ToDateTime(reader["SentTime"]),
                                IsRead = Convert.ToBoolean(reader["IsRead"])
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// 获取订单的对话消息
        /// </summary>
        public List<RecyclerMessageViewModel> GetOrderConversation(int orderId)
        {
            var messages = new List<RecyclerMessageViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        m.MessageID,
                        m.OrderID,
                        a.AppointmentID as OrderNumber,
                        m.SenderType,
                        m.SenderID,
                        CASE 
                            WHEN m.SenderType = 'user' THEN u.Username
                            WHEN m.SenderType = 'recycler' THEN r.Username
                            ELSE '系统'
                        END as SenderName,
                        m.Content,
                        m.SentTime,
                        m.IsRead
                    FROM Messages m
                    INNER JOIN Appointments a ON m.OrderID = a.AppointmentID
                    LEFT JOIN Users u ON m.SenderType = 'user' AND m.SenderID = u.UserID
                    LEFT JOIN Recyclers r ON m.SenderType = 'recycler' AND m.SenderID = r.RecyclerID
                    WHERE m.OrderID = @OrderID
                    ORDER BY m.SentTime ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new RecyclerMessageViewModel
                            {
                                MessageID = Convert.ToInt32(reader["MessageID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["OrderNumber"]):D6}",
                                SenderType = reader["SenderType"].ToString(),
                                SenderID = Convert.ToInt32(reader["SenderID"]),
                                SenderName = reader["SenderName"].ToString(),
                                Content = reader["Content"].ToString(),
                                SentTime = Convert.ToDateTime(reader["SentTime"]),
                                IsRead = Convert.ToBoolean(reader["IsRead"])
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public bool MarkRecyclerMessagesAsRead(int messageId, int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Messages 
                    SET IsRead = 1
                    FROM Messages m
                    INNER JOIN Appointments a ON m.OrderID = a.AppointmentID
                    WHERE m.MessageID = @MessageID 
                      AND a.RecyclerID = @RecyclerID
                      AND m.SenderType != 'recycler'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageId);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
