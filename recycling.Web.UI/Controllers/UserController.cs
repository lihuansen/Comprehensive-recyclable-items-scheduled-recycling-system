using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace recycling.Web.UI.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            ViewBag.LoginType = "password";
            return View();
        }
        public ActionResult PhoneLogin()
        {
            ViewBag.LoginType = "phone";
            return View("Login"); 
        }
        public ActionResult EmailLogin()
        {
            ViewBag.LoginType = "email";
            return View("Login");
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Forgot()
        {
            return View();
        }
    }
}