using System;
using System.Web.Mvc;
using AspNetSkeleton.Common;
using AspNetSkeleton.UI.Areas.Dashboard.Models;

namespace AspNetSkeleton.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly IClock _clock;

        public HomeController(IClock clock)
        {
            _clock = clock;
        }

        public ActionResult Index()
        {
            var model = new HomeIndexModel();

            var now = _clock.UtcNow.ToLocalTime();
            var midnight = now.Date + TimeSpan.FromDays(1);

            model.TimeToMidnight = midnight - now;

            ViewBag.ActiveMenuItem = "Dashboard";
            ViewBag.ActiveSubMenuItem = "Overview";
            return View(model);
        }
    }
}
