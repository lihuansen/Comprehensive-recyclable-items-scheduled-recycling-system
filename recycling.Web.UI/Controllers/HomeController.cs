using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace recycling.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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
    }
}