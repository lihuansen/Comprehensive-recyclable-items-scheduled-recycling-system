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
                if (recycler.IsActive != true)
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
                if (transporter.IsActive != true)
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
        /// 验证基地工作人员登录
        /// </summary>
        private (string ErrorMsg, SortingCenterWorkers Staff) ValidateSortingCenterWorker(string username, string passwordHash)
        {
            try
            {
                var worker = _staffDAL.GetSortingCenterWorkerByUsername(username);
                if (worker == null)
                    return ("基地工作人员账号不存在", null);

                // 不区分大小写比较哈希值
                if (!string.Equals(worker.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return ("密码错误", null);

                // 检查账号是否被禁用
                if (worker.IsActive != true)
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

        /// <summary>
        /// 通过ID获取运输人员信息（供UI层调用）
        /// </summary>
        public Transporters GetTransporterById(int transporterId)
        {
            if (transporterId <= 0)
                throw new ArgumentException("运输人员ID无效");

            try
            {
                return _staffDAL.GetTransporterById(transporterId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取运输人员信息失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 更新运输人员个人信息（不包括车辆类型和载重）
        /// </summary>
        public (bool Success, string Message) UpdateTransporterProfile(int transporterId, TransporterProfileViewModel model)
        {
            if (transporterId <= 0)
                return (false, "运输人员ID无效");

            if (model == null)
                return (false, "数据不能为空");

            try
            {
                var transporter = _staffDAL.GetTransporterById(transporterId);
                if (transporter == null)
                    return (false, "运输人员不存在");

                // 更新字段（不包括车辆类型和载重）
                transporter.FullName = model.FullName;
                transporter.PhoneNumber = model.PhoneNumber;
                transporter.IDNumber = model.IDNumber;
                transporter.LicenseNumber = model.LicenseNumber;
                transporter.Region = model.Region;

                bool result = _staffDAL.UpdateTransporter(transporter);
                return result ? (true, "个人信息更新成功") : (false, "更新失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"更新失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修改运输人员密码
        /// </summary>
        public (bool Success, string Message) ChangeTransporterPassword(int transporterId, string currentPassword, string newPassword)
        {
            if (transporterId <= 0)
                return (false, "运输人员ID无效");

            if (string.IsNullOrWhiteSpace(currentPassword))
                return (false, "请输入当前密码");

            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "请输入新密码");

            if (newPassword.Length < 6)
                return (false, "新密码长度不能少于6个字符");

            try
            {
                var transporter = _staffDAL.GetTransporterById(transporterId);
                if (transporter == null)
                    return (false, "运输人员不存在");

                // 验证当前密码
                string currentPasswordHash = HashPassword(currentPassword);
                if (!string.Equals(transporter.PasswordHash, currentPasswordHash, StringComparison.OrdinalIgnoreCase))
                    return (false, "当前密码错误");

                // 更新密码
                string newPasswordHash = HashPassword(newPassword);
                transporter.PasswordHash = newPasswordHash;

                bool result = _staffDAL.UpdateTransporter(transporter);
                return result ? (true, "密码修改成功，请重新登录") : (false, "密码修改失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"修改密码失败：{ex.Message}");
            }
        }

        #region 基地工作人员账号管理

        /// <summary>
        /// 根据ID获取基地工作人员信息
        /// </summary>
        public SortingCenterWorkers GetSortingCenterWorkerById(int workerId)
        {
            if (workerId <= 0)
                throw new ArgumentException("基地工作人员ID无效");

            try
            {
                return _staffDAL.GetSortingCenterWorkerById(workerId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取基地工作人员信息失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 更新基地工作人员个人信息
        /// </summary>
        public (bool Success, string Message) UpdateSortingCenterWorkerProfile(int workerId, SortingCenterWorkerProfileViewModel model)
        {
            if (workerId <= 0)
                return (false, "基地工作人员ID无效");

            if (model == null)
                return (false, "数据不能为空");

            try
            {
                var worker = _staffDAL.GetSortingCenterWorkerById(workerId);
                if (worker == null)
                    return (false, "基地工作人员不存在");

                // 更新字段
                worker.FullName = model.FullName;
                worker.PhoneNumber = model.PhoneNumber;
                worker.IDNumber = model.IDNumber;
                worker.Position = model.Position;
                worker.WorkStation = model.WorkStation;
                worker.Specialization = model.Specialization;
                worker.ShiftType = model.ShiftType;

                bool result = _staffDAL.UpdateSortingCenterWorker(worker);
                return result ? (true, "个人信息更新成功") : (false, "更新失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"更新失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修改基地工作人员密码
        /// </summary>
        public (bool Success, string Message) ChangeSortingCenterWorkerPassword(int workerId, string currentPassword, string newPassword)
        {
            if (workerId <= 0)
                return (false, "基地工作人员ID无效");

            if (string.IsNullOrWhiteSpace(currentPassword))
                return (false, "请输入当前密码");

            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "请输入新密码");

            if (newPassword.Length < 6)
                return (false, "新密码长度不能少于6个字符");

            try
            {
                var worker = _staffDAL.GetSortingCenterWorkerById(workerId);
                if (worker == null)
                    return (false, "基地工作人员不存在");

                // 验证当前密码
                string currentPasswordHash = HashPassword(currentPassword);
                if (!string.Equals(worker.PasswordHash, currentPasswordHash, StringComparison.Ordinal))
                    return (false, "当前密码错误");

                // 更新密码
                string newPasswordHash = HashPassword(newPassword);
                worker.PasswordHash = newPasswordHash;

                bool result = _staffDAL.UpdateSortingCenterWorker(worker);
                return result ? (true, "密码修改成功，请重新登录") : (false, "密码修改失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"修改密码失败：{ex.Message}");
            }
        }

        #endregion
    }
}
