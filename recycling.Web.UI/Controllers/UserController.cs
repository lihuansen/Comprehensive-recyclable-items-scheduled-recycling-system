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
        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            // 1. 先执行前端验证（模型绑定验证，如必填、格式）
            if (!ModelState.IsValid)
            {
                // 遍历错误字段，清空对应的值
                foreach (var key in ModelState.Keys.ToList())
                {
                    if (ModelState[key].Errors.Count > 0)
                    {
                        // 根据字段名清空错误值
                        switch (key)
                        {
                            case "Username":
                                model.Username = "";
                                break;
                            case "Password":
                                model.Password = "";
                                break;
                            case "ConfirmPassword":
                                model.ConfirmPassword = "";
                                break;
                            case "PhoneNumber":
                                model.PhoneNumber = "";
                                break;
                            case "Email":
                                model.Email = "";
                                break;
                        }
                    }
                }
                return View(model); // 回传清空错误字段后的模型
            }

            // 2. 执行BLL层业务验证（按优先级）
            string errorMsg = _userBLL.Register(model);
            if (errorMsg != null)
            {
                // 根据错误信息，判断哪个字段出错，仅清空对应字段
                if (errorMsg.Contains("用户名"))
                {
                    model.Username = ""; // 用户名错误，清空用户名
                }
                else if (errorMsg.Contains("密码"))
                {
                    model.Password = ""; // 密码错误，清空密码和确认密码
                    model.ConfirmPassword = "";
                }
                else if (errorMsg.Contains("手机号"))
                {
                    model.PhoneNumber = ""; // 手机号错误，清空手机号
                }
                else if (errorMsg.Contains("邮箱"))
                {
                    model.Email = ""; // 邮箱错误，清空邮箱
                }

                ModelState.AddModelError("", errorMsg);
                return View(model); // 回传清空错误字段后的模型
            }

            // 3. 注册成功：传递状态给视图，不直接跳转
            ViewBag.RegisterSuccess = true; // 标记注册成功
            return View(model);
        }

        public ActionResult Forgot()
        {
            return View();
        }
    }
}