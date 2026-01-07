using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    /// <summary>
    /// 钱包交易数据访问层
    /// </summary>
    public class WalletTransactionDAL
    {
        /// <summary>
        /// 添加交易记录
        /// </summary>
        public int AddTransaction(WalletTransaction transaction)
        {
            string sql = @"INSERT INTO WalletTransactions 
                          (UserID, TransactionType, Amount, BalanceBefore, BalanceAfter, 
                           PaymentAccountID, RelatedOrderID, TransactionStatus, Description, 
                           TransactionNo, CreatedDate, CompletedDate, Remarks)
                          VALUES 
                          (@UserID, @TransactionType, @Amount, @BalanceBefore, @BalanceAfter, 
                           @PaymentAccountID, @RelatedOrderID, @TransactionStatus, @Description, 
                           @TransactionNo, @CreatedDate, @CompletedDate, @Remarks);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", transaction.UserID),
                new SqlParameter("@TransactionType", transaction.TransactionType),
                new SqlParameter("@Amount", transaction.Amount),
                new SqlParameter("@BalanceBefore", transaction.BalanceBefore),
                new SqlParameter("@BalanceAfter", transaction.BalanceAfter),
                new SqlParameter("@PaymentAccountID", (object)transaction.PaymentAccountID ?? DBNull.Value),
                new SqlParameter("@RelatedOrderID", (object)transaction.RelatedOrderID ?? DBNull.Value),
                new SqlParameter("@TransactionStatus", transaction.TransactionStatus),
                new SqlParameter("@Description", (object)transaction.Description ?? DBNull.Value),
                new SqlParameter("@TransactionNo", transaction.TransactionNo),
                new SqlParameter("@CreatedDate", transaction.CreatedDate),
                new SqlParameter("@CompletedDate", (object)transaction.CompletedDate ?? DBNull.Value),
                new SqlParameter("@Remarks", (object)transaction.Remarks ?? DBNull.Value)
            };

            return Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnectionString, CommandType.Text, sql, parameters));
        }

        /// <summary>
        /// 根据用户ID获取交易记录列表（分页）
        /// </summary>
        public List<WalletTransaction> GetTransactionsByUserId(int userId, int pageIndex = 1, int pageSize = 20)
        {
            string sql = @"SELECT * FROM (
                            SELECT *, ROW_NUMBER() OVER (ORDER BY CreatedDate DESC) AS RowNum
                            FROM WalletTransactions 
                            WHERE UserID = @UserID
                          ) AS T
                          WHERE T.RowNum BETWEEN @StartRow AND @EndRow";

            int startRow = (pageIndex - 1) * pageSize + 1;
            int endRow = pageIndex * pageSize;

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@StartRow", startRow),
                new SqlParameter("@EndRow", endRow)
            };

            List<WalletTransaction> transactions = new List<WalletTransaction>();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                while (reader.Read())
                {
                    transactions.Add(MapReaderToTransaction(reader));
                }
            }
            return transactions;
        }

        /// <summary>
        /// 根据交易ID获取交易记录
        /// </summary>
        public WalletTransaction GetTransactionById(int transactionId)
        {
            string sql = "SELECT * FROM WalletTransactions WHERE TransactionID = @TransactionID";
            SqlParameter[] parameters = {
                new SqlParameter("@TransactionID", transactionId)
            };

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    return MapReaderToTransaction(reader);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据交易流水号获取交易记录
        /// </summary>
        public WalletTransaction GetTransactionByNo(string transactionNo)
        {
            string sql = "SELECT * FROM WalletTransactions WHERE TransactionNo = @TransactionNo";
            SqlParameter[] parameters = {
                new SqlParameter("@TransactionNo", transactionNo)
            };

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    return MapReaderToTransaction(reader);
                }
            }
            return null;
        }

        /// <summary>
        /// 更新交易状态
        /// </summary>
        public bool UpdateTransactionStatus(int transactionId, string status, DateTime? completedDate = null)
        {
            string sql = @"UPDATE WalletTransactions 
                          SET TransactionStatus = @Status,
                              CompletedDate = @CompletedDate
                          WHERE TransactionID = @TransactionID";

            SqlParameter[] parameters = {
                new SqlParameter("@TransactionID", transactionId),
                new SqlParameter("@Status", status),
                new SqlParameter("@CompletedDate", (object)completedDate ?? DBNull.Value)
            };

            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// 获取用户交易统计信息
        /// </summary>
        public Dictionary<string, decimal> GetUserTransactionStatistics(int userId)
        {
            string sql = @"
                SELECT 
                    SUM(CASE WHEN TransactionType IN ('Recharge', 'Refund', 'Income') AND TransactionStatus = 'Completed' THEN Amount ELSE 0 END) AS TotalIncome,
                    SUM(CASE WHEN TransactionType IN ('Withdraw', 'Payment') AND TransactionStatus = 'Completed' THEN Amount ELSE 0 END) AS TotalExpense,
                    COUNT(CASE WHEN MONTH(CreatedDate) = MONTH(GETDATE()) AND YEAR(CreatedDate) = YEAR(GETDATE()) THEN 1 END) AS MonthlyCount
                FROM WalletTransactions
                WHERE UserID = @UserID";

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId)
            };

            Dictionary<string, decimal> stats = new Dictionary<string, decimal>
            {
                { "TotalIncome", 0 },
                { "TotalExpense", 0 },
                { "MonthlyCount", 0 }
            };

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    stats["TotalIncome"] = reader["TotalIncome"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalIncome"]);
                    stats["TotalExpense"] = reader["TotalExpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalExpense"]);
                    stats["MonthlyCount"] = reader["MonthlyCount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["MonthlyCount"]);
                }
            }
            return stats;
        }

        /// <summary>
        /// 获取用户交易记录总数
        /// </summary>
        public int GetTransactionCountByUserId(int userId)
        {
            string sql = "SELECT COUNT(*) FROM WalletTransactions WHERE UserID = @UserID";
            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId)
            };
            return Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnectionString, CommandType.Text, sql, parameters));
        }

        /// <summary>
        /// 生成唯一的交易流水号
        /// </summary>
        public string GenerateTransactionNo()
        {
            // 格式: TXN + 年月日时分秒 + 6位随机数
            return "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100000, 999999).ToString();
        }

        /// <summary>
        /// 映射 SqlDataReader 到 WalletTransaction 对象
        /// </summary>
        private WalletTransaction MapReaderToTransaction(SqlDataReader reader)
        {
            return new WalletTransaction
            {
                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                TransactionType = reader["TransactionType"].ToString(),
                Amount = Convert.ToDecimal(reader["Amount"]),
                BalanceBefore = Convert.ToDecimal(reader["BalanceBefore"]),
                BalanceAfter = Convert.ToDecimal(reader["BalanceAfter"]),
                PaymentAccountID = reader["PaymentAccountID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PaymentAccountID"]),
                RelatedOrderID = reader["RelatedOrderID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RelatedOrderID"]),
                TransactionStatus = reader["TransactionStatus"].ToString(),
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                TransactionNo = reader["TransactionNo"].ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CompletedDate = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletedDate"]),
                Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString()
            };
        }
    }
}
