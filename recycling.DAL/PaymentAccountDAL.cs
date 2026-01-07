using System;
using System.Collections.Generic;
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

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", account.UserID),
                new SqlParameter("@AccountType", account.AccountType),
                new SqlParameter("@AccountName", account.AccountName),
                new SqlParameter("@AccountNumber", account.AccountNumber),
                new SqlParameter("@BankName", (object)account.BankName ?? DBNull.Value),
                new SqlParameter("@IsDefault", account.IsDefault),
                new SqlParameter("@IsVerified", account.IsVerified),
                new SqlParameter("@CreatedDate", account.CreatedDate),
                new SqlParameter("@Status", account.Status)
            };

            return Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnectionString, CommandType.Text, sql, parameters));
        }

        /// <summary>
        /// 根据用户ID获取支付账户列表
        /// </summary>
        public List<UserPaymentAccount> GetPaymentAccountsByUserId(int userId)
        {
            string sql = @"SELECT * FROM UserPaymentAccounts 
                          WHERE UserID = @UserID AND Status != 'Deleted'
                          ORDER BY IsDefault DESC, CreatedDate DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId)
            };

            List<UserPaymentAccount> accounts = new List<UserPaymentAccount>();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                while (reader.Read())
                {
                    accounts.Add(MapReaderToAccount(reader));
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

            SqlParameter[] parameters = {
                new SqlParameter("@AccountID", accountId)
            };

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    return MapReaderToAccount(reader);
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

            SqlParameter[] parameters = {
                new SqlParameter("@AccountID", account.AccountID),
                new SqlParameter("@AccountName", account.AccountName),
                new SqlParameter("@AccountNumber", account.AccountNumber),
                new SqlParameter("@BankName", (object)account.BankName ?? DBNull.Value),
                new SqlParameter("@IsDefault", account.IsDefault),
                new SqlParameter("@IsVerified", account.IsVerified),
                new SqlParameter("@LastUsedDate", (object)account.LastUsedDate ?? DBNull.Value),
                new SqlParameter("@Status", account.Status)
            };

            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// 删除支付账户（软删除）
        /// </summary>
        public bool DeletePaymentAccount(int accountId)
        {
            string sql = "UPDATE UserPaymentAccounts SET Status = 'Deleted' WHERE AccountID = @AccountID";
            SqlParameter[] parameters = {
                new SqlParameter("@AccountID", accountId)
            };
            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
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

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@AccountID", accountId)
            };

            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// 更新账户最后使用时间
        /// </summary>
        public bool UpdateLastUsedDate(int accountId)
        {
            string sql = "UPDATE UserPaymentAccounts SET LastUsedDate = @LastUsedDate WHERE AccountID = @AccountID";
            SqlParameter[] parameters = {
                new SqlParameter("@AccountID", accountId),
                new SqlParameter("@LastUsedDate", DateTime.Now)
            };
            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
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
