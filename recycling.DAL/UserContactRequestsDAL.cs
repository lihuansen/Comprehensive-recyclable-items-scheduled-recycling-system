using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 用户联系请求数据访问层
    /// </summary>
    public class UserContactRequestsDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 创建用户联系请求
        /// </summary>
        public int CreateContactRequest(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 检查是否存在待处理的请求
                string checkSql = @"
                    SELECT COUNT(*) 
                    FROM UserContactRequests 
                    WHERE UserID = @UserID AND RequestStatus = 1";

                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int existingCount = (int)checkCmd.ExecuteScalar();

                    // 如果已有待处理请求，不创建新的
                    if (existingCount > 0)
                    {
                        return 0; // 表示已存在
                    }
                }

                // 创建新的联系请求
                string insertSql = @"
                    INSERT INTO UserContactRequests (UserID, RequestStatus, RequestTime)
                    VALUES (@UserID, 1, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 获取所有待处理的联系请求（管理员查看）
        /// </summary>
        public List<UserContactRequestViewModel> GetPendingRequests()
        {
            List<UserContactRequestViewModel> requests = new List<UserContactRequestViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        r.RequestID,
                        r.UserID,
                        u.Username AS UserName,
                        u.Phone,
                        u.Email,
                        r.RequestStatus,
                        r.RequestTime,
                        r.ContactedTime,
                        r.AdminID,
                        s.Staffname AS AdminName
                    FROM UserContactRequests r
                    INNER JOIN Users u ON r.UserID = u.UserID
                    LEFT JOIN Staff s ON r.AdminID = s.StaffID
                    WHERE r.RequestStatus = 1
                    ORDER BY r.RequestTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        requests.Add(new UserContactRequestViewModel
                        {
                            RequestID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            UserName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            RequestStatus = reader.GetBoolean(5),
                            RequestTime = reader.GetDateTime(6),
                            ContactedTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            AdminID = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                            AdminName = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }

            return requests;
        }

        /// <summary>
        /// 获取所有联系请求（包括已处理的）
        /// </summary>
        public List<UserContactRequestViewModel> GetAllRequests()
        {
            List<UserContactRequestViewModel> requests = new List<UserContactRequestViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        r.RequestID,
                        r.UserID,
                        u.Username AS UserName,
                        u.Phone,
                        u.Email,
                        r.RequestStatus,
                        r.RequestTime,
                        r.ContactedTime,
                        r.AdminID,
                        s.Staffname AS AdminName
                    FROM UserContactRequests r
                    INNER JOIN Users u ON r.UserID = u.UserID
                    LEFT JOIN Staff s ON r.AdminID = s.StaffID
                    ORDER BY r.RequestTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        requests.Add(new UserContactRequestViewModel
                        {
                            RequestID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            UserName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            RequestStatus = reader.GetBoolean(5),
                            RequestTime = reader.GetDateTime(6),
                            ContactedTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            AdminID = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                            AdminName = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }

            return requests;
        }

        /// <summary>
        /// 标记请求为已处理（管理员开始处理时调用）
        /// </summary>
        public bool MarkAsContacted(int requestId, int adminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE UserContactRequests 
                    SET RequestStatus = 0, 
                        ContactedTime = GETDATE(),
                        AdminID = @AdminID
                    WHERE RequestID = @RequestID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// 检查用户是否有待处理的请求
        /// </summary>
        public bool HasPendingRequest(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT COUNT(*) 
                    FROM UserContactRequests 
                    WHERE UserID = @UserID AND RequestStatus = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}
