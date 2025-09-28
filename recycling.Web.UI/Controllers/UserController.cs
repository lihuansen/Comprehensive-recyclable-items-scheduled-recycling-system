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
        [HttpGet]
        public ActionResult Login()
        {
            //如果已登录，直接跳转到首页
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.LoginType = "password";
            return View(new LoginViewModel());
        }
        // POST: User/Login - 处理密码登录
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            ViewBag.LoginType = "password";

            // 模型验证
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 调用BLL层验证登录
            string errorMsg = _userBLL.Login(model);
            if (errorMsg != null)
            {
                // 根据错误类型清空对应字段
                if (errorMsg.Contains("验证码"))
                {
                    model.Captcha = ""; // 验证码错误，清空验证码
                }
                else if (errorMsg.Contains("密码"))
                {
                    model.Password = ""; // 密码错误，清空密码
                }

                ModelState.AddModelError("", errorMsg);
                return View(model);
            }

            // 登录成功，通过BLL层获取用户信息（不再直接调用DAL层）
            Users user = _userBLL.GetUserByUsername(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "登录异常，请重试");
                return View(model);
            }
            // 关键：更新最后登录时间（新增代码）
            try
            {
                _userBLL.UpdateLastLoginDate(user.UserID); // 使用UserID字段（与Model层一致）
            }
            catch (Exception ex)
            {
                // 捕获更新失败的异常，不阻断登录但记录错误
                ModelState.AddModelError("", "登录成功，但更新登录记录失败：" + ex.Message);
                // 仍允许登录成功，仅提示异常
            }

            // 存储会话信息
            Session["LoginUser"] = user;
            Session.Timeout = 30;

            return RedirectToAction("Index", "Home");
        }
        // 退出登录
        public ActionResult Logout()
        {
            Session.Clear(); // 清除所有会话数据
            Session.Abandon(); // 终止当前会话
            return RedirectToAction("Index", "Home");
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

            // 3. 注册成功：直接跳转到登录界面（移除停留逻辑）
            return RedirectToAction("Login");
        }

        /// <summary>
        /// 显示忘记密码页面
        /// </summary>
        public ActionResult Forgot()
        {
            return View(new ForgotPasswordViewModel());
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        [HttpPost]
        public JsonResult SendVerificationCode(string phoneNumber)
        {
            try
            {
                // 验证手机号格式
                if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^1[3-9]\d{9}$"))
                {
                    return Json(new { success = false, message = "请输入正确的手机号格式" });
                }
                
                // 检查手机号是否存在（但返回中性提示）
                bool isExists = _userBLL.IsPhoneExists(phoneNumber);
                
                // 无论手机号是否存在，都生成验证码（实际发送只对存在的手机号）
                string code = _userBLL.GenerateVerificationCode(phoneNumber);
                
                // 模拟发送验证码，实际项目中应调用短信服务API
                if (isExists)
                {
                    // 这里只是模拟发送，实际项目中需要集成短信服务
                    System.Diagnostics.Debug.WriteLine($"向手机号 {phoneNumber} 发送验证码：{code}");
                }
                
                // 返回中性提示，不泄露手机号是否存在
                return Json(new { success = true, message = "验证码已发送，请注意查收（有效期5分钟）" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "发送验证码失败，请稍后重试",ex });
            }
        }
        
        /// <summary>
        /// 处理密码重置
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Forgot", model);
            }
            
            string errorMsg = _userBLL.ResetPassword(model);
            if (errorMsg != null)
            {
                ModelState.AddModelError("", errorMsg);
                return View("Forgot", model);
            }
            
            // 密码重置成功，跳转到登录页并显示提示
            TempData["Message"] = "密码重置成功，请使用新密码登录";
            return RedirectToAction("Login");
        }
    }
}