using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using recycling.Model;
using recycling.BLL;

namespace recycling.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        private UserBLL _userBLL = new UserBLL();
        // 依赖BLL层，与UserController依赖UserBLL的方式一致
        private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();
        private readonly MessageBLL _messageBLL = new MessageBLL();
        private readonly OrderBLL _orderBLL = new OrderBLL();

        [HttpGet]
        public ActionResult Index(RecyclableQueryModel query)
        {
            // 检查是否是工作人员登录
            if (Session["LoginStaff"] != null)
            {
                var staffRole = Session["StaffRole"] as string;

                // 根据角色使用不同的布局
                switch (staffRole)
                {
                    case "recycler":
                        return RedirectToAction("RecyclerDashboard", "Staff");
                    case "admin":
                        return RedirectToAction("AdminDashboard", "Staff");
                    case "superadmin":
                        return RedirectToAction("SuperAdminDashboard", "Staff");
                    default:
                        break;
                }
            }

            try
            {
                // 确保数据存在
                _recyclableItemBLL.EnsureDataExists();

                // 获取品类列表供下拉框使用
                ViewBag.CategoryList = _recyclableItemBLL.GetAllCategories();

                // 设置默认值
                if (query.PageIndex < 1) query.PageIndex = 1;
                if (query.PageSize < 1) query.PageSize = 6;

                // 获取分页数据
                var pageResult = _recyclableItemBLL.GetPagedItems(query);

                // 保存查询条件用于视图
                ViewBag.QueryModel = query;

                return View(pageResult);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View(new PagedResult<RecyclableItems>());
            }
        }

        /// <summary>
        /// 订单管理页面
        /// </summary>
        [HttpGet]
        public ActionResult Order()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            var user = (Users)Session["LoginUser"];
            var orderBLL = new OrderBLL();

            try
            {
                // 获取订单统计信息
                var statistics = orderBLL.GetOrderStatistics(user.UserID);
                ViewBag.OrderStatistics = statistics;

                // 获取全部订单（默认显示）
                var orders = orderBLL.GetUserOrders(user.UserID, "all");
                ViewBag.CurrentStatus = "all";
                ViewBag.Orders = orders;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "加载订单失败：" + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// 根据状态筛选订单（AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult GetOrdersByStatus(string status)
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" });
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var orderBLL = new OrderBLL();
                var orders = orderBLL.GetUserOrders(user.UserID, status);

                // 转换为前端需要的格式
                var result = orders.Select(order => new
                {
                    id = order.AppointmentID,
                    number = $"AP{order.AppointmentID:D6}",
                    type = GetAppointmentTypeDisplayName(order.AppointmentType),
                    date = order.AppointmentDate.ToString("yyyy-MM-dd"),
                    timeSlot = GetTimeSlotDisplayName(order.TimeSlot),
                    weight = order.EstimatedWeight,
                    price = order.EstimatedPrice?.ToString("F2") ?? "待评估",
                    status = order.Status,
                    statusBadge = GetStatusBadgeClass(order.Status),
                    categories = order.CategoryNames?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0],
                    contactName = order.ContactName,
                    contactPhone = order.ContactPhone,
                    address = order.Address,
                    isUrgent = order.IsUrgent,
                    specialInstructions = order.SpecialInstructions,
                    createdDate = order.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return Json(new { success = true, orders = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        [HttpPost]
        public JsonResult GetOrderDetail(int appointmentId)
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" });
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var orderBLL = new OrderBLL();
                var orderDetail = orderBLL.GetOrderDetail(appointmentId, user.UserID);

                // 转换为前端需要的格式
                var result = new
                {
                    success = true,
                    order = new
                    {
                        id = orderDetail.Appointment.AppointmentID,
                        number = $"AP{orderDetail.Appointment.AppointmentID:D6}",
                        type = GetAppointmentTypeDisplayName(orderDetail.Appointment.AppointmentType),
                        date = orderDetail.Appointment.AppointmentDate.ToString("yyyy年MM月dd日"),
                        timeSlot = GetTimeSlotDisplayName(orderDetail.Appointment.TimeSlot),
                        weight = orderDetail.Appointment.EstimatedWeight,
                        price = orderDetail.Appointment.EstimatedPrice?.ToString("F2") ?? "待评估",
                        status = orderDetail.Appointment.Status,
                        contactName = orderDetail.Appointment.ContactName,
                        contactPhone = orderDetail.Appointment.ContactPhone,
                        address = orderDetail.Appointment.Address,
                        isUrgent = orderDetail.Appointment.IsUrgent,
                        specialInstructions = orderDetail.Appointment.SpecialInstructions,
                        createdDate = orderDetail.Appointment.CreatedDate.ToString("yyyy年MM月dd日 HH:mm"),
                        updatedDate = orderDetail.Appointment.UpdatedDate.ToString("yyyy年MM月dd日 HH:mm"),
                        recyclerID = orderDetail.Appointment.RecyclerID
                    },
                    categories = orderDetail.Categories.Select(c => new
                    {
                        name = c.CategoryName,
                        key = c.CategoryKey,
                        questionsAnswers = string.IsNullOrEmpty(c.QuestionsAnswers) ?
                            new Dictionary<string, string>() :
                            Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(c.QuestionsAnswers)
                    }).ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 辅助方法
        private string GetAppointmentTypeDisplayName(string type)
        {
            var types = AppointmentTypes.AllTypes;
            return types.ContainsKey(type) ? types[type] : type;
        }

        private string GetTimeSlotDisplayName(string timeSlot)
        {
            var slots = TimeSlots.AllSlots;
            return slots.ContainsKey(timeSlot) ? slots[timeSlot] : timeSlot;
        }

        private string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "已预约":
                    return "status-pending-badge";
                case "进行中":
                    return "status-confirmed-badge";
                case "已完成":
                    return "status-completed-badge";
                case "已取消":
                    return "status-cancelled-badge";
                default:
                    return "status-pending-badge";
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        [HttpPost]
        public JsonResult CancelOrder(int appointmentId)
        {
            if (Session["LoginUser"] == null)
            {
                return Json(new { success = false, message = "请先登录" });
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var orderBLL = new OrderBLL();
                var result = orderBLL.CancelOrder(appointmentId, user.UserID);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Message()
        {
            // 强制登录：如果未登录则引导到登录选择页（或直接到登录页）
            if (Session["LoginUser"] == null)
            {
                // 记录想要返回的地址以便登录后跳回（可选）
                TempData["ReturnUrl"] = Url.Action("Message", "Home");
                return RedirectToAction("LoginSelect", "Home"); // 或改为 RedirectToAction("Login","User") 根据你的流程
            }

            // 已登录，传递一些必要信息到视图（可在视图显示用户名等）
            var user = (recycling.Model.Users)Session["LoginUser"];
            ViewBag.UserName = user?.Username ?? "";

            return View();
        }
        public ActionResult Help()
        {
            return View();
        }
        public ActionResult Feedback()
        {
            return View();
        }
        /// <summary>
        /// 个人中心主页
        /// </summary>
        [HttpGet]
        public new ActionResult Profile()
        {
            // 检查登录状态 - 如果未登录，跳转到登录选择页
            if (Session["LoginUser"] == null)
            {
                TempData["ReturnUrl"] = Url.Action("Profile", "Home");
                return RedirectToAction("LoginSelect", "Home");
            }

            var user = (Users)Session["LoginUser"];

            // 从数据库重新获取最新用户信息，确保数据同步
            var currentUser = _userBLL.GetUserById(user.UserID);
            if (currentUser != null)
            {
                Session["LoginUser"] = currentUser; // 更新Session中的用户信息
            }

            return View(currentUser ?? user);
        }

        /// <summary>
        /// 显示编辑个人信息页面
        /// </summary>
        [HttpGet]
        public ActionResult EditProfile()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            var user = (Users)Session["LoginUser"];
            var model = new UpdateProfileViewModel
            {
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return View(model);
        }

        /// <summary>
        /// 处理个人信息更新
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UpdateProfileViewModel model)
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = (Users)Session["LoginUser"];
            try
            {
                string errorMsg = _userBLL.UpdateUserProfile(user.UserID, model);
                if (errorMsg != null)
                {
                    ModelState.AddModelError("", errorMsg);
                    return View(model);
                }

                // 更新成功后，重新从数据库获取用户信息并更新Session
                var updatedUser = _userBLL.GetUserById(user.UserID);
                Session["LoginUser"] = updatedUser;

                TempData["SuccessMessage"] = "个人信息更新成功";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "更新失败：" + ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// 显示修改密码页面
        /// </summary>
        [HttpGet]
        public ActionResult ChangePassword()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// 处理密码修改（修改成功后强制重新登录）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = (Users)Session["LoginUser"];
            try
            {
                string errorMsg = _userBLL.ChangePassword(user.UserID, model);
                if (errorMsg != null)
                {
                    ModelState.AddModelError("", errorMsg);
                    return View(model);
                }

                // 密码修改成功，清除Session强制重新登录
                Session.Clear();
                Session.Abandon();

                // 设置成功消息，并重定向到登录页
                TempData["SuccessMessage"] = "密码修改成功，请使用新密码重新登录";
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "密码修改失败：" + ex.Message);
                return View(model);
            }
        }

        public ActionResult LoginSelect()
        {
            return View();
        }
        /// <summary>
        /// 检查用户登录状态（AJAX调用）
        /// </summary>
        [HttpPost]
        public JsonResult CheckLoginStatus()
        {
            bool isLoggedIn = Session["LoginUser"] != null || Session["LoginStaff"] != null;

            return Json(new
            {
                isLoggedIn = isLoggedIn,
                userType = Session["LoginUser"] != null ? "user" :
                          Session["LoginStaff"] != null ? "staff" : "none"
            });
        }

        /// <summary>
        /// 用户消息中心页面（显示所有进行中订单的聊天入口）
        /// </summary>
        public ActionResult UserMessageCenter()
        {
            if (Session["LoginUser"] == null)
                return RedirectToAction("Login", "Home");

            var user = (Users)Session["LoginUser"];
            ViewBag.UserName = user.Username;

            // 获取用户的进行中订单及最新消息
            var messageBLL = new MessageBLL();
            var messages = messageBLL.GetUserMessages(user.UserID);
            return View(messages);
        }

        /// <summary>
        /// 获取订单聊天记录（用户端，AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult GetUserOrderConversation(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var messageBLL = new MessageBLL();
                var messages = messageBLL.GetOrderMessages(orderId); // 复用现有获取订单消息方法
                return Json(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 用户发送消息给回收员
        /// </summary>
        [HttpPost]
        public JsonResult UserSendMessageToRecycler(SendMessageRequest request)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var user = (Users)Session["LoginUser"];
                request.SenderID = user.UserID;

                var messageBLL = new MessageBLL();
                var result = messageBLL.UserSendMessage(request); // 调用修正后的方法
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"发送失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 标记用户接收的消息为已读
        /// </summary>
        [HttpPost]
        public JsonResult MarkUserMessagesAsRead(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var user = (Users)Session["LoginUser"];
                var messageBLL = new MessageBLL();
                bool result = messageBLL.MarkMessagesAsRead(orderId, "user", user.UserID);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SendMessage(SendMessageRequest request)
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                // 设置发送者信息（用户端）
                request.SenderType = "user";
                request.SenderID = ((Users)Session["LoginUser"]).UserID;

                // 验证订单归属（确保用户只能给自己的订单发消息）
                var order = _orderBLL.GetOrderDetail(request.OrderID, request.SenderID);
                if (order == null)
                {
                    return Json(new { success = false, message = "无权操作此订单" });
                }

                var result = _messageBLL.SendMessage(request);

                // 发送成功后，不需要返回完整消息列表，但为方便可返回 success
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"发送失败：{ex.Message}" });
            }
        }

        // 标记消息为已读
        [HttpPost]
        public JsonResult MarkAsRead(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var user = (Users)Session["LoginUser"];
                bool result = _messageBLL.MarkMessagesAsRead(orderId, "user", user.UserID);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 获取用户的历史会话列表（分页）
        [HttpPost]
        public JsonResult GetUserConversations(int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var user = (Users)Session["LoginUser"];
                var conversationBLL = new ConversationBLL();
                var convs = conversationBLL.GetUserConversations(user.UserID, pageIndex, pageSize);

                // 将 DateTime 转为 ISO 字符串
                var result = convs.Select(c => new
                {
                    conversationId = c.ConversationID,
                    orderId = c.OrderID,
                    orderNumber = c.OrderNumber,
                    recyclerId = c.RecyclerID,
                    recyclerName = c.RecyclerName,
                    createdTime = c.CreatedTime.HasValue ? c.CreatedTime.Value.ToString("o") : string.Empty,
                    endedTime = c.EndedTime.HasValue ? c.EndedTime.Value.ToString("o") : string.Empty,
                    status = c.Status
                }).ToList();

                return Json(new { success = true, conversations = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 获取某次已结束会话的历史消息（按 EndedTime 截取）
        [HttpPost]
        public JsonResult GetConversationMessagesBeforeEnd(int orderId, string endedTime)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                if (orderId <= 0 || string.IsNullOrWhiteSpace(endedTime))
                    return Json(new { success = false, message = "参数不完整" });

                if (!DateTime.TryParse(endedTime, out DateTime et))
                    return Json(new { success = false, message = "结束时间格式错误" });

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

                return Json(new { success = true, messages = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}