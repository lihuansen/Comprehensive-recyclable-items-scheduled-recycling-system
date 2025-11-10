using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    public class AdminContactDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 获取或创建用户与管理员的会话
        /// </summary>
        public (int ConversationId, bool IsNewConversation) GetOrCreateConversation(int userId, int? adminId = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 首先检查是否存在未结束的会话
                string checkSql = @"
                    SELECT TOP 1 ConversationID 
                    FROM AdminContactConversations 
                    WHERE UserID = @UserID 
                      AND (UserEnded = 0 OR AdminEnded = 0)
                    ORDER BY StartTime DESC";

                using (SqlCommand cmd = new SqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    object result = cmd.ExecuteScalar();
                    
                    if (result != null)
                    {
                        return (Convert.ToInt32(result), false);
                    }
                }

                // 如果没有未结束的会话，创建新会话
                string insertSql = @"
                    INSERT INTO AdminContactConversations (UserID, AdminID, StartTime, UserEnded, AdminEnded)
                    VALUES (@UserID, @AdminID, GETDATE(), 0, 0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@AdminID", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                    
                    return ((int)cmd.ExecuteScalar(), true);
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public bool SendMessage(int userId, int? adminId, string senderType, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                return false;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 插入消息
                        string insertMessageSql = @"
                            INSERT INTO AdminContactMessages (UserID, AdminID, SenderType, Content, SentTime, IsRead)
                            VALUES (@UserID, @AdminID, @SenderType, @Content, GETDATE(), 0)";

                        using (SqlCommand cmd = new SqlCommand(insertMessageSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@AdminID", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@SenderType", senderType);
                            cmd.Parameters.AddWithValue("@Content", content);
                            cmd.ExecuteNonQuery();
                        }

                        // 更新会话的最后消息时间
                        string updateConvSql = @"
                            UPDATE AdminContactConversations 
                            SET LastMessageTime = GETDATE()
                            WHERE UserID = @UserID 
                              AND (UserEnded = 0 OR AdminEnded = 0)";

                        using (SqlCommand cmd = new SqlCommand(updateConvSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 获取用户的所有会话列表
        /// </summary>
        public List<AdminContactConversations> GetUserConversations(int userId)
        {
            List<AdminContactConversations> conversations = new List<AdminContactConversations>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ConversationID, UserID, AdminID, StartTime, 
                           UserEndedTime, AdminEndedTime, UserEnded, AdminEnded, LastMessageTime
                    FROM AdminContactConversations
                    WHERE UserID = @UserID
                    ORDER BY StartTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            conversations.Add(new AdminContactConversations
                            {
                                ConversationID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                AdminID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                StartTime = reader.GetDateTime(3),
                                UserEndedTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                AdminEndedTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                UserEnded = reader.GetBoolean(6),
                                AdminEnded = reader.GetBoolean(7),
                                LastMessageTime = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return conversations;
        }

        /// <summary>
        /// 获取管理员的所有会话列表
        /// </summary>
        public List<AdminContactConversations> GetAllConversations()
        {
            List<AdminContactConversations> conversations = new List<AdminContactConversations>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ConversationID, UserID, AdminID, StartTime, 
                           UserEndedTime, AdminEndedTime, UserEnded, AdminEnded, LastMessageTime
                    FROM AdminContactConversations
                    ORDER BY LastMessageTime DESC, StartTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            conversations.Add(new AdminContactConversations
                            {
                                ConversationID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                AdminID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                StartTime = reader.GetDateTime(3),
                                UserEndedTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                AdminEndedTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                UserEnded = reader.GetBoolean(6),
                                AdminEnded = reader.GetBoolean(7),
                                LastMessageTime = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return conversations;
        }

        /// <summary>
        /// 获取会话的所有消息
        /// </summary>
        public List<AdminContactMessages> GetConversationMessages(int userId, DateTime? beforeTime = null)
        {
            List<AdminContactMessages> messages = new List<AdminContactMessages>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT MessageID, UserID, AdminID, SenderType, Content, SentTime, IsRead
                    FROM AdminContactMessages
                    WHERE UserID = @UserID";

                if (beforeTime.HasValue)
                {
                    sql += " AND SentTime <= @BeforeTime";
                }

                sql += " ORDER BY SentTime ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    if (beforeTime.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@BeforeTime", beforeTime.Value);
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new AdminContactMessages
                            {
                                MessageID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                SenderType = reader.GetString(3),
                                Content = reader.GetString(4),
                                SentTime = reader.GetDateTime(5),
                                IsRead = reader.GetBoolean(6)
                            });
                        }
                    }
                }
            }

            return messages;
        }

        /// <summary>
        /// 结束会话（用户端）
        /// </summary>
        public bool EndConversationByUser(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE AdminContactConversations
                    SET UserEnded = 1, UserEndedTime = GETDATE()
                    WHERE UserID = @UserID 
                      AND UserEnded = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// 结束会话（管理员端）
        /// </summary>
        public bool EndConversationByAdmin(int userId, int adminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE AdminContactConversations
                    SET AdminEnded = 1, AdminEndedTime = GETDATE(), AdminID = @AdminID
                    WHERE UserID = @UserID 
                      AND AdminEnded = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public Users GetUserById(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT UserID, Username, PhoneNumber, Email FROM Users WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Users
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PhoneNumber = reader.GetString(2),
                                Email = reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查会话是否已完全结束
        /// </summary>
        public bool IsBothEnded(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT TOP 1 UserEnded, AdminEnded
                    FROM AdminContactConversations
                    WHERE UserID = @UserID
                    ORDER BY StartTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool userEnded = reader.GetBoolean(0);
                            bool adminEnded = reader.GetBoolean(1);
                            return userEnded && adminEnded;
                        }
                    }
                }
            }

            return false;
        }
    }
}
