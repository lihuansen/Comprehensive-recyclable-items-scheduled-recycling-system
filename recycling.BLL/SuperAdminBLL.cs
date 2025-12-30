using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// SuperAdmin Business Logic Layer
    /// 超级管理员业务逻辑层
    /// </summary>
    public class SuperAdminBLL
    {
        private readonly SuperAdminDAL _superAdminDAL;

        public SuperAdminBLL()
        {
            _superAdminDAL = new SuperAdminDAL();
        }

        #region SuperAdmin Management

        /// <summary>
        /// Get all super admins with pagination
        /// 分页获取所有超级管理员
        /// </summary>
        public PagedResult<SuperAdmins> GetAllSuperAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _superAdminDAL.GetAllSuperAdmins(page, pageSize, searchTerm, isActive);
        }

        /// <summary>
        /// Get super admin by ID
        /// 根据ID获取超级管理员
        /// </summary>
        public SuperAdmins GetSuperAdminById(int superAdminId)
        {
            if (superAdminId <= 0)
            {
                throw new ArgumentException("Invalid super admin ID");
            }

            return _superAdminDAL.GetSuperAdminById(superAdminId);
        }

        /// <summary>
        /// Add new super admin
        /// 添加新超级管理员
        /// </summary>
        public (bool Success, string Message) AddSuperAdmin(SuperAdmins superAdmin, string password)
        {
            // Validation
            if (string.IsNullOrEmpty(superAdmin.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            if (string.IsNullOrEmpty(superAdmin.FullName))
            {
                return (false, "姓名不能为空");
            }

            // Hash password
            superAdmin.PasswordHash = HashPassword(password);
            superAdmin.IsActive = true;

            bool result = _superAdminDAL.AddSuperAdmin(superAdmin);
            return result ? (true, "添加超级管理员成功") : (false, "添加超级管理员失败");
        }

        /// <summary>
        /// Update super admin
        /// 更新超级管理员信息
        /// </summary>
        public (bool Success, string Message) UpdateSuperAdmin(SuperAdmins superAdmin)
        {
            // Validation
            if (superAdmin.SuperAdminID <= 0)
            {
                return (false, "Invalid super admin ID");
            }

            if (string.IsNullOrEmpty(superAdmin.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(superAdmin.FullName))
            {
                return (false, "姓名不能为空");
            }

            bool result = _superAdminDAL.UpdateSuperAdmin(superAdmin);
            return result ? (true, "更新超级管理员信息成功") : (false, "更新超级管理员信息失败");
        }

        /// <summary>
        /// Delete super admin (hard delete from database)
        /// 删除超级管理员（硬删除）
        /// </summary>
        public (bool Success, string Message) DeleteSuperAdmin(int superAdminId)
        {
            if (superAdminId <= 0)
            {
                return (false, "Invalid super admin ID");
            }

            try
            {
                bool result = _superAdminDAL.DeleteSuperAdmin(superAdminId);
                return result ? (true, "删除超级管理员成功") : (false, "删除超级管理员失败");
            }
            catch (InvalidOperationException ex)
            {
                // Foreign key constraint violation
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除超级管理员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get super admin statistics
        /// 获取超级管理员统计信息
        /// </summary>
        public Dictionary<string, object> GetSuperAdminStatistics()
        {
            return _superAdminDAL.GetSuperAdminStatistics();
        }

        /// <summary>
        /// Get all super admins for export (without pagination)
        /// 获取所有超级管理员用于导出（无分页）
        /// </summary>
        public List<SuperAdmins> GetAllSuperAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            return _superAdminDAL.GetAllSuperAdminsForExport(searchTerm, isActive);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hash password using SHA256
        /// 使用SHA256算法哈希密码
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
