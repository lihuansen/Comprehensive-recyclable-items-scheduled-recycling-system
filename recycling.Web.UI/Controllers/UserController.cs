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
                return View(model);
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
                    return View(model);
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
                return View(model);
            }
        }

        /// <summary>
        /// 邮箱登录专用 - 发送验证码（真实发送到邮箱）
        /// </summary>
        [HttpPost]
        public JsonResult SendEmailLoginCode(string email)
        {
            try
            {
                // 1. 验证邮箱格式
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s]+@[^\s]+\.[^\s]+$"))
                {
                    return Json(new { success = false, message = "请输入有效的邮箱地址" });
                }

                // 2. 调用BLL层生成并发送验证码（真实发送到邮箱）
                bool isSent = _userBLL.GenerateAndSendEmailCode(email);

                // 3. 中性提示（不泄露邮箱是否注册）
                string message = isSent
                    ? "若该邮箱已注册，验证码已发送至您的邮箱（5分钟内有效）"
                    : "发送失败，请稍后重试";

                // 4. 测试环境仍返回验证码（便于调试）
                string debugCode = _userBLL.GetVerificationCodeForTest(email);  // 需要在BLL层新增一个测试用方法

                return Json(new
                {
                    success = true,  // 无论是否注册，前端都显示相同提示
                    message = message,
                    debugCode = debugCode  // 测试用，生产环境可移除
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

        /// <summary>
        /// 显示预约上门页面 - 第一步
        /// </summary>
        [HttpGet]
        public ActionResult Appointment()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1), // 默认明天
                SelectedCategories = new List<string>()
            };

            // 设置视图数据
            ViewBag.AppointmentTypes = AppointmentTypes.AllTypes;
            ViewBag.RecyclingCategories = RecyclingCategories.AllCategories;
            ViewBag.TimeSlots = TimeSlots.AllSlots;

            return View(model);
        }

        /// <summary>
        /// 处理第一步提交，跳转到品类详情页面
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Appointment(AppointmentViewModel model)
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 设置视图数据
            ViewBag.AppointmentTypes = AppointmentTypes.AllTypes;
            ViewBag.RecyclingCategories = RecyclingCategories.AllCategories;
            ViewBag.TimeSlots = TimeSlots.AllSlots;

            // 验证至少选择一个品类
            if (model.SelectedCategories == null || !model.SelectedCategories.Any())
            {
                ModelState.AddModelError("SelectedCategories", "请至少选择一个回收品类");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 创建品类详情视图模型
                var detailModel = new CategoryDetailViewModel
                {
                    BasicInfo = model
                };

                // 为每个选中的品类加载问题
                foreach (var category in model.SelectedCategories)
                {
                    var questions = RecyclingCategories.GetCategoryQuestions();
                    if (questions.ContainsKey(category))
                    {
                        detailModel.CategoryQuestions[category] = questions[category];
                    }
                }

                // 存储到Session中，用于后续步骤
                Session["AppointmentBasicInfo"] = model;
                Session["CategoryDetailModel"] = detailModel;

                return RedirectToAction("CategoryDetails");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"处理预约信息失败：{ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// 显示品类详情页面 - 第二步
        /// </summary>
        [HttpGet]
        public ActionResult CategoryDetails()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 检查是否从第一步跳转过来
            var detailModel = Session["CategoryDetailModel"] as CategoryDetailViewModel;
            if (detailModel == null)
            {
                return RedirectToAction("Appointment");
            }

            return View(detailModel);
        }

        /// <summary>
        /// 处理品类详情提交，计算预估价格
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryDetails(CategoryDetailViewModel model)
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 从Session中获取基础信息
            var basicInfo = Session["AppointmentBasicInfo"] as AppointmentViewModel;
            if (basicInfo == null)
            {
                return RedirectToAction("Appointment");
            }

            model.BasicInfo = basicInfo;

            try
            {
                // 计算预估价格
                decimal totalPrice = CalculateEstimatedPrice(model);
                model.EstimatedPrice = totalPrice;

                // 更新Session中的模型
                Session["CategoryDetailModel"] = model;

                return View("CategoryDetails", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"计算价格失败：{ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// 最终提交预约
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAppointment()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 从Session中获取完整信息
            var basicInfo = Session["AppointmentBasicInfo"] as AppointmentViewModel;
            var detailModel = Session["CategoryDetailModel"] as CategoryDetailViewModel;

            if (basicInfo == null || detailModel == null)
            {
                return RedirectToAction("Appointment");
            }

            try
            {
                // 获取当前登录用户
                var user = (Users)Session["LoginUser"];

                // 这里将来会实现预约数据的保存逻辑
                // 暂时只是模拟成功

                // 清除Session中的临时数据
                Session.Remove("AppointmentBasicInfo");
                Session.Remove("CategoryDetailModel");

                TempData["SuccessMessage"] = $"预约成功！预估价格：{detailModel.EstimatedPrice:C}。我们将在24小时内与您确认。";

                return RedirectToAction("AppointmentSuccess");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"提交预约失败：{ex.Message}");
                return View("CategoryDetails", detailModel);
            }
        }

        /// <summary>
        /// 计算预估价格
        /// </summary>
        private decimal CalculateEstimatedPrice(CategoryDetailViewModel model)
        {
            decimal totalPrice = 0m;

            foreach (var category in model.BasicInfo.SelectedCategories)
            {
                // 获取该品类的基础价格
                decimal basePrice = BasePrices.Prices[category];

                // 计算该品类的重量占比
                decimal categoryWeight = model.BasicInfo.EstimatedWeight / model.BasicInfo.SelectedCategories.Count;

                // 基础价格
                decimal categoryBasePrice = basePrice * categoryWeight;

                // 根据问题答案调整价格
                if (model.CategoryQuestions.ContainsKey(category))
                {
                    var questions = model.CategoryQuestions[category];
                    decimal adjustmentFactor = 1.0m;

                    foreach (var question in questions.Questions)
                    {
                        if (!string.IsNullOrEmpty(question.SelectedValue))
                        {
                            var selectedOption = question.Options.FirstOrDefault(o => o.Value == question.SelectedValue);
                            if (selectedOption != null)
                            {
                                adjustmentFactor *= (selectedOption.PriceEffect * question.Weight + (1 - question.Weight));
                            }
                        }
                    }

                    categoryBasePrice *= adjustmentFactor;
                }

                totalPrice += categoryBasePrice;
            }

            // 应用紧急服务加成
            if (model.BasicInfo.IsUrgent)
            {
                totalPrice *= 1.2m; // 紧急服务加价20%
            }

            return Math.Round(totalPrice, 2);
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        [HttpPost]
        public ActionResult BackToAppointment()
        {
            return RedirectToAction("Appointment");
        }

        /// <summary>
        /// 预约成功页面
        /// </summary>
        public ActionResult AppointmentSuccess()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"] ?? "预约提交成功！";
            return View();
        }
    }
}