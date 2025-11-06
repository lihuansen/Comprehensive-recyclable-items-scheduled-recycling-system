using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class AdminBLL
    {
        private readonly AdminDAL _adminDAL;

        public AdminBLL()
        {
            _adminDAL = new AdminDAL();
        }

        #region User Management

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public PagedResult<Users> GetAllUsers(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllUsers(page, pageSize, searchTerm);
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        public Dictionary<string, object> GetUserStatistics()
        {
            return _adminDAL.GetUserStatistics();
        }

        #endregion

        #region Recycler Management

        /// <summary>
        /// Get all recyclers with pagination
        /// </summary>
        public PagedResult<Recyclers> GetAllRecyclers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllRecyclers(page, pageSize, searchTerm, isActive);
        }

        /// <summary>
        /// Get recycler by ID
        /// </summary>
        public Recyclers GetRecyclerById(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                throw new ArgumentException("Invalid recycler ID");
            }

            return _adminDAL.GetRecyclerById(recyclerId);
        }

        /// <summary>
        /// Add new recycler
        /// </summary>
        public (bool Success, string Message) AddRecycler(Recyclers recycler, string password)
        {
            // Validation
            if (string.IsNullOrEmpty(recycler.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            if (string.IsNullOrEmpty(recycler.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(recycler.Region))
            {
                return (false, "区域不能为空");
            }

            // Hash password
            recycler.PasswordHash = HashPassword(password);
            recycler.IsActive = true;
            recycler.Available = true;

            bool result = _adminDAL.AddRecycler(recycler);
            return result ? (true, "添加回收员成功") : (false, "添加回收员失败");
        }

        /// <summary>
        /// Update recycler
        /// </summary>
        public (bool Success, string Message) UpdateRecycler(Recyclers recycler)
        {
            // Validation
            if (recycler.RecyclerID <= 0)
            {
                return (false, "Invalid recycler ID");
            }

            if (string.IsNullOrEmpty(recycler.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(recycler.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(recycler.Region))
            {
                return (false, "区域不能为空");
            }

            bool result = _adminDAL.UpdateRecycler(recycler);
            return result ? (true, "更新回收员信息成功") : (false, "更新回收员信息失败");
        }

        /// <summary>
        /// Delete recycler (hard delete from database)
        /// </summary>
        public (bool Success, string Message) DeleteRecycler(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return (false, "Invalid recycler ID");
            }

            try
            {
                bool result = _adminDAL.DeleteRecycler(recyclerId);
                return result ? (true, "删除回收员成功") : (false, "删除回收员失败");
            }
            catch (InvalidOperationException ex)
            {
                // Foreign key constraint violation
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除回收员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get recycler completed orders count
        /// </summary>
        public int GetRecyclerCompletedOrdersCount(int recyclerId)
        {
            if (recyclerId <= 0)
            {
                return 0;
            }

            return _adminDAL.GetRecyclerCompletedOrdersCount(recyclerId);
        }

        /// <summary>
        /// Get recycler statistics
        /// </summary>
        public Dictionary<string, object> GetRecyclerStatistics()
        {
            return _adminDAL.GetRecyclerStatistics();
        }

        #endregion

        #region Order Management

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        public PagedResult<Dictionary<string, object>> GetAllOrders(int page = 1, int pageSize = 20, string status = null, string searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllOrders(page, pageSize, status, searchTerm);
        }

        /// <summary>
        /// Get order statistics
        /// </summary>
        public Dictionary<string, object> GetOrderStatistics()
        {
            return _adminDAL.GetOrderStatistics();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #endregion
    }
}
