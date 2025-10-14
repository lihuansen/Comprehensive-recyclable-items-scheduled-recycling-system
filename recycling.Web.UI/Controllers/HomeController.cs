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
        public ActionResult Index(string keyword, string category, int pageIndex = 1)
        {
            int pageSize = 10; // 可配置为常量
            try
            {
                // 获取分类列表并传递到视图
                ViewBag.Categories = _recyclableItemBLL.GetAllCategories();

                // 执行查询
                var pagedResult = _recyclableItemBLL.SearchItems(keyword, category, pageIndex, pageSize);

                // 保存查询参数用于分页回传
                ViewBag.CurrentKeyword = keyword;
                ViewBag.CurrentCategory = category;

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "查询失败：" + ex.Message;
                // 返回空分页结果避免视图报错
                return View(new PagedResult<RecyclableItems>
                {
                    Items = new List<RecyclableItems>(),
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = 0,
                    TotalPages = 0
                });
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