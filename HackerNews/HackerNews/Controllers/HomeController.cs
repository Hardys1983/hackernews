using System;
using System.Web.Mvc;

namespace HackerNews.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Guid = Guid.NewGuid().ToString();
            return View();
        }
    }
}