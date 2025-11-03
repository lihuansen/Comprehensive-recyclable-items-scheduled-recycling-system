using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class FeedbackDAL
    {
        private readonly string connectionString;

        public FeedbackDAL()
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["recyclingConnectionString"].ConnectionString;
        }

        /// <summary>
        /// 添加用户反馈
        /// </summary>
        public (bool Success, string Message) AddFeedback(UserFeedback feedback)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                        cmd.Parameters.AddWithValue("@Status", "待处理");
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
        /// 获取所有反馈（支持分页、筛选和搜索）
        /// </summary>
        public (List<UserFeedback> Feedbacks, int TotalCount) GetAllFeedbacks(
            string feedbackType = null, 
            string status = null, 
            string searchKeyword = null,
            int page = 1, 
            int pageSize = 20)
        {
            List<UserFeedback> feedbacks = new List<UserFeedback>();
            int totalCount = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 构建WHERE子句
                    List<string> whereConditions = new List<string>();
                    if (!string.IsNullOrEmpty(feedbackType))
                        whereConditions.Add("f.FeedbackType = @FeedbackType");
                    if (!string.IsNullOrEmpty(status))
                        whereConditions.Add("f.Status = @Status");
                    if (!string.IsNullOrEmpty(searchKeyword))
                        whereConditions.Add("(f.Subject LIKE @SearchKeyword OR u.UserName LIKE @SearchKeyword)");

                    string whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                    // 获取总数
                    string countSql = $@"SELECT COUNT(*) 
                                        FROM UserFeedback f
                                        LEFT JOIN Users u ON f.UserID = u.UserID
                                        {whereClause}";

                    using (SqlCommand cmd = new SqlCommand(countSql, conn))
                    {
                        AddSearchParameters(cmd, feedbackType, status, searchKeyword);
                        totalCount = (int)cmd.ExecuteScalar();
                    }

                    // 获取分页数据
                    string sql = $@"SELECT f.*, u.UserName
                                   FROM UserFeedback f
                                   LEFT JOIN Users u ON f.UserID = u.UserID
                                   {whereClause}
                                   ORDER BY f.CreatedDate DESC
                                   OFFSET @Offset ROWS
                                   FETCH NEXT @PageSize ROWS ONLY";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        AddSearchParameters(cmd, feedbackType, status, searchKeyword);
                        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feedbacks.Add(MapFeedbackFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取反馈列表时发生错误: {ex.Message}");
            }

            return (feedbacks, totalCount);
        }

        /// <summary>
        /// 根据ID获取反馈详情
        /// </summary>
        public UserFeedback GetFeedbackById(int feedbackId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT f.*, u.UserName
                                  FROM UserFeedback f
                                  LEFT JOIN Users u ON f.UserID = u.UserID
                                  WHERE f.FeedbackID = @FeedbackID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapFeedbackFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取反馈详情时发生错误: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 更新反馈状态和回复
        /// </summary>
        public (bool Success, string Message) UpdateFeedbackStatus(int feedbackId, string status, string adminReply)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE UserFeedback 
                                  SET Status = @Status, 
                                      AdminReply = @AdminReply, 
                                      UpdatedDate = @UpdatedDate
                                  WHERE FeedbackID = @FeedbackID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@AdminReply", string.IsNullOrEmpty(adminReply) ? (object)DBNull.Value : adminReply);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0 ? (true, "状态更新成功") : (false, "状态更新失败");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"更新状态时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取反馈统计数据
        /// </summary>
        public Dictionary<string, int> GetFeedbackStatistics()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>
            {
                { "Total", 0 },
                { "Pending", 0 },
                { "InProgress", 0 },
                { "Completed", 0 }
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    COUNT(*) AS Total,
                                    SUM(CASE WHEN Status = '待处理' THEN 1 ELSE 0 END) AS Pending,
                                    SUM(CASE WHEN Status = '处理中' THEN 1 ELSE 0 END) AS InProgress,
                                    SUM(CASE WHEN Status = '已完成' THEN 1 ELSE 0 END) AS Completed
                                   FROM UserFeedback";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats["Total"] = reader.GetInt32(0);
                                stats["Pending"] = reader.GetInt32(1);
                                stats["InProgress"] = reader.GetInt32(2);
                                stats["Completed"] = reader.GetInt32(3);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取反馈统计时发生错误: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// 添加搜索参数
        /// </summary>
        private void AddSearchParameters(SqlCommand cmd, string feedbackType, string status, string searchKeyword)
        {
            if (!string.IsNullOrEmpty(feedbackType))
                cmd.Parameters.AddWithValue("@FeedbackType", feedbackType);
            if (!string.IsNullOrEmpty(status))
                cmd.Parameters.AddWithValue("@Status", status);
            if (!string.IsNullOrEmpty(searchKeyword))
                cmd.Parameters.AddWithValue("@SearchKeyword", "%" + searchKeyword + "%");
        }

        /// <summary>
        /// 从DataReader映射反馈对象
        /// </summary>
        private UserFeedback MapFeedbackFromReader(SqlDataReader reader)
        {
            return new UserFeedback
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
            };
        }
    }
}
