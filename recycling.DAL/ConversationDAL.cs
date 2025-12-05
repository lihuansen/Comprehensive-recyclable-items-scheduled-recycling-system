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
    public class ConversationDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 检查是否用户和回收员都已结束会话
        /// 返回 (bothEnded, latestEndedTime)
        /// </summary>
        public (bool BothEnded, DateTime? LatestEndedTime) HasBothEnded(int orderId)
        {
            if (orderId <= 0) return (false, null);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 检查当前订单的会话状态
                string sql = @"
                    SELECT TOP 1 UserEnded, RecyclerEnded, UserEndedTime, RecyclerEndedTime
                    FROM Conversations
                    WHERE OrderID = @OrderID
                    ORDER BY ConversationID DESC";

                bool? userEnded = null;
                bool? recyclerEnded = null;
                DateTime? userEndedTime = null;
                DateTime? recyclerEndedTime = null;

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["UserEnded"] != DBNull.Value) userEnded = Convert.ToBoolean(reader["UserEnded"]);
                            if (reader["RecyclerEnded"] != DBNull.Value) recyclerEnded = Convert.ToBoolean(reader["RecyclerEnded"]);
                            if (reader["UserEndedTime"] != DBNull.Value) userEndedTime = Convert.ToDateTime(reader["UserEndedTime"]);
                            if (reader["RecyclerEndedTime"] != DBNull.Value) recyclerEndedTime = Convert.ToDateTime(reader["RecyclerEndedTime"]);
                        }
                    }
                }

                if (userEnded == true && recyclerEnded == true)
                {
                    var latest = userEndedTime > recyclerEndedTime ? userEndedTime : recyclerEndedTime;
                    return (true, latest);
                }

                return (false, null);
            }
        }

        // 通用：由谁结束（endedByType = "user" or "recycler"），记录结束状态并发送系统消息
        public bool EndConversation(int orderId, string endedByType, int endedById)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int userId = 0, recyclerId = 0;
                        
                        // 获取订单参与者
                        string getParticipants = "SELECT UserID, RecyclerID FROM Appointments WHERE AppointmentID = @AppointmentID";
                        using (SqlCommand cmd = new SqlCommand(getParticipants, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@AppointmentID", orderId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader["UserID"] != DBNull.Value) int.TryParse(reader["UserID"].ToString(), out userId);
                                    if (reader["RecyclerID"] != DBNull.Value) int.TryParse(reader["RecyclerID"].ToString(), out recyclerId);
                                }
                            }
                        }

                        // 检查或创建会话记录
                        string checkConv = "SELECT TOP 1 ConversationID, UserEnded, RecyclerEnded FROM Conversations WHERE OrderID = @OrderID ORDER BY ConversationID DESC";
                        int conversationId = 0;
                        bool existingUserEnded = false;
                        bool existingRecyclerEnded = false;

                        using (SqlCommand cmd = new SqlCommand(checkConv, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    conversationId = Convert.ToInt32(reader["ConversationID"]);
                                    if (reader["UserEnded"] != DBNull.Value) existingUserEnded = Convert.ToBoolean(reader["UserEnded"]);
                                    if (reader["RecyclerEnded"] != DBNull.Value) existingRecyclerEnded = Convert.ToBoolean(reader["RecyclerEnded"]);
                                }
                            }
                        }

                        // 如果回收员尝试结束但用户还没结束，返回失败
                        if (endedByType == "recycler" && !existingUserEnded)
                        {
                            trans.Rollback();
                            return false;
                        }

                        // 更新或创建会话记录
                        if (conversationId > 0)
                        {
                            DateTime endedTime = DateTime.Now;
                            // 更新现有记录
                            string updateSql = endedByType == "user"
                                ? "UPDATE Conversations SET UserEnded = 1, UserEndedTime = @EndedTime WHERE ConversationID = @ConversationID"
                                : "UPDATE Conversations SET RecyclerEnded = 1, RecyclerEndedTime = @EndedTime WHERE ConversationID = @ConversationID";

                            using (SqlCommand cmd = new SqlCommand(updateSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@EndedTime", endedTime);
                                cmd.Parameters.AddWithValue("@ConversationID", conversationId);
                                cmd.ExecuteNonQuery();
                            }

                            // 检查双方是否都已结束，如果是则设置 EndedTime
                            bool bothEnded = (endedByType == "user" && existingRecyclerEnded) || (endedByType == "recycler" && existingUserEnded);

                            if (bothEnded)
                            {
                                string updateEndedTimeSql = "UPDATE Conversations SET EndedTime = @EndedTime WHERE ConversationID = @ConversationID";
                                using (SqlCommand cmd = new SqlCommand(updateEndedTimeSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@EndedTime", endedTime);
                                    cmd.Parameters.AddWithValue("@ConversationID", conversationId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            DateTime endedTime = DateTime.Now;
                            // 创建新记录
                            string insertSql = @"
                                INSERT INTO Conversations (OrderID, UserID, RecyclerID, Status, CreatedTime, UserEnded, RecyclerEnded, UserEndedTime, RecyclerEndedTime)
                                VALUES (@OrderID, @UserID, @RecyclerID, @Status, @CreatedTime, @UserEnded, @RecyclerEnded, @UserEndedTime, @RecyclerEndedTime)";
                            
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                                cmd.Parameters.AddWithValue("@Status", "active");
                                cmd.Parameters.AddWithValue("@CreatedTime", DateTime.Now);
                                cmd.Parameters.AddWithValue("@UserEnded", endedByType == "user" ? 1 : 0);
                                cmd.Parameters.AddWithValue("@RecyclerEnded", endedByType == "recycler" ? 1 : 0);
                                cmd.Parameters.AddWithValue("@UserEndedTime", endedByType == "user" ? (object)endedTime : DBNull.Value);
                                cmd.Parameters.AddWithValue("@RecyclerEndedTime", endedByType == "recycler" ? (object)endedTime : DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 插入系统消息
                        string systemMessage = endedByType == "user" ? "用户已结束对话" : "回收员已结束对话";
                        string insertMsgSql = @"
                            INSERT INTO Messages (OrderID, SenderType, SenderID, Content, SentTime, IsRead)
                            VALUES (@OrderID, 'system', NULL, @Content, @SentTime, 1)";
                        
                        using (SqlCommand cmd = new SqlCommand(insertMsgSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.Parameters.AddWithValue("@Content", systemMessage);
                            cmd.Parameters.AddWithValue("@SentTime", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // 获取指定订单最近一次的结束会话（如果没有返回 null）
        public ConversationViewModel GetLatestConversation(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT TOP 1 ConversationID, OrderID, UserID, RecyclerID, Status, CreatedTime, EndedTime, UserEnded, RecyclerEnded
                    FROM Conversations
                    WHERE OrderID = @OrderID
                    ORDER BY ConversationID DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ORDERID", orderId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ConversationViewModel
                            {
                                ConversationID = Convert.ToInt32(reader["ConversationID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CreatedTime = reader["CreatedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CreatedTime"]),
                                EndedTime = reader["EndedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndedTime"]),
                                Status = reader["Status"].ToString(),
                                UserEnded = reader["UserEnded"] != DBNull.Value && Convert.ToBoolean(reader["UserEnded"]),
                                RecyclerEnded = reader["RecyclerEnded"] != DBNull.Value && Convert.ToBoolean(reader["RecyclerEnded"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某用户的历史会话列表（超过1个月的会话自动归入历史对话）
        /// </summary>
        public List<ConversationViewModel> GetUserConversations(int userId, int pageIndex = 1, int pageSize = 50)
        {
            var list = new List<ConversationViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 获取超过1个月的会话记录
                string sql = @"
                    SELECT DISTINCT
                        m.OrderID,
                        a.AppointmentID as OrderNumber,
                        a.RecyclerID,
                        ISNULL(r.Username, '') as RecyclerName,
                        MIN(m.SentTime) as CreatedTime,
                        MAX(m.SentTime) as LastMessageTime
                    FROM Messages m
                    INNER JOIN Appointments a ON m.OrderID = a.AppointmentID
                    LEFT JOIN Recyclers r ON a.RecyclerID = r.RecyclerID
                    WHERE a.UserID = @UserID 
                    AND m.SentTime < DATEADD(MONTH, -1, GETDATE())
                    GROUP BY m.OrderID, a.AppointmentID, a.RecyclerID, r.Username
                    ORDER BY LastMessageTime DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ConversationViewModel
                            {
                                ConversationID = 0, // No specific conversation ID for time-based history
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["OrderNumber"]):D6}",
                                RecyclerID = reader["RecyclerID"] != DBNull.Value ? Convert.ToInt32(reader["RecyclerID"]) : 0,
                                RecyclerName = reader["RecyclerName"].ToString(),
                                CreatedTime = reader["CreatedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CreatedTime"]),
                                EndedTime = reader["LastMessageTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastMessageTime"]),
                                Status = "历史"
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取用户的历史消息（超过1个月的消息）
        /// </summary>
        public List<Messages> GetUserHistoricalMessages(int orderId, int userId)
        {
            var messages = new List<Messages>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT m.MessageID, m.OrderID, m.SenderType, m.SenderID, m.Content, m.SentTime, m.IsRead
                    FROM Messages m
                    INNER JOIN Appointments a ON m.OrderID = a.AppointmentID
                    WHERE m.OrderID = @OrderID AND a.UserID = @UserID
                    AND m.SentTime < DATEADD(MONTH, -1, GETDATE())
                    ORDER BY m.SentTime ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Messages
                            {
                                MessageID = Convert.ToInt32(reader["MessageID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                SenderType = reader["SenderType"].ToString(),
                                SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : (int?)null,
                                Content = reader["Content"].ToString(),
                                SentTime = Convert.ToDateTime(reader["SentTime"]),
                                IsRead = Convert.ToBoolean(reader["IsRead"])
                            });
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// 获取某回收员的历史会话列表（按 EndedTime 降序）
        /// </summary>
        public List<ConversationViewModel> GetRecyclerConversations(int recyclerId, int pageIndex = 1, int pageSize = 50)
        {
            var list = new List<ConversationViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        c.ConversationID,
                        c.OrderID,
                        a.AppointmentID as OrderNumber,
                        c.RecyclerID,
                        ISNULL(u.Username, '') as UserName,
                        c.CreatedTime,
                        c.EndedTime,
                        c.Status
                    FROM Conversations c
                    INNER JOIN Appointments a ON c.OrderID = a.AppointmentID
                    LEFT JOIN Users u ON a.UserID = u.UserID
                    WHERE c.RecyclerID = @RecyclerID AND c.EndedTime IS NOT NULL
                    ORDER BY c.EndedTime DESC
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
                            list.Add(new ConversationViewModel
                            {
                                ConversationID = Convert.ToInt32(reader["ConversationID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["OrderNumber"]):D6}",
                                RecyclerID = reader["RecyclerID"] != DBNull.Value ? Convert.ToInt32(reader["RecyclerID"]) : 0,
                                UserName = reader["UserName"].ToString(),
                                CreatedTime = reader["CreatedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CreatedTime"]),
                                EndedTime = reader["EndedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndedTime"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取某个 conversation 的历史消息（即 OrderID 且 SentTime <= EndedTime）
        /// 这里也可以直接在 Controller 层复用 MessageDAL.GetOrderMessages 然后过滤
        /// 但提供一个便利方法
        /// </summary>
        public List<Messages> GetConversationMessagesBeforeEnd(int orderId, DateTime endedTime)
        {
            var messages = new List<Messages>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT MessageID, OrderID, SenderType, SenderID, Content, SentTime, IsRead
                    FROM Messages
                    WHERE OrderID = @OrderID AND SentTime <= @EndedTime
                    ORDER BY SentTime ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@EndedTime", endedTime);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Messages
                            {
                                MessageID = Convert.ToInt32(reader["MessageID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                SenderType = reader["SenderType"].ToString(),
                                SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : (int?)null,
                                Content = reader["Content"].ToString(),
                                SentTime = Convert.ToDateTime(reader["SentTime"]),
                                IsRead = Convert.ToBoolean(reader["IsRead"])
                            });
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// 检查当前对话状态并在双方都已结束后创建新对话
        /// 返回：是否需要开始新对话
        /// </summary>
        public bool EnsureActiveConversation(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // 检查是否存在会话且双方都已结束
                string checkSql = @"
                    SELECT TOP 1 ConversationID, UserEnded, RecyclerEnded, EndedTime
                    FROM Conversations 
                    WHERE OrderID = @OrderID 
                    ORDER BY ConversationID DESC";
                
                bool bothEnded = false;
                
                using (SqlCommand cmd = new SqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool userEnded = reader["UserEnded"] != DBNull.Value && Convert.ToBoolean(reader["UserEnded"]);
                            bool recyclerEnded = reader["RecyclerEnded"] != DBNull.Value && Convert.ToBoolean(reader["RecyclerEnded"]);
                            bothEnded = userEnded && recyclerEnded;
                        }
                    }
                }
                
                // 如果双方都已结束，创建新的对话记录
                if (bothEnded)
                {
                    int userId = 0, recyclerId = 0;
                    
                    // 获取订单参与者
                    string getParticipants = "SELECT UserID, RecyclerID FROM Appointments WHERE AppointmentID = @AppointmentID";
                    using (SqlCommand cmd = new SqlCommand(getParticipants, conn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["UserID"] != DBNull.Value) int.TryParse(reader["UserID"].ToString(), out userId);
                                if (reader["RecyclerID"] != DBNull.Value) int.TryParse(reader["RecyclerID"].ToString(), out recyclerId);
                            }
                        }
                    }
                    
                    // 创建新会话
                    string insertSql = @"
                        INSERT INTO Conversations (OrderID, UserID, RecyclerID, Status, CreatedTime, UserEnded, RecyclerEnded)
                        VALUES (@OrderID, @UserID, @RecyclerID, 'active', @CreatedTime, 0, 0)";
                    
                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                        cmd.Parameters.AddWithValue("@CreatedTime", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // 插入系统消息表示新对话开始
                    string insertMsgSql = @"
                        INSERT INTO Messages (OrderID, SenderType, SenderID, Content, SentTime, IsRead)
                        VALUES (@OrderID, 'system', NULL, @Content, @SentTime, 1)";
                    
                    using (SqlCommand cmd = new SqlCommand(insertMsgSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@Content", "新的对话已开始");
                        cmd.Parameters.AddWithValue("@SentTime", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    
                    return true;
                }
                
                return false;
            }
        }
    }
}
