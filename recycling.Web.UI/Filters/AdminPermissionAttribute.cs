using System;
using System.Web;
using System.Web.Mvc;
using recycling.Model;

namespace recycling.Web.UI.Filters
{
    /// <summary>
    /// 管理员权限验证过滤器
    /// 用于验证管理员是否有权限访问特定功能
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminPermissionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 所需的权限
        /// </summary>
        public string RequiredPermission { get; set; }

        public AdminPermissionAttribute(string requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 检查是否登录
            var loginStaff = filterContext.HttpContext.Session["LoginStaff"];
            var staffRole = filterContext.HttpContext.Session["StaffRole"] as string;

            // 如果未登录，跳转到登录页
            if (loginStaff == null || string.IsNullOrEmpty(staffRole))
            {
                filterContext.Result = new RedirectResult("~/Staff/Login");
                return;
            }

            // 如果是超级管理员，允许所有操作
            if (staffRole == "superadmin")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // 如果不是管理员角色，拒绝访问
            if (staffRole != "admin")
            {
                filterContext.Result = new HttpUnauthorizedResult("无权访问此功能");
                return;
            }

            // 检查管理员权限
            var admin = loginStaff as Admins;
            if (admin == null)
            {
                filterContext.Result = new HttpUnauthorizedResult("无效的管理员信息");
                return;
            }

            // 验证权限
            if (!AdminPermissions.HasPermission(admin.Character, RequiredPermission))
            {
                // 权限不足，返回403错误页或跳转到提示页
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml",
                    ViewData = new ViewDataDictionary
                    {
                        ["Message"] = $"您没有权限访问此功能。需要权限：{AdminPermissions.GetDisplayName(RequiredPermission)}"
                    }
                };
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
