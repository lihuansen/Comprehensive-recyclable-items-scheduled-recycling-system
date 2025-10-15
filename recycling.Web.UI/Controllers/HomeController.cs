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
        public ActionResult Order()
        {
            return View();
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