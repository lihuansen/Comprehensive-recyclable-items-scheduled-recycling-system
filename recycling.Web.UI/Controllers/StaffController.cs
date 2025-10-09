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
            // 生成验证码
            var model = new StaffLoginViewModel
            {
                GeneratedCaptcha = GenerateCaptcha()
            };
            return View(model);
        }

        private string GenerateCaptcha()
        {
            throw new NotImplementedException();
        }

        // POST: Staff/Login - 处理工作人员登录提交
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StaffLoginViewModel model)
        {
            // 验证码验证
            if (!string.Equals(model.Captcha, model.GeneratedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "验证码不正确");
                model.GeneratedCaptcha = GenerateCaptcha(); // 重新生成验证码
                return View(model);
            }

            // 模型验证
            if (!ModelState.IsValid)
            {
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 调用BLL验证登录
            var (errorMsg, user) = _staffBLL.Login(model.StaffRole, model.Username, model.Password);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ModelState.AddModelError("", errorMsg);
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 登录成功，存储用户信息到Session
            Session["StaffRole"] = model.StaffRole;
            Session["LoginStaff"] = user;
            Session.Timeout = 30;

            return RedirectToAction("Index", "Home");
        }

        // 退出登录
        public ActionResult Logout()
        {
            Session["StaffRole"] = null;
            Session["LoginStaff"] = null;
            return RedirectToAction("Login","Staff");
        }
    }
}