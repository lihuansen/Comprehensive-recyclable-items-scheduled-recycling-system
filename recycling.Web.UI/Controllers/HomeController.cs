using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace recycling.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Order()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 重定向到User控制器的预约页面
            return RedirectToAction("Appointment", "User");
        }
        public ActionResult Message()
        {
            return View();
        }
        public ActionResult Help()
        {
            return View();
        }
        public ActionResult Feedback()
        {
            return View();
        }
        public new ActionResult Profile()
        {
            return View();
        }
        public ActionResult LoginSelect()
        {
            return View();
        }
        /// <summary>
        /// 检查用户登录状态（AJAX调用）
        /// </summary>
        [HttpPost]
        public JsonResult CheckLoginStatus()
        {
            bool isLoggedIn = Session["LoginUser"] != null || Session["LoginStaff"] != null;

            return Json(new
            {
                isLoggedIn = isLoggedIn,
                userType = Session["LoginUser"] != null ? "user" :
                          Session["LoginStaff"] != null ? "staff" : "none"
            });
        }
    }
}