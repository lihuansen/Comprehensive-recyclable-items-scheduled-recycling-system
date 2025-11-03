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
        private readonly RecyclerOrderBLL _recyclerOrderBLL = new RecyclerOrderBLL();
        private readonly MessageBLL _messageBLL = new MessageBLL();
        private readonly OrderBLL _orderBLL = new OrderBLL();
        private readonly AdminBLL _adminBLL = new AdminBLL();

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
                else if (errorMsg.Contains("验证码"))
                {
                    model.Captcha = "";
                }
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
                    // 发送系统消息通知用户
                    var systemMessage = new SendMessageRequest
                    {
                        OrderID = appointmentId,
                        SenderType = "system",
                        SenderID = 0,
                        Content = $"回收员 {recycler.Username} 已接收您的订单，请保持电话畅通。"
                    };
                    _messageBLL.SendMessage(systemMessage);

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

        // 获取订单对话（回收员端），已包含 conversationEnded/endedBy/endedTime
        [HttpPost]
        public JsonResult GetOrderConversation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                if (orderId <= 0)
                    return Json(new { success = false, message = "无效订单ID" });

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
                var latestConv = convBll.GetLatestConversation(orderId);
                var bothInfo = convBll.HasBothEnded(orderId); // (bool, DateTime?)
                bool conversationBothEnded = bothInfo.BothEnded;
                string latestEndedTimeIso = bothInfo.LatestEndedTime.HasValue ? bothInfo.LatestEndedTime.Value.ToString("o") : string.Empty;
                string lastEndedBy = latestConv != null ? latestConv.Status ?? "" : "";

                return Json(new
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
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 发送消息给用户（回收员端 AJAX）
        /// 前端只需要提供 OrderID 和 Content，后台会填充 SenderType/SenderID
        /// </summary>
        [HttpPost]
        public JsonResult SendMessageToUser(SendMessageRequest request)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                if (request == null || request.OrderID <= 0 || string.IsNullOrWhiteSpace(request.Content))
                {
                    return Json(new { success = false, message = "参数不完整" });
                }

                request.SenderType = "recycler";
                request.SenderID = recycler.RecyclerID;

                var result = _messageBLL.SendMessage(request);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"发送失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 标记订单中对回收员可见的消息为已读（回收员已查看）
        /// </summary>
        [HttpPost]
        public JsonResult MarkMessagesAsRead(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var recycler = (Recyclers)Session["LoginStaff"];
                bool result = _messageBLL.MarkMessagesAsRead(orderId, "recycler", recycler.RecyclerID);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取订单详情（AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult GetOrderDetail(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];
                var result = _recyclerOrderBLL.GetOrderDetail(appointmentId, recycler.RecyclerID);

                if (result.Detail != null)
                {
                    return Json(new { success = true, data = result.Detail });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 获取回收员的历史会话列表（分页）
        [HttpPost]
        public JsonResult GetRecyclerConversations(int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

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

                return Json(new { success = true, conversations = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 获取某个已结束会话的历史消息（回收员端查看历史）
        [HttpPost]
        public JsonResult GetConversationMessagesBeforeEnd(int orderId, string endedTime)
        {
            try
            {
                if (Session["LoginStaff"] == null)
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

        // 回收员结束会话（StaffController）
        [HttpPost]
        public JsonResult EndConversation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null) return Json(new { success = false, message = "请先登录" });
                var recycler = (Recyclers)Session["LoginStaff"];
                var convBll = new ConversationBLL();
                bool ok = convBll.EndConversationBy(orderId, "recycler", recycler.RecyclerID);
                
                if (!ok)
                {
                    return Json(new { success = false, message = "用户需要先结束对话" });
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

        // 检查双方是否都已结束对话
        [HttpPost]
        public JsonResult CheckBothEnded(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var convBll = new ConversationBLL();
                var (bothEnded, _) = convBll.HasBothEnded(orderId);

                return Json(new { success = true, bothEnded = bothEnded });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 完成订单（回收员点击后把订单状态置为 已完成，并写入库存）
        [HttpPost]
        public JsonResult CompleteOrder(int appointmentId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var recycler = (Recyclers)Session["LoginStaff"];
                
                // 检查对话是否都已结束
                var convBll = new ConversationBLL();
                var (bothEnded, _) = convBll.HasBothEnded(appointmentId);
                
                if (!bothEnded)
                {
                    return Json(new { success = false, message = "双方必须都结束对话后才能完成订单" });
                }

                // 写入库存
                var inventoryBll = new InventoryBLL();
                bool inventoryAdded = inventoryBll.AddInventoryFromOrder(appointmentId, recycler.RecyclerID);
                
                if (!inventoryAdded)
                {
                    return Json(new { success = false, message = "写入库存失败" });
                }

                // 完成订单
                var orderBll = new OrderBLL();
                var result = orderBll.CompleteOrder(appointmentId, recycler.RecyclerID);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 仓库管理页面
        public ActionResult WarehouseManagement()
        {
            if (Session["LoginStaff"] == null)
                return RedirectToAction("Login", "Staff");

            return View();
        }

        // 获取库存汇总数据
        [HttpPost]
        public JsonResult GetInventorySummary()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                    return Json(new { success = false, message = "请先登录" });

                var recycler = (Recyclers)Session["LoginStaff"];
                var inventoryBll = new InventoryBLL();
                
                // 获取所有库存汇总（不过滤回收员）
                var summary = inventoryBll.GetInventorySummary(null);
                
                var result = summary.Select(s => new
                {
                    categoryKey = s.CategoryKey,
                    categoryName = s.CategoryName,
                    totalWeight = s.TotalWeight
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
                return RedirectToAction("StaffLogin", "Staff");
            }

            var staff = Session["LoginStaff"] as Staffs;
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
        public JsonResult GetRecyclerReviews()
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "未登录" });
                }

                var staff = Session["LoginStaff"] as Staffs;
                var role = Session["StaffRole"] as string;

                if (role != "recycler")
                {
                    return Json(new { success = false, message = "权限不足" });
                }

                var reviewBLL = new OrderReviewBLL();
                
                // 获取评价列表
                var reviews = reviewBLL.GetReviewsByRecyclerId(staff.StaffID);
                
                // 获取评分摘要
                var summary = reviewBLL.GetRecyclerRatingSummary(staff.StaffID);
                
                // 获取星级分布
                var distribution = reviewBLL.GetRecyclerRatingDistribution(staff.StaffID);

                var result = reviews.Select(r => new
                {
                    orderId = r.OrderID,
                    orderNumber = "AP" + r.OrderID.ToString("D6"),
                    userId = r.UserID,
                    starRating = r.StarRating,
                    reviewText = r.ReviewText,
                    createdDate = r.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return Json(new
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
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 历史会话页面 - 回收员查看历史对话
        /// </summary>
        [HttpGet]
        public ActionResult HistoryConversations()
        {
            // 检查登录
            if (Session["LoginStaff"] == null)
            {
                return RedirectToAction("StaffLogin", "Staff");
            }

            var staff = Session["LoginStaff"] as Staffs;
            var role = Session["StaffRole"] as string;

            if (role != "recycler")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// 获取回收员的历史会话列表
        /// </summary>
        [HttpPost]
        public JsonResult GetRecyclerConversations(int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "未登录" });
                }

                var staff = Session["LoginStaff"] as Staffs;
                var role = Session["StaffRole"] as string;

                if (role != "recycler")
                {
                    return Json(new { success = false, message = "权限不足" });
                }

                var conversationBLL = new ConversationBLL();
                var convs = conversationBLL.GetRecyclerConversations(staff.StaffID, pageIndex, pageSize);

                var result = convs.Select(c => new
                {
                    orderId = c.OrderID,
                    orderNumber = "AP" + c.OrderID.ToString("D6"),
                    userName = c.UserName,
                    endedTime = c.EndedTime?.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return Json(new { success = true, conversations = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取历史会话消息（回收员端）
        /// </summary>
        [HttpPost]
        public JsonResult GetConversationMessagesBeforeEnd(int orderId, string endedTime)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "未登录" });
                }

                DateTime et;
                if (!DateTime.TryParse(endedTime, out et))
                {
                    return Json(new { success = false, message = "时间格式无效" });
                }

                var conversationBLL = new ConversationBLL();
                var messages = conversationBLL.GetConversationMessagesBeforeEnd(orderId, et);

                var result = messages.Select(m => new
                {
                    senderType = m.SenderType,
                    senderId = m.SenderID,
                    content = m.Content,
                    sentTime = m.SentTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return Json(new { success = true, messages = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Admin - User Management

        /// <summary>
        /// 管理员 - 用户管理页面
        /// </summary>
        public ActionResult UserManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取用户列表（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetUsers(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            try
            {
                var result = _adminBLL.GetAllUsers(page, pageSize, searchTerm);
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 管理员 - 获取用户统计信息（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetUserStatistics()
        {
            try
            {
                var stats = _adminBLL.GetUserStatistics();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Admin - Recycler Management

        /// <summary>
        /// 管理员 - 回收员管理页面
        /// </summary>
        public ActionResult RecyclerManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取回收员列表（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetRecyclers(int page = 1, int pageSize = 20, string searchTerm = null, bool? isActive = null)
        {
            try
            {
                var result = _adminBLL.GetAllRecyclers(page, pageSize, searchTerm, isActive);
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 管理员 - 获取回收员详情（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetRecyclerDetails(int recyclerId)
        {
            try
            {
                var recycler = _adminBLL.GetRecyclerById(recyclerId);
                var completedOrders = _adminBLL.GetRecyclerCompletedOrdersCount(recyclerId);

                return Json(new { 
                    success = true, 
                    data = new 
                    { 
                        recycler = recycler, 
                        completedOrders = completedOrders 
                    } 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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
                var result = _adminBLL.DeleteRecycler(recyclerId);
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
        public JsonResult GetRecyclerStatistics()
        {
            try
            {
                var stats = _adminBLL.GetRecyclerStatistics();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Admin - Order Management

        /// <summary>
        /// 管理员 - 订单管理页面
        /// </summary>
        public ActionResult OrderManagement()
        {
            if (Session["StaffRole"] == null || Session["StaffRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        /// <summary>
        /// 管理员 - 获取订单列表（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetOrders(int page = 1, int pageSize = 20, string status = null, string searchTerm = null)
        {
            try
            {
                var result = _adminBLL.GetAllOrders(page, pageSize, status, searchTerm);
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 管理员 - 获取订单统计信息（API）
        /// </summary>
        [HttpGet]
        public JsonResult GetOrderStatistics()
        {
            try
            {
                var stats = _adminBLL.GetOrderStatistics();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


    }
}