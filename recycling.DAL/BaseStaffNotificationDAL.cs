using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    /// 基地工作人员通知数据访问层
    /// 基地员工通知数据访问处理类。
    public class BaseStaffNotificationDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
        private const string LegacyReceiptColumn = "RelatedWarehouseReceiptID";
        private const string CurrentReceiptColumn = "RelatedWarehouseReceipt";
        private readonly string _relatedWarehouseReceiptColumn;

        public BaseStaffNotificationDAL()
        {
            _relatedWarehouseReceiptColumn = NormalizeRelatedWarehouseReceiptColumn(DetectRelatedWarehouseReceiptColumn());
        }

        private string DetectRelatedWarehouseReceiptColumn()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = @"SELECT TOP 1 COLUMN_NAME
                                FROM INFORMATION_SCHEMA.COLUMNS
                                WHERE TABLE_NAME = 'BaseStaffNotifications'
                                  AND COLUMN_NAME IN ('RelatedWarehouseReceipt', 'RelatedWarehouseReceiptID')
                                ORDER BY CASE 
                                    WHEN COLUMN_NAME = 'RelatedWarehouseReceipt' THEN 1
                                    WHEN COLUMN_NAME = 'RelatedWarehouseReceiptID' THEN 2 END";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var value = cmd.ExecuteScalar() as string;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DetectRelatedWarehouseReceiptColumn Error: {ex.Message}");
            }

            return LegacyReceiptColumn;
        }

        private string NormalizeRelatedWarehouseReceiptColumn(string columnName)
        {
            if (string.Equals(columnName, CurrentReceiptColumn, StringComparison.OrdinalIgnoreCase))
            {
                return CurrentReceiptColumn;
            }

            if (string.Equals(columnName, LegacyReceiptColumn, StringComparison.OrdinalIgnoreCase))
            {
                return LegacyReceiptColumn;
            }

            throw new InvalidOperationException($"检测到未知入库关联列名：{columnName}");
        }

        /// 添加通知
        public bool AddNotification(BaseStaffNotifications notification)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = BuildAddNotificationSql();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerID", notification.WorkerID);
                        cmd.Parameters.AddWithValue("@NotificationType", (object)notification.NotificationType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", (object)notification.Title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Content", (object)notification.Content ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedTransportOrderID", (object)notification.RelatedTransportOrderID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedWarehouseReceipt", (object)notification.RelatedWarehouseReceipt ?? DBNull.Value);
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

        /// 向所有基地工作人员发送通知
        public bool AddNotificationsToAllWorkers(string notificationType, string title, string content, 
            int? relatedTransportOrderID = null, int? relatedWarehouseReceiptID = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = BuildAddNotificationsToAllWorkersSql();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NotificationType", (object)notificationType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", (object)title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Content", (object)content ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedTransportOrderID", (object)relatedTransportOrderID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RelatedWarehouseReceipt", (object)relatedWarehouseReceiptID ?? DBNull.Value);
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

        /// 获取工作人员的通知列表（分页）
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
                                    RelatedWarehouseReceipt = ReadRelatedWarehouseReceipt(reader),
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

        /// 获取工作人员未读通知数量
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

        /// 标记通知为已读
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

        /// 标记所有通知为已读
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

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkAllAsRead Error: {ex.Message}");
                return false;
            }
        }

        /// 删除通知
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

        /// 获取工作人员通知总数
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

        private int? ReadRelatedWarehouseReceipt(SqlDataReader reader)
        {
            try
            {
                if (reader.HasColumn(_relatedWarehouseReceiptColumn) && reader[_relatedWarehouseReceiptColumn] != DBNull.Value)
                {
                    return Convert.ToInt32(reader[_relatedWarehouseReceiptColumn]);
                }

                string fallbackColumn = string.Equals(_relatedWarehouseReceiptColumn, CurrentReceiptColumn, StringComparison.OrdinalIgnoreCase)
                    ? LegacyReceiptColumn
                    : CurrentReceiptColumn;

                if (reader.HasColumn(fallbackColumn) && reader[fallbackColumn] != DBNull.Value)
                {
                    return Convert.ToInt32(reader[fallbackColumn]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ReadRelatedWarehouseReceipt Error: Column={_relatedWarehouseReceiptColumn}, Message={ex.Message}");
            }

            return null;
        }

        private string BuildAddNotificationSql()
        {
            if (string.Equals(_relatedWarehouseReceiptColumn, CurrentReceiptColumn, StringComparison.Ordinal))
            {
                return @"INSERT INTO BaseStaffNotifications 
                         (WorkerID, NotificationType, Title, Content, 
                          RelatedTransportOrderID, RelatedWarehouseReceipt, 
                          CreatedDate, IsRead) 
                         VALUES 
                         (@WorkerID, @NotificationType, @Title, @Content, 
                          @RelatedTransportOrderID, @RelatedWarehouseReceipt, 
                          @CreatedDate, @IsRead)";
            }

            return @"INSERT INTO BaseStaffNotifications 
                     (WorkerID, NotificationType, Title, Content, 
                      RelatedTransportOrderID, RelatedWarehouseReceiptID, 
                      CreatedDate, IsRead) 
                     VALUES 
                     (@WorkerID, @NotificationType, @Title, @Content, 
                      @RelatedTransportOrderID, @RelatedWarehouseReceipt, 
                      @CreatedDate, @IsRead)";
        }

        private string BuildAddNotificationsToAllWorkersSql()
        {
            if (string.Equals(_relatedWarehouseReceiptColumn, CurrentReceiptColumn, StringComparison.Ordinal))
            {
                return @"INSERT INTO BaseStaffNotifications 
                         (WorkerID, NotificationType, Title, Content, 
                           RelatedTransportOrderID, RelatedWarehouseReceipt, 
                           CreatedDate, IsRead)
                          SELECT WorkerID, @NotificationType, @Title, @Content, 
                                 @RelatedTransportOrderID, @RelatedWarehouseReceipt, 
                                 @CreatedDate, 0
                          FROM SortingCenterWorkers 
                          WHERE ISNULL(IsActive, 1) = 1";
            }

            return @"INSERT INTO BaseStaffNotifications 
                     (WorkerID, NotificationType, Title, Content, 
                       RelatedTransportOrderID, RelatedWarehouseReceiptID, 
                       CreatedDate, IsRead)
                      SELECT WorkerID, @NotificationType, @Title, @Content, 
                             @RelatedTransportOrderID, @RelatedWarehouseReceipt, 
                             @CreatedDate, 0
                      FROM SortingCenterWorkers 
                      WHERE ISNULL(IsActive, 1) = 1";
        }
    }

    internal static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this IDataRecord record, string columnName)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
