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
                    string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, Region, LastLoginDate, IsActive, Available 
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
                                Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,
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
                    string sql = @"SELECT AdminID, Username, PasswordHash, FullName, Character, IsActive, CreatedDate, LastLoginDate 
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
                                FullName = reader["FullName"].ToString(),
                                Character = reader["Character"] != DBNull.Value 
                                    ? reader["Character"].ToString() 
                                    : null,
                                IsActive = reader["IsActive"] != DBNull.Value 
                                    ? Convert.ToBoolean(reader["IsActive"]) 
                                    : (bool?)null,
                                CreatedDate = reader["CreatedDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["CreatedDate"])
                                    : (DateTime?)null,
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

        #region 运输人员登录验证（对应Transporters表）
        /// <summary>
        /// 根据用户名查询运输人员信息
        /// </summary>
        public Transporters GetTransporterByUsername(string username)
        {
            Transporters transporter = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT TransporterID, Username, PasswordHash, FullName, PhoneNumber, 
                                          VehicleType, VehiclePlateNumber, Region, Available, CurrentStatus,
                                          LastLoginDate, IsActive 
                                  FROM Transporters 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            transporter = new Transporters
                            {
                                TransporterID = Convert.ToInt32(reader["TransporterID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                FullName = reader["FullName"]?.ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                VehicleType = reader["VehicleType"] != DBNull.Value ? reader["VehicleType"].ToString() : null,
                                VehiclePlateNumber = reader["VehiclePlateNumber"].ToString(),
                                Region = reader["Region"].ToString(),
                                Available = Convert.ToBoolean(reader["Available"]),
                                CurrentStatus = reader["CurrentStatus"].ToString(),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询运输人员失败：" + ex.Message);
                }
            }
            return transporter;
        }

        /// <summary>
        /// 更新运输人员最后登录时间
        /// </summary>
        public void UpdateTransporterLastLogin(int transporterId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Transporters 
                               SET LastLoginDate = @LastLoginDate 
                               WHERE TransporterID = @TransporterID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@TransporterID", transporterId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("更新运输人员登录时间失败：" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 通过ID获取运输人员信息
        /// </summary>
        public Transporters GetTransporterById(int transporterId)
        {
            Transporters transporter = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT TransporterID, Username, PasswordHash, FullName, PhoneNumber, 
                                          IDNumber, VehicleType, VehiclePlateNumber, VehicleCapacity,
                                          LicenseNumber, Region, Available, CurrentStatus, TotalTrips,
                                          TotalWeight, Rating, CreatedDate, LastLoginDate, IsActive 
                                  FROM Transporters 
                                  WHERE TransporterID = @TransporterID";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@TransporterID", transporterId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            transporter = new Transporters
                            {
                                TransporterID = Convert.ToInt32(reader["TransporterID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : null,
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                IDNumber = reader["IDNumber"] != DBNull.Value ? reader["IDNumber"].ToString() : null,
                                VehicleType = reader["VehicleType"] != DBNull.Value ? reader["VehicleType"].ToString() : null,
                                VehiclePlateNumber = reader["VehiclePlateNumber"].ToString(),
                                VehicleCapacity = reader["VehicleCapacity"] != DBNull.Value 
                                    ? Convert.ToDecimal(reader["VehicleCapacity"]) 
                                    : (decimal?)null,
                                LicenseNumber = reader["LicenseNumber"] != DBNull.Value ? reader["LicenseNumber"].ToString() : null,
                                Region = reader["Region"].ToString(),
                                Available = Convert.ToBoolean(reader["Available"]),
                                CurrentStatus = reader["CurrentStatus"].ToString(),
                                TotalTrips = Convert.ToInt32(reader["TotalTrips"]),
                                TotalWeight = Convert.ToDecimal(reader["TotalWeight"]),
                                Rating = reader["Rating"] != DBNull.Value 
                                    ? Convert.ToDecimal(reader["Rating"]) 
                                    : (decimal?)null,
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询运输人员失败：" + ex.Message);
                }
            }
            return transporter;
        }

        /// <summary>
        /// 更新运输人员信息
        /// </summary>
        public bool UpdateTransporter(Transporters transporter)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Transporters 
                               SET FullName = @FullName,
                                   PhoneNumber = @PhoneNumber,
                                   IDNumber = @IDNumber,
                                   VehicleType = NULL,
                                   VehiclePlateNumber = @VehiclePlateNumber,
                                   VehicleCapacity = NULL,
                                   LicenseNumber = @LicenseNumber,
                                   Region = @Region,
                                   PasswordHash = @PasswordHash
                               WHERE TransporterID = @TransporterID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FullName", (object)transporter.FullName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", transporter.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", (object)transporter.IDNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VehiclePlateNumber", transporter.VehiclePlateNumber);
                cmd.Parameters.AddWithValue("@LicenseNumber", (object)transporter.LicenseNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", transporter.Region);
                cmd.Parameters.AddWithValue("@PasswordHash", transporter.PasswordHash);
                cmd.Parameters.AddWithValue("@TransporterID", transporter.TransporterID);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("更新运输人员信息失败：" + ex.Message);
                }
            }
        }
        #endregion

        #region 基地工作人员登录验证（对应SortingCenterWorkers表）
        /// <summary>
        /// 根据用户名查询基地工作人员信息
        /// </summary>
        public SortingCenterWorkers GetSortingCenterWorkerByUsername(string username)
        {
            SortingCenterWorkers worker = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string sql = @"SELECT WorkerID, Username, PasswordHash, FullName, PhoneNumber, 
                                          SortingCenterID, SortingCenterName, Position, WorkStation,
                                          ShiftType, Available, CurrentStatus, LastLoginDate, IsActive 
                                  FROM SortingCenterWorkers 
                                  WHERE Username = @Username";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            worker = new SortingCenterWorkers
                            {
                                WorkerID = Convert.ToInt32(reader["WorkerID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                FullName = reader["FullName"]?.ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                SortingCenterID = Convert.ToInt32(reader["SortingCenterID"]),
                                SortingCenterName = reader["SortingCenterName"] != DBNull.Value ? reader["SortingCenterName"].ToString() : null,
                                Position = reader["Position"] != DBNull.Value ? reader["Position"].ToString() : null,
                                WorkStation = reader["WorkStation"]?.ToString(),
                                ShiftType = reader["ShiftType"].ToString(),
                                Available = Convert.ToBoolean(reader["Available"]),
                                CurrentStatus = reader["CurrentStatus"].ToString(),
                                LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LastLoginDate"])
                                    : (DateTime?)null,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("查询基地工作人员失败：" + ex.Message);
                }
            }
            return worker;
        }

        /// <summary>
        /// 更新基地工作人员最后登录时间
        /// </summary>
        public void UpdateSortingCenterWorkerLastLogin(int workerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE SortingCenterWorkers 
                               SET LastLoginDate = @LastLoginDate 
                               WHERE WorkerID = @WorkerID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@WorkerID", workerId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("更新基地工作人员登录时间失败：" + ex.Message);
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
                string sql = @"SELECT RecyclerID, Username, FullName, PhoneNumber, Region, IsActive, Available 
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
                            Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            Available = Convert.ToBoolean(reader["Available"])
                        };
                    }
                }
            }
            return null;
        }

        #region 基地工作人员账号管理

        /// <summary>
        /// 根据ID获取基地工作人员信息
        /// </summary>
        public SortingCenterWorkers GetSortingCenterWorkerById(int workerId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT WorkerID, Username, PasswordHash, FullName, PhoneNumber, IDNumber, 
                              SortingCenterID, SortingCenterName, Position, WorkStation, Specialization, 
                              ShiftType, Available, CurrentStatus, TotalItemsProcessed, TotalWeightProcessed, 
                              AccuracyRate, Rating, HireDate, CreatedDate, LastLoginDate, IsActive, AvatarURL, Notes 
                              FROM SortingCenterWorkers 
                              WHERE WorkerID = @WorkerID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WorkerID", workerId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new SortingCenterWorkers
                        {
                            WorkerID = Convert.ToInt32(reader["WorkerID"]),
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            FullName = reader["FullName"]?.ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            IDNumber = reader["IDNumber"]?.ToString(),
                            SortingCenterID = Convert.ToInt32(reader["SortingCenterID"]),
                            SortingCenterName = reader["SortingCenterName"]?.ToString(),
                            Position = reader["Position"]?.ToString(),
                            WorkStation = reader["WorkStation"]?.ToString(),
                            Specialization = reader["Specialization"]?.ToString(),
                            ShiftType = reader["ShiftType"].ToString(),
                            Available = Convert.ToBoolean(reader["Available"]),
                            CurrentStatus = reader["CurrentStatus"].ToString(),
                            TotalItemsProcessed = Convert.ToInt32(reader["TotalItemsProcessed"]),
                            TotalWeightProcessed = Convert.ToDecimal(reader["TotalWeightProcessed"]),
                            AccuracyRate = reader["AccuracyRate"] != DBNull.Value ? Convert.ToDecimal(reader["AccuracyRate"]) : (decimal?)null,
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToDecimal(reader["Rating"]) : (decimal?)null,
                            HireDate = reader["HireDate"] != DBNull.Value ? Convert.ToDateTime(reader["HireDate"]) : (DateTime?)null,
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : (DateTime?)null,
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            AvatarURL = reader["AvatarURL"]?.ToString(),
                            Notes = reader["Notes"]?.ToString()
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 更新基地工作人员信息
        /// </summary>
        public bool UpdateSortingCenterWorker(SortingCenterWorkers worker)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE SortingCenterWorkers 
                              SET FullName = @FullName, 
                                  PhoneNumber = @PhoneNumber, 
                                  IDNumber = @IDNumber, 
                                  Position = @Position, 
                                  WorkStation = @WorkStation, 
                                  Specialization = @Specialization, 
                                  ShiftType = @ShiftType,
                                  PasswordHash = @PasswordHash
                              WHERE WorkerID = @WorkerID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WorkerID", worker.WorkerID);
                cmd.Parameters.AddWithValue("@FullName", worker.FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", worker.PhoneNumber);
                cmd.Parameters.AddWithValue("@IDNumber", worker.IDNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", worker.Position ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkStation", worker.WorkStation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", worker.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ShiftType", worker.ShiftType);
                cmd.Parameters.AddWithValue("@PasswordHash", worker.PasswordHash);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        #endregion
    }
}

