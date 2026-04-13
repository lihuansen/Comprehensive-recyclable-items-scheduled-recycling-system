using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    /// 超级管理员数据访问处理类。
    /// 超级管理员数据访问层
    public class SuperAdminDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        #region 超级管理员管理

        /// 获取超级管理员列表。
        /// 分页获取所有超级管理员
        public PagedResult<SuperAdmins> GetAllSuperAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            var result = new PagedResult<SuperAdmins>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<SuperAdmins>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string countSql = "SELECT COUNT(*) FROM SuperAdmins " + whereClause;
                SqlCommand countCmd = new SqlCommand(countSql, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countCmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                if (isActive.HasValue)
                {
                    countCmd.Parameters.AddWithValue("@IsActive", isActive.Value);
                }
                result.TotalCount = (int)countCmd.ExecuteScalar();

                string sql = "SELECT * FROM SuperAdmins " + whereClause + 
                    " ORDER BY SuperAdminID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                if (isActive.HasValue)
                {
                    cmd.Parameters.AddWithValue("@IsActive", isActive.Value);
                }
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(MapSuperAdminFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// 根据编号获取超级管理员。
        /// 根据ID获取超级管理员
        public SuperAdmins GetSuperAdminById(int superAdminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM SuperAdmins WHERE SuperAdminID = @SuperAdminID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SuperAdminID", superAdminId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapSuperAdminFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// 新增超级管理员。
        /// 添加新超级管理员
        public bool AddSuperAdmin(SuperAdmins superAdmin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO SuperAdmins (Username, PasswordHash, FullName, IsActive, CreatedDate) 
                    VALUES (@Username, @PasswordHash, @FullName, @IsActive, GETDATE())";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", superAdmin.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", superAdmin.PasswordHash);
                cmd.Parameters.AddWithValue("@FullName", superAdmin.FullName);
                cmd.Parameters.AddWithValue("@IsActive", superAdmin.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// 更新超级管理员。
        /// 更新超级管理员信息
        public bool UpdateSuperAdmin(SuperAdmins superAdmin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE SuperAdmins SET 
                    Username = @Username,
                    FullName = @FullName,
                    IsActive = @IsActive
                    WHERE SuperAdminID = @SuperAdminID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SuperAdminID", superAdmin.SuperAdminID);
                cmd.Parameters.AddWithValue("@Username", superAdmin.Username);
                cmd.Parameters.AddWithValue("@FullName", superAdmin.FullName);
                cmd.Parameters.AddWithValue("@IsActive", superAdmin.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// 删除超级管理员。
        /// 删除超级管理员（硬删除）
        /// 删除超级管理员。
        public bool DeleteSuperAdmin(int superAdminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                try
                {
                    string sql = "DELETE FROM SuperAdmins WHERE SuperAdminID = @SuperAdminID";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@SuperAdminID", superAdminId);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该超级管理员，因为存在关联的记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// 获取超级管理员统计数据。
        /// 获取超级管理员统计信息
        public Dictionary<string, object> GetSuperAdminStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SuperAdmins", conn);
                stats["TotalSuperAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM SuperAdmins WHERE IsActive = 1", conn);
                stats["ActiveSuperAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM SuperAdmins 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["NewSuperAdminsThisMonth"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// 获取超级管理员列表导出数据。
        /// 获取所有超级管理员用于导出（无分页）
        public List<SuperAdmins> GetAllSuperAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            var superAdmins = new List<SuperAdmins>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string query = $@"
                    SELECT SuperAdminID, Username, FullName, CreatedDate, LastLoginDate, IsActive 
                    FROM SuperAdmins 
                    {whereClause}
                    ORDER BY CreatedDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                }
                if (isActive.HasValue)
                {
                    cmd.Parameters.AddWithValue("@IsActive", isActive.Value);
                }

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        superAdmins.Add(new SuperAdmins
                        {
                            SuperAdminID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            CreatedDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            LastLoginDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            IsActive = reader.GetBoolean(5)
                        });
                    }
                }
            }

            return superAdmins;
        }

        /// 获取管理员仪表盘统计数据。
        /// 获取管理员数据看板统计信息
        public Dictionary<string, object> GetAdminDashboardStatistics()
        {
            var stats = new Dictionary<string, object>();

            stats["TotalAdmins"] = 0;
            stats["ActiveAdmins"] = 0;
            stats["InactiveAdmins"] = 0;
            stats["NewAdminsThisMonth"] = 0;
            stats["RecentLoginCount"] = 0;
            stats["PermissionDistribution"] = new List<Dictionary<string, object>>();
            stats["RegistrationTrend"] = new List<Dictionary<string, object>>();
            stats["LoginActivityTrend"] = new List<Dictionary<string, object>>();
            stats["AdminRanking"] = new List<Dictionary<string, object>>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Admins", conn);
                stats["TotalAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Admins WHERE IsActive = 1", conn);
                stats["ActiveAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Admins WHERE IsActive = 0", conn);
                stats["InactiveAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Admins 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["NewAdminsThisMonth"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Admins 
                    WHERE LastLoginDate >= DATEADD(day, -7, GETDATE())", conn);
                stats["RecentLoginCount"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"
                    SELECT ISNULL(Character, 'unset') AS Permission, COUNT(*) AS AdminCount
                    FROM Admins
                    GROUP BY Character
                    ORDER BY AdminCount DESC", conn);

                var permissionDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        permissionDistribution.Add(new Dictionary<string, object>
                        {
                            ["Permission"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["PermissionDistribution"] = permissionDistribution;

                cmd = new SqlCommand(@"
                    SELECT YEAR(CreatedDate) AS RegYear, MONTH(CreatedDate) AS RegMonth, COUNT(*) AS AdminCount
                    FROM Admins
                    WHERE CreatedDate >= DATEADD(month, -6, GETDATE())
                    GROUP BY YEAR(CreatedDate), MONTH(CreatedDate)
                    ORDER BY RegYear, RegMonth", conn);

                var registrationTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int year = reader.GetInt32(0);
                        int month = reader.GetInt32(1);
                        registrationTrend.Add(new Dictionary<string, object>
                        {
                            ["Month"] = $"{year}-{month:D2}",
                            ["Count"] = reader.GetInt32(2)
                        });
                    }
                }
                stats["RegistrationTrend"] = registrationTrend;

                cmd = new SqlCommand(@"
                    SELECT CAST(LastLoginDate AS DATE) AS LoginDate, COUNT(*) AS LoginCount
                    FROM Admins
                    WHERE LastLoginDate >= DATEADD(day, -7, GETDATE())
                    GROUP BY CAST(LastLoginDate AS DATE)
                    ORDER BY LoginDate", conn);

                var loginActivityTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loginActivityTrend.Add(new Dictionary<string, object>
                        {
                            ["Date"] = reader.GetDateTime(0).ToString("MM-dd"),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["LoginActivityTrend"] = loginActivityTrend;

                cmd = new SqlCommand(@"
                    SELECT TOP 10 AdminID, FullName, Username, 
                           ISNULL(Character, 'unset') AS Character,
                           LastLoginDate, IsActive
                    FROM Admins
                    WHERE IsActive = 1
                    ORDER BY CASE WHEN LastLoginDate IS NULL THEN 0 ELSE 1 END DESC, LastLoginDate DESC", conn);

                var adminRanking = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int rank = 1;
                    while (reader.Read())
                    {
                        adminRanking.Add(new Dictionary<string, object>
                        {
                            ["Rank"] = rank++,
                            ["AdminID"] = reader.GetInt32(0),
                            ["Name"] = reader.GetString(1),
                            ["Username"] = reader.GetString(2),
                            ["Permission"] = reader.GetString(3),
                            ["LastLogin"] = reader.IsDBNull(4) ? "" : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm"),
                            ["IsActive"] = reader.GetBoolean(5)
                        });
                    }
                }
                stats["AdminRanking"] = adminRanking;
            }

            return stats;
        }

        #endregion

        #region 辅助方法

        /// 将数据读取结果映射为超级管理员实体。
        /// 从数据读取器映射超级管理员对象
        private SuperAdmins MapSuperAdminFromReader(SqlDataReader reader)
        {
            return new SuperAdmins
            {
                SuperAdminID = reader.GetInt32(reader.GetOrdinal("SuperAdminID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                URL = reader.IsDBNull(reader.GetOrdinal("URL")) ? null : reader.GetString(reader.GetOrdinal("URL"))
            };
        }

        /// 更新超级管理员头像
        public bool UpdateSuperAdminAvatar(int superAdminId, string avatarUrl)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE SuperAdmins SET URL = @URL WHERE SuperAdminID = @SuperAdminID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@URL", (object)avatarUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SuperAdminID", superAdminId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        #endregion
    }
}
