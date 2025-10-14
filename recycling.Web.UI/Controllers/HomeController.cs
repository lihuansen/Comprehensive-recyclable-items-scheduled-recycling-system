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
        // 依赖BLL层，与UserController依赖UserBLL的方式一致
        private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();

        [HttpGet]
        public ActionResult Index(RecyclableQueryModel query)
        {
            try
            {
                // 首次访问检查数据是否初始化
                _recyclableItemBLL.EnsureDataExists();

                // 1. 获取所有品类（供筛选下拉框）
                ViewBag.Categories = _recyclableItemBLL.GetAllCategories();

                // 2. 调用BLL获取分页数据
                var result = _recyclableItemBLL.GetPagedItems(query);

                // 3. 传递查询参数到视图（用于回显筛选条件）
                ViewBag.Query = query;
                return View(result);
            }
            catch (Exception ex)
            {
                // 异常处理：与UserController一致，通过ViewBag显示错误
                ViewBag.ErrorMessage = ex.Message;
                return View(new PagedResult<RecyclableItems>());
            }
        }
        public ActionResult Order()
        {
            // 检查登录状态
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("LoginSelect", "Home");
            }

            // 重定向到User控制器的预约页面
            return RedirectToAction("Appointment", "User");
        }
        public ActionResult Message()
        {
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
        public new ActionResult Profile()
        {
            return View();
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
    }
}