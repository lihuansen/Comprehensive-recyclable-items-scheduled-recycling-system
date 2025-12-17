using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using recycling.Model;

namespace recycling.DAL
{
    public class AdminDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        #region User Management

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public PagedResult<Users> GetAllUsers(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            var result = new PagedResult<Users>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<Users>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Get total count
                string countSql = "SELECT COUNT(*) FROM Users WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countSql += " AND (Username LIKE @SearchTerm OR Email LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm)";
                }

                SqlCommand countCmd = new SqlCommand(countSql, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countCmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                result.TotalCount = (int)countCmd.ExecuteScalar();

                // Get paged data
                string sql = @"SELECT * FROM Users WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sql += " AND (Username LIKE @SearchTerm OR Email LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm)";
                }
                sql += " ORDER BY RegistrationDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(new Users
                        {
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate")),
                            LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("LastLoginDate"))
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        public Dictionary<string, object> GetUserStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total users
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn);
                stats["TotalUsers"] = (int)cmd.ExecuteScalar();

                // Users registered this month
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Users 
                    WHERE YEAR(RegistrationDate) = YEAR(GETDATE()) 
                    AND MONTH(RegistrationDate) = MONTH(GETDATE())", conn);
                stats["NewUsersThisMonth"] = (int)cmd.ExecuteScalar();

                // Active users (logged in last 30 days)
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Users 
                    WHERE LastLoginDate >= DATEADD(day, -30, GETDATE())", conn);
                stats["ActiveUsers"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// <summary>
        /// Get all users for export (without pagination)
        /// </summary>
        public List<Users> GetAllUsersForExport(string searchTerm = null)
        {
            var users = new List<Users>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR Email LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm)";
                }

                string query = $@"
                    SELECT UserID, Username, Email, PhoneNumber, RegistrationDate, LastLoginDate 
                    FROM Users 
                    {whereClause}
                    ORDER BY RegistrationDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                }

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new Users
                        {
                            UserID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                            PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                            RegistrationDate = (DateTime)(reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4)),
                            LastLoginDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                        });
                    }
                }
            }

            return users;
        }

        #endregion

        #region Recycler Management

        /// <summary>
        /// Get all recyclers with pagination
        /// </summary>
        public PagedResult<Recyclers> GetAllRecyclers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            var result = new PagedResult<Recyclers>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<Recyclers>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR Region LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                // Get total count
                string countSql = "SELECT COUNT(*) FROM Recyclers " + whereClause;
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
                string sql = "SELECT * FROM Recyclers " + whereClause + 
                    " ORDER BY RecyclerID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                        result.Items.Add(MapRecyclerFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get recycler by ID
        /// </summary>
        public Recyclers GetRecyclerById(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Recyclers WHERE RecyclerID = @RecyclerID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRecyclerFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add new recycler
        /// </summary>
        public bool AddRecycler(Recyclers recycler)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Recyclers (Username, PasswordHash, PhoneNumber, FullName, Region, Available, IsActive, CreatedDate, Rating) 
                    VALUES (@Username, @PasswordHash, @PhoneNumber, @FullName, @Region, @Available, @IsActive, GETDATE(), 0)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", recycler.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", recycler.PasswordHash);
                cmd.Parameters.AddWithValue("@PhoneNumber", recycler.PhoneNumber);
                cmd.Parameters.AddWithValue("@FullName", recycler.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", recycler.Region);
                cmd.Parameters.AddWithValue("@Available", recycler.Available);
                cmd.Parameters.AddWithValue("@IsActive", recycler.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Update recycler information
        /// </summary>
        public bool UpdateRecycler(Recyclers recycler)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE Recyclers SET 
                    Username = @Username,
                    PhoneNumber = @PhoneNumber,
                    FullName = @FullName,
                    Region = @Region,
                    Available = @Available,
                    IsActive = @IsActive
                    WHERE RecyclerID = @RecyclerID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RecyclerID", recycler.RecyclerID);
                cmd.Parameters.AddWithValue("@Username", recycler.Username);
                cmd.Parameters.AddWithValue("@PhoneNumber", recycler.PhoneNumber);
                cmd.Parameters.AddWithValue("@FullName", recycler.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", recycler.Region);
                cmd.Parameters.AddWithValue("@Available", recycler.Available);
                cmd.Parameters.AddWithValue("@IsActive", recycler.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Delete recycler (hard delete from database)
        /// WARNING: This performs a hard delete. If the recycler has associated records 
        /// (orders, reviews, conversations, etc.), this operation may fail due to 
        /// foreign key constraints.
        /// </summary>
        public bool DeleteRecycler(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                try
                {
                    string sql = "DELETE FROM Recyclers WHERE RecyclerID = @RecyclerID";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    // If foreign key constraint violation (error 547), provide helpful message
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该回收员，因为存在关联的订单或评价记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get recycler completed orders count
        /// </summary>
        public int GetRecyclerCompletedOrdersCount(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM Appointments 
                    WHERE RecyclerID = @RecyclerID AND Status = N'已完成'";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Get recycler statistics
        /// </summary>
        public Dictionary<string, object> GetRecyclerStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total recyclers
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers", conn);
                stats["TotalRecyclers"] = (int)cmd.ExecuteScalar();

                // Active recyclers
                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE IsActive = 1", conn);
                stats["ActiveRecyclers"] = (int)cmd.ExecuteScalar();

                // Available recyclers
                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE Available = 1 AND IsActive = 1", conn);
                stats["AvailableRecyclers"] = (int)cmd.ExecuteScalar();

                // Top performers (top 5 by completed orders)
                cmd = new SqlCommand(@"SELECT TOP 5 r.RecyclerID, r.FullName, r.Username, COUNT(a.AppointmentID) AS CompletedOrders
                    FROM Recyclers r
                    LEFT JOIN Appointments a ON r.RecyclerID = a.RecyclerID AND a.Status = N'已完成'
                    WHERE r.IsActive = 1
                    GROUP BY r.RecyclerID, r.FullName, r.Username
                    ORDER BY CompletedOrders DESC", conn);

                var topPerformers = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topPerformers.Add(new Dictionary<string, object>
                        {
                            ["RecyclerID"] = reader.GetInt32(0),
                            ["FullName"] = reader.IsDBNull(1) ? reader.GetString(2) : reader.GetString(1),
                            ["Username"] = reader.GetString(2),
                            ["CompletedOrders"] = reader.GetInt32(3)
                        });
                    }
                }
                stats["TopPerformers"] = topPerformers;
            }

            return stats;
        }

        /// <summary>
        /// Get comprehensive recycler dashboard statistics for admin
        /// </summary>
        public Dictionary<string, object> GetRecyclerDashboardStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // === Basic Statistics (keep original) ===
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers", conn);
                stats["TotalRecyclers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE IsActive = 1", conn);
                stats["ActiveRecyclers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE Available = 1 AND IsActive = 1", conn);
                stats["AvailableRecyclers"] = (int)cmd.ExecuteScalar();

                // === Daily Recycling by Category (今日各分类回收总量) ===
                cmd = new SqlCommand(@"
                    SELECT CategoryName, ISNULL(SUM(Weight), 0) AS TotalWeight
                    FROM Inventory
                    WHERE CreatedDate >= CAST(GETDATE() AS DATE) AND CreatedDate < DATEADD(day, 1, CAST(GETDATE() AS DATE))
                    GROUP BY CategoryName
                    ORDER BY TotalWeight DESC", conn);

                var dailyCategoryWeight = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dailyCategoryWeight.Add(new Dictionary<string, object>
                        {
                            ["CategoryName"] = reader.GetString(0),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(1))
                        });
                    }
                }
                stats["DailyCategoryWeight"] = dailyCategoryWeight;

                // Total daily weight
                cmd = new SqlCommand(@"
                    SELECT ISNULL(SUM(Weight), 0) FROM Inventory 
                    WHERE CreatedDate >= CAST(GETDATE() AS DATE) AND CreatedDate < DATEADD(day, 1, CAST(GETDATE() AS DATE))", conn);
                stats["TodayTotalWeight"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // === Regional Recycling Totals (十个街道区域回收总量) ===
                cmd = new SqlCommand(@"
                    SELECT r.Region, ISNULL(SUM(i.Weight), 0) AS TotalWeight
                    FROM Recyclers r
                    LEFT JOIN Inventory i ON r.RecyclerID = i.RecyclerID
                    WHERE r.Region IS NOT NULL AND r.Region <> ''
                    GROUP BY r.Region
                    ORDER BY TotalWeight DESC", conn);

                var regionWeight = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        regionWeight.Add(new Dictionary<string, object>
                        {
                            ["Region"] = reader.GetString(0),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(1))
                        });
                    }
                }
                stats["RegionWeight"] = regionWeight;

                // === Recycler Total Weight Ranking (回收员累计回收总量排名 - 从入职到现在) ===
                cmd = new SqlCommand(@"
                    SELECT r.RecyclerID, ISNULL(r.FullName, r.Username) AS Name, r.Username, 
                           r.Region, ISNULL(r.Rating, 0) AS Rating,
                           ISNULL(SUM(i.Weight), 0) AS TotalWeight,
                           COUNT(DISTINCT i.OrderID) AS CompletedOrders
                    FROM Recyclers r
                    LEFT JOIN Inventory i ON r.RecyclerID = i.RecyclerID
                    WHERE r.IsActive = 1
                    GROUP BY r.RecyclerID, r.FullName, r.Username, r.Region, r.Rating
                    ORDER BY TotalWeight DESC", conn);

                var recyclerRanking = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int rank = 1;
                    while (reader.Read())
                    {
                        recyclerRanking.Add(new Dictionary<string, object>
                        {
                            ["Rank"] = rank++,
                            ["RecyclerID"] = reader.GetInt32(0),
                            ["Name"] = reader.GetString(1),
                            ["Username"] = reader.GetString(2),
                            ["Region"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            ["Rating"] = Convert.ToDecimal(reader.GetValue(4)),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(5)),
                            ["CompletedOrders"] = reader.GetInt32(6)
                        });
                    }
                }
                stats["RecyclerRanking"] = recyclerRanking;

                // === Additional Statistics ===
                // Today's completed orders
                cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Appointments 
                    WHERE Status = N'已完成' AND UpdatedDate >= CAST(GETDATE() AS DATE) AND UpdatedDate < DATEADD(day, 1, CAST(GETDATE() AS DATE))", conn);
                stats["TodayCompletedOrders"] = (int)cmd.ExecuteScalar();

                // This month's total weight
                cmd = new SqlCommand(@"
                    SELECT ISNULL(SUM(Weight), 0) FROM Inventory 
                    WHERE CreatedDate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1) 
                    AND CreatedDate < DATEADD(month, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))", conn);
                stats["MonthTotalWeight"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // All-time total weight
                cmd = new SqlCommand("SELECT ISNULL(SUM(Weight), 0) FROM Inventory", conn);
                stats["AllTimeTotalWeight"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // Average recycler rating
                cmd = new SqlCommand("SELECT ISNULL(AVG(Rating), 0) FROM Recyclers WHERE Rating IS NOT NULL AND IsActive = 1", conn);
                stats["AverageRecyclerRating"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // Pending orders (waiting for recyclers)
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'待接单'", conn);
                stats["PendingOrders"] = (int)cmd.ExecuteScalar();

                // In-progress orders
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'进行中'", conn);
                stats["InProgressOrders"] = (int)cmd.ExecuteScalar();

                // === Last 7 days weight trend ===
                cmd = new SqlCommand(@"
                    SELECT CAST(CreatedDate AS DATE) AS RecycleDate, ISNULL(SUM(Weight), 0) AS TotalWeight
                    FROM Inventory
                    WHERE CreatedDate >= DATEADD(day, -7, GETDATE())
                    GROUP BY CAST(CreatedDate AS DATE)
                    ORDER BY RecycleDate", conn);

                var weeklyTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        weeklyTrend.Add(new Dictionary<string, object>
                        {
                            ["Date"] = reader.GetDateTime(0).ToString("MM-dd"),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(1))
                        });
                    }
                }
                stats["WeeklyTrend"] = weeklyTrend;

                // === Category distribution (all time) ===
                cmd = new SqlCommand(@"
                    SELECT CategoryName, ISNULL(SUM(Weight), 0) AS TotalWeight
                    FROM Inventory
                    GROUP BY CategoryName
                    ORDER BY TotalWeight DESC", conn);

                var categoryDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categoryDistribution.Add(new Dictionary<string, object>
                        {
                            ["CategoryName"] = reader.GetString(0),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(1))
                        });
                    }
                }
                stats["CategoryDistribution"] = categoryDistribution;
            }

            return stats;
        }

        /// <summary>
        /// Get all recyclers for export (without pagination)
        /// </summary>
        public List<Recyclers> GetAllRecyclersForExport(string searchTerm = null, bool? isActive = null)
        {
            var recyclers = new List<Recyclers>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR Region LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string query = $@"
                    SELECT RecyclerID, Username, FullName, PhoneNumber, Region, 
                           Rating, Available, IsActive, RegistrationDate 
                    FROM Recyclers 
                    {whereClause}
                    ORDER BY RegistrationDate DESC";

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
                        recyclers.Add(new Recyclers
                        {
                            RecyclerID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Region = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Rating = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                            Available = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                            IsActive = reader.IsDBNull(7) ? false : reader.GetBoolean(7)
                        });
                    }
                }
            }

            return recyclers;
        }

        #endregion

        #region Order Management

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        public PagedResult<Dictionary<string, object>> GetAllOrders(int page = 1, int pageSize = 20, string status = null, string searchTerm = null)
        {
            var result = new PagedResult<Dictionary<string, object>>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<Dictionary<string, object>>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(status))
                {
                    whereClause += " AND a.Status = @Status";
                }
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (u.Username LIKE @SearchTerm OR r.FullName LIKE @SearchTerm OR r.Username LIKE @SearchTerm OR a.ContactName LIKE @SearchTerm)";
                }

                // Get total count
                string countSql = @"SELECT COUNT(*) FROM Appointments a
                    LEFT JOIN Users u ON a.UserID = u.UserID
                    LEFT JOIN Recyclers r ON a.RecyclerID = r.RecyclerID " + whereClause;

                SqlCommand countCmd = new SqlCommand(countSql, conn);
                if (!string.IsNullOrEmpty(status))
                {
                    countCmd.Parameters.AddWithValue("@Status", status);
                }
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countCmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                result.TotalCount = (int)countCmd.ExecuteScalar();

                // Get paged data
                string sql = @"SELECT 
                    a.AppointmentID,
                    a.AppointmentType,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.EstimatedWeight,
                    a.Status,
                    a.CreatedDate,
                    a.Address,
                    a.ContactName,
                    a.ContactPhone,
                    a.EstimatedPrice,
                    a.IsUrgent,
                    u.UserID,
                    u.Username AS UserUsername,
                    u.Email AS UserEmail,
                    u.PhoneNumber AS UserPhone,
                    r.RecyclerID,
                    r.Username AS RecyclerUsername,
                    r.FullName AS RecyclerFullName,
                    r.PhoneNumber AS RecyclerPhone
                    FROM Appointments a
                    LEFT JOIN Users u ON a.UserID = u.UserID
                    LEFT JOIN Recyclers r ON a.RecyclerID = r.RecyclerID " + whereClause +
                    " ORDER BY a.CreatedDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(status))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                }
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new Dictionary<string, object>
                        {
                            ["AppointmentID"] = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            ["AppointmentType"] = reader.GetString(reader.GetOrdinal("AppointmentType")),
                            ["AppointmentDate"] = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                            ["TimeSlot"] = reader.GetString(reader.GetOrdinal("TimeSlot")),
                            ["EstimatedWeight"] = reader.GetDecimal(reader.GetOrdinal("EstimatedWeight")),
                            ["Status"] = reader.GetString(reader.GetOrdinal("Status")),
                            ["CreatedDate"] = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            ["Address"] = reader.GetString(reader.GetOrdinal("Address")),
                            ["ContactName"] = reader.GetString(reader.GetOrdinal("ContactName")),
                            ["ContactPhone"] = reader.GetString(reader.GetOrdinal("ContactPhone")),
                            ["EstimatedPrice"] = reader.IsDBNull(reader.GetOrdinal("EstimatedPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("EstimatedPrice")),
                            ["IsUrgent"] = reader.GetBoolean(reader.GetOrdinal("IsUrgent")),
                            ["UserID"] = reader.GetInt32(reader.GetOrdinal("UserID")),
                            ["UserUsername"] = reader.GetString(reader.GetOrdinal("UserUsername")),
                            ["UserEmail"] = reader.GetString(reader.GetOrdinal("UserEmail")),
                            ["UserPhone"] = reader.GetString(reader.GetOrdinal("UserPhone")),
                            ["RecyclerID"] = reader.IsDBNull(reader.GetOrdinal("RecyclerID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RecyclerID")),
                            ["RecyclerUsername"] = reader.IsDBNull(reader.GetOrdinal("RecyclerUsername")) ? null : reader.GetString(reader.GetOrdinal("RecyclerUsername")),
                            ["RecyclerFullName"] = reader.IsDBNull(reader.GetOrdinal("RecyclerFullName")) ? null : reader.GetString(reader.GetOrdinal("RecyclerFullName")),
                            ["RecyclerPhone"] = reader.IsDBNull(reader.GetOrdinal("RecyclerPhone")) ? null : reader.GetString(reader.GetOrdinal("RecyclerPhone"))
                        };
                        result.Items.Add(order);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get order statistics
        /// </summary>
        public Dictionary<string, object> GetOrderStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total orders
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments", conn);
                stats["TotalOrders"] = (int)cmd.ExecuteScalar();

                // Completed orders
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'已完成'", conn);
                stats["CompletedOrders"] = (int)cmd.ExecuteScalar();

                // Pending orders
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'待接单'", conn);
                stats["PendingOrders"] = (int)cmd.ExecuteScalar();

                // In progress orders
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'进行中'", conn);
                stats["InProgressOrders"] = (int)cmd.ExecuteScalar();

                // Orders this month
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Appointments 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["OrdersThisMonth"] = (int)cmd.ExecuteScalar();

                // Total weight collected
                cmd = new SqlCommand("SELECT ISNULL(SUM(EstimatedWeight), 0) FROM Appointments WHERE Status = N'已完成'", conn);
                stats["TotalWeightCollected"] = cmd.ExecuteScalar();

                // Status distribution
                cmd = new SqlCommand(@"SELECT Status, COUNT(*) AS Count 
                    FROM Appointments 
                    GROUP BY Status 
                    ORDER BY Count DESC", conn);

                var statusDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statusDistribution.Add(new Dictionary<string, object>
                        {
                            ["Status"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["StatusDistribution"] = statusDistribution;
            }

            return stats;
        }

        #endregion

        #region Admin Management

        /// <summary>
        /// Get all admins with pagination
        /// </summary>
        public PagedResult<Admins> GetAllAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            var result = new PagedResult<Admins>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<Admins>()
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
                string countSql = "SELECT COUNT(*) FROM Admins " + whereClause;
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
                string sql = "SELECT * FROM Admins " + whereClause + 
                    " ORDER BY AdminID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                        result.Items.Add(MapAdminFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get admin by ID
        /// </summary>
        public Admins GetAdminById(int adminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Admins WHERE AdminID = @AdminID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AdminID", adminId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapAdminFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add new admin
        /// </summary>
        public bool AddAdmin(Admins admin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Admins (Username, PasswordHash, FullName, Character, IsActive, CreatedDate) 
                    VALUES (@Username, @PasswordHash, @FullName, @Character, @IsActive, GETDATE())";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", admin.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", admin.PasswordHash);
                cmd.Parameters.AddWithValue("@FullName", admin.FullName);
                cmd.Parameters.AddWithValue("@Character", (object)admin.Character ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", admin.IsActive ?? true);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Update admin information
        /// </summary>
        public bool UpdateAdmin(Admins admin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE Admins SET 
                    Username = @Username,
                    FullName = @FullName,
                    Character = @Character,
                    IsActive = @IsActive
                    WHERE AdminID = @AdminID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AdminID", admin.AdminID);
                cmd.Parameters.AddWithValue("@Username", admin.Username);
                cmd.Parameters.AddWithValue("@FullName", admin.FullName);
                cmd.Parameters.AddWithValue("@Character", (object)admin.Character ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", admin.IsActive ?? true);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Delete admin (hard delete from database)
        /// </summary>
        public bool DeleteAdmin(int adminId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                try
                {
                    string sql = "DELETE FROM Admins WHERE AdminID = @AdminID";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@AdminID", adminId);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    // If foreign key constraint violation (error 547), provide helpful message
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该管理员，因为存在关联的记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get admin statistics
        /// </summary>
        public Dictionary<string, object> GetAdminStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Total admins
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Admins", conn);
                stats["TotalAdmins"] = (int)cmd.ExecuteScalar();

                // Active admins
                cmd = new SqlCommand("SELECT COUNT(*) FROM Admins WHERE IsActive = 1", conn);
                stats["ActiveAdmins"] = (int)cmd.ExecuteScalar();

                // Admins created this month
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Admins 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["NewAdminsThisMonth"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// <summary>
        /// Get all admins for export (without pagination)
        /// </summary>
        public List<Admins> GetAllAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            var admins = new List<Admins>();

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
                    SELECT AdminID, Username, FullName, Character, CreatedDate, LastLoginDate, IsActive 
                    FROM Admins 
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
                        admins.Add(new Admins
                        {
                            AdminID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            Character = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            LastLoginDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            IsActive = reader.IsDBNull(6) ? (bool?)null : reader.GetBoolean(6)
                        });
                    }
                }
            }

            return admins;
        }

        #endregion

        #region Dashboard Statistics

        /// <summary>
        /// Get comprehensive dashboard statistics for super admin
        /// </summary>
        public Dictionary<string, object> GetDashboardStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // === User Statistics ===
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn);
                stats["TotalUsers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Users 
                    WHERE YEAR(RegistrationDate) = YEAR(GETDATE()) 
                    AND MONTH(RegistrationDate) = MONTH(GETDATE())", conn);
                stats["NewUsersThisMonth"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Users 
                    WHERE LastLoginDate >= DATEADD(day, -7, GETDATE())", conn);
                stats["ActiveUsersThisWeek"] = (int)cmd.ExecuteScalar();

                // === Recycler Statistics ===
                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers", conn);
                stats["TotalRecyclers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE IsActive = 1", conn);
                stats["ActiveRecyclers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Recyclers WHERE Available = 1 AND IsActive = 1", conn);
                stats["AvailableRecyclers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT ISNULL(AVG(Rating), 0) FROM Recyclers WHERE Rating IS NOT NULL", conn);
                stats["AverageRecyclerRating"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // === Admin Statistics ===
                cmd = new SqlCommand("SELECT COUNT(*) FROM Admins", conn);
                stats["TotalAdmins"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Admins WHERE IsActive = 1", conn);
                stats["ActiveAdmins"] = (int)cmd.ExecuteScalar();

                // === Order Statistics ===
                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments", conn);
                stats["TotalOrders"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'已预约'", conn);
                stats["PendingOrders"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'进行中'", conn);
                stats["InProgressOrders"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'已完成'", conn);
                stats["CompletedOrders"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Appointments WHERE Status = N'已取消'", conn);
                stats["CancelledOrders"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Appointments 
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND MONTH(CreatedDate) = MONTH(GETDATE())", conn);
                stats["OrdersThisMonth"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Appointments 
                    WHERE CreatedDate >= DATEADD(day, -7, GETDATE())", conn);
                stats["OrdersThisWeek"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM Appointments 
                    WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE)", conn);
                stats["OrdersToday"] = (int)cmd.ExecuteScalar();

                // === Weight and Revenue Statistics ===
                cmd = new SqlCommand("SELECT ISNULL(SUM(EstimatedWeight), 0) FROM Appointments WHERE Status = N'已完成'", conn);
                stats["TotalWeightCollected"] = Convert.ToDecimal(cmd.ExecuteScalar());

                cmd = new SqlCommand("SELECT ISNULL(SUM(EstimatedPrice), 0) FROM Appointments WHERE Status = N'已完成'", conn);
                stats["TotalRevenue"] = Convert.ToDecimal(cmd.ExecuteScalar());

                cmd = new SqlCommand(@"SELECT ISNULL(SUM(EstimatedWeight), 0) FROM Appointments 
                    WHERE Status = N'已完成' 
                    AND YEAR(UpdatedDate) = YEAR(GETDATE()) 
                    AND MONTH(UpdatedDate) = MONTH(GETDATE())", conn);
                stats["WeightThisMonth"] = Convert.ToDecimal(cmd.ExecuteScalar());

                cmd = new SqlCommand(@"SELECT ISNULL(SUM(EstimatedPrice), 0) FROM Appointments 
                    WHERE Status = N'已完成' 
                    AND YEAR(UpdatedDate) = YEAR(GETDATE()) 
                    AND MONTH(UpdatedDate) = MONTH(GETDATE())", conn);
                stats["RevenueThisMonth"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // === Order Trend (Last 7 days) ===
                cmd = new SqlCommand(@"
                    SELECT CAST(CreatedDate AS DATE) AS OrderDate, COUNT(*) AS OrderCount
                    FROM Appointments
                    WHERE CreatedDate >= DATEADD(day, -7, GETDATE())
                    GROUP BY CAST(CreatedDate AS DATE)
                    ORDER BY OrderDate", conn);

                var orderTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orderTrend.Add(new Dictionary<string, object>
                        {
                            ["Date"] = reader.GetDateTime(0).ToString("MM-dd"),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["OrderTrend"] = orderTrend;

                // === User Registration Trend (Last 30 days) ===
                cmd = new SqlCommand(@"
                    SELECT CAST(RegistrationDate AS DATE) AS RegDate, COUNT(*) AS UserCount
                    FROM Users
                    WHERE RegistrationDate >= DATEADD(day, -30, GETDATE())
                    GROUP BY CAST(RegistrationDate AS DATE)
                    ORDER BY RegDate", conn);

                var userTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userTrend.Add(new Dictionary<string, object>
                        {
                            ["Date"] = reader.GetDateTime(0).ToString("MM-dd"),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["UserRegistrationTrend"] = userTrend;

                // === Category Distribution ===
                cmd = new SqlCommand(@"
                    SELECT CategoryName, COUNT(*) AS Count
                    FROM AppointmentCategories
                    GROUP BY CategoryName
                    ORDER BY Count DESC", conn);

                var categoryDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categoryDistribution.Add(new Dictionary<string, object>
                        {
                            ["Category"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["CategoryDistribution"] = categoryDistribution;

                // === Order Status Distribution ===
                cmd = new SqlCommand(@"
                    SELECT Status, COUNT(*) AS Count 
                    FROM Appointments 
                    GROUP BY Status 
                    ORDER BY Count DESC", conn);

                var statusDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statusDistribution.Add(new Dictionary<string, object>
                        {
                            ["Status"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["OrderStatusDistribution"] = statusDistribution;

                // === Region Distribution ===
                cmd = new SqlCommand(@"
                    SELECT Region, COUNT(*) AS RecyclerCount
                    FROM Recyclers
                    WHERE IsActive = 1
                    GROUP BY Region
                    ORDER BY RecyclerCount DESC", conn);

                var regionDistribution = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        regionDistribution.Add(new Dictionary<string, object>
                        {
                            ["Region"] = reader.GetString(0),
                            ["Count"] = reader.GetInt32(1)
                        });
                    }
                }
                stats["RegionDistribution"] = regionDistribution;

                // === Top Recyclers by Completed Orders ===
                cmd = new SqlCommand(@"
                    SELECT TOP 5 r.RecyclerID, ISNULL(r.FullName, r.Username) AS Name, 
                           r.Rating, COUNT(a.AppointmentID) AS CompletedOrders
                    FROM Recyclers r
                    LEFT JOIN Appointments a ON r.RecyclerID = a.RecyclerID AND a.Status = N'已完成'
                    WHERE r.IsActive = 1
                    GROUP BY r.RecyclerID, r.FullName, r.Username, r.Rating
                    ORDER BY CompletedOrders DESC", conn);

                var topRecyclers = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topRecyclers.Add(new Dictionary<string, object>
                        {
                            ["RecyclerID"] = reader.GetInt32(0),
                            ["Name"] = reader.GetString(1),
                            ["Rating"] = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                            ["CompletedOrders"] = reader.GetInt32(3)
                        });
                    }
                }
                stats["TopRecyclers"] = topRecyclers;

                // === Feedback Statistics ===
                cmd = new SqlCommand("SELECT COUNT(*) FROM UserFeedback", conn);
                stats["TotalFeedbacks"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM UserFeedback WHERE Status = N'反馈中'", conn);
                stats["PendingFeedbacks"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM UserFeedback WHERE Status = N'已回复'", conn);
                stats["ProcessedFeedbacks"] = (int)cmd.ExecuteScalar();

                // === Review Statistics ===
                cmd = new SqlCommand("SELECT COUNT(*) FROM OrderReviews", conn);
                stats["TotalReviews"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT ISNULL(AVG(CAST(StarRating AS DECIMAL(3,2))), 0) FROM OrderReviews", conn);
                stats["AverageReviewRating"] = Convert.ToDecimal(cmd.ExecuteScalar());

                // === Monthly Order Trend (Last 6 months) ===
                cmd = new SqlCommand(@"
                    SELECT YEAR(CreatedDate) AS OrderYear, MONTH(CreatedDate) AS OrderMonth, COUNT(*) AS OrderCount
                    FROM Appointments
                    WHERE CreatedDate >= DATEADD(month, -6, GETDATE())
                    GROUP BY YEAR(CreatedDate), MONTH(CreatedDate)
                    ORDER BY OrderYear, OrderMonth", conn);

                var monthlyOrderTrend = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int year = reader.GetInt32(0);
                        int month = reader.GetInt32(1);
                        monthlyOrderTrend.Add(new Dictionary<string, object>
                        {
                            ["Month"] = $"{year}-{month:D2}",
                            ["Count"] = reader.GetInt32(2)
                        });
                    }
                }
                stats["MonthlyOrderTrend"] = monthlyOrderTrend;

                // === Inventory Statistics ===
                cmd = new SqlCommand(@"
                    SELECT CategoryKey, CategoryName, ISNULL(SUM(Weight), 0) AS TotalWeight
                    FROM Inventory
                    GROUP BY CategoryKey, CategoryName
                    ORDER BY TotalWeight DESC", conn);

                var inventoryStats = new List<Dictionary<string, object>>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventoryStats.Add(new Dictionary<string, object>
                        {
                            ["CategoryKey"] = reader.GetString(0),
                            ["CategoryName"] = reader.GetString(1),
                            ["TotalWeight"] = Convert.ToDecimal(reader.GetValue(2))
                        });
                    }
                }
                stats["InventoryStats"] = inventoryStats;
            }

            return stats;
        }

        #endregion

        #region Transporter Management

        /// <summary>
        /// Get all transporters with pagination
        /// </summary>
        public PagedResult<Transporters> GetAllTransporters(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            var result = new PagedResult<Transporters>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<Transporters>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build WHERE clause
                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR Region LIKE @SearchTerm OR VehiclePlateNumber LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                // Get total count
                string countSql = "SELECT COUNT(*) FROM Transporters " + whereClause;
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
                string sql = "SELECT * FROM Transporters " + whereClause + 
                    " ORDER BY TransporterID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                        result.Items.Add(MapTransporterFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get transporter by ID
        /// </summary>
        public Transporters GetTransporterById(int transporterId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Transporters WHERE TransporterID = @TransporterID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@TransporterID", transporterId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTransporterFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add new transporter
        /// </summary>
        public bool AddTransporter(Transporters transporter)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Transporters (Username, PasswordHash, FullName, PhoneNumber, IDNumber, VehicleType, VehiclePlateNumber, VehicleCapacity, LicenseNumber, Region, Available, CurrentStatus, IsActive, CreatedDate, Rating) 
                    VALUES (@Username, @PasswordHash, @FullName, @PhoneNumber, @IDNumber, @VehicleType, @VehiclePlateNumber, @VehicleCapacity, @LicenseNumber, @Region, @Available, @CurrentStatus, @IsActive, GETDATE(), 0)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", transporter.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", transporter.PasswordHash);
                cmd.Parameters.AddWithValue("@FullName", transporter.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", transporter.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", transporter.IDNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@VehicleType", transporter.VehicleType);
                cmd.Parameters.AddWithValue("@VehiclePlateNumber", transporter.VehiclePlateNumber);
                cmd.Parameters.AddWithValue("@VehicleCapacity", transporter.VehicleCapacity ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LicenseNumber", transporter.LicenseNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", transporter.Region);
                cmd.Parameters.AddWithValue("@Available", transporter.Available);
                cmd.Parameters.AddWithValue("@CurrentStatus", transporter.CurrentStatus ?? "空闲");
                cmd.Parameters.AddWithValue("@IsActive", transporter.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Update transporter information
        /// </summary>
        public bool UpdateTransporter(Transporters transporter)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE Transporters SET 
                    Username = @Username,
                    FullName = @FullName,
                    PhoneNumber = @PhoneNumber,
                    IDNumber = @IDNumber,
                    VehicleType = @VehicleType,
                    VehiclePlateNumber = @VehiclePlateNumber,
                    VehicleCapacity = @VehicleCapacity,
                    LicenseNumber = @LicenseNumber,
                    Region = @Region,
                    Available = @Available,
                    CurrentStatus = @CurrentStatus,
                    IsActive = @IsActive
                    WHERE TransporterID = @TransporterID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@TransporterID", transporter.TransporterID);
                cmd.Parameters.AddWithValue("@Username", transporter.Username);
                cmd.Parameters.AddWithValue("@FullName", transporter.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", transporter.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", transporter.IDNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@VehicleType", transporter.VehicleType);
                cmd.Parameters.AddWithValue("@VehiclePlateNumber", transporter.VehiclePlateNumber);
                cmd.Parameters.AddWithValue("@VehicleCapacity", transporter.VehicleCapacity ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LicenseNumber", transporter.LicenseNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", transporter.Region);
                cmd.Parameters.AddWithValue("@Available", transporter.Available);
                cmd.Parameters.AddWithValue("@CurrentStatus", transporter.CurrentStatus ?? "空闲");
                cmd.Parameters.AddWithValue("@IsActive", transporter.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Delete transporter (hard delete from database)
        /// </summary>
        public bool DeleteTransporter(int transporterId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                try
                {
                    string sql = "DELETE FROM Transporters WHERE TransporterID = @TransporterID";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@TransporterID", transporterId);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该运输人员，因为存在关联的记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get transporter statistics
        /// </summary>
        public Dictionary<string, object> GetTransporterStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Transporters", conn);
                stats["TotalTransporters"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Transporters WHERE IsActive = 1", conn);
                stats["ActiveTransporters"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Transporters WHERE Available = 1 AND IsActive = 1", conn);
                stats["AvailableTransporters"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// <summary>
        /// Get all transporters for export (without pagination)
        /// </summary>
        public List<Transporters> GetAllTransportersForExport(string searchTerm = null, bool? isActive = null)
        {
            var transporters = new List<Transporters>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR Region LIKE @SearchTerm OR VehiclePlateNumber LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string query = $@"
                    SELECT * FROM Transporters 
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
                        transporters.Add(MapTransporterFromReader(reader));
                    }
                }
            }

            return transporters;
        }

        #endregion

        #region SortingCenterWorker Management

        /// <summary>
        /// Get all sorting center workers with pagination
        /// </summary>
        public PagedResult<SortingCenterWorkers> GetAllSortingCenterWorkers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            var result = new PagedResult<SortingCenterWorkers>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<SortingCenterWorkers>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR SortingCenterName LIKE @SearchTerm OR Position LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string countSql = "SELECT COUNT(*) FROM SortingCenterWorkers " + whereClause;
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

                string sql = "SELECT * FROM SortingCenterWorkers " + whereClause + 
                    " ORDER BY WorkerID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                        result.Items.Add(MapSortingCenterWorkerFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get sorting center worker by ID
        /// </summary>
        public SortingCenterWorkers GetSortingCenterWorkerById(int workerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM SortingCenterWorkers WHERE WorkerID = @WorkerID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WorkerID", workerId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapSortingCenterWorkerFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add new sorting center worker
        /// </summary>
        public bool AddSortingCenterWorker(SortingCenterWorkers worker)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO SortingCenterWorkers (Username, PasswordHash, FullName, PhoneNumber, IDNumber, SortingCenterID, SortingCenterName, Position, WorkStation, Specialization, ShiftType, Available, CurrentStatus, IsActive, CreatedDate, Rating) 
                    VALUES (@Username, @PasswordHash, @FullName, @PhoneNumber, @IDNumber, @SortingCenterID, @SortingCenterName, @Position, @WorkStation, @Specialization, @ShiftType, @Available, @CurrentStatus, @IsActive, GETDATE(), 0)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", worker.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", worker.PasswordHash);
                cmd.Parameters.AddWithValue("@FullName", worker.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", worker.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", worker.IDNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SortingCenterID", worker.SortingCenterID);
                cmd.Parameters.AddWithValue("@SortingCenterName", worker.SortingCenterName);
                cmd.Parameters.AddWithValue("@Position", worker.Position);
                cmd.Parameters.AddWithValue("@WorkStation", worker.WorkStation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", worker.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ShiftType", worker.ShiftType);
                cmd.Parameters.AddWithValue("@Available", worker.Available);
                cmd.Parameters.AddWithValue("@CurrentStatus", worker.CurrentStatus ?? "空闲");
                cmd.Parameters.AddWithValue("@IsActive", worker.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Update sorting center worker information
        /// </summary>
        public bool UpdateSortingCenterWorker(SortingCenterWorkers worker)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE SortingCenterWorkers SET 
                    Username = @Username,
                    FullName = @FullName,
                    PhoneNumber = @PhoneNumber,
                    IDNumber = @IDNumber,
                    SortingCenterID = @SortingCenterID,
                    SortingCenterName = @SortingCenterName,
                    Position = @Position,
                    WorkStation = @WorkStation,
                    Specialization = @Specialization,
                    ShiftType = @ShiftType,
                    Available = @Available,
                    CurrentStatus = @CurrentStatus,
                    IsActive = @IsActive
                    WHERE WorkerID = @WorkerID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WorkerID", worker.WorkerID);
                cmd.Parameters.AddWithValue("@Username", worker.Username);
                cmd.Parameters.AddWithValue("@FullName", worker.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", worker.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", worker.IDNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SortingCenterID", worker.SortingCenterID);
                cmd.Parameters.AddWithValue("@SortingCenterName", worker.SortingCenterName);
                cmd.Parameters.AddWithValue("@Position", worker.Position);
                cmd.Parameters.AddWithValue("@WorkStation", worker.WorkStation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", worker.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ShiftType", worker.ShiftType);
                cmd.Parameters.AddWithValue("@Available", worker.Available);
                cmd.Parameters.AddWithValue("@CurrentStatus", worker.CurrentStatus ?? "空闲");
                cmd.Parameters.AddWithValue("@IsActive", worker.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Delete sorting center worker (hard delete from database)
        /// </summary>
        public bool DeleteSortingCenterWorker(int workerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                try
                {
                    string sql = "DELETE FROM SortingCenterWorkers WHERE WorkerID = @WorkerID";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@WorkerID", workerId);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "无法删除该基地人员，因为存在关联的记录。请先处理相关数据或改用禁用功能。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get sorting center worker statistics
        /// </summary>
        public Dictionary<string, object> GetSortingCenterWorkerStatistics()
        {
            var stats = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SortingCenterWorkers", conn);
                stats["TotalWorkers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM SortingCenterWorkers WHERE IsActive = 1", conn);
                stats["ActiveWorkers"] = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM SortingCenterWorkers WHERE Available = 1 AND IsActive = 1", conn);
                stats["AvailableWorkers"] = (int)cmd.ExecuteScalar();
            }

            return stats;
        }

        /// <summary>
        /// Get all sorting center workers for export (without pagination)
        /// </summary>
        public List<SortingCenterWorkers> GetAllSortingCenterWorkersForExport(string searchTerm = null, bool? isActive = null)
        {
            var workers = new List<SortingCenterWorkers>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string whereClause = "WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (Username LIKE @SearchTerm OR FullName LIKE @SearchTerm OR PhoneNumber LIKE @SearchTerm OR SortingCenterName LIKE @SearchTerm OR Position LIKE @SearchTerm)";
                }
                if (isActive.HasValue)
                {
                    whereClause += " AND IsActive = @IsActive";
                }

                string query = $@"
                    SELECT * FROM SortingCenterWorkers 
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
                        workers.Add(MapSortingCenterWorkerFromReader(reader));
                    }
                }
            }

            return workers;
        }

        #endregion

        #region Helper Methods

        private Recyclers MapRecyclerFromReader(SqlDataReader reader)
        {
            return new Recyclers
            {
                RecyclerID = reader.GetInt32(reader.GetOrdinal("RecyclerID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                Region = reader.GetString(reader.GetOrdinal("Region")),
                Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Rating")),
                CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                AvatarURL = reader.IsDBNull(reader.GetOrdinal("AvatarURL")) ? null : reader.GetString(reader.GetOrdinal("AvatarURL"))
            };
        }

        private Admins MapAdminFromReader(SqlDataReader reader)
        {
            return new Admins
            {
                AdminID = reader.GetInt32(reader.GetOrdinal("AdminID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Character = reader.IsDBNull(reader.GetOrdinal("Character")) ? null : reader.GetString(reader.GetOrdinal("Character")),
                CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        private Transporters MapTransporterFromReader(SqlDataReader reader)
        {
            return new Transporters
            {
                TransporterID = reader.GetInt32(reader.GetOrdinal("TransporterID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                IDNumber = reader.IsDBNull(reader.GetOrdinal("IDNumber")) ? null : reader.GetString(reader.GetOrdinal("IDNumber")),
                VehicleType = reader.GetString(reader.GetOrdinal("VehicleType")),
                VehiclePlateNumber = reader.GetString(reader.GetOrdinal("VehiclePlateNumber")),
                VehicleCapacity = reader.IsDBNull(reader.GetOrdinal("VehicleCapacity")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("VehicleCapacity")),
                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LicenseNumber")) ? null : reader.GetString(reader.GetOrdinal("LicenseNumber")),
                Region = reader.GetString(reader.GetOrdinal("Region")),
                Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                CurrentStatus = reader.GetString(reader.GetOrdinal("CurrentStatus")),
                TotalTrips = reader.GetInt32(reader.GetOrdinal("TotalTrips")),
                TotalWeight = reader.GetDecimal(reader.GetOrdinal("TotalWeight")),
                Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Rating")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                AvatarURL = reader.IsDBNull(reader.GetOrdinal("AvatarURL")) ? null : reader.GetString(reader.GetOrdinal("AvatarURL")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
            };
        }

        private SortingCenterWorkers MapSortingCenterWorkerFromReader(SqlDataReader reader)
        {
            return new SortingCenterWorkers
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                IDNumber = reader.IsDBNull(reader.GetOrdinal("IDNumber")) ? null : reader.GetString(reader.GetOrdinal("IDNumber")),
                SortingCenterID = reader.GetInt32(reader.GetOrdinal("SortingCenterID")),
                SortingCenterName = reader.GetString(reader.GetOrdinal("SortingCenterName")),
                Position = reader.GetString(reader.GetOrdinal("Position")),
                WorkStation = reader.IsDBNull(reader.GetOrdinal("WorkStation")) ? null : reader.GetString(reader.GetOrdinal("WorkStation")),
                Specialization = reader.IsDBNull(reader.GetOrdinal("Specialization")) ? null : reader.GetString(reader.GetOrdinal("Specialization")),
                ShiftType = reader.GetString(reader.GetOrdinal("ShiftType")),
                Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                CurrentStatus = reader.GetString(reader.GetOrdinal("CurrentStatus")),
                TotalItemsProcessed = reader.GetInt32(reader.GetOrdinal("TotalItemsProcessed")),
                TotalWeightProcessed = reader.GetDecimal(reader.GetOrdinal("TotalWeightProcessed")),
                AccuracyRate = reader.IsDBNull(reader.GetOrdinal("AccuracyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AccuracyRate")),
                Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Rating")),
                HireDate = reader.IsDBNull(reader.GetOrdinal("HireDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("HireDate")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                AvatarURL = reader.IsDBNull(reader.GetOrdinal("AvatarURL")) ? null : reader.GetString(reader.GetOrdinal("AvatarURL")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
            };
        }

        #endregion
    }
}
