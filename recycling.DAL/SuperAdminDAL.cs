using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// SuperAdmin Data Access Layer
    /// 超级管理员数据访问层
    /// </summary>
    public class SuperAdminDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        #region SuperAdmin Management

        /// <summary>
        /// Get all super admins with pagination
        /// 分页获取所有超级管理员
        /// </summary>
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

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                // Get total count
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

                // Get paged data
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

        /// <summary>
        /// Get super admin by ID
        /// 根据ID获取超级管理员
        /// </summary>
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

        /// <summary>
        /// Add new super admin
        /// 添加新超级管理员
        /// </summary>
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

        /// <summary>
        /// Update super admin information
        /// 更新超级管理员信息
        /// </summary>
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

        /// <summary>
        /// Delete super admin (hard delete from database)
        /// 删除超级管理员（硬删除）
        /// WARNING: This performs a hard delete. If the super admin has associated records,
        /// this operation may fail due to foreign key constraints.
        /// </summary>
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
                    // If foreign key constraint violation (error 547), provide helpful message
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该超级管理员，因为存在关联的记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get super admin statistics
        /// 获取超级管理员统计信息
        /// </summary>
        public Dictionary<string, object> GetSuperAdminStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total super admins
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SuperAdmins", conn);
                stats["TotalSuperAdmins"] = (int)cmd.ExecuteScalar();

                // Active super admins
                cmd = new SqlCommand("SELECT COUNT(*) FROM SuperAdmins WHERE IsActive = 1", conn);
                stats["ActiveSuperAdmins"] = (int)cmd.ExecuteScalar();

                // Super admins created this month
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM SuperAdmins 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["NewSuperAdminsThisMonth"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// <summary>
        /// Get all super admins for export (without pagination)
        /// 获取所有超级管理员用于导出（无分页）
        /// </summary>
        public List<SuperAdmins> GetAllSuperAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            var superAdmins = new List<SuperAdmins>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Map super admin from data reader
        /// 从数据读取器映射超级管理员对象
        /// </summary>
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
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        #endregion
    }
}
