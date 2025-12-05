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
        /// 工作人员登录验证（根据角色区分处理）
        /// </summary>
        /// <param name="role">角色（recycler/admin/superadmin）</param>
        /// <param name="username">账号</param>
        /// <param name="password">密码（明文）</param>
        /// <returns>返回值：(错误信息, 工作人员实体)；错误信息为null表示登录成功</returns>
        public (string ErrorMsg, object Staff) Login(string role, string username, string password)
        {
            // 1. 基础验证（与UserBLL保持一致）
            if (string.IsNullOrWhiteSpace(username))
                return ("请输入用户名", null);
            if (string.IsNullOrWhiteSpace(password))
                return ("请输入密码", null);
            if (string.IsNullOrWhiteSpace(role))
                return ("请选择角色", null);

            // 2. 密码哈希（与UserBLL完全一致）
            string passwordHash = HashPassword(password);

            // 3. 根据角色验证（保持原有逻辑）
            switch (role.ToLower())
            {
                case "recycler":
                    return ValidateRecycler(username, passwordHash);
                case "admin":
                    return ValidateAdmin(username, passwordHash);
                case "superadmin":
                    return ValidateSuperAdmin(username, passwordHash);
                case "transporter":
                    return ValidateTransporter(username, passwordHash);
                case "sortingcenterworker":
                    return ValidateSortingCenterWorker(username, passwordHash);
                default:
                    return ("无效的角色", null);
            }
        }

        #region 角色验证逻辑（调用DAL层）
        /// <summary>
        /// 验证回收员登录
        /// </summary>
        private (string ErrorMsg, Recyclers Staff) ValidateRecycler(string username, string passwordHash)
        {
            try
            {
                var recycler = _staffDAL.GetRecyclerByUsername(username);
                if (recycler == null)
                    return ("回收员账号不存在", null);

                // 修复：不区分大小写比较哈希值
                if (!string.Equals(recycler.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 检查账号是否被禁用
                if (!recycler.IsActive)
                    return ("账号已被禁用，无法登录", null);

                // 登录成功，更新最后登录时间
                _staffDAL.UpdateRecyclerLastLogin(recycler.RecyclerID);
                return (null, recycler);
            }
            catch (Exception ex)
            {
                return ($"登录失败：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 验证管理员登录
        /// </summary>
        private (string ErrorMsg, Admins Staff) ValidateAdmin(string username, string passwordHash)
        {
            try
            {
                var admin = _staffDAL.GetAdminByUsername(username);
                if (admin == null)
                    return ("管理员账号不存在", null);
                // 修复：不区分大小写比较哈希值
                if (!string.Equals(admin.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 登录成功，更新最后登录时间
                _staffDAL.UpdateAdminLastLogin(admin.AdminID);
                return (null, admin);
            }
            catch (Exception ex)
            {
                return ($"登录失败：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 验证超级管理员登录
        /// </summary>
        private (string ErrorMsg, SuperAdmins Staff) ValidateSuperAdmin(string username, string passwordHash)
        {
            try
            {
                var superAdmin = _staffDAL.GetSuperAdminByUsername(username);
                if (superAdmin == null)
                    return ("超级管理员账号不存在", null);
                // 修复：不区分大小写比较哈希值
                if (!string.Equals(superAdmin.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 登录成功，更新最后登录时间
                _staffDAL.UpdateSuperAdminLastLogin(superAdmin.SuperAdminID);
                return (null, superAdmin);
            }
            catch (Exception ex)
            {
                return ($"登录失败：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 验证运输人员登录
        /// </summary>
        private (string ErrorMsg, Transporters Staff) ValidateTransporter(string username, string passwordHash)
        {
            try
            {
                var transporter = _staffDAL.GetTransporterByUsername(username);
                if (transporter == null)
                    return ("运输人员账号不存在", null);

                // 不区分大小写比较哈希值
                if (!string.Equals(transporter.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 检查账号是否被禁用
                if (!transporter.IsActive)
                    return ("账号已被禁用，无法登录", null);

                // 登录成功，更新最后登录时间
                _staffDAL.UpdateTransporterLastLogin(transporter.TransporterID);
                return (null, transporter);
            }
            catch (Exception ex)
            {
                return ($"登录失败：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 验证分拣中心工作人员登录
        /// </summary>
        private (string ErrorMsg, SortingCenterWorkers Staff) ValidateSortingCenterWorker(string username, string passwordHash)
        {
            try
            {
                var worker = _staffDAL.GetSortingCenterWorkerByUsername(username);
                if (worker == null)
                    return ("分拣中心工作人员账号不存在", null);

                // 不区分大小写比较哈希值
                if (!string.Equals(worker.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 检查账号是否被禁用
                if (!worker.IsActive)
                    return ("账号已被禁用，无法登录", null);

                // 登录成功，更新最后登录时间
                _staffDAL.UpdateSortingCenterWorkerLastLogin(worker.WorkerID);
                return (null, worker);
            }
            catch (Exception ex)
            {
                return ($"登录失败：{ex.Message}", null);
            }
        }
        #endregion

        /// <summary>
        /// 密码哈希算法（与UserBLL保持完全一致）
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

        /// <summary>
        /// 通过ID获取回收员信息（供UI层调用）
        /// </summary>
        public Recyclers GetRecyclerById(int recyclerId)
        {
            if (recyclerId <= 0)
                throw new ArgumentException("回收员ID无效");

            try
            {
                return _staffDAL.GetRecyclerById(recyclerId); // 调用DAL层新方法
            }
            catch (Exception ex)
            {
                throw new Exception("获取回收员信息失败：" + ex.Message);
            }
        }
    }
}
