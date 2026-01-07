using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 用户支付账户数据访问层
    /// </summary>
    public class PaymentAccountDAL
    {
        // 从配置文件获取数据库连接字符串
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
        /// <summary>
        /// 添加支付账户
        /// </summary>
        public int AddPaymentAccount(UserPaymentAccount account)
        {
            string sql = @"INSERT INTO UserPaymentAccounts 
                          (UserID, AccountType, AccountName, AccountNumber, BankName, 
                           IsDefault, IsVerified, CreatedDate, Status)
                          VALUES 
                          (@UserID, @AccountType, @AccountName, @AccountNumber, @BankName, 
                           @IsDefault, @IsVerified, @CreatedDate, @Status);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", account.UserID);
                cmd.Parameters.AddWithValue("@AccountType", account.AccountType);
                cmd.Parameters.AddWithValue("@AccountName", account.AccountName);
                cmd.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                cmd.Parameters.AddWithValue("@BankName", (object)account.BankName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsDefault", account.IsDefault);
                cmd.Parameters.AddWithValue("@IsVerified", account.IsVerified);
                cmd.Parameters.AddWithValue("@CreatedDate", account.CreatedDate);
                cmd.Parameters.AddWithValue("@Status", account.Status);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 根据用户ID获取支付账户列表
        /// </summary>
        public List<UserPaymentAccount> GetPaymentAccountsByUserId(int userId)
        {
            string sql = @"SELECT * FROM UserPaymentAccounts 
                          WHERE UserID = @UserID AND Status != 'Deleted'
                          ORDER BY IsDefault DESC, CreatedDate DESC";

            List<UserPaymentAccount> accounts = new List<UserPaymentAccount>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(MapReaderToAccount(reader));
                    }
                }
            }
            return accounts;
        }

        /// <summary>
        /// 根据账户ID获取支付账户
        /// </summary>
        public UserPaymentAccount GetPaymentAccountById(int accountId)
        {
            string sql = @"SELECT * FROM UserPaymentAccounts 
                          WHERE AccountID = @AccountID AND Status != 'Deleted'";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AccountID", accountId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToAccount(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 更新支付账户
        /// </summary>
        public bool UpdatePaymentAccount(UserPaymentAccount account)
        {
            string sql = @"UPDATE UserPaymentAccounts 
                          SET AccountName = @AccountName,
                              AccountNumber = @AccountNumber,
                              BankName = @BankName,
                              IsDefault = @IsDefault,
                              IsVerified = @IsVerified,
                              LastUsedDate = @LastUsedDate,
                              Status = @Status
                          WHERE AccountID = @AccountID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                cmd.Parameters.AddWithValue("@AccountName", account.AccountName);
                cmd.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                cmd.Parameters.AddWithValue("@BankName", (object)account.BankName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsDefault", account.IsDefault);
                cmd.Parameters.AddWithValue("@IsVerified", account.IsVerified);
                cmd.Parameters.AddWithValue("@LastUsedDate", (object)account.LastUsedDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", account.Status);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 删除支付账户（软删除）
        /// </summary>
        public bool DeletePaymentAccount(int accountId)
        {
            string sql = "UPDATE UserPaymentAccounts SET Status = 'Deleted' WHERE AccountID = @AccountID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AccountID", accountId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 设置默认支付账户
        /// </summary>
        public bool SetDefaultAccount(int userId, int accountId)
        {
            string sql = @"
                UPDATE UserPaymentAccounts SET IsDefault = 0 WHERE UserID = @UserID;
                UPDATE UserPaymentAccounts SET IsDefault = 1 WHERE AccountID = @AccountID AND UserID = @UserID;
            ";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@AccountID", accountId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 更新账户最后使用时间
        /// </summary>
        public bool UpdateLastUsedDate(int accountId)
        {
            string sql = "UPDATE UserPaymentAccounts SET LastUsedDate = @LastUsedDate WHERE AccountID = @AccountID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AccountID", accountId);
                cmd.Parameters.AddWithValue("@LastUsedDate", DateTime.Now);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 映射 SqlDataReader 到 UserPaymentAccount 对象
        /// </summary>
        private UserPaymentAccount MapReaderToAccount(SqlDataReader reader)
        {
            return new UserPaymentAccount
            {
                AccountID = Convert.ToInt32(reader["AccountID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                AccountType = reader["AccountType"].ToString(),
                AccountName = reader["AccountName"].ToString(),
                AccountNumber = reader["AccountNumber"].ToString(),
                BankName = reader["BankName"] == DBNull.Value ? null : reader["BankName"].ToString(),
                IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                IsVerified = Convert.ToBoolean(reader["IsVerified"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                LastUsedDate = reader["LastUsedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastUsedDate"]),
                Status = reader["Status"].ToString()
            };
        }
    }
}
