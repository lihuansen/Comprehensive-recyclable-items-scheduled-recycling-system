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

        /// <summary>
        /// 显示密码登录页面（主登录界面）
        /// </summary>
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }
        // POST: User/Login - 处理密码登录
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
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

        /// <summary>
        /// 显示手机号登录页面
        /// </summary>
        [HttpGet]
        public ActionResult PhoneLogin()
        {
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new PhoneLoginViewModel());
        }

        /// <summary>
        /// 处理手机号登录提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PhoneLogin(PhoneLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 调用BLL层验证手机号登录
                var (errorMsg, user) = _userBLL.PhoneLogin(model.PhoneNumber, model.VerificationCode);
                if (errorMsg != null)
                {
                    // 验证码错误时清空验证码输入
                    if (errorMsg.Contains("验证码"))
                    {
                        model.VerificationCode = "";
                    }
                    ModelState.AddModelError("", errorMsg);
                    return View(model);
                }

                // 登录成功，更新最后登录时间
                _userBLL.UpdateLastLoginDate(user.UserID);

                // 存储会话信息
                Session["LoginUser"] = user;
                Session.Timeout = 30;

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "登录失败：" + ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// 手机号登录专用 - 发送验证码
        /// </summary>
        [HttpPost]
        public JsonResult SendLoginCode(string phoneNumber)
        {
            try
            {
                // 验证手机号格式
                if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^1[3-9]\d{9}$"))
                {
                    return Json(new { success = false, message = "请输入有效的11位手机号" });
                }

                // 检查手机号是否存在（中性提示）
                bool isRegistered = _userBLL.IsPhoneExists(phoneNumber);
                string code = isRegistered ? _userBLL.GenerateVerificationCode(phoneNumber) : "";

                return Json(new
                {
                    success = true,
                    message = "若该手机号已注册，验证码已生成（5分钟内有效）",
                    debugCode = code // 测试环境显示验证码
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "发送验证码失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 显示邮箱登录页面
        /// </summary>
        [HttpGet]
        public ActionResult EmailLogin()
        {
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new EmailLoginViewModel());
        }

        /// <summary>
        /// 处理邮箱登录提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailLogin(EmailLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            try
            {
                // 调用BLL层验证邮箱登录
                var (errorMsg, user) = _userBLL.EmailLogin(model.Email, model.VerificationCode);
                if (errorMsg != null)
                {
                    if (errorMsg.Contains("验证码"))
                    {
                        model.VerificationCode = "";
                    }
                    ModelState.AddModelError("", errorMsg);
                    return View("Login", model);
                }

                // 更新最后登录时间
                _userBLL.UpdateLastLoginDate(user.UserID);

                // 存储会话
                Session["LoginUser"] = user;
                Session.Timeout = 30;

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "登录失败：" + ex.Message);
                return View("Login", model);
            }
        }

        /// <summary>
        /// 邮箱登录专用 - 发送验证码
        /// </summary>
        [HttpPost]
        public JsonResult SendEmailLoginCode(string email)
        {
            try
            {
                // 验证邮箱格式
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s]+@[^\s]+\.[^\s]+$"))
                {
                    return Json(new { success = false, message = "请输入有效的邮箱地址" });
                }

                // 检查邮箱是否注册（中性提示）
                bool isRegistered = _userBLL.IsEmailExists(email); // 需要在BLL层实现IsEmailExists方法
                string code = isRegistered ? _userBLL.GenerateEmailVerificationCode(email) : "";

                return Json(new
                {
                    success = true,
                    message = "若该邮箱已注册，验证码已生成（5分钟内有效）",
                    debugCode = code // 测试环境显示验证码
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "发送验证码失败：" + ex.Message });
            }
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
                    return Json(new { success = false, message = "请输入有效的11位手机号" });
                }

                // 检查手机号是否存在（但不泄露结果）
                bool isRegistered = _userBLL.IsPhoneExists(phoneNumber);
                string code = isRegistered ? _userBLL.GenerateVerificationCode(phoneNumber) : "";

                // 中性提示语，不泄露手机号是否注册
                return Json(new
                {
                    success = true,
                    message = "若该手机号已注册，验证码已生成（5分钟内有效）",
                    debugCode = code // 测试环境显示验证码
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "生成验证码失败：" + ex.Message });
            }
        }


        /// <summary>
        /// 处理密码重置提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Forgot", model);
            }

            try
            {
                // 1. 验证验证码
                bool isCodeValid = _userBLL.VerifyVerificationCode(model.PhoneNumber, model.VerificationCode);
                if (!isCodeValid)
                {
                    ModelState.AddModelError("VerificationCode", "验证码不正确或已过期");
                    return View("Forgot", model);
                }

                // 2. 执行密码重置（包含与原密码比对）
                string errorMessage = _userBLL.ResetUserPassword(model.PhoneNumber, model.NewPassword);
                if (errorMessage != null)
                {
                    ModelState.AddModelError("", errorMessage);
                    return View("Forgot", model);
                }

                // 3. 重置成功，跳转登录页并提示
                TempData["SuccessMessage"] = "密码已成功重置，请使用新密码登录";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "操作失败：" + ex.Message);
                return View("Forgot", model);
            }
        }
    }
}