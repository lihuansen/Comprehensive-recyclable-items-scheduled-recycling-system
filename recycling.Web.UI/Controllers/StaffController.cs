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
            return View(new StaffLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StaffLoginViewModel model)
        {
            // 1. 模型验证
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. 验证码验证
            if (!string.Equals(model.Captcha?.Trim().ToUpper(), model.GeneratedCaptcha?.Trim().ToUpper()))
            {
                ModelState.AddModelError(nameof(model.Captcha), "验证码错误");
                return View(model);
            }

            // 3. 调用BLL验证登录
            var (errorMsg, user) = _staffBLL.Login(model.StaffRole, model.Username, model.Password);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                ModelState.AddModelError("", errorMsg);
                return View(model);
            }

            // 4. 登录成功，存储用户信息
            Session["StaffRole"] = model.StaffRole;
            Session["LoginStaff"] = user;
            Session.Timeout = 30;

            return RedirectToAction("Index", "Home");
        }


        // 退出登录
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}