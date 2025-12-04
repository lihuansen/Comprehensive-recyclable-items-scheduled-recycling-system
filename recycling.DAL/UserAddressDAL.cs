using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class UserAddressDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        public List<UserAddresses> GetUserAddresses(int userId)
        {
            var addresses = new List<UserAddresses>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT AddressID, UserID, Province, City, District, Street, DetailAddress, 
                              ContactName, ContactPhone, IsDefault, CreatedDate, UpdatedDate 
                       FROM UserAddresses 
                       WHERE UserID = @UserID 
                       ORDER BY IsDefault DESC, CreatedDate DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addresses.Add(MapReaderToAddress(reader));
                    }
                }
            }
            return addresses;
        }

        /// <summary>
        /// 根据ID获取地址
        /// </summary>
        public UserAddresses GetAddressById(int addressId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT AddressID, UserID, Province, City, District, Street, DetailAddress, 
                              ContactName, ContactPhone, IsDefault, CreatedDate, UpdatedDate 
                       FROM UserAddresses 
                       WHERE AddressID = @AddressID AND UserID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AddressID", addressId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToAddress(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        public UserAddresses GetDefaultAddress(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT AddressID, UserID, Province, City, District, Street, DetailAddress, 
                              ContactName, ContactPhone, IsDefault, CreatedDate, UpdatedDate 
                       FROM UserAddresses 
                       WHERE UserID = @UserID AND IsDefault = 1";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToAddress(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 添加新地址
        /// </summary>
        public int AddAddress(UserAddresses address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO UserAddresses 
                       (UserID, Province, City, District, Street, DetailAddress, ContactName, ContactPhone, IsDefault, CreatedDate)
                       VALUES (@UserID, @Province, @City, @District, @Street, @DetailAddress, @ContactName, @ContactPhone, @IsDefault, @CreatedDate);
                       SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", address.UserID);
                cmd.Parameters.AddWithValue("@Province", address.Province);
                cmd.Parameters.AddWithValue("@City", address.City);
                cmd.Parameters.AddWithValue("@District", address.District);
                cmd.Parameters.AddWithValue("@Street", address.Street);
                cmd.Parameters.AddWithValue("@DetailAddress", address.DetailAddress);
                cmd.Parameters.AddWithValue("@ContactName", address.ContactName);
                cmd.Parameters.AddWithValue("@ContactPhone", address.ContactPhone);
                cmd.Parameters.AddWithValue("@IsDefault", address.IsDefault);
                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 更新地址
        /// </summary>
        public bool UpdateAddress(UserAddresses address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE UserAddresses 
                       SET Street = @Street, DetailAddress = @DetailAddress, 
                           ContactName = @ContactName, ContactPhone = @ContactPhone, 
                           UpdatedDate = @UpdatedDate
                       WHERE AddressID = @AddressID AND UserID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Street", address.Street);
                cmd.Parameters.AddWithValue("@DetailAddress", address.DetailAddress);
                cmd.Parameters.AddWithValue("@ContactName", address.ContactName);
                cmd.Parameters.AddWithValue("@ContactPhone", address.ContactPhone);
                cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AddressID", address.AddressID);
                cmd.Parameters.AddWithValue("@UserID", address.UserID);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 删除地址
        /// </summary>
        public bool DeleteAddress(int addressId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM UserAddresses WHERE AddressID = @AddressID AND UserID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AddressID", addressId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        public bool SetDefaultAddress(int addressId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 先取消该用户的所有默认地址
                        string clearSql = "UPDATE UserAddresses SET IsDefault = 0 WHERE UserID = @UserID";
                        SqlCommand clearCmd = new SqlCommand(clearSql, conn, transaction);
                        clearCmd.Parameters.AddWithValue("@UserID", userId);
                        clearCmd.ExecuteNonQuery();

                        // 设置指定地址为默认
                        string setSql = "UPDATE UserAddresses SET IsDefault = 1, UpdatedDate = @UpdatedDate WHERE AddressID = @AddressID AND UserID = @UserID";
                        SqlCommand setCmd = new SqlCommand(setSql, conn, transaction);
                        setCmd.Parameters.AddWithValue("@AddressID", addressId);
                        setCmd.Parameters.AddWithValue("@UserID", userId);
                        setCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                        int rowsAffected = setCmd.ExecuteNonQuery();

                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 获取用户地址数量
        /// </summary>
        public int GetAddressCount(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM UserAddresses WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 将DataReader映射到UserAddresses对象
        /// </summary>
        private UserAddresses MapReaderToAddress(SqlDataReader reader)
        {
            return new UserAddresses
            {
                AddressID = Convert.ToInt32(reader["AddressID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                Province = reader["Province"].ToString(),
                City = reader["City"].ToString(),
                District = reader["District"].ToString(),
                Street = reader["Street"].ToString(),
                DetailAddress = reader["DetailAddress"].ToString(),
                ContactName = reader["ContactName"].ToString(),
                ContactPhone = reader["ContactPhone"].ToString(),
                IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedDate"]) : (DateTime?)null
            };
        }
    }
}
