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
        /// 结束会话：向 Conversations 表插入一条结束记录（记录 EndedTime）。
        /// 如果同一订单已存在未结束会话，则也可以插一条新的结束记录（实现为新插入一条记录）
        /// </summary>
        public bool EndConversation(int orderId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 获取关联回收员ID（如果有）
                int recyclerId = 0;
                string getRecyclerSql = "SELECT RecyclerID FROM Appointments WHERE AppointmentID = @AppointmentID";
                using (SqlCommand cmd = new SqlCommand(getRecyclerSql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", orderId);
                    conn.Open();
                    var obj = cmd.ExecuteScalar();
                    if (obj != null && obj != DBNull.Value)
                    {
                        int.TryParse(obj.ToString(), out recyclerId);
                    }
                    conn.Close();
                }

                string insertSql = @"
                    INSERT INTO Conversations (OrderID, UserID, RecyclerID, Status, CreatedTime, EndedTime)
                    VALUES (@OrderID, @UserID, @RecyclerID, @Status, @CreatedTime, @EndedTime)";

                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                    cmd.Parameters.AddWithValue("@Status", "ended");
                    cmd.Parameters.AddWithValue("@CreatedTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@EndedTime", DateTime.Now);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
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
