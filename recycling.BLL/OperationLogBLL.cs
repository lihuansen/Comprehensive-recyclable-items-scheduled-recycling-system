using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class OperationLogBLL
    {
        private readonly OperationLogDAL _logDAL;

        public OperationLogBLL()
        {
            _logDAL = new OperationLogDAL();
        }

        /// <summary>
        /// 记录管理员操作日志
        /// </summary>
        public bool LogOperation(int adminId, string adminUsername, string module, string operationType, string description, int? targetId = null, string targetName = null, string ipAddress = null, string result = "Success", string details = null)
        {
            var log = new AdminOperationLogs
            {
                AdminID = adminId,
                AdminUsername = adminUsername,
                Module = module,
                OperationType = operationType,
                Description = description,
                TargetID = targetId,
                TargetName = targetName,
                IPAddress = ipAddress,
                OperationTime = DateTime.Now,
                Result = result,
                Details = details
            };

            try
            {
                return _logDAL.AddLog(log);
            }
            catch
            {
                // Log failure should not affect main operation
                return false;
            }
        }

        /// <summary>
        /// 获取操作日志列表（分页）
        /// </summary>
        public PagedResult<AdminOperationLogs> GetLogs(int page = 1, int pageSize = 20, string module = null, string operationType = null, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _logDAL.GetLogs(page, pageSize, module, operationType, startDate, endDate, searchTerm);
        }

        /// <summary>
        /// 获取日志统计信息
        /// </summary>
        public Dictionary<string, object> GetLogStatistics()
        {
            return _logDAL.GetLogStatistics();
        }

        /// <summary>
        /// 导出日志（不分页）
        /// </summary>
        public List<AdminOperationLogs> GetLogsForExport(string module = null, string operationType = null, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null)
        {
            return _logDAL.GetLogsForExport(module, operationType, startDate, endDate, searchTerm);
        }

        #region Module Constants

        public static class Modules
        {
            public const string UserManagement = "UserManagement";
            public const string RecyclerManagement = "RecyclerManagement";
            public const string FeedbackManagement = "FeedbackManagement";
            public const string HomepageManagement = "HomepageManagement";
            public const string LogManagement = "LogManagement";
            public const string WarehouseManagement = "WarehouseManagement";
            public const string AdminManagement = "AdminManagement";
        }

        public static class OperationTypes
        {
            public const string View = "View";
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
            public const string Export = "Export";
            public const string Reply = "Reply";
            public const string Search = "Search";
        }

        /// <summary>
        /// 获取模块中文名称
        /// </summary>
        public static string GetModuleDisplayName(string module)
        {
            switch (module)
            {
                case Modules.UserManagement: return "用户管理";
                case Modules.RecyclerManagement: return "回收员管理";
                case Modules.FeedbackManagement: return "反馈管理";
                case Modules.HomepageManagement: return "首页页面管理";
                case Modules.LogManagement: return "日志管理";
                case Modules.WarehouseManagement: return "仓库管理";
                case Modules.AdminManagement: return "管理员管理";
                default: return module;
            }
        }

        /// <summary>
        /// 获取操作类型中文名称
        /// </summary>
        public static string GetOperationTypeDisplayName(string operationType)
        {
            switch (operationType)
            {
                case OperationTypes.View: return "查看";
                case OperationTypes.Create: return "新增";
                case OperationTypes.Update: return "更新";
                case OperationTypes.Delete: return "删除";
                case OperationTypes.Export: return "导出";
                case OperationTypes.Reply: return "回复";
                case OperationTypes.Search: return "搜索";
                default: return operationType;
            }
        }

        #endregion
    }
}
