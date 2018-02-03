using System;
using AspNetSkeleton.Common;
using AspNetSkeleton.UI.Areas.Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSkeleton.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class HomeController : Controller
    {
        readonly IClock _clock;

        public HomeController(IClock clock)
        {
            _clock = clock;
        }

        public IActionResult Index()
        {
            var model = new HomeIndexModel();

            var now = _clock.UtcNow.ToLocalTime();
            var midnight = now.Date + TimeSpan.FromDays(1);

            model.TimeToMidnight = midnight - now;

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Overview";
            return View(model);
        }
    }
}
