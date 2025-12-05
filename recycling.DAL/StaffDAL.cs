using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using recycling.Model;
using System.Configuration;

namespace recycling.DAL
{
    public class StaffDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        #region 回收员登录验证（对应Recyclers表）
        /// <summary>
        /// 根据用户名查询回收员信息
        /// </summary>
        public Recyclers GetRecyclerByUsername(string username)
        {
            Recyclers recycler = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, LastLoginDate, IsActive, Available 
                                  FROM Recyclers 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            recycler = new Recyclers
                            {
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null,
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                Available = Convert.ToBoolean(reader["Available"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询回收员失败：" + ex.Message);
                }
            }
            return recycler;
        }

        /// <summary>
        /// 更新回收员最后登录时间
        /// </summary>
        public void UpdateRecyclerLastLogin(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Recyclers 
                               SET LastLoginDate = @LastLoginDate 
                               WHERE RecyclerID = @RecyclerID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("更新回收员登录时间失败：" + ex.Message);
                }
            }
        }
        #endregion

        #region 管理员登录验证（对应Admins表）
        /// <summary>
        /// 根据用户名查询管理员信息
        /// </summary>
        public Admins GetAdminByUsername(string username)
        {
            Admins admin = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT AdminID, Username, PasswordHash, LastLoginDate 
                                  FROM Admins 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Admins
                            {
                                AdminID = Convert.ToInt32(reader["AdminID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询管理员失败：" + ex.Message);
                }
            }
            return admin;
        }

        /// <summary>
        /// 更新管理员最后登录时间
        /// </summary>
        public void UpdateAdminLastLogin(int adminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Admins 
                               SET LastLoginDate = @LastLoginDate 
                               WHERE AdminID = @AdminID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AdminID", adminId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("更新管理员登录时间失败：" + ex.Message);
                }
            }
        }
        #endregion

        #region 超级管理员登录验证（对应SuperAdmins表）
        /// <summary>
        /// 根据用户名查询超级管理员信息
        /// </summary>
        public SuperAdmins GetSuperAdminByUsername(string username)
        {
            SuperAdmins superAdmin = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT SuperAdminID, Username, PasswordHash, LastLoginDate 
                                  FROM SuperAdmins 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            superAdmin = new SuperAdmins
                            {
                                SuperAdminID = Convert.ToInt32(reader["SuperAdminID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询超级管理员失败：" + ex.Message);
                }
            }
            return superAdmin;
        }

        /// <summary>
        /// 更新超级管理员最后登录时间
        /// </summary>
        public void UpdateSuperAdminLastLogin(int superAdminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE SuperAdmins 
                               SET LastLoginDate = @LastLoginDate 
                               WHERE SuperAdminID = @SuperAdminID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@SuperAdminID", superAdminId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("更新超级管理员登录时间失败：" + ex.Message);
                }
            }
        }
        #endregion

        // 在StaffDAL类中添加
        /// <summary>
        /// 根据ID获取回收员信息（供BLL层调用）
        /// </summary>
        public Recyclers GetRecyclerById(int recyclerId)
        {
            if (recyclerId <= 0)
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT RecyclerID, Username, FullName, PhoneNumber, IsActive, Available 
                      FROM Recyclers 
                      WHERE RecyclerID = @RecyclerID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Recyclers
                        {
                            RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                            Username = reader["Username"].ToString(),
                            FullName = reader["FullName"]?.ToString(),
                            PhoneNumber = reader["PhoneNumber"]?.ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            Available = Convert.ToBoolean(reader["Available"])
                        };
                    }
                }
            }
            return null;
        }
    }
}

