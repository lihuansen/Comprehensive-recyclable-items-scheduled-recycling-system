using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using recycling.BLL;
using recycling.Model;

namespace recycling.Web.UI.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffBLL _staffBLL = new StaffBLL();

        // GET: Staff
        public ActionResult Index()
        {
            return View();
        }

        // 显示登录页（已有界面，无需修改）
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["LoginStaff"] != null)
                return RedirectToAction("Index", "Home");
            return View(new StaffLoginViewModel());
        }

        // 处理登录提交
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string role, string username, string password)
        {
            // 调用BLL验证登录
            var (errorMsg, user) = _staffBLL.Login(role, username, password);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                // 登录失败，返回错误信息
                ModelState.AddModelError("", errorMsg);
                return View();
            }

            // 登录成功，存储用户信息到Session
            Session["StaffRole"] = role;  // 存储角色类型
            Session["LoginStaff"] = user;  // 存储用户实体
            Session.Timeout = 30;  // 30分钟过期

            // 所有角色均跳转到用户首页
            return RedirectToAction("Index", "Home");
        }

        // 退出登录
        public ActionResult Logout()
        {
            Session["StaffRole"] = null;
            Session["LoginStaff"] = null;
            return RedirectToAction("Login");
        }
    }
}