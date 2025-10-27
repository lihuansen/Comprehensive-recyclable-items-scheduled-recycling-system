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

        /// <summary>
        /// 消息中心页面
        /// </summary>
        public ActionResult Message_Center()
        {
            if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
                return RedirectToAction("Login", "Staff");

            var recycler = (Recyclers)Session["LoginStaff"];
            ViewBag.StaffName = recycler.Username;

            // 通过 BLL 获取消息列表
            var messages = _recyclerOrderBLL.GetRecyclerMessages(recycler.RecyclerID);
            return View(messages);
        }

        /// <summary>
        /// 获取订单对话（AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult GetOrderConversation(int orderId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                // 通过 BLL 获取对话
                var messages = _recyclerOrderBLL.GetOrderConversation(orderId);
                return Json(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 发送消息（AJAX）
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

                request.SenderType = "recycler";
                request.SenderID = recycler.RecyclerID;

                // 通过 BLL 发送消息
                var result = _messageBLL.SendMessage(request);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"发送失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 标记消息为已读（AJAX）
        /// </summary>
        [HttpPost]
        public JsonResult MarkMessageAsRead(int messageId)
        {
            try
            {
                if (Session["LoginStaff"] == null)
                {
                    return Json(new { success = false, message = "请先登录" });
                }

                var recycler = (Recyclers)Session["LoginStaff"];

                // 通过 BLL 标记消息为已读
                var result = _recyclerOrderBLL.MarkMessageAsRead(messageId, recycler.RecyclerID);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失败：{ex.Message}" });
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
    }
}