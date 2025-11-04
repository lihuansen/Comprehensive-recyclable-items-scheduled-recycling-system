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
    public class MessageDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 发送消息
        /// </summary>
        public bool SendMessage(Messages message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Messages (OrderID, SenderType, SenderID, Content, SentTime, IsRead)
                    VALUES (@OrderID, @SenderType, @SenderID, @Content, @SentTime, @IsRead)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", message.OrderID);
                cmd.Parameters.AddWithValue("@SenderType", message.SenderType);
                cmd.Parameters.AddWithValue("@SenderID", message.SenderID);
                cmd.Parameters.AddWithValue("@Content", message.Content);
                cmd.Parameters.AddWithValue("@SentTime", message.SentTime);
                cmd.Parameters.AddWithValue("@IsRead", message.IsRead);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 获取订单的聊天记录
        /// </summary>
        public List<Messages> GetOrderMessages(int orderId)
        {
            var messages = new List<Messages>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT MessageID, OrderID, SenderType, SenderID, Content, SentTime, IsRead
                    FROM Messages
                    WHERE OrderID = @OrderID
                    ORDER BY SentTime ASC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new Messages
                        {
                            MessageID = Convert.ToInt32(reader["MessageID"]),
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            SenderType = reader["SenderType"].ToString(),
                            SenderID = reader["SenderID"] != DBNull.Value ? Convert.ToInt32(reader["SenderID"]) : (int?)null,
                            Content = reader["Content"].ToString(),
                            SentTime = Convert.ToDateTime(reader["SentTime"]),
                            IsRead = Convert.ToBoolean(reader["IsRead"])
                        };
                        messages.Add(message);
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public bool MarkMessagesAsRead(int orderId, string readerType, int readerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Messages 
                    SET IsRead = 1
                    WHERE OrderID = @OrderID 
                      AND SenderType != @ReaderType
                      AND IsRead = 0";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@ReaderType", readerType);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
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
            WHERE a.UserID = @UserId  -- 筛选当前用户的订单消息
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
    }
}
