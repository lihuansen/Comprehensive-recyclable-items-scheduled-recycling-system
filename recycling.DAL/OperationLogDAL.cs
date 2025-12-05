using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class OperationLogDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加操作日志
        /// </summary>
        public bool AddLog(AdminOperationLogs log)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO AdminOperationLogs 
                    (AdminID, AdminUsername, Module, OperationType, Description, TargetID, TargetName, IPAddress, OperationTime, Result, Details)
                    VALUES 
                    (@AdminID, @AdminUsername, @Module, @OperationType, @Description, @TargetID, @TargetName, @IPAddress, @OperationTime, @Result, @Details)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AdminID", log.AdminID);
                cmd.Parameters.AddWithValue("@AdminUsername", log.AdminUsername ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Module", log.Module);
                cmd.Parameters.AddWithValue("@OperationType", log.OperationType);
                cmd.Parameters.AddWithValue("@Description", log.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetID", log.TargetID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetName", log.TargetName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IPAddress", log.IPAddress ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OperationTime", log.OperationTime);
                cmd.Parameters.AddWithValue("@Result", log.Result ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Details", log.Details ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 获取操作日志列表（分页）
        /// </summary>
        public PagedResult<AdminOperationLogs> GetLogs(int page = 1, int pageSize = 20, string module = null, string operationType = null, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null)
        {
            var result = new PagedResult<AdminOperationLogs>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<AdminOperationLogs>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(module))
                {
                    whereClause += " AND Module = @Module";
                }
                if (!string.IsNullOrEmpty(operationType))
                {
                    whereClause += " AND OperationType = @OperationType";
                }
                if (startDate.HasValue)
                {
                    whereClause += " AND OperationTime >= @StartDate";
                }
                if (endDate.HasValue)
                {
                    whereClause += " AND OperationTime <= @EndDate";
                }
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (AdminUsername LIKE @SearchTerm OR Description LIKE @SearchTerm OR TargetName LIKE @SearchTerm)";
                }

                // Get total count
                string countSql = "SELECT COUNT(*) FROM AdminOperationLogs " + whereClause;
                SqlCommand countCmd = new SqlCommand(countSql, conn);
                AddWhereParameters(countCmd, module, operationType, startDate, endDate, searchTerm);
                result.TotalCount = (int)countCmd.ExecuteScalar();

                // Get paged data
                string sql = @"SELECT * FROM AdminOperationLogs " + whereClause +
                    " ORDER BY OperationTime DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                AddWhereParameters(cmd, module, operationType, startDate, endDate, searchTerm);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(MapLogFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取日志统计信息
        /// </summary>
        public Dictionary<string, object> GetLogStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total logs
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM AdminOperationLogs", conn);
                stats["TotalLogs"] = (int)cmd.ExecuteScalar();

                // Today's logs
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM AdminOperationLogs 
                    WHERE CAST(OperationTime AS DATE) = CAST(GETDATE() AS DATE)", conn);
                stats["TodayLogs"] = (int)cmd.ExecuteScalar();

                // This week's logs
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM AdminOperationLogs 
                    WHERE OperationTime >= DATEADD(day, -7, GETDATE())", conn);
                stats["WeekLogs"] = (int)cmd.ExecuteScalar();

                // Logs by module
                cmd = new SqlCommand(@"SELECT Module, COUNT(*) AS Count 
                    FROM AdminOperationLogs 
                    GROUP BY Module 
                    ORDER BY Count DESC", conn);

                var moduleDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        moduleDistribution.Add(new Dictionary<string, object>
                        {
                            ["Module"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["ModuleDistribution"] = moduleDistribution;

                // Logs by operation type
                cmd = new SqlCommand(@"SELECT OperationType, COUNT(*) AS Count 
                    FROM AdminOperationLogs 
                    GROUP BY OperationType 
                    ORDER BY Count DESC", conn);

                var operationDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        operationDistribution.Add(new Dictionary<string, object>
                        {
                            ["OperationType"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["OperationDistribution"] = operationDistribution;
            }

            return stats;
        }

        /// <summary>
        /// 导出日志（不分页）
        /// </summary>
        public List<AdminOperationLogs> GetLogsForExport(string module = null, string operationType = null, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null)
        {
            var logs = new List<AdminOperationLogs>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(module))
                {
                    whereClause += " AND Module = @Module";
                }
                if (!string.IsNullOrEmpty(operationType))
                {
                    whereClause += " AND OperationType = @OperationType";
                }
                if (startDate.HasValue)
                {
                    whereClause += " AND OperationTime >= @StartDate";
                }
                if (endDate.HasValue)
                {
                    whereClause += " AND OperationTime <= @EndDate";
                }
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (AdminUsername LIKE @SearchTerm OR Description LIKE @SearchTerm OR TargetName LIKE @SearchTerm)";
                }

                string sql = @"SELECT * FROM AdminOperationLogs " + whereClause + " ORDER BY OperationTime DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                AddWhereParameters(cmd, module, operationType, startDate, endDate, searchTerm);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(MapLogFromReader(reader));
                    }
                }
            }

            return logs;
        }

        #region Helper Methods

        private void AddWhereParameters(SqlCommand cmd, string module, string operationType, DateTime? startDate, DateTime? endDate, string searchTerm)
        {
            if (!string.IsNullOrEmpty(module))
            {
                cmd.Parameters.AddWithValue("@Module", module);
            }
            if (!string.IsNullOrEmpty(operationType))
            {
                cmd.Parameters.AddWithValue("@OperationType", operationType);
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            }
        }

        private AdminOperationLogs MapLogFromReader(SqlDataReader reader)
        {
            return new AdminOperationLogs
            {
                LogID = reader.GetInt32(reader.GetOrdinal("LogID")),
                AdminID = reader.GetInt32(reader.GetOrdinal("AdminID")),
                AdminUsername = reader.IsDBNull(reader.GetOrdinal("AdminUsername")) ? null : reader.GetString(reader.GetOrdinal("AdminUsername")),
                Module = reader.GetString(reader.GetOrdinal("Module")),
                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                TargetID = reader.IsDBNull(reader.GetOrdinal("TargetID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("TargetID")),
                TargetName = reader.IsDBNull(reader.GetOrdinal("TargetName")) ? null : reader.GetString(reader.GetOrdinal("TargetName")),
                IPAddress = reader.IsDBNull(reader.GetOrdinal("IPAddress")) ? null : reader.GetString(reader.GetOrdinal("IPAddress")),
                OperationTime = reader.GetDateTime(reader.GetOrdinal("OperationTime")),
                Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                Details = reader.IsDBNull(reader.GetOrdinal("Details")) ? null : reader.GetString(reader.GetOrdinal("Details"))
            };
        }

        #endregion
    }
}
