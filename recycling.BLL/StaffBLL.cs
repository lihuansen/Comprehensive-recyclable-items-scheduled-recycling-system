using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class StaffBLL
    {
        private readonly StaffDAL _staffDAL = new StaffDAL();

        /// <summary>
        /// 工作人员登录通用方法
        /// </summary>
        /// <param name="role">角色类型：Recycler/Admin/SuperAdmin</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果（错误信息+用户实体）</returns>
        public (string ErrorMsg, object User) Login(string role, string username, string password)
        {
            // 1. 验证输入
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return ("用户名和密码不能为空", null);
            }

            // 2. 密码哈希处理（与存储时一致）
            string passwordHash = HashPassword(password);

            // 3. 根据角色验证
            switch (role.ToLower())
            {
                case "recycler":
                    return ValidateRecycler(username, passwordHash);
                case "admin":
                    return ValidateAdmin(username, passwordHash);
                case "superadmin":
                    return ValidateSuperAdmin(username, passwordHash);
                default:
                    return ("请选择正确的角色", null);
            }
        }

        #region 角色验证私有方法
        private (string, object) ValidateRecycler(string username, string passwordHash)
        {
            var recycler = _staffDAL.GetRecyclerByUsername(username);
            if (recycler == null)
                return ("回收员用户名不存在", null);

            if (!recycler.IsActive)
                return ("账号已被禁用", null);

            if (recycler.PasswordHash != passwordHash)
                return ("密码错误", null);

            // 更新登录时间
            _staffDAL.UpdateRecyclerLastLogin(recycler.RecyclerID);
            return (null, recycler);
        }

        private (string, object) ValidateAdmin(string username, string passwordHash)
        {
            var admin = _staffDAL.GetAdminByUsername(username);
            if (admin == null)
                return ("管理员用户名不存在", null);

            if (!admin.IsActive)
                return ("账号已被禁用", null);

            if (admin.PasswordHash != passwordHash)
                return ("密码错误", null);

            // 更新登录时间
            _staffDAL.UpdateAdminLastLogin(admin.AdminID);
            return (null, admin);
        }

        private (string, object) ValidateSuperAdmin(string username, string passwordHash)
        {
            var superAdmin = _staffDAL.GetSuperAdminByUsername(username);
            if (superAdmin == null)
                return ("超级管理员用户名不存在", null);

            if (!superAdmin.IsActive)
                return ("账号已被禁用", null);

            if (superAdmin.PasswordHash != passwordHash)
                return ("密码错误", null);

            // 更新登录时间
            _staffDAL.UpdateSuperAdminLastLogin(superAdmin.SuperAdminID);
            return (null, superAdmin);
        }
        #endregion

        /// <summary>
        /// 密码SHA256哈希（与数据库存储一致）
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
