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
    }
}