using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using recycling.BLL;
using recycling.Model;
using Newtonsoft.Json;
using System.IO;
using OfficeOpenXml;

namespace recycling.Web.UI.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffBLL _staffBLL = new StaffBLL();
        private readonly RecyclerOrderBLL _recyclerOrderBLL = new RecyclerOrderBLL();
        private readonly MessageBLL _messageBLL = new MessageBLL();
        private readonly OrderBLL _orderBLL = new OrderBLL();
        private readonly AdminBLL _adminBLL = new AdminBLL();
        private readonly HomepageCarouselBLL _carouselBLL = new HomepageCarouselBLL();
        private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();

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
                var latestConv = convBll.GetLatestConversation(orderId);
                var bothInfo = convBll.HasBothEnded(orderId); // (bool, DateTime?)
                bool conversationBothEnded = bothInfo.BothEnded;
                string latestEndedTimeIso = bothInfo.LatestEndedTime.HasValue ? bothInfo.LatestEndedTime.Value.ToString("o") : string.Empty;
                string lastEndedBy = latestConv != null ? latestConv.Status ?? "" : "";

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

                // 写入库存
                var inventoryBll = new InventoryBLL();
                bool inventoryAdded = inventoryBll.AddInventoryFromOrder(appointmentId, recycler.RecyclerID);
                
                if (!inventoryAdded)
                {
                    var errorJson = JsonConvert.SerializeObject(new { success = false, message = "写入库存失败" });
                    return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
                }

                // 完成订单
                var orderBll = new OrderBLL();
                var result = orderBll.CompleteOrder(appointmentId, recycler.RecyclerID);
                var json = JsonConvert.SerializeObject(new { success = result.Success, message = result.Message });
                return Content(json, "application/json", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                return Content(errorJson, "application/json", System.Text.Encoding.UTF8);
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

        /// <summary>
        /// 历史会话页面 - 回收员查看历史对话
        /// </summary>
        [HttpGet]
        public ActionResult HistoryConversations()
        {
            // 检查登录
            if (Session["LoginStaff"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            var staff = Session["LoginStaff"] as Recyclers;
            var role = Session["StaffRole"] as string;

            if (role != "recycler")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        #region Admin - User Management

        /// <summary>
        /// 管理员 - 用户管理页面
        /// </summary>
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
        /// 管理员 - 导出用户数据到Excel
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
                // Set EPPlus license
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Get all users without pagination for export
                var users = _adminBLL.GetAllUsersForExport(searchTerm);

                // Create Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("用户数据");

                    // Set header
                    worksheet.Cells[1, 1].Value = "用户ID";
                    worksheet.Cells[1, 2].Value = "用户名";
                    worksheet.Cells[1, 3].Value = "邮箱";
                    worksheet.Cells[1, 4].Value = "手机号";
                    worksheet.Cells[1, 5].Value = "注册日期";
                    worksheet.Cells[1, 6].Value = "最后登录日期";
                    worksheet.Cells[1, 7].Value = "状态";

                    // Style header
                    using (var range = worksheet.Cells[1, 1, 1, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Fill data
                    int row = 2;
                    foreach (var user in users)
                    {
                        worksheet.Cells[row, 1].Value = user.UserID;
                        worksheet.Cells[row, 2].Value = user.Username;
                        worksheet.Cells[row, 3].Value = user.Email;
                        worksheet.Cells[row, 4].Value = user.PhoneNumber;
                        worksheet.Cells[row, 5].Value = user.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cells[row, 6].Value = user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未登录";
                        
                        // Determine user status
                        var isActive = user.LastLoginDate.HasValue && 
                                      (DateTime.Now - user.LastLoginDate.Value).TotalDays <= 30;
                        worksheet.Cells[row, 7].Value = isActive ? "活跃" : "不活跃";
                        
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate file
                    var fileName = $"用户数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
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
        /// 管理员 - 导出回收员数据到Excel
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
                // Set EPPlus license
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Get all recyclers without pagination for export
                var recyclers = _adminBLL.GetAllRecyclersForExport(searchTerm, isActive);

                // Create Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("回收员数据");

                    // Set header
                    worksheet.Cells[1, 1].Value = "回收员ID";
                    worksheet.Cells[1, 2].Value = "用户名";
                    worksheet.Cells[1, 3].Value = "姓名";
                    worksheet.Cells[1, 4].Value = "手机号";
                    worksheet.Cells[1, 5].Value = "区域";
                    worksheet.Cells[1, 6].Value = "评分";
                    worksheet.Cells[1, 7].Value = "完成订单数";
                    worksheet.Cells[1, 8].Value = "是否可接单";
                    worksheet.Cells[1, 9].Value = "账号状态";
                    worksheet.Cells[1, 10].Value = "注册日期";

                    // Style header
                    using (var range = worksheet.Cells[1, 1, 1, 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Fill data
                    int row = 2;
                    foreach (var recycler in recyclers)
                    {
                        worksheet.Cells[row, 1].Value = recycler.RecyclerID;
                        worksheet.Cells[row, 2].Value = recycler.Username;
                        worksheet.Cells[row, 3].Value = recycler.FullName ?? "-";
                        worksheet.Cells[row, 4].Value = recycler.PhoneNumber;
                        worksheet.Cells[row, 5].Value = recycler.Region;
                        worksheet.Cells[row, 6].Value = recycler.Rating?.ToString("F1") ?? "0.0";
                        
                        // Get completed orders count
                        var completedOrders = _adminBLL.GetRecyclerCompletedOrdersCount(recycler.RecyclerID);
                        worksheet.Cells[row, 7].Value = completedOrders;
                        
                        worksheet.Cells[row, 8].Value = recycler.Available ? "可接单" : "不可接单";
                        worksheet.Cells[row, 9].Value = recycler.IsActive ? "激活" : "禁用";
                        
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate file
                    var fileName = $"回收员数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
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
                var result = _adminBLL.DeleteAdmin(adminId);
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
        /// 超级管理员 - 导出管理员数据到Excel
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
                // Set EPPlus license
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Get all admins without pagination for export
                var admins = _adminBLL.GetAllAdminsForExport(searchTerm, isActive);

                // Create Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("管理员数据");

                    // Set header
                    worksheet.Cells[1, 1].Value = "管理员ID";
                    worksheet.Cells[1, 2].Value = "用户名";
                    worksheet.Cells[1, 3].Value = "姓名";
                    worksheet.Cells[1, 4].Value = "创建日期";
                    worksheet.Cells[1, 5].Value = "最后登录日期";
                    worksheet.Cells[1, 6].Value = "账号状态";

                    // Style header
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Fill data
                    int row = 2;
                    foreach (var admin in admins)
                    {
                        worksheet.Cells[row, 1].Value = admin.AdminID;
                        worksheet.Cells[row, 2].Value = admin.Username;
                        worksheet.Cells[row, 3].Value = admin.FullName;
                        worksheet.Cells[row, 4].Value = admin.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                        worksheet.Cells[row, 5].Value = admin.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未登录";
                        worksheet.Cells[row, 6].Value = (admin.IsActive ?? true) ? "激活" : "禁用";
                        
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate file
                    var fileName = $"管理员数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"导出失败: {ex.Message}";
                return RedirectToAction("AdminManagement");
            }
        }

        #endregion

        #region Admin Homepage Management

        /// <summary>
        /// Admin homepage management index page
        /// </summary>
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

                // Use HardDelete to permanently remove from database
                var (success, message) = _carouselBLL.HardDelete(id);
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

                // Use HardDelete instead of soft delete
                var (success, message) = _recyclableItemBLL.HardDelete(id);
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


    }
}