using AspNetSkeleton.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetSkeleton.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
