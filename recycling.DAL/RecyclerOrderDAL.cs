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

            // 获取回收员的区域信息（在构建查询前获取，避免N+1查询问题）
            string recyclerRegion = null;
            if (recyclerId > 0)
            {
                recyclerRegion = GetRecyclerRegion(recyclerId);
            }

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
                    // 使用统一的过滤条件构建方法
                    conditions.Add(BuildRecyclerFilterCondition(recyclerId, recyclerRegion, "a"));
                    parameters.Add(new SqlParameter("@RecyclerID", recyclerId));
                    
                    // 如果有区域过滤，添加区域参数
                    if (!string.IsNullOrEmpty(recyclerRegion))
                    {
                        parameters.Add(new SqlParameter("@RecyclerRegion", "%" + recyclerRegion + "%"));
                    }
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
            if (string.IsNullOrEmpty(appointmentType))
                return appointmentType;

            var cleanAppointmentType = appointmentType.Trim().ToLower();

            switch (cleanAppointmentType)
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
            if (string.IsNullOrEmpty(timeSlot))
                return timeSlot;

            var cleanTimeSlot = timeSlot.Trim().ToLower().Replace(" ", "");

            switch (cleanTimeSlot)
            {
                case "morning":
                    return "上午 (9:00-12:00)";
                case "afternoon":
                    return "下午 (13:00-17:00)";
                case "evening":
                    return "晚上 (18:00-21:00)";
                case "all_day":
                case "allday":
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
                        COUNT(CASE WHEN Status = '已预约' THEN 1 END) as Pending,
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
                      AND Status = '已预约'";

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
                // 获取回收员的区域信息
                string recyclerRegion = GetRecyclerRegion(recyclerId);
                
                // 使用统一的过滤条件构建方法
                string whereCondition = BuildRecyclerFilterCondition(recyclerId, recyclerRegion, "");
                
                string sql = $@"
                    SELECT 
                        COUNT(*) as TotalOrders,
                        COUNT(CASE WHEN Status = '已预约' THEN 1 END) as PendingOrders,
                        COUNT(CASE WHEN Status = '进行中' AND RecyclerID = @RecyclerID THEN 1 END) as ConfirmedOrders,
                        COUNT(CASE WHEN Status = '已完成' AND RecyclerID = @RecyclerID THEN 1 END) as CompletedOrders,
                        COUNT(CASE WHEN Status = '已取消' THEN 1 END) as CancelledOrders
                    FROM Appointments
                    WHERE {whereCondition}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    
                    if (!string.IsNullOrEmpty(recyclerRegion))
                    {
                        cmd.Parameters.AddWithValue("@RecyclerRegion", "%" + recyclerRegion + "%");
                    }

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
                                SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : 0,
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
                                SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : 0,
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

        /// <summary>
        /// 获取订单详情
        /// </summary>
        public OrderDetailModel GetOrderDetail(int appointmentId, int recyclerId)
        {
            var orderDetail = new OrderDetailModel();
            
            // 获取回收员的区域信息
            string recyclerRegion = GetRecyclerRegion(recyclerId);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 使用统一的过滤条件构建方法
                string recyclerFilter = BuildRecyclerFilterCondition(recyclerId, recyclerRegion, "a");
                string whereClause = $"WHERE a.AppointmentID = @AppointmentID AND ({recyclerFilter})";
                
                string sql = $@"
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
                a.UpdatedDate,
                a.SpecialInstructions,
                STUFF((
                    SELECT DISTINCT ', ' + ac.CategoryName
                    FROM AppointmentCategories ac
                    WHERE ac.AppointmentID = a.AppointmentID
                    FOR XML PATH('')
                ), 1, 2, '') AS CategoryNames
            FROM Appointments a
            {whereClause}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    
                    // 如果使用区域过滤，添加区域参数
                    if (!string.IsNullOrEmpty(recyclerRegion))
                    {
                        cmd.Parameters.AddWithValue("@RecyclerRegion", "%" + recyclerRegion + "%");
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderDetail.OrderNumber = $"AP{Convert.ToInt32(reader["AppointmentID"]):D6}";
                            orderDetail.AppointmentType = GetAppointmentTypeChinese(reader["AppointmentType"].ToString());
                            orderDetail.AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]).ToString("yyyy-MM-dd");
                            orderDetail.TimeSlot = GetTimeSlotChinese(reader["TimeSlot"].ToString());
                            orderDetail.EstimatedWeight = Convert.ToDecimal(reader["EstimatedWeight"]);
                            orderDetail.EstimatedPrice = reader["EstimatedPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EstimatedPrice"]);
                            orderDetail.IsUrgent = Convert.ToBoolean(reader["IsUrgent"]);
                            orderDetail.Address = reader["Address"].ToString();
                            orderDetail.ContactName = reader["ContactName"].ToString();
                            orderDetail.ContactPhone = reader["ContactPhone"].ToString();
                            orderDetail.Status = reader["Status"].ToString();
                            orderDetail.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]).ToString("yyyy-MM-dd HH:mm");
                            orderDetail.UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? "" : Convert.ToDateTime(reader["UpdatedDate"]).ToString("yyyy-MM-dd HH:mm");
                            orderDetail.SpecialInstructions = reader["SpecialInstructions"] == DBNull.Value ? "" : reader["SpecialInstructions"].ToString();
                            orderDetail.CategoryNames = reader["CategoryNames"] == DBNull.Value ? "" : reader["CategoryNames"].ToString();
                        }
                    }
                }
            }

            return orderDetail;
        }
        /// <summary>
        /// 获取用户的消息列表（按订单分组）
        /// </summary>
        public List<RecyclerMessageViewModel> GetUserMessages(int userId, int pageIndex = 1, int pageSize = 20)
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
            WHERE a.UserID = @UserId  -- 筛选当前用户的订单
            ORDER BY m.SentTime DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
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
                                SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : 0,
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
        /// 构建回收员订单过滤条件（用于WHERE子句）
        /// 注意：此方法返回的SQL片段是安全的，因为：
        /// 1. tableAlias参数经过严格验证，只允许字母和点号
        /// 2. 所有用户输入的值（recyclerId, recyclerRegion）都使用SQL参数化（@RecyclerID, @RecyclerRegion）
        /// 3. 此方法仅供内部调用，不直接暴露给用户输入
        /// </summary>
        /// <param name="recyclerId">回收员ID</param>
        /// <param name="recyclerRegion">回收员负责的区域（将使用参数化查询）</param>
        /// <param name="tableAlias">表别名（如"a"），仅用于内部表别名，受严格验证</param>
        /// <returns>WHERE子句的过滤条件字符串（使用参数化占位符）</returns>
        private string BuildRecyclerFilterCondition(int recyclerId, string recyclerRegion, string tableAlias = "")
        {
            // 验证表别名，防止SQL注入（仅允许字母、数字和下划线）
            if (!string.IsNullOrEmpty(tableAlias))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(tableAlias, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    throw new ArgumentException("Invalid table alias. Only letters, numbers, and underscores are allowed.", nameof(tableAlias));
                }
                tableAlias += ".";
            }

            // 如果回收员有指定区域，则显示：
            // 1. 已分配给该回收员的订单（不管地址是否匹配）
            // 2. 未分配但地址匹配回收员区域的订单
            // 注意：@RecyclerID 和 @RecyclerRegion 是参数化查询占位符，由调用方添加实际参数
            if (!string.IsNullOrEmpty(recyclerRegion))
            {
                return $"({tableAlias}RecyclerID = @RecyclerID OR ({tableAlias}RecyclerID IS NULL AND {tableAlias}Address LIKE @RecyclerRegion))";
            }
            else
            {
                // 如果回收员没有指定区域，则只显示已分配给该回收员的订单
                return $"{tableAlias}RecyclerID = @RecyclerID";
            }
        }

        /// <summary>
        /// 获取回收员的区域信息
        /// </summary>
        private string GetRecyclerRegion(int recyclerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = "SELECT Region FROM Recyclers WHERE RecyclerID = @RecyclerID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@RecyclerID", SqlDbType.Int) { Value = recyclerId });
                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : string.Empty;
                    }
                }
            }
            catch (SqlException ex)
            {
                // 记录错误日志并返回空字符串，避免影响订单查询主流程
                // 注意：生产环境应使用专业日志框架（如NLog、Serilog等）
                System.Diagnostics.Debug.WriteLine($"获取回收员区域信息失败: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                // 捕获其他异常
                System.Diagnostics.Debug.WriteLine($"获取回收员区域信息时发生意外错误: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
