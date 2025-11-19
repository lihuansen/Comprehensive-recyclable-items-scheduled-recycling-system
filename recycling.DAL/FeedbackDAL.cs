using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class FeedbackDAL
    {
        // 从配置文件获取数据库连接字符串
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加用户反馈
        /// </summary>
        public (bool Success, string Message) AddFeedback(UserFeedback feedback)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO UserFeedback 
                                   (UserID, FeedbackType, Subject, Description, ContactEmail, Status, CreatedDate)
                                   VALUES 
                                   (@UserID, @FeedbackType, @Subject, @Description, @ContactEmail, @Status, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", feedback.UserID);
                        cmd.Parameters.AddWithValue("@FeedbackType", feedback.FeedbackType);
                        cmd.Parameters.AddWithValue("@Subject", feedback.Subject);
                        cmd.Parameters.AddWithValue("@Description", feedback.Description);
                        cmd.Parameters.AddWithValue("@ContactEmail", string.IsNullOrEmpty(feedback.ContactEmail) ? (object)DBNull.Value : feedback.ContactEmail);
                        cmd.Parameters.AddWithValue("@Status", "反馈中");
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0 ? (true, "反馈提交成功") : (false, "反馈提交失败");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"提交反馈时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有反馈（管理员用）
        /// </summary>
        public List<UserFeedback> GetAllFeedbacks(string status = null, string feedbackType = null)
        {
            List<UserFeedback> feedbacks = new List<UserFeedback>();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT f.*, u.Username, u.Email 
                                   FROM UserFeedback f
                                   INNER JOIN Users u ON f.UserID = u.UserID
                                   WHERE 1=1";
                    
                    if (!string.IsNullOrEmpty(status))
                    {
                        sql += " AND f.Status = @Status";
                    }
                    
                    if (!string.IsNullOrEmpty(feedbackType))
                    {
                        sql += " AND f.FeedbackType = @FeedbackType";
                    }
                    
                    sql += " ORDER BY f.CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(status))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }
                        
                        if (!string.IsNullOrEmpty(feedbackType))
                        {
                            cmd.Parameters.AddWithValue("@FeedbackType", feedbackType);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feedbacks.Add(new UserFeedback
                                {
                                    FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                    FeedbackType = reader.GetString(reader.GetOrdinal("FeedbackType")),
                                    Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    ContactEmail = reader.IsDBNull(reader.GetOrdinal("ContactEmail")) ? null : reader.GetString(reader.GetOrdinal("ContactEmail")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    AdminReply = reader.IsDBNull(reader.GetOrdinal("AdminReply")) ? null : reader.GetString(reader.GetOrdinal("AdminReply")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 日志记录异常
                System.Diagnostics.Debug.WriteLine($"获取反馈列表时发生错误: {ex.Message}");
            }
            
            return feedbacks;
        }

        /// <summary>
        /// 更新反馈状态和管理员回复
        /// </summary>
        public (bool Success, string Message) UpdateFeedbackStatus(int feedbackId, string status, string adminReply)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Build SQL dynamically based on what needs to be updated
                    string sql = "UPDATE UserFeedback SET UpdatedDate = @UpdatedDate";
                    
                    if (!string.IsNullOrEmpty(status))
                    {
                        sql += ", Status = @Status";
                    }
                    
                    if (adminReply != null) // Check for null, not empty, to allow clearing replies
                    {
                        sql += ", AdminReply = @AdminReply";
                    }
                    
                    sql += " WHERE FeedbackID = @FeedbackID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        
                        if (!string.IsNullOrEmpty(status))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }
                        
                        if (adminReply != null)
                        {
                            cmd.Parameters.AddWithValue("@AdminReply", string.IsNullOrEmpty(adminReply) ? (object)DBNull.Value : adminReply);
                        }

                        int result = cmd.ExecuteNonQuery();
                        return result > 0 ? (true, "更新成功") : (false, "更新失败");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"更新反馈状态时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定用户的所有反馈（用户端查看自己的反馈）
        /// </summary>
        public List<UserFeedback> GetUserFeedbacks(int userId)
        {
            List<UserFeedback> feedbacks = new List<UserFeedback>();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT FeedbackID, UserID, FeedbackType, Subject, Description, 
                                          ContactEmail, Status, AdminReply, CreatedDate, UpdatedDate 
                                   FROM UserFeedback
                                   WHERE UserID = @UserID
                                   ORDER BY CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feedbacks.Add(new UserFeedback
                                {
                                    FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                    FeedbackType = reader.GetString(reader.GetOrdinal("FeedbackType")),
                                    Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    ContactEmail = reader.IsDBNull(reader.GetOrdinal("ContactEmail")) ? null : reader.GetString(reader.GetOrdinal("ContactEmail")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    AdminReply = reader.IsDBNull(reader.GetOrdinal("AdminReply")) ? null : reader.GetString(reader.GetOrdinal("AdminReply")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 日志记录异常
                System.Diagnostics.Debug.WriteLine($"获取用户反馈列表时发生错误: {ex.Message}");
            }
            
            return feedbacks;
        }
    }
}
