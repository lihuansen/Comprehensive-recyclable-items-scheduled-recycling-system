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

            if (string.IsNullOrEmpty(transporter.VehicleType))
            {
                return (false, "车辆类型不能为空");
            }

            if (string.IsNullOrEmpty(transporter.VehiclePlateNumber))
            {
                return (false, "车牌号不能为空");
            }

            if (string.IsNullOrEmpty(transporter.Region))
            {
                return (false, "区域不能为空");
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

            if (string.IsNullOrEmpty(transporter.VehicleType))
            {
                return (false, "车辆类型不能为空");
            }

            if (string.IsNullOrEmpty(transporter.VehiclePlateNumber))
            {
                return (false, "车牌号不能为空");
            }

            if (string.IsNullOrEmpty(transporter.Region))
            {
                return (false, "区域不能为空");
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
            if (string.IsNullOrEmpty(worker.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }

            if (string.IsNullOrEmpty(worker.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(worker.SortingCenterName))
            {
                return (false, "基地名称不能为空");
            }

            if (string.IsNullOrEmpty(worker.Position))
            {
                return (false, "职位不能为空");
            }

            if (string.IsNullOrEmpty(worker.ShiftType))
            {
                return (false, "班次类型不能为空");
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
            if (worker.WorkerID <= 0)
            {
                return (false, "Invalid worker ID");
            }

            if (string.IsNullOrEmpty(worker.Username))
            {
                return (false, "用户名不能为空");
            }

            if (string.IsNullOrEmpty(worker.PhoneNumber))
            {
                return (false, "手机号不能为空");
            }

            if (string.IsNullOrEmpty(worker.SortingCenterName))
            {
                return (false, "基地名称不能为空");
            }

            if (string.IsNullOrEmpty(worker.Position))
            {
                return (false, "职位不能为空");
            }

            if (string.IsNullOrEmpty(worker.ShiftType))
            {
                return (false, "班次类型不能为空");
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

        #endregion
    }
}
