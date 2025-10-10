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

        /// <summary>
        /// 显示工作人员登录页
        /// </summary>
        [HttpGet]
        public ActionResult Login()
        {
            // 已登录则跳转首页
            if (Session["LoginStaff"] != null)
                return RedirectToAction("Index", "Home");

            // 生成验证码并传递到视图
            var model = new StaffLoginViewModel
            {
                GeneratedCaptcha = GenerateCaptcha()
            };
            return View(model);
        }

        /// <summary>
        /// 处理工作人员登录提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StaffLoginViewModel model)
        {
            // 1. 验证码验证
            if (!string.Equals(model.Captcha, model.GeneratedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "验证码不正确");
                model.GeneratedCaptcha = GenerateCaptcha(); // 重新生成验证码
                return View(model);
            }

            // 2. 模型验证
            if (!ModelState.IsValid)
            {
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 3. 调用BLL验证登录
            var (errorMsg, staff) = _staffBLL.Login(model.StaffRole, model.Username, model.Password);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                ModelState.AddModelError("", errorMsg);
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 4. 登录成功，存储Session
            Session["LoginStaff"] = staff; // 存储工作人员实体
            Session["StaffRole"] = model.StaffRole; // 存储角色（用于权限判断）
            Session.Timeout = 30; // 30分钟过期

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 工作人员退出登录
        /// </summary>
        public ActionResult Logout()
        {
            Session["LoginStaff"] = null;
            Session["StaffRole"] = null;
            return RedirectToAction("Login", "Staff");
        }

        /// <summary>
        /// 生成4位随机验证码（与User登录保持一致）
        /// </summary>
        private string GenerateCaptcha()
        {
            var random = new Random();
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}