using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class UserDAL
    {
        // 从配置文件获取数据库连接字符串
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        public bool IsUsernameExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// 检查手机号是否已存在
        /// </summary>
        public bool IsPhoneExists(string phoneNumber)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Users WHERE PhoneNumber = @PhoneNumber";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        public bool IsEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// 插入新用户到数据库
        /// </summary>
        public int InsertUser(Users user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Users (Username, PasswordHash, PhoneNumber, Email, RegistrationDate)
                               VALUES (@Username, @PasswordHash, @PhoneNumber, @Email, @RegistrationDate);
                               SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@RegistrationDate", user.RegistrationDate);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 根据用户名查询用户信息（已修正实体类名）
        /// </summary>
        public Users GetUserByUsername(string username)  // 改为Users，与InsertUser的实体类一致
        {
            Users user = null;  // 实体类名统一为Users
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT UserID, Username, PasswordHash, PhoneNumber, Email, RegistrationDate 
                                  FROM Users 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new Users  // 实例化时使用统一的实体类名
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Email = reader["Email"].ToString(),
                                RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 建议使用日志框架记录错误，而非直接抛出
                    throw new Exception("查询用户失败：" + ex.Message);
                }
            }
            return user;
        }

        /// <summary>
        /// 更新用户最后登录时间
        /// </summary>
        /// <param name="userId">用户ID（对应Users表的UserID）</param>
        /// <param name="lastLoginDate">最后登录时间</param>
        public void UpdateLastLoginDate(int userId, DateTime lastLoginDate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 匹配Users表字段：UserID（主键）和LastLoginDate
                string sql = @"UPDATE Users 
                       SET LastLoginDate = @LastLoginDate 
                       WHERE UserID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                // 参数化查询，确保与datetime2类型匹配
                cmd.Parameters.AddWithValue("@LastLoginDate", lastLoginDate);
                cmd.Parameters.AddWithValue("@UserID", userId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // 保持与现有DAL异常处理风格一致
                    throw new Exception("更新最后登录时间失败：" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 根据手机号更新密码
        /// </summary>
        public bool UpdatePasswordByPhone(string phoneNumber, string newPasswordHash)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Users 
                               SET PasswordHash = @PasswordHash,
                                   LastPasswordChangeDate = @LastPasswordChangeDate
                               WHERE PhoneNumber = @PhoneNumber";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                cmd.Parameters.AddWithValue("@LastPasswordChangeDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

    }
}
