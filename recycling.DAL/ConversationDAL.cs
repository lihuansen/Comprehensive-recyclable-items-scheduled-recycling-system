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
        /// 检查是否用户和回收员都已结束会话且自最新结束时间后没有新消息
        /// 返回 (bothEnded, latestEndedTime)
        /// </summary>
        public (bool BothEnded, DateTime? LatestEndedTime) HasBothEnded(int orderId)
        {
            if (orderId <= 0) return (false, null);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 获取用户结束和回收员结束的最大时间（若不存在则为 NULL）
                string sql = @"
                    SELECT 
                        MAX(CASE WHEN Status = 'ended_by_user' THEN EndedTime END) AS UserEnded,
                        MAX(CASE WHEN Status = 'ended_by_recycler' THEN EndedTime END) AS RecyclerEnded
                    FROM Conversations
                    WHERE OrderID = @OrderID";

                DateTime? userEnded = null;
                DateTime? recyclerEnded = null;

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["UserEnded"] != DBNull.Value) userEnded = Convert.ToDateTime(reader["UserEnded"]);
                            if (reader["RecyclerEnded"] != DBNull.Value) recyclerEnded = Convert.ToDateTime(reader["RecyclerEnded"]);
                        }
                    }
                }

                if (!userEnded.HasValue || !recyclerEnded.HasValue)
                {
                    return (false, null); // 双方至少有一方未结束
                }

                // 取两者的最大时间作为最新结束时间
                var latest = userEnded.Value > recyclerEnded.Value ? userEnded.Value : recyclerEnded.Value;

                // 检查在 latest 之后是否存在任何新消息
                string checkMsgSql = @"
                    SELECT TOP 1 1 FROM Messages
                    WHERE OrderID = @OrderID AND SentTime > @LatestEndedTime";

                using (SqlCommand cmd2 = new SqlCommand(checkMsgSql, conn))
                {
                    cmd2.Parameters.AddWithValue("@OrderID", orderId);
                    cmd2.Parameters.AddWithValue("@LatestEndedTime", latest);
                    var obj = cmd2.ExecuteScalar();
                    bool hasAfter = obj != null;
                    if (hasAfter)
                    {
                        return (false, latest); // 有新消息 -> 不能认为双方结束（需要新会话）
                    }
                    else
                    {
                        return (true, latest); // 双方都结束，并且没有新消息
                    }
                }
            }
        }

        // 通用：由谁结束（endedByType = "user" or "recycler"），记录一条结束记录
        public bool EndConversation(int orderId, string endedByType, int endedById)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                int userId = 0, recyclerId = 0;
                string getParticipants = "SELECT UserID, RecyclerID FROM Appointments WHERE AppointmentID = @AppointmentID";
                using (SqlCommand cmd = new SqlCommand(getParticipants, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", orderId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["UserID"] != DBNull.Value) int.TryParse(reader["UserID"].ToString(), out userId);
                            if (reader["RecyclerID"] != DBNull.Value) int.TryParse(reader["RecyclerID"].ToString(), out recyclerId);
                        }
                    }
                    conn.Close();
                }

                string status = endedByType == "recycler" ? "ended_by_recycler" : "ended_by_user";
                string insertSql = @"
                    INSERT INTO Conversations (OrderID, UserID, RecyclerID, Status, CreatedTime, EndedTime)
                    VALUES (@OrderID, @UserID, @RecyclerID, @Status, @CreatedTime, @EndedTime)";
                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@CreatedTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@EndedTime", DateTime.Now);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        // 获取指定订单最近一次的结束会话（如果没有返回 null）
        public ConversationViewModel GetLatestConversation(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT TOP 1 ConversationID, OrderID, UserID, RecyclerID, Status, CreatedTime, EndedTime
                    FROM Conversations
                    WHERE OrderID = @OrderID
                    ORDER BY EndedTime DESC, ConversationID DESC";
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
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某用户的历史会话列表（按 EndedTime 降序）
        /// </summary>
        public List<ConversationViewModel> GetUserConversations(int userId, int pageIndex = 1, int pageSize = 50)
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
                        ISNULL(r.Username, '') as RecyclerName,
                        c.CreatedTime,
                        c.EndedTime,
                        c.Status
                    FROM Conversations c
                    INNER JOIN Appointments a ON c.OrderID = a.AppointmentID
                    LEFT JOIN Recyclers r ON c.RecyclerID = r.RecyclerID
                    WHERE a.UserID = @UserID
                    ORDER BY c.EndedTime DESC
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
                                ConversationID = Convert.ToInt32(reader["ConversationID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = $"AP{Convert.ToInt32(reader["OrderNumber"]):D6}",
                                RecyclerID = reader["RecyclerID"] != DBNull.Value ? Convert.ToInt32(reader["RecyclerID"]) : 0,
                                RecyclerName = reader["RecyclerName"].ToString(),
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
                    WHERE c.RecyclerID = @RecyclerID
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
                                SenderID = Convert.ToInt32(reader["SenderID"]),
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
    }
}
