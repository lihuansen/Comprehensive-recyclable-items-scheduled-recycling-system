using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using recycling.BLL;
using recycling.Model;

namespace recycling.Web.UI.Controllers
{
    public class UserController : Controller
    {
        private UserBLL _userBLL = new UserBLL();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            ViewBag.LoginType = "password";
            return View();
        }
        public ActionResult PhoneLogin()
        {
            ViewBag.LoginType = "phone";
            return View("Login"); 
        }
        public ActionResult EmailLogin()
        {
            ViewBag.LoginType = "email";
            return View("Login");
        }   
        public ActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 调用BLL层进行注册验证和处理
                string errorMessage = _userBLL.Register(model);

                if (string.IsNullOrEmpty(errorMessage))
                {
                    // 注册成功，跳转到登录页并显示成功信息
                    TempData["SuccessMessage"] = "注册成功，请登录";
                    return RedirectToAction("Login");
                }
                else
                {
                    // 注册失败，显示错误信息（按优先级）
                    ModelState.AddModelError("", errorMessage);
                }
            }

            // 验证失败，返回注册页面
            return View(model);
        }

        public ActionResult Forgot()
        {
            return View();
        }
    }
}