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
        private readonly UserBLL _userBLL = new UserBLL();
        private readonly UserNotificationBLL _notificationBLL = new UserNotificationBLL();
        private readonly UserAddressBLL _addressBLL = new UserAddressBLL();
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

            var user = (Users)Session["LoginUser"];

            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1), // 默认明天
                SelectedCategories = new List<string>()
            };

            // 获取用户的地址列表
            var userAddresses = _addressBLL.GetUserAddresses(user.UserID);
            ViewBag.UserAddresses = userAddresses;

            // 获取默认地址ID
            var defaultAddress = userAddresses.FirstOrDefault(a => a.IsDefault);
            if (defaultAddress != null)
            {
                model.SelectedAddressID = defaultAddress.AddressID;
            }

            // 设置视图数据
            ViewBag.AppointmentTypes = AppointmentTypes.AllTypes;
            ViewBag.RecyclingCategories = RecyclingCategories.AllCategories;
            ViewBag.TimeSlots = TimeSlots.AllSlots;
            ViewBag.Streets = Streets.LuohuStreets;

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

            var user = (Users)Session["LoginUser"];

            // 获取用户的地址列表并设置到ViewBag
            var userAddresses = _addressBLL.GetUserAddresses(user.UserID);
            ViewBag.UserAddresses = userAddresses;

            // 设置视图数据
            ViewBag.AppointmentTypes = AppointmentTypes.AllTypes;
            ViewBag.RecyclingCategories = RecyclingCategories.AllCategories;
            ViewBag.TimeSlots = TimeSlots.AllSlots;
            ViewBag.Streets = Streets.LuohuStreets;

            // 验证至少选择一个品类
            if (model.SelectedCategories == null || !model.SelectedCategories.Any())
            {
                ModelState.AddModelError("SelectedCategories", "请至少选择一个回收品类");
            }

            // 处理地址信息：从选择的地址或手动输入获取
            if (model.SelectedAddressID.HasValue && model.SelectedAddressID.Value > 0)
            {
                // 从用户地址管理中获取地址信息
                var selectedAddress = _addressBLL.GetAddressById(model.SelectedAddressID.Value, user.UserID);
                if (selectedAddress != null)
                {
                    model.ContactName = selectedAddress.ContactName;
                    model.ContactPhone = selectedAddress.ContactPhone;
                    model.Street = selectedAddress.Street;
                    
                    // 构建完整地址，将街道键转换为显示名称
                    string streetDisplayName = selectedAddress.Street;
                    if (!string.IsNullOrEmpty(selectedAddress.Street) && Streets.LuohuStreets.ContainsKey(selectedAddress.Street))
                    {
                        streetDisplayName = Streets.LuohuStreets[selectedAddress.Street];
                    }
                    model.Address = $"{selectedAddress.Province}{selectedAddress.City}{selectedAddress.District}{streetDisplayName}{selectedAddress.DetailAddress}";
                }
                else
                {
                    ModelState.AddModelError("SelectedAddressID", "所选地址不存在，请重新选择");
                }
            }
            else
            {
                // 手动输入地址时验证必填字段
                if (string.IsNullOrWhiteSpace(model.ContactName))
                {
                    ModelState.AddModelError("ContactName", "请输入联系人姓名");
                }
                if (string.IsNullOrWhiteSpace(model.ContactPhone))
                {
                    ModelState.AddModelError("ContactPhone", "请输入联系电话");
                }
                if (string.IsNullOrWhiteSpace(model.Street))
                {
                    ModelState.AddModelError("Street", "请选择街道");
                }
                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    ModelState.AddModelError("Address", "请输入详细地址");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 如果是手动输入地址，拼接完整地址：省 + 市 + 区 + 街道 + 详细地址
                if (!model.SelectedAddressID.HasValue || model.SelectedAddressID.Value == 0)
                {
                    string streetName = "";
                    if (!string.IsNullOrEmpty(model.Street) && Streets.LuohuStreets.ContainsKey(model.Street))
                    {
                        streetName = Streets.LuohuStreets[model.Street];
                    }
                    else if (!string.IsNullOrEmpty(model.Street))
                    {
                        streetName = model.Street;
                    }
                    string fullAddress = $"广东省深圳市罗湖区{streetName}{model.Address}";
                    model.Address = fullAddress;
                }

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
        public ActionResult CategoryDetails(FormCollection form)
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 从Session中获取基础信息和原始模型
            var basicInfo = Session["AppointmentBasicInfo"] as AppointmentViewModel;
            var originalDetailModel = Session["CategoryDetailModel"] as CategoryDetailViewModel;

            if (basicInfo == null || originalDetailModel == null)
            {
                return RedirectToAction("Appointment");
            }

            try
            {
                // 创建新的详情模型
                var model = new CategoryDetailViewModel
                {
                    BasicInfo = basicInfo,
                    CategoryQuestions = new Dictionary<string, CategoryQuestions>()
                };

                // 从FormCollection中提取用户选择的答案
                foreach (var categoryEntry in originalDetailModel.CategoryQuestions)
                {
                    var categoryKey = categoryEntry.Key;
                    var originalQuestions = categoryEntry.Value;
                    var updatedQuestions = new CategoryQuestions
                    {
                        CategoryName = originalQuestions.CategoryName,
                        Questions = new List<Question>()
                    };

                    // 复制原始问题结构，但更新用户选择的答案
                    foreach (var originalQuestion in originalQuestions.Questions)
                    {
                        var questionKey = $"CategoryQuestions[{categoryKey}].Questions[{originalQuestions.Questions.IndexOf(originalQuestion)}].SelectedValue";
                        var selectedValue = form[questionKey];

                        updatedQuestions.Questions.Add(new Question
                        {
                            Id = originalQuestion.Id,
                            Text = originalQuestion.Text,
                            Type = originalQuestion.Type,
                            Options = originalQuestion.Options,
                            SelectedValue = selectedValue,
                            Weight = originalQuestion.Weight
                        });
                    }

                    model.CategoryQuestions[categoryKey] = updatedQuestions;
                }

                // 计算预估价格
                decimal totalPrice = CalculateEstimatedPrice(model);
                model.EstimatedPrice = totalPrice;

                // 更新Session中的模型（包含用户选择的答案）
                Session["CategoryDetailModel"] = model;

                return View("CategoryDetails", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"计算价格失败：{ex.Message}");
                return View("CategoryDetails", originalDetailModel);
            }
        }

        /// <summary>
        /// 最终提交预约（修正版本）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAppointment(FormCollection form)
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
                TempData["ErrorMessage"] = "预约信息已过期，请重新填写";
                return RedirectToAction("Appointment");
            }

            try
            {
                // 获取当前登录用户
                var user = (Users)Session["LoginUser"];

                // 从表单中获取最终价格
                var finalPriceValue = form["FinalEstimatedPrice"];
                decimal finalPrice = 0m;
                if (!string.IsNullOrEmpty(finalPriceValue) && decimal.TryParse(finalPriceValue, out decimal parsedPrice))
                {
                    finalPrice = parsedPrice;
                }
                else
                {
                    finalPrice = detailModel.EstimatedPrice;
                }

                // 从表单中提取品类答案
                var categoryAnswers = new Dictionary<string, Dictionary<string, string>>();

                foreach (var categoryEntry in detailModel.CategoryQuestions)
                {
                    var categoryKey = categoryEntry.Key;
                    var answers = new Dictionary<string, string>();

                    foreach (var question in categoryEntry.Value.Questions)
                    {
                        var answerKey = $"CategoryAnswers[{categoryKey}].{question.Id}";
                        var selectedValue = form[answerKey];

                        if (!string.IsNullOrEmpty(selectedValue))
                        {
                            answers[question.Id] = selectedValue;
                        }
                    }

                    if (answers.Count > 0)
                    {
                        categoryAnswers[categoryKey] = answers;
                    }
                }

                // 创建BLL实例
                var appointmentBLL = new AppointmentBLL();

                // 准备提交数据
                var submission = new AppointmentSubmissionModel
                {
                    BasicInfo = basicInfo,
                    CategoryAnswers = categoryAnswers,
                    FinalPrice = finalPrice
                };

                // 提交到数据库
                var (success, appointmentId, errorMessage) = appointmentBLL.SubmitAppointment(submission, user.UserID);

                if (!success)
                {
                    TempData["ErrorMessage"] = errorMessage;
                    return View("CategoryDetails", detailModel);
                }

                // 发送订单创建通知
                _notificationBLL.SendOrderCreatedNotification(user.UserID, appointmentId);

                // 清除Session中的临时数据
                Session.Remove("AppointmentBasicInfo");
                Session.Remove("CategoryDetailModel");

                TempData["SuccessMessage"] = $"预约成功！预约号：#AP{appointmentId:D6}，预估价格：¥{finalPrice:F2}。我们将在24小时内与您确认。";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"提交预约失败：{ex.Message}";
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

        /// <summary>
        /// 获取用户地址列表（JSON API）
        /// </summary>
        [HttpGet]
        public JsonResult GetUserAddresses()
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var addresses = _addressBLL.GetUserAddresses(user.UserID);
                var result = addresses.Select(a => {
                    // 将街道键转换为显示名称以构建完整地址
                    string streetDisplayName = a.Street;
                    if (!string.IsNullOrEmpty(a.Street) && Streets.LuohuStreets.ContainsKey(a.Street))
                    {
                        streetDisplayName = Streets.LuohuStreets[a.Street];
                    }
                    return new
                    {
                        addressId = a.AddressID,
                        contactName = a.ContactName,
                        contactPhone = a.ContactPhone,
                        province = a.Province,
                        city = a.City,
                        district = a.District,
                        street = a.Street,
                        detailAddress = a.DetailAddress,
                        fullAddress = $"{a.Province}{a.City}{a.District}{streetDisplayName}{a.DetailAddress}",
                        isDefault = a.IsDefault
                    };
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "获取地址列表失败：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取单个地址详情（JSON API）
        /// </summary>
        [HttpGet]
        public JsonResult GetAddressById(int addressId)
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var address = _addressBLL.GetAddressById(addressId, user.UserID);
                if (address == null)
                {
                    return Json(new { success = false, message = "地址不存在" }, JsonRequestBehavior.AllowGet);
                }

                // 将街道键转换为显示名称以构建完整地址
                string streetDisplayName = address.Street;
                if (!string.IsNullOrEmpty(address.Street) && Streets.LuohuStreets.ContainsKey(address.Street))
                {
                    streetDisplayName = Streets.LuohuStreets[address.Street];
                }
                string fullAddress = $"{address.Province}{address.City}{address.District}{streetDisplayName}{address.DetailAddress}";

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        addressId = address.AddressID,
                        contactName = address.ContactName,
                        contactPhone = address.ContactPhone,
                        province = address.Province,
                        city = address.City,
                        district = address.District,
                        street = address.Street,
                        detailAddress = address.DetailAddress,
                        fullAddress = fullAddress,
                        isDefault = address.IsDefault
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "获取地址详情失败：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 在预约过程中新增地址（JSON API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddAddressForAppointment(string street, string detailAddress, string contactName, string contactPhone, bool isDefault = false)
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" });
            }

            try
            {
                var user = (Users)Session["LoginUser"];

                var newAddress = new UserAddresses
                {
                    UserID = user.UserID,
                    Street = street,
                    DetailAddress = detailAddress,
                    ContactName = contactName,
                    ContactPhone = contactPhone,
                    IsDefault = isDefault
                };

                var (success, message, addressId) = _addressBLL.AddAddress(newAddress);

                if (success)
                {
                    // 获取新添加的地址完整信息
                    var address = _addressBLL.GetAddressById(addressId, user.UserID);
                    
                    // 将街道键转换为显示名称以构建完整地址
                    string streetDisplayName = address.Street;
                    if (!string.IsNullOrEmpty(address.Street) && Streets.LuohuStreets.ContainsKey(address.Street))
                    {
                        streetDisplayName = Streets.LuohuStreets[address.Street];
                    }
                    string fullAddress = $"{address.Province}{address.City}{address.District}{streetDisplayName}{address.DetailAddress}";

                    return Json(new
                    {
                        success = true,
                        message = message,
                        data = new
                        {
                            addressId = address.AddressID,
                            contactName = address.ContactName,
                            contactPhone = address.ContactPhone,
                            province = address.Province,
                            city = address.City,
                            district = address.District,
                            street = address.Street,
                            detailAddress = address.DetailAddress,
                            fullAddress = fullAddress,
                            isDefault = address.IsDefault
                        }
                    });
                }

                return Json(new { success = false, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "添加地址失败：" + ex.Message });
            }
        }
    }
}