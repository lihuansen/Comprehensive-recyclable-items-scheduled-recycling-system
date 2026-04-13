using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// 超级管理员业务逻辑处理类。
    /// 超级管理员业务逻辑层
    public class SuperAdminBLL
    {
        private readonly SuperAdminDAL _superAdminDAL;

        public SuperAdminBLL()
        {
            _superAdminDAL = new SuperAdminDAL();
        }

        #region 超级管理员管理

        /// 获取超级管理员列表。
        /// 分页获取所有超级管理员
        public PagedResult<SuperAdmins> GetAllSuperAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _superAdminDAL.GetAllSuperAdmins(page, pageSize, searchTerm, isActive);
        }

        /// 根据编号获取超级管理员。
        /// 根据ID获取超级管理员
        public SuperAdmins GetSuperAdminById(int superAdminId)
        {
            if (superAdminId <= 0)
            {
                throw new ArgumentException("Invalid super admin ID");
            }

            return _superAdminDAL.GetSuperAdminById(superAdminId);
        }

        /// 新增超级管理员。
        /// 添加新超级管理员
        public (bool Success, string Message) AddSuperAdmin(SuperAdmins superAdmin, string password)
        {
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

            superAdmin.PasswordHash = HashPassword(password);
            superAdmin.IsActive = true;

            bool result = _superAdminDAL.AddSuperAdmin(superAdmin);
            return result ? (true, "添加超级管理员成功") : (false, "添加超级管理员失败");
        }

        /// 更新超级管理员。
        /// 更新超级管理员信息
        public (bool Success, string Message) UpdateSuperAdmin(SuperAdmins superAdmin)
        {
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

        /// 删除超级管理员。
        /// 删除超级管理员（硬删除）
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
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除超级管理员失败：{ex.Message}");
            }
        }

        /// 获取超级管理员统计数据。
        /// 获取超级管理员统计信息
        public Dictionary<string, object> GetSuperAdminStatistics()
        {
            return _superAdminDAL.GetSuperAdminStatistics();
        }

        /// 获取超级管理员列表导出数据。
        /// 获取所有超级管理员用于导出（无分页）
        public List<SuperAdmins> GetAllSuperAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            return _superAdminDAL.GetAllSuperAdminsForExport(searchTerm, isActive);
        }

        /// 获取管理员仪表盘统计数据。
        /// 获取管理员数据看板统计信息
        public Dictionary<string, object> GetAdminDashboardStatistics()
        {
            return _superAdminDAL.GetAdminDashboardStatistics();
        }

        #endregion

        #region 辅助方法

        /// 对密码进行 SHA256 哈希处理。
        /// 使用SHA256算法哈希密码
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

        #region 超级管理员头像方法

        /// 更新超级管理员头像
        public bool UpdateSuperAdminAvatar(int superAdminId, string avatarUrl)
        {
            if (superAdminId <= 0)
                throw new ArgumentException("超级管理员ID无效");
            return _superAdminDAL.UpdateSuperAdminAvatar(superAdminId, avatarUrl);
        }

        #endregion
    }
}
