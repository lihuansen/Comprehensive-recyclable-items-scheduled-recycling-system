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
        private readonly UserBLL _userBLL = new UserBLL();
        // 依赖BLL层，与UserController依赖UserBLL的方式一致
        private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();
        private readonly MessageBLL _messageBLL = new MessageBLL();
        private readonly OrderBLL _orderBLL = new OrderBLL();
        private readonly StaffBLL _staffBLL = new StaffBLL();
        private readonly HomepageCarouselBLL _carouselBLL = new HomepageCarouselBLL();
        private readonly FeedbackBLL _feedbackBLL = new FeedbackBLL();
        private readonly UserNotificationBLL _notificationBLL = new UserNotificationBLL();

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
                // 获取轮播内容
                ViewBag.CarouselItems = _carouselBLL.GetAllActive();

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

                // 发送订单取消通知
                if (result.Success)
                {
                    _notificationBLL.SendOrderCancelledNotification(user.UserID, appointmentId);
                }

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
            
            // 获取未读通知数量
            ViewBag.UnreadCount = _notificationBLL.GetUnreadCount(user.UserID);

            return View();
        }
        public ActionResult Help()
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

                var user = (Users)Session["LoginUser"];
                var messageBLL = new MessageBLL();
                var messages = messageBLL.GetOrderMessages(orderId);
                
                // 格式化消息以匹配前端期望的格式
                var formattedMessages = messages.Select(m => new
                {
                    messageId = m.MessageID,
                    orderId = m.OrderID,
                    senderType = m.SenderType?.ToLower() ?? "",
                    senderId = m.SenderID,
                    senderName = m.SenderType?.ToLower() == "user" ? "用户" : "回收员",
                    content = m.Content ?? "",
                    sentTime = m.SentTime.HasValue ? m.SentTime.Value.ToString("o") : "",
                    isRead = m.IsRead
                }).ToList();
                
                return Json(new { success = true, data = formattedMessages });
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
        // 修改：GetOrderMessages（用户端） — 增加 conversationEnded/endedBy/endedTime
        [HttpPost]
        public JsonResult GetOrderMessages(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var messages = _messageBLL.GetOrderMessages(orderId);
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

                var convBll = new ConversationBLL();
                var latestConv = convBll.GetLatestConversation(orderId);
                bool conversationEnded = latestConv != null && latestConv.EndedTime.HasValue;
                
                // 使用公共方法确定谁已经结束了对话
                string endedBy = convBll.GetConversationEndedByStatus(orderId);
                
                string endedTimeIso = conversationEnded ? latestConv.EndedTime.Value.ToString("o") : string.Empty;

                return Json(new { success = true, messages = result, conversationEnded = conversationEnded, endedBy = endedBy, endedTime = endedTimeIso });
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
        // 新增：用户结束会话（HomeController）
        [HttpPost]
        public JsonResult EndConversation(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null) return Json(new { success = false, message = "请先登录" });
                var user = (Users)Session["LoginUser"];
                var convBll = new ConversationBLL();
                bool ok = convBll.EndConversationBy(orderId, "user", user.UserID);
                
                if (!ok)
                {
                    return Json(new { success = false, message = "结束对话失败" });
                }

                // 检查双方是否都已结束
                var (bothEnded, _) = convBll.HasBothEnded(orderId);

                return Json(new
                {
                    success = true,
                    message = "对话已结束",
                    bothEnded = bothEnded
                });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
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

        // 获取用户的历史消息（超过1个月的消息）
        [HttpPost]
        public JsonResult GetUserHistoricalMessages(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var user = (Users)Session["LoginUser"];
                var conversationBLL = new ConversationBLL();
                var messages = conversationBLL.GetUserHistoricalMessages(orderId, user.UserID);

                var result = messages.Select(m => new
                {
                    messageId = m.MessageID,
                    orderId = m.OrderID,
                    senderType = (m.SenderType ?? string.Empty).ToLower(),
                    senderId = m.SenderID,
                    content = m.Content ?? string.Empty,
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

        /// <summary>
        /// 联系回收员视图（用户端）
        /// </summary>
        [HttpGet]
        public ActionResult ContactRecycler(int orderId)
        {
            // 验证用户登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            try
            {
                var user = (Users)Session["LoginUser"];

                // 通过BLL层获取订单详情（含回收员ID），避免直接调用DAL
                var orderDetail = _orderBLL.GetOrderDetail(orderId, user.UserID);
                if (orderDetail == null || orderDetail.Appointment.RecyclerID == null)
                {
                    ViewBag.ErrorMsg = "无法联系回收员：订单不存在或未分配回收员";
                    return View();
                }

                // 通过BLL层获取回收员信息
                var recycler = _staffBLL.GetRecyclerById(orderDetail.Appointment.RecyclerID.Value);
                if (recycler == null)
                {
                    ViewBag.ErrorMsg = "回收员信息不存在";
                    return View();
                }

                // 设置ViewBag变量供视图使用
                ViewBag.OrderId = orderId;
                ViewBag.OrderNumber = $"AP{orderId:D6}";
                ViewBag.RecyclerName = recycler.FullName ?? recycler.Username;
                ViewBag.RecyclerId = recycler.RecyclerID;
                ViewBag.UserId = user.UserID;

                // 构建视图模型（保留用于向后兼容）
                var model = new ContactRecyclerViewModel
                {
                    OrderId = orderId,
                    OrderNumber = $"AP{orderId:D6}",
                    RecyclerName = recycler.FullName ?? recycler.Username,
                    RecyclerPhone = recycler.PhoneNumber,
                    UserName = user.Username,
                    UserPhone = user.PhoneNumber
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = $"加载联系页面失败：{ex.Message}";
                return View();
            }
        }

        /// <summary>
        /// 发送消息给回收员
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessageToRecycler(ContactRecyclerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ContactRecycler", model);
            }

            try
            {
                var user = (Users)Session["LoginUser"];
                var messageBLL = new MessageBLL();

                // 创建 SendMessageRequest 对象
                var request = new SendMessageRequest
                {
                    OrderID = model.OrderId,
                    SenderType = "user",
                    SenderID = user.UserID,
                    Content = model.MessageContent,
                    SentTime = DateTime.Now,
                    IsRead = false
                };

                // 调用BLL层发送消息
                var result = messageBLL.SendMessage(request);

                if (result.Success)
                {
                    ViewBag.SuccessMsg = "消息发送成功";
                    return View("ContactRecycler", model);
                }
                ViewBag.ErrorMsg = result.Message ?? "消息发送失败，请重试";
                return View("ContactRecycler", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = $"发送消息出错：{ex.Message}";
                return View("ContactRecycler", model);
            }
        }

        // 评价订单页面
        public ActionResult ReviewOrder(int orderId)
        {
            if (Session["LoginUser"] == null)
                return RedirectToAction("Login", "Home");

            var user = (Users)Session["LoginUser"];
            
            try
            {
                var orderBll = new OrderBLL();
                var order = orderBll.GetOrderDetail(orderId, user.UserID);
                
                if (order.Appointment.Status != "已完成")
                {
                    TempData["ErrorMsg"] = "只能评价已完成的订单";
                    return RedirectToAction("Order", "Home");
                }

                // 检查订单是否有分配的回收员
                if (!order.Appointment.RecyclerID.HasValue || order.Appointment.RecyclerID.Value <= 0)
                {
                    TempData["ErrorMsg"] = "该订单未分配回收员，无法评价";
                    return RedirectToAction("Order", "Home");
                }

                // 检查是否已评价
                var reviewBll = new OrderReviewBLL();
                if (reviewBll.HasReviewed(orderId, user.UserID))
                {
                    TempData["ErrorMsg"] = "该订单已经评价过了";
                    return RedirectToAction("Order", "Home");
                }

                ViewBag.Order = order;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "加载评价页面失败：" + ex.Message;
                return RedirectToAction("Order", "Home");
            }
        }

        // 提交评价
        [HttpPost]
        public JsonResult SubmitReview(int orderId, int recyclerId, int starRating, string reviewText)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                if (recyclerId <= 0)
                    return Json(new { success = false, message = "订单未分配回收员，无法评价" });

                var user = (Users)Session["LoginUser"];
                var reviewBll = new OrderReviewBLL();

                var result = reviewBll.AddReview(orderId, user.UserID, recyclerId, starRating, reviewText);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 检查订单是否已评价
        [HttpPost]
        public JsonResult CheckReviewed(int orderId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var user = (Users)Session["LoginUser"];
                var reviewBll = new OrderReviewBLL();
                
                bool hasReviewed = reviewBll.HasReviewed(orderId, user.UserID);
                return Json(new { success = true, hasReviewed = hasReviewed });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==================== 管理员联系功能 ====================

        // ==================== 用户反馈功能 ====================

        /// <summary>
        /// 用户反馈页面（GET）
        /// </summary>
        [HttpGet]
        public ActionResult Feedback()
        {
            // 检查登录状态 - 必须登录后才能访问反馈页面
            if (Session["LoginUser"] == null)
            {
                TempData["ReturnUrl"] = Url.Action("Feedback", "Home");
                return RedirectToAction("LoginSelect", "Home");
            }

            return View();
        }

        /// <summary>
        /// 提交用户反馈（POST）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFeedback(string FeedbackType, string Subject, 
                                          string Description, string ContactEmail)
        {
            try
            {
                // 检查登录状态
                if (Session["LoginUser"] == null)
                {
                    TempData["ErrorMessage"] = "请先登录";
                    return RedirectToAction("LoginSelect", "Home");
                }

                var user = (Users)Session["LoginUser"];

                // 创建反馈对象
                var feedback = new UserFeedback
                {
                    UserID = user.UserID,
                    FeedbackType = FeedbackType,
                    Subject = Subject,
                    Description = Description,
                    ContactEmail = ContactEmail,
                    Status = "反馈中",
                    CreatedDate = DateTime.Now
                };

                // 调用BLL层添加反馈
                var (success, message) = _feedbackBLL.AddFeedback(feedback);

                if (success)
                {
                    TempData["SuccessMessage"] = "反馈提交成功！感谢您的反馈，我们会尽快处理。";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction("Feedback", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "提交失败：" + ex.Message;
                return RedirectToAction("Feedback", "Home");
            }
        }

        /// <summary>
        /// 上传用户头像
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadAvatar(HttpPostedFileBase avatarFile)
        {
            try
            {
                // 检查登录状态
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                // 检查文件是否为空
                if (avatarFile == null || avatarFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "请选择要上传的图片" });
                }

                // 检查文件大小（限制为5MB）
                if (avatarFile.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "图片大小不能超过5MB" });
                }

                // 检查文件类型
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                string fileExtension = System.IO.Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "只支持 JPG、PNG、GIF、BMP 格式的图片" });
                }

                var user = (Users)Session["LoginUser"];

                // 生成唯一文件名
                string fileName = $"user_{user.UserID}_{DateTime.Now.Ticks}{fileExtension}";
                string uploadPath = Server.MapPath("~/Uploads/Avatars/");
                
                // 确保目录存在
                if (!System.IO.Directory.Exists(uploadPath))
                {
                    System.IO.Directory.CreateDirectory(uploadPath);
                }

                string filePath = System.IO.Path.Combine(uploadPath, fileName);

                // 保存文件
                avatarFile.SaveAs(filePath);

                // 生成相对URL路径
                string avatarUrl = $"/Uploads/Avatars/{fileName}";

                // 更新数据库
                bool success = _userBLL.UpdateUserAvatar(user.UserID, avatarUrl);
                if (success)
                {
                    // 更新Session中的用户信息
                    user.url = avatarUrl;
                    Session["LoginUser"] = user;

                    return Json(new { success = true, message = "头像上传成功", avatarUrl = avatarUrl });
                }
                else
                {
                    // 如果数据库更新失败，删除已上传的文件
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return Json(new { success = false, message = "头像上传失败，请重试" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "上传失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 设置默认头像
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SetDefaultAvatar(string avatarName)
        {
            try
            {
                // 检查登录状态
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                // 验证头像名称
                string[] validAvatars = { "avatar1.svg", "avatar2.svg", "avatar3.svg", "avatar4.svg", "avatar5.svg" };
                if (!validAvatars.Contains(avatarName))
                {
                    return Json(new { success = false, message = "无效的头像选择" });
                }

                var user = (Users)Session["LoginUser"];
                string avatarUrl = $"/Uploads/Avatars/Default/{avatarName}";

                // 更新数据库
                bool success = _userBLL.UpdateUserAvatar(user.UserID, avatarUrl);
                if (success)
                {
                    // 更新Session中的用户信息
                    user.url = avatarUrl;
                    Session["LoginUser"] = user;

                    return Json(new { success = true, message = "默认头像设置成功", avatarUrl = avatarUrl });
                }
                else
                {
                    return Json(new { success = false, message = "设置失败，请重试" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "设置失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 用户查看自己的反馈记录（我的反馈）
        /// </summary>
        [HttpGet]
        public ActionResult MyFeedback()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                TempData["ReturnUrl"] = Url.Action("MyFeedback", "Home");
                return RedirectToAction("LoginSelect", "Home");
            }

            var user = (Users)Session["LoginUser"];
            
            try
            {
                // 获取用户的所有反馈记录
                var feedbacks = _feedbackBLL.GetUserFeedbacks(user.UserID);
                return View(feedbacks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "加载反馈记录失败：" + ex.Message;
                return View(new List<UserFeedback>());
            }
        }

        // ==================== 用户通知功能 ====================

        /// <summary>
        /// 获取用户通知列表（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetUserNotifications(int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                // 验证分页参数，防止DoS攻击
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // 限制最大页面大小

                var user = (Users)Session["LoginUser"];
                var notifications = _notificationBLL.GetUserNotifications(user.UserID, pageIndex, pageSize);
                var totalCount = _notificationBLL.GetTotalCount(user.UserID);
                var unreadCount = _notificationBLL.GetUnreadCount(user.UserID);

                var result = notifications.Select(n => new
                {
                    notificationId = n.NotificationID,
                    notificationType = n.NotificationType,
                    typeDisplayName = NotificationTypes.GetDisplayName(n.NotificationType),
                    typeIcon = NotificationTypes.GetIcon(n.NotificationType),
                    typeColor = NotificationTypes.GetColor(n.NotificationType),
                    title = n.Title,
                    content = n.Content,
                    relatedOrderId = n.RelatedOrderID,
                    relatedFeedbackId = n.RelatedFeedbackID,
                    createdDate = n.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                    isRead = n.IsRead,
                    readDate = n.ReadDate?.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return Json(new
                {
                    success = true,
                    notifications = result,
                    totalCount = totalCount,
                    unreadCount = unreadCount,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取未读通知数量（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetUnreadNotificationCount()
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var user = (Users)Session["LoginUser"];
                var unreadCount = _notificationBLL.GetUnreadCount(user.UserID);

                return Json(new { success = true, unreadCount = unreadCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 标记通知为已读（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarkNotificationAsRead(int notificationId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var user = (Users)Session["LoginUser"];
                var result = _notificationBLL.MarkAsRead(notificationId, user.UserID);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 标记所有通知为已读（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarkAllNotificationsAsRead()
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var user = (Users)Session["LoginUser"];
                var result = _notificationBLL.MarkAllAsRead(user.UserID);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 删除通知（AJAX）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteNotification(int notificationId)
        {
            try
            {
                if (Session["LoginUser"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var user = (Users)Session["LoginUser"];
                var result = _notificationBLL.DeleteNotification(notificationId, user.UserID);

                return Json(new { success = result, message = result ? "删除成功" : "删除失败" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}