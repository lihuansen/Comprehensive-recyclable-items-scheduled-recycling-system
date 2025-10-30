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
