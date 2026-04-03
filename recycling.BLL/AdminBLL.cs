using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class AdminBLL
    {
        private readonly AdminDAL _adminDAL;
        private static readonly Regex ChinaMobileRegex = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
        private static readonly Regex ChinaIdNumber18Regex = new Regex(@"^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$", RegexOptions.Compiled);
        private static readonly Regex ChinaIdNumber15Regex = new Regex(@"^[1-9]\d{5}\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}$", RegexOptions.Compiled);
        private static readonly int[] ChinaIdNumberWeights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        private static readonly char[] ChinaIdNumberCheckCodes = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

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

        /// <summary>
        /// Get comprehensive user dashboard statistics
        /// </summary>
        public Dictionary<string, object> GetUserDashboardStatistics()
        {
            return _adminDAL.GetUserDashboardStatistics();
        }

        /// <summary>
        /// Get all users for export (without pagination)
        /// </summary>
        public List<Users> GetAllUsersForExport(string searchTerm = null)
        {
            return _adminDAL.GetAllUsersForExport(searchTerm);
        }

        #endregion

        #region Recycler Management

        /// <summary>
        /// Get all recyclers with pagination (optimized with completed orders count and sort order)
        /// </summary>
        public PagedResult<RecyclerListViewModel> GetAllRecyclersWithDetails(int page = 1, int pageSize = 8, string searchTerm = null, bool? isActive = null, string sortOrder = "ASC")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 8;

            return _adminDAL.GetAllRecyclersWithDetails(page, pageSize, searchTerm, isActive, sortOrder);
        }

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
            if (recycler == null)
            {
                return (false, "回收员信息不能为空");
            }

            recycler.Username = recycler.Username?.Trim();
            recycler.PhoneNumber = recycler.PhoneNumber?.Trim();
            recycler.Region = recycler.Region?.Trim();
            recycler.FullName = recycler.FullName?.Trim();
            password = password?.Trim();

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

            if (!Regex.IsMatch(recycler.PhoneNumber, @"^1[3-9]\d{9}$"))
            {
                return (false, "请输入有效的11位手机号");
            }

            if (_adminDAL.IsRecyclerUsernameExists(recycler.Username))
            {
                return (false, "用户名已存在，请更换其他用户名");
            }

            if (_adminDAL.IsRecyclerPhoneNumberExists(recycler.PhoneNumber))
            {
                return (false, "手机号已存在，请更换其他手机号");
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
            if (recycler == null)
            {
                return (false, "回收员信息不能为空");
            }

            recycler.Username = recycler.Username?.Trim();
            recycler.PhoneNumber = recycler.PhoneNumber?.Trim();
            recycler.Region = recycler.Region?.Trim();
            recycler.FullName = recycler.FullName?.Trim();

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

            if (!Regex.IsMatch(recycler.PhoneNumber, @"^1[3-9]\d{9}$"))
            {
                return (false, "请输入有效的11位手机号");
            }

            if (_adminDAL.IsRecyclerUsernameExists(recycler.Username, recycler.RecyclerID))
            {
                return (false, "用户名已存在，请更换其他用户名");
            }

            if (_adminDAL.IsRecyclerPhoneNumberExists(recycler.PhoneNumber, recycler.RecyclerID))
            {
                return (false, "手机号已存在，请更换其他手机号");
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

        /// <summary>
        /// Get comprehensive recycler dashboard statistics for admin
        /// </summary>
        public Dictionary<string, object> GetRecyclerDashboardStatistics()
        {
            return _adminDAL.GetRecyclerDashboardStatistics();
        }

        /// <summary>
        /// Get all recyclers for export (without pagination)
        /// </summary>
        public List<Recyclers> GetAllRecyclersForExport(string searchTerm = null, bool? isActive = null)
        {
            return _adminDAL.GetAllRecyclersForExport(searchTerm, isActive);
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

        #region Admin Management

        /// <summary>
        /// Get all admins with pagination
        /// </summary>
        public PagedResult<Admins> GetAllAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllAdmins(page, pageSize, searchTerm, isActive);
        }

        /// <summary>
        /// Get admin by ID
        /// </summary>
        public Admins GetAdminById(int adminId)
        {
            if (adminId <= 0)
            {
                throw new ArgumentException("Invalid admin ID");
            }

            return _adminDAL.GetAdminById(adminId);
        }

        /// <summary>
        /// Add new admin
        /// </summary>
        public (bool Success, string Message) AddAdmin(Admins admin, string password)
        {
            // Validation
            if (string.IsNullOrEmpty(admin.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            if (string.IsNullOrEmpty(admin.FullName))
            {
                return (false, "姓名不能为空");
            }

            // Hash password
            admin.PasswordHash = HashPassword(password);
            admin.IsActive = true;

            bool result = _adminDAL.AddAdmin(admin);
            return result ? (true, "添加管理员成功") : (false, "添加管理员失败");
        }

        /// <summary>
        /// Update admin
        /// </summary>
        public (bool Success, string Message) UpdateAdmin(Admins admin)
        {
            // Validation
            if (admin.AdminID <= 0)
            {
                return (false, "Invalid admin ID");
            }

            if (string.IsNullOrEmpty(admin.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(admin.FullName))
            {
                return (false, "姓名不能为空");
            }

            bool result = _adminDAL.UpdateAdmin(admin);
            return result ? (true, "更新管理员信息成功") : (false, "更新管理员信息失败");
        }

        /// <summary>
        /// Delete admin (hard delete from database)
        /// </summary>
        public (bool Success, string Message) DeleteAdmin(int adminId)
        {
            if (adminId <= 0)
            {
                return (false, "Invalid admin ID");
            }

            try
            {
                bool result = _adminDAL.DeleteAdmin(adminId);
                return result ? (true, "删除管理员成功") : (false, "删除管理员失败");
            }
            catch (InvalidOperationException ex)
            {
                // Foreign key constraint violation
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除管理员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get admin statistics
        /// </summary>
        public Dictionary<string, object> GetAdminStatistics()
        {
            return _adminDAL.GetAdminStatistics();
        }

        /// <summary>
        /// Get all admins for export (without pagination)
        /// </summary>
        public List<Admins> GetAllAdminsForExport(string searchTerm = null, bool? isActive = null)
        {
            return _adminDAL.GetAllAdminsForExport(searchTerm, isActive);
        }

        #endregion

        #region Dashboard Statistics

        /// <summary>
        /// Get comprehensive dashboard statistics for super admin
        /// </summary>
        public Dictionary<string, object> GetDashboardStatistics()
        {
            return _adminDAL.GetDashboardStatistics();
        }

        #endregion

        #region Transporter Management

        /// <summary>
        /// Get all transporters with pagination
        /// </summary>
        public PagedResult<Transporters> GetAllTransporters(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllTransporters(page, pageSize, searchTerm, isActive);
        }

        /// <summary>
        /// Get transporter by ID
        /// </summary>
        public Transporters GetTransporterById(int transporterId)
        {
            if (transporterId <= 0)
            {
                throw new ArgumentException("Invalid transporter ID");
            }

            return _adminDAL.GetTransporterById(transporterId);
        }

        /// <summary>
        /// Add new transporter
        /// </summary>
        public (bool Success, string Message) AddTransporter(Transporters transporter, string password)
        {
            if (transporter == null)
            {
                return (false, "运输人员信息不能为空");
            }

            transporter.Username = transporter.Username?.Trim();
            transporter.PhoneNumber = transporter.PhoneNumber?.Trim();
            transporter.IDNumber = NormalizeChinaIdNumber(transporter.IDNumber);
            transporter.Region = transporter.Region?.Trim();
            transporter.FullName = transporter.FullName?.Trim();
            password = password?.Trim();

            if (string.IsNullOrEmpty(transporter.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            if (string.IsNullOrEmpty(transporter.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(transporter.Region))
            {
                return (false, "区域不能为空");
            }

            if (!ChinaMobileRegex.IsMatch(transporter.PhoneNumber))
            {
                return (false, "请输入有效的11位手机号");
            }

            if (!string.IsNullOrEmpty(transporter.IDNumber) && !IsValidChinaIdNumber(transporter.IDNumber))
            {
                return (false, "请输入有效的身份证号");
            }

            if (_adminDAL.IsTransporterUsernameExists(transporter.Username))
            {
                return (false, "用户名已存在，请更换其他用户名");
            }

            if (_adminDAL.IsTransporterPhoneNumberExists(transporter.PhoneNumber))
            {
                return (false, "手机号已存在，请更换其他手机号");
            }

            if (!string.IsNullOrEmpty(transporter.IDNumber) && _adminDAL.IsTransporterIDNumberExists(transporter.IDNumber))
            {
                return (false, "身份证号已存在，请核对后重试");
            }

            transporter.PasswordHash = HashPassword(password);
            transporter.IsActive = true;
            transporter.Available = true;

            bool result = _adminDAL.AddTransporter(transporter);
            return result ? (true, "添加运输人员成功") : (false, "添加运输人员失败");
        }

        /// <summary>
        /// Update transporter
        /// </summary>
        public (bool Success, string Message) UpdateTransporter(Transporters transporter)
        {
            if (transporter == null)
            {
                return (false, "运输人员信息不能为空");
            }

            transporter.Username = transporter.Username?.Trim();
            transporter.PhoneNumber = transporter.PhoneNumber?.Trim();
            transporter.IDNumber = NormalizeChinaIdNumber(transporter.IDNumber);
            transporter.Region = transporter.Region?.Trim();
            transporter.FullName = transporter.FullName?.Trim();

            if (transporter.TransporterID <= 0)
            {
                return (false, "Invalid transporter ID");
            }

            if (string.IsNullOrEmpty(transporter.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(transporter.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(transporter.Region))
            {
                return (false, "区域不能为空");
            }

            if (!ChinaMobileRegex.IsMatch(transporter.PhoneNumber))
            {
                return (false, "请输入有效的11位手机号");
            }

            if (!string.IsNullOrEmpty(transporter.IDNumber) && !IsValidChinaIdNumber(transporter.IDNumber))
            {
                return (false, "请输入有效的身份证号");
            }

            if (_adminDAL.IsTransporterUsernameExists(transporter.Username, transporter.TransporterID))
            {
                return (false, "用户名已存在，请更换其他用户名");
            }

            if (_adminDAL.IsTransporterPhoneNumberExists(transporter.PhoneNumber, transporter.TransporterID))
            {
                return (false, "手机号已存在，请更换其他手机号");
            }

            if (!string.IsNullOrEmpty(transporter.IDNumber) && _adminDAL.IsTransporterIDNumberExists(transporter.IDNumber, transporter.TransporterID))
            {
                return (false, "身份证号已存在，请核对后重试");
            }

            bool result = _adminDAL.UpdateTransporter(transporter);
            return result ? (true, "更新运输人员信息成功") : (false, "更新运输人员信息失败");
        }

        /// <summary>
        /// Delete transporter
        /// </summary>
        public (bool Success, string Message) DeleteTransporter(int transporterId)
        {
            if (transporterId <= 0)
            {
                return (false, "Invalid transporter ID");
            }

            try
            {
                bool result = _adminDAL.DeleteTransporter(transporterId);
                return result ? (true, "删除运输人员成功") : (false, "删除运输人员失败");
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除运输人员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get transporter statistics
        /// </summary>
        public Dictionary<string, object> GetTransporterStatistics()
        {
            return _adminDAL.GetTransporterStatistics();
        }

        /// <summary>
        /// Get comprehensive transporter dashboard statistics
        /// </summary>
        public Dictionary<string, object> GetTransporterDashboardStatistics()
        {
            return _adminDAL.GetTransporterDashboardStatistics();
        }

        /// <summary>
        /// Get all transporters for export
        /// </summary>
        public List<Transporters> GetAllTransportersForExport(string searchTerm = null, bool? isActive = null)
        {
            return _adminDAL.GetAllTransportersForExport(searchTerm, isActive);
        }

        #endregion

        #region SortingCenterWorker Management

        /// <summary>
        /// Get all sorting center workers with pagination
        /// </summary>
        public PagedResult<SortingCenterWorkers> GetAllSortingCenterWorkers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _adminDAL.GetAllSortingCenterWorkers(page, pageSize, searchTerm, isActive);
        }

        /// <summary>
        /// Get sorting center worker by ID
        /// </summary>
        public SortingCenterWorkers GetSortingCenterWorkerById(int workerId)
        {
            if (workerId <= 0)
            {
                throw new ArgumentException("Invalid worker ID");
            }

            return _adminDAL.GetSortingCenterWorkerById(workerId);
        }

        /// <summary>
        /// Add new sorting center worker
        /// </summary>
        public (bool Success, string Message) AddSortingCenterWorker(SortingCenterWorkers worker, string password)
        {
            if (worker == null)
            {
                return (false, "基地人员信息不能为空");
            }

            SanitizeSortingCenterWorkerInput(worker);
            password = password?.Trim();

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            var validationResult = ValidateSortingCenterWorkerForSave(worker, null);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            worker.PasswordHash = HashPassword(password);
            worker.IsActive = true;
            worker.Available = true;

            bool result = _adminDAL.AddSortingCenterWorker(worker);
            return result ? (true, "添加基地人员成功") : (false, "添加基地人员失败");
        }

        /// <summary>
        /// Update sorting center worker
        /// </summary>
        public (bool Success, string Message) UpdateSortingCenterWorker(SortingCenterWorkers worker)
        {
            if (worker == null)
            {
                return (false, "基地人员信息不能为空");
            }

            SanitizeSortingCenterWorkerInput(worker);

            if (worker.WorkerID <= 0)
            {
                return (false, "Invalid worker ID");
            }

            var validationResult = ValidateSortingCenterWorkerForSave(worker, worker.WorkerID);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            bool result = _adminDAL.UpdateSortingCenterWorker(worker);
            return result ? (true, "更新基地人员信息成功") : (false, "更新基地人员信息失败");
        }

        /// <summary>
        /// Delete sorting center worker
        /// </summary>
        public (bool Success, string Message) DeleteSortingCenterWorker(int workerId)
        {
            if (workerId <= 0)
            {
                return (false, "Invalid worker ID");
            }

            try
            {
                bool result = _adminDAL.DeleteSortingCenterWorker(workerId);
                return result ? (true, "删除基地人员成功") : (false, "删除基地人员失败");
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"删除基地人员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get sorting center worker statistics
        /// </summary>
        public Dictionary<string, object> GetSortingCenterWorkerStatistics()
        {
            return _adminDAL.GetSortingCenterWorkerStatistics();
        }

        /// <summary>
        /// Get comprehensive sorting center worker dashboard statistics
        /// </summary>
        public Dictionary<string, object> GetSortingCenterWorkerDashboardStatistics()
        {
            return _adminDAL.GetSortingCenterWorkerDashboardStatistics();
        }

        /// <summary>
        /// Get all sorting center workers for export
        /// </summary>
        public List<SortingCenterWorkers> GetAllSortingCenterWorkersForExport(string searchTerm = null, bool? isActive = null)
        {
            return _adminDAL.GetAllSortingCenterWorkersForExport(searchTerm, isActive);
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

        /// <summary>
        /// Validate China resident ID number (format + checksum)
        /// </summary>
        private bool IsValidChinaIdNumber(string idNumber)
        {
            if (string.IsNullOrEmpty(idNumber))
            {
                return false;
            }

            idNumber = NormalizeChinaIdNumber(idNumber);

            if (ChinaIdNumber15Regex.IsMatch(idNumber))
            {
                return true;
            }

            if (!ChinaIdNumber18Regex.IsMatch(idNumber))
            {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idNumber[i] - '0') * ChinaIdNumberWeights[i];
            }

            char expectedCheckCode = ChinaIdNumberCheckCodes[sum % 11];
            char actualCheckCode = char.ToUpperInvariant(idNumber[17]);
            return actualCheckCode == expectedCheckCode;
        }

        private string NormalizeChinaIdNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
            {
                return string.Empty;
            }

            StringBuilder normalized = new StringBuilder(idNumber.Length);
            foreach (char c in idNumber.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c >= '０' && c <= '９')
                {
                    normalized.Append((char)('0' + (c - '０')));
                    continue;
                }

                if (c == 'x' || c == 'ｘ' || c == 'Ｘ')
                {
                    normalized.Append('X');
                    continue;
                }

                normalized.Append(c);
            }

            return normalized.ToString();
        }

        private void SanitizeSortingCenterWorkerInput(SortingCenterWorkers worker)
        {
            worker.Username = worker.Username?.Trim();
            worker.PhoneNumber = worker.PhoneNumber?.Trim();
            worker.IDNumber = NormalizeChinaIdNumber(worker.IDNumber);
            worker.FullName = worker.FullName?.Trim();
        }

        private (bool Success, string Message) ValidateSortingCenterWorkerForSave(SortingCenterWorkers worker, int? excludeWorkerId)
        {
            if (string.IsNullOrEmpty(worker.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(worker.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(worker.IDNumber))
            {
                return (false, "身份证号不能为空");
            }

            if (!ChinaMobileRegex.IsMatch(worker.PhoneNumber))
            {
                return (false, "请输入有效的11位手机号");
            }

            if (!IsValidChinaIdNumber(worker.IDNumber))
            {
                return (false, "请输入有效的身份证号");
            }

            if (_adminDAL.IsSortingCenterWorkerUsernameExists(worker.Username, excludeWorkerId))
            {
                return (false, "用户名已存在，请更换其他用户名");
            }

            if (_adminDAL.IsSortingCenterWorkerPhoneNumberExists(worker.PhoneNumber, excludeWorkerId))
            {
                return (false, "手机号已存在，请更换其他手机号");
            }

            if (_adminDAL.IsSortingCenterWorkerIDNumberExists(worker.IDNumber, excludeWorkerId))
            {
                return (false, "身份证号已存在，请核对后重试");
            }

            return (true, string.Empty);
        }

        #endregion

        #region Staff Avatar Methods

        /// <summary>
        /// 更新回收员头像
        /// </summary>
        public bool UpdateRecyclerAvatar(int recyclerId, string avatarUrl)
        {
            if (recyclerId <= 0)
                throw new ArgumentException("回收员ID无效");
            return _adminDAL.UpdateRecyclerAvatar(recyclerId, avatarUrl);
        }

        /// <summary>
        /// 更新管理员头像
        /// </summary>
        public bool UpdateAdminAvatar(int adminId, string avatarUrl)
        {
            if (adminId <= 0)
                throw new ArgumentException("管理员ID无效");
            return _adminDAL.UpdateAdminAvatar(adminId, avatarUrl);
        }

        /// <summary>
        /// 更新运输人员头像
        /// </summary>
        public bool UpdateTransporterAvatar(int transporterId, string avatarUrl)
        {
            if (transporterId <= 0)
                throw new ArgumentException("运输人员ID无效");
            return _adminDAL.UpdateTransporterAvatar(transporterId, avatarUrl);
        }

        /// <summary>
        /// 更新基地工作人员头像
        /// </summary>
        public bool UpdateSortingCenterWorkerAvatar(int workerId, string avatarUrl)
        {
            if (workerId <= 0)
                throw new ArgumentException("基地工作人员ID无效");
            return _adminDAL.UpdateSortingCenterWorkerAvatar(workerId, avatarUrl);
        }

        #endregion
    }
}
