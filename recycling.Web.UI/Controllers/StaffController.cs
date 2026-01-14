using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using recycling.BLL;
using recycling.Model;
using Newtonsoft.Json;
using System.IO;
using recycling.Web.UI.Filters;

namespace recycling.Web.UI.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffBLL _staffBLL = new StaffBLL();
        private readonly RecyclerOrderBLL _recyclerOrderBLL = new RecyclerOrderBLL();
        private readonly MessageBLL _messageBLL = new MessageBLL();
        private readonly OrderBLL _orderBLL = new OrderBLL();
        private readonly AdminBLL _adminBLL = new AdminBLL();
        private readonly SuperAdminBLL _superAdminBLL = new SuperAdminBLL();
        private readonly HomepageCarouselBLL _carouselBLL = new HomepageCarouselBLL();
        private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();
        private readonly FeedbackBLL _feedbackBLL = new FeedbackBLL();
        private readonly OperationLogBLL _operationLogBLL = new OperationLogBLL();
        private readonly UserNotificationBLL _notificationBLL = new UserNotificationBLL();
        private readonly TransportationOrderBLL _transportationOrderBLL = new TransportationOrderBLL();
        private readonly WarehouseReceiptBLL _warehouseReceiptBLL = new WarehouseReceiptBLL();

        // File upload constants
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".ogg" };

        /// <summary>
        /// Helper method to return JSON with proper UTF-8 encoding
        /// </summary>
        private ContentResult JsonContent(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return Content(json, "application/json", System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Helper method to escape CSV fields (handles commas, quotes, newlines)
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // If field contains comma, quote, or newline, wrap it in quotes and escape any quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        private string GetClientIpAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            return ipAddress ?? "Unknown";
        }

        /// <summary>
        /// 获取当前管理员信息
        /// </summary>
        private (int AdminID, string Username) GetCurrentAdmin()
        {
            if (Session["LoginStaff"] == null)
                return (0, null);

            var staffRole = Session["StaffRole"] as string;
            if (staffRole == "admin")
            {
                var admin = (Admins)Session["LoginStaff"];
                return (admin.AdminID, admin.Username);
            }
            else if (staffRole == "superadmin")
            {
                var superAdmin = (SuperAdmins)Session["LoginStaff"];
                return (superAdmin.SuperAdminID, superAdmin.Username);
            }

            return (0, null);
        }

        /// <summary>
        /// 记录管理员操作日志
        /// </summary>
        private void LogAdminOperation(string module, string operationType, string description, int? targetId = null, string targetName = null, string result = "Success", string details = null)
        {
            var (adminId, adminUsername) = GetCurrentAdmin();
            if (adminId > 0)
            {
                _operationLogBLL.LogOperation(adminId, adminUsername, module, operationType, description, targetId, targetName, GetClientIpAddress(), result, details);
            }
        }

        /// <summary>
        /// 工作人员首页 - 重定向到专用首页
        /// </summary>
        public ActionResult Index()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            return RedirectToAction("StaffIndex", "Home");
        }

        /// <summary>
        /// 显示工作人员登录页（与用户登录页逻辑一致）
        /// </summary>
        [HttpGet]
        public ActionResult Login()
        {
            // 已登录则跳转首页（与用户登录逻辑一致）
            if (Session["LoginStaff"] != null)
                return RedirectToAction("StaffIndex", "Home");

            // 生成验证码并传递到视图（与用户登录逻辑一致）
            var model = new StaffLoginViewModel
            {
                GeneratedCaptcha = GenerateCaptcha()
            };
            return View(model);
        }

        /// <summary>
        /// 处理工作人员登录提交（与用户登录处理逻辑一致）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StaffLoginViewModel model)
        {
            // 1. 验证码验证（与用户登录逻辑一致）
            if (string.IsNullOrEmpty(model.GeneratedCaptcha) ||
                !string.Equals(model.Captcha, model.GeneratedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "验证码不正确");
                model.GeneratedCaptcha = GenerateCaptcha(); // 重新生成验证码
                model.Captcha = ""; // 清空错误的验证码输入
                return View(model);
            }

            // 2. 模型验证
            if (!ModelState.IsValid)
            {
                // 清空错误的密码输入（与用户登录逻辑一致）
                if (ModelState.Keys.Any(k => k == "Password" && ModelState[k].Errors.Count > 0))
                {
                    model.Password = "";
                }
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 3. 调用BLL验证登录
            var (errorMsg, staff) = _staffBLL.Login(model.StaffRole, model.Username, model.Password);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                ModelState.AddModelError("", errorMsg);
                // 根据错误类型清空对应字段（与用户登录逻辑一致）
                if (errorMsg.Contains("密码"))
                {
                    model.Password = "";
                }
                // 无论哪种错误，都需要清空验证码输入并生成新验证码
                model.Captcha = "";
                model.GeneratedCaptcha = GenerateCaptcha();
                return View(model);
            }

            // 4. 登录成功，存储Session（与用户登录逻辑一致）
            Session["LoginStaff"] = staff;
            Session["StaffRole"] = model.StaffRole;
            Session.Timeout = 30;

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 工作人员退出登录（与用户退出逻辑一致）
        /// </summary>
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Staff");
        }

        /// <summary>
        /// 生成4位随机验证码（与User登录保持完全一致）
        /// </summary>
        private string GenerateCaptcha()
        {
            // 使用与用户登录完全相同的字符集和生成逻辑
            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 移除易混淆字符
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 回收员工作台
        /// </summary>
        public ActionResult RecyclerDashboard()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
                return RedirectToAction("Login", "Staff");

            var recycler = (Recyclers)Session["LoginStaff"];
            ViewBag.StaffName = recycler.Username;
            ViewBag.DisplayName = "回收员";
            ViewBag.StaffRole = "recycler";

            return View();
        }

        /// <summary>
        /// 管理员工作台
        /// </summary>
        public ActionResult AdminDashboard()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "admin")
                return RedirectToAction("Login", "Staff");

            var admin = (Admins)Session["LoginStaff"];
            ViewBag.StaffName = admin.Username;
            ViewBag.DisplayName = "管理员";
            ViewBag.StaffRole = "admin";

            return View();
        }

        /// <summary>
        /// 管理员账号自我管理（无权限限制）
        /// </summary>
        public ActionResult AccountSelfManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "admin")
                return RedirectToAction("Login", "Staff");

            var admin = (Admins)Session["LoginStaff"];
            ViewBag.StaffName = admin.Username;
            ViewBag.DisplayName = "管理员";
            ViewBag.StaffRole = "admin";

            return View();
        }

        /// <summary>
        /// 管理员/超级管理员 - 获取自己的账号信息
        /// </summary>
        [HttpGet]
        public ContentResult GetSelfAccountInfo()
        {
            if (Session["LoginStaff"] == null)
            {
                return JsonContent(new { success = false, message = "未登录" });
            }

            string role = Session["StaffRole"] as string;

            try
            {
                // 超级管理员
                if (role == "superadmin")
                {
                    var currentSuperAdmin = (SuperAdmins)Session["LoginStaff"];
                    var superAdmin = _superAdminBLL.GetSuperAdminById(currentSuperAdmin.SuperAdminID);
                    return JsonContent(new {
                        success = true,
                        data = new {
                            superAdminId = superAdmin.SuperAdminID,
                            username = superAdmin.Username,
                            fullName = superAdmin.FullName,
                            isActive = superAdmin.IsActive,
                            createdAt = superAdmin.CreatedDate,
                            lastLogin = superAdmin.LastLoginDate
                        }
                    });
                }
                // 管理员
                else if (role == "admin")
                {
                    var currentAdmin = (Admins)Session["LoginStaff"];
                    var admin = _adminBLL.GetAdminById(currentAdmin.AdminID);
                    return JsonContent(new {
                        success = true,
                        data = new {
                            adminId = admin.AdminID,
                            username = admin.Username,
                            fullName = admin.FullName,
                            isActive = admin.IsActive,
                            createdAt = admin.CreatedDate,
                            lastLogin = admin.LastLoginDate
                        }
                    });
                }
                else
                {
                    return JsonContent(new { success = false, message = "无权限" });
                }
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员/超级管理员 - 更新自己的账号信息（仅限本人操作）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateSelfAccount(string fullName, string oldPassword, string newPassword)
        {
            if (Session["LoginStaff"] == null)
            {
                return Json(new { success = false, message = "未登录" });
            }

            string role = Session["StaffRole"] as string;

            try
            {
                // 超级管理员
                if (role == "superadmin")
                {
                    var currentSuperAdmin = (SuperAdmins)Session["LoginStaff"];
                    var superAdmin = _superAdminBLL.GetSuperAdminById(currentSuperAdmin.SuperAdminID);

                    // 验证旧密码（如果要修改密码）
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        if (string.IsNullOrEmpty(oldPassword))
                        {
                            return Json(new { success = false, message = "请输入旧密码" });
                        }

                        // 验证旧密码 - 使用与SuperAdminBLL相同的哈希方法
                        string encryptedOldPassword = HashPasswordSHA256(oldPassword);
                        if (superAdmin.PasswordHash != encryptedOldPassword)
                        {
                            return Json(new { success = false, message = "旧密码错误" });
                        }

                        // 更新密码
                        superAdmin.PasswordHash = HashPasswordSHA256(newPassword);
                    }

                    // 更新姓名
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        superAdmin.FullName = fullName;
                    }

                    var result = _superAdminBLL.UpdateSuperAdmin(superAdmin);

                    // 更新 Session 中的超级管理员信息
                    if (result.Success)
                    {
                        Session["LoginStaff"] = superAdmin;
                    }

                    // 记录操作日志
                    LogAdminOperation(OperationLogBLL.Modules.AccountManagement, OperationLogBLL.OperationTypes.Update, 
                        $"超级管理员更新自己的账号信息", superAdmin.SuperAdminID, superAdmin.Username, result.Success ? "Success" : "Failed");

                    return Json(new { success = result.Success, message = result.Message });
                }
                // 管理员
                else if (role == "admin")
                {
                    var currentAdmin = (Admins)Session["LoginStaff"];
                    var admin = _adminBLL.GetAdminById(currentAdmin.AdminID);

                    // 验证旧密码（如果要修改密码）
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        if (string.IsNullOrEmpty(oldPassword))
                        {
                            return Json(new { success = false, message = "请输入旧密码" });
                        }

                        // 验证旧密码 - 使用与AdminBLL相同的哈希方法
                        string encryptedOldPassword = HashPasswordSHA256(oldPassword);
                        if (admin.PasswordHash != encryptedOldPassword)
                        {
                            return Json(new { success = false, message = "旧密码错误" });
                        }

                        // 更新密码
                        admin.PasswordHash = HashPasswordSHA256(newPassword);
                    }

                    // 更新姓名
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        admin.FullName = fullName;
                    }

                    var result = _adminBLL.UpdateAdmin(admin);

                    // 更新 Session 中的管理员信息
                    if (result.Success)
                    {
                        Session["LoginStaff"] = admin;
                    }

                    // 记录操作日志
                    LogAdminOperation(OperationLogBLL.Modules.AccountManagement, OperationLogBLL.OperationTypes.Update, 
                        $"更新自己的账号信息", admin.AdminID, admin.Username, result.Success ? "Success" : "Failed");

                    return Json(new { success = result.Success, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = "无权限" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Hash password using SHA256 (same as AdminBLL)
        /// </summary>
        private string HashPasswordSHA256(string password)
        {
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// 超级管理员工作台
        /// </summary>
        public ActionResult SuperAdminDashboard()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "superadmin")
                return RedirectToAction("Login", "Staff");

            var superAdmin = (SuperAdmins)Session["LoginStaff"];
            ViewBag.StaffName = superAdmin.Username;
            ViewBag.DisplayName = "超级管理员";
            ViewBag.StaffRole = "superadmin";

            return View();
        }

        /// <summary>
        /// 运输人员工作台
        /// </summary>
        public ActionResult TransporterDashboard()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            var transporter = (Transporters)Session["LoginStaff"];
            ViewBag.StaffName = transporter.Username;
            ViewBag.DisplayName = "运输人员";
            ViewBag.StaffRole = "transporter";

            return View();
        }

        /// <summary>
        /// 运输管理页面（运输人员端）
        /// </summary>
        public ActionResult TransportationManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            var transporter = (Transporters)Session["LoginStaff"];
            ViewBag.StaffName = transporter.Username;
            ViewBag.TransporterRegion = transporter.Region;

            return View();
        }

        /// <summary>
        /// 获取运输人员的运输单列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTransporterOrders(string status)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 获取运输单列表，在数据库层面进行状态筛选
                var orders = _transportationOrderBLL.GetTransportationOrdersByTransporter(
                    transporter.TransporterID, 
                    status
                );

                // 获取所有订单用于计算统计数据
                // Note: 当筛选状态时，这会产生额外的数据库查询，但确保统计数据始终准确
                // 对于运输人员通常不会有大量订单，这个开销是可接受的
                var allOrders = string.IsNullOrEmpty(status) || status == "all"
                    ? orders
                    : _transportationOrderBLL.GetTransportationOrdersByTransporter(transporter.TransporterID, null);

                // 计算统计数据
                var statistics = new
                {
                    pending = allOrders.Count(o => o.Status == "待接单"),
                    inTransit = allOrders.Count(o => o.Status == "运输中"),
                    completed = allOrders.Count(o => o.Status == "已完成"),
                    total = allOrders.Count
                };

                return Json(new
                {
                    success = true,
                    data = orders.Select(o => new
                    {
                        o.TransportOrderID,
                        o.OrderNumber,
                        o.PickupAddress,
                        o.DestinationAddress,
                        o.ContactPerson,
                        o.ContactPhone,
                        o.EstimatedWeight,
                        o.ActualWeight,
                        o.ItemCategories,
                        o.SpecialInstructions,
                        o.Status,
                        // Prefer Stage when present; fall back to TransportStage for backward compatibility
                        // Use Stage if it has a value, otherwise use TransportStage
                        Stage = !string.IsNullOrEmpty(o.Stage) ? o.Stage : o.TransportStage,
                        CreatedDate = o.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                        AcceptedDate = o.AcceptedDate?.ToString("yyyy-MM-dd HH:mm"),
                        PickupDate = o.PickupDate?.ToString("yyyy-MM-dd HH:mm"),
                        CompletedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm")
                    }),
                    statistics
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"获取运输单列表失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 验证运输单权限的辅助方法
        /// </summary>
        private (bool success, string message, TransportationOrders order) ValidateTransportationOrderAccess(
            int orderId, 
            int transporterId, 
            string expectedStatus = null)
        {
            var order = _transportationOrderBLL.GetTransportationOrderById(orderId);
            
            if (order == null)
            {
                return (false, "运输单不存在", null);
            }

            if (order.TransporterID != transporterId)
            {
                return (false, "无权操作此运输单", null);
            }

            if (expectedStatus != null && order.Status != expectedStatus)
            {
                return (false, $"运输单状态不正确，当前状态为{order.Status}", null);
            }

            return (true, null, order);
        }

        /// <summary>
        /// 获取有效的运输阶段（优先使用Stage列，回退到TransportStage列）
        /// </summary>
        private string GetEffectiveTransportStage(TransportationOrders order)
        {
            // Prefer Stage column, fall back to TransportStage for backward compatibility
            return !string.IsNullOrEmpty(order.Stage) ? order.Stage : order.TransportStage;
        }

        /// <summary>
        /// 接收运输单（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptTransportOrder(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "待接单");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 接单
                bool result = _transportationOrderBLL.AcceptTransportationOrder(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "接单成功" });
                }
                else
                {
                    return Json(new { success = false, message = "接单失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"接单失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 开始运输（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult StartTransport(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "已接单");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 开始运输
                bool result = _transportationOrderBLL.StartTransportation(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "已开始运输" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 完成运输（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CompleteTransport(int orderId, decimal? actualWeight)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "运输中");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 验证运输阶段必须是"到达送货地点"（或 NULL 以支持旧订单）
                string currentStage = GetEffectiveTransportStage(validation.order);
                if (currentStage != "到达送货地点" && currentStage != null)
                {
                    return Json(new { success = false, message = $"运输阶段不正确，当前阶段为{currentStage}，必须先完成前面的步骤" });
                }

                // 完成运输
                bool result = _transportationOrderBLL.CompleteTransportation(orderId, actualWeight);

                if (result)
                {
                    return Json(new { success = true, message = "运输已完成" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 确认收货地点（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfirmPickupLocation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "已接单");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 确认收货地点
                bool result = _transportationOrderBLL.ConfirmPickupLocation(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "已确认收货地点" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 到达收货地点（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ArriveAtPickupLocation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "运输中");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 验证运输阶段（如果Stage为null，说明数据库没有此列，跳过验证以保持向后兼容）
                string currentStage = GetEffectiveTransportStage(validation.order);
                if (currentStage != null && 
                    currentStage != "确认取货地点" &&
                    currentStage != "确认收货地点")
                {
                    return Json(new { success = false, message = $"运输阶段不正确，当前阶段为{currentStage}" });
                }

                // 到达收货地点
                bool result = _transportationOrderBLL.ArriveAtPickupLocation(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "已到达收货地点" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 装货完毕（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CompleteLoading(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "运输中");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 验证运输阶段（如果Stage为null，说明数据库没有此列，跳过验证以保持向后兼容）
                string currentStage = GetEffectiveTransportStage(validation.order);
                if (currentStage != null && 
                    currentStage != "到达取货地点" &&
                    currentStage != "到达收货地点")
                {
                    return Json(new { success = false, message = $"运输阶段不正确，当前阶段为{currentStage}" });
                }

                // 装货完毕
                bool result = _transportationOrderBLL.CompleteLoading(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "装货完毕" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 确认送货地点（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfirmDeliveryLocation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "运输中");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 验证运输阶段（如果Stage为null，说明数据库没有此列，跳过验证以保持向后兼容）
                // 接受"装货完成"（新）和"装货完毕"（旧）两种说法
                string currentStage = GetEffectiveTransportStage(validation.order);
                if (currentStage != null && 
                    currentStage != "装货完成" && 
                    currentStage != "装货完毕")
                {
                    return Json(new { success = false, message = $"运输阶段不正确，当前阶段为{currentStage}" });
                }

                // 确认送货地点
                bool result = _transportationOrderBLL.ConfirmDeliveryLocation(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "已确认送货地点" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 到达送货地点（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ArriveAtDeliveryLocation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var transporter = (Transporters)Session["LoginStaff"];

                // 验证运输单权限和状态
                var validation = ValidateTransportationOrderAccess(orderId, transporter.TransporterID, "运输中");
                if (!validation.success)
                {
                    return Json(new { success = false, message = validation.message });
                }

                // 验证运输阶段（如果Stage为null，说明数据库没有此列，跳过验证以保持向后兼容）
                string currentStage = GetEffectiveTransportStage(validation.order);
                if (currentStage != null && currentStage != "确认送货地点")
                {
                    return Json(new { success = false, message = $"运输阶段不正确，当前阶段为{currentStage}" });
                }

                // 到达送货地点
                bool result = _transportationOrderBLL.ArriveAtDeliveryLocation(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "已到达送货地点" });
                }
                else
                {
                    return Json(new { success = false, message = "操作失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 基地工作人员工作台
        /// </summary>
        public ActionResult SortingCenterWorkerDashboard()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            ViewBag.StaffName = worker.Username;
            ViewBag.DisplayName = "基地工作人员";
            ViewBag.StaffRole = "sortingcenterworker";

            return View();
        }

        /// <summary>
        /// 回收员订单管理页面
        /// </summary>
        public ActionResult Recycler_OrderManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
                return RedirectToAction("Login", "Staff");

            var recycler = (Recyclers)Session["LoginStaff"];

            // 通过 BLL 获取订单统计
            var statistics = _recyclerOrderBLL.GetRecyclerOrderStatistics(recycler.RecyclerID);
            ViewBag.OrderStatistics = statistics;
            ViewBag.StaffName = recycler.Username;

            return View();
        }

        /// <summary>
        /// 获取回收员订单列表（AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult GetRecyclerOrders(OrderFilterModel filter)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                // 设置分页参数
                if (filter.PageIndex < 1) filter.PageIndex = 1;
                if (filter.PageSize < 1) filter.PageSize = 10;

                // 通过 BLL 获取订单数据
                var result = _recyclerOrderBLL.GetRecyclerOrders(filter, recycler.RecyclerID);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 接收订单
        /// </summary>
        [HttpPost]
        public JsonResult AcceptOrder(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                // 通过 BLL 接收订单
                var result = _recyclerOrderBLL.AcceptOrder(appointmentId, recycler.RecyclerID);

                if (result.Success)
                {
                    // 发送系统消息通知用户（聊天消息）
                    var systemMessage = new SendMessageRequest
                    {
                        OrderID = appointmentId,
                        SenderType = "system",
                        SenderID = 0,
                        Content = $"回收员 {recycler.Username} 已接收您的订单，请保持电话畅通。"
                    };
                    _messageBLL.SendMessage(systemMessage);

                    // 发送用户通知消息
                    _notificationBLL.SendOrderAcceptedNotification(appointmentId, recycler.Username);

                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"接收失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 回收员回退订单
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RollbackOrder(int appointmentId, string reason)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                // 调用 BLL 回退订单
                var result = _recyclerOrderBLL.RollbackOrder(appointmentId, recycler.RecyclerID, reason);

                if (result.Success)
                {
                    // 发送系统消息通知用户
                    var reasonText = string.IsNullOrEmpty(reason) ? "物品不符合回收要求" : reason;
                    var systemMessage = new SendMessageRequest
                    {
                        OrderID = appointmentId,
                        SenderType = "system",
                        SenderID = 0,
                        Content = $"订单已被回收员 {recycler.Username} 回退。原因：{reasonText}。如有疑问，请与回收员联系沟通。"
                    };
                    _messageBLL.SendMessage(systemMessage);

                    // 发送用户通知消息
                    _notificationBLL.SendOrderRolledBackNotification(appointmentId, recycler.Username, reasonText);

                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"回退失败：{ex.Message}" });
            }
        }

        // <summary>
        /// 消息中心页面（回收员端）
        /// 该方法返回视图，视图的 Model 为 List<RecyclerMessageViewModel>（每条为一条消息记录）
        /// </summary>
        public ActionResult Message_Center()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
                return RedirectToAction("Login", "Staff");

            var recycler = (Recyclers)Session["LoginStaff"];
            ViewBag.StaffName = recycler.Username;

            // 获取消息列表（后端按时间倒序返回所有消息条目）
            var messages = _recyclerOrderBLL.GetRecyclerMessages(recycler.RecyclerID);
            return View(messages);
        }

        /// <summary>
        /// 联系用户视图（回收员端）
        /// </summary>
        [HttpGet]
        public ActionResult ContactUser(int orderId)
        {
            // 验证回收员登录状态
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                var recycler = (Recyclers)Session["LoginStaff"];

                // 通过BLL层获取订单详情
                var orderResult = _recyclerOrderBLL.GetOrderDetail(orderId, recycler.RecyclerID);
                if (orderResult.Detail == null)
                {
                    ViewBag.ErrorMsg = "无法联系用户：订单不存在或无权访问";
                    return View();
                }

                // 获取用户信息
                var userBLL = new UserBLL();
                var user = userBLL.GetUserById(orderResult.Detail.UserID);
                if (user == null)
                {
                    ViewBag.ErrorMsg = "用户信息不存在";
                    return View();
                }

                // 设置ViewBag变量供视图使用
                ViewBag.OrderId = orderId;
                ViewBag.OrderNumber = orderResult.Detail.OrderNumber;
                ViewBag.UserName = user.Username;
                ViewBag.UserId = user.UserID;
                ViewBag.RecyclerId = recycler.RecyclerID;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = $"加载联系页面失败：{ex.Message}";
                return View();
            }
        }

        // 获取订单对话（回收员端），已包含 conversationEnded/endedBy/endedTime
        [HttpPost]
        public ContentResult GetOrderConversation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                if (orderId <= 0)
                    return JsonContent(new { success = false, message = "无效订单ID" });

                var messagesVm = _recyclerOrderBLL.GetOrderConversation(orderId);
                var result = messagesVm.Select(m => new
                {
                    messageId = m.MessageID,
                    orderId = m.OrderID,
                    senderType = (m.SenderType ?? string.Empty).ToLower(),
                    senderId = m.SenderID,
                    senderName = m.SenderName ?? "",
                    content = m.Content ?? "",
                    // 若 SentTime 是 Nullable<DateTime>（DateTime?），先判断后再取 Value.ToString("o")
                    sentTime = (m.SentTime != null && m.SentTime != default(DateTime))
                                ? (m.SentTime is DateTime dt ? dt.ToString("o") : m.SentTime.ToString())
                                : string.Empty,
                    isRead = m.IsRead
                }).ToList();

                // 获取最近结束信息（供前端显示“谁结束了/是否双方都结束”）
                var convBll = new ConversationBLL();
                var bothInfo = convBll.HasBothEnded(orderId); // (bool, DateTime?)
                bool conversationBothEnded = bothInfo.BothEnded;
                string latestEndedTimeIso = bothInfo.LatestEndedTime.HasValue ? bothInfo.LatestEndedTime.Value.ToString("o") : string.Empty;
                
                // 使用公共方法确定谁已经结束了对话
                string lastEndedBy = convBll.GetConversationEndedByStatus(orderId);

                return JsonContent(new
                {
                    success = true,
                    messages = result,
                    conversationLastEndedBy = lastEndedBy,
                    conversationBothEnded = conversationBothEnded,
                    conversationLatestEndedTime = latestEndedTimeIso
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 发送消息给用户（回收员端 AJAX）
        /// 前端只需要提供 OrderID 和 Content，后台会填充 SenderType/SenderID
        /// </summary>
        [HttpPost]
        public ContentResult SendMessageToUser(SendMessageRequest request)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                if (request == null || request.OrderID <= 0 || string.IsNullOrWhiteSpace(request.Content))
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "参数不完整" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                request.SenderType = "recycler";
                request.SenderID = recycler.RecyclerID;

                var result = _messageBLL.SendMessage(request);
                var json = JsonConvert.SerializeObject(new { success = result.Success, message = result.Message });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = $"发送失败：{ex.Message}" });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        /// <summary>
        /// 标记订单中对回收员可见的消息为已读（回收员已查看）
        /// </summary>
        [HttpPost]
        public ContentResult MarkMessagesAsRead(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                bool result = _messageBLL.MarkMessagesAsRead(orderId, "recycler", recycler.RecyclerID);
                var json = JsonConvert.SerializeObject(new { success = result });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        /// <summary>
        /// 获取回收员未读消息数量（AJAX）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclerUnreadCount()
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, unreadCount = 0 });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                var unreadCount = _messageBLL.GetRecyclerUnreadCount(recycler.RecyclerID);

                var json = JsonConvert.SerializeObject(new { success = true, unreadCount = unreadCount });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message, unreadCount = 0 });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        /// <summary>
        /// 获取订单详情（AJAX）
        /// </summary>
        [HttpPost]
        public ContentResult GetOrderDetail(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                var result = _recyclerOrderBLL.GetOrderDetail(appointmentId, recycler.RecyclerID);

                if (result.Detail != null)
                {
                    var json = JsonConvert.SerializeObject(new { success = true, data = result.Detail });
                    return Content(json, "application/json", System.Text.Encoding.UTF8);
                }
                else
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = result.Message });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 获取回收员的历史会话列表（分页）
        [HttpPost]
        public ContentResult GetRecyclerConversations(int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                var conversationBLL = new ConversationBLL();
                var convs = conversationBLL.GetRecyclerConversations(recycler.RecyclerID, pageIndex, pageSize);

                var result = convs.Select(c => new
                {
                    conversationId = c.ConversationID,
                    orderId = c.OrderID,
                    orderNumber = c.OrderNumber,
                    userName = c.UserName,
                    createdTime = c.CreatedTime.HasValue ? c.CreatedTime.Value.ToString("o") : string.Empty,
                    endedTime = c.EndedTime.HasValue ? c.EndedTime.Value.ToString("o") : string.Empty,
                    status = c.Status
                }).ToList();

                var json = JsonConvert.SerializeObject(new { success = true, conversations = result });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 获取某个已结束会话的历史消息（回收员端查看历史）
        [HttpPost]
        public ContentResult GetConversationMessagesBeforeEnd(int orderId, string endedTime)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                if (orderId <= 0 || string.IsNullOrWhiteSpace(endedTime))
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "参数不完整" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                if (!DateTime.TryParse(endedTime, out DateTime et))
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "结束时间格式错误" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var conversationBLL = new ConversationBLL();
                var messages = conversationBLL.GetConversationMessagesBeforeEnd(orderId, et);

                var result = messages.Select(m => new
                {
                    messageId = m.MessageID,
                    orderId = m.OrderID,
                    senderType = (m.SenderType ?? string.Empty).ToLower(),
                    senderId = m.SenderID,
                    content = m.Content ?? string.Empty,
                    // 若 SentTime 是 Nullable<DateTime>（DateTime?），先判断后再取 Value.ToString("o")
                    sentTime = (m.SentTime != null && m.SentTime != default(DateTime))
                                ? (m.SentTime is DateTime dt ? dt.ToString("o") : m.SentTime.ToString())
                                : string.Empty,
                    isRead = m.IsRead
                }).ToList();

                var json = JsonConvert.SerializeObject(new { success = true, messages = result });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 回收员结束会话（StaffController）
        [HttpPost]
        public ContentResult EndConversation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                var convBll = new ConversationBLL();
                bool ok = convBll.EndConversationBy(orderId, "recycler", recycler.RecyclerID);

                if (!ok)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "用户需要先结束对话" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                // 检查双方是否都已结束
                var (bothEnded, _) = convBll.HasBothEnded(orderId);

                var json = JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "对话已结束",
                    bothEnded = bothEnded
                });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 检查双方是否都已结束对话
        [HttpPost]
        public ContentResult CheckBothEnded(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var convBll = new ConversationBLL();
                var (bothEnded, _) = convBll.HasBothEnded(orderId);

                var json = JsonConvert.SerializeObject(new { success = true, bothEnded = bothEnded });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 完成订单（回收员点击后把订单状态置为 已完成，并写入库存）
        [HttpPost]
        public ContentResult CompleteOrder(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                // 检查对话是否都已结束
                var convBll = new ConversationBLL();
                var (bothEnded, _) = convBll.HasBothEnded(appointmentId);

                if (!bothEnded)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "双方必须都结束对话后才能完成订单" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                return ExecuteOrderCompletion(appointmentId, recycler.RecyclerID);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 直接完成订单（从订单列表，不检查对话状态）
        [HttpPost]
        public ContentResult CompleteOrderDirect(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "请先登录" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                return ExecuteOrderCompletion(appointmentId, recycler.RecyclerID);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }
        }

        // 共享方法：执行订单完成操作（写入库存和更新状态）
        private ContentResult ExecuteOrderCompletion(int appointmentId, int recyclerId)
        {
            // 写入库存
            var inventoryBll = new InventoryBLL();
            bool inventoryAdded = inventoryBll.AddInventoryFromOrder(appointmentId, recyclerId);

            if (!inventoryAdded)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = "写入库存失败" });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
            }

            // 完成订单
            var orderBll = new OrderBLL();
            var result = orderBll.CompleteOrder(appointmentId, recyclerId);
            
            // 发送订单完成通知和评价提醒
            if (result.Success)
            {
                _notificationBLL.SendOrderCompletedNotification(appointmentId);
                _notificationBLL.SendReviewReminderNotification(appointmentId);
            }
            
            var json = JsonConvert.SerializeObject(new { success = result.Success, message = result.Message });
            return Content(json, "application/json", System.Text.Encoding.UTF8);
        }

        // 仓库管理页面 - 管理员端
        [AdminPermission(AdminPermissions.WarehouseManagement)]
        public ActionResult WarehouseManagement()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
                return RedirectToAction("Login", "Staff");

            return View();
        }

        // 获取库存汇总数据 - 管理员端（从仓库库存数据获取）
        [HttpPost]
        public JsonResult GetInventorySummary()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return Json(new { success = false, message = "权限不足" });

                // 使用InventoryBLL获取仓库类型的库存数据
                var inventoryBll = new InventoryBLL();
                var summary = inventoryBll.GetInventorySummary(null, "Warehouse");

                var result = summary.Select(s => new
                {
                    categoryKey = s.CategoryKey,
                    categoryName = s.CategoryName,
                    totalWeight = s.TotalWeight,
                    totalPrice = s.TotalPrice
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 获取库存明细数据 - 管理员端（从仓库库存数据获取）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetInventoryDetail(int page = 1, int pageSize = 20, string categoryKey = null)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                // 使用InventoryBLL获取仓库类型的库存明细数据（包含回收员信息）
                var inventoryBll = new InventoryBLL();
                var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey, "Warehouse");

                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 暂存点管理页面 - 回收员端
        /// </summary>
        [HttpGet]
        public ActionResult StoragePointManagement()
        {
            // 检查登录
            if (Session["LoginStaff"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var staff = Session["LoginStaff"] as Recyclers;
            var role = Session["StaffRole"] as string;

            if (role != "recycler")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.RecyclerName = staff.Username;
            ViewBag.Region = staff.Region;

            return View();
        }

        /// <summary>
        /// Get storage point inventory summary for recycler (AJAX)
        /// Simplified implementation: query directly from completed orders
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetStoragePointSummary()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("GetStoragePointSummary: 用户未登录");
                    return JsonContent(new { success = false, message = "未登录，请重新登录" });
                }

                var staff = Session["LoginStaff"] as Recyclers;
                var role = Session["StaffRole"] as string;

                if (staff == null || role != "recycler")
                {
                    System.Diagnostics.Debug.WriteLine($"GetStoragePointSummary: 权限验证失败 - Role: {role}");
                    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
                }

                System.Diagnostics.Debug.WriteLine($"GetStoragePointSummary: 开始查询回收员 ID={staff.RecyclerID} 的库存数据");

                // Use simplified implementation: query directly from order tables
                var storagePointBll = new StoragePointBLL();

                // Get inventory summary for this recycler (grouped by category)
                var summary = storagePointBll.GetStoragePointSummary(staff.RecyclerID);

                System.Diagnostics.Debug.WriteLine($"GetStoragePointSummary: 成功查询到 {summary.Count} 条汇总记录");

                // Return success even if data is empty
                var result = summary.Select(s => new
                {
                    categoryKey = s.CategoryKey,
                    categoryName = s.CategoryName,
                    totalWeight = s.TotalWeight,
                    totalPrice = s.TotalPrice
                }).ToList();

                return JsonContent(new { success = true, data = result });
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                var errorMsg = $"数据库连接或查询错误 (错误代码: {sqlEx.Number})";
                System.Diagnostics.Debug.WriteLine($"GetStoragePointSummary SQL错误: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"详细信息: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {sqlEx.StackTrace}");
                
                // Provide helpful error messages based on SQL error codes
                switch (sqlEx.Number)
                {
                    case 208: // Invalid object name
                        errorMsg = "数据库表不存在，请联系管理员检查数据库配置";
                        break;
                    case 4060: // Cannot open database
                        errorMsg = "无法连接到数据库，请检查数据库服务";
                        break;
                    case 18456: // Login failed
                        errorMsg = "数据库身份验证失败，请检查连接配置";
                        break;
                    default:
                        errorMsg += $": {sqlEx.Message}";
                        break;
                }
                
                return JsonContent(new { success = false, message = errorMsg });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStoragePointSummary 错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                var innerMsg = ex.InnerException != null ? $" 详情: {ex.InnerException.Message}" : "";
                return JsonContent(new { success = false, message = $"获取数据失败: {ex.Message}{innerMsg}" });
            }
        }

        /// <summary>
        /// Get storage point inventory details for recycler (AJAX)
        /// Simplified implementation: query directly from completed orders
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetStoragePointDetail(string categoryKey = null)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("GetStoragePointDetail: 用户未登录");
                    return JsonContent(new { success = false, message = "未登录，请重新登录" });
                }

                var staff = Session["LoginStaff"] as Recyclers;
                var role = Session["StaffRole"] as string;

                if (staff == null || role != "recycler")
                {
                    System.Diagnostics.Debug.WriteLine($"GetStoragePointDetail: 权限验证失败 - Role: {role}");
                    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
                }

                System.Diagnostics.Debug.WriteLine($"GetStoragePointDetail: 开始查询回收员 ID={staff.RecyclerID}, CategoryKey={categoryKey ?? "全部"} 的库存明细");

                // Use simplified implementation: query directly from order tables
                var storagePointBll = new StoragePointBLL();

                // Get inventory detail list for this recycler
                var detailList = storagePointBll.GetStoragePointDetail(staff.RecyclerID, categoryKey);

                System.Diagnostics.Debug.WriteLine($"GetStoragePointDetail: 成功查询到 {detailList.Count} 条明细记录");

                // Return success even if data is empty
                var result = detailList.Select(d => new
                {
                    orderId = d.OrderID,
                    categoryKey = d.CategoryKey,
                    categoryName = d.CategoryName,
                    weight = d.Weight,
                    price = d.Price,
                    createdDate = d.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return JsonContent(new { success = true, data = result });
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                var errorMsg = $"数据库连接或查询错误 (错误代码: {sqlEx.Number})";
                System.Diagnostics.Debug.WriteLine($"GetStoragePointDetail SQL错误: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"详细信息: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {sqlEx.StackTrace}");
                
                // Provide helpful error messages based on SQL error codes
                switch (sqlEx.Number)
                {
                    case 208: // Invalid object name
                        errorMsg = "数据库表不存在，请联系管理员检查数据库配置";
                        break;
                    case 4060: // Cannot open database
                        errorMsg = "无法连接到数据库，请检查数据库服务";
                        break;
                    case 18456: // Login failed
                        errorMsg = "数据库身份验证失败，请检查连接配置";
                        break;
                    default:
                        errorMsg += $": {sqlEx.Message}";
                        break;
                }
                
                return JsonContent(new { success = false, message = errorMsg });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStoragePointDetail 错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                var innerMsg = ex.InnerException != null ? $" 详情: {ex.InnerException.Message}" : "";
                return JsonContent(new { success = false, message = $"获取数据失败: {ex.Message}{innerMsg}" });
            }
        }

        /// <summary>
        /// Get available transporters in the same region as the recycler (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetAvailableTransporters()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return JsonContent(new { success = false, message = "未登录，请重新登录" });
                }

                var staff = Session["LoginStaff"] as Recyclers;
                var role = Session["StaffRole"] as string;

                if (staff == null || role != "recycler")
                {
                    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
                }

                // Check if recycler has a region assigned
                if (string.IsNullOrWhiteSpace(staff.Region))
                {
                    return JsonContent(new { success = false, message = "您的账号未分配区域，请联系管理员" });
                }

                // Get transporters in the same region who are active and available
                using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT TransporterID, Username, FullName, PhoneNumber, Region, 
                                  VehicleType, VehiclePlateNumber, VehicleCapacity, 
                                  CurrentStatus, Available, Rating
                           FROM Transporters 
                           WHERE Region = @Region 
                           AND IsActive = 1 
                           AND Available = 1
                           ORDER BY Rating DESC, FullName";

                    var cmd = new System.Data.SqlClient.SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Region", staff.Region);

                    var transporters = new List<object>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transporters.Add(new
                            {
                                transporterId = reader.GetInt32(0),
                                username = reader.GetString(1),
                                fullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                phoneNumber = reader.GetString(3),
                                region = reader.GetString(4),
                                vehicleType = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                vehiclePlateNumber = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                vehicleCapacity = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                currentStatus = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                available = reader.GetBoolean(9),
                                rating = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10)
                            });
                        }
                    }

                    return JsonContent(new { success = true, data = transporters });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAvailableTransporters 错误: {ex.Message}");
                return JsonContent(new { success = false, message = $"获取运输人员失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取所有活跃的基地工作人员列表（用于运输单的基地联系人下拉选择）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetBaseStaffList()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return JsonContent(new { success = false, message = "未登录，请重新登录" });
                }

                var role = Session["StaffRole"] as string;
                if (role != "recycler")
                {
                    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
                }

                // Get all active base staff from SortingCenterWorkers table
                using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT WorkerID, FullName, PhoneNumber, Position 
                           FROM SortingCenterWorkers 
                           WHERE IsActive = 1 
                           ORDER BY FullName";

                    var cmd = new System.Data.SqlClient.SqlCommand(sql, conn);

                    var baseStaffList = new List<object>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            baseStaffList.Add(new
                            {
                                workerId = reader.GetInt32(0),
                                fullName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                phoneNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                position = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }

                    if (baseStaffList.Count == 0)
                    {
                        return JsonContent(new { success = false, message = "当前没有可用的基地工作人员" });
                    }

                    return JsonContent(new { success = true, data = baseStaffList });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBaseStaffList 错误: {ex.Message}");
                return JsonContent(new { success = false, message = $"获取基地工作人员失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 创建运输单
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult CreateTransportationOrder(int transporterId, string pickupAddress, 
            decimal estimatedWeight, decimal itemTotalValue, string itemCategories, 
            string baseContactPerson, string baseContactPhone, string specialInstructions)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return JsonContent(new { success = false, message = "未登录，请重新登录" });
                }

                var staff = Session["LoginStaff"] as Recyclers;
                var role = Session["StaffRole"] as string;

                if (staff == null || role != "recycler")
                {
                    return JsonContent(new { success = false, message = "权限不足，仅回收员可访问" });
                }

                // 验证必填字段
                if (transporterId <= 0)
                {
                    return JsonContent(new { success = false, message = "请选择运输人员" });
                }

                if (string.IsNullOrWhiteSpace(pickupAddress))
                {
                    return JsonContent(new { success = false, message = "请填写取货地址" });
                }

                if (estimatedWeight <= 0)
                {
                    return JsonContent(new { success = false, message = "预估重量必须大于0" });
                }

                // 创建运输单对象
                // 目的地固定为"深圳基地"：根据业务需求，当前所有运输都统一送往深圳基地集中分拣中心
                // 如果将来需要支持多个基地，需要修改此处逻辑为可配置的选项
                // 回收员联系人信息从session自动获取，确保数据准确性
                var order = new TransportationOrders
                {
                    RecyclerID = staff.RecyclerID,
                    TransporterID = transporterId,
                    PickupAddress = pickupAddress,
                    DestinationAddress = "深圳基地", // 固定目的地
                    ContactPerson = string.IsNullOrWhiteSpace(staff.FullName) ? staff.Username : staff.FullName, // 回收员姓名，如果FullName为空则使用Username
                    ContactPhone = staff.PhoneNumber, // 回收员电话
                    BaseContactPerson = baseContactPerson, // 基地联系人（可编辑）
                    BaseContactPhone = baseContactPhone, // 基地联系电话（可编辑）
                    EstimatedWeight = estimatedWeight,
                    ItemTotalValue = itemTotalValue,
                    ItemCategories = itemCategories,
                    SpecialInstructions = specialInstructions
                };

                // 调用BLL创建运输单
                var transportOrderBLL = new TransportationOrderBLL();
                var (orderId, orderNumber) = transportOrderBLL.CreateTransportationOrder(order);

                if (orderId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"运输单创建成功，ID: {orderId}, OrderNumber: {orderNumber}");
                    return JsonContent(new 
                    { 
                        success = true, 
                        message = "运输单创建成功", 
                        orderId = orderId,
                        orderNumber = orderNumber
                    });
                }
                else
                {
                    return JsonContent(new { success = false, message = "运输单创建失败" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTransportationOrder 错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                return JsonContent(new { success = false, message = $"创建运输单失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 用户评价页面 - 回收员查看收到的评价
        /// </summary>
        [HttpGet]
        public ActionResult UserReviews()
        {
            // 检查登录
            if (Session["LoginStaff"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var staff = Session["LoginStaff"] as Recyclers;
            var role = Session["StaffRole"] as string;

            if (role != "recycler")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// 获取回收员的评价数据
        /// </summary>
        [HttpPost]
        public ContentResult GetRecyclerReviews()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return JsonContent(new { success = false, message = "未登录" });
                }

                var staff = Session["LoginStaff"] as Recyclers;
                var role = Session["StaffRole"] as string;

                if (role != "recycler")
                {
                    return JsonContent(new { success = false, message = "权限不足" });
                }

                var reviewBLL = new OrderReviewBLL();

                // 获取评价列表
                var reviews = reviewBLL.GetReviewsByRecyclerId(staff.RecyclerID);

                // 获取评分摘要
                var summary = reviewBLL.GetRecyclerRatingSummary(staff.RecyclerID);

                // 获取星级分布
                var distribution = reviewBLL.GetRecyclerRatingDistribution(staff.RecyclerID);

                var result = reviews.Select(r => new
                {
                    orderId = r.OrderID,
                    orderNumber = "AP" + r.OrderID.ToString("D6"),
                    userId = r.UserID,
                    starRating = r.StarRating,
                    reviewText = r.ReviewText,
                    createdDate = r.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return JsonContent(new
                {
                    success = true,
                    reviews = result,
                    averageRating = Math.Round(summary.AverageRating, 2),
                    totalReviews = summary.TotalReviews,
                    distribution = distribution
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }



        #region Admin - User Management

        /// <summary>
        /// 管理员 - 用户管理页面
        /// </summary>
        [AdminPermission(AdminPermissions.UserManagement)]
        public ActionResult UserManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取用户列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetUsers(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            try
            {
                var result = _adminBLL.GetAllUsers(page, pageSize, searchTerm);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取用户统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetUserStatistics()
        {
            try
            {
                var stats = _adminBLL.GetUserStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 导出用户数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportUsers(string searchTerm = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                // Get all users without pagination for export
                var users = _adminBLL.GetAllUsersForExport(searchTerm);

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add UTF-8 BOM for proper Excel display of Chinese characters
                csv.Append("\uFEFF");

                // Add header
                csv.AppendLine("用户ID,用户名,邮箱,手机号,注册日期,最后登录日期,状态");

                // Add data rows
                foreach (var user in users)
                {
                    // Determine user status
                    var isActive = user.LastLoginDate.HasValue &&
                                  (DateTime.Now - user.LastLoginDate.Value).TotalDays <= 30;
                    var status = isActive ? "活跃" : "不活跃";
                    var lastLogin = user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未登录";

                    csv.AppendLine($"{user.UserID},{EscapeCsvField(user.Username)},{EscapeCsvField(user.Email)},{EscapeCsvField(user.PhoneNumber)},{user.RegistrationDate:yyyy-MM-dd HH:mm:ss},{EscapeCsvField(lastLogin)},{EscapeCsvField(status)}");
                }

                // Generate file
                var fileName = $"用户数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                // 记录导出操作日志
                LogAdminOperation(OperationLogBLL.Modules.UserManagement, OperationLogBLL.OperationTypes.Export, $"导出用户数据，共{users.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("UserManagement");
            }
        }

        #endregion

        #region Admin - Recycler Management

        /// <summary>
        /// 管理员 - 回收员管理页面
        /// </summary>
        [AdminPermission(AdminPermissions.RecyclerManagement)]
        public ActionResult RecyclerManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取回收员列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            try
            {
                var result = _adminBLL.GetAllRecyclers(page, pageSize, searchTerm, isActive);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取回收员列表（优化版，包含完成订单数和排序）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclersOptimized(int page = 1, int pageSize = 8, string searchTerm = null, bool? isActive = null, string sortOrder = "ASC")
        {
            try
            {
                var result = _adminBLL.GetAllRecyclersWithDetails(page, pageSize, searchTerm, isActive, sortOrder);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取回收员详情（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclerDetails(int recyclerId)
        {
            try
            {
                var recycler = _adminBLL.GetRecyclerById(recyclerId);
                var completedOrders = _adminBLL.GetRecyclerCompletedOrdersCount(recyclerId);

                return JsonContent(new {
                    success = true,
                    data = new
                    {
                        recycler = recycler,
                        completedOrders = completedOrders
                    }
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 添加回收员（API）
        /// </summary>
        [HttpPost]
        public JsonResult AddRecycler(Recyclers recycler, string password)
        {
            try
            {
                var result = _adminBLL.AddRecycler(recycler, password);
                
                // 记录操作日志
                if (result.Success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.RecyclerManagement, OperationLogBLL.OperationTypes.Create, $"添加回收员：{recycler.Username}", null, recycler.Username, "Success");
                }
                else
                {
                    LogAdminOperation(OperationLogBLL.Modules.RecyclerManagement, OperationLogBLL.OperationTypes.Create, $"添加回收员失败：{recycler.Username}", null, recycler.Username, "Failed");
                }
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 更新回收员信息（API）
        /// </summary>
        [HttpPost]
        public JsonResult UpdateRecycler(Recyclers recycler)
        {
            try
            {
                var result = _adminBLL.UpdateRecycler(recycler);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.RecyclerManagement, OperationLogBLL.OperationTypes.Update, 
                    result.Success ? $"更新回收员信息：{recycler.Username}" : $"更新回收员信息失败：{recycler.Username}", 
                    recycler.RecyclerID, recycler.Username, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 删除回收员（API）
        /// </summary>
        [HttpPost]
        public JsonResult DeleteRecycler(int recyclerId)
        {
            try
            {
                // 获取回收员信息用于日志记录
                var recycler = _adminBLL.GetRecyclerById(recyclerId);
                string recyclerName = recycler?.Username ?? $"ID:{recyclerId}";
                
                var result = _adminBLL.DeleteRecycler(recyclerId);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.RecyclerManagement, OperationLogBLL.OperationTypes.Delete, $"删除回收员：{recyclerName}", recyclerId, recyclerName, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取回收员统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclerStatistics()
        {
            try
            {
                var stats = _adminBLL.GetRecyclerStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取回收员数据看板统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetRecyclerDashboardStatistics()
        {
            try
            {
                // Permission check
                if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
                {
                    return JsonContent(new { success = false, message = "权限不足" });
                }

                var stats = _adminBLL.GetRecyclerDashboardStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 导出回收员数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportRecyclers(string searchTerm = null, bool? isActive = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                // Get all recyclers without pagination for export
                var recyclers = _adminBLL.GetAllRecyclersForExport(searchTerm, isActive);

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add UTF-8 BOM for proper Excel display of Chinese characters
                csv.Append("\uFEFF");

                // Add header
                csv.AppendLine("回收员ID,用户名,姓名,手机号,区域,评分,完成订单数,是否可接单,账号状态,注册日期");

                // Add data rows
                foreach (var recycler in recyclers)
                {
                    // Get completed orders count
                    var completedOrders = _adminBLL.GetRecyclerCompletedOrdersCount(recycler.RecyclerID);
                    var availableStatus = recycler.Available ? "可接单" : "不可接单";
                    var activeStatus = recycler.IsActive ? "激活" : "禁用";
                    var createdDate = recycler.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";

                    csv.AppendLine($"{recycler.RecyclerID},{EscapeCsvField(recycler.Username)},{EscapeCsvField(recycler.FullName ?? "-")},{EscapeCsvField(recycler.PhoneNumber)},{EscapeCsvField(recycler.Region)},{EscapeCsvField(recycler.Rating?.ToString("F1") ?? "0.0")},{completedOrders},{EscapeCsvField(availableStatus)},{EscapeCsvField(activeStatus)},{EscapeCsvField(createdDate)}");
                }

                // Generate file
                var fileName = $"回收员数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                // 记录导出操作日志
                LogAdminOperation(OperationLogBLL.Modules.RecyclerManagement, OperationLogBLL.OperationTypes.Export, $"导出回收员数据，共{recyclers.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("RecyclerManagement");
            }
        }

        #endregion

        #region Admin - Order Management

        /// <summary>
        /// 管理员 - 订单管理页面
        /// </summary>
        public ActionResult OrderManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取订单列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetOrders(int page = 1, int pageSize = 20, string status = null, string searchTerm = null)
        {
            try
            {
                var result = _adminBLL.GetAllOrders(page, pageSize, status, searchTerm);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取订单统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetOrderStatistics()
        {
            try
            {
                var stats = _adminBLL.GetOrderStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region SuperAdmin - Data Dashboard

        /// <summary>
        /// 超级管理员 - 数据看板页面
        /// </summary>
        public ActionResult DataDashboard()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return RedirectToAction("Login", "Staff");
            }

            var superAdmin = (SuperAdmins)Session["LoginStaff"];
            ViewBag.StaffName = superAdmin.Username;

            return View();
        }

        /// <summary>
        /// 超级管理员 - 获取数据看板统计数据（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetDashboardStatistics()
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var stats = _adminBLL.GetDashboardStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region SuperAdmin - Admin Management

        /// <summary>
        /// 超级管理员 - 管理员管理页面
        /// </summary>
        public ActionResult AdminManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 超级管理员 - 获取管理员列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _adminBLL.GetAllAdmins(page, pageSize, searchTerm, isActive);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 获取管理员详情（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetAdminDetails(int adminId)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var admin = _adminBLL.GetAdminById(adminId);
                return JsonContent(new {
                    success = true,
                    data = admin
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 添加管理员（API）
        /// </summary>
        [HttpPost]
        public JsonResult AddAdmin(Admins admin, string password)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _adminBLL.AddAdmin(admin, password);
                
                // 记录操作日志
                if (result.Success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.AdminManagement, OperationLogBLL.OperationTypes.Create, $"添加管理员：{admin.Username}", null, admin.Username, "Success");
                }
                else
                {
                    LogAdminOperation(OperationLogBLL.Modules.AdminManagement, OperationLogBLL.OperationTypes.Create, $"添加管理员失败：{admin.Username}", null, admin.Username, "Failed");
                }
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 更新管理员信息（API）
        /// </summary>
        [HttpPost]
        public JsonResult UpdateAdmin(Admins admin)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _adminBLL.UpdateAdmin(admin);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.AdminManagement, OperationLogBLL.OperationTypes.Update, 
                    result.Success ? $"更新管理员信息：{admin.Username}" : $"更新管理员信息失败：{admin.Username}", 
                    admin.AdminID, admin.Username, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 删除管理员（API）
        /// </summary>
        [HttpPost]
        public JsonResult DeleteAdmin(int adminId)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                // 获取管理员信息用于日志记录
                var admin = _adminBLL.GetAdminById(adminId);
                string adminName = admin?.Username ?? $"ID:{adminId}";
                
                var result = _adminBLL.DeleteAdmin(adminId);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.AdminManagement, OperationLogBLL.OperationTypes.Delete, $"删除管理员：{adminName}", adminId, adminName, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 获取管理员统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetAdminStatistics()
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var stats = _adminBLL.GetAdminStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 导出管理员数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportAdmins(string searchTerm = null, bool? isActive = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                // Get all admins without pagination for export
                var admins = _adminBLL.GetAllAdminsForExport(searchTerm, isActive);

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add UTF-8 BOM for proper Excel display of Chinese characters
                csv.Append("\uFEFF");

                // Add header
                csv.AppendLine("管理员ID,用户名,姓名,创建日期,最后登录日期,账号状态");

                // Add data rows
                foreach (var admin in admins)
                {
                    var createdDate = admin.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                    var lastLoginDate = admin.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未登录";
                    var activeStatus = (admin.IsActive ?? true) ? "激活" : "禁用";

                    csv.AppendLine($"{admin.AdminID},{EscapeCsvField(admin.Username)},{EscapeCsvField(admin.FullName)},{EscapeCsvField(createdDate)},{EscapeCsvField(lastLoginDate)},{EscapeCsvField(activeStatus)}");
                }

                // Generate file
                var fileName = $"管理员数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                // 记录导出操作日志
                LogAdminOperation(OperationLogBLL.Modules.AdminManagement, OperationLogBLL.OperationTypes.Export, $"导出管理员数据，共{admins.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("AdminManagement");
            }
        }

        #endregion

        #region SuperAdmin - SuperAdmin Account Management

        /// <summary>
        /// 超级管理员 - 超级管理员账号管理页面
        /// </summary>
        public ActionResult SuperAdminAccountManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 超级管理员 - 获取超级管理员列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSuperAdmins(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _superAdminBLL.GetAllSuperAdmins(page, pageSize, searchTerm, isActive);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 获取超级管理员详情（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSuperAdminDetails(int superAdminId)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var superAdmin = _superAdminBLL.GetSuperAdminById(superAdminId);
                return JsonContent(new {
                    success = true,
                    data = superAdmin
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 添加超级管理员（API）
        /// </summary>
        [HttpPost]
        public JsonResult AddSuperAdmin(SuperAdmins superAdmin, string password)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _superAdminBLL.AddSuperAdmin(superAdmin, password);
                
                // 记录操作日志
                if (result.Success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.SuperAdminManagement, OperationLogBLL.OperationTypes.Create, $"添加超级管理员：{superAdmin.Username}", null, superAdmin.Username, "Success");
                }
                else
                {
                    LogAdminOperation(OperationLogBLL.Modules.SuperAdminManagement, OperationLogBLL.OperationTypes.Create, $"添加超级管理员失败：{superAdmin.Username}", null, superAdmin.Username, "Failed");
                }
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 更新超级管理员信息（API）
        /// </summary>
        [HttpPost]
        public JsonResult UpdateSuperAdmin(SuperAdmins superAdmin)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var result = _superAdminBLL.UpdateSuperAdmin(superAdmin);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.SuperAdminManagement, OperationLogBLL.OperationTypes.Update, 
                    result.Success ? $"更新超级管理员信息：{superAdmin.Username}" : $"更新超级管理员信息失败：{superAdmin.Username}", 
                    superAdmin.SuperAdminID, superAdmin.Username, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 删除超级管理员（API）
        /// </summary>
        [HttpPost]
        public JsonResult DeleteSuperAdmin(int superAdminId)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                // 获取超级管理员信息用于日志记录
                var superAdmin = _superAdminBLL.GetSuperAdminById(superAdminId);
                string superAdminName = superAdmin?.Username ?? $"ID:{superAdminId}";
                
                var result = _superAdminBLL.DeleteSuperAdmin(superAdminId);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.SuperAdminManagement, OperationLogBLL.OperationTypes.Delete, $"删除超级管理员：{superAdminName}", superAdminId, superAdminName, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 获取超级管理员统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSuperAdminStatistics()
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return JsonContent(new { success = false, message = "权限不足" });
            }

            try
            {
                var stats = _superAdminBLL.GetSuperAdminStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 超级管理员 - 导出超级管理员数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportSuperAdmins(string searchTerm = null, bool? isActive = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "superadmin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                // Get all super admins without pagination for export
                var superAdmins = _superAdminBLL.GetAllSuperAdminsForExport(searchTerm, isActive);

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add UTF-8 BOM for proper Excel display of Chinese characters
                csv.Append("\uFEFF");

                // Add header
                csv.AppendLine("超级管理员ID,用户名,姓名,创建日期,最后登录日期,账号状态");

                // Add data rows
                foreach (var superAdmin in superAdmins)
                {
                    var createdDate = superAdmin.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                    var lastLoginDate = superAdmin.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未登录";
                    var activeStatus = superAdmin.IsActive ? "激活" : "禁用";

                    csv.AppendLine($"{superAdmin.SuperAdminID},{EscapeCsvField(superAdmin.Username)},{EscapeCsvField(superAdmin.FullName)},{EscapeCsvField(createdDate)},{EscapeCsvField(lastLoginDate)},{EscapeCsvField(activeStatus)}");
                }

                // Generate file
                var fileName = $"超级管理员数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                // 记录导出操作日志
                LogAdminOperation(OperationLogBLL.Modules.SuperAdminManagement, OperationLogBLL.OperationTypes.Export, $"导出超级管理员数据，共{superAdmins.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("SuperAdminAccountManagement");
            }
        }

        #endregion

        #region Admin Homepage Management

        /// <summary>
        /// Admin homepage management index page
        /// </summary>
        [AdminPermission(AdminPermissions.HomepageManagement)]
        public ActionResult HomepageManagement()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
                return RedirectToAction("Login", "Staff");

            var admin = Session["LoginStaff"];
            if (staffRole == "admin")
                ViewBag.StaffName = ((Admins)admin).Username;
            else
                ViewBag.StaffName = ((SuperAdmins)admin).Username;

            return View();
        }

        /// <summary>
        /// Admin homepage carousel management page
        /// </summary>
        [AdminPermission(AdminPermissions.HomepageManagement)]
        public ActionResult HomepageCarouselManagement()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
                return RedirectToAction("Login", "Staff");

            var admin = Session["LoginStaff"];
            if (staffRole == "admin")
                ViewBag.StaffName = ((Admins)admin).Username;
            else
                ViewBag.StaffName = ((SuperAdmins)admin).Username;

            return View();
        }

        /// <summary>
        /// Get carousel list (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetCarouselList(int page = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var result = _carouselBLL.GetPaged(page, pageSize);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get carousel by ID (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetCarousel(int id)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var carousel = _carouselBLL.GetById(id);
                if (carousel == null)
                    return JsonContent(new { success = false, message = "轮播内容不存在" });

                return JsonContent(new { success = true, data = carousel });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Add carousel item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult AddCarousel(HomepageCarousel carousel, HttpPostedFileBase MediaFile)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                // Validate and handle file upload
                if (MediaFile != null && MediaFile.ContentLength > 0)
                {
                    // Validate file type
                    string fileExtension = System.IO.Path.GetExtension(MediaFile.FileName).ToLower();

                    if (carousel.MediaType == "Image")
                    {
                        if (!AllowedImageExtensions.Contains(fileExtension))
                        {
                            return JsonContent(new { success = false, message = "图片格式不支持，请上传 jpg, jpeg, png 或 gif 格式" });
                        }
                    }
                    else if (carousel.MediaType == "Video")
                    {
                        if (!AllowedVideoExtensions.Contains(fileExtension))
                        {
                            return JsonContent(new { success = false, message = "视频格式不支持，请上传 mp4, webm 或 ogg 格式" });
                        }
                    }
                    else
                    {
                        return JsonContent(new { success = false, message = "无效的媒体类型" });
                    }

                    // Generate unique filename
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadPath = Server.MapPath("~/Uploads/Carousel/");

                    // Create directory if it doesn't exist
                    if (!System.IO.Directory.Exists(uploadPath))
                    {
                        System.IO.Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = System.IO.Path.Combine(uploadPath, fileName);
                    MediaFile.SaveAs(filePath);

                    // Set MediaUrl to relative path
                    carousel.MediaUrl = "/Uploads/Carousel/" + fileName;
                }
                else
                {
                    return JsonContent(new { success = false, message = "请选择要上传的文件" });
                }

                // Get admin ID
                int adminId = 0;
                if (staffRole == "admin")
                    adminId = ((Admins)Session["LoginStaff"]).AdminID;
                else
                    adminId = ((SuperAdmins)Session["LoginStaff"]).SuperAdminID;

                var (success, message) = _carouselBLL.Add(carousel, adminId);
                
                // 记录操作日志
                if (success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Create, $"添加轮播内容：{carousel.Title}", null, carousel.Title, "Success");
                    // 发送轮播图更新通知给所有用户
                    _notificationBLL.SendCarouselUpdatedNotification("add", carousel.Title ?? "新内容");
                }
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update carousel item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult UpdateCarousel(HomepageCarousel carousel, HttpPostedFileBase MediaFile)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                // Handle file upload if provided
                if (MediaFile != null && MediaFile.ContentLength > 0)
                {
                    // Validate file type
                    string fileExtension = System.IO.Path.GetExtension(MediaFile.FileName).ToLower();

                    if (carousel.MediaType == "Image")
                    {
                        if (!AllowedImageExtensions.Contains(fileExtension))
                        {
                            return JsonContent(new { success = false, message = "图片格式不支持，请上传 jpg, jpeg, png 或 gif 格式" });
                        }
                    }
                    else if (carousel.MediaType == "Video")
                    {
                        if (!AllowedVideoExtensions.Contains(fileExtension))
                        {
                            return JsonContent(new { success = false, message = "视频格式不支持，请上传 mp4, webm 或 ogg 格式" });
                        }
                    }
                    else
                    {
                        return JsonContent(new { success = false, message = "无效的媒体类型" });
                    }

                    // Get old file path to delete later
                    var oldCarousel = _carouselBLL.GetById(carousel.CarouselID);
                    string oldFilePath = null;
                    if (oldCarousel != null && !string.IsNullOrEmpty(oldCarousel.MediaUrl) && oldCarousel.MediaUrl.StartsWith("/Uploads/Carousel/"))
                    {
                        // Map the relative path and validate it's within our upload directory
                        string mappedPath = Server.MapPath("~" + oldCarousel.MediaUrl);
                        string uploadDir = Server.MapPath("~/Uploads/Carousel/");

                        // Ensure the resolved path is actually within the upload directory
                        if (mappedPath.StartsWith(uploadDir, StringComparison.OrdinalIgnoreCase))
                        {
                            oldFilePath = mappedPath;
                        }
                    }

                    // Generate unique filename
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadPath = Server.MapPath("~/Uploads/Carousel/");

                    // Create directory if it doesn't exist
                    if (!System.IO.Directory.Exists(uploadPath))
                    {
                        System.IO.Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = System.IO.Path.Combine(uploadPath, fileName);
                    MediaFile.SaveAs(filePath);

                    // Set MediaUrl to relative path
                    carousel.MediaUrl = "/Uploads/Carousel/" + fileName;

                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(oldFilePath) && System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch (System.IO.IOException ex)
                        {
                            // Log file deletion error but continue - file may be in use
                            System.Diagnostics.Debug.WriteLine($"Failed to delete old carousel file: {ex.Message}");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Log permission error but continue
                            System.Diagnostics.Debug.WriteLine($"No permission to delete old carousel file: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // Keep existing MediaUrl if no new file is uploaded
                    var existingCarousel = _carouselBLL.GetById(carousel.CarouselID);
                    if (existingCarousel != null)
                    {
                        carousel.MediaUrl = existingCarousel.MediaUrl;
                    }
                }

                var (success, message) = _carouselBLL.Update(carousel);
                
                // 记录操作日志
                if (success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Update, $"更新轮播内容：{carousel.Title}", carousel.CarouselID, carousel.Title, "Success");
                    // 发送轮播图更新通知给所有用户
                    _notificationBLL.SendCarouselUpdatedNotification("update", carousel.Title ?? "已有内容");
                }
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete carousel item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult DeleteCarousel(int id)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                // 获取轮播内容信息用于日志记录
                var carousel = _carouselBLL.GetById(id);
                string carouselTitle = carousel?.Title ?? $"ID:{id}";

                // Use HardDelete to permanently remove from database
                var (success, message) = _carouselBLL.HardDelete(id);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Delete, $"删除轮播内容：{carouselTitle}", id, carouselTitle, success ? "Success" : "Failed");
                
                // 发送轮播图更新通知给所有用户
                if (success)
                {
                    _notificationBLL.SendCarouselUpdatedNotification("delete", carouselTitle);
                }
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get maximum DisplayOrder for carousel (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetMaxCarouselOrder()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                int maxOrder = _carouselBLL.GetMaxDisplayOrder();
                return JsonContent(new { success = true, maxOrder = maxOrder });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Admin recyclable items management page
        /// </summary>
        [AdminPermission(AdminPermissions.HomepageManagement)]
        public ActionResult RecyclableItemsManagement()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
                return RedirectToAction("Login", "Staff");

            var admin = Session["LoginStaff"];
            if (staffRole == "admin")
                ViewBag.StaffName = ((Admins)admin).Username;
            else
                ViewBag.StaffName = ((SuperAdmins)admin).Username;

            return View();
        }

        /// <summary>
        /// Get recyclable items list (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetRecyclableItemsList(int page = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var query = new RecyclableQueryModel
                {
                    PageIndex = page,
                    PageSize = pageSize
                };
                var result = _recyclableItemBLL.GetPagedItems(query);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get recyclable item by ID (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetRecyclableItem(int id)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var item = _recyclableItemBLL.GetById(id);
                if (item == null)
                    return JsonContent(new { success = false, message = "可回收物品不存在" });

                return JsonContent(new { success = true, data = item });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Add recyclable item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult AddRecyclableItem(RecyclableItems item)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                var (success, message) = _recyclableItemBLL.Add(item);
                
                // 记录操作日志
                if (success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Create, $"添加可回收物品：{item.Name}", null, item.Name, "Success");
                }
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update recyclable item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult UpdateRecyclableItem(RecyclableItems item)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                var (success, message) = _recyclableItemBLL.Update(item);
                
                // 记录操作日志
                if (success)
                {
                    LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Update, $"更新可回收物品：{item.Name}", item.ItemId, item.Name, "Success");
                }
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete recyclable item (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult DeleteRecyclableItem(int id)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                    return JsonContent(new { success = false, message = "权限不足" });

                // 获取物品信息用于日志记录
                var item = _recyclableItemBLL.GetById(id);
                string itemName = item?.Name ?? $"ID:{id}";

                // Use HardDelete instead of soft delete
                var (success, message) = _recyclableItemBLL.HardDelete(id);
                
                // 记录操作日志
                LogAdminOperation(OperationLogBLL.Modules.HomepageManagement, OperationLogBLL.OperationTypes.Delete, $"删除可回收物品：{itemName}", id, itemName, success ? "Success" : "Failed");
                
                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get maximum SortOrder for recyclable items (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetMaxRecyclableItemOrder()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return JsonContent(new { success = false, message = "请先登录" });

                int maxOrder = _recyclableItemBLL.GetMaxSortOrder();
                return JsonContent(new { success = true, maxOrder = maxOrder });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region 反馈管理功能

        /// <summary>
        /// 反馈管理页面
        /// </summary>
        [HttpGet]
        [AdminPermission(AdminPermissions.FeedbackManagement)]
        public ActionResult FeedbackManagement()
        {
            // 检查登录状态
            if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
            {
                TempData["ErrorMessage"] = "您没有权限访问该页面";
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 获取所有反馈列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetAllFeedbacks(string status, string feedbackType)
        {
            try
            {
                // 检查登录状态和权限
                if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                {
                    return JsonContent(new { success = false, message = "无权限" });
                }

                // 获取反馈列表
                var feedbacks = _feedbackBLL.GetAllFeedbacks(status, feedbackType);

                return JsonContent(new { success = true, feedbacks = feedbacks });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新反馈状态（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult UpdateFeedbackStatus(int feedbackId, string status, string adminReply)
        {
            try
            {
                // 检查登录状态和权限
                if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                {
                    return JsonContent(new { success = false, message = "无权限" });
                }

                // 获取反馈信息用于通知
                var feedback = _feedbackBLL.GetFeedbackById(feedbackId);
                string feedbackSubject = feedback?.Subject ?? $"反馈#{feedbackId}";

                // 更新反馈状态
                var (success, message) = _feedbackBLL.UpdateFeedbackStatus(feedbackId, status, adminReply);

                // 记录操作日志
                if (success)
                {
                    string operationType = !string.IsNullOrEmpty(adminReply) ? OperationLogBLL.OperationTypes.Reply : OperationLogBLL.OperationTypes.Update;
                    string description = !string.IsNullOrEmpty(adminReply) ? $"回复反馈 #{feedbackId}" : $"更新反馈状态为：{status}";
                    LogAdminOperation(OperationLogBLL.Modules.FeedbackManagement, operationType, description, feedbackId, null, "Success");

                    // 发送反馈回复通知（当有管理员回复或状态更新为已完成时）
                    if (!string.IsNullOrEmpty(adminReply) || status == "已完成")
                    {
                        _notificationBLL.SendFeedbackRepliedNotification(feedbackId, feedbackSubject);
                    }
                }

                return JsonContent(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region 日志管理功能

        /// <summary>
        /// 日志管理页面
        /// </summary>
        [HttpGet]
        [AdminPermission(AdminPermissions.LogManagement)]
        public ActionResult LogManagement()
        {
            // 检查登录状态
            if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var staffRole = Session["StaffRole"] as string;
            if (staffRole != "admin" && staffRole != "superadmin")
            {
                TempData["ErrorMessage"] = "您没有权限访问该页面";
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 获取操作日志列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetOperationLogs(int page = 1, int pageSize = 20, string module = null, string operationType = null, string startDate = null, string endDate = null, string searchTerm = null)
        {
            try
            {
                // 检查登录状态和权限
                if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                {
                    return JsonContent(new { success = false, message = "无权限" });
                }

                DateTime? start = null;
                DateTime? end = null;

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime startParsed))
                {
                    start = startParsed;
                }
                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime endParsed))
                {
                    end = endParsed.AddDays(1).AddSeconds(-1); // Include the entire end day
                }

                var logs = _operationLogBLL.GetLogs(page, pageSize, module, operationType, start, end, searchTerm);

                return JsonContent(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取日志统计信息（AJAX）
        /// </summary>
        [HttpGet]
        public ContentResult GetLogStatistics()
        {
            try
            {
                // 检查登录状态和权限
                if (Session["LoginStaff"] == null || Session["StaffRole"] == null)
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var staffRole = Session["StaffRole"] as string;
                if (staffRole != "admin" && staffRole != "superadmin")
                {
                    return JsonContent(new { success = false, message = "无权限" });
                }

                var stats = _operationLogBLL.GetLogStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 导出操作日志
        /// </summary>
        [HttpGet]
        public ActionResult ExportOperationLogs(string module = null, string operationType = null, string startDate = null, string endDate = null, string searchTerm = null)
        {
            // Permission check
            if (Session["StaffRole"] == null || (Session["StaffRole"].ToString() != "admin" && Session["StaffRole"].ToString() != "superadmin"))
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                DateTime? start = null;
                DateTime? end = null;

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime startParsed))
                {
                    start = startParsed;
                }
                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime endParsed))
                {
                    end = endParsed.AddDays(1).AddSeconds(-1);
                }

                var logs = _operationLogBLL.GetLogsForExport(module, operationType, start, end, searchTerm);

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add UTF-8 BOM for proper Excel display of Chinese characters
                csv.Append("\uFEFF");

                // Add header
                csv.AppendLine("日志ID,操作时间,管理员ID,管理员用户名,模块,操作类型,操作描述,目标ID,目标名称,IP地址,结果");

                // Add data rows
                foreach (var log in logs)
                {
                    var moduleDisplay = OperationLogBLL.GetModuleDisplayName(log.Module);
                    var operationDisplay = OperationLogBLL.GetOperationTypeDisplayName(log.OperationType);
                    var resultDisplay = log.Result == "Success" ? "成功" : "失败";

                    csv.AppendLine($"{log.LogID},{EscapeCsvField(log.OperationTime.ToString("yyyy-MM-dd HH:mm:ss"))},{log.AdminID},{EscapeCsvField(log.AdminUsername)},{EscapeCsvField(moduleDisplay)},{EscapeCsvField(operationDisplay)},{EscapeCsvField(log.Description)},{log.TargetID},{EscapeCsvField(log.TargetName)},{EscapeCsvField(log.IPAddress)},{EscapeCsvField(resultDisplay)}");
                }

                // Generate file
                var fileName = $"操作日志_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                // 记录导出操作
                LogAdminOperation(OperationLogBLL.Modules.LogManagement, OperationLogBLL.OperationTypes.Export, $"导出操作日志，共{logs.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("LogManagement");
            }
        }

        #endregion

        #region 运输人员管理功能

        /// <summary>
        /// 管理员 - 运输人员管理页面
        /// </summary>
        [AdminPermission(AdminPermissions.TransporterManagement)]
        public ActionResult TransporterManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取运输人员列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetTransporters(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            try
            {
                var result = _adminBLL.GetAllTransporters(page, pageSize, searchTerm, isActive);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取运输人员详情（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetTransporterDetails(int transporterId)
        {
            try
            {
                var transporter = _adminBLL.GetTransporterById(transporterId);
                return JsonContent(new {
                    success = true,
                    data = transporter
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 添加运输人员（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddTransporter(Transporters transporter, string password)
        {
            try
            {
                var result = _adminBLL.AddTransporter(transporter, password);
                
                if (result.Success)
                {
                    LogAdminOperation("TransporterManagement", OperationLogBLL.OperationTypes.Create, $"添加运输人员：{transporter.Username}", null, transporter.Username, "Success");
                }
                else
                {
                    LogAdminOperation("TransporterManagement", OperationLogBLL.OperationTypes.Create, $"添加运输人员失败：{transporter.Username}", null, transporter.Username, "Failed");
                }
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 更新运输人员信息（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateTransporter(Transporters transporter)
        {
            try
            {
                var result = _adminBLL.UpdateTransporter(transporter);
                
                LogAdminOperation("TransporterManagement", OperationLogBLL.OperationTypes.Update, 
                    result.Success ? $"更新运输人员信息：{transporter.Username}" : $"更新运输人员信息失败：{transporter.Username}", 
                    transporter.TransporterID, transporter.Username, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 删除运输人员（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteTransporter(int transporterId)
        {
            try
            {
                var transporter = _adminBLL.GetTransporterById(transporterId);
                string transporterName = transporter?.Username ?? $"ID:{transporterId}";
                
                var result = _adminBLL.DeleteTransporter(transporterId);
                
                LogAdminOperation("TransporterManagement", OperationLogBLL.OperationTypes.Delete, $"删除运输人员：{transporterName}", transporterId, transporterName, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取运输人员统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetTransporterStatistics()
        {
            try
            {
                var stats = _adminBLL.GetTransporterStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 导出运输人员数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportTransporters(string searchTerm = null, bool? isActive = null)
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                var transporters = _adminBLL.GetAllTransportersForExport(searchTerm, isActive);

                var csv = new System.Text.StringBuilder();
                csv.Append("\uFEFF");
                csv.AppendLine("运输人员ID,用户名,姓名,手机号,车牌号,区域,评分,是否可接单,账号状态,注册日期");

                foreach (var t in transporters)
                {
                    var availableStatus = t.Available ? "可接单" : "不可接单";
                    var activeStatus = t.IsActive ? "激活" : "禁用";
                    var createdDate = t.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

                    csv.AppendLine($"{t.TransporterID},{EscapeCsvField(t.Username)},{EscapeCsvField(t.FullName ?? "-")},{EscapeCsvField(t.PhoneNumber)},{EscapeCsvField(t.VehiclePlateNumber)},{EscapeCsvField(t.Region)},{EscapeCsvField(t.Rating?.ToString("F1") ?? "0.0")},{EscapeCsvField(availableStatus)},{EscapeCsvField(activeStatus)},{EscapeCsvField(createdDate)}");
                }

                var fileName = $"运输人员数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                LogAdminOperation("TransporterManagement", OperationLogBLL.OperationTypes.Export, $"导出运输人员数据，共{transporters.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("TransporterManagement");
            }
        }

        #endregion

        #region 基地人员管理功能

        /// <summary>
        /// 管理员 - 基地人员管理页面
        /// </summary>
        [AdminPermission(AdminPermissions.SortingCenterWorkerManagement)]
        public ActionResult SortingCenterWorkerManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取基地人员列表（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSortingCenterWorkers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            try
            {
                var result = _adminBLL.GetAllSortingCenterWorkers(page, pageSize, searchTerm, isActive);
                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取基地人员详情（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSortingCenterWorkerDetails(int workerId)
        {
            try
            {
                var worker = _adminBLL.GetSortingCenterWorkerById(workerId);
                return JsonContent(new {
                    success = true,
                    data = worker
                });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 添加基地人员（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddSortingCenterWorker(SortingCenterWorkers worker, string password)
        {
            try
            {
                var result = _adminBLL.AddSortingCenterWorker(worker, password);
                
                if (result.Success)
                {
                    LogAdminOperation("SortingCenterWorkerManagement", OperationLogBLL.OperationTypes.Create, $"添加基地人员：{worker.Username}", null, worker.Username, "Success");
                }
                else
                {
                    LogAdminOperation("SortingCenterWorkerManagement", OperationLogBLL.OperationTypes.Create, $"添加基地人员失败：{worker.Username}", null, worker.Username, "Failed");
                }
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 更新基地人员信息（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateSortingCenterWorker(SortingCenterWorkers worker)
        {
            try
            {
                var result = _adminBLL.UpdateSortingCenterWorker(worker);
                
                LogAdminOperation("SortingCenterWorkerManagement", OperationLogBLL.OperationTypes.Update, 
                    result.Success ? $"更新基地人员信息：{worker.Username}" : $"更新基地人员信息失败：{worker.Username}", 
                    worker.WorkerID, worker.Username, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 删除基地人员（API）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteSortingCenterWorker(int workerId)
        {
            try
            {
                var worker = _adminBLL.GetSortingCenterWorkerById(workerId);
                string workerName = worker?.Username ?? $"ID:{workerId}";
                
                var result = _adminBLL.DeleteSortingCenterWorker(workerId);
                
                LogAdminOperation("SortingCenterWorkerManagement", OperationLogBLL.OperationTypes.Delete, $"删除基地人员：{workerName}", workerId, workerName, result.Success ? "Success" : "Failed");
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 获取基地人员统计信息（API）
        /// </summary>
        [HttpGet]
        public ContentResult GetSortingCenterWorkerStatistics()
        {
            try
            {
                var stats = _adminBLL.GetSortingCenterWorkerStatistics();
                return JsonContent(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 管理员 - 导出基地人员数据到CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportSortingCenterWorkers(string searchTerm = null, bool? isActive = null)
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            try
            {
                var workers = _adminBLL.GetAllSortingCenterWorkersForExport(searchTerm, isActive);

                var csv = new System.Text.StringBuilder();
                csv.Append("\uFEFF");
                csv.AppendLine("人员ID,用户名,姓名,手机号,班次,评分,是否可用,账号状态,注册日期");

                foreach (var w in workers)
                {
                    var availableStatus = w.Available ? "可用" : "不可用";
                    var activeStatus = w.IsActive ? "激活" : "禁用";
                    var createdDate = w.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

                    csv.AppendLine($"{w.WorkerID},{EscapeCsvField(w.Username)},{EscapeCsvField(w.FullName ?? "-")},{EscapeCsvField(w.PhoneNumber)},{EscapeCsvField(w.ShiftType)},{EscapeCsvField(w.Rating?.ToString("F1") ?? "0.0")},{EscapeCsvField(availableStatus)},{EscapeCsvField(activeStatus)},{EscapeCsvField(createdDate)}");
                }

                var fileName = $"基地人员数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

                LogAdminOperation("SortingCenterWorkerManagement", OperationLogBLL.OperationTypes.Export, $"导出基地人员数据，共{workers.Count}条记录");

                return File(fileBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("SortingCenterWorkerManagement");
            }
        }

        #endregion

        #region 运输人员账号管理功能

        /// <summary>
        /// 运输人员 - 个人中心页面
        /// </summary>
        public ActionResult TransporterProfile()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            var transporter = (Transporters)Session["LoginStaff"];
            
            // 重新从数据库获取最新信息
            var latestTransporter = _staffBLL.GetTransporterById(transporter.TransporterID);
            if (latestTransporter != null)
            {
                Session["LoginStaff"] = latestTransporter;
                return View(latestTransporter);
            }
            
            return View(transporter);
        }

        /// <summary>
        /// 运输人员 - 显示编辑个人信息页面
        /// </summary>
        [HttpGet]
        public ActionResult TransporterEditProfile()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            var transporter = (Transporters)Session["LoginStaff"];
            
            var model = new TransporterProfileViewModel
            {
                FullName = transporter.FullName,
                PhoneNumber = transporter.PhoneNumber,
                IDNumber = transporter.IDNumber,
                VehiclePlateNumber = transporter.VehiclePlateNumber,
                LicenseNumber = transporter.LicenseNumber,
                Region = transporter.Region
            };

            return View(model);
        }

        /// <summary>
        /// 运输人员 - 处理编辑个人信息提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TransporterEditProfile(TransporterProfileViewModel model)
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var transporter = (Transporters)Session["LoginStaff"];
            var result = _staffBLL.UpdateTransporterProfile(transporter.TransporterID, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                
                // 更新Session中的数据
                var updatedTransporter = _staffBLL.GetTransporterById(transporter.TransporterID);
                if (updatedTransporter != null)
                {
                    Session["LoginStaff"] = updatedTransporter;
                }
                
                return RedirectToAction("TransporterProfile");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        /// <summary>
        /// 运输人员 - 显示修改密码页面
        /// </summary>
        [HttpGet]
        public ActionResult TransporterChangePassword()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// 运输人员 - 处理修改密码提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TransporterChangePassword(ChangePasswordViewModel model)
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "transporter")
                return RedirectToAction("Login", "Staff");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var transporter = (Transporters)Session["LoginStaff"];
            var result = _staffBLL.ChangeTransporterPassword(transporter.TransporterID, model.CurrentPassword, model.NewPassword);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                // 清除Session，要求重新登录
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Staff");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        #endregion

        #region 基地工作人员账号管理

        /// <summary>
        /// 基地工作人员 - 个人中心
        /// </summary>
        public ActionResult SortingCenterWorkerProfile()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            
            // 重新从数据库获取最新信息
            var latestWorker = _staffBLL.GetSortingCenterWorkerById(worker.WorkerID);
            if (latestWorker != null)
            {
                Session["LoginStaff"] = latestWorker;
                return View(latestWorker);
            }
            
            return View(worker);
        }

        /// <summary>
        /// 基地工作人员 - 显示编辑个人信息页面
        /// </summary>
        [HttpGet]
        public ActionResult SortingCenterWorkerEditProfile()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            
            var model = new SortingCenterWorkerProfileViewModel
            {
                FullName = worker.FullName,
                PhoneNumber = worker.PhoneNumber,
                IDNumber = worker.IDNumber,
                Position = worker.Position,
                WorkStation = worker.WorkStation,
                Specialization = worker.Specialization,
                ShiftType = worker.ShiftType
            };

            return View(model);
        }

        /// <summary>
        /// 基地工作人员 - 处理编辑个人信息提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortingCenterWorkerEditProfile(SortingCenterWorkerProfileViewModel model)
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            var result = _staffBLL.UpdateSortingCenterWorkerProfile(worker.WorkerID, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                
                // 更新Session中的数据
                var updatedWorker = _staffBLL.GetSortingCenterWorkerById(worker.WorkerID);
                if (updatedWorker != null)
                {
                    Session["LoginStaff"] = updatedWorker;
                }
                
                return RedirectToAction("SortingCenterWorkerProfile");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        /// <summary>
        /// 基地工作人员 - 显示修改密码页面
        /// </summary>
        [HttpGet]
        public ActionResult SortingCenterWorkerChangePassword()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// 基地工作人员 - 处理修改密码提交
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortingCenterWorkerChangePassword(ChangePasswordViewModel model)
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            var result = _staffBLL.ChangeSortingCenterWorkerPassword(worker.WorkerID, model.CurrentPassword, model.NewPassword);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                // 清除Session，要求重新登录
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Staff");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        #endregion

        #region 基地管理功能

        /// <summary>
        /// 基地管理主页
        /// </summary>
        public ActionResult BaseManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            ViewBag.StaffName = worker.Username;
            ViewBag.DisplayName = "基地工作人员";
            ViewBag.StaffRole = "sortingcenterworker";

            return View();
        }

        /// <summary>
        /// 基地运输管理页面（查看运输中的订单）
        /// </summary>
        public ActionResult BaseTransportationManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            ViewBag.StaffName = worker.Username;
            ViewBag.DisplayName = "基地工作人员";
            ViewBag.StaffRole = "sortingcenterworker";

            return View();
        }

        /// <summary>
        /// 获取运输中的订单列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetInTransitOrders()
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var orders = _warehouseReceiptBLL.GetInTransitOrders();
                return JsonContent(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"获取运输中订单失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 获取已完成的运输单列表（AJAX）- 用于仓库管理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetCompletedTransportOrders()
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var orders = _warehouseReceiptBLL.GetCompletedTransportOrders();
                return JsonContent(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"获取已完成运输单失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 基地仓库管理页面（入库单管理）
        /// </summary>
        public ActionResult BaseWarehouseManagement()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                return RedirectToAction("Login", "Staff");

            var worker = (SortingCenterWorkers)Session["LoginStaff"];
            ViewBag.StaffName = worker.Username;
            ViewBag.DisplayName = "基地工作人员";
            ViewBag.StaffRole = "sortingcenterworker";

            // 创建视图模型并加载数据
            var viewModel = new BaseWarehouseManagementViewModel();

            try
            {
                // 加载已完成的运输单（待入库）
                var orders = _warehouseReceiptBLL.GetCompletedTransportOrders();
                viewModel.CompletedTransportOrders = orders?.ToList() ?? new List<TransportNotificationViewModel>();

                // 加载入库记录
                var receipts = _warehouseReceiptBLL.GetWarehouseReceipts(1, 50, null, null);
                viewModel.WarehouseReceipts = receipts?.ToList() ?? new List<WarehouseReceiptViewModel>();

                // 加载当前库存汇总信息
                var inventoryBll = new InventoryBLL();
                var inventorySummary = inventoryBll.GetInventorySummary(null, "Warehouse");
                if (inventorySummary != null && inventorySummary.Any())
                {
                    viewModel.InventorySummary = inventorySummary.Select(s => new InventorySummaryViewModel
                    {
                        CategoryKey = s.CategoryKey,
                        CategoryName = s.CategoryName,
                        TotalWeight = s.TotalWeight,
                        TotalPrice = s.TotalPrice
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不中断页面加载
                System.Diagnostics.Debug.WriteLine($"加载仓库数据失败：{ex.Message}");
            }

            return View(viewModel);
        }

        /// <summary>
        /// 获取入库单列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetWarehouseReceipts(int page = 1, int pageSize = 20, string status = null)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var worker = (SortingCenterWorkers)Session["LoginStaff"];
                var receipts = _warehouseReceiptBLL.GetWarehouseReceipts(page, pageSize, status, null);
                
                return JsonContent(new { success = true, data = receipts });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"获取入库单列表失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 创建入库单（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult CreateWarehouseReceipt(int transportOrderId, decimal totalWeight, string itemCategories, string notes)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                var worker = (SortingCenterWorkers)Session["LoginStaff"];

                // 创建入库单（BLL层会进行所有必要的验证）
                var (success, message, receiptId, receiptNumber) = _warehouseReceiptBLL.CreateWarehouseReceipt(
                    transportOrderId, 
                    worker.WorkerID, 
                    totalWeight, 
                    itemCategories, 
                    notes);

                if (success)
                {
                    return JsonContent(new 
                    { 
                        success = true, 
                        message = message,
                        receiptId = receiptId,
                        receiptNumber = receiptNumber
                    });
                }
                else
                {
                    return JsonContent(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"创建入库单失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 检查运输单是否已创建入库单（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult CheckWarehouseReceipt(int transportOrderId)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                bool hasReceipt = _warehouseReceiptBLL.HasWarehouseReceipt(transportOrderId);
                return JsonContent(new { success = true, hasReceipt = hasReceipt });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"检查失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 获取仓库库存汇总 - 基地工作人员端（AJAX）
        /// Get warehouse inventory summary for base staff
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetBaseWarehouseInventorySummary()
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                // 使用InventoryBLL获取仓库类型的库存数据
                var inventoryBll = new InventoryBLL();
                var summary = inventoryBll.GetInventorySummary(null, "Warehouse");

                var result = summary.Select(s => new
                {
                    categoryKey = s.CategoryKey,
                    categoryName = s.CategoryName,
                    totalWeight = s.TotalWeight,
                    totalPrice = s.TotalPrice
                }).ToList();

                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"获取库存汇总失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 获取仓库库存明细 - 基地工作人员端（AJAX）
        /// Get warehouse inventory detail for base staff
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ContentResult GetBaseWarehouseInventoryDetail(int page = 1, int pageSize = 20, string categoryKey = null)
        {
            try
            {
                if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "sortingcenterworker")
                {
                    return JsonContent(new { success = false, message = "请先登录" });
                }

                // 使用InventoryBLL获取仓库类型的库存明细数据（包含回收员信息）
                var inventoryBll = new InventoryBLL();
                var result = inventoryBll.GetInventoryDetailWithRecycler(page, pageSize, categoryKey, "Warehouse");

                return JsonContent(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, message = $"获取库存明细失败：{ex.Message}" });
            }
        }

        #endregion
    }
}
