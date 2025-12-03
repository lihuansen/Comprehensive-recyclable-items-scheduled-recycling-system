using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 用户通知数据访问层
    /// </summary>
    public class UserNotificationDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加通知
        /// </summary>
        public bool AddNotification(UserNotifications notification)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO UserNotifications (UserID, NotificationType, Title, Content, RelatedOrderID, RelatedFeedbackID, CreatedDate, IsRead)
                    VALUES (@UserID, @NotificationType, @Title, @Content, @RelatedOrderID, @RelatedFeedbackID, @CreatedDate, 0)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", notification.UserID);
                cmd.Parameters.AddWithValue("@NotificationType", notification.NotificationType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", notification.Title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Content", notification.Content ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RelatedOrderID", notification.RelatedOrderID.HasValue ? (object)notification.RelatedOrderID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@RelatedFeedbackID", notification.RelatedFeedbackID.HasValue ? (object)notification.RelatedFeedbackID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDate", notification.CreatedDate);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 批量添加通知 (用于向所有用户发送通知)
        /// </summary>
        public bool AddNotificationsToAllUsers(string notificationType, string title, string content)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO UserNotifications (UserID, NotificationType, Title, Content, CreatedDate, IsRead)
                    SELECT UserID, @NotificationType, @Title, @Content, @CreatedDate, 0
                    FROM Users";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@NotificationType", notificationType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Content", content ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 获取用户的通知列表（分页）
        /// </summary>
        public List<UserNotifications> GetUserNotifications(int userId, int pageIndex = 1, int pageSize = 20)
        {
            var notifications = new List<UserNotifications>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT NotificationID, UserID, NotificationType, Title, Content, 
                           RelatedOrderID, RelatedFeedbackID, CreatedDate, IsRead, ReadDate
                    FROM UserNotifications
                    WHERE UserID = @UserID
                    ORDER BY CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var notification = new UserNotifications
                        {
                            NotificationID = Convert.ToInt32(reader["NotificationID"]),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            NotificationType = reader["NotificationType"]?.ToString(),
                            Title = reader["Title"]?.ToString(),
                            Content = reader["Content"]?.ToString(),
                            RelatedOrderID = reader["RelatedOrderID"] != DBNull.Value ? Convert.ToInt32(reader["RelatedOrderID"]) : (int?)null,
                            RelatedFeedbackID = reader["RelatedFeedbackID"] != DBNull.Value ? Convert.ToInt32(reader["RelatedFeedbackID"]) : (int?)null,
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            IsRead = Convert.ToBoolean(reader["IsRead"]),
                            ReadDate = reader["ReadDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReadDate"]) : (DateTime?)null
                        };
                        notifications.Add(notification);
                    }
                }
            }
            return notifications;
        }

        /// <summary>
        /// 获取用户未读通知数量
        /// </summary>
        public int GetUnreadCount(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM UserNotifications WHERE UserID = @UserID AND IsRead = 0";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public bool MarkAsRead(int notificationId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE UserNotifications 
                    SET IsRead = 1, ReadDate = @ReadDate
                    WHERE NotificationID = @NotificationID AND UserID = @UserID AND IsRead = 0";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@NotificationID", notificationId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ReadDate", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public bool MarkAllAsRead(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE UserNotifications 
                    SET IsRead = 1, ReadDate = @ReadDate
                    WHERE UserID = @UserID AND IsRead = 0";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ReadDate", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected >= 0; // Return true even if no rows updated
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public bool DeleteNotification(int notificationId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM UserNotifications WHERE NotificationID = @NotificationID AND UserID = @UserID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@NotificationID", notificationId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 获取用户通知总数
        /// </summary>
        public int GetTotalCount(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM UserNotifications WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 获取订单对应用户ID
        /// </summary>
        public int GetUserIdByOrderId(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT UserID FROM Appointments WHERE AppointmentID = @OrderID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", orderId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// 获取反馈对应用户ID
        /// </summary>
        public int GetUserIdByFeedbackId(int feedbackId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT UserID FROM UserFeedback WHERE FeedbackID = @FeedbackID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }
}
