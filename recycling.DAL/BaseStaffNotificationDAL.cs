using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 基地工作人员通知数据访问层
    /// Base Staff Notifications Data Access Layer
    /// </summary>
    public class BaseStaffNotificationDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加通知
        /// </summary>
        public bool AddNotification(BaseStaffNotifications notification)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO BaseStaffNotifications 
                                 (WorkerID, NotificationType, Title, Content, 
                                  RelatedTransportOrderID, RelatedWarehouseReceiptID, 
                                  CreatedDate, IsRead) 
                                 VALUES 
                                 (@WorkerID, @NotificationType, @Title, @Content, 
                                  @RelatedTransportOrderID, @RelatedWarehouseReceiptID, 
                                  @CreatedDate, @IsRead)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", notification.WorkerID);
                        cmd.Parameters.AddWithValue("@NotificationType", (object)notification.NotificationType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", (object)notification.Title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Content", (object)notification.Content ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedTransportOrderID", (object)notification.RelatedTransportOrderID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedWarehouseReceiptID", (object)notification.RelatedWarehouseReceiptID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedDate", notification.CreatedDate);
                        cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 向所有基地工作人员发送通知
        /// </summary>
        public bool AddNotificationsToAllWorkers(string notificationType, string title, string content, 
            int? relatedTransportOrderID = null, int? relatedWarehouseReceiptID = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO BaseStaffNotifications 
                                 (WorkerID, NotificationType, Title, Content, 
                                  RelatedTransportOrderID, RelatedWarehouseReceiptID, 
                                  CreatedDate, IsRead)
                                 SELECT WorkerID, @NotificationType, @Title, @Content, 
                                        @RelatedTransportOrderID, @RelatedWarehouseReceiptID, 
                                        @CreatedDate, 0
                                 FROM SortingCenterWorkers 
                                 WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NotificationType", (object)notificationType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", (object)title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Content", (object)content ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedTransportOrderID", (object)relatedTransportOrderID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedWarehouseReceiptID", (object)relatedWarehouseReceiptID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddNotificationsToAllWorkers Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取工作人员的通知列表（分页）
        /// </summary>
        public List<BaseStaffNotifications> GetWorkerNotifications(int workerId, int pageIndex = 1, int pageSize = 20)
        {
            var notifications = new List<BaseStaffNotifications>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM (
                                    SELECT *, ROW_NUMBER() OVER (ORDER BY CreatedDate DESC) AS RowNum
                                    FROM BaseStaffNotifications
                                    WHERE WorkerID = @WorkerID
                                  ) AS T
                                  WHERE RowNum BETWEEN @StartRow AND @EndRow
                                  ORDER BY CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        int startRow = (pageIndex - 1) * pageSize + 1;
                        int endRow = pageIndex * pageSize;

                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        cmd.Parameters.AddWithValue("@StartRow", startRow);
                        cmd.Parameters.AddWithValue("@EndRow", endRow);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notifications.Add(new BaseStaffNotifications
                                {
                                    NotificationID = Convert.ToInt32(reader["NotificationID"]),
                                    WorkerID = Convert.ToInt32(reader["WorkerID"]),
                                    NotificationType = reader["NotificationType"] == DBNull.Value ? null : reader["NotificationType"].ToString(),
                                    Title = reader["Title"] == DBNull.Value ? null : reader["Title"].ToString(),
                                    Content = reader["Content"] == DBNull.Value ? null : reader["Content"].ToString(),
                                    RelatedTransportOrderID = reader["RelatedTransportOrderID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RelatedTransportOrderID"]),
                                    RelatedWarehouseReceiptID = reader["RelatedWarehouseReceiptID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RelatedWarehouseReceiptID"]),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    IsRead = Convert.ToBoolean(reader["IsRead"]),
                                    ReadDate = reader["ReadDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReadDate"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWorkerNotifications Error: {ex.Message}");
            }

            return notifications;
        }

        /// <summary>
        /// 获取工作人员未读通知数量
        /// </summary>
        public int GetUnreadCount(int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) FROM BaseStaffNotifications 
                                 WHERE WorkerID = @WorkerID AND IsRead = 0";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUnreadCount Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public bool MarkAsRead(int notificationId, int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE BaseStaffNotifications 
                                 SET IsRead = 1, ReadDate = @ReadDate 
                                 WHERE NotificationID = @NotificationID AND WorkerID = @WorkerID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NotificationID", notificationId);
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        cmd.Parameters.AddWithValue("@ReadDate", DateTime.Now);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkAsRead Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public bool MarkAllAsRead(int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE BaseStaffNotifications 
                                 SET IsRead = 1, ReadDate = @ReadDate 
                                 WHERE WorkerID = @WorkerID AND IsRead = 0";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        cmd.Parameters.AddWithValue("@ReadDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkAllAsRead Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public bool DeleteNotification(int notificationId, int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"DELETE FROM BaseStaffNotifications 
                                 WHERE NotificationID = @NotificationID AND WorkerID = @WorkerID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NotificationID", notificationId);
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取工作人员通知总数
        /// </summary>
        public int GetTotalCount(int workerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) FROM BaseStaffNotifications 
                                 WHERE WorkerID = @WorkerID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", workerId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTotalCount Error: {ex.Message}");
                return 0;
            }
        }
    }
}
